using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public class InmemoryHashAudit : AuditBase
    {
        public InmemoryHashAudit()
            : base("dbo.WriteAuditRecord2")
        {
        }
    }
}
