using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        Brush currentBrush;
        DoubleAnimation animClose, animOpenOpacity, animConfirmCancelUp, animConfirmCancelDown;
        ThicknessAnimation animSelect,animUnSelect;
        int currentAnimate = -1;
        int currentSelected = 0;
        int lastItem = -1;
        int confirmationMode = 0;
        Color confirmColor = new Color();
        Color cancelColor = new Color();
        public Menu()
        {
            InitializeComponent();
            SystemParameters.StaticPropertyChanged += this.SystemParameters_StaticPropertyChanged;
            this.SetBackgroundColor();
            gridExit.Opacity = 0.0;
            gridRestart.Opacity = 0.0;
            gridSleep.Opacity = 0.0;
            labelCurrentItemSelected.Opacity = 0.0;
            confirmColor.A = 255;
            confirmColor.R = 76;
            confirmColor.G = 175;
            confirmColor.B = 80;
            cancelColor.A = 255;
            cancelColor.R = 244;
            cancelColor.G = 67;
            cancelColor.B = 54;
        }
        private void SetBackgroundColor()
        {
            currentBrush = SystemParameters.WindowGlassBrush;
            Color f = SystemParameters.WindowGlassColor;
            switch (confirmationMode)
            {
                case 0:
                    {
                        itemExit.Fill = currentBrush;
                        itemRestart.Fill = currentBrush;
                        itemSleep.Fill = currentBrush;
                    }break;
                case 1:
                    {
                        itemExit.Fill = currentBrush;
                    }break;
                case 2:
                    {
                        itemRestart.Fill = currentBrush;
                    } break;
                case 3:
                    {
                        itemSleep.Fill = currentBrush;
                    } break;
            }
            
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WindowGlassBrush")
            {
                this.SetBackgroundColor();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentAnimate = 0;
            animConfirmCancelUp = new DoubleAnimation(0.5, 1.0, new Duration(TimeSpan.FromMilliseconds(350)));
            animConfirmCancelDown = new DoubleAnimation(1.0, 0.5, new Duration(TimeSpan.FromMilliseconds(350)));
            animOpenOpacity = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(250)));
            animOpenOpacity.Completed += (sO, aO) =>
            {
                if (currentAnimate == 1)
                {
                    gridSleep.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                    currentAnimate = -1;
                    selectItem(0);
                }
                if (currentAnimate == 0)
                { 
                    gridRestart.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                    currentAnimate++;
                }
            };
            gridExit.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
            
        }
        public void closeWindow()
        {
            currentAnimate = 0;
            animClose = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(150)));
            animClose.Completed += (sO, aO) =>
            {
                if (currentAnimate == 2)
                {
                    labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animClose);
                    Close();
                }
                if (currentAnimate == 1)
                {
                    gridExit.BeginAnimation(Window.OpacityProperty, animClose);
                    currentAnimate++;
                }
                if (currentAnimate == 0)
                {
                    gridRestart.BeginAnimation(Window.OpacityProperty, animClose);
                    currentAnimate++;
                }
            };
            gridSleep.BeginAnimation(Window.OpacityProperty, animClose);
        }
        public void selectItem(int index)
        {
            Thickness currThick = new Thickness();
            switch (confirmationMode)
            {
                case 0:
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    currThick = gridExit.Margin;
                                    currThick.Bottom = 50;
                                } break;
                            case 1:
                                {
                                    currThick = gridRestart.Margin;
                                    currThick.Bottom = 50;
                                } break;
                            case 2:
                                {
                                    currThick = gridSleep.Margin;
                                    currThick.Bottom = 50;
                                } break;
                        }
                    }break;
                case 1:
                    {
                        if (index == 1)
                        {
                            currThick = new Thickness(50, 0, 0, 0);
                        }else
                        {
                            currThick = new Thickness(150, 0, 0, 0);
                        }
                    } break;
                case 2:
                case 3:
                    {
                        if (index == 0)
                        {
                            currThick = new Thickness(50, 0, 0, 0);
                        }
                        else
                        {
                            currThick = new Thickness(150, 0, 0, 0);
                        }
                    } break;
            }
            

            animSelect = new ThicknessAnimation(currThick, new Duration(TimeSpan.FromMilliseconds(150)));
            switch (confirmationMode)
            {
                case 0:
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    gridExit.BeginAnimation(Grid.MarginProperty, animSelect);
                                    labelCurrentItemSelected.Content = "Завершение работы";
                                    labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);

                                } break;
                            case 1:
                                {
                                    gridRestart.BeginAnimation(Grid.MarginProperty, animSelect);
                                    labelCurrentItemSelected.Content = "Перезагрузка";
                                    labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                                } break;
                            case 2:
                                {
                                    gridSleep.BeginAnimation(Grid.MarginProperty, animSelect);
                                    labelCurrentItemSelected.Content = "Спящий режим";
                                    labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                                } break;

                        }
                    }break;
                case 1:
                    {
                        switch (index)
                        {
                            case 1:
                                {
                                    gridRestart.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                    itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                } break;
                            case 2:
                                {
                                    gridSleep.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                    itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                } break;

                        }
                    } break;
                case 2:
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    gridExit.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                    itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                } break;
                            case 2:
                                {
                                    gridSleep.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                    itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                } break;

                        }
                    } break;
                case 3:
                    {
                        switch (index)
                        {
                            case 0:
                                {
                                    gridExit.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                    itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                } break;
                            case 1:
                                {
                                    gridRestart.BeginAnimation(Grid.MarginProperty, animSelect);
                                    itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                                    itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                                } break;

                        }
                    } break;
            }
            currThick = new Thickness();
            switch (confirmationMode)
            {
                case 0:
                    {
                        switch (lastItem)
                        {
                            case 0:
                                {
                                    currThick = gridExit.Margin;
                                    currThick.Bottom = 0;
                                } break;
                            case 1:
                                {
                                    currThick = gridRestart.Margin;
                                    currThick.Bottom = 0;
                                } break;
                            case 2:
                                {
                                    currThick = gridSleep.Margin;
                                    currThick.Bottom = 0;
                                } break;
                        }
                    }break;
                case 1:
                    {
                        if (lastItem == 1)
                        {
                            currThick = new Thickness(50, 50, 0, 0);
                        }else
                        {
                            currThick = new Thickness(150, 50, 0, 0);
                        }
                    } break;
                case 2:
                case 3:
                    {
                        if (lastItem == 0)
                        {
                            currThick = new Thickness(50, 50, 0, 0);
                        }
                        else
                        {
                            currThick = new Thickness(150, 50, 0, 0);
                        }
                    } break;
            }
            
            animUnSelect = new ThicknessAnimation(currThick, new Duration(TimeSpan.FromMilliseconds(150)));
            switch (lastItem)
            {
                case 0:
                    {
                        gridExit.BeginAnimation(Grid.MarginProperty, animUnSelect);
                    } break;
                case 1:
                    {
                        gridRestart.BeginAnimation(Grid.MarginProperty, animUnSelect);
                    } break;
                case 2:
                    {
                        gridSleep.BeginAnimation(Grid.MarginProperty, animUnSelect);
                    } break;

            }
            lastItem = index;
        }

        public void nextItem()
        {
            switch (confirmationMode)
            {
                case 0:
                    {
                        if (currentSelected == 2)
                        {
                            currentSelected = 0;
                        }
                        else
                        {
                            currentSelected++;
                        }
                    }break;
                case 1:
                    {
                        if (currentSelected == 2)
                        {
                            currentSelected = 1;
                        }
                        else
                        {
                            currentSelected = 2;
                        }
                        
                    }break;
                case 2:
                    {
                        if (currentSelected == 2)
                        {
                            currentSelected = 0;
                        }
                        else
                        {
                            currentSelected = 2;
                        }

                    } break;
                case 3:
                    {
                        if (currentSelected == 1)
                        {
                            currentSelected = 0;
                        }
                        else
                        {
                            currentSelected = 1;
                        }

                    } break;
            }
            selectItem(currentSelected);
        }

        public void previousItem()
        {
            switch (confirmationMode)
            {
                case 0:
                    {
                        if (currentSelected == 0)
                        {
                            currentSelected = 2;
                        }
                        else
                        {
                            currentSelected--;
                        }
                    }break;
                case 1:
                    {
                        if (currentSelected == 1)
                        {
                            currentSelected = 2;
                        }
                        else
                        {
                            currentSelected = 1;
                        }
                    } break;
                case 2:
                    {
                        if (currentSelected == 0)
                        {
                            currentSelected = 2;
                        }
                        else
                        {
                            currentSelected = 0;
                        }
                    } break;
                case 3:
                    {
                        if (currentSelected == 0)
                        {
                            currentSelected = 1;
                        }
                        else
                        {
                            currentSelected = 0;
                        }
                    } break;
            }
            selectItem(currentSelected);
        }
        public void enterItem()
        {
            if (confirmationMode == 0)
            {
                ThicknessAnimation mainAnim = new ThicknessAnimation(new Thickness(100, 0, 0, 200), new Duration(TimeSpan.FromMilliseconds(250)));
                ThicknessAnimation firstAnim = new ThicknessAnimation(new Thickness(50, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(150)));
                ThicknessAnimation secondAnim = new ThicknessAnimation(new Thickness(150, 0, 0, 0), new Duration(TimeSpan.FromMilliseconds(350)));
                switch (currentSelected)
                {
                    case 0:
                        {
                            labelCurrentItemSelected.Content = "Выключить компьютер?";
                            labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                            gridExit.BeginAnimation(Grid.MarginProperty, mainAnim);
                            gridRestart.BeginAnimation(Grid.MarginProperty, firstAnim);
                            gridSleep.BeginAnimation(Grid.MarginProperty, secondAnim);
                            imageRestart.Source = new BitmapImage(new Uri("Images/menu_confirm.png", UriKind.Relative));
                            imageSleep.Source = new BitmapImage(new Uri("Images/menu_cancel.png", UriKind.Relative));
                            itemRestart.Fill = new SolidColorBrush(confirmColor);
                            itemSleep.Fill = new SolidColorBrush(cancelColor);
                            itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                            itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                            confirmationMode = 1;
                            currentSelected = 2;
                            lastItem = 1;
                            selectItem(2);
                        } break;
                    case 1:
                        {
                            labelCurrentItemSelected.Content = "Перезагрузить компьютер?";
                            labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                            gridRestart.BeginAnimation(Grid.MarginProperty, mainAnim);
                            gridExit.BeginAnimation(Grid.MarginProperty, firstAnim);
                            gridSleep.BeginAnimation(Grid.MarginProperty, secondAnim);
                            imageExit.Source = new BitmapImage(new Uri("Images/menu_confirm.png", UriKind.Relative));
                            imageSleep.Source = new BitmapImage(new Uri("Images/menu_cancel.png", UriKind.Relative));
                            itemExit.Fill = new SolidColorBrush(confirmColor);
                            itemSleep.Fill = new SolidColorBrush(cancelColor);
                            itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                            itemSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                            confirmationMode = 2;
                            currentSelected = 2;
                            lastItem = 0;
                            selectItem(2);
                        } break;
                    case 2:
                        {
                            labelCurrentItemSelected.Content = "Отправиться в спящий режим?";
                            labelCurrentItemSelected.BeginAnimation(Window.OpacityProperty, animOpenOpacity);
                            gridSleep.BeginAnimation(Grid.MarginProperty, mainAnim);
                            gridExit.BeginAnimation(Grid.MarginProperty, firstAnim);
                            gridRestart.BeginAnimation(Grid.MarginProperty, secondAnim);
                            imageExit.Source = new BitmapImage(new Uri("Images/menu_confirm.png", UriKind.Relative));
                            imageRestart.Source = new BitmapImage(new Uri("Images/menu_cancel.png", UriKind.Relative));
                            itemExit.Fill = new SolidColorBrush(confirmColor);
                            itemRestart.Fill = new SolidColorBrush(cancelColor);
                            itemExit.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                            itemRestart.BeginAnimation(Window.OpacityProperty, animConfirmCancelDown);
                            confirmationMode = 3;
                            currentSelected = 1;
                            lastItem = 0;
                            selectItem(1);
                        } break;
                }
            }
            else
            {
                animClose = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(350)));
                ThicknessAnimation cancelAnim = new ThicknessAnimation(new Thickness(100, 10, 0, 0), new Duration(TimeSpan.FromMilliseconds(350)));
                switch (confirmationMode)
                {
                    case 1:
                        {
                            if (currentSelected == 2)
                            {
                                closeWindow();
                            }
                            if (currentSelected == 1)
                            {
                                confirmationMode = 4;
                                gridRestart.BeginAnimation(Window.OpacityProperty, animClose);
                                gridSleep.BeginAnimation(Grid.MarginProperty, cancelAnim);
                                gridSleep.BeginAnimation(Window.OpacityProperty, animConfirmCancelUp);
                            }
                        }break;
                    case 2:
                        {
                            if (currentSelected == 2)
                            {
                                closeWindow();
                            }
                        }break;
                    case 3:
                        {
                            if (currentSelected == 1)
                            {
                                closeWindow();
                            }
                        }break;
                }
            }
        }
    }
}
