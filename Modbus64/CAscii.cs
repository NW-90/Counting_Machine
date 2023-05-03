using System;

namespace Modbus64
{
  

    internal class CAscii
    {
        public static byte Ascii2Num(byte nChar)
        {
            if ((nChar >= 0x30) && (nChar <= 0x39))
            {
                return (byte) (nChar - 0x30);
            }
            if ((nChar >= 0x41) && (nChar <= 70))
            {
                return (byte) ((nChar - 0x41) + 10);
            }
            return 0;
        }

        public static byte HiLo4BitsToByte(byte nHi, byte nLo)
        {
            return (byte) (((15 & nHi) << 4) | (15 & nLo));
        }

        public static byte LRC(byte[] nMsg, int DataLen)
        {
            byte num = 0;
            for (int i = 0; i < DataLen; i++)
            {
                num = (byte) (num + nMsg[i]);
            }
            return num;
        }

        public static byte LRCASCII(byte[] MsgASCII, int DataLen)
        {
            byte num = 0;
            byte num2 = 0;
            int num3 = (DataLen - 5) / 2;
            for (int i = 0; i < num3; i++)
            {
                num2 = HiLo4BitsToByte(Ascii2Num(MsgASCII[1 + (i * 2)]), Ascii2Num(MsgASCII[(1 + (i * 2)) + 1]));
                num = (byte) (num + num2);
            }
            return num;
        }

        public static byte Num2Ascii(byte nNum)
        {
            if (nNum <= 9)
            {
                return (byte) (nNum + 0x30);
            }
            if ((nNum >= 10) && (nNum <= 15))
            {
                return (byte) ((nNum - 10) + 0x41);
            }
            return 0x30;
        }

        public static void RTU2ASCII(byte[] nRtu, int Size, byte[] nAscii)
        {
            for (int i = 0; i < Size; i++)
            {
                nAscii[1 + (i * 2)] = Num2Ascii(ByteAccess.HI4BITS(nRtu[i]));
                nAscii[(1 + (i * 2)) + 1] = Num2Ascii(ByteAccess.LO4BITS(nRtu[i]));
            }
        }

        public static bool VerifyRespLRC(byte[] Resp, int Length)
        {
            if (Length < 5)
            {
                return false;
            }
            return (LRCASCII(Resp, Length) == HiLo4BitsToByte(Ascii2Num(Resp[Length - 4]), Ascii2Num(Resp[Length - 3])));
        }
    }
}

