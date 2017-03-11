using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLog
{
    public abstract class LogBase
    {
        public abstract void Write(string message);
    }
}
