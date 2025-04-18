<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="CBS.FrontDesk.UI.ReportForm.ReportViewer" %>
<%@ Register Assembly="CrystalDecisions.Web" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TSC - Report Viewer</title>
    <script src='<%= ResolveUrl("~/aspnet_client/system_web/4_0_30319/crystalreportviewers13/js/crviewer/crv.js") %>' type="text/javascript"></script>

    <style type="text/css">
        html, body {
            margin: 0;
            padding: 0;
            height: 100vh; /* Full viewport height */
            overflow: hidden;
        }

        form {
            height: 100%;
            display: flex;
            flex-direction: column;
        }

        .report-header {
            text-align: center;
            color: navy;
            font-weight: bold;
            text-transform: uppercase;
            margin: 0;
            padding: 10px;
            flex-shrink: 0;
        }

        .report-viewer-container {
            flex: 1; /* Takes the remaining space */
            overflow: hidden;
        }

        .report-viewer-container > div {
            height: 100% !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h3 class="report-header">
            <%= ViewState["ReportName"] != null ? ViewState["ReportName"].ToString() : "Crystal Report Viewer" %>
        </h3>

        <div class="report-viewer-container">
            <CR:CrystalReportViewer 
                ID="CrystalReportViewer1"
                runat="server"
                AutoDataBind="true"
                EnableDatabaseLogonPrompt="false"
                EnableParameterPrompt="false"
                HasPrintButton="true"
                HasExportButton="true"
                HasZoomFactorList="true"
                HasToggleGroupTreeButton="true"
                HasGotoPageButton="true"
                HasSearchButton="true"
                HasRefreshButton="true"
                DisplayToolbar="true"
                ReuseParameterValuesOnRefresh="true"
                ToolPanelView="None"
                BestFitPage="true"
                Width="100%" 
                Height="100%" />
        </div>
    </form>
</body>
</html>
