using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.Report;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.AppFiles.Accountingv2Reporting.DataSets;
using CrystalDecisions.Web;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ZXing.Common;

namespace CBS.FrontDesk.UI.Controllers.CheckManagementSystem.Operations.ChequeBookListing
{
    public class ChequeBookReportController : BaseController
    {
        private readonly ChequeBookReportService _chequeBookReportService;
        private readonly BranchServices _branchServices;

        public ChequeBookReportController(BranchServices branchServices, ChequeBookReportService chequeBookReportService )
        {
            _chequeBookReportService = chequeBookReportService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
           await LoadViewBagData();
            return View();
        }

        private async Task LoadViewBagData()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
          
        }


        /// <summary>
        /// Generates Cheque Book report dataset based on user-selected filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateChequeBookReport(ChequeBookQuery query)
        {
            try
            {
                // ✅ Get FLAT LIST (same pattern as Trial Balance)
                var reportData =
                    await _chequeBookReportService.BuildChequeBookReportAsync(query);

                if (reportData == null || !reportData.Any())
                {
                    Session["rptSource"] = null;
                    return Json(new
                    {
                        success = false,
                        message = "No data found for the selected criteria."
                    });
                }

                // ✅ Store list directly for Crystal
                Session["rptSource"] = reportData;
                Session["ChequeBookQuery"] = query;
                Session["ChequeBookCount"] = reportData.Count;

                return Json(new
                {
                    success = true,
                    count = reportData.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// Setup Crystal Report parameters for the Cheque Book report.
        /// </summary>
        [HttpPost]
        public ActionResult PrepareReport()
        {
            try
            {
                Session["rptType"] = "ReportParameterLess";
                Session["ReportName"] = "ChequeBook.rpt";
                Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/ChequeBook.rpt";
                Session["rpttitle"] = "ChequeBookRegistry";

                

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult DebugChequeBookData()
        {
            try
            {
                if (Session["rptSource"] == null)
                {
                    return Content("Session[rptSource] is null");
                }

                var data = Session["rptSource"];
                string result = $"Data Type: {data.GetType().FullName}<br>";

                if (data is System.Data.DataSet ds)
                {
                    result += $"DataSet Name: {ds.DataSetName}<br>";
                    result += $"Tables Count: {ds.Tables.Count}<br><br>";

                    foreach (DataTable table in ds.Tables)
                    {
                        result += $"Table: {table.TableName} (Rows: {table.Rows.Count})<br>";
                        result += "Columns: " + string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) + "<br>";

                        if (table.Rows.Count > 0)
                        {
                            result += "First row values: ";
                            for (int i = 0; i < Math.Min(table.Columns.Count, 5); i++)
                            {
                                result += $"{table.Columns[i].ColumnName}: {table.Rows[0][i]}, ";
                            }
                        }
                        result += "<br><br>";
                    }
                }
                else if (data is List<ChequeBookReportRow> list)
                {
                    result += $"List Count: {list.Count}<br>";
                    if (list.Count > 0)
                    {
                        var firstItem = list[0];
                        result += $"First item: {firstItem.CustomerName}, {firstItem.AccountNumber}<br>";
                        result += $"Properties: {string.Join(", ", firstItem.GetType().GetProperties().Select(p => p.Name))}";
                    }
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}<br>{ex.StackTrace}");
            }
        }

    }


}

