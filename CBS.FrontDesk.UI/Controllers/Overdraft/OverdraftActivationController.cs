using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.Overdraft;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Overdraft;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.Overdraft
{
    //[CheckSessionTimeOutAttribute]

    public class OverdraftActivationController : BaseController
    {
        private readonly OverdraftActivationServices _overdraftActivationServices;
        private readonly OverdraftFacilityConfigServices _overdraftFacilityConfigServices;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly PenaltyServices _penaltyServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;

        public OverdraftActivationController(OverdraftActivationServices cashDeskService = null, ChartOfAccountServicesAnnex chartOfAccountServices = null, BranchServices memberNoneCashOperationServices = null, LoanProductServices loanProductServices = null, PenaltyServices penaltyServices = null, IndividualProfileServices individualProfileServices = null, OverdraftFacilityConfigServices overdraftFacilityConfigServices = null)
        {
            _overdraftActivationServices = cashDeskService;
            this.chartOfAccountServices=chartOfAccountServices;
            _branchServices=memberNoneCashOperationServices;
            _loanProductServices=loanProductServices;
            _penaltyServices=penaltyServices;
            _individualProfileServices=individualProfileServices;
            _overdraftFacilityConfigServices=overdraftFacilityConfigServices;
        }
        public async Task<ActionResult> Index()
        {
            await LoadDropdowns();
            return View();
        }
       
        [HttpGet]
        public async Task<ActionResult> GetCustomerAccountsAndDetails(string customerId)
        {
            // Example dummy logic
            var customer = await _individualProfileServices.GetCustomerLight(customerId);
            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });


            var accounts = (await _individualProfileServices.GetCustomerAccounts(customerId)).Select(a => new { id = a.id, accountNumber = a.accountNumber, productName = a.product.Name }).ToList();

            return Json(new
            {
                success = true,
                customerName = customer.CustomerList.name,
                branchId = customer.CustomerList.BranchId,
                branchCode = customer.CustomerList.branchCode,
                branchName = customer.CustomerList.branch,
                accounts
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> LoadData(GetOverdraftActivationRequestsDataTableQuery query)
        {
            try
            {
                var dataTable = await _overdraftActivationServices.GetDataTableAsync(query);
                var memberList = JsonConvert.DeserializeObject<List<CMoneyMembersActivationAccount>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = memberList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading C-Money activation data.");
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                if (path == "list")
                {
                    var account = await _overdraftActivationServices.GetAll();
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
                        var overdraftFacilityConfig = await _overdraftActivationServices.Get(KEY);
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
                var overdraftActivation = await _overdraftActivationServices.Get(id);
                if (overdraftActivation == null)
                {
                    return Content("<div class='text-danger p-3'>No data found.</div>");
                }

                return PartialView("_OverdraftFacilityConfigForm", overdraftActivation);
            }
            else
            {
                return PartialView("_OverdraftFacilityConfigForm", new OverdraftFacilityConfig());
            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _overdraftActivationServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }


        public async Task<bool> LoadDropdowns()
        {
            
            //var branches = await _branchServices.GetBranches();
            //ViewBag.branches = branches.ToList();
            ViewBag.OverdraftConfigs = (await _overdraftFacilityConfigServices.GetAll()).ToList();
            
            return true;
        }
    }
}