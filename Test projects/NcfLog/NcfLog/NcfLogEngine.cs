using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maxima.BookmakerPrototype.Log
{
    internal class NcfLogEngine : ILogEngine
    {
        private IFileWriter _writer;

        private LogLevel _minLevel;

        internal NcfLogEngine(LogLevel minLevel, IFileWriter writer)
        {
            this._minLevel = minLevel;
            this._writer = writer;
        }

        void ILogEngine.Initialize()
        {
            if (this._writer != null)
            {
                this._writer.Initialize();
            }
        }

        public void Log(LogLevel level, string message, params object[] parameters)
        {
            if (this._writer != null && level >= this._minLevel)
            {
                this.LogEx(level, null, message, parameters);
            }
        }

        public void LogException(LogLevel level, Exception exception, string message, params object[] parameters)
        {
            if (this._writer != null && level < this._minLevel)
            {
                this.LogEx(level, exception, message, parameters);
            }
        }

        void IDisposable.Dispose()
        {
            this._writer.Dispose();
        }

        private void LogEx(LogLevel level, Exception exception, string message, params object[] parameters)
        {
            StringBuilder sb = NcfLogEngineProvider.AcquireStringBuilder();
            
            this.WriteDateTime(sb);
            sb.Append(" ");
            this.WriteLevel(sb, level);
            sb.Append(" ");
            this.WriteMessage(sb, message, parameters);

            if (exception != null)
            {
                sb.AppendLine();
                this.WriteException(sb, exception);
            }

            sb.AppendLine();

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            this._writer.Write(buffer);
        }

        private void WriteDateTime(StringBuilder sb)
        {
            DateTime now = DateTime.Now;
            sb.Append(now.ToString("yyyy-MM-dd hh:mm:ss.fff"));
        }

        private void WriteLevel(StringBuilder sb, LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    sb.Append("DEBUG");
                    break;
                case LogLevel.Info:
                    sb.Append("INFO");
                    break;
                case LogLevel.Warning:
                    sb.Append("WARNING");
                    break;
                case LogLevel.Error:
                    sb.Append("ERROR");
                    break;
                case LogLevel.Fatal:
                    sb.Append("FATAL");
                    break;
                default:
                    sb.Append("TRACE");
                    break;
            }
        }

        private void WriteMessage(StringBuilder sb, string message, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                sb.Append(message);
            }
            else
            {
                sb.AppendFormat(message, parameters);
            }
        }

        private void WriteException(StringBuilder sb, Exception exception)
        {
            sb.Append("Exception:").Append(exception.ToString());
        }
    }
}
