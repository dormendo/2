using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.SbReg.Csv
{
	/// <summary>
	/// ��������� ������ ����
	/// </summary>
	public enum CsvReadFieldResult
	{
		/// <summary>
		/// ���� ��������� �������
		/// </summary>
		Success = 0,

		/// <summary>
		/// ��������� ������ ������������ ���� ������ ���� ��� �������������
		/// </summary>
		QuoteInNqField = 1,

		/// <summary>
		/// ���� � �������������� �������� ������� ����� ��������� ������������
		/// </summary>
		DelimiterExpected = 2,

		/// <summary>
		/// �� ������ ����������� ������������
		/// </summary>
		QuoteNotFound = 3
	}
}
