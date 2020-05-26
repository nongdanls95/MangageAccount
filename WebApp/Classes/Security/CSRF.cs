using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using WebApp.Classes.Connector;

/// <summary>
/// Summary description for CSRF
/// </summary>

namespace WebApp.Classes.Security
{
    public class CSRF
    {
        public static bool IsCSRFTokenMatch(HttpSessionState Session, string csrfToken)
        {
            if (AppHttpHandler.AppHttpHandlerConfig.Security.EnableCSRF)
            {
                if (csrfToken != null &&
                    csrfToken.Length != 0 &&
                    AppSession.GetSession("CSRF_TOKEN", Session) != null &&
                    string.Equals(AppSession.GetSession("CSRF_TOKEN", Session).ToString(), csrfToken))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool IsCSRFTokenMatch(HttpSessionState Session, QueryParameter queryParam)
        {
            string csrfToken = null;

            if (queryParam != null && queryParam["CSRF_TOKEN"] != null)
                csrfToken = queryParam["CSRF_TOKEN"].ToString();

            return IsCSRFTokenMatch(Session, csrfToken);
        }
    }
}