﻿using System.Web.Http;

namespace WebApp1
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{

			// Web API routes
			config.MapHttpAttributeRoutes();
		}
	}
}