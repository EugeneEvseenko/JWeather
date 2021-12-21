using JWeather.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Логика взаимодействия для Auth.xaml
    /// </summary>
    public partial class Auth : Window
    {
        long DeviceId = 0;
        HelpClass helper = new HelpClass();
        Logger logger;
        DoubleAnimation opacityAnimationOpen,opacityAnimationClose,positionOpen,positionClose;
        public Auth(long deviceId)
        {
            InitializeComponent();
            Opacity = 0;
            LogManager.Configuration = helper.GetLoggingConfiguration();
            logger = LogManager.GetCurrentClassLogger();
            try
            {
                SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
                this.SetBackgroundColor();
                DeviceId = deviceId;
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            ((MainWindow)this.Tag).MM.FlushMemory();
            base.OnClosed(e);
        }

        private void SetBackgroundColor()
        {
            AuthLayout.Background = SystemParameters.WindowGlassBrush;
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                SetBackgroundColor();
            }
        }
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
                DialogResult = Result;
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
            BeginAnimation(TopProperty, positionOpen);
            BeginAnimation(OpacityProperty, opacityAnimationOpen);
            accessLevelComboBox.SelectedIndex = 0;
            descriptionText.Text = String.Format("Устройство '{0}' запрашивает доступ к управлению ПК, разрешить?",DeviceId);
        }
        public void SelectAccess()
        {
            if (accessLevelComboBox.SelectedIndex == 0)
            {
                ToolTipBlock.Text = "Гостевые привелегии включают в себя только отображение данных ПК";
                passwordBox.Visibility = Visibility.Collapsed;
                OkButton.IsEnabled = true;
            }
            else if(accessLevelComboBox.SelectedIndex == 1)
            {
                ToolTipBlock.Text = "Привелегии администратора дают полный контроль над ПК, для авторизации устройства нужен ПИН-код администратора.";
                passwordBox.Visibility = Visibility.Visible;
                passwordBox.Password = string.Empty;
                OkButton.IsEnabled = false;
            }
        }
        private void AccessLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectAccess();
        }

        private void nameTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTipBlock.Text = "Имя устройства";
        }

        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTipBlock.Text = "Запомнить устройство и автоматически давать установленные привелегии для него";
        }

        private void accessLevelComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (accessLevelComboBox.SelectedIndex == 0)
            {
                ToolTipBlock.Text = "Гостевые привелегии включают в себя только отображение данных ПК";
            }
            else if (accessLevelComboBox.SelectedIndex == 1)
            {
                ToolTipBlock.Text = "Привелегии администратора дают полный контроль над ПК, для авторизации устройства нужен ПИН-код администратора.";
            }
        }

        private void passwordBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTipBlock.Text = "Пин-код администратора";
        }

        private void all_MouseLeave(object sender, MouseEventArgs e)
        {
            ToolTipBlock.Text = string.Empty;
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password.Equals(Settings.Default.pin.ToString()))
            {
                OkButton.IsEnabled = true;
            }
            else
            {
                OkButton.IsEnabled = false;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            nameTextBox.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation(TopProperty, positionOpen);
            BeginAnimation(OpacityProperty, opacityAnimationOpen);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation(TopProperty, positionClose);
            BeginAnimation(OpacityProperty, opacityAnimationClose);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            nameTextBox.Visibility = Visibility.Collapsed;
        }
        public AuthItem authItem;
        bool Result = false;
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            BeginAnimation(TopProperty, positionClose);
            BeginAnimation(OpacityProperty, opacityAnimationClose);
            authItem = new AuthItem
            {
                Name = nameTextBox.Text,
                AccesLevel = accessLevelComboBox.SelectedIndex + 1,
                DeviceId = DeviceId
            };
            Result = true;
        }
    }
}
