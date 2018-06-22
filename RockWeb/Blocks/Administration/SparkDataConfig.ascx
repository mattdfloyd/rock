<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SparkDataConfig.ascx.cs" Inherits="RockWeb.Blocks.Administration.SparkDataConfig" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tachometer"></i>
                    Data Automation Settings
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />

                <Rock:PanelWidget ID="pwAccountSettings" runat="server" Title="Account Settings">
                    <Rock:NumberBox ID="nbGenderAutoFill" runat="server" AppendText="%" CssClass="input-width-md" Label="Gender AutoFill Confidence" MinimumValue="0" MaximumValue="100" NumberType="Double" Help="The minimum confidence level required to automatically set blank genders in the Data Automation service job. If set to 0 then gender will not be automatically determined." />
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwNcoaConfiguration" runat="server" Title="National Change of Address (NCOA)">
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
                </Rock:PanelWidget>

                <div class="actions margin-t-lg">
                    <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save"
                        DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving"
                        CompletedText="Success" CompletedMessage="&nbsp;Changes Have Been Saved!" CompletedDuration="2"></Rock:BootstrapButton>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
