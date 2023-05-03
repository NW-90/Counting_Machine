using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//sing System.Windows.Forms;

namespace Modbus64
{
    

    internal class CTxRx
    {
        private bool _connected;
        private bool _License;
        private Modbus64.Mode _Mode;
        private byte[] _RxBuf = new byte[600];
        private int _RxBufSize;
        private int _Timeout = 0x3e8;
        private byte[] _TxBuf = new byte[600];
        private int _TxBufSize;
        private TcpClient client;
        private string Error = "";
        private bool firstTime = true;
        private int Time;
        private int timeStamp;
        private ushort TransactionID;
        private UdpClient udpClient;

        private void DiscardReadBuffer()
        {
            switch (this._Mode)
            {
                case Modbus64.Mode.TCP_IP:
                case Modbus64.Mode.RTU_OVER_TCP_IP:
                case Modbus64.Mode.ASCII_OVER_TCP_IP:
                {
                    byte[] buffer = new byte[100];
                    if (this.client.GetStream().CanRead)
                    {
                        while (this.client.GetStream().DataAvailable)
                        {
                            this.client.GetStream().Read(buffer, 0, 100);
                        }
                    }
                    break;
                }
                case Modbus64.Mode.UDP_IP:
                    break;

                default:
                    return;
            }
        }

        public string GetErrorMessage()
        {
            return this.Error;
        }

        public int GetRxBuffer(byte[] byteArray)
        {
            if (byteArray.GetLength(0) >= this._RxBufSize)
            {
                Array.Copy(this._RxBuf, byteArray, this._RxBufSize);
                return this._RxBufSize;
            }
            return 0;
        }

        public int GetTxBuffer(byte[] byteArray)
        {
            if (byteArray.GetLength(0) >= this._TxBufSize)
            {
                Array.Copy(this._TxBuf, byteArray, this._TxBufSize);
                return this._TxBufSize;
            }
            return 0;
        }

        private bool ResponseTimeout()
        {
            int num = Environment.TickCount & 0x7fffffff;
            int millisecondsTimeout = this._Timeout / 100;
            if (millisecondsTimeout == 0)
            {
                millisecondsTimeout = 1;
            }
            while (!this.client.GetStream().DataAvailable)
            {
                if (Math.Abs((int) ((Environment.TickCount & 0x7fffffff) - num)) > this._Timeout)
                {
                    return true;
                }
                Thread.Sleep(millisecondsTimeout);
            }
            return false;
        }

        public void SetClient(TcpClient _client)
        {
            this.client = _client;
        }

        public void SetClient(UdpClient _client)
        {
            this.udpClient = _client;
        }

        public Result TxRx(byte[] TXBuf, int QueryLength, byte[] RXBuf, int ResponseLength)
        {
            Result sUCCESS;
            int num = Environment.TickCount & 0x7fffffff;
            this._TxBufSize = 0;
            this._RxBufSize = 0;
            //if (!this._License)
            //{
            //    if (this.firstTime)
            //    {
            //        this.timeStamp = num;
            //        this.firstTime = false;
            //        MessageBox.Show("WSMBT is running in evaluation mode and will run for 30 minutes.", "WSMBT Modbus Master TCP/IP Component");
            //    }
            //    else if ((num - this.timeStamp) > 0x1b7740)
            //    {
            //        return Result.DEMO_TIMEOUT;
            //    }
            //}
            if ((num - this.Time) < 20)
            {
                Thread.Sleep(4);
            }
            if (!this._connected)
            {
                return Result.ISCLOSED;
            }
            switch (this._Mode)
            {
                case Modbus64.Mode.TCP_IP:
                case Modbus64.Mode.RTU_OVER_TCP_IP:
                case Modbus64.Mode.ASCII_OVER_TCP_IP:
                    if (this.client != null)
                    {
                        try
                        {
                            if (!this.client.Connected)
                            {
                                return Result.ISCLOSED;
                            }
                            goto Label_00EF;
                        }
                        catch (Exception exception)
                        {
                            this.Error = exception.Message;
                            return Result.ISCLOSED;
                        }
                        break;
                    }
                    return Result.ISCLOSED;

                case Modbus64.Mode.UDP_IP:
                    break;

                default:
                    goto Label_00EF;
            }
            if (this.udpClient == null)
            {
                return Result.ISCLOSED;
            }
        Label_00EF:
            switch (this._Mode)
            {
                case Modbus64.Mode.TCP_IP:
                    if (ResponseLength != 0x7fffffff)
                    {
                        sUCCESS = this.TxRxTCP(TXBuf, QueryLength, RXBuf, ResponseLength);
                        break;
                    }
                    return Result.ILLEGAL_FUNCTION;

                case Modbus64.Mode.RTU_OVER_TCP_IP:
                    sUCCESS = this.TxRxRTUOverTCP(TXBuf, QueryLength, RXBuf, ResponseLength);
                    break;

                case Modbus64.Mode.ASCII_OVER_TCP_IP:
                    sUCCESS = this.TxRxASCIIOverTCP(TXBuf, QueryLength, RXBuf, ResponseLength);
                    break;

                case Modbus64.Mode.UDP_IP:
                    if (ResponseLength != 0x7fffffff)
                    {
                        sUCCESS = this.TxRxUDP(TXBuf, QueryLength, RXBuf, ResponseLength);
                        break;
                    }
                    return Result.ILLEGAL_FUNCTION;

                default:
                    sUCCESS = Result.SUCCESS;
                    break;
            }
            this.Time = Environment.TickCount;
            return sUCCESS;
        }

