using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Drawing;

namespace spex
{
    class Monitor : Canvas
    {

        private WriteableBitmap bitmap;

        private const double marginTop = 20;
        private const double marginBottom = 35;
        private const double marginLeft = 65;
        private const double marginRight = 20;
        private const double gridStandard = 150;
        private const double minSpan = 1e-10;
        private const double maxSpan = 1e10;
        private const int maxPixel = 10000000;
        private const int minPixel = -10000000;

        private Line xAxis, yAxis;
        private SolidColorBrush axisBrush;
        private SolidColorBrush gridBrush;
        private SolidColorBrush subGridBrush;
        private Viewbox viewBox;
        private TextBlock xLabel = new TextBlock();
        private TextBlock yLabel = new TextBlock();

        private System.Windows.Point selectStartPoint = new System.Windows.Point();
        private System.Windows.Shapes.Rectangle selectedArea = new System.Windows.Shapes.Rectangle();
        private SolidColorBrush rectBrush;
        private bool isSelecting = false;

        private List<object> fixedItems = new List<object>();

        private IEnumerable<System.Windows.Point> data = new List<System.Windows.Point>();

        #region DependencyProperties

        public static readonly DependencyProperty DisplayAreaProperty = DependencyProperty.Register(
            "DisplayArea", typeof(Rect), typeof(Monitor),
            new PropertyMetadata(
                new Rect(new System.Windows.Point(0, 0), new System.Windows.Point(1000, 1000)),
                null,
                new CoerceValueCallback(
                    (DependencyObject d, object baseValue) =>
                    {
                        bool result = ((Rect)baseValue).Width > minSpan && ((Rect)baseValue).Width < maxSpan && ((Rect)baseValue).Height > minSpan && ((Rect)baseValue).Height < maxSpan;
                        if (result)
                        {
                            return baseValue;
                        }
                        else
                        {
                            return d.GetValue(DisplayAreaProperty);
                        }
                    })));

        public Rect DisplayArea
        {
            get { return (Rect)GetValue(DisplayAreaProperty); }
            set { SetValue(DisplayAreaProperty, value); }
        }

        public static readonly DependencyProperty XLabelProperty = DependencyProperty.Register(
            "XLabel", typeof(string), typeof(Monitor), new PropertyMetadata("XLabel"));

        public string XLabel
        {
            get { return (string)GetValue(XLabelProperty); }
            set
            {
                SetValue(XLabelProperty, value);
            }
        }

        public static readonly DependencyProperty YLabelProperty = DependencyProperty.Register(
            "YLabel", typeof(string), typeof(Monitor), new PropertyMetadata("YLabel"));

        public string YLabel
        {
            get { return (string)GetValue(YLabelProperty); }
            set
            {
                SetValue(YLabelProperty, value);
            } 
        }

        public struct BasePoint
        {
            public double displayBase;
            public double dataBase;
            public BasePoint(double data, double display)
            {
                displayBase = display;
                dataBase = data;
            }
        }

        // (x of data, x for display)
        public static readonly DependencyProperty XBasePointProperty = DependencyProperty.Register(
            "XBasePoint", typeof(BasePoint), typeof(Monitor),
            new PropertyMetadata(new BasePoint(0.0, 0.0)));

        public BasePoint XBasePoint
        {
            get { return (BasePoint)GetValue(XBasePointProperty); }
            set { SetValue(XBasePointProperty, value); }
        }

        // display / data > 0
        public static readonly DependencyProperty XScaleProperty = DependencyProperty.Register(
            "XScale", typeof(double), typeof(Monitor),
            new PropertyMetadata(1.0, null, new CoerceValueCallback(
                    (DependencyObject d, object baseValue) =>
                    {
                        if ((double)baseValue > 0)
                        {
                            return baseValue;
                        }
                        else
                        {
                            return d.GetValue(XScaleProperty);
                        }
                    })));

        public double XScale
        {
            get { return (double)GetValue(XScaleProperty); }
            set { SetValue(XScaleProperty, value); }
        }

        public static readonly DependencyProperty YBasePointProperty = DependencyProperty.Register(
            "YBasePoint", typeof(BasePoint), typeof(Monitor),
            new PropertyMetadata(new BasePoint(0.0, 0.0)));

