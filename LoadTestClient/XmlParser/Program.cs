using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XmlParser
{
	public static class XmlWriterExtensions
	{
		public async static Task WriteStartElementAsync(this XmlWriter writer, string localName)
		{
			await writer.WriteStartElementAsync(null, localName, null);
		}
	}

	class Program
	{
		public sealed class StringWriterWithEncoding : StringWriter
		{
			public StringWriterWithEncoding()
			{
			}

			public override Encoding Encoding
			{
				get
				{
					return Encoding.UTF8;
				}
			}
		}

		static async Task Main(string[] args)
		{
			string soap12 = "http://www.w3.org/2003/05/soap-envelope";
			string lanit = "http://www.lanit.ru/Norma/WebServices/";

			StringWriterWithEncoding sw = new StringWriterWithEncoding();
			using (XmlWriter w = XmlWriter.Create(sw, new XmlWriterSettings { Encoding = Encoding.UTF8, Async = true }))
			{
				w.WriteStartDocument();
				w.WriteStartElement("soap12", "Envelope", soap12);
				w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
				w.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

				w.WriteStartElement("Body", soap12);

				w.WriteStartElement("GetElementPositionsResponse", lanit);

				w.WriteStartElement("ElementPositionsOutDoc");
				w.WriteStartAttribute("ViewDate"); w.WriteValue(DateTime.Now); w.WriteEndAttribute();
				w.WriteAttributeString("TotalCount", "TotalCount");

				await w.WriteStartElementAsync("DataHeader");
				w.WriteAttributeString("RequestId", null);
				w.WriteAttributeString("Version", "Version");
				w.WriteAttributeString("RequestDateTime", "RequestDateTime");
				w.WriteAttributeString("UserID", "UserID");
				w.WriteAttributeString("UserName", "UserName");
				w.WriteAttributeString("ErrorCode", "ErrorCode");
				w.WriteAttributeString("ErrorDescription", "ErrorDescription");
				w.WriteEndElement();

				w.WriteEndElement();

				w.WriteEndElement();

				w.WriteEndElement();

				w.WriteEndElement();
				w.WriteEndDocument();
			}




			XDocument xdoc;
			using (FileStream fs = new FileStream("Request.txt", FileMode.Open))
			{
				XmlReader reader = XmlReader.Create(fs, null);
				xdoc = XDocument.Load(reader);
			}

			XElement epin = xdoc.Root.Element(XName.Get("Body", "http://www.w3.org/2003/05/soap-envelope"))
				?.Element(XName.Get("GetElementPositions", "http://www.lanit.ru/Norma/WebServices/"))
				?.Element(XName.Get("ElementPositionsIn", "http://www.lanit.ru/Norma/WebServices/"));
			if (epin != null)
			{
				(bool requestIdSpecified, string requestId) = GetAttribute(epin, "RequestID");
				(bool userLoginSpecified, string userLogin) = GetAttribute(epin, "UserLogin");
				(bool classifierIdSpecified, string classifierId) = GetAttribute(epin, "ClassifierID");
				(bool elementIdSpecified, string elementId) = GetAttribute(epin, "ElementID");
				(bool viewDateSpecified, string viewDate) = GetAttribute(epin, "ViewDate");
				(bool pageNumberSpecified, string pageNumber) = GetAttribute(epin, "PageNumber");
				(bool recordsOnPageSpecified, string recordsOnPage) = GetAttribute(epin, "RecordsOnPage");
				(bool offsetSpecified, string offset) = GetAttribute(epin, "Offset");
				(bool recordsCountSpecified, string recordsCount) = GetAttribute(epin, "RecordsCount");
				(bool dontGetCountSpecified, string dontGetCount) = GetAttribute(epin, "DontGetCount");
				(bool orderBySpecified, string orderBy) = GetAttribute(epin, "OrderBy");


			}


		}

		private static (bool, string) GetAttribute(XElement e, XName name)
		{
			XAttribute a = e.Attribute(name);
			if (a == null)
			{
				return (false, null);
			}

			return (true, a.Value);
		}
	}
}
