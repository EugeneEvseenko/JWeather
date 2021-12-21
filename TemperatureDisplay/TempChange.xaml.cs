using JWeather.Properties;
using LiveCharts;
using LiveCharts.Wpf;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для TempChange.xaml
    /// </summary>

    public partial class TempChange : Window
    {
        public TempChange()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
            this.SetBackgroundColor();
            Opacity = 0;
            logger = LogManager.GetCurrentClassLogger();
            Color white = Colors.White;
            white.ScA = 0.1F;
            itemBrush = new SolidColorBrush(white);
        }
        protected override void OnClosed(EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= this.SystemParameters_StaticPropertyChanged;
            base.OnClosed(e);
        }

        private void SetBackgroundColor()
        {
            currentBrush = SystemParameters.WindowGlassBrush;
            TempLayout.Background = currentBrush;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                this.SetBackgroundColor();
            }
        }
        Logger logger;
        List<YearItem> listYears = new List<YearItem>();
        HelpClass FSP = new HelpClass();
        string dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\LogTemperature\";
        int lastISM;
        int lastISD;
        Brush currentBrush;
        SolidColorBrush itemBrush;
        DoubleAnimation opacityAnimationOpen, opacityAnimationClose, positionOpen, positionClose;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] LabelsX { get; set; }
        public string[] LabelsY { get; set; }
        public DayItem SelectedDay;
        public SQLiteConnection SQLconnection = new SQLiteConnection();
        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                Start();
            };
            
            AnimateWindow(1);
            
        }
        public void Start()
        {
            listBoxHistory.ItemsSource = list;
            try
            {
                if (Directory.GetDirectories(dir).Length != 0)
                {
                    string[] yearsDirs = Directory.GetDirectories(dir);
                    for (int y = 0; y < yearsDirs.Length; y++)
                    {
                        string[] monthFiles = Directory.GetFiles(yearsDirs[y]);
                        listYears.Add(new YearItem
                        {
                            Year = Convert.ToInt32(yearsDirs[y].Substring(yearsDirs[y].LastIndexOf(@"\") + 1)),
                            CountMonths = monthFiles.Length,
                            ListMonths = new List<MonthItem>(12)
                        });
                        listYearsCB.Items.Add(listYears[y].Year.ToString());
                        for (int m = 0; m < monthFiles.Length; m++)
                        {
                            string nameMonth = monthFiles[m].Substring(monthFiles[m].LastIndexOf(@"\") + 1);
                            nameMonth = nameMonth.Remove(nameMonth.LastIndexOf("."));
                            listYears[y].ListMonths.Insert(m, new MonthItem
                            {
                                MonthNumber = Convert.ToInt32(nameMonth),
                                ListDays = new List<DayItem>()
                            });
                            SQLiteConnection connection =
                                new SQLiteConnection(string.Format("Data Source={0};", monthFiles[m]));
                            SQLiteCommand commandDay =
                                new SQLiteCommand("select * from sqlite_sequence;", connection);
                            connection.Open();
                            SQLiteDataReader readerDay = commandDay.ExecuteReader();
                            int counter = 0;
                            foreach (DbDataRecord recordDay in readerDay)
                            {
                                listYears[y].ListMonths[m].ListDays.Add(new DayItem
                                {
                                    DayNumber = Convert.ToInt32(recordDay["name"].ToString()),
                                    ListHum = new List<int>(),
                                    ListTemp = new List<int>(),
                                    ListTime = new List<string>(),
                                    WTemp = new List<int>(),
                                    WHum = new List<int>(),
                                    WPressure = new List<int>(),
                                    WWindDeg = new List<int>(),
                                    WWindSpeed = new List<int>(),
                                    WSunset = new List<int>(),
                                    WSunrise = new List<int>(),
                                    WVisibility = new List<int>(),
                                    WDescription = new List<string>(),
                                    WImage = new List<string>()
                                });
                                SQLiteCommand commandDayData = new SQLiteCommand("select * from '" + recordDay["name"].ToString() + "'; ", connection);
                                SQLiteDataReader readerDayData = commandDayData.ExecuteReader();
                                foreach (DbDataRecord recordDayData in readerDayData)
                                {
                                    //MessageBox.Show(recordDayData["Time"].ToString());
                                    listYears[y].ListMonths[m].ListDays[counter].ListTemp.Add(Convert.ToInt32(recordDayData["Temperature"].ToString()));
                                    listYears[y].ListMonths[m].ListDays[counter].ListHum.Add(Convert.ToInt32(recordDayData["Humidity"].ToString()));
                                    listYears[y].ListMonths[m].ListDays[counter].ListTime.Add(recordDayData["Time"].ToString());
                                    if (!recordDayData["WeatherTemp"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WTemp.Add(Convert.ToInt32(recordDayData["WeatherTemp"].ToString()));
                                    }
                                    if (!recordDayData["WeatherHum"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WHum.Add(Convert.ToInt32(recordDayData["WeatherHum"].ToString()));
                                    }
                                    if (!recordDayData["WeatherPressure"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WPressure.Add(Convert.ToInt32(recordDayData["WeatherPressure"].ToString()));
                                    }
                                    if (!recordDayData["WeatherWindDeg"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WWindDeg.Add(Convert.ToInt32(recordDayData["WeatherWindDeg"].ToString()));
                                    }
                                    if (!recordDayData["WeatherWindSpeed"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WWindSpeed.Add(Convert.ToInt32(recordDayData["WeatherWindSpeed"].ToString()));
                                    }
                                    if (!recordDayData["WeatherSunset"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WSunset.Add(Convert.ToInt32(recordDayData["WeatherSunset"].ToString()));
                                    }
                                    if (!recordDayData["WeatherSunrise"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WSunrise.Add(Convert.ToInt32(recordDayData["WeatherSunrise"].ToString()));
                                    }
                                    if (!recordDayData["WeatherVisibility"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WVisibility.Add(Convert.ToInt32(recordDayData["WeatherVisibility"].ToString()));
                                    }
                                    if (!recordDayData["WeatherDescription"].ToString().Equals(string.Empty))
                                    {
                                        listYears[y].ListMonths[m].ListDays[counter].WDescription.Add(recordDayData["WeatherDescription"].ToString());
                                    }
                                    if (!recordDayData["WeatherImage"].ToString().Equals(string.Empty))
                                    { listYears[y].ListMonths[m].ListDays[counter].WImage.Add(recordDayData["WeatherImage"].ToString()); }
                                }
                                counter++;
                            }
                            connection.Close();
                        }

                    }
                    if (listYears.Count > 0)
                    {
                        listYearsCB.SelectedIndex = listYearsCB.Items.Count - 1;
                    }
                    GraphT.DataContext = this;
                    GraphT.DataTooltip.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
                    GraphModeSelector.SelectedIndex = Settings.Default.chartMode;
                }
                else
                {
                    this.Close();
                    MessageBox.Show("Нет записей для отображения.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public void ClearVisibilityMonths()
        {
            Jan.Visibility = Visibility.Collapsed;
            Feb.Visibility = Visibility.Collapsed;
            Mar.Visibility = Visibility.Collapsed;
            Apr.Visibility = Visibility.Collapsed;
            May.Visibility = Visibility.Collapsed;
            May.Visibility = Visibility.Collapsed;
            Jun.Visibility = Visibility.Collapsed;
            Jul.Visibility = Visibility.Collapsed;
            Aug.Visibility = Visibility.Collapsed;
            Sep.Visibility = Visibility.Collapsed;
            Oct.Visibility = Visibility.Collapsed;
            Nov.Visibility = Visibility.Collapsed;
            Dec.Visibility = Visibility.Collapsed;
            Jan.IsEnabled = false;
            Feb.IsEnabled = false;
            Mar.IsEnabled = false;
            Apr.IsEnabled = false;
            May.IsEnabled = false;
            May.IsEnabled = false;
            Jun.IsEnabled = false;
            Jul.IsEnabled = false;
            Aug.IsEnabled = false;
            Sep.IsEnabled = false;
            Oct.IsEnabled = false;
            Nov.IsEnabled = false;
            Dec.IsEnabled = false;
        }
        public void SetVisibleMonth(int Index)
        {

            switch (Index)
            {
                case 1:
                    {
                        Jan.Visibility = Visibility.Visible;
                        Jan.IsEnabled = true;
                    }
                    break;
                case 2:
                    {
                        Feb.Visibility = Visibility.Visible;
                        Feb.IsEnabled = true;
                    }
                    break;
                case 3:
                    {
                        Mar.Visibility = Visibility.Visible;
                        Mar.IsEnabled = true;
                    }
                    break;
                case 4:
                    {
                        Apr.Visibility = Visibility.Visible;
                        Apr.IsEnabled = true;
                    }
                    break;
                case 5:
                    {
                        May.Visibility = Visibility.Visible;
                        May.IsEnabled = true;
                    }
                    break;
                case 6:
                    {
                        Jun.Visibility = Visibility.Visible;
                        Jun.IsEnabled = true;
                    }
                    break;
                case 7:
                    {
                        Jul.Visibility = Visibility.Visible;
                        Jul.IsEnabled = true;
                    }
                    break;
                case 8:
                    {
                        Aug.Visibility = Visibility.Visible;
                        Aug.IsEnabled = true;
                    }
                    break;
                case 9:
                    {
                        Sep.Visibility = Visibility.Visible;
                        Sep.IsEnabled = true;
                    }
                    break;
                case 10:
                    {
                        Oct.Visibility = Visibility.Visible;
                        Oct.IsEnabled = true;
                    }
                    break;
                case 11:
                    {
                        Nov.Visibility = Visibility.Visible;
                        Nov.IsEnabled = true;
                    }
                    break;
                case 12:
                    {
                        Dec.Visibility = Visibility.Visible;
                        Dec.IsEnabled = true;
                    }
                    break;
            }
        }
        MonthItem selectedMonth = new MonthItem();
        public MonthItem SelectFirstAviableMonth()
        {
            for (int i = listYears[listYearsCB.SelectedIndex].ListMonths.Count - 1; i > 0; i--)
            {
                if (listYears[listYearsCB.SelectedIndex].ListMonths[i].ListDays.Count != 0)
                {
                    return listYears[listYearsCB.SelectedIndex].ListMonths[i];
                }
            }
            return new MonthItem();
        }
        private void listYearsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ClearVisibilityMonths();
                foreach (MonthItem mi in listYears[listYearsCB.SelectedIndex].ListMonths)
                {
                    if (mi.ListDays.Count > 0)
                    {
                        SetVisibleMonth(mi.MonthNumber);
                    }
                }
                selectedMonth = SelectFirstAviableMonth();
                listMonthsCB.SelectedIndex = selectedMonth.MonthNumber - 1;
                lastISM = selectedMonth.MonthNumber - 1;
                rightYear.Text = listYears[listYearsCB.SelectedIndex].ListMonths.Count.ToString() + " " + StaticHelper.getEnding(listYears[listYearsCB.SelectedIndex].ListMonths.Count, "записанных месяцев", "записанный месяц", "записанных месяца") + " за " + listYears[listYearsCB.SelectedIndex].Year.ToString() + " год";
                if (Settings.Default.chartMode == GraphType.AverageYear)
                {
                    UpdateGraph(GraphType.AverageYear);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public void UpdateGraph(int Mode)
        {
            try
            {
                switch (Mode)
                {
                    case GraphType.DayData:
                        {
                            xAx.Title = "Время";
                            yAx.Title = "Данные (° , %)";
                            ChartValues<double> tempChart = new ChartValues<double>(), HumChart = new ChartValues<double>();
                            for (int i = 0; i < listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListHum.Count; i++)
                            {
                                tempChart.Add(SelectedDay.ListTemp[i]);
                                HumChart.Add(SelectedDay.ListHum[i]);
                            }
                            SeriesCollection = new SeriesCollection
                            {
                               new LineSeries
                               {
                                  Title = "Температура",
                                  Values = tempChart,
                                  //Stroke = new SolidColorBrush(Colors.SpringGreen),
                                  PointGeometry = null
                               },
                               new LineSeries
                               {
                                  Title = "Влажность",
                                  Values = HumChart,
                                  //Stroke = new SolidColorBrush(Colors.Tomato),
                                  PointGeometry = null
                               }
                            };
                            LabelsX = SelectedDay.ListTime.ToArray();
                            xAx.Labels = SelectedDay.ListTime.ToArray();
                        }
                        break;
                    case GraphType.AverageMonth:
                        {
                            xAx.Title = "День";
                            yAx.Title = "Данные (° , %)";
                            ChartValues<double> tempChart = new ChartValues<double>();
                            ChartValues<double> humChart = new ChartValues<double>();
                            List<string> days = new List<string>();
                            foreach (DayItem mi in selectedMonth.ListDays)
                            {
                                int t = 0, h = 0;
                                for (int i = 0; i < mi.ListHum.Count; i++)
                                {
                                    t += mi.ListTemp[i];
                                    h += mi.ListHum[i];
                                }
                                days.Add(mi.DayNumber.ToString() + " " + StaticHelper.GetMonth(true, selectedMonth.MonthNumber).ToLower());
                                tempChart.Add(Convert.ToDouble(t / mi.ListHum.Count));
                                humChart.Add(Convert.ToDouble(h / mi.ListHum.Count));
                            }
                            SeriesCollection = new SeriesCollection
                            {
                               new LineSeries
                               {
                                  Title = "Средняя температура",
                                  Values = tempChart,
                                  //Stroke = new SolidColorBrush(Colors.SpringGreen),
                                  PointGeometry = null
                               },
                               new LineSeries
                               {
                                  Title = "Средняя влажность",
                                  Values = humChart,
                                  //Stroke = new SolidColorBrush(Colors.Tomato),
                                  PointGeometry = null
                               }
                            };
                            LabelsX = new string[days.Count];
                            LabelsX = days.ToArray();
                            xAx.Labels = days.ToArray();
                        }
                        break;
                    case GraphType.MinimalMonth:
                        {
                            xAx.Title = "День";
                            yAx.Title = "Данные (° , %)";
                            ChartValues<double> tempChart = new ChartValues<double>();
                            ChartValues<double> humChart = new ChartValues<double>();
                            List<string> days = new List<string>();
                            foreach (DayItem mi in selectedMonth.ListDays)
                            {
                                days.Add(mi.DayNumber.ToString() + " " + StaticHelper.GetMonth(true, selectedMonth.MonthNumber).ToLower());
                                tempChart.Add(Convert.ToDouble(mi.ListTemp.ToArray().Min()));
                                humChart.Add(Convert.ToDouble(mi.ListHum.ToArray().Min()));
                            }
                            SeriesCollection = new SeriesCollection
                            {
                               new LineSeries
                               {
                                  Title = "Минимальная температура",
                                  Values = tempChart,
                                  //Stroke = new SolidColorBrush(Colors.SpringGreen),
                                  PointGeometry = null
                               },
                               new LineSeries
                               {
                                  Title = "Минимальная влажность",
                                  Values = humChart,
                                  //Stroke = new SolidColorBrush(Colors.Tomato),
                                  PointGeometry = null
                               }
                            };
                            LabelsX = new string[days.Count];
                            LabelsX = days.ToArray();
                            xAx.Labels = days.ToArray();
                        }
                        break;

                    case GraphType.MaximalMonth:
                        {
                            xAx.Title = "День";
                            yAx.Title = "Данные (° , %)";
                            ChartValues<double> tempChart = new ChartValues<double>();
                            ChartValues<double> humChart = new ChartValues<double>();
                            List<string> days = new List<string>();
                            foreach (DayItem mi in selectedMonth.ListDays)
                            {
                                days.Add(mi.DayNumber.ToString() + " " + StaticHelper.GetMonth(true, selectedMonth.MonthNumber).ToLower());
                                tempChart.Add(Convert.ToDouble(mi.ListTemp.ToArray().Max()));
                                humChart.Add(Convert.ToDouble(mi.ListHum.ToArray().Max()));
                            }
                            SeriesCollection = new SeriesCollection
                            {
                               new LineSeries
                               {
                                  Title = "Максимальная температура",
                                  Values = tempChart,
                                  //Stroke = new SolidColorBrush(Colors.SpringGreen),
                                  PointGeometry = null
                               },
                               new LineSeries
                               {
                                  Title = "Максимальная влажность",
                                  Values = humChart,
                                  //Stroke = new SolidColorBrush(Colors.Tomato),
                                  PointGeometry = null
                               }
                            };
                            LabelsX = new string[days.Count];
                            LabelsX = days.ToArray();
                            xAx.Labels = days.ToArray();
                        }
                        break;
                    case GraphType.AverageYear:
                        {
                            xAx.Title = "Месяц";
                            yAx.Title = "Данные (° , %)";
                            ChartValues<double> tempChart = new ChartValues<double>();
                            ChartValues<double> humChart = new ChartValues<double>();
                            List<string> months = new List<string>();
                            foreach (MonthItem mi in listYears[listYearsCB.SelectedIndex].ListMonths)
                            {
                                int monthH = 0, monthT = 0, d = 0;
                                months.Add(StaticHelper.GetMonth(false, mi.MonthNumber));
                                foreach (DayItem di in mi.ListDays)
                                {
                                    for (int i = 0; i < di.ListHum.Count; i++)
                                    {
                                        monthH += di.ListHum[i];
                                        monthT += di.ListTemp[i];
                                    }
                                    d += di.ListTemp.Count;
                                }
                                tempChart.Add(Convert.ToDouble(monthT / d));
                                humChart.Add(Convert.ToDouble(monthH / d));
                            }
                            SeriesCollection = new SeriesCollection
                            {
                               new LineSeries
                               {
                                  Title = "Средняя температура",
                                  Values = tempChart,
                                  //Stroke = new SolidColorBrush(Colors.SpringGreen),
                                  PointGeometry = null
                               },
                               new LineSeries
                               {
                                  Title = "Средняя влажность",
                                  Values = humChart,
                                  //Stroke = new SolidColorBrush(Colors.Tomato),
                                  PointGeometry = null
                               }
                            };
                            LabelsX = new string[months.Count];
                            LabelsX = months.ToArray();
                            xAx.Labels = months.ToArray();
                        }
                        break;
                }

                GraphT.Series = SeriesCollection;
                Settings.Default.chartMode = Mode;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        private void listMonthsCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectedMonth = listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex];
                if (listYears[listYearsCB.SelectedIndex].ListMonths[listYears[listYearsCB.SelectedIndex].ListMonths.Count - 1].ListDays.Count > 0)
                {
                    listDayCB.Items.Clear();
                    foreach (DayItem di in selectedMonth.ListDays)
                    {
                        listDayCB.Items.Add(new ComboBoxItem { Content = di.DayNumber.ToString() });
                    }
                    listDayCB.SelectedIndex = listDayCB.Items.Count - 1;
                    lastISD = listDayCB.SelectedIndex;
                    rightMonth.Text = listDayCB.Items.Count.ToString() + " " + StaticHelper.getEnding(listDayCB.Items.Count, "записанных дней", "запасанный день", "записанных дня") + " за " + StaticHelper.GetMonth(false, selectedMonth.MonthNumber).ToLower();
                }
                else
                {
                    MessageBox.Show("За " + listMonthsCB.SelectedItem.ToString().Substring(4).ToLower() + " нет записей для отображения.");
                    listMonthsCB.SelectedIndex = lastISM;
                    listDayCB.SelectedIndex = lastISD;
                }
                if (Settings.Default.chartMode == GraphType.AverageMonth)
                {
                    UpdateGraph(GraphType.AverageMonth);
                }
                if (Settings.Default.chartMode == GraphType.MinimalMonth)
                {
                    UpdateGraph(GraphType.MinimalMonth);
                }
                if (Settings.Default.chartMode == GraphType.MaximalMonth)
                {
                    UpdateGraph(GraphType.MaximalMonth);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        ObservableCollection<TempChangeItem> list = new ObservableCollection<TempChangeItem>();
        private void listDayCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelectedDay = selectedMonth.ListDays[listDayCB.SelectedIndex];
                rightDay.Text = SelectedDay.ListHum.Count.ToString() + " " + StaticHelper.getEnding(SelectedDay.ListHum.Count, "записей", "запись", "записи") + " за " + SelectedDay.DayNumber.ToString() + " " + StaticHelper.GetMonth(true, selectedMonth.MonthNumber).ToLower();
                if (list.Count != 0)
                {
                    list.Clear();
                }
                DateTime nowTime = new DateTime(listYears[listYearsCB.SelectedIndex].Year
                        , selectedMonth.MonthNumber
                        , SelectedDay.DayNumber);
                string name = SelectedDay.DayNumber.ToString() + " " + StaticHelper.GetMonth(true, nowTime.Month).ToLower() + " " + nowTime.Year.ToString();
                TitleCT.Text = "Изменение температуры за " + name;
                Title = "Изменение температуры за " + name;
                bottomLabel.Text = FSP.GetDoW(nowTime.DayOfWeek.ToString());
                bool flag = true;
                for (int i = 0; i < SelectedDay.ListHum.Count; i++)
                {
                    list.Add(new TempChangeItem
                    {
                        Time = SelectedDay.ListTime[i],
                        Temperature = SelectedDay.ListTemp[i] + "°",
                        Humidity = SelectedDay.ListHum[i] + "%"
                    });
                    if (flag)
                    {
                        list[i].BrushItem = new SolidColorBrush(Colors.Transparent);
                    }
                    else
                    {
                        list[i].BrushItem = itemBrush;
                    }
                    flag = !flag;
                }
                setTempHumInfo(averageMode);
                if (Settings.Default.chartMode == GraphType.DayData)
                {
                    UpdateGraph(GraphType.DayData);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {

        }
        int averageMode = 0;
        public void setTempHumInfo(int mode)
        {
            switch (mode)
            {
                case 0:
                    {

                        bottomLabelAverageTemp.Text = "Средняя температура за день " + StaticHelper.GetAverage(listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListTemp) + "°";
                        bottomLabelAverageHum.Text = "Средняя влажность за день " + StaticHelper.GetAverage(listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListHum) + "%";
                    }
                    break;
                case 1:
                    {
                        bottomLabelAverageTemp.Text = "Максимальная температура за день " + listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListTemp.ToArray().Max().ToString() + "°";
                        bottomLabelAverageHum.Text = "Максимальная влажность за день " + listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListHum.ToArray().Max().ToString() + "%";
                    }
                    break;
                case 2:
                    {
                        bottomLabelAverageTemp.Text = "Минимальная температура за день " + listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListTemp.ToArray().Min().ToString() + "°";
                        bottomLabelAverageHum.Text = "Минимальная влажность за день " + listYears[listYearsCB.SelectedIndex].ListMonths[listMonthsCB.SelectedIndex].ListDays[listDayCB.SelectedIndex].ListHum.ToArray().Min().ToString() + "%";
                    }
                    break;
            }
        }

        private void GridBottom_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (averageMode == 2)
            {
                averageMode = 0;
            }
            else
            {
                averageMode++;
            }
            setTempHumInfo(averageMode);
        }

        private void TempLayout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        

        private void Window_Activated(object sender, EventArgs e)
        {
            dropShadowWindow.Opacity = 0.5;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            dropShadowWindow.Opacity = 0.0;
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimateWindow(0);
        }

        private void listDayCB_MouseEnter(object sender, MouseEventArgs e)
        {
            listDayCB.Focus();
        }

        private void listMonthsCB_MouseEnter(object sender, MouseEventArgs e)
        {
            listMonthsCB.Focus();
        }

        private void listYearsCB_MouseEnter(object sender, MouseEventArgs e)
        {
            listYearsCB.Focus();
        }

        private void GraphModeSelector_MouseEnter(object sender, MouseEventArgs e)
        {
            GraphModeSelector.Focus();
        }

        private void GraphModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateGraph(GraphModeSelector.SelectedIndex);
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
    }
}
