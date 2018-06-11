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
    
    /// <summary>
    ///
    /// </summary>
    public partial class ServiceJobHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceJobHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServiceJobId = c.Int(nullable: false),
                        ServiceWorkerIpAddress = c.String(maxLength: 45),
                        StartDateTime = c.DateTime(),
                        StopDateTime = c.DateTime(),
                        Status = c.String(maxLength: 50),
                        StatusMessage = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.ServiceJob", t => t.ServiceJobId, cascadeDelete: true)
                .Index(t => t.ServiceJobId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.ServiceJob", "EnableHistory", c => c.Boolean(nullable: false, defaultValue: true ) );
            AddColumn("dbo.ServiceJob", "HistoryCount", c => c.Int(nullable: false, defaultValue: 100 ) );
/*
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Checkr", "", CheckrSystemGuid.CHECKR_PAGE, "fa fa-shield", "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Checkr Settings", "Block for updating the settings used by the Checkr integration.", "~/Blocks/Security/BackgroundCheck/CheckrSettings.ascx", "Security  > Background Check", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.UpdateBlockType( "Checkr Request List", "Lists all the Checkr background check requests.", "~/Blocks/Security/BackgroundCheck/CheckrRequestList.ascx", "Security > Background Check", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE, "Checkr Settings", "Main", "", "", 0, CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, "Checkr Request List", "Main", "", "", 1, CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );

            // Attrib for BlockType: Request List:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, FieldType.PAGE_REFERENCE, "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE );

            // Attrib Value for Block:Request List, Attribute:Workflow Detail Page Page: Protect My Ministry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE, "EBD0D19C-E73D-41AE-82D4-C89C21C35998", Rock.SystemGuid.Page.WORKFLOW_DETAIL );
*/

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ServiceJobHistory", "ServiceJobId", "dbo.ServiceJob");
            DropForeignKey("dbo.ServiceJobHistory", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ServiceJobHistory", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.ServiceJobHistory", new[] { "Guid" });
            DropIndex("dbo.ServiceJobHistory", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ServiceJobHistory", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ServiceJobHistory", new[] { "ServiceJobId" });
            DropColumn("dbo.ServiceJob", "HistoryCount");
            DropColumn("dbo.ServiceJob", "EnableHistory");
            DropTable("dbo.ServiceJobHistory");

            /*
                        RockMigrationHelper.DeleteBlockAttributeValue( Block.BIO, CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );
            RockMigrationHelper.DeletePage( CheckrSystemGuid.CHECKR_PAGE );
*/
        }
    }
}
