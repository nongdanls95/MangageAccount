using System;
using System.Data;
using System.Text;
using System.Web;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp.Handlers
{
    /// <summary>
    /// Summary description for AppLogin
    /// </summary>
    public class AppLogin : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.BYPASS);

                QueryResult queryResult = Login(context, queryParameter);
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

        public static QueryResult Login(HttpContext context, QueryParameter queryParameter)
        {
            queryParameter = new QueryParameter(queryParameter.Parameter);
            if (queryParameter.Parameter.ContainsKey("PASSWORD"))
            {
                string password = queryParameter["PASSWORD"].ToString();
                string hashPassword = AuthenUtil.GetStringSha256Hash(password);
                queryParameter.Add("PASSWORD", hashPassword);
            }

            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("APP_LOGIN_Q", queryParameter);
            if (queryResult.Success && queryResult.DataTable != null && queryResult.DataTable.Rows.Count > 0)
            {
                foreach (DataColumn dataColumn in queryResult.DataTable.Columns)
                {
                    foreach (AppSessionItem sessionItem in AppSession.AppSessionConfig.SessionItems)
                    {
                        if (sessionItem.Name.Equals(dataColumn.ColumnName))
                        {
                            AppSession.SetSession(dataColumn.ColumnName, queryResult.DataTable.Rows[0][dataColumn.ColumnName], context.Session);
                            break;
                        }
                    }
                }

                string userID = AppSession.GetSession("USER_ID", context.Session).ToString();
                string token = AuthenUtil.GenerateToken();

                AppSession.SetSession("AUTHEN_TOKEN", token, context.Session);
                AppSession.SetSession("IS_GUEST", false, context.Session);

                if (AppHttpHandler.AppHttpHandlerConfig.Security.EnableDuplicateAuthenChecking)
                    AuthenUtil.StoreToken(userID, token);

                // ถ้าต้องการให้ retrun ค่าของ USER_ID ไปด้วยให้ลบบรรทัดนี้เลย
                queryResult.DataTable.Columns.Remove("USER_ID");
            }
            return queryResult;
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