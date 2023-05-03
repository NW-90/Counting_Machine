using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using System.Drawing;
using static System.Math;

using Alturos.Yolo;
using Alturos.Yolo.Model;

namespace DLCounting
{

    public class TrackableObject
    {

        public YoloItem currentItem = new YoloItem();
        public Rectangle currentBoundingRect;
        public List<Point> CenterPositions = new List<Point>();
        //public Point CenterPositions = new Point();


        public bool counted;
        public int objectID;

        public double DistanceMin = 0;
        public double DistanceMax = 0;

        public double dblCurrentDiagonalSize;
        public double dblCurrentAspectRatio;
        public int intCurrentRectArea;
        public bool blnCurrentMatchFoundOrNewBlob;
        public bool blnStillBeingTracked;
        public int intNumOfConsecutiveFramesWithoutAMatch;
        public Point predictedNextPosition;




        // constructor '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        public TrackableObject(YoloItem _contour,int ID)
        {
            currentItem = _contour;
         
            Point currentCenter = new Point();

            currentCenter.X = Convert.ToInt32((double)(currentItem.X + currentItem.X + currentItem.Width) / 2.0);
            currentCenter.Y = Convert.ToInt32((double)(currentItem.Y + currentItem.Y + currentItem.Height) / 2.0);
            CenterPositions.Add(currentCenter);  //add center

            //CenterPositions.X = Convert.ToInt32((double)(currentItem.X + currentItem.X + currentItem.Width) / 2.0);
            //CenterPositions.Y = Convert.ToInt32((double)(currentItem.Y + currentItem.Y + currentItem.Height) / 2.0);




           // dblCurrentDiagonalSize = Sqrt((Math.Pow(currentItem.Width, 2)) + (Math.Pow(currentItem.Height, 2)));
           // dblCurrentAspectRatio = (double)currentItem.Width / (double)currentItem.Height;


            objectID = ID;
            counted = false;


            intCurrentRectArea = currentItem.Width * currentItem.Height;

            blnStillBeingTracked = true;
            blnCurrentMatchFoundOrNewBlob = true;

            intNumOfConsecutiveFramesWithoutAMatch = 0;

        }

        //''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        public void predictNextPosition()
        {

            int numPositions = CenterPositions.Count();

            if (numPositions == 1)
            {

                predictedNextPosition.X = CenterPositions.Last().X;
                predictedNextPosition.Y = CenterPositions.Last().Y;

            }
            else if (numPositions == 2)
            {

                int deltaX = CenterPositions[1].X - CenterPositions[0].X;
                int deltaY = CenterPositions[1].Y - CenterPositions[0].Y;

                predictedNextPosition.X = CenterPositions.Last().X + deltaX;
                predictedNextPosition.Y = CenterPositions.Last().Y + deltaY;

            }
            else if (numPositions == 3)
            {

                int sumOfXChanges = ((CenterPositions[2].X - CenterPositions[1].X) * 2) + ((CenterPositions[1].X - CenterPositions[0].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 3.0)));

                int sumOfYChanges = ((CenterPositions[2].Y - CenterPositions[1].Y) * 2) + ((CenterPositions[1].Y - CenterPositions[0].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 3.0)));

                predictedNextPosition.X = CenterPositions.Last().X + deltaX;
                predictedNextPosition.Y = CenterPositions.Last().Y + deltaY;

            }
            else if (numPositions == 4)
            {

                int sumOfXChanges = ((CenterPositions[3].X - CenterPositions[2].X) * 3) + ((CenterPositions[2].X - CenterPositions[1].X) * 2) + ((CenterPositions[1].X - CenterPositions[0].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 6.0)));

                int sumOfYChanges = ((CenterPositions[3].Y - CenterPositions[2].Y) * 3) + ((CenterPositions[2].Y - CenterPositions[1].Y) * 2) + ((CenterPositions[1].Y - CenterPositions[0].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 6.0)));

                predictedNextPosition.X = CenterPositions.Last().X + deltaX;
                predictedNextPosition.Y = CenterPositions.Last().Y + deltaY;

            }
            else if (numPositions >= 5)
            {

                int sumOfXChanges = ((CenterPositions[numPositions - 1].X - CenterPositions[numPositions - 2].X) * 4) + ((CenterPositions[numPositions - 2].X - CenterPositions[numPositions - 3].X) * 3) + ((CenterPositions[numPositions - 3].X - CenterPositions[numPositions - 4].X) * 2) + ((CenterPositions[numPositions - 4].X - CenterPositions[numPositions - 5].X) * 1);

                int deltaX = Convert.ToInt32(Math.Round((double)(sumOfXChanges / 10.0)));

                int sumOfYChanges = ((CenterPositions[numPositions - 1].Y - CenterPositions[numPositions - 2].Y) * 4) + ((CenterPositions[numPositions - 2].Y - CenterPositions[numPositions - 3].Y) * 3) + ((CenterPositions[numPositions - 3].Y - CenterPositions[numPositions - 4].Y) * 2) + ((CenterPositions[numPositions - 4].Y - CenterPositions[numPositions - 5].Y) * 1);

                int deltaY = Convert.ToInt32(Math.Round((double)(sumOfYChanges / 10.0)));

                predictedNextPosition.X = CenterPositions.Last().X + deltaX;
                predictedNextPosition.Y = CenterPositions.Last().Y + deltaY;

            }
            else
            {
                //should never get here
            }

        }

    }

}
