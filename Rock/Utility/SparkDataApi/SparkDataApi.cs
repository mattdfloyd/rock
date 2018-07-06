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
using Rock.Utility.NcoaApi;
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
        /// Spark Data account status
        /// </summary>
        public enum AccountStatus
        {
            EnabledCard,
            EnabledNoCard,
            EnabledCardExpired,
            Disabled,
            AccountNoName,
            AccountNotFound,
            InvalidSparkDataKey
        }

        /// <summary>
        /// Checks if the account is valid on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        public AccountStatus CheckAccount( string sparkDataApiKey )
        {
            try
            {
                var request = new RestRequest( "api/SparkData/ValidateAccount", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter( "sparkDataApiKey", sparkDataApiKey );
                var response = _client.Get<AccountStatus>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
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

        public string GetPrice( string service )
        {
            try
            {
                var request = new RestRequest( "api/SparkData/GetPrice", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter( "service", service );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Content.Trim('"');
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
                throw new AggregateException( "Could not get price of service from Spark Server", ex );
            }
        }

        #region NCOA

        /// <summary>
        /// Initiates the report on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        public GroupNameTransactionKey NcoaIntiateReport( string sparkDataApiKey, int? numberRecords, int? personAliasId = null )
        {
            try
            {
                string url;
                if ( personAliasId.HasValue )
                {
                    url = $"api/SparkData/Ncoa/IntiateReport/{sparkDataApiKey}/{numberRecords ?? 0}/{personAliasId.Value}";
                }
                else
                {
                    url = $"api/SparkData/Ncoa/IntiateReport/{sparkDataApiKey}/{numberRecords ?? 0}";
                }

                var request = new RestRequest( url, Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                var response = _client.Post<GroupNameTransactionKey>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
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
                throw new AggregateException( "Could not initiate Spark report", ex );
            }
        }

        /// <summary>
        /// Gets the credentials from the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <returns>The username and password</returns>
        public UsernamePassword NcoaGetCredentials( string sparkDataApiKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/GetCredentials/{sparkDataApiKey}", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                var response = _client.Get<UsernamePassword>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
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
                throw new AggregateException( "Could not get credentials from Spark", ex );
            }
        }

        /// <summary>
        /// Sent Complete report message to Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="reportKey">The report key.</param>
        /// <param name="exportFileKey">The export file key.</param>
        /// <returns>Return true if successful</returns>
        public bool NcoaCompleteReport( string sparkDataApiKey, string reportKey, string exportFileKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/CompleteReport/{sparkDataApiKey}/{reportKey}/{exportFileKey}", Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                // IRestResponse response = client.Execute( request );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Content.AsBoolean();
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
                throw new AggregateException( "Could not complete Spark report", ex );
            }
        }

        /// <summary>
        /// Send a failed message to Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="reportKey">The report key.</param>
        /// <returns>Return true if successful</returns>
        public bool NcoaCompleteFailed( string sparkDataApiKey, string reportKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/CompleteFailed/{sparkDataApiKey}/{reportKey}", Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                // IRestResponse response = client.Execute( request );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Content.AsBoolean();
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
                throw new AggregateException( "Could not send Spark report failed", ex );
            }
        }

        #endregion
    }
}
