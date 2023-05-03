
using DiawModbus.Exceptions;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;


namespace DiawModbus
{

    public class ModbusServer
    {
      

        private int baudrate = 0x2580;
        private byte[] bytes = new byte[0x834];
        private Thread clientConnectionThread;
        public bool[] coils = new bool[0xffff];
        private bool dataReceived = false;
        public bool[] discreteInputs = new bool[0xffff];
        public short[] holdingRegisters = new short[0xffff];
        public short[] inputRegisters = new short[0xffff];
        private IPAddress ipAddressIn;
        private IPEndPoint iPEndPoint;
        private DateTime lastReceive;
        private Thread listenerThread;
        private ModbusProtocol[] modbusLogData = new ModbusProtocol[100];
        private int nextSign = 0;
        private int numberOfConnections = 0;
        private System.IO.Ports.Parity parity = System.IO.Ports.Parity.Even;
        private int port = 0x1f6;
        private int portIn;
        private byte[] readBuffer = new byte[0x82e];
        private ModbusProtocol receiveData;
        private ModbusProtocol sendData = new ModbusProtocol();
        private bool serialFlag;
        private System.IO.Ports.SerialPort serialport;
        private string serialPort = "COM1";
        private System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.One;
        private NetworkStream stream;
        private TCPHandler tcpHandler;
        private UdpClient udpClient;
        private bool udpFlag;
        private byte unitIdentifier = 1;


        public delegate void CoilsChanged(int coil, int numberOfCoils);
        public delegate void HoldingRegistersChanged(int register, int numberOfRegisters);
        public delegate void LogDataChanged();
        public delegate void NumberOfConnectedClientsChanged();

        public event CoilsChanged coilsChanged;
        public event HoldingRegistersChanged holdingRegistersChanged;
        public event LogDataChanged logDataChanged;
        public event NumberOfConnectedClientsChanged numberOfConnectedClientsChanged;

