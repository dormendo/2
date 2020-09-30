using System;
using System.Collections.Generic;
using System.Text;

namespace LoadTestClient
{
	/// <summary>
	/// ��������� ������ ������
	/// </summary>
	public enum CsvReadLineResult
	{
		/// <summary>
		/// ������ ��������� �������
		/// </summary>
		Success = 0,

		/// <summary>
		/// ������ ��� ������ ������
		/// </summary>
		Error = 1,

		/// <summary>
		/// ����� �����
		/// </summary>
		Eof = 2
	}
}
