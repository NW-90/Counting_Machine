using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;


using static System.Math;

using Emgu.CV;
using Emgu.CV.CvEnum; //Emgu Cv imports
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;

namespace DLCounting
{
    public class Blob
    {

        public VectorOfPoint currentContour = new VectorOfPoint();

        public Rectangle currentBoundingRect;

        public bool IntersectFilterState = false;
        // The circle's coordinates.
        public int Cx;
        public int Cy;
        public int Radius;

        public Point PointP1, PointP2;

        public Point PointTop;
        public Point PointBotton;

        public Point IntersectionPoint;

        public List<Point> centerPositions = new List<Point>();
        public List<int> FrameTrack = new List<int>();

       
        public List<double> ListDiagonalSize = new List<double>();

        public double dblCurrentDiagonalSize;
        public double dblCurrentAspectRatio;

        public int intCurrentRectArea;


        //public List<double> ContourArea = new List<double>();
        public double ContourArea = 0;


        public Rectangle AcceptBoundingRect;
        public double[] AcceptContourArea = new double[3] { 0, 0, 0 };

        public double AcceptContour = 0.0;

        public bool blnCurrentMatchFoundOrNewBlob;

        public bool blnStillBeingTracked;
        public int intNumOfFramesWithoutAMatch = 0;
        public int[] intNumOfConsecutiveFramesWithoutAMatch = new int[3] { 0, 0, 0 };
     
        public Point predictedNextPosition;

        public bool[] counted = new bool[3] { false, false, false };
     

        public bool DL_Counted;

        public int intNumOfFramescounted = 0;
        public int[] intNumOfConsecutiveFramescounted = new int[3] { 0, 0, 0 };


        public int[] count_num = new int[3] { 0, 0, 0 };
        public bool Accept_count = false;
        public int count_All = 0;
        public int count_Track = 0;


        public List<string> DL_Type = new List<string>();  //ใช้เก็บ DL_ID > 3 
        public List<string> DL_Confidence = new List<string>();  //ใช้เก็บ DL_ID > 3 
        public bool DL_ID_ACTIVE;
        public string DL_Type_ACCEPT;


        public double LastDistance = 0;

        public double DistanceMin = 9999999;
        public double DistanceMax = 0;

        public double AreaMin = 99999999999999;
        public double AreaMax = 0;

        public double FrameTrackCount = 0;


        public List<string> Color_Type = new List<string>();  //ใช้เก็บ DL_ID > 3 
        public bool Color_Type_ACTIVE;


        public int AreaAceptCase=0;


        public string Object_ID = "";

