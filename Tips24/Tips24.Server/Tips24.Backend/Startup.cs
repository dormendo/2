using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tips24.Backend
{
	public class Startup
	{
		public static JsonSerializerSettings JsonSettings;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			SqlServer sqlServer = new SqlServer(Configuration);
			services.AddSingleton(sqlServer);

			services.AddSingleton<EmployeeAuthenticator>();
			services.AddTransient<Auth.Logout>();
			services.AddTransient<Auth.FollowRegistrationLink>();
			services.AddTransient<Auth.SendVerificationCode>();
			services.AddTransient<Auth.CheckVerificationCode>();
			services.AddTransient<Auth.Register>();
			services.AddTransient<Auth.Login>();
			services.AddTransient<Auth.JoinPlace>();
			services.AddTransient<Auth.CheckEmployeeStatus>();
			services.AddTransient<Auth.EnterSecuredSession>();
			services.AddTransient<Auth.KeepSecuredSessionAlive>();
			services.AddTransient<Auth.ExitSecuredSession>();

			JsonSettings = new JsonSerializerSettings();
			JsonSettings.MissingMemberHandling = MissingMemberHandling.Error;
			JsonSettings.FloatFormatHandling = FloatFormatHandling.Symbol;
			JsonSettings.FloatParseHandling = FloatParseHandling.Decimal;
			JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() => JsonSettings);
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
