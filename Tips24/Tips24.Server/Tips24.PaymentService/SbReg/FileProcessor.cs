using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tips24.PaymentService.SbReg.Csv;
using Tips24.PaymentService.Share;

namespace Tips24.PaymentService.SbReg
{
	public class FileProcessor : IDisposable
	{
		#region Типы данных

		private enum CreatePaymentResult
		{
			Success,
			Fail,
			Duplicate
		}

		#endregion
		private static CultureInfo ruCulture = CultureInfo.GetCultureInfo("ru-RU");

		private string _sourceFilePath;

		private string _sourceFileName;

		private string _outputTopLevelDir;

		private string _outputFolder;

		private DateTime _dateTime;

		private string _csvFilePath;

		private string _logFilePath;

		private Logger _logger;

		private StreamReader _fileReader;

		private Share.ShareService _shareService;

		private int? _documentId;

		private SqlServer _sqlServer;

		public FileProcessor(string sourceFile, string outputDir, Share.ShareService shareService, SqlServer sqlServer)
		{
			this._sourceFilePath = sourceFile;
			this._sourceFileName = Path.GetFileName(this._sourceFilePath);
			this._outputTopLevelDir = outputDir;
			this._dateTime = DateTime.Now;
			this._shareService = shareService;
			this._sqlServer = sqlServer;
		}

		public async Task Run()
		{
			this.CreateOutputFolder();

			try
			{
				this.OpenFiles();

				using (SqlConnection conn = _sqlServer.GetConnection())
				{
					await conn.OpenAsync();
					await this.ProcessCsvFile(conn);
				}
			}
			catch (Exception ex)
			{
				await this._logger.Write(ex.ToString(), true);
				throw;
			}
			finally
			{
				this.Dispose();
			}
		}

		#region Инициализация и деинициализация

		private void CreateOutputFolder()
		{
			string yearAndMonth = this._dateTime.ToString("yyMM");
			string day = this._dateTime.ToString("dd");
			string time = this._dateTime.ToString("HHmmss");
			string leafFolder = time + "_" + this._sourceFileName;
			this._outputFolder = Path.Combine(this._outputTopLevelDir, yearAndMonth, day, leafFolder);
			Directory.CreateDirectory(this._outputFolder);

			this._csvFilePath = Path.Combine(this._outputFolder, this._sourceFileName);
			this._logFilePath = Path.Combine(this._outputFolder, leafFolder + ".log");

			File.Move(this._sourceFilePath, this._csvFilePath);

			//using (FileStream sourceFileStream = new FileStream(this._sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 128 * 1024, FileOptions.SequentialScan))
			//{
			//	using (FileStream copyFileStream = new FileStream(this._csvFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, FileOptions.WriteThrough))
			//	{
			//		sourceFileStream.CopyTo(copyFileStream);
			//	}
			//}
		}

		private void OpenFiles()
		{
			this._fileReader = new StreamReader(this._csvFilePath, Encoding.GetEncoding(1251), false, 128 * 1024);
			this._logger = new Logger(this._logFilePath);
		}

		public void Dispose()
		{
			if (this._fileReader != null)
			{
				this._fileReader.Dispose();
			}

			if (this._logger != null)
			{
				this._logger.Dispose();
			}
		}

		#endregion

