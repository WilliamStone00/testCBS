using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Message;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2
{
    //[CheckSessionTimeOutAttribute]
    public class ChartOfAccountV2Controller : BaseController
    {
        private readonly ChartOfAccountsV2mockService _accountsService;

        // Constructor DI: make sure you register ChartOfAccountsV2Service in your DI container
        public ChartOfAccountV2Controller(ChartOfAccountsV2mockService accountsService)
        {
            _accountsService = accountsService ?? throw new ArgumentNullException(nameof(accountsService));
        }

        // GET: /Accounting_V2/ChartOfAccounts
        [HttpGet]
        public ActionResult Index()
        {
            // If you need viewbag/loader data, call a Loader method similar to your old controller.
            return View();
        }

        // GET: /Accounting_V2/ChartOfAccounts/GetTreeData
        // Returns jsTree-compatible JSON nodes
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
                var result = await _accountsService.UpdateAccountNameAsync(request.Id, request.NameEn, request.NameFr);

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
