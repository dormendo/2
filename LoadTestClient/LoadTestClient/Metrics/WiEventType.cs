using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	enum WiEventType
	{
		Started,
		Connection,
		Connected,
		RequestStarted,
		RequestSent,
		FirstBlockReceived,
		Received,
		Disconnected,
		RequestSaved,
		ConversionStarted,
		ConversionCompleted,
		ConnectionError,
		Error
	}
}
