using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp.Handlers
{
    public class AppLogout : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.LOGIN_REQUIRED);

                Logout(context.Session, context.Response);
            }
            catch (Exception exception)
            {
                AppHttpHandler.ProcessException(exception, context);
            }
            finally
            {
                context.Response.Flush();
                context.Response.End();
            }
        }

        public static void Logout(HttpSessionState Session, HttpResponse Response = null)
        {
            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryParameter logoutParameter = new QueryParameter();
            QueryResult logoutResult = new QueryResult();

            if (Session != null)
            {
                if (AppSession.GetSession("USER_ID", Session) != null)
                {
                    string userID = AppSession.GetSession("USER_ID", Session).ToString();

                    if (AppSession.GetSession("AUTHEN_TOKEN", Session) != null)
                        AuthenUtil.ClearToken(userID, AppSession.GetSession("AUTHEN_TOKEN", Session).ToString());

                    logoutParameter.Add("USER_ID", userID);
                    logoutResult = dbConnector.ExecuteStoredProcedure("SYS_I_LOGOUT", logoutParameter);
                    logoutResult.Success = true;
                    logoutResult.Message = string.Empty;
                    logoutResult.RemoveOutputParam("error");
                }
                
                AppSession.SetSession("USER_ID", null, Session);
                AppSession.SetSession("AUTHEN_TOKEN", null, Session);
                AppSession.SetSession("IS_GUEST", true, Session);

                //ถ้าอยาก clear session จะต้องบังคับให้ client refresh หน้าเว็บด้วยนะ เพราะต้อง regen CSRF ด้วย
                Session.Clear();
                Session.Abandon();
            }

            if (Response != null)
            {
                HttpCookie authenTokenCookie = new HttpCookie("AUTHEN_TOKEN");
                authenTokenCookie.Value = "";
                Response.Cookies.Add(authenTokenCookie);

                //ถ้าอยาก clear session จะต้องบังคับให้ client refresh หน้าเว็บด้วยนะ เพราะต้อง regen CSRF ด้วย
                Response.Cookies["esrith.session.id"].Expires = DateTime.Now.AddDays(-30);

                Response.ClearContent();
                Response.ContentType = "application/json";
                Response.Write(logoutResult.ToJson());
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}