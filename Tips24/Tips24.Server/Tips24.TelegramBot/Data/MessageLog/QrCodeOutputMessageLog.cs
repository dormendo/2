using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tips24.TelegramBot
{
	public class QrCodeOutputMessageLog : MessageLog
	{
		private string _fileId;

		private string _receiverCode;

		public QrCodeOutputMessageLog(Employee employee, string fileId, string receiverCode, ReplyKeyboardMarkup keyboard)
			: base(employee, Types.QrCode, keyboard)
		{
			this._fileId = fileId;
			this._receiverCode = receiverCode;
		}

		protected override void SetExtendedJsonProperties(JsonTextWriter jw)
		{
			jw.WritePropertyName("fileId");
			jw.WriteValue(this._fileId);
			jw.WritePropertyName("receiverCode");
			jw.WriteValue(this._receiverCode);
		}
	}
}
