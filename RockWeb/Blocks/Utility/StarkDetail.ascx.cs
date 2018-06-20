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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Utility;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Stark Detail" )]
    [Category( "Utility" )]
    [Description( "Template block for developers to use to start a new detail block." )]
    [EmailField("Email")]
    public partial class StarkDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        Ncoa ncoa = new Ncoa( "123" );
        static Dictionary<int, Ncoa.PersonAddressItem> addresses = null;
        static string fileName = "Test3"; //"_" + DateTime.Now.Ticks;
        static string exportfileid = string.Empty;
        static List<Ncoa.TrueNcoaReturnRecord> records = new List<Ncoa.TrueNcoaReturnRecord>();

        protected void btnGetAddresses_Click( object sender, EventArgs e )
        {
            addresses = ncoa.GetAddresses();
        }

        protected void btnUploadAddresses_Click( object sender, EventArgs e )
        {
            ncoa.UploadAddresses( addresses, fileName );
        }

        protected void btnCreateReport_Click( object sender, EventArgs e )
        {
            ncoa.CreateReport( fileName );
        }

        protected void btnIsReportCreated_Click( object sender, EventArgs e )
        {
            ncoa.IsReportCreated( fileName );
        }

        protected void btnCreateReportExport_Click( object sender, EventArgs e )
        {

            ncoa.CreateReportExport( fileName, out exportfileid );
        }

        protected void btnIsReportExportCreated_Click( object sender, EventArgs e )
        {
            ncoa.IsReportExportCreated( exportfileid );
        }

        protected void btnDownloadExport_Click( object sender, EventArgs e )
        {
            ncoa.DownloadExport( exportfileid, out records );
        }

        protected void btnSaveRecords_Click( object sender, EventArgs e )
        {
            ncoa.SaveRecords( records, @"R:\TrueNCOA\output.csv" );
        }

        protected void btnParseData_Click( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var txtName = new TextBox();
            txtName.ID = this.ClientID + "tb";
            txtName.Text = "hello";
            ph1.Controls.Add( txtName );

            
            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}