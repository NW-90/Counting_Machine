namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class ModbusException : Exception
    {
        public ModbusException()
        {
        }

        public ModbusException(string message) : base(message)
        {
        }

        protected ModbusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ModbusException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

