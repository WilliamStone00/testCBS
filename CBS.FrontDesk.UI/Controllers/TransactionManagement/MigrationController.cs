using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]
    public class MigrationController : BaseController
    {
        // GET: Migration
        private readonly MemberAccountJob _services;
        private readonly SavingProductServices _savingProductServices;
        private readonly IBranchServices _branchServices;
        public MigrationController(MemberAccountJob services, SavingProductServices savingProduct = null, IBranchServices branchServices = null)
        {
            _services = services;
            _savingProductServices = savingProduct;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> MembersAccount()
        {
            ViewBag.Products = await _savingProductServices.GetSavingProducts();
            ViewBag.Branches = await _branchServices.GetBranches();
            return View(new MemberAccountUpload());
        }
        [HttpPost]
        public async Task<ActionResult> MembersAccount(MemberAccountUpload model)
        {
            try
            {
                var account = await _services.ExtractFile(model);
                var data = await _services.UploadMembersAccount(account);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            catch (Exception ex)
            {
                // Log the exception
                // Handle the error gracefully, possibly return a meaningful error message
                return Json(new { success = false, message = "An error occurred while queuing the background job." });
            }
        }

        public ActionResult CheckJobStatus(string jobId)
        {
            var jobData = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
            var status = jobData.History[0].StateName;
            return Json(new { status });
        }

    }

}