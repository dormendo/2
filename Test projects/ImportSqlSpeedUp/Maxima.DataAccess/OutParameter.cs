using System;
using System.Data;
using System.Data.Common;

namespace Maxima.DataAccess
{
    public sealed class OutParameter<T> : OutParameterBase
    {
        public OutParameter(T initialValue)
            : base(initialValue)
        {
        }

        public T Value
        {
            get
            {
                return (T) ValueInternal;
            }
        }
    }

    public abstract class OutParameterBase
    {
        protected object ValueInternal;
        private DbParameter _param;

        internal OutParameterBase(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Initial value required to infer db type of a parameter");
            ValueInternal = value;
        }

        internal void BindToParameter(DbParameter param)
        {
            if (param == null)
                throw new ArgumentNullException("param");
            param.Direction = ParameterDirection.Output;
            param.Value = ValueInternal;
            _param = param;
        }

        internal void ReadOutputValue()
        {
            if (_param == null)
                throw new InvalidOperationException("BindToParameter must be called prior to reading an output value");
            var val = _param.Value;
            if (val != DBNull.Value && val.GetType() != ValueInternal.GetType())
                throw new ArgumentException("Output value must be of type " + ValueInternal.GetType());
            ValueInternal = val == DBNull.Value ? null : val;
            _param = null;
        }
    }
}