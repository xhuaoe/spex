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
    /// HardwareSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HardwareSettingsWindow : Window
    {
        #region DependencyProperties
        public static readonly DependencyProperty SpexPosProperty = DependencyProperty.Register(
            "SpexPos", typeof(double), typeof(HardwareSettingsWindow));
        public double SpexPos
        {
            get { return (double)GetValue(SpexPosProperty); }
            set { SetValue(SpexPosProperty, value); }
        }

        public static readonly DependencyProperty TempProperty = DependencyProperty.Register(
            "Temp", typeof(double), typeof(HardwareSettingsWindow));
        public double Temp
        {
            get { return (double)GetValue(TempProperty); }
            set { SetValue(TempProperty, value); }
        }

        public static readonly DependencyProperty ExpoTimeProperty = DependencyProperty.Register(
            "ExpoTime", typeof(int), typeof(HardwareSettingsWindow));
        public int ExpoTime
        {
            get { return (int)GetValue(ExpoTimeProperty); }
            set { SetValue(ExpoTimeProperty, value); }
        }

        public static readonly DependencyProperty ROIFromProperty = DependencyProperty.Register(
            "ROIFrom", typeof(int), typeof(HardwareSettingsWindow));
        public int ROIFrom
        {
            get { return (int)GetValue(ROIFromProperty); }
            set { SetValue(ROIFromProperty, value); }
        }

        public static readonly DependencyProperty ROIToProperty = DependencyProperty.Register(
            "ROITo", typeof(int), typeof(HardwareSettingsWindow));
        public int ROITo
        {
            get { return (int)GetValue(ROIToProperty); }
            set { SetValue(ROIToProperty, value); }
        }
        #endregion
        public HardwareSettingsWindow()
        {
            InitializeComponent();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!ValidWindow.IsValid(this))
            {
                return;
            }
            this.DialogResult = true;
        }
    }
}