        private Result TxRxASCIIOverTCP(byte[] TXBuf, int QueryLength, byte[] RXBuf, int ResponseLength)
        {
            int offset = 0;
            byte[] nAscii = new byte[0x213];
            byte[] buffer = new byte[0x20b];
            CAscii.RTU2ASCII(TXBuf, QueryLength, nAscii);
            byte n = CAscii.LRC(TXBuf, QueryLength);
            nAscii[0] = 0x3a;
            nAscii[(QueryLength * 2) + 1] = CAscii.Num2Ascii(ByteAccess.HI4BITS(n));
            nAscii[(QueryLength * 2) + 2] = CAscii.Num2Ascii(ByteAccess.LO4BITS(n));
            nAscii[(QueryLength * 2) + 3] = 13;
            nAscii[(QueryLength * 2) + 4] = 10;
            this.DiscardReadBuffer();
            if (this.client.GetStream().CanWrite)
            {
                try
                {
                    this.client.GetStream().Write(nAscii, 0, (QueryLength * 2) + 5);
                    this._TxBufSize = (QueryLength * 2) + 5;
                    Array.Copy(nAscii, this._TxBuf, this._TxBufSize);
                    if (TXBuf[0] == 0)
                    {
                        return Result.SUCCESS;
                    }
                    if (this.client.GetStream().CanRead)
                    {
                        try
                        {
                            do
                            {
                                if (this.ResponseTimeout())
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                int num4 = this.client.GetStream().Read(buffer, offset, 11 - offset);
                                offset += num4;
                                if (offset == 0)
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                            }
                            while ((11 - offset) > 0);
                        }
                        catch (Exception exception2)
                        {
                            this.Error = exception2.Message;
                            return Result.READ;
                        }
                        finally
                        {
                            this._RxBufSize = offset;
                            Array.Copy(buffer, this._RxBuf, this._RxBufSize);
                        }
                        if (CAscii.HiLo4BitsToByte(CAscii.Ascii2Num(buffer[3]), CAscii.Ascii2Num(buffer[4])) > 0x80)
                        {
                            if (!CAscii.VerifyRespLRC(buffer, 11))
                            {
                                return Result.CRC;
                            }
                            return (Result) CAscii.HiLo4BitsToByte(CAscii.Ascii2Num(buffer[5]), CAscii.Ascii2Num(buffer[6]));
                        }
                        if (ResponseLength == 0x7fffffff)
                        {
                            if (CAscii.HiLo4BitsToByte(CAscii.Ascii2Num(buffer[3]), CAscii.Ascii2Num(buffer[4])) != 0x11)
                            {
                                this.DiscardReadBuffer();
                                return Result.RESPONSE;
                            }
                            ResponseLength = CAscii.HiLo4BitsToByte(CAscii.Ascii2Num(buffer[5]), CAscii.Ascii2Num(buffer[6])) + 3;
                        }
                        try
                        {
                            int num5 = (ResponseLength * 2) + 5;
                            do
                            {
                                if (this.ResponseTimeout())
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                int num6 = this.client.GetStream().Read(buffer, offset, num5 - offset);
                                offset += num6;
                            }
                            while ((num5 - offset) > 0);
                            if (!CAscii.VerifyRespLRC(buffer, offset))
                            {
                                return Result.CRC;
                            }
                            if ((buffer[offset - 2] != 13) || (buffer[offset - 1] != 10))
                            {
                                return Result.RESPONSE;
                            }
                            int num3 = (offset - 5) / 2;
                            for (int i = 0; i < num3; i++)
                            {
                                RXBuf[i] = CAscii.HiLo4BitsToByte(CAscii.Ascii2Num(buffer[1 + (i * 2)]), CAscii.Ascii2Num(buffer[2 + (i * 2)]));
                            }
                            return Result.SUCCESS;
                        }
                        catch (Exception exception3)
                        {
                            this.Error = exception3.Message;
                            return Result.READ;
                        }
                        finally
                        {
                            this._RxBufSize = offset;
                            Array.Copy(buffer, this._RxBuf, this._RxBufSize);
                        }
                    }
                    this.Error = "You cannot read from this NetworkStream.";
                    return Result.READ;
                }
                catch (Exception exception)
                {
                    this.Error = exception.Message;
                    return Result.WRITE;
                }
            }
            this.Error = "You cannot write to this NetworkStream.";
            return Result.WRITE;
        }

