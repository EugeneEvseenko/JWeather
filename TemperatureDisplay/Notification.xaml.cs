using NLog;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JWeather
{
    /// <summary>
    /// Логика взаимодействия для Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        public Notification(int index, int notification, double top, string title = "JWeather",
            string body = "Description",int dialog = 0, string confirmText = "OK", string cancelText = "Cancel")
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
            this.SetBackgroundColor();
            LogManager.Configuration = helper.GetLoggingConfiguration();
            logger = LogManager.GetCurrentClassLogger();
            try
            {
                TitleString = title;
                DescriptionString = body;
                notificationNumber = notification;
                CurIndex = index;
                dialogT = dialog;
                if (notificationNumber == NotificationType.FanOff || notificationNumber == NotificationType.FanOn)
                {
                    if (notificationNumber == NotificationType.FanOn)
                    {
                        FanState = true;
                    }
                    else
                    {
                        FanState = false;
                    }
                    NotifyImage.Source = BitmapFrame.Create(new Uri(UriFan));
                    startRotateAnimation.Completed += (s, a) =>
                    {
                        rotateNotify.BeginAnimation(RotateTransform.AngleProperty, startRotateAnimation);
                    };

                }
                if (notificationNumber == NotificationType.Error)
                {
                    NotifyImage.Source = BitmapFrame.Create(new Uri(UriInfo));
                }
                if (notificationNumber == NotificationType.LowTemp)
                {
                    NotifyImage.Source = BitmapFrame.Create(new Uri(UriLow));
                }
                if (notificationNumber == NotificationType.HighTemp)
                {
                    NotifyImage.Source = BitmapFrame.Create(new Uri(UriLow));
                    rotateNotify.Angle = 180;
                }
                if (notificationNumber == NotificationType.Dialog)
                {
                    DescriptionBlock.Height = 47;
                    NotifyImage.Source = BitmapFrame.Create(new Uri(UriQuestion));
                    if (dialog == DialogType.ConfirmCancel)
                    {
                        YesNoButtons.Visibility = Visibility.Visible;
                    }
                }
                Top = top;
                double from = (primaryMonitorArea.Right - Width) + 400;
                double to = primaryMonitorArea.Right - Width;
                openAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(400)))
                {
                    EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                };
                closeAnimation = new DoubleAnimation(to, from, new Duration(TimeSpan.FromMilliseconds(400)))
                {
                    EasingFunction = new CircleEase()
                };
                closeAnimation.Completed += CloseAnimation_Completed;
                openAnimation.Completed += (s, a) =>
                {
                    if (dialog == DialogType.None)
                    {
                        CloseTimer = new System.Windows.Forms.Timer();
                        if (notificationNumber == 0 || notificationNumber == 1)
                        {
                            CloseTimer.Interval = 2000;
                        }
                        else
                        {
                            CloseTimer.Interval = 5000;
                        }
                        CloseTimer.Tick += CloseTimer_Elapsed;
                        CloseTimer.Start();
                    }
                    else
                    {
                        if (dialog == DialogType.ConfirmCancel)
                        {

                        }
                    }
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void CloseAnimation_Completed(object sender, EventArgs e)
        {
            if (dialogT != DialogType.None)
            {
                DialogResult = result;
            }
            else
            {
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ((MainWindow)this.Tag).notificationsList[CurIndex].IsShowed = false;
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            ((MainWindow)this.Tag).MM.FlushMemory();
            base.OnClosed(e);
        }

        private void SetBackgroundColor()
        {
            backgroundEllipse.Fill = SystemParameters.WindowGlassBrush;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                SetBackgroundColor();
            }
        }
        int CurIndex;
        Rect primaryMonitorArea = SystemParameters.WorkArea;
        Logger logger;
        HelpClass helper = new HelpClass();
        public int notificationNumber = 0;
        bool FanState;
        string TitleString, DescriptionString;
        DoubleAnimation openAnimation, closeAnimation;
        int dialogT;
        private const string UriFan = @"pack://application:,,,/Images/fan-notify.png";
        private const string UriInfo = @"pack://application:,,,/Images/info.png";
        private const string UriHigh = @"pack://application:,,,/Images/notification_high.png";
        private const string UriLow = @"pack://application:,,,/Images/notification_low.png";
        private const string UriQuestion = @"pack://application:,,,/Images/question.png";
        DoubleAnimation startRotateAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromMilliseconds(100)))
        {
            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
        };
        DoubleAnimation defaultAnimationImage = new DoubleAnimation(0, 90, new Duration(TimeSpan.FromMilliseconds(700)))
        {
            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
        };
        DoubleAnimation stopRotateAnimation = new DoubleAnimation(360, 0, new Duration(TimeSpan.FromMilliseconds(10000)))
        {
            EasingFunction = new CircleEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };
        DoubleAnimation LeaveAnimation = new DoubleAnimation(0, 20, new Duration(TimeSpan.FromMilliseconds(500)))
        {
            EasingFunction = new CircleEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };
        DoubleAnimation EnterAnimation = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromMilliseconds(500)))
        {
            EasingFunction = new CircleEase
            {
                EasingMode = EasingMode.EaseOut
            }
        };
        private void CloseButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButton.Opacity = 0.7;
        }

        private void CloseButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CloseButton.Opacity = 0.6;
        }

        private void CloseButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BeginAnimation(LeftProperty, closeAnimation);
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (dialogT == DialogType.None)
            {
                CloseTimer.Stop();
            }
            Opacity = 1;
            CloseButton.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (dialogT == DialogType.None)
            {
                CloseTimer.Start();
            }
            Opacity = 0.95;
            CloseButton.Visibility = Visibility.Hidden;
        }

        System.Windows.Forms.Timer CloseTimer;

        private void gridConfirm_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            itemConfirm.BeginAnimation(Shape.StrokeThicknessProperty, EnterAnimation);
        }

        private void gridConfirm_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            itemConfirm.BeginAnimation(Shape.StrokeThicknessProperty, LeaveAnimation);
        }

        private void gridCancel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            itemCancel.BeginAnimation(Shape.StrokeThicknessProperty, EnterAnimation);
        }

        private void gridCancel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            itemCancel.BeginAnimation(Shape.StrokeThicknessProperty, LeaveAnimation);
        }
        bool result = false;
        private void gridConfirm_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            result = true;
            BeginAnimation(LeftProperty, closeAnimation);
        }

        private void gridCancel_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BeginAnimation(LeftProperty, closeAnimation);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BeginAnimation(LeftProperty, openAnimation);
                NotifyImage.BeginAnimation(WidthProperty, defaultAnimationImage);
                //NotifyImage.BeginAnimation(HeightProperty, defaultAnimationImage);
                if (notificationNumber == 0 || notificationNumber == 1)
                {
                    if (FanState)
                    {
                        TitleBlock.Text = "Вентилятор";
                        DescriptionBlock.Text = "Включен";
                        rotateNotify.BeginAnimation(RotateTransform.AngleProperty, startRotateAnimation);
                    }
                    else
                    {
                        TitleBlock.Text = "Вентилятор";
                        DescriptionBlock.Text = "Выключен";
                        rotateNotify.BeginAnimation(RotateTransform.AngleProperty, stopRotateAnimation);
                    }
                }
                else
                {
                    TitleBlock.Text = TitleString;
                    DescriptionBlock.Text = DescriptionString;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void CloseTimer_Elapsed(object sender, EventArgs e)
        {
            BeginAnimation(LeftProperty, closeAnimation);
        }
    }
}
