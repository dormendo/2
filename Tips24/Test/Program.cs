using System;
using System.IO;
using Newtonsoft.Json;
using Tips24.PaymentService;
using Tips24.PaymentService.YksSms;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			string responseJson;
			using (StreamReader sr = new StreamReader("response.json"))
			{
				responseJson = sr.ReadToEnd();
			}

			PaymentDoc payment = JsonConvert.DeserializeObject<PaymentDoc>(responseJson, Startup.JsonSettings);
		}
	}
}
