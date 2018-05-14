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
using Rock;
using Rock.Plugin;
using Rock.Checkr.Constants;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    public partial class Checkr_CreatePages : Migration
    {
        /// <summary>
        /// The Protect My Ministry workflow action.
        /// </summary>
        public static readonly string PMM_WORKFLOWACTION = "16D12EF7-C546-4039-9036-B73D118EDC90";

        /// <summary>
        /// The new Protect My Ministry workflow action name
        /// </summary>
        public static readonly string NEW_PMM_WORKFLOWACTION_NAME = "Background Check (PMM)";

        /// <summary>
        /// Makes the Checkr the default workflow action.
        /// </summary>
        public void MakeCheckrDefaultWorkflowAction()
        {
            // Remove Checr Background Check Workflow from Bio
            RockMigrationHelper.DeleteBlockAttributeValue( Block.BIO, WorkflowAction.BIO, CheckrSystemGuid.CHECKR_WORKFLOWACTION );

            // Add PMM Background Check Workflow to Bio
            RockMigrationHelper.AddBlockAttributeValue( Block.BIO, WorkflowAction.BIO, PMM_WORKFLOWACTION, appendToExisting: true );
            // Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", newPMM_WorkflowActionName, PMM_WorkflowAction ) );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", CheckrConstants.CHECKR_WORKFLOWACTION_NAME, CheckrSystemGuid.CHECKR_WORKFLOWACTION ) );
        }

        /// <summary>
        /// Makes the PMM the default workflow action.
        /// </summary>
        public void MakePMMDefaultWorkflowAction()
        {
            // Remove PMM Background Check Workflow from Bio
            RockMigrationHelper.DeleteBlockAttributeValue( Block.BIO, WorkflowAction.BIO, PMM_WORKFLOWACTION );

            // Add Checkr Background Check Workflow to Bio
            RockMigrationHelper.AddBlockAttributeValue( Block.BIO, WorkflowAction.BIO, CheckrSystemGuid.CHECKR_WORKFLOWACTION, appendToExisting: true );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", NEW_PMM_WORKFLOWACTION_NAME, PMM_WORKFLOWACTION ) );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", "Background Check", CheckrSystemGuid.CHECKR_WORKFLOWACTION ) );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Checkr", "", CheckrSystemGuid.CHECKR_PAGE, "fa fa-shield", "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Checkr Settings", "Block for updating the settings used by the Checkr integration.", "~/Blocks/Security/BackgroundCheck/CheckrSettings.ascx", "Security  > Background Check", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.UpdateBlockType( "Checkr Request List", "Lists all the Checkr background check requests.", "~/Blocks/Security/BackgroundCheck/CheckrRequestList.ascx", "Security > Background Check", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE, "Checkr Settings", "Main", "", "", 0, CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.AddBlock( CheckrSystemGuid.CHECKR_PAGE, "", CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, "Checkr Request List", "Main", "", "", 1, CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );

            // Attrib for BlockType: Request List:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE, FieldType.PAGE_REFERENCE, "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE );

            // Attrib Value for Block:Request List, Attribute:Workflow Detail Page Page: Protect My Ministry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( CheckrSystemGuid.CHECKR_REQUESTLIST_WORKFLOWDETAILPAGE_ATTRIBUTE, "EBD0D19C-E73D-41AE-82D4-C89C21C35998", Rock.SystemGuid.Page.WORKFLOW_DETAIL );

            int count = (int)SqlScalar( "SELECT COUNT(Id) FROM [dbo].[BackgroundCheck]" );
            if ( count != 0 )
            {
                MakeCheckrDefaultWorkflowAction();
            }
            else
            {
                // Do nothing if PMM have been used.
            }

            // Add PMM Background Check Workflow to Bio 
            //RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "16D12EF7-C546-4039-9036-B73D118EDC90", appendToExisting: true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_SETTINGS_BLOCK );
            RockMigrationHelper.DeleteBlock( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCK );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_SETTINGS_BLOCKTYPE );
            RockMigrationHelper.DeleteBlockType( CheckrSystemGuid.CHECKR_REQUESTLIST_BLOCKTYPE );
            RockMigrationHelper.DeletePage( CheckrSystemGuid.CHECKR_PAGE );

            //RockMigrationHelper.DeleteBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "xx" );
        }
    }
}