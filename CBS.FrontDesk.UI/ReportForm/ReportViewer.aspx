<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="CBS.FrontDesk.UI.ReportForm.ReportViewer" %>
<%@ Register Assembly="CrystalDecisions.Web" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TSC - Report Viewer</title>

    <!-- 🌐 Font Awesome for icons -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet" />

    <script src='<%= ResolveUrl("~/aspnet_client/system_web/4_0_30319/crystalreportviewers13/js/crviewer/crv.js") %>' type="text/javascript"></script>

    <style type="text/css">
        html, body {
            margin: 0;
            padding: 0;
            height: 100%;
            overflow: hidden; /* ❌ Prevent page-level scroll */
            font-family: 'Segoe UI', sans-serif;
            background-color: #f9f9f9;
        }

        form {
            height: 100%;
            display: flex;
            flex-direction: column;
            overflow: hidden; /* ❌ Prevent scroll in form */
        }

        .report-header {
            text-align: center;
            color: #005086;
            font-weight: bold;
            text-transform: uppercase;
            padding: 16px;
            background-color: #f4f6f8;
            font-size: 20px;
            border-bottom: 2px solid #e0e0e0;
            flex-shrink: 0;
        }

        .export-bar {
            text-align: center;
            padding: 14px 0;
            background-color: #ffffff;
            border-bottom: 1px solid #ddd;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
            flex-shrink: 0;
        }

        .export-bar a {
            display: inline-block;
            margin: 0 8px;
            padding: 10px 18px;
            border-radius: 25px;
            background-color: #005086;
            color: white;
            font-weight: 600;
            font-size: 14px;
            text-decoration: none;
            transition: all 0.3s ease;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        .export-bar a i {
            margin-right: 6px;
        }

        .export-bar a:hover {
            background-color: #003b66;
            transform: translateY(-2px);
        }

        .report-viewer-container {
            flex-grow: 1;
            overflow: auto; /* ✅ Scroll only here */
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
        <!-- 🔷 Report Title -->
        <div class="report-header">
            <%= ViewState["ReportName"] != null ? ViewState["ReportName"].ToString() : "Crystal Report Viewer" %>
        </div>

        <!-- 🔘 Export Buttons -->
        <div class="export-bar">
            <a href='<%= Request.RawUrl + (Request.RawUrl.Contains("?") ? "&" : "?") + "action=exportpdf" %>' target="_blank">
                <i class="fas fa-file-pdf"></i> Export to PDF
            </a>
            <a href='<%= Request.RawUrl + (Request.RawUrl.Contains("?") ? "&" : "?") + "action=exportexcel" %>' target="_blank">
                <i class="fas fa-file-excel"></i> Export to Excel
            </a>
            <a href='<%= Request.RawUrl + (Request.RawUrl.Contains("?") ? "&" : "?") + "action=exportword" %>' target="_blank">
                <i class="fas fa-file-word"></i> Export to Word
            </a>
        </div>

        <!-- 📄 Report Viewer Panel -->
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
