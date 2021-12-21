using JWeather.Objects;
using JWeather.Properties;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для WeatherWindow.xaml
    /// </summary>
    public partial class WeatherWindow : Window
    {
        public WeatherWindow()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            SetBackgroundColor();
            /*var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - Width;
            Top = primaryMonitorArea.Bottom - Height - 290;*/
            logger = LogManager.GetCurrentClassLogger();
        }
        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                SetBackgroundColor();
            }
        }
        private void SetBackgroundColor()
        {
            ContentLayout.Background = SystemParameters.WindowGlassBrush;
        }
        
        Logger logger;
        WebClient JsonParser = new WebClient();
        WebClient ImageDownloader = new WebClient();
        public string dirOfImage = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\WeatherImage\";
        const string apiUrl = "http://api.openweathermap.org/data/2.5/forecast?lat=[replaselat]&lon=[replaselon]&lang=ru&units=metric&appid=6f20e7bfbca27f5c319265ac3df56662";
        HelpClass helper = new HelpClass();
        double[] coordinates = new double[2];
        DoubleAnimation updateAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromMilliseconds(1000)))
        {
            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut }
        };
        public void LoadWeatherInfo()
        {
            int connection = helper.GetConnectionAviable();
            if (connection == 1)
            {
                try
                {
                    string url = "";
                    TitleLabel.ToolTip = null;
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
                            TitleLabel.Content = "Нет доступа к местоположению";
                            TitleLabel.ToolTip = "Отсутствует доступ к местоположению.\nПроверьте настройки конфиденциальности, включите службу определения местоположения.";
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
                            TitleLabel.Content = "Местоположение не задано. ";
                            TitleLabel.ToolTip = "Для настройки местоположения перейдите в настройки приложения. Настройки -> Погода -> Местоположение.";
                        }
                    }
                    if (!errorAviables)
                    {
                        endUpdate = false;
                        rotateUpdate.BeginAnimation(RotateTransform.AngleProperty, updateAnimation);
                        JsonParser.Encoding = Encoding.UTF8;
                        JsonParser.DownloadStringCompleted += new DownloadStringCompletedEventHandler(Detail_DownloadStringCompleted);
                        //ImageDownloader.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(Image_DownloadDataCompleted);
                        JsonParser.DownloadStringAsync(new Uri(url));
                    }
                }
                catch (Exception ex)
                {
                    TitleLabel.Content = "Ошибка";
                    logger.Error(ex);
                    endUpdate = true;
                }
            } 
            else if (connection == 2)
            {
                logger.Info("Не возможно обновить информацию о погоде из-за проблем подключения к сети.");
                TitleLabel.Content = "Время ожидания подключения вышло";
            }
            else
            {
                logger.Info("Не возможно обновить информацию о погоде из-за отсутствия подключения к сети.");
                TitleLabel.Content = "Нет соединения";
            }
        }
        //BitmapImage addImage;
        private void Image_DownloadDataCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //addImage= new BitmapImage(new Uri(dirOfImage + WeatherInfo.weather[0].icon + ".png"));

        }

        WeatherForecast WeatherInfo;
        static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
        DoubleAnimation animClose, animOpen;
        public void AnimateWindow(int mode)
        {
            if (mode == 0)
            {
                animClose = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(150)));
                animClose.Completed += (s, a) =>
                {
                    Close();
                };
                this.BeginAnimation(Window.OpacityProperty, animClose);
            }
            if (mode == 1)
            {
                animOpen = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(150)));
                animOpen.Completed += (s, a) =>
                {
                    LoadWeatherInfo();
                };
                this.BeginAnimation(Window.OpacityProperty, animOpen);
            }
        }
        List<DateTime> listDate = new List<DateTime>();
        public void LoadList()
        {
            if(DateComboBox.Items.Count > 0)
            {
                DateComboBox.Items.Clear();
            }
            if(listDate.Count > 0)
            {
                listDate.Clear();
            }
            foreach (List item in WeatherInfo.list)
            {
                DateTime dateTime = ConvertFromUnixTimestamp(item.dt).ToLocalTime();
                //MessageBox.Show(dateTime.ToShortDateString());
                if (!listDate.Contains(dateTime.Date))
                {
                    listDate.Add(dateTime.Date);
                }
            }
            
            foreach(DateTime t in listDate)
            {
                if (DateTime.Now.Date == t || DateTime.Now.AddDays(1).Date == t || DateTime.Now.AddDays(2).Date == t)
                {
                    if (DateTime.Now.Date == t)
                    {
                        DateComboBox.Items.Add("Сегодня");
                    }
                    if (DateTime.Now.AddDays(1).Date == t)
                    {
                        DateComboBox.Items.Add("Завтра");
                    }
                    if (DateTime.Now.AddDays(2).Date == t)
                    {
                        DateComboBox.Items.Add("Послезавтра");
                    }
                }
                else
                {
                    DateComboBox.Items.Add(t.ToString("d MMMM"));
                }
            }
            if(DateComboBox.SelectedIndex == -1)
            {
                DateComboBox.SelectedIndex = 0;
            }
            
            
        }
        bool endUpdate = true;
        private void Detail_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                WeatherInfo = JsonConvert.DeserializeObject<WeatherForecast>(e.Result);
                for (int i = 0; i < WeatherInfo.cnt;i++)
                {
                    
                    if (!new FileInfo(dirOfImage + WeatherInfo.list[i].weather[0].icon + ".png").Exists)
                    {
                        ImageDownloader.DownloadFileAsync(new Uri("https://openweathermap.org/themes/openweathermap/assets/vendor/owm/img/widgets/" + WeatherInfo.list[i].weather[0].icon + ".png"), dirOfImage + WeatherInfo.list[i].weather[0].icon + ".png");
                    }
                }
                LoadList();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        
        private void ContentLayout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
        }

        private void DateComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (listBoxForecast.Items.Count > 0)
                {
                    listBoxForecast.Items.Clear();
                }
                foreach (List item in WeatherInfo.list)
                {
                    if (ConvertFromUnixTimestamp(item.dt).ToLocalTime().Date == listDate[DateComboBox.SelectedIndex].Date)
                    {
                        string temp = "";
                        if (item.main.temp > 0)
                        {
                            temp = "+" + Convert.ToInt32(item.main.temp) + "°";
                        }
                        else
                        {
                            temp = Convert.ToInt32(item.main.temp) + "°";
                        }
                        Visibility WAVisibility = Visibility.Visible;
                        string speed = "";
                        if (item.wind.speed != 0)
                        {
                            speed = Convert.ToString(item.wind.speed) + " м/с, ";

                            if (Settings.Default.unitWind.Equals("kmh"))
                            {
                                speed = new HelpClass().GetKmH(item.wind.speed) + " км/ч, ";
                            }
                            speed = speed + " " + new HelpClass().GetFullWind(item.wind.deg);
                        }
                        else
                        {
                            WAVisibility = Visibility.Hidden;
                            speed = "Штиль";
                        }
                        BitmapImage outImage = new BitmapImage(new Uri(dirOfImage + item.weather[0].icon + ".png"));
                        listBoxForecast.Items.Add(new
                        {
                            Date = ConvertFromUnixTimestamp(item.dt).ToLocalTime().ToShortTimeString(),
                            Temperature = temp,
                            ImageWeather = outImage,
                            WindAngleVisibility = WAVisibility,
                            WindAngle = item.wind.deg,
                            WindInfo = speed,
                            Description = item.weather[0].description.First().ToString().ToUpper() + item.weather[0].description.Substring(1)
                        });
                    }
                }
                endUpdate = true;
                TitleLabel.Content = " Прогноз на " + listDate[DateComboBox.SelectedIndex].ToString("d MMMM") + " на каждые 3 часа.";
                DateComboBox.Focus();
            }catch
            {
                //logger.Error(ex);
            }
        }

        private void RefreshWeather_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadWeatherInfo();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateAnimation.Completed += UpdateAnimation_Completed;
            AnimateWindow(1);
        }

        private void UpdateAnimation_Completed(object sender, EventArgs e)
        {
            if (!endUpdate)
            {
                rotateUpdate.BeginAnimation(RotateTransform.AngleProperty, updateAnimation);
            }
        }
    }
}
