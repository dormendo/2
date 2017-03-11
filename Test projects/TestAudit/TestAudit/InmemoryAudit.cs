using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public class InmemoryAudit : AuditBase
    {
        public InmemoryAudit()
            : base("dbo.WriteAuditRecord")
        {
        }
    }
}
