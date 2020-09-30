using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Tips24.PayoutService
{
	public class Startup
	{
		public static JsonSerializerSettings JsonSettings;

		public static PayoutServiceConfiguration Config;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

			Config = new PayoutServiceConfiguration();
			configuration.GetSection("PayoutService").Bind(Config);
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			SqlServer sqlServer = new SqlServer(Configuration);
			services.AddSingleton(sqlServer);

			JsonSettings = new JsonSerializerSettings();
			JsonSettings.MissingMemberHandling = MissingMemberHandling.Error;
			JsonSettings.FloatFormatHandling = FloatFormatHandling.Symbol;
			JsonSettings.FloatParseHandling = FloatParseHandling.Decimal;
			JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() => JsonSettings);

			services.AddHttpsRedirection(options =>
			{
				options.RedirectStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status307TemporaryRedirect;
				options.HttpsPort = 5501;
			});

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
