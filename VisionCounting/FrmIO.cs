using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MetroFramework.Forms;
using MetroFramework.Components;
using MetroFramework;


using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using Ini;
using System.Threading;

using DiawModbus;
using DLCounting;

using Modbus64;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace VisionCounting
{
    public partial class FrmIO : MetroForm
    {

        #region  NOCLOSE_BUTTON
        private const int CP_NOCLOSE_BUTTON = 0x200;
        private const int WS_SYSMENU = 0x80000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_SYSMENU;
                return cp;
            }
        }
        #endregion


        public Frmmain ParentForm { get; set; }


        private string aPath = "";
        IniFile ini;

        System.Globalization.CultureInfo DatetimeSystem = System.Globalization.CultureInfo.GetCultureInfo("th-TH");
        UTF8Encoding utf8 = new UTF8Encoding();

       


        public string Data = "";
        public string BarCodeData = "";


        public byte[] start_decoding = new byte[6] { 0x57, 0x00, 0x18, 0x00, 0x55, 0x00 };//57 00 18 00 55 00
        public byte[] stop_decoding = new byte[6] { 0x57, 0x00, 0x19, 0x00, 0x55, 0x00 }; //57 00 19 00 55 00

        string BARCODE1_PORT = "COM3";

        Thread threadIO;
        private bool runIO = false;
        public bool ReadIO = false;


        public ModbusClient modbusIAI;
        private delegate void UpdateReceiveDataCallback(object sender);
        public int LoopID = 0;

        bool[] OutputCoils = new bool[16] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        PictureBox[] PicDI;
        PictureBox[] PicDO;
        Button[] ButtonOutputCoils;


        bool connectADAM1 = false;
        bool connectADAM2 = false;

        private Thread _readThreadBarcode;
        private volatile bool _keepReadingBarcode;
        public bool Statebarcode = false;


        public bool BoxReady = false;
        public bool PcsConvRunning = false;
        public bool PLC_Warning = false;
        public bool PLC_Error = false;
        public bool PLC_Initial = false;
        public bool PLC_Reset = false;

        public bool AutoStartPcsConv = false;
        public bool GateClose = false;
       

        public FrmIO()
        {
            InitializeComponent();

            aPath = Application.StartupPath + "\\config.ini";
            ini = new IniFile(aPath);




            ButtonOutputCoils = new Button[] {   btDO0_1, btDO1_1, btDO2_1, btDO3_1, btDO4_1
                                                ,btDO5_1, btDO6_1, btDO7_1, btDO8_1, btDO9_1
                                                ,btDO10_1, btDO11_1, btDO12_1, btDO13_1, btDO14_1
                                                , btDO15_1 };

            PicDI = new PictureBox[] { picDI0_1, picDI1_1, picDI2_1, picDI3_1, picDI4_1
                                      ,picDI5_1, picDI6_1, picDI7_1, picDI8_1, picDI9_1
                                      ,picDI10_1, picDI11_1, picDI12_1, picDI13_1, picDI14_1
                                      ,picDI15_1 };
            
            PicDO = new PictureBox[] { pic_DO0_1, pic_DO1_1, pic_DO2_1, pic_DO3_1, pic_DO4_1
                                      ,pic_DO5_1, pic_DO6_1, pic_DO7_1, pic_DO8_1, pic_DO9_1
                                      ,pic_DO10_1, pic_DO11_1, pic_DO12_1, pic_DO13_1, pic_DO14_1
                                      ,pic_DO15_1 };
        }


        public bool ComplexPing(string IP)
        {

            bool ret = false;

            Ping pingSender = new Ping();

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            // Wait 10 seconds for a reply.
            int timeout = 100;

            // Set options for transmission:
            // The data can go through 64 gateways or routers
            // before it is destroyed, and the data packet
            // cannot be fragmented.
            PingOptions options = new PingOptions(64, true);

            // Send the request.
            try
            {
                PingReply reply = pingSender.Send(IP, timeout, buffer, options);

                if (reply.Status == IPStatus.Success)
                {
                    ret = true;
                    //Console.WriteLine("Address: {0}", reply.Address.ToString());
                    //Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                    //Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                    //Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                    //Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                }
                else
                {
                    //Console.WriteLine(reply.Status);
                    ret = false;
                }
            }
            catch
            {
                ret = false;
            }


            return ret;
        }

        private void FrmIO_Load(object sender, EventArgs e)
        {

            foreach (string s in SerialPort.GetPortNames())
            {
                cbCOMIAI.Items.Add(s);
                cbSerial.Items.Add(s);
            }

            string ret1 = ini.IniReadValue("ADAM IP", "ADAM6251", "192.168.1.40");
            string ret2 = ini.IniReadValue("ADAM IP", "ADAM6256", "192.168.1.41");


            #region Read IO Information


            cbCOMIAI.Text = ini.IniReadValue("IAI", "Port", "");
            cbSerial.Text = ini.IniReadValue("Barcode", "Port", "");
            txtBarcodeLength.Text = ini.IniReadValue("Barcode", "Length", txtBarcodeLength.Text);



            #endregion Read IO Information











            try
            {


                bool pingNetwork1 = ComplexPing(ret1);
                bool pingNetwork2 = ComplexPing(ret2);



                Modbus64.Result Result;

                if (pingNetwork1 == true)
                { 
                        
                        modbus64Control1.Mode = Modbus64.Mode.TCP_IP;
                        modbus64Control1.ResponseTimeout = 100;

                        Result = modbus64Control1.Connect(ret1, 502); //OUTPUT ADAM-6251
                        if (Result == Modbus64.Result.SUCCESS)
                        {
                            lbInputState.Text = "Connect...";
                            connectADAM1 = true;
                        }
                        else
                        {
                            lbInputState.Text = "Disconnect...";
                            connectADAM1 = false;
                        }
                }
                else
                {
                    lbInputState.Text = "Disconnect...";
                    connectADAM1 = false;

                }


                if (pingNetwork2 == true)
                {
                    modbus64Control2.Mode = Modbus64.Mode.TCP_IP;
                    modbus64Control2.ResponseTimeout = 100;

                    Result = modbus64Control2.Connect(ret2, 502); //OUTPUT ADAM-6256
                    if (Result == Modbus64.Result.SUCCESS)
                    {

                        connectADAM2 = true;

                        lbOutputState.Text = "Connect...";

                        bool[] OutputCoilsState = new bool[16];
                        Result = modbus64Control2.ReadCoils(1, 17 - 1, 16, OutputCoilsState); //OUTPUT ADAM-6256
                        if (Result == Modbus64.Result.SUCCESS)
                        {
                            OutputCoils = OutputCoilsState;
                        }

                    }
                    else
                    {
                        lbOutputState.Text = "Disconnect...";
                        connectADAM2 = false;
                    }
                }
                else
                {
                    lbOutputState.Text = "Disconnect...";
                    connectADAM2 = false;

                }


                if (connectADAM2 == true)
                {
                    UpdateOutput(0, true); //Auto mode

                    UpdateOutput(1, false); //ManualStartPcsConv
                    UpdateOutput(2, false); //AutoStartPcsConv
                    UpdateOutput(3, false); //GateClose
                    UpdateOutput(4, false);
                    UpdateOutput(5, false);
                    UpdateOutput(6, false); //ResultTrigger
                    UpdateOutput(7, false); //FG

                    UpdateOutput(8, false); //Near the set target value
                    UpdateOutput(9, false);

                }




                runIO = true;
                threadIO = new Thread(new ThreadStart(ThreadEventIO));
                threadIO.IsBackground = true;
                threadIO.Start();
                ReadIO = true;


            }
            catch
            {


            }





            if (cbCOMIAI.Text != "")
            {
                InitialModbusSerial(cbCOMIAI.Text);
            }

            timerDelay.Enabled = true;






            BarcodeInitial();


        }


        public void BarcodeInitial()
        {

            if (cbSerial.Text != "")
            { 
              InitialSerialPort(cbSerial.Text);
            }
     
        }

        public bool InitialSerialPort(string COM)
        {
            bool error = false;


          

            try
            {

                Serialport.PortName = COM;
                Serialport.BaudRate = 9600;
                Serialport.DataBits = 8;

                Serialport.StopBits = StopBits.One;
                Serialport.Parity = Parity.None;
                Serialport.Handshake = Handshake.None;

                Serialport.Open();
                error = true;
                Statebarcode = true;
                StartBarcodeReading();
                timerBarcode.Enabled = true;
            }
            catch
            {
                error = false;
                Statebarcode = false;
                timerBarcode.Enabled = false;
            }

            return error;
        }


        private void StartBarcodeReading()
        {

           
            if (!_keepReadingBarcode)
            {
                _keepReadingBarcode = true;
                _readThreadBarcode = new Thread(ReadBarcodePort);
                _readThreadBarcode.Start();
            }
        }

        private void StopBarcodeReading()
        {
            if (_keepReadingBarcode)
            {
                _keepReadingBarcode = false;
                _readThreadBarcode.Join();
                _readThreadBarcode = null;
            }
        }

        private void ReadBarcodePort()
        {
            while (_keepReadingBarcode)
            {
                if (Serialport.IsOpen)
                {


                    try
                    {

                        string Data = Serialport.ReadLine();

                        string sOut = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(Data));
                        string[] data = sOut.Split('\0');
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (data[i].Length >= 5)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    ADDBarcodeBOXID(data[i].Trim());
                                }));
                            }
                        }

                    }
                    catch (TimeoutException) { }
                }
                else
                {

                    TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
                    Thread.Sleep(waitTime);
                }
            }
        }

        private void BtDone_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void BtSave_Click(object sender, EventArgs e)
        {
            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);

            ini.IniWriteValue("IAI", "Port", cbCOMIAI.Text);
            ini.IniWriteValue("Barcode", "Port", cbSerial.Text);
            ini.IniWriteValue("Barcode", "Length", txtBarcodeLength.Text);

          
        }



       

        private void ThreadEventIO()
        {


            while (runIO)
            {
                //this.Invoke(new EventHandler(StatusUpdate));
                try
                {

                    if (ReadIO == true) //เปิดใช้ตอน Setup เท่านั้น
                    {

                        Modbus64.Result Result;

                        bool[] InputCoilsState = new bool[16];
                        Result = modbus64Control1.ReadCoils(1, 1 - 1, 16, InputCoilsState); //OUTPUT ADAM-6256
                        if (Result == Modbus64.Result.SUCCESS)
                        {
                            string DataString = "";
                            string str = "";

                            for (int i = 0; i < InputCoilsState.Length; i++)
                            {

                                this.Invoke(new Action(() =>
                                {

                                    if (InputCoilsState[i] == true)
                                    {
                                        PicDI[i].Image = pictrue.Image;
                                    }
                                    else
                                    {
                                        PicDI[i].Image = picfail.Image;
                                    }

                                   

                                }));




                            }

                             
                            if (InputCoilsState[0] == true)//BoxReady
                            {
                                BoxReady = true;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicBoxReady.BackColor = Color.Red;

                                    if (ParentForm.picBoxPosRun.Image==null)
                                    { 
                                      ParentForm.picBoxPosRun.Image = ParentForm.picbox.Image;
                                    }

                                }
                            }
                            else
                            {
                                BoxReady = false;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicBoxReady.BackColor = Color.Black;

                                   // if (ParentForm.picBoxPosRun.Image != null)
                                   // {
                                        ParentForm.picBoxPosRun.Image = null;
                                    //}
                                }
                                
                            }

                            if (InputCoilsState[1] == true)//PcsConvRunning
                            {

                                PcsConvRunning = true;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicConveyorState.BackColor = Color.Red;

                                }

                            }
                            else
                            {
                                PcsConvRunning = false;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicConveyorState.BackColor = Color.Black;

                                }
                            }

                            if (InputCoilsState[2] == true)//PLC_Warning
                            {
                                PLC_Warning = true;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Warning.BackColor = Color.Red;

                                }
                                
                            }
                            else
                            {
                                PLC_Warning = false;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Warning.BackColor = Color.Black;

                                }
                            }

                            if (InputCoilsState[3] == true)//PLC Error
                            {
                                PLC_Error = true;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Error.BackColor = Color.Red;

                                }
                                
                            }
                            else
                            {
                                PLC_Error = false;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Error.BackColor = Color.Black;

                                }

                            }

                            if (InputCoilsState[4] == true)//PLC Initail
                            {
                                PLC_Initial = true;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Initial.BackColor = Color.Red;

                                }

                            }
                            else
                            {
                                PLC_Initial = false;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Initial.BackColor = Color.Black;

                                }

                            }
                            
                            if (InputCoilsState[5] == true)//PLC-->Rest
                            {
                                PLC_Reset = true;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Reset.BackColor = Color.Red;

                                }

                            }
                            else
                            {
                                PLC_Reset = false;

                                if (ParentForm != null)
                                {
                                    ParentForm.PicPLC_Reset.BackColor = Color.Black;

                                }

                            }

                        }

                        bool[] OutputCoilsState = new bool[16];
                        Result = modbus64Control2.ReadCoils(1, 16, 16, OutputCoilsState); //OUTPUT ADAM-6256
                        if (Result == Modbus64.Result.SUCCESS)
                        {
                            string DataString = "";
                            string str = "";



                            for (int i = 0; i < OutputCoilsState.Length; i++)
                            {

                                this.Invoke(new Action(() =>
                                {

                                    if (OutputCoilsState[i] == true)
                                    {
                                        PicDO[i].Image = pictrue.Image;
                                        ButtonOutputCoils[i].Text = "ON";
                                    }
                                    else
                                    {
                                        PicDO[i].Image = picfail.Image;
                                        ButtonOutputCoils[i].Text = "OFF";
                                    }

                                }));

                            }

                            if (OutputCoilsState[2] == true)//AutoStartPcsConv
                            {
                                AutoStartPcsConv = true;
                                if (ParentForm != null)
                                {
                                    ParentForm.picAutoStartPcsConv.BackColor = Color.Red;

                                   

                                }
                            }
                            else
                            {
                                AutoStartPcsConv = false;
                                if (ParentForm != null)
                                {
                                    ParentForm.picAutoStartPcsConv.BackColor = Color.Black;
                                }

                            }

                            if (OutputCoilsState[3] == true)//GateClose
                            {

                                GateClose = true;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicGateClose.BackColor = Color.Red;

                                }

                            }
                            else
                            {
                                GateClose = false;
                                if (ParentForm != null)
                                {
                                    ParentForm.PicGateClose.BackColor = Color.Black;

                                }
                            }

                        }

                        this.Invoke(new EventHandler(InvokeUpdateIO));

                        Thread.Sleep(50);
                    }
                }
                catch
                {
                    // returnData = "--------";
                    // remoteIP = string.Empty;

                    //  this.Invoke(new EventHandler(StatusUpdate));

                }
            }

        }

        private void InvokeUpdateIO(object sender, EventArgs e)
        {

            //#region Update ADM6251_1


            //if (ParentForm != null)
            //{
            //   // ParentForm.frmOperate.pictstateADAM.BackColor = Color.Yellow;
            //}


            //if (adaM6251.DI_CH[0] == true)
            //{
            //    picDI0_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI0_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[1] == true)
            //{
            //    picDI1_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI1_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[2] == true)
            //{
            //    picDI2_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI2_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[3] == true)
            //{
            //    picDI3_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI3_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[4] == true)
            //{
            //    picDI4_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI4_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[5] == true)
            //{
            //    picDI5_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI5_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[6] == true)
            //{
            //    picDI6_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI6_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[7] == true)
            //{
            //    picDI7_1.Image = pictrue.Image;
            //    //if (ParentForm != null)
            //    //{
            //    //    ParentForm.frmOperate.picDI7_1.Image = pictrue.Image;
            //    //}
            //}
            //else
            //{
            //    picDI7_1.Image = picfail.Image;
            //    //if (ParentForm != null)
            //    //{
            //    //    ParentForm.frmOperate.picDI7_1.Image = pictrue.Image;
            //    //}
            //}
            //if (adaM6251.DI_CH[8] == true)
            //{
            //    picDI8_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI8_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[9] == true)
            //{
            //    picDI9_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI9_1.Image = picfail.Image;
            //}


            //if (adaM6251.DI_CH[10] == true)
            //{
            //    picDI10_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI10_1.Image = picfail.Image;
            //}


            //if (adaM6251.DI_CH[11] == true)
            //{
            //    picDI11_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI11_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[12] == true)
            //{
            //    picDI12_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI12_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[13] == true)
            //{
            //    picDI13_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI13_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[14] == true)
            //{
            //    picDI14_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI14_1.Image = picfail.Image;
            //}
            //if (adaM6251.DI_CH[15] == true)
            //{
            //    picDI15_1.Image = pictrue.Image;
            //}
            //else
            //{
            //    picDI15_1.Image = picfail.Image;
            //}






            //if (adaM6251.DI_CH[10] == true)//Update 09052017
            //{
            //    //IdenStart = true;
            //    //if (ParentForm != null)
            //    //{
            //    //    //  ParentForm.frmOperate.picIden_Start.BackColor = Color.Green;
            //    //}
            //}
            //else
            //{
            //    //if (ParentForm != null)
            //    //{
            //    //    // ParentForm.frmOperate.picIden_Start.BackColor = Color.Black;
            //    //}
            //}



            //picDI0_1.Refresh();
            //picDI1_1.Refresh();
            //picDI2_1.Refresh();
            //picDI3_1.Refresh();

            //picDI4_1.Refresh();
            //picDI5_1.Refresh();
            //picDI6_1.Refresh();
            //picDI7_1.Refresh();

            //picDI8_1.Refresh();
            //picDI9_1.Refresh();
            //picDI10_1.Refresh();
            //picDI11_1.Refresh();

            //picDI12_1.Refresh();
            //picDI13_1.Refresh();
            //picDI14_1.Refresh();
            //picDI15_1.Refresh();
            //#endregion

            //#region Update ADM6256_1
            //if (adaM6256.DO_CH[0] == true)
            //{
            //    pic_DO0_1.Image = pictrue.Image;
            //    btDO0_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO0_1.Image = picfail.Image;
            //    btDO0_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[1] == true)
            //{
            //    pic_DO1_1.Image = pictrue.Image;
            //    btDO1_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO1_1.Image = picfail.Image;
            //    btDO1_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[2] == true)
            //{
            //    pic_DO2_1.Image = pictrue.Image;
            //    btDO2_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO2_1.Image = picfail.Image;
            //    btDO2_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[3] == true)
            //{
            //    pic_DO3_1.Image = pictrue.Image;
            //    btDO3_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO3_1.Image = picfail.Image;
            //    btDO3_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[4] == true)
            //{
            //    pic_DO4_1.Image = pictrue.Image;
            //    btDO4_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO4_1.Image = picfail.Image;
            //    btDO4_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[5] == true)
            //{
            //    pic_DO5_1.Image = pictrue.Image;
            //    btDO5_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO5_1.Image = picfail.Image;
            //    btDO5_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[6] == true)
            //{
            //    pic_DO6_1.Image = pictrue.Image;
            //    btDO6_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO6_1.Image = picfail.Image;
            //    btDO6_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[7] == true)
            //{
            //    pic_DO7_1.Image = pictrue.Image;
            //    btDO7_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO7_1.Image = picfail.Image;
            //    btDO7_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[8] == true)
            //{
            //    pic_DO8_1.Image = pictrue.Image;
            //    btDO8_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO8_1.Image = picfail.Image;
            //    btDO8_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[9] == true)
            //{
            //    pic_DO9_1.Image = pictrue.Image;
            //    btDO9_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO9_1.Image = picfail.Image;
            //    btDO9_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[10] == true)
            //{
            //    pic_DO10_1.Image = pictrue.Image;
            //    btDO10_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO10_1.Image = picfail.Image;
            //    btDO10_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[11] == true)
            //{
            //    pic_DO11_1.Image = pictrue.Image;
            //    btDO11_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO11_1.Image = picfail.Image;
            //    btDO11_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[12] == true)
            //{
            //    pic_DO12_1.Image = pictrue.Image;
            //    btDO12_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO12_1.Image = picfail.Image;
            //    btDO12_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[13] == true)
            //{
            //    pic_DO13_1.Image = pictrue.Image;
            //    btDO13_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO13_1.Image = picfail.Image;
            //    btDO13_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[14] == true)
            //{
            //    pic_DO14_1.Image = pictrue.Image;
            //    btDO14_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO14_1.Image = picfail.Image;
            //    btDO14_1.Text = "ON";
            //}
            //if (adaM6256.DO_CH[15] == true)
            //{
            //    pic_DO15_1.Image = pictrue.Image;
            //    btDO15_1.Text = "OFF";
            //}
            //else
            //{
            //    pic_DO15_1.Image = picfail.Image;
            //    btDO15_1.Text = "ON";
            //}

            //pic_DO0_1.Refresh();
            //pic_DO1_1.Refresh();
            //pic_DO2_1.Refresh();
            //pic_DO3_1.Refresh();

            //pic_DO4_1.Refresh();
            //pic_DO5_1.Refresh();
            //pic_DO6_1.Refresh();
            //pic_DO7_1.Refresh();

            //pic_DO8_1.Refresh();
            //pic_DO9_1.Refresh();
            //pic_DO10_1.Refresh();
            //pic_DO11_1.Refresh();

            //pic_DO12_1.Refresh();
            //pic_DO13_1.Refresh();
            //pic_DO14_1.Refresh();
            //pic_DO15_1.Refresh();
            //#endregion




            //if (ParentForm != null)
            //{
            //    ParentForm.frmOperate.pictstateADAM.BackColor = Color.Green;
            //}
        }

        private void BtDO0_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(0);
        }

        private void BtDO1_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(1);
        }

        private void BtDO2_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(2);
        }

        private void BtDO3_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(3);
        }

        private void BtDO4_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(4);
        }

        private void BtDO5_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(5);
        }

        private void BtDO6_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(6);
        }

        private void BtDO7_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(7);
        }

        private void BtDO8_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(8);
        }

        private void BtDO9_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(9);
        }

        private void BtDO10_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(10);
        }

        private void BtDO11_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(11);
        }

        private void BtDO12_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(12);
        }

        private void BtDO13_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(13);
        }

        private void GroupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void BtDO14_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(14);
        }

        private void BtDO15_1_Click(object sender, EventArgs e)
        {
            UpdateChangeOutput(15);
        }


        public void UpdateChangeOutput(int input)
        {
            int CH = input;
            if (OutputCoils[CH] == false)
            {
                UpdateOutput(CH, true);
            }
            else
            {
                UpdateOutput(CH, false);
            }
        }


        public void UpdateOutput(int index, bool state)
        {

            Modbus64.Result Result;

            OutputCoils[index] = state;

            Result = modbus64Control2.WriteMultipleCoils(1, 17 - 1, 16, OutputCoils);
            if (Result != Modbus64.Result.SUCCESS)
            {
                // MessageBox.Show(modbus64Control2.GetLastErrorString());
            }
            else
            {


            }
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            if (cbCOMIAI.Text != "")
            {
                InitialModbusSerial(cbCOMIAI.Text);
            }
        }
        private void UpdateReceiveDataIAI(object sender)
        {
            if (this.txtReceiveData.InvokeRequired)
            {
                UpdateReceiveDataCallback method = new UpdateReceiveDataCallback(this.UpdateReceiveDataIAI);
                object[] args = new object[] { this };
                base.Invoke(method, args);
            }
            else
            {



                this.txtReceiveData.AppendText("Rx: ");
                string text = BitConverter.ToString(this.modbusIAI.receiveData).Replace("-", " ");

                if (text.Length > 5)
                {
                    string textID = text.Substring(0, 2);
                    string textCode = text.Substring(3, 2);

                    lbRxID.Text = textID;
                    lbRxFC.Text = textCode;
                }

                this.txtReceiveData.AppendText(text);
                this.txtReceiveData.AppendText(Environment.NewLine);
            }
        }

        private void UpdateSendDataIAI(object sender)
        {

            lbRxID.Text = "";
            lbRxFC.Text = "";

            this.txtReceiveData.AppendText("Tx: ");
            string text = BitConverter.ToString(this.modbusIAI.sendData).Replace("-", " ");


            if (text.Length > 5)
            {
                string textID = text.Substring(0, 2);
                string textCode = text.Substring(3, 2);

                if (textCode == "05")
                {


                }

                lbRxID.Text = textID;
                lbRxFC.Text = textCode;
            }

            this.txtReceiveData.AppendText(text);
            this.txtReceiveData.AppendText(Environment.NewLine);
        }

        public void InitialModbusSerial(string COM)
        {
            if (COM != "")
            {
                cbCOMIAI.Text = COM;

                modbusIAI = new ModbusClient(COM);

                int UnitIdentifier = 1;
                modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                modbusIAI.Baudrate = 115200;
                modbusIAI.DataBits = 8;

                modbusIAI.StopBits = System.IO.Ports.StopBits.One;

                modbusIAI.ConnectionTimeout = 50;

                modbusIAI.ModbusMode = 1;
                modbusIAI.Parity = System.IO.Ports.Parity.None;
                modbusIAI.Connect(modbusIAI.ModbusMode); // 1 : Serial port


                this.modbusIAI.receiveDataChanged += new ModbusClient.ReceiveDataChanged(this.UpdateReceiveDataIAI);
                this.modbusIAI.sendDataChanged += new ModbusClient.SendDataChanged(this.UpdateSendDataIAI);

                cbCOMIAI.Enabled = false;
                btConnect.Enabled = false;

                Servo_On(1);
                // Servo_On(2);

                timerState.Enabled = true;
            }
        }

        public void DisconnectModbusSerial()
        {
            cbCOMIAI.Enabled = false;
            btConnect.Enabled = false;

            timerState.Enabled = false;

            modbusIAI.Disconnect();
        }

        private void TimerState_Tick(object sender, EventArgs e)
        {
            if (ckGetStatus.Checked == false)
            {
                timerState.Enabled = true;
                return;
            }

            timerState.Enabled = false;

            if (this.modbusIAI == null)
            {
                return;
            }

            if (this.modbusIAI.Connected)
            {


                int UnitIdentifier = 1;

                if (LoopID == 1)
                {
                    if (ckID1.Checked == true)
                    {
                        UnitIdentifier = 1;
                        if (modbusIAI != null)
                        {
                            modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                        }

                        GetDataState(1);
                    }
                    else
                    {
                        //LoopID = 2; //Next ID
                        LoopID = 1; //Next ID
                    }
                }

                if (LoopID == 2)
                {
                    //if (ckID2.Checked == true)
                    //{
                    //    UnitIdentifier = 2;
                    //    if (modbusIAI != null)
                    //    {
                    //        modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                    //    }

                    //    GetDataState(2);
                    //}
                    //else
                    //{
                    //    LoopID = 3;//Next ID
                    //}
                }

                //if (LoopID == 3)
                //{
                //    if (ckID3.Checked == true)
                //    {
                //        UnitIdentifier = 3;
                //        if (modbusIAI != null)
                //        {
                //            modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                //        }

                //        GetDataState(3);//Next ID
                //    }
                //    else
                //    {
                //        LoopID = 4;
                //    }
                //}

                LoopID++;

                // if (LoopID > 3
                if (LoopID > 2)
                {
                    LoopID = 1;//Next ID
                }


                //UnitIdentifier = int.Parse(cbID.Text);
                //if (modbusClient != null)
                //{
                //    modbusClient.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                //}
                //GetDataState(int.Parse(cbID.Text));


            }

            timerState.Enabled = true;
        }

        private void ModbusConnection()
        {
            this.modbusIAI.Connect(1);// 1 : Serial port
        }

        public void GetDataState(int ID)
        {

            timerState.Enabled = false;

            try
            {
                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();
                }


                string StartingAddress = "9000";
                string NumberOfValues = "15";

                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);
                int intNumberOfValues = int.Parse(NumberOfValues, System.Globalization.NumberStyles.HexNumber);


                int[] numArray = this.modbusIAI.ReadHoldingRegisters(intStartingAddress, intNumberOfValues);



                if (numArray.Length >= 21)
                {

                    #region Current position monitor

                    /*
                    The current position is “00000BFEH” → Converted to a decimal value → 3070 (x 0.01 mm) The current position is 30.7 mm. 
                     * Example 2: If the current position is read “FFFFFFF5H” (negative position) → FFFFFFFFH – FFFFFFF5H + 1 (make sure to add 1) 
                     * → Convert to a decimal value → 11 (x 0.01 mm) The current position is -0.11 mm 
                    */

                    string Current_position_monitor_str = numArray[0].ToString("X4") + numArray[1].ToString("X4");
                    double Current_position = 0.0;

                    if (numArray[0] < 0)
                    {
                        int negative_position = (numArray[0] - numArray[1] + 1);
                        Current_position = Convert.ToDouble(negative_position) * (-0.01);
                    }
                    else
                    {
                        int intCurrent_position_monitor_str = int.Parse(Current_position_monitor_str, System.Globalization.NumberStyles.HexNumber);
                        Current_position = Convert.ToDouble(intCurrent_position_monitor_str) * 0.01;

                    }

                    if (ID == 1)
                    {
                        txtCurrentPosition.Text = Current_position.ToString("0.00");
                    }
                    //else if (ID == 2)
                    //{
                    //    txtCurrentPosition2.Text = Current_position.ToString("0.00");
                    //}
                    //else if (ID == 3)
                    //{
                    //    //txtCurrentPosition3.Text = Current_position.ToString("0.00");
                    //}

                    #endregion Current position monitor



                    string Present_alarm_code_query_str = numArray[2].ToString();


                    if (ID == 1)
                    {
                        txtAlarmCode.Text = Present_alarm_code_query_str;
                    }
                    //else if (ID == 2)
                    //{
                    //    txtAlarmCode2.Text = Present_alarm_code_query_str;
                    //}
                    //else if (ID == 3)
                    //{
                    //    //txtAlarmCode3.Text = Present_alarm_code_query_str;
                    //}

                    string Input_port_query_str = numArray[3].ToString(); //-> DIPM


                    if (numArray[3] < 0)
                    {

                        string s = Convert.ToString(numArray[3] & 2047, 2);
                        // int int_Input_port_query_str = int.Parse(Input_port_query_str, System.Globalization.NumberStyles.HexNumber);
                        numArray[3] = Math.Abs(numArray[3]);
                    }
                    else
                    {


                    }

                    #region Input_port_query_str
                    //Bit
                    //15------0   
                    string Binary1 = Convert.ToString(numArray[3], 2);
                    if (Binary1.Length != 16)
                    {
                        int AddZero = 16 - Binary1.Length;
                        for (int j = 0; j < AddZero; j++)
                        {
                            Binary1 = "0" + Binary1;
                        }
                    }

                    if (Binary1.Length == 16)
                    {

                        txtBinary0.Text = Binary1.ToString();
                        string retBuf = Binary1;





                        if (ID == 1)
                        {
                            if (retBuf.Substring(15, 1) == "1")//PC1
                            {
                                picDIPM01_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM01_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(14, 1) == "1")//PC2
                            {
                                picDIPM02_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM02_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(13, 1) == "1")//PC4
                            {
                                picDIPM03_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM03_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(12, 1) == "1")//PC8
                            {
                                picDIPM04_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM04_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(11, 1) == "1")//PC16
                            {
                                picDIPM05_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM05_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(10, 1) == "1")//PC32
                            {
                                picDIPM06_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM06_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(9, 1) == "1")//0
                            {
                                picDIPM07_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM07_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(8, 1) == "1")//0
                            {
                                picDIPM08_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM08_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(7, 1) == "1")//0
                            {
                                picDIPM09_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM09_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(6, 1) == "1")//BKRL
                            {
                                picDIPM10_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM10_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(5, 1) == "1")//RMOD
                            {
                                picDIPM11_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM11_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(4, 1) == "1")//HOME
                            {
                                picDIPM12_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM12_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(3, 1) == "1")//*STP
                            {
                                picDIPM13_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM13_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(2, 1) == "1")//CSTR
                            {
                                picDIPM14_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM14_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(1, 1) == "1") //RES
                            {
                                picDIPM15_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM15_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(0, 1) == "1") //SON
                            {
                                picDIPM16_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDIPM16_ID1.BackColor = Color.Black;
                            }

                        }
                        //else if (ID == 2)
                        //{
                        //    if (retBuf.Substring(15, 1) == "1")//PC1
                        //    {
                        //        picDIPM01_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM01_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(14, 1) == "1")//PC2
                        //    {
                        //        picDIPM02_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM02_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(13, 1) == "1")//PC4
                        //    {
                        //        picDIPM03_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM03_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(12, 1) == "1")//PC8
                        //    {
                        //        picDIPM04_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM04_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(11, 1) == "1")//PC16
                        //    {
                        //        picDIPM05_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM05_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(10, 1) == "1")//PC32
                        //    {
                        //        picDIPM06_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM06_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(9, 1) == "1")//0
                        //    {
                        //        picDIPM07_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM07_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(8, 1) == "1")//0
                        //    {
                        //        picDIPM08_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM08_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(7, 1) == "1")//0
                        //    {
                        //        picDIPM09_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM09_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(6, 1) == "1")//BKRL
                        //    {
                        //        picDIPM10_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM10_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(5, 1) == "1")//RMOD
                        //    {
                        //        picDIPM11_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM11_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(4, 1) == "1")//HOME
                        //    {
                        //        picDIPM12_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM12_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(3, 1) == "1")//*STP
                        //    {
                        //        picDIPM13_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM13_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(2, 1) == "1")//CSTR
                        //    {
                        //        picDIPM14_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM14_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(1, 1) == "1") //RES
                        //    {
                        //        picDIPM15_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM15_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(0, 1) == "1") //SON
                        //    {
                        //        picDIPM16_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDIPM16_ID2.BackColor = Color.Black;
                        //    }

                        //}
                        //else if (ID == 3)
                        //{
                        //    //if (retBuf.Substring(15, 1) == "1")//PC1
                        //    //{
                        //    //    picDIPM01_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM01_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(14, 1) == "1")//PC2
                        //    //{
                        //    //    picDIPM02_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM02_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(13, 1) == "1")//PC4
                        //    //{
                        //    //    picDIPM03_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM03_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(12, 1) == "1")//PC8
                        //    //{
                        //    //    picDIPM04_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM04_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(11, 1) == "1")//PC16
                        //    //{
                        //    //    picDIPM05_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM05_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(10, 1) == "1")//PC32
                        //    //{
                        //    //    picDIPM06_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM06_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(9, 1) == "1")//0
                        //    //{
                        //    //    picDIPM07_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM07_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(8, 1) == "1")//0
                        //    //{
                        //    //    picDIPM08_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM08_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(7, 1) == "1")//0
                        //    //{
                        //    //    picDIPM09_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM09_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(6, 1) == "1")//BKRL
                        //    //{
                        //    //    picDIPM10_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM10_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(5, 1) == "1")//RMOD
                        //    //{
                        //    //    picDIPM11_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM11_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(4, 1) == "1")//HOME
                        //    //{
                        //    //    picDIPM12_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM12_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(3, 1) == "1")//*STP
                        //    //{
                        //    //    picDIPM13_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM13_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(2, 1) == "1")//CSTR
                        //    //{
                        //    //    picDIPM14_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM14_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(1, 1) == "1") //RES
                        //    //{
                        //    //    picDIPM15_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM15_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(0, 1) == "1") //SON
                        //    //{
                        //    //    picDIPM16_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDIPM16_ID3.BackColor = Color.Black;
                        //    //}

                        //}



                    }
                    #endregion Output_port_monitor_query_str

                    string Output_port_monitor_query_str = numArray[4].ToString(); //->DOPM
                    #region Output_port_monitor_query_str
                    //Bit
                    //15------0   
                    Binary1 = Convert.ToString(numArray[4], 2);
                    if (Binary1.Length != 16)
                    {
                        int AddZero = 16 - Binary1.Length;
                        for (int j = 0; j < AddZero; j++)
                        {
                            Binary1 = "0" + Binary1;
                        }
                    }

                    if (Binary1.Length == 16)
                    {

                        txtBinary2.Text = Binary1.ToString();
                        string retBuf = Binary1;


                        if (ID == 1)
                        {

                            if (retBuf.Substring(15, 1) == "1")//PM1
                            {
                                picDOPM01_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM01_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(14, 1) == "1")//PM2
                            {
                                picDOPM02_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM02_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(13, 1) == "1")//PM4
                            {
                                picDOPM03_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM03_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(12, 1) == "1")//PM8
                            {
                                picDOPM04_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM04_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(11, 1) == "1")//PM16
                            {
                                picDOPM05_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM05_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(10, 1) == "1")//PM32
                            {
                                picDOPM06_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM06_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(9, 1) == "1")//MOVE
                            {
                                picDOPM07_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM07_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(8, 1) == "1")//ZONE1
                            {
                                picDOPM08_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM08_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(7, 1) == "1")//PZONE
                            {
                                picDOPM09_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM09_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(6, 1) == "1")//RMDS
                            {
                                picDOPM10_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM10_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(5, 1) == "1")//HEND
                            {
                                picDOPM11_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM11_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(4, 1) == "1")//PEND
                            {
                                picDOPM12_ID1.BackColor = Color.Red;
                                picDOPM12_2_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM12_ID1.BackColor = Color.Black;
                                picDOPM12_2_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(3, 1) == "1")//SV
                            {
                                picDOPM13_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM13_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(2, 1) == "1")//*EMGS
                            {
                                picDOPM14_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM14_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(1, 1) == "1") //*ALM
                            {
                                picDOPM15_ID1.BackColor = Color.Red;
                                picDOPM15_2_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM15_ID1.BackColor = Color.Black;
                                picDOPM15_2_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(0, 1) == "1") // 0
                            {
                                picDOPM16_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDOPM16_ID1.BackColor = Color.Black;
                            }
                        }
                        //else if (ID == 2)
                        //{

                        //    if (retBuf.Substring(15, 1) == "1")//PM1
                        //    {
                        //        picDOPM01_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM01_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(14, 1) == "1")//PM2
                        //    {
                        //        picDOPM02_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM02_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(13, 1) == "1")//PM4
                        //    {
                        //        picDOPM03_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM03_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(12, 1) == "1")//PM8
                        //    {
                        //        picDOPM04_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM04_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(11, 1) == "1")//PM16
                        //    {
                        //        picDOPM05_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM05_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(10, 1) == "1")//PM32
                        //    {
                        //        picDOPM06_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM06_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(9, 1) == "1")//MOVE
                        //    {
                        //        picDOPM07_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM07_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(8, 1) == "1")//ZONE1
                        //    {
                        //        picDOPM08_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM08_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(7, 1) == "1")//PZONE
                        //    {
                        //        picDOPM09_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM09_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(6, 1) == "1")//RMDS
                        //    {
                        //        picDOPM10_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM10_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(5, 1) == "1")//HEND
                        //    {
                        //        picDOPM11_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM11_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(4, 1) == "1")//PEND
                        //    {
                        //        picDOPM12_ID2.BackColor = Color.Red;
                        //        picDOPM12_2_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM12_ID2.BackColor = Color.Black;
                        //        picDOPM12_2_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(3, 1) == "1")//SV
                        //    {
                        //        picDOPM13_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM13_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(2, 1) == "1")//*EMGS
                        //    {
                        //        picDOPM14_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM14_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(1, 1) == "1") //*ALM
                        //    {
                        //        picDOPM15_ID2.BackColor = Color.Red;
                        //        picDOPM15_2_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM15_ID2.BackColor = Color.Black;
                        //        picDOPM15_2_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(0, 1) == "1") // 0
                        //    {
                        //        picDOPM16_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDOPM16_ID2.BackColor = Color.Black;
                        //    }
                        //}
                        //else if (ID == 3)
                        //{

                        //    //if (retBuf.Substring(15, 1) == "1")//PM1
                        //    //{
                        //    //    picDOPM01_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM01_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(14, 1) == "1")//PM2
                        //    //{
                        //    //    picDOPM02_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM02_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(13, 1) == "1")//PM4
                        //    //{
                        //    //    picDOPM03_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM03_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(12, 1) == "1")//PM8
                        //    //{
                        //    //    picDOPM04_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM04_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(11, 1) == "1")//PM16
                        //    //{
                        //    //    picDOPM05_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM05_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(10, 1) == "1")//PM32
                        //    //{
                        //    //    picDOPM06_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM06_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(9, 1) == "1")//MOVE
                        //    //{
                        //    //    picDOPM07_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM07_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(8, 1) == "1")//ZONE1
                        //    //{
                        //    //    picDOPM08_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM08_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(7, 1) == "1")//PZONE
                        //    //{
                        //    //    picDOPM09_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM09_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(6, 1) == "1")//RMDS
                        //    //{
                        //    //    picDOPM10_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM10_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(5, 1) == "1")//HEND
                        //    //{
                        //    //    picDOPM11_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM11_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(4, 1) == "1")//PEND
                        //    //{
                        //    //    picDOPM12_ID3.BackColor = Color.Red;
                        //    //    picDOPM12_2_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM12_ID3.BackColor = Color.Black;
                        //    //    picDOPM12_2_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(3, 1) == "1")//SV
                        //    //{
                        //    //    picDOPM13_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM13_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(2, 1) == "1")//*EMGS
                        //    //{
                        //    //    picDOPM14_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM14_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(1, 1) == "1") //*ALM
                        //    //{
                        //    //    picDOPM15_ID3.BackColor = Color.Red;
                        //    //    picDOPM15_2_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM15_ID3.BackColor = Color.Black;
                        //    //    picDOPM15_2_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(0, 1) == "1") // 0
                        //    //{
                        //    //    picDOPM16_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDOPM16_ID3.BackColor = Color.Black;
                        //    //}
                        //}



                    }
                    #endregion Output_port_monitor_query_str

                    string Device_status_query_1_str = numArray[5].ToString();// DSS1
                    #region Device_status_query_1

                    Binary1 = Convert.ToString(numArray[5], 2);
                    if (Binary1.Length != 16)
                    {
                        int AddZero = 16 - Binary1.Length;
                        for (int j = 0; j < AddZero; j++)
                        {
                            Binary1 = "0" + Binary1;
                        }
                    }

                    if (Binary1.Length == 16)
                    {

                        txtBinary1.Text = Binary1.ToString();
                        string retBuf = Binary1;


                        if (ID == 1)
                        {
                            if (retBuf.Substring(0, 1) == "1")//[Bit 15] Emergency stop
                            {
                                picDSS1_0_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_0_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(1, 1) == "1")// [Bit 14] Safety speed enabled/disabled 
                            {
                                picDSS1_1_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_1_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(2, 1) == "1")//[Bit 13] Controller ready 
                            {
                                picDSS1_2_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_2_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(3, 1) == "1")// [Bit 12] Servo ON/OFF
                            {
                                picDSS1_3_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_3_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(4, 1) == "1")//[Bit 11] Missed work in push-motion operation
                            {
                                picDSS1_4_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_4_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(5, 1) == "1")//[Bit 10] Major failure 
                            {
                                picDSS1_5_ID1.BackColor = Color.Red;
                                picDSS1_5_2_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_5_ID1.BackColor = Color.Black;
                                picDSS1_5_2_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(6, 1) == "1")//[Bit 9] Minor failure 
                            {
                                picDSS1_6_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_6_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(7, 1) == "1")//[Bit 8] Absolute error 
                            {
                                picDSS1_7_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_7_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(8, 1) == "1")//[Bit 7] Brake 
                            {
                                picDSS1_8_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_8_ID1.BackColor = Color.Black;
                            }

                            //if (retBuf.Substring(9, 1) == "1")//[Bit 6] not use 

                            if (retBuf.Substring(10, 1) == "1")//[Bit 5] Pause 
                            {
                                picDSS1_9_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_9_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(11, 1) == "1")//[Bit 4] Home return completion 
                            {
                                picDSS1_10_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_10_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(12, 1) == "1")//[Bit 3] Position complete
                            {
                                picDSS1_11_ID1.BackColor = Color.Red;
                                picDSS1_11_2_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picDSS1_11_ID1.BackColor = Color.Black;
                                picDSS1_11_2_ID1.BackColor = Color.Black;
                            }
                            //if (retBuf.Substring(13, 1) == "1")//[Bit 2] not use
                            //if (retBuf.Substring(14, 1) == "1")//[Bit 1] not use
                            //if (retBuf.Substring(15, 1) == "1")//[Bit 0] not use
                        }
                        //else if (ID == 2)
                        //{
                        //    if (retBuf.Substring(0, 1) == "1")//[Bit 15] Emergency stop
                        //    {
                        //        picDSS1_0_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_0_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(1, 1) == "1")// [Bit 14] Safety speed enabled/disabled 
                        //    {
                        //        picDSS1_1_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_1_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(2, 1) == "1")//[Bit 13] Controller ready 
                        //    {
                        //        picDSS1_2_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_2_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(3, 1) == "1")// [Bit 12] Servo ON/OFF
                        //    {
                        //        picDSS1_3_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_3_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(4, 1) == "1")//[Bit 11] Missed work in push-motion operation
                        //    {
                        //        picDSS1_4_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_4_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(5, 1) == "1")//[Bit 10] Major failure 
                        //    {
                        //        picDSS1_5_ID2.BackColor = Color.Red;
                        //        picDSS1_5_2_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_5_ID2.BackColor = Color.Black;
                        //        picDSS1_5_2_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(6, 1) == "1")//[Bit 9] Minor failure 
                        //    {
                        //        picDSS1_6_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_6_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(7, 1) == "1")//[Bit 8] Absolute error 
                        //    {
                        //        picDSS1_7_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_7_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(8, 1) == "1")//[Bit 7] Brake 
                        //    {
                        //        picDSS1_8_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_8_ID2.BackColor = Color.Black;
                        //    }

                        //    //if (retBuf.Substring(9, 1) == "1")//[Bit 6] not use 

                        //    if (retBuf.Substring(10, 1) == "1")//[Bit 5] Pause 
                        //    {
                        //        picDSS1_9_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_9_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(11, 1) == "1")//[Bit 4] Home return completion 
                        //    {
                        //        picDSS1_10_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_10_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(12, 1) == "1")//[Bit 3] Position complete
                        //    {
                        //        picDSS1_11_ID2.BackColor = Color.Red;
                        //        picDSS1_11_2_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picDSS1_11_ID2.BackColor = Color.Black;
                        //        picDSS1_11_2_ID2.BackColor = Color.Black;
                        //    }
                        //    //if (retBuf.Substring(13, 1) == "1")//[Bit 2] not use
                        //    //if (retBuf.Substring(14, 1) == "1")//[Bit 1] not use
                        //    //if (retBuf.Substring(15, 1) == "1")//[Bit 0] not use
                        //}
                        //else if (ID == 3)
                        //{
                        //    //if (retBuf.Substring(0, 1) == "1")//[Bit 15] Emergency stop
                        //    //{
                        //    //    picDSS1_0_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_0_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(1, 1) == "1")// [Bit 14] Safety speed enabled/disabled 
                        //    //{
                        //    //    picDSS1_1_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_1_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(2, 1) == "1")//[Bit 13] Controller ready 
                        //    //{
                        //    //    picDSS1_2_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_2_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(3, 1) == "1")// [Bit 12] Servo ON/OFF
                        //    //{
                        //    //    picDSS1_3_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_3_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(4, 1) == "1")//[Bit 11] Missed work in push-motion operation
                        //    //{
                        //    //    picDSS1_4_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_4_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(5, 1) == "1")//[Bit 10] Major failure 
                        //    //{
                        //    //    picDSS1_5_ID3.BackColor = Color.Red;
                        //    //    picDSS1_5_2_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_5_ID3.BackColor = Color.Black;
                        //    //    picDSS1_5_2_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(6, 1) == "1")//[Bit 9] Minor failure 
                        //    //{
                        //    //    picDSS1_6_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_6_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(7, 1) == "1")//[Bit 8] Absolute error 
                        //    //{
                        //    //    picDSS1_7_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_7_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(8, 1) == "1")//[Bit 7] Brake 
                        //    //{
                        //    //    picDSS1_8_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_8_ID3.BackColor = Color.Black;
                        //    //}

                        //    ////if (retBuf.Substring(9, 1) == "1")//[Bit 6] not use 

                        //    //if (retBuf.Substring(10, 1) == "1")//[Bit 5] Pause 
                        //    //{
                        //    //    picDSS1_9_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_9_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(11, 1) == "1")//[Bit 4] Home return completion 
                        //    //{
                        //    //    picDSS1_10_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_10_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(12, 1) == "1")//[Bit 3] Position complete
                        //    //{
                        //    //    picDSS1_11_ID3.BackColor = Color.Red;
                        //    //    picDSS1_11_2_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picDSS1_11_ID3.BackColor = Color.Black;
                        //    //    picDSS1_11_2_ID3.BackColor = Color.Black;
                        //    //}
                        //    //if (retBuf.Substring(13, 1) == "1")//[Bit 2] not use
                        //    //if (retBuf.Substring(14, 1) == "1")//[Bit 1] not use
                        //    //if (retBuf.Substring(15, 1) == "1")//[Bit 0] not use
                        //}




                    }
                    #endregion Device_status_query_1

                    string Device_status_query_2_str = numArray[6].ToString();
                    string Expansion_device_status_query_str = numArray[7].ToString();

                    string System_status_query_str = numArray[8].ToString("X4") + numArray[9].ToString("X4");
                    // int int_System_status_query_str = hexToBinary(System_status_query_str); // int.Parse(System_status_query_str, System.Globalization.NumberStyles.HexNumber);

                    string HEX_System_status_query_str = hexToBinary(System_status_query_str);
                    #region System_status_query
                    //Bit
                    //32------0   
                    Binary1 = HEX_System_status_query_str;//Convert.ToString(int_System_status_query_str, 2);

                    if (Binary1.Length != 48)
                    {
                        int AddZero = 16 - Binary1.Length;
                        for (int j = 0; j < AddZero; j++)
                        {
                            Binary1 = "0" + Binary1;
                        }
                    }


                    if (Binary1.Length == 48)
                    {

                        txtBinary3.Text = Binary1.ToString();
                        string retBuf = Binary1.Substring(40, 8);



                        if (ID == 1)
                        {
                            if (retBuf.Substring(3, 1) == "1")//Operation mode status 
                            {
                                picOperationMode_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picOperationMode_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(4, 1) == "1")//HEND Home return completion status 
                            {
                                picHomeComplete_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picHomeComplete_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(5, 1) == "1")// SV Servo status 
                            {
                                picServoState_ID1.BackColor = Color.Red;
                            }
                            else
                            {
                                picServoState_ID1.BackColor = Color.Black;
                            }

                            if (retBuf.Substring(6, 1) == "1")// SON Servo command status 
                            {
                                // picDSS1_1.BackColor = Color.Red;
                            }
                            else
                            {
                                // picDSS1_1.BackColor = Color.Black;
                            }
                            if (retBuf.Substring(7, 1) == "1")// MPOW Drive source ON
                            {
                                picMPOW_ID1.BackColor = Color.Red;

                                if (ParentForm != null)
                                {
                                     ParentForm.picIAI.Image = ParentForm.pictrue.Image;
                                }
                            }
                            else
                            {
                                picMPOW_ID1.BackColor = Color.Black;
                                if (ParentForm != null)
                                {
                                     ParentForm.picIAI.Image = ParentForm.pictrue.Image;
                                }
                            }
                        }
                        //else if (ID == 2)
                        //{
                        //    if (retBuf.Substring(3, 1) == "1")//Operation mode status 
                        //    {
                        //        picOperationMode_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picOperationMode_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(4, 1) == "1")//HEND Home return completion status 
                        //    {
                        //        picHomeComplete_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picHomeComplete_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(5, 1) == "1")// SV Servo status 
                        //    {
                        //        picServoState_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picServoState_ID2.BackColor = Color.Black;
                        //    }

                        //    if (retBuf.Substring(6, 1) == "1")// SON Servo command status 
                        //    {
                        //        // picDSS1_1.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        // picDSS1_1.BackColor = Color.Black;
                        //    }
                        //    if (retBuf.Substring(7, 1) == "1")// MPOW Drive source ON
                        //    {
                        //        picMPOW_ID2.BackColor = Color.Red;
                        //    }
                        //    else
                        //    {
                        //        picMPOW_ID2.BackColor = Color.Black;
                        //    }
                        //}
                        //else if (ID == 3)
                        //{
                        //    //if (retBuf.Substring(3, 1) == "1")//Operation mode status 
                        //    //{
                        //    //    picOperationMode_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picOperationMode_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(4, 1) == "1")//HEND Home return completion status 
                        //    //{
                        //    //    picHomeComplete_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picHomeComplete_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(5, 1) == "1")// SV Servo status 
                        //    //{
                        //    //    picServoState_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picServoState_ID3.BackColor = Color.Black;
                        //    //}

                        //    //if (retBuf.Substring(6, 1) == "1")// SON Servo command status 
                        //    //{
                        //    //    // picDSS1_1.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    // picDSS1_1.BackColor = Color.Black;
                        //    //}
                        //    //if (retBuf.Substring(7, 1) == "1")// MPOW Drive source ON
                        //    //{
                        //    //    picMPOW_ID3.BackColor = Color.Red;
                        //    //}
                        //    //else
                        //    //{
                        //    //    picMPOW_ID3.BackColor = Color.Black;
                        //    //}
                        //}



                    }
                    #endregion System_status_query
                    //        int int_System_status_query = int.Parse(System_status_query_str, System.Globalization.NumberStyles.HexNumber);

                    // [1] Automatic servo OFF [2] EEPROM accessed  [3] Operation mode (AUTO/MANU) [4] Home return completion  [5] Servo ON/OFF [6] Servo command  [7] Drive source ON (normal/cut off) 
                    //  int HexNumber = int.Parse(Device_status_query_1_str, System.Globalization.NumberStyles.HexNumber);





                    string Current_speed_monito_str = numArray[10].ToString() + numArray[11].ToString();

                    #region current ampere monitor


                    string current_ampere_monitor_str = numArray[12].ToString("X4") + numArray[13].ToString("X4");
                    double current_ampere = 0.0;


                    int int_current_ampere_monitor = int.Parse(current_ampere_monitor_str, System.Globalization.NumberStyles.HexNumber);
                    current_ampere = Convert.ToDouble(int_current_ampere_monitor);



                    if (ID == 1)
                    {
                        txtCurrentAmp.Text = current_ampere.ToString("0"); // mA
                    }
                    //else if (ID == 2)
                    //{
                    //    txtCurrentAmp2.Text = current_ampere.ToString("0"); // mA
                    //}
                    //else if (ID == 3)
                    //{
                    //   // txtCurrentAmp3.Text = current_ampere.ToString("0"); // mA
                    //}



                    #endregion current ampere monitor


                    string Deviation_monitor_str = numArray[14].ToString() + numArray[15].ToString();
                    string System_timer_query_str = numArray[16].ToString() + numArray[17].ToString();

                    string Special_input_port_query_str = numArray[18].ToString();
                    string Zone_status_query_str = numArray[19].ToString();
                    string Position_complete_number_status_query = numArray[20].ToString();


                    //  txtPositionComplete.Text = Position_complete_number_status_query;

                    if (ID == 1)
                    {
                        picConnectID1.BackColor = Color.Red;

                    }
                    else if (ID == 2)
                    {
                      //  picConnectID2.BackColor = Color.Red;
                    }
                    else if (ID == 3)
                    {
                        //picConnectID3.BackColor = Color.Red;
                    }
                }


            }
            catch (Exception exception)
            {

                if (ID == 1)
                {
                    picConnectID1.BackColor = Color.Black;

                }
                else if (ID == 2)
                {
                  //  picConnectID2.BackColor = Color.Black;
                }
                else if (ID == 3)
                {
                    //picConnectID3.BackColor = Color.Black;
                }

            }

            timerState.Enabled = true;
        }


        public string hexToBinary(string hex)
        {
            string result = "";
            string[] starr = new string[hex.Length];
            hex = hex.ToUpper();

            for (int i = 0; i < hex.Length; i++)
            {
                starr[i] = hex.ToCharArray()[i].ToString();
            }

            for (int i = 0; i < starr.Length; i++)
            {
                switch (starr[i])
                {
                    case "0":
                        result += "0000";
                        break;
                    case "1":
                        result += "0001";
                        break;
                    case "2":

                        result += "0010";
                        break;
                    case "3":

                        result += "0011";
                        break;
                    case "4":

                        result += "0100";
                        break;
                    case "5":

                        result += "0101";
                        break;

                    case "6":
                        result += "0110";
                        break;

                    case "7":
                        result += "0111";
                        break;
                    case "8":
                        result += "1000";
                        break;
                    case "9":
                        result += "1001";
                        break;
                    case "A":
                        result += "1010";
                        break;
                    case "B":
                        result += "1011";
                        break;
                    case "C":
                        result += "1100";
                        break;
                    case "D":
                        result += "1101";
                        break;
                    case "E":
                        result += "1110";
                        break;
                    case "F":
                        result += "1111";
                        break;
                }
            }
            return result;
        }

        private void BtHome_MouseDown(object sender, MouseEventArgs e)
        {
            Home_MouseDown(1);
        }

        private void BtHome_MouseUp(object sender, MouseEventArgs e)
        {
            Home_MouseUp(1);
        }

        private void BtJogFW_MouseDown(object sender, MouseEventArgs e)
        {
            JogFW_MouseDown(1);
        }

        private void BtJogFW_MouseUp(object sender, MouseEventArgs e)
        {
            JogFW_MouseUp(1);
        }

        private void BtJogBW_MouseDown(object sender, MouseEventArgs e)
        {
            JogBW_MouseDown(1);
        }

        private void BtJogBW_MouseUp(object sender, MouseEventArgs e)
        {
            JogBW_MouseUp(1);
        }

        private void BtServoStop_Click(object sender, EventArgs e)
        {
            Servo_Stop(1);
        }

        public void Servo_Stop(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "042C";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);


            }
            catch (Exception exception)
            {
                //   MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void Home_MouseDown(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "040B";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);


            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void Home_MouseUp(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "040B";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, false);


            }
            catch (Exception exception)
            {
                //  MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void JogFW_MouseDown(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0416";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);

            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void JogFW_MouseUp(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0416";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, false);

            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void JogBW_MouseDown(int UnitID)
        {

            timerState.Enabled = false;
            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0417";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);
                //  Thread.Sleep(500);
                //  this.modbusIAI.WriteSingleCoil(intStartingAddress, false);

            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }
        public void JogBW_MouseUp(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0417";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, false);
                //  Thread.Sleep(500);
                //  this.modbusIAI.WriteSingleCoil(intStartingAddress, false);

            }
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        private void BtMoveTG_Click(object sender, EventArgs e)
        {
            MoveTG(txtTragetPos.Text, txtTragetSpeed.Text, txtTragetAcc.Text, ckIncremental.Checked, 1);
        }
        public void MoveTG(string Position, string Speed, string Acc, bool mode, int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "9900";
                int[] numArray = new int[9];


                //Taget Position 2 byte mm

                double input_int = double.Parse(Position) * 100; //0.01mm

                Int64 input_int64 = Convert.ToInt64(input_int);

                string Hexinput_int = input_int64.ToString("X8");

                if (Hexinput_int.Length == 8)
                {

                    //txtBinary1.Text = Binary1.ToString();
                    string retBuf = Hexinput_int;
                    string H_HEX = retBuf.Substring(0, 4);
                    string L_HEX = retBuf.Substring(4, 4);

                    int int_H_HEX = int.Parse(H_HEX, System.Globalization.NumberStyles.HexNumber);
                    int int_L_HEX = int.Parse(L_HEX, System.Globalization.NumberStyles.HexNumber);

                    numArray[0] = int_H_HEX;
                    numArray[1] = int_L_HEX; //50 mm
                }
                else
                {
                    if (Hexinput_int.Length == 16) //negative -
                    {

                        //txtBinary1.Text = Binary1.ToString();
                        string retBuf = Hexinput_int;
                        string H_HEX = retBuf.Substring(0, 8);
                        string L_HEX = retBuf.Substring(8, 8);

                        int int_H_HEX = int.Parse(H_HEX, System.Globalization.NumberStyles.HexNumber);
                        int int_L_HEX = int.Parse(L_HEX, System.Globalization.NumberStyles.HexNumber);

                        numArray[0] = int_H_HEX;
                        numArray[1] = int_L_HEX; //50 mm
                    }
                }



                //(positioning band 2 byte mm
                numArray[2] = 0;
                numArray[3] = 10;  // (50mmx100) =5000



                double Speed_int = double.Parse(Speed) * 100; //0.01mm
                Int64 Speed_int64 = Convert.ToInt64(Speed_int);
                string HexSpeed_int = Speed_int64.ToString("X4");
                int int_Speed_HEX = int.Parse(HexSpeed_int, System.Globalization.NumberStyles.HexNumber);

                numArray[4] = 0;
                numArray[5] = int_Speed_HEX;// 10000;  //0.1 (mm) x 100 = 10 → 000AH


                double ACC_int = double.Parse(Acc) * 100; //0.01mm
                Int64 ACC_int64 = Convert.ToInt64(ACC_int);
                string Hex_ACC_int = ACC_int64.ToString("X4");
                int int_ACC_HEX = int.Parse(Hex_ACC_int, System.Globalization.NumberStyles.HexNumber);
                //acceleration/deceleration G  ->0.3
                numArray[6] = int_ACC_HEX;//30;     //0.3 (G) x 100 = 30 → 001EH 

                //push current limit Input unit (%) 0000H 
                numArray[7] = 0;

                //control flag normal operation is 0000H 

                if (mode == false)
                {
                    numArray[8] = 0;
                }
                else
                {

                    numArray[8] = 8;
                }


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);
                this.modbusIAI.WriteMultipleRegisters(intStartingAddress, numArray);




            }
            catch (Exception exception)
            {
                //  MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        private void BtAlarmReset_Click(object sender, EventArgs e)
        {

        }

        private void BtAlarmReset_MouseDown(object sender, MouseEventArgs e)
        {
            AlarmReset_MouseDown(1);
        }

        private void BtAlarmReset_MouseUp(object sender, MouseEventArgs e)
        {
            AlarmReset_MouseUp(1);
        }
        public void AlarmReset_MouseDown(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0407";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);


            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        public void AlarmReset_MouseUp(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0407";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, false);


            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        private void BtServoOn_Click(object sender, EventArgs e)
        {
            Servo_On(1);
        }

        private void BtServoOff_Click(object sender, EventArgs e)
        {
            Servo_Off(1);
        }


        public void Servo_On(int UnitID)
        {

            timerState.Enabled = false;
            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0403";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, true);




            }
            catch (Exception exception)
            {
                //  MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        public void Servo_Off(int UnitID)
        {

            timerState.Enabled = false;

            try
            {

                int UnitIdentifier = UnitID;

                if (modbusIAI != null)
                {
                    modbusIAI.UnitIdentifier = Convert.ToByte(UnitIdentifier);
                }

                if (!this.modbusIAI.Connected)
                {

                    ModbusConnection();

                }


                string StartingAddress = "0403";


                int intStartingAddress = int.Parse(StartingAddress, System.Globalization.NumberStyles.HexNumber);


                this.modbusIAI.WriteSingleCoil(intStartingAddress, false);




            }
            catch (Exception exception)
            {
                // MessageBox.Show(exception.Message, "Exception Write values to Server", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

            timerState.Enabled = true;
        }

        private void BtDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectModbusSerial();
        }

        private void TimerDelay_Tick(object sender, EventArgs e)
        {
            if ((connectADAM1 == true) && (connectADAM2 = true))
            {
                if (ParentForm != null)
                {
                    ParentForm.picDigitalIO.Image = ParentForm.pictrue.Image;
                }
            }
            else
            {
                if (ParentForm != null)
                {
                    ParentForm.picDigitalIO.Image = ParentForm.picfail.Image;
                }
            }

            timerDelay.Enabled = false;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {



            if (checkEP3000.Checked == true)
            {
                SendByteArray(start_decoding);
            }


            if (Statebarcode == true)
            {
                if (ParentForm != null)
                {
                    ParentForm.picBarcode.Image = ParentForm.pictrue.Image;
                }


            }
            else
            {

                if (ParentForm != null)
                {
                    ParentForm.picBarcode.Image = ParentForm.picfail.Image;
                }

            }


         }

         public void SendByteArray(byte[] data)
        {
            if (!Serialport.IsOpen) return;
            Serialport.Write(data, 0, data.Length);
        }

       

        public void ADDBarcodeBOXID(string BarcodeDetail)
        {
            if (checkEP3000.Checked == true)
            {
               

                int BarcodeLengthck = int.Parse(txtBarcodeLength.Text);

                string[] RetArrayck = BarcodeDetail.Split('\\');
                if (RetArrayck.Length > 0)
                {
                    BarcodeDetail = RetArrayck[0];
                }

                if (BarcodeDetail.Length >= 1)
                {

                    var items = BarCodelistView.Items;

                    items.Add(BarcodeDetail);

                    if (items.Count > 0)
                    {

                        for (int i = 0; i < items.Count; i++)
                        {
                            string itemcodeselect = items[i].Text;

                        }
                    }


                    if (items.Count > 3)
                    {
                        BarCodelistView.Clear();
                    }
                }

                return;
            }



            int BarcodeLength = int.Parse(txtBarcodeLength.Text);

            string[] RetArray = BarcodeDetail.Split('\\');


            if (RetArray.Length > 0)
            {
                BarcodeDetail = RetArray[0];
            }


            BarcodeDetail.Replace("1U", "");


            if (BarcodeDetail.Length == BarcodeLength)  //13
            {
                ParentForm.txtproduct_qrcode_find.Text = "";
                var items = BarCodelistView.Items;

                items.Add(BarcodeDetail);


                if (ParentForm != null)//check barcode state.....
                {

                    if (ParentForm.ckManualMode.Checked == true) //Auto mode
                    {

                        if (ParentForm.frmDigitalIO.BoxReady == true)
                        {
                            if (ParentForm.frmDigitalIO.PcsConvRunning == false)
                            {
                            
                                    ParentForm.txtproduct_qrcode_find.Text = BarcodeDetail.Trim();
                            }
                        }
                    }
                }



               txtproduct_qrcode_find.Text = BarcodeDetail.Trim();



                if (items.Count > 0)
                {

                    for (int i = 0; i < items.Count; i++)
                    {

                        string itemcodeselect = items[i].Text;

                    }

                }

                if (items.Count >= 10)
                {
                    BarCodelistView.Clear();
                }



            }
            else
            {
                ParentForm.txtproduct_qrcode_find.Text = "";
            }


          
        }

        private void Btbarcodeconnect1_Click(object sender, EventArgs e)
        {
            InitialSerialPort(cbSerial.Text);
        }

      


        private void CkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            if(ckTopMost.Checked == true)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

       

        private void FrmIO_FormClosing(object sender, FormClosingEventArgs e)
        {


            if (_readThreadBarcode != null)
            {
                _readThreadBarcode.Abort();
            }

            if (threadIO != null)
            {
                threadIO.Abort();
            }


            if (Serialport.IsOpen)
            {
                Serialport.Close();
            }

            
            modbus64Control1.Close();
            modbus64Control2.Close();


        }


        ////////////////////////////END//////////////////////////
    }
}
