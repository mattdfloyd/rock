// <copyright>
// Copyright by the Spark Development Network
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
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RestSharp;
using Rock.Utility.Settings.SparkData;

namespace Rock.Utility.SparkDataApi
{
    // API Calls to Spark Data server.
    public class SparkDataApi
    {
        internal RestClient _client;

        public SparkDataApi()
        {
            _client = new RestClient( SparkDataConfig.SPARK_SERVER );
        }

        /// <summary>
        /// Checks if the account is valid on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        public void CheckAccount( string sparkDataApiKey )
        {
            try
            {
                var request = new RestRequest( "api/SparkData/ValidateAccount", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter( "sparkDataApiKey", sparkDataApiKey );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    if ( !response.Content.AsBoolean() )
                    {
                        throw new UnauthorizedAccessException( "No valid credit card found in Spark account" );
                    }
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Could not authenticate Spark Data account", ex );
            }
        }
    }
}
