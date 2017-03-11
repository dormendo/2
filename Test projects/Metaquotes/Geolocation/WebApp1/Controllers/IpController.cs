using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Geolocation.Data;

namespace WebApp1.Controllers
{
	public class IpController : ApiController
	{
		private static byte[] _ipNotFoundMessage;

		static IpController()
		{
			_ipNotFoundMessage = Encoding.UTF8.GetBytes("IP-адрес не найден");
		}


		[HttpGet]
		[Route("ip/location")]
		public HttpResponseMessage Location(string ip)
		{
			byte[] result = MvcApplication.DbDataProvider.GetLocationByIp(ip);
			HttpResponseMessage message;
			if (result == null)
			{
				message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				HttpContent content = new ByteArrayContent(_ipNotFoundMessage);
				content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf-8" };
				message.Content = content;
			}
			else
			{
				message = new HttpResponseMessage();
				HttpContent content = new ByteArrayContent(result);
				content.Headers.ContentType = new MediaTypeHeaderValue("text/json") { CharSet = "utf-8" };
				message.Content = content;
			}

			return message;
		}
	}
}
