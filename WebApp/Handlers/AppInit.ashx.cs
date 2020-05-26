using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp.Handlers
{
    public class AppInit : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);
                QueryResult queryResult = new QueryResult();

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.BYPASS);

                if (context.Session != null)
                {
                    if (AppSession.GetSession("USER_ID", context.Session) != null)
                    {
                        IDatabaseConnector dbConnector = new DatabaseConnectorClass();

                        string userID = AppSession.GetSession("USER_ID", context.Session).ToString();
                        string sqlStmt = "SELECT PASSWORD FROM UM_USER WHERE USER_ID=?";
                        queryParameter = new QueryParameter();
                        queryParameter.Add("USER_ID", userID);
                        QueryResult queryUser = dbConnector.ExecuteStatement(sqlStmt, queryParameter);
                        if (queryUser.DataTable != null && queryUser.DataTable.Rows.Count > 0)
                        {
                            queryParameter = new QueryParameter();
                            queryParameter.Add("USERNAME", userID);
                            queryParameter.Add("PASSWORD", queryUser.DataTable.Rows[0][0].ToString());
                            queryUser = dbConnector.ExecuteStoredProcedure("APP_LOGIN_Q", queryParameter);

                            queryParameter = new QueryParameter();
                            queryParameter.Add("APP_SESSION_USER_ID", userID);
                            queryResult = dbConnector.ExecuteStoredProcedure("APP_CONFIG_Q", queryParameter);

                            queryResult.AddOutputParam("userInfo", ConnectorUtil.DataTableToDictionary(queryUser.DataTable, dbConnector.DateTimeFormat, dbConnector.CultureInfo));
                        }
                    }

                    if (AppSession.GetSession("REQUEST_DATA", context.Session) != null)
                    {
                        queryResult.AddOutputParam("requestData", AppSession.GetSession("REQUEST_DATA", context.Session));
                    }
                }
                else
                    throw new Exception("EMPTY_SESSION");

                context.Response.ContentType = "application/json";
                context.Response.Write(queryResult.ToJson());
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;

                AppHttpHandler.ProcessResponse(context);
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}