using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;

namespace TestWebApi.Controllers
{
	public class ValuesController : ApiController
	{
		[Route("PositionList.asmx/WSDL")]
		[HttpGet]
		public HttpResponseMessage Wsdl()
		{
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { Private = true, MaxAge = TimeSpan.Zero };
			FileStream fs = new FileStream(HostingEnvironment.MapPath("~/App_Data/wsdl.xml"), FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
			StreamContent content = new StreamContent(fs);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
			response.Content = content;
			return response;
		}

		[Route("PositionList.asmx/BigXml")]
		[HttpGet]
		public HttpResponseMessage BigXml()
		{
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { Private = true, MaxAge = TimeSpan.Zero };
			FileStream fs = new FileStream(HostingEnvironment.MapPath("~/App_Data/Big.xml"), FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
			StreamContent content = new StreamContent(fs);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/soap+xml") { CharSet = "utf-8" };
			response.Content = content;
			return response;
		}

		[Route("PositionList.asmx/Generate")]
		[HttpGet]
		public HttpResponseMessage Generate()
		{
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { Private = true, MaxAge = TimeSpan.Zero };

			Psc psc = new Psc() { Count = 100 * 1024 * 1024 / 16 };
			PushStreamContent content = new PushStreamContent(psc.OnStreamAvailable);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			response.Content = content;
			return response;
		}

		[Route("PositionList.asmx/Generate2")]
		[HttpGet]
		public HttpResponseMessage Generate2()
		{
			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
			response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { Private = true, MaxAge = TimeSpan.Zero };
			string path = HostingEnvironment.MapPath("~/App_Data/" + Guid.NewGuid() + ".xml");
			using (FileStream ws = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024*1024, FileOptions.WriteThrough))
			{
				Psc psc = new Psc { Count = 100 * 1024 * 1024 / 16 };
				psc.OnStreamAvailable2(ws, null, null);
			}

			FileStream fs = new FileStream(HostingEnvironment.MapPath("~/App_Data/Big.xml"), FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
			StreamContent content = new StreamContent(fs);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/soap+xml") { CharSet = "utf-8" };
			response.Content = content;
			return response;
		}

		private class Psc
		{
			public int Count { get; set; }

			public void OnStreamAvailable(Stream stream, HttpContent arg2, TransportContext arg3)
			{
				using (BufferedStream bs = new BufferedStream(stream, 1024 * 1024))
				{
					for (int i = 0; i < this.Count; i++)
					{
						byte[] ba = Guid.NewGuid().ToByteArray();
						bs.Write(ba, 0, 16);
					}
					stream.Flush();
				}

				stream.Flush();
				stream.Close();
			}

			public void OnStreamAvailable2(Stream stream, HttpContent arg2, TransportContext arg3)
			{
				for (int i = 0; i < this.Count; i++)
				{
					byte[] ba = Guid.NewGuid().ToByteArray();
					stream.Write(ba, 0, 16);
				}

				stream.Flush();
				stream.Close();
			}

		}
	}
}
