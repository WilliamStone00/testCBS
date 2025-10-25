using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2
{
    //[CheckSessionTimeOutAttribute]
    public class ChartOfAccountV2Controller : BaseController
    {
        private readonly ChartOfAccountsV2mockService _accountsService;
        private readonly ChartOfAccountsV2Service _accountsService2;
        private readonly BranchServices _branchServices;
        private readonly AffiliateAccountMockService _affiliateAccountMockService;

        // Constructor DI: make sure you register ChartOfAccountsV2Service in your DI container
        public ChartOfAccountV2Controller(ChartOfAccountsV2mockService accountsService, BranchServices branchServices, AffiliateAccountMockService affiliateAccountMockService, ChartOfAccountsV2Service chartOfAccountsV2Service)
        {
            _accountsService = accountsService;
            _branchServices = branchServices;
            _affiliateAccountMockService = affiliateAccountMockService;
            _accountsService2 = chartOfAccountsV2Service;
        }

       
        [HttpGet]
        public ActionResult Index()
        {
            
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetTreeData(string branchId = null)
       {
            try
            {
                var nodes = await _accountsService.GetAccountTreeForJsTreeAsync(branchId);
                return Json(nodes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // keep the response simple and non-technical like your other controllers
                System.Diagnostics.Debug.WriteLine("GetTreeData error: " + ex);
                return Json(new { success = false, message = "Failed to load chart of accounts." }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            return true;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await loader();
            return View();
        }

        // Note: use [FromBody] so model binder reads the JSON DataTables sends.
        [HttpPost]
        public async Task<JsonResult> LoadData(COADATATABLE_Query query)
        {               try
            {
              //  var data = await _accountsService.GetDataTableAsync(query);
              var data = await _accountsService2.GetDataTableAsync(query);

                var response = JsonConvert.DeserializeObject<List<HoPcmfAccountTreeDto>>(JsonConvert.SerializeObject(data.data));
                var mapped = _accountsService2.MapCodesToAccountNumbers(response.ToList());
                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = mapped
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<HoPcmfAccountTreeDto>(),
                    error = ex.Message
                });
            }
        }

        // GET: /Accounting_V2/ChartOfAccounts/GetAccount?id=xxx
        //[HttpGet]
        //public async Task<ActionResult> GetAccount(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //        return Json(new { success = false, message = "Invalid account id." }, JsonRequestBehavior.AllowGet);

        //    try
        //    {
        //        var account = await _accountsService.GetChartOfAccountByIdAsync(id); // implement this service method if not present
        //        if (account == null)
        //            return Json(new { success = false, message = "Account not found." }, JsonRequestBehavior.AllowGet);

        //        return Json(new { success = true, data = account }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("GetAccount error: " + ex);
        //        return Json(new { success = false, message = "Failed to load account." }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        // POST: /Accounting_V2/ChartOfAccounts/UpdateAccountName
        // Accepts UpdateAccountNameRequest (Id, NameEn, NameFr)
        [HttpPost]
        public async Task<ActionResult> UpdateAccountName(UpdateAccountNameRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
                return Json(new { success = false, message = "Invalid request." });

            try
            {
                // I previously provided UpdateAccountNameAsync(accountId, newNameEn, newNameFr)
                var result = await _accountsService2.UpdateAccountNameAsync(request);

                // ExecutionMessages is your standard response wrapper used across services
                return Json(new
                {
                    success = result?.Result ?? false,
                    status = result?.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateAccountName error: " + ex);
                return Json(new { success = false, message = "Failed to update account name." });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await loader();

            if (path == "list")
            {
                var data = await _affiliateAccountMockService.GetAsync();
                return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                var model = new AddAffiliateAccountCommand();
                if (!string.IsNullOrWhiteSpace(KEY))
                {
                    model.ParentId = KEY;
                }
                return PartialView(partialView, model);
            }
            else // This handles the "get" path for editing
            {
                // Get the Affiliateresponse from service
                //var entity = await _accountsService.GetAccountTreeDtoByIdAsync(KEY);
                var entity = await _accountsService2.GetAccountByIdAsync(KEY);

                if (entity == null)
                {
                    // Handle not found case
                    return Content("Affiliate not found");
                }                

                return PartialView(partialView, entity);
            }
        }

        // GET: /Accounting_V2/ChartOfAccounts/Delete?KEY=...
        //[HttpGet]
        //public async Task<ActionResult> Delete(string KEY)
        //{
        //    if (string.IsNullOrWhiteSpace(KEY))
        //        return Json(new { success = false, message = "Invalid id provided." }, JsonRequestBehavior.AllowGet);

        //    try
        //    {
        //        var result = await _accountsService.DeleteAsync(KEY); // implement DeleteAsync in service or map to existing Delete
        //        return Json(new
        //        {
        //            success = result?.Result ?? false,
        //            status = result?.MessageStatus,
        //            message = Messaging.MessageResult(result)
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Delete error: " + ex);
        //        return Json(new { success = false, message = "Failed to delete account." }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}
