using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp.Handlers
{
    /// <summary>
    /// Summary description for AppData
    /// </summary>
    public class AppData : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.LOGIN_REQUIRED);

                QueryResult queryResult = ExecuteProcedure(queryParameter);
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

        public static QueryResult ExecuteProcedure(QueryParameter queryParameter)
        {
            foreach (AppSessionProcedureSessionParameter procedureSessionParameter in AppSession.AppSessionConfig.ProcedureSessionParameters)
            {
                bool included = false;
                foreach (KeyValuePair<string, object> parameter in queryParameter.Parameter)
                {
                    if (parameter.Key.Equals(procedureSessionParameter.Name))
                    {
                        queryParameter[AppSession.AppSessionConfig.ProcedureSessionParameterPrefix + procedureSessionParameter.Name] = AppSession.GetSession(procedureSessionParameter.Name);
                        included = true;
                        break;
                    }
                }
                if (procedureSessionParameter.AutoInclusion && !included)
                {
                    queryParameter[AppSession.AppSessionConfig.ProcedureSessionParameterPrefix + procedureSessionParameter.Name] = AppSession.GetSession(procedureSessionParameter.Name);
                }
            }
            return new DatabaseConnectorClass().ExecuteStoredProcedure(queryParameter);
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