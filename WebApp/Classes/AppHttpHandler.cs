using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;
using WebApp.Handlers;

namespace WebApp.Classes
{
    public class AppHttpHandler
    {
        public static AppHttpHandlerConfig AppHttpHandlerConfig
        {
            get { return GetAppHttpHandlerConfig(); }
        }
        private static string _AppHttpHandlerConfigFilePath = HttpContext.Current.Server.MapPath("~/Configs/AppHttpHandler.json");

        private static AppHttpHandlerConfig _AppHttpHandlerConfig;
        private static DateTime _AppHttpHandlerConfigDate = DateTime.Now;

        private static AppHttpHandlerConfig GetAppHttpHandlerConfig()
        {
            DateTime lastModified = File.GetLastWriteTime(_AppHttpHandlerConfigFilePath);

            if (_AppHttpHandlerConfig == null || _AppHttpHandlerConfigDate != lastModified)
            {
                using (StreamReader r = new StreamReader(_AppHttpHandlerConfigFilePath))
                {
                    string appHttpHandlerConfigJSONString = r.ReadToEnd();
                    _AppHttpHandlerConfig = JsonConvert.DeserializeObject<AppHttpHandlerConfig>(appHttpHandlerConfigJSONString);
                }

                _AppHttpHandlerConfigDate = lastModified;
            }
            return _AppHttpHandlerConfig;
        }

        public static void ProcessRequest(HttpContext context, QueryParameter queryParameter, AuthenUtil.AuthenMode authenMode)
        {
            //Set Encoding.
            context.Response.ContentEncoding = Encoding.UTF8;

            // Security checks
            string errorMessage = "";

            if (!AuthenUtil.IsValidAuthen(context.Request, context.Session, authenMode, out errorMessage))
            {
                //message can be "DUPLICATE_LOGIN", "NOT_AUTHORIZED"
                throw new Exception(errorMessage);
            }

            if (!CSRF.IsCSRFTokenMatch(context.Session, queryParameter))
            {
                throw new Exception("CSRF_TOKEN_MISMATCH");
            }
        }

        public static void SetCookie(HttpContext context)
        {
            //assign authentication token for checking authentication.
            if (AppSession.GetSession("AUTHEN_TOKEN", context.Session) != null)
            {
                HttpCookie authenTokenCookie = new HttpCookie("AUTHEN_TOKEN");
                authenTokenCookie.Value = AppSession.GetSession("AUTHEN_TOKEN", context.Session).ToString();
                context.Response.Cookies.Add(authenTokenCookie);
            }

            //assign CSRF token for checking form sending from across domain.
            if (AppSession.GetSession("CSRF_TOKEN", context.Session) != null)
            {
                HttpCookie authenTokenCookie = new HttpCookie("CSRF_TOKEN");
                authenTokenCookie.Value = AppSession.GetSession("CSRF_TOKEN", context.Session).ToString();
                context.Response.Cookies.Add(authenTokenCookie);
            }
        }

        public static void ProcessResponse(HttpContext context)
        {
            // Edit response

            SetCookie(context);
        }

        public static void ProcessException(Exception exception, HttpContext context)
        {
            context.Response.ContentEncoding = Encoding.UTF8;

            QueryResult queryResult = new QueryResult();
            queryResult.Success = false;
            queryResult.Message = exception.Message;
            context.Response.ContentType = "application/json";
            context.Response.Write(queryResult.ToJson());
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
        }
    }

    public class AppHttpHandlerSecurityConfig
    {
        public bool RedirectHttpToHttps { get; set; }
        public bool EnableCSRF { get; set; }
        public bool EnableDuplicateAuthenChecking { get; set; }
    }
    public class AppHttpHandlerEmailConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderAddress { get; set; }
        public bool EnableSSL { get; set; }
    }
    public class AppHttpHandlerLogConfig
    {
        public List<string> HideFields { get; set; }
        public List<string> SessionFields { get; set; }
    }
    public class AppHttpHandlerConfig
    {
        public AppHttpHandlerSecurityConfig Security { get; set; }
        public AppHttpHandlerEmailConfig Email { get; set; }
        public AppHttpHandlerLogConfig Logs { get; set; }
    }
}