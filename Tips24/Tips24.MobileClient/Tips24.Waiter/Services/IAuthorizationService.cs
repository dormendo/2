using System;
using System.Threading.Tasks;

namespace Tips24.Client.Services
{
    public interface IAuthorizationService
    {
		string GetRegularAuthKey();

		void SetRegularAuthKey(string authKey);

		string GetSecuredAuthKey();

		void SetSecuredAuthKey(string authKey);
	}
}
