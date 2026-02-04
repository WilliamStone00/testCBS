using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2;
using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Web;

using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.EndOfYearClosure
{
    public class EndOfYearClosureController : Controller
    {
        private static readonly Random _random = new Random();

        private readonly BranchServices _branchServices;
        private readonly EndOfYearClosureService _endOfYearClosureService;
        private readonly BranchAccountService _branchAccountService;



        public EndOfYearClosureController(BranchServices branchServices, EndOfYearClosureService endOfYearClosureService, BranchAccountService branchAccountService)
        {

            _branchServices = branchServices;
            _endOfYearClosureService = endOfYearClosureService;
            _branchAccountService = branchAccountService;


        }
        // GET: EndOfYearClosure
        public async Task<ActionResult> Index()
        {
           
            await loader();
            
            return View(new EndOfYear());
        }
        public async Task<ActionResult> List()
        {
            await loader();
            return View(new EndOfYear());
        }

        public async Task<bool> loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;


            var CounterBranches = await _branchServices.GetBranches();
            ViewBag.CounterBranches = CounterBranches;

            ViewBag.AdjustmentType = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Depreciation", Text = "Depreciation" },
                    new SelectListItem { Value = "Provision(Bad Debts)", Text = "Provision(Bad Debts)" },
                    new SelectListItem { Value = "Accrual", Text = "Accrual" },
                    new SelectListItem { Value = "Deferral", Text = "Deferral" },
                    new SelectListItem { Value = "Tax Provision", Text ="Tax Provision" },
                    new SelectListItem { Value = "Error Correction", Text = "Error Correction" },
                    new SelectListItem { Value = "Other", Text = "Other" }
                };


            return true;
        }
        [HttpGet]
        public async Task<ActionResult> GetClosureOpenYearByBranchId(string branchId, string init)
        {
            try
            {
                
                var years = await _endOfYearClosureService.GetAccountingYearByBranchIdAsync(branchId,init);

                var firstYear = years.FirstOrDefault();
                if (firstYear != null)
                {
                    Session["SelectedAccountingYearId"] = firstYear.Id;
                }

                var result = years.Select(x => new
                {
                    id = x.Id,
                    year = x.Year
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }



        [HttpGet]
        public async Task<ActionResult> GetYearClosureStatus(string branchId, string accountingYearId)
        {
            var response = await _endOfYearClosureService
                .GetYearClosureStatusAsync(branchId, accountingYearId);

            return Json(new
            {
                success = response.IsSuccess,
                message = response.Message,
                data = response.ApiResponseData
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<ActionResult> LoadWorkflowByStatus(string branchId, string accountingYearId)
        {
            if (string.IsNullOrWhiteSpace(branchId) || string.IsNullOrWhiteSpace(accountingYearId))
            {
                return new HttpStatusCodeResult(400, "BranchId and AccountingYearId are required.");
            }

            var response = await _endOfYearClosureService
                .GetYearClosureStatusAsync(branchId, accountingYearId);

            if (response == null || !response.IsSuccess || response.ApiResponseData == null)
            {
                return PartialView("_Initiation"); // safe default
            }

            var step = response.ApiResponseData.Step;

            switch (step)
            {
                case 0: // NotStarted
                case 1: // Initiated
                    return PartialView("_Initiation");

                case 2: // Adjustment
                    await loader();
                    return PartialView("_Adjustment");

                case 3: // Closing
                    await loader();
                    return PartialView("_ClosingForm");

                case 4: // Review
                    return PartialView("_Review");

                default:
                    return PartialView("_Initiation");
            }
        }



        [HttpPost]
        public async Task<ActionResult> InitiateClosure(CloseYearInitiate model)
        {
            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                model.AccountingYearId = Session["SelectedAccountingYearId"]?.ToString();
            }

            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                return Json(new
                {
                    success = false,
                    message = "Accounting Year is missing. Please select a branch and try again."
                });
            }

            try
            {
                var response = await _endOfYearClosureService.SaveInitiateClosure(model);

                // 🔴 CRITICAL: null safety
                if (response == null)
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = 502,
                        message = "No response from Close Of Year service."
                    });
                }

                return Json(new
                {
                    success = response.IsSuccess,
                    statusCode = response.IsSuccess ? 200 : 400,
                    message = response.Message,
                    data = response.ApiResponseData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Close of Year failed: {ex.Message}"
                });
            }
        }
        //[HttpPost]
        //public async Task<ActionResult> InitiateClosure(CloseYearInitiate model)
        //{
        //    if (string.IsNullOrEmpty(model.AccountingYearId))
        //    {
        //        model.AccountingYearId = Session["SelectedAccountingYearId"]?.ToString();
        //    }
        //    var response = await _endOfYearClosureService.SaveInitiateClosure(model);
        //    // 🔧 TEST MODE: ignore service, force success
        //    return Json(new3

        //    {
        //        success = true,
        //        statusCode = 200,
        //        message = "✅ [TEST MODE] Closure initiated successfully.",
        //        reloadDataView = "no",
        //        accountingYearId = model.AccountingYearId
        //    });
        //}


        [HttpPost]
        public async Task<ActionResult> ReviewClosure( ReviewClosureRequest model)
        {
            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                model.AccountingYearId = Session["SelectedAccountingYearId"]?.ToString();
            }

            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                return Json(new
                {
                    success = false,
                    message = "Accounting Year is missing. Please select a branch and try again."
                });
            }

            try
            {
                var result = await _endOfYearClosureService.SaveReviewClosure(model);

                if (result == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No response from Review Closure service."
                    });
                }

                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Review of year closure failed: {ex.Message}"
                });
            }
        }





        [HttpGet]
        public async Task<ActionResult> GetAllEndTask()
        {
            try
            {
                var entries = await _endOfYearClosureService.GetAllEndTaskAsync();

                if (entries == null || !entries.Any())
                    return HttpNotFound("Not found");

                var model = new EndOfYearTaskViewModel
                {
                    Tasks = entries
                };

                return PartialView("_Review", model);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        [HttpPost]
        public async Task<JsonResult> LoadYearEndChecklistStatus(YearEndChecklistStatusQuery query)
        {
            try
            {
                var data = await _endOfYearClosureService.AdjustmentEntriesDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var EndChecklist = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.EndOfYearClosure.EndOfYear>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = EndChecklist,
                    success = true,
                    message = "Display DataTable for End of Year check  status   successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }



        }

        public string GenerateReference()
        {
            // Use _random instance to prevent duplicate numbers
            return $"REF-{DateTime.Now:yyyyMMddHHmmss}-{_random.Next(100, 999)}";
        }


        [HttpPost]
        public async Task<ActionResult> PostAdjustment(EndOfYear model)
        {


            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                model.AccountingYearId = Session["SelectedAccountingYearId"]?.ToString();
            }

            if (string.IsNullOrEmpty(model.AccountingYearId))
            {
                return Json(new
                {
                    success = false,
                    message = "Accounting Year is missing. Please select a branch and try again."
                });
            }
            try
            {
            

                // ✅ Validate payload
                if (model == null)
                    return Json(new { success = false, message = "Invalid payload received." });

                if (model.Payload == null || model.Payload.Entries == null || !model.Payload.Entries.Any())
                    return Json(new { success = false, message = "No journal lines provided." });

                // ✅ Ensure Debit = Credit
                var totalDr = model.Payload.Entries.Where(e => e.Dr).Sum(e => e.Amount);
                var totalCr = model.Payload.Entries.Where(e => e.Cr).Sum(e => e.Amount);
                if (totalDr != totalCr)
                    return Json(new { success = false, message = "Debit and Credit totals must balance." });

                // ✅ Add metadata
                model.PostMode = "HOLD_FOR_APPROVAL";
                
                model.CreatedDate = DateTime.Now;
                model.State = "INITIATED";
                model.Reference = GenerateReference();



                // ✅ Call service
                var execMessage = await _endOfYearClosureService.PostAdjustmentAsync(model);

                // ✅ Handle response (pattern same as CreateOrUpdate)
                if (execMessage == null)
                    return Json(new { success = false, message = "No response from service." });

                if (!execMessage.Result)
                    return Json(new
                    {
                        success = false,
                        message = execMessage.MessageString ?? "Failed to post journal entry.",
                        data = execMessage.Data
                    });

                return Json(new
                {
                    success = true,
                    message = execMessage.MessageString ?? "Journal entry posted successfully.",
                    data = execMessage.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }



        public async Task<ActionResult> GetAccounts(string branchId, bool isHeadOffice = false)
        {
            try
            {
                // Resolve branch
                if (isHeadOffice)
                {
                    branchId = _branchAccountService.GetHeadOfficeBranchID();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(branchId))
                        return Json(new
                        {
                            success = false,
                            message = "⚠️ Please select a branch."
                        }, JsonRequestBehavior.AllowGet);
                }

                var branchAccounts =
                    await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                var result = _branchAccountService.DropDownGen(branchAccounts.ToList());

                if (result == null || !result.Any())
                    return Json(new
                    {
                        success = false,
                        message = "⚠️ No accounts found for this branch."
                    }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = result
                }, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return Json(new
                {
                    success = false,
                    message = "⚠️ Timeout while fetching accounts — backend service not responding."
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> CloseYear(CloseOfYear model)
        {
            if (string.IsNullOrEmpty(model.AccountingYear))
            {
                model.AccountingYear = Session["SelectedAccountingYearId"]?.ToString();
            }

            if (string.IsNullOrEmpty(model.AccountingYear))
            {
                return Json(new
                {
                    success = false,
                    message = "Accounting Year is missing. Please select a branch and try again."
                });
            }

            try
            {
                var response = await _endOfYearClosureService.SaveClosure(model);

                // 🔴 CRITICAL: null safety
                if (response == null)
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = 502,
                        message = "No response from Close Of Year service."
                    });
                }

                return Json(new
                {
                    success = response.IsSuccess,
                    statusCode = response.IsSuccess ? 200 : 400,
                    message = response.Message,
                    data = response.ApiResponseData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Review of year closure failed: {ex.Message}"
                });
            }
        }




    }
}