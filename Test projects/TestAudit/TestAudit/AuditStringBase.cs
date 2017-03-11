using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAudit
{
    public abstract class AuditStringBase : AuditBase
    {
        protected AuditStringBase(string spName)
            : base(spName)
        {
        }

        protected override void AddMessageParam(System.Data.SqlClient.SqlCommand command, string message)
        {
            command.Parameters.Add("@message", SqlDbType.VarChar, 2000).Value = message;
        }
    }
}
