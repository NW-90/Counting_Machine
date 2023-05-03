namespace DiawModbus
{
    using DiawModbus.Exceptions;
    using System;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    public class ModbusClient
    {
        private int dataBits;
        
        private int baudRate;
        private int bytesToRead;
        private int connectTimeout;
        private byte[] crc;
        private bool dataReceived;
        private byte functionCode;
        private string ipAddress;
        private byte[] length;
        private System.IO.Ports.Parity parity;
        private int port;
        private int portOut;
        private byte[] protocolIdentifier;
        private byte[] quantity;
        private byte[] readBuffer;
        private bool receiveActive;
        public byte[] receiveData;
        public byte[] sendData;
        private SerialPort serialport;
        private byte[] startingAddress;
        private System.IO.Ports.StopBits stopBits;
        private NetworkStream stream;
        private TcpClient tcpClient;
        private byte[] transactionIdentifier;
        private uint transactionIdentifierInternal;
        private bool udpFlag;
        private byte unitIdentifier;
        public int ModbusMode = 0;

        [field: CompilerGenerated, DebuggerBrowsable(0)]
        public event ReceiveDataChanged receiveDataChanged;

        [field: CompilerGenerated, DebuggerBrowsable(0)]
        public event SendDataChanged sendDataChanged;

        public ModbusClient()
        {
            this.ipAddress = "127.0.0.1";
            this.port = 0x1f6;
            this.transactionIdentifierInternal = 0;
            this.transactionIdentifier = new byte[2];
            this.protocolIdentifier = new byte[2];
            this.crc = new byte[2];
            this.length = new byte[2];
            this.unitIdentifier = 1;
            this.startingAddress = new byte[2];
            this.quantity = new byte[2];
            this.udpFlag = false;
            this.baudRate = 0x2580;
            this.dataBits = 8;
            this.connectTimeout = 0x3e8;
            this.parity = System.IO.Ports.Parity.Odd;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.dataReceived = false;
            this.receiveActive = false;
            this.readBuffer = new byte[0x100];
            this.bytesToRead = 0;
            //Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
            //Console.WriteLine();
        }
        public ModbusClient(string serialPort)
        {
            this.ipAddress = "127.0.0.1";
            this.port = 0x1f6;
            this.transactionIdentifierInternal = 0;
            this.transactionIdentifier = new byte[2];
            this.protocolIdentifier = new byte[2];
            this.crc = new byte[2];
            this.length = new byte[2];
            this.unitIdentifier = 1;
            this.startingAddress = new byte[2];
            this.quantity = new byte[2];
            this.udpFlag = false;
            this.baudRate = 0x2580;
            this.dataBits = 8;
            this.connectTimeout = 0x3e8;
            this.parity = System.IO.Ports.Parity.None;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.dataReceived = false;
            this.receiveActive = false;
            this.readBuffer = new byte[0x100];
            this.bytesToRead = 0;
           // Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
           // Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
           // Console.WriteLine();
            this.serialport = new SerialPort();
            this.serialport.PortName = serialPort;

            this.serialport.DataBits = dataBits;
            
            this.serialport.BaudRate = this.baudRate;
            this.serialport.Parity = this.parity;
            this.serialport.StopBits = this.stopBits;
            this.serialport.WriteTimeout = 0x2710;
            this.serialport.ReadTimeout = this.connectTimeout;
            this.serialport.DataReceived += new SerialDataReceivedEventHandler(this.DataReceivedHandler);
        }
        public ModbusClient(string ipAddress, int port)
        {
            this.ipAddress = "127.0.0.1";
            this.port = 0x1f6;
            this.transactionIdentifierInternal = 0;
            this.transactionIdentifier = new byte[2];
            this.protocolIdentifier = new byte[2];
            this.crc = new byte[2];
            this.length = new byte[2];
            this.unitIdentifier = 1;
            this.startingAddress = new byte[2];
            this.quantity = new byte[2];
            this.udpFlag = false;
            this.baudRate = 0x2580;
            this.connectTimeout = 0x3e8;
            this.parity = System.IO.Ports.Parity.Even;
            this.stopBits = System.IO.Ports.StopBits.One;
            this.dataReceived = false;
            this.receiveActive = false;
            this.readBuffer = new byte[0x100];
            this.bytesToRead = 0;
            Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
            Console.WriteLine();
            this.ipAddress = ipAddress;
            this.port = port;
        }
        public bool Available(int timeout)
        {
            Ping ping = new Ping();
            System.Net.IPAddress address = System.Net.IPAddress.Parse(this.ipAddress);
            string s = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            return (ping.Send(address, timeout, bytes).Status == IPStatus.Success);
        }
        public static ushort calculateCRC(byte[] data, ushort numberOfBytes, int startByte)
        {
            byte[] buffer = new byte[] { 
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40, 0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41,
                0, 0xc1, 0x81, 0x40, 1, 0xc0, 0x80, 0x41, 1, 0xc0, 0x80, 0x41, 0, 0xc1, 0x81, 0x40
            };
            byte[] buffer2 = new byte[] { 
                0, 0xc0, 0xc1, 1, 0xc3, 3, 2, 0xc2, 0xc6, 6, 7, 0xc7, 5, 0xc5, 0xc4, 4,
                0xcc, 12, 13, 0xcd, 15, 0xcf, 0xce, 14, 10, 0xca, 0xcb, 11, 0xc9, 9, 8, 200,
                0xd8, 0x18, 0x19, 0xd9, 0x1b, 0xdb, 0xda, 0x1a, 30, 0xde, 0xdf, 0x1f, 0xdd, 0x1d, 0x1c, 220,
                20, 0xd4, 0xd5, 0x15, 0xd7, 0x17, 0x16, 0xd6, 210, 0x12, 0x13, 0xd3, 0x11, 0xd1, 0xd0, 0x10,
                240, 0x30, 0x31, 0xf1, 0x33, 0xf3, 0xf2, 50, 0x36, 0xf6, 0xf7, 0x37, 0xf5, 0x35, 0x34, 0xf4,
                60, 0xfc, 0xfd, 0x3d, 0xff, 0x3f, 0x3e, 0xfe, 250, 0x3a, 0x3b, 0xfb, 0x39, 0xf9, 0xf8, 0x38,
                40, 0xe8, 0xe9, 0x29, 0xeb, 0x2b, 0x2a, 0xea, 0xee, 0x2e, 0x2f, 0xef, 0x2d, 0xed, 0xec, 0x2c,
                0xe4, 0x24, 0x25, 0xe5, 0x27, 0xe7, 230, 0x26, 0x22, 0xe2, 0xe3, 0x23, 0xe1, 0x21, 0x20, 0xe0,
                160, 0x60, 0x61, 0xa1, 0x63, 0xa3, 0xa2, 0x62, 0x66, 0xa6, 0xa7, 0x67, 0xa5, 0x65, 100, 0xa4,
                0x6c, 0xac, 0xad, 0x6d, 0xaf, 0x6f, 110, 0xae, 170, 0x6a, 0x6b, 0xab, 0x69, 0xa9, 0xa8, 0x68,
                120, 0xb8, 0xb9, 0x79, 0xbb, 0x7b, 0x7a, 0xba, 190, 0x7e, 0x7f, 0xbf, 0x7d, 0xbd, 0xbc, 0x7c,
                180, 0x74, 0x75, 0xb5, 0x77, 0xb7, 0xb6, 0x76, 0x72, 0xb2, 0xb3, 0x73, 0xb1, 0x71, 0x70, 0xb0,
                80, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 150, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54,
                0x9c, 0x5c, 0x5d, 0x9d, 0x5f, 0x9f, 0x9e, 0x5e, 90, 0x9a, 0x9b, 0x5b, 0x99, 0x59, 0x58, 0x98,
                0x88, 0x48, 0x49, 0x89, 0x4b, 0x8b, 0x8a, 0x4a, 0x4e, 0x8e, 0x8f, 0x4f, 0x8d, 0x4d, 0x4c, 140,
                0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 70, 0x86, 130, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80, 0x40
            };
            ushort num = numberOfBytes;
            byte num2 = 0xff;
            byte num3 = 0xff;
            for (int i = 0; num > 0; i++)
            {
                num = (ushort) (num - 1);
                int index = num3 ^ data[i + startByte];
                num3 = (byte) (num2 ^ buffer[index]);
                num2 = buffer2[index];
            }
            return (ushort) ((num2 << 8) | num3);
        }
        public void Connect(int selectmode)
        {

            ModbusMode= selectmode;
            if (ModbusMode == 1) //Serial port
            {

                if (this.serialport != null)
                {
                    if (!this.serialport.IsOpen)
                    {
                        this.serialport.BaudRate = this.baudRate;
                        this.serialport.Parity = this.parity;
                        this.serialport.StopBits = this.stopBits;
                        this.serialport.Handshake = Handshake.None;
                        this.serialport.RtsEnable = true;
                        this.serialport.DtrEnable = true;
                
                        this.serialport.WriteTimeout = 10000;
                        this.serialport.ReadTimeout = this.connectTimeout;

                        try
                        {
                            this.serialport.Open();
                        }
                        catch
                        {

                        }
                    }
                }
            }
            else
            {
                if (!this.udpFlag)
                {
                    this.tcpClient = new TcpClient(this.ipAddress, this.port);
                    this.stream = this.tcpClient.GetStream();
                    this.stream.ReadTimeout = this.connectTimeout;
                }
                else
                {
                    this.tcpClient = new TcpClient();
                }
            }
        }

        public void Connect(string ipAddress, int port)
        {
            if (!this.udpFlag)
            {
                this.ipAddress = ipAddress;
                this.port = port;
                this.tcpClient = new TcpClient(ipAddress, port);
                this.stream = this.tcpClient.GetStream();
                this.stream.ReadTimeout = this.connectTimeout;
            }
            else
            {
                this.tcpClient = new TcpClient();
            }
        }

        public static int[] ConvertDoubleToTwoRegisters(int doubleValue)
        {
            byte[] bytes = BitConverter.GetBytes(doubleValue);
            byte[] buffer1 = new byte[4];
            buffer1[0] = bytes[2];
            buffer1[1] = bytes[3];
            byte[] buffer2 = buffer1;
            byte[] buffer4 = new byte[4];
            buffer4[0] = bytes[0];
            buffer4[1] = bytes[1];
            byte[] buffer3 = buffer4;
            return new int[] { BitConverter.ToInt32(buffer3, 0), BitConverter.ToInt32(buffer2, 0) };
        }

        public static int[] ConvertDoubleToTwoRegisters(int doubleValue, RegisterOrder registerOrder)
        {
            int[] numArray = ConvertDoubleToTwoRegisters(doubleValue);
            int[] numArray2 = numArray;
            if (registerOrder == RegisterOrder.HighLow)
            {
                numArray2 = new int[] { numArray[1], numArray[0] };
            }
            return numArray2;
        }

        public static int[] ConvertFloatToTwoRegisters(float floatValue)
        {
            byte[] bytes = BitConverter.GetBytes(floatValue);
            byte[] buffer1 = new byte[4];
            buffer1[0] = bytes[2];
            buffer1[1] = bytes[3];
            byte[] buffer2 = buffer1;
            byte[] buffer4 = new byte[4];
            buffer4[0] = bytes[0];
            buffer4[1] = bytes[1];
            byte[] buffer3 = buffer4;
            return new int[] { BitConverter.ToInt32(buffer3, 0), BitConverter.ToInt32(buffer2, 0) };
        }

        public static int[] ConvertFloatToTwoRegisters(float floatValue, RegisterOrder registerOrder)
        {
            int[] numArray = ConvertFloatToTwoRegisters(floatValue);
            int[] numArray2 = numArray;
            if (registerOrder == RegisterOrder.HighLow)
            {
                numArray2 = new int[] { numArray[1], numArray[0] };
            }
            return numArray2;
        }

        public static int ConvertRegistersToDouble(int[] registers)
        {
            if (registers.Length != 2)
            {
                throw new ArgumentException("Input Array length invalid");
            }
            int num = registers[1];
            int num2 = registers[0];
            byte[] bytes = BitConverter.GetBytes(num);
            byte[] buffer2 = BitConverter.GetBytes(num2);
            byte[] buffer3 = new byte[] { buffer2[0], buffer2[1], bytes[0], bytes[1] };
            return BitConverter.ToInt32(buffer3, 0);
        }

        public static int ConvertRegistersToDouble(int[] registers, RegisterOrder registerOrder)
        {
            int[] numArray = new int[] { registers[0], registers[1] };
            if (registerOrder == RegisterOrder.HighLow)
            {
                numArray = new int[] { registers[1], registers[0] };
            }
            return ConvertRegistersToDouble(numArray);
        }

        public static float ConvertRegistersToFloat(int[] registers)
        {
            if (registers.Length != 2)
            {
                throw new ArgumentException("Input Array length invalid");
            }
            int num = registers[1];
            int num2 = registers[0];
            byte[] bytes = BitConverter.GetBytes(num);
            byte[] buffer2 = BitConverter.GetBytes(num2);
            byte[] buffer3 = new byte[] { buffer2[0], buffer2[1], bytes[0], bytes[1] };
            return BitConverter.ToSingle(buffer3, 0);
        }

        public static float ConvertRegistersToFloat(int[] registers, RegisterOrder registerOrder)
        {
            int[] numArray = new int[] { registers[0], registers[1] };
            if (registerOrder == RegisterOrder.HighLow)
            {
                numArray = new int[] { registers[1], registers[0] };
            }
            return ConvertRegistersToFloat(numArray);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            while (this.receiveActive | this.dataReceived)
            {
                Thread.Sleep(10);
            }
            this.receiveActive = true;
            long num = 0x1312d00L;
            SerialPort port = (SerialPort) sender;
            if (this.bytesToRead == 0)
            {
                port.DiscardInBuffer();
                this.receiveActive = false;
            }
            else
            {
                this.readBuffer = new byte[0x100];
                int count = 0;
                int length = 0;
                DateTime now = DateTime.Now;

                do
                {
                    try
                    {
                        now = DateTime.Now;
                        while (port.BytesToRead == 0 || port.IsOpen == false)
                        {
                            Thread.Sleep(1);
                            if ((DateTime.Now.Ticks - now.Ticks) > num)
                            {
                                break;
                            }
                        }
                        count = port.BytesToRead;
                        byte[] buffer = new byte[count];
                        port.Read(buffer, 0, count);
                        Array.Copy(buffer, 0, this.readBuffer, length, buffer.Length);
                        length += buffer.Length;
                    }
                    catch (Exception)
                    {
                    }
                }
                while (!(DetectValidModbusFrame(this.readBuffer, length) | (this.bytesToRead < length)) && ((DateTime.Now.Ticks - now.Ticks) < num));


                this.bytesToRead = 0;
                this.dataReceived = true;
                this.receiveActive = false;

                if (port.IsOpen == true)
                {
                    port.DiscardInBuffer();
                }
               
                this.receiveData = new byte[length];
                Array.Copy(this.readBuffer, 0, this.receiveData, 0, length);
                if (this.receiveDataChanged != null)
                {
                    this.receiveDataChanged(this);
                }
            }
        }

        public static bool DetectValidModbusFrame(byte[] readBuffer, int length)
        {
            if (length < 6)
            {
                return false;
            }
            if ((readBuffer[0] < 1) | (readBuffer[0] > 0xf7))
            {
                return false;
            }
            byte[] bytes = new byte[2];
            bytes = BitConverter.GetBytes(calculateCRC(readBuffer, (ushort) (length - 2), 0));
            if ((bytes[0] != readBuffer[length - 2]) | (bytes[1] != readBuffer[length - 1]))
            {
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                if (this.serialport.IsOpen)
                {
                    this.serialport.Close();
                }
            }
            else
            {
                if (this.stream != null)
                {
                    this.stream.Close();
                }
                if (this.tcpClient != null)
                {
                    this.tcpClient.Close();
                }
            }
        }

        ~ModbusClient()
        {
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                if (this.serialport.IsOpen)
                {
                    this.serialport.Close();
                }
            }
            else if ((this.tcpClient != null) & !this.udpFlag)
            {
                this.stream.Close();
                this.tcpClient.Close();
            }
        }

        public bool[] ReadCoils(int startingAddress, int quantity)
        {
            this.transactionIdentifierInternal++;

            if ((ModbusMode == 1)&&(this.serialport != null) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            if ((startingAddress > 0xffff) | (quantity > 0x7d0))
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 1;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            this.quantity = BitConverter.GetBytes(quantity);
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], this.quantity[1], this.quantity[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = this.crc[0];
            data[13] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                if ((quantity % 8) == 0)
                {
                    this.bytesToRead = 5 + (quantity / 8);
                }
                else
                {
                    this.bytesToRead = 6 + (quantity / 8);
                }
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num = 0xff;
                while ((num != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num = data[6];
                }
                if (num != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x81) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x81) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x81) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x81) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data[8] + 3), 6));
                if (((this.crc[0] != data[data[8] + 9]) | (this.crc[1] != data[data[8] + 10])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
            bool[] flagArray = new bool[quantity];
            for (int i = 0; i < quantity; i++)
            {
                int num4 = data[9 + (i / 8)];
                int num5 = Convert.ToInt32(Math.Pow(2.0, (double) (i % 8)));
                flagArray[i] = Convert.ToBoolean((int) ((num4 & num5) / num5));
            }
            return flagArray;
        }

        public bool[] ReadDiscreteInputs(int startingAddress, int quantity)
        {
            this.transactionIdentifierInternal++;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            if ((startingAddress > 0xffff) | (quantity > 0x7d0))
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 2;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            this.quantity = BitConverter.GetBytes(quantity);
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], this.quantity[1], this.quantity[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = this.crc[0];
            data[13] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                if ((quantity % 8) == 0)
                {
                    this.bytesToRead = 5 + (quantity / 8);
                }
                else
                {
                    this.bytesToRead = 6 + (quantity / 8);
                }
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num = 0xff;
                while ((num != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num = data[6];
                }
                if (num != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 130) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 130) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 130) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 130) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data[8] + 3), 6));
                if (((this.crc[0] != data[data[8] + 9]) | (this.crc[1] != data[data[8] + 10])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
            bool[] flagArray = new bool[quantity];
            for (int i = 0; i < quantity; i++)
            {
                int num4 = data[9 + (i / 8)];
                int num5 = Convert.ToInt32(Math.Pow(2.0, (double) (i % 8)));
                flagArray[i] = Convert.ToBoolean((int) ((num4 & num5) / num5));
            }
            return flagArray;
        }

        public int[] ReadHoldingRegisters(int startingAddress, int quantity)
        {
            this.transactionIdentifierInternal++;

            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            if ((startingAddress > 0xffff) | (quantity > 0xffff))
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 65535");
            }


            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 3;

            this.startingAddress = BitConverter.GetBytes(startingAddress);
            this.quantity = BitConverter.GetBytes(quantity);
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], this.quantity[1], this.quantity[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));

            data[12] = this.crc[0];
            data[13] = this.crc[1];

            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 5 + (2 * quantity);
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num = 0xff;
                while ((num != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num = data[6];
                }
                if (num != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x100];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x83) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x83) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x83) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x83) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data[8] + 3), 6));
                if (((this.crc[0] != data[data[8] + 9]) | (this.crc[1] != data[data[8] + 10])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
            int[] numArray = new int[quantity];
            for (int i = 0; i < quantity; i++)
            {
                byte num5 = data[9 + (i * 2)];
                byte num4 = data[(9 + (i * 2)) + 1];
                data[9 + (i * 2)] = num4;
                data[(9 + (i * 2)) + 1] = num5;
                numArray[i] = BitConverter.ToInt16(data, 9 + (i * 2));
            }
            return numArray;
        }

        public object[] ReadHoldingRegisters(int startingAddress, int quantity, DataType dataType, RegisterOrder registerOrder)
        {
            int num = quantity;
            if (((dataType == DataType.Long) | (dataType == DataType.ULong)) | (dataType == DataType.Float))
            {
                num = quantity * 2;
            }
            if (dataType == DataType.Float)
            {
                num = quantity * 4;
            }
            int[] source = this.ReadHoldingRegisters(startingAddress, num);
            if (dataType == DataType.Short)
            {
                return source.Cast<object>().ToArray<object>();
            }
            return source.Cast<object>().ToArray<object>();
        }

        public int[] ReadInputRegisters(int startingAddress, int quantity)
        {
            this.transactionIdentifierInternal++;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            if ((startingAddress > 0xffff) | (quantity > 0xffff))
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 65535");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 4;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            this.quantity = BitConverter.GetBytes(quantity);
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], this.quantity[1], this.quantity[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = this.crc[0];
            data[13] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 5 + (2 * quantity);
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num = 0xff;
                while ((num != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num = data[6];
                }
                if (num != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x84) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x84) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x84) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x84) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data[8] + 3), 6));
                if (((this.crc[0] != data[data[8] + 9]) | (this.crc[1] != data[data[8] + 10])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
            int[] numArray = new int[quantity];
            for (int i = 0; i < quantity; i++)
            {
                byte num5 = data[9 + (i * 2)];
                byte num4 = data[(9 + (i * 2)) + 1];
                data[9 + (i * 2)] = num4;
                data[(9 + (i * 2)) + 1] = num5;
                numArray[i] = BitConverter.ToInt16(data, 9 + (i * 2));
            }
            return numArray;
        }

        public int[] ReadWriteMultipleRegisters(int startingAddressRead, int quantityRead, int startingAddressWrite, int[] values)
        {
            this.transactionIdentifierInternal++;
            byte[] bytes = new byte[2];
            byte[] buffer2 = new byte[2];
            byte[] buffer3 = new byte[2];
            byte[] buffer4 = new byte[2];
            byte num = 0;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            if ((((startingAddressRead > 0xffff) | (quantityRead > 0x7d)) | (startingAddressWrite > 0xffff)) | (values.Length > 0x79))
            {
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 0x17;
            bytes = BitConverter.GetBytes(startingAddressRead);
            buffer2 = BitConverter.GetBytes(quantityRead);
            buffer3 = BitConverter.GetBytes(startingAddressWrite);
            buffer4 = BitConverter.GetBytes(values.Length);
            num = Convert.ToByte((int) (values.Length * 2));
            byte[] data = new byte[0x13 + (values.Length * 2)];
            data[0] = this.transactionIdentifier[1];
            data[1] = this.transactionIdentifier[0];
            data[2] = this.protocolIdentifier[1];
            data[3] = this.protocolIdentifier[0];
            data[4] = this.length[1];
            data[5] = this.length[0];
            data[6] = this.unitIdentifier;
            data[7] = this.functionCode;
            data[8] = bytes[1];
            data[9] = bytes[0];
            data[10] = buffer2[1];
            data[11] = buffer2[0];
            data[12] = buffer3[1];
            data[13] = buffer3[0];
            data[14] = buffer4[1];
            data[15] = buffer4[0];
            data[0x10] = num;
            for (int i = 0; i < values.Length; i++)
            {
                byte[] buffer6 = BitConverter.GetBytes(values[i]);
                data[0x11 + (i * 2)] = buffer6[1];
                data[0x12 + (i * 2)] = buffer6[0];
            }
            this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data.Length - 8), 6));
            data[data.Length - 2] = this.crc[0];
            data[data.Length - 1] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 5 + (2 * quantityRead);
                this.serialport.Write(data, 6, data.Length - 6);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[data.Length - 6];
                    Array.Copy(data, 6, this.sendData, 0, data.Length - 6);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num3 = 0xff;
                while ((num3 != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num3 = data[6];
                }
                if (num3 != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x97) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x97) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x97) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x97) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            int[] numArray = new int[quantityRead];
            for (int j = 0; j < quantityRead; j++)
            {
                byte num7 = data[9 + (j * 2)];
                byte num6 = data[(9 + (j * 2)) + 1];
                data[9 + (j * 2)] = num6;
                data[(9 + (j * 2)) + 1] = num7;
                numArray[j] = BitConverter.ToInt16(data, 9 + (j * 2));
            }
            return numArray;
        }

        public void WriteMultipleCoils(int startingAddress, bool[] values)
        {
            this.transactionIdentifierInternal++;
            byte num = (byte) ((values.Length / 8) + 1);
            byte[] bytes = BitConverter.GetBytes(values.Length);
            byte num2 = 0;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes((int) (7 + ((values.Length / 8) + 1)));
            this.functionCode = 15;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            byte[] data = new byte[0x10 + (values.Length / 8)];
            data[0] = this.transactionIdentifier[1];
            data[1] = this.transactionIdentifier[0];
            data[2] = this.protocolIdentifier[1];
            data[3] = this.protocolIdentifier[0];
            data[4] = this.length[1];
            data[5] = this.length[0];
            data[6] = this.unitIdentifier;
            data[7] = this.functionCode;
            data[8] = this.startingAddress[1];
            data[9] = this.startingAddress[0];
            data[10] = bytes[1];
            data[11] = bytes[0];
            data[12] = num;
            for (int i = 0; i < values.Length; i++)
            {
                byte num4;
                if ((i % 8) == 0)
                {
                    num2 = 0;
                }
                if (values[i])
                {
                    num4 = 1;
                }
                else
                {
                    num4 = 0;
                }
                num2 = (byte) ((num4 << (i % 8)) | num2);
                data[13 + (i / 8)] = num2;
            }
            this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data.Length - 8), 6));
            data[data.Length - 2] = this.crc[0];
            data[data.Length - 1] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 8;
                this.serialport.Write(data, 6, data.Length - 6);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[data.Length - 6];
                    Array.Copy(data, 6, this.sendData, 0, data.Length - 6);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num5 = 0xff;
                while ((num5 != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num5 = data[6];
                }
                if (num5 != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x8f) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x8f) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x8f) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x8f) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
                if (((this.crc[0] != data[12]) | (this.crc[1] != data[13])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
        }

        public void WriteMultipleRegisters(int startingAddress, int[] values)
        {
            this.transactionIdentifierInternal++;
            byte num = (byte) (values.Length * 2);
            byte[] bytes = BitConverter.GetBytes(values.Length);
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);

            this.length = BitConverter.GetBytes((int) (7 + (values.Length * 2)));

            this.functionCode = 0x10; //FC

            this.startingAddress = BitConverter.GetBytes(startingAddress);
            byte[] data = new byte[15 + (values.Length * 2)];
            data[0] = this.transactionIdentifier[1];
            data[1] = this.transactionIdentifier[0];
            data[2] = this.protocolIdentifier[1];
            data[3] = this.protocolIdentifier[0];
            data[4] = this.length[1];
            data[5] = this.length[0];
            data[6] = this.unitIdentifier;
            data[7] = this.functionCode;
            data[8] = this.startingAddress[1];
            data[9] = this.startingAddress[0];
            data[10] = bytes[1];
            data[11] = bytes[0];
            data[12] = num;
            for (int i = 0; i < values.Length; i++)
            {
                byte[] buffer3 = BitConverter.GetBytes(values[i]);
                data[13 + (i * 2)] = buffer3[1];
                data[14 + (i * 2)] = buffer3[0];
            }
            this.crc = BitConverter.GetBytes(calculateCRC(data, (ushort) (data.Length - 8), 6));
            data[data.Length - 2] = this.crc[0];
            data[data.Length - 1] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 8;
                this.serialport.Write(data, 6, data.Length - 6);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[data.Length - 6];
                    Array.Copy(data, 6, this.sendData, 0, data.Length - 6);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num3 = 0xff;
                while ((num3 != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num3 = data[6];
                }
                if (num3 != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x90) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x90) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x90) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x90) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
                if (((this.crc[0] != data[12]) | (this.crc[1] != data[13])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
        }

        public void WriteSingleCoil(int startingAddress, bool value)
        {
            this.transactionIdentifierInternal++;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            byte[] bytes = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);
            this.functionCode = 5;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            if (value)
            {
                bytes = BitConverter.GetBytes(0xff00);
            }
            else
            {
                bytes = BitConverter.GetBytes(0);
            }
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], bytes[1], bytes[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = this.crc[0];
            data[13] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 8;
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                for (byte i = 0xff; (i != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)); i = data[6])
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x85) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x85) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x85) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x85) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
                if (((this.crc[0] != data[12]) | (this.crc[1] != data[13])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
        }

        public void WriteSingleRegister(int startingAddress, int value)
        {
            this.transactionIdentifierInternal++;
            if (((ModbusMode == 1)&&(this.serialport != null)) && !this.serialport.IsOpen)
            {
                throw new SerialPortNotOpenedException("serial port not opened");
            }
            if (((this.tcpClient == null) & !this.udpFlag) & (this.serialport == null))
            {
                throw new ConnectionException("connection error");
            }
            byte[] bytes = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes(this.transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes(0);
            this.length = BitConverter.GetBytes(6);

            this.functionCode = 6; //---Change FC ID

            this.startingAddress = BitConverter.GetBytes(startingAddress);
            bytes = BitConverter.GetBytes(value);
            byte[] data = new byte[] { this.transactionIdentifier[1], this.transactionIdentifier[0], this.protocolIdentifier[1], this.protocolIdentifier[0], this.length[1], this.length[0], this.unitIdentifier, this.functionCode, this.startingAddress[1], this.startingAddress[0], bytes[1], bytes[0], this.crc[0], this.crc[1] };
            this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = this.crc[0];
            data[13] = this.crc[1];
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.dataReceived = false;
                this.bytesToRead = 8;
                this.serialport.Write(data, 6, 8);
                if (this.sendDataChanged != null)
                {
                    this.sendData = new byte[8];
                    Array.Copy(data, 6, this.sendData, 0, 8);
                    this.sendDataChanged(this);
                }
                DateTime now = DateTime.Now;
                byte num = 0xff;
                while ((num != this.unitIdentifier) & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                {
                    while (!this.dataReceived & ((DateTime.Now.Ticks - now.Ticks) <= (0x2710L * this.connectTimeout)))
                    {
                        Thread.Sleep(1);
                    }
                    data = new byte[0x834];
                    Array.Copy(this.readBuffer, 0, data, 6, this.readBuffer.Length);
                    num = data[6];
                }
                if (num != this.unitIdentifier)
                {
                    data = new byte[0x834];
                }
            }
            else if (this.tcpClient.Client.Connected | this.udpFlag)
            {
                if (this.udpFlag)
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.port);
                    client.Send(data, data.Length - 2, endPoint);
                    this.portOut = ((IPEndPoint) client.Client.LocalEndPoint).Port;
                    client.Client.ReceiveTimeout = 0x1388;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(this.ipAddress), this.portOut);
                    data = client.Receive(ref endPoint);
                }
                else
                {
                    this.stream.Write(data, 0, data.Length - 2);
                    if (this.sendDataChanged != null)
                    {
                        this.sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, this.sendData, 0, data.Length - 2);
                        this.sendDataChanged(this);
                    }
                    data = new byte[0x834];
                    int length = this.stream.Read(data, 0, data.Length);
                    if (this.receiveDataChanged != null)
                    {
                        this.receiveData = new byte[length];
                        Array.Copy(data, 0, this.receiveData, 0, length);
                        this.receiveDataChanged(this);
                    }
                }
            }
            if ((data[7] == 0x86) & (data[8] == 1))
            {
                throw new FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if ((data[7] == 0x86) & (data[8] == 2))
            {
                throw new StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if ((data[7] == 0x86) & (data[8] == 3))
            {
                throw new QuantityInvalidException("quantity invalid");
            }
            if ((data[7] == 0x86) & (data[8] == 4))
            {
                throw new ModbusException("error reading");
            }
            if ((ModbusMode == 1)&&(this.serialport != null))
            {
                this.crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
                if (((this.crc[0] != data[12]) | (this.crc[1] != data[13])) & this.dataReceived)
                {
                    throw new CRCCheckFailedException("Response CRC check failed");
                }
                if (!this.dataReceived)
                {
                    throw new TimeoutException("No Response from Modbus Slave");
                }
            }
        }

        public int Baudrate
        {
            get
            {
                return this.baudRate;
            }
            set
            {
                this.baudRate = value;
            }
        }


        public int DataBits
        {
            get
            {
                return this.dataBits;
            }
            set
            {
                this.dataBits = value;
            }
        }
        public bool Connected
        {
            get
            {
                if ((ModbusMode == 1)&&(this.serialport != null))
                {
                    return this.serialport.IsOpen;
                }
                if (this.udpFlag & (this.tcpClient != null))
                {
                    return true;
                }
                if (this.tcpClient == null)
                {
                    return false;
                }
                return this.tcpClient.Connected;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return this.connectTimeout;
            }
            set
            {
                this.connectTimeout = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
            set
            {
                this.ipAddress = value;
            }
        }

        public System.IO.Ports.Parity Parity
        {
            get
            {
                if ((ModbusMode == 1)&&(this.serialport != null))
                {
                    return this.parity;
                }
                return System.IO.Ports.Parity.Odd;
            }
            set
            {
                if ((ModbusMode == 1)&&(this.serialport != null))
                {
                    this.parity = value;
                }
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        public System.IO.Ports.StopBits StopBits
        {
            get
            {
                if ((ModbusMode == 1)&&(this.serialport != null))
                {
                    return this.stopBits;
                }
                return System.IO.Ports.StopBits.One;
            }
            set
            {
                if ((ModbusMode == 1)&&(this.serialport != null))
                {
                    this.stopBits = value;
                }
            }
        }

        public bool UDPFlag
        {
            get
            {
                return this.udpFlag;
            }
            set
            {
                this.udpFlag = value;
            }
        }

        public byte UnitIdentifier
        {
            get
            {
                return this.unitIdentifier;
            }
            set
            {
                this.unitIdentifier = value;
            }
        }

        public enum DataType
        {
            Short,
            UShort,
            Long,
            ULong,
            Float,
            Double
        }

        public delegate void ReceiveDataChanged(object sender);

        public enum RegisterOrder
        {
            LowHigh,
            HighLow
        }

        public delegate void SendDataChanged(object sender);
    }
}

