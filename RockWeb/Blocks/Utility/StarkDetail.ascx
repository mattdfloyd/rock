<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StarkDetail.ascx.cs" Inherits="RockWeb.Blocks.Utility.StarkDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i> 
                    Blank Detail Block
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <div class="alert alert-info">
                    <asp:LinkButton ID="btnGetAddresses" runat="server" CssClass="btn btn-default" OnClick="btnGetAddresses_Click">GetAddresses</asp:LinkButton>
                    <asp:LinkButton ID="btnUploadAddresses" runat="server" CssClass="btn btn-default" OnClick="btnUploadAddresses_Click">UploadAddresses</asp:LinkButton>
                    <asp:LinkButton ID="btnCreateReport" runat="server" CssClass="btn btn-default" OnClick="btnCreateReport_Click">CreateReport</asp:LinkButton>
                    <asp:LinkButton ID="btnIsReportCreated" runat="server" CssClass="btn btn-default" OnClick="btnIsReportCreated_Click">IsReportCreated</asp:LinkButton>
                    <asp:LinkButton ID="btnCreateReportExport" runat="server" CssClass="btn btn-default" OnClick="btnCreateReportExport_Click">CreateReportExport</asp:LinkButton>
                    <asp:LinkButton ID="btnIsReportExportCreated" runat="server" CssClass="btn btn-default" OnClick="btnIsReportExportCreated_Click">IsReportExportCreated</asp:LinkButton>
                    <asp:LinkButton ID="btnDownloadExport" runat="server" CssClass="btn btn-default" OnClick="btnDownloadExport_Click">DownloadExport</asp:LinkButton>
                    <asp:LinkButton ID="btnSaveRecords" runat="server" CssClass="btn btn-default" OnClick="btnSaveRecords_Click">CreateReport</asp:LinkButton>
                    <asp:LinkButton ID="btnParseData" runat="server" CssClass="btn btn-default" OnClick="btnParseData_Click">ParseData</asp:LinkButton>
                </div>
                <asp:PlaceHolder ID="ph1" runat="server" />
                <Rock:ControlMirror ID="mMirror" ControlID="ph1" runat="server" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>