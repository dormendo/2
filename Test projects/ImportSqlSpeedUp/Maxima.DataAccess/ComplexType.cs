using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Maxima.DataAccess
{
    /// <summary>
    /// Represents parameter of user-defined table type in db schema
    /// </summary>
    public abstract class ComplexType
    {
        private readonly List<IReadOnlyCollection<object>> _rows = new List<IReadOnlyCollection<object>>();
        private readonly string _typeName;

        protected ComplexType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException();
            _typeName = typeName;
        }

        public IEnumerable<IReadOnlyCollection<object>> Rows { get { return _rows; } }

        public void AddRow(IReadOnlyCollection<object> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            _rows.Add(values.ToArray());
        }

        internal DataTable ToDataTable()
        {
            var dt = new DataTable(_typeName);
            foreach (var column in GetColumns())
                dt.Columns.Add(column.Key, column.Value);
            Debug.Assert(Rows.All(r => r.Count == dt.Columns.Count));
            foreach (object[] rowValues in Rows) // line 30: all items in Rows are of type object[]
            {
                dt.Rows.Add(rowValues);
            }
            return dt;
        }

        protected abstract IList<KeyValuePair<string, Type>> GetColumns();
    }
}