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

namespace Rock.Migrations
{
    [MigrationNumber( 2, "1.8.0" )]
    public partial class Checkr_CreatePages : Migration
    {
        public static readonly string BLOCK_BIO = "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A";
        public static readonly string BIO_WORKFLOWACTION = "7197A0FB-B330-43C4-8E62-F3C14F649813";
        public static readonly string PMM_WORKFLOWACTION = "16D12EF7-C546-4039-9036-B73D118EDC90";
        public static readonly string NEW_PMM_WORKFLOWACTION_NAME = "Background Check (PMM)";
        public static readonly string CHECKR_WORKFLOWACTION_NAME = "Background Check (Checkr)";

        /// <summary>
        /// Makes the Checkr the default workflow action.
        /// </summary>
        public void MakeCheckrDefaultWorkflowAction()
        {
            // Remove Checr Background Check Workflow from Bio
            RockMigrationHelper.DeleteBlockAttributeValue( BLOCK_BIO, BIO_WORKFLOWACTION, CheckrConstants.CHECKR_WORKFLOWACTION );

            // Add PMM Background Check Workflow to Bio
            RockMigrationHelper.AddBlockAttributeValue( BLOCK_BIO, BIO_WORKFLOWACTION, PMM_WORKFLOWACTION, appendToExisting: true );
            // Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", newPMM_WorkflowActionName, PMM_WorkflowAction ) );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", CHECKR_WORKFLOWACTION_NAME, CheckrConstants.CHECKR_WORKFLOWACTION ) );
        }

        /// <summary>
        /// Makes the PMM the default workflow action.
        /// </summary>
        public void MakePMMDefaultWorkflowAction()
        {
            // Remove PMM Background Check Workflow from Bio
            RockMigrationHelper.DeleteBlockAttributeValue( BLOCK_BIO, BIO_WORKFLOWACTION, PMM_WORKFLOWACTION );

            // Add Checkr Background Check Workflow to Bio
            RockMigrationHelper.AddBlockAttributeValue( BLOCK_BIO, BIO_WORKFLOWACTION, CheckrConstants.CHECKR_WORKFLOWACTION, appendToExisting: true );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", NEW_PMM_WORKFLOWACTION_NAME, PMM_WORKFLOWACTION ) );
            Sql( string.Format( "UPDATE [dbo].[WorkflowType] SET [Name] = '{0}' WHERE [Guid] = '{1}'", "Background Check", CheckrConstants.CHECKR_WORKFLOWACTION ) );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Checkr", "", "13283F50-2008-42CB-AB64-6165B360A9DC", "fa fa-shield", "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Checkr Settings", "Block for updating the settings used by the Checkr integration.", "~/Blocks/Security/BackgroundCheck/CheckrSettings.ascx", "Security  > Background Check", "60DE985A-1F09-4071-970E-7EFBA8C32893" );
            RockMigrationHelper.UpdateBlockType( "Checkr Request List", "Lists all the Checkr background check requests.", "~/Blocks/Security/BackgroundCheck/CheckrRequestList.ascx", "Security > Background Check", "6AD69F29-39E3-4786-886F-489AD9FBB550" );
            RockMigrationHelper.AddBlock( "13283F50-2008-42CB-AB64-6165B360A9DC", "", "60DE985A-1F09-4071-970E-7EFBA8C32893", "Checkr Settings", "Main", "", "", 0, "6D9BF15B-089A-4D37-B196-1A0892123080" );
            RockMigrationHelper.AddBlock( "13283F50-2008-42CB-AB64-6165B360A9DC", "", "6AD69F29-39E3-4786-886F-489AD9FBB550", "Checkr Request List", "Main", "", "", 1, "4BA08F8C-9595-406F-A301-19C6EEEAD232" );

            // Attrib for BlockType: Request List:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "6AD69F29-39E3-4786-886F-489AD9FBB550", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", "EBD0D19C-E73D-41AE-82D4-C89C21C35998" );

            // Attrib Value for Block:Request List, Attribute:Workflow Detail Page Page: Protect My Ministry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4BA08F8C-9595-406F-A301-19C6EEEAD232", "EBD0D19C-E73D-41AE-82D4-C89C21C35998", @"ba547eed-5537-49cf-bd4e-c583d760788c,513C4FDF-E54E-43C1-82C6-BB8B9AAE5D01" );

            int count = (int) SqlScalar( "SELECT COUNT(Id) FROM [dbo].[BackgroundCheck]" );
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
            RockMigrationHelper.DeleteBlock( "4BA08F8C-9595-406F-A301-19C6EEEAD232" );
            RockMigrationHelper.DeleteBlock( "6D9BF15B-089A-4D37-B196-1A0892123080" );
            RockMigrationHelper.DeleteBlockType( "60DE985A-1F09-4071-970E-7EFBA8C32893" ); // Protect My Ministry Settings
            RockMigrationHelper.DeleteBlockType( "6AD69F29-39E3-4786-886F-489AD9FBB550" ); // Request List
            RockMigrationHelper.DeletePage( "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); //  Page: Protect My Ministry, Layout: Full Width, Site: Rock RMS

            //RockMigrationHelper.DeleteBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "xx" );
        }
    }
}
