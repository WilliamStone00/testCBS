using Antlr.Runtime.Misc;
using CBS.FrontDesk.Data.Entity.Accounting;
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

    /// <summary>
    /// The ReportViewer class handles the rendering and export of Crystal Reports in the web application.
    /// It serves as the main interface for generating, displaying, and exporting reports in various formats (PDF, Excel, Word).
    /// 
    /// ⚡ **Key Functionalities:**
    /// - **Report Rendering:** Loads Crystal Reports from specified file paths and binds data to the viewer.
    /// - **Export Handling:** Supports exporting reports to PDF, Excel, and Word formats.
    /// - **Parameter Application:** Applies parameters to the main report and subreports using session-based data.
    /// - **Session Management:** Caches the report document to enable paging, reloading, and exporting without data loss.
    /// - **Error Handling:** Displays user-friendly error messages for issues such as session expiration, invalid report paths, and data binding failures.
    /// 
    /// 🚧 **Developer Notes:**
    /// - Ensure that the report path is validated to prevent path traversal vulnerabilities.
    /// - Handle session expiration gracefully by displaying informative messages to the user.
    /// - Subreport data binding relies on matching subreport names; ensure subreport names are accurate and consistent.
    /// - Dispose of report documents properly to prevent memory leaks and 'Maximum report processing jobs limit reached' errors.
    /// - Export formats are determined via query string parameters (`action`), which are case-insensitive.
    /// 
    /// 📅 **Last Updated:** May 10, 2025  
    /// 🏢 **Company:** FLUX SARL  
    /// © Copyright FLUX SARL. All rights reserved.
    /// </summary>
    public partial class ReportViewer : Page
    {
        /// <summary>
        /// Initializes the page and handles report binding during postbacks.
        /// If the page is being reloaded (IsPostBack), the method attempts to restore the cached report document from the session.
        /// If the report document is unavailable (e.g., session expired), a clear error message is displayed to the user.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The method primarily handles postback scenarios to rebind the report document.
        /// - If the session has expired, a user-friendly error message is displayed, providing options to close the tab or return to the homepage.
        /// - The `Session["ReportDocument"]` object is expected to be of type `ReportDocument`.
        /// - The report viewer (`CrystalReportViewer1`) is re-bound to maintain the report state across postbacks.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="sender">The source of the event, typically the page itself.</param>
        /// <param name="e">Event data associated with the initialization event.</param>
        protected void Page_Init(object sender, EventArgs e)
        {
            // ✅ Check if the page is being reloaded (e.g., due to a postback event like pagination or export).
            if (IsPostBack)
            {
                // ✅ Attempt to retrieve the cached report document from the session.
                if (Session["ReportDocument"] is ReportDocument cachedDoc)
                {
                    // ✅ Rebind the cached report document to the CrystalReportViewer control.
                    // This ensures that the report maintains its state during postbacks.
                    CrystalReportViewer1.ReportSource = cachedDoc;
                    CrystalReportViewer1.DataBind(); // Apply data binding to refresh the report view.
                }
                else
                {
                    // ❌ If the report document is not found in the session, the session has likely expired.
                    // Display a clear, user-friendly error message with action options.

                    ShowError(@"
                ⚠️ <strong>SESSION EXPIRED:</strong><br/>
                The report session has expired or is unavailable.<br/>
                Please return to the reporting module and regenerate the report.<br/><br/>
                <a href='javascript:window.close();' style='color: red; font-weight: bold;'>Close this tab</a> or 
                <a href='/Home/Index' style='color: green; font-weight: bold;'>Go to Home Page</a>.");
                }
            }
        }

        /// <summary>
        /// Handles the initial report load and export actions based on query string parameters.
        /// This method is responsible for managing the initial report rendering and exporting reports to specific formats (PDF, Excel, Word).
        /// 
        /// ⚡ **Developer Notes:**
        /// - The method is executed every time the page is loaded or reloaded, including during postbacks.
        /// - If an export action is detected via the query string (`action`), the report is exported immediately.
        /// - If no export action is provided and the page is not a postback, the main report is loaded and bound to the viewer.
        /// - Exception handling is implemented to display user-friendly error messages in case of failures during export or report loading.
        /// 
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="sender">The source of the event, typically the page itself.</param>
        /// <param name="e">Event data associated with the load event.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // ✅ Retrieve the export action from the query string parameter 'action'.
                // Example: /ReportViewer.aspx?action=exportpdf
                string exportAction = Request.QueryString["action"];

                // ✅ If the 'action' query string is present and not empty, handle the export process.
                if (!string.IsNullOrWhiteSpace(exportAction))
                {
                    // ⚡ Handle export action (e.g., exportpdf, exportexcel, exportword).
                    HandleExport(exportAction);
                    return; // Exit the method to prevent further processing.
                }

                // ✅ If the page is not a postback (first load or direct access), proceed with the report loading.
                if (!IsPostBack)
                {
                    // 🛠️ Reset the action session variable to avoid unintended export actions during subsequent postbacks.
                    Session["action"] = null;

                    // 🚀 Load and bind the main Crystal Report document to the viewer.
                    LoadAndBindCrystalReport();
                }
            }
            catch (Exception ex)
            {
                // ❌ If any exception occurs during the load or export process, display a user-friendly error message.
                ShowError($"❌ <strong>Unexpected Error:</strong> Failed during export or load.<br/>Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles exporting the report in the specified format (PDF, Excel, Word).
        /// The export process is initiated by the query string parameter `action`.
        /// The method retrieves the report path and main data from the session and triggers the export process.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The export action is case-insensitive and supports the following values: `exportpdf`, `exportexcel`, `exportword`.
        /// - If the session data (report path or main data) is missing, the method displays an error and terminates further processing.
        /// - The export action is saved in the session under the key `"action"` to track the current export operation.
        /// - The actual export logic is delegated to the `ExportReport` method, which handles report generation and output.
        /// 
        /// 🛠️ **Expected Session Variables:**
        /// - `"reportPath"`: The relative path to the report file (e.g., `FinancialReport.rpt`).
        /// - `"MainData"`: The main dataset for the report, typically a DataTable or DataSet.
        /// - `"displayName"`: The display name for the report file during export (default: `"TSCReport"`).
        /// 
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="exportAction">The export action as specified in the query string (e.g., exportpdf, exportexcel, exportword).</param>
        private void HandleExport(string exportAction)
        {
            try
            {
                // ✅ Retrieve the report path from the session. This path is set during the report loading process.
                string reportPath = Session["reportPath"]?.ToString();

                // ✅ Retrieve the main data object for the report.
                // Expected to be a DataTable, DataSet, or other supported data source.
                object mainData = Session["MainData"];

                // ✅ Retrieve the display name for the exported file.
                // If not provided, defaults to "TSCReport".
                string displayName = Session["displayName"]?.ToString() ?? "TSCReport";

                // ✅ Save the export action in the session for reference during the export process.
                Session["action"] = exportAction;

                // ⚠️ Validate the report path and main data.
                // If either is null or empty, the export process cannot proceed.
                if (string.IsNullOrEmpty(reportPath) || mainData == null)
                {
                    // Display a user-friendly error message if data is missing.
                    ShowError("⚠️ Data not available for export. Please reload the report.");
                    return;
                }

                // ✅ Proceed to export the report using the specified export action.
                // Delegates the actual export logic to the `ExportReport` method.
                ExportReport(reportPath, mainData, displayName, exportAction);
            }
            catch (Exception ex)
            {
                // ❌ Handle any unexpected exceptions during the export process.
                // Displays a formatted error message to the user.
                ShowError($"Export Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the report document to the specified format (PDF, Excel, Word).
        /// This method handles the report export process by loading the report file, applying data and parameters, 
        /// and exporting the document to the requested format. The exported file is sent as a response to the client.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The report path must be a valid relative path to the `.rpt` file located in the `AppFiles/Reporting` directory.
        /// - The main data source must be a compatible data source object (e.g., DataTable, DataSet, etc.).
        /// - Export formats are determined using the `exportAction` parameter (`exportpdf`, `exportexcel`, `exportword`).
        /// - The method handles potential exceptions during the report loading, data binding, and export processes.
        /// - Proper resource disposal is ensured using a `using` statement to prevent memory leaks.
        ///
        /// 📦 **Session Dependencies:**
        /// - `"reportPath"`: The relative path to the report file.
        /// - `"MainData"`: The data source for the main report.
        /// - `"displayName"`: The base name for the exported file.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="reportPath">The relative path to the report file (e.g., `FinancialReport.rpt`).</param>
        /// <param name="mainData">The data source for the report (e.g., DataTable, DataSet).</param>
        /// <param name="displayName">The display name for the exported file (default: `TSCReport`).</param>
        /// <param name="exportAction">The export format action (e.g., `exportpdf`, `exportexcel`, `exportword`).</param>
        private void ExportReport(string reportPath, object mainData, string displayName, string exportAction)
        {
            try
            {
                // ✅ Resolve the full physical path to the report file.
                // The file must exist in the `AppFiles/Reporting` directory to be accessible.
                string filePath = Server.MapPath("~/AppFiles/Reporting/" + reportPath);

                // ✅ Ensure the report file path is valid and the file exists.
                if (!File.Exists(filePath))
                {
                    ShowError("❌ Report file not found. Please verify the report path and try again.");
                    return;
                }

                // ✅ Using statement ensures that the report document resources are properly disposed of after use.
                using (ReportDocument reportDoc = new ReportDocument())
                {
                    // ✅ Load the report file into the `ReportDocument` object.
                    reportDoc.Load(filePath);

                    // ✅ Set the main data source for the report.
                    reportDoc.SetDataSource(mainData);

                    // ✅ Apply report parameters from the session data.
                    ApplyReportParameters(reportDoc);

                    // ✅ Bind data to any subreports associated with the main report.
                    BindSubReports(reportDoc);

                    // ✅ Determine the export format using the specified export action (e.g., PDF, Excel, Word).
                    ExportFormatType format = GetExportFormat(exportAction);

                    // ✅ Construct the filename with a timestamp to avoid filename collisions.
                    string filename = $"{displayName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}";

                    // ✅ Export the report document to the specified format and send it as a response to the client.
                    reportDoc.ExportToHttpResponse(format, Response, false, filename);
                }
            }
            catch (Exception ex)
            {
                // ❌ Handle unexpected exceptions during report export and display a user-friendly error message.
                ShowError($"⚠️ Export Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines the export format based on the provided action string.
        /// This method translates the export action (e.g., `exportpdf`, `exportexcel`, `exportword`) 
        /// into the corresponding `ExportFormatType` enumeration used by Crystal Reports.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The method is case-insensitive and converts the export action to lowercase for comparison.
        /// - Supported export actions:
        ///   - `"exportpdf"`  ➔ PDF Format
        ///   - `"exportexcel"` ➔ Excel Format
        ///   - `"exportword"`  ➔ Word Format
        /// - If an invalid export action is provided, an `ArgumentException` is thrown with a detailed error message.
        /// - The method is designed to be easily extendable for additional export formats if required.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="exportAction">The export action as a string (e.g., `exportpdf`, `exportexcel`, `exportword`).</param>
        /// <returns>The corresponding `ExportFormatType` for the specified export action.</returns>
        /// <exception cref="ArgumentException">Thrown when the provided export action is not recognized.</exception>
        private ExportFormatType GetExportFormat(string exportAction)
        {
            // ✅ Normalize the export action to lowercase for case-insensitive comparison.
            exportAction = exportAction.ToLower();

            // ✅ Switch statement to determine the export format based on the export action.
            switch (exportAction)
            {
                // PDF Format
                case "exportpdf":
                    return ExportFormatType.PortableDocFormat;

                // Excel Format
                case "exportexcel":
                    return ExportFormatType.Excel;

                // Word Format
                case "exportword":
                    return ExportFormatType.WordForWindows;

                // ❌ If the export action is not recognized, throw an exception with a detailed error message.
                default:
                    throw new ArgumentException($"Invalid export type: '{exportAction}'. Supported types are 'exportpdf', 'exportexcel', 'exportword'.");
            }
        }


        /// <summary>
        /// Applies parameters to the Crystal Report document based on the session data.
        /// This method iterates through the parameter collection stored in the session (`ReportParameters`) 
        /// and assigns the parameter values to the report document.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The session parameter collection is expected to be of type `Dictionary<string, object>`.
        /// - Each key in the dictionary represents the parameter name, and the value is the parameter value.
        /// - Parameter names are matched case-insensitively against the report parameters.
        /// - If a parameter is not found in the report, it is skipped without interrupting the process.
        /// - If a parameter assignment fails, an error message is displayed with specific details.
        ///
        /// 🛠️ **Expected Session Variables:**
        /// - `"ReportParameters"`: A dictionary containing parameter names and values to be applied to the report.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="reportDoc">The Crystal Report document to which the parameters will be applied.</param>
        private void ApplyReportParameters(ReportDocument reportDoc)
        {
            // ✅ Check if the "ReportParameters" session variable is a valid dictionary.
            if (Session["ReportParameters"] is Dictionary<string, object> parameters)
            {
                // ✅ Iterate through each parameter in the dictionary.
                foreach (var param in parameters)
                {
                    try
                    {
                        // ✅ Extract the parameter name and value.
                        string parameterName = param.Key;
                        object parameterValue = param.Value;

                        // ✅ Check if the parameter exists in the report definition (case-insensitive).
                        bool parameterExists = reportDoc.DataDefinition.ParameterFields
                            .Cast<ParameterFieldDefinition>()
                            .Any(p => p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase));

                        // ✅ Apply the parameter value only if it exists in the report.
                        if (parameterExists)
                        {
                            reportDoc.SetParameterValue(parameterName, parameterValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        // ❌ Handle parameter assignment errors and display a user-friendly error message.
                        // Stops further processing to avoid cascading errors.
                        ShowError($"⚠️ <strong>Parameter Error:</strong> Failed to apply parameter <code>{param.Key}</code>.<br/>Details: {ex.Message}");
                        return; // Exit method to prevent further processing.
                    }
                }
            }
        }

        /// <summary>
        /// Binds data to the subreports within the main Crystal Report document.
        /// This method iterates through the `SubReportsData` session dictionary and assigns data sources 
        /// to subreports by matching the subreport name with the dictionary key.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The session variable `"SubReportsData"` is expected to be a dictionary with subreport names as keys 
        ///   and corresponding data sources as values.
        /// - Subreport names are compared in a case-insensitive manner, and `.rpt` extensions are appended if missing.
        /// - Only subreports that exist within the main report document are processed.
        /// - If a subreport fails to load or bind data, an error message is displayed and further processing stops.
        ///
        /// 🛠️ **Expected Session Variables:**
        /// - `"SubReportsData"`: A dictionary containing subreport names and their respective data sources.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="reportDoc">The main Crystal Report document containing subreports.</param>
        private void BindSubReports(ReportDocument reportDoc)
        {
            // ✅ Check if the "SubReportsData" session variable is a valid dictionary.
            if (Session["SubReportsData"] is Dictionary<string, object> subReports)
            {
                // ✅ Iterate over each subreport entry in the dictionary.
                foreach (var sub in subReports)
                {
                    // ✅ Check that the subreport data is not null before proceeding.
                    if (sub.Value != null)
                    {
                        try
                        {
                            // ✅ Extract the subreport name and data source.
                            string subreportName = sub.Key;
                            object subreportData = sub.Value;

                            // ✅ Ensure the subreport name ends with `.rpt` extension.
                            // This standardizes subreport name format for consistency.
                            if (!subreportName.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase))
                            {
                                subreportName += ".rpt";
                            }

                            // ✅ Verify the subreport exists within the main report document.
                            bool subreportExists = reportDoc.Subreports
                                .Cast<ReportDocument>()
                                .Any(s => s.Name.Equals(subreportName, StringComparison.OrdinalIgnoreCase));

                            // ✅ Bind the data source to the subreport if it exists.
                            if (subreportExists)
                            {
                                reportDoc.Subreports[subreportName].SetDataSource(subreportData);
                            }
                        }
                        catch (Exception ex)
                        {
                            // ❌ Handle subreport binding errors and display a user-friendly error message.
                            // The method terminates to prevent further processing and potential cascading errors.
                            ShowError($"❌ <strong>Subreport Error:</strong> Failed to load subreport <code>{sub.Key}</code>.<br/>Details: {ex.Message}");
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the main report document and binds the data for initial rendering in the CrystalReportViewer.
        /// This method retrieves the report path and data source from the query string and session, 
        /// verifies the existence of the report file, applies parameters, and sets the report source in the viewer.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The report path (`reportPath`) is passed as a query string parameter and validated for security.
        /// - The report data (`MainData`) is retrieved from the session and applied as the main data source.
        /// - Subreport data and report parameters are applied via dedicated methods.
        /// - Session variables (`ReportDocument`, `reportPath`, `displayName`) are updated to facilitate subsequent operations like exporting.
        /// 
        /// 🛠️ **Expected Query String Parameters:**
        /// - `reportPath`: Relative path to the report file (e.g., `MonthlyReport.rpt`).
        /// - `reportName`: Display name for the report (optional, used for exporting).
        ///
        /// 🛠️ **Expected Session Variables:**
        /// - `"MainData"`: The main data source for the report.
        /// - `"SubReportsData"`: Dictionary containing subreport data sources (optional).
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        private void LoadAndBindCrystalReport()
        {
            try
            {
                // ✅ Retrieve the report path from the query string.
                // Example URL: /ReportViewer.aspx?reportPath=MonthlyReport.rpt
                string relativePath = Request.QueryString["reportPath"];
                string displayName = Request.QueryString["reportName"];

                // 🔐 Security Check: Validate the report path to prevent directory traversal attacks.
                if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
                {
                    ShowError("❌ <strong>Invalid Report Path:</strong> The specified report path is either missing or malformed.");
                    return;
                }

                // 🔍 Resolve the full server path to the report file.
                // Converts the relative path to an absolute path on the server.
                string fullPath = Server.MapPath("~/AppFiles/Reporting/" + relativePath);

                // 📛 Check if the report file exists at the specified path.
                if (!File.Exists(fullPath))
                {
                    ShowError("❌ <strong>Report Not Found:</strong> The requested report file does not exist on the server.");
                    return;
                }

                // ✅ Verify that the main data source is available in the session.
                // If the session data is missing, display a session expiration error.
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

                // ✅ Initialize a new `ReportDocument` object for loading and binding data.
                ReportDocument rd = new ReportDocument();

                // ✅ Load the report file into the `ReportDocument` object.
                rd.Load(fullPath);

                // ✅ Set the main data source for the report.
                rd.SetDataSource(Session["MainData"]);

                // ✅ Bind subreport data using the session-based dictionary.
                BindSubReports(rd);

                // ✅ Apply report parameters (if available) to the main report.
                ApplyReportParameters(rd);

                // ✅ Assign the report to the CrystalReportViewer control for rendering.
                CrystalReportViewer1.ReportSource = rd;
                CrystalReportViewer1.DataBind();

                // ✅ Cache the report document in the session for postbacks and export operations.
                Session["ReportDocument"] = rd;
                Session["reportPath"] = relativePath;
                Session["displayName"] = displayName;
            }
            catch (Exception ex)
            {
                // ❌ Handle unexpected exceptions during report loading and data binding.
                ShowError($"Unexpected Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a styled error message in a full HTML view.
        /// This method clears the current response content, applies a consistent HTML structure with embedded CSS styles, 
        /// and presents the error message to the user in a well-designed layout. The response is terminated after rendering the error page.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The error message is expected to contain HTML content, allowing for rich formatting.
        /// - The page layout includes a header, a sub-header, a message container, and a footer section.
        /// - The method uses embedded CSS for styling to ensure consistent presentation across browsers.
        /// - The response is terminated using `Response.End()` to prevent further content rendering.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="messageHtml">The error message content formatted in HTML.</param>
        private void ShowError(string messageHtml)
        {
            // ✅ Clear the current response content to avoid mixing new content with the previous response.
            Response.Clear();

            // ✅ Set the content type to HTML to render the error message as a full HTML page.
            Response.ContentType = "text/html";

            // ✅ Write the complete HTML structure for the error page.
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
                    background-color: #fff;
                    border: 1px solid #ccc;
                    border-radius: 8px;
                    padding: 30px;
                    box-shadow: 0 0 12px rgba(0,0,0,0.1);
                }}
                .header-line {{
                    height: 3px;
                    background-color: #007f45;
                    margin-bottom: 25px;
                }}
                .header {{
                    text-align: center;
                    font-size: 20px;
                    font-weight: bold;
                    color: #007f45;
                }}
                .sub-header {{
                    text-align: center;
                    font-size: 14px;
                    font-weight: bold;
                    margin-bottom: 30px;
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

            // ✅ End the response to prevent further content from being rendered.
            Response.End();
        }

        /// <summary>
        /// Releases resources and performs cleanup when the page is unloaded.
        /// This method ensures that the report document (`ReportDocument`) is properly disposed of 
        /// to prevent memory leaks and avoid reaching the maximum report processing limit.
        /// The method only disposes of the report document if an export operation is not in progress.
        /// 
        /// ⚡ **Developer Notes:**
        /// - The `Page_Unload` event is triggered when the page is unloaded from memory, such as when the user navigates away or closes the page.
        /// - The method checks if the session variable `"ReportDocument"` is a valid `ReportDocument` object before attempting to dispose of it.
        /// - If an export operation is in progress (indicated by the `"action"` session variable), the report document is retained.
        /// - Garbage collection is explicitly invoked using `GC.Collect()` and `GC.WaitForPendingFinalizers()` to ensure complete cleanup of unmanaged resources.
        ///
        /// 📅 **Last Updated:** May 10, 2025  
        /// 🏢 **Company:** FLUX SARL  
        /// </summary>
        /// <param name="sender">The source of the event, typically the page itself.</param>
        /// <param name="e">Event data associated with the unload event.</param>
        protected void Page_Unload(object sender, EventArgs e)
        {
            try
            {
                // ✅ Check if the session contains a valid report document.
                if (Session["ReportDocument"] is ReportDocument reportDoc)
                {
                    // ✅ Verify that no export operation is in progress.
                    // If the "action" session variable is set, retain the report document for export.
                    string exportAction = Session["action"]?.ToString();

                    if (string.IsNullOrWhiteSpace(exportAction))
                    {
                        // ✅ Properly close and dispose of the report document to release resources.
                        reportDoc.Close();
                        reportDoc.Dispose();

                        // ✅ Remove the report document reference from the session to prevent memory leaks.
                        //Session["ReportDocument"] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // ❌ Optional: Handle or log the error encountered during the unload process.
                // Consider implementing a logging mechanism to capture the exception details.
            }
            finally
            {
                // ✅ Force garbage collection to clean up unmanaged resources.
                // This is especially important for Crystal Reports to avoid memory leaks.
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }







}