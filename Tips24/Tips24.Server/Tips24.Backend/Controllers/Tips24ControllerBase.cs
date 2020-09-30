using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Tips24.Backend.Controllers
{
	public abstract class Tips24ControllerBase : ControllerBase
	{
		protected T GetHandler<T>()
		{
			return (T)this.HttpContext.RequestServices.GetService(typeof(T));
		}
	}
}
