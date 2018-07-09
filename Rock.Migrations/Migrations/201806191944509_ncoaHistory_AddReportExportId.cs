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
namespace Rock.Migrations
{
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class NcoaHistory_AddReportExportId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.NcoaHistory", "ReportExportId", c => c.String() );

            #region Add GetNcoa Job

            Sql( $@"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.GetNcoa')
BEGIN
    INSERT INTO [dbo].[ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid]
    )
    VALUES (
         0 
        ,0 
        ,'Get National Change of Address (NCOA)'
        ,'Job to get a National Change of Address (NCOA) report for all active people''s addresses.'
        ,'Rock.Jobs.GetNcoa'
        ,'0 0/10 0 ? * * *'
        ,1
        ,'{ServiceJob.GET_NCOA}');
END" );

            #endregion

            #region Page and block

            // Add the new page
            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Spark Data Settings", "", "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "fa fa-tachometer", "A2D5F989-1E30-47B9-AAFC-F7EC627AFF21" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Spark Data Settings", "Block used to set values specific to Spark Data (NCOA, Etc).", "~/Blocks/Administration/SparkDataSettings.ascx", "Administration", "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
            RockMigrationHelper.AddBlock( true, "0591e498-0ad6-45a5-b8ca-9bca5c771f03", "", "6B6A429D-E42C-70B5-4A04-98E886C45E7A", "Spark Data Settings", "Main", @"", @"", 0, "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );

            #endregion

            #region System e-mail
            // Add system emails for event/suggestion notifications
            RockMigrationHelper.UpdateSystemEmail( "System", "Following Event Notification", "", "", "", "", "", "Spark Data: {{ SparkDataService }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    The '{{ SparkDataService }}' job has finished.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "CBCBE0F0-67FB-6393-4D9C-592C839A2E54" );
            #endregion

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.NcoaHistory", "ReportExportId" );
            RockMigrationHelper.DeleteBlock( "E7BA08B2-F8CC-2FA8-4677-EA3E776F4EEB" );
            RockMigrationHelper.DeleteBlockType( "6B6A429D-E42C-70B5-4A04-98E886C45E7A" );
            RockMigrationHelper.DeletePage( "0591e498-0ad6-45a5-b8ca-9bca5c771f03" );
            Sql( $@"DELETE FROM [dbo].[ServiceJob] WHERE [Guid] = '{ServiceJob.GET_NCOA}'" );
            RockMigrationHelper.DeleteSystemEmail( "CBCBE0F0-67FB-6393-4D9C-592C839A2E54" );
        }
    }
}
