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
    /// DisplaySettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DisplaySettingsWindow : Window
    {

        #region DependencyProperties

        public static readonly DependencyProperty XDataBaseProperty = DependencyProperty.Register(
            "XDataBase", typeof(double), typeof(DisplaySettingsWindow));

        public double XDataBase
        {
            get { return (double)GetValue(XDataBaseProperty); }
            set { SetValue(XDataBaseProperty, value); }
        }

        public static readonly DependencyProperty YDataBaseProperty = DependencyProperty.Register(
            "YDataBase", typeof(double), typeof(DisplaySettingsWindow));

        public double YDataBase
        {
            get { return (double)GetValue(YDataBaseProperty); }
            set { SetValue(YDataBaseProperty, value); }
        }

        public static readonly DependencyProperty XDisplayBaseProperty = DependencyProperty.Register(
            "XDisplayBase", typeof(double), typeof(DisplaySettingsWindow));

        public double XDisplayBase
        {
            get { return (double)GetValue(XDisplayBaseProperty); }
            set { SetValue(XDisplayBaseProperty, value); }
        }

        public static readonly DependencyProperty YDisplayBaseProperty = DependencyProperty.Register(
            "YDisplayBase", typeof(double), typeof(DisplaySettingsWindow));

        public double YDisplayBase
        {
            get { return (double)GetValue(YDisplayBaseProperty); }
            set { SetValue(YDisplayBaseProperty, value); }
        }

        public static readonly DependencyProperty XLabelProperty = DependencyProperty.Register(
            "XLabel", typeof(string), typeof(DisplaySettingsWindow));

        public string XLabel
        {
            get { return (string)GetValue(XLabelProperty); }
            set { SetValue(XLabelProperty, value); }
        }

        public static readonly DependencyProperty YLabelProperty = DependencyProperty.Register(
            "YLabel", typeof(string), typeof(DisplaySettingsWindow));

        public string YLabel
        {
            get { return (string)GetValue(YLabelProperty); }
            set { SetValue(YLabelProperty, value); }
        }

        public static readonly DependencyProperty XScaleProperty = DependencyProperty.Register(
            "XScale", typeof(double), typeof(DisplaySettingsWindow));

        public double XScale
        {
            get { return (double)GetValue(XScaleProperty); }
            set { SetValue(XScaleProperty, value); }
        }

        public static readonly DependencyProperty YScaleProperty = DependencyProperty.Register(
            "YScale", typeof(double), typeof(DisplaySettingsWindow));

        public double YScale
        {
            get { return (double)GetValue(YScaleProperty); }
            set { SetValue(YScaleProperty, value); }
        }
        #endregion

        public DisplaySettingsWindow()
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