        public BasePoint YBasePoint
        {
            get { return (BasePoint)GetValue(YBasePointProperty); }
            set { SetValue(YBasePointProperty, value); }
        }

        public static readonly DependencyProperty YScaleProperty = DependencyProperty.Register(
            "YScale", typeof(double), typeof(Monitor),
            new PropertyMetadata(1.0, null, new CoerceValueCallback(
                    (DependencyObject d, object baseValue) =>
                    {
                        if ((double)baseValue > 0)
                        {
                            return baseValue;
                        }
                        else
                        {
                            return d.GetValue(YScaleProperty);
                        }
                    })));

        public double YScale
        {
            get { return (double)GetValue(YScaleProperty); }
            set { SetValue(YScaleProperty, value); }
        }

        #endregion

        public IEnumerable<System.Windows.Point> Data
        {
            get { return data; }
            set
            {
                data = value;
                //adjustToFullSize();
                //RepaintBackground(ActualWidth, ActualHeight);
                RepaintLine(data, false);
            }
        }

        public Monitor()
        {

            System.Windows.Media.Color axisColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            axisBrush = new SolidColorBrush(axisColor);

            System.Windows.Media.Color gridColor = System.Windows.Media.Color.FromRgb(34, 39, 49);
            gridBrush = new SolidColorBrush(gridColor);

            System.Windows.Media.Color subGridColor = System.Windows.Media.Color.FromRgb(28, 32, 40);
            subGridBrush = new SolidColorBrush(subGridColor);

            System.Windows.Media.Color rectColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            rectBrush = new SolidColorBrush(rectColor);

            xAxis = new Line();
            xAxis.Stroke = axisBrush;

            yAxis = new Line();
            yAxis.Stroke = axisBrush;

            Children.Add(xAxis);
            fixedItems.Add(xAxis);
            Children.Add(yAxis);
            fixedItems.Add(yAxis);

            yLabel.RenderTransform = new RotateTransform(270);
            yLabel.Foreground = axisBrush;
            SetLeft(yLabel, 5);
            Children.Add(yLabel);
            fixedItems.Add(yLabel);

            xLabel.Foreground = axisBrush;
            SetBottom(xLabel, 5);
            Children.Add(xLabel);
            fixedItems.Add(xLabel);

            selectedArea.Stroke = rectBrush;
            Children.Add(selectedArea);
            fixedItems.Add(selectedArea);
            SetZIndex(selectedArea, 2);

            viewBox = new Viewbox();
            viewBox.Child = new System.Windows.Controls.Image();
            ((System.Windows.Controls.Image)viewBox.Child).Source = bitmap;
            viewBox.Stretch = Stretch.Fill;
            Children.Add(viewBox);
            fixedItems.Add(viewBox);
            SetZIndex(viewBox, 1);

            SizeChanged += Monitor_SizeChanged;

        }

        private double arrange(double length, double minPosition, double maxPosition)
        {
            double unit = gridStandard / length * (maxPosition - minPosition);
            double p = 1;
            while (unit < 0.1)
            {
                unit *= 10;
                p *= 10;
            }
            while (unit > 1)
            {
                unit /= 10;
                p /= 10;
            }
            if (unit > 0.3 && unit < 0.75)
            {
                unit = 0.5 / p;
            }
            else if (unit < 0.5)
            {
                unit = 0.1 / p;
            }
            else
            {
                unit = 1 / p;
            }
            return unit;
        }

        private void drawAxises(double width, double height)
        {
            xAxis.X1 = marginLeft;
            xAxis.X2 = width - marginRight;
            xAxis.Y1 = xAxis.Y2 = height - marginBottom;
            yAxis.Y1 = marginTop;
            yAxis.Y2 = height - marginBottom;
            yAxis.X1 = yAxis.X2 = marginLeft;
            yLabel.Text = YLabel;
            SetBottom(yLabel, height / 2);
            xLabel.Text = XLabel;
            SetLeft(xLabel, width / 2);
            //YLabel = height.ToString();
            //XLabel = width.ToString();
        }

