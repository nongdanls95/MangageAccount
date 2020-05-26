using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WebApp.Classes;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;

namespace WebApp.Classes.Connector
{
    public class NetworkConnector : IDisposable
    {
        private static Dictionary<string, Connector.NetworkConnector> NetConnector = new Dictionary<string, Connector.NetworkConnector>();

        public static bool Access(string Path)
        {
            bool success = false;
            Path = System.IO.Path.GetPathRoot(Path);
            if (Path.StartsWith(@"\\"))
            {
                if (!NetConnector.ContainsKey(Path))
                {
                    NetConnector.Add(Path, new Connector.NetworkConnector());
                }
                int counter = 0;
                if (NetConnector[Path].LastError != 0)
                {
                    while (!(success = NetConnector[Path].NetUseWithCredentials(Path)) && counter < 10)
                    {
                        System.Threading.Thread.Sleep(10);
                        counter++;
                    }
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                return true;
            }
            return success;
        }
        public static bool Access(string Path, string User, string Domain, string Password)
        {
            bool success = false;
            Path = System.IO.Path.GetPathRoot(Path);
            if (!NetConnector.ContainsKey(Path))
            {
                NetConnector.Add(Path, new Connector.NetworkConnector());
            }
            int counter = 0;
            while (!(success = NetConnector[Path].NetUseWithCredentials(Path, User, Domain, Password)) && counter < 10)
            {
                System.Threading.Thread.Sleep(10);
                counter++;
            }
            return success;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct USE_INFO_2
        {
            internal LPWSTR ui2_local;
            internal LPWSTR ui2_remote;
            internal LPWSTR ui2_password;
            internal DWORD ui2_status;
            internal DWORD ui2_asg_type;
            internal DWORD ui2_refcount;
            internal DWORD ui2_usecount;
            internal LPWSTR ui2_username;
            internal LPWSTR ui2_domainname;
        }

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseAdd(
            LPWSTR UncServerName,
            DWORD Level,
            ref USE_INFO_2 Buf,
            out DWORD ParmError);

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseDel(
            LPWSTR UncServerName,
            LPWSTR UseName,
            DWORD ForceCond);

        private bool disposed = false;

        private string sUNCPath;
        private string sUser;
        private string sPassword;
        private string sDomain;
        private int iLastError = -1;

        /// <summary>
        /// A disposeable class that allows access to a UNC resource with credentials.
        /// </summary>
        public NetworkConnector()
        {
        }

        /// <summary>
        /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
        /// </summary>
        public int LastError
        {
            get { return iLastError; }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                NetUseDelete();
            }
            disposed = true;
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Connects to a UNC path using the credentials supplied.
        /// </summary>
        /// <param name="UNCPath">Fully qualified domain name UNC path</param>
        /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
        public bool NetUseWithCredentials(string UNCPath)
        {
            sUNCPath = UNCPath;
            sUser = Core.WebConfigReadKey("UNC_USERNAME");
            sPassword = Core.WebConfigReadKey("UNC_PASSWORD");
            sDomain = Core.WebConfigReadKey("UNC_DOMAIN");
            return NetUseWithCredentials();
        }

        /// <summary>
        /// Connects to a UNC path using the credentials supplied.
        /// </summary>
        /// <param name="UNCPath">Fully qualified domain name UNC path</param>
        /// <param name="User">A user with sufficient rights to access the path.</param>
        /// <param name="Domain">Domain of User.</param>
        /// <param name="Password">Password of User</param>
        /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
        public bool NetUseWithCredentials(string UNCPath, string User, string Domain, string Password)
        {
            sUNCPath = UNCPath;
            sUser = User;
            sPassword = Password;
            sDomain = Domain;
            return NetUseWithCredentials();
        }

        private bool NetUseWithCredentials()
        {
            uint returncode;
            try
            {
                USE_INFO_2 useinfo = new USE_INFO_2();

                useinfo.ui2_remote = sUNCPath;
                useinfo.ui2_username = sUser;
                useinfo.ui2_domainname = sDomain;
                useinfo.ui2_password = sPassword;
                useinfo.ui2_asg_type = 0;
                useinfo.ui2_usecount = 1;
                uint paramErrorIndex;
                returncode = NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
                iLastError = (int)returncode;
                return returncode == 0;
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        /// <summary>
        /// Ends the connection to the remote resource 
        /// </summary>
        /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
        public bool NetUseDelete()
        {
            uint returncode;
            try
            {
                returncode = NetUseDel(null, sUNCPath, 2);
                iLastError = (int)returncode;
                return (returncode == 0);
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        ~NetworkConnector()
        {
            Dispose();
        }

    }

}