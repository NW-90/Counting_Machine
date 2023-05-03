namespace DiawModbus
{
    using System;

    public class ModbusProtocol
    {
        public byte byteCount;
        public ushort crc;
        public byte errorCode;
        public byte exceptionCode;
        public byte functionCode;
        public ushort length;
        public ushort protocolIdentifier;
        public ushort quantity;
        public ushort quantityRead;
        public ushort quantityWrite;
        public ushort[] receiveCoilValues;
        public ushort[] receiveRegisterValues;
        public bool request;
        public bool response;
        public bool[] sendCoilValues;
        public short[] sendRegisterValues;
        public ushort startingAddressRead;
        public ushort startingAddressWrite;
        public ushort startingAdress;
        public DateTime timeStamp;
        public ushort transactionIdentifier;
        public byte unitIdentifier;

        public enum ProtocolType
        {
            ModbusTCP,
            ModbusUDP,
            ModbusRTU
        }
    }
}

