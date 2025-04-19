using CrystalDecisions.CrystalReports.Engine;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            // Ensure logic only runs on initial load (not postbacks)
            if (!IsPostBack)
            {
                try
                {
                    string relativePath = Request.QueryString["reportPath"];
                    string displayName = Request.QueryString["reportName"];
                    ViewState["ReportName"] = displayName ?? "Crystal Report";

                    // Path validation
                    if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
                    {
                        ShowError("❌ <strong>Invalid Report Path:</strong> The specified report path is either missing or malformed.");
                        return;
                    }

                    string fullPath = Server.MapPath("~/AppFiles/Reporting/" + relativePath);

                    // File existence check
                    if (!File.Exists(fullPath))
                    {
                        ShowError("❌ <strong>Report Not Found:</strong> The requested report file does not exist on the server.");
                        return;
                    }

                    // Session check for report data
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

                    // Load the report
                    ReportDocument rd = new ReportDocument();
                    rd.Load(fullPath);

                    // Bind main report data
                    rd.SetDataSource(Session["MainData"]);

                    // Bind subreport data (if available)
                    if (Session["SubReportsData"] is Dictionary<string, object> subReports)
                    {
                        foreach (var sub in subReports)
                        {
                            if (sub.Value != null)
                            {
                                try
                                {
                                    string subreportName = sub.Key.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase)
                                        ? sub.Key
                                        : sub.Key + ".rpt";

                                    if (rd.Subreports.Cast<ReportDocument>().Any(s => s.Name.Equals(subreportName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        rd.Subreports[subreportName].SetDataSource(sub.Value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ShowError($"❌ <strong>Subreport Error:</strong> Failed to load subreport <code>{sub.Key}</code>.<br/>Details: {ex.Message}");
                                    return;
                                }
                            }
                        }
                    }

                    // Set parameters if available
                    if (Session["ReportParameters"] is Dictionary<string, object> parameters)
                    {
                        foreach (var param in parameters)
                        {
                            try
                            {
                                if (rd.DataDefinition.ParameterFields[param.Key] != null)
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

                    // Assign and render the report
                    CrystalReportViewer1.ReportSource = rd;
                    CrystalReportViewer1.DataBind();
                }
                catch (Exception ex)
                {
                    // Catch-all fallback for unexpected issues
                    ShowError($"❌ <strong>Unexpected Error:</strong> An unexpected issue occurred while loading the report.<br/>Details: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Displays an explicit error block to the user in HTML.
        /// </summary>
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
                .header {{
                    text-align: center;
                    font-size: 20px;
                    font-weight: bold;
                    text-transform: uppercase;
                    color: #005086;
                    margin-bottom: 5px;
                }}
                .header-line {{
                    height: 3px;
                    background-color: #28a745;
                    margin: 0 auto 25px auto;
                    width: 100%;
                }}
                .sub-header {{
                    text-align: center;
                    font-size: 16px;
                    font-weight: bold;
                    text-transform: uppercase;
                    margin-bottom: 30px;
                    color: #333;
                }}
                .message {{
                    font-size: 16px;
                    line-height: 1.7;
                    text-align: center;
                    margin-bottom: 30px;
                }}
                .actions {{
                    text-align: center;
                    margin-bottom: 40px;
                }}
                .btn {{
                    display: inline-block;
                    background-color: #005086;
                    color: white;
                    padding: 10px 20px;
                    margin: 5px 10px;
                    border-radius: 5px;
                    font-weight: bold;
                    text-decoration: none;
                }}
                .btn:hover {{
                    background-color: #003f6b;
                }}
                .footer {{
                    font-size: 13px;
                    color: #999;
                    text-align: center;
                    border-top: 1px solid #eee;
                    padding-top: 12px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>TRUST SOFT CREDIT</div>
                <div class='header-line'></div>
                <div class='sub-header'>REPORT VIEWER NOTICE</div>
                <div class='message'>
                    {messageHtml}
                </div>
                <div class='actions'>
                    <a href='javascript:window.close();' class='btn'>Close This Page</a>
                    <a href='/Home/Index' class='btn'>Back to Home</a>
                </div>
                <div class='footer'>
                    Powered by Flux SARL<br/>
                    &copy; 2019 - {DateTime.Now.Year} All rights reserved.
                </div>
            </div>
        </body>
        </html>
    ");

            Response.End();
        }

    }

}