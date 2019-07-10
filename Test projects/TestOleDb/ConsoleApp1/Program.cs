using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			OleDbEnumerator e = new OleDbEnumerator();
			DataTable dt = e.GetElements();

			List<string> strings = new List<string>
			{
				"Provider=sqloledb;Data Source=NSIGPB_AGL;Initial Catalog=GPBNSI_CURRENT;Integrated Security=SSPI;",
				"Provider=MSDAORA.1;Data Source=ORACLERDF64;Persist Security Info=True;User ID=NORMA_CB_NEW;Password=NORMA_CB_NEW;",
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"\\\\w4dev3\\c$\\te mp\\Feoktistov\\DKSNSI-8777\\Родитель_КР1-Пакет_0-15.01.2019-16_42.xls\";Extended Properties=\"Excel 8.0; HDR = Yes; IMEX = 1\"",
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"\\\\w4dev3\\1.mdb\";User ID=;Password=;",
				"Provider=PGNP.1;Password=sasha2;Persist Security Info=True;User ID=sasha;Initial Catalog=dbname;Data Source=srv;Extended Properties=\"PORT=5432; SSL = allow; \"",
				"Provider=MySqlProv.3.9;Data Source=sui;Password=syu;User ID=sty;Location=srt;Extended Properties=\"PORT=3306; \"",
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"\\\\w4dev3\\c$\";Extended Properties=\"text; HDR = Yes; FMT = Delimited; CharacterSet = 65001; \"",
				"Provider=VFPOLEDB.1;Data Source=\"\\\\w4dev3\\c$\";",
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"\\\\w4dev3\\c$\";Extended Properties=dBASE IV;User ID=;Password="
			};

			foreach (string s in strings)
			{
				OleDbConnectionStringBuilder b = new OleDbConnectionStringBuilder(s);
			}
		}
	}
}
