/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Xml;

namespace Aras.IO
{
    public class Server
    {
        private String _uRL;
        public String URL
        {
            get
            {
                return this._uRL;
            }
            private set
            {
                if (value != null)
                {
                    try
                    {
                        this._uRL = new Uri(value).AbsoluteUri;
                    }
                    catch (Exception e)
                    {
                        throw new Exceptions.ArgumentException("Invalid URL Format: " + value, e);
                    }
                }
                else
                {
                    throw new Exceptions.ArgumentException("Invalid URL: null");
                }
            }
        }

        private String _proxyURL;
        public String ProxyURL
        {
            get
            {
                return this._proxyURL;
            }
            set
            {
                if (value != null)
                {
                    try
                    {
                        this._proxyURL = value;
                        WebRequest.DefaultWebProxy = new WebProxy(this._proxyURL);
                    }
                    catch (Exception e)
                    {
                        WebRequest.DefaultWebProxy = null;
                        throw new Exceptions.ArgumentException("Invalid Proxy: " + value, e);
                    }
                }
                else
                {
                    this._proxyURL = null;
                    WebRequest.DefaultWebProxy = null;
                }
            }
        }

        private String _serverURL;
        public String ServerURL
        {
            get
            {
                if (this._serverURL == null)
                {
                    this._serverURL = this.URL + "/Server";
                }

                return this._serverURL;
            }
        }

        private String _apiURL;
        public String ApiURL
        {
            get
            {
                if (this._apiURL == null)
                {
                    this._apiURL = this.ServerURL + "/InnovatorServer.aspx";
                }

                return this._apiURL;
            }
        }

        private String _authenticationBrokerURL;
        public String AuthenticationBrokerURL
        {
            get
            {
                if (this._authenticationBrokerURL == null)
                {
                    this._authenticationBrokerURL = this.ServerURL + "/AuthenticationBroker.asmx";
                }

                return this._authenticationBrokerURL;
            }
        }

        private String _dBListURL;
        public String DBListURL
        {
            get
            {
                if (this._dBListURL == null)
                {
                    this._dBListURL = this.ServerURL + "/dblist.aspx";
                }

                return this._dBListURL;
            }
        }

        private String _clientURL;
        public String ClientURL
        {
            get
            {
                if (this._clientURL == null)
                {
                    this._clientURL = this.URL + "/Client";
                }

                return this._clientURL;
            }
        }

        private String _javascriptClientURL;
        public String JavascriptClientURL
        {
            get
            {
                if (this._javascriptClientURL == null)
                {
                    this._javascriptClientURL = this.ClientURL + "/javascript";
                }

                return this._javascriptClientURL;
            }
        }

        public static String PasswordHash(String Password)
        {
            String md5password = null;

            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(Password));
                StringBuilder md5string = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    md5string.Append(data[i].ToString("x2"));
                }

                md5password = md5string.ToString();
            }

            return md5password;
        }

        private object _databasesCacheLock = new object();
        private Dictionary<String, Database> _databasesCache;
        private Dictionary<String, Database> DatabaseCache
        {
            get
            {
                lock (this._databasesCacheLock)
                {
                    if (this._databasesCache == null)
                    {
                        this._databasesCache = new Dictionary<String, Database>();

                        try
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.DBListURL);
                            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                            request.Headers.Add("Cache-Control", "no-cache");

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                using (Stream result = response.GetResponseStream())
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.Load(result);
                                    XmlNode dblist = doc.SelectSingleNode("DBList");

                                    foreach (XmlNode db in dblist.ChildNodes)
                                    {
                                        String id = db.Attributes["id"].Value;
                                        this._databasesCache[id] = new Database(this, id);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exceptions.ServerException("Unable to connect to Server", ex);
                        }
                    }
                }

                return this._databasesCache;
            }
        }

        public IEnumerable<Database> Databases
        {
            get
            {
                return this.DatabaseCache.Values;
            }
        }

        public Database Database(String ID)
        {
            if (this.DatabaseCache.ContainsKey(ID))
            {
                return this.DatabaseCache[ID];
            }
            else
            {
                throw new Exceptions.ArgumentException("Invalid Database ID: " + ID);
            }
        }

        public static String NewID()
        {
            StringBuilder ret = new StringBuilder(32);

            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }

            return ret.ToString();
        }

        public override string ToString()
        {
            return this.URL;
        }

        public Server(String URL)
        {
            // Store URL
            this.URL = URL;
        }
    }
}
