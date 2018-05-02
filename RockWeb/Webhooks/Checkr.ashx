<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.Checkr" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Net;

using Rock;
using Rock.Data;
using Rock.Model;

namespace RockWeb.Webhooks
{
    /// <summary>
    /// Handles the background check results sent from Checkr
    /// </summary>
    public class Checkr : IHttpHandler
    {
        public void ProcessRequest( HttpContext context )
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid request type." );
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            try
            {
                var rockContext = new Rock.Data.RockContext();

                if ( !request.UserAgent.StartsWith( "Checkr-Webhook/" ) )
                {
                    response.Write( "Invalid User-Agent." );
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                string postedData = string.Empty;
                using ( var reader = new StreamReader( request.InputStream ) )
                {
                    postedData = reader.ReadToEnd();
                }

                if ( !Rock.Checkr.Checkr.SaveWebhookResults( postedData ) )
                {
                    response.Write( "Invalid Data." );
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                try
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch { }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, context );
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