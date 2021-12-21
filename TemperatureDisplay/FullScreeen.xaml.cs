using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Speech.Synthesis;
using JWeather.Properties;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для FullScreeen.xaml
    /// </summary>
    public partial class FullScreeen : Window
    {
        HelpClass FSP = new HelpClass();
        MemoryManagement MM = new MemoryManagement();
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
                    SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
        }


        double opac;
        string dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\LogTemperature\";
        int[] arrayTemp;
        int[] arrayHum;
        List<string> listTemp = new List<string>();
        List<string> listHum = new List<string>();
        SpeechSynthesizer speaker = new SpeechSynthesizer();
        public bool isTemp = true;
        DoubleAnimation animClose, animOpen; 
        System.Windows.Forms.Timer timer3;
        System.Windows.Forms.Timer timerTime;
        System.Windows.Media.Brush currentBrush;
        public FullScreeen()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
            this.SetBackgroundColor();
        }
        private void SetBackgroundColor()
        {
            currentBrush = SystemParameters.WindowGlassBrush;
            TopText.Foreground = currentBrush;
            CenterText.Foreground = currentBrush;
            BottomText.Foreground = currentBrush;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                this.SetBackgroundColor();
            }
        }
        public void animateWindow(int mode)
        {
            if (mode == 0)
            {
                animClose = new DoubleAnimation(opac, 0.0, new Duration(TimeSpan.FromMilliseconds(350)));
                animClose.Completed += (s, a) => this.Close();
                this.BeginAnimation(Window.OpacityProperty, animClose);
            }
            if (mode == 1)
            {
                animOpen = new DoubleAnimation(0.0, opac, new Duration(TimeSpan.FromMilliseconds(350)));
                animOpen.Completed += (s, a) =>
                {
                    CheckBoxCenter.IsChecked = true;
                    timer3.Start();
                };
                this.BeginAnimation(Window.OpacityProperty, animOpen);
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int temp = ((MainWindow)this.Tag).temperature;
            int hum = ((MainWindow)this.Tag).humidity;
            switch (((MainWindow)this.Tag).modeFSM)
            {
                case 1:
                    {
                        isTemp = true;
                    } break;
                case 2:
                    {
                        isTemp = false;
                    } break;
            }
            string temperature = temp.ToString();
            if (isTemp)
            {
                opac = Settings.Default.opacityTemp / 100f;
            }
            else
            {
                opac = Settings.Default.opacityTime / 100f;
            }

            animateWindow(1);
            timer3 = new System.Windows.Forms.Timer();
            if (isTemp)
            {
                if (Settings.Default.hideWhenRecalledTemp)
                {
                    timer3.Interval = 1;
                }
                else
                {
                    timer3.Interval = Settings.Default.delayTemp * 1000;
                }
            }
            else
            {
                if (Settings.Default.hideWhenRecalledTime)
                {
                    timer3.Interval = 1;
                }
                else
                {
                    timer3.Interval = Settings.Default.delayTime * 1000;
                }
            }
            timer3.Tick += new EventHandler((sender3, e3) =>
            {
                if (isTemp)
                {
                    if (Settings.Default.hideWhenRecalledTemp)
                    {
                        if (((MainWindow)this.Tag).windowFFS)
                        {
                            timer3.Stop();
                            CheckBoxCenter.IsChecked = false;
                            animateWindow(0);
                        }
                    }
                    else
                    {
                        timer3.Stop();
                        CheckBoxCenter.IsChecked = false;
                        animateWindow(0);
                    }
                }
                else
                {
                    if (Settings.Default.hideWhenRecalledTime)
                    {
                        if (((MainWindow)this.Tag).windowFFS)
                        {
                            timer3.Stop();
                            CheckBoxCenter.IsChecked = false;
                            animateWindow(0);
                        }
                    }
                    else
                    {
                        timer3.Stop();
                        CheckBoxCenter.IsChecked = false;
                        animateWindow(0);
                    }
                }
            });

            switch (isTemp)
            {
                case true:
                    {
                        TopText.Text = "";
                        CenterText.Text = "Температура  " + temp + "° \n Влажность " + hum + "%";
                        CenterText.FontSize = 50;
                        BottomText.FontSize = 20;
                        DateTime localDate = DateTime.Now;
                        string y = localDate.Year.ToString();
                        string d = localDate.Day.ToString();
                        if (localDate.Day < 10)
                        {
                            d = d.Insert(0, "0");
                        }
                        d = d.Insert(d.Length, ".tdi");
                        string mi = localDate.Month.ToString();
                        if (localDate.Month < 10)
                        {
                            mi = mi.Insert(0, "0");
                        }
                        string filename = dir + y + @"\" + mi + @"\" + d;
                        bool order = false;
                        if (File.Exists(filename))
                        {
                            order = true;
                            string line;
                            System.IO.StreamReader reader = new System.IO.StreamReader(filename);
                            listHum.Clear();
                            listTemp.Clear();
                            while ((line = reader.ReadLine()) != null)
                            {
                                string te, hu;
                                te = line.Substring(0, line.LastIndexOf("/"));
                                hu = line;
                                hu = hu.Remove(0, hu.LastIndexOf("/") + 1);
                                hu = hu.Substring(0, hu.LastIndexOf("="));
                                listTemp.Add(te);
                                listHum.Add(hu);
                            }
                            reader.Close();
                            arrayTemp = new int[listTemp.Count];
                            arrayHum = new int[listTemp.Count];
                            for (int i = 0; i < listTemp.Count; i++)
                            {
                                arrayTemp[i] = int.Parse(listTemp[i]);
                                arrayHum[i] = int.Parse(listHum[i]);
                            }
                            BottomText.Text = "Максимум\n[ " + arrayTemp.Max().ToString() + "° / " + arrayHum.Max().ToString() + "% ]"
                            + "\nМинимум\n[ " + arrayTemp.Min().ToString() + "° / " + arrayHum.Min().ToString() + "% ]";
                        }
                        else
                        {
                            BottomText.Text = "";
                        }
                        string voice = "";
                        if (Settings.Default.voiceTemp)
                        {
                            voice = "Температура " + temp + " " + FSP.GetDegree(temp.ToString());
                        }
                        if (Settings.Default.voiceHum)
                        {
                            voice = voice.Insert(voice.Length, "," + "Влажность " + hum + "%");
                        }
                        if (order)
                        {
                            if (Settings.Default.voiceMaxTemp)
                            {
                                voice = voice.Insert(voice.Length, ", Максимальная температура " + arrayTemp.Max().ToString() + " " + FSP.GetDegree(arrayTemp.Max().ToString()));
                            }
                            if (Settings.Default.voiceMaxHum)
                            {
                                voice = voice.Insert(voice.Length, ", Максимальная влажность " + arrayHum.Max().ToString() + "%");
                            }
                            if (Settings.Default.voiceMinTemp)
                            {
                                voice = voice.Insert(voice.Length, ", Минимальная температура " + arrayTemp.Min().ToString() + " " + FSP.GetDegree(arrayTemp.Min().ToString()));
                            }
                            if (Settings.Default.voiceMinHum)
                            {
                                voice = voice.Insert(voice.Length, " , Минимальная влажность " + arrayHum.Min().ToString() + "%");
                            }
                        }
                        if (voice.Length != 0)
                        {
                            speaker.SpeakAsync(voice);
                        }

                    } break;
                case false:
                    {
                        CenterText.FontSize =  80;
                        BottomText.FontSize = 20;
                        timerTime = new System.Windows.Forms.Timer();
                        timerTime.Interval = 1;
                        timerTime.Tick += new EventHandler((sender1, e1) =>
                        {
                            DateTime localDate = DateTime.Now;
                            CenterText.Text = localDate.ToLongTimeString();
                            TopText.Text = localDate.Day.ToString() + " " + StaticHelper.GetMonth(true,localDate.Month).ToLower()
                            + "\n" + FSP.GetDoW(localDate.DayOfWeek.ToString());
                        });
                        BottomText.Text = "Температура  " + temp + "° \n Влажность " + hum + "%";
                        DateTime localDate2 = DateTime.Now;
                        string voice = "";
                        if (Settings.Default.voiceDate)
                        {
                            voice = localDate2.Day.ToString() + " " + StaticHelper.GetMonth(true, localDate2.Month);
                        }
                        if (Settings.Default.voiceDoW)
                        {
                            voice = voice.Insert(voice.Length, " , " + FSP.GetDoW(localDate2.DayOfWeek.ToString()));
                        }
                        if (Settings.Default.voiceTime)
                        {
                            voice = voice.Insert(voice.Length, " , " + FSP.GetHourMin(localDate2.Hour, localDate2.Minute.ToString()));
                        }
                        if (voice.Length != 0)
                        {
                            speaker.SpeakAsync(voice);
                        }
                        
                        timerTime.Start();
                    } break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }
}
