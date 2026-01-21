using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.UI.AppFiles.Reporting.Accounting;
using ClosedXML.Excel;
using   CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DocumentFormat.OpenXml.EMMA;
//using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace CBS.FrontDesk.UI.Controllers
{
    // [CheckSessionTimeOutAttribute]

    public class ReportsController : BaseController
    {
        private readonly AccountingServices _accountServices;

        public ReportsController()
        {
            _accountServices = new AccountingServices();
        }
        public void ReportParameterLess()
        {
            ReportDocument rd = new ReportDocument();
            try
            {
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                var rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    Response.Write("<H2>No Report with such Name found</H2>");
                    return;
                }


                string strRptPath = Server.MapPath(rptpath);
                rd.Load(strRptPath);
                if (rptSource.GetType() != typeof(string))
                {
                    rd.SetDataSource(rptSource);
                }

                string year = System.Web.HttpContext.Current.Session["Year"]?.ToString() ?? "Non";
                string dates = System.Web.HttpContext.Current.Session["Dates"]?.ToString() ?? "Non";
                string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "Non";
                string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "Non";

                if (year != "Non")
                {
                    rd.SetParameterValue("param", $"Header summary: {year}");
                }

                if (dates != "Non" && !string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    rd.SetParameterValue("DateFrom", strFromDate);
                    rd.SetParameterValue("DateTo", strToDate);
                }

                string savedFileName = $"{rpttitle}-{DateTime.UtcNow.ToString("dd_mm_yyyy_hhmmss")}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, savedFileName);
            }
            catch (Exception ex)
            {
                // Log the exception
                // Handle specific exceptions if needed
                Response.Write("<H2>An error occurred while generating the report</H2>");
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
        }

        public void ReportParameterLessCOLL()
        {
            ReportDocument rd = new ReportDocument();
            try
            {
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                var rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    Response.Write("<H2>❌ No Report with such Name found</H2>");
                    Response.Write($"<p>ReportName: {strReportName ?? "null"}</p>");
                    Response.Write($"<p>rptSource: {(rptSource == null ? "null" : rptSource.GetType().Name)}</p>");
                    Response.Write($"<p>rptpath: {rptpath ?? "null"}</p>");
                    return;
                }

                // Log what we're loading
                System.Diagnostics.Debug.WriteLine($"=== CRYSTAL REPORT LOADING ===");
                System.Diagnostics.Debug.WriteLine($"Report: {strReportName}");
                System.Diagnostics.Debug.WriteLine($"Path: {rptpath}");
                System.Diagnostics.Debug.WriteLine($"Source Type: {rptSource.GetType().FullName}");

                // Log DataSet details if it's a DataSet
                if (rptSource is System.Data.DataSet dataSet)
                {
                    System.Diagnostics.Debug.WriteLine($"DataSet: {dataSet.DataSetName}, Tables: {dataSet.Tables.Count}");
                    foreach (DataTable table in dataSet.Tables)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Table: '{table.TableName}' ({table.Rows.Count} rows)");
                    }
                }

                string strRptPath = Server.MapPath(rptpath);

                // Check if report file exists
                if (!System.IO.File.Exists(strRptPath))
                {
                    Response.Write($"<H2>❌ Report file not found</H2>");
                    Response.Write($"<p>Path: {strRptPath}</p>");
                    return;
                }

                rd.Load(strRptPath);

                // Set data source
                rd.SetDataSource(rptSource);

                // Log Crystal's internal table recognition
                if (rd.Database != null && rd.Database.Tables != null)
                {
                    System.Diagnostics.Debug.WriteLine("=== CRYSTAL INTERNAL TABLES ===");
                    foreach (CrystalDecisions.CrystalReports.Engine.Table table in rd.Database.Tables)
                    {
                        System.Diagnostics.Debug.WriteLine($"Crystal Table: {table.Name}");
                    }
                }

                // Set parameters if available
                string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? string.Empty;
                string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    try
                    {
                        rd.SetParameterValue("DateFrom", strFromDate);
                        rd.SetParameterValue("DateTo", strToDate);
                    }
                    catch (Exception paramEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Parameter Error: {paramEx.Message}");
                        // Continue without parameters
                    }
                }

                // Export to PDF
                string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
                rd.ExportToHttpResponse(
                    ExportFormatType.PortableDocFormat,
                    System.Web.HttpContext.Current.Response,
                    false,
                    savedFileName);

                System.Diagnostics.Debug.WriteLine("✅ Report exported successfully");
            }
            catch (CrystalDecisions.CrystalReports.Engine.LoadSaveReportException loadEx)
            {
                System.Diagnostics.Debug.WriteLine($"CRYSTAL LOAD ERROR: {loadEx.Message}");
                Response.Write($"<H2>❌ Report Loading Error</H2>");
                Response.Write($"<p>{loadEx.Message}</p>");
                if (loadEx.InnerException != null)
                {
                    Response.Write($"<p>Inner: {loadEx.InnerException.Message}</p>");
                }
            }
            catch (CrystalDecisions.CrystalReports.Engine.DataSourceException dataEx)
            {
                System.Diagnostics.Debug.WriteLine($"CRYSTAL DATA SOURCE ERROR: {dataEx.Message}");
                Response.Write($"<H2>❌ Data Binding Error</H2>");
                Response.Write($"<p>{dataEx.Message}</p>");
                Response.Write($"<p>Check that your DataSet table names match exactly what Crystal expects.</p>");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRYSTAL GENERAL ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                Response.Write($"<H2>❌ An error occurred while generating the report</H2>");
                Response.Write($"<p>{ex.Message}</p>");
                Response.Write($"<pre>{ex.StackTrace}</pre>");
            }
            finally
            {
                CleanReport(rd);
            }
        }

        private void SetReportParameters(ReportDocument reportDocument)
        {
            try
            {
                var query = Session["ChequeBookQuery"] as ChequeBookQuery;
                if (query != null)
                {
                    if (query.StartDate.HasValue)
                    {
                        reportDocument.SetParameterValue("DateFrom", query.StartDate.Value.ToString("dd/MM/yyyy"));
                    }

                    if (query.EndDate.HasValue)
                    {
                        reportDocument.SetParameterValue("DateTo", query.EndDate.Value.ToString("dd/MM/yyyy"));
                    }
                }

                reportDocument.SetParameterValue("GeneratedOn", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            }
            catch
            {
                // Ignore parameter errors
            }
        }


        public void IncomeStatementSubReports()
        {                // Create a new ReportDocument
            ReportDocument rd = new ReportDocument();

            try
            {
                // Retrieve parameters from session
                string strReportName = HttpContext.Session["ReportName"]?.ToString();
                var rptSource = HttpContext.Session["rptSource"];
                var rptpath = HttpContext.Session["rptpath"]?.ToString();
                var rpttitle = HttpContext.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    HttpContext.Response.Write("<H2>No Report with such Name found</H2>");
                    return;
                }

                string strRptPath = HttpContext.Server.MapPath(rptpath);
                rd.Load(strRptPath);

                // Cast rptSource to List<PaymentReciptDS>
                if (rptSource is List<PaymentReciptDS> reportData)
                {
                    // Check if the list has at least one item
                    if (reportData.Count > 0)
                    {
                        // Set data source for the main report
                        rd.SetDataSource(reportData);

                        // Set data sources for subreports
                        foreach (ReportDocument subReport in rd.Subreports)
                        {
                            string subReportName = subReport.Name;

                            switch (subReportName)
                            {
                                case "DenominationSubReport.rpt":
                                    // Extract Denominations from the first PaymentReciptDS object
                                    var denominations = reportData[0].DenominationDs;
                                    subReport.SetDataSource(denominations);
                                    break;

                                case "PaymentDetailSubReport.rpt":
                                    // Extract Payment Details from the first PaymentReciptDS object
                                    var paymentDetails = reportData[0].PaymentDetailDs;
                                    subReport.SetDataSource(paymentDetails);
                                    break;
                                case "PaymentDetailSubReportLoan.rpt":
                                    // Extract Payment Details from the first PaymentReciptDS object
                                    var paymentDetailsLoan = reportData[0].PaymentDetailDs;
                                    subReport.SetDataSource(paymentDetailsLoan);
                                    break;

                                    // Add more cases if you have more subreports
                            }
                        }
                    }
                    else
                    {
                        HttpContext.Response.Write("<H2>No data found in report source</H2>");
                        return;
                    }
                }
                else
                {
                    HttpContext.Response.Write("<H2>Invalid report source</H2>");
                    return;
                }

                // Set report parameters if needed
                string year = HttpContext.Session["Year"]?.ToString() ?? "Non";
                string dates = HttpContext.Session["Dates"]?.ToString() ?? "Non";
                string strFromDate = HttpContext.Session["DateFrom"]?.ToString() ?? "Non";
                string strToDate = HttpContext.Session["DateTo"]?.ToString() ?? "Non";

                if (year != "Non")
                {
                    rd.SetParameterValue("param", $"Header summary: {year}");
                }

                if (dates != "Non" && !string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    rd.SetParameterValue("DateFrom", strFromDate);
                    rd.SetParameterValue("DateTo", strToDate);
                }

                // Export the report to PDF and send to response
                string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, savedFileName);


            }
            catch (Exception ex)
            {
                // Log the exception
                // Handle specific exceptions if needed
                HttpContext.Response.Write("<H2>An error occurred while generating the report</H2>");
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
        }



        // ====== ADJUST if your report uses different table/alias names ======
        static class ReceiptSchema
        {
            public const string MainTable = "PaymentReciptDS";
            public const string DetailTable = "PaymentDetailDs";
            public const string DenominationTable = "DenominationDs";
            public const string ParentIdColumn = "Id";                 // header PK
            public const string ChildFkColumn = "PaymentReciptDSId";  // FK expected by subreport link
        }

        public void ReportParameterLessWithSubReports()
        {
            var rd = new ReportDocument();

            try
            {
                // ---- Read session inputs
                string strReportName = HttpContext.Session["ReportName"]?.ToString();
                var rptSource = HttpContext.Session["rptSource"];
                string rptpath = HttpContext.Session["rptpath"]?.ToString();
                string rpttitle = HttpContext.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || string.IsNullOrEmpty(rptpath) || string.IsNullOrEmpty(rpttitle))
                {
                    HttpContext.Response.Write("<H2>No Report with such Name found</H2>");
                    return;
                }

                // ---- Load report
                string strRptPath = HttpContext.Server.MapPath(rptpath);
                rd.Load(strRptPath);
                // Safety: don't carry design-time data
                rd.ReportOptions.EnableSaveDataWithReport = false;

                // ---- Build DataSet & bind (PUSH model)
                if (rptSource is List<PaymentReciptDS> reportData && reportData.Count > 0)
                {
                    var ds = BuildReceiptDataSet(reportData);

                    // Main report
                    rd.SetDataSource(ds);

                    // EVERY subreport INSTANCE (handles duplicates in your two-copy layout)
                    foreach (Section sec in rd.ReportDefinition.Sections)
                    {
                        foreach (ReportObject ro in sec.ReportObjects)
                        {
                            if (ro is SubreportObject sro)
                            {
                                var sub = rd.OpenSubreport(sro.SubreportName);
                                BindSubreportTables(sub, ds);
                            }
                        }
                    }
                }
                else
                {
                    HttpContext.Response.Write("<H2>No data found in report source</H2>");
                    return;
                }

                // ---- Parameters (your original logic)
                string year = HttpContext.Session["Year"]?.ToString() ?? "Non";
                string dates = HttpContext.Session["Dates"]?.ToString() ?? "Non";
                string strFrom = HttpContext.Session["DateFrom"]?.ToString() ?? "Non";
                string strTo = HttpContext.Session["DateTo"]?.ToString() ?? "Non";

                if (HttpContext.Session["ReportParameters"] is Dictionary<string, object> parameters &&
                    rd.DataDefinition.ParameterFields.Count > 0)
                {
                    var fields = rd.DataDefinition.ParameterFields.Cast<ParameterFieldDefinition>().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    foreach (var p in parameters)
                        if (fields.Contains(p.Key))
                            rd.SetParameterValue(p.Key, p.Value);
                }

                if (year != "Non")
                    rd.SetParameterValue("param", $"Header summary: {year}");

                if (dates != "Non" && !string.IsNullOrEmpty(strFrom) && !string.IsNullOrEmpty(strTo))
                {
                    rd.SetParameterValue("DateFrom", strFrom);
                    rd.SetParameterValue("DateTo", strTo);
                }

                // ---- Export
                string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat,
                                        System.Web.HttpContext.Current.Response,
                                        false, savedFileName);
            }
            catch
            {
                HttpContext.Response.Write("<H2>An error occurred while generating the report</H2>");
            }
            finally
            {
                CleanReport(rd); // keep your existing cleanup
            }
        }

        /* ======================= Helpers ======================= */

        // Build a DataSet whose table names match the report/subreports
        private static System.Data.DataSet BuildReceiptDataSet(List<PaymentReciptDS> headers)
        {
            var ds = new System.Data.DataSet("ReceiptDS");

            // Header table
            ds.Tables.Add(ToDataTable(headers, ReceiptSchema.MainTable));

            // Detail table with FK to header
            ds.Tables.Add(ToChildTableWithFk(
                parents: headers,
                childSel: h => h.PaymentDetailDs ?? new List<PaymentDetailDS>(),
                tableName: ReceiptSchema.DetailTable,
                fkColumn: ReceiptSchema.ChildFkColumn,
                parentId: ReceiptSchema.ParentIdColumn));

            // Denomination table with FK to header
            ds.Tables.Add(ToChildTableWithFk(
                parents: headers,
                childSel: h => h.DenominationDs ?? new List<DenominationDS>(),
                tableName: ReceiptSchema.DenominationTable,
                fkColumn: ReceiptSchema.ChildFkColumn,
                parentId: ReceiptSchema.ParentIdColumn));

            return ds;
        }

        // Generic: convert list to DataTable using readable scalar properties
        private static DataTable ToDataTable<T>(IEnumerable<T> items, string tableName)
        {
            var dt = new DataTable(tableName);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && IsScalar(p.PropertyType))
                                 .ToArray();

            foreach (var p in props)
                dt.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);

            foreach (var it in items)
            {
                var row = dt.NewRow();
                foreach (var p in props)
                    row[p.Name] = p.GetValue(it) ?? DBNull.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }

        // Build a child DataTable from nested lists and add FK back to parent
        private static DataTable ToChildTableWithFk<TParent, TChild>(
            IEnumerable<TParent> parents,
            Func<TParent, IEnumerable<TChild>> childSel,
            string tableName,
            string fkColumn,
            string parentId)
        {
            // Child scalar props
            var childProps = typeof(TChild).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(p => p.CanRead && IsScalar(p.PropertyType))
                                           .ToArray();

            var dt = new DataTable(tableName);
            foreach (var p in childProps)
                dt.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
            if (!dt.Columns.Contains(fkColumn))
                dt.Columns.Add(fkColumn, typeof(object)); // keep it flexible (Id could be string/Guid/int)

            // Parent Id getter
            var parentIdProp = typeof(TParent).GetProperty(parentId, BindingFlags.Public | BindingFlags.Instance);
            foreach (var parent in parents)
            {
                var pid = parentIdProp?.GetValue(parent);
                var children = childSel(parent) ?? Enumerable.Empty<TChild>();

                foreach (var ch in children)
                {
                    var row = dt.NewRow();
                    foreach (var p in childProps)
                        row[p.Name] = p.GetValue(ch) ?? DBNull.Value;
                    row[fkColumn] = pid ?? DBNull.Value;
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        // Bind subreport tables by name (case-insensitive), supports aliases
        private static void BindSubreportTables(ReportDocument sub, System.Data.DataSet ds)
        {
            foreach (Table t in sub.Database.Tables)
            {
                var dt = FindTable(ds, t.Name);
                if (dt != null)
                {
                    t.SetDataSource(dt);
                }
                else
                {
                    // Optional: try known names
                    if (t.Name.Equals(ReceiptSchema.DetailTable, StringComparison.OrdinalIgnoreCase) && ds.Tables.Contains(ReceiptSchema.DetailTable))
                        t.SetDataSource(ds.Tables[ReceiptSchema.DetailTable]);
                    else if (t.Name.Equals(ReceiptSchema.DenominationTable, StringComparison.OrdinalIgnoreCase) && ds.Tables.Contains(ReceiptSchema.DenominationTable))
                        t.SetDataSource(ds.Tables[ReceiptSchema.DenominationTable]);
                    else if (t.Name.Equals(ReceiptSchema.MainTable, StringComparison.OrdinalIgnoreCase) && ds.Tables.Contains(ReceiptSchema.MainTable))
                        t.SetDataSource(ds.Tables[ReceiptSchema.MainTable]);
                }
            }
        }

        // Case-insensitive lookup; also tolerates schema/alias prefixes
        private static DataTable FindTable(System.Data.DataSet ds, string crystalTableName)
        {
            if (string.IsNullOrEmpty(crystalTableName)) return null;

            // exact (case-insensitive)
            foreach (DataTable dt in ds.Tables)
                if (string.Equals(dt.TableName, crystalTableName, StringComparison.OrdinalIgnoreCase))
                    return dt;

            // last token after '.' (handles dbo.Table, Command aliases)
            var last = crystalTableName.Split('.').Last();
            foreach (DataTable dt in ds.Tables)
                if (string.Equals(dt.TableName.Split('.').Last(), last, StringComparison.OrdinalIgnoreCase))
                    return dt;

            return null;
        }

        private static bool IsScalar(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            return t.IsPrimitive
                || t.IsEnum
                || t == typeof(string)
                || t == typeof(decimal)
                || t == typeof(DateTime)
                || t == typeof(Guid)
                || t == typeof(TimeSpan)
                || t == typeof(double)
                || t == typeof(float);
        }



        public void ReportParameterLessWithSubReportsxxxx()
        {
            ReportDocument rd = new ReportDocument();

            try
            {
                // --- Read session inputs
                string strReportName = HttpContext.Session["ReportName"]?.ToString();
                var rptSource = HttpContext.Session["rptSource"];
                string rptpath = HttpContext.Session["rptpath"]?.ToString();
                string rpttitle = HttpContext.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    HttpContext.Response.Write("<H2>No Report with such Name found</H2>");
                    return;
                }

                // --- Load report
                string strRptPath = HttpContext.Server.MapPath(rptpath);
                rd.Load(strRptPath);
                // Optional hygiene: make sure design-time data isn't carried along
                // rd.ReportOptions.EnableSaveDataWithReport = false;

                // --- Bind main + subreports (PUSH model)
                if (rptSource is List<PaymentReciptDS> reportData && reportData.Count > 0)
                {
                    rd.SetDataSource(reportData);

                    // Prepare the collections you'll feed to subreports
                    var denominations = reportData[0].DenominationDs ?? new List<DenominationDS>();
                    var paymentDetails = reportData[0].PaymentDetailDs ?? new List<PaymentDetailDS>();
                    var paymentDetailsL = reportData[0].PaymentDetailDs ?? new List<PaymentDetailDS>(); // if loan uses same list

                    // IMPORTANT: bind EVERY subreport INSTANCE (handles duplicates)
                    foreach (Section sec in rd.ReportDefinition.Sections)
                    {
                        foreach (ReportObject ro in sec.ReportObjects)
                        {
                            if (ro is SubreportObject sro)
                            {
                                ReportDocument sub = rd.OpenSubreport(sro.SubreportName);
                                string name = (sub.Name ?? "").ToLowerInvariant();

                                if (name == "denominationsubreport.rpt")
                                {
                                    sub.SetDataSource(denominations);
                                }
                                else if (name == "paymentdetailsubreport.rpt")
                                {
                                    sub.SetDataSource(paymentDetails);
                                }
                                else if (name == "paymentdetailsubreportloan.rpt")
                                {
                                    sub.SetDataSource(paymentDetailsL);
                                }
                                // add more else-if blocks if you add more subreports
                            }
                        }
                    }
                }
                else
                {
                    HttpContext.Response.Write("<H2>No data found in report source</H2>");
                    return;
                }

                // --- Parameters (your existing code)
                string year = HttpContext.Session["Year"]?.ToString() ?? "Non";
                string dates = HttpContext.Session["Dates"]?.ToString() ?? "Non";
                string strFrom = HttpContext.Session["DateFrom"]?.ToString() ?? "Non";
                string strTo = HttpContext.Session["DateTo"]?.ToString() ?? "Non";

                if (Session["ReportParameters"] is Dictionary<string, object> parameters &&
                    rd.DataDefinition.ParameterFields.Count > 0)
                {
                    foreach (var p in parameters)
                    {
                        if (rd.DataDefinition.ParameterFields.Cast<ParameterFieldDefinition>()
                              .Any(f => f.Name.Equals(p.Key, StringComparison.OrdinalIgnoreCase)))
                        {
                            rd.SetParameterValue(p.Key, p.Value);
                        }
                    }
                }

                if (year != "Non")
                    rd.SetParameterValue("param", $"Header summary: {year}");

                if (dates != "Non" && !string.IsNullOrEmpty(strFrom) && !string.IsNullOrEmpty(strTo))
                {
                    rd.SetParameterValue("DateFrom", strFrom);
                    rd.SetParameterValue("DateTo", strTo);
                }

                // --- Export
                string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat,
                                        System.Web.HttpContext.Current.Response,
                                        false, savedFileName);
            }
            catch
            {
                HttpContext.Response.Write("<H2>An error occurred while generating the report</H2>");
            }
            finally
            {
                CleanReport(rd);
            }
        }


        public void ReportParameterLessWithSubReportsx()
        {                // Create a new ReportDocument
            ReportDocument rd = new ReportDocument();

            try
            {
                // Retrieve parameters from session
                string strReportName = HttpContext.Session["ReportName"]?.ToString();
                var rptSource = HttpContext.Session["rptSource"];
                var rptpath = HttpContext.Session["rptpath"]?.ToString();
                var rpttitle = HttpContext.Session["rpttitle"]?.ToString();

                if (string.IsNullOrEmpty(strReportName) || rptSource == null || rptpath == null || rpttitle == null)
                {
                    HttpContext.Response.Write("<H2>No Report with such Name found</H2>");
                    return;
                }

                string strRptPath = HttpContext.Server.MapPath(rptpath);
                rd.Load(strRptPath);

                // Cast rptSource to List<PaymentReciptDS>
                if (rptSource is List<PaymentReciptDS> reportData)
                {
                    // Check if the list has at least one item
                    if (reportData.Count > 0)
                    {
                        // Set data source for the main report
                        rd.SetDataSource(reportData);

                        // Set data sources for subreports
                        foreach (ReportDocument subReport in rd.Subreports)
                        {
                            string subReportName = subReport.Name;

                            switch (subReportName)
                            {
                                case "DenominationSubReport.rpt":
                                    // Extract Denominations from the first PaymentReciptDS object
                                    var denominations = reportData[0].DenominationDs;
                                    subReport.SetDataSource(denominations);
                                    break;

                                case "PaymentDetailSubReport.rpt":
                                    // Extract Payment Details from the first PaymentReciptDS object
                                    var paymentDetails = reportData[0].PaymentDetailDs;
                                    subReport.SetDataSource(paymentDetails);
                                    break;
                                case "PaymentDetailSubReportLoan.rpt":
                                    // Extract Payment Details from the first PaymentReciptDS object
                                    var paymentDetailsLoan = reportData[0].PaymentDetailDs;
                                    subReport.SetDataSource(paymentDetailsLoan);
                                    break;

                                    // Add more cases if you have more subreports
                            }
                        }
                    }
                    else
                    {
                        HttpContext.Response.Write("<H2>No data found in report source</H2>");
                        return;
                    }
                }
                else
                {
                    HttpContext.Response.Write("<H2>Invalid report source</H2>");
                    return;
                }

                // Set report parameters if needed
                string year = HttpContext.Session["Year"]?.ToString() ?? "Non";
                string dates = HttpContext.Session["Dates"]?.ToString() ?? "Non";
                string strFromDate = HttpContext.Session["DateFrom"]?.ToString() ?? "Non";
                string strToDate = HttpContext.Session["DateTo"]?.ToString() ?? "Non";

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

                            return;
                        }
                    }
                }

                if (year != "Non")
                {
                    rd.SetParameterValue("param", $"Header summary: {year}");
                }

                if (dates != "Non" && !string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    rd.SetParameterValue("DateFrom", strFromDate);
                    rd.SetParameterValue("DateTo", strToDate);
                }

                // Export the report to PDF and send to response
                string savedFileName = $"{rpttitle}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, savedFileName);

            }
            catch (Exception ex)
            {
                // Log the exception
                // Handle specific exceptions if needed
                HttpContext.Response.Write("<H2>An error occurred while generating the report</H2>");
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
        }
        public ActionResult DownloadFromJquery(string Filename)
        {
            string filePath = Path.Combine(Server.MapPath("~/AppFiles/Exports"), Filename);
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound();
            }
            //Response.Clear();
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", $"attachment; filename=\"{excelName}\"");
            //Response.BinaryWrite(memoryStream.ToArray());
            //Response.Flush();
            //Response.Close();
            return File(filePath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Filename);
        }

        public ActionResult AccountingPDFReport(string FileType = "")
        {
            ReportDocument rd = new ReportDocument();

            var Message = (string)this.HttpContext.Session["errorMessage"];
            try
            {

                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                 
                string fileType = System.Web.HttpContext.Current.Session["fileType"].ToString();
                string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();

                string rptType = System.Web.HttpContext.Current.Session["rptType"].ToString();

                if (rptSource != "empty")
                {
                    List<TrialBalance6ColumnDto> trialBalance6ColumnDto = new List<TrialBalance6ColumnDto>();
                    List<TrialBalance4ColumnDto> trialBalance4ColumnDto = new List<TrialBalance4ColumnDto>();
                    List<ManualEntry> manualEntries = new List<ManualEntry>();
                    AccountingGeneralLedger accountingGeneralLedger = new AccountingGeneralLedger();
                    AccountingEntriesReport journalEntryDto = new AccountingEntriesReport();
                    AccountingGeneralLedgerDetails accountingGeneralLedgerDetails = new AccountingGeneralLedgerDetails();
                    if (fileType.Contains("TB6"))
                    {
                        trialBalance6ColumnDto = (List<TrialBalance6ColumnDto>)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        rd.SetDataSource(trialBalance6ColumnDto);

                    }
                    else if (fileType.Contains("TB4"))
                    {
                        trialBalance4ColumnDto = (List<TrialBalance4ColumnDto>)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        rd.SetDataSource(trialBalance4ColumnDto);

                    }
                    else if (fileType.Contains("JE"))
                    {
                        journalEntryDto = (AccountingEntriesReport)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        rd.SetDataSource(journalEntryDto.BuildJournalEntry(journalEntryDto, GetUserDto().FullName));

                    }
                    else if (fileType.Contains("GL"))
                    {
                        accountingGeneralLedgerDetails = (AccountingGeneralLedgerDetails)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        var listData = accountingGeneralLedgerDetails.ConvertToGeneralLedgerDto(accountingGeneralLedgerDetails);
                        rd.SetDataSource(listData);

                    }
                    else if (fileType.Contains("MET"))
                    {
                        manualEntries = (List<ManualEntry>)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        var listData = manualEntries;//<<accountingGeneralLedgerDetails.ConvertToGeneralLedgerDto(accountingGeneralLedgerDetails);
                        rd.SetDataSource(listData);

                    }
                    else if (fileType.Contains("JournalEntryReference"))
                    {
                        var journalEntries = (List<AccountingEntryReport>)rptSource;

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);
                        var listData = journalEntries;//<<accountingGeneralLedgerDetails.ConvertToGeneralLedgerDto(accountingGeneralLedgerDetails);
                        rd.SetDataSource(listData);
                    }
                        string SavedFileName = string.Format($"{rptType}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                    //Export the report to a byte array
                    Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    //Clear the response and set the content type
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.ContentType = "application/pdf";

                    //Write the report bytes to the response
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();

                }
            }
            catch (Exception ex)
            {
                return Json(Message, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
            return View();
        }

        /// <summary>
        /// Generates and exports accounting reports as PDF documents
        /// Supports multiple report types: Trial Balance (4/6 column), Journal Entries, General Ledger, and Manual Entries
        /// </summary>
        /// <param name="FileType">Optional parameter for file type specification (currently unused)</param>
        /// <returns>ActionResult - typically returns a View or PDF response</returns>
        //public ActionResult AccountingPDFReport(string FileType = "")
        //{
        //    try
        //    {
        //        // Retrieve report configuration from session
        //        var reportConfiguration = GetReportConfigurationFromSession();

        //        // Validate session data before proceeding
        //        if (!IsValidReportConfiguration(reportConfiguration))
        //        {
        //            return HandleInvalidConfiguration();
        //        }

        //        // Generate the PDF report
        //        var pdfBytes = GenerateReportPdf(reportConfiguration);

        //        // Return the PDF to the client
        //        return DeliverPdfResponse(pdfBytes, reportConfiguration.ReportType);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception for debugging purposes
        //        LogException(ex);

        //        // Return user-friendly error response
        //        return HandleReportGenerationError();
        //    }
        //}

        #region Private Helper Methods

        /// <summary>
        /// Extracts report configuration parameters from the current session
        /// </summary>
        /// <returns>ReportConfiguration object containing session parameters</returns>
        private ReportConfiguration GetReportConfigurationFromSession()
        {
            var session = System.Web.HttpContext.Current.Session;

            return new ReportConfiguration
            {
                ReportSource = session["rptSource"],
                FileType = session["fileType"]?.ToString() ?? string.Empty,
                ReportPath = session["rptpath"]?.ToString() ?? string.Empty,
                ReportType = session["rptType"]?.ToString() ?? string.Empty
            };
        }

        /// <summary>
        /// Validates that the report configuration contains required data
        /// </summary>
        /// <param name="config">Report configuration to validate</param>
        /// <returns>True if configuration is valid, false otherwise</returns>
        private bool IsValidReportConfiguration(ReportConfiguration config)
        {
            return config.ReportSource != null &&
                   config.ReportSource.ToString() != "empty" &&
                   !string.IsNullOrEmpty(config.FileType) &&
                   !string.IsNullOrEmpty(config.ReportPath) &&
                   !string.IsNullOrEmpty(config.ReportType);
        }

        /// <summary>
        /// Generates PDF bytes for the specified report configuration
        /// </summary>
        /// <param name="config">Report configuration containing data source and settings</param>
        /// <returns>Byte array containing the generated PDF</returns>
        private byte[] GenerateReportPdf(ReportConfiguration config)
        {
            ReportDocument reportDocument = new ReportDocument();

            try
            {
                // Load the Crystal Report template
                string reportTemplatePath = Server.MapPath(config.ReportPath);
                reportDocument.Load(reportTemplatePath);

                // Set the data source based on report type
                SetReportDataSource(reportDocument, config);

                // Export report to PDF format
                using (Stream stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat))
                {
                    byte[] pdfBytes = new byte[stream.Length];
                    stream.Read(pdfBytes, 0, pdfBytes.Length);
                    return pdfBytes;
                }
            }
            finally
            {
                // Ensure proper cleanup of Crystal Report resources
                CleanReport(reportDocument);
            }
        }

        /// <summary>
        /// Sets the appropriate data source for the report based on file type
        /// </summary>
        /// <param name="reportDocument">Crystal Report document to configure</param>
        /// <param name="config">Report configuration containing data source and type information</param>
        private void SetReportDataSource(ReportDocument reportDocument, ReportConfiguration config)
        {
            switch (GetReportTypeFromFileType(config.FileType))
            {
                case ReportType.TrialBalance6Column:
                    SetTrialBalance6ColumnDataSource(reportDocument, config.ReportSource);
                    break;

                case ReportType.TrialBalance4Column:
                    SetTrialBalance4ColumnDataSource(reportDocument, config.ReportSource);
                    break;

                case ReportType.JournalEntry:
                    SetJournalEntryDataSource(reportDocument, config.ReportSource);
                    break;

                case ReportType.GeneralLedger:
                    SetGeneralLedgerDataSource(reportDocument, config.ReportSource);
                    break;

                case ReportType.ManualEntry:
                    SetManualEntryDataSource(reportDocument, config.ReportSource);
                    break;

                default:
                    throw new ArgumentException($"Unsupported file type: {config.FileType}");
            }
        }

        /// <summary>
        /// Determines the report type based on the file type string
        /// </summary>
        /// <param name="fileType">File type string from session</param>
        /// <returns>Corresponding ReportType enum value</returns>
        private ReportType GetReportTypeFromFileType(string fileType)
        {
            if (fileType.Contains("TB6")) return ReportType.TrialBalance6Column;
            if (fileType.Contains("TB4")) return ReportType.TrialBalance4Column;
            if (fileType.Contains("JE")) return ReportType.JournalEntry;
            if (fileType.Contains("GL")) return ReportType.GeneralLedger;
            if (fileType.Contains("MET")) return ReportType.ManualEntry;

            return ReportType.Unknown;
        }

        /// <summary>
        /// Sets data source for 6-column Trial Balance reports
        /// </summary>
        private void SetTrialBalance6ColumnDataSource(ReportDocument reportDocument, object dataSource)
        {
            var trialBalance6ColumnDto = (List<TrialBalance6ColumnDto>)dataSource;
            reportDocument.SetDataSource(trialBalance6ColumnDto);
        }

        /// <summary>
        /// Sets data source for 4-column Trial Balance reports
        /// </summary>
        private void SetTrialBalance4ColumnDataSource(ReportDocument reportDocument, object dataSource)
        {
            var trialBalance4ColumnDto = (List<TrialBalance4ColumnDto>)dataSource;
            reportDocument.SetDataSource(trialBalance4ColumnDto);
        }

        /// <summary>
        /// Sets data source for Journal Entry reports
        /// </summary>
        private void SetJournalEntryDataSource(ReportDocument reportDocument, object dataSource)
        {
            var journalEntryDto = (AccountingEntriesReport)dataSource;
            var processedData = journalEntryDto.BuildJournalEntry(journalEntryDto, GetUserDto().FullName);
            reportDocument.SetDataSource(processedData);
        }

        /// <summary>
        /// Sets data source for General Ledger reports
        /// </summary>
        private void SetGeneralLedgerDataSource(ReportDocument reportDocument, object dataSource)
        {
            var accountingGeneralLedgerDetails = (AccountingGeneralLedgerDetails)dataSource;
            var convertedData = accountingGeneralLedgerDetails.ConvertToGeneralLedgerDto(accountingGeneralLedgerDetails);
            reportDocument.SetDataSource(convertedData);
        }

        /// <summary>
        /// Sets data source for Manual Entry reports
        /// </summary>
        private void SetManualEntryDataSource(ReportDocument reportDocument, object dataSource)
        {
            var manualEntries = (List<ManualEntry>)dataSource;
            reportDocument.SetDataSource(manualEntries);
        }

        /// <summary>
        /// Delivers the PDF response to the client browser
        /// </summary>
        /// <param name="pdfBytes">PDF content as byte array</param>
        /// <param name="reportType">Type of report for filename generation</param>
        /// <returns>ActionResult that writes PDF to response</returns>
        private ActionResult DeliverPdfResponse(byte[] pdfBytes, string reportType)
        {
            // Generate unique filename with timestamp
            string fileName = GenerateReportFileName(reportType);

            // Configure response headers for PDF delivery
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}.pdf");

            // Write PDF content to response
            Response.BinaryWrite(pdfBytes);
            Response.Flush();
            Response.End();

            return new EmptyResult();
        }

        /// <summary>
        /// Generates a unique filename for the report
        /// </summary>
        /// <param name="reportType">Type of report</param>
        /// <returns>Formatted filename string</returns>
        private string GenerateReportFileName(string reportType)
        {
            return $"{reportType}-{DateTime.UtcNow:dd_MM_yyyy_HHmmss}";
        }

        /// <summary>
        /// Handles cases where session configuration is invalid
        /// </summary>
        /// <returns>ActionResult with error message</returns>
        private ActionResult HandleInvalidConfiguration()
        {
            Response.Write("<H2>Invalid report configuration. Please try again.</H2>");
            return View();
        }

        /// <summary>
        /// Handles report generation errors
        /// </summary>
        /// <returns>ActionResult with error message</returns>
        private ActionResult HandleReportGenerationError()
        {
            Response.Write("<H2>An error occurred while generating the report</H2>");
            return View();
        }

        /// <summary>
        /// Logs exceptions for debugging purposes
        /// </summary>
        /// <param name="exception">Exception to log</param>
        private void LogException(Exception exception)
        {
            // TODO: Implement proper logging mechanism
            // Example: Logger.Error("Report generation failed", exception);
            System.Diagnostics.Debug.WriteLine($"Report generation error: {exception}");
        }

        #endregion

        #region Helper Classes and Enums

        /// <summary>
        /// Configuration object for report generation
        /// </summary>
        private class ReportConfiguration
        {
            public object ReportSource { get; set; }
            public string FileType { get; set; }
            public string ReportPath { get; set; }
            public string ReportType { get; set; }
        }

        /// <summary>
        /// Enumeration of supported report types
        /// </summary>
        private enum ReportType
        {
            Unknown,
            TrialBalance6Column,
            TrialBalance4Column,
            JournalEntry,
            GeneralLedger,
            ManualEntry
        }

        #endregion
        public void CleanReport(ReportDocument rd)
        {
            if (rd != null)
            {
                rd.Close();
                rd.Clone();
                rd.Dispose();
                rd = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }
        public void ReportWithParameter()
        {
            ReportDocument rd = new ReportDocument();
            try
            {
                // Initialize validity flag
                bool isValid = true;

                // Retrieve session values


                string parameters = System.Web.HttpContext.Current.Session["param_size"]?.ToString() ?? "N/A";

                if (parameters == "4")
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strDatePrinted = System.Web.HttpContext.Current.Session["DatePrinted"]?.ToString() ?? "N/A";
                    //string strBrnachName = System.Web.HttpContext.Current.Session["BranchName"]?.ToString() ?? "N/A";
                    string strPrintedBy = System.Web.HttpContext.Current.Session["FullName"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        // Load and configure the report document

                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        SetReportParameter(rd, "DateFrom", strFromDate);
                        SetReportParameter(rd, "DateTo", strToDate);
                        SetReportParameter(rd, "PrintedBy", strPrintedBy);
                        SetReportParameter(rd, "DateNow", strDatePrinted);
                        //SetReportParameter(rd, "BranchName", strBrnachName);

                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                        // Clean up the report document
                        CleanReport(rd);
                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }
                else if (parameters == "3")
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strBrnachName = System.Web.HttpContext.Current.Session["RPTBranchName"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        SetReportParameter(rd, "DateFrom", strFromDate);
                        SetReportParameter(rd, "DateTo", strToDate);
                        SetReportParameter(rd, "BranchName", strBrnachName);

                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }

                else if (parameters == "5")
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strBrnachName = System.Web.HttpContext.Current.Session["RPTBranchName"]?.ToString() ?? "N/A";
                    string strPrintedBy = System.Web.HttpContext.Current.Session["PrintedBy"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        SetReportParameter(rd, "DateFrom", strFromDate);
                        SetReportParameter(rd, "DateTo", strToDate);
                        SetReportParameter(rd, "BranchName", strBrnachName);
                        SetReportParameter(rd, "PrintedBy", strPrintedBy);
                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                        // Clean up the report document
                        CleanReport(rd);
                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }
                else if (parameters == "6")
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strPrintedBy = System.Web.HttpContext.Current.Session["PrintedBy"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        SetReportParameter(rd, "DateFrom", strFromDate);
                        SetReportParameter(rd, "DateTo", strToDate);
                        SetReportParameter(rd, "PrintedBy", strPrintedBy);
                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                        // Clean up the report document
                        CleanReport(rd);
                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }
                else if (parameters == "member_listing")
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    //string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    //string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strPrintedBy = System.Web.HttpContext.Current.Session["ParamTitle"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        //SetReportParameter(rd, "DateFrom", strFromDate);
                        //SetReportParameter(rd, "DateTo", strToDate);
                        SetReportParameter(rd, "ParamTitle", strPrintedBy);
                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                        // Clean up the report document
                        CleanReport(rd);
                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }
                else
                {
                    string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                    var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                    string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                    string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                    string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                    string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();
                    // Validate if the report Name is present
                    if (string.IsNullOrEmpty(strReportName))
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        string strRptPath = Server.MapPath(rptpath);
                        rd.Load(strRptPath);

                        // Set data source if available
                        if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                        {
                            rd.SetDataSource(rptSource);
                        }

                        // Set report parameters
                        SetReportParameter(rd, "DateFrom", strFromDate);
                        SetReportParameter(rd, "DateTo", strToDate);

                        // Export the report to PDF
                        string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                        rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                        // Clean up the report document
                    }
                    else
                    {
                        Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                    }
                }


            }
            catch (Exception ex)
            {
                // Handle specific known exception
                if (ex.Message.Contains("Error in formula PercentagePassed"))
                {
                    Response.Write("<H2>No Data was found</H2>");
                }
                else
                {
                    Response.Write($"{ex.ToString()}<H2>Nothing Found; report session expired</H2>");
                }
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
        }

        public void ReportWithParameters()
        {
            ReportDocument rd = new ReportDocument();

            try
            {
                // Initialize validity flag
                bool isValid = true;

                // Retrieve session values
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"]?.ToString();
                var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
                string rptpath = System.Web.HttpContext.Current.Session["rptpath"]?.ToString();
                string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"]?.ToString() ?? "N/A";
                string strToDate = System.Web.HttpContext.Current.Session["DateTo"]?.ToString() ?? "N/A";
                string strDatePrinted = System.Web.HttpContext.Current.Session["DatePrinted"]?.ToString() ?? "N/A";
                string strPrintedBy = System.Web.HttpContext.Current.Session["FullName"]?.ToString() ?? "N/A";
                string strtitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();

                // Validate if the report Name is present
                if (string.IsNullOrEmpty(strReportName))
                {
                    isValid = false;
                }

                if (isValid)
                {
                    // Load and configure the report document
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);

                    // Set data source if available
                    if (rptSource != null && rptSource.GetType().ToString() != "System.String")
                    {
                        rd.SetDataSource(rptSource);
                    }

                    // Set report parameters
                    SetReportParameter(rd, "DateFrom", strFromDate);
                    SetReportParameter(rd, "DateTo", strToDate);
                    SetReportParameter(rd, "PrintedBy", strPrintedBy);
                    SetReportParameter(rd, "DateNow", strDatePrinted);

                    // Export the report to PDF
                    string SavedFileName = $"{strtitle}-{DateTime.UtcNow.ToString("dd_MM_yyyy_HHmmss")}";
                    rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);

                    // Clean up the report document
                    CleanReport(rd);
                }
                else
                {
                    Response.Write("<H2>Nothing Found; No Report Name found</H2>");
                }
            }
            catch (Exception ex)
            {
                // Handle specific known exception
                if (ex.Message.Contains("Error in formula PercentagePassed"))
                {
                    Response.Write("<H2>No Data was found</H2>");
                }
                else
                {
                    Response.Write($"{ex.ToString()}<H2>Nothing Found; report session expired</H2>");
                }
            }
            finally
            {
                CleanReport(rd); // ✅ Guaranteed cleanup
            }
        }


        // Helper method to set report parameter
        private void SetReportParameter(ReportDocument report, string parameterName, string parameterValue)
        {
            if (!string.IsNullOrEmpty(parameterValue))
            {
                report.SetParameterValue(parameterName, parameterValue);
            }

        }

        public ActionResult DownloadExcelFilelist()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource" + Session.SessionID];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            var model = rptSource;

            Export export = new Export();
            export.ToExcel(Response, model as IEnumerable<object>, strtitle);


            return new EmptyResult();


        }

        public ActionResult Download()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
            var model = rptSource;

            if (rpTType == "EXCEL")
            {
                Export export = new Export();
                export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            }
            else
            {
                string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();

                if (rptSource != "empty")
                {
                    ReportDocument rd = new ReportDocument();
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);

                    rd.SetDataSource(rptSource);
                    string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                    // Export the report to a byte array
                    Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    // Clear the response and set the content type
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.ContentType = "application/pdf";

                    // Write the report bytes to the response
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                    //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    //CleanReport(rd);


                }

            }
            return new EmptyResult();


        }

        public ActionResult DownloadExcelFile()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
            var model = rptSource;

            if (rpTType == "EXCEL")
            {
                Export export = new Export();
                export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            }
            else
            {
                string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();

                if (rptSource != "empty")
                {
                    ReportDocument rd = new ReportDocument();
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);

                    rd.SetDataSource(rptSource);
                    string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                    // Export the report to a byte array
                    Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    // Clear the response and set the content type
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.ContentType = "application/pdf";

                    // Write the report bytes to the response
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                    //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    //CleanReport(rd);


                }

            }
            return new EmptyResult();


        }

        public ActionResult DownloadBSFile()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var dtoPasser = System.Web.HttpContext.Current.Session["dtoPasser"];
            var strtitle = System.Web.HttpContext.Current.Session["rpttitle"];
            var rpTType = System.Web.HttpContext.Current.Session["rptType"];
            var rptpath = System.Web.HttpContext.Current.Session["rptpath"];


            if (rpTType == "EXCEL")
            {
                //Export export = new Export();
                //export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            }
            else
            {

                if (rptSource != "empty" && rptSource !=null)
                {
                    if (rpTType.ToString().ToUpper() == "BS")
                    {
                        var model = (BalanceSheetData)rptSource;
                        var user = this.GetUserDto();
                        var modeli = (BSQuery)dtoPasser;
                        var assetsModel = model.ConvertToBalanceSheetInfo($"{user.firstName} {user.lastName}", modeli.ToDate.ToString("dd-MM-yyyy"), BSCartegory.Assets, BSCartegory.LIABILITIES);
                        var LiabilityModel = model.ConvertToBalanceSheetInfo($"{user.firstName} {user.lastName}", modeli.ToDate.ToString("dd-MM-yyyy"), BSCartegory.Assets, BSCartegory.LIABILITIES);

                        ReportDocument rd = new ReportDocument();
                        string strRptPath = Server.MapPath(rptpath.ToString());
                        rd.Load(strRptPath);
                        rd.SetDataSource(assetsModel);
                        rd.SetDataSource(LiabilityModel);
                        string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                        //Export the report to a byte array
                        Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);

                        //Clear the response and set the content type
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";

                        //Write the report bytes to the response
                        Response.BinaryWrite(bytes);
                        Response.Flush();
                        Response.End();
                    }
                    else
                    {
                        var model = (IncomeAndExpenseDto)rptSource;
                        var user = this.GetUserDto();
                        var modeli = (BSQuery)dtoPasser;
                        var assetsModel = model.ConvertToIncomeStatementModel($"{user.firstName}");
                        ReportDocument rd = new ReportDocument();
                        string strRptPath = Server.MapPath(rptpath.ToString());
                        rd.Load(strRptPath);
                        rd.SetDataSource(assetsModel);

                        string SavedFileName = string.Format($"{strtitle}");
                        //Export the report to a byte array
                        Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);

                        //Clear the response and set the content type
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Response.ContentType = "application/pdf";

                        //Write the report bytes to the response
                        Response.BinaryWrite(bytes);
                        Response.Flush();
                        Response.End();
                    }
                    //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    //CleanReport(rd);

                }

            }
            return new EmptyResult();


        }
        static string GetDayWithSuffix(int day)
        {
            string suffix = "th";
            if (day % 10 == 1 && day != 11) suffix = "st";
            else if (day % 10 == 2 && day != 12) suffix = "nd";
            else if (day % 10 == 3 && day != 13) suffix = "rd";

            return day.ToString("00") + suffix;
        }
        //public ActionResult DownloadBSFile()
        //{
        //    try
        //    {
        //        var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
        //        var dtoPasser = System.Web.HttpContext.Current.Session["dtoPasser"];
        //        string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
        //        string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
        //        string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
        //        var model = (BalanceSheetData)rptSource;

        //        if (rpTType == "EXCEL")
        //        {
        //            Export export = new Export();
        //            export.ToExcel(Response, model as IEnumerable<object>, strtitle);
        //        }
        //        else
        //        {

        //            if (rptSource != "empty")
        //            {
        //                var user = this.GetUserDto();
        //                var modeli = (BSQuery)dtoPasser;

        //                var assetsModel = model.ConvertToBalanceSheetInfo($"{user.firstName} {user.lastName}", modeli.Date.ToString("dd/MM/yyyy"), BSCartegory.Assets, BSCartegory.LIABILITIES);

        //                ReportDocument rd = new ReportDocument();
        //                string strRptPath = Server.MapPath(rptpath);
        //                rd.Load(strRptPath);
        //                rd.SetDataSource(assetsModel);



        //                string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
        //                // Export the report to a byte array
        //                Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
        //                byte[] bytes = new byte[stream.Length];
        //                stream.Read(bytes, 0, bytes.Length);

        //                // Clear the response and set the content type
        //                Response.ClearContent();
        //                Response.ClearHeaders();
        //                Response.ContentType = "application/pdf";

        //                // Write the report bytes to the response
        //                Response.BinaryWrite(bytes);
        //                Response.Flush();
        //                Response.End();
        //                //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
        //                //CleanReport(rd);


        //            }
        //        }
        //    }
        //    catch (LogOnException logOnEx)
        //    {
        //        // Handle Crystal Reports logon exception
        //        // Log the exception details and provide a user-friendly message
        //        // Example logging (assuming you have a logging mechanism)
        //        //Logger.LogError("Crystal Reports logon failed.", logOnEx);
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while trying to generate the report.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle general exceptions
        //        // Log the exception details and provide a user-friendly message
        //        //Logger.LogError("An error occurred while generating the report.", ex);
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        //    }

        //    return new EmptyResult();
        //}

        public ActionResult DownloadExcelFileForTB4C()
        {
            ReportDocument rd = new ReportDocument();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            string rpTType = System.Web.HttpContext.Current.Session["rptType"].ToString();
            string rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var model = rptSource;

            if (rpTType == "EXCEL")
            {
                Export export = new Export();
                export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            }
            else
            {

                if (rptSource != "empty")
                {
                    string strRptPath = Server.MapPath(rptpath);
                    rd.Load(strRptPath);

                    rd.SetDataSource(rptSource);
                    string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.Date.ToString("dd_mm_yyyy_hhmmss")}");
                    // Export the report to a byte array
                    Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    // Clear the response and set the content type
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.ContentType = "application/pdf";

                    // Write the report bytes to the response
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                    //rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
                    //CleanReport(rd);


                }

            }
            return new EmptyResult();


        }


        public ActionResult PrintAccountLedgerDtoInExcel()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<AccountLedgerDto> accounts = (rptSource == "empty") ? new List<AccountLedgerDto>() : (List<AccountLedgerDto>)rptSource;
            AccountLedgerDto trialBalance = (rptSource == "empty") ? new AccountLedgerDto() : accounts[0];
            var rpt = " General Ledger as of the " + trialBalance.FromDate;

            if ((rptSource == "empty"))
            {
                return new EmptyResult();
            }
            else
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(rpttitle);
                    var headerStyle = workbook.Style;

                    headerStyle.Font.Bold = true;
                    headerStyle.Font.FontSize = 14;
                    headerStyle.Font.FontColor = XLColor.Black;
                    // Apply border to branch range
                    headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print letterhead
                    worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                    worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}";
                    worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                    worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                    worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                    worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                    worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                    worksheet.Cell(1, 1).Value = "BranchName";
                    worksheet.Cell(2, 1).Value = $"Address";
                    worksheet.Cell(3, 1).Value = $"Capital";
                    worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                    worksheet.Cell(5, 1).Value = $"Website";
                    worksheet.Cell(6, 1).Value = $"Branch Telephone";
                    worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                    // Apply header style

                    var headerRange = worksheet.Range(1, 1, 7, 1);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Font.FontSize = 12;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    var headerRange2 = worksheet.Range(1, 1, 7, 2);

                    headerRange2.Style.Font.FontSize = 12;

                    headerRange2.Style.Alignment.WrapText = true;
                    headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print account details
                    var titleRange = worksheet.Range("A10:D10");
                    titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 14;
                    titleRange.Style.Font.FontColor = XLColor.Black;
                    // Apply border to title range
                    titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    titleRange.Value = rpt;
                    // Print balance sheet header
                    worksheet.Cell(12, 1).Value = "Account Number";
                    worksheet.Cell(12, 2).Value = "Account Name";
                    worksheet.Cell(12, 3).Value = "Current Balance";
                    //worksheet.Cell(12, 4).Value = "Debit Balance";
                    //worksheet.Cell(12, 5).Value = "Credit Balance";
                    //worksheet.Cell(12, 6).Value = "Current Balance";


                    // Apply header style
                    var headerRange0 = worksheet.Range(12, 1, 12, 3);
                    headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange0.Style.Font.FontSize = 12;
                    headerRange0.Style.Font.Bold = true;
                    headerRange0.Style.Alignment.WrapText = true;
                    headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 3);
                    headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    int row = 13;
                    foreach (var account in accounts)
                    {
                        worksheet.Cell(row, 1).Value = account.AccountNumber;
                        worksheet.Cell(row, 2).Value = account.AccountName;
                        worksheet.Cell(row, 3).Value = account.CurrentBalance;


                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    if ((rptSource == "empty"))
                    {

                    }
                    else
                    {
                        //var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
                        //footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        //footerRange.Style.Font.FontSize = 12;
                        //footerRange.Style.Font.Bold = true;
                        //footerRange.Style.Alignment.WrapText = false;
                        //footerRange.Style.Alignment.JustifyLastLine = false;
                        //footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        //footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        //footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        //footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        //footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        //// Print totals
                        //worksheet.Cell(row, 2).Value = "Totals";
                        //worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                        //worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                        //worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                        //worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();

                    }


                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "GeneralLedger.xlsx");
                    }
                    //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                    // Save the workbook

                }
            }

            //return new EmptyResult(); 
        }

        public ActionResult PrintMFIChartOfAccount()
        {
            try
            {
                var rptSource = System.Web.HttpContext.Current.Session["rptSource" + Session.SessionID];
                var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString();

                if (rptSource == null || rpttitle == null)
                {
                    // Handle the case where session variables are not set
                    return new HttpStatusCodeResult(400, "Report source or title is missing.");
                }

                List<ChartofAccountManagementPosition> accounts = (rptSource.ToString() == "empty")
                    ? new List<ChartofAccountManagementPosition>()
                    : rptSource as List<ChartofAccountManagementPosition>;

                if (rptSource.ToString() == "empty")
                {
                    // Return early if there is no data
                    return new HttpStatusCodeResult(204, "No content available for the report.");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(rpttitle);
                    var headerStyle = workbook.Style;
                    headerStyle.Font.Bold = true;
                    headerStyle.Font.FontSize = 14;
                    headerStyle.Font.FontColor = XLColor.Black;

                    // Apply border to branch range
                    headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;

                    // Print letterhead
                    worksheet.Cell(1, 2).Value = "HEAD OFFICE";
                    worksheet.Cell(2, 2).Value = "BRANCH LOCATION";
                    worksheet.Cell(3, 2).Value = "Capital";
                    worksheet.Cell(4, 2).Value = "ImmatriculationNumber";
                    worksheet.Cell(5, 2).Value = "WebSite";
                    worksheet.Cell(6, 2).Value = "BranchTelephone";
                    worksheet.Cell(7, 2).Value = "HeadOfficeTelePhone";

                    worksheet.Cell(1, 1).Value = "BranchName";
                    worksheet.Cell(2, 1).Value = $"Address";
                    worksheet.Cell(3, 1).Value = $"Capital";
                    worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                    worksheet.Cell(5, 1).Value = $"Website";
                    worksheet.Cell(6, 1).Value = $"Branch Telephone";
                    worksheet.Cell(7, 1).Value = $"Head Office Telephone";

                    // Apply header style
                    var headerRange = worksheet.Range(1, 1, 7, 1);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Font.FontSize = 12;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    var headerRange2 = worksheet.Range(1, 1, 7, 2);
                    headerRange2.Style.Font.FontSize = 12;
                    headerRange2.Style.Alignment.WrapText = true;
                    headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    // Print account details
                    var titleRange = worksheet.Range("B10:F10");
                    titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 14;
                    titleRange.Style.Font.FontColor = XLColor.Black;
                    titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    titleRange.Value = $"BAPCCUL CHART OF ACCOUNT";

                    // Print balance sheet header
                    worksheet.Cell(12, 1).Value = "OLD ACCOUNT NUMBER";
                    worksheet.Cell(12, 2).Value = "NEW ACCOUNT NUMBER";
                    worksheet.Cell(12, 3).Value = "ACCOUNT NAME";

                    var headerRange0 = worksheet.Range(12, 1, 12, 4);
                    headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange0.Style.Font.FontSize = 12;
                    headerRange0.Style.Font.Bold = true;
                    headerRange0.Style.Alignment.WrapText = true;
                    headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    var headerRange10 = worksheet.Range(13, 1, accounts.Count + 13, 8);
                    headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    int row = 13;
                    foreach (var account in accounts)
                    {
                        worksheet.Cell(row, 1).Value = account.Old_AccountNumber;
                        worksheet.Cell(row, 2).Value = account.New_AccountNumber;
                        worksheet.Cell(row, 3).Value = account.Description;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                    return DownloadExcelFileSlow(workbook, "CHARTOFACCOUNT.xlsx");
                }
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                // Optionally, return an error response or view
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message);
            }
        }

        public class ExcelFileResult : FileResult
        {
            private readonly XLWorkbook _workbook;
            private readonly string _fileName;

            public ExcelFileResult(XLWorkbook workbook, string fileName)
                : base("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                _workbook = workbook;
                _fileName = fileName;
            }

            protected override void WriteFile(HttpResponseBase response)
            {
                response.AddHeader("Content-Disposition", $"attachment; filename=\"{_fileName}\"");
                using (var memoryStream = new MemoryStream())
                {
                    _workbook.SaveAs(memoryStream);
                    memoryStream.WriteTo(response.OutputStream);
                }
            }
        }

        public ActionResult DownloadExcelFileSlow(XLWorkbook workbook, string fileName)
        {
            try
            {
                return new ExcelFileResult(workbook, fileName);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Content($"Error generating Excel file: {ex.Message}");
            }
        }

        public ActionResult PrintJournalEntryDtoInExcel()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();

            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<JournalEntryDto> accounts = (rptSource == "empty") ? new List<JournalEntryDto>() : (List<JournalEntryDto>)rptSource;
            JournalEntryDto trialBalance = (rptSource == "empty") ? new JournalEntryDto() : accounts[0];

            if ((rptSource == "empty"))
            {
                return new EmptyResult();
            }
            else
            {

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(rpttitle);
                    var headerStyle = workbook.Style;

                    headerStyle.Font.Bold = true;
                    headerStyle.Font.FontSize = 14;
                    headerStyle.Font.FontColor = XLColor.Black;
                    // Apply border to branch range
                    headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print letterhead
                    worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                    worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}";
                    worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                    worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                    worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                    worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                    worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                    worksheet.Cell(1, 1).Value = "BranchName";
                    worksheet.Cell(2, 1).Value = $"Address";
                    worksheet.Cell(3, 1).Value = $"Capital";
                    worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                    worksheet.Cell(5, 1).Value = $"Website";
                    worksheet.Cell(6, 1).Value = $"Branch Telephone";
                    worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                    // Apply header style

                    var headerRange = worksheet.Range(1, 1, 7, 1);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Font.FontSize = 12;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    var headerRange2 = worksheet.Range(1, 1, 7, 2);

                    headerRange2.Style.Font.FontSize = 12;

                    headerRange2.Style.Alignment.WrapText = true;
                    headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print account details
                    var titleRange = worksheet.Range("B10:F10");
                    titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 14;
                    titleRange.Style.Font.FontColor = XLColor.Black;
                    // Apply border to title range
                    titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    titleRange.Value = $" {trialBalance.BranchName} Journal Entries from {trialBalance.FromDate} to {trialBalance.ToDate}";
                    // Print balance sheet header

                    worksheet.Cell(12, 1).Value = "Entry Date";
                    worksheet.Cell(12, 2).Value = "Reference";
                    worksheet.Cell(12, 3).Value = "Account Number";
                    worksheet.Cell(12, 4).Value = "Description";
                    worksheet.Cell(12, 5).Value = "Debit Balance";
                    worksheet.Cell(12, 6).Value = "Credit Balance";



                    // Apply header style
                    var headerRange0 = worksheet.Range(12, 1, 12, 6);
                    headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange0.Style.Font.FontSize = 12;
                    headerRange0.Style.Font.Bold = true;
                    headerRange0.Style.Alignment.WrapText = true;
                    headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 6);
                    headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    int row = 13;
                    foreach (var account in accounts)
                    {
                        worksheet.Cell(row, 1).Value = account.EntryDate;
                        worksheet.Cell(row, 2).Value = account.Reference;
                        worksheet.Cell(row, 3).Value = account.AccountNumber;
                        worksheet.Cell(row, 4).Value = account.Description;
                        worksheet.Cell(row, 5).Value = ConvertToLong(account.Debit);
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 6).Value = ConvertToLong(account.Credit);
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";


                        row++;
                    }

                    worksheet.Columns().AdjustToContents();


                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "JE.xlsx");
                    }
                    //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                    // Save the workbook

                }
            }



            //return new EmptyResult();
        }
        public static long ConvertToLong(object value)
        {
            try
            {
                if (value == null)
                {
                    // Handle null value by returning 0 or a default value.
                    return 0;
                }

                // If the value is already a numeric type, convert directly.
                if (value is int || value is long || value is short || value is byte)
                {
                    return Convert.ToInt64(value);
                }

                if (value is decimal || value is double || value is float)
                {
                    // Round the value before converting to avoid truncation errors.
                    return Convert.ToInt64(Math.Round(Convert.ToDecimal(value)));
                }

                if (value is string)
                {
                    // Remove potential formatting characters like commas or currency symbols.
                    string cleanedValue = value.ToString().Replace(",", "").Replace("$", "").Trim();

                    if (decimal.TryParse(cleanedValue, out decimal parsedDecimal))
                    {
                        return Convert.ToInt64(Math.Round(parsedDecimal));
                    }
                    else
                    {
                        throw new FormatException("The string value cannot be parsed as a numeric value.");
                    }
                }

                // Attempt to convert any other object type if possible.
                if (value is IConvertible)
                {
                    return Convert.ToInt64(value);
                }

                // If none of the above conditions are met, throw an exception.
                throw new InvalidCastException("The provided value is not convertible to a long.");
            }
            catch (OverflowException)
            {
                // Handle values that are out of range for Int64.
                throw new OverflowException("The value is too large or too small to be converted to a long.");
            }
            catch (FormatException ex)
            {
                // Handle invalid formats.
                throw new FormatException($"Invalid format: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions.
                throw new Exception($"An error occurred during conversion: {ex.Message}");
            }
        }

        public ActionResult PrintTrialBalance4Column()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();


            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();

            List<TrialBalance4ColumnDto> accounts = (rptSource == "empty") ? new List<TrialBalance4ColumnDto>() : (List<TrialBalance4ColumnDto>)rptSource;
            TrialBalance4ColumnDto trialBalance = (rptSource == "empty") ? new TrialBalance4ColumnDto() : accounts[0];
            if ((rptSource == "empty"))
            {
                return new EmptyResult();
            }
            else
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(rpttitle);
                    var headerStyle = workbook.Style;

                    headerStyle.Font.Bold = true;
                    headerStyle.Font.FontSize = 14;
                    headerStyle.Font.FontColor = XLColor.Black;
                    // Apply border to branch range
                    headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print letterhead
                    worksheet.Cell(1, 2).Value = trialBalance.BranchName;
                    worksheet.Cell(2, 2).Value = $"{trialBalance.BranchLocation}, {trialBalance.BranchAddress}";
                    worksheet.Cell(3, 2).Value = $" {trialBalance.Capital}";
                    worksheet.Cell(4, 2).Value = $"{trialBalance.ImmatriculationNumber}";
                    worksheet.Cell(5, 2).Value = $"{trialBalance.WebSite}";
                    worksheet.Cell(6, 2).Value = $" {trialBalance.BranchTelephone}";
                    worksheet.Cell(7, 2).Value = $"{trialBalance.HeadOfficeTelePhone}";

                    worksheet.Cell(1, 1).Value = "BranchName";
                    worksheet.Cell(2, 1).Value = $"Address";
                    worksheet.Cell(3, 1).Value = $"Capital";
                    worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                    worksheet.Cell(5, 1).Value = $"Website";
                    worksheet.Cell(6, 1).Value = $"Branch Telephone";
                    worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                    // Apply header style

                    var headerRange = worksheet.Range(1, 1, 7, 1);
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Font.FontSize = 12;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.WrapText = true;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                    var headerRange2 = worksheet.Range(1, 1, 7, 2);

                    headerRange2.Style.Font.FontSize = 12;

                    headerRange2.Style.Alignment.WrapText = true;
                    headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    // Print account details
                    var titleRange = worksheet.Range("B10:G10");
                    titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 14;
                    titleRange.Style.Font.FontColor = XLColor.Black;
                    // Apply border to title range
                    titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    titleRange.Value = $"  Trial Balance as at {trialBalance.FromDate} to {trialBalance.ToDate}";
                    // Print balance sheet header
                    worksheet.Cell(12, 1).Value = "Account Number";
                    worksheet.Cell(12, 2).Value = "Account Name";
                    worksheet.Cell(12, 3).Value = "Beginning Balance";
                    worksheet.Cell(12, 4).Value = "Debit Balance";
                    worksheet.Cell(12, 5).Value = "Credit Balance";
                    worksheet.Cell(12, 6).Value = "Ending Balance";


                    // Apply header style
                    var headerRange0 = worksheet.Range(12, 1, 12, 8);
                    headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange0.Style.Font.FontSize = 12;
                    headerRange0.Style.Font.Bold = true;
                    headerRange0.Style.Alignment.WrapText = true;
                    headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 8);
                    headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    int row = 13;
                    foreach (var account in accounts)
                    {
                        worksheet.Cell(row, 1).Value = account.AccountNumber;
                        worksheet.Cell(row, 2).Value = account.AccountName;
                        worksheet.Cell(row, 3).Value = account.BeginningBalance;
                        worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 4).Value = account.DebitBalance;
                        worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 5).Value = account.CreditBalance;
                        worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                        worksheet.Cell(row, 6).Value = account.EndingBalance;
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    if ((rptSource == "empty"))
                    {

                    }
                    else
                    {
                        var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
                        footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        footerRange.Style.Font.FontSize = 12;
                        footerRange.Style.Font.Bold = true;
                        footerRange.Style.Alignment.WrapText = false;
                        footerRange.Style.Alignment.JustifyLastLine = false;
                        footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        // Print totals
                        worksheet.Cell(row, 2).Value = "Totals";
                        worksheet.Cell(row, 3).Value = trialBalance.totalBeginningBalance.ToString();
                        worksheet.Cell(row, 4).Value = trialBalance.totalDebitBalance.ToString();
                        worksheet.Cell(row, 5).Value = trialBalance.totalCreditBalance.ToString();
                        worksheet.Cell(row, 6).Value = trialBalance.totalEndingBalance.ToString();

                    }


                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _accountServices.GetBranchName() + "-" + trialBalance.BranchName + "JournalENtries.xlsx");
                    }
                    //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                    // Save the workbook

                }
            }


            //return new EmptyResult();
        }
        public ActionResult PrintTrialBalance6Column()
        {
            //new Dto();
            //List<TrialBalance4ColumnDto> accounts = new List<TrialBalance4ColumnDto>();


            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var paths = System.Web.HttpContext.Current.Session["rptpath"].ToString();
            var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            var rpttype = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            List<TrialBalance6ColumnDto> accounts = (rptSource == "empty") ? new List<TrialBalance6ColumnDto>() : (List<TrialBalance6ColumnDto>)rptSource;
            TrialBalance6ColumnDto trialBalance = (rptSource == "empty") ? new TrialBalance6ColumnDto() : accounts[0];
            if ((rptSource == "empty"))
            {
                return new EmptyResult();// Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (rpttype.ToLower() == "pdf")
                {

                }
                else
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add(rpttitle);
                        var headerStyle = workbook.Style;

                        headerStyle.Font.Bold = true;
                        headerStyle.Font.FontSize = 14;
                        headerStyle.Font.FontColor = XLColor.Black;
                        // Apply border to branch range
                        headerStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerStyle.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerStyle.Border.TopBorder = XLBorderStyleValues.Thin;
                        // Print letterhead
                        worksheet.Cell(1, 2).Value = trialBalance.branchName;
                        worksheet.Cell(2, 2).Value = $"{trialBalance.branchLocation}, {trialBalance.branchAddress}";
                        worksheet.Cell(3, 2).Value = $" {trialBalance.capital}";
                        worksheet.Cell(4, 2).Value = $"{trialBalance.immatriculationNumber}";
                        worksheet.Cell(5, 2).Value = $"{trialBalance.webSite}";
                        worksheet.Cell(6, 2).Value = $" {trialBalance.branchTelephone}";
                        worksheet.Cell(7, 2).Value = $"{trialBalance.headOfficeTelePhone}";

                        worksheet.Cell(1, 1).Value = "BranchName";
                        worksheet.Cell(2, 1).Value = $"Address";
                        worksheet.Cell(3, 1).Value = $"Capital";
                        worksheet.Cell(4, 1).Value = $"Immatriculation Number";
                        worksheet.Cell(5, 1).Value = $"Website";
                        worksheet.Cell(6, 1).Value = $"Branch Telephone";
                        worksheet.Cell(7, 1).Value = $"Head Office Telephone";
                        // Apply header style

                        var headerRange = worksheet.Range(1, 1, 7, 1);
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange.Style.Font.FontSize = 12;
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.WrapText = true;
                        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                        var headerRange2 = worksheet.Range(1, 1, 7, 2);

                        headerRange2.Style.Font.FontSize = 12;

                        headerRange2.Style.Alignment.WrapText = true;
                        headerRange2.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerRange2.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerRange2.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerRange2.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        // Print account details
                        var titleRange = worksheet.Range("B10:G10");
                        titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        titleRange.Style.Font.Bold = true;
                        titleRange.Style.Font.FontSize = 14;
                        titleRange.Style.Font.FontColor = XLColor.Black;
                        // Apply border to title range
                        titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        titleRange.Value = $"  Trial Balance as at {trialBalance.fromDate} to {trialBalance.toDate}";
                        // Print balance sheet header
                        worksheet.Cell(12, 1).Value = "Account Number";
                        worksheet.Cell(12, 2).Value = "Account Name";
                        worksheet.Cell(12, 3).Value = "Beginning Debit Balance";
                        worksheet.Cell(12, 4).Value = "Beginning Credit Balance";
                        worksheet.Cell(12, 5).Value = "Debit Balance";
                        worksheet.Cell(12, 6).Value = "Credit Balance";
                        worksheet.Cell(12, 7).Value = "Ending Debit Balance";
                        worksheet.Cell(12, 8).Value = "Ending Credit Balance";

                        // Apply header style
                        var headerRange0 = worksheet.Range(12, 1, 12, 8);
                        headerRange0.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        headerRange0.Style.Font.FontSize = 12;
                        headerRange0.Style.Font.Bold = true;
                        headerRange0.Style.Alignment.WrapText = true;
                        headerRange0.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerRange0.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerRange0.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerRange0.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        var headerRange10 = worksheet.Range(13, 1, accounts.Count() + 13, 8);
                        headerRange10.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        headerRange10.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        headerRange10.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        headerRange10.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        int row = 13;
                        foreach (var account in accounts)
                        {
                            worksheet.Cell(row, 1).Value = account.accountNumber;
                            worksheet.Cell(row, 2).Value = account.accountName;
                            worksheet.Cell(row, 3).Value = ConvertToLong(account.beginningDebitBalance);
                            worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 4).Value = ConvertToLong(account.beginningCreditBalance);
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 5).Value = ConvertToLong(account.debitBalance);
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 6).Value = ConvertToLong(account.creditBalance);
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 7).Value = ConvertToLong(account.endDebitBalance);
                            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 8).Value = ConvertToLong(account.endCreditBalance);
                            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                            row++;
                        }

                        // Autofit columns
                        worksheet.Columns().AdjustToContents();

                        if ((rptSource == "empty"))
                        {

                        }
                        else
                        {
                            var footerRange = worksheet.Range(accounts.Count() + 13, 2, accounts.Count() + 13, 8);
                            footerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                            footerRange.Style.Font.FontSize = 12;
                            footerRange.Style.Font.Bold = true;
                            footerRange.Style.Alignment.WrapText = false;
                            footerRange.Style.Alignment.JustifyLastLine = false;
                            footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            footerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            footerRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            footerRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            footerRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            //var balance = accounts.Where(x => x.accountName == "Total").First();
                            // Print totals
                            worksheet.Cell(row, 2).Value = "Totals";
                            worksheet.Cell(row, 3).Value = ConvertToLong(trialBalance.totalBeginningDebitBalance.ToString());
                            worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 4).Value = ConvertToLong(trialBalance.totalBeginningCreditBalance.ToString());
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 5).Value = ConvertToLong(trialBalance.totalDebitBalance.ToString());
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 6).Value = ConvertToLong(trialBalance.totalCreditBalance.ToString());
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 7).Value = ConvertToLong(trialBalance.totalEndDebitBalance.ToString());
                            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 8).Value = ConvertToLong(trialBalance.totalEndCreditBalance.ToString());
                            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0";
                            //worksheet.Cell(row, 2).Value = "Totals";
                            //worksheet.Cell(row, 3).Value = trialBalance.beginningDebitBalance.ToString();
                            //worksheet.Cell(row, 4).Value = trialBalance.beginningCreditBalance.ToString();
                            //worksheet.Cell(row, 5).Value = trialBalance.debitBalance.ToString();
                            //worksheet.Cell(row, 6).Value = trialBalance.creditBalance.ToString();
                            //worksheet.Cell(row, 7).Value = trialBalance.endDebitBalance.ToString();
                            //worksheet.Cell(row, 8).Value = trialBalance.endCreditBalance.ToString();
                        }


                        using (var stream = new MemoryStream())
                        {
                            workbook.SaveAs(stream);
                            stream.Position = 0;

                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BapCCUL-" + trialBalance.branchName + "TB6C.xlsx");
                        }
                        //worksheet.Cell(row + 1, 1).Value = $"Ending Balance Sign: {trialBalance.EndingBalanceSigne}";

                        // Save the workbook

                    }
                }


            }

            return new EmptyResult();
        }


        public ActionResult PrintGeneralLedgerOfAccount()
        {
            // Retrieve session values
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            var rptTitle = System.Web.HttpContext.Current.Session["rpttitle"]?.ToString() ?? "General Ledger";

            if (rptSource == null || rptSource.ToString() == "empty")
            {
                return new EmptyResult();
            }

            var accounts = rptSource as AccountingGeneralLedgerDetails;
            if (accounts == null)
            {
                return new EmptyResult();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(rptTitle);

                // Define reusable styles
                var headerStyle = workbook.Style;
                headerStyle.Font.Bold = true;
                headerStyle.Font.FontSize = 12;
                headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Print letterhead
                AddLetterhead(worksheet, accounts);

                // Process each ledger account
                int currentRow = 9; // Start data after letterhead
                foreach (var ledger in accounts.LedgerDetails)
                {
                    currentRow = AddLedgerHeader(worksheet, ledger, accounts, currentRow);
                    currentRow = AddLedgerEntries(worksheet, ledger, currentRow);
                }

                // Autofit columns for better presentation
                worksheet.Columns().AdjustToContents();

                // Save and return the Excel file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"GeneralLedger_{accounts.BranchName}.xlsx");
                }
            }
        }

        private void AddLetterhead(IXLWorksheet worksheet, AccountingGeneralLedgerDetails accounts)
        {
            var letterheadData = new (string Label, string Value)[]
            {
        ("Branch Name", accounts.BranchName),
        ("Address", $"{accounts.BranchLocation}, {accounts.BranchAddress}"),
        ("Capital", accounts.Capital),
        ("Immatriculation Number", accounts.ImmatriculationNumber),
        ("Website", accounts.WebSite),
        ("Branch Telephone", accounts.BranchTelephone),
        ("Head Office Telephone", accounts.HeadOfficeTelePhone)
            };

            for (int i = 0; i < letterheadData.Length; i++)
            {
                worksheet.Cell(i + 1, 1).Value = letterheadData[i].Label;
                worksheet.Cell(i + 1, 2).Value = letterheadData[i].Value;
            }

            var letterheadRange = worksheet.Range(1, 1, letterheadData.Length, 2);
            letterheadRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            letterheadRange.Style.Font.Bold = true;
            letterheadRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        private int AddLedgerHeader(IXLWorksheet worksheet, LedgerDetails ledger, AccountingGeneralLedgerDetails accounts, int startRow)
        {
            // Add ledger title
            var titleCell = worksheet.Cell(startRow, 1);
            titleCell.Value = $"Entries of {ledger.AccountNumber} - {ledger.AccountName} ({accounts.FromDate:dd-MMM-yyyy} to {accounts.ToDate:dd-MMM-yyyy})";
            var titleRange = worksheet.Range(startRow, 1, startRow, 5);
            titleRange.Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            //Account Header 
            worksheet.Cell(startRow + 2, 1).Value = "Account Number:";
            worksheet.Cell(startRow + 2, 2).Value = ledger.AccountNumber;
            worksheet.Cell(startRow + 3, 1).Value = "Account Name:";
            worksheet.Cell(startRow + 3, 2).Value = ledger.AccountName;
            var letterheadRange = worksheet.Range(startRow + 2, 1, startRow + 3, 2);
            letterheadRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            letterheadRange.Style.Font.Bold = true;
            letterheadRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            return startRow + 5; // Move to the next section
        }

        private int AddLedgerEntries(IXLWorksheet worksheet, LedgerDetails ledger, int startRow)
        {
            // Add column headers
            var headers = new[]
            {
        "Entry DateTime", "Reference", "Account Number","Description",

        "Debit", "Credit ", "Balance"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(startRow, i + 1).Value = headers[i];
            }

            var headerRange = worksheet.Range(startRow, 1, startRow, headers.Length);
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Populate ledger entries
            int currentRow = startRow + 1;
            foreach (var entry in ledger.AccountingEntries)
            {
                worksheet.Cell(currentRow, 1).Value = entry.EntryDateTime;
                worksheet.Cell(currentRow, 2).Value = entry.ReferenceID;
                worksheet.Cell(currentRow, 3).Value = entry.AccountNumber;
                worksheet.Cell(currentRow, 4).Value = entry.Description;
                worksheet.Cell(currentRow, 5).Value = ConvertToLong(entry.DrAmount);
                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(currentRow, 6).Value = ConvertToLong(entry.CrAmount);
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                worksheet.Cell(currentRow, 7).Value = ConvertToLong(entry.CurrentBalance);
                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                currentRow++;
            }

            // Add totals
            worksheet.Cell(currentRow, 4).Value = "Totals";
            worksheet.Cell(currentRow, 5).Value = ConvertToLong(ledger.AccountingEntries.Sum(e => Convert.ToDouble(e.DrAmount)));
            worksheet.Cell(currentRow, 6).Value = ConvertToLong(ledger.AccountingEntries.Sum(e => Convert.ToDouble(e.CrAmount)));
            //worksheet.Cell(currentRow, 6).Value = ConvertToLong(ledger.AccountingEntries[currentRow-1]);
            var totalsRange = worksheet.Range(currentRow, 5, currentRow, 7);
            totalsRange.Style.Font.Bold = true;
            totalsRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            return currentRow + 2; // Leave a gap before the next ledger
        }


    }
    public class ExcelResult : ActionResult
    {
        private readonly XLWorkbook _workbook;
        private readonly string _fileName;

        public ExcelResult(XLWorkbook workbook, string fileName)
        {
            _workbook = workbook;
            _fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            try
            {
                response.Buffer = true;
                response.Clear();
                response.ClearContent();
                response.ClearHeaders();
                response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                response.AddHeader("content-disposition", $"attachment;filename={_fileName}");

                using (var memoryStream = new MemoryStream())
                {
                    _workbook.SaveAs(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(response.OutputStream);
                }

                response.Flush();
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new HttpException("Error generating Excel file: " + ex.Message);
            }
        }
    }


}

public class Export
{
    public void ToExcel(HttpResponseBase response, IEnumerable<object> object_list, string strtitle)
    {
        try
        {
            var grid = new System.Web.UI.WebControls.GridView();

            // Ensure type safety
            var nonNullClientsList = object_list?.Where(item => item != null) ?? Enumerable.Empty<object>();

            grid.DataSource = nonNullClientsList;
            grid.DataBind();

            response.ClearContent();
            response.AddHeader("content-disposition", $"attachment; filename={strtitle}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xls");
            response.ContentType = "application/excel";

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    grid.RenderControl(htw);
                    response.Write(sw.ToString());
                    response.End();
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Error exporting to Excel: {ex.Message}");
        }
    }
}


