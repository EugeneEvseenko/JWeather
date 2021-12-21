using CoreAudioApi;
using JWeather.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JWeather
{

    



    public partial class Volume : Window
    {
        private MMDevice device;
        MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
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
        int volumeLvl = 0;
        bool thisisexit = false;
        public Volume()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;

            // Call this if you haven't set Background in XAML.
            this.SetBackgroundColor();
            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        }


        protected override void OnClosed(EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= this.SystemParameters_StaticPropertyChanged;
            base.OnClosed(e);
        }

        private void SetBackgroundColor()
        {
            VolumeBar.Foreground = SystemParameters.WindowGlassBrush;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                this.SetBackgroundColor();
            }
        }

        DoubleAnimation animVol;
        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            
                VolumeText.Dispatcher.Invoke(new MethodInvoker(delegate
                {
                    volumeLvl = (int)(data.MasterVolume * 100);
                    animVol = new DoubleAnimation(VolumeBar.Value, data.MasterVolume, new Duration(TimeSpan.FromMilliseconds(250)))
                    {
                        EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                    };
                    VolumeBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animVol);
                    VolumeBar.Value = data.MasterVolume;
                    thisisexit = false;
                    if (volumeLvl == 0)
                    {
                        volumeStateL.Visibility = Visibility.Visible;
                        volumeStateH.Visibility = Visibility.Hidden;
                        volumeStateM.Visibility = Visibility.Hidden;
                    }
                    if (volumeLvl > 0 && volumeLvl < 50)
                    {
                        volumeStateL.Visibility = Visibility.Hidden;
                        volumeStateH.Visibility = Visibility.Hidden;
                        volumeStateM.Visibility = Visibility.Visible;
                    }
                    if (volumeLvl > 50)
                    {
                        volumeStateL.Visibility = Visibility.Hidden;
                        volumeStateH.Visibility = Visibility.Visible;
                        volumeStateM.Visibility = Visibility.Hidden;
                    }
                }));
            
        }
        System.Windows.Threading.DispatcherTimer timerExit;
        double opac;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            opac = Settings.Default.opacityVolume / 100f;
            animClose = new DoubleAnimation(opac, 0.0, new Duration(TimeSpan.FromMilliseconds(350)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };
            animOpen = new DoubleAnimation(0, opac, new Duration(TimeSpan.FromMilliseconds(350)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            animClose.Completed += (sO, aO) =>
            {
                Close();
                MM.FlushMemory();
            };
            animOpen.Completed += (s, a) =>
            {

                timerExit = new System.Windows.Threading.DispatcherTimer();
                timerExit.Tick += new EventHandler(HandleTimerElapsed);
                timerExit.Interval = new TimeSpan(0, 0, 0, 0, Settings.Default.delayVolume);
                timerExit.Start();
            };
            animVolumeClose = new DoubleAnimation(151, 0, new Duration(TimeSpan.FromMilliseconds(250)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            };
            animVolumeOpen = new DoubleAnimation(0, 151, new Duration(TimeSpan.FromMilliseconds(250)))
            {
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            };
            VolumeBar.Value = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            animateWindow(true);
        }
        private void HandleTimerElapsed(object sender, EventArgs e)
        {
            if (thisisexit)
            {
                animateWindow(false);
            }
            thisisexit = true;
        }
        DoubleAnimation animClose, animOpen;
        DoubleAnimation animVolumeOpen,animVolumeClose;
        public void animateWindow(bool mode)
        {
            if (!mode)
            {
                timerExit.Stop();
                VolumeBar.BeginAnimation(WidthProperty, animVolumeClose);
                VolumeBar.BeginAnimation(HeightProperty, animVolumeClose);
                BeginAnimation(Window.OpacityProperty, animClose);
            }
            if (mode)
            {
                
                BeginAnimation(OpacityProperty, animOpen);
                
                VolumeBar.BeginAnimation(WidthProperty, animVolumeOpen);
                VolumeBar.BeginAnimation(HeightProperty, animVolumeOpen);
            }
        }
    }
}
