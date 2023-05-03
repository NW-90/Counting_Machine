namespace DiawModbus.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class CRCCheckFailedException : ModbusException
    {
        public CRCCheckFailedException()
        {
        }

        public CRCCheckFailedException(string message) : base(message)
        {
        }

        protected CRCCheckFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CRCCheckFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

