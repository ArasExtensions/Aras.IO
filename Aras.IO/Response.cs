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

namespace Aras.IO
{
    public class Response
    {
        internal CookieContainer Cookies { get; private set; }

        public XmlDocument Doc { get; private set; }

        public XmlNamespaceManager Namespaces { get; private set; }

        public XmlNode Result
        {
            get
            {
                return this.Doc.SelectSingleNode(".//Result");
            }
        }

        public IEnumerable<Item> Items
        {
            get
            {
                List<Item> ret = new List<Item>();

                if (this.Result != null)
                {
                    XmlNodeList itemnodes = this.Result.SelectNodes("Item");

                    if (itemnodes != null)
                    {
                        foreach (XmlNode itemnode in itemnodes)
                        {
                            ret.Add(new Item(this.Doc, itemnode));
                        }
                    }
                }
                else
                {
                    XmlNodeList itemnodes = this.Doc.SelectNodes("Item");

                    if (itemnodes != null)
                    {
                        foreach (XmlNode itemnode in itemnodes)
                        {
                            ret.Add(new Item(this.Doc, itemnode));
                        }
                    }
                }

                return ret;
            }
        }

        internal XmlNode Fault
        {
            get
            {
                return this.Doc.SelectSingleNode(".//SOAP-ENV:Fault", this.Namespaces);
            }
        }

        public Boolean IsError
        {
            get
            {
                if (this.Fault != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public String ErrorMessage
        {
            get
            {
                if (this.Fault != null)
                {
                    XmlNode faultstring = this.Fault.SelectSingleNode(".//faultstring", this.Namespaces);

                    if (faultstring != null)
                    {
                        return faultstring.InnerText;
                    }
                    else
                    {
                        XmlNode exception = this.Fault.SelectSingleNode(".//af:exception", this.Namespaces);
                        return exception.Attributes["message"].Value;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        internal Response(CookieCollection Cookies, XmlDocument Doc)
        {
            // Store Cookies
            this.Cookies = new CookieContainer();
            this.Cookies.Add(Cookies);

            // Store XML
            this.Doc = Doc;
            this.Namespaces = new XmlNamespaceManager(this.Doc.NameTable);
            this.Namespaces.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");
            this.Namespaces.AddNamespace("af", "http://www.aras.com/InnovatorFault");
        }
    }
}
