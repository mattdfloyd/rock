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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility.Settings.SparkData;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Spark Data Settings
    /// </summary>
    [DisplayName( "Spark Data Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to Spark Data (NCOA, Etc)." )]
    public partial class SparkDataConfig : RockBlock
    {
        #region private variables

        private RockContext _rockContext = new RockContext();

        private NcoaSettings _ncoaSettings = new NcoaSettings();

        private SparkDataConfig _sparkDataConfig = new SparkDataConfig();
        #endregion

        #region Base Control Methods

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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindControls();
                GetSettings();
                SetPanels();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the SystemConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles saving all the data set by the user to the web.config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnSaveConfig_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            SaveSettings();
        }

        /// <summary>
        /// Handles the CheckedChanged event when enabling/disabling a Spark Data option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cbSparkDataEnabled_CheckedChanged( object sender, EventArgs e )
        {
            SetPanels();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the controls.
        /// </summary>
        private void BindControls()
        {
        }

        /// <summary>
        /// Enables the data automation panels and sets the titles.
        /// </summary>
        private void SetPanels()
        {
            SetPanel( pwNcoaConfiguration, pnlNcoaConfiguration, "National Change of Address (NCOA)", cbNcoaConfiguration.Checked );
        }

        /// <summary>
        /// Enables a data automation panel and sets the title.
        /// </summary>
        /// <param name="panelWidget">The panel widget.</param>
        /// <param name="panel">The panel.</param>
        /// <param name="title">The title.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void SetPanel( PanelWidget panelWidget, Panel panel, string title, bool enabled )
        {
            panel.Enabled = enabled;
            var enabledLabel = string.Empty;
            if ( enabled )
            {
                enabledLabel = "<span class='label label-success'>Enabled</span>";
            }
            else
            {
                enabledLabel = "<span class='label label-warning'>Disabled</span>";
            }

            panelWidget.Title = string.Format( "<h3 class='panel-title pull-left margin-r-sm'>{0}</h3> <div class='pull-right'>{1}</div>", title, enabledLabel );
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            // Get Ncoa Configuration Settings
            nbMinMoveDistance.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE );
            cb48MonAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();
            cbInvalidAddressAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();

            // Get Data Automation Settings
            _ncoaSettings = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA_NCOA ).FromJsonOrNull<NcoaSettings>() ?? new NcoaSettings();
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        private void SaveSettings()
        {
            // Ncoa Configuration
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE, nbMinMoveDistance.Text );
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS, cb48MonAsPrevious.Checked.ToString() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS, cbInvalidAddressAsPrevious.Checked.ToString() );

            // Save Spark Data
            _ncoaSettings = new NcoaSettings();
            _sparkDataConfig = new SparkDataConfig();
            //Update _ncoaSettings

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA_NCOA, _ncoaSettings.ToJson() );
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );
        }

        #endregion

        #region Update Connection Status

        /// <summary>
        /// Handles the ItemDataBound event of the rptPersonConnectionStatusDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPersonConnectionStatusDataView_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            PersonConnectionStatusDataView personConnectionStatusDataView = e.Item.DataItem as PersonConnectionStatusDataView;
            HiddenField hfPersonConnectionStatusValueId = e.Item.FindControl( "hfPersonConnectionStatusValueId" ) as HiddenField;
            DataViewItemPicker dvpPersonConnectionStatusDataView = e.Item.FindControl( "dvpPersonConnectionStatusDataView" ) as DataViewItemPicker;
            if ( personConnectionStatusDataView != null )
            {
                hfPersonConnectionStatusValueId.Value = personConnectionStatusDataView.PersonConnectionStatusValue.Id.ToString();
                dvpPersonConnectionStatusDataView.EntityTypeId = CacheEntityType.GetId<Rock.Model.Person>();
                dvpPersonConnectionStatusDataView.Label = personConnectionStatusDataView.PersonConnectionStatusValue.ToString();
                dvpPersonConnectionStatusDataView.SetValue( personConnectionStatusDataView.DataViewId );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class PersonConnectionStatusDataView
        {
            /// <summary>
            /// Gets or sets the connection status value.
            /// </summary>
            /// <value>
            /// The connection status value.
            /// </value>
            public CacheDefinedValue PersonConnectionStatusValue { get; set; }

            /// <summary>
            /// Gets or sets the data view identifier.
            /// </summary>
            /// <value>
            /// The data view identifier.
            /// </value>
            public int? DataViewId { get; set; }
        }

        #endregion
    }
}