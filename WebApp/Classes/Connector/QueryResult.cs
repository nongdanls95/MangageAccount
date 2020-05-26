using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for Connector
/// </summary>
namespace WebApp.Classes.Connector
{
    public class QueryResult
    {

        private bool _success = true;
        private int _total = -1;
        private string _message = string.Empty;
        private DataTable _dataTable = null;
        private Dictionary<string, object> _outputParameters = null;
        private Dictionary<string, object> _ntParameters = null;

        private Dictionary<string, object> _serializeObject = null;
        public QueryResult()
        {
            this._dataTable = new DataTable();
            this._outputParameters = new Dictionary<string, object>();
        }
        public QueryResult(DataTable data)
        {
            this._dataTable = data;
            this._outputParameters = new Dictionary<string, object>();
        }
        public QueryResult(string reqString)
        {
            JavaScriptSerializer serializer = null;
            try
            {
                serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
                this._outputParameters = new Dictionary<string, object>();
                this._outputParameters = serializer.Deserialize<Dictionary<string, object>>(reqString);
                if (this._outputParameters.ContainsKey("success") && this._outputParameters["success"] != null)
                {
                    this.Success = bool.Parse(this._outputParameters["success"].ToString());
                }
                if (this._outputParameters.ContainsKey("message") && this._outputParameters["message"] != null)
                {
                    this.Message = this._outputParameters["message"].ToString();
                }
            }
            catch { }
        }
        public QueryResult(Stream reqStream)
            : this(new StreamReader(reqStream).ReadToEnd())
        {
        }
        public QueryResult(Exception ex)
        {
            Exception exception = ex.GetBaseException();
            exception = exception == null ? ex : exception;
            this._dataTable = new DataTable();
            this._outputParameters = new Dictionary<string, object>();
            //try
            //{
            //    this._outputParameters.Add("error", new Dictionary<string, object>()
            //    {
            //        {"source",exception.Source},
            //        {"stackTrace",exception.StackTrace},
            //        {"hresult",exception.HResult}
            //    });
            //}
            //catch { }
            this._message = exception.Message;
            this._success = false;
            this._total = -1;
        }

        public void AddOutputParam(string paramName, object value)
        {
            try
            {
                if (this._outputParameters == null)
                    this._outputParameters = new Dictionary<string, object>();
                this._outputParameters.Add(paramName, value);
            }
            catch { }
        }
        public void RemoveOutputParam(string paramName)
        {
            try
            {
                if (this._outputParameters == null) return;
                this._outputParameters.Remove(paramName);
            }
            catch { }
        }

        public void AddNTParam(string paramName, object value)
        {
            try
            {
                if (this._ntParameters == null)
                    this._ntParameters = new Dictionary<string, object>();
                this._ntParameters.Add(paramName, value);
            }
            catch { }
        }

        public void RemoveNTParam(string paramName)
        {
            try
            {
                if (this._ntParameters == null) return;
                this._ntParameters.Remove(paramName);
            }
            catch { }
        }

        public string ToJson(bool includeTextarea = false)
        {
            JavaScriptSerializer serializer = null;
            try
            {
                this.ToDictionary();
                serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
                if (!includeTextarea)
                {
                    return serializer.Serialize(this._serializeObject);
                }
                else
                {
                    return "<textarea>" + serializer.Serialize(this._serializeObject) + "</textarea>";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
        public Dictionary<string, object> ToDictionary()
        {
            try
            {
                this._serializeObject = new Dictionary<string, object>();
                this._serializeObject.Add("success", this.Success);
                if (this.Total == 0 || this.Total == -1 && this._dataTable != null)
                {
                    this._serializeObject.Add("total", this._dataTable.Rows.Count);
                }
                else
                {
                    this._serializeObject.Add("total", this.Total);
                }
                this._serializeObject.Add("message", this.Message);
                this._serializeObject.Add("data", ConnectorUtil.DataTableToDictionary(this._dataTable));
                foreach (var param in OutputParameters)
                {
                    if (this._serializeObject.ContainsKey(param.Key))
                    {
                        this._serializeObject[param.Key] = param.Value;
                    }
                    else
                    {
                        this._serializeObject.Add(param.Key, param.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                this._serializeObject = new Dictionary<string, object>();
                this._serializeObject.Add("success", false);
                this._serializeObject.Add("total", -1);
                this._serializeObject.Add("message", "ไม่สามารถสร้าง Dictionary ได้เนื่องจาก : " + ex.Message);
                this._serializeObject.Add("data", null);
                //this._serializeObject.Add("error", new Dictionary<string, object>()
                //{
                //    {"source",ex.Source},
                //    {"stackTrace",ex.StackTrace},
                //    {"hresult",ex.HResult}
                //});
            }
            return this._serializeObject;
        }

        public bool Success
        {
            get
            {
                return _success;
            }
            set
            {
                _success = value;
            }
        }
        public int Total
        {
            get
            {
                return _total;
            }
            set
            {
                _total = value;
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        public DataTable DataTable
        {
            get
            {
                return _dataTable;
            }
            set
            {
                _dataTable = value;
            }
        }
        public Dictionary<string, object> OutputParameters
        {
            get
            {
                return _outputParameters;
            }
        }
        public Dictionary<string, object> NTParameters
        {
            get
            {
                return _ntParameters;
            }
        }
    }
}