using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZXing;
using ZXing.QrCode;

namespace QrCodeTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		CancellationTokenSource _cts;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void BtnTips_Click(object sender, RoutedEventArgs e)
		{
			this.txtCode.Text =
				"ST00011|Name=ООО Чаевые-24|PersonalAcc=40702810970010113722|BankName=МОСКОВСКИЙ ФИЛИАЛ АО КБ \"МОДУЛЬБАНК\"|BIC=044525092|CorrespAcc=30101810645250000092|PayeeINN=1651083591|" +
				"Purpose=Дарение чаевых коллективу по договору-оферте tips24.ru/1-1|" +
				"PayerAddress=Москва, бул. Матроса Железняка, д. 9|LastName=Гость|FirstName=заведения";
		}

		private void BtnIp_Click(object sender, RoutedEventArgs e)
		{
			this.txtCode.Text =
				"ST00011|Name=ИП Галяутдинов Ринат Ибрагимович|PersonalAcc=40802810470210002677|BankName=МОСКОВСКИЙ ФИЛИАЛ АО КБ \"МОДУЛЬБАНК\"|BIC=044525092|CorrespAcc=30101810645250000092|PayeeINN=165117672519|" +
				"Purpose=Дарение чаевых коллективу по договору-оферте tips24.ru/1-1|" +
				"PayerAddress=Москва, бул. Матроса Железняка, д. 9|LastName=Гость|FirstName=заведения";
		}

		private void TxtCode_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = this.txtCode.Text;
			if (text.Length > 0)
			{
				this.GenerateQrCode(text);
			}
			else
			{
				this.image.Source = null;
			}
		}

		private void GenerateQrCode(string text)
		{
			QRCodeWriter qrWriter = new QRCodeWriter();
			Dictionary<ZXing.EncodeHintType, object> hints = new Dictionary<ZXing.EncodeHintType, object>();
			hints.Add(ZXing.EncodeHintType.CHARACTER_SET, "windows-1251");
			hints.Add(ZXing.EncodeHintType.MARGIN, 1);
			ZXing.Common.BitMatrix matrix = qrWriter.encode(text.Trim(), ZXing.BarcodeFormat.QR_CODE, 640, 640, hints);

			ZXing.Presentation.BarcodeWriter writer = new ZXing.Presentation.BarcodeWriter();
			WriteableBitmap wb = writer.Write(matrix);
			this.image.Source = wb;
		}
	}
}
