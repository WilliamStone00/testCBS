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
        //public void CleanReport(ReportDocument rd)
        //{
        //    if (rd != null)
        //    {
        //        rd.Close();
        //        rd.Dispose();
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }

        //}
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

                    // ✅ Clear all previous content BEFORE exporting
                    Response.Clear();
                    Response.Buffer = true;
                    Response.ContentType = "";

                    string reportName = (Session["displayName"]?.ToString() ?? "TSCReport").Replace(" ", "_");
                    string filename = $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    exportDoc.ExportToHttpResponse(format, Response, false, filename);

                    // ✅ Do NOT call Response.End (deprecated), use CompleteRequest instead
                    Context.ApplicationInstance.CompleteRequest();
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
                //CleanReport(rd);
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
                Session["displayName"] = displayName;
    
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
        /// <summary>
        /// Cleans up the report document and releases allocated resources when the page is unloaded.
        /// This method ensures that no report processing jobs are left hanging, preventing the
        /// "Maximum report processing jobs limit reached" error.
        /// </summary>
        protected void Page_Unload(object sender, EventArgs e)
        {
            try
            {
                if (Session["ReportDocument"] is ReportDocument reportDoc)
                {
                    // 🛑 Check if the export operation is ongoing
                    string exportAction = Session["action"]?.ToString();

                    // ✅ Only dispose of the report if it's NOT being exported
                    if (string.IsNullOrWhiteSpace(exportAction))
                    {
                        reportDoc.Close();
                        reportDoc.Dispose();
                        Session["ReportDocument"] = null;
                    }

                    // Clear the export action flag
                    Session["action"] = null;
                }
            }
            catch (Exception ex)
            {
                // Log or handle unload error (optional)
            }
            finally
            {
                // Ensure that memory is released
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

    }

    //public partial class ReportViewer : System.Web.UI.Page
    //{
    //    /// <summary>
    //    /// Dynamically loads and renders a Crystal Report in the viewer UI based on session-passed data and parameters.
    //    ///
    //    /// ✅ Supported Report Types:
    //    /// - Reports with/without parameters
    //    /// - Reports with/without subreports
    //    /// - Parameter and subreport names are case-insensitive
    //    ///
    //    /// ❌ Not Supported:
    //    /// - Reports requiring runtime database login
    //    /// - Nested subreports
    //    /// - Reports expecting dynamic subreport names
    //    ///
    //    /// 📅 Created On: April 18, 2025  
    //    /// 🏢 Company: FLUX SARL  
    //    /// © Copyright FLUX SARL. All rights reserved.
    //    /// </summary>

    //    /// <summary>
    //    /// Handles the initialization of the page, primarily restoring the report document for postbacks.
    //    /// This method ensures that the report document is re-bound to the CrystalReportViewer on postbacks 
    //    /// to avoid data loss and maintain report state (e.g., pagination, export).
    //    /// </summary>
    //    /// <param name="sender">The source of the event.</param>
    //    /// <param name="e">An EventArgs object containing the event data.</param>
    //    protected void Page_Init(object sender, EventArgs e)
    //    {
    //        // ✅ Check if the current request is a postback (e.g., navigating to another report page, exporting, etc.)
    //        if (IsPostBack)
    //        {
    //            // 🔄 Attempt to retrieve the cached report document from session
    //            if (Session["ReportDocument"] is ReportDocument cachedDoc)
    //            {
    //                // ✅ Re-bind the report document to the CrystalReportViewer to maintain state
    //                CrystalReportViewer1.ReportSource = cachedDoc;

    //                // 📦 Re-bind the data to ensure the viewer is populated with the report content
    //                CrystalReportViewer1.DataBind();
    //            }
    //            else
    //            {
    //                // ❌ Handle the case where the session has expired or the report document is not found
    //                // Display a user-friendly error message with navigation options
    //                ShowError(@"
    //            ⚠️ <strong>SESSION EXPIRED:</strong><br/>
    //            The report session has expired or is unavailable.<br/>
    //            Please return to the reporting module and regenerate the report.<br/><br/>
    //            <a href='javascript:window.close();' style='color: red; font-weight: bold;'>Close this tab</a> or 
    //            <a href='/Home/Index' style='color: green; font-weight: bold;'>Go to Home Page</a>.");
    //            }
    //        }
    //    }


    //    /// <summary>
    //    /// Handles the loading and exporting of the report.
    //    /// If the `action` query parameter is present, the report is exported in the specified format.
    //    /// Otherwise, the report is loaded and rendered in the viewer.
    //    /// </summary>
    //    /// <param name="sender">The source of the event.</param>
    //    /// <param name="e">An EventArgs object containing the event data.</param>
    //    protected void Page_Load(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            // 🔍 Extract the 'action' query string parameter to check for export requests (e.g., "exportpdf", "exportexcel", "exportword")
    //            string exportAction = Request.QueryString["action"];

    //            // ✅ Check if an export action is specified and the report document is available in session
    //            if (!string.IsNullOrWhiteSpace(exportAction) && Session["ReportDocument"] is ReportDocument exportDoc)
    //            {
    //                // 🔥 Call the export handler to process the export in the specified format
    //                HandleExport(exportDoc, exportAction);
    //                return; // Exit the method after handling the export
    //            }

    //            // 🚀 First-time page load (not a postback)
    //            if (!IsPostBack)
    //            {
    //                // 📄 Load and bind the report to the viewer
    //                LoadAndBindReport();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            // 🛑 Catch any exceptions during export or report loading
    //            // Display a formatted error message to the user
    //            ShowError($"❌ <strong>Error:</strong> {ex.Message}");
    //        }
    //    }


    //    /// <summary>
    //    /// Handles exporting the report to PDF, Excel, or Word format.
    //    /// The export type is determined based on the `exportAction` parameter.
    //    /// </summary>
    //    /// <param name="reportDoc">The ReportDocument instance to be exported.</param>
    //    /// <param name="exportAction">The export action type ("exportpdf", "exportexcel", "exportword").</param>
    //    private void HandleExport(ReportDocument reportDoc, string exportAction)
    //    {
    //        try
    //        {
    //            // 📦 Determine the export format based on the action parameter
    //            ExportFormatType format;

    //            switch (exportAction.ToLower())
    //            {
    //                case "exportpdf":
    //                    format = ExportFormatType.PortableDocFormat; // PDF format
    //                    break;

    //                case "exportexcel":
    //                    format = ExportFormatType.Excel; // Excel format
    //                    break;

    //                case "exportword":
    //                    format = ExportFormatType.WordForWindows; // Word format
    //                    break;

    //                default:
    //                    // ❌ If the action is not recognized, throw an exception
    //                    throw new ArgumentException("Invalid export type");
    //            }

    //            // 🧹 Clear previous response content to avoid conflicts during export
    //            Response.Clear();

    //            // ✅ Enable response buffering for optimal export performance
    //            Response.Buffer = true;

    //            // 📜 Generate a sanitized report name, replacing spaces with underscores
    //            string reportName = (Session["displayName"]?.ToString() ?? "TSC_Report").Replace(" ", "_");

    //            // 📦 Construct the export filename with a timestamp for uniqueness
    //            string filename = $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}";

    //            // 🛠️ Export the report to the specified format and send it to the client as a downloadable file
    //            reportDoc.ExportToHttpResponse(format, Response, false, filename);

    //            // ✅ Complete the request to prevent further processing
    //            // ⚠️ Avoid using Response.End() as it is deprecated in newer ASP.NET versions
    //            Context.ApplicationInstance.CompleteRequest();
    //        }
    //        catch (Exception ex)
    //        {
    //            // 🛑 Handle any exceptions that occur during the export process
    //            ShowError($"⚠️ <strong>Export Error:</strong> {ex.Message}");
    //        }
    //    }


    //    /// <summary>
    //    /// Loads the report document from the specified path, binds main data, subreports, and parameters,
    //    /// and assigns it to the CrystalReportViewer for rendering.
    //    /// This method also caches the report document in session for reuse during postbacks.
    //    /// </summary>
    //    private void LoadAndBindReport()
    //    {
    //        try
    //        {
    //            // 📌 Extract query string parameters for report path and display name
    //            string relativePath = Request.QueryString["reportPath"];
    //            string displayName = Request.QueryString["reportName"];

    //            // ✅ Store display name in ViewState for potential reuse
    //            ViewState["ReportName"] = displayName ?? "Crystal Report";

    //            // 🔐 Validate report path to prevent directory traversal attacks
    //            if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
    //            {
    //                ShowError("⚠️ <strong>Invalid Report Path:</strong> The report path is invalid.");
    //                return; // Exit method to prevent further processing
    //            }

    //            // 🔍 Resolve the absolute server path for the report file
    //            string fullPath = Server.MapPath("~/AppFiles/Reporting/" + relativePath);

    //            // 📛 Check if the report file physically exists on the server
    //            if (!File.Exists(fullPath))
    //            {
    //                ShowError("⚠️ <strong>Report Not Found:</strong> The report file does not exist.");
    //                return; // Exit method if file is not found
    //            }

    //            // ❌ Check if the main data source is still available in the session
    //            if (Session["MainData"] == null)
    //            {
    //                ShowError(@"
    //            ⚠️ <strong>SESSION EXPIRED:</strong><br/>
    //            The data required to generate this report is no longer available in memory.<br/>
    //            Please return to the reporting module and regenerate the report.<br/><br/>
    //            <a href='javascript:window.close();' style='color: red; font-weight: bold;'>Close this tab</a> or 
    //            <a href='/Home/Index' style='color: green; font-weight: bold;'>Go to Home Page</a>.");
    //                return; // Exit method if session data is not available
    //            }

    //            // 📄 Initialize a new Crystal ReportDocument instance
    //            ReportDocument reportDoc = new ReportDocument();

    //            // 🗄️ Load the report file from the specified path
    //            reportDoc.Load(fullPath);

    //            // 🧩 Bind the main data source to the report
    //            reportDoc.SetDataSource(Session["MainData"]);

    //            // 🔗 Bind subreport data (if available in session)
    //            BindSubReports(reportDoc);

    //            // 🧷 Apply report parameters (if defined in session)
    //            ApplyReportParameters(reportDoc);

    //            // 🖼️ Assign the report document to the CrystalReportViewer control
    //            CrystalReportViewer1.ReportSource = reportDoc;

    //            // 🔄 Trigger data binding for rendering the report
    //            CrystalReportViewer1.DataBind();

    //            // 🧠 Cache the report document in session for postback and export handling
    //            Session["ReportDocument"] = reportDoc;

    //            // 💾 Store the display name in session for use in export filename
    //            Session["displayName"] = displayName;
    //        }
    //        catch (Exception ex)
    //        {
    //            // ⚠️ Display a user-friendly error message with exception details
    //            ShowError($"❌ <strong>Unexpected Error:</strong> An unexpected issue occurred while loading the report.<br/>Details: {ex.Message}");
    //        }
    //    }

    //    /// <summary>
    //    /// Binds data to subreports within the main report document.
    //    /// This method retrieves subreport data from the session and applies them to the corresponding subreports in the report.
    //    /// If a subreport is not found or data binding fails, an error message is displayed.
    //    /// </summary>
    //    /// <param name="reportDoc">The Crystal Report document containing the subreports.</param>
    //    private void BindSubReports(ReportDocument reportDoc)
    //    {
    //        // 🧷 Check if there are subreports data in the session
    //        if (Session["SubReportsData"] is Dictionary<string, object> subReports)
    //        {
    //            // 🔄 Iterate over each subreport entry in the dictionary
    //            foreach (var sub in subReports)
    //            {
    //                // 🛠️ Proceed only if the subreport data is not null
    //                if (sub.Value != null)
    //                {
    //                    try
    //                    {
    //                        // 🧾 Ensure the subreport name ends with `.rpt`
    //                        string subreportName = sub.Key.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase)
    //                            ? sub.Key
    //                            : sub.Key + ".rpt";

    //                        // ✅ Check if the subreport exists within the main report
    //                        bool subreportExists = reportDoc.Subreports
    //                            .Cast<ReportDocument>()
    //                            .Any(s => s.Name.Equals(subreportName, StringComparison.OrdinalIgnoreCase));

    //                        // 🔗 Bind data to the subreport only if it exists
    //                        if (subreportExists)
    //                        {
    //                            reportDoc.Subreports[subreportName].SetDataSource(sub.Value);
    //                        }
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        // ⚠️ Handle subreport binding errors and display a user-friendly error message
    //                        ShowError($"❌ <strong>Subreport Error:</strong> Failed to load subreport <code>{sub.Key}</code>.<br/>Details: {ex.Message}");
    //                        return; // Exit the method to prevent further processing
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Applies the report parameters to the specified report document.
    //    /// This method retrieves parameter values from the session and applies them to the report.
    //    /// If a parameter is not defined in the report, it is skipped without causing an error.
    //    /// </summary>
    //    /// <param name="reportDoc">The Crystal Report document to apply parameters to.</param>
    //    private void ApplyReportParameters(ReportDocument reportDoc)
    //    {
    //        // 🧷 Check if there are parameters in session and the report has defined parameters
    //        if (Session["ReportParameters"] is Dictionary<string, object> parameters && reportDoc.DataDefinition.ParameterFields.Count > 0)
    //        {
    //            // 🔄 Iterate over each parameter in the session
    //            foreach (var param in parameters)
    //            {
    //                try
    //                {
    //                    // ✅ Verify that the parameter exists in the report definition
    //                    bool parameterExists = reportDoc.DataDefinition.ParameterFields
    //                        .Cast<ParameterFieldDefinition>()
    //                        .Any(p => p.Name.Equals(param.Key, StringComparison.OrdinalIgnoreCase));

    //                    // 🛠️ Apply the parameter value only if it exists in the report
    //                    if (parameterExists)
    //                    {
    //                        reportDoc.SetParameterValue(param.Key, param.Value);
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    // ⚠️ Handle parameter application errors gracefully and display a user-friendly error
    //                    ShowError($"⚠️ <strong>Parameter Error:</strong> Failed to apply parameter <code>{param.Key}</code>.<br/>Details: {ex.Message}");
    //                    return; // Exit the method on error
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Handles error display with custom styling.
    //    /// </summary>
    //    private void ShowError(string messageHtml)
    //    {
    //        Response.Clear();
    //        Response.ContentType = "text/html";
    //        Response.Write($@"
    //        <html>
    //        <head>
    //            <title>TSC - Error</title>
    //            <style>
    //                body {{
    //                    font-family: 'Segoe UI', sans-serif;
    //                    background-color: #f5f7fa;
    //                    color: #333;
    //                }}
    //                .container {{
    //                    width: 600px;
    //                    margin: 100px auto;
    //                    background-color: #fff;
    //                    border: 1px solid #ddd;
    //                    padding: 20px;
    //                    border-radius: 8px;
    //                    box-shadow: 0px 0px 10px #ccc;
    //                }}
    //                .header {{
    //                    font-size: 18px;
    //                    font-weight: bold;
    //                    color: #007f45;
    //                    text-align: center;
    //                    margin-bottom: 15px;
    //                }}
    //                .message {{
    //                    font-size: 14px;
    //                    text-align: center;
    //                }}
    //            </style>
    //        </head>
    //        <body>
    //            <div class='container'>
    //                <div class='header'>Trust Soft Credit - Report Viewer</div>
    //                <div class='message'>{messageHtml}</div>
    //            </div>
    //        </body>
    //        </html>");
    //        Response.End();
    //    }

    //    /// <summary>
    //    /// Cleans up the report document and releases allocated resources when the page is unloaded.
    //    /// This method ensures that no report processing jobs are left hanging, preventing the
    //    /// "Maximum report processing jobs limit reached" error.
    //    /// </summary>
    //    protected void Page_Unload(object sender, EventArgs e)
    //    {
    //        try
    //        {
    //            if (Session["ReportDocument"] is ReportDocument reportDoc)
    //            {
    //                // ✅ Retrieve export action from session (not from the request)
    //                string exportAction = Session["ExportAction"]?.ToString();

    //                // 🛑 Only clear the report if it's NOT being exported
    //                if (string.IsNullOrWhiteSpace(exportAction))
    //                {
    //                    reportDoc.Close();
    //                    reportDoc.Dispose();
    //                    Session["ReportDocument"] = null;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ShowError($"⚠️ <strong>Unload Error:</strong> {ex.Message}");
    //        }
    //        finally
    //        {
    //            // 🧹 Force garbage collection to reclaim memory
    //            GC.Collect();
    //            GC.WaitForPendingFinalizers();
    //        }
    //    }

    //}

}