        private Result TxRxRTUOverTCP(byte[] TXBuf, int QueryLength, byte[] RXBuf, int ResponseLength)
        {
            int offset = 0;
            byte[] buffer = CCRC16.CRC16(TXBuf, QueryLength);
            TXBuf[QueryLength] = buffer[0];
            TXBuf[QueryLength + 1] = buffer[1];
            this.DiscardReadBuffer();
            if (this.client.GetStream().CanWrite)
            {
                try
                {
                    this.client.GetStream().Write(TXBuf, 0, QueryLength + 2);
                    this._TxBufSize = QueryLength + 2;
                    Array.Copy(TXBuf, this._TxBuf, this._TxBufSize);
                    if (TXBuf[0] == 0)
                    {
                        return Result.SUCCESS;
                    }
                    if (this.client.GetStream().CanRead)
                    {
                        try
                        {
                            do
                            {
                                if (this.ResponseTimeout())
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                int num2 = this.client.GetStream().Read(RXBuf, offset, 5 - offset);
                                offset += num2;
                                if (offset == 0)
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                            }
                            while ((5 - offset) > 0);
                        }
                        catch (Exception exception2)
                        {
                            this.Error = exception2.Message;
                            return Result.READ;
                        }
                        finally
                        {
                            this._RxBufSize = offset;
                            Array.Copy(RXBuf, this._RxBuf, this._RxBufSize);
                        }
                        if (RXBuf[1] > 0x80)
                        {
                            byte[] buffer3 = CCRC16.CRC16(RXBuf, 5);
                            if ((buffer3[0] == 0) && (buffer3[1] == 0))
                            {
                                return (Result) RXBuf[2];
                            }
                            return Result.CRC;
                        }
                        if (ResponseLength == 0x7fffffff)
                        {
                            if (RXBuf[1] != 0x11)
                            {
                                this.DiscardReadBuffer();
                                return Result.RESPONSE;
                            }
                            ResponseLength = RXBuf[2] + 3;
                        }
                        try
                        {
                            int num3 = ResponseLength + 2;
                            do
                            {
                                if (this.ResponseTimeout())
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                int num4 = this.client.GetStream().Read(RXBuf, offset, num3 - offset);
                                offset += num4;
                            }
                            while ((num3 - offset) > 0);
                            byte[] buffer2 = CCRC16.CRC16(RXBuf, ResponseLength + 2);
                            if ((buffer2[0] == 0) && (buffer2[1] == 0))
                            {
                                return Result.SUCCESS;
                            }
                            return Result.CRC;
                        }
                        catch (Exception exception3)
                        {
                            this.Error = exception3.Message;
                            return Result.READ;
                        }
                        finally
                        {
                            this._RxBufSize = offset;
                            Array.Copy(RXBuf, this._RxBuf, this._RxBufSize);
                        }
                    }
                    this.Error = "You cannot read from this NetworkStream.";
                    return Result.READ;
                }
                catch (Exception exception)
                {
                    this.Error = exception.Message;
                    return Result.WRITE;
                }
            }
            this.Error = "You cannot write to this NetworkStream.";
            return Result.WRITE;
        }

