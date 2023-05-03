namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class StartingAddressInvalidException : ModbusException
    {
        public StartingAddressInvalidException()
        {
        }

        public StartingAddressInvalidException(string message) : base(message)
        {
        }

        protected StartingAddressInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public StartingAddressInvalidException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

