using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TestTcp
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
		Task<byte[]> ExecuteWcfProcess(byte[] message);
    }
}
