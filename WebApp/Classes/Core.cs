using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace WebApp.Classes
{
    public class Core
    {
        public static DateTime StringToDateTime(string date)
        {
            string[] format = new string[] {
                "dd/MM/yyyy HH:mm:ss",
                "d/M/yyyy H:m:s",
                "dd/MM/yyyy HH:mm",
                "d/M/yyyy H:m",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "HH:mm:ss",
                "HH:mm",
            };
            foreach (string f in format)
            {
                try
                {
                    return StringToDateTime(date, f);
                }
                catch { }
            }
            throw new Exception(string.Format("ไม่สามารถแปลง {0} เป็น DateTime ได้", date));
        }
        public static DateTime StringToDateTime(string date, string format)
        {
            try
            {
                return DateTime.ParseExact(date, format, new System.Globalization.CultureInfo("th-TH"));
            }
            catch
            {
                throw new Exception(string.Format("ไม่สามารถแปลง {0} เป็น DateTime ได้", date));
            }
        }
        public static string DateTimeToString(DateTime? date, string format)
        {
            if (date != null && date.HasValue)
            {
                return DateTimeToString(date.Value, format);
            }
            return null;
        }
        public static string DateTimeToString(DateTime? date, string format, System.Globalization.CultureInfo cultureInfo)
        {
            if (date != null && date.HasValue)
            {
                return DateTimeToString(date.Value, format, cultureInfo);
            }
            return null;
        }
        public static string DateTimeToString(DateTime? date)
        {
            if (date != null && date.HasValue)
            {
                return DateTimeToString(date.Value);
            }
            return null;
        }
        public static string DateTimeToString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm:ss", new System.Globalization.CultureInfo("th-TH"));
        }
        public static string DateTimeToString(DateTime date, string format)
        {
            return DateTimeToString(date, format, new System.Globalization.CultureInfo("th-TH"));
        }
        public static string DateTimeToString(DateTime date, string format, System.Globalization.CultureInfo cultureInfo)
        {
            return date.ToString(format, cultureInfo);
        }
        public static long DateTimeToUnixTimeStamp(DateTime date, double timezone = -420)
        {
            return long.Parse((date - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(timezone * -1)).TotalMilliseconds.ToString("###0"));
        }
        public static DateTime UnixTimeStampToDateTime(long milliseconds, double timezone = -420)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds).AddMinutes(timezone * -1);
        }
        public static DateTime UnixTimeStampToDateTime(int seconds, double timezone = -420)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).AddMinutes(timezone * -1);
        }

        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt)
        {
            try
            {
                return DataTableToDictionary(dt, new System.Globalization.CultureInfo("th-TH"));
            }
            catch (Exception ex)
            {
                throw new Exception("ไม่สามารถแปลงจาก DataTable เป็น Dictionary ได้ เนื่องจาก " + ex.Message);
            }
        }
        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt, System.Globalization.CultureInfo cultureInfo)
        {
            List<Dictionary<string, object>> dataDict = null;
            try
            {
                dataDict = dt.AsEnumerable().Select(dr => dt.Columns.Cast<DataColumn>().ToDictionary(
                dc => dc.ColumnName,
                dc => dr[dc] is DateTime
                    ? DateTimeToString(dr[dc] as DateTime?, "dd/MM/yyyy HH:mm:ss", cultureInfo)
                    : dr[dc]
                    )).ToList();
                return dataDict;
            }
            catch (Exception ex)
            {
                throw new Exception("ไม่สามารถแปลงจาก DataTable เป็น Dictionary ได้ เนื่องจาก " + ex.Message);
            }
        }

        public static ConnectionStringSettings GetConnectionString(string key = "dbConnection")
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["PREFIX"]))
            {
                return ConfigurationManager.ConnectionStrings[key];
            }
            else
            {
                if (ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["PREFIX"] + key] != null)
                {
                    return ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["PREFIX"] + key];
                }
                else
                {
                    return ConfigurationManager.ConnectionStrings[key];
                }
            }
        }
        public static string WebConfigReadKey(string key)
        {
            string prefix = ConfigurationManager.AppSettings["PREFIX"];
            string value = string.Empty;

            value = ConfigurationManager.AppSettings[prefix + key];
            if (value == null)
            {
                value = ConfigurationManager.AppSettings[key];
            }
            if (!string.IsNullOrEmpty(value))
            {
                foreach (string _key in ConfigurationManager.AppSettings.Keys)
                {
                    if (_key == "PREFIX") continue;
                    string strReplace = string.Empty;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        strReplace = "${" + _key.Replace(prefix, "") + "}";
                    }
                    else
                    {
                        strReplace = "${" + _key + "}";
                    }
                    if (value.IndexOf(strReplace) != -1)
                    {
                        value = value.Replace(strReplace, WebConfigReadKey(_key));
                    }
                }
            }

            return value;
        }

        public static void WriteLogInfo(string message)
        {
            WriteLog("LogDetail", "info", message);
        }
        public static void WriteLogInfo(string format, params object[] args)
        {
            WriteLog("LogDetail", "info", format, null, args);
        }

        public static void WriteLogDebug(string message)
        {
            WriteLog("LogDetail", "debug", message);
        }
        public static void WriteLogDebug(string format, params object[] args)
        {
            WriteLog("LogDetail", "debug", format, null, args);
        }

        public static void WriteLogError(string message)
        {
            WriteLog("LogDetail", "error", message);
        }
        public static void WriteLogError(string format, params object[] args)
        {
            WriteLog("LogDetail", "error", format, null, args);
        }
        public static void WriteLogError(string message, Exception ex)
        {
            WriteLog("LogDetail", "error", message, ex);
        }

        public static void WriteRequestLogInfo(string message)
        {
            WriteLog("LogRequest", "info", message);
        }
        public static void WriteRequestLogInfo(string format, params object[] args)
        {
            WriteLog("LogRequest", "info", format, null, args);
        }

        public static void WriteRequestLogDebug(string message)
        {
            WriteLog("LogRequest", "debug", message);
        }
        public static void WriteRequestLogDebug(string format, params object[] args)
        {
            WriteLog("LogRequest", "debug", format, null, args);
        }

        public static void WriteRequestLogError(string message)
        {
            WriteLog("LogRequest", "error", message);
        }
        public static void WriteRequestLogError(string format, params object[] args)
        {
            WriteLog("LogRequest", "error", format, null, args);
        }
        public static void WriteRequestLogError(string message, Exception ex)
        {
            WriteLog("LogRequest", "error", message, ex);
        }

        private static void WriteLog(string logger, string logType, string message, Exception ex = null)
        {
            //try
            //{
            //    log4net.Config.XmlConfigurator.Configure();
            //    log4net.ILog _log = log4net.LogManager.GetLogger(logger);
            //    if (logType == "info")
            //    {
            //        _log.Info(message, ex);
            //    }
            //    else if (logType == "error")
            //    {
            //        _log.Error(message, ex);
            //    }
            //    else if (logType == "debug")
            //    {
            //        _log.Debug(message, ex);
            //    }
            //}
            //catch { }
        }
        private static void WriteLog(string logger, string logType, string format, Exception ex = null, params object[] args)
        {
            //try
            //{
            //    log4net.Config.XmlConfigurator.Configure();
            //    log4net.ILog _log = log4net.LogManager.GetLogger(logger);
            //    if (logType == "info")
            //    {
            //        _log.InfoFormat(format, args);
            //    }
            //    else if (logType == "error")
            //    {
            //        _log.ErrorFormat(format, args);
            //    }
            //    else if (logType == "debug")
            //    {
            //        _log.DebugFormat(format, args);
            //    }
            //}
            //catch { }
        }

    }
}