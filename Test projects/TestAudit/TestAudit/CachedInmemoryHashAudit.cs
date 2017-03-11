using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public class CachedInmemoryHashAudit : CachedAuditBase
    {
        public CachedInmemoryHashAudit()
            : base("dbo.WriteAuditRecord2")
        {
        }
    }
}
