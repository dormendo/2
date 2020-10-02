using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Maxima.Common.BufferManagement;
using Maxima.Common.Utils;
using Maxima.Server.Processing;
using Maxima.Tcp;
using Newtonsoft.Json;

namespace Maxima.Server
{
    public sealed class CommandContext : IDisposable
    {
        private static readonly Encoding DefaultEncoding = Encoding.GetEncoding(1251);
        private readonly IBufferManager _bufferManager;

        public static readonly JsonSerializerSettings SerializerSettings;

        private readonly TcpMessage _rawMessage;
        private readonly ServerConnection _connection;
        private IList<ArraySegment<byte>> _buffersToRelease;
        private bool _isDisposed;
        private const string ErrorCommand = "error";

        public CommandMessage Message { get; private set; }

        public IPAddress ClientIPAddress { get; private set; }

        public string CurrPos { get; set; }

        public string Command { get; private set; }

        static CommandContext()
        {
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.ContractResolver = new NullValueContractResolver();
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            SerializerSettings.Converters.Add(new CustomDateTimeConverter());
        }

        internal CommandContext(ServerConnection connection, TcpMessage rawMessage, IBufferManager bufferManager)
        {
            this._connection = connection;
            this._rawMessage = rawMessage;
            this.Command = this._rawMessage.Header.Command;
            this.ClientIPAddress = this._connection.ClientAddress;
            this.CurrPos = String.Format("Exec:{0} ", this.Command);
            this._bufferManager = bufferManager;
        }

        internal void CreateCommandMessage()
        {
            BufferInfo buffer;
#warning здесь вставлен костыль - проверка имени команды, поскольку старый клиент отображения линии посылает рассогласованные данные
            // а именно true в бите отвечающем за компрессию, но не сжимает данные в теле сообщения при данной команде
            if (this._rawMessage.Header.IsCompressed && this._rawMessage.Header.Command != "loadliv3")
            {
                buffer = MessageCompressor.Decompress(_bufferManager, _rawMessage.BodyBuffers, _rawMessage.Header.Size);
                _buffersToRelease = buffer.Buffers;
            }
            else
            {
                buffer = new BufferInfo(_rawMessage.BodyBuffers, _rawMessage.Header.Size);
            }

            this.Message = new CommandMessage(this._rawMessage.Header, buffer);
        }

        #region Получение и обработка запроса

        public string GetStringData()
        {
            return this.GetStringData(DefaultEncoding);
        }

        public string GetStringData(Encoding encoding)
        {
            return this.Message.AcquireStringData(encoding);
        }

        public JsonTextReader GetJsonReader()
        {
            return this.GetJsonReader(DefaultEncoding);
        }

        public JsonTextReader GetJsonReader(Encoding encoding)
        {
            return new JsonTextReader(new StringReader(this.GetStringData(encoding)));
        }

        public T GetRequest<T>() where T : class
        {
            return this.GetRequest<T>(DefaultEncoding);
        }

