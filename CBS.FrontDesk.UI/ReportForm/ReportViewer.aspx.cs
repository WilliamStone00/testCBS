using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.ReportForm
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        /// <summary>
        /// Dynamically loads and renders a Crystal Report in the viewer UI based on session-passed data and parameters.
        ///
        /// ✅ **Supported Report Types:**
        /// - Reports with no parameters
        /// - Reports with parameters (passed as `Session["ReportParameters"]`)
        /// - Reports with no subreports
        /// - Reports with subreports (passed via `Session["SubReportsData"]`)
        /// - Reports where parameter and subreport names match exactly (case-insensitive)
        /// - Reports loaded via relative path in query string (`reportPath`)
        ///
        /// ❌ **Unsupported or Not Designed For:**
        /// - Reports requiring database login (e.g., SQL Server authentication at runtime)
        /// - Reports with nested subreports (subreports inside subreports)
        /// - Reports using runtime parameter prompts (disabled via `EnableParameterPrompt=false`)
        /// - Reports expecting dynamic subreport names not configured in advance
        ///
        /// ⚠️ **Note to Developers:**
        /// This method is a critical runtime component for Crystal Report rendering.
        /// ❌ DO NOT MODIFY this method without express written approval from the Technical Director or Lead Architect.
        /// Unauthorized changes can impact mission-critical reporting processes.
        ///
        /// 📅 **Created On:** April 18, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// © Copyright FLUX SARL. All rights reserved.
        /// </summary>

        // 🔁 Page_Init is used to re-bind the report on postbacks (e.g., pagination, export)
        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                // ✅ Restore cached report document from session
                if (Session["ReportDocument"] is ReportDocument cachedDoc)
                {
                    CrystalReportViewer1.ReportSource = cachedDoc;
                    CrystalReportViewer1.DataBind(); // Bind it again to allow paging/export
                }
                else
                {
                    // ❌ Show clear message if session expired
                    ShowError(@"
                        ⚠️ <strong>SESSION EXPIRED:</strong><br/>
                        The report session has expired or is unavailable.<br/>
                        Please return to the reporting module and regenerate the report.<br/><br/>
                        <a href='javascript:window.close();' style='color: red; font-weight: bold;'>Close this tab</a> or 
                        <a href='/Home/Index' style='color: green; font-weight: bold;'>Go to Home Page</a>.");
                }

                return;
            }
        }

        // 🚀 Page_Load is used only for first-time report setup
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // ✅ Handle export requests (PDF, Excel, Word) before rendering
                string exportAction = Request.QueryString["action"];
                if (!string.IsNullOrWhiteSpace(exportAction) && Session["ReportDocument"] is ReportDocument exportDoc)
                {
                    ExportFormatType format;
                    switch (exportAction.ToLowerInvariant())
                    {
                        case "exportpdf":
                            format = ExportFormatType.PortableDocFormat;
                            break;
                        case "exportexcel":
                            format = ExportFormatType.Excel;
                            break;
                        case "exportword":
                            format = ExportFormatType.WordForWindows;
                            break;
                        default:
                            ShowError("❌ <strong>Invalid Export Type:</strong><br/>Supported types: exportpdf, exportexcel, exportword.");
                            return;
                    }

                    string filename = $"{(ViewState["ReportName"] ?? "Report")}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    exportDoc.ExportToHttpResponse(format, Response, false, filename);
                    Response.End(); // Stop the pipeline
                    return;
                }

                // 🧭 Standard load on first visit only
                if (!IsPostBack)
                {
                    LoadAndBindCrystalReport();
                }
            }
            catch (Exception ex)
            {
                ShowError($"❌ <strong>Unexpected Error:</strong> Failed during export or load.<br/>Details: {ex.Message}");
            }
        }


        // 🔄 Loads and binds the Crystal Report for first-time rendering
        private void LoadAndBindCrystalReport()
        {
            try
            {
                // 📌 Extract query string parameters
                string relativePath = Request.QueryString["reportPath"];
                string displayName = Request.QueryString["reportName"];
                ViewState["ReportName"] = displayName ?? "Crystal Report";

                // 🔐 Validate report path
                if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
                {
                    ShowError("❌ <strong>Invalid Report Path:</strong> The specified report path is either missing or malformed.");
                    return;
                }

                // 🔍 Resolve the full server path to the report file
                string fullPath = Server.MapPath("~/AppFiles/Reporting/" + relativePath);

                // 📛 Check if file physically exists
                if (!File.Exists(fullPath))
                {
                    ShowError("❌ <strong>Report Not Found:</strong> The requested report file does not exist on the server.");
                    return;
                }

                // ❌ Ensure main report data is still in session
                if (Session["MainData"] == null)
                {
                    ShowError(@"
                    ⚠️ <strong>SESSION EXPIRED:</strong><br/>
                    The data required to generate this report is no longer available in memory.<br/>
                    Please return to the reporting module and regenerate the report.<br/><br/>
                    <a href='javascript:window.close();' style='color: red; font-weight: bold;'>Close this tab</a> or 
                    <a href='/Home/Index' style='color: green; font-weight: bold;'>Go to Home Page</a>.");
                    return;
                }

                // 📄 Initialize and load report file
                ReportDocument rd = new ReportDocument();
                rd.Load(fullPath);

                // 🧩 Bind main report data from session
                rd.SetDataSource(Session["MainData"]);

                // 🔗 Bind subreport data (if available)
                if (Session["SubReportsData"] is Dictionary<string, object> subReports)
                {
                    foreach (var sub in subReports)
                    {
                        if (sub.Value != null)
                        {
                            try
                            {
                                // 🧾 Ensure subreport name ends with `.rpt`
                                string subreportName = sub.Key.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase)
                                    ? sub.Key
                                    : sub.Key + ".rpt";

                                // ✅ Check if subreport exists and bind its data
                                if (rd.Subreports.Cast<ReportDocument>()
                                    .Any(s => s.Name.Equals(subreportName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    rd.Subreports[subreportName].SetDataSource(sub.Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                // ⚠️ Display detailed error if a subreport fails
                                ShowError($"❌ <strong>Subreport Error:</strong> Failed to load subreport <code>{sub.Key}</code>.<br/>Details: {ex.Message}");
                                return;
                            }
                        }
                    }
                }

                // 🧷 Bind report parameters if any are defined
                if (Session["ReportParameters"] is Dictionary<string, object> parameters && rd.DataDefinition.ParameterFields.Count > 0)
                {
                    foreach (var param in parameters)
                    {
                        try
                        {
                            // Only bind if parameter actually exists in the report
                            if (rd.DataDefinition.ParameterFields.Cast<ParameterFieldDefinition>()
                                .Any(p => p.Name.Equals(param.Key, StringComparison.OrdinalIgnoreCase)))
                            {
                                rd.SetParameterValue(param.Key, param.Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowError($"⚠️ <strong>Parameter Error:</strong> Failed to apply parameter <code>{param.Key}</code>.<br/>Details: {ex.Message}");
                            return;
                        }
                    }
                }

                // 🖼️ Assign and bind final document to viewer
                CrystalReportViewer1.ReportSource = rd;
                CrystalReportViewer1.DataBind();

                // 🧠 Cache the document in session for paging/export/postback
                Session["ReportDocument"] = rd;
            }
            catch (Exception ex)
            {
                ShowError($"❌ <strong>Unexpected Error:</strong> An unexpected issue occurred while loading the report.<br/>Details: {ex.Message}");
            }
        }

        // 🚨 Helper to show styled error messages in full HTML view
        private void ShowError(string messageHtml)
        {
            Response.Clear();
            Response.ContentType = "text/html";
            Response.Write($@"
                <html>
                <head>
                    <title>TRUST SOFT CREDIT - REPORT VIEWER</title>
                    <style>
                        body {{
                            font-family: Segoe UI, Arial, sans-serif;
                            padding: 0;
                            margin: 0;
                            background-color: #f5f7fa;
                            color: #333;
                        }}
                        .container {{
                            max-width: 800px;
                            margin: 60px auto;
                            background: #fff;
                            border: 1px solid #ccc;
                            border-radius: 8px;
                            padding: 30px;
                            box-shadow: 0 0 12px rgba(0,0,0,0.1);
                        }}
                        .header-line {{
                            height: 3px;
                            background-color: #007f45;
                            margin: 10px 0 25px 0;
                        }}
                        .header {{
                            text-align: center;
                            font-size: 20px;
                            font-weight: bold;
                            text-transform: uppercase;
                            color: #007f45;
                        }}
                        .sub-header {{
                            text-align: center;
                            font-size: 14px;
                            font-weight: bold;
                            text-transform: uppercase;
                            margin-bottom: 30px;
                            color: #333;
                        }}
                        .message {{
                            font-size: 16px;
                            line-height: 1.7;
                            text-align: center;
                        }}
                        .footer {{
                            margin-top: 40px;
                            font-size: 13px;
                            color: #999;
                            text-align: center;
                            border-top: 1px solid #eee;
                            padding-top: 12px;
                        }}
                        a {{
                            color: #007f45;
                            text-decoration: none;
                            font-weight: bold;
                        }}
                        a:hover {{
                            text-decoration: underline;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>TRUST SOFT CREDIT</div>
                        <div class='sub-header'>REPORT VIEWER NOTICE</div>
                        <div class='header-line'></div>
                        <div class='message'>
                            {messageHtml}
                        </div>
                        <div class='footer'>
                            Powered by Flux SARL<br/>
                            &copy; 2019 - {DateTime.Now.Year} All rights reserved.
                        </div>
                    </div>
                </body>
                </html>");
            Response.End(); // Stop execution after displaying error
        }
    }

}