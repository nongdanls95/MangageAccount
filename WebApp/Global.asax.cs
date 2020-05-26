using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Security;
using WebApp.Handlers;

namespace WebApp
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            SSLManager.EnableTrustedHosts();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            foreach(AppSessionItem item in AppSession.AppSessionConfig.SessionItems)
            {
                AppSession.SetSession(item.Name, item.Value, Session);
            }

            AppSession.SetSession("CSRF_TOKEN", AuthenUtil.GenerateToken(), Session);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

                HttpContext.Current.Response.End();
            }
            else if (HttpContext.Current.Request.HttpMethod == "GET" || HttpContext.Current.Request.HttpMethod == "POST")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            }

            if (AppHttpHandler.AppHttpHandlerConfig.Security.RedirectHttpToHttps)
            {
                switch (Request.Url.Scheme)
                {
                    case "https":
                        Response.AddHeader("Strict-Transport-Security", "max-age=300");
                        break;
                    case "http":
                        var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
                        Response.Status = "301 Moved Permanently";
                        Response.AddHeader("Location", path);
                        return;
                }
            }
        }

        protected void Application_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            AppHttpHandler.SetCookie(Context);
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

            AppLogout.Logout(Session);
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}