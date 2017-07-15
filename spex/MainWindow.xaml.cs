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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Configuration;
using System.Xml;

namespace spex
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DependencyProperties
        public double SpexPos
        {
            get { return (double)GetValue(SpexPosProperty); }
            set
            {
                SetValue(SpexPosProperty, value);
            }
        }
        public static readonly DependencyProperty SpexPosProperty = DependencyProperty.Register(
            "SpexPos", typeof(double), typeof(MainWindow));
        #endregion

        private CaptureWindow cw;

        private Thread getTempThread;

        private bool getTemp;

        public MainWindow()
        {
            Move.Init();
            PortIO.Init();

            capturing = false;
            specCapturing = true;
            moving = false;
            acquiring = false;
            acquire = false;
            getTemp = true;

            InitializeComponent();
            /////////////////////////////////
            monitor.XLabel = "wavenumber/cm-1";
            /////////////////////////////////

            if (!Ccd.Init())
            {
                MessageBox.Show("CCD初始化失败", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                startStopCapture.IsEnabled = false;
                autoSpex.IsEnabled = false;
                getTemp = false;
            }

            double t;
            if (!double.TryParse(ConfigurationManager.AppSettings["spexPos"], out t))
            {
                SpexPos = 0;
            }
            else
            {
                SpexPos = t;
            }

            getTempThread = new Thread(new ThreadStart(
                () =>
                {
                    while (getTemp)
                    {
                        Thread.Sleep(500);
                        this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                        {
                            if (!temperature.IsFocused)
                            {
                                temperature.Content = (Ccd.GetTemp() / 100.0).ToString();
                            }
                        });
                    }
                }));
            getTempThread.Start();
        }

        private void camara_Click_1(object sender, RoutedEventArgs e)
        {
            if (cw == null)
            {
                cw = new CaptureWindow();
                cw.Owner = this;
            }
            cw.Show();
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "Text Files(*.txt)|*.txt";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<Point> points = new List<Point>();
                bool readSuccessful = true;
                StreamReader reader = new StreamReader(dlg.FileName);
                string[] pointXY;
                char[] separator = new char[] { ' ' };
                double temp;
                while (!reader.EndOfStream)
                {
                    Point point = new Point();
                    pointXY = reader.ReadLine().Split(separator, 2);
                    if (pointXY.Length != 2)
                    {
                        MessageBox.Show(
                            "数据格式错误。每行必须有两个数",
                            this.Title,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        readSuccessful = false;
                        break;
                    }
                    if (double.TryParse(pointXY[0], out temp))
                    {
                        point.X = temp;
                        if (double.TryParse(pointXY[1], out temp))
                        {
                            point.Y = temp;
                            points.Add(point);
                        }
                        else
                        {
                            MessageBox.Show(
                                "无法解析数据。请检查是否有输入错误",
                                this.Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            readSuccessful = false;
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                                "无法解析数据。请检查是否有输入错误",
                                this.Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        readSuccessful = false;
                        break;
                    }
                }
                reader.Close();
                if (readSuccessful)
                {
                    monitor.Data = points;
                }
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.DefaultExt = "txt";
            dlg.Filter = "Text Files(*.txt)|*.txt";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                foreach (Point point in monitor.Data)
                {
                    writer.WriteLine(point.X + " " + point.Y);
                }
                writer.Close();
            }
        }

        private Thread getFrameThread;

        private bool capturing;

        private bool specCapturing;

        private void startStopCapture_Click(object sender, RoutedEventArgs e)
        {
            if (!capturing)
            {
                capturing = true;
                startStopCapture.Content = "停止";
                startStopCapture.ToolTip = "Stop";
                UInt32 size = Ccd.Setup(specCapturing, (UInt32)Ccd.ExpoTime);
                if (size == 0)
                {
                    MessageBox.Show("无法初始化sequence", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    startStopCapture.Content = "监测";
                    startStopCapture.ToolTip = "Start";
                    capturing = false;
                }
                else
                {
                    bool specNow = specCapturing;
                    getFrameThread = new Thread(new ThreadStart(
                        () =>
                        {
                            DataProcessing dp = new DataProcessing(specNow);
                            while (capturing)
                            {
                                Thread.Sleep(Ccd.ExpoTime);
                                Ccd.GetFrame(dp.DataArray);
                                this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                                {
                                    if (specNow)
                                    {
                                        dp.GetList(SpexPos);
                                        monitor.Data = dp.PointArray;
                                    }
                                    if (cw != null && cw.IsVisible)
                                    {
                                        cw.DataArray = dp.DataArray;
                                    }
                                });
                                if (specNow != specCapturing)
                                {
                                    Ccd.Stop();
                                    if (Ccd.Setup(specCapturing, (UInt32)Ccd.ExpoTime) == 0)
                                    {
                                        break;
                                    }
                                    specNow = specCapturing;
                                    dp = new DataProcessing(specNow);
                                }
                            }
                            Ccd.Stop();
                            this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                            {
                                startStopCapture.Content = "监测";
                                startStopCapture.ToolTip = "Start";
                            });
                        }));
                    getFrameThread.Start();
                }
            }
            else
            {
                capturing = false;
            }
        }

        private void displaySettings_Click(object sender, RoutedEventArgs e)
        {
            DisplaySettingsWindow displaySettingsWindow = new DisplaySettingsWindow();
            displaySettingsWindow.Owner = this;
            displaySettingsWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            displaySettingsWindow.XLabel = monitor.XLabel;
            displaySettingsWindow.YLabel = monitor.YLabel;
            displaySettingsWindow.XDataBase = DataProcessing.XDataBase;
            displaySettingsWindow.YDataBase = DataProcessing.YDataBase;
            displaySettingsWindow.XDisplayBase = SpexPos;
            displaySettingsWindow.YDisplayBase = DataProcessing.YDisplayBase;
            displaySettingsWindow.XScale = DataProcessing.XScale;
            displaySettingsWindow.YScale = DataProcessing.YScale;
            displaySettingsWindow.ShowDialog();
            if (displaySettingsWindow.DialogResult == true)
            {
                monitor.XLabel = displaySettingsWindow.XLabel;
                monitor.YLabel = displaySettingsWindow.YLabel;
                DataProcessing.XDataBase = displaySettingsWindow.XDataBase;
                DataProcessing.YDataBase = displaySettingsWindow.YDataBase;
                SpexPos = displaySettingsWindow.XDisplayBase;
                DataProcessing.YDisplayBase = displaySettingsWindow.YDisplayBase;
                DataProcessing.XScale = displaySettingsWindow.XScale;
                DataProcessing.YScale = displaySettingsWindow.YScale;
                monitor.RepaintBackground(monitor.ActualWidth, monitor.ActualHeight);
            }
        }

        private void openShuttle_Click(object sender, RoutedEventArgs e)
        {
            Shutter.Open();
        }

        private void closeShuttle_Click(object sender, RoutedEventArgs e)
        {
            Shutter.Close();
        }

        private Thread moveThread, moveProgressThread;

        private bool moving;

        private void startStopMoving_Click(object sender, RoutedEventArgs e)
        {
            if (moving)
            {
                Move.Stop();
            }
            else
            {
                moving = true;
                startStopMoving.Content = "停止";
                startStopMoving.ToolTip = "Stop moving";
                double start, end;
                start = SpexPos;
                if (double.TryParse(spexPos.Text, out end))
                {
                    moveAndProgress(start, end);
                }
                else
                {
                    spexPos.Focus();
                    startStopMoving.Content = "开始";
                    startStopMoving.ToolTip = "Start moving";
                    moving = false;
                }
            }
        }

        private void moveAndProgress(double start, double end)
        {
            moveThread = new Thread(new ParameterizedThreadStart(
                    (object step) =>
                    {
                        Move.MoveStep((double)step);
                        if (moving)
                        {
                            this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                            {
                                startStopMoving.Content = "开始";
                                startStopMoving.ToolTip = "Start moving";
                                moving = false;
                            });
                        }
                    }));
            double pos = SpexPos;
            moveProgressThread = new Thread(new ThreadStart(
                () =>
                {
                    while (moveThread != null && moveThread.ThreadState == ThreadState.Running)
                    {
                        Thread.Sleep(250);
                        this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                        {
                            SpexPos = pos + Move.Progress();
                            //spexMoveTo.Text = ((int)Move.SpexPos).ToString();
                        });
                    }
                }));
            moveThread.Start(end - start);
            moveProgressThread.Start();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (capturing)
            {
                capturing = false;
                getFrameThread.Join();
            }
            if (getTemp)
            {
                getTemp = false;
                getTempThread.Join();
            }
            if (moving)
            {
                Move.Stop();
                moveThread.Join();
                moving = false;
                moveProgressThread.Join();
            }
            saveConfig("spexPos", SpexPos.ToString());
            PortIO.Uninit();
            Ccd.Uninit();
        }

        private void saveConfig(string key, string value)
        {
            ConfigXmlDocument doc = new ConfigXmlDocument();
            string fileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            doc.Load(fileName);
            XmlNodeList nodes = doc.GetElementsByTagName("add");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlAttribute att = nodes[i].Attributes["key"];
                if (att.Value == key)
                {
                    att = nodes[i].Attributes["value"];
                    att.Value = value;
                    break;
                }
            }
            doc.Save(fileName);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private Thread autoSpexThread;

        private bool acquiring;
        private bool acquire;

        private void autoSpex_Click(object sender, RoutedEventArgs e)
        {
            if (!acquiring)
            {
                AutoSpexWindow autoSpexWindow = new AutoSpexWindow();
                autoSpexWindow.Owner = this;
                autoSpexWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;

                autoSpexWindow.ShowDialog();
                if (autoSpexWindow.DialogResult == true)
                {
                    acquiring = true;
                    acquire = true;
                    autoSpex.Content = "停止采谱";
                    autoSpex.ToolTip = "Stop acquiring";
                    startStopMoving.IsEnabled = false;
                    startStopCapture.IsEnabled = false;
                    displaySettings.IsEnabled = false;
                    hardwareSettings.IsEnabled = false;

                    Move.Stop();
                    capturing = false;

                    double waveNumFrom, waveNumTo, waveNumStep, waveNumDisInit, waveNumNow;
                    int overlappedPixel;
                    int expoTime;

                    waveNumFrom = autoSpexWindow.WaveNumFrom;
                    waveNumTo = autoSpexWindow.WaveNumTo;
                    waveNumStep = (Ccd.ROITo - Ccd.ROIFrom) * DataProcessing.XScale - autoSpexWindow.OverlappedWaveNum;
                    overlappedPixel = (int)(autoSpexWindow.OverlappedWaveNum / monitor.XScale + 0.5);
                    waveNumDisInit = waveNumFrom - (Ccd.ROIFrom - DataProcessing.XDataBase) * DataProcessing.XScale - SpexPos;
                    expoTime = autoSpexWindow.ExpoTime;
                    waveNumNow = SpexPos;

                    autoSpexThread = new Thread(new ThreadStart(
                        () =>
                        {
                            List<Point> data = new List<Point>();
                            if (moveThread != null)
                            {
                                moveThread.Join();
                            }
                            if (moveProgressThread != null)
                            {
                                moveProgressThread.Join();
                            }
                            if (getFrameThread != null)
                            {
                                getFrameThread.Join();
                            }

                            DataProcessing dp = new DataProcessing(true);
                            this.Dispatcher.Invoke((ThreadStart)delegate()
                                {
                                    moveAndProgress(waveNumNow, waveNumNow + waveNumDisInit);
                                });
                            moveThread.Join();
                            waveNumNow += waveNumDisInit;
                            if (Ccd.Setup(true, (UInt32)expoTime) != 0)
                            {
                                Ccd.GetFrame(dp.DataArray);
                                Ccd.Stop();
                                this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                                {
                                    dp.GetList(SpexPos);
                                    monitor.Data = new List<Point>(dp.PointArray);
                                });
                                waveNumFrom += waveNumStep;
                                while (acquire && waveNumFrom < waveNumTo)
                                {
                                    //MessageBox.Show(waveNumNow.ToString());
                                    this.Dispatcher.Invoke((ThreadStart)delegate()
                                    {
                                        moveAndProgress(waveNumNow, waveNumNow + waveNumStep);
                                    });
                                    moveThread.Join();
                                    waveNumNow += waveNumStep;
                                    if (Ccd.Setup(true, (UInt32)expoTime) == 0)
                                    {
                                        MessageBox.Show("无法初始化sequence", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                                        break;
                                    }
                                    Ccd.GetFrame(dp.DataArray);
                                    Ccd.Stop();
                                    this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                                    {
                                        dp.GetList(SpexPos);
                                        dp.Append((System.Collections.Generic.List<Point>)monitor.Data);
                                        monitor.RepaintLine(monitor.Data, false);
                                    });
                                    waveNumFrom += waveNumStep;
                                }
                            }

                            this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                            {
                                MessageBox.Show(SpexPos.ToString());
                                displaySettings.IsEnabled = true;
                                hardwareSettings.IsEnabled = true;
                                startStopCapture.IsEnabled = true;
                                startStopMoving.IsEnabled = true;
                                autoSpex.Content = "自动采谱";
                                autoSpex.ToolTip = "Start acquiring";
                            });
                            acquiring = false;
                        }));
                    autoSpexThread.Start();
                }
            }
            else
            {
                acquire = false;
                Move.Stop();
            }
        }

        private void hardwareSettings_Click(object sender, RoutedEventArgs e)
        {
            HardwareSettingsWindow hardwareSettingsWindow = new HardwareSettingsWindow();
            hardwareSettingsWindow.Owner = this;
            hardwareSettingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            hardwareSettingsWindow.SpexPos = SpexPos;
            hardwareSettingsWindow.Temp = Ccd.Temp;
            hardwareSettingsWindow.ExpoTime = Ccd.ExpoTime;
            hardwareSettingsWindow.ROIFrom = Ccd.ROIFrom;
            hardwareSettingsWindow.ROITo = Ccd.ROITo;

            hardwareSettingsWindow.ShowDialog();
            if (hardwareSettingsWindow.DialogResult == true)
            {
                if (hardwareSettingsWindow.SpexPos != SpexPos)
                {
                    if (moving)
                    {
                        MessageBox.Show("请等待电机停止转动", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        SpexPos = hardwareSettingsWindow.SpexPos;
                    }
                }
                //if (hardwareSettingsWindow.Temp > -100 && hardwareSettingsWindow.Temp < 40)
                {
                    Ccd.SetTemp((Int16)(hardwareSettingsWindow.Temp * 100));
                    Ccd.Temp = hardwareSettingsWindow.Temp;
                }
                if (Ccd.ExpoTime != hardwareSettingsWindow.ExpoTime || Ccd.ROIFrom != hardwareSettingsWindow.ROIFrom || Ccd.ROITo != hardwareSettingsWindow.ROITo)
                {
                    if (capturing)
                    {
                        MessageBox.Show("请等待CCD停止读取数据", this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        Ccd.ExpoTime = hardwareSettingsWindow.ExpoTime;
                        Ccd.ROIFrom = hardwareSettingsWindow.ROIFrom;
                        Ccd.ROITo = hardwareSettingsWindow.ROITo;
                    }
                }
            }
        }

        private void comboBoxItemSpec_Selected(object sender, RoutedEventArgs e)
        {
            specCapturing = true;
        }

        private void comboBoxItemImage_Selected(object sender, RoutedEventArgs e)
        {
            specCapturing = false;
        }

        private void singlePointMode_Click(object sender, RoutedEventArgs e)
        {
            Move.Stop();
            capturing = false;

            double wavenumFrom = -100, wavenumTo = 100, wavenumStep = 1;
            int column = (int)DataProcessing.XDataBase;
            uint expoTime = (uint)Ccd.ExpoTime;
            double end = SpexPos + wavenumTo;
            double wavenumNow = SpexPos;

            Thread thread = new Thread(new ThreadStart(
                () =>
                    {
                        this.Dispatcher.Invoke((ThreadStart)delegate()
                                {
                                    moveAndProgress(SpexPos, SpexPos + wavenumFrom);
                                });
                        moveThread.Join();
                        wavenumNow += wavenumFrom;
                        DataProcessing dp = new DataProcessing(true);
                        Ccd.Setup(true, expoTime);
                        Ccd.GetFrame(dp.DataArray);
                        Ccd.Stop();
                        this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                        {
                            dp.GetList(SpexPos);
                            monitor.Data = new List<Point>(dp.PointArray);
                        });
                        while (wavenumNow < end)
	                    {
                            this.Dispatcher.Invoke((ThreadStart)delegate()
                            {
                                moveAndProgress(SpexPos, SpexPos + wavenumStep);
                            });
                            moveThread.Join();
                            wavenumNow += wavenumStep;
                            Ccd.Setup(true, expoTime);
                            Ccd.GetFrame(dp.DataArray);
                            Ccd.Stop();
                            this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                            {
                                dp.GetList(SpexPos);
                                dp.Append((System.Collections.Generic.List<Point>)monitor.Data);
                                monitor.RepaintLine(monitor.Data, false);
                            });
	                    }
                    }));
            thread.Start();
        }
    }
}
