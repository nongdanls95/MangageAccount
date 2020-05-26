using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for Connector
/// </summary>
namespace WebApp.Classes.Connector
{
    public class QueryParameter
    {
        private string FILE_PARAM_NAME = "__FILE__";
        public bool IsUseSession = false;
        public Dictionary<string, object> Parameter;
        public List<FileParameter> Files;
        public QueryParameter()
        {
            this.Parameter = new Dictionary<string, object>();
        }
        public QueryParameter(Dictionary<string, object> param)
        {
            try
            {
                this.Parameter = param;
            }
            catch (Exception ex) { throw ex; }
        }
        public QueryParameter(string reqString)
        {
            _QueryParameter(reqString);
        }
        public QueryParameter(byte[] reqBytes)
        {
            string reqString = System.Text.Encoding.UTF8.GetString(reqBytes);
            _QueryParameter(reqString);
        }
        public QueryParameter(Stream reqStream)
        {

            StreamReader streamReader = null;
            string reqString = null;
            try
            {
                streamReader = new StreamReader(reqStream);
                reqString = streamReader.ReadToEnd();
                _QueryParameter(reqString);
            }
            catch (Exception ex) { throw ex; }
        }
        public QueryParameter(HttpRequest reqHttp)
        {
            _QueryParameter(reqHttp);
        }

