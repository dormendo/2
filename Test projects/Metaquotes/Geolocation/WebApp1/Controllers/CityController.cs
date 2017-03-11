using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;

namespace WebApp1.Controllers
{
	public class CityController : ApiController
	{
		private static byte[] _cityNotFoundMessage;

		static CityController()
		{
			_cityNotFoundMessage = Encoding.UTF8.GetBytes("Город не найден");
		}


		[HttpGet]
		[Route("city/locations")]
		public HttpResponseMessage Locations(string city)
		{
			byte[] result = MvcApplication.DbDataProvider.GetLocationsByCity(city);
			HttpResponseMessage message;
			if (result == null)
			{
				message = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				HttpContent content = new ByteArrayContent(_cityNotFoundMessage);
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
