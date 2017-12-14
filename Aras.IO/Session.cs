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
using System.Xml;
using System.IO;
using System.Web;
using System.Net.Http;

namespace Aras.IO
{
    public class Session
    {
        private const int bufferlength = 4096;

        public Database Database { get; private set; }

        internal CookieContainer Cookies { get; private set; }

        public String Username { get; private set; }

        public String Password { get; private set; }

        public String UserID { get; private set; }

        private String _vaultID;
        public String VaultID
        {
            get
            {
                if (this._vaultID == null)
                {
                    Request request = this.Request(IO.Request.Operations.ApplyItem);
                    Item user = request.NewItem("User", "get");
                    user.Select = "default_vault";
                    user.ID = this.UserID;
                    Response response = request.Execute();

                    if (!response.IsError)
                    {
                        this._vaultID = response.Items.First().GetProperty("default_vault");
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }

                return this._vaultID;
            }
        }

        private String _vaultBaseURL;
        public String VaultBaseURL
        {
            get
            {
                if (this._vaultBaseURL == null)
                {
                    Request request = this.Request(IO.Request.Operations.ApplyItem);
                    Item vault = request.NewItem("Vault", "get");
                    vault.Select = "vault_url";
                    vault.ID = this.VaultID;
                    Response response = request.Execute();

                    if (!response.IsError)
                    {
                        this._vaultBaseURL = response.Items.First().GetProperty("vault_url");
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }

                return this._vaultBaseURL;
            }
        }

        public String UserType { get; private set; }

        public Request Request(Request.Operations Operation)
        {
            return new Request(Operation, this);
        }

        public Request Request(Request.Operations Operation, Item Item)
        {
            Request request = this.Request(Operation);
            request.AddItem(Item);
            return request;
        }

        private Random Random;
        private Double DownloadRandom()
        {
            return this.Random.NextDouble();
        }

        private String DownloadToken(String ID)
        {

            byte[] buffer = new byte[bufferlength];
            int read = 0;

            String url = this.Database.Server.AuthenticationBrokerURL + "/GetFileDownloadToken?rnd=" + this.DownloadRandom().ToString();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = this.Cookies;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Headers.Add("AUTHPASSWORD", this.Password);
            request.Headers.Add("AUTHUSER", this.Username);
            request.Accept = "application/json; charset=utf-8";
            request.Headers.Add("DATABASE", this.Database.ID);
            request.Headers.Add("SOAPACTION", "GetFileDownloadToken");
            request.Headers.Add("TIMEZONE_NAME", "GMT Standard Time");

            String body = "{\"param\":{\"fileId\":\"" + ID + "\"}}";
            byte[] bodybytes = System.Text.Encoding.ASCII.GetBytes(body);

            request.ContentLength = bodybytes.Length;

            using (Stream poststream = request.GetRequestStream())
            {
                poststream.Write(bodybytes, 0, bodybytes.Length);
            }

            using (HttpWebResponse webresponse = (HttpWebResponse)request.GetResponse())
            {
                // Store Cookies
                this.Cookies.Add(webresponse.Cookies);

                using (Stream result = webresponse.GetResponseStream())
                {
                    String resultstring = "";

                    while ((read = result.Read(buffer, 0, bufferlength)) > 0)
                    {
                        resultstring = resultstring + Encoding.UTF8.GetString(buffer, 0, read);
                    }

                    return resultstring.Substring(6, resultstring.Length - 8);
                }
            }
        }

        private Dictionary<String, String> URLCache;
        public String VaultURL(String ID, String Filename)
        {
            if (this.URLCache.ContainsKey(ID))
            {
                this.URLCache[ID] = this.VaultBaseURL + "?dbname=" + this.Database.ID + "&fileId=" + ID + "&fileName=" + HttpUtility.UrlEncode(Filename) + "&vaultId=" + this.VaultID + "&token=" + this.DownloadToken(ID);
            }

            return this.URLCache[ID];
        }

        public void VaultRead(String ID, String Filename, Stream Output)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.VaultURL(ID, Filename));
            request.CookieContainer = this.Cookies;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream responsestream = response.GetResponseStream())
                {
                    responsestream.CopyTo(Output);
                }
            }
        }

        public Response VaultWrite(Stream Stream, String Filename)
        {
            IO.Response response = null;

            // Read Cached File
            byte[] filebytes = null;

            if (Stream is MemoryStream)
            {
                filebytes = ((MemoryStream)Stream).ToArray();
            }
            else
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    Stream.CopyTo(ms);
                    filebytes = ms.ToArray();
                }
            }

            // Build Request
            String contentboundary = "-------------S36Ut9A3ZtWwum";
            MultipartFormDataContent content = new MultipartFormDataContent(contentboundary);

            StringContent soapaction = new StringContent("ApplyItem");
            content.Add(soapaction, "SOAPACTION");

            StringContent authuser = new StringContent(this.Username);
            content.Add(authuser, "AUTHUSER");

            StringContent password = new StringContent(this.Password);
            content.Add(password, "AUTHPASSWORD");

            StringContent database = new StringContent(this.Database.ID);
            content.Add(database, "DATABASE");

            StringContent locale = new StringContent("");
            content.Add(locale, "LOCALE");

            StringContent timezone = new StringContent("GMT Standard Time");
            content.Add(timezone, "TIMEZONE_NAME");

            StringContent vault = new StringContent(this.VaultID);
            content.Add(vault, "VAULTID");

            IO.Item dbfile = new IO.Item("File", "add");
            dbfile.ID = Server.NewID();
            dbfile.SetProperty("filename", Filename);
            dbfile.SetProperty("file_size", filebytes.Length.ToString());
            IO.Item dbloacted = new IO.Item("Located", "add");
            dbloacted.SetProperty("related_id", this.VaultID);
            dbfile.AddRelationship(dbloacted);

            String xmldata = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><ApplyItem>" + dbfile.GetString() + "</ApplyItem></SOAP-ENV:Body></SOAP-ENV:Envelope>";
            StringContent xml = new StringContent(xmldata);
            content.Add(xml, "XMLdata");

            ByteArrayContent filedata = new ByteArrayContent(filebytes);
            content.Add(filedata, dbfile.ID, Filename);

            // Post Request to Vault Server
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.VaultBaseURL);
            request.CookieContainer = this.Cookies;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentType = "multipart/form-data; boundary=" + contentboundary;
            request.Method = "POST";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Headers.Add("Cache-Control", "no-cache");
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            using (Stream poststream = request.GetRequestStream())
            {
                content.CopyToAsync(poststream);
            }

            using (HttpWebResponse webresponse = (HttpWebResponse)request.GetResponse())
            {
                using (Stream result = webresponse.GetResponseStream())
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(result);
                    response = new IO.Response(webresponse.Cookies, doc);
                }
            }

            return response;
        }

        internal Session(Database Database, XmlNode Node , String Username, String Password, CookieContainer Cookies)
        {
            this.Random = new Random();
            this.URLCache = new Dictionary<String, String>();
            this.Database = Database;
            this.Username = Username;
            this.Password = Password;
            this.Cookies = Cookies;
            this.UserID = Node.SelectSingleNode("id").InnerText;
            this.UserType = Node.SelectSingleNode("user_type").InnerText;
        }
    }
}
