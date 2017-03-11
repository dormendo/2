using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Geolocation.Data;

namespace WebApp1
{
	public class MvcApplication : HttpApplication
	{
		public static IDataProvider DbDataProvider
		{
			get;
			private set;
		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			string dbFile = ConfigurationManager.AppSettings["DatabaseFile"];
			bool fastAvailability = Convert.ToBoolean(ConfigurationManager.AppSettings["DatabaseAvailableFast"]);
			DataProvider dp = new DataProvider(dbFile, fastAvailability);
			dp.Initialize();
			DbDataProvider = dp;
		}
	}
}
