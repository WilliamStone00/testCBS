using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.CashReconciliation;
using CBS.BusinessService.AccountingV2.ConfigurationsManualEntry;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.AccountingYear
{
    public class AccountingYearController : Controller
    {

        public readonly AccountingYearService _accountingYearService;
        private readonly BranchServices _branchServices;
        public AccountingYearController(BranchServices branchServices, AccountingYearService accountingYearService)
        {
            _accountingYearService = accountingYearService;
            _branchServices = branchServices;

        }
        // GET: AccountingYear
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(); //**//*
        }

        private List<StringValues> getAllYearStatus()
        {
            return new List<StringValues>
            {
                new StringValues
                {
                    Value = "open", Text = "OPEN"
                },
                new StringValues
                {
                    Value = "close", Text = "CLOSE"
                },
                new StringValues
                {
                    Value = "lock", Text = "LOCK"
                },
            };
        }

        private List<StringValues> getNext5Years()
        {
            return Enumerable.Range(DateTime.Now.Year, 5)
                .Select(y => new StringValues
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
            ViewBag.Status = getAllYearStatus();
            ViewBag.Year = getNext5Years();
            return true;

        }




        [HttpGet]
        public async Task<ActionResult> New()
        {
            await loader();
            return PartialView("_Update", new accountingyear());
        }


        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var entries = await _accountingYearService.GetAllAsync();
                //var entries = await _accountingYearService.GetAllAsyncs();
                if (entries == null || !entries.Any())
                    return HttpNotFound("Not found");

                // Return a view or partial view depending on your page setup
                return PartialView("_Datatable", entries);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }

        }

        [HttpGet]
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "ID is required");

            await loader();
            try
            {
                var entry = await _accountingYearService.GetData(id);
                if (entry == null)
                    return HttpNotFound("accounting year not found");

                entry.StatusB = entry.Status.ToLower();

                entry.YearB = entry.Year.ToString();
                // Return the partial view that will be injected into the modal
                return PartialView("_Update", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        
        public async Task<ActionResult> CreateOrUpdate(accountingyear model)
        {
            model.Status = model.StatusB;
            model.Year = int.Parse(model.YearB);
            if (model == null)
                return Json(new { success = false, message = "Invalid or empty model." });

            try
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    // No Id → create new record
                    var execMessage = await _accountingYearService.Create(model);

                    if (execMessage == null)
                        return Json(new { success = false, message = "No response from service." });

                    if (!execMessage.Result)
                        return Json(new
                        {
                            success = false,
                            message = execMessage.MessageString ?? "Failed to save Accounting Year.",
                            data = execMessage.Data
                        });

                    return Json(new
                    {
                        success = true,
                        message = execMessage.MessageString ?? "Accounting Year created successfully.",
                        data = execMessage.Data
                    });
                }
                else
                {
                    // Id present → update existing record
                    var result = await _accountingYearService.UpdateAccoutingYearAsync(model);

                    if (result == null)
                        return Json(new { success = false, message = "No response from service." });

                    return Json(new
                    {
                        success = result.Result,
                        message = Messaging.MessageResult(result),
                        data = result.Data
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }



    }
}