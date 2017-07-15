using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace spex
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class CaptureWindow : Window
    {
        private WriteableBitmap bitmap;

        private UInt16[] pixels;

        private UInt16[] dataArray;

        private double min, max;

        public UInt16[] DataArray
        {
            set
            {
                dataArray = value;
                bitmap = new WriteableBitmap(DataProcessing.Width, dataArray.Length / DataProcessing.Width, 96, 96, PixelFormats.Gray16, null);
                pixels = new UInt16[dataArray.Length];
                if (min > max)
                {
                    minMaxRefresh();
                }
                refresh();
            }
        }

        public CaptureWindow()
        {
            Width = DataProcessing.Width;
            Height = DataProcessing.Height;
            min = UInt16.MaxValue;
            max = 1;
            InitializeComponent();
            image.MouseRightButtonDown += image_MouseRightButtonDown;
            image.Source = bitmap;
        }

        void image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            minMaxRefresh();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void refresh()
        {
            int length = dataArray.Length;

            for (int i = 0; i < length; i++)
            {
                if (dataArray[i] > max)
                {
                    pixels[i] = UInt16.MaxValue;
                }
                else if (dataArray[i] < min)
                {
                    pixels[i] = UInt16.MinValue;
                }
                else
                {
                    pixels[i] = (UInt16)((dataArray[i] - min) / (max - min) * UInt16.MaxValue);
                }
            }
            Console.WriteLine("min: " + min + " max: " + max);
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8, 0);
            image.Source = bitmap;
        }

        private void minMaxRefresh()
        {
            min = UInt16.MaxValue;
            max = 1;
            int length = dataArray.Length;
            for (int i = 0; i < length; i++)
            {
                if (dataArray[i] > max)
                {
                    max = dataArray[i];
                }
                if (dataArray[i] < min)
                {
                    min = dataArray[i];
                }
            }
            if (max - min < 1)
            {
                max = min + 1;
            }
        }
    }
}
