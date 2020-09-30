using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tips24.Backend.Auth;
using Tips24.DataAccess;
using Tips24.Dto.Auth;

namespace Tips24.Backend.Controllers
{
	[ApiController]
	public class AuthController : Tips24ControllerBase
	{
		/// <summary>
		/// Метод вызывается для удаления из таблицы Auth обоих ключей авторизации.
		/// На клиенте это должно приводить к удалению обоих ключей из защищённого хранилища и к возврату на страницу логина.
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/Logout")]
		public async Task<IActionResult> Logout(Dto.Request data)
		{
			return await GetHandler<Logout>().Handle(this.Request, data);
		}

		/// <summary>
		/// Менеджер рассылает официантам ссылку на регистрацию и привязку к заведению. Официант проходит по этой ссылке на своём мобильном устройстве.
		/// Приложение официанта открывается и обрабатывает эту ссылку, получает из неё единственный параметр (Guid), и отправляет его в этот метод.
		/// Метод возвращает информацию о заведении, которое выпустило ссылку. Если пользователь был авторизован на клиенте, клиент отправляет в данный метод постоянный ключ авторизации.
		/// В этом случае метод также возвращает информацию об официанте, в том числе флаг уволенности.
		/// Когда клиент получает ответ этого метода, он действует по одному из трёх сценариев:
		/// 1. Если он авторизован и не уволен, нужно отобразить страницу с выбором способа оплаты.
		/// 2. Если он авторизован и уволен, нужно отобразить страницу с предложением присоединиться к новому заведению без повторной регистрации.
		/// 3. Если он не авторизован - отобразить страницу регистрации (и присоединения к заведению)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/FollowReglink")]
		public async Task<IActionResult> FollowRegistrationLink(FollowReglinkRequest data)
		{
			return await GetHandler<FollowRegistrationLink>().Handle(this.Request, data);
		}

		/// <summary>
		/// Метод принимает номер телефона без +7, 8, скобок, дефисов и пробелов, только 10 цифр. И пин-код. Возвращает постоянный ключ авторизации
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/Login")]
		public async Task<IActionResult> Login(LoginRequest data)
		{
			return await GetHandler<Login>().Handle(data);
		}

		/// <summary>
		/// Если уволенный авторизованный пользователь открывает ссылку на присоединение к заведению, он может присоединиться к заведению, выпустившему ссылку.
		/// Метод принимает Guid ссылки и идентификатор текущего заведения официанта (удалю в ближайшее время)
		/// Метод ничего не возвращает, после него сразу нужно вызвать CheckEmployeeStatus (исправлю в ближайшее время)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/JoinPlace")]
		public async Task<IActionResult> JoinPlace(JoinPlaceRequest data)
		{
			return await GetHandler<JoinPlace>().Handle(this.Request, data);
		}

		/// <summary>
		/// Метод принимает данные для регистрации официанта. Возвращает постоянный ключ авторизации (пользователь сразу становится авторизованным)
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/Register")]
		public async Task<IActionResult> Register(RegisterRequest data)
		{
			return await GetHandler<Register>().Handle(data);
		}

		/// <summary>
		/// Метод создаёт заявку на отправку СМС для верификации телефона, возвращает идентификатор заявки
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/SendVcode")]
		public async Task<IActionResult> SendVerificationCode(SendVcodeRequest data)
		{
			return await GetHandler<SendVerificationCode>().Handle(data);
		}

		/// <summary>
		/// Метод проверяет код из СМС
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/CheckVcode")]
		public async Task<IActionResult> CheckVerificationCode(CheckVcodeRequest data)
		{
			return await GetHandler<CheckVerificationCode>().Handle(data);
		}

		/// <summary>
		/// Так называемый сетевой пинг
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/CheckEmployeeStatus")]
		public async Task<IActionResult> CheckEmployeeStatus(CheckStatusRequest data)
		{
			return await GetHandler<CheckEmployeeStatus>().Handle(this.Request, data);
		}

		/// <summary>
		/// Авторизация в защищённую сессию. Возвращает ключ авторизации в защищённую сессию. Ключ без обращений к функционалу защищенной области действует в течение 10 минут.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/EnterSs")]
		public async Task<IActionResult> EnterSecuredSession(EnterSsRequest data)
		{
			return await GetHandler<EnterSecuredSession>().Handle(this.Request, data);
		}

		/// <summary>
		/// Продляет защищённую сессию. Можно вызывать, например, 1 раз в минуту в фоновом потоке.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/KeepSsAlive")]
		public async Task<IActionResult> KeepSecuredSectionAlive(KeepSsAliveRequest data)
		{
			return await GetHandler<KeepSecuredSessionAlive>().Handle(this.Request, data);
		}

		/// <summary>
		/// Выход из защищённой сессии
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("Auth/ExitSs")]
		public async Task<IActionResult> ExitSecuredSession(ExitSsRequest data)
		{
			return await GetHandler<ExitSecuredSession>().Handle(this.Request, data);
		}
	}
}
