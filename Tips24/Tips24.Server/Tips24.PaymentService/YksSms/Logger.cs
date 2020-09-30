using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tips24.PaymentService.YksSms
{
	public class Logger
	{
		private string _logFolder;

		public Logger(string logFolder)
		{
			this._logFolder = logFolder;
		}

		public void WriteCreateLog(int requestId, CreatePaymentRequestData data)
		{
			WriteLog(requestId, "CreatePayment", data.ResponseData, data.ExceptionData);
		}

		public void WriteCheckLog(int requestId, CheckPaymentRequestData data)
		{
			WriteLog(requestId, "CheckPayment", data.ResponseData, data.ExceptionData);
		}

		public void WriteHookErrorLog(string json, Exception ex)
		{
			string folderName = Path.Combine(this._logFolder, "HookError");
			Directory.CreateDirectory(folderName);
			DateTime now = DateTime.Now;
			string fileName = "HookError-" + now.ToString("yyMMdd-HHmmss.fff") + ".log";
			string filePath = Path.Combine(folderName, fileName);

			using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
			{
				sw.WriteLine(json);
				sw.Write(ex.ToString());
			}
		}

		private void WriteLog(int requestId, string type, ResponseData responseData, Exception exceptionData)
		{
			string folderName = this.GetLogFolder(requestId);
			string filePath = Path.Combine(folderName, GetFileName(requestId, type, responseData.HasError || exceptionData != null, responseData.RequestJson != null));

			using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
			{
				sw.WriteLine("Запрос:");
				if (responseData.RequestJson != null)
				{
					sw.WriteLine(responseData.RequestJson);
				}

				if (responseData.ResponseJson != null)
				{
					sw.WriteLine();
					sw.WriteLine("Ответ:");
					sw.Write(responseData.ResponseJson);
				}

				if (exceptionData != null)
				{
					sw.WriteLine();
					sw.WriteLine("Исключение:");
					sw.Write(exceptionData.ToString());
				}
			}
		}

		private static string GetFileName(int requestId, string type, bool isError, bool isFromHook)
		{
			DateTime now = DateTime.Now;
			return "Request-" + requestId.ToString("000000") + "-" + type + "-" + now.ToString("yyMMdd-HHmmss.fff") + (isFromHook ? "-Hook" : "") + (isError ? "-Error" : "") + ".log";
		}

		private string GetLogFolder(int requestId)
		{
			string hundreds = ((requestId / 100) * 100).ToString("0000");
			string folderName = Path.Combine(this._logFolder, hundreds, requestId.ToString("#000"));
			Directory.CreateDirectory(folderName);
			return folderName;
		}

		internal void WriteHookLog(string json, int requestId)
		{
			string folderName = Path.Combine(this._logFolder, "HookLog");
			Directory.CreateDirectory(folderName);
			DateTime now = DateTime.Now;
			string fileName = "HookLog-" + requestId.ToString("000000") + "-" + now.ToString("yyMMdd-HHmmss.fff") + ".log";
			string filePath = Path.Combine(folderName, fileName);

			using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
			{
				sw.WriteLine(json);
			}
		}
	}
}
