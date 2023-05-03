using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Math;

using Emgu.CV;
using Emgu.CV.CvEnum; //usual Emgu Cv imports
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Dnn;
using Emgu.CV.Cuda;

using Basler.Pylon;

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

using Ini;


using Alturos.Yolo;
using Alturos.Yolo.Model;

using System.Threading;

using VisionCounting;

using MetroFramework.Forms;
using MetroFramework.Components;
using MetroFramework;

using System.Drawing.Drawing2D;

namespace DLCounting
{

    public partial class Frmmain : MetroForm
    {
        private YoloWrapper _yoloWrapper;

        // member variables B,G,R '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        private MCvScalar SCALAR_BLACK = new MCvScalar(0.0, 0.0, 0.0);
        private MCvScalar SCALAR_WHITE = new MCvScalar(255.0, 255.0, 255.0);
        private MCvScalar SCALAR_BLUE = new MCvScalar(255.0, 0.0, 0.0); //B,G,R
        private MCvScalar SCALAR_GREEN = new MCvScalar(0.0, 200.0, 0.0);
        private MCvScalar SCALAR_RED = new MCvScalar(0.0, 0.0, 255.0);
        private MCvScalar SCALAR_GRAY = new MCvScalar(50, 50, 50);
        private MCvScalar SCALAR_YELLOW = new MCvScalar(0.0, 255.0, 255.0);

        private double lowThr;
        private double highThr;
        private double accThr;

        private double dist,
                    param,
                    minRad,
                    maxRad,
                    rad1;
        private int radiusThr,
                    thick,
                    blue,
                    green,
                    red,
                    gb;

        private bool blnFormClosing = false;

        private Bitmap m_bitmap = null; /* The bitmap is used for displaying the image. */

        private Bitmap m_bitmapDL = null;
        private Bitmap m_bitmapColor = null;
        private Bitmap m_bitmapROI = null;
        private Bitmap bitmapDL = null; //......ROI Crop




        public IntPtr PtrImage;
        public int ImageWidth = 0;
        public int ImageHeight = 0;


        public Camera camera = null;
        public PixelDataConverter converter = new PixelDataConverter();
        private Stopwatch CameraGrapWatch = new Stopwatch();

        private Stopwatch CameraGrapTriggerWatch = new Stopwatch();

        public bool ContinuousShotStatus = false;
        public int CameraIndex = 0;
        public bool Colorselect = false;
        public string CamSN = "";

        public bool CameraStatus = false;


        public Emgu.CV.Image<Bgr, Byte> currentFrame;

        public Emgu.CV.Image<Bgr, Byte> currentFrameDiff;
        public Emgu.CV.Image<Bgr, Byte> currentFrameBlob;
        public Emgu.CV.Image<Bgr, Byte> currentFrameBlobROI;
        public Emgu.CV.Image<Bgr, Byte> currentFrameBlobColor;
        public Emgu.CV.Image<Bgr, Byte> currentFrameDL;

        public Mat imgcurrentFrameDL = null;
        public Mat imgcurrentFrameBlob = null;
        public Mat imgcurrentFrameBlobColor = null;
        public Mat imgcurrentFrameDiff = null;


        public Mat currentFrameLineConcat = null;
        public Mat currentFrameLineConcatMark = null;

        public Mat currentFrameLine = null;
        public Mat currentFrameLineMark = null;

        public Mat currentFrameLineTemp = null;
        public Mat currentFrameLineTempMark = null;

        public Emgu.CV.Image<Bgr, Byte> currentFrameROI;

        public bool ProcessEnd = false;

        private VideoCapture HSVcapture;
        private bool HSVcaptureInProgress;
        private bool HSVimageInProgress;
        String filenameload;
        Image<Bgr, Byte> HSVImageFrame;// = new Image<Bgr, Byte>(320, 240);
        Image<Bgr, Byte> ImageHSVwheel;// = new Image<Bgr, Byte>("HSV-Wheel.png");
        Image<Hsv, Byte> hsvImage;// = new Image<Hsv, Byte>(0, 0);
        int diff_LH;


        System.Globalization.CultureInfo DatetimeSystem_US = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        System.Globalization.CultureInfo DatetimeSystem_TH = System.Globalization.CultureInfo.GetCultureInfo("th-TH");

        public Thread YoloThread;
        public bool Yolorun = false;
        public bool YoloState = false;

        public Image imageDLDraw = null;
        public string ObjectDLSelect = "";
        public double DLConfidence = 0.4;
        public string DLSpeedUpdate = "";

        public int DL_Type_Count_Accept = 3;

        public Thread BlobThread;
        public bool Blobrun = false;
        public bool BlobState = false;
        Image imageDraw = null;


        public Thread BlobColorThread;
        public bool BlobColorrun = false;
        public bool BlobColorState = false;

        public bool VideoTestMode = false;
        
        public int FrameTriggerCount = 0;
        public int FrameTriggerCount_EndProcess = 0;
        public long FrameTriggerTimeGrab = 0; //> 200 ms


        public bool FirstFrame =false;
        public int FrameCount = 0;
        public int FrameCount_Old = 0;
        public int LineCount = 0;

        int Old_BlobFrameCount = 0;

        public int countdetect = 0;

        public int Count_old = 9999;


        int Old_FrameProcess = 0;
        Rectangle Old_RectangleProcess;
        double Old_Area = 0;
        int Old_case = 0;


        public int grabResult_Width = 0;
        public int grabResult_Height = 0;

        public Point[] crossingLineCheck1 = new Point[2];
        public Point[] crossingLineCheck2 = new Point[2];
        public Point[] crossingLineCheck3 = new Point[2];

        public Point[] crossingLineStart = new Point[2];
        public Point[] crossingLineDL = new Point[2];

        public List<YoloItem> items_old;
        public List<YoloItem> current_items;

        public List<TrackableObject> ObjectTrack = new List<TrackableObject>();
        public int horizontalLinePosition = 0;


        public int verticalLinePositionEnd_1 = 0;
        public int verticalLinePositionEnd_2 = 0;
        public int verticalLinePositionEnd_3 = 0;
        public int verticalLinePositionStart = 0;
        public int verticalLineDLEnd = 0;
        

        public int ObjectCount1 = 0;
        public int ObjectCount2 = 0;
        public int ObjectCount3 = 0;

        public int ObjectCountAll = 0;
        public int RotateBufferCount = 0;

        public double dblLeastDistance = 100.0;
      
        Mat imgDifference = null;
        Mat imgOutDifference = null;
        Mat imgOutDifferenceGray = null;

        Mat imageContours = null;
        UMat uimageContours = null;

        List<Blob> blobs = new List<Blob>();



        Mat imgFramecurrent;
        
       
        public FrmIO frmDigitalIO = new FrmIO();
        public FrmColorView frmColorView = new FrmColorView();
        public FrmProduct frmProduct = new FrmProduct();
       
        public int Zoom = 75;

        public bool CheckROIMode = false;

        public bool BlobProcessState = false;
        public bool BlobColorProcessState = false;
        public bool DiffProcessState = false;
        public bool DLProcessState = false;

        public int intnumOffsetLeft = 0;
        public int intnumOffsetRight = 0;
        public int intnumZoomProcess = 50;
        public int intnumZoomBuffer = 50;

        public Label[] lbimageinfo;

        public List<Blob> blobsCount = new List<Blob>();


        public bool YellowColorMode = true;// true->HSV
        public bool GrayColorMode = true;
        public bool WhiteColorMode = true;
        public bool BlueColorMode = true;

        public int HminYellow = 0;
        public int SminYellow = 0;
        public int VminYellow = 0;

        public int HmaxYellow = 180;
        public int SmaxYellow = 255;
        public int VmaxYellow = 255;


        public int HminGray = 0;
        public int SminGray = 0;
        public int VminGray = 0;

        public int HmaxGray = 180;
        public int SmaxGray = 255;
        public int VmaxGray = 255;


        public int HminWhite = 0;
        public int SminWhite = 0;
        public int VminWhite = 0;

        public int HmaxWhite = 180;
        public int SmaxWhite = 255;
        public int VmaxWhite = 255;


        public int HminBlue = 0;
        public int SminBlue = 0;
        public int VminBlue = 0;

        public int HmaxBlue = 180;
        public int SmaxBlue = 255;
        public int VmaxBlue = 255;

        public int BminYellow = 0;
        public int GminYellow = 0;
        public int RminYellow = 0;

        public int BmaxYellow = 180;
        public int GmaxYellow = 255;
        public int RmaxYellow = 255;



        public int BminGray = 0;
        public int GminGray = 0;
        public int RminGray = 0;

        public int BmaxGray = 255;
        public int GmaxGray = 255;
        public int RmaxGray = 255;


        public int BminWhite = 0;
        public int GminWhite = 0;
        public int RminWhite = 0;

        public int BmaxWhite = 255;
        public int GmaxWhite = 255;
        public int RmaxWhite = 255;


        public int BminBlue = 0;
        public int GminBlue = 0;
        public int RminBlue = 0;

        public int BmaxBlue = 255;
        public int GmaxBlue = 255;
        public int RmaxBlue = 255;


        public bool EnableBlueValue = false;
        public bool EnableWhiteValue = false;
        public bool EnableGrayValue = false;
        public bool EnableYellowValue = false;


        public bool ColorYellowSelect = false;
        public bool ColorGraySelect = false;
        public bool ColorWhiteSelect = false;
        public bool ColorBlueSelect = false;



        Emgu.CV.VideoCapture videocapture;
        Emgu.CV.VideoWriter  videoWriter;


        bool IsPlaying = false;
        int TotalFrames;
        int CurrentFrameNo;
       
        int FPS;


        int ThresholdColorView = 0;
        bool EnableDebugMode = false;
        bool EnableDrawCurrentBlob = false;
        bool EnableDrawTracking = false;

        bool EnableColorTypeDetect = false;
        bool ColorTypeDetectState = false;
        bool DetectCaseZero = false;

        bool EnableDLTrainingSave = false;
        bool EnableDLCheck = false;

        bool DLCheckResult = false;

        public Int64 AreaAceptmin = 0;
        public Int64 AreaAceptmax = 0;
        public Int64 LeastDistance = 0;

        public Int64 AreaAceptCase1 = 0;
        public Int64 AreaAceptCase2 = 0;
        public Int64 AreaAceptCase3 = 0;
        public Int64 AreaAceptCase4 = 0;
        public Int64 AreaAceptCase5 = 0;
        public Int64 AreaAceptCase6 = 0;
        public Int64 AreaAceptCase7 = 0;
        public Int64 AreaAceptCase8 = 0;
        public Int64 AreaAceptCase9 = 0;
        public Int64 AreaAceptCase10 = 0;


        string CammeraSN = "40035372";  //MC# 1
        

        double CrossedLineStart = 0.60;

        double CrossedLineEnd_1 = 0.60;
        double CrossedLineEnd_2 = 0.60;
        double CrossedLineEnd_3 = 0.60;

        double CrossedLineDL = 0.80;

        int ClearObjectNotMove = 5;

        Brush[] BrushesColor = new Brush[10];

        public bool video_rec_start = false;
        public int Frame_Rec_Count = 0;

        public int BlobFrameCount =0 ;

        public bool OnGrabState = false;


        string DataSetPath = "";
        string DataSetPath_DATA = "";
        string DataSetPath_DATA_IMG = "";
        string DataSetPath_DATA_IMG_DL_ERROR = "";

        string DataResultPath = "";

        public int ProcessstopState=0;
        public int ProcessDelayCount = 0;

        int btPlay_Left ;
        int btPlay_Top;
        Size btPlay_Size;


        int btClearObjectZero_Left;
        int btClearObjectZero_Top;
        Size btClearObjectZero_Size;

        public int NumDilate = 3;
        public int NumErode = 1;

        bool ViewAllDataSet = false;
        public Frmmain()
        {
            InitializeComponent();


            UpdateDeviceList();

            // Disable all buttons.
            EnableButtons(false, false);



            if (CvInvoke.HaveOpenCL)
            {
                CvInvoke.UseOpenCL = true;
               
            }
            else
            {
                CvInvoke.UseOpenCL = false;

            }
          
            
                                                   
            lbimageinfo = new Label[] {  lbimageinfo1, lbimageinfo2, lbimageinfo3, lbimageinfo4};

            BrushesColor[0] = Brushes.Red;
            BrushesColor[1] = Brushes.Green;
            BrushesColor[2] = Brushes.Orange;
            BrushesColor[3] = Brushes.Brown;
            BrushesColor[4] = Brushes.DeepPink;
            BrushesColor[5] = Brushes.Gold;
            BrushesColor[6] = Brushes.OrangeRed;
            BrushesColor[7] = Brushes.Purple;
            BrushesColor[8] = Brushes.HotPink;
            BrushesColor[9] = Brushes.PaleVioletRed;



        }

        private Mat ResizeRunGPU(Mat matSrc)
        {
            Mat matDst = new Mat();
            using (GpuMat gMatSrc = new GpuMat())
            using (GpuMat gMatDst = new GpuMat())
            {
                gMatSrc.Upload(matSrc);
                Emgu.CV.Cuda.CudaInvoke.Resize(gMatSrc, gMatDst, new Size(0, 0), 0.5, 0.5);
                gMatDst.Download(matDst);
            }
            return matDst;

        }

        private void Frmmain_Load(object sender, EventArgs e)
        {

            lbAlarmInformation3.Text = "";

            btPlay_Left = btPlay.Left;
            btPlay_Top = btPlay.Top;
            btPlay_Size = btPlay.Size;

            btClearObjectZero_Left = btClearObjectZero.Left;
            btClearObjectZero_Top = btClearObjectZero.Top;
            btClearObjectZero_Size = btClearObjectZero.Size;

            picBarcode.Image = picIAI.Image = picCamera.Image=picDigitalIO.Image = picfail.Image;

            imagecrop1.Image = null;
            imagecrop1.Invalidate();

            imagecrop2.Image = null;
            imagecrop2.Invalidate();

            imagecrop3.Image = null;
            imagecrop3.Invalidate();

            imagecrop4.Image = null;
            imagecrop4.Invalidate();


            for (int i = 0; i < lbimageinfo.Length; i++)
            {
                
                lbimageinfo[i].Text = "Count:";
            }

   

            #region Check Directory Path 
            DataSetPath = "C:\\DLDATA";
            DataSetPath_DATA = DataSetPath + "\\DATA";
            DataSetPath_DATA_IMG = DataSetPath_DATA + "\\IMG";

            DataSetPath_DATA_IMG_DL_ERROR = DataSetPath_DATA + "\\IMG_ERROR_DL";

            try
            {
                if (!Directory.Exists(DataSetPath))
                {
                    CreateIfMissing(DataSetPath);
                }

            }
            catch
            {

            }


            try
            {
                if (!Directory.Exists(DataSetPath_DATA_IMG))
                {
                    CreateIfMissing(DataSetPath_DATA_IMG);
                }

            }
            catch
            {

            }

            try
            {
                if (!Directory.Exists(DataSetPath_DATA_IMG_DL_ERROR))
                {
                    CreateIfMissing(DataSetPath_DATA_IMG_DL_ERROR);
                }

            }
            catch
            {

            }




            try
            {
                if (!Directory.Exists(DataSetPath_DATA))
                {
                    CreateIfMissing(DataSetPath_DATA);
                }

            }
            catch
            {

            }


            // CreateIfMissing(ProductPath);



            
            DataResultPath = "C:\\Image Result";
            try
            {
                if (!Directory.Exists(DataResultPath))
                {
                    CreateIfMissing(DataResultPath);
                }

            }
            catch
            {

            }

            #endregion Check Directory Path



            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect(".");

            //namesFile
            if (config == null)
            {
                return;
            }

            //******Disalble CoDA
            this.Initialize(config);
            LoadAvailableConfigurations();

            OpenColorView();
            frmColorView.Hide();

            OpenIO();
            frmDigitalIO.Hide();

            OpenProduct();
            frmProduct.Hide();

  
            Zoom = imgDiff.Zoom;

            imgDiff.CenterToImage();
            imgDiff.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;


            imagecrop1.CenterToImage();
            imagecrop1.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;
            imagecrop2.CenterToImage();
            imagecrop2.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;
            imagecrop3.CenterToImage();
            imagecrop3.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;
            imagecrop4.CenterToImage();
            imagecrop4.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;
            


            PanelAlarm.Left =  (int)(this.Width / 2) - (int)(PanelAlarm.Width / 2);
            PanelAlarm.Top =  (int)(this.Height / 2) - (int)(PanelAlarm.Height / 2);
            PanelAlarm.BringToFront();

            

            btDisConnect.Left = btConnect.Left;
            btDisConnect.Top = btConnect.Top;

            btPause.Left = btPlay.Left;
            btPause.Top = btPlay.Top;


            string ObjectNames = Application.StartupPath + "\\obj.names";

            using (StreamReader r = new StreamReader(ObjectNames))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {

                    cbObjectList.Items.Add(line.Trim());
                    frmProduct.cbObjectDL.Items.Add(line.Trim());
                }
            }

            LoadConfig();

            RadioColorType1Update();
            UpdateColorThresholdView();



            InitialConnectSystem();


            timerprocess.Enabled = true;
        }


        public void InitialConnectSystem()
        {

            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);


            
            string retCammeraSN = ini.IniReadValue("Camera", "SN", "40035372");
            CammeraSN = retCammeraSN;// "40035372";  //MC# 1

            ini.IniWriteValue("Camera", "SN", CammeraSN);

