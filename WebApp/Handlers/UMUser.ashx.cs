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
    /// <summary>
    /// Summary description for UMUser
    /// </summary>
    public class UMUser : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.BYPASS);

                string mode = null;
                if (queryParameter["MODE"] != null)
                    mode = queryParameter["MODE"].ToString();

                QueryResult queryResult = null;
                if (mode == "CREATE")
                    queryResult = CreateUser(queryParameter);
                else if (mode == "UPDATE")
                    queryResult = UpdateUser(queryParameter);
                else if (mode == "DELETE")
                    queryResult = DeleteUser(queryParameter);
                else if (mode == "EXIST")
                    queryResult = UserExists(queryParameter);
                else if (mode == "FORGETPWD")
                    queryResult = ForgetPassword(queryParameter);
                else
                {
                    queryResult = new QueryResult();
                    queryResult.Success = false;
                    queryResult.Message = "Operation Invalid";
                }

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

        public static QueryResult CreateUser(QueryParameter queryParameter)
        {
            queryParameter = new QueryParameter(queryParameter.Parameter);

            if (queryParameter.Parameter.ContainsKey("PASSWORD"))
            {
                string password = queryParameter["PASSWORD"].ToString();
                string hashPassword = AuthenUtil.GetStringSha256Hash(password);
                queryParameter.Add("PASSWORD", hashPassword);
            }
            
            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_I", queryParameter);

            return queryResult;
        }

        public static QueryResult UpdateUser(QueryParameter queryParameter)
        {
            queryParameter = new QueryParameter(queryParameter.Parameter);

            if (queryParameter.Parameter.ContainsKey("PASSWORD"))
            {
                string password = queryParameter["PASSWORD"].ToString();
                string hashPassword = AuthenUtil.GetStringSha256Hash(password);
                queryParameter.Add("PASSWORD", hashPassword);
            }
            
            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_U", queryParameter);

            return queryResult;
        }

        public static QueryResult DeleteUser(QueryParameter queryParameter)
        {
            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_D", queryParameter);

            return queryResult;
        }
        
        public static QueryResult UserExists(QueryParameter queryParameter)
        {
            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_EXISTS", queryParameter);

            return queryResult;
        }

        public static QueryResult ForgetPassword(QueryParameter queryParameter)
        {
            QueryResult result = null;
            string userID = queryParameter["USER_ID"].ToString();

            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryParameter param = new QueryParameter();
            param.Add("USER_ID", userID);
            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_Q", param);

            if (queryResult.Success)
            {
                string userEmail = queryResult.DataTable.Rows[0]["EMAIL"].ToString();
                string token = AuthenUtil.GetStringSha256Hash(AuthenUtil.GenerateToken());

                param = new QueryParameter();
                param.Add("USER_ID", userID);
                param.Add("TOKEN", token);

                result = dbConnector.ExecuteStoredProcedure("APP_FORGET_PWD_TOKEN_I", param);

                if (result.Success)
                {
                    string passwordResetUrl = string.Format("http://localhost/WebApp/resetpassword.aspx?userID={0}&token={1}", userID, token);

                    QueryParameter mailParameter = new QueryParameter();
                    mailParameter.Add("MAIL_TO", userEmail);
                    mailParameter.Add("MAIL_SUBJECT", "Reset Password");
                    mailParameter.Add("MAIL_BODY", string.Format(@"
                        <h1>Reset Password</h1>
                        <div>
                            You have requested to reset password for account {0} <br/>
                            <b>Plase contact administrator if you have not issued reset password request.</b>
                        </div>
                        <br/>
                        Click <a href=""{1}"">here</a> to reset password.
                    ", userID, passwordResetUrl));

                    result = MailUtil.SendEmail(mailParameter);
                }
            }
            else
            {
                result = new QueryResult();
                result.Success = false;
                result.Message = "USER_NOT_EXIST";
            }

            return result;
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