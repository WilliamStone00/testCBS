using CBS.BusinessService.AccountingDayObject;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using ClosedXML.Excel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
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
                BranchName = b.Name        // Assuming 'Name' is the branch name in your source data
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



        //public async Task<ActionResult> GetReport(DailyTeller request)
        //{

        //}
        //public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string dateFrom = null, string dateTo = null)
        //{
        //    if (path == "search")
        //    {
        //        if (KEY != string.Empty)
        //        {
        //            var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo), KEY);
        //            return PartialView(partialView, data.ToList());

        //        }
        //        else
        //        {
        //            var data = await _services.GetDailyTellers(_services.GetDateTime(dateFrom), _services.GetDateTime(dateTo));
        //            return PartialView(partialView, data.ToList());

        //        }
        //    }

        //    else if (path == "new")
        //    {
        //        await LoadDropdowns();
        //        return PartialView(partialView, new DailyTeller());
        //    }
        //    else
        //    {
        //        ViewBag.Key = KEY;
        //        await LoadDropdowns();
        //        var dailyTeller = await _services.GetDailyTeller(KEY);
        //        dailyTeller.UserId = $"{dailyTeller.UserId}@{dailyTeller.UserName}";
        //        return PartialView(partialView, dailyTeller);

        //    }


        //}




    }

}