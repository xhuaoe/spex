using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// AutoSpexWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AutoSpexWindow : Window
    {
        #region DependencyProperties

        public static readonly DependencyProperty WaveNumFromProperty = DependencyProperty.Register(
            "WaveNumFrom", typeof(double), typeof(AutoSpexWindow));
        public double WaveNumFrom
        {
            get { return (double)GetValue(WaveNumFromProperty); }
            set { SetValue(WaveNumFromProperty, value); }
        }

        public static readonly DependencyProperty WaveNumToProperty = DependencyProperty.Register(
            "WaveNumTo", typeof(double), typeof(AutoSpexWindow));
        public double WaveNumTo
        {
            get { return (double)GetValue(WaveNumToProperty); }
            set { SetValue(WaveNumToProperty, value); }
        }

        public static readonly DependencyProperty ExpoTimeProperty = DependencyProperty.Register(
            "ExpoTime", typeof(int), typeof(AutoSpexWindow));
        public int ExpoTime
        {
            get { return (int)GetValue(ExpoTimeProperty); }
            set { SetValue(ExpoTimeProperty, value); }
        }

        public static readonly DependencyProperty OverlappedWaveNumProperty = DependencyProperty.Register(
            "OverlappedWaveNum", typeof(double), typeof(AutoSpexWindow));
        public double OverlappedWaveNum
        {
            get { return (double)GetValue(OverlappedWaveNumProperty); }
            set { SetValue(OverlappedWaveNumProperty, value); }
        }
        #endregion
        public AutoSpexWindow()
        {
            InitializeComponent();
        }

        private void startStopAcquiring_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidWindow.IsValid(this))
            {
                return;
            }
            this.DialogResult = true;
        }
    }
}
