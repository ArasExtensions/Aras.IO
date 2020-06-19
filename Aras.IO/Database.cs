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

namespace Aras.IO
{
    public class Database
    {
        public Server Server { get; private set; }

        public String ID { get; private set; }

        private object SessionCacheLock = new object();
        private Dictionary<String, Session> SessionCache;

        public Session Login(String Username, String AccessToken)
        {
            lock (this.SessionCacheLock)
            {
                if (!this.SessionCache.ContainsKey(Username))
                {
                    IO.Request request = new IO.Request(IO.Request.Operations.ValidateUser, this, Username, AccessToken);
                    IO.Response response = request.Execute();

                    if (!response.IsError)
                    {
                        this.SessionCache[Username] = new Session(this, response.Result, Username, AccessToken);
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response);
                    }
                }
                else
                {
                    // Check Password
                    if (!this.SessionCache[Username].AccessToken.Equals(AccessToken))
                    {
                        throw new Exceptions.ArgumentException("Invalid Password");
                    }
                }

                return this.SessionCache[Username];
            }
        }

        public override String ToString()
        {
            return this.ID;
        }

        internal Database(Server Server, String ID)
        {
            this.SessionCache = new Dictionary<String, Session>();

            // Store Server
            this.Server = Server;

            // Store ID
            this.ID = ID;
        }
    }
}
