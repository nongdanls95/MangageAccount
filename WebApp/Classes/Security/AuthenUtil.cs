using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;

namespace WebApp.Classes.Security
{
    public class AuthenUtil
    {
        public enum AuthenMode
        {
            BYPASS,
            LOGIN_REQUIRED
        }

        public static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static string GenerateToken()
        {
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return token;
        }

        public static bool IsTokenMatchInDatabase(string userID, string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryParameter queryParam = new QueryParameter();
            queryParam.Add("USER_ID", userID);
            queryParam.Add("TOKEN", GetStringSha256Hash(token));

            QueryResult queryResult = dbConnector.ExecuteStoredProcedure("APP_TOKEN_MATCH", queryParam);

            return queryResult.Success;
        }

        public static void StoreToken(string userID, string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new Exception("Authentication token cannot be empty");

            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryParameter queryParam = new QueryParameter();
            queryParam.Add("USER_ID", userID);
            queryParam.Add("TOKEN", GetStringSha256Hash(token));

            dbConnector.ExecuteStoredProcedure("APP_TOKEN_I", queryParam);
        }

        public static void ClearToken(string userID, string token)
        {
            if (string.IsNullOrEmpty(token))
                return;

            IDatabaseConnector dbConnector = new DatabaseConnectorClass();
            QueryParameter queryParam = new QueryParameter();
            queryParam.Add("USER_ID", userID);
            queryParam.Add("TOKEN", GetStringSha256Hash(token));

            dbConnector.ExecuteStoredProcedure("APP_TOKEN_D", queryParam);
        }

        public static bool IsValidAuthen(HttpRequest Request, HttpSessionState Session, AuthenMode authenMode, out string errorMessage)
        {
            string userID = "";
            string cookieToken = "";
            string sessionToken = "";

            if (AppSession.GetSession("USER_ID", Session) != null)
                userID = AppSession.GetSession("USER_ID", Session).ToString();

            if (Request.Cookies["AUTHEN_TOKEN"] != null)
                cookieToken = Request.Cookies["AUTHEN_TOKEN"].Value;

            if (AppSession.GetSession("AUTHEN_TOKEN", Session) != null)
                sessionToken = AppSession.GetSession("AUTHEN_TOKEN", Session).ToString();

            errorMessage = "";

            if (authenMode == AuthenMode.BYPASS)
            {
                return true;
            }
            else if (!string.IsNullOrEmpty(sessionToken) && sessionToken == cookieToken)
            {
                if (AppHttpHandler.AppHttpHandlerConfig.Security.EnableDuplicateAuthenChecking)
                {
                    if (IsTokenMatchInDatabase(userID, sessionToken))
                        return true;
                    else
                    {
                        //Duplicate login detected.
                        errorMessage = "DUPLICATE_LOGIN";
                        return false;
                    }
                }
                else
                    return true;
            }
            else
            {
                errorMessage = "NOT_AUTHORIZED";
                return false;
            }
        }
    }
}