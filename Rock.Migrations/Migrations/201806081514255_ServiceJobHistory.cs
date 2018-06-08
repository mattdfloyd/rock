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
        }
    }
}
