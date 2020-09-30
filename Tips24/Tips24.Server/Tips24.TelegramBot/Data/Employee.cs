using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.TelegramBot
{
	public class Employee
	{
		public long TelegramUserId { get; set; }

		public int Id { get; set; }

		public string Phone { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public decimal Balance { get; set; }

		public PlaceData Place { get; set; }

		public GroupData Group { get; set; }

		public bool IsManager { get; set; }

		public bool IsOwner { get; set; }

		public bool IsFired { get; set; }

		public TurnData Turn { get; set; }

		public QrCodeData QrCode { get; set; }

		public DialogSessionData DialogSession { get; set; }

		public bool IsActive
		{
			get
			{
				return !this.IsFired && this.Place.IsActive;
			}
		}

		internal string GetTurnEnterMessage()
		{
			return this.FirstName + " " + this.LastName + " (" + this.Id.ToString() + ") вошёл в смену";
		}

		internal string GetTurnExitMessage()
		{
			return this.FirstName + " " + this.LastName + " (" + this.Id.ToString() + ") вышел из смены";
		}

		public string GetFullName()
		{
			return this.FirstName + " " + this.LastName;
		}
	}
}