        private void placeViewBox(double width, double height)
        {
            SetTop(viewBox, marginTop);
            SetLeft(viewBox, marginLeft);
            viewBox.Width = width - marginLeft - marginRight;
            viewBox.Height = height - marginTop - marginBottom;
        }

        private void drawXGrids(double width, double height, double minPosition, double maxPosition)
        {
            double unit = arrange(width - marginLeft - marginRight, minPosition, maxPosition);
            double xValue = Math.Floor(minPosition / unit) * unit + unit;
            Line gridLine;
            TextBlock tb;
            /*
            tb = new TextBlock();
            tb.Text = minPosition.ToString("F");
            tb.Foreground = axisBrush;
            SetLeft(tb, marginLeft);
            SetTop(tb, height - marginBottom);
            Children.Add(tb);
            */
            for (double xPoint = marginLeft + (xValue - minPosition) / (maxPosition - minPosition) * (width - marginLeft - marginRight);
                xPoint < width - marginRight;
                xPoint += unit / (maxPosition - minPosition) * (width - marginLeft - marginRight))
            {
                gridLine = new Line();
                gridLine.X1 = gridLine.X2 = xPoint;
                gridLine.Y1 = marginTop;
                gridLine.Y2 = height - marginBottom;
                gridLine.Stroke = gridBrush;
                Children.Add(gridLine);
                tb = new TextBlock();
                tb.Text = xValue.ToString("g7");
                if (Math.Abs(xValue) / (maxPosition - minPosition) < 1e-10)
                {
                    tb.Text = "0";
                }
                tb.Foreground = axisBrush;
                SetLeft(tb, xPoint);
                SetTop(tb, height - marginBottom);
                Children.Add(tb);
                xValue += unit;
            }
        }

        private void drawYGrids(double width, double height, double minPosition, double maxPosition)
        {
            double unit = arrange(height - marginTop - marginBottom, minPosition, maxPosition);
            double yValue = Math.Floor(minPosition / unit) * unit + unit;
            Line gridLine;
            TextBlock tb;
            /*
            tb = new TextBlock();
            tb.Text = minPosition.ToString("F");
            tb.Foreground = axisBrush;
            SetRight(tb, width - marginLeft);
            SetBottom(tb, marginBottom);
            Children.Add(tb);
            */
            for (double yPoint = height - marginBottom - (yValue - minPosition) / (maxPosition - minPosition) * (height - marginTop - marginBottom);
                yPoint > marginTop;
                yPoint -= unit / (maxPosition - minPosition) * (height - marginTop - marginBottom))
            {
                gridLine = new Line();
                gridLine.Y1 = gridLine.Y2 = yPoint;
                gridLine.X1 = marginLeft;
                gridLine.X2 = width - marginRight;
                gridLine.Stroke = gridBrush;
                Children.Add(gridLine);
                tb = new TextBlock();
                tb.Text = yValue.ToString("g5");
                if (Math.Abs(yValue) / (maxPosition - minPosition) < 1e-10)
                {
                    tb.Text = "0";
                }
                tb.Foreground = axisBrush;
                SetRight(tb, width - marginLeft);
                SetBottom(tb, height - yPoint);
                Children.Add(tb);
                yValue += unit;
            }
        }

        private double dataToDisplay(BasePoint basePoint, double scale, double xData)
        {
            return (xData - basePoint.dataBase) * scale + basePoint.displayBase;
        }

        public void RepaintBackground(double width, double height)
        {
            drawAxises(width, height);
            placeViewBox(width, height);
            Children.RemoveRange(fixedItems.Count, Children.Count - fixedItems.Count);
            drawXGrids(width, height, dataToDisplay(XBasePoint, XScale, DisplayArea.Left), dataToDisplay(XBasePoint, XScale, DisplayArea.Right));
            drawYGrids(width, height, dataToDisplay(YBasePoint, YScale, DisplayArea.Top), dataToDisplay(YBasePoint, YScale, DisplayArea.Bottom));
        }

