namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class ConnectionException : ModbusException
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