            bool ret = InitailCamera(CammeraSN);//ON MC
            if (ret == true)
            {
                ini.IniWriteValue("Camera", "SN", CammeraSN);

                picCamera.Image = pictrue.Image;

                btInitialCamera.Enabled = true;

                ContinuousShot();


               
                BlobColorrun = true;

  
                imageDLDraw = imageDL.Image;


                btConnect.Visible = false;
                btDisConnect.Visible = true;

                btPlay.Visible = true;
                btPause.Visible = false;
            }
            else
            {
                picCamera.Image = picfail.Image;

                btInitialCamera.Enabled = false;

                btConnect.Visible = true;
                btDisConnect.Visible = false;

                btPlay.Visible = true;
                btPause.Visible = false;

                txtCameraModel.Text = "Camera SN :" + CammeraSN;
                txtCameraModel.BackColor = Color.Red;
            }
        }

        public void RadioColorType1Update()
        {
            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);


            string retHmin = ini.IniReadValue("HSV Blue", "Hmin", "0");
            string retSmin = ini.IniReadValue("HSV Blue", "Smin", "0");
            string retVmin = ini.IniReadValue("HSV Blue", "Vmin", "0");

            string retHmax = ini.IniReadValue("HSV Blue", "Hmax", "180");
            string retSmax = ini.IniReadValue("HSV Blue", "Smax", "360");
            string retVmax = ini.IniReadValue("HSV Blue", "Vmax", "360");


            string retBmin = ini.IniReadValue("BGR Blue", "Bmin", "0");
            string retGmin = ini.IniReadValue("BGR Blue", "Gmin", "0");
            string retRmin = ini.IniReadValue("BGR Blue", "Rmin", "0");

            string retBmax = ini.IniReadValue("BGR Blue", "Bmax", "255");
            string retGmax = ini.IniReadValue("BGR Blue", "Gmax", "255");
            string retRmax = ini.IniReadValue("BGR Blue", "Rmax", "255");

            HminBlue = int.Parse(retHmin);
            SminBlue = int.Parse(retSmin);
            VminBlue = int.Parse(retVmin);

            HmaxBlue = int.Parse(retHmax);
            SmaxBlue = int.Parse(retSmax);
            VmaxBlue = int.Parse(retVmax);

            BminBlue = int.Parse(retBmin);
            GminBlue = int.Parse(retGmin);
            RminBlue = int.Parse(retRmin);

            BmaxBlue = int.Parse(retBmax);
            GmaxBlue = int.Parse(retGmax);
            RmaxBlue = int.Parse(retRmax);


            numHmin.Value = (decimal)HminBlue;
            numSmin.Value = (decimal)SminBlue;
            numVmin.Value = (decimal)VminBlue;

            numHmax.Value = (decimal)HmaxBlue;
            numSmax.Value = (decimal)SmaxBlue;
            numVmax.Value = (decimal)VmaxBlue;

            numBmin.Value = (decimal)BminBlue;
            numGmin.Value = (decimal)GminBlue;
            numRmin.Value = (decimal)RminBlue;

            numBmax.Value = (decimal)BmaxBlue;
            numGmax.Value = (decimal)GmaxBlue;
            numRmax.Value = (decimal)RmaxBlue;


            string retcolor = ini.IniReadValue("Color Blue", "Mode", "true");
            if (retcolor == "1")
            {
                ckColorMode1.Checked = true;
                ckColorMode2.Checked = false;
                BlueColorMode = true;
            }
            else
            {
                ckColorMode1.Checked = false;
                ckColorMode2.Checked = true;
                BlueColorMode = false;
            }



        }
        public void RadioColorType2Update()
        {
            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);


            string retHmin = ini.IniReadValue("HSV White", "Hmin", "0");
            string retSmin = ini.IniReadValue("HSV White", "Smin", "0");
            string retVmin = ini.IniReadValue("HSV White", "Vmin", "0");

            string retHmax = ini.IniReadValue("HSV White", "Hmax", "180");
            string retSmax = ini.IniReadValue("HSV White", "Smax", "360");
            string retVmax = ini.IniReadValue("HSV White", "Vmax", "360");

            string retBmin = ini.IniReadValue("BGR White", "Bmin", "0");
            string retGmin = ini.IniReadValue("BGR White", "Gmin", "0");
            string retRmin = ini.IniReadValue("BGR White", "Rmin", "0");

            string retBmax = ini.IniReadValue("BGR White", "Bmax", "255");
            string retGmax = ini.IniReadValue("BGR White", "Gmax", "255");
            string retRmax = ini.IniReadValue("BGR White", "Rmax", "255");

            HminWhite = int.Parse(retHmin);
            SminWhite = int.Parse(retSmin);
            VminWhite = int.Parse(retVmin);


            if (int.Parse(retHmax) > 180)
            {
                HmaxWhite = 180;
            }
            else
            {
                HmaxWhite = int.Parse(retHmax);
            }

            SmaxWhite = int.Parse(retSmax);
            VmaxWhite = int.Parse(retVmax);

            BminWhite = int.Parse(retBmin);
            GminWhite = int.Parse(retGmin);
            RminWhite = int.Parse(retRmin);

            BmaxWhite = int.Parse(retBmax);
            GmaxWhite = int.Parse(retGmax);
            RmaxWhite = int.Parse(retRmax);

            numHmin.Value = (decimal)HminWhite;
            numSmin.Value = (decimal)SminWhite;
            numVmin.Value = (decimal)VminWhite;

            numHmax.Value = (decimal)HmaxWhite;
            numSmax.Value = (decimal)SmaxWhite;
            numVmax.Value = (decimal)VmaxWhite;

            numBmin.Value = (decimal)BminWhite;
            numGmin.Value = (decimal)GminWhite;
            numRmin.Value = (decimal)RminWhite;

            numBmax.Value = (decimal)BmaxWhite;
            numGmax.Value = (decimal)GmaxWhite;
            numRmax.Value = (decimal)RmaxWhite;

            string retcolor = ini.IniReadValue("Color White", "Mode", "true");
            if (retcolor == "1")
            {
                ckColorMode1.Checked = true;
                ckColorMode2.Checked = false;
                WhiteColorMode = true;
            }
            else
            {
                ckColorMode1.Checked = false;
                ckColorMode2.Checked = true;
                WhiteColorMode = false;
            }



        }
        public void RadioColorType3Update()
        {
            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);




            string retHmin = ini.IniReadValue("HSV Gray", "Hmin", "0");
            string retSmin = ini.IniReadValue("HSV Gray", "Smin", "0");
            string retVmin = ini.IniReadValue("HSV Gray", "Vmin", "0");

            string retHmax = ini.IniReadValue("HSV Gray", "Hmax", "180");
            string retSmax = ini.IniReadValue("HSV Gray", "Smax", "360");
            string retVmax = ini.IniReadValue("HSV Gray", "Vmax", "360");

            string retBmin = ini.IniReadValue("BGR Gray", "Bmin", "0");
            string retGmin = ini.IniReadValue("BGR Gray", "Gmin", "0");
            string retRmin = ini.IniReadValue("BGR Gray", "Rmin", "0");

            string retBmax = ini.IniReadValue("BGR Gray", "Bmax", "255");
            string retGmax = ini.IniReadValue("BGR Gray", "Gmax", "255");
            string retRmax = ini.IniReadValue("BGR Gray", "Rmax", "255");


            HminGray = int.Parse(retHmin);
            SminGray = int.Parse(retSmin);
            VminGray = int.Parse(retVmin);

            HmaxGray = int.Parse(retHmax);
            SmaxGray = int.Parse(retSmax);
            VmaxGray = int.Parse(retVmax);

            BminBlue = int.Parse(retBmin);
            GminBlue = int.Parse(retGmin);
            RminBlue = int.Parse(retRmin);

            BmaxBlue = int.Parse(retBmax);
            GmaxBlue = int.Parse(retGmax);
            RmaxBlue = int.Parse(retRmax);

            numHmin.Value = (decimal)HminGray;
            numSmin.Value = (decimal)SminGray;
            numVmin.Value = (decimal)VminGray;


            if (HmaxGray > 180)
            {
                HmaxGray = 180;
            }


            numHmax.Value = (decimal)HmaxGray;
            numSmax.Value = (decimal)SmaxGray;
            numVmax.Value = (decimal)VmaxGray;

            numBmin.Value = (decimal)BminGray;
            numGmin.Value = (decimal)GminGray;
            numRmin.Value = (decimal)RminGray;

            if (BmaxGray > 255)
            {
                BmaxGray = 255;
            }
            if (GmaxGray > 255)
            {
                GmaxGray = 255;
            }
            if (RmaxGray > 255)
            {
                RmaxGray = 255;
            }

            numBmax.Value = (decimal)BmaxGray;
            numGmax.Value = (decimal)GmaxGray;
            numRmax.Value = (decimal)RmaxGray;

            string retcolor = ini.IniReadValue("Color Gray", "Mode", "true");
            if (retcolor == "1")
            {
                ckColorMode1.Checked = true;
                ckColorMode2.Checked = false;
                GrayColorMode = true;
            }
            else
            {
                ckColorMode1.Checked = false;
                ckColorMode2.Checked = true;
                GrayColorMode = false;
            }


        }
        public void RadioColorType4Update()
        {
            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);


            string retHmin = ini.IniReadValue("HSV Yellow", "Hmin", "0");
            string retSmin = ini.IniReadValue("HSV Yellow", "Smin", "0");
            string retVmin = ini.IniReadValue("HSV Yellow", "Vmin", "0");

            string retHmax = ini.IniReadValue("HSV Yellow", "Hmax", "180");
            string retSmax = ini.IniReadValue("HSV Yellow", "Smax", "360");
            string retVmax = ini.IniReadValue("HSV Yellow", "Vmax", "360");

            string retBmin = ini.IniReadValue("BGR Yellow", "Bmin", "0");
            string retGmin = ini.IniReadValue("BGR Yellow", "Gmin", "0");
            string retRmin = ini.IniReadValue("BGR Yellow", "Rmin", "0");

            string retBmax = ini.IniReadValue("BGR Yellow", "Bmax", "255");
            string retGmax = ini.IniReadValue("BGR Yellow", "Gmax", "255");
            string retRmax = ini.IniReadValue("BGR Yellow", "Rmax", "255");

            HminYellow = int.Parse(retHmin);
            SminYellow = int.Parse(retSmin);
            VminYellow = int.Parse(retVmin);

            HmaxYellow = int.Parse(retHmax);
            SmaxYellow = int.Parse(retSmax);
            VmaxYellow = int.Parse(retVmax);

            BminYellow = int.Parse(retBmin);
            GminYellow = int.Parse(retGmin);
            RminYellow = int.Parse(retRmin);

            BmaxYellow = int.Parse(retBmax);
            GmaxYellow = int.Parse(retGmax);
            RmaxYellow = int.Parse(retRmax);

            numHmin.Value = (decimal)HminYellow;
            numSmin.Value = (decimal)SminYellow;
            numVmin.Value = (decimal)VminYellow;

            numHmax.Value = (decimal)HmaxYellow;
            numSmax.Value = (decimal)SmaxYellow;
            numVmax.Value = (decimal)VmaxYellow;

            numBmin.Value = (decimal)BminYellow;
            numGmin.Value = (decimal)GminYellow;
            numRmin.Value = (decimal)RminYellow;

            numBmax.Value = (decimal)BmaxYellow;
            numGmax.Value = (decimal)GmaxYellow;
            numRmax.Value = (decimal)RmaxYellow;

            string retcolor = ini.IniReadValue("Color Yellow", "Mode", "true");
            if (retcolor == "1")
            {
                ckColorMode1.Checked = true;
                ckColorMode2.Checked = false;
                YellowColorMode = true;
            }
            else
            {
                ckColorMode1.Checked = false;
                ckColorMode2.Checked = true;
                YellowColorMode = false;
            }



        }
        public void SaveConfig()
        {


            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);



             intnumOffsetLeft  = (Int16)numOffsetLeftProcess.Value;
             intnumOffsetRight = (Int16)numOffsetRightProcess.Value;
             intnumZoomProcess = (Int16)numZoomProcess.Value;
             intnumZoomBuffer  = (Int16)numZoomBuffer.Value;

            ini.IniWriteValue("View Config", "OffsetLeft", intnumOffsetLeft.ToString());
            ini.IniWriteValue("View Config", "OffsetRight", intnumOffsetRight.ToString());
            ini.IniWriteValue("View Config", "ZoomProcess", intnumZoomProcess.ToString());
            ini.IniWriteValue("View Config", "ZoomBuffer", intnumZoomBuffer.ToString());



            int Hmin = (Int16)numHmin.Value;
            int Smin = (Int16)numSmin.Value;
            int Vmin = (Int16)numVmin.Value;

            int Hmax = (Int16)numHmax.Value;
            int Smax = (Int16)numSmax.Value;
            int Vmax = (Int16)numVmax.Value;


            int Bmin = (Int16)numBmin.Value;
            int Gmin = (Int16)numGmin.Value;
            int Rmin = (Int16)numRmin.Value;

            int Bmax = (Int16)numBmax.Value;
            int Gmax = (Int16)numGmax.Value;
            int Rmax = (Int16)numRmax.Value;

            if (radioColorTypeBlue.Checked == true)//Blue
            {
                ini.IniWriteValue("HSV Blue", "Hmin", Hmin.ToString());
                ini.IniWriteValue("HSV Blue", "Smin", Smin.ToString());
                ini.IniWriteValue("HSV Blue", "Vmin", Vmin.ToString());

                ini.IniWriteValue("HSV Blue", "Hmax", Hmax.ToString());
                ini.IniWriteValue("HSV Blue", "Smax", Smax.ToString());
                ini.IniWriteValue("HSV Blue", "Vmax", Vmax.ToString());



                ini.IniWriteValue("BGR Blue", "Bmin", Bmin.ToString());
                ini.IniWriteValue("BGR Blue", "Gmin", Gmin.ToString());
                ini.IniWriteValue("BGR Blue", "Rmin", Rmin.ToString());

                ini.IniWriteValue("BGR Blue", "Bmax", Bmax.ToString());
                ini.IniWriteValue("BGR Blue", "Gmax", Gmax.ToString());
                ini.IniWriteValue("BGR Blue", "Rmax", Rmax.ToString());


                HminBlue = Hmin;
                SminBlue = Smin;
                VminBlue = Vmin;

                HmaxBlue = Hmax;
                SmaxBlue = Smax;
                VmaxBlue = Vmax;

                BminBlue = Bmin;
                GminBlue = Gmin;
                RminBlue = Rmin;

                BmaxBlue = Bmax;
                GmaxBlue = Gmax;
                RmaxBlue = Rmax;

                if (ckColorMode1.Checked)
                {
                    ini.IniWriteValue("Color Blue", "Mode", "1");
                    BlueColorMode = true;
                }
                else
                {
                    ini.IniWriteValue("Color Blue", "Mode", "0");
                    BlueColorMode = false;
                }

            }
            else if (radioColorTypeWhite.Checked == true) //White
            {
                ini.IniWriteValue("HSV White", "Hmin", Hmin.ToString());
                ini.IniWriteValue("HSV White", "Smin", Smin.ToString());
                ini.IniWriteValue("HSV White", "Vmin", Vmin.ToString());

                ini.IniWriteValue("HSV White", "Hmax", Hmax.ToString());
                ini.IniWriteValue("HSV White", "Smax", Smax.ToString());
                ini.IniWriteValue("HSV White", "Vmax", Vmax.ToString());

                ini.IniWriteValue("BGR White", "Bmin", Bmin.ToString());
                ini.IniWriteValue("BGR White", "Gmin", Gmin.ToString());
                ini.IniWriteValue("BGR White", "Rmin", Rmin.ToString());

                ini.IniWriteValue("BGR White", "Bmax", Bmax.ToString());
                ini.IniWriteValue("BGR White", "Gmax", Gmax.ToString());
                ini.IniWriteValue("BGR White", "Rmax", Rmax.ToString());


                HminWhite = Hmin;
                SminWhite = Smin;
                VminWhite = Vmin;

                HmaxWhite = Hmax;
                SmaxWhite = Smax;
                VmaxWhite = Vmax;


                BminWhite = Bmin;
                GminWhite = Gmin;
                RminWhite = Rmin;

                BmaxWhite = Bmax;
                GmaxWhite = Gmax;
                RmaxWhite = Rmax;


                if (ckColorMode1.Checked)
                {
                    ini.IniWriteValue("Color White", "Mode", "1");
                    WhiteColorMode = true;
                }
                else
                {
                    ini.IniWriteValue("Color White", "Mode", "0");
                    WhiteColorMode = false;
                }

            }
            else if (radioColorTypeGray.Checked == true)//Gray
            {
                ini.IniWriteValue("HSV Gray", "Hmin", Hmin.ToString());
                ini.IniWriteValue("HSV Gray", "Smin", Smin.ToString());
                ini.IniWriteValue("HSV Gray", "Vmin", Vmin.ToString());

                ini.IniWriteValue("HSV Gray", "Hmax", Hmax.ToString());
                ini.IniWriteValue("HSV Gray", "Smax", Smax.ToString());
                ini.IniWriteValue("HSV Gray", "Vmax", Vmax.ToString());

                ini.IniWriteValue("BGR Gray", "Bmin", Bmin.ToString());
                ini.IniWriteValue("BGR Gray", "Gmin", Gmin.ToString());
                ini.IniWriteValue("BGR Gray", "Rmin", Rmin.ToString());

                ini.IniWriteValue("BGR Gray", "Bmax", Bmax.ToString());
                ini.IniWriteValue("BGR Gray", "Gmax", Gmax.ToString());
                ini.IniWriteValue("BGR Gray", "Rmax", Rmax.ToString());



                HminGray = Hmin;
                SminGray = Smin;
                VminGray = Vmin;

                HmaxGray = Hmax;
                SmaxGray = Smax;
                VmaxGray = Vmax;

                BminGray = Bmin;
                GminGray = Gmin;
                RminGray = Rmin;

                BmaxGray = Bmax;
                GmaxGray = Gmax;
                RmaxGray = Rmax;


                if (ckColorMode1.Checked)
                {
                    ini.IniWriteValue("Color Gray", "Mode", "1");
                    GrayColorMode = true;
                }
                else
                {
                    ini.IniWriteValue("Color Gray", "Mode", "0");
                    GrayColorMode = false;
                }

            }
            else if (radioColorTypeYellow.Checked == true) //Yellow
            {
                ini.IniWriteValue("HSV Yellow", "Hmin", Hmin.ToString());
                ini.IniWriteValue("HSV Yellow", "Smin", Smin.ToString());
                ini.IniWriteValue("HSV Yellow", "Vmin", Vmin.ToString());

                ini.IniWriteValue("HSV Yellow", "Hmax", Hmax.ToString());
                ini.IniWriteValue("HSV Yellow", "Smax", Smax.ToString());
                ini.IniWriteValue("HSV Yellow", "Vmax", Vmax.ToString());

                ini.IniWriteValue("BGR Yellow", "Bmin", Bmin.ToString());
                ini.IniWriteValue("BGR Yellow", "Gmin", Gmin.ToString());
                ini.IniWriteValue("BGR Yellow", "Rmin", Rmin.ToString());

                ini.IniWriteValue("BGR Yellow", "Bmax", Bmax.ToString());
                ini.IniWriteValue("BGR Yellow", "Gmax", Gmax.ToString());
                ini.IniWriteValue("BGR Yellow", "Rmax", Rmax.ToString());


                HminYellow = Hmin;
                SminYellow = Smin;
                VminYellow = Vmin;

                HmaxYellow = Hmax;
                SmaxYellow = Smax;
                VmaxYellow = Vmax;


                BminYellow = Bmin;
                GminYellow = Gmin;
                RminYellow = Rmin;

                BmaxYellow = Bmax;
                GmaxYellow = Gmax;
                RmaxYellow = Rmax;

                if (ckColorMode1.Checked)
                {
                    ini.IniWriteValue("Color Yellow", "Mode", "1");
                    YellowColorMode = true;
                }
                else
                {
                    ini.IniWriteValue("Color Yellow", "Mode", "0");
                    YellowColorMode = false;
                }

            }


            if (ckEnableBlue.Checked)
            {
                ini.IniWriteValue("Enable Blue", "value", "1");
                EnableBlueValue = true;
            }
            else
            {
                ini.IniWriteValue("Enable Blue", "value", "0");
                EnableBlueValue = false;
            }

            if (ckEnableWhite.Checked)
            {
                ini.IniWriteValue("Enable White", "value", "1");
                EnableWhiteValue = true;
            }
            else
            {
                ini.IniWriteValue("Enable White", "value", "0");
                EnableWhiteValue = false;
            }


            if (ckEnableGray.Checked)
            {
                ini.IniWriteValue("Enable Gray", "value", "1");
                EnableGrayValue = true;
            }
            else
            {
                ini.IniWriteValue("Enable Gray", "value", "0");
                EnableGrayValue = false;
            }

            if (ckEnableYellow.Checked)
            {
                ini.IniWriteValue("Enable Yellow", "value", "1");
                EnableYellowValue = true;
            }
            else
            {
                ini.IniWriteValue("Enable Yellow", "value", "0");
                EnableYellowValue = false;
            }


            if (ckDebugmode.Checked == true)
            {
                ini.IniWriteValue("EnableDebugMode", "value", "1");
                EnableDebugMode = true;
            }
            else
            {
                ini.IniWriteValue("EnableDebugMode", "value", "0");
                EnableDebugMode = false;
            }

           

           


            if (ckColorTypeDetect.Checked == true)
            {
                ini.IniWriteValue("ColorTypeDetect", "value", "1");
                EnableColorTypeDetect = true;

               
            }
            else
            {
                ini.IniWriteValue("ColorTypeDetect", "value", "0");
                EnableColorTypeDetect = false;
            }




            if (ckSaveResultImage.Checked == true)
            {
                ini.IniWriteValue("SaveResultImage", "value", "1");
            

            }
            else
            {
                ini.IniWriteValue("SaveResultImage", "value", "0");
             
            }
            



            LoadConfig();
        }
        public void LoadConfig()
        {

            string aPath = Application.StartupPath + "\\config.ini";
            IniFile ini = new IniFile(aPath);


           

            string retOffsetLeft  = ini.IniReadValue("View Config", "OffsetLeft",  "0");
            string retOffsetRight = ini.IniReadValue("View Config", "OffsetRight", "0");
            string retZoomProcess = ini.IniReadValue("View Config", "ZoomProcess", "50");
            string retZoomBuffer  = ini.IniReadValue("View Config", "ZoomBuffer",  "50");

            intnumOffsetLeft  = int.Parse(retOffsetLeft);
            intnumOffsetRight = int.Parse(retOffsetRight);
            intnumZoomProcess = int.Parse(retZoomProcess);
            intnumZoomBuffer = int.Parse(retZoomBuffer);

            numOffsetLeftProcess.Value = intnumOffsetLeft;
            numOffsetRightProcess.Value= intnumOffsetRight;
            numZoomProcess.Value = intnumZoomProcess;
            numZoomBuffer.Value = intnumZoomBuffer;


            string retHmin = ini.IniReadValue("HSV Blue", "Hmin", "0");
            string retSmin = ini.IniReadValue("HSV Blue", "Smin", "0");
            string retVmin = ini.IniReadValue("HSV Blue", "Vmin", "0");

            string retHmax = ini.IniReadValue("HSV Blue", "Hmax", "180");
            string retSmax = ini.IniReadValue("HSV Blue", "Smax", "360");
            string retVmax = ini.IniReadValue("HSV Blue", "Vmax", "360");


            HminBlue = int.Parse(retHmin);
            SminBlue = int.Parse(retSmin);
            VminBlue = int.Parse(retVmin);

            HmaxBlue = int.Parse(retHmax);
            SmaxBlue = int.Parse(retSmax);
            VmaxBlue = int.Parse(retVmax);


            string retBmin = ini.IniReadValue("BGR Blue", "Bmin", "0");
            string retGmin = ini.IniReadValue("BGR Blue", "Gmin", "0");
            string retRmin = ini.IniReadValue("BGR Blue", "Rmin", "0");

            string retBmax = ini.IniReadValue("BGR Blue", "Bmax", "360");
            string retGmax = ini.IniReadValue("BGR Blue", "Gmax", "360");
            string retRmax = ini.IniReadValue("BGR Blue", "Rmax", "360");


            BminBlue = int.Parse(retBmin);
            GminBlue = int.Parse(retGmin);
            RminBlue = int.Parse(retRmin);

            BmaxBlue = int.Parse(retBmax);
            GmaxBlue = int.Parse(retGmax);
            RmaxBlue = int.Parse(retRmax);


            string retColorMode = ini.IniReadValue("Color Blue", "Mode", "1");

            if (retColorMode == "1")
            {
                BlueColorMode = true;
            }
            else
            {
                BlueColorMode = false;
            }




            retHmin = ini.IniReadValue("HSV White", "Hmin", "0");
            retSmin = ini.IniReadValue("HSV White", "Smin", "0");
            retVmin = ini.IniReadValue("HSV White", "Vmin", "0");

            retHmax = ini.IniReadValue("HSV White", "Hmax", "180");
            retSmax = ini.IniReadValue("HSV White", "Smax", "360");
            retVmax = ini.IniReadValue("HSV White", "Vmax", "360");

            HminWhite = int.Parse(retHmin);
            SminWhite = int.Parse(retSmin);
            VminWhite = int.Parse(retVmin);

            HmaxWhite = int.Parse(retHmax);
            SmaxWhite = int.Parse(retSmax);
            VmaxWhite = int.Parse(retVmax);

            retBmin = ini.IniReadValue("BGR White", "Bmin", "0");
            retGmin = ini.IniReadValue("BGR White", "Gmin", "0");
            retRmin = ini.IniReadValue("BGR White", "Rmin", "0");

            retBmax = ini.IniReadValue("BGR White", "Bmax", "360");
            retGmax = ini.IniReadValue("BGR White", "Gmax", "360");
            retRmax = ini.IniReadValue("BGR White", "Rmax", "360");

            BminWhite = int.Parse(retBmin);
            GminWhite = int.Parse(retGmin);
            RminWhite = int.Parse(retRmin);

            BmaxWhite = int.Parse(retBmax);
            GmaxWhite = int.Parse(retGmax);
            RmaxWhite = int.Parse(retRmax);


            retColorMode = ini.IniReadValue("Color White", "Mode", "1");

            if (retColorMode == "1")
            {
                WhiteColorMode = true;
            }
            else
            {
                WhiteColorMode = false;
            }


            retHmin = ini.IniReadValue("HSV Gray", "Hmin", "0");
            retSmin = ini.IniReadValue("HSV Gray", "Smin", "0");
            retVmin = ini.IniReadValue("HSV Gray", "Vmin", "0");

            retHmax = ini.IniReadValue("HSV Gray", "Hmax", "180");
            retSmax = ini.IniReadValue("HSV Gray", "Smax", "360");
            retVmax = ini.IniReadValue("HSV Gray", "Vmax", "360");


            HminGray = int.Parse(retHmin);
            SminGray = int.Parse(retSmin);
            VminGray = int.Parse(retVmin);

            HmaxGray = int.Parse(retHmax);
            SmaxGray = int.Parse(retSmax);
            VmaxGray = int.Parse(retVmax);


            retBmin = ini.IniReadValue("BGR Gray", "Bmin", "0");
            retGmin = ini.IniReadValue("BGR Gray", "Gmin", "0");
            retRmin = ini.IniReadValue("BGR Gray", "Rmin", "0");

            retBmax = ini.IniReadValue("BGR Gray", "Bmax", "360");
            retGmax = ini.IniReadValue("BGR Gray", "Gmax", "360");
            retRmax = ini.IniReadValue("BGR Gray", "Rmax", "360");

            BminGray = int.Parse(retBmin);
            GminGray = int.Parse(retGmin);
            RminGray = int.Parse(retRmin);

            BmaxGray = int.Parse(retBmax);
            GmaxGray = int.Parse(retGmax);
            RmaxGray = int.Parse(retRmax);


            retColorMode = ini.IniReadValue("Color Gray", "Mode", "1");

            if (retColorMode == "1")
            {
                GrayColorMode = true;
            }
            else
            {
                GrayColorMode = false;
            }



            retHmin = ini.IniReadValue("HSV Yellow", "Hmin", "0");
            retSmin = ini.IniReadValue("HSV Yellow", "Smin", "0");
            retVmin = ini.IniReadValue("HSV Yellow", "Vmin", "0");

            retHmax = ini.IniReadValue("HSV Yellow", "Hmax", "180");
            retSmax = ini.IniReadValue("HSV Yellow", "Smax", "360");
            retVmax = ini.IniReadValue("HSV Yellow", "Vmax", "360");

            HminYellow = int.Parse(retHmin);
            SminYellow = int.Parse(retSmin);
            VminYellow = int.Parse(retVmin);

            HmaxYellow = int.Parse(retHmax);
            SmaxYellow = int.Parse(retSmax);
            VmaxYellow = int.Parse(retVmax);



            retBmin = ini.IniReadValue("BGR Yellow", "Bmin", "0");
            retGmin = ini.IniReadValue("BGR Yellow", "Gmin", "0");
            retRmin = ini.IniReadValue("BGR Yellow", "Rmin", "0");

            retBmax = ini.IniReadValue("BGR Yellow", "Bmax", "360");
            retGmax = ini.IniReadValue("BGR Yellow", "Gmax", "360");
            retRmax = ini.IniReadValue("BGR Yellow", "Rmax", "360");

            BminYellow = int.Parse(retBmin);
            GminYellow = int.Parse(retGmin);
            RminYellow = int.Parse(retRmin);

            BmaxYellow = int.Parse(retBmax);
            GmaxYellow = int.Parse(retGmax);
            RmaxYellow = int.Parse(retRmax);


            retColorMode = ini.IniReadValue("Color Yellow", "Mode", "1");

            if (retColorMode == "1")
            {
                YellowColorMode = true;
            }
            else
            {
                YellowColorMode = false;
            }


            string retEnableBlue = ini.IniReadValue("Enable Blue", "value", "1");
            if (retEnableBlue=="1")
            {
                ckEnableBlue.Checked = true;
                EnableBlueValue = true;
                radioColorBlue.Enabled = true;
                radioColorTypeBlue.Enabled = true;
            }
            else
            {
                ckEnableBlue.Checked = false;
                EnableBlueValue = false;
                radioColorBlue.Enabled = false;
                radioColorTypeBlue.Enabled = false;
            }


            string retEnableWhite = ini.IniReadValue("Enable White", "value", "1");
            if (retEnableWhite == "1")
            {
                ckEnableWhite.Checked = true;
                EnableWhiteValue = true;
                radioColorWhite.Enabled = true;
                radioColorTypeWhite.Enabled = true;
            }
            else
            {
                ckEnableWhite.Checked = false;
                EnableWhiteValue = false;
                radioColorWhite.Enabled = false;
                radioColorTypeWhite.Enabled = false;
            }


            string retEnableGray = ini.IniReadValue("Enable Gray", "value", "1");
            if (retEnableGray == "1")
            {
                ckEnableGray.Checked = true;
                EnableGrayValue = true;
                radioColorGray.Enabled = true;
                radioColorTypeGray.Enabled = true;
               
            }
            else
            {
                ckEnableGray.Checked = false;
                EnableGrayValue = false;
                radioColorGray.Enabled = false;
                radioColorTypeGray.Enabled = false;
            }




            string retEnableYellow = ini.IniReadValue("Enable Yellow", "value", "1");
            if (retEnableYellow == "1")
            {
                ckEnableYellow.Checked = true;
                EnableYellowValue = true;
                radioColorYellow.Enabled = true;
                radioColorTypeYellow.Enabled = true;
            }
            else
            {
                ckEnableYellow.Checked = false;
                EnableYellowValue = false;
                radioColorYellow.Enabled = false;
                radioColorTypeYellow.Enabled = false;
            }



            if (radioColorTypeBlue.Checked)//"ฟ้า"
            {
                if (BlueColorMode)
                {
                    ckColorMode1.Checked = true;
                    ckColorMode2.Checked = false;

                }
                else
                {
                    ckColorMode1.Checked = false;
                    ckColorMode2.Checked = true;
                }
            }
            if (radioColorTypeWhite.Checked)//"ขาว"
            {
                if (WhiteColorMode)
                {
                    ckColorMode1.Checked = true;
                    ckColorMode2.Checked = false;

                }
                else
                {
                    ckColorMode1.Checked = false;
                    ckColorMode2.Checked = true;
                }
            }
            if (radioColorTypeGray.Checked)//"เทา"
            {
                if (GrayColorMode)
                {
                    ckColorMode1.Checked = true;
                    ckColorMode2.Checked = false;

                }
                else
                {
                    ckColorMode1.Checked = false;
                    ckColorMode2.Checked = true;
                }
            }
            if (radioColorTypeYellow.Checked)//"เหลือง"
            {
                if (YellowColorMode)
                {
                    ckColorMode1.Checked = true;
                    ckColorMode2.Checked = false;

                }
                else
                {
                    ckColorMode1.Checked = false;
                    ckColorMode2.Checked = true;
                }
            }




            string retEnableDebugMode = ini.IniReadValue("EnableDebugMode", "value", "1");
            string retEnableDrawCurrentBlob = ini.IniReadValue("EnableDrawCurrentBlob", "value", "1");
            string retEnableDrawTracking = ini.IniReadValue("EnableDrawTracking", "value", "1");

            if (retEnableDebugMode == "1")
            {
                ckDebugmode.Checked = true;
                EnableDebugMode = true;
            }
            else
            {
                ckDebugmode.Checked = false;
                EnableDebugMode = false;
            }

           

            


            string StrColorTypeDetect = ini.IniReadValue("ColorTypeDetect", "value", "1");

            if (StrColorTypeDetect=="1")
            {
                ckColorTypeDetect.Checked = true;
                EnableColorTypeDetect = true;
            }
            else
            {
                ckColorTypeDetect.Checked = false;
                EnableColorTypeDetect = false;
            }


           


            string StrCropImage = ini.IniReadValue("CropImage", "Enable", "1");
            if (StrCropImage == "1")
            {
                ckCheckCropImage.Checked = true;

            }
            else
            {
                ckCheckCropImage.Checked = false;
            }


            string retSaveResultImage = ini.IniReadValue("SaveResultImage", "value", "0");

            if (retSaveResultImage == "1")
            {
                ckSaveResultImage.Checked = true;

            }
            else
            {
                ckSaveResultImage.Checked = false;
            }


            


        }


        public void UpdateROISelect(bool state)
        {
            if (state == true)
            {


                imgDiff.SelectionRegion = new RectangleF(200, 200, 64, 64);
                // imageBlob.SelectionRegion = new RectangleF(roi_x, roi_y, roi_Width, roi_Height);
                imgDiff.CenterToImage();
                imgDiff.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal; //.....Disable ScrollBar

                // PicObjectDetect.Zoom = Zoom;
            }
            else
            {
                imgDiff.SelectionRegion = new RectangleF(0, 0, 0, 0);

                imgDiff.CenterToImage();
                imgDiff.SizeMode = Cyotek.Windows.Forms.ImageBoxSizeMode.Normal;
                // imgDiff.CenterToImage();
                // PicObjectDetect.Zoom = Zoom;
            }

           CheckROIMode = state;
        }

        private bool IsFormAlreadyOpen(Form FormCk)
        {

            foreach (Form f in Application.OpenForms)
            {
                if (f == FormCk)
                {
                    return true;
                }
            }

            return false;
        }

        public void OpenColorView()
        {
            if (IsFormAlreadyOpen(frmColorView) == true)//เช็ค  ว่าเปิดอยู่หรือเปล่า 
            {

                frmColorView.Activate();
                frmColorView.Visible = true;
                frmColorView.Show();
                frmColorView.ParentForm = this;

            }
            else
            {

                try
                {
                    frmColorView.Show();
                    frmColorView.Visible = true;
                    frmColorView.ParentForm = this;

                }
                catch
                {

                }
            }
        }


      

        public void OpenProduct()
        {
            if (IsFormAlreadyOpen(frmProduct) == true)//เช็ค  ว่าเปิดอยู่หรือเปล่า 
            {

                frmProduct.Activate();
                frmProduct.Visible = true;
                frmProduct.Show();
                frmProduct.ParentForm = this;

            }
            else
            {

                try
                {
                    frmProduct.Show();
                    frmProduct.Visible = true;
                    frmProduct.ParentForm = this;

                }
                catch
                {

                }
            }
        }
        public void OpenIO()
        {
            if (IsFormAlreadyOpen(frmDigitalIO) == true)//เช็ค  ว่าเปิดอยู่หรือเปล่า 
            {

                frmDigitalIO.Activate();
                frmDigitalIO.Visible = true;
                frmDigitalIO.Show();
                frmDigitalIO.ParentForm = this;

            }
            else
            {

                try
                {
                    frmDigitalIO.Show();
                    frmDigitalIO.Visible = true;
                    frmDigitalIO.ParentForm = this;

                }
                catch
                {

                }
            }
        }
        private void Initialize(string path)
        {
            var configurationDetector = new ConfigurationDetector();
            var config = configurationDetector.Detect(path);

            if (config == null)
            {
                return;
            }

            this.Initialize(config);
        }
        private void Initialize(YoloConfiguration config)
        {
            try
            {
                if (this._yoloWrapper != null)
                {
                    this._yoloWrapper.Dispose();
                }

                var useOnlyCpu = false;// this.cpuToolStripMenuItem.Checked;

                var sw = new Stopwatch();
                sw.Start();

                _yoloWrapper = new YoloWrapper(config.ConfigFile, config.WeightsFile, config.NamesFile, 0, useOnlyCpu);

                sw.Stop();


                var detectionSystemDetail = string.Empty;
                if (!string.IsNullOrEmpty(this._yoloWrapper.EnvironmentReport.GraphicDeviceName))
                {
                    detectionSystemDetail = $"({this._yoloWrapper.EnvironmentReport.GraphicDeviceName})";
                }

                lbYoloInitial.Text = $"Initialize Yolo in {sw.Elapsed.TotalMilliseconds:0} ms - Detection System:{this._yoloWrapper.DetectionSystem} {detectionSystemDetail} Weights:{config.WeightsFile}";

              

            }
            catch (Exception exception)
            {
                MessageBox.Show($"{nameof(Initialize)} - {exception}", "Error Initialize", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAvailableConfigurations()
        {
            var configPath = "config";

            if (!Directory.Exists(configPath))
            {
                return;
            }

            var configs = Directory.GetDirectories(configPath);
            if (configs.Length == 0)
            {
                return;
            }

           
        }




        private void ShowException(Exception exception)
        {
            //  MessageBox.Show("Exception caught:\n" + exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lberror.Text = "Exception caught:\n" + exception.Message;
        }

        private void DestroyCamera()
        {
            // Disable all parameter controls.
            try
            {
                if (camera != null)
                {


                }
            }
            catch (Exception exception)
            {
                 ShowException(exception);
            }

            // Destroy the camera object.
            try
            {
                if (camera != null)
                {
                    camera.Close();
                    camera.Dispose();
                    camera = null;
                }
            }
            catch (Exception exception)
            {
                 ShowException(exception);
            }

            // Destroy the converter object.
            if (converter != null)
            {
                converter.Dispose();
                converter = null;
            }


        }


        // Starts the grabbing of a single image and handles exceptions.
        private void OneShot()
        {
            try
            {
                // Starts the grabbing of one image.
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception exception)
            {
                btInitialCamera.Enabled = true;
                ShowException(exception);
            }
        }


        // Starts the continuous grabbing of images and handles exceptions.
        private void ContinuousShot()
        {
            try
            {
                ckTriggerMode.ForeColor = Color.Red;

                if (ckTriggerMode.Checked==true)
                {
                   


                    // Select GPIO line 1
                    camera.Parameters[PLCamera.LineSelector].SetValue(PLCamera.LineSelector.Line1);
                    // Set the line mode to Input
                    camera.Parameters[PLCamera.LineMode].SetValue(PLCamera.LineMode.Input);
                 
                    // Enable the line inverter for the I/O line selected
                    camera.Parameters[PLCamera.LineInverter].SetValue(false);     //Fix Adjust....
                    // Set the parameter value to 10 microseconds
                    camera.Parameters[PLCamera.LineDebouncerTime].SetValue(500.0); //Fix Adjust....

                    // Select the Frame Start trigger
                    camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
                    // Set the trigger activation mode to rising edge
                    camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);
                  
                    camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);

                    camera.Parameters[PLCamera.TriggerDelay].SetValue(0);

                    ckTriggerMode.ForeColor = Color.Green;


                }
                else
                {
                    camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                }


                // Start the grabbing of images until grabbing is stopped.
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                //camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);




            }
            catch (Exception exception)
            {
                btInitialCamera.Enabled = true;
                ShowException(exception);
            }
        }


        private void Stop()
        {
            // Stop the grabbing.
            if(camera==null)
            {
                return;
            }

            try
            {
                camera.StreamGrabber.Stop();
            }
            catch (Exception exception)
            {
                btInitialCamera.Enabled = true;
                ShowException(exception);
            }
        }




        // Occurs when a device with an opened connection is removed.
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<EventArgs>(OnConnectionLost), sender, e);
                return;
            }

            // Close the camera object.
            DestroyCamera();
            // Because one device is gone, the list needs to be updated.
            UpdateDeviceList();
        }




        private void EnableButtons(bool canGrab, bool canStop)
        {
           btContinuousShot.Enabled = canGrab;
           btOneShot.Enabled = canGrab;
           btStopGrab.Enabled = canStop;
        }



        // Occurs when the connection to a camera device is opened.
        private void OnCameraOpened(Object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<EventArgs>(OnCameraOpened), sender, e);
                return;
            }

            // The image provider is ready to grab. Enable the grab buttons.
            EnableButtons(true, false);
        }


        // Occurs when the connection to a camera device is closed.
        private void OnCameraClosed(Object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<EventArgs>(OnCameraClosed), sender, e);
                return;
            }

            // The camera connection is closed. Disable all buttons.
            EnableButtons(false, false);
        }


        // Occurs when a camera starts grabbing.
        private void OnGrabStarted(Object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<EventArgs>(OnGrabStarted), sender, e);
                return;
            }

            // Reset the stopwatch used to reduce the amount of displayed images. The camera may acquire images faster than the images can be displayed.

            CameraGrapWatch.Reset();

            CameraGrapTriggerWatch.Reset();

            // Do not update the device list while grabbing to reduce jitter. Jitter may occur because the GUI thread is blocked for a short time when enumerating.
            updateDeviceListTimer.Stop();

            // The camera is grabbing. Disable the grab buttons. Enable the stop button.
            EnableButtons(false, true);
        }


        // Occurs when an image has been acquired and is ready to be processed.
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper GUI thread.
                // The grab result will be disposed after the event call. Clone the event arguments for marshaling to the GUI thread.
                BeginInvoke(new EventHandler<ImageGrabbedEventArgs>(OnImageGrabbed), sender, e.Clone());
                return;
            }

            try
            {
                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.
             
                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    // Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                        if (!CameraGrapWatch.IsRunning || CameraGrapWatch.ElapsedMilliseconds > 20) //50fps
                       // if (!CameraGrapWatch.IsRunning || CameraGrapWatch.ElapsedMilliseconds > 33) //30fps
                                                                                        //if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 5) //200fps 0.005 ---> ms
                                                                                        // if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 4) //>220fps
                        {

                        OnGrabState = true;

                        


                        Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);

                        grabResult_Width = bitmap.Width;
                        grabResult_Height = bitmap.Height;

                        // Lock the bits of the bitmap.
                        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                        // Place the pointer to the buffer of the bitmap.
                        converter.OutputPixelFormat = PixelType.BGRA8packed;
                        //converter.OutputPixelFormat = PixelType.BayerBG8;

                        IntPtr ptrBmp = bmpData.Scan0;
                        converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult); //Convert Pixel Format................
                        bitmap.UnlockBits(bmpData);


                       
                       
                        if (BlobColorProcessState == false || video_rec_start == true)
                        {
                            m_bitmapColor = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
                        }

                        if (DLProcessState == false)
                        {
                            m_bitmapDL = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
                        }

                        m_bitmapROI = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);


                        FrameCount++;

                        if (FrameCount > 100000) //Restart frame count = 0
                        {
                            FrameCount = 0;
                        }



                        if (ckTriggerMode.Checked == true)  //Add Trigger mode  12/05/2020
                        {

                            FrameTriggerCount++;

                            lbTriggerCount.Text = FrameTriggerCount.ToString("0") + "  /  " + CameraGrapTriggerWatch.ElapsedMilliseconds.ToString() + " Milliseconds";
                            FrameTriggerTimeGrab = CameraGrapTriggerWatch.ElapsedMilliseconds;


                            BlobColorProcessState = true;





                            TriggerBlobColorEvent();



                         


                            CameraGrapTriggerWatch.Restart();



                        }

                        if (PanelCamera.Visible == true)
                        {
                            Bitmap bitmapOld = pictureBox.Image as Bitmap;
                            pictureBox.Image = bitmap;


                            if (bitmapOld != null)
                            {
                                bitmapOld.Dispose();
                            }
                        }


                        double FrameRate = camera.Parameters[PLCamera.ResultingFrameRate].GetValue();

                        CameraGrapWatch.Restart();
                        OnGrabState = false;



                    }
                }
          
            }
            catch (Exception exception)
            {
                btInitialCamera.Enabled = true;
                ShowException(exception);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }


        private void UpdateDeviceList()
        {
            try
            {
                // Ask the camera finder for a list of camera devices.
                List<ICameraInfo> allCameras = CameraFinder.Enumerate();

                ListView.ListViewItemCollection items = deviceListView.Items;

                // Loop over all cameras found.
                foreach (ICameraInfo cameraInfo in allCameras)
                {
                    // Loop over all cameras in the list of cameras.
                    bool newitem = true;
                    foreach (ListViewItem item in items)
                    {
                        ICameraInfo tag = item.Tag as ICameraInfo;

                        // Is the camera found already in the list of cameras?
                        if (tag[CameraInfoKey.FullName] == cameraInfo[CameraInfoKey.FullName])
                        {
                            tag = cameraInfo;
                            newitem = false;
                            break;
                        }
                    }

                    // If the camera is not in the list, add it to the list.
                    if (newitem)
                    {
                        // Create the item to display.
                        ListViewItem item = new ListViewItem(cameraInfo[CameraInfoKey.FriendlyName]);

                        // Create the tool tip text.
                        string toolTipText = "";
                        foreach (KeyValuePair<string, string> kvp in cameraInfo)
                        {
                            toolTipText += kvp.Key + ": " + kvp.Value + "\n";
                        }
                        item.ToolTipText = toolTipText;

                        // Store the camera info in the displayed item.
                        item.Tag = cameraInfo;

                        // Attach the device data.
                        deviceListView.Items.Add(item);
                    }
                }



                // Remove old camera devices that have been disconnected.
                foreach (ListViewItem item in items)
                {
                    bool exists = false;

                    // For each camera in the list, check whether it can be found by enumeration.
                    foreach (ICameraInfo cameraInfo in allCameras)
                    {
                        if (((ICameraInfo)item.Tag)[CameraInfoKey.FullName] == cameraInfo[CameraInfoKey.FullName])
                        {
                            exists = true;
                            break;
                        }
                    }
                    // If the camera has not been found, remove it from the list view.
                    if (!exists)
                    {
                        deviceListView.Items.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                ShowException(exception);
            }
        }

        // Occurs when a camera has stopped grabbing.
        private void OnGrabStopped(Object sender, GrabStopEventArgs e)
        {
            if (InvokeRequired)
            {
                // If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
                BeginInvoke(new EventHandler<GrabStopEventArgs>(OnGrabStopped), sender, e);
                return;
            }

            // Reset the stopwatch.
            CameraGrapWatch.Reset();

            // Re-enable the updating of the device list.
            updateDeviceListTimer.Start();

            // The camera stopped grabbing. Enable the grab buttons. Disable the stop button.
            EnableButtons(true, false);

            // If the grabbed stop due to an error, display the error message.
            if (e.Reason != GrabStopReason.UserRequest)
            {
                // MessageBox.Show("A grab error occured:\n" + e.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lberror.Text = "A grab error occured:\n" + e.ErrorMessage;
            }
        }

        public void ClosingCamera()
        {

            //try
            //{
            //    CreateIfMissing(Application.StartupPath + "\\Camera");
            //    string ReadFileTo = Application.StartupPath + "\\Camera\\CAM_" + CamSN + ".ini";
            //    IniFile iniData = new IniFile(ReadFileTo.ToString());
            //    iniData.IniWriteValue("Exposure", "Value", camera.Parameters[PLCamera.ExposureTimeAbs].GetValue().ToString());
            //}
            //catch
            //{

            //}



            // Close the camera object.
            DestroyCamera();



        }
        private void CreateIfMissing(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch
            {

            }
        }

        private void Frmmain_FormClosing(object sender, FormClosingEventArgs e)
        {

            BlobColorState = false;
            YoloState = false;

            BlobColorProcessState = false;
            DLProcessState = false;


            if (YoloThread != null)
            {
                YoloThread.Abort();
            }

            if (BlobThread != null)
            {
                BlobThread.Abort();
            }

            if (BlobColorThread != null)
            {
                BlobColorThread.Abort();
            }

            _yoloWrapper?.Dispose();

            // Destroy the old camera object.
            if (camera != null)
            {
                DestroyCamera();
            }


        }

    

        private void DrawDLToImage(List<YoloItem> items)
        {


            if (items == null)
            {
                return;
            }

            if(items.Count<=0)
            {
                return;
            }
      

           
            if (bitmapDL == null)
            {
               
                return;
            }

            
            Image image = bitmapDL;// imageDL.Image;
            Font DrawF = new Font("Times New Roman", 30, FontStyle.Bold, GraphicsUnit.Pixel);


           // var oldImage = this.imageDL.Image;

            //this.imageDL.Image = image;

            //oldImage?.Dispose();

           // return;


            if (image == null)
            {
                return;
            }
           // Image imageDraw = imgDiff.Image;
            //using (var canvas = Graphics.FromImage(imageDraw))
            using (var canvas = Graphics.FromImage(image))

            {

                foreach (var item in items)
                {


                    var x = item.X;
                    var y = item.Y;
                    var width = item.Width;
                    var height = item.Height;

                    using (var overlayBrush = new SolidBrush(Color.FromArgb(150, 255, 0, 0)))
                    using (var pen = this.GetBrush(item.Confidence, image.Width))
                    {



                        int Py = y - DrawF.Height;
                        int Px = x;



                        if ((item.Confidence) > (DLConfidence/100))  //-->//item.Type
                        {
                            try
                            {

                                if (item.Type != ObjectDLSelect)
                                {
                                    if (ViewAllDataSet == true)
                                    {
                                        canvas.DrawRectangle(pen, x, y, width, height);
                                        canvas.DrawString(item.Type + "  " + (item.Confidence * 100).ToString("0.00") + " %", DrawF, Brushes.White, Px, Py);
                                    }
                                    //canvas.FillRectangle(overlayBrush, x, y, width, height);
                                }
                                else
                                {
                                    canvas.DrawRectangle(pen, x, y, width, height);
                                    canvas.DrawString(item.Type + "  " + (item.Confidence * 100).ToString("0.00") + " %", DrawF, Brushes.White, Px, Py);
                                }

                                
                               
                                canvas.Flush();
                            }
                            catch
                            {


                            }
                        }


                    }
                }
            }


            //try
            //{ 
            var oldImage = this.imageDL.Image;

            this.imageDL.Image = image;

            oldImage?.Dispose();
            //}
            //catch
            //{

            //}


            //image.Dispose(); // Release the memory.


            //if (bitmapDLOld != null)
            //{
            //    // Dispose the bitmap.
            //    bitmapDLOld.Dispose();
            //}

        }

   

        public void DeleteDir(string Dir)
        {
            string[] Files = Directory.GetFiles(Dir);
            //string[] dirs = Directory.GetDirectories(Dir);

            foreach (string file in Files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            Directory.Delete(Dir, false);

          
        }
        public void StartProcess()
        {



            string DataResultProduct = DataResultPath + "\\" + txtProductName.Text.Trim();


            if (Directory.Exists(DataResultProduct))
            {
                try
                {
                    DeleteDir(DataResultProduct);
                }
                catch
                {

                }
               
            }


            if ((grabResult_Width > 0) && (grabResult_Height > 0))
            {
                m_bitmapDL = new Bitmap(grabResult_Width, grabResult_Height, PixelFormat.Format32bppRgb);
                m_bitmapColor = new Bitmap(grabResult_Width, grabResult_Height, PixelFormat.Format32bppRgb);
                m_bitmapROI = new Bitmap(grabResult_Width, grabResult_Height, PixelFormat.Format32bppRgb);
            }

            m_bitmapDL = null;
            m_bitmapColor = null;
            m_bitmapROI = null;

        

            FirstFrame = false;

            LineCount = 0;
            countdetect = 0;

            FrameCount = 0;
            FrameCount_Old = 0;

            FrameTriggerCount = 0;

            ObjectCount1 = 0;
            ObjectCount2 = 0;
            ObjectCount3 = 0;

            ObjectCountAll = 0;
            RotateBufferCount = 0;

            Count_old = 9999;


            lbObjectCount.Text = "0";

            lbdiffCount.BringToFront();
            lbdiffCount.Text = txtTargetCount.Text;

            Old_FrameProcess = 0;
            Old_Area =0;
            Old_RectangleProcess = new Rectangle(0, 0, 0, 0);
            Old_case = 0;


            try
            {
                DLConfidence = double.Parse(txtConfidence.Text)/100;
            }
            catch
            {
                DLConfidence = 0.6;
            }

            ObjectDLSelect = cbObjectList.Text;


            blobsCount = new List<Blob>();



            imagecrop1.Image = null;
            imagecrop1.Invalidate();

            imagecrop2.Image = null;
            imagecrop2.Invalidate();

            imagecrop3.Image = null;
            imagecrop3.Invalidate();

            imagecrop4.Image = null;
            imagecrop4.Invalidate();



            imagecrop1.Zoom = intnumZoomBuffer;
            imagecrop1.CenterToImage();
            imagecrop2.Zoom = intnumZoomBuffer;
            imagecrop2.CenterToImage();
            imagecrop3.Zoom = intnumZoomBuffer;
            imagecrop3.CenterToImage();
            imagecrop4.Zoom = intnumZoomBuffer;
            imagecrop4.CenterToImage();

            for (int i = 0; i < lbimageinfo.Length; i++)
            {
                
                lbimageinfo[i].Text = "Count:";
            }


            


            txtResultCount.Text = "";

            if (ckColorTypeDetect.Checked == true)
            {
                EnableColorTypeDetect = true;
            }
            else
            {
                EnableColorTypeDetect = false;
            }


            if (ckDLTrainingSave.Checked == true)
            {
                EnableDLTrainingSave = true;
            }
            else
            {
                EnableDLTrainingSave = false;
            }

            if (ckDLProcess.Checked == true)
            {
                EnableDLCheck = true;
            }
            else
            {
                EnableDLCheck = false;
            }

            
            DetectCaseZero = false;
            DLCheckResult = true;

            CircleProgressbar.Value = 0;


            PanelAlarm.Visible =false;
            PanelAlarm.BringToFront();
           // PanelAlarm.Dock = DockStyle.Fill;
            lbAlarmInformation.Text = "";

            txtInfo.Text = "Running......";

            frmDigitalIO.UpdateOutput(5, false);


            UpdateDilateErode();


        }
        private void DrawLeastOneObjectCrossedTheLine(Point[] crossingLine, Color color, int LineWidth)
        {

            if (imgDiff.Image == null)
            {
                return;
            }

            try
            {
                Image image = imgDiff.Image;


                using (var canvas = Graphics.FromImage(image))
                {

                    canvas.DrawLine(new Pen(color, LineWidth), crossingLine[0].X, crossingLine[0].Y, crossingLine[1].X, crossingLine[1].Y);
                    canvas.Flush();


                   
                }

            }
            catch
            {

            }
        }



        public void ObjectProcessTrigger(bool ResuleEnable, bool DLEnable)
        {


            if (m_bitmapColor == null)
            {
                return;
            }





            currentFrameBlob = new Emgu.CV.Image<Bgr, Byte>(m_bitmapColor).Clone();
            currentFrameBlobROI = new Emgu.CV.Image<Bgr, Byte>(m_bitmapColor).Clone();

            


            Rectangle roi = new Rectangle(0 + intnumOffsetLeft, 0, currentFrameBlob.Width - intnumOffsetRight, currentFrameBlob.Height); // set the roi
            currentFrameBlobROI.ROI = roi;


            if (currentFrameBlobROI == null)
            {
                return;
            }



            Mat structuringElement3x3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            Mat structuringElement5x5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
            Mat structuringElement7x7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(-1, -1));
            Mat structuringElement9x9 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(9, 9), new Point(-1, -1));
            Mat structuringElement12x12 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(12, 12), new Point(-1, -1));







            BlobColorProcessState = true;

            Mat imgFramecurrent = (Mat)currentFrameBlobROI.Mat.Clone();

          

            Mat YellowimgMark = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat GrayimgMark = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat WhiteimgMark = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat BlueimgMark = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);

            Mat BinaryimgMark = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);

            Mat YellowimgOutCvtColor = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat YellowimgThresh = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);

            Mat GrayimgOutCvtColor = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat GrayimgThresh = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);

            Mat WhiteimgOutCvtColor = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat WhiteimgThresh = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);

            Mat BlueimgOutCvtColor = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);
            Mat BlueimgThresh = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 1);


            Mat imageConvexHulls = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 3);
            imageContours = new Mat(imgFramecurrent.Size, DepthType.Cv8U, 3);

            List<Blob> currentFrameBlobs = new List<Blob>();  //object ทั้งหมดโดยรามสีเทา

            List<Blob> currentFrameBlobsColorYellow = new List<Blob>();
            List<Blob> currentFrameBlobsColorWhite = new List<Blob>();
            List<Blob> currentFrameBlobsColorGray = new List<Blob>();
            List<Blob> currentFrameBlobsColorBlue = new List<Blob>();

            List<Blob> currentFrameBlobsOverColor = new List<Blob>();




            #region Color Mode


            if (EnableYellowValue == true)
            {
                if (YellowColorMode)// true->HSV
                {
                    CvInvoke.CvtColor(imgFramecurrent, YellowimgOutCvtColor, ColorConversion.Bgr2Hsv);

                    CvInvoke.InRange(YellowimgOutCvtColor, new ScalarArray(new MCvScalar(HminYellow, SminYellow, VminYellow)),
                                         new ScalarArray(new MCvScalar(HmaxYellow, SmaxYellow, VmaxYellow)), YellowimgThresh);
                }
                else
                {
                    CvInvoke.InRange(imgFramecurrent, new ScalarArray(new MCvScalar(BminYellow, GminYellow, RminYellow)),
                                        new ScalarArray(new MCvScalar(BmaxYellow, GmaxYellow, RmaxYellow)), YellowimgThresh);
                }
            }


            if (EnableGrayValue == true)
            {
                if (GrayColorMode)// true->HSV
                {
                    CvInvoke.CvtColor(imgFramecurrent, GrayimgOutCvtColor, ColorConversion.Bgr2Hsv);

                    CvInvoke.InRange(GrayimgOutCvtColor, new ScalarArray(new MCvScalar(HminGray, SminGray, VminGray)),
                                         new ScalarArray(new MCvScalar(HmaxGray, SmaxGray, VmaxGray)), GrayimgThresh);
                }
                else
                {

                    CvInvoke.InRange(imgFramecurrent, new ScalarArray(new MCvScalar(BminGray, GminGray, RminGray)),
                                        new ScalarArray(new MCvScalar(BmaxGray, GmaxGray, RmaxGray)), GrayimgThresh);

                }
            }


            if (EnableWhiteValue == true)
            {
                if (WhiteColorMode)// true->HSV
                {
                    CvInvoke.CvtColor(imgFramecurrent, WhiteimgOutCvtColor, ColorConversion.Bgr2Hsv);

                    CvInvoke.InRange(WhiteimgOutCvtColor, new ScalarArray(new MCvScalar(HminWhite, SminWhite, VminWhite)),
                                         new ScalarArray(new MCvScalar(HmaxWhite, SmaxWhite, VmaxWhite)), WhiteimgThresh);
                }
                else
                {

                    CvInvoke.InRange(imgFramecurrent, new ScalarArray(new MCvScalar(BminWhite, GminWhite, RminWhite)),
                                        new ScalarArray(new MCvScalar(BmaxWhite, GmaxWhite, RmaxWhite)), WhiteimgThresh);

                }
            }


            if (EnableBlueValue == true)
            {
                if (BlueColorMode)// true->HSV
                {
                    CvInvoke.CvtColor(imgFramecurrent, BlueimgOutCvtColor, ColorConversion.Bgr2Hsv);

                    CvInvoke.InRange(BlueimgOutCvtColor, new ScalarArray(new MCvScalar(HminBlue, SminBlue, VminBlue)),
                                         new ScalarArray(new MCvScalar(HmaxBlue, SmaxBlue, VmaxBlue)), BlueimgThresh);
                }
                else
                {

                    CvInvoke.InRange(imgFramecurrent, new ScalarArray(new MCvScalar(BminBlue, GminBlue, RminBlue)),
                                        new ScalarArray(new MCvScalar(BmaxBlue, GmaxBlue, RmaxGray)), BlueimgThresh);

                }
            }

            #endregion Color Mode




            #region Mark BitwiseAnd
            if (ckMaskDisplay.Checked)
            {


                if (EnableYellowValue == true)
                {
                    CvInvoke.BitwiseAnd(imgFramecurrent, imgFramecurrent, YellowimgMark, YellowimgThresh);
                }
                if (EnableGrayValue == true)
                {
                    CvInvoke.BitwiseAnd(imgFramecurrent, imgFramecurrent, GrayimgMark, GrayimgThresh);
                }
                if (EnableWhiteValue == true)
                {
                    CvInvoke.BitwiseAnd(imgFramecurrent, imgFramecurrent, WhiteimgMark, WhiteimgThresh);
                }
                if (EnableBlueValue == true)
                {
                    CvInvoke.BitwiseAnd(imgFramecurrent, imgFramecurrent, BlueimgMark, BlueimgThresh);
                }

                if (ThresholdColorView == 1)
                {
                    imgThresholdColor.Image = BlueimgMark;
                }
                else if (ThresholdColorView == 2)
                {
                    imgThresholdColor.Image = WhiteimgMark;
                }
                else if (ThresholdColorView == 3)
                {
                    imgThresholdColor.Image = GrayimgMark;
                }
                else if (ThresholdColorView == 4)
                {
                    imgThresholdColor.Image = YellowimgMark;
                }

            }
            else
            {
                if (ThresholdColorView == 1)
                {
                    imgThresholdColor.Image = BlueimgThresh;
                }
                else if (ThresholdColorView == 2)
                {
                    imgThresholdColor.Image = WhiteimgThresh;
                }
                else if (ThresholdColorView == 3)
                {
                    imgThresholdColor.Image = GrayimgThresh;
                }
                else if (ThresholdColorView == 4)
                {
                    imgThresholdColor.Image = YellowimgThresh;
                }



            }
            #endregion Mark BitwiseAnd



            #region Dilate & Erode





            if (EnableYellowValue == true)
            {

                for (int i = 0; i < NumDilate; i++)
                {
                    CvInvoke.Dilate(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
                for (int i = 0; i < NumErode; i++)
                {
                    CvInvoke.Erode(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
            }

            if (EnableGrayValue == true)
            {
                for (int i = 0; i < NumDilate; i++)
                {
                    CvInvoke.Dilate(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
                for (int i = 0; i < NumErode; i++)
                {
                    CvInvoke.Erode(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
            }

            if (EnableWhiteValue == true)
            {
                for (int i = 0; i < NumDilate; i++)
                {
                    CvInvoke.Dilate(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
                for (int i = 0; i < NumErode; i++)
                {
                    CvInvoke.Erode(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }

            }

            if (EnableBlueValue == true)
            {

                for (int i = 0; i < NumDilate; i++)
                {
                    CvInvoke.Dilate(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }
                for (int i = 0; i < NumErode; i++)
                {
                    CvInvoke.Erode(BlueimgThresh, BlueimgThresh, structuringElement5x5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0, 0, 0));
                }

            }


            #endregion Dilate & Erode


            #region FindContours

            Mat imgThreshCopyYellow = (Mat)YellowimgThresh.Clone();
            Mat imgThreshCopyGray = (Mat)GrayimgThresh.Clone();
            Mat imgThreshCopyWhite = (Mat)WhiteimgThresh.Clone();
            Mat imgThreshCopyBlue = (Mat)BlueimgThresh.Clone();



            bool YellowContoursTrack = false;
            bool GrayContoursTrack = false;
            bool WhiteContoursTrack = false;
            bool BlueContoursTrack = false;

            double percentageobstart = 4.0;

            if (EnableYellowValue == true)
            {
                VectorOfVectorOfPoint YellowContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(imgThreshCopyYellow, YellowContours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);


                CvInvoke.DrawContours(imageContours, YellowContours, -1, SCALAR_YELLOW, -1);

                int tempVarConvex = YellowContours.Size;


                for (int i = 0; i < tempVarConvex; i++)
                {

                    Blob possibleBlob = new Blob(YellowContours[i]);


                    if ((possibleBlob.ContourArea >= AreaAceptmin) && (possibleBlob.ContourArea <= AreaAceptmax))//&& (possibleBlob.currentBoundingRect.X > (verticalLinePositionStart + possibleBlob.currentBoundingRect.Width / percentageobstart)))
                    {

                        CvInvoke.Rectangle(imageContours, possibleBlob.currentBoundingRect, SCALAR_RED, 3);

                        currentFrameBlobsColorYellow.Add(possibleBlob);//เก็บค่าไว้ไปตรวจสอบว่า object ทีวิ่งเข้ามาเป็นสีอะไร

                        YellowContoursTrack = true;


                        if (ColorYellowSelect == false)//ถ้าไม่ได้เลือกสี เหลืองไว้
                        {
                            currentFrameBlobsOverColor.Add(possibleBlob);
                        }
                        else
                        {
                            currentFrameBlobs.Add(possibleBlob);

                        }

                    }
                }
            }

            if (EnableGrayValue == true)
            {
                VectorOfVectorOfPoint GrayContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(imgThreshCopyGray, GrayContours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(imageContours, GrayContours, -1, SCALAR_GRAY, -1);

                int tempVarConvex = GrayContours.Size;

                for (int i = 0; i < tempVarConvex; i++)
                {

                    Blob possibleBlob = new Blob(GrayContours[i]);

                    if ((possibleBlob.ContourArea >= AreaAceptmin) && (possibleBlob.ContourArea <= AreaAceptmax))// && (possibleBlob.currentBoundingRect.X > (verticalLinePositionStart + possibleBlob.currentBoundingRect.Width / percentageobstart)))
                    {

                        CvInvoke.Rectangle(imageContours, possibleBlob.currentBoundingRect, SCALAR_RED, 3);

                        currentFrameBlobsColorGray.Add(possibleBlob);//เก็บค่าไว้ไปตรวจสอบว่า object ทีวิ่งเข้ามาเป็นสีอะไร
                        GrayContoursTrack = true;


                        if (ColorGraySelect == false)//ถ้าไม่ได้เลือกสี เหลืองไว้
                        {
                            currentFrameBlobsOverColor.Add(possibleBlob);
                        }
                    }
                }
            }

            if (EnableWhiteValue == true)
            {
                VectorOfVectorOfPoint WhiteContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(imgThreshCopyWhite, WhiteContours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(imageContours, WhiteContours, -1, SCALAR_WHITE, -1);

                int tempVarConvex = WhiteContours.Size;

                for (int i = 0; i < tempVarConvex; i++)
                {

                    Blob possibleBlob = new Blob(WhiteContours[i]);

                    if ((possibleBlob.ContourArea >= AreaAceptmin) && (possibleBlob.ContourArea <= AreaAceptmax))// && (possibleBlob.currentBoundingRect.X > (verticalLinePositionStart + possibleBlob.currentBoundingRect.Width / percentageobstart)))
                    {
                        CvInvoke.Rectangle(imageContours, possibleBlob.currentBoundingRect, SCALAR_RED, 3);

                        currentFrameBlobsColorWhite.Add(possibleBlob);//เก็บค่าไว้ไปตรวจสอบว่า object ทีวิ่งเข้ามาเป็นสีอะไร
                        WhiteContoursTrack = true;


                        if (ColorWhiteSelect == false)//ถ้าไม่ได้เลือกสี เหลืองไว้
                        {
                            currentFrameBlobsOverColor.Add(possibleBlob);
                        }
                        else
                        {
                            currentFrameBlobs.Add(possibleBlob);

                        }

                    }
                }
            }

            if (EnableBlueValue == true)
            {



                VectorOfVectorOfPoint BlueContours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(imgThreshCopyBlue, BlueContours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                CvInvoke.DrawContours(imageContours, BlueContours, -1, SCALAR_BLUE, -1); //Draw fill

                int tempVarConvex = BlueContours.Size;


                for (int i = 0; i < tempVarConvex; i++)
                {

                    Blob possibleBlob = new Blob(BlueContours[i]);

                    if ((possibleBlob.ContourArea >= AreaAceptmin) && (possibleBlob.ContourArea <= AreaAceptmax))// && (possibleBlob.currentBoundingRect.X > (verticalLinePositionStart + possibleBlob.currentBoundingRect.Width / percentageobstart))) //30/10/2019
                    {

                        CvInvoke.Rectangle(imageContours, possibleBlob.currentBoundingRect, SCALAR_RED, 3);

                        currentFrameBlobsColorBlue.Add(possibleBlob);//เก็บค่าไว้ไปตรวจสอบว่า object ทีวิ่งเข้ามาเป็นสีอะไร
                        BlueContoursTrack = true;


                        if (ColorBlueSelect == false)//ถ้าไม่ได้เลือกสี
                        {

                            currentFrameBlobsOverColor.Add(possibleBlob);
                        }
                        else
                        {
                            currentFrameBlobs.Add(possibleBlob);

                        }
                    }
                }
            }

            #endregion FindContours


            if (PanelBlob.Visible == true)
            {
                ImageContoursBox.Image = imageContours; //วาด  Contours ทั้งหมด

            }


            string sdate = DateTime.Now.ToString();
            DateTime time = DateTime.Parse(sdate);
            string resultdate = time.ToString("dd/MM/yyyy", DatetimeSystem_US);
            string resulttime = time.ToString("HH:mm:ss", DatetimeSystem_US);
            string resultdatetime = time.ToString("dd/MM/yyyy HH:mm:ss", DatetimeSystem_US);

            string informationall = "";
            string LastInfo = "";
            List<int> count_Track = new List<int>();


            if (ResuleEnable == true)
            {

                if (currentFrameBlobs.Count > 0)   //Calculate Area , Count , DL type
                {

                    for (int i = 0; i < currentFrameBlobs.Count; i++)
                    {
                        int CaseDectect = 0;
                        bool StateContourAreaZero = false;

                        double AcceptContourArea = currentFrameBlobs[i].ContourArea;// currentFrameBlobs[i].AcceptContourArea[0];

                        double AreaMin = currentFrameBlobs[i].AreaMin;
                        double AreaMax = currentFrameBlobs[i].AreaMax;


                        currentFrameBlobs[i].AcceptBoundingRect = currentFrameBlobs[i].currentBoundingRect;

                        if (AcceptContourArea == 0) //Cace จากมีการแยก objectในขณะที่ผ่านเส้นนับ
                        {
                            CaseDectect = 0;
                            DetectCaseZero = false;
                            StateContourAreaZero = true;
                        }
                        else if (AcceptContourArea >= AreaAceptCase10)
                        {
                            CaseDectect = 10;

                        }
                        else if (AcceptContourArea >= AreaAceptCase9)
                        {
                            CaseDectect = 9;

                        }
                        else if (AcceptContourArea >= AreaAceptCase8)
                        {
                            CaseDectect = 8;

                        }
                        else if (AcceptContourArea >= AreaAceptCase7)
                        {
                            CaseDectect = 7;

                        }
                        else if (AcceptContourArea >= AreaAceptCase6)
                        {
                            CaseDectect = 6;

                        }
                        else if (AcceptContourArea >= AreaAceptCase5)
                        {
                            CaseDectect = 5;

                        }
                        else if (AcceptContourArea >= AreaAceptCase4)
                        {
                            CaseDectect = 4;

                        }
                        else if (AcceptContourArea >= AreaAceptCase3)
                        {
                            CaseDectect = 3;

                        }

                        else if (AcceptContourArea >= AreaAceptCase2)
                        {
                            CaseDectect = 2;

                        }

                        else if (AcceptContourArea >= AreaAceptCase1)
                        {
                            CaseDectect = 1;

                        }
                        else  //Out Off Area
                        {
                            CaseDectect = 0;
                            if (AcceptContourArea == 0)
                            {
                                CaseDectect = 0;
                                DetectCaseZero = false;
                                StateContourAreaZero = true;
                            }
                            else
                            {
                                DetectCaseZero = true;
                            }
                        }

                        currentFrameBlobs[i].AreaAceptCase = CaseDectect;
                        ObjectCountAll += currentFrameBlobs[i].AreaAceptCase;
                        currentFrameBlobs[i].count_Track = ObjectCountAll;
                        count_Track.Add(ObjectCountAll);

                        if (CaseDectect == 0)
                        {
                            informationall += "  " + @"Count : Null";//rentFrameBlobs[i].count_Track.ToString();
                        }
                        else
                        {
                            informationall += "  " + @"Count : " + currentFrameBlobs[i].count_Track.ToString();
                        }

                        informationall += "  " + @"Area = " + AcceptContourArea.ToString("0.0");
                        informationall += "  " + @"Case = " + currentFrameBlobs[i].AreaAceptCase.ToString("0");
                        informationall += "  " + @"Frame = " + FrameTriggerCount.ToString("0");
                        informationall += "  " + @"Watch = " + CameraGrapTriggerWatch.ElapsedMilliseconds.ToString();
                        informationall += "  " + @"Timestamp : " + resulttime + "\r\n";

                    }

                    LastInfo += " " + @"Count Sum : " + ObjectCountAll.ToString() + "\r\n";
                    LastInfo += " " + "\r\n";
                    LastInfo += " " + @"Count :" + count_Track.Min().ToString("0") + " - " + count_Track.Max().ToString("0") + "\r\n";
                    LastInfo += " " + "\r\n";
                    LastInfo += " " + @"Object : " + currentFrameBlobs.Count.ToString() + "\r\n"; ;


                    txtResultCount.Text += informationall;


                }

            }




            //DL Process.......

            #region Check DL existingBlobs


            if (currentFrameBlobs.Count > 0)
            {

             
            }
            

            if (DLEnable  == true)//DL enable check from product database
            {


                Image<Bgr, Byte> ImageDL = currentFrameBlobROI.Copy();
                bitmapDL = ImageDL.ToBitmap();
              
                   if (bitmapDL != null)
                   {

                   //  if (OnGrabState == false) //กัน memory ทับซ้อนกัน รอจนกว่ามีการวาง memory เสร็จ OnGrabState==false
                   //  {


                        current_items = DetectObject(bitmapDL);

                        this.Invoke(new Action(() =>
                        {
                            if ((current_items != null) && (current_items.Count > 0))
                            {
                                lbSpeedDLResult.Text = $"DL processed in " + DLSpeedUpdate + " ms Count = " + current_items.Count.ToString();
                            }
                            else
                            {
                                lbSpeedDLResult.Text = $"DL processed in " + DLSpeedUpdate + " ms ";
                            }


                        }));
                    // }

                 }
              }



            if (DLEnable == true)//DL enable check from product database
            {
                foreach (Blob existingBlob in currentFrameBlobs)
                {


                    Rectangle currentBoundingRect = existingBlob.currentBoundingRect;

                    Point currentCenter = new Point();

                    currentCenter.X = Convert.ToInt32((double)(currentBoundingRect.X + currentBoundingRect.X + currentBoundingRect.Width) / 2.0);
                    currentCenter.Y = Convert.ToInt32((double)(currentBoundingRect.Y + currentBoundingRect.Y + currentBoundingRect.Height) / 2.0);



                    if (current_items != null)  //ตรวจสอบ DL Identify  เพื่อระบุว่าเป็น Object อะไร
                    {

                        foreach (var item in current_items)
                        {


                            var x = item.X;
                            var y = item.Y;
                            var width = item.Width;
                            var height = item.Height;


                            Point PCheck = new Point();

                            PCheck.X = Convert.ToInt32((double)(item.X + item.X + item.Width) / 2.0);
                            PCheck.Y = Convert.ToInt32((double)(item.Y + item.Y + item.Height) / 2.0);

                            Rectangle RectangleDL = new Rectangle(item.X, item.Y, item.Width, item.Height);


                            try
                            {
                                bool Retexist = FindPoint(currentBoundingRect.X, currentBoundingRect.Y, currentBoundingRect.X + currentBoundingRect.Width, currentBoundingRect.Y + currentBoundingRect.Height, PCheck.X, PCheck.Y);
                                bool retIntersect = currentBoundingRect.IntersectsWith(RectangleDL);

                                if ((Retexist == true) || (retIntersect == true)) //ถ้ามีการนับ ณ จุดที่ Object Detect พอดี
                                {

                                    if (ObjectDLSelect == item.Type)
                                    {
                                        if (item.Confidence > (DLConfidence/100))
                                        {
                                            existingBlob.DL_Type.Add(item.Type);
                                            existingBlob.DL_Confidence.Add((item.Confidence * 100).ToString("0.00"));

                                            if (existingBlob.DL_Type.Count >= DL_Type_Count_Accept)
                                            {
                                                existingBlob.DL_Type_ACCEPT = item.Type;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        DLCheckResult = false;
                                    }

                                }

                            }
                            catch
                            {


                            }

                        }
                    }


                }
            }




            #endregion Check DL  existingBlobs




            Mat imgFrame2Copy = (Mat)imgFramecurrent.Clone();


            int Image_Width = imgFramecurrent.Width;
            int Image_Height = imgFramecurrent.Height;


            Image<Bgr, Byte> img2 = imgFrame2Copy.ToImage<Bgr, Byte>();

            imgDiff.Image = img2.ToBitmap();

            Image imageDraw = imgDiff.Image;//imageBlob.Image;

            Font DrawAreaF = new Font("Times New Roman", 24, FontStyle.Bold, GraphicsUnit.Pixel);
            Font DrawCountF = new Font("Times New Roman", 30, FontStyle.Bold, GraphicsUnit.Pixel);
            Font DrawF = new Font("Times New Roman", 30, FontStyle.Bold, GraphicsUnit.Pixel);
            Font DrawF_DL = new Font("Times New Roman", 20, FontStyle.Bold, GraphicsUnit.Pixel);


            using (var canvas = Graphics.FromImage(imageDraw))
            {


                var size = Image_Width / 300;



                for (int i = 0; i < currentFrameBlobs.Count; i++)
                {

                    Rectangle BoundingRect = currentFrameBlobs[i].currentBoundingRect;

                    using (var penDashStyle = new Pen(Brushes.White, size))
                    using (var penCount = new Pen(Brushes.Green, size))
                    using (var penGreen = new Pen(Brushes.Green, size))
                    using (var penRed = new Pen(Brushes.Red, size))
                    {


                        Rectangle BoundingRect2 = currentFrameBlobs[i].currentBoundingRect;

                        penCount.DashStyle = DashStyle.Dot;
                        canvas.DrawRectangle(penCount, BoundingRect2.X, BoundingRect2.Y, BoundingRect2.Width, BoundingRect2.Height);





                        try
                        {

                            string CountStr = currentFrameBlobs[i].count_Track.ToString();
                            canvas.DrawString(CountStr, DrawCountF, Brushes.Yellow, BoundingRect.X, BoundingRect.Y);

                            string AreaStr = @"Area = " + currentFrameBlobs[i].ContourArea.ToString();
                            canvas.DrawString(AreaStr, DrawAreaF, Brushes.White, BoundingRect.X, BoundingRect.Y + (BoundingRect.Height / 2));

                            string CaseStr = @"Case = " + currentFrameBlobs[i].AreaAceptCase.ToString();
                            canvas.DrawString(CaseStr, DrawAreaF, Brushes.Pink, BoundingRect.X, BoundingRect.Y + (BoundingRect.Height / 2) + (DrawAreaF.Height));




                            if (DLEnable == true)//DL enable check from product database
                            {
                                if (currentFrameBlobs[i].DL_Type.Count > 0)
                                {
                                    string information3 = @"DL_TYPE =" + currentFrameBlobs[i].DL_Type.Last();
                                    canvas.DrawString(information3, DrawF_DL, Brushes.OrangeRed, BoundingRect.X, BoundingRect.Y + (BoundingRect.Height / 3));
                                }
                            }


                            canvas.Flush();



                        }
                        catch
                        {


                        }


                    }


                }




            }


            if (currentFrameBlobs.Count > 0)   
            {

                imagecrop1.Image = imagecrop2.Image;
                imagecrop2.Image = imagecrop3.Image;
                imagecrop3.Image = imagecrop4.Image;
                imagecrop4.Image = imageDraw;


                lbimageinfo1.Text = lbimageinfo2.Text;
                lbimageinfo2.Text = lbimageinfo3.Text;
                lbimageinfo3.Text = lbimageinfo4.Text;
                lbimageinfo4.Text = LastInfo;
                
                if (txtProductName.Text.Trim() != "")
                {

                
                    string ResultNameDatetime = time.ToString("ddMMyyyy_HHmm", DatetimeSystem_US) + "_" +  ObjectCountAll.ToString() + "_" + FrameTriggerCount.ToString() + ".jpg";
                   
                    string DataResultProduct = DataResultPath + "\\" + txtProductName.Text.Trim();


                    if (!Directory.Exists(DataResultProduct))
                    {
                        CreateIfMissing(DataResultProduct);
                    }

                    if (ckSaveResultImage.Checked == true)
                    {
                        try
                        {
                            imageDraw.Save(DataResultProduct + "\\" + ResultNameDatetime, ImageFormat.Jpeg);
                        }
                        catch
                        {

                        }
                    }




                }
            }

            
            imgDiff.Invalidate();


            if (currentFrameBlobsOverColor.Count > 0)
            {
                ColorTypeDetectState = true;
            }


            currentFrameBlobs.Clear();

            currentFrameBlobsColorYellow.Clear();
            currentFrameBlobsColorWhite.Clear();
            currentFrameBlobsColorGray.Clear();
            currentFrameBlobsColorBlue.Clear();

            currentFrameBlobsOverColor.Clear();

            BlobColorProcessState = false;

            lbObjectCount.Text = ObjectCountAll.ToString();
            lbObjectCount2.Text = ObjectCountAll.ToString();
            lbObjectAllCaseCount.Text = blobsCount.Count.ToString();


        }

     
        private enum EndpointStyle
        {
            None,
            ArrowHead,
            Fletching
        }


        private void DrawPoint(Graphics gr, PointF pt, Brush brush, Pen pen)
        {
            const int RADIUS = 6;
            gr.FillEllipse(brush,
                pt.X - RADIUS, pt.Y - RADIUS,
                2 * RADIUS, 2 * RADIUS);
            gr.DrawEllipse(pen,
                pt.X - RADIUS, pt.Y - RADIUS,
                2 * RADIUS, 2 * RADIUS);
        }

        double Map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }



        private int FindLineCircleIntersections(float cx, float cy, float radius,
         PointF point1, PointF point2, out PointF intersection1, out PointF intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }


        private void FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4,
         out bool lines_intersect, out bool segments_intersect,
         out PointF intersection, out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);
            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }
    

        public bool FindPoint(int x1, int y1, int x2, int y2, int x, int y)
        {
            if (x > x1 && x < x2 && y > y1 && y < y2)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

      

       
      
        public byte[] imageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }


        private void Detect(Bitmap Image)
        {
            if (this._yoloWrapper == null)
            {
                return;
            }


            var sw = new Stopwatch();
            sw.Start();


            List<YoloItem> items;

            byte[] imagebyte;

            try
            {

                if (Image != null)
                {

                    imagebyte = imageToByteArray(Image);
                  

                    DLProcessState = true;

                    items = _yoloWrapper.Detect(imagebyte).ToList();


                    sw.Stop();
                   

                    this.DrawBorder2Image(items);

                    DLProcessState = false;

                }

            }
            catch
            {
                DLProcessState = false;
            }
        }





        public List<YoloItem> DetectObject(Bitmap Image)
        {

            List<YoloItem> items = null;

            if (this._yoloWrapper == null)
            {
                return items;
            }


            var sw = new Stopwatch();
            sw.Start();


            byte[] imagebyte;

            try
            {

                if (Image != null)
                {


                  
                    imagebyte = imageToByteArray(Image);
                  

                    DLProcessState = true;

                    items = _yoloWrapper.Detect(imagebyte).ToList();


                    sw.Stop();

                    DLSpeedUpdate = sw.Elapsed.TotalMilliseconds.ToString("0");

                    //****CURRENTLY IN USE ELSEWHERE
                    //will run on the UI thread. Something like this
                    this.Invoke(new Action(() => 
                    {
                        DrawDLToImage(items);
                    }));
                 

                    DLProcessState = false;

                   

                }

            }
            catch
            {
                DLProcessState = false;
               
            }

            return items;
        }


        
        public List<YoloItem> ReDLDetectObject(Bitmap Image)
        {

            List<YoloItem> items = null;

            if (this._yoloWrapper == null)
            {
                return items;
            }


            var sw = new Stopwatch();
            sw.Start();


            byte[] imagebyte;

            try
            {

                if (Image != null)
                {



                    imagebyte = imageToByteArray(Image);


                   // DLProcessState = true;

                    items = _yoloWrapper.Detect(imagebyte).ToList();


                    sw.Stop();

                    DLSpeedUpdate = sw.Elapsed.TotalMilliseconds.ToString("0");

                    //****CURRENTLY IN USE ELSEWHERE
                    //will run on the UI thread. Something like this
                   // this.Invoke(new Action(() =>
                   // {
                   //     DrawDLToImage(items);
                   // }));


                   // DLProcessState = false;



                }

            }
            catch
            {
                DLProcessState = false;

            }

            return items;
        }
        private void Detect(Emgu.CV.Image<Bgr, Byte> currentFrame)
        {
            if (this._yoloWrapper == null)
            {
                return;
            }


            var sw = new Stopwatch();
            sw.Start();
            List<YoloItem> items;
            byte[] imagebyte;

            try
            {

                //if (m_bitmap_process != null)
                // {


                //BitmapData bmpData = m_bitmap_process.LockBits(new Rectangle(0, 0, m_bitmap_process.Width, m_bitmap_process.Height), ImageLockMode.ReadWrite, m_bitmap_process.PixelFormat);



                //picProcess.Image = m_bitmap_process;

                   imagebyte = currentFrame.Bytes;// ImageToByte(m_bitmap_process);

                   items = _yoloWrapper.Detect(imagebyte).ToList();


                    sw.Stop();
                  //  txtInfo.Text = $"Result [ processed in {sw.Elapsed.TotalMilliseconds:0} ms ]";

                   // this.dataGridViewResult.DataSource = items;
                    this.DrawBorder2Image(items);





                  //  m_bitmap_process.UnlockBits(bmpData);

                   
                  //  }
            }
            catch
            {

            }
        }


        private void DrawBorder2Image(List<YoloItem> items, YoloItem selectedItem = null)
        {

           
            Image  image = m_bitmap;

            Font DrawF = new Font("Times New Roman", 30, FontStyle.Bold, GraphicsUnit.Pixel);

            using (var canvas = Graphics.FromImage(image))
            {
                // Modify the image using g here... 
                // Create a brush with an alpha value and use the g.FillRectangle function
                foreach (var item in items)
                {

                    //item.Type
                    var x = item.X;
                    var y = item.Y;
                    var width = item.Width;
                    var height = item.Height;

                    using (var overlayBrush = new SolidBrush(Color.FromArgb(150, 255, 255, 102)))
                    using (var pen = this.GetBrush(item.Confidence, image.Width))
                    {
                        if (item.Equals(selectedItem))
                        {
                            canvas.FillRectangle(overlayBrush, x, y, width, height);
                        }


                        int Py = y - DrawF.Height;
                        int Px = x;

                        double Confidence = double.Parse(txtConfidence.Text);

                        if (item.Confidence > Confidence)
                        {
                            try
                            {
                                canvas.DrawRectangle(pen, x, y, width, height);
                                canvas.DrawString(item.Type + "  " + (item.Confidence * 100).ToString("0.00") + " %", DrawF, Brushes.White, Px, Py);
                                canvas.Flush();
                            }
                            catch
                            {


                            }
                        }
                    }
                }
            }

            //var oldImage = this.picProcess.Image;
           // this.picProcess.Image = image;

            //oldImage?.Dispose();
        }

        private Pen GetBrush(double confidence, int width)
        {
            var size = width / 300;

            if (confidence > 0.5)
            {
                return new Pen(Brushes.GreenYellow, size);
            }
            else if (confidence > 0.2 && confidence <= 0.5)
            {
                return new Pen(Brushes.Orange, size);
            }

            return new Pen(Brushes.DarkRed, size);
        }

        private void DataGridViewResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DataGridViewResult_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (!this.dataGridViewResult.Focused)
            //{
            //    return;
            //}

            //var items = this.dataGridViewResult.DataSource as List<YoloItem>;
            //var selectedItem = this.dataGridViewResult.CurrentRow?.DataBoundItem as YoloItem;
            //this.DrawBorder2Image(items, selectedItem);
        }

      
         public Mat ConvertBitmapToMat(Bitmap bmp)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
    
            System.Drawing.Imaging.BitmapData bmpData =
            bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
            bmp.PixelFormat);
   
            // data = scan0 is a pointer to our memory block.
            IntPtr data = bmpData.Scan0;
    
            // step = stride = amount of bytes for a single line of the image
            int step = bmpData.Stride;
    
            // So you can try to get you Mat instance like this:
            Mat mat = new Mat(bmp.Height, bmp.Width, Emgu.CV.CvEnum.DepthType.Cv32F, 4, data, step);
   
            // Unlock the bits.
            bmp.UnlockBits(bmpData);
  
            return mat;
        }


       

  
        

     
        private void Button3_Click(object sender, EventArgs e)
        {
            YoloState = true;
        }

        private void Button4_Click(object sender, EventArgs e)
        {

            //StopProcess();

           

        }


        public static double Euclidean(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

     
      

        private void BtClearObject_Click(object sender, EventArgs e)
        {
           
        }

   
 

        private void Imagecamera_MouseMove(object sender, MouseEventArgs e)
        {
            //int offsetX = (int)(e.Location.X / imagecamera.ZoomScale);
            //int offsetY = (int)(e.Location.Y / imagecamera.ZoomScale);
            //int horizontalScrollBarValue = imagecamera.HorizontalScrollBar.Visible ? (int)imagecamera.HorizontalScrollBar.Value : 0;
            //int verticalScrollBarValue = imagecamera.VerticalScrollBar.Visible ? (int)imagecamera.VerticalScrollBar.Value : 0;
            //lbPos.Text = Convert.ToString(offsetX + horizontalScrollBarValue) + " , " + Convert.ToString(offsetY + verticalScrollBarValue);
        }

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void Timerprocess_Tick(object sender, EventArgs e)
        {


           

            timerprocess.Enabled = false;
            timerprocessstop.Enabled = true;

            if(ckManualMode.Checked==false)
            {
                timerprocess.Enabled = true;
                timerprocessstop.Enabled = true;
                return;
            }
            //if ((frmDigitalIO.PLC_Warning == false) && (frmDigitalIO.PLC_Error == false))
            //{

            
             if(frmDigitalIO.PLC_Initial == true)
             {
               
             }

             if (frmDigitalIO.PLC_Reset == true)  //เครียรค่าทุกอย่าง.....
             {
                txtProductName.Text = "";
                txtTargetCount.Text = "";
                txtproduct_qrcode_find.Text = "";


                frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                frmDigitalIO.UpdateOutput(3, false); //GateClose OFF


                ObjectCount1 = 0;
                ObjectCount2 = 0;
                ObjectCount3 = 0;

                ObjectCountAll = 0;
                RotateBufferCount = 0;

                Count_old = 9999;

                lbObjectCount.Text = "0";


            }

            


                    if (frmDigitalIO.BoxReady == true)
                    {
                        if (frmDigitalIO.PcsConvRunning == false)
                        {
                    //frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                             
                              txtproduct_qrcode_find.BackColor = Color.Yellow;
                              frmDigitalIO.SendByteArray(frmDigitalIO.start_decoding);
                              txtInfo.Text = "Scan QRcode Start.....";


                              frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                              frmDigitalIO.UpdateOutput(3, false); //GateClose OFF


                               if (txtproduct_qrcode_find.Text != "")
                               {
                                     
                                            frmProduct.FindProductBYBarcode(txtproduct_qrcode_find.Text);

                                             //  frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                                             //  frmDigitalIO.UpdateOutput(3, false); //GateClose OFF

                        

                                            if ((txtProductName.Text != "") && (txtTargetCount.Text != ""))
                                            {
                                              

                                                txtproduct_qrcode_findAcept.Text = txtproduct_qrcode_find.Text;

                                                 txtproduct_qrcode_find.BackColor = Color.Green;
                                                 frmDigitalIO.txtproduct_qrcode_find.Text = "";
                                                  txtproduct_qrcode_find.Text = "";

                                                 //if (frmDigitalIO.txtCurrentPosition.Text != txtProductPos.Text)
                                                 //{
                                                  frmDigitalIO.MoveTG(txtProductPos.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);
                                                  //Thread.Sleep(1000);
                                                  //}

                                                  BlobColorProcessState = false;
                                                  DLProcessState = false;

                                                

                                                AutoPlay();

                                                //Thread.Sleep(1000);

                                                frmDigitalIO.UpdateOutput(2, true); //AutoStartPcsConv OFF


                                                txtInfo.Text = "Process Start.....";

                            
                                                timerprocessstop.Enabled = true;
                                                ProcessstopState = 0;


                        
                                                PanelAlarm2.Visible = false;



                                                PanelStart.Visible = true;
                                                PanelStart.BringToFront();


                                                lbStartInfo.Text = "พร้อม...";



                                            }
                                            else
                                            {
                                                txtproduct_qrcode_find.BackColor = Color.Yellow;
                                            }

                                       
                                    }
                                    else
                                    {


                                            //BlobColorState = false;
                                            //YoloState = false;

                                            //BlobColorProcessState = false;
                                            //DLProcessState = false;

                                            //btPlay.Visible = true;
                                            //btPause.Visible = false;
                                   }





                        }
                        else
                        {
                            if (timerprocessstop.Enabled == false)
                            {
                                txtInfo.Text = "Process Start.....";
                            }
                        }

                    }
                    else
                    {


                        if (frmDigitalIO.BoxReady == false)
                        {
                            if (frmDigitalIO.PcsConvRunning == false)
                            {
                                if (BlobColorState == false)
                                {

                                  frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                                  frmDigitalIO.UpdateOutput(3, false); //GateClose OFF


                                  frmDigitalIO.txtproduct_qrcode_find.Text = "";
                            //frmDigitalIO.UpdateOutput(7, false); //FG //FG NG
                            //frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER

                            //frmDigitalIO.UpdateOutput(3, false); //Gate Close

                            //frmDigitalIO.UpdateOutput(5, false); ////Alarm Near the set target value OFF
                                  txtInfo.Text = "Process Stop.....";
                                }
                                //else
                                //{
                                //   // txtInfo.Text = "Process Stop2.....";
                                //}
                            }
                            else
                            {

                            }
                          
                        }

                        

                    }
                
            //}
            //else
            //{
            //    txtInfo.Text = "Please Check PLC Conveyor.....";

            //}
      

             timerprocess.Enabled = true;
        }

        private void BtBrowsImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "jpg",
                Filter = "jpg files (*.jpg)|*.jpg",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
               string ret = openFileDialog1.FileName;


                m_bitmap = new Bitmap(ret);
                currentFrame = new Emgu.CV.Image<Bgr, Byte>(m_bitmap);

                imgFramecurrent = (Mat)currentFrame.Mat.Clone();
               // imagecamera.Image = currentFrame;
            }

        }

      

        // I'm deliberately NOT using a Forms timer to have callbacks from the "wrong" thread
        private System.Threading.Timer timer;

        private void EnsureTimer()
        {
            if (timer != null) return;
            timer = new System.Threading.Timer(RandomizeGlobalStyles);
        }

        private void RandomizeGlobalStyles(object state)
        {
            Random rng = new Random();

        
        }


      

        private void BtExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure exit program ?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {

               // frmPLC.Close();
                this.Close();
                Application.Exit();
                Environment.Exit(0);
            }
        }

        private void MetroTile1_Click_1(object sender, EventArgs e)
        {
            metroStyleManager.Theme = metroStyleManager.Theme == MetroThemeStyle.Light ? MetroThemeStyle.Dark : MetroThemeStyle.Light;
        }

        private void MetroTileSwitch_Click_1(object sender, EventArgs e)
        {
            var m = new Random();
            int next = m.Next(0, 13);
            metroStyleManager.Style = (MetroColorStyle)next;
        }

        private void MetroButton6_Click(object sender, EventArgs e)
        {
            MetroMessageBox.Show(this, "This is a sample MetroMessagebox `OK` only button", "MetroMessagebox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string FormatRectangle(RectangleF rect)
        {
            return string.Format("X:{0}, Y:{1}, W:{2}, H:{3}", (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }
       
        public void ReCheckColor()
        {

            if (CheckROIMode == true)
            {

                if (currentFrameBlobROI == null)
                {
                    return;
                }


                RectangleF Rec = imgDiff.SelectionRegion;

                Rectangle crop_region = new Rectangle((int)Rec.X, (int)Rec.Y, (int)Rec.Width, (int)Rec.Height); // set the roi

                Mat imgFramecurrent = (Mat)currentFrameBlobROI.Mat.Clone();

                Image<Bgr, Byte> buffer_im = imgFramecurrent.ToImage<Bgr, Byte>();

                buffer_im.ROI = crop_region;

                Image<Bgr, Byte> cropped_im = buffer_im.Copy();

                Mat imgBgr2Hsv = new Mat(buffer_im.ROI.Size, DepthType.Cv8U, 1);

                CvInvoke.CvtColor(cropped_im, imgBgr2Hsv, ColorConversion.Bgr2Hsv);

                frmColorView.imageROIHSV.Image = imgBgr2Hsv;
                frmColorView.imageROIBGR.Image = cropped_im;

                List<Int32> Blue = new List<Int32>();
                List<Int32> Green = new List<Int32>();
                List<Int32> Red = new List<Int32>();


                List<Int32> HColor = new List<Int32>();
                List<Int32> SColor = new List<Int32>();
                List<Int32> VColor = new List<Int32>();



                frmColorView.lbMinBColor.Text = "";
                frmColorView.lbMinGColor.Text = "";
                frmColorView.lbMinRColor.Text = "";

                frmColorView.lbMaxBColor.Text = "";
                frmColorView.lbMaxGColor.Text = "";
                frmColorView.lbMaxRColor.Text = "";


                frmColorView.lbMinHColor.Text = "";
                frmColorView.lbMinSColor.Text = "";
                frmColorView.lbMinVColor.Text = "";

                frmColorView.lbMaxHColor.Text = "";
                frmColorView.lbMaxSColor.Text = "";
                frmColorView.lbMaxVColor.Text = "";

                
                    for (int y = 0; y < cropped_im.Height-5; y++)
                    {
                        for (int x = 0; x < cropped_im.Width-5; x++)
                        {
                            try
                            {
                                byte B = cropped_im.Data[x, y, 0]; //Get Pixel Color B| fast way
                                byte G = cropped_im.Data[x, y, 1]; //Get Pixel Color G| fast way
                                byte R = cropped_im.Data[x, y, 2]; //Get Pixel Color R| fast way

                                if(B!=0)
                                { 
                                 Blue.Add(B);
                                }

                                if (G != 0)
                                {
                                    Green.Add(G);
                                }

                               if (R != 0)
                               {
                                Red.Add(R);
                               }
                            }
                            catch
                            {
                               break;
                            }
                        }
                    }

                    if((Blue.Count==0)|| (Green.Count == 0)|| (Red.Count == 0))
                    {

                    }
                    else
                    {
                        frmColorView.lbMinBColor.Text = Blue.Min().ToString();
                        frmColorView.lbMinGColor.Text = Green.Min().ToString();
                        frmColorView.lbMinRColor.Text = Red.Min().ToString();

                        frmColorView.lbMaxBColor.Text = Blue.Max().ToString();
                        frmColorView.lbMaxGColor.Text = Green.Max().ToString();
                        frmColorView.lbMaxRColor.Text = Red.Max().ToString();
                    }
                

                   Image<Bgr, Byte> bufferHSV_im = imgBgr2Hsv.ToImage<Bgr, Byte>();

                
                    for (int y = 0; y < bufferHSV_im.Height-5; y++)
                    {
                        for (int x = 0; x < bufferHSV_im.Width-5; x++)
                        {
                            try
                            {
                                byte H = bufferHSV_im.Data[x, y, 0]; //Get Pixel Color H| fast way
                                byte S = bufferHSV_im.Data[x, y, 1]; //Get Pixel Color S| fast way
                                byte V = bufferHSV_im.Data[x, y, 2]; //Get Pixel Color V| fast way

                                if (H != 0)
                                {
                                    HColor.Add(H);
                                }
                                if (S != 0)
                                {
                                    SColor.Add(S);
                                }
                                if (V != 0)
                                {
                                    VColor.Add(V);
                                }
                            }
                            catch
                            {
                                break;
                            }

                       }
                    }

                if ((HColor.Count == 0) || (SColor.Count == 0) || (VColor.Count == 0))
                {

                }
                else
                {

                    frmColorView.lbMinHColor.Text = HColor.Min().ToString();
                    frmColorView.lbMinSColor.Text = SColor.Min().ToString();
                    frmColorView.lbMinVColor.Text = VColor.Min().ToString();

                    frmColorView.lbMaxHColor.Text = HColor.Max().ToString();
                    frmColorView.lbMaxSColor.Text = SColor.Max().ToString();
                    frmColorView.lbMaxVColor.Text = VColor.Max().ToString();
                }


            }
        }

      

        private void PicObjectDetect_Click_1(object sender, EventArgs e)
        {

        }

        private void BtRecheckColor_Click(object sender, EventArgs e)
        {
            ReCheckColor();
        }

        private void BunifuImageButton1_Click(object sender, EventArgs e)
        {
            OpenColorView();
        }

     
  
   

        private void UpdateDeviceListTimer_Tick(object sender, EventArgs e)
        {
            UpdateDeviceList();
        }

      
        private void ButtonOneShot_Click(object sender, EventArgs e)
        {
            OneShot();
        }

        private void ButtonContinuousShot_Click(object sender, EventArgs e)
        {
            ContinuousShot();
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            Stop();
        }


        private void TriggerBlobColorEvent()
        {

                try
                {

                        if (BlobColorProcessState == true) //CameraGrapTriggerWatch>xxx
                        {
                            if (m_bitmapColor != null)
                            {


                                var sw = new Stopwatch();
                                sw.Start();

                                this.Invoke(new Action(() =>
                                {

                                   

                                    if (FrameTriggerTimeGrab > 200)
                                    {

                                    }


                                    long TimeCameraGrapTriggerWatch = CameraGrapTriggerWatch.ElapsedMilliseconds;

                                    DetectCaseZero = false;
                                    ColorTypeDetectState = false;
                                    DLCheckResult = true;



                                    ObjectProcessTrigger(true, EnableDLCheck);


                                    if (txtTargetCount.Text == "")
                                    {
                                        txtTargetCount.Text = "0";
                                    }


                                    double numTG = double.Parse(txtTargetCount.Text);

                                    double CountAll = Convert.ToDouble(ObjectCountAll);

                                    double PercentageCalculation = 0;// ((CountAll / numTG) * 100);


                                    if(CountAll>=1)
                                    {
                                        PanelStart.Visible = false;
                                        lbAlarmInformation3.Text = "";

                                    }

                                    try
                                    {
                                        double diffCountDisplay = numTG - CountAll;
                                        lbdiffCount.Text = diffCountDisplay.ToString("0");
                                    }
                                    catch
                                    {


                                    }

                                    if (numTG == 0)
                                    {
                                        CircleProgressbar.Value = 0;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            PercentageCalculation = ((CountAll / numTG) * 100);
                                        }
                                        catch
                                        {
                                            PercentageCalculation = 0;
                                        }

                                        if (PercentageCalculation > 100)
                                        {
                                            CircleProgressbar.Value = 100;
                                        }
                                        else
                                        {

                                            CircleProgressbar.Value = Convert.ToInt32(PercentageCalculation);// (numTG - CountAll) / 100;
                                        }
                                    }

                                    if (numTG <= CountAll)
                                    {
                                        ProcessstopState = 1;
                                    }




                                    if (ckReduceSpeed.Checked == true)
                                    {
                                        double diffCount = numTG - CountAll;
                                        double PercentageCheck = Convert.ToDouble(numReduceSpeed.Value);
                                        if (diffCount <= PercentageCheck)
                                        {
                                            frmDigitalIO.UpdateOutput(5, true); ////Alarm Near the set target value ON
                                        }

                                        


                                    }

                                    if (EnableDLCheck == true)
                                    {
                                        if (DLCheckResult == false) //ถ้า DL not math
                                        {
                                            ProcessstopState = 2;


                                        }
                                    }



                                    //if (DetectCaseZero == true)
                                    //{
                                    //    // ProcessstopState = 3;

                                    //}



                                    if (EnableColorTypeDetect == true)
                                    {
                                        if (ColorTypeDetectState == true)
                                        {

                                            ProcessstopState = 4;

                                        }

                                    }



                                    sw.Stop();

                                    BlobFrameCount++;

                                    Old_BlobFrameCount = FrameCount;

                                    lbSpeedBlobColorResult.Text = $"[Blob processed in {sw.Elapsed.TotalMilliseconds:0} ms ] Frame : " + BlobFrameCount.ToString();
                                    // lbSpeedGrabResult.Text = $"[Grab processed in {CameraGrapWatch.Elapsed.TotalMilliseconds:0} ms ]" + "  FPS : " + (1 / CameraGrapWatch.Elapsed.TotalSeconds).ToString("0.00");

                                    if (BlobFrameCount > 10000)
                                    {
                                        BlobFrameCount = 0;
                                    }
                                   

                                }));


                            }
                        }


                    BlobColorProcessState = false;
                   



                }
                catch
                {


                }
           
        }

     

        private void btOneShot_Click(object sender, EventArgs e)
        {
            OneShot();
        }

        private void btContinuousShot_Click(object sender, EventArgs e)
        {
            ContinuousShot();
        }

        private void btStopGrab_Click(object sender, EventArgs e)
        {
            Stop();
            video_rec_start = false;
        }

        private void btInitialCamera_Click(object sender, EventArgs e)
        {
           
            bool ret = InitailCamera(CammeraSN);//ON MC
            if (ret == true)
            {
                btInitialCamera.Enabled = true;
               
            }
            else
            {
                btInitialCamera.Enabled = false;
            }
        }

        public bool InitailCamera(string SN)
        {

              bool ret = false;
              string SerialNumber = SN;
              txtCameraModel.Text = "";
               lberror.Text = "";

               // Destroy the old camera object.
               if (camera != null)
               {
                  DestroyCamera();
                  converter = new PixelDataConverter();
               }

               
                try
                {
                    // Create a new camera object.
                    camera = new Camera(SerialNumber);

                   
                    CamSN = camera.CameraInfo[CameraInfoKey.SerialNumber];
                    txtCameraModel.Text = camera.CameraInfo[CameraInfoKey.ModelName] + " " + CamSN;

                    camera.CameraOpened += Configuration.AcquireContinuous;

                    // Register for the events of the image provider needed for proper operation.
                    camera.ConnectionLost += OnConnectionLost;
                    camera.CameraOpened += OnCameraOpened;
                    camera.CameraClosed += OnCameraClosed;
                    camera.StreamGrabber.GrabStarted += OnGrabStarted;
                    camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                    camera.StreamGrabber.GrabStopped += OnGrabStopped;

                    // Open the connection to the camera device.
                    camera.Open();
                    ret = true;

                }
                catch (Exception exception)
                {
                    ShowException(exception);
                    btInitialCamera.Enabled = true;
                }
            //}

            return ret;
        }

        private void btDoneCamera_Click(object sender, EventArgs e)
        {
            PanelCamera.Visible = false;
            this.Refresh();
        }

        private void btMenu_Click(object sender, EventArgs e)
        {

           

            if (PanelMenu.Visible==true)
            {
                PanelMenu.Visible = false;
            }
            else
            {
                PanelMenu.BringToFront();
                PanelMenu.Visible = true;
                PanelMenu.Left = btExit.Left - PanelMenu.Width + btExit.Width;
                PanelMenu.Top  = btExit.Top;
            }
            this.Refresh();

        }

        private void btDoneMenu_Click(object sender, EventArgs e)
        {
            PanelMenu.Visible = false;
            this.Refresh();
        }

        private void lbCameraMenu_Click(object sender, EventArgs e)
        {

            PanelCamera.Left = PanelProcess.Left;// (int)(this.Width / 2) - (int)(PanelCamera.Width / 2);
            PanelCamera.Top = PanelProcess.Top; // (int)(this.Height / 2) - (int)(PanelCamera.Height / 2);

            PanelCamera.Visible = true;
            PanelCamera.BringToFront();

            PanelMenu.Visible = false;
        }

        private void label33_Click(object sender, EventArgs e)
        {
            imgDiff.Zoom = 73;
        }

        private void lbBlobMenu_Click(object sender, EventArgs e)
        {
            PanelBlob.Left = PanelProcess.Left; // (int)(this.Width / 2) - (int)(PanelBlob.Width / 2);
            PanelBlob.Top = PanelProcess.Top;// (int)(this.Height / 2) - (int)(PanelBlob.Height / 2);

            PanelBlob.Visible = true;
            PanelBlob.BringToFront();

            PanelMenu.Visible = false;
        }

        private void btDoneBlob_Click(object sender, EventArgs e)
        {
            PanelBlob.Visible = false;
            this.Refresh();

        }

        private void BtImageInfo_Click(object sender, EventArgs e)
        {
            PanelImageInfomation.Left = PanelProcess.Left;// (int)(this.Width / 2) - (int)(PanelImageInfomation.Width / 2);
            PanelImageInfomation.Top = PanelProcess.Top;// (int)(this.Height / 2) - (int)(PanelImageInfomation.Height / 2);

            PanelImageInfomation.Visible = true;
            PanelImageInfomation.BringToFront();
        }

        private void btDonePanelInfo_Click(object sender, EventArgs e)
        {
            PanelImageInfomation.Hide();
        }

        private void lbProductMenu_Click(object sender, EventArgs e)
        {
            OpenProduct();
        }

   
        private void PanelBlob_MouseDown(object sender, MouseEventArgs e)
        {
            PanelBlob.BringToFront();
        }

        private void PanelCamera_MouseDown(object sender, MouseEventArgs e)
        {
            PanelCamera.BringToFront();
        }

      
        private void RadioColorType1_CheckedChanged(object sender, EventArgs e)
        {
           RadioColorType1Update();
        }

        private void RadioColorType2_CheckedChanged(object sender, EventArgs e)
        {
           RadioColorType2Update();
        }

        private void RadioColorType3_CheckedChanged(object sender, EventArgs e)
        {
           RadioColorType3Update();
        }

        private void RadioColorType4_CheckedChanged(object sender, EventArgs e)
        {
           RadioColorType4Update();
        }

      

        public void AutoPlay()
        {
            if ((txtProductName.Text != "") && (txtTargetCount.Text != ""))
            {
               // PanelBlob.Visible = false;

                if (ckColorTypeDetect.Checked == true)
                {
                    EnableColorTypeDetect = true;
                }
                else
                {
                    EnableColorTypeDetect = false;
                }

               


                    BlobColorState = true;
                    YoloState = true;
                    BlobColorProcessState = false;
                    DLProcessState = false;
               

                StartProcess();

                btPlay.Visible = false;
                btPause.Visible = true;
                timerprocessstop.Enabled = false;

                if (txtProductPos.Text != "")
                {
                    frmDigitalIO.MoveTG(txtProductPos.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);
                }
            }
        }
        private void btPlay_Click(object sender, EventArgs e)
        {

            PanelStart.Visible = false;

            Old_BlobFrameCount = 0;

            btPlay.Left = btPlay_Left;
            btPlay.Top = btPlay_Top ;
            btPlay.Size = btPlay_Size ;

            btPause.Left = btPlay_Left;
            btPause.Top = btPlay_Top;
            btPause.Size = btPlay_Size;

            btPlay.Left = btPlay_Left;
            btPlay.Top = btPlay_Top;
            btPlay.Size = btPlay_Size;


            if ((txtProductName.Text != "") && (txtTargetCount.Text != ""))
            {


                string DataResultProduct = DataResultPath + "\\" + txtProductName.Text.Trim();


                if (Directory.Exists(DataResultProduct))
                {
                    try
                    {
                        DeleteDir(DataResultProduct);
                    }
                    catch
                    {

                    }

                }


                //PanelBlob.Visible = false;

                if (ckColorTypeDetect.Checked == true)
                {
                    EnableColorTypeDetect = true;
                }
                else
                {
                    EnableColorTypeDetect = false;
                }

                

                StartProcess();

                btPlay.Visible = false;
                btPause.Visible = true;

                if (txtProductPos.Text != "")
                {
                    frmDigitalIO.MoveTG(txtProductPos.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);
                }
               
            }
            else
            {
                MetroMessageBox.Show(this, "Please Select Product ...", "Alarm Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              
            }


        }

        private void btStop_Click(object sender, EventArgs e)
        {

            BlobColorState = true;
            YoloState = true;

            BlobColorProcessState = false;
            DLProcessState = false;
        }

     

        private void lbDigitalIOMenu_Click(object sender, EventArgs e)
        {
            OpenIO();
        }

        public void UpdateColorThresholdView()
        {
            if (radioColorBlue.Checked == true)
            {
                ThresholdColorView = 1;
            }
            else if (radioColorWhite.Checked == true)
            {
                ThresholdColorView = 2;
            }
            else if (radioColorGray.Checked == true)
            {
                ThresholdColorView = 3;
            }
            else if (radioColorYellow.Checked == true)
            {
                ThresholdColorView = 4;
            }

        }
        private void radioColorBlue_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorThresholdView();

        }

        private void radioColorWhite_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorThresholdView();
        }

        private void radioColorGray_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorThresholdView();
        }

        private void radioColorYellow_CheckedChanged(object sender, EventArgs e)
        {
            UpdateColorThresholdView();
        }

        private void ckDebugmode_CheckedChanged(object sender, EventArgs e)
        {
            if(ckDebugmode.Checked==true)
            {
                EnableDebugMode = true;
            }
            else
            {
                EnableDebugMode = false;
            }
        }

      

        private void BtSave2_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        public void UpdateParameterObjectTrack()
        {
            AreaAceptmin = (Int64)numRectAreaAceptMin.Value;
            AreaAceptmax = (Int64)numRectAreaAceptMax.Value;
            LeastDistance = (Int64)numLeastDistance.Value;

            AreaAceptCase1 = (Int64)numObCase1.Value;
            AreaAceptCase2 = (Int64)numObCase2.Value;
            AreaAceptCase3 = (Int64)numObCase3.Value;
            AreaAceptCase4 = (Int64)numObCase4.Value;
            AreaAceptCase5 = (Int64)numObCase5.Value;

        }
        private void numLeastDistance_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameterObjectTrack();
        }

        private void numRectAreaAceptMin_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameterObjectTrack();
        }

        private void numRectAreaAceptMax_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameterObjectTrack();
        }

       
      

        private void BtClearObjectZero_Click(object sender, EventArgs e)
        {
            Old_BlobFrameCount = 0;


            btClearObjectZero.Left = btClearObjectZero_Left;
            btClearObjectZero.Top = btClearObjectZero_Top;
            btClearObjectZero.Size = btClearObjectZero_Size;

            
            if (ckColorTypeDetect.Checked == true)
            {

                EnableColorTypeDetect = true;
            }
            else
            {
                EnableColorTypeDetect = false;
            }



            ObjectCount1 = 0;
            ObjectCount2 = 0;
            ObjectCount3 = 0;

            ObjectTrack = new List<TrackableObject>();
            blobsCount = new List<Blob>();

            Count_old = 9999;
            ObjectCountAll = 0;

            txtResultCount.Text = "";


            imagecrop1.Image = null;
            imagecrop1.Invalidate();

            imagecrop2.Image = null;
            imagecrop2.Invalidate();


            imagecrop3.Image = null;
            imagecrop3.Invalidate();

            imagecrop4.Image = null;
            imagecrop4.Invalidate();


            for (int i = 0; i < lbimageinfo.Length; i++)
            {
               
                lbimageinfo[i].Text = "Count: ";
            }

           
            frmDigitalIO.UpdateOutput(5, false); ////Alarm Near the set target value OFF

            frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
            frmDigitalIO.UpdateOutput(3, false); //GateClose OFF

            PanelStart.Visible = false;
            PanelAlarm2.Visible = false;
        }

        private void BtPause_Click(object sender, EventArgs e)
        {
            //BlobColorState = false;

            Old_BlobFrameCount = 0;

            YoloState = false;

            //BlobColorProcessState = false;
            DLProcessState = false;

            btPlay.Visible = true;
            btPause.Visible = false;

            frmDigitalIO.UpdateOutput(5, false); ////Alarm Near the set target value OFF

            if ((txtProductName.Text != "") && (txtTargetCount.Text != ""))
            {

               // frmDigitalIO.MoveGateEnd();

            }
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            InitialConnectSystem();
        }

        private void BtDisConnect_Click(object sender, EventArgs e)
        {
            BlobColorState = false;
            YoloState = false;

            BlobColorProcessState = false;
            DLProcessState = false;

            btPlay.Visible = true;
            btPause.Visible = false;
        }

      

       

       

        public void UpdateObjectCase()
        {
            AreaAceptCase1 = (Int64)numObCase1.Value;
            AreaAceptCase2 = (Int64)numObCase2.Value;
            AreaAceptCase3 = (Int64)numObCase3.Value;        
            AreaAceptCase4 = (Int64)numObCase4.Value;
            AreaAceptCase5 = (Int64)numObCase5.Value;

        }
        private void numObCase1_ValueChanged(object sender, EventArgs e)
        {
            UpdateObjectCase();
        }

        private void numObCase2_ValueChanged(object sender, EventArgs e)
        {
            UpdateObjectCase();
        }

        private void numObCase3_ValueChanged(object sender, EventArgs e)
        {
            UpdateObjectCase();
        }

        private void numObCase4_ValueChanged(object sender, EventArgs e)
        {
            UpdateObjectCase();
        }

        private void numObCase5_ValueChanged(object sender, EventArgs e)
        {
            UpdateObjectCase();
        }

      

        private void LbCountViewMenu_Click(object sender, EventArgs e)
        {

            if (PanelViewSetup.Visible == false)
            {
                PanelViewSetup.Visible = true;
                PanelViewSetup.BringToFront();
            }
            else
            {
                PanelViewSetup.Visible = false;
            }
       
        }

        public void ClearUIDisplay()
        {
            picShow.Image = null;
            txtProductName.Text = "";
            txtProductFullName.Text = "";
            txtBarcodeID.Text = "";
            txtTargetCount.Text = "";
            CircleProgressbar.Value = 0;

            ckReduceSpeed.Checked = false;
            numReduceSpeed.Value = 80;
           

            ckDLProcess.Checked = false;
            cbObjectList.Text = "";
            txtConfidence.Text = "";
            numDLCountAccept.Value = 0;

            numLeastDistance.Value = 0;
            numRectAreaAceptMin.Value = 0;
            numRectAreaAceptMax.Value = 0;

            numObCase1.Value = 0;
            numObCase2.Value = 0;
            numObCase3.Value = 0;
            numObCase4.Value = 0;
            numObCase5.Value = 0;

            numObCase6.Value = 0;
            numObCase7.Value = 0;
            numObCase8.Value = 0;
            numObCase9.Value = 0;
            numObCase10.Value = 0;
        }
 
        
        private void Label41_Click(object sender, EventArgs e)
        {
            if(groupDebug.Visible==false)
            {
                groupDebug.Visible = true;
                groupDebug.Left = 10;
                groupDebug.Top = 432;
                groupDebug.BringToFront();
            }
            else 
            {
                groupDebug.Visible = false;
            }
        }

        private void BunifuImageButton1_Click_1(object sender, EventArgs e)
        {
            PanelAlarm.Hide();
            ColorTypeDetectState = false;
            frmDigitalIO.UpdateOutput(5, false); //Clear Alarm
        }

        private void ckManualMode_CheckedChanged(object sender, EventArgs e)
        {
            if(ckManualMode.Checked==true)
            {

                frmDigitalIO.UpdateOutput(0, true); //Auto mode
                frmDigitalIO.UpdateOutput(1, false); //Manual Converyer Off
                ckConveyorRun.Visible = false;
                lbMode.Text = "Auto";
            }
            else
            {
                frmDigitalIO.UpdateOutput(0, false); //Manual mode
                ckConveyorRun.Visible = true;
                lbMode.Text = "Manual";

            }
        }

        private void btOnConveryer_Click(object sender, EventArgs e)
        {
            frmDigitalIO.UpdateOutput(1, true); //Manual Converyer On --> On Light
        }

        private void ckConveyorRun_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void ckConveyorRun_Click(object sender, EventArgs e)
        {
            if (ckConveyorRun.Checked == true)
            {
                frmDigitalIO.UpdateOutput(1, true); //Manual Converyer On
            }
            else
            {
                frmDigitalIO.UpdateOutput(1, false); //Manual Converyer Off
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void CkColorTypeDetect_CheckedChanged(object sender, EventArgs e)
        {
            if(ckColorTypeDetect.Checked==true)
            {
                EnableColorTypeDetect = true;
            }
            else
            {
                EnableColorTypeDetect = false;
            }
            
        }

        private void timerprocessstop_Tick(object sender, EventArgs e)
        {
           

            
            timerprocessstop.Enabled = false;

            lbStateStop.Text = ProcessstopState.ToString();

            if ((ProcessstopState == 1)) //OK Result
            {
                if (frmDigitalIO.PcsConvRunning == false)
                {


                    BlobColorState = false;
                    YoloState = false;

                    BlobColorProcessState = false;
                    DLProcessState = false;

                    btPlay.Visible = true;
                    btPause.Visible = false;


                    
                    frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER OFF
                    frmDigitalIO.MoveTG(txtProductPosEnd.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);

                    ProcessstopState = 8;
                    timerprocessstop.Enabled = true;

                }
                else
                {

                    frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF

                    frmDigitalIO.UpdateOutput(3, true); //GateClose ON

                    frmDigitalIO.UpdateOutput(7, true); //FG 

                    double numTG = double.Parse(txtTargetCount.Text);
                    double CountAll = Convert.ToDouble(ObjectCountAll);


                    frmDigitalIO.UpdateOutput(6, true); //RESULT TRGGER  ON

                    if (numTG < CountAll)
                    {

                        PanelAlarm.Visible = true;
                        PanelAlarm.BringToFront();


                        PanelAlarm2.Visible = true;
                        PanelAlarm2.BringToFront();

                        lbAlarmInformation3.Visible = true;
                        lbAlarmInformation3.BringToFront();

                        lbAlarmInformation.Text = "ข้อต่อนับเกิน " + Math.Abs(CountAll - numTG).ToString() + " ตัว";
                        lbAlarmInformation2.Text = "ข้อต่อนับเกิน " + Math.Abs(CountAll - numTG).ToString() + " ตัว";
                        lbAlarmInformation3.Text = "ข้อต่อนับเกิน " + Math.Abs(CountAll - numTG).ToString() + " ตัว";
                    }
                    else if (numTG == CountAll)
                    {
                        lbAlarmInformation.Text = "";
                        lbAlarmInformation2.Text = "";
                        lbAlarmInformation3.Text = "";
                    }



                    ProcessDelayCount = 0;
                    ProcessstopState = 11;

                    timerprocessstop.Enabled = true;
                }
            }
            else if ((ProcessstopState == 11)) //delay 5 Sec.  ให้อ่านต่อเพื่อนับว่างานเกินหรือเปล่า
            {
                ProcessDelayCount++;

                if (ProcessDelayCount == 1)
                {
                    frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER OFF
                    //frmDigitalIO.UpdateOutput(5, false); ////Alarm Near the set target value OFF
                }
                else if (ProcessDelayCount==5)
                {
                    

                    frmDigitalIO.MoveTG(txtProductPosEnd.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);
                    ProcessstopState = 8;

                    
                }


                timerprocessstop.Enabled = true;
            }
            else if (ProcessstopState == 2)
            {

                BlobColorState = false;
                YoloState = false;

                BlobColorProcessState = false;
                DLProcessState = false;

                btPlay.Visible = true;
                btPause.Visible = false;




                PanelAlarm.Visible = true;
                PanelAlarm.BringToFront();
                lbAlarmInformation.Text = "Error : ข้อต่อผิดประเภท";


                PanelAlarm2.Visible = true;
                PanelAlarm2.BringToFront();
                lbAlarmInformation2.Text = "Error : ข้อต่อผิดประเภท";

                lbAlarmInformation3.Visible = true;
                lbAlarmInformation3.Text = "Error : ข้อต่อผิดประเภท";

                frmDigitalIO.UpdateOutput(5, true); ////Alarm Near the set target value ON



                txtInfo.Text = "Error : ข้อต่อผิดประเภท";
                txtInfoDetail.Text = "Object Detect DL Condition....";
                frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                Thread.Sleep(3000);
                frmDigitalIO.UpdateOutput(3, true); //GateClose ON

                frmDigitalIO.UpdateOutput(7, false); //FG = NG

                frmDigitalIO.UpdateOutput(6, true); //RESULT TRGGER  ON




                Thread.Sleep(5000);
                frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER OFF



                frmDigitalIO.MoveTG(txtProductPosEnd.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);

                ProcessstopState = 8;

                timerprocessstop.Enabled = true;



            }
            else if (ProcessstopState == 3)//Object Detect Out Area Condition....
            {

                BlobColorState = false;
                YoloState = false;

                BlobColorProcessState = false;
                DLProcessState = false;

                btPlay.Visible = true;
                btPause.Visible = false;


                PanelAlarm.Visible = true;
                PanelAlarm.BringToFront();
                lbAlarmInformation.Text = "Error : ข้อต่อผิดประเภท";

                PanelAlarm2.Visible = true;
                PanelAlarm2.BringToFront();
                lbAlarmInformation2.Text = "Error : ข้อต่อผิดประเภท";

                lbAlarmInformation3.Visible = true;
                lbAlarmInformation3.Text = "Error : ข้อต่อผิดประเภท";


                txtInfo.Text = "Object Detect Out Area Condition....";
                txtInfoDetail.Text = "Object Detect Out Area Condition....";
                frmDigitalIO.UpdateOutput(5, true); ////Alarm Near the set target value ON




                frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                Thread.Sleep(3000);
                frmDigitalIO.UpdateOutput(3, true); //GateClose ON


                frmDigitalIO.UpdateOutput(7, false); //FG = NG

                frmDigitalIO.UpdateOutput(6, true); //RESULT TRGGER  ON
                Thread.Sleep(5000);
                frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER OFF



                frmDigitalIO.MoveTG(txtProductPosEnd.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);

                ProcessstopState = 8;
                timerprocessstop.Enabled = true;
            }
            else if (ProcessstopState == 4) //"Object Color Type Alarm.....";
            {

                BlobColorState = false;
                YoloState = false;

                BlobColorProcessState = false;
                DLProcessState = false;

                btPlay.Visible = true;
                btPause.Visible = false;

                PanelAlarm.Visible = true;
                PanelAlarm.BringToFront();
                lbAlarmInformation.Text = "Error : ข้อต่อผิดสี";
                txtInfo.Text = "Object Detect Color Condition....";
                txtInfoDetail.Text = "Object Detect Color Condition....";


                PanelAlarm2.Visible = true;
                PanelAlarm2.BringToFront();
                lbAlarmInformation2.Text = "Error : ข้อต่อผิดสี";

                lbAlarmInformation3.Visible = true;
                lbAlarmInformation3.Text = "Error : ข้อต่อผิดสี";


                frmDigitalIO.UpdateOutput(5, true); ////Alarm Near the set target value ON



                frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                Thread.Sleep(3000);
                frmDigitalIO.UpdateOutput(3, true); //GateClose ON



                frmDigitalIO.UpdateOutput(7, false); //FG = NG

                frmDigitalIO.UpdateOutput(6, true); //RESULT TRGGER  ON
                Thread.Sleep(5000);
                frmDigitalIO.UpdateOutput(6, false); //RESULT TRGGER OFF



                frmDigitalIO.MoveTG(txtProductPosEnd.Text.Trim(), frmDigitalIO.txtTragetSpeed.Text, frmDigitalIO.txtTragetAcc.Text, false, 1);

                ProcessstopState = 8;

                timerprocessstop.Enabled = true;
            }
            else if (ProcessstopState == 8)//End Complete
            {
                if (frmDigitalIO.PcsConvRunning == false)
                {


                    BlobColorState = false;
                    YoloState = false;

                    BlobColorProcessState = false;
                    DLProcessState = false;

                    btPlay.Visible = true;
                    btPause.Visible = false;

                    frmDigitalIO.UpdateOutput(2, false); //AutoStartPcsConv OFF
                    frmDigitalIO.UpdateOutput(3, false); //GateClose OFF

                    //timerprocessstop.Enabled = false;

                    txtInfo.Text = "Process End Complete";

                    txtproduct_qrcode_find.Text = "";


                    frmDigitalIO.UpdateOutput(5, false); //Clear Alarm

                    ProcessstopState = 9;



                    timerprocessstop.Enabled = true;




                }
                else
                {
                    txtInfo.Text = "Wait for process done....";
                }
            }
            else if (ProcessstopState == 9)
            {
                ProcessstopState = 0;
                timerprocessstop.Enabled = false;
            }

            

           
        }

    

        private void BtDonePassword_Click(object sender, EventArgs e)
        {

            PanelPassword.Visible = false;
            txtPassword.Text = "";
       

        }

     

        

        public void UpdateDilateErode()
        {
            NumDilate = (int) numofDilate.Value;
            NumErode = (int)numofErode.Value;
        }

        private void numofDilate_ValueChanged(object sender, EventArgs e)
        {
            UpdateDilateErode();
        }

        private void numofErode_ValueChanged(object sender, EventArgs e)
        {
            UpdateDilateErode();
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            OpenColorView();
        }

        private void CkViewAllDataSet_CheckedChanged(object sender, EventArgs e)
        {
            if(ckViewAllDataSet.Checked==true)
            {
                ViewAllDataSet = true;
            }
            else
            {
                ViewAllDataSet = false;

            }
        }

       

        private void btSaveView_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void numZoomProcess_ValueChanged(object sender, EventArgs e)
        {
           
            intnumZoomProcess = (Int16)numZoomProcess.Value;
            imgDiff.Zoom = intnumZoomProcess;


        }

        private void numZoomBuffer_ValueChanged(object sender, EventArgs e)
        {
            
            intnumZoomBuffer = (Int16)numZoomBuffer.Value;
            imagecrop1.Zoom = intnumZoomBuffer;
            imagecrop2.Zoom = intnumZoomBuffer;
            imagecrop3.Zoom = intnumZoomBuffer;
            imagecrop4.Zoom = intnumZoomBuffer;
        }

        private void numOffsetLeftProcess_ValueChanged(object sender, EventArgs e)
        {
            intnumOffsetLeft = (Int16)numOffsetLeftProcess.Value;
          
        }

        private void numOffsetRightProcess_ValueChanged(object sender, EventArgs e)
        {
            intnumOffsetRight = (Int16)numOffsetRightProcess.Value;
           
        }

        private void btdonesetview_Click(object sender, EventArgs e)
        {
            PanelViewSetup.Hide();
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            imgDiff.Zoom = intnumZoomProcess;
            imgDiff.CenterToImage();
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            imagecrop1.Zoom = intnumZoomBuffer;
            imagecrop1.CenterToImage();
        }

        private void bunifuImageButton6_Click(object sender, EventArgs e)
        {
            imagecrop2.Zoom = intnumZoomBuffer;
            imagecrop2.CenterToImage();
        }

        private void bunifuImageButton4_Click(object sender, EventArgs e)
        {
            imagecrop3.Zoom = intnumZoomBuffer;
            imagecrop3.CenterToImage();
        }

        private void CkColorBlob_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btBlobTest_Click(object sender, EventArgs e)
        {
            ObjectProcessTrigger(ckObjectBlob.Checked, ckYoloDL.Checked);
        }

        private void btDLTest_Click(object sender, EventArgs e)
        {
            ObjectProcessTrigger(false, true);
        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {
            imagecrop4.Zoom = intnumZoomBuffer;
            imagecrop4.CenterToImage();
        }

      

        private void btPLCReset_Click(object sender, EventArgs e)
        {
            frmDigitalIO.UpdateOutput(4, true); ////Reset PLC
            Thread.Sleep(1000);
            frmDigitalIO.UpdateOutput(4, false); ////Reset PLC
        }

     

        private void bunifuImageButton7_Click(object sender, EventArgs e)
        {
            PanelAlarm2.Visible = false;

        }

        private void bunifuImageButton8_Click(object sender, EventArgs e)
        {
            PanelStart.Visible = false;
        }

   
        private void ckDLTrainingSave_CheckedChanged(object sender, EventArgs e)
        {
            if (ckDLTrainingSave.Checked == true)
            {
                EnableDLTrainingSave = true;
            }
            else
            {
                EnableDLTrainingSave = false;
            }
        }

        private void BtColorView_Click(object sender, EventArgs e)
        {
            OpenColorView();
        }

      
        private void BtSaveHsvType_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }



        public double distanceBetweenPoints(Point point1, Point point2)
        {

            int intX = Math.Abs(point1.X - point2.X);
            int intY = Math.Abs(point1.Y - point2.Y);

            return Math.Sqrt((Math.Pow(intX, 2)) + (Math.Pow(intY, 2)));

        }



        public Size getTextSize(string strText, int intFontFace, double dblFontScale, int intFontThickness)
        {

            Size textSize = new Size(); //this will be the return value

            int intNumChars = strText.Count();

            textSize.Width = 55 * intNumChars;
            textSize.Height = 65;

            return (textSize);

        }

    
      


        //////////////////////////END/////////////////////////////
    }





}
