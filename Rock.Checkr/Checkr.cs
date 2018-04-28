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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using Rock.Attribute;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Checkr.Constants;
using Rock.Checkr.SystemKey;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Rock.Checkr
{
    /// <summary>
    /// Checkr Background Check 
    /// </summary>
    [Description( "Checkr Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "Checkr" )]

    [UrlLinkField( "Request URL", "The Checkr URL to send requests to.", true, "https://services.priorityresearch.com/webservice/default.cfm", "", 0 )]
    [UrlLinkField( "Return URL", "The Web Hook URL for Checkr to send results to (e.g. 'http://www.mysite.com/Webhooks/ProtectMyMinistry.ashx').", true, "", "", 1 )]
    public class Checkr : BackgroundCheckComponent
    {
        const string LOGIN_URL = "https://api.checkr.com";
        const string CANIDATES_URL = "https://api.checkr.com/v1/candidates";
        const string REPORT_URL = "https://api.checkr.com/v1/reports";
        const string PACKAGES_URL = "https://api.checkr.com/v1/packages";
        const string DOCUMENT_URL = "https://api.checkr.com/v1/documents";
        #region Utilities

        /// <summary>
        /// Creates the rest client.
        /// </summary>
        /// <param name="url">The Rest Client URL.</param>
        /// <param name="parameters">Dictionary of the parameters to pass to the Rest Client.</param>
        /// <returns></returns>
        private static IRestResponse RestClientPost( string url, Dictionary<string, object> parameters, string username = "", string password = "" )
        {
            IRestResponse restResponse = null;
            var restClient = new RestClient( url );
            if ( username.IsNotNullOrWhitespace() )
            {
                restClient.Authenticator = new HttpBasicAuthenticator( username, password );
            }

            var restRequest = new RestRequest( Method.POST );
            foreach ( var parameter in parameters )
            {
                restRequest.AddParameter( parameter.Key, parameter.Value );
            }

            restResponse = restClient.Execute( restRequest );
            return restResponse;
        }

        private static IRestResponse RestClientGet( string url, string username = "", string password = "" )
        {
            IRestResponse restResponse = null;
            var restClient = new RestClient( url );
            if ( username.IsNotNullOrWhitespace() )
            {
                restClient.Authenticator = new HttpBasicAuthenticator( username, password );
            }

            var restRequest = new RestRequest();
            restResponse = restClient.Execute( restRequest );
            return restResponse;
        }

        #endregion

        private bool CreateCandidate( Person person, string token, out string candidateId, List<string> errorMessages )
        {
            var restResponse = Checkr.RestClientPost(
                CANIDATES_URL,
                new Dictionary<string, object>()
                {
                    { "first_name", person.FirstName },
                    { "middle_name", person.MiddleName },
                    { "no_middle_name", person.MiddleName.IsNullOrWhiteSpace() },
                    { "last_name", person.LastName },
                    { "email", person.Email }
                }, token );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                string errorMessage = "Failed to authorize Checkr Account. Check Checkr Access Token in 'System Settings', 'Checkr' ";
                errorMessages.Add( errorMessage );
                candidateId = null;
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                string errorMessage = "Failed to create Checkr Canidate: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                candidateId = null;
                return false;
            }
            else
            {
                dynamic response = JObject.Parse( restResponse.Content );
                candidateId = response.id;
                return true;
            }
        }

        private bool CreateReport( string candidateId, RockContext rockContext, Model.Workflow workflow, CacheAttribute requestTypeAttribute, string token, out string reportId, List<string> errorMessages )
        {
            CacheDefinedValue pkgTypeDefinedValue = null;
            string packageName = "tasker_pro";

            if ( requestTypeAttribute == null )
            {
                string errorMessage = "Failed to retrieve Checkr Package Type Attribute";
                errorMessages.Add( errorMessage );
                reportId = null;
                return false;
            }

            pkgTypeDefinedValue = CacheDefinedValue.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
            if ( pkgTypeDefinedValue == null )
            {
                string errorMessage = "Failed to retrieve Checkr Package Type Defined Value";
                errorMessages.Add( errorMessage );
                reportId = null;
                return false;
            }

            if ( pkgTypeDefinedValue.Attributes == null )
            {
                pkgTypeDefinedValue.LoadAttributes( rockContext );
            }

            packageName = pkgTypeDefinedValue.GetAttributeValue( "PMMPackageName" );
            var restResponse = Checkr.RestClientPost(
                REPORT_URL,
                new Dictionary<string, object>()
                {
                    { "package", packageName },
                    { "candidate_id", candidateId }
                }, token );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                string errorMessage = "Failed to authorize Checkr Account. Check Checkr Access Token in 'System Settings', 'Checkr' ";
                errorMessages.Add( errorMessage );
                reportId = null;
                return false;
            }

            if ( restResponse.StatusCode == HttpStatusCode.BadRequest )
            {
                string errorMessage = "Failed to create Checkr Report: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                reportId = null;
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.Created )
            {
                string errorMessage = "Failed to create Checkr Report: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                reportId = null;
                return false;
            }
            else
            {
                dynamic response = JObject.Parse( restResponse.Content );
                reportId = response.id;
                return true;
            }
        }

        private static bool GetReport( string reportId, string token, out string response, List<string> errorMessages )
        {
            var restResponse = Checkr.RestClientGet(
                CANIDATES_URL,
                token );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                string errorMessage = "Failed to authorize Checkr Account. Check Checkr Access Token in 'System Settings', 'Checkr' ";
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                string errorMessage = "Failed to get Checkr Report: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }
            else
            {
                response = restResponse.Content;
                return true;
            }
        }

        private static bool GetPackages( string token, out string response, List<string> errorMessages )
        {
            var restResponse = Checkr.RestClientGet(
                PACKAGES_URL,
                token );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                string errorMessage = "Failed to authorize Checkr Account. Check Checkr Access Token in 'System Settings', 'Checkr' ";
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                string errorMessage = "Failed to get Checkr Packages: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }
            else
            {
                response = restResponse.Content;
                return true;
            }
        }

        private static bool GetDocument( string documentId, string token, out string response, List<string> errorMessages )
        {
            var restResponse = Checkr.RestClientGet(
                DOCUMENT_URL + "/" + documentId,
                token );

            if ( restResponse.StatusCode == HttpStatusCode.Unauthorized )
            {
                string errorMessage = "Failed to authorize Checkr Account. Check Checkr Access Token in 'System Settings', 'Checkr' ";
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }

            if ( restResponse.StatusCode != HttpStatusCode.OK )
            {
                string errorMessage = "Failed to get Checkr Document: " + restResponse.Content;
                errorMessages.Add( errorMessage );
                response = null;
                return false;
            }
            else
            {
                response = restResponse.Content;
                return true;
            }
        }
        /// <summary>
        /// Updates the packages.
        /// </summary>
        public static bool UpdatePackages( List<string> errorMessages )
        {
            string accessToken = Rock.Web.SystemSettings.GetValue( SystemSetting.ACCESS_TOKEN );
            string response;
            if ( !Checkr.GetPackages( accessToken, out response, errorMessages ) )
            {
                return false;
            }

            dynamic packagesRestResponse = JObject.Parse( response );

            List<string> packages;
            using ( var rockContext = new RockContext() )
            {

                var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() );

                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                packages = definedValueService
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() )
                    .Where( v => v.Value.StartsWith( CheckrConstants.TYPENAME_PREFIX ) )
                    .ToList()
                    .Select( v => { v.LoadAttributes( rockContext ); return v.GetAttributeValue( "PMMPackageName" ).ToString(); } ) // v => v.Value.Substring( CheckrConstants.TYPENAME_PREFIX.Length ) )
                    .ToList();

                foreach ( var packageRestResponse in packagesRestResponse.data )
                {
                    string packageName = packageRestResponse.slug;
                    if ( !packages.Contains( packageName ) )
                    {
                        DefinedValue definedValue = null;

                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        definedValueService.Add( definedValue );

                        definedValue.Value = CheckrConstants.TYPENAME_PREFIX + packageName.Replace('_', ' ').FixCase();

                        definedValue.Description = packageRestResponse.name == "Educatio Report" ? "Education Report" : packageRestResponse.name;
                        rockContext.SaveChanges();

                        definedValue.LoadAttributes( rockContext );

                        definedValue.SetAttributeValue( "PMMPackageName", packageName );
                        definedValue.SetAttributeValue( "DefaultCounty", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeCounty", "False" );
                        definedValue.SetAttributeValue( "DefaultState", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeState", "False" );
                        definedValue.SetAttributeValue( "MVRJurisdiction", string.Empty );
                        definedValue.SetAttributeValue( "SendHomeStateMVR", "False" );
                        definedValue.SaveAttributeValues( rockContext );

                        CacheDefinedValue.Remove( definedValue.Id );
                    }
                }
            }

            return true;
        }

        public static string GetDocumentUrl( string reportId)
        {
            string url = null;
            string token = Rock.Web.SystemSettings.GetValue( SystemSetting.ACCESS_TOKEN );
            string response;
            List<string> errorMessages = new List<string>();
            if (!GetReport( reportId, token, out response, errorMessages ))
            {
                return null;
            }

            dynamic reportRestResponse = JObject.Parse( response );
            string documentId = reportRestResponse.document_ids.FirstOrDefault();
            if (documentId.IsNullOrWhiteSpace())
            {
                errorMessages.Add( "No document found" );
                return null;
            }

            if ( !GetDocument( documentId, token, out response, errorMessages) )
            {
                return null;
            }

            dynamic documentRestResponse = JObject.Parse( response );
            url = documentRestResponse.download_uri;
            return url;
        }

        /// <summary>
        /// Sends a background request to Checkr
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not
        /// </returns>
        public override bool SendRequest( RockContext rockContext, Model.Workflow workflow,
                    CacheAttribute personAttribute, CacheAttribute ssnAttribute, CacheAttribute requestTypeAttribute,
                    CacheAttribute billingCodeAttribute, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                string token = Rock.Web.SystemSettings.GetValue( SystemSetting.ACCESS_TOKEN );

                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'Checkr' background check provider requires a valid workflow." );
                    return false;
                }

                // Get the person that the request is for
                Person person = null;
                if ( personAttribute != null )
                {
                    Guid? personAliasGuid = workflow.GetAttributeValue( personAttribute.Key ).AsGuidOrNull();
                    if ( personAliasGuid.HasValue )
                    {
                        person = new PersonAliasService( rockContext ).Queryable()
                            .Where( p => p.Guid.Equals( personAliasGuid.Value ) )
                            .Select( p => p.Person )
                            .FirstOrDefault();
                        person.LoadAttributes( rockContext );
                    }
                }

                if ( person == null )
                {
                    errorMessages.Add( "The 'Checkr' background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                    return false;
                }

                string canidateId;
                if ( !CreateCandidate( person, token, out canidateId, errorMessages ) )
                {
                    return false;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                return false;
            }
        }
    }
}