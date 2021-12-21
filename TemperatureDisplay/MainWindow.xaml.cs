using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Timers;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using CoreAudioApi;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using System.Net.NetworkInformation;
using NLog;
using System.ComponentModel;
using JWeather.Properties;
using System.Data.SQLite;
using System.Data;
using System.Data.Common;
using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;

namespace JWeather
{

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int cmdShow);
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr handle);
        readonly Mutex mutex = new Mutex(false, "71933024-B7FA-416D-AA56-543E31AABB7D");

        public MainWindow()
        {

            if (!mutex.WaitOne(500, false))
            {
                //MessageBox.Show("Приложение уже запущено!", "Ошибочка!");
                string processName = Process.GetCurrentProcess().ProcessName;
                Process process = Process.GetProcesses().Where(p => p.ProcessName == processName).FirstOrDefault();
                if (process != null)
                {
                    IntPtr handle = process.MainWindowHandle;
                    ShowWindow(handle, 1);
                    SetForegroundWindow(handle);
                }
                this.Close();
                return;
            }
            InitializeComponent();

            if (Environment.GetCommandLineArgs().Last() == "-Embedding")
                this.Title += " [COM]";
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
            this.SetBackgroundColor();
            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            CalculateNotificationsList();
            Left = primaryMonitorArea.Right - Width;
            Top = primaryMonitorArea.Bottom - Height - 20;
            LogManager.Configuration = helper.GetLoggingConfiguration();
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("Старт программы");
        }
        protected override void OnClosed(EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(AddressChangedCallback);
            base.OnClosed(e);
        }
        Color currentColor;
        private void SetBackgroundColor()
        {
            currentBrush = SystemParameters.WindowGlassBrush;
            currentColor = ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color;
            currentColor.ScA = 0.5F;

            SetColorMain();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                SetBackgroundColor();
            }
        }

        public void SetColorMain()
        {
            ContentLayout.Background = currentBrush;
        }
        Logger logger;
        public Brush currentBrush;
        System.Timers.Timer timerDelay;
        bool startResolution = true;
        private delegate void updateDelegate(string txt);
        private delegate void putInfoDelegate(string text, string hint);
        private delegate void putAnimationDelegate(DoubleAnimation doubleAnimation);
        public string dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\LogTemperature\";
        public string dirOfImage = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\WeatherImage\";
        public string fileDevices = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\devises.json";
        public MemoryManagement MM = new MemoryManagement();
        public int temperature;
        public int humidity;
        string temp, hum;
        private MMDevice device;
        MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
        List<string> listTargetDay = new List<string>();
        public SerialPort arduinoPort = new SerialPort(Settings.Default.defaultPort, 9600);
        TaskbarIcon tbi;
        public int modeFSM = 0;
        public string strFromPort;
        public bool IRMode = false;
        public string command;
        public bool volumeShowed = false;
        private SettingForm SettingF;
        private FullScreeen FormFullScreen;
        private FullScreenWeather FullScreenW;
        private TempChange ChangeTemp;
        private Volume FormVolume;
        public class MemoryManagement
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

            public void FlushMemory()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
        }

        public bool ArduinoDetected()
        {
            try
            {
                arduinoPort.Open();
                //Thread.Sleep(1000);
                arduinoPort.Write("1");
                string returnMessage = arduinoPort.ReadLine();
                bool detected = false;
                foreach (string sym in tempdisplayLine)
                {
                    if (returnMessage.Contains(sym))
                    {
                        detected = true;
                        break;
                    }
                }
                arduinoPort.Close();
                return detected;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }
        bool ArduinoPortFound = true;
        string[] tempdisplayLine = { "T", "e", "m", "p", "r", "a", "t", "u", "D", "i", "s", "l", "y" };
        public const string UriLock = @"pack://application:,,,/Images/lock-outline.png";
        public const string UriUnlock = @"pack://application:,,,/Images/lock-open-outline.png";
        public const string UriFan = @"pack://application:,,,/Images/fan.png";
        public const string UriFanOff = @"pack://application:,,,/Images/fan-off.png";
        private static IDuplexTypedMessageReceiver<JResponse, JRequest> JReceiver;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            versionLabel.Content = helper.GetCurrentVersion();
            doubleAnimation.Completed += DoubleAnimation_Completed;
            tbi = new TaskbarIcon();
            tbi.TrayLeftMouseDown += OnClickTrayIcon;
            tbi.ToolTip = "Поиск устройства...";
            tbi.Icon = Properties.Resources.tray;
            if (Settings.Default.LockingWindow)
            {
                LockWindow.Source = BitmapFrame.Create(new Uri(UriLock));
                LockWindow.ToolTip = "Открепить окно";
            }
            else
            {
                LockWindow.Source = BitmapFrame.Create(new Uri(UriUnlock));
                LockWindow.ToolTip = "Закрепить окно";
            }
            ConnectPort(true);
            CheckDir();
            Activate();
            LoadDevices();
            try
            {
                NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
                IDuplexTypedMessagesFactory aReceiverFactory = new DuplexTypedMessagesFactory();
                JReceiver = aReceiverFactory.CreateDuplexTypedMessageReceiver<JResponse, JRequest>();
                JReceiver.MessageReceived += OnMessageReceived;
                IMessagingSystemFactory aMessaging = new TcpMessagingSystemFactory();
                IDuplexInputChannel anInputChannel
                    = aMessaging.CreateDuplexInputChannel(String.Format("tcp://{0}:{1}", "192.168.1.2", "8060"));
                JReceiver.AttachDuplexInputChannel(anInputChannel);
            }catch(Exception ex)
            {
                logger.Error(ex);
            }
        }
        AuthItems AuthDevices = new AuthItems();
        public void LoadDevices()
        {
            try
            {
                if (File.Exists(fileDevices))
                {
                    using (StreamReader sr = new StreamReader(fileDevices, Encoding.Default))
                    {
                        string jsonText = sr.ReadToEnd();
                        AuthDevices = new AuthItems
                        {
                            authItems = new List<AuthItem>()
                        };
                        AuthDevices = JsonConvert.DeserializeObject<AuthItems>(jsonText);
                        sr.Close();
                    }
                }
                else
                {
                    AuthDevices.authItems = new List<AuthItem>();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public AuthItem CheckDevice(long deviceId)
        {
            try
            {
                if (AuthDevices.authItems.Count > 0)
                {
                    foreach (AuthItem item in AuthDevices.authItems)
                    {
                        if (item.DeviceId == deviceId)
                        {
                            return item;
                        }
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }
        Auth authWindow;
        public void OnMessageReceived(object sender, TypedRequestReceivedEventArgs<JRequest> e)
        {
            try
            {
                RequestObject requestObject = JsonConvert.DeserializeObject<RequestObject>(e.RequestMessage.Text);
                switch (requestObject.option)
                {
                    case RequestType.getVolume:
                        {
                            Dispatcher.Invoke(() =>
                            {
                                long volume = Convert.ToInt64(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                                ResponseObject responseObject = new ResponseObject
                                {
                                    response = RequestType.getVolume,
                                    value = volume
                                };
                                
                                JResponse aResponse = new JResponse
                                {
                                    Text = JsonConvert.SerializeObject(responseObject)
                                };
                                JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                            });
                        }
                        break;
                    case RequestType.setVolume:
                        {
                            Dispatcher.Invoke(() =>
                            {
                                device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)requestObject.value / 100;
                                if (FormVolume == null && menuWindow == null)
                                {
                                    FormVolume = new Volume
                                    {
                                        Tag = this
                                    };
                                    FormVolume.Closed += FormVolume_Closed;
                                    FormVolume.Show();
                                }
                            });
                        }
                        break;
                    case RequestType.setFanState:
                        {
                            bool state = Convert.ToBoolean(requestObject.value);
                            Dispatcher.Invoke(() =>
                            {
                                SetFanState(state);
                                ShowNotification(Convert.ToInt32(requestObject.value));
                                ResponseObject responseObject = new ResponseObject
                                {
                                    response = RequestType.setFanState,
                                    value = Convert.ToInt64(state)
                                };
                                JResponse aResponse = new JResponse
                                {
                                    Text = JsonConvert.SerializeObject(responseObject)
                                };
                                JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                            });
                        }
                        break;
                    case RequestType.getFanState:
                        {
                            Dispatcher.Invoke(() =>
                            {
                                ResponseObject responseObject = new ResponseObject
                                {
                                    response = RequestType.getFanState,
                                    value = Convert.ToInt64(FanEnabled)
                                };
                                JResponse aResponse = new JResponse
                                {
                                    Text = JsonConvert.SerializeObject(responseObject)
                                };
                                JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                            });
                        }
                        break;
                    case RequestType.Authorization:
                        {
                            if (CheckDevice(requestObject.value) != null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    AuthItem ai = CheckDevice(requestObject.value);
                                    string accessString;
                                    if(ai.AccesLevel == 1)
                                    {
                                        accessString = "гостя";
                                    }
                                    else
                                    {
                                        accessString = "администратора";
                                    }
                                    ShowNotification(NotificationType.Error,
                                        "Подключено устройство",
                                        String.Format("{0} был подключён с правами {1}.",ai.Name, accessString));
                                    ResponseObject responseObject = new ResponseObject
                                    {
                                        response = RequestType.Authorization,
                                        value = Convert.ToInt64(ai.AccesLevel)
                                    };
                                    JResponse aResponse = new JResponse
                                    {
                                        Text = JsonConvert.SerializeObject(responseObject)
                                    };
                                    JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                                });
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    authWindow = new Auth(requestObject.value)
                                    {
                                        Tag = this
                                    };
                                    if (authWindow.ShowDialog() == true)
                                    {
                                        if(authWindow.rememberDevice.IsChecked == true)
                                        {
                                            AuthDevices.authItems.Add(authWindow.authItem);
                                            using (StreamWriter sw = new StreamWriter(fileDevices, false, Encoding.Default))
                                            {
                                                sw.WriteLine(JsonConvert.SerializeObject(AuthDevices));
                                                sw.Close();
                                            }
                                        }
                                        ResponseObject responseObject = new ResponseObject
                                        {
                                            response = RequestType.Authorization,
                                            value = Convert.ToInt64(authWindow.authItem.AccesLevel)
                                        };
                                        JResponse aResponse = new JResponse
                                        {
                                            Text = JsonConvert.SerializeObject(responseObject)
                                        };
                                        JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                                    }
                                    else
                                    {
                                        ResponseObject responseObject = new ResponseObject
                                        {
                                            response = RequestType.Authorization,
                                            value = Convert.ToInt64(0)
                                        };
                                        JResponse aResponse = new JResponse
                                        {
                                            Text = JsonConvert.SerializeObject(responseObject)
                                        };
                                        JReceiver.SendResponseMessage(e.ResponseReceiverId, aResponse);
                                    }
                                });
                            }
                        }
                        break;
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        int timeStart = 0;
        public void AddressChangedCallback(object sender, EventArgs e)
        {
            LoadWeatherInfo();
        }
        public void ConnectPort(bool notification)
        {
            if (Settings.Default.defaultPort.Contains("Авто"))
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    arduinoPort = new SerialPort(port, 9600)
                    {
                        ReadTimeout = 2000
                    };
                    if (ArduinoDetected())
                    {
                        ArduinoPortFound = true;
                        logger.Info("Arduino обнаружен в порте " + arduinoPort.PortName);
                        if (ShowDialigNotification(NotificationType.Dialog, "Порт обнаружен", "Arduino обнаружен в порте " + arduinoPort.PortName + ". Выбрать этот порт по умолчанию?", DialogType.ConfirmCancel))
                        {
                            Settings.Default.defaultPort = arduinoPort.PortName;
                            Settings.Default.Save();
                        }
                        break;
                    }
                    else
                    {
                        ArduinoPortFound = false;
                    }
                }
            }
            else
            {
                ArduinoPortFound = true;
                arduinoPort = new SerialPort(Settings.Default.defaultPort, 9600);
            }
            if (ArduinoPortFound)
            {

                //Thread.Sleep(1000);
                arduinoPort.ReadTimeout = 1000;
                arduinoPort.DtrEnable = true;
                try
                {
                    arduinoPort.Open();
                    //Thread.Sleep(1000);
                    arduinoPort.WriteLine("0");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    if (notification)
                    {
                        string body = "Ошибка открытия порта Serial.Завершите работу с портом " + arduinoPort.PortName + " и повторите попытку.";
                        ShowNotification(2, "Внимание!", body);
                        /*ShowToast(
                            "Внимание!",
                            body,
                            "notification_basic.png",
                            Properties.Resources.notification_basic);*/
                    }
                    if (waitArduinoBar.Visibility == Visibility.Hidden)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            waitArduinoBar.Visibility = Visibility.Visible;
                            waitArduinoBar.IsIndeterminate = true;
                            waitConnectionImage.Visibility = Visibility.Visible;
                            TemperatureBlock.Visibility = Visibility.Hidden;
                            HumidityBlock.Visibility = Visibility.Hidden;
                        });
                    }
                }
                arduinoPort.DataReceived += OnBufferData;
            }
            else
            {
                logger.Error("Порт не найден или занят! Проверь подключение к компьютеру!");
                if (notification)
                {
                    string body = "Порт не найден или занят! Проверь подключение к компьютеру!";
                    ShowNotification(2, "Внимание!", body);
                    /*ShowToast(
                            "Внимание!",
                            body,
                            "notification_basic.png",
                            Properties.Resources.notification_basic);*/
                }
                if (waitArduinoBar.Visibility == Visibility.Hidden)
                {
                    Dispatcher.Invoke(() =>
                    {
                        waitArduinoBar.Visibility = Visibility.Visible;
                        waitArduinoBar.IsIndeterminate = true;
                        waitConnectionImage.Visibility = Visibility.Visible;
                        TemperatureBlock.Visibility = Visibility.Hidden;
                        HumidityBlock.Visibility = Visibility.Hidden;
                    });
                }
            }

        }
        public SQLiteConnection SQLconnection = new SQLiteConnection();
        public void AppendNote(string baselocation)
        {
            DateTime localDate = DateTime.Now;
            string h = localDate.Hour.ToString();
            if (localDate.Hour < 10)
            {
                h = h.Insert(0, "0");
            }
            string m = localDate.Minute.ToString();
            if (localDate.Minute < 10)
            {
                m = m.Insert(0, "0");
            }
            SQLconnection = new SQLiteConnection("Data Source = " + baselocation + "; Version=3;");
            SQLconnection.Open();
            using (SQLiteCommand command = new SQLiteCommand(SQLconnection))
            {
                command.CommandText = "INSERT INTO '" + localDate.Day.ToString() + @"' 
('Time', 'Temperature', 'Humidity', 'WeatherTemp', 'WeatherHum', 'WeatherPressure', 'WeatherWindDeg', 
'WeatherWindSpeed', 'WeatherSunset', 'WeatherSunrise', 'WeatherVisibility', 'WeatherDescription', 'WeatherImage')
VALUES 
('" + h + ":" + m + @"', '" + temperature + "', '" + humidity + "'," + " '" + WeatherInfo.main.temp
+ "'," + " '" + WeatherInfo.main.humidity + "'," + "'" + Convert.ToInt32(WeatherInfo.main.pressure)
+ "'," + " '" + Convert.ToInt32(WeatherInfo.wind.deg) + "'," + " '" + Convert.ToInt32(WeatherInfo.wind.speed)
+ "'," + " '" + WeatherInfo.sys.sunset + "'," + " '" + WeatherInfo.sys.sunrise
+ "'," + " '" + Convert.ToInt32(WeatherInfo.visibility) + "'," + " '" + WeatherInfo.weather[0].description
+ "'," + " '" + WeatherInfo.weather[0].icon + " '" + ");";
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                SQLconnection.Close();
            }
        }
        public void CheckDir()
        {
            LoadWeatherInfo();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (!Directory.Exists(dirOfImage))
            {
                Directory.CreateDirectory(dirOfImage);
            }
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timeStart = DateTime.Now.Minute;
            timer1.Interval = 60000;
            timer1.Elapsed += new ElapsedEventHandler((sender1, e1) =>
            {
                try
                {
                    DateTime localDate = DateTime.Now;
                    switch (Settings.Default.weatherUpdate)
                    {
                        case 1:
                            {
                                if ((localDate.Minute == 10) || (localDate.Minute == 20) || (localDate.Minute == 30) || (localDate.Minute == 40)
                                    || (localDate.Minute == 50) || (localDate.Minute == 0))
                                {
                                    LoadWeatherInfo();
                                    logger.Info("Обновление погоды");
                                }
                            }
                            break;
                        case 2:
                            {
                                if ((localDate.Minute == 15) || (localDate.Minute == 30) || (localDate.Minute == 45) || (localDate.Minute == 0))
                                {
                                    LoadWeatherInfo();
                                    logger.Info("Обновление погоды");
                                }
                            }
                            break;
                        case 3:
                            {
                                if ((localDate.Minute == 20) || (localDate.Minute == 40) || (localDate.Minute == 0))
                                {
                                    LoadWeatherInfo();
                                    logger.Info("Обновление погоды");
                                }
                            }
                            break;
                        case 4:
                            {
                                if ((localDate.Minute == 30) || (localDate.Minute == 0))
                                {
                                    LoadWeatherInfo();
                                    logger.Info("Обновление погоды");
                                }
                            }
                            break;
                        case 5:
                            {
                                if (localDate.Minute == 0)
                                {
                                    LoadWeatherInfo();
                                    logger.Info("Обновление погоды");
                                }
                            }
                            break;
                    }
                    if (arduinoPort.IsOpen)
                    {
                        string year = localDate.Year.ToString();
                        string mon = localDate.Month.ToString();
                        string d = localDate.Day.ToString();
                        if (localDate.Day < 10)
                        {
                            d = d.Insert(0, "0");
                        }
                        if (localDate.Month < 10)
                        {
                            mon = mon.Insert(0, "0");
                        }
                        if (!Directory.Exists(dir + year + @"\"))
                        {
                            Directory.CreateDirectory(dir + year + @"\");
                        }
                        string baseName = dir + year + @"\" + localDate.Month.ToString() + ".db";
                        if (!File.Exists(baseName))
                        {
                            SQLiteConnection.CreateFile(baseName);
                        }
                        SQLconnection = new SQLiteConnection("Data Source = " + baseName + "; Version=3;");
                        SQLconnection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(SQLconnection))
                        {
                            command.CommandText = @"CREATE TABLE IF NOT EXISTS [" + localDate.Day.ToString() + @"] (
                    [id] integer NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                    [Time] text NOT NULL,
                    [Temperature] integer NOT NULL,
                    [Humidity] integer NOT NULL,
                    [WeatherTemp] integer,
                    [WeatherHum] integer,
                    [WeatherPressure] integer,
                    [WeatherWindDeg] integer,
                    [WeatherWindSpeed] integer,
                    [WeatherSunset] integer,
                    [WeatherSunrise] integer,
                    [WeatherVisibility] integer,
                    [WeatherDescription] text,
                    [WeatherImage] text
                    );";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                        SQLconnection.Close();
                        if (localDate.Minute == timeStart)
                        {
                            baloonShowMax = true;
                            baloonShowMin = true;
                        }
                        if (humidity == 0)
                        {
                            arduinoPort.WriteLine("0");
                        }
                        switch (Settings.Default.tempChangeTime)
                        {
                            case 15:
                                {
                                    MM.FlushMemory();
                                    if ((localDate.Minute == 15) || (localDate.Minute == 30) || (localDate.Minute == 45) || (localDate.Minute == 0))
                                    {
                                        AppendNote(baseName);
                                        logger.Info("Запись температуры и влажности");
                                    }
                                }
                                break;
                            case 30:
                                {
                                    MM.FlushMemory();
                                    if ((localDate.Minute == 30) || (localDate.Minute == 0))
                                    {
                                        AppendNote(baseName);
                                        logger.Info("Запись температуры и влажности");
                                    }
                                }
                                break;
                            case 60:
                                {
                                    MM.FlushMemory();
                                    if (localDate.Minute == 0)
                                    {
                                        AppendNote(baseName);
                                        logger.Info("Запись температуры и влажности");
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        ConnectPort(false);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
            timer1.Start();
        }
        private void OnBufferData(object sender, SerialDataReceivedEventArgs e)
        {
            if (!arduinoPort.IsOpen) return;
            try
            {
                string testtxt = arduinoPort.ReadLine();
                modeFSM = 0;
                if (waitArduinoBar.Visibility == Visibility.Visible)
                {
                    Dispatcher.Invoke(() =>
                    {
                        waitArduinoBar.Visibility = Visibility.Hidden;
                        waitArduinoBar.IsIndeterminate = false;
                        waitConnectionImage.Visibility = Visibility.Hidden;
                        TemperatureBlock.Visibility = Visibility.Visible;
                        HumidityBlock.Visibility = Visibility.Visible;
                    });
                }
                //logger.Info("Входящая строка от МК - " + testtxt);
                if (testtxt != null)
                {
                    if (testtxt.Length > 2)
                    {
                        if (testtxt.Contains("Temperature"))
                        {
                            testtxt = "0/0";
                        }
                        if (testtxt.Contains("FanState - "))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                bool state = Convert.ToBoolean(int.Parse(testtxt.Substring(11).Trim()));
                                FanEnabled = state;
                                SetFanState(FanEnabled);
                                if (FanEnabled)
                                {
                                    FanButton.Source = BitmapFrame.Create(new Uri(UriFan));
                                    FanButton.ToolTip = "Выключить вентилятор";
                                    ShowNotification(1);
                                }
                                else
                                {
                                    FanButton.Source = BitmapFrame.Create(new Uri(UriFanOff));
                                    FanButton.ToolTip = "Включить вентилятор";
                                    ShowNotification(0);
                                }
                            });
                            return;
                        }
                        string left = testtxt.Substring(0, testtxt.LastIndexOf("/"));
                        string right = testtxt.Substring(testtxt.LastIndexOf("/") + 1).Trim();
                        if (left.Contains("|"))
                        {
                            IRMode = true;
                            command = left.Remove(left.LastIndexOf("|")).Trim();
                            left = left.Substring(left.LastIndexOf("|") + 1);
                        }
                        if (left.Length < 3 && right.Length < 3)
                        {
                            if (left.Length != 0 && right.Length != 0)
                            {
                                temp = left.Trim();
                                hum = right.Trim();
                                strFromPort = testtxt;
                                if (strFromPort.Contains("Failed to read from DHT sensor!"))
                                {
                                    logger.Error("Ошибка чтения данных с датчика темперуры!");
                                    ShowNotification(2, "Внимание!", "Ошибка чтения данных с датчика темперуры!");
                                    /*ShowToast(
                            "Внимание!",
                            "Ошибка чтения данных с датчика темперуры!",
                            "notification_basic.png",
                            Properties.Resources.notification_basic);*/
                                }
                                else
                                {
                                    if (IRMode)
                                    {
                                        if (command.Equals(Settings.Default.comandTemp))
                                        {
                                            modeFSM = 1;
                                        }
                                        else
                                            if (command.Equals(Settings.Default.comandTime))
                                        {
                                            modeFSM = 2;
                                        }
                                        else
                                                if (command.Equals(Settings.Default.comandSpace))
                                        {
                                            modeFSM = 3;
                                        }
                                        else
                                                    if (command.Contains(Settings.Default.comandVolumeUp))
                                        {
                                            modeFSM = 4;
                                        }
                                        else
                                                        if (command.Contains(Settings.Default.comandVolumeDown))
                                        {
                                            modeFSM = 5;
                                        }
                                        else
                                                            if (command.Equals(Settings.Default.comandF11))
                                        {
                                            modeFSM = 6;
                                        }
                                        else
                                                                if (command.Equals(Settings.Default.comandMenu))
                                        {
                                            modeFSM = 7;
                                        }
                                        else
                                                                    if (command.Equals(Settings.Default.comandMenuLeft))
                                        {
                                            modeFSM = 8;
                                        }
                                        else
                                                                        if (command.Equals(Settings.Default.comandMenuRight))
                                        {
                                            modeFSM = 9;
                                        }
                                        else
                                                                            if (command.Equals(Settings.Default.comandMenuEnter))
                                        {
                                            modeFSM = 10;
                                        }
                                        else
                                                                                if (command.Equals(Settings.Default.comandWeather))
                                        {
                                            modeFSM = 11;
                                        }
                                        else
                                            if (command.Equals(Settings.Default.comandFan))
                                        {
                                            modeFSM = 12;
                                        }
                                        IRMode = false;
                                    }
                                    else { modeFSM = 0; }

                                    // updateTextBox(strFromPort);
                                    TemperatureBlock.Dispatcher.BeginInvoke(new updateDelegate(UpdateTextBox), strFromPort);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void CreateDelay(int modeDelay)
        {
            startResolution = false;
            switch (modeDelay)
            {
                case 0:
                    {
                        timerDelay = new System.Timers.Timer((Settings.Default.delayTime * 1000) + 500);
                    }
                    break;
                case 1:
                    {
                        timerDelay = new System.Timers.Timer((Settings.Default.delayTemp * 1000) + 500);
                    }
                    break;
                case 2:
                    {
                        timerDelay = new System.Timers.Timer(300); ;
                    }
                    break;
                case 3:
                    {
                        timerDelay = new System.Timers.Timer(500); ;
                    }
                    break;
                case 4:
                    {
                        timerDelay = new System.Timers.Timer((Settings.Default.delayWeather * 1000) + 500);
                    }
                    break;
                default:
                    {
                        timerDelay = new System.Timers.Timer(); ;
                    }
                    break;
            }
            timerDelay.AutoReset = false;
            timerDelay.Elapsed += new ElapsedEventHandler((sender2, e2) =>
            {
                startResolution = true;
            });
            timerDelay.Start();
        }
        Menu menuWindow;
        bool commandEnabled = true;
        public bool windowFFS = false;
        public bool baloonShowMax = true;
        public bool baloonShowMin = true;
        private void UpdateTextBox(string txt)
        {
            try
            {
                temperature = int.Parse(temp.Trim());
                humidity = int.Parse(hum.Trim());
                if (temperature > 0)
                {
                    TemperatureBlock.Content = "+" + temp + "°";
                    Title = "+" + temp + "° / ";
                }
                else
                {
                    TemperatureBlock.Content = temp + "°";
                    Title = temp + "°";
                }
                HumidityBlock.Content = hum + "%";
                Title = Title + hum + "%";
                if (Settings.Default.notifyMinTemp)
                {
                    if (temperature < Settings.Default.minNotificationTemp)
                    {
                        if (baloonShowMin)
                        {
                            ShowNotification(3, "JWeather", "Температура упала ниже " + Settings.Default.minNotificationTemp.ToString() + "°");
                            /*ShowToast(
                            "JWeather",
                            "Температура упала ниже " + Settings.Default.minNotificationTemp.ToString() + "°",
                            "notification_low.png",
                            Properties.Resources.notification_low);*/
                            baloonShowMin = false;
                        }
                        tbi.Icon = JWeather.Properties.Resources.icon_low;
                        tbi.ToolTipText = "Температура " + temp + "° \n" + "Влажность " + hum + "%"
                            + " \n \n Температура упала ниже " + Settings.Default.minNotificationTemp.ToString() + "°";

                    }
                    else
                    {
                        tbi.Icon = Properties.Resources.tray;
                        tbi.ToolTipText = "Температура " + temp + "° \n" + "Влажность " + hum + "%";
                    }
                }
                if (Settings.Default.notifyMaxTemp)
                {
                    if (temperature > Settings.Default.maxNotificationTemp)
                    {
                        if (baloonShowMax)
                        {
                            ShowNotification(4, "JWeather", "Температура поднялась выше " + Settings.Default.maxNotificationTemp.ToString() + "°");
                            /*ShowToast(
                            "JWeather",
                            "Температура поднялась выше " + Settings.Default.maxNotificationTemp.ToString() + "°",
                            "notification_high.png",
                            Properties.Resources.notification_high);*/
                            baloonShowMax = false;
                        }
                        tbi.Icon = JWeather.Properties.Resources.icon_high;
                        tbi.ToolTipText = "Температура " + temp + "° \n" + "Влажность " + hum + "%"
                            + " \n \n Температура поднялась выше " + Settings.Default.maxNotificationTemp.ToString() + "°";
                    }
                    else
                    {
                        tbi.Icon = Properties.Resources.tray;
                        tbi.ToolTipText = "Температура " + temp + "° \n" + "Влажность " + hum + "%";
                    }
                }
                if (FormFullScreen != null)
                {
                    if (!FormFullScreen.isTemp)
                    {
                        FormFullScreen.BottomText.Text = "Температура " + temp + "° \n" + "Влажность " + hum + "%";
                    }
                    else
                    {
                        FormFullScreen.CenterText.Text = "Температура " + temp + "° \n" + "Влажность " + hum + "%";
                    }
                }
                if (SettingF != null)
                {
                    SettingF.Title = "Настройки JWeather - " + temp + "° " + hum + "% ";
                }
                if (SettingF != null)
                {
                    if (SettingF.currentTab != 6)
                    {
                        SettingF.OnTimeEvent();
                        commandEnabled = SettingF.treatComand;
                    }
                    else
                    {
                        SettingF.OnTimeEvent();
                        commandEnabled = false;
                    }
                }
                else
                {
                    commandEnabled = true;
                }
                if (commandEnabled)
                {
                    if (modeFSM == 2)
                    {
                        if (startResolution)
                        {
                            if (Settings.Default.hideWhenRecalledTime)
                            {
                                if (FormFullScreen != null)
                                {
                                    windowFFS = true;
                                }
                                else
                                {
                                    CreateDelay(3);
                                    FormFullScreen = new FullScreeen();
                                    FormFullScreen.Closed += FormFullScreen_Closed;
                                    FormFullScreen.Tag = this;
                                    FormFullScreen.Show();
                                }
                            }
                            else
                            {
                                if (FormFullScreen == null)
                                {
                                    CreateDelay(0);
                                    FormFullScreen = new FullScreeen();
                                    FormFullScreen.Closed += FormFullScreen_Closed;
                                    FormFullScreen.Tag = this;
                                    FormFullScreen.Show();
                                }
                            }
                        }
                    }
                    if (modeFSM == 1)
                    {
                        if (startResolution)
                        {
                            if (Settings.Default.hideWhenRecalledTemp)
                            {
                                if (FormFullScreen != null)
                                {
                                    windowFFS = true;
                                }
                                else
                                {
                                    CreateDelay(3);
                                    FormFullScreen = new FullScreeen();
                                    FormFullScreen.Closed += FormFullScreen_Closed;
                                    FormFullScreen.Tag = this;
                                    FormFullScreen.Show();
                                }
                            }
                            else
                            {
                                if (FormFullScreen == null)
                                {
                                    CreateDelay(0);
                                    FormFullScreen = new FullScreeen();
                                    FormFullScreen.Closed += FormFullScreen_Closed;
                                    FormFullScreen.Tag = this;
                                    FormFullScreen.Show();
                                }
                            }
                        }
                    }
                    if (modeFSM == 3)
                    {
                        if (startResolution)
                        {
                            CreateDelay(2);
                            System.Windows.Forms.SendKeys.SendWait(" ");
                        }
                    }
                    if (modeFSM == 6)
                    {
                        if (startResolution)
                        {
                            CreateDelay(2);
                            System.Windows.Forms.SendKeys.SendWait("{F11}");
                        }
                    }
                    if (modeFSM == 7)
                    {
                        if (startResolution)
                        {
                            if (menuWindow == null)
                            {
                                menuWindow = new Menu
                                {
                                    Tag = this
                                };
                                menuWindow.Closed += FormMenu_Closed;
                                menuWindow.Show();
                            }
                            else
                            {
                                menuWindow.closeWindow();
                            }
                        }
                    }
                    if (modeFSM == 8)
                    {
                        if (startResolution)
                        {
                            if (menuWindow != null)
                            {
                                CreateDelay(2);
                                menuWindow.previousItem();
                            }
                            else
                            {
                                System.Windows.Forms.SendKeys.SendWait("{LEFT}");
                            }
                        }
                    }
                    if (modeFSM == 9)
                    {
                        if (startResolution)
                        {
                            if (menuWindow != null)
                            {
                                CreateDelay(2);
                                menuWindow.nextItem();
                            }
                            else
                            {
                                System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
                            }
                        }
                    }
                    if (modeFSM == 10)
                    {
                        if (startResolution)
                        {
                            if (menuWindow != null)
                            {
                                CreateDelay(2);
                                menuWindow.enterItem();
                            }
                            else
                            {
                                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                            }
                        }
                    }
                    if (modeFSM == 5)
                    {
                        if (device.AudioEndpointVolume.MasterVolumeLevelScalar != 0)
                        {
                            if (device.AudioEndpointVolume.MasterVolumeLevelScalar > 0.05)
                            {
                                device.AudioEndpointVolume.MasterVolumeLevelScalar = device.AudioEndpointVolume.MasterVolumeLevelScalar - (float)0.05;
                            }
                            if (device.AudioEndpointVolume.MasterVolumeLevelScalar < 0.05)
                            {
                                device.AudioEndpointVolume.MasterVolumeLevelScalar = 0;
                            }
                            if (startResolution && menuWindow == null)
                            {
                                if (FormVolume == null)
                                {
                                    FormVolume = new Volume
                                    {
                                        Tag = this
                                    };
                                    FormVolume.Show();
                                    FormVolume.Closed += FormVolume_Closed;
                                }
                            }
                        }
                    }
                    if (modeFSM == 4)
                    {
                        if (device.AudioEndpointVolume.MasterVolumeLevelScalar != 1)
                        {
                            if (device.AudioEndpointVolume.MasterVolumeLevelScalar < 0.95)
                            {
                                device.AudioEndpointVolume.MasterVolumeLevelScalar = device.AudioEndpointVolume.MasterVolumeLevelScalar + (float)0.05;
                            }
                            if (device.AudioEndpointVolume.MasterVolumeLevelScalar > 0.95)
                            {
                                device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                            }
                            if (startResolution)
                            {
                                if (FormVolume == null && menuWindow == null)
                                {
                                    FormVolume = new Volume
                                    {
                                        Tag = this
                                    };
                                    FormVolume.Closed += FormVolume_Closed;
                                    FormVolume.Show();
                                }
                            }
                        }
                    }
                    if (modeFSM == 11)
                    {
                        if (startResolution)
                        {
                            if (Settings.Default.hideWhenRecalledWeather)
                            {
                                if (FullScreenW != null)
                                {
                                    windowFFS = true;
                                }
                                else
                                {
                                    CreateDelay(3);
                                    FullScreenW = new FullScreenWeather();
                                    FullScreenW.Closed += FullScreenWeather_Closed;
                                    FullScreenW.Tag = this;
                                    FullScreenW.Show();
                                }
                            }
                            else
                            {
                                if (FormFullScreen == null)
                                {
                                    CreateDelay(4);
                                    FullScreenW = new FullScreenWeather();
                                    FullScreenW.Closed += FullScreenWeather_Closed;
                                    FullScreenW.Tag = this;
                                    FullScreenW.Show();
                                }
                            }
                        }
                    }
                    if (modeFSM == 12)
                    {
                        if (startResolution)
                        {
                            CreateDelay(2);
                            SetFanState(!FanEnabled);
                            if (FanEnabled)
                            {
                                ShowNotification(1);
                            }
                            else
                            {
                                ShowNotification(0);
                            }
                        }
                    }
                    if (modeFSM == 13)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        void FormFullScreen_Closed(object sender, EventArgs e)
        {
            windowFFS = false;
            FormFullScreen = null;
        }
        void FullScreenWeather_Closed(object sender, EventArgs e)
        {
            windowFFS = false;
            FullScreenW = null;
        }

        void FormVolume_Closed(object sender, EventArgs e)
        {
            FormVolume = null;
        }

        void FormMenu_Closed(object sender, EventArgs e)
        {
            menuWindow = null;
        }
        private void OnClickTrayIcon(object sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                if (IsActive)
                {
                    AnimateWindow(0);
                }
                else
                {
                    Activate();
                }
            }
            else
            {
                AnimateWindow(1);
            }
        }
        DoubleAnimation animClose, animOpen;
        public void AnimateWindow(int mode)
        {
            if (mode == 0)
            {
                animClose = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(350)));
                animClose.Completed += (s, a) => this.Hide();
                this.BeginAnimation(Window.OpacityProperty, animClose);
            }
            if (mode == 1)
            {
                this.Show();
                animOpen = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(350)));
                animOpen.Completed += (s, a) =>
                {
                    this.Activate();
                };
                this.BeginAnimation(Window.OpacityProperty, animOpen);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!Settings.Default.LockingWindow)
            {
                AnimateWindow(0);
            }
            else
            {
                dropShadowWindow.Opacity = 0.0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SetFanState(false);
            arduinoPort.Close();
            JReceiver.DetachDuplexInputChannel();
        }


        WebClient JsonParser = new WebClient();
        WebClient ImageDownloader = new WebClient();
        HelpClass helper = new HelpClass();
        const string apiUrl = "http://api.openweathermap.org/data/2.5/weather?lat=[replaselat]&lon=[replaselon]&lang=ru&units=metric&appid=6f20e7bfbca27f5c319265ac3df56662";

        double[] coordinates = new double[2];
        public void LoadWeatherInfo()
        {
            int connection = helper.GetConnectionAviable();
            if (connection == 1)
            {
                try
                {

                    //rotateUpdate.Dispatcher.BeginInvoke(new putAnimationDelegate(PutAnimationUpdate),updateAnimation);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                try
                {
                    string url = "";
                    bool errorAviables = false;
                    if (Settings.Default.AutoLocation)
                    {
                        coordinates = helper.GetLocation();
                        if (coordinates[0] != 0 && coordinates[1] != 0)
                        {
                            url = apiUrl.Replace("[replaselat]", coordinates[0].ToString());
                            url = url.Replace("[replaselon]", coordinates[1].ToString());
                        }
                        else
                        {
                            ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                    "Нет доступа к местоположению",
                    "Отсутствует доступ к местоположению.\nПроверьте настройки конфиденциальности, включите службу определения местоположения.");
                            errorAviables = true;
                            endUpdate = true;
                        }
                    }
                    else
                    {
                        if (Settings.Default.Latitude != 0 && Settings.Default.Longitude != 0)
                        {
                            url = apiUrl.Replace("[replaselat]", Settings.Default.Latitude.ToString());
                            url = url.Replace("[replaselon]", Settings.Default.Longitude.ToString());
                        }
                        else
                        {
                            /*MessageBoxResult result = CustomMessageBox.ShowOKCancel(
                                "Хотите определить местоположение автоматически или задать вручную?",
                                "Местоположение не задано",
                                "Вручную",
                                "Автоматически",
                                MessageBoxImage.Question);
                            */
                            if (!ShowDialigNotification(NotificationType.Dialog, "Местоположение не задано", "Хотите определить местоположение автоматически?", DialogType.ConfirmCancel))
                            {
                                AnimateWindow(0);
                                if (SettingF == null)
                                {
                                    SettingF = new SettingForm
                                    {
                                        Tag = this
                                    };
                                    SettingF.Show();
                                }
                                SettingF.SelectItem(7);
                            }
                            else
                            {
                                Settings.Default.AutoLocation = true;
                                Settings.Default.Save();
                                coordinates = helper.GetLocation();
                                if (coordinates[0] != 0 && coordinates[1] != 0)
                                {
                                    url = apiUrl.Replace("[replaselat]", coordinates[0].ToString());
                                    url = url.Replace("[replaselon]", coordinates[1].ToString());
                                }
                                else
                                {

                                    ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                            "Нет доступа к местоположению",
                            "Отсутствует доступ к местоположению.\nПроверьте настройки конфиденциальности, включите службу определения местоположения.");
                                    errorAviables = true;
                                    endUpdate = true;
                                }
                            }
                        }
                    }
                    if (!errorAviables)
                    {
                        endUpdate = false;
                        Dispatcher.Invoke(() =>
                        {
                            AnimationUpdate();
                        });
                        JsonParser.Encoding = Encoding.UTF8;
                        JsonParser.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Detail_DownloadStringCompleted);
                        ImageDownloader.DownloadFileCompleted += new AsyncCompletedEventHandler(Image_DownloadDataCompleted);
                        JsonParser.DownloadStringAsync(new Uri(url));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    endUpdate = true;
                    ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                    "Ошибка при обновлении погоды",
                    "Не возможно обновить информацию о погоде из-за внутрипрограммной ошибки.");
                }
            }
            else if (connection == 2)
            {
                logger.Info("Не возможно обновить информацию о погоде из-за проблем подключения к сети.");
                ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                    "Время ожидания подключения вышло",
                    "Не возможно обновить информацию о погоде из-за проблем подключения к сети.");
            }
            else
            {
                logger.Info("Не возможно обновить информацию о погоде из-за отсутсвия подключения к сети.");
                ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                    "Нет соединения",
                    "Не возможно обновить информацию о погоде из-за отсутсвия подключения к сети.");
            }
        }
        private void PutInfo(string text, string hint)
        {
            if (TitleText.Length > 0)
            {
                ActualTime.Text = TitleText + " - " + text;
                ActualTime.ToolTip = hint;
            }
            else
            {
                ActualTime.Text = text;
                ActualTime.ToolTip = hint;
            }

        }
        private void Image_DownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
        {
            WeatherImage.Source = new BitmapImage(new Uri(dirOfImage + WeatherInfo.weather[0].icon + ".png"));
        }


        Rootobject WeatherInfo;
        static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        bool endUpdate = true;
        DoubleAnimation doubleAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromMilliseconds(1000)))
        {
            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
        };
        private void AnimationUpdate()
        {
            rotateUpdate.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
        }
        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            if (!endUpdate)
            {
                rotateUpdate.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            }
        }
        string TitleText = String.Empty;
        private void PutWeatherInfo(Rootobject rootobject)
        {
            WeatherInfo = rootobject;
            WeatherBlock.Content = WeatherInfo.weather[0].description.First().ToString().ToUpper() + WeatherInfo.weather[0].description.Substring(1);
            if (WeatherInfo.main.temp > 0)
            {
                TWeatherBlock.Content = "+" + Convert.ToInt32(WeatherInfo.main.temp) + "°";
            }
            else
            {
                TWeatherBlock.Content = Convert.ToInt32(WeatherInfo.main.temp) + "°";
            }
            DoubleAnimation windAnimation = new DoubleAnimation(rotateWind.Angle, WeatherInfo.wind.deg, new Duration(TimeSpan.FromMilliseconds(550)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };
            DateTime actualDate = ConvertFromUnixTimestamp(WeatherInfo.dt);
            DateTime sunriseDate = ConvertFromUnixTimestamp(WeatherInfo.sys.sunrise);
            DateTime sunsetDate = ConvertFromUnixTimestamp(WeatherInfo.sys.sunset);
            TimeSpan spanDay = sunsetDate - sunriseDate;
            WeatherImage.ToolTip = "Облака " + WeatherInfo.clouds.all.ToString() + "%";
            DateTime localDate = DateTime.Now;
            TitleText = "Актуально на " + actualDate.ToLocalTime().ToShortTimeString()
                + " [ Обновлено в " + localDate.ToShortTimeString() + " ] " + WeatherInfo.name + "," + WeatherInfo.sys.country;
            ActualTime.Text = TitleText;
            ActualTime.ToolTip = null;
            SWeatherBlock.Content = sunriseDate.ToLocalTime().ToShortTimeString();
            EWeatherBlock.Content = sunsetDate.ToLocalTime().ToShortTimeString();

            DWeatherBlock.Content = new HelpClass().GetLenghtDay(spanDay);
            HWeatherBlock.Content = WeatherInfo.main.humidity + "%";
            string pressure = Convert.ToInt32(WeatherInfo.main.pressure) + " гПа";
            if (Settings.Default.unitAP.Equals("mm"))
            {
                pressure = new HelpClass().GetMMRS(WeatherInfo.main.pressure) + " мм рт.ст.";
            }
            PWeatherBlock.Content = pressure;
            if (WeatherInfo.wind.speed != 0)
            {
                WindDeg.Visibility = Visibility.Visible;
                string speed = Convert.ToString(WeatherInfo.wind.speed) + " м/с, ";
                if (Settings.Default.unitWind.Equals("kmh"))
                {
                    speed = new HelpClass().GetKmH(WeatherInfo.wind.speed) + " км/ч, ";
                }
                WWeatherBlock.Content = speed + new HelpClass().GetWind(WeatherInfo.wind.deg);
                rotateWind.BeginAnimation(RotateTransform.AngleProperty, windAnimation);
                WindDeg.ToolTip = new HelpClass().GetFullWind(WeatherInfo.wind.deg) + " (" + WeatherInfo.wind.deg.ToString() + "°)";
                WWeatherBlock.ToolTip = WindDeg.ToolTip;
            }
            else
            {
                WindDeg.Visibility = Visibility.Hidden;
                WWeatherBlock.Content = "Штиль";
            }
            if (WeatherInfo.visibility > 1000)
            {
                double count = WeatherInfo.visibility / 1000;
                VWeatherBlock.Content = count.ToString() + " км";
            }
            else
            {
                VWeatherBlock.Content = WeatherInfo.visibility.ToString() + " м";
            }
            WindDeg.RenderTransform = new RotateTransform(WeatherInfo.wind.deg, 0, 0);

            if (!new FileInfo(dirOfImage + WeatherInfo.weather[0].icon + ".png").Exists)
            {
                ImageDownloader.DownloadFileAsync(new Uri("https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/widgets/" + WeatherInfo.weather[0].icon + ".png"), dirOfImage + WeatherInfo.weather[0].icon + ".png");
            }
            else
            {
                WeatherImage.Source = new BitmapImage(new Uri(dirOfImage + WeatherInfo.weather[0].icon + ".png"));
            }
            Dispatcher.Invoke(() =>
            {
                WeatherToast.Show();
                endUpdate = true;
            });
        }
        private void Detail_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {

                Dispatcher.Invoke(() =>
                {
                    Rootobject WI = JsonConvert.DeserializeObject<Rootobject>(e.Result);
                    PutWeatherInfo(WI);
                });

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                ActualTime.Dispatcher.BeginInvoke(new putInfoDelegate(PutInfo),
                    "Ошибка при обновлении погоды",
                    "Не возможно обновить информацию о погоде из-за внутрипрограммной ошибки.");
            }
        }

        private void RefreshWeather_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadWeatherInfo();
        }

        private void OptionsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
            if (SettingF == null)
            {
                SettingF = new SettingForm
                {
                    Tag = this
                };
                SettingF.Show();
            }
            else
            {
                if (!SettingF.IsVisible)
                {
                    SettingF = new SettingForm
                    {
                        Tag = this
                    };
                    SettingF.Show();
                }
                else
                {
                    SettingF.Activate();
                }
            }
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TempChangesButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
            if (ChangeTemp == null)
            {
                ChangeTemp = new TempChange
                {
                    Tag = this
                };
                ChangeTemp.Show();
            }
            else
            {
                if (!ChangeTemp.IsVisible)
                {
                    ChangeTemp = new TempChange
                    {
                        Tag = this
                    };
                    ChangeTemp.Show();
                }
                else
                {
                    ChangeTemp.Activate();
                }
            }
        }
        private void LockWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Settings.Default.LockingWindow = !Settings.Default.LockingWindow;
            Settings.Default.Save();
            if (Settings.Default.LockingWindow)
            {
                LockWindow.Source = BitmapFrame.Create(new Uri(UriLock));
                LockWindow.ToolTip = "Открепить окно";
            }
            else
            {
                LockWindow.Source = BitmapFrame.Create(new Uri(UriUnlock));
                LockWindow.ToolTip = "Закрепить окно";
            }
        }

        private void MinimizeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
        }
        WeatherWindow windowWeather;


        private void MoreWeather_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (windowWeather == null)
            {
                windowWeather = new WeatherWindow
                {
                    Tag = this
                };
                windowWeather.Closed += WindowWeather_Closed;
                windowWeather.Show();
            }
            else
            {
                if (!windowWeather.IsVisible)
                {
                    windowWeather = new WeatherWindow
                    {
                        Tag = this
                    };
                    windowWeather.Show();
                }
                else
                {
                    windowWeather.Close();
                }
            }
        }
        public bool FanEnabled = false;
        private void FanButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FanEnabled = !FanEnabled;
            SetFanState(FanEnabled);
        }
        public void SetFanState(bool state)
        {
            if (arduinoPort.IsOpen)
            {
                if (state)
                {
                    FanEnabled = true;
                    FanButton.Source = BitmapFrame.Create(new Uri(UriFan));
                    FanButton.ToolTip = "Выключить вентилятор";
                    arduinoPort.Write("switchFan true");
                }
                else
                {
                    FanEnabled = false;
                    FanButton.Source = BitmapFrame.Create(new Uri(UriFanOff));
                    FanButton.ToolTip = "Включить вентилятор";
                    arduinoPort.Write("switchFan false");
                }
            }
        }
        //Notification notification;
        Rect primaryMonitorArea = SystemParameters.WorkArea;
        public Double notificationTop = 0;
        public List<NotificationItem> notificationsList = new List<NotificationItem>();
        public void CalculateNotificationsList()
        {
            int count = Convert.ToInt32(primaryMonitorArea.Height / 140);
            if (notificationsList.Count > 0)
            {
                notificationsList.Clear();
            }
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    notificationsList.Add(new NotificationItem { IsShowed = false, Position = 20 });
                }
                else
                {
                    notificationsList.Add(new NotificationItem { IsShowed = false, Position = notificationsList[i - 1].Position + 140 });
                }
            }
        }
        public void ShowNotification(int NotificationNumber, string title = "JWeather", string body = "")
        {
            for (int i = 0; i < notificationsList.Count; i++)
            {
                if (!notificationsList[i].IsShowed)
                {
                    notificationsList[i].IsShowed = true;
                    new Notification(i, NotificationNumber, notificationsList[i].Position, title, body)
                    {
                        Tag = this
                    }.Show();
                    return;
                }
            }
        }
        public bool ShowDialigNotification(int NotificationNumber, string title = "JWeather", string body = "",
            int DialogT = 0, string confirmText = "OK", string cancelText = "Cancel")
        {
            for (int i = 0; i < notificationsList.Count; i++)
            {
                if (!notificationsList[i].IsShowed)
                {
                    notificationsList[i].IsShowed = true;

                    if (new Notification(i, NotificationNumber, notificationsList[i].Position, title, body, DialogT
                        , confirmText, cancelText)
                    {
                        Tag = this
                    }.ShowDialog() == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        private void WeatherBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //ShowNotification(NotificationType.FanOn);
            //ShowNotification(NotificationType.FanOff);
            /*ShowNotification(NotificationType.Error, "Заголовок", "Уведомление об ошибке");
            ShowNotification(NotificationType.HighTemp, "Заголовок", "Уведомление о повышении температуры");
            ShowNotification(NotificationType.LowTemp, "Заголовок", "Уведомление о понижении температуры");*/
            //ShowDialigNotification(NotificationType.Dialog, "Местоположение не задано", "Хотите определить местоположение автоматически?", DialogType.ConfirmCancel);
        }

        void WindowWeather_Closed(object sender, EventArgs e)
        {
            windowWeather = null;
        }

        private void WindowMenu_Activated(object sender, EventArgs e)
        {
            dropShadowWindow.Opacity = 0.5;
        }
    }
}
