using System;

namespace Modbus64
{
    

    internal class ByteAccess
    {
        public static byte HI4BITS(byte n)
        {
            return (byte) ((n >> 4) & 15);
        }

        public static byte HiByte(ushort nValue)
        {
            return (byte) (nValue >> 8);
        }

        public static ushort HiWord(uint nValue)
        {
            return (ushort) (nValue >> 0x10);
        }

        public static byte LO4BITS(byte n)
        {
            return (byte) (n & 15);
        }

        public static byte LoByte(ushort nValue)
        {
            return (byte) (nValue & 0xff);
        }

        public static ushort LoWord(uint nValue)
        {
            return (ushort) (nValue & 0xffff);
        }

        public static uint MakeLong(ushort high, ushort low)
        {
            return (uint) ((low & 0xffff) | ((high & 0xffff) << 0x10));
        }

        public static ushort MakeWord(byte high, byte low)
        {
            return (ushort) ((low & 0xff) | ((high & 0xff) << 8));
        }
    }
}

