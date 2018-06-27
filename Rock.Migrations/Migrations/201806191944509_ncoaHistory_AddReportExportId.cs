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
    using System;
    using System.Data.Entity.Migrations;
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
            #region Add GetNcoa Job

            Sql( $@"
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
        ,'GetNcoa'
        ,'Job that get NCOA data.'
        ,'Rock.Jobs.GetNcoa'
        ,'0 0/10 0 ? * * *'
        ,1
        ,'{ServiceJob.GET_NCOA}')" );

            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.NcoaHistory", "ReportExportId" );
        }
    }
}