        private void CreateAnswer(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            switch (receiveData.functionCode)
            {
                case 0x10:
                    if (!this.FunctionCode16Disabled)
                    {
                        this.WriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    else
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    break;

                case 0x17:
                    if (!this.FunctionCode23Disabled)
                    {
                        this.ReadWriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    else
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    break;

                case 1:
                    if (this.FunctionCode1Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.ReadCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 2:
                    if (this.FunctionCode2Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.ReadDiscreteInputs(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 3:
                    if (this.FunctionCode3Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.ReadHoldingRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 4:
                    if (this.FunctionCode4Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.ReadInputRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 5:
                    if (this.FunctionCode5Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.WriteSingleCoil(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 6:
                    if (this.FunctionCode6Disabled)
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
                    }
                    this.WriteSingleRegister(receiveData, sendData, stream, portIn, ipAddressIn);
                    break;

                case 15:
                    if (!this.FunctionCode15Disabled)
                    {
                        this.WriteMultipleCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    else
                    {
                        sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    break;

                default:
                    sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                    sendData.exceptionCode = 1;
                    this.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    break;
            }
            sendData.timeStamp = DateTime.Now;
        }
        private void CreateLogData(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            for (int i = 0; i < 0x62; i++)
            {
                this.modbusLogData[0x63 - i] = this.modbusLogData[(0x63 - i) - 2];
            }
            this.modbusLogData[0] = receiveData;
            this.modbusLogData[1] = sendData;
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int num = 0xfa0 / this.baudrate;
            if ((DateTime.Now.Ticks - this.lastReceive.Ticks) > (0x2710L * num))
            {
                this.nextSign = 0;
            }
            System.IO.Ports.SerialPort port = (System.IO.Ports.SerialPort) sender;
            int bytesToRead = port.BytesToRead;
            byte[] buffer = new byte[bytesToRead];
            port.Read(buffer, 0, bytesToRead);
            Array.Copy(buffer, 0, this.readBuffer, this.nextSign, buffer.Length);
            this.lastReceive = DateTime.Now;
            this.nextSign = bytesToRead + this.nextSign;
            if (ModbusClient.DetectValidModbusFrame(this.readBuffer, this.nextSign))
            {
                this.dataReceived = true;
                this.nextSign = 0;
            }
            else
            {
                this.dataReceived = false;
            }
        }
        public void Listen()
        {
            this.listenerThread = new Thread(new ThreadStart(this.ListenerThread));
            this.listenerThread.Start();
        }

        private void ListenerThread()
        {
            if (!this.udpFlag & !this.serialFlag)
            {
                if (this.udpClient != null)
                {
                    try
                    {
                        this.udpClient.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
                this.tcpHandler = new TCPHandler(this.port);
                this.tcpHandler.dataChanged += new TCPHandler.DataChanged(this.ProcessReceivedData);
                this.tcpHandler.numberOfClientsChanged += new TCPHandler.NumberOfClientsChanged(this.numberOfClientsChanged);
            }
            else
            {
                while (true)
                {
                    if (this.udpFlag)
                    {
                        if ((this.udpClient == null) | this.PortChanged)
                        {
                            this.udpClient = new UdpClient(this.port);
                            this.udpClient.Client.ReceiveTimeout = 0x3e8;
                            this.iPEndPoint = new IPEndPoint(IPAddress.Any, this.port);
                            this.PortChanged = false;
                        }
                        if (this.tcpHandler != null)
                        {
                            this.tcpHandler.Disconnect();
                        }
                        try
                        {
                            this.bytes = this.udpClient.Receive(ref this.iPEndPoint);
                            this.portIn = this.iPEndPoint.Port;
                            NetworkConnectionParameter parameter = new NetworkConnectionParameter {
                                bytes = this.bytes
                            };
                            this.ipAddressIn = this.iPEndPoint.Address;
                            parameter.portIn = this.portIn;
                            parameter.ipAddressIn = this.ipAddressIn;
                            ParameterizedThreadStart start = new ParameterizedThreadStart(this.ProcessReceivedData);
                            new Thread(start).Start(parameter);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    if (this.serialFlag)
                    {
                        if (this.serialport == null)
                        {
                            this.serialport = new System.IO.Ports.SerialPort();
                            this.serialport.PortName = this.serialPort;
                            this.serialport.BaudRate = this.baudrate;
                            this.serialport.Parity = this.parity;
                            this.serialport.StopBits = this.stopBits;
                            this.serialport.WriteTimeout = 0x2710;
                            this.serialport.ReadTimeout = 0x3e8;
                            this.serialport.DataReceived += new SerialDataReceivedEventHandler(this.DataReceivedHandler);
                            this.serialport.Open();
                        }
                        if (this.dataReceived)
                        {
                            NetworkConnectionParameter parameter2 = new NetworkConnectionParameter {
                                bytes = this.readBuffer
                            };
                            ParameterizedThreadStart start2 = new ParameterizedThreadStart(this.ProcessReceivedData);
                            new Thread(start2).Start(parameter2);
                            this.dataReceived = false;
                        }
                    }
                }
            }
        }
        private void numberOfClientsChanged()
        {
            this.numberOfConnections = this.tcpHandler.NumberOfConnectedClients;
            if (this.numberOfConnectedClientsChanged != null)
            {
                this.numberOfConnectedClientsChanged();
            }
        }
        private void ProcessReceivedData(object networkConnectionParameter)
        {
            byte[] destinationArray = new byte[((NetworkConnectionParameter) networkConnectionParameter).bytes.Length];
            NetworkStream stream = ((NetworkConnectionParameter) networkConnectionParameter).stream;
            int portIn = ((NetworkConnectionParameter) networkConnectionParameter).portIn;
            IPAddress ipAddressIn = ((NetworkConnectionParameter) networkConnectionParameter).ipAddressIn;
            Array.Copy(((NetworkConnectionParameter) networkConnectionParameter).bytes, 0, destinationArray, 0, ((NetworkConnectionParameter) networkConnectionParameter).bytes.Length);
            ModbusProtocol receiveData = new ModbusProtocol();
            ModbusProtocol sendData = new ModbusProtocol();
            ushort[] dst = new ushort[1];
            byte[] src = new byte[2];
            receiveData.timeStamp = DateTime.Now;
            receiveData.request = true;
            if (!this.serialFlag)
            {
                src[1] = destinationArray[0];
                src[0] = destinationArray[1];
                Buffer.BlockCopy(src, 0, dst, 0, 2);
                receiveData.transactionIdentifier = dst[0];
                src[1] = destinationArray[2];
                src[0] = destinationArray[3];
                Buffer.BlockCopy(src, 0, dst, 0, 2);
                receiveData.protocolIdentifier = dst[0];
                src[1] = destinationArray[4];
                src[0] = destinationArray[5];
                Buffer.BlockCopy(src, 0, dst, 0, 2);
                receiveData.length = dst[0];
            }
            receiveData.unitIdentifier = destinationArray[6 - (6 * Convert.ToInt32(this.serialFlag))];
            if (!((receiveData.unitIdentifier != this.unitIdentifier) & (receiveData.unitIdentifier > 0)))
            {
                receiveData.functionCode = destinationArray[7 - (6 * Convert.ToInt32(this.serialFlag))];
                src[1] = destinationArray[8 - (6 * Convert.ToInt32(this.serialFlag))];
                src[0] = destinationArray[9 - (6 * Convert.ToInt32(this.serialFlag))];
                Buffer.BlockCopy(src, 0, dst, 0, 2);
                receiveData.startingAdress = dst[0];
                if (receiveData.functionCode <= 4)
                {
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.quantity = dst[0];
                }
                if (receiveData.functionCode == 5)
                {
                    receiveData.receiveCoilValues = new ushort[1];
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, receiveData.receiveCoilValues, 0, 2);
                }
                if (receiveData.functionCode == 6)
                {
                    receiveData.receiveRegisterValues = new ushort[1];
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, receiveData.receiveRegisterValues, 0, 2);
                }
                if (receiveData.functionCode == 15)
                {
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.quantity = dst[0];
                    receiveData.byteCount = destinationArray[12 - (6 * Convert.ToInt32(this.serialFlag))];
                    if ((receiveData.byteCount % 2) > 0)
                    {
                        receiveData.receiveCoilValues = new ushort[(receiveData.byteCount / 2) + 1];
                    }
                    else
                    {
                        receiveData.receiveCoilValues = new ushort[receiveData.byteCount / 2];
                    }
                    Buffer.BlockCopy(destinationArray, 13 - (6 * Convert.ToInt32(this.serialFlag)), receiveData.receiveCoilValues, 0, receiveData.byteCount);
                }
                if (receiveData.functionCode == 0x10)
                {
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.quantity = dst[0];
                    receiveData.byteCount = destinationArray[12 - (6 * Convert.ToInt32(this.serialFlag))];
                    receiveData.receiveRegisterValues = new ushort[receiveData.quantity];
                    for (int i = 0; i < receiveData.quantity; i++)
                    {
                        src[1] = destinationArray[(13 + (i * 2)) - (6 * Convert.ToInt32(this.serialFlag))];
                        src[0] = destinationArray[(14 + (i * 2)) - (6 * Convert.ToInt32(this.serialFlag))];
                        Buffer.BlockCopy(src, 0, receiveData.receiveRegisterValues, i * 2, 2);
                    }
                }
                if (receiveData.functionCode == 0x17)
                {
                    src[1] = destinationArray[8 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[9 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.startingAddressRead = dst[0];
                    src[1] = destinationArray[10 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[11 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.quantityRead = dst[0];
                    src[1] = destinationArray[12 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[13 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.startingAddressWrite = dst[0];
                    src[1] = destinationArray[14 - (6 * Convert.ToInt32(this.serialFlag))];
                    src[0] = destinationArray[15 - (6 * Convert.ToInt32(this.serialFlag))];
                    Buffer.BlockCopy(src, 0, dst, 0, 2);
                    receiveData.quantityWrite = dst[0];
                    receiveData.byteCount = destinationArray[0x10 - (6 * Convert.ToInt32(this.serialFlag))];
                    receiveData.receiveRegisterValues = new ushort[receiveData.quantityWrite];
                    for (int j = 0; j < receiveData.quantityWrite; j++)
                    {
                        src[1] = destinationArray[(0x11 + (j * 2)) - (6 * Convert.ToInt32(this.serialFlag))];
                        src[0] = destinationArray[(0x12 + (j * 2)) - (6 * Convert.ToInt32(this.serialFlag))];
                        Buffer.BlockCopy(src, 0, receiveData.receiveRegisterValues, j * 2, 2);
                    }
                }
                this.CreateAnswer(receiveData, sendData, stream, portIn, ipAddressIn);
                this.CreateLogData(receiveData, sendData);
                if (this.logDataChanged != null)
                {
                    this.logDataChanged();
                }
            }
        }
        private void ReadCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x7d0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if ((receiveData.quantity % 8) == 0)
                {
                    sendData.byteCount = (byte) (receiveData.quantity / 8);
                }
                else
                {
                    sendData.byteCount = (byte) ((receiveData.quantity / 8) + 1);
                }
                sendData.sendCoilValues = new bool[receiveData.quantity];
                Array.Copy(this.coils, receiveData.startingAdress + 1, sendData.sendCoilValues, 0, receiveData.quantity);
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            buffer[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendCoilValues = null;
            }
            if (sendData.sendCoilValues != null)
            {
                for (int i = 0; i < sendData.byteCount; i++)
                {
                    bytes = new byte[2];
                    for (int j = 0; j < 8; j++)
                    {
                        byte num3;
                        if (sendData.sendCoilValues[(i * 8) + j])
                        {
                            num3 = 1;
                        }
                        else
                        {
                            num3 = 0;
                        }
                        bytes[1] = (byte) (bytes[1] | (num3 << j));
                        if ((((i * 8) + j) + 1) >= sendData.sendCoilValues.Length)
                        {
                            break;
                        }
                    }
                    buffer[9 + i] = bytes[1];
                }
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
        }
        private void ReadDiscreteInputs(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x7d0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if ((receiveData.quantity % 8) == 0)
                {
                    sendData.byteCount = (byte) (receiveData.quantity / 8);
                }
                else
                {
                    sendData.byteCount = (byte) ((receiveData.quantity / 8) + 1);
                }
                sendData.sendCoilValues = new bool[receiveData.quantity];
                Array.Copy(this.discreteInputs, receiveData.startingAdress + 1, sendData.sendCoilValues, 0, receiveData.quantity);
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            buffer[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendCoilValues = null;
            }
            if (sendData.sendCoilValues != null)
            {
                for (int i = 0; i < sendData.byteCount; i++)
                {
                    bytes = new byte[2];
                    for (int j = 0; j < 8; j++)
                    {
                        byte num3;
                        if (sendData.sendCoilValues[(i * 8) + j])
                        {
                            num3 = 1;
                        }
                        else
                        {
                            num3 = 0;
                        }
                        bytes[1] = (byte) (bytes[1] | (num3 << j));
                        if ((((i * 8) + j) + 1) >= sendData.sendCoilValues.Length)
                        {
                            break;
                        }
                    }
                    buffer[9 + i] = bytes[1];
                }
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
        }
        private void ReadHoldingRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x7d))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.byteCount = (byte) (2 * receiveData.quantity);
                sendData.sendRegisterValues = new short[receiveData.quantity];
                Buffer.BlockCopy(this.holdingRegisters, (receiveData.startingAdress * 2) + 2, sendData.sendRegisterValues, 0, receiveData.quantity * 2);
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = (ushort) (3 + sendData.byteCount);
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            buffer[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            if (sendData.sendRegisterValues != null)
            {
                for (int i = 0; i < (sendData.byteCount / 2); i++)
                {
                    bytes = BitConverter.GetBytes(sendData.sendRegisterValues[i]);
                    buffer[9 + (i * 2)] = bytes[1];
                    buffer[10 + (i * 2)] = bytes[0];
                }
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
        }
        private void ReadInputRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x7d))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.byteCount = (byte) (2 * receiveData.quantity);
                sendData.sendRegisterValues = new short[receiveData.quantity];
                Buffer.BlockCopy(this.inputRegisters, (receiveData.startingAdress * 2) + 2, sendData.sendRegisterValues, 0, receiveData.quantity * 2);
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = (ushort) (3 + sendData.byteCount);
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            buffer[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            if (sendData.sendRegisterValues != null)
            {
                for (int i = 0; i < (sendData.byteCount / 2); i++)
                {
                    bytes = BitConverter.GetBytes(sendData.sendRegisterValues[i]);
                    buffer[9 + (i * 2)] = bytes[1];
                    buffer[10 + (i * 2)] = bytes[0];
                }
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
        }
        private void ReadWriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if (((((receiveData.quantityRead < 1) | (receiveData.quantityRead > 0x7d)) | (receiveData.quantityWrite < 1)) | (receiveData.quantityWrite > 0x79)) | (receiveData.byteCount != (receiveData.quantityWrite * 2)))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((((receiveData.startingAddressRead + 1) + receiveData.quantityRead) > 0xffff) | (((receiveData.startingAddressWrite + 1) + receiveData.quantityWrite) > 0xffff)) | (receiveData.quantityWrite < 0)) | (receiveData.quantityRead < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.sendRegisterValues = new short[receiveData.quantityRead];
                Buffer.BlockCopy(this.holdingRegisters, (receiveData.startingAddressRead * 2) + 2, sendData.sendRegisterValues, 0, receiveData.quantityRead * 2);
                for (int i = 0; i < receiveData.quantityWrite; i++)
                {
                    this.holdingRegisters[(receiveData.startingAddressWrite + i) + 1] = (short) receiveData.receiveRegisterValues[i];
                }
                sendData.byteCount = (byte) (2 * receiveData.quantityRead);
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = Convert.ToUInt16((int) (3 + (2 * receiveData.quantityRead)));
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            buffer[8] = sendData.byteCount;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else if (sendData.sendRegisterValues != null)
            {
                for (int j = 0; j < (sendData.byteCount / 2); j++)
                {
                    bytes = BitConverter.GetBytes(sendData.sendRegisterValues[j]);
                    buffer[9 + (j * 2)] = bytes[1];
                    buffer[10 + (j * 2)] = bytes[0];
                }
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
            if (this.holdingRegistersChanged != null)
            {
                this.holdingRegistersChanged(receiveData.startingAddressWrite, receiveData.quantityWrite);
            }
        }
        private void sendException(int errorCode, int exceptionCode, ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = receiveData.unitIdentifier;
            sendData.errorCode = (byte) errorCode;
            sendData.exceptionCode = (byte) exceptionCode;
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = (ushort) (3 + sendData.byteCount);
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[(9 + sendData.byteCount) + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.errorCode;
            buffer[8] = sendData.exceptionCode;
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
        }
        public void StopListening()
        {
            if ((this.SerialFlag & (this.serialport != null)) && this.serialport.IsOpen)
            {
                this.serialport.Close();
            }
            try
            {
                this.tcpHandler.Disconnect();
                this.listenerThread.Abort();
            }
            catch (Exception)
            {
            }
            this.listenerThread.Join();
            try
            {
                this.clientConnectionThread.Abort();
            }
            catch (Exception)
            {
            }
        }
        private void WriteMultipleCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;
            if ((receiveData.quantity == 0) | (receiveData.quantity > 0x7b0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                for (int i = 0; i < receiveData.quantity; i++)
                {
                    int num2 = i % 0x10;
                    int num3 = 1;
                    num3 = num3 << num2;
                    if ((receiveData.receiveCoilValues[i / 0x10] & ((ushort) num3)) == 0)
                    {
                        this.coils[(receiveData.startingAdress + i) + 1] = false;
                    }
                    else
                    {
                        this.coils[(receiveData.startingAdress + i) + 1] = true;
                    }
                }
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = 6;
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[12 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                bytes = BitConverter.GetBytes((int) receiveData.startingAdress);
                buffer[8] = bytes[1];
                buffer[9] = bytes[0];
                bytes = BitConverter.GetBytes((int) receiveData.quantity);
                buffer[10] = bytes[1];
                buffer[11] = bytes[0];
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
            if (this.coilsChanged != null)
            {
                this.coilsChanged(receiveData.startingAdress, 1);
            }
        }
        private void WriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;
            if ((receiveData.quantity == 0) | (receiveData.quantity > 0x7b0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((receiveData.startingAdress + 1) + receiveData.quantity) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                for (int i = 0; i < receiveData.quantity; i++)
                {
                    this.holdingRegisters[(receiveData.startingAdress + i) + 1] = (short) receiveData.receiveRegisterValues[i];
                }
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = 6;
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[12 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                bytes = BitConverter.GetBytes((int) receiveData.startingAdress);
                buffer[8] = bytes[1];
                buffer[9] = bytes[0];
                bytes = BitConverter.GetBytes((int) receiveData.quantity);
                buffer[10] = bytes[1];
                buffer[11] = bytes[0];
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
            if (this.holdingRegistersChanged != null)
            {
                this.holdingRegistersChanged(receiveData.startingAdress, receiveData.quantity);
            }
        }
        private void WriteSingleCoil(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveCoilValues = receiveData.receiveCoilValues;
            if ((receiveData.receiveCoilValues[0] > 0) & (receiveData.receiveCoilValues[0] != 0xff00))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if (receiveData.receiveCoilValues[0] == 0xff00)
                {
                    this.coils[receiveData.startingAdress + 1] = true;
                }
                if (receiveData.receiveCoilValues[0] == 0)
                {
                    this.coils[receiveData.startingAdress + 1] = false;
                }
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = 6;
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[12 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                bytes = BitConverter.GetBytes((int) receiveData.startingAdress);
                buffer[8] = bytes[1];
                buffer[9] = bytes[0];
                bytes = BitConverter.GetBytes((int) receiveData.receiveCoilValues[0]);
                buffer[10] = bytes[1];
                buffer[11] = bytes[0];
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
            if (this.coilsChanged != null)
            {
                this.coilsChanged(receiveData.startingAdress, 1);
            }
        }
        private void WriteSingleRegister(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            byte[] buffer;
            sendData.response = true;
            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;
            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveRegisterValues = receiveData.receiveRegisterValues;
            if ((receiveData.receiveRegisterValues[0] < 0) | (receiveData.receiveRegisterValues[0] > 0xffff))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1) > 0xffff) | (receiveData.startingAdress < 0))
            {
                sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                this.holdingRegisters[receiveData.startingAdress + 1] = (short) receiveData.receiveRegisterValues[0];
            }
            if (sendData.exceptionCode > 0)
            {
                sendData.length = 3;
            }
            else
            {
                sendData.length = 6;
            }
            if (sendData.exceptionCode > 0)
            {
                buffer = new byte[9 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            else
            {
                buffer = new byte[12 + (2 * Convert.ToInt32(this.serialFlag))];
            }
            byte[] bytes = new byte[2];
            sendData.length = (byte) (buffer.Length - 6);
            bytes = BitConverter.GetBytes((int) sendData.transactionIdentifier);
            buffer[0] = bytes[1];
            buffer[1] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.protocolIdentifier);
            buffer[2] = bytes[1];
            buffer[3] = bytes[0];
            bytes = BitConverter.GetBytes((int) sendData.length);
            buffer[4] = bytes[1];
            buffer[5] = bytes[0];
            buffer[6] = sendData.unitIdentifier;
            buffer[7] = sendData.functionCode;
            if (sendData.exceptionCode > 0)
            {
                buffer[7] = sendData.errorCode;
                buffer[8] = sendData.exceptionCode;
                sendData.sendRegisterValues = null;
            }
            else
            {
                bytes = BitConverter.GetBytes((int) receiveData.startingAdress);
                buffer[8] = bytes[1];
                buffer[9] = bytes[0];
                bytes = BitConverter.GetBytes((int) receiveData.receiveRegisterValues[0]);
                buffer[10] = bytes[1];
                buffer[11] = bytes[0];
            }
            try
            {
                if (this.serialFlag)
                {
                    if (!this.serialport.IsOpen)
                    {
                        throw new SerialPortNotOpenedException("serial port not opened");
                    }
                    sendData.crc = ModbusClient.calculateCRC(buffer, Convert.ToUInt16((int) (buffer.Length - 8)), 6);
                    bytes = BitConverter.GetBytes((int) sendData.crc);
                    buffer[buffer.Length - 2] = bytes[0];
                    buffer[buffer.Length - 1] = bytes[1];
                    this.serialport.Write(buffer, 6, buffer.Length - 6);
                }
                else if (this.udpFlag)
                {
                    IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                    this.udpClient.Send(buffer, buffer.Length, endPoint);
                }
                else
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
            }
            if (this.holdingRegistersChanged != null)
            {
                this.holdingRegistersChanged(receiveData.startingAdress, 1);
            }
        }
        public int Baudrate
        {
            get
            {
                return this.baudrate;
            }
            set
            {
                this.baudrate = value;
            }
        }

        public bool FunctionCode15Disabled { get; set; }

        public bool FunctionCode16Disabled { get; set; }

        public bool FunctionCode1Disabled { get; set; }

        public bool FunctionCode23Disabled { get; set; }

        public bool FunctionCode2Disabled { get; set; }

        public bool FunctionCode3Disabled { get; set; }

        public bool FunctionCode4Disabled { get; set; }

        public bool FunctionCode5Disabled { get; set; }

        public bool FunctionCode6Disabled { get; set; }

        public ModbusProtocol[] ModbusLogData
        {
            get
            {
                return this.modbusLogData;
            }
        }
        public int NumberOfConnections
        {
            get
            {
                return this.numberOfConnections;
            }
        }
        public System.IO.Ports.Parity Parity
        {
            get
            {
                return this.parity;
            }
            set
            {
                this.parity = value;
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
        public bool PortChanged { get; set; }
        public bool SerialFlag
        {
            get
            {
                return this.serialFlag;
            }
            set
            {
                this.serialFlag = value;
            }
        }
        public string SerialPort
        {
            get
            {
                return this.serialPort;
            }
            set
            {
                this.serialPort = value;
            }
        }
        public System.IO.Ports.StopBits StopBits
        {
            get
            {
                return this.stopBits;
            }
            set
            {
                this.stopBits = value;
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

    }
}

