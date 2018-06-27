<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SparkDataSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.SparkDataSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Spark Data Settings
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />

                <fieldset>
                    <legend>
                        Spark Data
                    </legend>

                    <Rock:PanelWidget ID="pwGeneralSettings" runat="server" Title="General Settings">
                        <div class="row">
                            <div class="col-md-4">
                                <Rock:RockTextBox ID="txtSparkDataApiKey" runat="server" Label="Spark Data Api Key" Required="true" />
                            </div>
                            <div class="col-md-4">
                                <Rock:GroupRolePicker ID="grpNotificationGroup" runat="server" Label="Global Notification Application Group" />
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="pwNcoaConfiguration" runat="server" Title="National Change of Address (NCOA)">
                        <Rock:RockCheckBox ID="cbNcoaConfiguration" runat="server"
                            Label="Enable" Text="Enable the automatic updating of change of addresses via NCOA services."
                            AutoPostBack="true" OnCheckedChanged="cbSparkDataEnabled_CheckedChanged" />

                        <hr />

                        <asp:Panel ID="pnlNcoaConfiguration" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbMinMoveDistance" runat="server" AppendText="miles" CssClass="input-width-md" Label="Minimum Move Distance to Inactivate" NumberType="Double" Text="250" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cb48MonAsPrevious" runat="server" Label="Mark 48 Month Move as Previous Addresses" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbInvalidAddressAsPrevious" runat="server" Label="Mark Invalid Addresses as Previous Addresses" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:DataViewPicker ID="dvpPersonDataView" runat="server" Label="Person Data View" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbRecurringEnabled" runat="server" Label="Recurring Enabled" OnCheckedChanged="cbRecurringEnabled_CheckedChanged" AutoPostBack="true"/>
                                </div>
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbRecurrenceInterval" runat="server" AppendText="Days" CssClass="input-width-md" Label="Recurrence Interval" NumberType="Integer" Text="95" Required="true" />
                                </div>
                            </div>
                        </asp:Panel>
                    </Rock:PanelWidget>

                </fieldset>

                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save"
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="&nbsp;Changes Have Been Saved!" CompletedDuration="2">
                    </Rock:BootstrapButton>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
