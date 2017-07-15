using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace spex
{
    public class DataProcessing
    {
        public const int Width = 2048, Height = 512;
        private UInt16[] dataArray;
        private Point[] pointArray;
        private double[] yArray;

        public static double XDataBase = 0;
        public static double XScale = 1;
        public static double YDataBase = 0;
        public static double YScale = 1;
        public static double YDisplayBase = 0;

        public UInt16[] DataArray
        {
            get { return dataArray; }
        }

        public Point[] PointArray
        {
            get { return pointArray; }
        }

        public DataProcessing(bool spec)
        {
            if (spec)
            {
                dataArray = new UInt16[Width];
            }
            else
            {
                dataArray = new UInt16[Width * Height];
            }
            pointArray = new Point[Ccd.ROITo - Ccd.ROIFrom];
            /*
            for (int i = 0; i < Width; i++)
            {
                pointArray[i].X = i;
            }
             */
            yArray = new double[Width];
        }

        public void GetList(double XDisplayBase)// use this function only when spec == true
        {
            for (int i = Ccd.ROIFrom; i < Ccd.ROITo; i++)
            {
                pointArray[i - Ccd.ROIFrom].X = (i - XDataBase) * XScale + XDisplayBase;
                pointArray[i - Ccd.ROIFrom].Y = (dataArray[i] - YDataBase) * YScale + YDisplayBase;
            }
        }

        public void Append(List<Point> source)
        {
            // no checking for overflow; rely on exceptions
            int firstGreaterI = source.FindIndex(new Predicate<Point>(
                (Point p) =>
                {
                    return p.X > pointArray[0].X;
                }));
            if (firstGreaterI == -1)
            {
                foreach (Point p in pointArray)
                {
                    source.Add(p);
                }
            }
            else
            {
                int scount = source.Count(), pcount = pointArray.Count();
                double l, r;
                l = (source[firstGreaterI].X - pointArray[0].X) / XScale;
                r = 1.0 - l;
                source[firstGreaterI - 1] = new Point(source[firstGreaterI - 1].X,
                    (source[firstGreaterI - 1].Y + l * pointArray[0].Y) / (1.0 + l));
                int i;
                for (i = 0; i + firstGreaterI < scount; i++)
                {
                    pointArray[i].Y = (pointArray[i].Y * r + pointArray[i + 1].Y * l);
                    source[firstGreaterI + i] = new Point(source[firstGreaterI + i].X,
                    (source[firstGreaterI + i].Y + pointArray[i].Y) / 2);
                }
                for (; i < pcount - 1; i++)
                {
                    pointArray[i].X += r * XScale;
                    pointArray[i].Y = (pointArray[i].Y * r + pointArray[i + 1].Y * l);
                    source.Add(pointArray[i]);
                }
            }
        }
    }
}