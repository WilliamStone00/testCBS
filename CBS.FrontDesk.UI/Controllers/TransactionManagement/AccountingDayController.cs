using CBS.BusinessService.AccountingDayObject;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{

    [CheckSessionTimeOutAttribute]
    public class AccountingDayController : BaseController
    {
        // GET: AccountingDay
        private readonly AccountingDayServices _services;
        private readonly BranchServices _branchServices;
        public AccountingDayController(AccountingDayServices services, BranchServices branchServices = null)
        {
            _services = services;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            // Fetch the list of branches from the service
            var branches = await _branchServices.GetBranches();

            // Map the branches to the BranchListing class
            var branchListings = branches.Select(b => new BranchListing
            {
                BranchId = b.Id,           // Assuming 'Id' is the branch identifier in your source data
                BranchCode = b.BranchCode,       // Assuming 'Code' is the branch code in your source data
                BranchName = b.Name        // Assuming 'Name' is the branch Name in your source data
            }).ToList();

            // Create an instance of the OpenOrCloseOfAccountingDayCommand with the mapped branches
            var model = new OpenOrCloseOfAccountingDayCommand
            {
                Date = DateTime.Today,    // Set the default date
                Branches = branchListings, // Set the branches
                IsCentraliseOpening = false, // Set default to false, can be overridden by user
                OpenOrCloseAccountingDay = "Open" // Default to "Open", can be overridden by user
            };

            // Pass the model to the view
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> OpenOrClose(OpenOrCloseOfAccountingDayCommand model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, status = "ValidationError", message = string.Join(" ", errors) });
            }

            try
            {
                if (model.OpenOrCloseAccountingDay == "Open")
                {
                    var data = await _services.OpenOfAccountingDay(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else if (model.OpenOrCloseAccountingDay == "Close")
                {
                    var data = await _services.CloseOfAccountingDay(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else
                {
                    return Json(new { success = false, status = "InvalidAction", message = "Invalid action specified. It must be 'Open' or 'Close'." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error", message = $"An error occurred: {ex.Message}" });
            }
        }
        public async Task<ActionResult> Status()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> CloseAccountingDay(string id)
        {
            try
            {
                var data = await _services.AccountingDayActions(id, "Close");
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error closing accounting day: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ReopenAccountingDay(string id)
        {
            try
            {
                var data = await _services.AccountingDayActions(id, "Reopened");
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error reopening accounting day: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RemoveAccountingDay(string id)
        {
            try
            {
                var data = await _services.AccountingDayActions(id, "Remove");
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing accounting day: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrentAccountingDay()
        {
            try
            {
                const string SessionKeyDate = "CurrentAccountingDate";
                const string SessionKeyExpiry = "CurrentAccountingDateExpiry";

                // Check if the accounting date is already in session and not expired
                var sessionDate = HttpContext.Session[SessionKeyDate] as string;
                var expiryObj = HttpContext.Session[SessionKeyExpiry] as DateTime?;

                if (!string.IsNullOrEmpty(sessionDate) && expiryObj.HasValue && expiryObj.Value > DateTime.Now)
                {
                    return Json(new
                    {
                        success = true,
                        status = "OK",
                        data = sessionDate
                    }, JsonRequestBehavior.AllowGet);
                }

                // If not found or expired, fetch new value from service
                var date = await _services.GetCurrentAccountingDate();
                var formattedDate = date.ToString("dd-MM-yyyy HH:mm:ss");

                // Store in session with expiry 1 hour from now
                HttpContext.Session[SessionKeyDate] = formattedDate;
                HttpContext.Session[SessionKeyExpiry] = DateTime.Now.AddHours(1);

                return Json(new
                {
                    success = true,
                    status = "OK",
                    data = formattedDate
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error retrieving accounting day: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



        public async Task<ActionResult> GetAccountingDayDetails(string id)
        {
            var accountingDay = await _services.GetAccountingDay(id); // Fetch the accounting day by ID
            var accountingDayQuery = new GetAccountingDayQuery { AccountingDay = accountingDay };
            return PartialView("_AccountingDayDetails", accountingDayQuery);
        }

        [HttpPost]
        public async Task<JsonResult> GetAccountingDays(string queryParameter, string branchId, string status, DateTime? dateFrom, DateTime? dateTo)
        {
            try
            {
                // Create the query object
                var getAccounting = new GetAccountingDayQuery
                {
                    BranchId = branchId,
                    DateFrom = dateFrom ?? DateTime.MinValue,
                    DateTo = dateTo ?? DateTime.MaxValue,
                    QueryParameter = queryParameter
                };

                // Validate the query object
                var validationContext = new ValidationContext(getAccounting);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(getAccounting, validationContext, validationResults, true);

                if (!isValid)
                {
                    var errors = validationResults.Select(v => v.ErrorMessage).ToList();
                    return Json(new { success = false, status = "ValidationError", message = string.Join(" ", errors) });
                }

                // Fetch the accounting days based on the parameters
                var accountingDays = await _services.GetAccountingDayQueries(getAccounting);
                return Json(new { data = accountingDays.ToList() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        [HttpPost]
        public async Task<ActionResult> DownloadAccountingDayStatus(GetAccountingDayQuery request)
        {
            // Clear ModelState errors for properties you don't want to validate
            ModelState.Clear();

            // Manually add the validation errors for `GetTillStatusQuery`
            TryValidateModel(request, nameof(request));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();

                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }

           

            // Call your service to get the data
            var response = await _services.GetAccountingDayQueries(request);

            var data = await _services.GetAccountingDayDsAsync(response.ToList()); // Convert to list if needed

            // Check if data is available
            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected query criteria." });

            }
            // If data is available, proceed with setting session variables and redirecting to the report
            this.HttpContext.Session["rptSource"] = data;
            this.HttpContext.Session["param_size"] = "6";
            this.HttpContext.Session["DateFrom"] = request.DateFrom;
            this.HttpContext.Session["DateTo"] = request.DateTo;
            this.HttpContext.Session["PrintedBy"] = Session["FullName"].ToString();
            this.HttpContext.Session["rptType"] = "ReportWithParameter";
            this.HttpContext.Session["ReportName"] = "AccountingDayrpt.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/AccountingDay/AccountingDayrpt.rpt";
            this.HttpContext.Session["rpttitle"] = $"AccountingDayDetails";
            // Return the view that opens the report in a new window
            return Json(new { success = true, message = "Success." });
        }

    }

}