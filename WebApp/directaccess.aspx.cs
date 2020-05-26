using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Handlers;

namespace WebApp
{
    public partial class directaccess : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.UrlReferrer != null)
            {
                QueryParameter queryParam = new QueryParameter(Request);

                if (queryParam.Parameter.ContainsKey("USERNAME") && queryParam.Parameter.ContainsKey("PASSWORD"))
                {
                    if (AppSession.GetSession("USER_ID") == null)
                    {
                        QueryParameter loginParam = new QueryParameter();
                        loginParam.Add("USERNAME", queryParam["USERNAME"]);
                        loginParam.Add("PASSWORD", queryParam["PASSWORD"]);
                        QueryResult queryResult = AppLogin.Login(HttpContext.Current, loginParam);
                    }
                    queryParam.Remove("USERNAME");
                    queryParam.Remove("PASSWORD");
                }
                AppSession.SetSession("REQUEST_DATA", queryParam.Parameter);
                Response.Redirect(Request.ApplicationPath);
            }
        }
    }
}