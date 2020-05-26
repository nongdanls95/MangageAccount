using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WebApp.Classes.Security
{
    public class SSLManager
    {
        public SSLManager()
        {
        }

        //private static readonly string[] TrustedHosts = new[] {
        //      "localhost",
        //      "krungsriapp.com",
        //      "cluster5-uat.webcon.krungsri.net",
        //      "157.179.28.135",
        //      "192.168.35.117",
        //      "192.168.35.116"
        //};

        public static void EnableTrustedHosts()
        {
            ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, errors) =>
            {
                //I will force to trust all SSL. 
                return true;

                //if (errors == SslPolicyErrors.None)
                //{
                //    return true;
                //}

                //var request = sender as HttpWebRequest;
                //if (request != null)
                //{
                //    return TrustedHosts.Contains(request.RequestUri.Host);
                //}
                //return false;
            };
        }
    }
}