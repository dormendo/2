using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Client.Services.Api
{
    public enum ApiResponseKind : byte
    {
		Success = 0,
		Error = 1,
		NotAuthorized = 2
    }
}
