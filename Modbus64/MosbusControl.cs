using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Modbus64
{
  

    public class Modbus64Control : Component
    {
        private int _ConnectTimeout;
        private Modbus64.Mode _Mode;
        private int _ResponseTimeout;
        private TcpClient client;
        private IContainer components;
        private string Error;
        private CModbus Modbus;
        private Result Res;
        private CTxRx TxRx;
        private UdpClient udpClient;

        public Modbus64Control()
        {
            this._ResponseTimeout = 0x3e8;
            this._ConnectTimeout = 0x3e8;
            this.Error = "";
            this.InitializeComponent();
            this.Modbus = new CModbus(this.TxRx = new CTxRx());
            this.TxRx.Mode = Modbus64.Mode.TCP_IP;
            this.TxRx.connected = false;
        }

        public Modbus64Control(IContainer container)
        {
            this._ResponseTimeout = 0x3e8;
            this._ConnectTimeout = 0x3e8;
            this.Error = "";
            container.Add(this);
            this.InitializeComponent();
            this.Modbus = new CModbus(this.TxRx = new CTxRx());
            this.TxRx.Mode = Modbus64.Mode.TCP_IP;
            this.TxRx.connected = false;
        }

        public void Close()
        {
            if (this.client != null)
            {
                this.client.Close();
            }
            if (this.udpClient != null)
            {
                this.udpClient.Close();
            }
            this.TxRx.connected = false;
        }

        public Result Connect(string ipAddress, int port)
        {
            if (this.TxRx.connected)
            {
                return Result.SUCCESS;
            }
            switch (this._Mode)
            {
                case Modbus64.Mode.TCP_IP:
                case Modbus64.Mode.RTU_OVER_TCP_IP:
                case Modbus64.Mode.ASCII_OVER_TCP_IP:
                    this.client = new TcpClient();
                    this.TxRx.SetClient(this.client);
                    this.TxRx.Timeout = this._ResponseTimeout;
                    this.client.SendTimeout = 0x7d0;
                    this.client.ReceiveTimeout = this._ResponseTimeout;
                    try
                    {
                        IAsyncResult asyncResult = this.client.BeginConnect(ipAddress, port, null, null);
                        WaitHandle asyncWaitHandle = asyncResult.AsyncWaitHandle;
                        if (!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds((double) this._ConnectTimeout), false))
                        {
                            Result result2;
                            this.client.Close();
                            this.Res = result2 = Result.CONNECT_TIMEOUT;
                            return result2;
                        }
                        this.client.EndConnect(asyncResult);
                        asyncWaitHandle.Close();
                    }
                    catch (Exception exception)
                    {
                        this.Error = exception.Message;
                        return (this.Res = Result.CONNECT_ERROR);
                    }
                    this.TxRx.Mode = this._Mode;
                    this.TxRx.connected = true;
                    break;

                case Modbus64.Mode.UDP_IP:
                {
                    this.udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    try
                    {
                        this.udpClient.Connect(endPoint);
                    }
                    catch (Exception exception2)
                    {
                        this.Error = exception2.Message;
                        return (this.Res = Result.CONNECT_ERROR);
                    }
                    this.TxRx.SetClient(this.udpClient);
                    this.TxRx.Timeout = this._ResponseTimeout;
                    this.udpClient.Client.SendTimeout = 0x7d0;
                    this.udpClient.Client.ReceiveTimeout = this._ResponseTimeout;
                    this.TxRx.Mode = this._Mode;
                    this.TxRx.connected = true;
                    break;
                }
            }
            return (this.Res = Result.SUCCESS);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public short[] FloatToRegisters(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            short[] numArray1 = new short[2];
            numArray1[1] = BitConverter.ToInt16(bytes, 0);
            numArray1[0] = BitConverter.ToInt16(bytes, 2);
            return numArray1;
        }

        public string GetLastErrorString()
        {
            switch (this.Res)
            {
                case Result.SUCCESS:
                    return "Success";

                case Result.ILLEGAL_FUNCTION:
                    return "Illegal function.";

                case Result.ILLEGAL_DATA_ADDRESS:
                    return "Illegal data address.";

                case Result.ILLEGAL_DATA_VALUE:
                    return "Illegal data value.";

                case Result.SLAVE_DEVICE_FAILURE:
                    return "Slave device failure.";

                case Result.ACKNOWLEDGE:
                    return "Acknowledge.";

                case Result.SLAVE_DEVICE_BUSY:
                    return "Slave device busy.";

                case Result.NEGATIVE_ACKNOWLEDGE:
                    return "Negative acknowledge.";

                case Result.MEMORY_PARITY_ERROR:
                    return "Memory parity error.";

                case Result.CONNECT_ERROR:
                    return this.Error;

                case Result.CONNECT_TIMEOUT:
                    return "Could not connect within the specified time";

                case Result.WRITE:
                    return ("Write error. " + this.TxRx.GetErrorMessage());

                case Result.READ:
                    return ("Read error. " + this.TxRx.GetErrorMessage());

                case Result.RESPONSE_TIMEOUT:
                    return "Response timeout.";

                case Result.ISCLOSED:
                    return "Connection is closed.";

                case Result.CRC:
                    return "CRC Error.";

                case Result.RESPONSE:
                    return "Not the expected response received.";

                case Result.BYTECOUNT:
                    return "Byte count error.";

                case Result.QUANTITY:
                    return "Quantity is out of range.";

                case Result.FUNCTION:
                    return "Modbus function code out of range. 1 - 127.";

                case Result.DEMO_TIMEOUT:
                    return "Demo mode expired. Restart your application to continue.";
            }
            return ("Unknown Error - " + this.Res.ToString());
        }

        public int GetRxBuffer(byte[] byteArray)
        {
            return this.TxRx.GetRxBuffer(byteArray);
        }

        public int GetTxBuffer(byte[] byteArray)
        {
            return this.TxRx.GetTxBuffer(byteArray);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
        }

        public short[] Int32ToRegisters(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            short[] numArray1 = new short[2];
            numArray1[1] = BitConverter.ToInt16(bytes, 0);
            numArray1[0] = BitConverter.ToInt16(bytes, 2);
            return numArray1;
        }

        public bool LicenseKey(string LicenseKey)
        {
            if (LicenseKey.Length == 0x1d)
            {
                byte[] data = new byte[0x19];
                for (int i = 0; i < 0x19; i++)
                {
                    data[i] = (byte) LicenseKey[i];
                }
                byte[] buffer2 = CCRC16.CRC16(data, 0x19);
                if (string.Format("{0:X2}{1:X2}", buffer2[1], buffer2[0]) == LicenseKey.Substring(0x19, 4))
                {
                    this.TxRx.License = true;
                    return true;
                }
            }
            return false;
        }

        public Result MaskWriteRegister(byte unitId, ushort address, ushort andMask, ushort orMask)
        {
            return (this.Res = this.Modbus.MaskWriteRegister(unitId, address, andMask, orMask));
        }

        public Result ReadCoils(byte unitId, ushort address, ushort quantity, bool[] coils)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, 1, address, quantity, coils, 0));
        }

        public Result ReadCoils(byte unitId, ushort address, ushort quantity, bool[] coils, int offset)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, 1, address, quantity, coils, offset));
        }

        public Result ReadDiscreteInputs(byte unitId, ushort address, ushort quantity, bool[] discreteInputs)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, 2, address, quantity, discreteInputs, 0));
        }

        public Result ReadDiscreteInputs(byte unitId, ushort address, ushort quantity, bool[] discreteInputs, int offset)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, 2, address, quantity, discreteInputs, offset));
        }

        public Result ReadHoldingRegisters(byte unitId, ushort address, ushort quantity, short[] registers)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, 3, address, quantity, registers, 0));
        }

        public Result ReadHoldingRegisters(byte unitId, ushort address, ushort quantity, short[] registers, int offset)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, 3, address, quantity, registers, offset));
        }

        public Result ReadInputRegisters(byte unitId, ushort address, ushort quantity, short[] registers)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, 4, address, quantity, registers, 0));
        }

        public Result ReadInputRegisters(byte unitId, ushort address, ushort quantity, short[] registers, int offset)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, 4, address, quantity, registers, offset));
        }

        public Result ReadUserDefinedCoils(byte unitId, byte function, ushort address, ushort quantity, bool[] coils)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, function, address, quantity, coils, 0));
        }

        public Result ReadUserDefinedCoils(byte unitId, byte function, ushort address, ushort quantity, bool[] coils, int offset)
        {
            return (this.Res = this.Modbus.ReadFlags(unitId, function, address, quantity, coils, offset));
        }

        public Result ReadUserDefinedRegisters(byte unitId, byte function, ushort address, ushort quantity, short[] registers)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, function, address, quantity, registers, 0));
        }

        public Result ReadUserDefinedRegisters(byte unitId, byte function, ushort address, ushort quantity, short[] registers, int offset)
        {
            return (this.Res = this.Modbus.ReadRegisters(unitId, function, address, quantity, registers, offset));
        }

        public Result ReadWriteMultipleRegisters(byte unitId, ushort readAddress, ushort readQuantity, short[] readRegisters, ushort writeAddress, ushort writeQuantity, short[] writeRegisters)
        {
            return (this.Res = this.Modbus.ReadWriteMultipleRegisters(unitId, readAddress, readQuantity, readRegisters, writeAddress, writeQuantity, writeRegisters));
        }

        public float RegistersToFloat(short hiReg, short loReg)
        {
            //return BitConverter.ToSingle(BitConverter.GetBytes(loReg).Concat<byte>(BitConverter.GetBytes(hiReg)).ToArray<byte>(), 0);
            return 0;
        }

        public int RegistersToInt32(short hiReg, short loReg)
        {
          //  return BitConverter.ToInt32(BitConverter.GetBytes(loReg).Concat<byte>(BitConverter.GetBytes(hiReg)).ToArray<byte>(), 0);
            return 0;
        }

        public Result ReportSlaveID(byte unitId, out byte byteCount, byte[] deviceSpecific)
        {
            return (this.Res = this.Modbus.ReportSlaveID(unitId, out byteCount, deviceSpecific));
        }

        public Result WriteMultipleCoils(byte unitId, ushort address, ushort quantity, bool[] coils)
        {
            return (this.Res = this.Modbus.WriteFlags(unitId, 15, address, quantity, coils, 0));
        }

        public Result WriteMultipleCoils(byte unitId, ushort address, ushort quantity, bool[] coils, int offset)
        {
            return (this.Res = this.Modbus.WriteFlags(unitId, 15, address, quantity, coils, offset));
        }

        public Result WriteMultipleRegisters(byte unitId, ushort address, ushort quantity, short[] registers)
        {
            return (this.Res = this.Modbus.WriteRegisters(unitId, 0x10, address, quantity, registers, 0));
        }

        public Result WriteMultipleRegisters(byte unitId, ushort address, ushort quantity, short[] registers, int offset)
        {
            return (this.Res = this.Modbus.WriteRegisters(unitId, 0x10, address, quantity, registers, offset));
        }

        public Result WriteSingleCoil(byte unitId, ushort address, bool coil)
        {
            return (this.Res = this.Modbus.WriteSingleCoil(unitId, address, coil));
        }

        public Result WriteSingleRegister(byte unitId, ushort address, short register)
        {
            return (this.Res = this.Modbus.WriteSingleRegister(unitId, address, register));
        }

        public Result WriteUserDefinedCoils(byte unitId, byte function, ushort address, ushort quantity, bool[] coils)
        {
            return (this.Res = this.Modbus.WriteFlags(unitId, function, address, quantity, coils, 0));
        }

        public Result WriteUserDefinedCoils(byte unitId, byte function, ushort address, ushort quantity, bool[] coils, int offset)
        {
            return (this.Res = this.Modbus.WriteFlags(unitId, function, address, quantity, coils, offset));
        }

        public Result WriteUserDefinedRegisters(byte unitId, byte function, ushort address, ushort quantity, short[] registers)
        {
            return (this.Res = this.Modbus.WriteRegisters(unitId, function, address, quantity, registers, 0));
        }

        public Result WriteUserDefinedRegisters(byte unitId, byte function, ushort address, ushort quantity, short[] registers, int offset)
        {
            return (this.Res = this.Modbus.WriteRegisters(unitId, function, address, quantity, registers, offset));
        }

        [Category("Modbus"), Description("Max time to wait for connection 100 - 30000ms.")]
        public int ConnectTimeout
        {
            get
            {
                return this._ConnectTimeout;
            }
            set
            {
                if ((value >= 100) && (value <= 0x7530))
                {
                    this._ConnectTimeout = value;
                }
            }
        }

        [Category("Modbus"), Description("Select which protocol mode to use.")]
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

        [Category("Modbus"), Description("Max time to wait for response 100 - 30000ms.")]
        public int ResponseTimeout
        {
            get
            {
                return this._ResponseTimeout;
            }
            set
            {
                if ((value >= 100) && (value <= 0x7530))
                {
                    this._ResponseTimeout = value;
                }
            }
        }
    }
}

