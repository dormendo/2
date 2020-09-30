using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	enum EventType
	{
		Start,
		Complete,

		LoadConfigStart,
		LoadConfigEnd,

		PrepareTestStart,
		PrepareTestEnd,

		CheckConfigStart,
		CheckConfigEnd,
		LoadDataFileStart,
		LoadDataFileEnd,
		LoadTemplatesStart,
		LoadTemplatesEnd,
		PrepareTraceFolderStart,
		PrepareTraceFolderEnd,
		DeleteTraceFolderStart,
		DeleteTraceFolderEnd,

		RunTestStart,
		RunTestEnd,

		CreateThreadsAndConnectStart,
		CreateThreadsAndConnectEnd,

		SendRequestsStart,
		SendRequestsEnd,

		Tcp2HttpResponsesConversionStart,
		Tcp2HttpResponsesConversionEnd,

		Error
	}
}
