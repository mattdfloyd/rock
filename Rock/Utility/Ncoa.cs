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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using RestSharp;
using Rock;
using Rock.Cache;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.NcoaApi;
using Rock.Utility.Settings.SparkData;

namespace Rock.Utility
{
    public class Ncoa
    {
        /// <summary>
        /// Checks the early access status of this organization.
        /// </summary>
        private void CheckAccount( string sparkDataApiKey )
        {
            var client = new RestClient( "http://www.rockrms.com/api/SparkData/ValidateAccount" );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "sparkDataApiKey", sparkDataApiKey );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                if ( !response.Content.AsBoolean() )
                {
                    throw new UnauthorizedAccessException( "Could not authenticate Spark Data account: No valid credit card found in Spark account" );
                }
            }
            else
            {
                throw new UnauthorizedAccessException( $"Could not authenticate Spark Data account: {response.StatusCode.ConvertToString()} '{response.Content}'" );
            }
        }

        /// <summary>
        /// Initiates the report.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <exception cref="UnauthorizedAccessException">
        /// Could not authenticate Spark Data account: No valid credit card found in Spark account
        /// or
        /// </exception>
        private string IntiateReport( string sparkDataApiKey, int? numberRecords, int? personAliasId = null )
        {
            var client = new RestClient( $"http://www.rockrms.com/api/SparkData/Ncoa/IntiateReport/{sparkDataApiKey}/{numberRecords}" );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            if ( personAliasId.HasValue )
            {
                request.AddParameter( "personAliasId", personAliasId.Value );
            }

            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return response.Content;
            }
            else
            {
                throw new Exception( $"Could not initiate Spark report: {response.StatusCode.ConvertToString()} '{response.Content}'" );
            }
        }

        /// <summary>
        /// Initiates the report.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <exception cref="UnauthorizedAccessException">
        /// Could not authenticate Spark Data account: No valid credit card found in Spark account
        /// or
        /// </exception>
        private UsernamePassword GetCredentials( string sparkDataApiKey )
        {
            var client = new RestClient( $"api/SparkData/Ncoa/GetCredentials/{sparkDataApiKey}" );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            // IRestResponse response = client.Execute( request );
            var response = client.Get<UsernamePassword>( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return response.Data;
            }
            else
            {
                throw new Exception( $"Could not get credentials from Spark: {response.StatusCode.ConvertToString()} '{response.Content}'" );
            }
        }

        /// <summary>
        /// Initiates the report.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <exception cref="UnauthorizedAccessException">
        /// Could not authenticate Spark Data account: No valid credit card found in Spark account
        /// or
        /// </exception>
        private bool CompleteReport( string sparkDataApiKey, string reportKey, string exportFileKey )
        {
            var client = new RestClient( $"api/SparkData/Ncoa/GetCredentials/{sparkDataApiKey}" );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            // IRestResponse response = client.Execute( request );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return !response.Content.AsBoolean();
            }
            else
            {
                throw new Exception( $"Could not complete Spark report: {response.StatusCode.ConvertToString()} '{response.Content}'" );
            }
        }

        public Dictionary<int, int> DataViewPeopleDirectory( int dataViewId, RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( dataViewId );

            // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
            if ( dataViewService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
            {
                throw new Exception( "Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            // Evaluate the Data View that defines the candidate population.
            List<string> errorMessages;

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

            if ( errorMessages.Any() )
            {
                throw new Exception( "Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
            }

            return personQuery.Where( paramExpression, whereExpression, null ).Select( p => p.Id ).ToDictionary( p => p, p => p );
        }

        public Dictionary<int, PersonAddressItem> GetAddresses( int? dataViewId )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var familyGroupType = CacheGroupType.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeLoc = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var inactiveStatus = CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

                if ( familyGroupType != null && homeLoc != null && inactiveStatus != null )
                {
                    var groupMembers = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupTypeId == familyGroupType.Id && // Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY
                            m.Person.RecordStatusValueId != inactiveStatus.Id && // Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE
                            m.Group.GroupLocations.Any( gl => gl.GroupLocationTypeValueId.HasValue &&
                                     gl.GroupLocationTypeValueId == homeLoc.Id ) ); // CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME

                    var peopleHomelocation = groupMembers.Select( m => new
                    {
                        m.PersonId,
                        m.GroupId,
                        m.Person.FirstName,
                        m.Person.LastName,
                        m.Person.Aliases,
                        HomeLocation = m.Group.GroupLocations
                            .Where( gl =>
                                gl.GroupLocationTypeValueId.HasValue &&
                                gl.GroupLocationTypeValueId == homeLoc.Id ) // CacheDefinedValue.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME
                            .Select( gl => new
                            {
                                gl.Location.Street1,
                                gl.Location.Street2,
                                gl.Location.City,
                                gl.Location.State,
                                gl.Location.PostalCode,
                                gl.Location.Country
                            } ).FirstOrDefault()
                    } ).Where( m => m.HomeLocation != null ).DistinctBy( m => m.PersonId );

                    if ( dataViewId.HasValue )
                    {
                        var dataViewQuery = DataViewPeopleDirectory( dataViewId.Value, rockContext );
                        peopleHomelocation = peopleHomelocation.Where( p => dataViewQuery.ContainsKey( p.PersonId ) );
                    }

                    return peopleHomelocation
                        .Select( g => new
                        {
                            g.PersonId,
                            HomeLocation = new PersonAddressItem()
                            {
                                PersonId = g.PersonId,
                                FamilyId = g.GroupId,
                                PersonAliasId = g.Aliases.Count == 0 ? 0 : g.Aliases.FirstOrDefault().Id,
                                FirstName = g.FirstName,
                                LastName = g.LastName,
                                Street1 = g.HomeLocation.Street1,
                                Street2 = g.HomeLocation.Street2,
                                City = g.HomeLocation.City,
                                State = g.HomeLocation.State,
                                PostalCode = g.HomeLocation.PostalCode,
                                Country = g.HomeLocation.Country
                            }
                        } )
                        .ToDictionary( k => k.PersonId, v => v.HomeLocation );
                }
            }

            return null;
        }

        private void SendNotification( SparkDataConfig sparkDataConfig )
        {
            if ( !sparkDataConfig.GlobalNotificationApplicationGroupId.HasValue || sparkDataConfig.GlobalNotificationApplicationGroupId.Value == 0 )
            {
                return;
            }

            var recipients = new List<RecipientData>();
            using ( RockContext rockContext = new RockContext() )
            {
                Group group = new GroupService( rockContext ).Get( sparkDataConfig.GlobalNotificationApplicationGroupId.Value );

                foreach(var groupMember in group.Members)
                {
                    if ( groupMember.GroupMemberStatus == GroupMemberStatus.Active )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", groupMember.Person );
                        mergeFields.Add( "GroupMember", groupMember );
                        mergeFields.Add( "Group", groupMember.Group );
                        mergeFields.Add( "SparkDataService", "National Change of Address (NCOA)" );
                        mergeFields.Add( "SparkDataConfig", sparkDataConfig );
                        recipients.Add( new RecipientData( groupMember.Person.Email, mergeFields ) );
                    }
                }

                SystemEmailService emailService = new SystemEmailService( rockContext );
                SystemEmail systemEmail = emailService.Get( SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION.AsGuid() );

                var emailMessage = new RockEmailMessage( systemEmail.Guid );
                emailMessage.SetRecipients( recipients );
                emailMessage.Send();
            }
        }

        public void Start( int? personAliasId = null, SparkDataConfig sparkDataConfig = null )
        {
            Start( sparkDataConfig, personAliasId );
        }

        public void Start( SparkDataConfig sparkDataConfig, int? personAliasId = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            CheckAccount( sparkDataConfig.SparkDataApiKey );
            var addresses = GetAddresses( sparkDataConfig.NcoaSettings.PersonDataViewId );
            sparkDataConfig.NcoaSettings.CurrentReportKey = IntiateReport( sparkDataConfig.SparkDataApiKey, addresses.Count, personAliasId );
            var credentials = GetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );
            trueNcoaApi.UploadAddresses( addresses, sparkDataConfig.NcoaSettings.CurrentReportKey );
            sparkDataConfig.NcoaSettings.CurrentUploadCount = addresses.Count;
            trueNcoaApi.CreateReport( sparkDataConfig.NcoaSettings.CurrentReportKey );
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Report";
            SaveSettings( sparkDataConfig );
        }

        public void PendingReport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            var credentials = GetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );
            if ( !trueNcoaApi.IsReportCreated( sparkDataConfig.NcoaSettings.CurrentReportKey ) )
            {
                return;
            }

            string exportFileId;
            if ( trueNcoaApi.CreateReportExport( sparkDataConfig.NcoaSettings.CurrentReportKey, out exportFileId ) )
            {
                sparkDataConfig.NcoaSettings.CurrentReportExportKey = exportFileId;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Export";
                SaveSettings( sparkDataConfig );
            }
        }

        public void PendingExport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            var credentials = GetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );
            if ( !trueNcoaApi.IsReportExportCreated( sparkDataConfig.NcoaSettings.CurrentReportExportKey ) )
            {
                return;
            }

            List<TrueNcoaReturnRecord> trueNcoaReturnRecords;
            if ( trueNcoaApi.DownloadExport( sparkDataConfig.NcoaSettings.CurrentReportExportKey, out trueNcoaReturnRecords ) )
            {
                if ( trueNcoaReturnRecords != null && trueNcoaReturnRecords.Count != 0 )
                {
                    var ncoaHistoryList = trueNcoaReturnRecords.Select( r => r.ToNcoaHistory() );
                    using ( var rockContext = new RockContext() )
                    {
                        var ncoaHistoryService = new NcoaHistoryService( rockContext );
                        ncoaHistoryService.AddRange( ncoaHistoryList );
                        rockContext.SaveChanges();
                    }
                }

                sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                SaveSettings( sparkDataConfig );

                CompleteReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.CurrentReportExportKey, sparkDataConfig.NcoaSettings.CurrentReportExportKey );
                //Notify group
                SendNotification( sparkDataConfig );
            }
        }




        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>The spark data configuration.</returns>
        public static SparkDataConfig GetSettings( SparkDataConfig sparkDataConfig = null )
        {
            // Get Spark Data settings
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();
            }


            if ( sparkDataConfig == null )
            {
                sparkDataConfig = new SparkDataConfig();
            }

            return sparkDataConfig;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public static void SaveSettings( SparkDataConfig sparkDataConfig )
        {
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, sparkDataConfig.ToJson() );
        }
    }
}
