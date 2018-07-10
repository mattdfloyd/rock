<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SparkDataSettings.ascx.cs" Inherits="RockWeb.Blocks.Administration.SparkDataSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-exclamation"></i>
                    Spark Data
                </h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />

                <fieldset>

                    <asp:Panel ID="pnlSignIn" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-2">
                                        <asp:Image ID="imgCheckrImage" runat="server" CssClass="img-responsive" style=" max-width: 25%;"/>
                                    </div>
                                    <div class="col-md-10">
                                        <div class="row">
                                            <h1>Enhance Your Data</h1>
                                            
                                            <p> Spark Data is a set of services that allows you to easily clean and enhance your data with
                                            little effort. Before you can begin you'll need to get an API key from the Rock RMS website
                                            and ensure that a credit card is on file for use with paid services.</p>
                                            <p><a href="https://www.rockrms.com/">Sign-up Now</a></p>
                                        </div>
                                        <asp:ValidationSummary ID="vsSignIn" runat="server" HeaderText="Please Correct the Following" ValidationGroup="SignInValidationGroup" CssClass="alert alert-validation" />
                                        <div class="row">
                                            <div class="col-md-6">
                                                <Rock:RockTextBox ID="txtSparkDataApiKeyLogin" runat="server" Label="Spark Data Api Key" Required="true" ValidationGroup="SignInValidationGroup"/>
                                            </div>
                                            <div class="col-md-6">
                                                <Rock:GroupPicker ID="grpNotificationGroupLogin" runat="server" Label="Notification Group" Help="Members of this group will recieve notifications when specific jobs and tasks complete." />
                                            </div>
                                        </div>
                                        <asp:LinkButton ID="btnSaveLogin" runat="server" CssClass="btn btn-primary" OnClick="btnSaveLogin_Click" ValidationGroup="SignInValidationGroup" >Save</asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlSparkDataEdit" runat="server" Visible="false">
                        <p> For more information about your account, or to update your payment information please visit your organization's profile on the
                        Rock RMS website.</p>
                        <p><a href="https://www.rockrms.com/">Organization Profile</a></p>
                        <asp:ValidationSummary ID="vsSparkDataEdit" runat="server" HeaderText="Please Correct the Following" ValidationGroup="SparkDataEditValidationGroup" CssClass="alert alert-validation" />
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="txtSparkDataApiKeyEdit" runat="server" Label="Spark Data Api Key" Required="true" ValidationGroup="SparkDataEditValidationGroup" />
                            </div>
                            <div class="col-md-6">
                                <Rock:GroupPicker ID="grpNotificationGroupEdit" runat="server" Label="Notification Group" Help="Members of this group will recieve notifications when specific jobs and tasks complete." />
                            </div>
                        </div>
                        <asp:LinkButton ID="btnSaveEdit" runat="server" CssClass="btn btn-primary" OnClick="btnSaveEdit_Click" ValidationGroup="SparkDataEditValidationGroup" >Save</asp:LinkButton>
                        <asp:LinkButton ID="btnCancelEdit" runat="server" CssClass="btn btn-default" OnClick="btnCancelEdit_Click">Cancel</asp:LinkButton>
                    </asp:Panel>

                    <asp:Panel ID="pnlAccountStatus" runat="server" CssClass="panel panel-widget panel-heading">
                        Spark Data Status
                                    <Rock:HighlightLabel ID="hlAccountStatus" runat="server" LabelType="Success" Text="" />
                        <div class="pull-right">
                        <asp:LinkButton ID="btnUpdateSettings" runat="server" CssClass="btn btn-default btn-sm" OnClick="btnUpdateSettings_Click" >Update Settings</asp:LinkButton>
                            </div>
                    </asp:Panel>

                    <Rock:PanelWidget ID="pwNcoaConfiguration" runat="server" Title="National Change of Address (NCOA)">
                        <Rock:NotificationBox ID="nbCreditCard" runat="server" NotificationBoxType="Warning"
                            Heading="Note" Text=" This service requires a credit card on file to process payments for running the files."/>
                        <asp:ValidationSummary ID="vsNcoa" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" ValidationGroup="NcoaValidationGroup" />
                        <div class="row">
                            <div class="col-md-12">
                                <div class="pull-left">
                                    <Rock:RockCheckBox ID="cbNcoaConfiguration" runat="server"
                                        Label="Enable" Text="Enable the automatic updating of change of addresses via NCOA services."
                                        AutoPostBack="true" OnCheckedChanged="cbNcoaConfiguration_CheckedChanged" />
                                </div>
                                <div class="pull-right">
                                    <asp:LinkButton ID="lbStartNcoa" runat="server" CssClass="btn btn-default" OnClick="btnStartNcoa_Click" ToolTip="Start NCOA"><i class="fa fa-play"></i> Run Manually</asp:LinkButton>
                                </div>
                            </div>
                        </div>

                        <hr />
                        <asp:Panel ID="pnlNcoaConfiguration" runat="server" Enabled="false" CssClass="data-integrity-options">
                            <asp:CheckBox ID="cbAcceptTerms" runat="server" AutoPostBack="true" OnCheckedChanged="cbAcceptTerms_CheckedChanged" Text="By accepting these terms, you agree that Rock RMS may share your data with TrueNCOA for NCOA processing. You understand that through your use
                                of the Services you consent to the collection and use of this information, including the storage, processing and use by TrueNCOA and its affiliates. Customer
                                information will only be shared by TrueNCOA to provide or improve our products, services and advertising; it will not be shared with third parties for their
                                marketing purposes. Read TrueNCOA’s full Terms of Service here, and read TrueNCOA’s Privacy Policy here." />
                            <asp:CheckBox ID="cbAckPrice" runat="server" AutoPostBack="true" OnCheckedChanged="cbAckPrice_CheckedChanged" Text="I ackowledge that running this service will change the card on file &#36;xx for each file run." />
                            <br />
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:DataViewPicker ID="dvpPersonDataView" runat="server" Label="Person Data View" Required="true" ValidationGroup="NcoaValidationGroup" Help="Person data view filter to apply." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbMinMoveDistance" runat="server" AppendText="miles" CssClass="input-width-md" Label="Minimum Move Distance to Inactivate" NumberType="Double" Text="250" Help="Minimum move distance that a person moved before marking the person's account to inactivate" />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cb48MonAsPrevious" runat="server" Label="Mark 48 Month Move as Previous Addresses" Help="Mark moves in the 19-48 month catagory as a previous address." />
                                </div>
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbInvalidAddressAsPrevious" runat="server" Label="Mark Invalid Addresses as Previous Addresses" Help="Mark Invalid Addresses as Previous Addresses"/>
                                </div>
                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-md-4">
                                    <Rock:RockCheckBox ID="cbRecurringEnabled" runat="server" Label="Recurring Enabled" OnCheckedChanged="cbRecurringEnabled_CheckedChanged" AutoPostBack="true" Help="Should the job run periodically"/>
                                </div>
                                <div class="col-md-4">
                                    <Rock:NumberBox ID="nbRecurrenceInterval" runat="server" AppendText="Days" CssClass="input-width-md" Label="Recurrence Interval" NumberType="Integer" Text="95" Required="true" ValidationGroup="NcoaValidationGroup" Help="After how many days should the job automatically start after the last successful run" />
                                </div>
                            </div>
                            <div class="actions margin-t-lg">
                                <Rock:BootstrapButton ID="bbtnSaveConfig" runat="server" CssClass="btn btn-primary" AccessKey="s" ToolTip="Alt+s" OnClick="bbtnSaveConfig_Click" Text="Save"
                                    DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Saving" ValidationGroup="NcoaValidationGroup"
                                    CompletedText="Success" CompletedMessage="&nbsp;Changes Have Been Saved!" CompletedDuration="2">
                                </Rock:BootstrapButton>
                            </div>
                        </asp:Panel>
                    </Rock:PanelWidget>

                </fieldset>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
