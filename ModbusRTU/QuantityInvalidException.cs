namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class QuantityInvalidException : ModbusException
    {
        public QuantityInvalidException()
        {
        }

        public QuantityInvalidException(string message) : base(message)
        {
        }

        protected QuantityInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public QuantityInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

