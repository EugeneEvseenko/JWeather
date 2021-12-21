using JWeather.Properties;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для FullScreenWeather.xaml
    /// </summary>
    public partial class FullScreenWeather : Window
    {
        public FullScreenWeather()
        {
            InitializeComponent();
        }
        DoubleAnimation animClose, animOpen;
        System.Windows.Forms.Timer timerDelay;
        public void animateWindow(int mode)
        {
            if (mode == 0)
            {
                animClose = new DoubleAnimation(Settings.Default.opacityWeather, 0.0, new Duration(TimeSpan.FromMilliseconds(350)));
                animClose.Completed += (s, a) => this.Close();
                this.BeginAnimation(Window.OpacityProperty, animClose);
            }
            if (mode == 1)
            {
                animOpen = new DoubleAnimation(0.0, Settings.Default.opacityWeather, new Duration(TimeSpan.FromMilliseconds(350)));
                animOpen.Completed += (s, a) =>
                {
                    CheckBoxCenter.IsChecked = true;
                    timerDelay.Start();
                };
                this.BeginAnimation(Window.OpacityProperty, animOpen);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timerDelay = new System.Windows.Forms.Timer();
            if (Settings.Default.hideWhenRecalledWeather)
            {
                timerDelay.Interval = 1;
                timerDelay.Tick += new EventHandler((sender3, e3) =>
                {
                    if (((MainWindow)this.Tag).windowFFS)
                    {
                        timerDelay.Stop();
                        CheckBoxCenter.IsChecked = false;
                        animateWindow(0);
                    }
                });
            }
            else
            {
                timerDelay.Interval = Settings.Default.delayWeather * 1000;
                timerDelay.Tick += new EventHandler((sender3, e3) =>
                {
                    timerDelay.Stop();
                    CheckBoxCenter.IsChecked = false;
                    animateWindow(0);
                });
            }
            weatherImage.Source = ((MainWindow)this.Tag).WeatherImage.Source;
            CenterText.Text = ((MainWindow)this.Tag).TWeatherBlock.Content.ToString();
            BottomText.Text = ((MainWindow)this.Tag).WeatherBlock.Content.ToString();
            animateWindow(1);

        }
    }
}
