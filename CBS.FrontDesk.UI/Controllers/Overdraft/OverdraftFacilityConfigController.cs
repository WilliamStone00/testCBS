using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Overdraft;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP;
using CBS.FrontDesk.Data.Entity.Overdraft;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.Overdraft
{
    //[CheckSessionTimeOutAttribute]

    public class OverdraftFacilityConfigController : BaseController
    {
        private readonly OverdraftFacilityConfigServices _overdraftFacilityConfigServices;
        private readonly BranchServices _branchServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly PenaltyServices _penaltyServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;

        public OverdraftFacilityConfigController(OverdraftFacilityConfigServices cashDeskService = null, ChartOfAccountServicesAnnex chartOfAccountServices = null, BranchServices memberNoneCashOperationServices = null, LoanProductServices loanProductServices = null, PenaltyServices penaltyServices = null)
        {
            _overdraftFacilityConfigServices = cashDeskService;
            this.chartOfAccountServices=chartOfAccountServices;
            _branchServices=memberNoneCashOperationServices;
            _loanProductServices=loanProductServices;
            _penaltyServices=penaltyServices;
        }
        public async Task<ActionResult> Index()
        {
            await LoadDropdowns();
            return View();
        }
        public ActionResult Transfters()
        {
            return View();
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "list")
                {
                    var account = await _overdraftFacilityConfigServices.GetAll();
                    return PartialView(partialView, account);
                }
                else
                {
                    await LoadDropdowns();
                    if (KEY==null)
                    {
                        return PartialView(partialView, new OverdraftFacilityConfig());

                    }
                    else
                    {
                        var overdraftFacilityConfig = await _overdraftFacilityConfigServices.Get(KEY);
                        return PartialView(partialView, overdraftFacilityConfig);

                    }
                }

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetConfig(string id)
        {
            await LoadDropdowns();

            if (id!=null && id!="")
            {
                var overdraftFacilityConfig = await _overdraftFacilityConfigServices.Get(id);
                if (overdraftFacilityConfig == null)
                {
                    return Content("<div class='text-danger p-3'>No data found.</div>");
                }

                return PartialView("_OverdraftFacilityConfigForm", overdraftFacilityConfig);
            }
            else
            {
                return PartialView("_OverdraftFacilityConfigForm", new OverdraftFacilityConfig());
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _overdraftFacilityConfigServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }


        public async Task<bool> LoadDropdowns()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            var Products = await _loanProductServices.GetStringValuesAsync();
            ViewBag.Products = Products.ToList();
            var branches = await _branchServices.GetBranches();
            ViewBag.branches = branches.ToList();
            ViewBag.Penalties = (await _penaltyServices.GetStringValuesAsync()).ToList();
            
            return true;
        }
    }
}