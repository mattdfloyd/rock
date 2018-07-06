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
using System.ComponentModel;
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
using Rock.Utility;
using Rock.Utility.SparkDataApi;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Spark Data Settings
    /// </summary>
    [DisplayName( "Spark Data Settings" )]
    [Category( "Administration" )]
    [Description( "Block used to set values specific to Spark Data (NCOA, Etc)." )]
    public partial class SparkDataSettings : RockBlock
    {
        #region private variables

        private RockContext _rockContext = new RockContext();

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
            dvpPersonDataView.EntityTypeId = CacheEntityType.GetId<Rock.Model.Person>();
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
        /// Handles the Click event of the btnSaveLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveLogin_Click( object sender, EventArgs e )
        {
            // Get Spark Data
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

            _sparkDataConfig.GlobalNotificationApplicationGroupId = grpNotificationGroupLogin.GroupId;
            _sparkDataConfig.SparkDataApiKey = txtSparkDataApiKeyLogin.Text;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            GetSettings();
        }

        /// <summary>
        /// Handles the Click event of the btnUpdateSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateSettings_Click( object sender, EventArgs e )
        {
            pnlSparkDataEdit.Visible = true;
            pnlSignIn.Visible = false;
            pnlAccountStatus.Visible = false;
            pwNcoaConfiguration.Visible = false;
            bbtnSaveConfig.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelEdit_Click( object sender, EventArgs e )
        {
            GetSettings();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveEdit_Click( object sender, EventArgs e )
        {
            // Get Spark Data
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

            _sparkDataConfig.GlobalNotificationApplicationGroupId = grpNotificationGroupEdit.GroupId;
            _sparkDataConfig.SparkDataApiKey = txtSparkDataApiKeyEdit.Text;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            GetSettings();
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

        /// <summary>
        /// Handles the CheckedChanged event when enabling/disabling the recurring enabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbRecurringEnabled_CheckedChanged( object sender, EventArgs e )
        {
            nbRecurrenceInterval.Enabled = cbRecurringEnabled.Checked;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAcceptTerms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAcceptTerms_CheckedChanged( object sender, EventArgs e )
        {
            // Update Spark Data settings
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

            if ( _sparkDataConfig.NcoaSettings == null )
            {
                _sparkDataConfig.NcoaSettings = new NcoaSettings();
            }

            _sparkDataConfig.NcoaSettings.IsAcceptedTerms = cbAcceptTerms.Checked;
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Update if Run Manually button is enabled
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbAckPrice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAckPrice_CheckedChanged( object sender, EventArgs e )
        {
            // Update Spark Data settings
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

            if ( _sparkDataConfig.NcoaSettings == null )
            {
                _sparkDataConfig.NcoaSettings = new NcoaSettings();
            }

            _sparkDataConfig.NcoaSettings.IsAckPrice = cbAckPrice.Checked;
            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Update if Run Manually button is enabled
            SetStartNcoaEnabled();
        }

        /// <summary>
        /// Handles the Click event of the btnStartNcoa control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStartNcoa_Click( object sender, EventArgs e )
        {
            Ncoa ncoa = new Ncoa();
            var sparkDataConfig = Ncoa.GetSettings();
            sparkDataConfig.NcoaSettings.PersonAliasId = CurrentPersonAliasId;
            sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
            Ncoa.SaveSettings( sparkDataConfig );
            using ( RockContext rockContext = new RockContext() )
            {
                ServiceJob job = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.GET_NCOA.AsGuid() );
                if ( job != null )
                {
                    var transaction = new Rock.Transactions.RunJobNowTransaction( job.Id );

                    // Process the transaction on another thread
                    System.Threading.Tasks.Task.Run( () => transaction.Execute() );

                    mdGridWarning.Show( string.Format( "The '{0}' job has been started.", job.Name ), ModalAlertType.Information );
                    lbStartNcoa.Enabled = false;
                }
            }
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
            SetStartNcoaEnabled();
        }

        private void SetStartNcoaEnabled()
        {
            if ( _sparkDataConfig == null || _sparkDataConfig.NcoaSettings == null )
            {
                _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

                // Get NCOA settings
                if ( _sparkDataConfig.NcoaSettings == null )
                {
                    _sparkDataConfig.NcoaSettings = new NcoaSettings();
                }
            }

            if ( _sparkDataConfig.NcoaSettings.CurrentReportStatus.Contains( "Pending" ) )
            {
                lbStartNcoa.Enabled = false;
            }
            else
            {
                lbStartNcoa.Enabled = cbAcceptTerms.Checked && cbAckPrice.Checked && cbNcoaConfiguration.Checked;
            }
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

            panelWidget.Title = string.Format( "<h3 class='panel-title pull-left margin-r-sm'>{0}</h3> {1} ", title, enabledLabel );
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        private void GetSettings()
        {
            // Get Spark Data settings
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();
            if ( _sparkDataConfig.SparkDataApiKey.IsNullOrWhiteSpace() )
            {
                pnlSparkDataEdit.Visible = false;
                pnlSignIn.Visible = true;
                pnlAccountStatus.Visible = false;
                pwNcoaConfiguration.Visible = false;
                bbtnSaveConfig.Visible = false;

                txtSparkDataApiKeyLogin.Text = _sparkDataConfig.SparkDataApiKey;
                grpNotificationGroupLogin.GroupId = _sparkDataConfig.GlobalNotificationApplicationGroupId;
            }
            else
            {
                pnlSparkDataEdit.Visible = false;
                pnlSignIn.Visible = false;
                pnlAccountStatus.Visible = true;
                pwNcoaConfiguration.Visible = true;
                bbtnSaveConfig.Visible = true;

                // Get Ncoa configuration settings
                nbMinMoveDistance.Text = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_MINIMUM_MOVE_DISTANCE_TO_INACTIVATE );
                cb48MonAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_48_MONTH_AS_PREVIOUS ).AsBoolean();
                cbInvalidAddressAsPrevious.Checked = Rock.Web.SystemSettings.GetValue( SystemSetting.NCOA_SET_INVALID_AS_PREVIOUS ).AsBoolean();

                txtSparkDataApiKeyEdit.Text = _sparkDataConfig.SparkDataApiKey;
                grpNotificationGroupEdit.GroupId = _sparkDataConfig.GlobalNotificationApplicationGroupId;

                // Get NCOA settings
                if ( _sparkDataConfig.NcoaSettings == null )
                {
                    _sparkDataConfig.NcoaSettings = new NcoaSettings();
                }

                dvpPersonDataView.SelectedValue = _sparkDataConfig.NcoaSettings.PersonDataViewId.ToStringSafe();
                cbRecurringEnabled.Checked = _sparkDataConfig.NcoaSettings.RecurringEnabled;
                nbRecurrenceInterval.Enabled = _sparkDataConfig.NcoaSettings.RecurringEnabled;
                nbRecurrenceInterval.Text = _sparkDataConfig.NcoaSettings.RecurrenceInterval.ToStringSafe();
                cbNcoaConfiguration.Checked = _sparkDataConfig.NcoaSettings.IsEnabled;
                cbAcceptTerms.Checked = _sparkDataConfig.NcoaSettings.IsAcceptedTerms;
                cbAckPrice.Checked = _sparkDataConfig.NcoaSettings.IsAckPrice;

                nbCreditCard.Visible = false;

                if ( _sparkDataConfig.NcoaSettings.CurrentReportStatus == null )
                {
                    _sparkDataConfig.NcoaSettings.CurrentReportStatus = string.Empty;
                }

                SetStartNcoaEnabled();

                if ( _sparkDataConfig.SparkDataApiKey.IsNullOrWhiteSpace() )
                {
                    pnlSignIn.Visible = true;
                    pnlSparkDataEdit.Visible = false;

                }
                else
                {
                    pnlSignIn.Visible = false;

                    SparkDataApi sparkDataApi = new SparkDataApi();
                    try
                    {
                        var accountStatus = sparkDataApi.CheckAccount( _sparkDataConfig.SparkDataApiKey );
                        switch ( accountStatus )
                        {
                            case SparkDataApi.AccountStatus.AccountNoName:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Account does not have a Name";
                                break;
                            case SparkDataApi.AccountStatus.AccountNotFound:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Account not Found";
                                break;
                            case SparkDataApi.AccountStatus.Disabled:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Disabled";
                                break;
                            case SparkDataApi.AccountStatus.EnabledCardExpired:
                                hlAccountStatus.LabelType = LabelType.Danger;
                                hlAccountStatus.Text = "Enabled - Card Expired";
                                break;
                            case SparkDataApi.AccountStatus.EnabledNoCard:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Enabled - No Card on File";
                                nbCreditCard.Visible = true;
                                break;
                            case SparkDataApi.AccountStatus.EnabledCard:
                                hlAccountStatus.LabelType = LabelType.Success;
                                hlAccountStatus.Text = "Enabled - Card on File";
                                break;
                            case SparkDataApi.AccountStatus.InvalidSparkDataKey:
                                hlAccountStatus.LabelType = LabelType.Warning;
                                hlAccountStatus.Text = "Account does not have a Name";
                                break;
                        }

                        string cost = sparkDataApi.GetPrice( "CF20766E-80F9-E282-432F-6A9E19F0BFF1" );
                        cbAckPrice.Text = cbAckPrice.Text.Replace( "$xx", "$" + cost );
                    }
                    catch
                    {
                        hlAccountStatus.LabelType = LabelType.Danger;
                        hlAccountStatus.Text = "Error Connecting to Spark Server";
                    }
                }
            }
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

            // Get Spark Data
            _sparkDataConfig = Rock.Web.SystemSettings.GetValue( SystemSetting.SPARK_DATA ).FromJsonOrNull<SparkDataConfig>() ?? new SparkDataConfig();

            // Get NCOA settings
            if ( _sparkDataConfig.NcoaSettings == null )
            {
                _sparkDataConfig.NcoaSettings = new NcoaSettings();
            }

            _sparkDataConfig.NcoaSettings.PersonDataViewId = dvpPersonDataView.SelectedValue.AsIntegerOrNull();
            _sparkDataConfig.NcoaSettings.RecurringEnabled = cbRecurringEnabled.Checked;
            _sparkDataConfig.NcoaSettings.RecurrenceInterval = nbRecurrenceInterval.Text.AsInteger();
            _sparkDataConfig.NcoaSettings.IsEnabled = cbNcoaConfiguration.Checked;
            _sparkDataConfig.NcoaSettings.IsAckPrice = cbAckPrice.Checked;
            _sparkDataConfig.NcoaSettings.IsAcceptedTerms = cbAcceptTerms.Checked;

            Rock.Web.SystemSettings.SetValue( SystemSetting.SPARK_DATA, _sparkDataConfig.ToJson() );

            // Save job active status
            using ( var rockContext = new RockContext() )
            {
                var ncoaJob = new ServiceJobService( rockContext ).Get( Rock.SystemGuid.ServiceJob.GET_NCOA.AsGuid() );
                if ( ncoaJob != null )
                {
                    ncoaJob.IsActive = cbNcoaConfiguration.Checked;
                    rockContext.SaveChanges();
                }
            }
        }

        #endregion
    }
}