        public T GetRequest<T>(Encoding encoding) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(this.GetStringData(encoding), SerializerSettings);
            }
            catch (Exception e)
            {
                ServerLog.AddFBLog(Command + ".RequestParsingError", e.ToString());
                return null;
            }
        }

        #endregion

        #region Отправка результата клиенту

        public async Task SendAsync(string data, bool compress = false)
        {
            await this.SendAsync(data, DefaultEncoding, compress);
        }

        public async Task SendAsync(byte[] data, CompressOption compressOption, HeaderCommand headerCommand = HeaderCommand.Default)
        {
            var dataBuffer = new[] { new ArraySegment<byte>(data) };
            await SendAsync(dataBuffer, data.Length, compressOption, headerCommand);
        }

        public async Task SendAsync(IList<ArraySegment<byte>> dataBuffers, int length, CompressOption compressOption, HeaderCommand headerCommand = HeaderCommand.Default)
        {
            TcpMessage tcpMessage;

            string command = (headerCommand == HeaderCommand.Error ? ErrorCommand : this.Message.Command);

            if (compressOption == CompressOption.Yes)
            {
                var temp = MessageCompressor.Compress(_bufferManager, dataBuffers, length);
                TcpMessageHeader header = new TcpMessageHeader(command, false, true, (int)temp.Length);
                tcpMessage = TcpMessage.CreateMessage(header, temp.Buffers, _bufferManager);
            }
            else
            {
                TcpMessageHeader header = new TcpMessageHeader(command, false, compressOption != CompressOption.No, length);
                tcpMessage = TcpMessage.CreateMessage(header, dataBuffers);
            }

            using (tcpMessage)
            {
                await _connection.WriteResponse(tcpMessage);
            }
        }

        public async Task SendErrorJsonAsync(string error, bool compress = false, HeaderCommand headerCommand = HeaderCommand.Default)
        {
            await this.SendAsync(PrepareErrorJson(error, false), compress ? CompressOption.Yes : CompressOption.No, headerCommand);
        }

        public async Task SendFatalJsonAsync(string value, bool compress = false)
        {
            await this.SendAsync(PrepareErrorJson(value, false, "fatal"), compress ? CompressOption.Yes : CompressOption.No);
        }

        public async Task SendJsonAsync(object obj, bool compress = false)
        {
            using (var stream = new BufferWriteStream(_bufferManager))
            {
                try
                {
                    using (var streamWriter = new StreamWriter(stream, DefaultEncoding))
                    using (var writer = new JsonTextWriter(streamWriter))
                    {
                        JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                        serializer.Serialize(writer, obj);
                    }
                    await this.SendAsync(stream.Buffers, (int) stream.Length, compress ? CompressOption.Yes : CompressOption.No);
                }
                finally
                {
                    _bufferManager.CheckIn(stream.Buffers);
                }
            }
        }

        /// <summary>
        /// Пересылка данных с помощью потока, всегда без сжатия.
        /// </summary>
        /// <param name="stream">поток для отправки</param>
        /// <param name="length">длина сообщения</param>
        public async Task SendStreamAsync(Stream stream, int length)
        {
            TcpMessageHeader header = new TcpMessageHeader(this.Message.Command, false, false, length);
            // посылаем заголовок и данные потока
            await this._connection.WriteStreamDataAsync(header, stream);
        }

        private async Task SendAsync(string data, Encoding encoding, bool compress = false)
        {
            await this.SendAsync(encoding.GetBytes(data), compress ? CompressOption.Yes : CompressOption.No);
        }

        #endregion

        #region json

        public static string SerializeObjectToString(object obj)
        {
            return JsonConvert.SerializeObject(obj, SerializerSettings);
        }

        public static byte[] PrepareJson(object obj, bool compress = false)
        {
            byte[] json = DefaultEncoding.GetBytes(SerializeObjectToString(obj));
            return (compress ? MessageCompressor.Compress(json) : json);
        }

        public static byte[] PrepareJsonFromString(string jsonStr, bool compress = false)
        {
            byte[] json = DefaultEncoding.GetBytes(jsonStr);
            return (compress ? MessageCompressor.Compress(json) : json);
        }

        public static byte[] PrepareErrorJson(string error, bool compress = false, string errorKey = "error")
        {
            using (MemoryStream ms = new MemoryStream(error.Length * 2 + 32))
            {
                using (StreamWriter sw = new StreamWriter(ms, DefaultEncoding))
                {
                    using (JsonTextWriter writer = new JsonTextWriter(sw))
                    {
                        writer.WriteStartObject();
                        writer.WritePropertyName(errorKey);
                        writer.WriteValue(error);
                        writer.WriteEndObject();
                        writer.Flush();
                    }
                }

                byte[] result = ms.ToArray();
                return (compress ? MessageCompressor.Compress(result) : result);
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_buffersToRelease != null)
                    _bufferManager.CheckIn(_buffersToRelease);
                _rawMessage.Dispose();
            }
            _isDisposed = true;
        }
    }
}
