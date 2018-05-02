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
using Rock.Checkr.CheckrApi;
using System.Data.Entity;

namespace Rock.Checkr
{
    /// <summary>
    /// Checkr Background Check 
    /// </summary>
    [Description( "Checkr Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "Checkr" )]

    [UrlLinkField( "Request URL", "The Checkr URL to send requests to.", true, "https://services.priorityresearch.com/webservice/default.cfm", "", 0 )]
    [UrlLinkField( "Return URL", "The Web Hook URL for Checkr to send results to (e.g. 'http://www.mysite.com/Webhooks/Checkr.ashx').", true, "", "", 1 )]
    public class Checkr : BackgroundCheckComponent
    {
        /// <summary>
        /// Creates the candidate.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="candidateId">The candidate identifier.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public static bool CreateCandidate( Person person, out string candidateId, List<string> errorMessages, out string request, out string response )
        {
            CreateCandidateResponse createCandidateResponse;
            candidateId = null;
            if ( CheckrApiUtility.CreateCandidate( person, out createCandidateResponse, errorMessages , out request, out response) )
            {
                candidateId = createCandidateResponse.Id;
                return true;
            }

            return false;
        }

        public static bool CreateInvitation( string candidateId, string package, List<string> errorMessages, out string request, out string response)
        {
            CreateInvitationResponse createInvitationResponse;
            if ( CheckrApiUtility.CreateInvitation( candidateId, package, out createInvitationResponse, errorMessages, out request, out response ) )
            {
                candidateId = createInvitationResponse.Id;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the Checkr packages and update the list on the server.
        /// </summary>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public static bool UpdatePackages( List<string> errorMessages )
        {
            GetPackagesResponse getPackagesResponse;
            if ( !CheckrApiUtility.GetPackages( out getPackagesResponse, errorMessages ) )
            {
                return false;
            }

            List<string> packages;
            using ( var rockContext = new RockContext() )
            {

                var definedType = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() );

                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                packages = definedValueService
                    .GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_PACKAGES.AsGuid() )
                    .Where( v => v.ForeignId == 2 )
                    .ToList()
                    .Select( v => { v.LoadAttributes( rockContext ); return v.GetAttributeValue( "PMMPackageName" ).ToString(); } ) // v => v.Value.Substring( CheckrConstants.TYPENAME_PREFIX.Length ) )
                    .ToList();

                foreach ( var packageRestResponse in getPackagesResponse.Data )
                {
                    string packageName = packageRestResponse.Slug;
                    if ( !packages.Contains( packageName ) )
                    {
                        DefinedValue definedValue = null;

                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        definedValue.ForeignId = 2;
                        definedValueService.Add( definedValue );

                        definedValue.Value = CheckrConstants.TYPENAME_PREFIX + packageName.Replace( '_', ' ' ).FixCase();

                        definedValue.Description = packageRestResponse.Name == "Educatio Report" ? "Education Report" : packageRestResponse.Name;
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

        public static string GetDocumentUrl( string documentId )
        {
            GetDocumentResponse getDocumentResponse;
            List<string> errorMessages = new List<string>();

            if ( CheckrApiUtility.GetDocument( documentId, out getDocumentResponse, errorMessages ) )
            {
                return getDocumentResponse.DownloadUri;
            }

            return null;
        }




        /// <summary>
        /// Saves the attribute value.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        /// <returns></returns>
        private static bool SaveAttributeValue( Rock.Model.Workflow workflow, string key, string value,
            CacheFieldType fieldType, RockContext rockContext, Dictionary<string, string> qualifiers = null )
        {
            bool createdNewAttribute = false;

            if ( workflow.Attributes.ContainsKey( key ) )
            {
                workflow.SetAttributeValue( key, value );
            }
            else
            {
                // Read the attribute
                var attributeService = new AttributeService( rockContext );
                var attribute = attributeService
                    .Get( workflow.TypeId, "WorkflowTypeId", workflow.WorkflowTypeId.ToString() )
                    .Where( a => a.Key == key )
                    .FirstOrDefault();

                // If workflow attribute doesn't exist, create it 
                // ( should only happen first time a background check is processed for given workflow type)
                if ( attribute == null )
                {
                    attribute = new Rock.Model.Attribute();
                    attribute.EntityTypeId = workflow.TypeId;
                    attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                    attribute.EntityTypeQualifierValue = workflow.WorkflowTypeId.ToString();
                    attribute.Name = key.SplitCase();
                    attribute.Key = key;
                    attribute.FieldTypeId = fieldType.Id;
                    attributeService.Add( attribute );

                    if ( qualifiers != null )
                    {
                        foreach ( var keyVal in qualifiers )
                        {
                            var qualifier = new Rock.Model.AttributeQualifier();
                            qualifier.Key = keyVal.Key;
                            qualifier.Value = keyVal.Value;
                            attribute.AttributeQualifiers.Add( qualifier );
                        }
                    }

                    createdNewAttribute = true;
                }

                // Set the value for this action's instance to the current time
                var attributeValue = new Rock.Model.AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = value;
                new AttributeValueService( rockContext ).Add( attributeValue );
            }

            return createdNewAttribute;
        }


        private static void UpdateWorkflow(int id, string recommendation, string reportLink, string reportStatus, RockContext rockContext )
        {
            bool createdNewAttribute = false;
            var workflowService = new WorkflowService( rockContext );
            var workflow = new WorkflowService( rockContext ).Get( id );
            if ( workflow != null && workflow.IsActive )
            {
                workflow.LoadAttributes();

                // Save the recommendation 
                if ( !string.IsNullOrWhiteSpace( recommendation ) )
                {
                    if ( SaveAttributeValue( workflow, "ReportRecommendation", recommendation,
                        CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "ispassword", "false" } } ) )
                    {
                        createdNewAttribute = true;
                    }

                }
                // Save the report link 
                if ( !string.IsNullOrWhiteSpace( reportLink ) )
                {
                    if ( SaveAttributeValue( workflow, "ReportLink", reportLink,
                        CacheFieldType.Get( Rock.SystemGuid.FieldType.URL_LINK.AsGuid() ), rockContext ) )
                    {
                        createdNewAttribute = true;
                    }
                }

                // Save the status
                if ( SaveAttributeValue( workflow, "ReportStatus", reportStatus,
                    CacheFieldType.Get( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ), rockContext,
                    new Dictionary<string, string> { { "fieldtype", "ddl" }, { "values", "Pass,Fail,Review" } } ) )
                {
                    createdNewAttribute = true;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    workflow.SaveAttributeValues( rockContext );
                    foreach ( var activity in workflow.Activities )
                    {
                        activity.SaveAttributeValues( rockContext );
                    }
                } );
            }

            rockContext.SaveChanges();

            if ( createdNewAttribute )
            {
                CacheAttribute.RemoveEntityAttributes();
            }
        }

        public static bool SaveWebhookResults( string postedData )
        {
            GenericWebhook genericWebhook = JsonConvert.DeserializeObject<GenericWebhook>( postedData );
            if ( genericWebhook == null )
            {
                string errorMessage = "Webhook data is not valid: " + postedData;
                ExceptionLogService.LogException( new Exception( errorMessage ), null );
                return false;
            }

            if ( genericWebhook.Type == Enums.WebhookTypes.InvitationCompleted || genericWebhook.Type == Enums.WebhookTypes.InvitationCreated || genericWebhook.Type == Enums.WebhookTypes.InvitationExpired )
            {
                InvitationWebhook invitationWebhook = JsonConvert.DeserializeObject<InvitationWebhook>( postedData );
                if ( invitationWebhook == null )
                {
                    string errorMessage = "Invitation Webhook data is not valid: " + postedData;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                using ( var rockContext = new RockContext() )
                {
                    var backgroundCheck = new BackgroundCheckService( rockContext )
                        .Queryable( "PersonAlias.Person" ).AsNoTracking()
                        .Where( g => g.RequestId == invitationWebhook.Data.Object.CandidateId )
                        .Where( g => g.ForeignId == 2 )
                        .FirstOrDefault();

                    if (backgroundCheck == null)
                    {
                        string errorMessage = "Background Check not found: Candidate ID: " + invitationWebhook.Data.Object.CandidateId;
                        ExceptionLogService.LogException( new Exception( errorMessage ), null );
                        return false;
                    }

                    backgroundCheck.ResponseData = backgroundCheck.ResponseData + string.Format( @"
Webhook Data ({0}): 
------------------------ 
{1}

", RockDateTime.Now.ToString(), postedData );
                    backgroundCheck.Status = genericWebhook.Type.ToString().SplitCase();
                }
            }
            else if ( genericWebhook.Type == Enums.WebhookTypes.ReportCreated )
            {
                ReportWebhook reportWebhook = JsonConvert.DeserializeObject<ReportWebhook>( postedData );
                if ( reportWebhook == null )
                {
                    string errorMessage = "Report Webhook data is not valid: " + postedData;
                    ExceptionLogService.LogException( new Exception( errorMessage ), null );
                    return false;
                }

                using ( var rockContext = new RockContext() )
                {
                    var backgroundCheck = new BackgroundCheckService( rockContext )
                        .Queryable( "PersonAlias.Person" ).AsNoTracking()
                        .Where( g => g.RequestId == reportWebhook.Data.Object.CandidateId )
                        .Where( g => g.ForeignId == 2 )
                        .ToList()
                        .FirstOrDefault();

                    backgroundCheck.Status = genericWebhook.Type.ToString().SplitCase();
                    backgroundCheck.ResponseDate = RockDateTime.Now;
                    //backgroundCheck.RecordFound = reportStatus == "Review";

                }
            }

            return true;
            /*
            // Get the orderid from the XML
                orderId = ( from o in xResult.Descendants( "OrderDetail" ) select (string)o.Attribute( "OrderId" ) ).FirstOrDefault() ?? "OrderIdUnknown";

            if ( !string.IsNullOrEmpty( orderId ) && orderId != "OrderIdUnknown" )
            {
                // Find and update the associated workflow
                var workflowService = new WorkflowService( rockContext );
                var workflow = new WorkflowService( rockContext ).Get( orderId.AsInteger() );
                if ( workflow != null && workflow.IsActive )
                {
                    workflow.LoadAttributes();

                    Rock.Security.BackgroundCheck.ProtectMyMinistry.SaveResults( xResult, workflow, rockContext );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        workflow.SaveAttributeValues( rockContext );
                        foreach ( var activity in workflow.Activities )
                        {
                            activity.SaveAttributeValues( rockContext );
                        }
                    } );

                }
            }
            */
        }










        public static string GetDocumentIdFromReport( string reportId )
        {
            List<string> errorMessages = new List<string>();
            GetReportResponse getReportResponse;
            if ( !CheckrApiUtility.GetReport( reportId, out getReportResponse, errorMessages ) )
            {
                return null;
            }

            if ( getReportResponse.DocumentIds == null || getReportResponse.DocumentIds.Count == 0 )
            {
                errorMessages.Add( "No document found" );
                return null;
            }

            string documentId = getReportResponse.DocumentIds[0];
            if ( documentId.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Empty document ID returned" );
                return null;
            }

            return documentId;
        }

        private bool CreateReport( string candidateId, RockContext rockContext, Model.Workflow workflow, CacheAttribute requestTypeAttribute, out string reportId, List<string> errorMessages, out string request, out string response )
        {
            reportId = null;
            CacheDefinedValue pkgTypeDefinedValue = null;
            request = string.Empty;
            response = string.Empty;
            string packageName = "tasker_pro";

            if ( requestTypeAttribute == null )
            {
                string errorMessage = "Failed to retrieve Checkr Package Type Attribute";
                errorMessages.Add( errorMessage );
                return false;
            }

            pkgTypeDefinedValue = CacheDefinedValue.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
            if ( pkgTypeDefinedValue == null )
            {
                string errorMessage = "Failed to retrieve Checkr Package Type Defined Value";
                errorMessages.Add( errorMessage );

                return false;
            }

            if ( pkgTypeDefinedValue.Attributes == null )
            {
                pkgTypeDefinedValue.LoadAttributes( rockContext );
            }

            packageName = pkgTypeDefinedValue.GetAttributeValue( "PMMPackageName" );
            CreateReportResponse createReportResponse;
            if ( !CheckrApiUtility.CreateReport( candidateId, packageName, out createReportResponse, errorMessages, out request, out response ) )
            {
                return false;
            }

            reportId = createReportResponse.Id;
            return true;
        }



        private void UpdateBackgroundCheck( RockContext rockContext, string request, string response, BackgroundCheck backgroundCheck )
        {
            backgroundCheck.RequestDate = RockDateTime.Now;
            backgroundCheck.ResponseData = string.Format( @"
Request ({0}): 
------------------------ 
{1}

Response ({2}): 
------------------------ 
{3}

", RockDateTime.Now, request, RockDateTime.Now, response );
            rockContext.SaveChanges();
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

                int? personAliasId = person.PrimaryAliasId;
                BackgroundCheck backgroundCheck;
                if ( personAliasId.HasValue )
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        var backgroundCheckService = new BackgroundCheckService( newRockContext );
                        backgroundCheck = backgroundCheckService.Queryable()
                            .Where( c =>
                                c.WorkflowId.HasValue &&
                                c.WorkflowId.Value == workflow.Id )
                            .FirstOrDefault();

                        if ( backgroundCheck == null )
                        {
                            backgroundCheck = new Rock.Model.BackgroundCheck();
                            backgroundCheck.PersonAliasId = personAliasId.Value;
                            backgroundCheck.WorkflowId = workflow.Id;
                            backgroundCheck.ForeignId = 2;
                            backgroundCheckService.Add( backgroundCheck );
                        }

                        backgroundCheck.RequestDate = RockDateTime.Now;
                        newRockContext.SaveChanges();
                    }
                }
                else
                {
                    errorMessages.Add( "The 'Checkr' background check provider requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                    return false;
                }

                string candidateId;
                string request;
                string response;
                using ( var newRockContext = new RockContext() )
                {
                    bool result = CreateCandidate( person, out candidateId, errorMessages, out request, out response );
                    UpdateBackgroundCheck( newRockContext, request, response, backgroundCheck );
                    if ( !result )
                    {
                        return false;
                    }

                    if ( requestTypeAttribute != null )
                    {
                        CacheDefinedValue pkgTypeDefinedValue = CacheDefinedValue.Get( workflow.GetAttributeValue( requestTypeAttribute.Key ).AsGuid() );
                        if ( pkgTypeDefinedValue != null )
                        {
                            if ( pkgTypeDefinedValue.Attributes == null )
                            {
                                pkgTypeDefinedValue.LoadAttributes( rockContext );
                            }

                            string packageName = pkgTypeDefinedValue.GetAttributeValue( "PMMPackageName" );
                            result = CreateInvitation( candidateId, packageName, errorMessages, out request, out response );
                            UpdateBackgroundCheck( newRockContext, request, response, backgroundCheck );
                            if ( result )
                            {
                                if ( SaveAttributeValue( workflow, "RequestStatus", "SUCCESS",
                                    CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), newRockContext, null ) )
                                {
                                    newRockContext.SaveChanges();
                                    CacheAttribute.RemoveEntityAttributes();
                                }

                                return true;
                            }
                        }
                    }
                }

                return false;
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