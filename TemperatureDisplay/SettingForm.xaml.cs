using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Device.Location;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using NLog;
using JWeather.Properties;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для SettingForm.xaml
    /// </summary>
    public partial class SettingForm : Window
    {
        
        Brush currentBrush;
        Color currentColor;
        Logger logger;
        DoubleAnimation opacityAnimationOpen, opacityAnimationClose, positionOpen, positionClose;
        public SettingForm()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            SetBackgroundColor();
            Opacity = 0;
            logger = LogManager.GetCurrentClassLogger();
            whatsNewFile = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\History.txt";
        }
        protected override void OnClosed(EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= this.SystemParameters_StaticPropertyChanged;
            base.OnClosed(e);
            
        }
        private void SettingsForm_Loaded(object sender, RoutedEventArgs e)
        {
            positionOpen = new DoubleAnimation(Top + 200.0, Top, new Duration(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            positionClose = new DoubleAnimation(Top, Top + 200.0, new Duration(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };
            positionClose.Completed += (s, a) =>
            {
                Close();
            };
            opacityAnimationOpen = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            opacityAnimationClose = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(400)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };
            positionOpen.Completed += (s, a) =>
             {
                 Activate();
             };
            AnimateWindow(1);
            
        }
        private void SetBackgroundColor()
        {
            currentBrush = SystemParameters.WindowGlassBrush;
            SettingsLayout.Background = currentBrush;
            currentColor = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
            currentColor.ScA = 0.5F;
            SelectItem(currentTab);
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                this.SetBackgroundColor();
            }
        }
        public void UpdatePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ListPorts.Items.Add("Авто");
            foreach (string port in ports)
            {
                ListPorts.Items.Add(port);
            }
            if (ListPorts.Items.Count > ports.Length + 1)
            {
                ListPorts.Items.Clear();
                UpdatePorts();
            }
        }
        string whatsNewFile;
        public int currentTab = 0;
        
        public void SelectItem(int index)
        {
            currentTab = index;
            switch (index)
            {
                case 0:
                    {
                        optionMain.Visibility = Visibility.Visible;

                        optionFS1.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;
                        ButtonAbout.Background = Brushes.Transparent;
                        serialEnabled = false;
                        autorunCheckbox.IsChecked = Settings.Default.Autorun;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonBasic.Background = currentBrush;
                        labelBasic.Foreground = new SolidColorBrush(Colors.White);
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonCommands.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        UpdatePorts();
                        ListPorts.SelectedItem = Settings.Default.defaultPort;

                        notifyMinTempCheckBox.IsChecked = Settings.Default.notifyMinTemp;
                        sliderNotifyMinTemp.IsEnabled = Settings.Default.notifyMinTemp; 
                        notifyMaxTempCheckBox.IsChecked = Settings.Default.notifyMaxTemp;
                        sliderNotifyMaxTemp.IsEnabled = Settings.Default.notifyMaxTemp;

                        sliderNotifyMinTemp.Maximum = Settings.Default.maxNotificationTemp - 1;
                        sliderNotifyMinTemp.Value = Settings.Default.minNotificationTemp;
                        sliderNotifyMaxTemp.Minimum = Settings.Default.minNotificationTemp + 1;
                        sliderNotifyMaxTemp.Value = Settings.Default.maxNotificationTemp;
                        LabelMinTempNotify.Content = "Минимальная температура (" + sliderNotifyMinTemp.Value.ToString() + "°)";
                        indicatorMin2.Content = (sliderNotifyMinTemp.Value + 1).ToString() + "°";
                        LabelMaxTempNotify.Content = "Максимальная температура (" + sliderNotifyMaxTemp.Value.ToString() + "°)";
                        indicatorMax.Content = (sliderNotifyMaxTemp.Value - 1).ToString() + "°";

                        switch (Settings.Default.tempChangeTime)
                        {
                            case 15:
                                {
                                    changeTimeComboBox.SelectedIndex = 0;
                                }break;
                            case 30:
                                {
                                    changeTimeComboBox.SelectedIndex = 1;
                                } break;
                            case 60:
                                {
                                    changeTimeComboBox.SelectedIndex = 2;
                                } break;
                        }
                    }break;
                case 1:
                    {
                        optionFS1.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelBasic.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;

                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonCommands.Background = Brushes.Transparent;
                        sliderFS1.Value = Settings.Default.opacityTemp;
                        voiceTempCheckBox.IsChecked = Settings.Default.voiceTemp;
                        voiceHumCheckBox.IsChecked = Settings.Default.voiceHum;
                        voiceMaxTempCheckBox.IsChecked = Settings.Default.voiceMaxTemp;
                        voiceMinTempCheckBox.IsChecked = Settings.Default.voiceMinTemp;
                        voiceMaxHumCheckBox.IsChecked = Settings.Default.voiceMaxHum;
                        voiceMinHumCheckBox.IsChecked = Settings.Default.voiceMinHum;
                        hideRecalledTemp.IsChecked = Settings.Default.hideWhenRecalledTemp;
                        sliderTemp.Value = Settings.Default.delayTemp;
                        serialEnabled = false;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        ButtonFS1.Background = currentBrush;
                        labelFS1.Foreground = new SolidColorBrush(Colors.White);
                    } break;
                case 2:
                    {
                        optionFS2.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;

                        sliderFS2.Value = Settings.Default.opacityTime;
                        voiceTimeCheckBox.IsChecked = Settings.Default.voiceTime;
                        voiceDOWCheckBox.IsChecked = Settings.Default.voiceDoW;
                        voiceDateCheckBox.IsChecked = Settings.Default.voiceDate;
                        hideRecalledTime.IsChecked = Settings.Default.hideWhenRecalledTime;
                        sliderTime.Value = Settings.Default.delayTime;
                        serialEnabled = false;
                        ButtonCommands.Background = Brushes.Transparent;
                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        ButtonFS2.Background = currentBrush;
                        labelFS2.Foreground = new SolidColorBrush(Colors.White);
                    } break;
                case 3:
                    {
                        optionVolume.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;

                        serialEnabled = false;
                        sliderV.Value = Settings.Default.opacityVolume;
                        sliderVolumeTime.Value = Settings.Default.delayVolume / 1000f;
                        ButtonCommands.Background = Brushes.Transparent;
                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = currentBrush;
                        labelVolume.Foreground = new SolidColorBrush(Colors.White);
                    } break;
                case 4:
                    {
                        optionMonitoring.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush; 

                        serialEnabled = true;
                        ButtonCommands.Background = Brushes.Transparent;
                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = currentBrush;
                        labelMonitoring.Foreground = new SolidColorBrush(Colors.White);
                    } break;
                case 5:
                    {
                        optionAbout.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;

                        serialEnabled = false;
                        ButtonCommands.Background = Brushes.Transparent;
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        ButtonAbout.Background = currentBrush;
                        labelAbout.Foreground = new SolidColorBrush(Colors.White);
                        VersionInfoLabel.Content = helper.GetCurrentVersion();
                        if (File.Exists(whatsNewFile))
                        {
                            FileStream fStream;
                            TextRange range;
                            range = new TextRange(whatsNew.Document.ContentStart, whatsNew.Document.ContentEnd);
                            fStream = new FileStream(whatsNewFile, FileMode.OpenOrCreate);
                            range.Load(fStream, DataFormats.Text);
                            fStream.Close();
                            whatsNew.Document.LineHeight = 5;
                            whatsNew.IsReadOnly = true;
                        }
                        else
                        {
                            MessageBox.Show("History.txt не найден!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } break;
                case 6:
                    {
                        optionComands.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionWeather.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelWeather.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;

                        serialEnabled = false;
                        ButtonCommands.Background = currentBrush;
                        labelComand.Foreground = new SolidColorBrush(Colors.White);
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonWeather.Background = Brushes.Transparent;
                        TextBoxCTemp.Text = Settings.Default.comandTemp;
                        TextBoxCTime.Text = Settings.Default.comandTime;
                        TextBoxCSpace.Text = Settings.Default.comandSpace;
                        TextBoxCVolumeUp.Text = Settings.Default.comandVolumeUp;
                        TextBoxCVolumeDown.Text = Settings.Default.comandVolumeDown;
                        TextBoxCF11.Text = Settings.Default.comandF11;
                        TextBoxCMenu.Text = Settings.Default.comandMenu;
                        TextBoxCWeather.Text = Settings.Default.comandWeather;
                    }break;
                case 7:
                    {
                        optionWeather.Visibility = Visibility.Visible;

                        optionMain.Visibility = Visibility.Hidden;
                        optionFS1.Visibility = Visibility.Hidden;
                        optionMonitoring.Visibility = Visibility.Hidden;
                        optionVolume.Visibility = Visibility.Hidden;
                        optionFS2.Visibility = Visibility.Hidden;
                        optionComands.Visibility = Visibility.Hidden;
                        optionAbout.Visibility = Visibility.Hidden;

                        labelFS1.Foreground = currentBrush;
                        labelFS2.Foreground = currentBrush;
                        labelAbout.Foreground = currentBrush;
                        labelMonitoring.Foreground = currentBrush;
                        labelVolume.Foreground = currentBrush;
                        labelBasic.Foreground = currentBrush;
                        labelComand.Foreground = currentBrush;

                        serialEnabled = false;
                        ButtonWeather.Background = currentBrush;
                        labelWeather.Foreground = new SolidColorBrush(Colors.White);
                        ButtonFS2.Background = Brushes.Transparent;
                        ButtonFS1.Background = Brushes.Transparent;
                        ButtonBasic.Background = Brushes.Transparent;
                        ButtonChangeTemp.Background = Brushes.Transparent;
                        ButtonMonitoring.Background = Brushes.Transparent;
                        ButtonAbout.Background = Brushes.Transparent;
                        ButtonCommands.Background = Brushes.Transparent;
                        sliderWeatherTimeUpdate.Value = Settings.Default.weatherUpdate;
                        autoLocation.IsChecked = Settings.Default.AutoLocation;
                        if (Settings.Default.AutoLocation)
                        {
                            getLocation();
                            LatLonGrid.Visibility = Visibility.Hidden;
                            if (helper.GetConnectionAviable() == 1)
                            {
                                autoLocation.IsEnabled = true;
                            }
                            else
                            {
                                coordinatesLabel.Content = "Опция доступна только при активном соединении к интернету.";
                                autoLocation.IsEnabled = false;
                            }
                        }
                        else
                        {
                            if (helper.GetConnectionAviable() == 1)
                            {
                                coordinatesLabel.Content = "Местоположение не определено";
                                autoLocation.IsEnabled = true;
                                coorButton.Visibility = Visibility.Visible;
                                latEdit.IsEnabled = true;
                                lonEdit.IsEnabled = true;
                            }
                            else
                            {
                                coordinatesLabel.Content = "Опция доступна только при активном соединении к интернету.";
                                autoLocation.IsEnabled = false;
                                latEdit.IsEnabled = false;
                                lonEdit.IsEnabled = false;
                                coorButton.Visibility = Visibility.Hidden;
                            }
                            
                            LatLonGrid.Visibility = Visibility.Visible;
                            latEdit.Text = Settings.Default.Latitude.ToString();
                            lonEdit.Text = Settings.Default.Longitude.ToString();
                        }
                        switch (Settings.Default.unitWind)
                        {
                            case "ms":
                                {
                                    radioButtonUnitWMS.IsChecked = true;
                                }break;
                            case "kmh":
                                {
                                    radioButtonUnitWKH.IsChecked = true;
                                }break;
                        }
                        switch (Settings.Default.unitAP)
                        {
                            case "hpa":
                                {
                                    radioButtonUnitAPG.IsChecked = true;
                                } break;
                            case "mm":
                                {
                                    radioButtonUnitAPR.IsChecked = true;
                                } break;
                        }
                    } break;

            }
        }
        public List<string> listComand = new List<string>();
        public bool serialPause = false;
        public bool IRMode = false;
        public string comand;
        public bool treatComand = true;

        public void OnTimeEvent()
        {
            if (changeComandTemp)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCTemp.Text = comandC;
                    checkErrors("temp");
                }
            }
            if (changeComandTime)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCTime.Text = comandC;
                    checkErrors("time");
                }
            }
            if (changeComandSpace)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCSpace.Text = comandC;
                    checkErrors("space");
                }
            }
            if (changeComandVUp)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCVolumeUp.Text = comandC;
                    checkErrors("vup");
                }
            }
            if (changeComandVDown)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCVolumeDown.Text = comandC;
                    checkErrors("vdown");
                }
            }
            if (changeComandF11)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCF11.Text = comandC;
                    checkErrors("f11");
                }
            }
            if (changeComandMenu)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCMenu.Text = comandC;
                    checkErrors("menu");
                }
            }
            if (changeComandWeather)
            {
                if (((MainWindow)this.Tag).strFromPort.Contains("|"))
                {
                    string comandC = ((MainWindow)this.Tag).strFromPort;
                    comandC = comandC.Remove(comandC.LastIndexOf("|"));
                    TextBoxCWeather.Text = comandC;
                    checkErrors("weather");
                }
            }
            if (serialEnabled)
            {
                if (!serialPause)
                {
                    if (logAll.Text.Length > 0)
                    {
                        logAll.Text = logAll.Text.Insert(logAll.Text.Length, ((MainWindow)this.Tag).strFromPort + "\n");
                        if ((bool)Autoscroll.IsChecked)
                        {
                            logAll.ScrollToEnd();
                        }
                    }
                    else
                    {
                        logAll.Text = ((MainWindow)this.Tag).strFromPort + "\n";
                    }
                    string left, right, testtxt;

                    testtxt = ((MainWindow)this.Tag).strFromPort;
                    left = testtxt.Substring(0, testtxt.LastIndexOf("/"));

                    right = testtxt.Substring(testtxt.LastIndexOf("/") + 1).Trim();
                    if (left.Contains("|"))
                    {
                        IRMode = true;
                        comand = left.Remove(left.LastIndexOf("|")).Trim();
                        left = left.Substring(left.LastIndexOf("|") + 1);
                    }
                    if (IRMode)
                    {

                        if (listComand.Count == 0)
                        {
                            listComand.Add(comand);
                            IRLog.Items.Add(comand.ToString());
                            Clipboard.SetText(comand.ToString());
                        }
                        else
                        {
                            if (!listComand.Contains(comand))
                            {
                                listComand.Add(comand);
                                IRLog.Items.Add(comand.ToString());
                                Clipboard.SetText(comand.ToString());
                                if ((bool)Autoscroll.IsChecked)
                                {
                                    // IRLog.ScrollToEnd();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ButtonBasic_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(0);
        }

        private void ButtonFS1_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(1);
        }

        private void ButtonFS2_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(2);
        }

        private void ButtonChangeTemp_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(3);
        }

        private void SliderFS1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentTab == 1)
            {
                LabelTransFS1.Content = "Прозрачность - " + Convert.ToInt32(e.NewValue) + "%";
                Settings.Default.opacityTemp = Convert.ToInt32(e.NewValue);
            }
        }
        RegistryKey reg;
        public void SetAutorun(bool mode)
        {
            try
            {
                reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
                if (mode)
                {
                    reg.SetValue("TempDisplay", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                }
                else
                {
                    reg.DeleteValue("TempDisplay");
                }
                reg.Close();
            }
            catch { }
        }
        private void AutorunCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.Autorun = (bool)autorunCheckbox.IsChecked;
            SetAutorun(Settings.Default.Autorun);
        }

        private void AutorunCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.Autorun = (bool)autorunCheckbox.IsChecked;
            SetAutorun(Settings.Default.Autorun);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.defaultPort = ListPorts.SelectedItem.ToString();
            if (Settings.Default.maxNotificationTemp != Convert.ToInt32(sliderNotifyMaxTemp.Value)
                || Settings.Default.minNotificationTemp != Convert.ToInt32(sliderNotifyMinTemp.Value))
            {
                Settings.Default.maxNotificationTemp = Convert.ToInt32(sliderNotifyMaxTemp.Value);
                Settings.Default.minNotificationTemp = Convert.ToInt32(sliderNotifyMinTemp.Value);
                ((MainWindow)this.Tag).arduinoPort.WriteLine("maxTemp" + sliderNotifyMaxTemp.Value.ToString());
                System.Threading.Thread.Sleep(1500);
                ((MainWindow)this.Tag).arduinoPort.WriteLine("minTemp" + sliderNotifyMinTemp.Value.ToString());
            }
            
            Settings.Default.delayTemp = (int)sliderTemp.Value;
            Settings.Default.delayTime = (int)sliderTime.Value;
            Settings.Default.delayVolume = Convert.ToInt32(sliderVolumeTime.Value * 1000);
            Settings.Default.weatherUpdate = (int)sliderWeatherTimeUpdate.Value;
            if (!Settings.Default.AutoLocation && !coordinatesLabel.Content.Equals("Местоположение не определено"))
            {
                Settings.Default.Latitude = Double.Parse(latEdit.Text.Replace('.',','));
                Settings.Default.Longitude = Double.Parse(lonEdit.Text.Replace('.', ','));
                ((MainWindow)this.Tag).LoadWeatherInfo();
            }
            Settings.Default.Save();
            myToast.Show();
        }

        private void VoiceTempCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceTemp = (bool)voiceTempCheckBox.IsChecked;
        }

        private void VoiceHumCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceHum = (bool)voiceHumCheckBox.IsChecked;
        }

        private void VoiceMaxTempCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceMaxTemp = (bool)voiceMaxTempCheckBox.IsChecked;
        }

        private void VoiceMinTempCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceMinTemp = (bool)voiceMinTempCheckBox.IsChecked;
        }

        private void VoiceMaxHumCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceMaxHum = (bool)voiceMaxHumCheckBox.IsChecked;
        }

        private void VoiceMinHumCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceMinHum = (bool)voiceMinHumCheckBox.IsChecked;
        }

        private void VoiceTimeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceTime = (bool)voiceTimeCheckBox.IsChecked;
        }

        private void VoiceDOWCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceDoW = (bool)voiceDOWCheckBox.IsChecked;
        }

        private void VoiceDateCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.voiceDate = (bool)voiceDateCheckBox.IsChecked;
        }
        private void SliderFS2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentTab == 2)
            {
                LabelTransFS2.Content = "Прозрачность - " + Convert.ToInt32(e.NewValue) + "%";
                Settings.Default.opacityTime = Convert.ToInt32(e.NewValue);
            }
        }

        private void ButtonMonitoring_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(4);
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (serialPause)
            {
                serialPause = false;
                buttonPause.Content = "Пауза";
            }
            else
            {
                serialPause = true;
                buttonPause.Content = "Старт";
            }
        }

        private void CopyIRCode_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(IRLog.Items.ToString());
        }

        

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(5);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.instagram.com/jonikevseenko/");
        }

        private void Image_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/jonik_evseenko");
        }
        
        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\History.txt"))
            {
                System.Diagnostics.Process.Start(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\History.txt");
            }
            else
            {
                MessageBox.Show("History.txt не найден!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SerialWriter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (serialWriter.Text.Length > 0)
                {
                    try
                    {
                        ((MainWindow)Tag).arduinoPort.Write(serialWriter.Text);
                        string returnMessage = ((MainWindow)Tag).arduinoPort.ReadLine();
                        LabelOtvet.Content = ("Отправленная команда - '" + serialWriter.Text + "'. Ответ - '" +
                            returnMessage + "'.").Replace(Environment.NewLine, " ");
                        serialWriter.Clear();
                    }
                    catch { }
                    
                }
            }
        }


        private void UpdatePortsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdatePorts();
            ListPorts.SelectedItem = Settings.Default.defaultPort;
        }
        HelpClass helper = new HelpClass();
        private void SliderTemp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabeTemp.Content = "Время отображения на экране - " + helper.GetSecondString((int)e.NewValue);
        }

        private void SliderTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelVoiceTime.Content = "Время отображения на экране - " + helper.GetSecondString((int)e.NewValue);
        }

        private void SliderV_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentTab == 3)
            {
                LabelTransV.Content = "Прозрачность - " + Convert.ToInt32(e.NewValue) + "%";
                Settings.Default.opacityVolume = Convert.ToInt32(e.NewValue);
            }
        }

        private void SliderVolumeTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue.ToString().Length == 1)
            {
                LabelVolumeTime.Content = "Время отображения на экране - " + helper.GetSecondString((int)e.NewValue);
            }
            else
            {
                LabelVolumeTime.Content = "Время отображения на экране - " + sliderVolumeTime.Value.ToString().Replace(",", ".") + " секунды";
            }
        }

        private void HideRecalledTime_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.hideWhenRecalledTime = hideRecalledTime.IsChecked.Value;
            sliderTime.IsEnabled = !hideRecalledTime.IsChecked.Value;
        }

        private void HideRecalledTime_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.hideWhenRecalledTime = hideRecalledTime.IsChecked.Value;
            sliderTime.IsEnabled = !hideRecalledTime.IsChecked.Value;
        }

        private void HideRecalledTemp_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.hideWhenRecalledTemp = hideRecalledTemp.IsChecked.Value;
            sliderTemp.IsEnabled = !hideRecalledTemp.IsChecked.Value;
        }

        private void HideRecalledTemp_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.hideWhenRecalledTemp = hideRecalledTemp.IsChecked.Value;
            sliderTemp.IsEnabled = !hideRecalledTemp.IsChecked.Value;
        }
        
        private void SliderNotifyMinTemp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
                if (currentTab == 0 && sliderNotifyMinTemp != null)
                {
                    LabelMinTempNotify.Content = "Минимальная температура (" + Convert.ToString(sliderNotifyMinTemp.Value) + "°)";
                    indicatorMin2.Content = (sliderNotifyMinTemp.Value + 1).ToString() + "°";
                    sliderNotifyMaxTemp.Minimum = sliderNotifyMinTemp.Value + 1;
                }
            
        }

        private void SliderNotifyMaxTemp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (currentTab == 0 && sliderNotifyMaxTemp != null)
                {
                    LabelMaxTempNotify.Content = "Максимальная температура (" + sliderNotifyMaxTemp.Value.ToString() + "°)";
                    indicatorMax.Content = (sliderNotifyMaxTemp.Value - 1).ToString() + "°";
                    sliderNotifyMinTemp.Maximum = sliderNotifyMaxTemp.Value - 1;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void notifyMinTempCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.notifyMinTemp = true;
            sliderNotifyMinTemp.Value = Settings.Default.minNotificationTemp;
            sliderNotifyMinTemp.IsEnabled = true;
        }

        private void notifyMinTempCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.notifyMinTemp = false;
            sliderNotifyMinTemp.Value = 0;
            sliderNotifyMinTemp.IsEnabled = false;
        }

        private void notifyMaxTempCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.notifyMaxTemp = false;
            sliderNotifyMaxTemp.Value = 50;
            sliderNotifyMaxTemp.IsEnabled = false;
        }

        private void notifyMaxTempCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.notifyMaxTemp = true;
            sliderNotifyMaxTemp.Value = Settings.Default.maxNotificationTemp;
            sliderNotifyMaxTemp.IsEnabled = true;
        }
        
        private void changeTimeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (changeTimeComboBox.SelectedIndex)
            {
                case 0:
                    {
                        Settings.Default.tempChangeTime = 15;
                    }break;
                case 1:
                    {
                        Settings.Default.tempChangeTime = 30;
                    } break;
                case 2:
                    {
                        Settings.Default.tempChangeTime = 60;
                    } break;
            }
        }

        private void ButtonCommands_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(6);
        }

        private void useAsTemp_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCTemp.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandTemp = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsTime_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCTime.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandTime = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsSpace_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCSpace.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandSpace = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsVUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCVolumeUp.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandVolumeUp = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsVDown_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCVolumeDown.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandVolumeDown = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsF11_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCF11.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandF11 = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text)
                && !IRLog.SelectedValue.ToString().Contains(TextBoxCWeather.Text))
            {
                SelectItem(6);
                TextBoxCMenu.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandMenu = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }

        private void useAsWeather_Click(object sender, RoutedEventArgs e)
        {
            if (!IRLog.SelectedValue.ToString().Contains(TextBoxCSpace.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCTemp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCTime.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeUp.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCVolumeDown.Text) && !IRLog.SelectedValue.ToString().Contains(TextBoxCF11.Text) &&
                        !IRLog.SelectedValue.ToString().Contains(TextBoxCMenu.Text))
            {
                SelectItem(6);
                TextBoxCWeather.Text = IRLog.SelectedValue.ToString();
                Settings.Default.comandWeather = IRLog.SelectedValue.ToString();
            }
            else
            {
                serialToast.Message = "Невозможно выбрать команду из-за совпадения с другими командами.";
                serialToast.Show();
            }
        }
        bool changeComandTemp, changeComandTime, changeComandSpace, changeComandVUp, changeComandVDown, changeComandF11, changeComandMenu, changeComandWeather = false;
        bool serialEnabled = false;
        private void buttonCTemp_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandTemp)
            {
                if (TextBoxCTemp.Text != "Ожидание...")
                {
                    if ((!TextBoxCTemp.Text.Equals(TextBoxCTime.Text)) && (!TextBoxCTemp.Text.Equals(TextBoxCSpace.Text)) &&
                        (!TextBoxCTemp.Text.Equals(TextBoxCVolumeUp.Text)) && (!TextBoxCTemp.Text.Equals(TextBoxCVolumeDown.Text)) &&
                        (!TextBoxCTemp.Text.Equals(TextBoxCF11.Text)) && (!TextBoxCTemp.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCTemp.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandTemp = TextBoxCTemp.Text;
                        cancelComandTemp.Visibility = Visibility.Hidden;
                        changeComandTemp = false;
                        treatComand = true;
                        buttonCTemp.Content = "Изменить";
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandTemp.Visibility = Visibility.Visible;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandTemp = true;
                treatComand = false;
                TextBoxCTemp.Text = "Ожидание...";
                buttonCTemp.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void EnableButton_Checked(object sender, RoutedEventArgs e)
        {
            treatComand = true;
        }

        private void EnableButton_Unchecked(object sender, RoutedEventArgs e)
        {
            treatComand = false;
        }

        private void cancelComandTemp_Click(object sender, RoutedEventArgs e)
        {
            cancelComandTemp.Visibility = Visibility.Hidden;
            changeComandTemp = false;
            treatComand = true;
            buttonCTemp.Content = "Изменить";
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCTemp.Text = Settings.Default.comandTemp;
            checkErrors("temp");
        }

        private void buttonCTime_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandTime)
            {
                if (TextBoxCTime.Text != "Ожидание...")
                {
                    if ((!TextBoxCTime.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCTime.Text.Equals(TextBoxCSpace.Text)) &&
                        (!TextBoxCTime.Text.Equals(TextBoxCVolumeUp.Text)) && (!TextBoxCTime.Text.Equals(TextBoxCVolumeDown.Text)) &&
                        (!TextBoxCTime.Text.Equals(TextBoxCF11.Text)) && (!TextBoxCTime.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCTime.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandTime = TextBoxCTime.Text;
                        cancelComandTime.Visibility = Visibility.Hidden;
                        changeComandTime = false;
                        treatComand = true;
                        buttonCTime.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandTime.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandTime = true;
                treatComand = false;
                TextBoxCTime.Text = "Ожидание...";
                buttonCTime.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandTime_Click(object sender, RoutedEventArgs e)
        {
            cancelComandTime.Visibility = Visibility.Hidden;
            changeComandTime = false;
            treatComand = true;
            buttonCTime.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCTime.Text = Settings.Default.comandTime;
            checkErrors("time");
        }

        private void buttonCSpace_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandSpace)
            {
                if (TextBoxCSpace.Text != "Ожидание...")
                {
                    if ((!TextBoxCSpace.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCSpace.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCSpace.Text.Equals(TextBoxCVolumeUp.Text)) && (!TextBoxCSpace.Text.Equals(TextBoxCVolumeDown.Text)) &&
                        (!TextBoxCSpace.Text.Equals(TextBoxCF11.Text)) && (!TextBoxCSpace.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCSpace.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandSpace = TextBoxCSpace.Text;
                        cancelComandSpace.Visibility = Visibility.Hidden;
                        changeComandSpace = false;
                        treatComand = true;
                        buttonCSpace.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandSpace.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandSpace = true;
                treatComand = false;
                TextBoxCSpace.Text = "Ожидание...";
                buttonCSpace.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandSpace_Click(object sender, RoutedEventArgs e)
        {
            cancelComandSpace.Visibility = Visibility.Hidden;
            changeComandSpace = false;
            treatComand = true;
            buttonCSpace.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCSpace.Text = Settings.Default.comandSpace;
            checkErrors("space");
        }

        private void buttonCVUp_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandVUp)
            {
                if (TextBoxCVolumeUp.Text != "Ожидание...")
                {
                    if ((!TextBoxCVolumeUp.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCVolumeUp.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCVolumeUp.Text.Equals(TextBoxCSpace.Text)) && (!TextBoxCVolumeUp.Text.Equals(TextBoxCVolumeDown.Text)) &&
                        (!TextBoxCVolumeUp.Text.Equals(TextBoxCF11.Text)) && (!TextBoxCVolumeUp.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCVolumeUp.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandVolumeUp = TextBoxCVolumeUp.Text;
                        cancelComandVUp.Visibility = Visibility.Hidden;
                        changeComandVUp = false;
                        treatComand = true;
                        buttonCVUp.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandVUp.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandVUp = true;
                treatComand = false;
                TextBoxCVolumeUp.Text = "Ожидание...";
                buttonCVUp.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandVUp_Click(object sender, RoutedEventArgs e)
        {
            cancelComandVUp.Visibility = Visibility.Hidden;
            changeComandVUp = false;
            treatComand = true;
            buttonCVUp.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCVolumeUp.Text = Settings.Default.comandVolumeUp;
            checkErrors("vup");
        }

        private void buttonCVDown_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandVDown)
            {
                if (TextBoxCVolumeDown.Text != "Ожидание...")
                {
                    if ((!TextBoxCVolumeDown.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCVolumeDown.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCVolumeDown.Text.Equals(TextBoxCSpace.Text)) && (!TextBoxCVolumeDown.Text.Equals(TextBoxCVolumeUp.Text)) &&
                        (!TextBoxCVolumeDown.Text.Equals(TextBoxCF11.Text)) && (!TextBoxCVolumeDown.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCVolumeDown.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandVolumeDown = TextBoxCVolumeDown.Text;
                        cancelComandVDown.Visibility = Visibility.Hidden;
                        changeComandVDown = false;
                        treatComand = true;
                        buttonCVDown.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandVDown.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandVDown = true;
                treatComand = false;
                TextBoxCVolumeDown.Text = "Ожидание...";
                buttonCVDown.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandVDown_Click(object sender, RoutedEventArgs e)
        {
            cancelComandVDown.Visibility = Visibility.Hidden;
            changeComandVDown = false;
            treatComand = true;
            buttonCVUp.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCVolumeDown.Text = Settings.Default.comandVolumeDown;
            checkErrors("vdown");
        }

        private void buttonCF11_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandF11)
            {
                if (TextBoxCF11.Text != "Ожидание...")
                {
                    if ((!TextBoxCF11.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCF11.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCF11.Text.Equals(TextBoxCSpace.Text)) && (!TextBoxCF11.Text.Equals(TextBoxCVolumeUp.Text)) &&
                        (!TextBoxCF11.Text.Equals(TextBoxCVolumeDown.Text)) && (!TextBoxCF11.Text.Equals(TextBoxCWeather.Text))
                        && (!TextBoxCF11.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandF11 = TextBoxCF11.Text;
                        cancelComandF11.Visibility = Visibility.Hidden;
                        changeComandF11 = false;
                        treatComand = true;
                        buttonCF11.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandF11.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandF11 = true;
                treatComand = false;
                TextBoxCF11.Text = "Ожидание...";
                buttonCF11.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandF11_Click(object sender, RoutedEventArgs e)
        {
            cancelComandF11.Visibility = Visibility.Hidden;
            changeComandF11 = false;
            treatComand = true;
            buttonCF11.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCF11.Text = Settings.Default.comandF11;
            checkErrors("f11");
        }
        public void checkErrors(string comand)
        {
            switch (comand)
            {
                case "temp":
                    {
                        alertCTemp.Visibility = Visibility.Hidden;
                        if (TextBoxCTemp.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTemp.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCTemp.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "time":
                    {
                        alertCTime.Visibility = Visibility.Hidden;
                        if (TextBoxCTime.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCTime.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCTime.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "space":
                    {
                        alertCSpace.Visibility = Visibility.Hidden;
                        if (TextBoxCSpace.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCSpace.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCSpace.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "vup":
                    {
                        alertCVUp.Visibility = Visibility.Hidden;
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeUp.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCVUp.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "vdown":
                    {
                        alertCVDown.Visibility = Visibility.Hidden;
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCVolumeDown.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCVDown.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "f11":
                    {
                        alertCF11.Visibility = Visibility.Hidden;
                        if (TextBoxCF11.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCMenu.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCF11.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCF11.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "menu":
                    {
                        alertCMenu.Visibility = Visibility.Hidden;
                        if (TextBoxCMenu.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCMenu.Visibility = Visibility.Visible;
                            alertCWeather.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCWeather.Visibility = Visibility.Hidden;
                        }
                    } break;
                case "weather":
                    {
                        alertCWeather.Visibility = Visibility.Hidden;
                        if (TextBoxCWeather.Text.Equals(TextBoxCTemp.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCTemp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTemp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCWeather.Text.Equals(TextBoxCTime.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCTime.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCTime.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCWeather.Text.Equals(TextBoxCSpace.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCSpace.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCSpace.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCWeather.Text.Equals(TextBoxCVolumeUp.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCVUp.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVUp.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCWeather.Text.Equals(TextBoxCVolumeDown.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCVDown.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCVDown.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCWeather.Text.Equals(TextBoxCF11.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCF11.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCF11.Visibility = Visibility.Hidden;
                        }
                        if (TextBoxCMenu.Text.Equals(TextBoxCWeather.Text))
                        {
                            alertCWeather.Visibility = Visibility.Visible;
                            alertCMenu.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            alertCMenu.Visibility = Visibility.Hidden;
                        }
                    } break;
            }
        }
        private void itemCopy_Click(object sender, RoutedEventArgs e)
        {
            if (IRLog.SelectedIndex != -1)
            {
                Clipboard.SetText(IRLog.SelectedValue.ToString());
                serialToast.Message = "Скопировано в буфер обмена ''" + IRLog.SelectedValue.ToString() + "''";
                serialToast.Show();
            }
        }

        private void itemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IRLog.SelectedIndex != -1)
            {
                serialToast.Message = "Удален из списка ''" + IRLog.SelectedValue.ToString() + "''";
                serialToast.Show();
                IRLog.Items.RemoveAt(IRLog.SelectedIndex);
            }
        }

        private void InfoCommand_MouseUp(object sender, MouseButtonEventArgs e)
        {
            serialToast.Message = "'1' - принудительный опрос датчика.\n '2' - проверка связи с МК. \n'getMinMax' - получение текущего заданного значения минимальной и максимальной температуры.";
            serialToast.Show();
        }

        private void buttonCMenu_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandMenu)
            {
                if (TextBoxCMenu.Text != "Ожидание...")
                {
                    if ((!TextBoxCMenu.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCMenu.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCMenu.Text.Equals(TextBoxCSpace.Text)) && (!TextBoxCMenu.Text.Equals(TextBoxCVolumeUp.Text)) &&
                        (!TextBoxCMenu.Text.Equals(TextBoxCVolumeDown.Text)) && (!TextBoxCMenu.Text.Equals(TextBoxCF11.Text))
                        && (!TextBoxCMenu.Text.Equals(TextBoxCWeather.Text)))
                    {
                        Settings.Default.comandMenu = TextBoxCMenu.Text;
                        cancelComandMenu.Visibility = Visibility.Hidden;
                        changeComandMenu = false;
                        treatComand = true;
                        buttonCMenu.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCWeather.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandMenu.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCWeather.IsEnabled = false;
                changeComandMenu = true;
                treatComand = false;
                TextBoxCMenu.Text = "Ожидание...";
                buttonCMenu.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void cancelComandMenu_Click(object sender, RoutedEventArgs e)
        {
            cancelComandMenu.Visibility = Visibility.Hidden;
            changeComandMenu = false;
            treatComand = true;
            buttonCMenu.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCWeather.IsEnabled = true;
            TextBoxCMenu.Text = Settings.Default.comandMenu;
            checkErrors("menu");
        }

        private void buttonCWeather_Click(object sender, RoutedEventArgs e)
        {
            if (changeComandWeather)
            {
                if (TextBoxCWeather.Text != "Ожидание...")
                {
                    if ((!TextBoxCWeather.Text.Equals(TextBoxCTemp.Text)) && (!TextBoxCWeather.Text.Equals(TextBoxCTime.Text)) &&
                        (!TextBoxCWeather.Text.Equals(TextBoxCSpace.Text)) && (!TextBoxCWeather.Text.Equals(TextBoxCVolumeUp.Text)) &&
                        (!TextBoxCWeather.Text.Equals(TextBoxCVolumeDown.Text)) && (!TextBoxCWeather.Text.Equals(TextBoxCF11.Text))
                        && (!TextBoxCWeather.Text.Equals(TextBoxCMenu.Text)))
                    {
                        Settings.Default.comandWeather = TextBoxCWeather.Text;
                        cancelComandWeather.Visibility = Visibility.Hidden;
                        changeComandWeather = false;
                        treatComand = true;
                        buttonCWeather.Content = "Изменить";
                        buttonCTemp.IsEnabled = true;
                        buttonCTime.IsEnabled = true;
                        buttonCSpace.IsEnabled = true;
                        buttonCVUp.IsEnabled = true;
                        buttonCVDown.IsEnabled = true;
                        buttonCF11.IsEnabled = true;
                        buttonCMenu.IsEnabled = true;
                    }
                    else
                    {
                        comandToast.Message = "Невозможно сохранить команду из-за совпадения с другими командами.";
                        comandToast.Show();
                    }
                }
                else
                {
                    comandToast.Message = "Невозможно сохранить без установленного значения.";
                    comandToast.Show();
                }
            }
            else
            {
                cancelComandWeather.Visibility = Visibility.Visible;
                buttonCTemp.IsEnabled = false;
                buttonCTime.IsEnabled = false;
                buttonCSpace.IsEnabled = false;
                buttonCVUp.IsEnabled = false;
                buttonCVDown.IsEnabled = false;
                buttonCF11.IsEnabled = false;
                buttonCMenu.IsEnabled = false;
                changeComandWeather = true;
                treatComand = false;
                TextBoxCWeather.Text = "Ожидание...";
                buttonCWeather.Content = "Сохранить";
                comandToast.Message = "Нажмите кнопку на пульте.";
                comandToast.Show();
            }
        }

        private void SettingsLayout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
        }

        private void SettingsForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Reload();
        }

        private void SettingsForm_Activated(object sender, EventArgs e)
        {
            dropShadowWindow.Opacity = 0.5;
        }

        private void SettingsForm_Deactivated(object sender, EventArgs e)
        {
            dropShadowWindow.Opacity = 0.0;
        }
        double[] coordinates = new double[2];
        
        const string apiUrl = "http://api.openweathermap.org/data/2.5/weather?lat=[replaselat]&lon=[replaselon]&lang=ru&units=metric&appid=6f20e7bfbca27f5c319265ac3df56662";
        public void getLocation()
        {
            try
            {
                coordinatesLabel.ToolTip = null;
                int connection = helper.GetConnectionAviable();
                if (connection == 1)
                {
                    if (Settings.Default.AutoLocation)
                    {
                        coordinates = helper.GetLocation();
                        string url = "";
                        WebClient JsonParser = new WebClient();
                        JsonParser.DownloadStringCompleted += JsonParser_DownloadStringCompletedAuto;
                        url = apiUrl.Replace("[replaselat]", coordinates[0].ToString());
                        url = url.Replace("[replaselon]", coordinates[1].ToString());
                        JsonParser.DownloadStringAsync(new Uri(url));
                    }
                }
                else
                {
                    coordinatesLabel.Content = "[ ! ] Не удалось определить местоположение";
                    coordinatesLabel.ToolTip = "Не удалось определить местоположение. Проверь подключение к интернету.";
                }
            }catch(Exception ex)
            {
                coordinatesLabel.Content = "[ ! ] Не удалось определить местоположение";
                coordinatesLabel.ToolTip = "Не удалось определить местоположение из-за внутрипрограммной ошибки.";
                logger.Error(ex);
            }
        }

        private void JsonParser_DownloadStringCompletedAuto(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Rootobject WI = JsonConvert.DeserializeObject<Rootobject>(e.Result);
                Dispatcher.Invoke(() =>
                {
                    coordinatesLabel.Content = WI.name + "," + WI.sys.country + " [ Ш:" + coordinates[0].ToString() +
                    " Д: " + coordinates[1].ToString() + " ]";
                });
            }
            catch (Exception ex)
            {
                coordinatesLabel.Content = "[ ! ] Не удалось определить местоположение";
                coordinatesLabel.ToolTip = "Не удалось определить местоположение из-за внутрипрограммной ошибки.";
                logger.Error(ex);
            }
        }

        private void autoLocation_Click(object sender, RoutedEventArgs e)
        {
            coordinatesLabel.ToolTip = null;
            Settings.Default.AutoLocation = (bool)autoLocation.IsChecked;
            if (Settings.Default.AutoLocation)
            {
                getLocation();
                LatLonGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                if (helper.GetConnectionAviable() == 1)
                {
                    coorButton.Visibility = Visibility.Visible;
                }
                else
                {
                    coorButton.Visibility = Visibility.Hidden;
                }
                coordinatesLabel.Content = "Местоположение не определено";
                LatLonGrid.Visibility = Visibility.Visible;
            }
        }

        private void coorButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            coordinates = helper.GetLocation();
            latEdit.Text = coordinates[0].ToString();
            lonEdit.Text = coordinates[1].ToString();
        }
        private bool DoubleCharChecker(string str)
        {
            foreach (char c in str)
            {
                if (c.Equals('-'))
                    return true;

                else if (c.Equals('.'))
                    return true;

                else if (Char.IsNumber(c))
                    return true;
            }
            return false;
        }
        private void coordinatesChanged(object sender, TextChangedEventArgs e)
        {
            if (latEdit.Text.Length > 0 && lonEdit.Text.Length > 0 
                && DoubleCharChecker(latEdit.Text) && DoubleCharChecker(lonEdit.Text) && helper.GetConnectionAviable() == 1)
            {
                string url = "";
                WebClient JsonParser = new WebClient();
                JsonParser.DownloadStringCompleted += JsonParser_DownloadStringCompleted;
                url = apiUrl.Replace("[replaselat]", latEdit.Text);
                url = url.Replace("[replaselon]", lonEdit.Text);
                JsonParser.DownloadStringAsync(new Uri(url));
            }
        }
        private void JsonParser_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Rootobject WI = JsonConvert.DeserializeObject<Rootobject>(e.Result);
                Dispatcher.Invoke(() =>
                {
                    if(WI.sys.country != null && WI.name != null)
                    {
                        coordinatesLabel.Content = WI.name + "," + WI.sys.country;
                    }
                    else
                    {
                        coordinatesLabel.Content = "Местоположение не определено";
                    }
                    
                });
            }
            catch (Exception ex)
            {
                coordinatesLabel.Content = "[ ! ] Не удалось определить местоположение";
                coordinatesLabel.ToolTip = "Не удалось определить местоположение из-за внутрипрограммной ошибки.";
                logger.Error(ex);
            }
        }

        public void AnimateWindow(int mode)
        {
            if (mode == 0)
            {
                BeginAnimation(TopProperty, positionClose);
                BeginAnimation(OpacityProperty, opacityAnimationClose);
            }
            if (mode == 1)
            {
                BeginAnimation(TopProperty, positionOpen);
                BeginAnimation(OpacityProperty, opacityAnimationOpen);
            }
        }
        private void cancelComandWeather_Click(object sender, RoutedEventArgs e)
        {
            cancelComandWeather.Visibility = Visibility.Hidden;
            changeComandWeather = false;
            treatComand = true;
            buttonCWeather.Content = "Изменить";
            buttonCTemp.IsEnabled = true;
            buttonCTime.IsEnabled = true;
            buttonCSpace.IsEnabled = true;
            buttonCVUp.IsEnabled = true;
            buttonCVDown.IsEnabled = true;
            buttonCF11.IsEnabled = true;
            buttonCMenu.IsEnabled = true;
            TextBoxCWeather.Text = Settings.Default.comandWeather;
            checkErrors("weather");
        }

        private void ButtonWeather_Click(object sender, RoutedEventArgs e)
        {
            SelectItem(7);
        }

        private void sliderWeatherTimeUpdate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelTimeWeatherUpdate.Content = "Обновлять погоду каждые " + helper.ConvertOptionUpdateWeather(Convert.ToInt32(e.NewValue));
        }


        private void radioButtonUnitWMS_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.unitWind = "ms";
        }

        private void radioButtonUnitWKH_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.unitWind = "kmh";
        }

        private void radioButtonUnitAPG_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.unitAP = "hpa";
        }

        private void radioButtonUnitAPR_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.unitAP = "mm";
        }

        
    }
}
