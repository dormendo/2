using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tips24.Dto;
using Tips24.Dto.Auth;

namespace Tips24.Client.Services.Api
{
	public class AuthControllerClient : ControllerClient
	{
		public AuthControllerClient(HttpClient httpClient, ClientInformation clientInfo)
			: base(httpClient, clientInfo, "Auth")
		{
		}

		public async Task<ApiResponse> Logout()
		{
			return await new ApiMethod<Request, object>(
				this._httpClient, this._controllerUriPart, "Logout", AuthHeaderKind.Regular).Call();
		}

		public async Task<ApiResponse<FollowReglinkResponse>> FollowRegistrationLink(FollowReglinkRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<FollowReglinkRequest, FollowReglinkResponse>(
				this._httpClient, this._controllerUriPart, "FollowReglink", AuthHeaderKind.OptionalRegular).Call(data);
		}

		public async Task<ApiResponse<LoginResponse>> Login(LoginRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<LoginRequest, LoginResponse>(
				this._httpClient, this._controllerUriPart, "Login").Call(data);
		}

		public async Task<ApiResponse> JoinPlace(JoinPlaceRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<JoinPlaceRequest, object>(
				this._httpClient, this._controllerUriPart, "JoinPlace", AuthHeaderKind.Regular).Call(data);
		}

		public async Task<ApiResponse<RegisterResponse>> Register(RegisterRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<RegisterRequest, RegisterResponse>(
				this._httpClient, this._controllerUriPart, "Register").Call(data);
		}

		public async Task<ApiResponse<SendVcodeResponse>> SendVerificationCode(SendVcodeRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<SendVcodeRequest, SendVcodeResponse>(
				this._httpClient, this._controllerUriPart, "SendVcode").Call(data);
		}

		public async Task<ApiResponse> CheckVerificationCode(CheckVcodeRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<CheckVcodeRequest, object>(
				this._httpClient, this._controllerUriPart, "CheckVcode").Call(data);
		}

		public async Task<ApiResponse<CheckStatusResponse>> CheckEmployeeStatus(CheckStatusRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<CheckStatusRequest, CheckStatusResponse>(
				this._httpClient, this._controllerUriPart, "CheckStatus", AuthHeaderKind.Regular).Call(data);
		}

		public async Task<ApiResponse<EnterSsResponse>> EnterSecuredSession(EnterSsRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<EnterSsRequest, EnterSsResponse>(
				this._httpClient, this._controllerUriPart, "EnterSs", AuthHeaderKind.Regular).Call(data);
		}

		public async Task<ApiResponse> KeepSecuredSectionAlive(KeepSsAliveRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<KeepSsAliveRequest, object>(
				this._httpClient, this._controllerUriPart, "KeepSsAlive", AuthHeaderKind.Secured).Call(data);
		}

		public async Task<ApiResponse> ExitSecuredSession(ExitSsRequest data)
		{
			data.ClientInfo = this._clientInfo;
			return await new ApiMethod<ExitSsRequest, object>(
				this._httpClient, this._controllerUriPart, "ExitSs", AuthHeaderKind.Secured).Call(data);
		}
	}
}
