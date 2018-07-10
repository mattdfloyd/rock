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
using System.Web.UI.WebControls;
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
    /// <summary>
    /// Make NCOA calls to get change of address information
    /// </summary>
    public class Ncoa
    {
        #region Get Addresses

        /// <summary>
        /// PeopleIds inside a DataView filter.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>Returns a directory of people IDs that result from applying the DataView filter</returns>
        public Dictionary<int, int> DataViewPeopleDirectory( int dataViewId, RockContext rockContext )
        {
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.GetNoTracking( dataViewId );

            // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
            if ( dataViewService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
            {
                throw new Exception( "Data View Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
            }

            // Evaluate the Data View that defines the candidate population.
            List<string> errorMessages;

            var personService = new PersonService( rockContext );

            var personQuery = personService.Queryable();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

            if ( errorMessages.Any() )
            {
                throw new Exception( "Data View Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
            }

            return personQuery.Where( paramExpression, whereExpression, null ).Select( p => p.Id ).ToDictionary( p => p, p => p );
        }

        /// <summary>
        /// Gets the addresses.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <returns>Directory of addresses</returns>
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

                throw new Exception( "Get Address: Could not find expected constant, type or value" );
            }
        }

        #endregion

        /// <summary>
        /// Sends the notification that NCOA finished
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void SentNotification( SparkDataConfig sparkDataConfig, string status )
        {
            if ( !sparkDataConfig.GlobalNotificationApplicationGroupId.HasValue || sparkDataConfig.GlobalNotificationApplicationGroupId.Value == 0 )
            {
                return;
            }

            var recipients = new List<RecipientData>();
            using ( RockContext rockContext = new RockContext() )
            {
                Group group = new GroupService( rockContext ).GetNoTracking( sparkDataConfig.GlobalNotificationApplicationGroupId.Value );

                foreach ( var groupMember in group.Members )
                {
                    if ( groupMember.GroupMemberStatus == GroupMemberStatus.Active )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "Person", groupMember.Person );
                        mergeFields.Add( "GroupMember", groupMember );
                        mergeFields.Add( "Group", groupMember.Group );
                        mergeFields.Add( "SparkDataService", "National Change of Address (NCOA)" );
                        mergeFields.Add( "SparkDataConfig", sparkDataConfig );
                        mergeFields.Add( "Status", status );
                        recipients.Add( new RecipientData( groupMember.Person.Email, mergeFields ) );
                    }
                }

                SystemEmailService emailService = new SystemEmailService( rockContext );
                SystemEmail systemEmail = emailService.GetNoTracking( SystemGuid.SystemEmail.SPARK_DATA_NOTIFICATION.AsGuid() );

                var emailMessage = new RockEmailMessage( systemEmail.Guid );
                emailMessage.SetRecipients( recipients );
                emailMessage.Send();
            }
        }

        /// <summary>
        /// Starts the NCOA request.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        public void Start( SparkDataConfig sparkDataConfig )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var accountStatus = sparkDataApi.CheckAccount( sparkDataConfig.SparkDataApiKey );
            switch ( accountStatus )
            {
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNoName:
                    throw new UnauthorizedAccessException( "Account does not have a name." );
                case SparkDataApi.SparkDataApi.AccountStatus.AccountNotFound:
                    throw new UnauthorizedAccessException( "Account not found." );
                case SparkDataApi.SparkDataApi.AccountStatus.Disabled:
                    throw new UnauthorizedAccessException( "Account is disabled." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledCardExpired:
                    throw new UnauthorizedAccessException( "Credit card on Spark server expired." );
                case SparkDataApi.SparkDataApi.AccountStatus.EnabledNoCard:
                    throw new UnauthorizedAccessException( "No credit card found on Spark server." );
                case SparkDataApi.SparkDataApi.AccountStatus.InvalidSparkDataKey:
                    throw new UnauthorizedAccessException( "Invalid Spark Data Key." );
            }

            var addresses = GetAddresses( sparkDataConfig.NcoaSettings.PersonDataViewId );
            if ( addresses.Count == 0)
            {
                sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                SaveSettings( sparkDataConfig );
                return;
            }

            GroupNameTransactionKey groupNameTransactionKey = sparkDataApi.NcoaIntiateReport( sparkDataConfig.SparkDataApiKey, addresses.Count, sparkDataConfig.NcoaSettings.PersonAliasId );
            sparkDataConfig.NcoaSettings.FileName = groupNameTransactionKey.TransactionKey;
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );

            string id;
            trueNcoaApi.CreateFile( sparkDataConfig.NcoaSettings.FileName, groupNameTransactionKey.GroupName, out id );
            trueNcoaApi.Id = id;

            trueNcoaApi.UploadAddresses( addresses, sparkDataConfig.NcoaSettings.FileName );

            sparkDataConfig.NcoaSettings.CurrentReportKey = id;
            sparkDataConfig.NcoaSettings.CurrentUploadCount = addresses.Count;
            trueNcoaApi.CreateReport( sparkDataConfig.NcoaSettings.FileName );
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Report";
            SaveSettings( sparkDataConfig );

            // Delete previous NcoaHistory entries
            using ( RockContext rockContext = new RockContext() )
            {
                NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                ncoaHistoryService.DeleteRange( ncoaHistoryService.Queryable() );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Resume a pending report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingReport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );
            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );
            if ( !trueNcoaApi.IsReportCreated( sparkDataConfig.NcoaSettings.FileName ) )
            {
                return;
            }

            string exportFileId;
            trueNcoaApi.CreateReportExport( sparkDataConfig.NcoaSettings.FileName, out exportFileId );

            sparkDataConfig.NcoaSettings.CurrentReportExportKey = exportFileId;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Pending: Export";
            SaveSettings( sparkDataConfig );
        }

        /// <summary>
        /// Resume a pending export.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        public void PendingExport( SparkDataConfig sparkDataConfig = null )
        {
            if ( sparkDataConfig == null )
            {
                sparkDataConfig = GetSettings();
            }

            SparkDataApi.SparkDataApi sparkDataApi = new SparkDataApi.SparkDataApi();
            var credentials = sparkDataApi.NcoaGetCredentials( sparkDataConfig.SparkDataApiKey );

            var trueNcoaApi = new TrueNcoaApi( sparkDataConfig.NcoaSettings.CurrentReportKey, credentials );
            if ( !trueNcoaApi.IsReportExportCreated( sparkDataConfig.NcoaSettings.FileName ) )
            {
                return;
            }

            List<TrueNcoaReturnRecord> trueNcoaReturnRecords;
            trueNcoaApi.DownloadExport( sparkDataConfig.NcoaSettings.CurrentReportExportKey, out trueNcoaReturnRecords );

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

            sparkDataApi.NcoaCompleteReport( sparkDataConfig.SparkDataApiKey, sparkDataConfig.NcoaSettings.CurrentReportExportKey, sparkDataConfig.NcoaSettings.CurrentReportExportKey );

            //Notify group
            SentNotification( sparkDataConfig, "finished" );
        }

        #region Settings
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

        #endregion

    }
}
