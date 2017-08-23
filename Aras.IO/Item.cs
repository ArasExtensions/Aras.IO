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

namespace Aras.IO
{
    public class Item
    {
        private static String[] SystemChildNodeNames = new String[] { "id", "config_id", "itemtype", "source_id", "related_id", "Relationships", "to_state" };

        internal XmlDocument Doc { get; private set; }

        internal XmlNode Node { get; private set; }

        internal byte[] GetBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes(this.Doc.OuterXml);
        }

        internal String GetString()
        {
            return this.Doc.OuterXml;
        }

        public String ID
        {
            get
            {
                XmlAttribute id = this.Node.Attributes["id"];

                if (id != null)
                {
                    return id.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute id = this.Node.Attributes["id"];

                if (id == null)
                {
                    id = this.Doc.CreateAttribute("id");
                    this.Node.Attributes.Append(id);
                }

                id.Value = value;
            }
        }

        public String ConfigID
        {
            get
            {
                return this.GetProperty("config_id");
            }
            set
            {
                this.SetProperty("config_id", value);
            }
        }

        public Int32 Generation
        {
            get
            {
                if (this.GetProperty("generation") == null)
                {
                    return 1;
                }
                else
                {
                    return Int32.Parse(this.GetProperty("generation", "1"));
                }
            }
        }

        public Boolean IsCurrent
        {
            get
            {
                return this.GetProperty("is_current", "0").Equals("1");
            }
        }

        public String ItemType
        {
            get
            {
                XmlAttribute type = this.Node.Attributes["type"];

                if (type != null)
                {
                    return type.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute type = this.Node.Attributes["type"];

                if (type == null)
                {
                    type = this.Doc.CreateAttribute("type");
                    this.Node.Attributes.Append(type);
                }

                type.Value = value;
            }
        }

        public String Action
        {
            get
            {
                XmlAttribute type = this.Node.Attributes["action"];

                if (type != null)
                {
                    return type.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute type = this.Node.Attributes["action"];

                if (type == null)
                {
                    type = this.Doc.CreateAttribute("action");
                    this.Node.Attributes.Append(type);
                }

                type.Value = value;
            }
        }

        public Boolean DoGetItem
        {
            get
            {
                XmlAttribute type = this.Node.Attributes["doGetItem"];

                if (type != null)
                {
                    return "1".Equals(type.Value);
                }
                else
                {
                    return true;
                }
            }
            set
            {
                XmlAttribute type = this.Node.Attributes["doGetItem"];

                if (type == null)
                {
                    type = this.Doc.CreateAttribute("doGetItem");
                    this.Node.Attributes.Append(type);
                }

                if (value)
                {
                    type.Value = "1";
                }
                else
                {
                    type.Value = "0";
                }
            }
        }

        public String Select
        {
            get
            {
                XmlAttribute select = this.Node.Attributes["select"];

                if (select != null)
                {
                    return select.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {

                XmlAttribute select = this.Node.Attributes["select"];

                if (select == null)
                {
                    select = this.Doc.CreateAttribute("select");
                    this.Node.Attributes.Append(select);
                }

                select.Value = value;
            }
        }

        public String OrderBy
        {
            get
            {
                XmlAttribute orderBy = this.Node.Attributes["orderBy"];

                if (orderBy != null)
                {
                    return orderBy.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                XmlAttribute orderBy = this.Node.Attributes["orderBy"];

                if (orderBy == null)
                {
                    orderBy = this.Doc.CreateAttribute("orderBy");
                    this.Node.Attributes.Append(orderBy);
                }

                orderBy.Value = value;
            }
        }

        public String Where
        {
            get
            {
                XmlAttribute where = this.Node.Attributes["where"];

                if (where != null)
                {
                    return where.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    XmlAttribute where = this.Node.Attributes["where"];

                    if (where == null)
                    {
                        where = this.Doc.CreateAttribute("where");
                        this.Node.Attributes.Append(where);
                    }

                    where.Value = value;
                }
            }
        }

        public int Page
        {
            get
            {
                XmlAttribute page = this.Node.Attributes["page"];

                if (page != null)
                {
                    return int.Parse(page.Value);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                XmlAttribute page = this.Node.Attributes["page"];

                if (page == null)
                {
                    page = this.Doc.CreateAttribute("page");
                    this.Node.Attributes.Append(page);
                }

                page.Value = value.ToString();
            }
        }

        public int PageSize
        {
            get
            {
                XmlAttribute pagesize = this.Node.Attributes["pagesize"];

                if (pagesize != null)
                {
                    return int.Parse(pagesize.Value);
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                XmlAttribute pagesize = this.Node.Attributes["pagesize"];

                if (pagesize == null)
                {
                    pagesize = this.Doc.CreateAttribute("pagesize");
                    this.Node.Attributes.Append(pagesize);
                }

                pagesize.Value = value.ToString();
            }
        }

        public int ItemMax
        {
            get
            {
                XmlAttribute itemmax = this.Node.Attributes["itemmax"];

                if (itemmax != null)
                {
                    return int.Parse(itemmax.Value);
                }
                else
                {
                    return 0;
                }
            }
        }

        public int PageMax
        {
            get
            {
                XmlAttribute pagemax = this.Node.Attributes["pagemax"];

                if (pagemax != null)
                {
                    return int.Parse(pagemax.Value);
                }
                else
                {
                    return 0;
                }
            }
        }

        public IEnumerable<String> PropertyNames
        {
            get
            {
                List<String> ret = new List<String>();

                foreach (XmlNode childnode in this.Node.ChildNodes)
                {
                    String name = childnode.Name;

                    if (!SystemChildNodeNames.Contains(name))
                    {
                        ret.Add(name);
                    }
                }

                return ret;
            }
        }

        public Boolean IsPropertyItem(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);
            return (propnode.FirstChild != null);
        }

        public String GetProperty(String Name, String Default)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlAttribute is_null = propnode.Attributes["is_null"];

                if (is_null != null && is_null.Value == "1")
                {
                    return null;
                }
                else
                {
                    return propnode.InnerText;
                }
            }
            else
            {
                return Default;
            }
        }

        public String GetProperty(String Name)
        {
            return this.GetProperty(Name, null);
        }

        public Item GetPropertyItem(String Name)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode != null)
            {
                XmlNode itemnode = propnode.SelectSingleNode("Item");

                if (itemnode != null)
                {
                    return new Item(this.Doc, itemnode);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public void SetProperty(String Name, String Value)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode == null)
            {
                propnode = this.Doc.CreateNode(XmlNodeType.Element, Name, null);
                this.Node.AppendChild(propnode);
            }

            propnode.InnerText = Value;
        }

        public void SetPropertyItem(String Name, Item Value)
        {
            XmlNode propnode = this.Node.SelectSingleNode(Name);

            if (propnode == null)
            {
                propnode = this.Doc.CreateNode(XmlNodeType.Element, Name, null);
                this.Node.AppendChild(propnode);
            }

            propnode.AppendChild(this.Doc.ImportNode(Value.Node, true));
        }

        internal XmlNode RelationshipsNode
        {
            get
            {
                XmlNode relsnode = this.Node.SelectSingleNode("Relationships");

                if (relsnode == null)
                {
                    relsnode = this.Doc.CreateNode(XmlNodeType.Element, "Relationships", null);
                    this.Node.AppendChild(relsnode);
                }

                return relsnode;
            }
        }

        public void AddRelationship(Item Relationship)
        {
            XmlNode rel = this.Doc.ImportNode(Relationship.Node, true);
            this.RelationshipsNode.AppendChild(rel);
        }

        public Item NewRelationship(String ItemType, String Action)
        {
            Item relationship = new Item(ItemType, Action);
            this.AddRelationship(relationship);
            return relationship;
        }

        public IEnumerable<Item> Relationships
        {
            get
            {
                List<Item> ret = new List<Item>();

                if (this.RelationshipsNode != null)
                {
                    foreach (XmlNode relnode in this.RelationshipsNode.ChildNodes)
                    {
                        ret.Add(new Item(this.Doc, relnode));
                    }
                }

                return ret;
            }
        }

        public IEnumerable<Item> ToStates
        {
            get
            {
                List<Item> tostates = new List<Item>();

                foreach(XmlNode tostatenode in this.Node.SelectNodes("to_state"))
                {
                    XmlNode itemnode = tostatenode.SelectSingleNode("Item");
                    Item tostate = new Item(this.Doc, itemnode);
                    tostates.Add(tostate);
                }

                return tostates;
            }
        }

        public override string ToString()
        {
            return this.Node.OuterXml;
        }

        internal Item(XmlDocument Doc, XmlNode Node)
        {
            this.Doc = Doc;
            this.Node = Node;
        }

        public Item(String ItemType, String Action)
        {
            // Create XML Document
            this.Doc = new XmlDocument();

            // Create Item Node
            this.Node = this.Doc.CreateNode(XmlNodeType.Element, "Item", null);
            this.Doc.AppendChild(this.Node);

            // Set ItemType
            this.ItemType = ItemType;

            // Set Action
            this.Action = Action;
        }
    }
}
