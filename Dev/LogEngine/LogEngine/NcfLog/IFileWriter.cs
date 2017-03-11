using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogEngine
{
    internal interface IFileWriter : IDisposable
    {
        void Initialize();

        void Write(byte[] buffer);
    }
}
