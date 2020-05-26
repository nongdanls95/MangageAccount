using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.SessionState;
using WebApp.Classes;
using WebApp.Classes.Connector;
using WebApp.Classes.Security;

namespace WebApp.Handlers
{
    /// <summary>
    /// Summary description for AppFile
    /// </summary>
    public class AppFile : IHttpHandler, IRequiresSessionState
    {
        enum Actions
        {
            Upload,
            Download,
            Remove
        };
        private static string _appFileActionParameterName = "ACTION";
        private static string _appFileFileIdParameterName = "FILE_ID";
        private static string _appFileFileNameParameterName = "FILE_NAME";
        private static string _appFilePath = "AppUploadedFiles";

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                QueryParameter queryParameter = new QueryParameter(context);

                AppHttpHandler.ProcessRequest(context, queryParameter, AuthenUtil.AuthenMode.LOGIN_REQUIRED);

                QueryResult queryResult = new QueryResult();
                int action;
                try
                {
                    action = int.Parse(queryParameter[_appFileActionParameterName].ToString());
                }
                catch
                {
                    action = -1;
                }
                if (action == (int)Actions.Upload)
                {
                    queryResult.DataTable.Columns.Add("FILE_PARAMETER_NAME");
                    queryResult.DataTable.Columns.Add("FILE_NAME");
                    queryResult.DataTable.Columns.Add("FILE_ID");
                    foreach (string fileParameterName in context.Request.Files)
                    {
                        HttpPostedFile file = context.Request.Files[fileParameterName];
                        string resultFileId = null;
                        if (!string.IsNullOrEmpty(file.FileName))
                        {
                            resultFileId = SaveFile(file);
                            if (string.IsNullOrEmpty(resultFileId))
                            {
                                queryResult.Success = false;
                                queryResult.DataTable.Rows.Clear();
                                break;
                            }
                            else
                            {
                                DataRow dataRow = queryResult.DataTable.NewRow();
                                dataRow["FILE_ID"] = resultFileId;
                                dataRow["FILE_PARAMETER_NAME"] = fileParameterName;
                                dataRow["FILE_NAME"] = file.FileName;
                                queryResult.DataTable.Rows.Add(dataRow);
                            }
                        }
                    }
                    context.Response.ContentType = "application/json";
                    context.Response.Write(queryResult.ToJson());
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                }
                else if (action == (int)Actions.Download)
                {
                    string fileId = queryParameter[_appFileFileIdParameterName].ToString();
                    string extension;
                    FileStream f = GetFile(fileId, out extension);
                    if (f == null)
                    {
                        queryResult.Success = false;
                        queryResult.Message = "File not found.";
                        context.Response.ContentType = "application/json";
                        context.Response.Write(queryResult.ToJson());
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    }
                    else
                    {
                        string fileName = "";
                        try
                        {
                            fileName = HttpUtility.UrlDecode(queryParameter[_appFileFileNameParameterName].ToString());
                        }
                        catch { }
                        if (fileName.Equals(""))
                        {
                            fileName = fileId;
                        }
                        int length = (int)f.Length;
                        byte[] buffer = new byte[length];
                        int sum = 0;
                        int count;
                        while ((count = f.Read(buffer, sum, length - sum)) > 0)
                        {
                            sum += count;
                        }
                        f.Close();
                        string contentType = "";
                        string contentDispositionMode = "inline";
                        if (string.IsNullOrEmpty(extension))
                        {
                            contentDispositionMode = "attachment";
                        }
                        else
                        {
                            switch (extension.ToLower())
                            {
                                case ".gif":
                                    contentType = "image/gif";
                                    break;
                                case ".jpg":
                                case ".jpe":
                                case ".jpeg":
                                    contentType = "image/jpeg";
                                    break;
                                case ".png":
                                    contentType = "image/png";
                                    break;
                                case ".bmp":
                                    contentType = "image/bmp";
                                    break;
                                case ".tif":
                                case ".tiff":
                                    contentType = "image/tiff";
                                    break;
                                case ".eps":
                                    contentType = "application/postscript";
                                    break;
                                default:
                                    contentDispositionMode = "attachment";
                                    string mimeType = extension.ToLower();
                                    mimeType = mimeType.Replace(".", "");
                                    contentType = "application/" + mimeType;
                                    break;
                            }
                        }
                        context.Response.Headers.Add("Content-Length", length.ToString());
                        context.Response.Headers.Add("Content-Disposition", contentDispositionMode + "; filename=" + HttpUtility.UrlPathEncode(fileName));
                        context.Response.ContentType = contentType;
                        context.Response.BinaryWrite(buffer);
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                    }
                }
                else if (action == (int)Actions.Remove)
                {
                    RemoveFile(queryParameter[_appFileFileIdParameterName].ToString());
                    context.Response.ContentType = "application/json";
                    context.Response.Write(queryResult.ToJson());
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                }
                else
                {
                    queryResult.Success = false;
                    queryResult.Message = _appFileActionParameterName + " is not valid.";
                    context.Response.ContentType = "application/json";
                    context.Response.Write(queryResult.ToJson());
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                }

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

        private static string GetPath()
        {
            string path = Path.Combine(Path.GetDirectoryName(HttpContext.Current.Server.MapPath("~")), _appFilePath);
            if (NetworkConnector.Access(path))
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            else
            {
                throw new Exception("Cannot access file path.");
            }
        }

        public static FileStream GetFile(string fileId, out string extension)
        {
            try
            {
                extension = Path.GetExtension(fileId);
                return new FileStream(Path.Combine(GetPath(), fileId), FileMode.Open, FileAccess.Read);
            }
            catch
            {
                extension = null;
                return null;
            }
        }

        public static string SaveFile(HttpPostedFile file)
        {
            try
            {
                string extension = Path.GetExtension(file.FileName);
                string fileId = string.Format("{0}-{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), new Random().Next(999).ToString("000"), extension);
                file.SaveAs(Path.Combine(GetPath(), fileId));
                return fileId;
            }
            catch
            {
                return null;
            }
        }

        public static bool RemoveFile(string fileId)
        {
            try
            {
                string removeFilePath = Path.Combine(GetPath(), fileId);
                if (File.Exists(removeFilePath))
                {
                    File.Delete(removeFilePath);
                    return true;
                }
                else
                {
                    throw new Exception("File does not exist.");
                }
            }
            catch
            {
                return false;
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