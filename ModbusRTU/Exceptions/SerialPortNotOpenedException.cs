namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class SerialPortNotOpenedException : ModbusException
    {
        public SerialPortNotOpenedException()
        {
        }

        public SerialPortNotOpenedException(string message) : base(message)
        {
        }

        protected SerialPortNotOpenedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SerialPortNotOpenedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

