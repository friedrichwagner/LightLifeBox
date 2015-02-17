using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using PILEDServer;
using Lumitech.Helpers;

namespace LightLife
{
    public struct kd
    {
        public Single k;
        public Single d;

        //Steigung = dy/dx, d=y-k*x
        public kd(Point p1, Point p2)
        {
            k = (Single) ((p2.Y - p1.Y) / (p2.X - p1.X));
            d = (Single)(p2.Y - k * p2.X);
        }

        //Umkehrtransformation xy --> Pixel, aus kd von Pixel --> xy
        //k=dx/dy, d=X-k*y, weil Punkt umgekehrt definiert ist
        public kd(Point p1, Single k1)
        {
            k = 1 / k1;
            d = (Single)(p1.X - k * p1.Y);
        }

    }

    public static class CanvasExtensions
    {
        /// <summary>
        /// Removes all instances of a type of object from the children collection.
        /// </summary>
        /// <typeparam name="T">The type of object you want to remove.</typeparam>
        /// <param name="targetCollection">A reference to the canvas you want items removed from.</param>
        public static void Remove<T>(this UIElementCollection targetCollection)
        {
            // This will loop to the end of the children collection.
            int index = 0;

            // Loop over every element in the children collection.
            while (index < targetCollection.Count)
            {
                // Remove the item if it's of type T
                if (targetCollection[index] is T)
                    targetCollection.RemoveAt(index);
                else
                    index++;
            }
        }
    }

    public static class CIEChart
    {
        private static Point Y1= new Point(452, 0.0);
        private static Point Y2 = new Point(21, 0.9);
        private static kd kdY = new kd(Y2, Y1);
        private static kd kdY2 = new kd(Y1, kdY.k);

        private static Point X1 = new Point(44, 0.0);
        private static Point X2 = new Point(423, 0.8);
        private static kd kdX = new kd(X2, X1);
        private static kd kdX2 = new kd(X1, kdX.k);

        public static Point PixelToXY(ref Point pixel)
        {
            Point ret = new Point();

            ClipPixelToRectangle(ref pixel);

            ret.X = kdX.k * pixel.X + kdX.d;
            ret.Y = kdY.k * pixel.Y + kdY.d;

            return ret;
        }

        public static Point XyToPixel(ref Point xy)
        {
            Point ret = new Point();
            ClipXy(ref xy);

            ret.X = kdX2.k * xy.X + kdX2.d;
            ret.Y = kdY2.k * xy.Y + kdY2.d;

            return ret;
        }

        private static void ClipPixelToRectangle(ref Point pixel)
        {
            if (pixel.X < X1.X) pixel.X = X1.X;
            if (pixel.X > X2.X) pixel.X = X2.X;

            if (pixel.Y < Y2.X) pixel.Y = Y2.X;
            if (pixel.Y > Y1.X) pixel.Y = Y1.X;
        }

        private static void ClipXy(ref Point xy)
        {
            if (xy.X < 0.0) xy.X = 0;
            if (xy.X > 1.0) xy.X = 1.0;

            if (xy.Y < 0.0) xy.Y = 0;
            if (xy.Y > 1.0) xy.Y = 1.0;
        }

        public static void DrawPoint(Canvas c, Point p, string tag)
        {
            // Create a red Ellipse.
            Ellipse myEllipse = new Ellipse();

            // Create a SolidColorBrush with a red color to fill the  
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();

            // Describes the brush's color using RGB values.  
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 0);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.
            myEllipse.Width = 10;
            myEllipse.Height = 10;
            myEllipse.Tag = tag;

            Canvas.SetLeft(myEllipse, p.X-5); //Startkoordinate des Kreises ist links oben, d.h. halbe breite abziehen
            Canvas.SetTop(myEllipse, p.Y-5);

            c.Children.Add(myEllipse);
        }

        public static void DrawPlanck(Canvas c)
        {
            int cct1, cct2;
            Point p1 = new Point(); Point p2 = new Point();

            for (cct1=PILEDData.MIN_CCT, cct2=PILEDData.MIN_CCT+100; cct1< PILEDData.MAX_CCT; cct1+=100, cct2+=100)
            {
                CIECoords cie1 = Photometric.PlanckLine[cct1];
                p1.X = (float)cie1.x; p1.Y = (float)cie1.y;

                CIECoords cie2 = Photometric.PlanckLine[cct2];
                p2.X = (float)cie2.x; p2.Y = (float)cie2.y;

                AddXYLine(c, p1, p2);
            }
        }

        public static void DrawPILedTriangle(Canvas c)
        {
            AddXYLine(c, PILEDData.wl460nm, PILEDData.wl615nm);
            AddXYLine(c, PILEDData.wl615nm, PILEDData.wl560nm);
            AddXYLine(c, PILEDData.wl560nm, PILEDData.wl460nm);
        }

        private static void AddXYLine(Canvas c, Point xy1, Point xy2)
        {
            Point p1 = new Point(); Point p2 = new Point();
            Point p3 = new Point(); Point p4 = new Point();

            p1.X = xy1.X; p1.Y = xy1.Y; p2 = XyToPixel(ref p1);
            p3.X = xy2.X; p3.Y = xy2.Y; p4 = XyToPixel(ref p3);


            Line l = new Line();
            l.X1 = p2.X; l.Y1 = p2.Y;
            l.X2 = p4.X; l.Y2 = p4.Y;
            SolidColorBrush b = new SolidColorBrush();
            b.Color = Colors.Black;
            l.StrokeThickness = 2;
            l.Stroke = b;
            c.Children.Add(l);
        }

        public static void RemoveAllPoints(Canvas c)
        {
            c.Children.Remove<Ellipse>();
        }
    }
}