        //for has file & session (from proxy page)
        public QueryParameter(HttpContext reqContext)
        {
            List<string> sessionKey = new List<string>();
            //List<Dictionary<string, object>> fileList = null;
            NameValueCollection appSettings = WebConfigurationManager.AppSettings;
            string tempPath = string.Empty;
            try
            {
                if (reqContext.Request.ContentType.StartsWith("multipart/form-data"))
                {
                    if (reqContext.Request.Form.Count > 0)
                    {
                        try
                        {
                            this.Parameter = new Dictionary<string, object>();
                            for (int i = 0; i < reqContext.Request.Form.Count; i++)
                            {
                                this.Parameter.Add(reqContext.Request.Form.GetKey(i), reqContext.Request.Form[i]);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
                else
                {
                    _QueryParameter(reqContext.Request);
                }

                //if (reqContext.Request.Files.Count > 0)
                //{
                //    fileList = new List<Dictionary<string, object>>();
                //    tempPath = System.IO.Path.Combine(reqContext.Server.MapPath("~/"), Core.WebConfigReadKey("TEMPORARY_PATH"));
                //    try
                //    {
                //        if (NetworkConnector.Access(tempPath))
                //        {
                //            if (!Directory.Exists(tempPath))
                //            {
                //                Directory.CreateDirectory(tempPath);
                //            }
                //            DeleteOldFile(tempPath);
                //        }
                //    }
                //    catch { }
                //    if (this.Parameter == null)
                //    {
                //        this.Parameter = new Dictionary<string, object>();
                //    }
                //    if (!this.Parameter.ContainsKey(FILE_PARAM_NAME))
                //    {
                //        this.Parameter.Add(FILE_PARAM_NAME, fileList);
                //    }

                //    for (int i = 0; i < reqContext.Request.Files.Count; i++)
                //    {
                //        string fileName = reqContext.Request.Files[i].FileName;
                //        if (string.IsNullOrEmpty(fileName)) continue;
                //        string contentType = reqContext.Request.Files[i].ContentType;
                //        string fileExt = System.IO.Path.GetExtension(fileName);
                //        string newFileName = string.Format("{0}-{1}{2}", DateTime.Now.Ticks, new Random().Next(999).ToString("000"), fileExt);
                //        string fullName = System.IO.Path.Combine(tempPath, newFileName);
                //        if (!Directory.Exists(tempPath))
                //        {
                //            Directory.CreateDirectory(tempPath);
                //        }
                //        reqContext.Request.Files[i].SaveAs(fullName);
                //        fileList.Add(new Dictionary<string, object>(){
                //            {"NAME",fileName},
                //            {"CONTENT_TYPE",contentType},
                //            {"FULLNAME",fullName}
                //        });
                //    }
                //}
                if (this.Parameter == null)
                {
                    this.Parameter = new Dictionary<string, object>();
                }
            }
            catch { }
        }

        public object this[string key]
        {
            get
            {
                if (this.Parameter != null && this.Parameter.ContainsKey(key))
                {
                    return this.Parameter[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.Add(key, value);
            }
        }

        private void _QueryParameter(string reqString)
        {
            string boundary = GetBoundary(reqString);
            if (string.IsNullOrEmpty(boundary))
            {
                JavaScriptSerializer serializer = null;
                try
                {
                    serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
                    this.Parameter = new Dictionary<string, object>();
                    this.Parameter = serializer.Deserialize<Dictionary<string, object>>(reqString);
                    try
                    {
                        if (this.Parameter.ContainsKey(FILE_PARAM_NAME))
                        {
                            this.Files = new List<FileParameter>();
                            foreach (Dictionary<string, object> files in (this.Parameter[FILE_PARAM_NAME] as System.Collections.ArrayList))
                            {
                                this.Files.Add(new FileParameter(files["NAME"].ToString(), files["FULLNAME"].ToString()));
                            }
                        }
                    }
                    catch { }
                    return;
                }
                catch { }
                try
                {
                    serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
                    this.Parameter = new Dictionary<string, object>();
                    this.Parameter = serializer.Deserialize<Dictionary<string, object>>(reqString);
                    try
                    {
                        if (this.Parameter.ContainsKey(FILE_PARAM_NAME))
                        {
                            this.Files = new List<FileParameter>();
                            foreach (Dictionary<string, object> files in (this.Parameter[FILE_PARAM_NAME] as System.Collections.ArrayList))
                            {
                                this.Files.Add(new FileParameter(files["NAME"].ToString(), files["FULLNAME"].ToString()));
                            }
                        }
                    }
                    catch { }
                    return;
                }
                catch { }
                try
                {
                    this.Parameter = new Dictionary<string, object>();
                    System.Collections.Specialized.NameValueCollection nameValueCollect = HttpUtility.ParseQueryString(reqString);
                    for (int i = 0; i < nameValueCollect.Count; i++)
                    {
                        try
                        {
                            if (nameValueCollect[i].StartsWith("[") && nameValueCollect[i].EndsWith("]"))
                            {
                                this.Parameter.Add(nameValueCollect.GetKey(i), serializer.Deserialize<List<Dictionary<string, object>>>(nameValueCollect[i]));
                            }
                            else if (nameValueCollect[i].StartsWith("{") && nameValueCollect[i].EndsWith("}"))
                            {
                                this.Parameter.Add(nameValueCollect.GetKey(i), serializer.Deserialize<Dictionary<string, object>>(nameValueCollect[i]));
                            }
                            else
                            {
                                this.Parameter.Add(nameValueCollect.GetKey(i), nameValueCollect[i]);
                            }
                        }
                        catch
                        {
                            this.Parameter.Add(nameValueCollect.GetKey(i), nameValueCollect[i]);
                        }
                    }
                    return;
                }
                catch { }
            }
            else
            {
                try
                {
                    this.Parameter = new Dictionary<string, object>();
                    for (int i = reqString.IndexOf(boundary) + boundary.Length; i < reqString.Length; i = reqString.IndexOf(boundary, i) > -1 ? reqString.IndexOf(boundary, i) + boundary.Length : reqString.Length)
                    {
                        string paramName = string.Empty;
                        string paramContentType = string.Empty;
                        string paramFileName = string.Empty;
                        object paramValue = null;

                        string formData = string.Empty;
                        try
                        {
                            formData = reqString.Substring(i, reqString.IndexOf(boundary, i) - i);
                        }
                        catch
                        {
                            formData = reqString.Substring(i);
                        }
                        string[] readFormdata = System.Text.RegularExpressions.Regex.Split(formData, "\r\n\r\n").Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();
                        if (readFormdata.Length < 2) continue;
                        foreach (string readLine in System.Text.RegularExpressions.Regex.Split(readFormdata[0], "\r\n").Where(s => !string.IsNullOrEmpty(s.Trim())))
                        {
                            if (readLine.StartsWith("Content-Disposition"))
                            {
                                foreach (string pName in readLine.Split(';').Select(s => s.Trim()))
                                {
                                    if (pName.StartsWith("name"))
                                    {
                                        paramName = pName.Trim().Replace("name=", "").Substring(1);
                                        paramName = paramName.Substring(0, paramName.Length - 1);
                                    }
                                    else if (pName.StartsWith("filename"))
                                    {
                                        paramFileName = pName.Trim().Replace("filename=", "").Substring(1);
                                        paramFileName = paramFileName.Substring(0, paramFileName.Length - 1);
                                    }

                                }
                            }
                            else if (readLine.StartsWith("Content-Type"))
                            {
                                paramContentType = readLine.Trim().Replace("Content-Type: ", "");
                            }
                        }
                        paramValue = readFormdata[1].Substring(0, readFormdata[1].LastIndexOf("\r\n"));
                        if (string.IsNullOrEmpty(paramName)) continue;
                        if (string.IsNullOrEmpty(paramFileName))
                        {
                            this.Parameter.Add(paramName, paramValue);
                        }
                        else
                        {
                            if (!this.Parameter.ContainsKey(FILE_PARAM_NAME))
                            {
                                this.Parameter.Add(FILE_PARAM_NAME, new List<Dictionary<string, object>>());
                            }
                            List<Dictionary<string, object>> fileList = this.Parameter[FILE_PARAM_NAME] as List<Dictionary<string, object>>;
                            fileList.Add(new Dictionary<string, object>(){
                                {"NAME",paramFileName},
                                {"CONTENT_TYPE",paramContentType},
                                //{"CONTENT",System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(paramValue.ToString())) }
                                {"FULLNAME",paramContentType}
                            });
                        }

                    }

                    return;
                }
                catch { }
            }
        }

        private void _QueryParameter(HttpRequest reqHttp)
        {
            if (reqHttp.InputStream.Length > 0)
            {
                StreamReader streamReader = null;
                string reqString = null;
                try
                {
                    streamReader = new StreamReader(reqHttp.InputStream);
                    reqString = streamReader.ReadToEnd();
                    _QueryParameter(reqString);
                }
                catch (Exception ex) { throw ex; }
            }
            else if (reqHttp.Form.Count > 0)
            {
                try
                {
                    this.Parameter = new Dictionary<string, object>();
                    for (int i = 0; i < reqHttp.Form.Count; i++)
                    {
                        this.Parameter.Add(reqHttp.Form.GetKey(i), reqHttp.Form[i]);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else if (reqHttp.QueryString.Count > 0)
            {
                try
                {
                    this.Parameter = new Dictionary<string, object>();
                    for (int i = 0; i < reqHttp.QueryString.Count; i++)
                    {
                        this.Parameter.Add(reqHttp.QueryString.GetKey(i), reqHttp.QueryString[i]);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private string GetBoundary(string reqString)
        {
            string boundary = string.Empty;
            try
            {
                string firstLine = reqString.Split('\n')[0];
                if (firstLine.StartsWith("--") && firstLine.EndsWith("\r"))
                {
                    boundary = firstLine.Replace("\r", "");
                }
            }
            catch { }
            return boundary;
        }

        public void Add(string paramName, object paramValue)
        {
            try
            {
                if (this.Parameter.ContainsKey(paramName))
                {
                    this.Parameter[paramName] = paramValue;
                }
                else
                {
                    this.Parameter.Add(paramName, paramValue);
                }
            }
            catch { }
        }

        public void Remove(string paramName)
        {
            try
            {
                if (this.Parameter == null) return;
                if (!this.Parameter.ContainsKey(paramName)) return;
                this.Parameter.Remove(paramName);
            }
            catch { }
        }

        public string ToJson(bool includeTextarea = false)
        {
            JavaScriptSerializer serializer = null;
            try
            {
                serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
                if (!includeTextarea)
                {
                    return serializer.Serialize(this.Parameter);
                }
                else
                {
                    return "<textarea>" + serializer.Serialize(this.Parameter) + "</textarea>";
                }
            }
            catch (Exception ex) { throw ex; }
        }
        public Stream ToStream(bool includeTextarea = false)
        {
            try
            {
                return new MemoryStream(this.ToBytes(includeTextarea));
            }
            catch (Exception ex) { throw ex; }
        }
        public byte[] ToBytes(bool includeTextarea = false)
        {
            try
            {
                return System.Text.Encoding.UTF8.GetBytes(this.ToJson(includeTextarea));
            }
            catch (Exception ex) { throw ex; }
        }


        private void DeleteOldFile(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fis = di.GetFiles("*.*");
            foreach (FileInfo fi in fis)
            {
                if (fi.LastWriteTime < DateTime.Today)
                {
                    fi.IsReadOnly = false;
                    fi.Delete();
                }
            }
        }

    }

    public class FileParameter
    {
        private FileInfo _File;
        private string _Name;

        public FileInfo File
        {
            get
            {
                return _File;
            }
        }
        public string Name
        {
            get
            {
                return _Name;
            }
        }

        public FileParameter(string name, string fullpath)
        {
            this._Name = name;
            this._File = new FileInfo(fullpath);
        }
        public void Save(string filepath)
        {
            try
            {
                string fullpath = System.IO.Path.Combine(filepath, this._Name);
                string directory = System.IO.Path.GetDirectoryName(fullpath);
                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }
                else if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                this._File.MoveTo(fullpath);
            }
            catch (Exception ex)
            {
                this._File.Delete();
                throw ex;
            }
        }
        public void Save(string filepath, string filename)
        {
            try
            {
                string fullpath = System.IO.Path.Combine(filepath, filename);
                string directory = System.IO.Path.GetDirectoryName(fullpath);
                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }
                else if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                this._File.MoveTo(fullpath);
            }
            catch (Exception ex)
            {
                this._File.Delete();
                throw ex;
            }
        }
    }
}