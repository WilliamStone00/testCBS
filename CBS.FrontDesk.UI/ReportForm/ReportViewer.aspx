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
            height: 100%;
            overflow: auto; /* ✅ Allow scroll if report is taller than viewport */
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
            padding: 12px;
            border-bottom: 1px solid #ccc;
            flex-shrink: 0;
            background-color: #f9f9f9;
        }

        .report-viewer-container {
            flex-grow: 1;
            overflow: auto;
            padding: 0;
        }

        .report-viewer-container > div {
            min-height: 100%;
        }

        #CrystalReportViewer1 {
            width: 100%;
            height: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="report-header">
            <%= ViewState["ReportName"] != null ? ViewState["ReportName"].ToString() : "Crystal Report Viewer" %>
        </div>

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
