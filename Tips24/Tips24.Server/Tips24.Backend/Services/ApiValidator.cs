using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Backend
{
	public static class ApiValidator
	{
		public static bool ValidatePhone(string phone, out ErrorResponse error)
		{
			error = null;
			if (string.IsNullOrEmpty(phone))
			{
				error = new ErrorResponse("input_phone_notset", "Номер телефона не задан");
				return false;
			}

			bool phoneIsCorrect = phone.Length == 10;

			if (phoneIsCorrect)
			{
				for (int i = 0; i < phone.Length; i++)
				{
					if (!char.IsDigit(phone[i]))
					{
						phoneIsCorrect = false;
						break;
					}
				}
			}

			if (!phoneIsCorrect)
			{
				error = new ErrorResponse("input_phone_incorrect", "Номер телефона задан в неверном формате");
				return false;
			}

			return true;
		}

		public static bool ValidatePlaceId(int placeId, out ErrorResponse error)
		{
			error = null;
			if (placeId <= 0)
			{
				error = new ErrorResponse(placeId == 0 ? "input_place_notset" : "input_incorrect_place", "Заведение не задано");
			}

			return error == null;
		}

		public static bool ValidateVerificationId(Guid verificationId, out ErrorResponse error)
		{
			error = null;
			if (verificationId == Guid.Empty)
			{
				error = new ErrorResponse("input_verificationid_notset", "Идентификатор запроса верификации не задан");
			}

			return error == null;
		}

		public static bool ValidateVerificationCode(string code, out ErrorResponse error)
		{
			error = null;
			if (string.IsNullOrEmpty(code))
			{
				error = new ErrorResponse("input_code_notset", "Код верификации не задан");
				return false;
			}

			bool codeFormatIsWrong = code.Length != 6;
			if (!codeFormatIsWrong)
			{
				for (int i = 0; i < code.Length; i++)
				{
					if (!char.IsDigit(code[i]))
					{
						codeFormatIsWrong = true;
						break;
					}
				}
			}

			if (codeFormatIsWrong)
			{
				error = new ErrorResponse("input_code_incorrect", "Код верификации имеет некорректный формат");
			}

			return error == null;
		}

		public static bool ValidateLinkParameter(Guid linkParameter, out ErrorResponse error)
		{
			error = null;
			if (linkParameter == Guid.Empty)
			{
				error = new ErrorResponse("input_linkparameter_notset", "Идентификатор ссылки регистрации не задан");
			}

			return error == null;
		}

		public static bool ValidatePinCode(string code, out ErrorResponse error)
		{
			error = null;
			if (string.IsNullOrEmpty(code))
			{
				error = new ErrorResponse("input_pincode_notset", "Пин-код не задан");
				return false;
			}

			bool codeFormatIsWrong = code.Length != 4;
			if (!codeFormatIsWrong)
			{
				for (int i = 0; i < code.Length; i++)
				{
					if (!char.IsDigit(code[i]))
					{
						codeFormatIsWrong = true;
						break;
					}
				}
			}

			if (codeFormatIsWrong)
			{
				error = new ErrorResponse("input_pincode_incorrect", "Пин-код имеет некорректный формат");
			}

			return error == null;
		}

		public static bool ValidateFirstName(string firstName, out ErrorResponse error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(firstName))
			{
				error = new ErrorResponse("input_firstname_notset", "Имя не задано");
				return false;
			}

			return error == null;
		}

		public static bool ValidateLastName(string lastName, out ErrorResponse error)
		{
			error = null;
			if (string.IsNullOrWhiteSpace(lastName))
			{
				error = new ErrorResponse("input_lastname_notset", "Фамилия не задана");
				return false;
			}

			return error == null;
		}
	}
}
