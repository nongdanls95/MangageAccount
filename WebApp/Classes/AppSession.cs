using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace WebApp.Classes
{

    public class AppSession
    {
        public static AppSessionConfig AppSessionConfig
        {
            get { return GetAppSessionConfig(); }
        }
        private static string _AppSessionConfigFilePath = HttpContext.Current.Server.MapPath("~/Configs/AppSession.json");

        private static AppSessionConfig _AppSessionConfig;
        private static DateTime _AppSessionConfigDate = DateTime.Now;

        private static AppSessionConfig GetAppSessionConfig()
        {
            DateTime lastModified = File.GetLastWriteTime(_AppSessionConfigFilePath);

            if (_AppSessionConfig == null || _AppSessionConfigDate != lastModified)
            {
                using (StreamReader r = new StreamReader(_AppSessionConfigFilePath))
                {
                    string appSessionConfigJSONString = r.ReadToEnd();
                    _AppSessionConfig = JsonConvert.DeserializeObject<AppSessionConfig>(appSessionConfigJSONString);
                }

                _AppSessionConfigDate = lastModified;
            }
            return _AppSessionConfig;
        }

        public static List<Dictionary<string, object>> GetProcedureSessionParameters()
        {
            List<Dictionary<string, object>> procedureSessionParameters = new List<Dictionary<string, object>>();

            return procedureSessionParameters;
        }

        public static void SetSession(string name, object value, HttpSessionState Session = null)
        {
            if (Session == null)
                Session = HttpContext.Current.Session;

            Session[AppSessionConfig.SessionItemPrefix + name] = value;
        }

        public static object GetSession(string name, HttpSessionState Session = null)
        {
            if (Session == null)
                Session = HttpContext.Current.Session;

            return Session == null ? null : Session[AppSessionConfig.SessionItemPrefix + name];
        }
    }

    public class AppSessionConfig
    {
        public string SessionItemPrefix { get; set; }
        public List<AppSessionItem> SessionItems { get; set; }
        public string ProcedureSessionParameterPrefix { get; set; }
        public List<AppSessionProcedureSessionParameter> ProcedureSessionParameters { get; set; }
    }

    public class AppSessionItem
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class AppSessionProcedureSessionParameter
    {
        public string Name { get; set; }
        public string SessionItemName { get; set; }
        public bool AutoInclusion { get; set; }
    }
}