        void Monitor_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RepaintBackground(e.NewSize.Width, e.NewSize.Height);
            RepaintLine(data,
                true);
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            System.Windows.Point clickedPoint = e.GetPosition(this);
            if (clickedPoint.X > marginLeft && clickedPoint.X < this.ActualWidth - marginRight)
            {
                if (clickedPoint.Y > marginTop && clickedPoint.Y < this.ActualHeight - marginBottom)
                {
                    isSelecting = true;
                    selectStartPoint = clickedPoint;
                    SetTop(selectedArea, selectStartPoint.Y);
                    SetLeft(selectedArea,  selectStartPoint.X);
                    selectedArea.Width = 0;
                    selectedArea.Height = 0;
                    selectedArea.Visibility = Visibility.Visible;
                }
            }
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            System.Windows.Point clickedPoint = e.GetPosition(this);
            if (isSelecting)
            {
                if (clickedPoint.X > marginLeft && clickedPoint.X < this.ActualWidth - marginRight)
                {
                    SetLeft(selectedArea, Math.Min(clickedPoint.X, selectStartPoint.X));
                    selectedArea.Width = Math.Abs(clickedPoint.X - selectStartPoint.X);               
                }
                if (clickedPoint.Y > marginTop && clickedPoint.Y < this.ActualHeight - marginBottom)
                {
                    SetTop(selectedArea, Math.Min(clickedPoint.Y, selectStartPoint.Y));
                    selectedArea.Height = Math.Abs(clickedPoint.Y - selectStartPoint.Y);
                }               
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (isSelecting)
            {
                //MessageBox.Show("selecting!");
                selectedArea.Visibility = Visibility.Collapsed;
                isSelecting = false;
                if (selectedArea.Width > 1 || selectedArea.Height > 1)
                {
                    double newMinX, newMinY, newMaxX, newMaxY;
                    newMinX = (GetLeft(selectedArea) - marginLeft) / (ActualWidth - marginLeft - marginRight) * DisplayArea.Width + DisplayArea.Left;
                    newMaxX = (selectedArea.Width + GetLeft(selectedArea) - marginLeft) / (ActualWidth - marginLeft - marginRight) * DisplayArea.Width + DisplayArea.Left;
                    newMinY = (ActualHeight - GetTop(selectedArea) - selectedArea.Height - marginBottom) / (ActualHeight - marginBottom - marginTop) * DisplayArea.Height + DisplayArea.Top;
                    newMaxY = (ActualHeight - GetTop(selectedArea) - marginBottom) / (ActualHeight - marginBottom - marginTop) * DisplayArea.Height + DisplayArea.Top;
                    DisplayArea = new Rect(new System.Windows.Point(newMinX, newMinY), new System.Windows.Point(newMaxX, newMaxY));
                    RepaintBackground(ActualWidth, ActualHeight);
                    RepaintLine(data, true);
                }
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (isSelecting)
            {
                selectedArea.Visibility = Visibility.Collapsed;
                isSelecting = false;
            }
        }

        private void adjustToFullSize()
        {
            int count = data.Count();
            if (count == 0)
            {
                DisplayArea = (Rect)DisplayAreaProperty.DefaultMetadata.DefaultValue;
            }
            else if (count == 1)
            {
                double xSpan = ((Rect)DisplayAreaProperty.DefaultMetadata.DefaultValue).Width;
                double ySpan = ((Rect)DisplayAreaProperty.DefaultMetadata.DefaultValue).Height;
                DisplayArea = new Rect(
                    new System.Windows.Point(data.ElementAt(0).X - xSpan / 2, data.ElementAt(0).Y - ySpan / 2),
                    new System.Windows.Point(data.ElementAt(0).X + xSpan / 2, data.ElementAt(0).Y + ySpan / 2));
            }
            else
            {
                double newMinX, newMinY, newMaxX, newMaxY;
                newMinX = newMaxX = data.ElementAt(0).X;
                newMinY = newMaxY = data.ElementAt(0).Y;
                foreach (System.Windows.Point point in data)
                {
                    if (newMinX > point.X)
                    {
                        newMinX = point.X;
                    }
                    if (newMaxX < point.X)
                    {
                        newMaxX = point.X;
                    }
                    if (newMinY > point.Y)
                    {
                        newMinY = point.Y;
                    }
                    if (newMaxY < point.Y)
                    {
                        newMaxY = point.Y;
                    }
                }
                DisplayArea = new Rect(new System.Windows.Point(newMinX, newMinY), new System.Windows.Point(newMaxX, newMaxY));
            }
        }

        protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            adjustToFullSize();
            RepaintBackground(ActualWidth, ActualHeight);
            RepaintLine(data, true);
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            System.Windows.Point wheelPoint = e.GetPosition(this);
            wheelPoint.X -= marginLeft;
            wheelPoint.Y = ActualHeight - wheelPoint.Y - marginBottom;
            wheelPoint.X = wheelPoint.X / (ActualWidth - marginLeft - marginRight) * DisplayArea.Width + DisplayArea.Left;
            wheelPoint.Y = wheelPoint.Y / (ActualHeight - marginTop - marginBottom) * DisplayArea.Height + DisplayArea.Top;
            if (wheelPoint.X > DisplayArea.Left && wheelPoint.X < DisplayArea.Right && wheelPoint.Y > DisplayArea.Top && wheelPoint.Y < DisplayArea.Bottom)
            {
                double deltaUnit = 120;
                DisplayArea = new Rect(
                    new System.Windows.Point(
                        wheelPoint.X - (wheelPoint.X - DisplayArea.Left) * (1 - e.Delta / deltaUnit * 0.1),
                        wheelPoint.Y - (wheelPoint.Y - DisplayArea.Top) * (1 - e.Delta / deltaUnit * 0.1)),
                    new System.Windows.Point(
                        wheelPoint.X + (DisplayArea.Right - wheelPoint.X) * (1 - e.Delta / deltaUnit * 0.1),
                        wheelPoint.Y + (DisplayArea.Bottom - wheelPoint.Y) * (1 - e.Delta / deltaUnit * 0.1)));
                RepaintBackground(ActualWidth, ActualHeight);
                RepaintLine(data, false);
            }
        }