        private Result TxRxTCP(byte[] TXBuf, int QueryLength, byte[] RXBuf, int ResponseLength)
        {
            int offset = 0;
            this.TransactionID = (ushort) (this.TransactionID + 1);
            this.DiscardReadBuffer();
            for (int i = 0; i < 5; i++)
            {
                this._TxBuf[i] = 0;
            }
            this._TxBuf[0] = (byte) (this.TransactionID >> 8);
            this._TxBuf[1] = (byte) (this.TransactionID & 0xff);
            this._TxBuf[4] = (byte) (QueryLength >> 8);
            this._TxBuf[5] = (byte) (QueryLength & 0xff);
            for (int j = 0; j < QueryLength; j++)
            {
                this._TxBuf[6 + j] = TXBuf[j];
            }
            if (this.client.GetStream().CanWrite)
            {
                try
                {
                    this.client.GetStream().Write(this._TxBuf, 0, QueryLength + 6);
                    this._TxBufSize = QueryLength + 6;
                    if (this.client.GetStream().CanRead)
                    {
                        try
                        {
                            do
                            {
                                if (this.ResponseTimeout())
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                offset += this.client.GetStream().Read(this._RxBuf, offset, (ResponseLength + 6) - offset);
                                if (offset == 0)
                                {
                                    return Result.RESPONSE_TIMEOUT;
                                }
                                if ((offset == 9) && (this._RxBuf[7] > 0x80))
                                {
                                    this._RxBufSize = offset;
                                    return (Result) this._RxBuf[8];
                                }
                            }
                            while (offset < (ResponseLength + 6));
                            if (this.TransactionID != ByteAccess.MakeWord(this._RxBuf[0], this._RxBuf[1]))
                            {
                                this.DiscardReadBuffer();
                                this._RxBufSize = offset;
                                return Result.TRANSACTIONID;
                            }
                            if ((offset - 6) < ResponseLength)
                            {
                                return Result.RESPONSE_TIMEOUT;
                            }
                            for (int k = 0; k < (Math.Min(offset, ResponseLength + 6) - 6); k++)
                            {
                                RXBuf[k] = this._RxBuf[k + 6];
                            }
                            return Result.SUCCESS;
                        }
                        catch (Exception exception2)
                        {
                            this.Error = exception2.Message;
                            return Result.READ;
                        }
                        finally
                        {
                            this._RxBufSize = offset;
                        }
                    }
                    this.Error = "You cannot read from this NetworkStream.";
                    return Result.READ;
                }
                catch (Exception exception)
                {
                    this.Error = exception.Message;
                    return Result.WRITE;
                }
            }
            this.Error = "You cannot write to this NetworkStream.";
            return Result.WRITE;
        }

        private Result TxRxUDP(byte[] TXBuf, int QueryLength, byte[] RXBuf, int ResponseLength)
        {
            byte[] buffer;
            int length = 0;
            this.TransactionID = (ushort) (this.TransactionID + 1);
            this.DiscardReadBuffer();
            for (int i = 0; i < 5; i++)
            {
                this._TxBuf[i] = 0;
            }
            this._TxBuf[0] = (byte) (this.TransactionID >> 8);
            this._TxBuf[1] = (byte) (this.TransactionID & 0xff);
            this._TxBuf[4] = (byte) (QueryLength >> 8);
            this._TxBuf[5] = (byte) (QueryLength & 0xff);
            for (int j = 0; j < QueryLength; j++)
            {
                this._TxBuf[6 + j] = TXBuf[j];
            }
            try
            {
                this.udpClient.Send(this._TxBuf, QueryLength + 6);
            }
            catch (Exception exception)
            {
                this.Error = exception.Message;
                return Result.WRITE;
            }
            this._TxBufSize = QueryLength + 6;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                buffer = this.udpClient.Receive(ref remoteEP);
            }
            catch (Exception exception2)
            {
                this.Error = exception2.Message;
                return Result.READ;
            }
            length = buffer.Length;
            this._RxBufSize = length;
            Array.Copy(buffer, this._RxBuf, this._RxBufSize);
            if ((length >= 9) && (buffer[7] > 0x80))
            {
                this._RxBufSize = length;
                return (Result) buffer[8];
            }
            if ((length - 6) < ResponseLength)
            {
                return Result.RESPONSE_TIMEOUT;
            }
            if (this.TransactionID != ByteAccess.MakeWord(buffer[0], buffer[1]))
            {
                this.udpClient.Receive(ref remoteEP);
                return Result.TRANSACTIONID;
            }
            for (int k = 0; k < (Math.Min(length, ResponseLength + 6) - 6); k++)
            {
                RXBuf[k] = buffer[k + 6];
            }
            return Result.SUCCESS;
        }

        public bool connected
        {
            get
            {
                return this._connected;
            }
            set
            {
                this._connected = value;
            }
        }

        public bool License
        {
            get
            {
                return this._License;
            }
            set
            {
                this._License = value;
            }
        }

        public Modbus64.Mode Mode
        {
            get
            {
                return this._Mode;
            }
            set
            {
                this._Mode = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this._Timeout;
            }
            set
            {
                this._Timeout = value;
            }
        }
    }
}

