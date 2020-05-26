using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for Model
/// </summary>
namespace WebApp.Classes.Connector
{
    public class ConnectorUtil
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
        public static string DateTimeToString(DateTime? date)
        {
            if (date != null && date.HasValue)
            {
                return DateTimeToString(date.Value);
            }
            return null;
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
            return DataTableToDictionary(dt, "dd/MM/yyyy HH:mm:ss");
        }
        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt, string format)
        {
            return DataTableToDictionary(dt, format, new System.Globalization.CultureInfo("th-TH"));
        }
        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt, System.Globalization.CultureInfo cultureInfo)
        {
            return DataTableToDictionary(dt, "dd/MM/yyyy HH:mm:ss", cultureInfo);
        }
        public static List<Dictionary<string, object>> DataTableToDictionary(DataTable dt, string format, System.Globalization.CultureInfo cultureInfo)
        {
            List<Dictionary<string, object>> dataDict = null;
            try
            {
                dataDict = dt.AsEnumerable().Select(dr => dt.Columns.Cast<DataColumn>().ToDictionary(
                dc => dc.ColumnName,
                dc => dr[dc] is DateTime
                    ? DateTimeToString(dr[dc] as DateTime?, format, cultureInfo)
                    : dr[dc]
                    )).ToList();
                return dataDict;
            }
            catch (Exception ex)
            {
                throw new Exception("ไม่สามารถแปลงจาก DataTable เป็น Dictionary ได้ เนื่องจาก " + ex.Message);
            }
        }
        public static Type DBTypeMap(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Binary:
                    return typeof(byte[]);

                case DbType.Boolean:
                    return typeof(bool);

                case DbType.Byte:
                    return typeof(byte);

                case DbType.Currency:
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return typeof(decimal);

                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Time:
                    //case DbType.DateTimeOffset:
                    return typeof(DateTime);

                case DbType.DateTimeOffset:
                    return typeof(DateTimeOffset);

                case DbType.Double:
                    return typeof(double);

                case DbType.Guid:
                    return typeof(Guid);

                case DbType.Int16:
                    return typeof(short);

                case DbType.Int32:
                    return typeof(int);

                case DbType.Int64:
                    return typeof(long);

                case DbType.Object:
                case DbType.Xml:
                    return typeof(object);

                case DbType.SByte:
                    return typeof(sbyte);

                case DbType.Single:
                    return typeof(float);

                case DbType.UInt16:
                    return typeof(ushort);

                case DbType.UInt32:
                    return typeof(uint);

                case DbType.UInt64:
                    return typeof(ulong);

                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                case DbType.AnsiString:
                default:
                    return typeof(string);
            }
        }


        private static System.Xml.XmlElement logXmlConfig = null;
        private static void InitLogConfigure()
        {
            StringBuilder logConfig = new StringBuilder();
            logConfig.AppendLine("<log4net>");
            logConfig.AppendLine("<appender name=\"LogStoreAppender\" type=\"log4net.Appender.RollingFileAppender\">");
            logConfig.AppendLine("<file type=\"log4net.Util.PatternString\" value=\"..\\Logs\\%date{yyyy-MM}\\store-%date{yyyy-MM-dd}.log\"/>");
            logConfig.AppendLine("<appendToFile value=\"true\"/>");
            logConfig.AppendLine("<rollingStyle value=\"Composite\"/>");
            logConfig.AppendLine("<datePattern value=\"yyyy-MM-dd\"/>");
            logConfig.AppendLine("<maxSizeRollBackups value=\"10\"/>");
            logConfig.AppendLine("<maximumFileSize value=\"50MB\"/>");
            logConfig.AppendLine("<layout type=\"log4net.Layout.PatternLayout\">");
            logConfig.AppendLine("<conversionPattern value=\"%-5p %d - %m%n\"/>");
            logConfig.AppendLine("</layout>");
            logConfig.AppendLine("</appender>");
            logConfig.AppendLine("<logger name=\"LogStore\">");
            logConfig.AppendLine("<level value=\"ALL\"/>");
            logConfig.AppendLine("<appender-ref ref=\"LogStoreAppender\"/>");
            logConfig.AppendLine("</logger>");
            logConfig.AppendLine("</log4net>");
            System.Xml.XmlDocument xmlElement = new System.Xml.XmlDocument();
            xmlElement.InnerXml = logConfig.ToString();
            logXmlConfig = xmlElement.DocumentElement;

        }

        public static void WriteLogInfo(string message)
        {
            WriteLog("info", message);
        }
        public static void WriteLogInfo(string format, params object[] args)
        {
            WriteLog("info", format, null, args);
        }

        public static void WriteLogDebug(string message)
        {
            WriteLog("debug", message);
        }
        public static void WriteLogDebug(string format, params object[] args)
        {
            WriteLog("debug", format, null, args);
        }

        public static void WriteLogError(string message)
        {
            WriteLog("error", message);
        }
        public static void WriteLogError(string format, params object[] args)
        {
            WriteLog("error", format, null, args);
        }
        public static void WriteLogError(string message, Exception ex)
        {
            WriteLog("error", message, ex);
        }

        private static void WriteLog(string logType, string message, Exception ex = null)
        {
            //try
            //{
            //    //if (logXmlConfig == null) { InitLogConfigure(); }
            //    //log4net.Config.XmlConfigurator.Configure(logXmlConfig);
            //    log4net.Config.XmlConfigurator.Configure();
            //    log4net.ILog _log = log4net.LogManager.GetLogger("LogStore");
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
        private static void WriteLog(string logType, string format, Exception ex = null, params object[] args)
        {
            //try
            //{
            //    //if (logXmlConfig == null) { InitLogConfigure(); }
            //    //log4net.Config.XmlConfigurator.Configure(logXmlConfig);
            //    log4net.Config.XmlConfigurator.Configure();
            //    log4net.ILog _log = log4net.LogManager.GetLogger("LogStore");
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

        public static Assembly AssemblyFromName(string assemblyName)
        {
            Assembly assembly = null;
            string assemblySearchString = string.Empty;
            string[] assemblyProcessor = { "", ", processorArchitecture='AMD64'", ", processorArchitecture='x86'" };
            AssemblyInfo aInfo;
            IAssemblyCache ac;
            int hr = -1;
            foreach (string processor in assemblyProcessor)
            {
                try
                {
                    aInfo = new AssemblyInfo();
                    assemblySearchString = assemblyName;
                    aInfo.cchBuf = 1024; // should be fine...
                    aInfo.currentAssemblyPath = new String('\0', aInfo.cchBuf);
                    hr = CreateAssemblyCache(out ac, 0);
                    if (hr >= 0)
                    {
                        hr = ac.QueryAssemblyInfo(0, assemblySearchString, ref aInfo);
                        if (hr < 0)
                            throw new Exception("Assembly not found");
                    }
                    assembly = Assembly.LoadFrom(aInfo.currentAssemblyPath);
                    return assembly;
                }
                catch { continue; }
            }
            try
            {
                assembly = System.Reflection.Assembly.Load(assemblyName);
                return assembly;
            }
            catch
            {
                throw new Exception("Assembly not found");
            }
        }
        [System.Runtime.InteropServices.ComImport, System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown), System.Runtime.InteropServices.Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            void Reserved0();

            [System.Runtime.InteropServices.PreserveSig]
            int QueryAssemblyInfo(int flags, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string assemblyName, ref AssemblyInfo assemblyInfo);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct AssemblyInfo
        {
            public int cbAssemblyInfo;
            public int assemblyFlags;
            public long assemblySizeInKB;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public string currentAssemblyPath;
            public int cchBuf; // size of path buf.
        }

        [System.Runtime.InteropServices.DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
    }
}