        // constructor '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        public Blob(VectorOfPoint _contour)
        {

            currentContour = _contour;

            currentBoundingRect = CvInvoke.BoundingRectangle(currentContour);

            double ObjectArea = CvInvoke.ContourArea(currentContour, false);

            Point currentCenter = new Point();

            currentCenter.X = Convert.ToInt32((double)(currentBoundingRect.X + currentBoundingRect.X + currentBoundingRect.Width) / 2.0);
            currentCenter.Y = Convert.ToInt32((double)(currentBoundingRect.Y + currentBoundingRect.Y + currentBoundingRect.Height) / 2.0);

           

            centerPositions.Add(currentCenter);


            ContourArea = ObjectArea;
     
            dblCurrentDiagonalSize = Sqrt((Math.Pow(currentBoundingRect.Width, 2)) + (Math.Pow(currentBoundingRect.Height, 2)));

            dblCurrentAspectRatio = (double)currentBoundingRect.Width / (double)currentBoundingRect.Height;

            intCurrentRectArea = currentBoundingRect.Width * currentBoundingRect.Height;


            Cx = currentCenter.X;
            Cy = currentCenter.Y;
           // Radius = Convert.ToInt32(dblCurrentDiagonalSize / 2);
            Radius = Convert.ToInt32(currentBoundingRect.Width / 2);

            FrameTrack.Add(0);


            PointTop.X = currentBoundingRect.X;
            PointTop.Y = currentBoundingRect.Y;

            PointBotton.X = currentBoundingRect.X;
            PointBotton.Y = currentBoundingRect.Y + currentBoundingRect.Height;

            blnStillBeingTracked = true;
            blnCurrentMatchFoundOrNewBlob = true;


            intNumOfFramesWithoutAMatch = 0;
            intNumOfConsecutiveFramesWithoutAMatch[0] = 0;
            intNumOfConsecutiveFramesWithoutAMatch[1] = 0;
            intNumOfConsecutiveFramesWithoutAMatch[2] = 0;

            intNumOfFramescounted = 0;
            intNumOfConsecutiveFramescounted[0] = 0;
            intNumOfConsecutiveFramescounted[1] = 0;
            intNumOfConsecutiveFramescounted[2] = 0;
          
            counted[0] = false;
            counted[1] = false;
            counted[2] = false;

            DL_Counted = false;
          
            count_num[0] = 0;
            count_num[1] = 0;
            count_num[2] = 0;

            AcceptContour = 0.0;

            count_All = 0;
            Accept_count = false;
            count_Track = 0;

            DL_ID_ACTIVE = false;
            DL_Type_ACCEPT = "";

            Color_Type_ACTIVE = false;

            FrameTrackCount = 0;

            IntersectFilterState = false;

            Object_ID = "";



        }

        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        public void predictNextPosition() //เป็นการคำนวณจุด X,Y ใหม่ที่คาดการว่าจะปรากฏในอนาคต
        {

            int numPositions = centerPositions.Count();

            

            if (numPositions == 1) //ถ้ามีตำแหน่งเดียวให้ใช้ตำแหน่งล่าสุด
            {

                predictedNextPosition.X = centerPositions.Last().X;
                predictedNextPosition.Y = centerPositions.Last().Y;

            }
            else if (numPositions == 2)//ถ้ามี 2 ตำแหน่ง ให้หาค่า delta มา + ตำแหน่่งล่าสุด
            {

                int deltaX = centerPositions[1].X - centerPositions[0].X;
                int deltaY = centerPositions[1].Y - centerPositions[0].Y;

                predictedNextPosition.X = centerPositions.Last().X + deltaX;
                predictedNextPosition.Y = centerPositions.Last().Y + deltaY;

            }
            else if (numPositions == 3)
            {

                int sumOfXChanges = ((centerPositions[2].X - centerPositions[1].X) * 2) + ((centerPositions[1].X - centerPositions[0].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 3.0)));

                int sumOfYChanges = ((centerPositions[2].Y - centerPositions[1].Y) * 2) + ((centerPositions[1].Y - centerPositions[0].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 3.0)));

                predictedNextPosition.X = centerPositions.Last().X + deltaX;
                predictedNextPosition.Y = centerPositions.Last().Y + deltaY;

            }
            else if (numPositions == 4)
            {

                int sumOfXChanges = ((centerPositions[3].X - centerPositions[2].X) * 3) + ((centerPositions[2].X - centerPositions[1].X) * 2) + ((centerPositions[1].X - centerPositions[0].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 6.0)));

                int sumOfYChanges = ((centerPositions[3].Y - centerPositions[2].Y) * 3) + ((centerPositions[2].Y - centerPositions[1].Y) * 2) + ((centerPositions[1].Y - centerPositions[0].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 6.0)));

                predictedNextPosition.X = centerPositions.Last().X + deltaX;
                predictedNextPosition.Y = centerPositions.Last().Y + deltaY;

            }
            else if (numPositions >= 5)
            {

                int sumOfXChanges = ((centerPositions[numPositions - 1].X - centerPositions[numPositions - 2].X) * 4) + ((centerPositions[numPositions - 2].X - centerPositions[numPositions - 3].X) * 3) + ((centerPositions[numPositions - 3].X - centerPositions[numPositions - 4].X) * 2) + ((centerPositions[numPositions - 4].X - centerPositions[numPositions - 5].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 10.0)));

                int sumOfYChanges = ((centerPositions[numPositions - 1].Y - centerPositions[numPositions - 2].Y) * 4) + ((centerPositions[numPositions - 2].Y - centerPositions[numPositions - 3].Y) * 3) + ((centerPositions[numPositions - 3].Y - centerPositions[numPositions - 4].Y) * 2) + ((centerPositions[numPositions - 4].Y - centerPositions[numPositions - 5].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 10.0)));

                predictedNextPosition.X = centerPositions.Last().X + deltaX;
                predictedNextPosition.Y = centerPositions.Last().Y + deltaY;

            }
            else
            {

            }

        }

    }
}


