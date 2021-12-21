using CoreAudioApi;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace JWeather
{
    public static class GraphType
    {
        public const int DayData = 0;
        public const int AverageMonth = 1;
        public const int MinimalMonth = 2;
        public const int MaximalMonth = 3;
        public const int AverageYear = 4;
    }
    public static class NotificationType
    {
        public const int FanOff = 0;
        public const int FanOn = 1;
        public const int Error = 2;
        public const int HighTemp = 3;
        public const int LowTemp = 4;
        public const int Dialog = 5;
    }
    public static class RequestType
    {
        public const string setFanState = "setFanState";
        public const string getFanState = "getFanState";
        public const string Authorization = "Authorization";
        public const string getVolume = "getVolume";
        public const string setVolume = "setVolume";
    }
    public static class DialogType
    {
        public const int None = 0;
        public const int ConfirmCancel = 1;
        public const int Custom = 2;
    }
    public class AuthItem
    {
        public long DeviceId { get; set; }
        public string Name { get; set; }
        public int AccesLevel { get; set; }
    }
    public class AuthItems
    {
        public List<AuthItem> authItems { get; set; }
    }
    public class NotificationItem
    {
        public bool IsShowed { get; set; }
        public double Position { get; set; }
        public Notification Window { get; set; }
    }
    public class YearItem
    {
        public List<MonthItem> ListMonths { get; set; }
        public int CountMonths { get; set; }
        public int Year { get; set; }
    }
    public class MonthItem
    {
        public List<DayItem> ListDays { get; set; }
        //public int CountDays { get; set; } = 0;
        public int MonthNumber{ get; set; }
    }
    public class DayItem
    {
        public List<int> ListTemp { get; set; }
        public List<int> ListHum { get; set; }
        public List<string> ListTime { get; set; }
        public List<int> WTemp { get; set; }
        public List<int> WHum { get; set; }
        public List<int> WPressure { get; set; }
        public List<int> WWindDeg { get; set; }
        public List<int> WWindSpeed { get; set; }
        public List<int> WSunset { get; set; }
        public List<int> WSunrise { get; set; }
        public List<int> WVisibility { get; set; }
        public List<string> WDescription { get; set; }
        public List<string> WImage { get; set; }
        public int DayNumber { get; set; }
        //public string PathFile { get; set; }
    }
    public class TempChangeItem
    {
        public string Time { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public SolidColorBrush BrushItem { get; set; }
    }
    public class RequestObject
    {
        public string option { get; set; }
        public long value { get; set; }
    }
    public class ResponseObject
    {
        public string response { get; set; }
        public long value { get; set; }
    }
    public class JRequest
    {
        public string Text { get; set; }
    }
    
    public class JResponse
    {
        public string Text { get; set; }
    }
    public static class StaticHelper
    {
        public static bool ValidateVersionSystem()
        {
            int majorVer = Environment.OSVersion.Version.Major;
            int minorVer = Environment.OSVersion.Version.Minor;
            int buildVer = Environment.OSVersion.Version.Build;
            if (majorVer >= 6 && buildVer >= 7600)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static double GetAverage(List<int> list)
        {
            int average = 0;
            for (int i = 0; i < list.Count; i++)
            {
                average += list[i];
            }
            average = average / list.Count;
            return average;
        }
        public static string GetMonth(bool mode, int month)
        {
            if (mode)
            {
                switch (month)
                {
                    case 1:
                        {
                            return "Января";
                        }
                    case 2:
                        {
                            return "Февраля";
                        }
                    case 3:
                        {
                            return "Марта";
                        }
                    case 4:
                        {
                            return "Апреля";
                        }
                    case 5:
                        {
                            return "Мая";
                        }
                    case 6:
                        {
                            return "Июня";
                        }
                    case 7:
                        {
                            return "Июля";
                        }
                    case 8:
                        {
                            return "Августа";
                        }
                    case 9:
                        {
                            return "Сентября";
                        }
                    case 10:
                        {
                            return "Октября";
                        }
                    case 11:
                        {
                            return "Ноября";
                        }
                    case 12:
                        {
                            return "Декабря";
                        }
                    default:
                        {
                            return "ЖОПА";
                        }
                }
            }
            else
            {
                switch (month)
                {
                    case 1:
                        {
                            return "Январь";
                        }
                    case 2:
                        {
                            return "Февраль";
                        }
                    case 3:
                        {
                            return "Март";
                        }
                    case 4:
                        {
                            return "Апрель";
                        }
                    case 5:
                        {
                            return "Май";
                        }
                    case 6:
                        {
                            return "Июнь";
                        }
                    case 7:
                        {
                            return "Июль";
                        }
                    case 8:
                        {
                            return "Август";
                        }
                    case 9:
                        {
                            return "Сентябрь";
                        }
                    case 10:
                        {
                            return "Октябрь";
                        }
                    case 11:
                        {
                            return "Ноябрь";
                        }
                    case 12:
                        {
                            return "Декабрь";
                        }
                    default:
                        {
                            return "ЖОПА";
                        }
                }
            }
        }
        public static string getEnding(int number, string first, string second, string third)
        {
            string outText = "";
            string x = number.ToString();
            if (number > 9)
            {
                if (number >= 11 && number <= 14)
                {
                    x = number.ToString();
                }
                else
                {
                    if (number > 110)
                    {
                        int upThird = int.Parse(x.Substring(x.Length - 2));
                        if (upThird >= 11 && upThird <= 14)
                        {
                            x = upThird.ToString();
                        }
                        else
                        {
                            x = x.Substring(x.Length - 1);
                        }
                    }
                    else
                    {
                        x = x.Substring(x.Length - 1);
                    }
                }
            }
            int count = int.Parse(x);
            switch (count)
            {
                case 0:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    {
                        outText += " " + first;
                    }
                    break;
                case 1:
                    {
                        outText += " " + second;
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    {
                        outText += " " + third;
                    }
                    break;
                default:
                    {
                        outText += " ";
                    }
                    break;
            }

            return outText;
        }
    }
    class HelpClass
    {
        public string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public LoggingConfiguration GetLoggingConfiguration()
        {
            var config = new LoggingConfiguration();

            var target =
                new FileTarget
                {
                    FileName = "${basedir}/logs/${shortdate}.log"
                };

            config.AddTarget("logfile", target);

            var dbTarget = new DatabaseTarget
            {
                ConnectionString = @"<server>;Initial Catalog=<database>;Persist Security Info=True;User ID=<user>;Password=<password>",

                CommandText =
@"INSERT INTO [Log] (Date, Thread, Level, Logger, Message, Exception) 
    VALUES(GETDATE(), @thread, @level, @logger, @message, @exception)"
            };

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@thread", new NLog.Layouts.SimpleLayout("${threadid}")));

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@level", new NLog.Layouts.SimpleLayout("${level}")));

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@logger", new NLog.Layouts.SimpleLayout("${logger}")));

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@message", new NLog.Layouts.SimpleLayout("${message}")));

            dbTarget.Parameters.Add(new DatabaseParameterInfo("@exception", new NLog.Layouts.SimpleLayout("${exception}")));

            config.AddTarget("database", dbTarget);

            var rule = new LoggingRule("*", LogLevel.Debug, target);

            config.LoggingRules.Add(rule);

            var dbRule = new LoggingRule("*", LogLevel.Debug, dbTarget);

            config.LoggingRules.Add(dbRule);
            return config;
        }
        public int GetConnectionAviable()
        {
            IPStatus status = IPStatus.Unknown;
            try
            {
                status = new Ping().Send("google.com",2000).Status;
            }
            catch { }

            if (status == IPStatus.Success)
            {
                return 1;
            }
            else if(status == IPStatus.TimedOut)
            {
                return 2;
            }
            else 
            {
                return 0;
            }

        }
        public double[] GetLocation()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
            {
                MovementThreshold = 1.0
            };
            watcher.TryStart(false, TimeSpan.FromMinutes(1.0));
            Thread.Sleep(100);
            double[] vs = new double[2];
            if (watcher.Position.Location.IsUnknown == false)
            {
                GeoCoordinate coor = watcher.Position.Location;
                vs[0] = coor.Latitude;
                vs[1] = coor.Longitude;
            }
            else
            {
                vs[0] = 0;
                vs[1] = 0;
            }
            return vs;
        }
        public string GetKmH(double speed)
        {
            double kmh = speed * 3.6;
            return Convert.ToInt32(kmh).ToString();
        }
        public string GetMMRS(double pressure)
        {
            double mm = pressure * 0.750062;
            return Convert.ToInt32(mm).ToString();
        }
        public string GetLenghtDay(TimeSpan lenghtDay)
        {
            int hours = lenghtDay.Hours;
            int minutes = lenghtDay.Minutes % 60;
            return hours.ToString() + " ч " + minutes + " мин";
        }
        public string GetWind(double deg)
        {
            string outText = "none";
            if ((deg >= 0 && deg <= 20) || (deg >= 335 && deg <= 360))
            {
                outText = "С";
            }
            else if (deg > 20 && deg <= 65)
            {
                outText = "СВ";
            }
            else if (deg > 65 && deg <= 110)
            {
                outText = "В";
            }
            else if (deg > 110 && deg <= 155)
            {
                outText = "ЮВ";
            }
            else if (deg > 155 && deg <= 200)
            {
                outText = "Ю";
            }
            else if (deg > 200 && deg <= 245)
            {
                outText = "ЮЗ";
            }
            else if (deg > 245 && deg <= 290)
            {
                outText = "З";
            }
            else if (deg > 290 && deg <= 335)
            {
                outText = "СЗ";
            }
            return outText;
        }
        public string GetFullWind(double deg)
        {
            string outText = "none";
            if ((deg >= 0 && deg <= 20) || (deg >= 335 && deg <= 360))
            {
                outText = "Северный ветер";
            }
            else if (deg > 20 && deg <= 65)
            {
                outText = "Северо-Восточный ветер";
            }
            else if (deg > 65 && deg <= 110)
            {
                outText = "Восточный ветер";
            }
            else if (deg > 110 && deg <= 155)
            {
                outText = "Юго-Восточный ветер";
            }
            else if (deg > 155 && deg <= 200)
            {
                outText = "Южный ветер";
            }
            else if (deg > 200 && deg <= 245)
            {
                outText = " Юго-Западный ветер";
            }
            else if (deg > 245 && deg <= 290)
            {
                outText = "Западный ветер";
            }
            else if (deg > 290 && deg <= 335)
            {
                outText = "Северо-Западный ветер";
            }
            return outText;
        }
        public double GetTotalNumber(int temp, int hum)
        {
            double returnTemp = (temp % 100) * 50;
            //returnTemp = returnTemp;
            return returnTemp;
        }
        public string GetSecondString(int sec)
        {
            string ssec = sec.ToString();
            switch (int.Parse(ssec[ssec.Length - 1].ToString()))
            {
                case 1:
                    {
                        return sec.ToString() + " секунда";
                    } 
                case 2:
                case 3:
                case 4:
                    {
                        return sec.ToString() + " секунды";
                    } 
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 0:
                    {
                        return sec.ToString() + " секунд";
                    } 
                default:
                    {
                        return "";
                    } 
            }
        }
        public string ConvertOptionUpdateWeather(int number)
        {
            switch (number)
            {
                case 1:
                    {
                        return "10 минут";
                    }
                case 2:
                    {
                        return "15 минут";
                    } 
                case 3:
                    {
                        return "20 минут";
                    } 
                case 4:
                    {
                        return "30 минут";
                    } 
                case 5:
                    {
                        return "60 минут";
                    } 
                default:
                    {
                        return "[ Ошибка convertOptionUpdateWeather ]";
                    }
            }
        }
        public string GetMinutesString(double sec)
        {
            string ssec = sec.ToString();
            switch (int.Parse(ssec[ssec.Length - 1].ToString()))
            {
                case 1:
                    {
                        return sec.ToString() + " минута";
                    } 
                case 2:
                case 3:
                case 4:
                    {
                        return sec.ToString() + " минуты";
                    } 
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 0:
                    {
                        return sec.ToString() + " минут";
                    } 
                default:
                    {
                        return "";
                    } 
            }
        }
        public string GetDegree(string degree)
        {
            switch (int.Parse(degree[degree.Length - 1].ToString()))
            {
                case 1:
                    {
                        return " градус";
                    } 
                case 2:
                case 3:
                case 4:
                    {
                        return " градуса";
                    } 
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 0:
                    {
                        return " градусов";
                    } 
                default:
                    {
                        return "";
                    } 
            }
        }
        public string GetDoW(string day)
        {
            switch (day)
            {
                case "Monday":
                    {
                        return "Понедельник";
                    } 
                case "Tuesday":
                    {
                        return "Вторник";
                    } 
                case "Wednesday":
                    {
                        return "Среда";
                    } 
                case "Thursday":
                    {
                        return "Четверг";
                    } 
                case "Friday":
                    {
                        return "Пятница";
                    } 
                case "Saturday":
                    {
                        return "Суббота";
                    } 
                case "Sunday":
                    {
                        return "Воскресенье";
                    } 
                default:
                    {
                        return "ЖОПЕНЬКА";
                    }

            }
        }
        public string GetHourMin(int Hour, string Min)
        {
            string text = "";
            switch (Hour)
            {
                case 1:
                    {
                        text = "час ночи ";
                    } break;
                case 2:
                case 3:
                    {
                        text = Hour.ToString() + " часа ночи ";
                    } break;
                case 0:
                    {
                        text = "двенадцать часов ночи ";
                    } break;
                case 4:
                    {
                        text = "четыре часа утра ";
                    } break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    {
                        text = Hour.ToString() + " часов утра ";
                    } break;
                case 12:
                    {
                        text = Hour.ToString() + " часов дня ";
                    } break;
                case 13:
                    {
                        text = "час дня ";
                    } break;
                case 14:
                    {
                        text = "2 часа дня ";
                    } break;
                case 15:
                    {
                        text = "3 часа дня ";
                    } break;
                case 16:
                    {
                        text = "4 часа дня ";
                    } break;
                case 17:
                    {
                        text = "5 часов вечера ";
                    } break;
                case 18:
                    {
                        text = "6 часов вечера ";
                    } break;
                case 19:
                    {
                        text = "7 часов вечера ";
                    } break;
                case 20:
                    {
                        text = "8 часов вечера ";
                    } break;
                case 21:
                    {
                        text = "9 часов вечера ";
                    } break;
                case 22:
                    {
                        text = "10 часов вечера ";
                    } break;
                case 23:
                    {
                        text = "11 часов вечера ";
                    } break;
            }
            int num = 0;
            if (Min.Length != 1)
            {
                num = int.Parse(Min[0].ToString()) * 10;
            }
            switch (int.Parse(Min[Min.Length - 1].ToString()))
            {
                case 1:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " одиннадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " одна минута");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " одна минута");
                        }

                    } break;
                case 2:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " двенадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " две минуты");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " две  минуты");
                        }
                    } break;
                case 3:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " тринадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " три минуты");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " три  минуты");
                        }
                    } break;
                case 4:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " четырнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " четыре минуты");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " четыре  минуты");
                        }
                    } break;
                case 5:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " пятнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " пять минут");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " пять минут");
                        }
                    } break;
                case 6:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " шестнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " шесть минут");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " шесть минут");
                        }
                    } break;
                case 7:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " семнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " семь минут");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " семь минут");
                        }
                    } break;
                case 8:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " восемнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " восемь минут");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " восемь минут");
                        }
                    } break;
                case 9:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " девятнадцать минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " девять минут");
                            }
                        }
                        else
                        {
                            text = text.Insert(text.Length, " девять минут");
                        }
                    } break;
                case 0:
                    {
                        if (Min.Length != 1)
                        {
                            if (Min[0].ToString() == "1")
                            {
                                text = text.Insert(text.Length, " десять минут");
                            }
                            else
                            {
                                text = text.Insert(text.Length, num.ToString() + " минут");
                            }
                        }
                    } break;
            }
            return text;
        }
        
    }
}
