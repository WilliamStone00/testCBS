using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2
{


    public class CustomeReportsController : BaseController
    {
        public CustomeReportsController()
        {
        }

        public async Task<ActionResult> Index()
        {
            ReportTypes();
            return View();
        }


        public void  ReportTypes()
        {
            ViewBag.reportTypes = new List<SelectListItem>() {
            new SelectListItem { Value = "Trial Balance 6 Columns", Text = "Trial Balance 6 Columns" },
            new SelectListItem { Value = "Trial Balance 4 Columns", Text = "Trial Balance 4 Columns" },
            new SelectListItem { Value = "General Ledger", Text = "General Ledger" },
            new SelectListItem { Value = "Balance Sheet", Text = "Balance Sheet" },
            new SelectListItem { Value = "Income Statement", Text = "Income Statement" },
            new SelectListItem { Value = "Cash Flow Statement", Text = "Cash Flow Statement" },
            new SelectListItem { Value = "Detailed Trial Balance", Text = "Detailed Trial Balance" },
            new SelectListItem { Value = "Account Statement", Text = "Account Statement" },
            new SelectListItem { Value = "Journal Listing", Text = "Journal Listing" },
            new SelectListItem { Value = "Chart of Accounts", Text = "Chart of Accounts" },
            new SelectListItem { Value = "Subsidiary Ledger", Text = "Subsidiary Ledger" },
            new SelectListItem { Value = "Consolidated Trial Balance", Text = "Consolidated Trial Balance" },
            new SelectListItem { Value = "Branch Trial Balance", Text = "Branch Trial Balance" },
            new SelectListItem { Value = "Transaction Listing", Text = "Transaction Listing" },
            };

        }




    }
}
