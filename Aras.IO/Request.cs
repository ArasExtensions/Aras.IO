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
using System.Xml;
using System.Net;
using System.IO;

namespace Aras.IO
{
    public class Request
    {
        public enum Operations { ValidateUser, ApplyItem, ApplyAML, GetItemNextStates, PromoteItem };

        public Operations Operation { get; private set; }

        public Server Server { get; private set; }

        public Database Database { get; private set; }

        public String Username { get; private set; }

        public String AccessToken { get; private set; }

        internal CookieContainer Cookies { get; private set; }

        private List<Item> ItemsCache;

        public IEnumerable<Item> Items
        {
            get
            {
                return this.ItemsCache;
            }
        }

        public Item NewItem(String ItemType, String Action)
        {
            Item item = new Item(ItemType, Action);
            this.ItemsCache.Add(item);
            return item;
        }

        public void AddItem(Item Item)
        {
            this.ItemsCache.Add(Item);
        }

        private HttpWebRequest _hTTPRequest;
        private HttpWebRequest HTTPRequest
        {
            get
            {
                if (this._hTTPRequest == null)
                {
                    this._hTTPRequest = (HttpWebRequest)WebRequest.Create(this.Database.Server.ApiURL);
                    this._hTTPRequest.CookieContainer = this.Cookies;
                    this._hTTPRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    this._hTTPRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    this._hTTPRequest.Headers.Add("Cache-Control", "no-cache");
                    this._hTTPRequest.Method = "POST";
                    this._hTTPRequest.ContentType = "text/xml; charset=utf-8";
                    this._hTTPRequest.Headers.Add("Authorization", "Bearer " + this.AccessToken);
                    this._hTTPRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    this._hTTPRequest.Headers.Add("DATABASE", this.Database.ID);
                    this._hTTPRequest.Headers.Add("SOAPACTION", this.Operation.ToString());
                    this._hTTPRequest.Headers.Add("TIMEZONE_NAME", "GMT Standard Time");

                    // Get bytes for SOAP Header
                    String headerstring = "<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\"><SOAP-ENV:Body><" + this.Operation.ToString() + ">";

                    if (this.Operation == Operations.ApplyAML)
                    {
                        headerstring += "<AML>";
                    }

                    byte[] header = System.Text.Encoding.ASCII.GetBytes(headerstring);

                    // Get bytes for SOAP Data
                    List<byte[]> datalist = new List<byte[]>();
                    int datalength = 0;

                    if (this.Items != null)
                    {
                        foreach (Item item in this.Items)
                        {
                            byte[] thisdata = item.GetBytes();
                            datalength += thisdata.Length;
                            datalist.Add(thisdata);
                        }
                    }

                    // Get Bytes for SOAP Footer
                    String footerstring = "</" + this.Operation.ToString() + "></SOAP-ENV:Body></SOAP-ENV:Envelope>";

                    if (this.Operation == Operations.ApplyAML)
                    {
                        footerstring = "</AML>" + footerstring;
                    }

                    byte[] footer = System.Text.Encoding.ASCII.GetBytes(footerstring);

                    // Write SOAP Message to Request and update length
                    this._hTTPRequest.ContentLength = header.Length + datalength + footer.Length;

                    using (Stream poststream = this._hTTPRequest.GetRequestStream())
                    {
                        poststream.Write(header, 0, header.Length);

                        foreach (byte[] data in datalist)
                        {
                            poststream.Write(data, 0, data.Length);
                        }

                        poststream.Write(footer, 0, footer.Length);
                    }
                }

                return this._hTTPRequest;
            }
        }

        public Response Execute()
        {
            try
            {
                using (HttpWebResponse webresponse = (HttpWebResponse)this.HTTPRequest.GetResponse())
                {
                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new Response(webresponse.Cookies, doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        public async Task<Response> ExecuteAsync()
        {
            try
            {
                using (Task<WebResponse> task = this.HTTPRequest.GetResponseAsync())
                {
                    WebResponse webresponse = await task;

                    using (Stream result = webresponse.GetResponseStream())
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(result);
                        return new Response(((HttpWebResponse)webresponse).Cookies, doc);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exceptions.ServerException("Unable to connect to Server", ex);
            }
        }

        internal Request(Operations Operation, Session Session)
        {
            this.Operation = Operation;
            this.Server = Session.Database.Server;
            this.Database = Session.Database;
            this.Username = Session.Username;
            this.AccessToken = Session.AccessToken;
            this.ItemsCache = new List<Item>();
            this.Cookies = Session.Cookies;
        }

        internal Request(Operations Operation, Database Database, String Username, String AccessToken)
        {
            this.Operation = Operation;
            this.Server = Database.Server;
            this.Database = Database;
            this.Username = Username;
            this.AccessToken = AccessToken;
            this.ItemsCache = new List<Item>();
            this.Cookies = new CookieContainer();
        }
    }
}
