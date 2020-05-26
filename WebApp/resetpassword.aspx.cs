using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp
{
    public partial class resetpassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string userID = Request.Params["userID"];
            string token = Request.Params["token"];

            if (userID != null && token != null)
            {
                IDatabaseConnector dbConnector = new DatabaseConnectorClass();
                QueryParameter param = new QueryParameter();
                QueryResult queryResult = null;

                // check if userId, token is valid.

                param = new QueryParameter();
                param.Add("USER_ID", userID);
                param.Add("TOKEN", token);
                queryResult = dbConnector.ExecuteStoredProcedure("APP_FORGET_PWD_TOKEN_MATCH", param);

                if (!queryResult.Success)
                {
                    this.PasswordResetForm.Visible = false;

                    if(queryResult.Message == "TOKEN_EXPIRED")
                        this.Message.Text = "URL is expired";
                    else
                        this.Message.Text = "Unauthorized";
                }

                //if this request come form asp.net webform, then
                //  check if password == confirmpassword
                if (IsPostBack)
                {
                    string password = this.Password.Text.Trim();
                    string passwordConfirm = this.PasswordConfirm.Text;

                    if(password.Length == 0)
                    {
                        this.Message.Text = "Password is empty";
                    }
                    else if (password != passwordConfirm)
                    {
                        this.Message.Text = "Password is not match";
                    }
                    else
                    {
                        string hashPassword = AuthenUtil.GetStringSha256Hash(password);

                        param = new QueryParameter();
                        param.Add("USER_ID", userID);
                        param.Add("PASSWORD", hashPassword);

                        queryResult = dbConnector.ExecuteStoredProcedure("UM_USER_PWD_U", param);

                        if (queryResult.Success)
                        {
                            param = new QueryParameter();
                            param.Add("USER_ID", userID);
                            queryResult = dbConnector.ExecuteStoredProcedure("APP_FORGET_PWD_TOKEN_D", param);

                            this.PasswordResetForm.Visible = false;
                            this.Message.Text = "Password reset successful. Please go to login page.";
                        }
                    }
                }
            }
            else
            {
                this.PasswordResetForm.Visible = false;
                this.Message.Text = "Unauthorized";
            }
        }
    }
}