		private async Task ProcessCsvFile(SqlConnection conn)
		{
			CsvLineReader csv = new CsvLineReader(this._fileReader);
			for (int i = 1; ; i++)
			{
				CsvReadRecordResult result = csv.Read();
				if (result.LineResult != CsvReadLineResult.Success)
				{
					break;
				}

				if (result.Fields.Count == 12)
				{
					CreatePaymentResult cpr;
					Payment payment = null;
					Exception exception = null;
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						(cpr, payment, exception) = await this.ProcessPayment(result.Fields, conn, tx);
						tx.Commit();
					}

					DiagOptions o = DiagOptions.Tech;
					string message;
					if (cpr == CreatePaymentResult.Success)
					{
						o = DiagOptions.Tech | DiagOptions.Biz;
						message = "SbReg (" + _sourceFileName + "): Платёж " + payment.Id.ToString() +
							(payment.Status == Share.PaymentStatus.Approved ? " успешно проведён" : " ожидает подтверждения") + ": " + payment.RawData;
					}
					else if (cpr == CreatePaymentResult.Duplicate)
					{
						message = "SbReg (" + _sourceFileName + "): Платёж уже обработан: " + payment.RawData;
						await this._logger.WriteLine(i, "Платёж \"" + result.Fields[3] + "; " + result.Fields[4] + "\" уже создан");
					}
					else
					{
						message = "SbReg (" + _sourceFileName + "): Ошибка \"" + exception.Message + "\" при проведении платежа: " + payment.RawData;
						await this._logger.WriteLine(i, exception.ToString());
					}

					Helper.SaveDiagMessage(_sqlServer, o, message, null);
				}
				else if (result.Fields.Count == 6)
				{
					try
					{
						await this.ProcessTotal(result.Fields);
					}
					catch (Exception ex)
					{
						await this._logger.Write("Ошибка при обработке итоговой строки: " + ex.ToString(), true);
					}
				}
				else
				{
					await this._logger.WriteLine(i, "Строка неопознанного формата");
				}
			}
		}

		private async Task<(CreatePaymentResult, Share.Payment, Exception)> ProcessPayment(List<string> fields, SqlConnection conn, SqlTransaction tx)
		{
			Payment payment = new Payment();

			try
			{
				string purpose = fields[7].Trim();
				(int placeId, int? employeeId) = Helper.ParsePurposeCode(purpose);

				payment.Id = 0;
				payment.Status = PaymentStatus.Approved;

				if (this._documentId.HasValue)
				{
					payment.DocumentName = null;
					payment.DocumentId = this._documentId;
				}
				else
				{
					payment.DocumentName = this._sourceFileName;
					payment.DocumentId = null;
				}

				payment.ExternalId = fields[3] + ";" + fields[4];
				payment.DataSource = "SBBREG";
				payment.Provider = "SBERBK";
				payment.OriginalAmount = decimal.Parse(fields[8], ruCulture);
				payment.ReceivedAmount = decimal.Parse(fields[9], ruCulture);
				//payment.CommissionAmount = decimal.Parse(fields[10], ruCulture);
				//payment.IncomeAmount = 0M;
				//payment.PayoutAmount = 0M;
				payment.PaymentDateTime = DateTime.ParseExact(fields[0] + fields[1], "dd-MM-yyyyHH-mm-ss", CultureInfo.InvariantCulture);
				payment.IsTimeSpecified = true;
				payment.ArrivalDateTime = DateTime.Now;
				payment.Fio = fields[5].Trim();
				payment.Address = fields[6].Trim();
				payment.Purpose = fields[7].Trim();
				payment.PlaceId = placeId;
				payment.EmployeeId = employeeId;
				payment.RawData = string.Join(";", fields);

				if (await this._shareService.ProceedPayment(payment, conn, tx))
				{
					if (!this._documentId.HasValue)
					{
						this._documentId = payment.DocumentId;
					}

					return (CreatePaymentResult.Success, payment, null);
				}
				else
				{
					return (CreatePaymentResult.Duplicate, payment, null);
				}
			}
			catch (Exception ex)
			{
				string exStr = ex.ToString();
				await this._logger.Write("Ошибка при проведении платежа: " + exStr, true);
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, "SbAcquiringHostedService.CreatePayment: " + exStr, null);
				return (CreatePaymentResult.Fail, payment, ex);
			}
		}

		private async Task ProcessTotal(List<string> fields)
		{
			if (!this._documentId.HasValue)
			{
				return;
			}

			Document document = new Document();
			document.DocumentId = this._documentId.Value;
			document.DocumentNumber = fields[4];
			document.DocumentDate = DateTime.ParseExact(fields[5], "dd-MM-yyyy", CultureInfo.InvariantCulture);
			await this._shareService.SaveDocumentProperties(document);
		}
	}
}
