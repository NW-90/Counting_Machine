namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class FunctionCodeNotSupportedException : ModbusException
    {
        public FunctionCodeNotSupportedException()
        {
        }

        public FunctionCodeNotSupportedException(string message) : base(message)
        {
        }

        protected FunctionCodeNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public FunctionCodeNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