        // source: blog.csdn.net/waleswood/article/details/21744131
        // bug remained: precision loss at very small scale: wrong inclination or disappearance
        public void RepaintLine(IEnumerable<System.Windows.Point> data, bool needResize)
        {
            int count = data.Count();
            double minX = DisplayArea.Left, maxX = DisplayArea.Right, minY = DisplayArea.Top, maxY = DisplayArea.Bottom;
            if (needResize || bitmap == null)
            {
                bitmap = new WriteableBitmap(
                    (int)ActualWidth,
                    (int)ActualHeight,
                    96, 96, PixelFormats.Pbgra32, null);
            }

            System.Drawing.Point[] points = new System.Drawing.Point[count];
            int i = 0;
            double tempX, tempY;
            foreach (System.Windows.Point point in data)
            {
                tempX = (point.X - minX) / (maxX - minX) * bitmap.PixelWidth;
                tempY = (maxY - point.Y) / (maxY - minY) * bitmap.PixelHeight;
                points[i].X = (int)((point.X - minX) / (maxX - minX) * bitmap.PixelWidth);
                points[i].Y = (int)((maxY - point.Y) / (maxY - minY) * bitmap.PixelHeight);

                if (tempX < minPixel) { points[i].X = minPixel; }
                else if (tempX > maxPixel) { points[i].X = maxPixel; }
                if (tempY < minPixel) { points[i].Y = minPixel; }
                else if (tempY > maxPixel) { points[i].Y = maxPixel; }
                    
                i++;
            }
            bitmap.Lock();

            using (Bitmap backBufferBitmap = new Bitmap(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.BackBuffer))
            {
                using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                {
                    backBufferGraphics.Clear(System.Drawing.Color.FromArgb(0));
                    if (count > 1) backBufferGraphics.DrawLines(System.Drawing.Pens.Gold, points);
                    else if (count == 1) backBufferGraphics.DrawEllipse(System.Drawing.Pens.Gold,
                        new RectangleF(points[0].X - 0.5f, points[0].Y - 0.5f, 1, 1));
                    backBufferGraphics.Flush();
                }
            }

            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();

            ((System.Windows.Controls.Image)viewBox.Child).Source = bitmap;
        }
    }
}
