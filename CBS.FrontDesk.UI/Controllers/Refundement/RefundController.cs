using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.Config;
using CBS.BusinessService.Repayment;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.LoanRepayment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Refundement
{
    public class RefundController : Controller
    {

        private readonly RefundServices _refundServices;
        private readonly BranchServices _branchServices;


        public RefundController(RefundServices refundServices, BranchServices branchServices)
        {
            _refundServices = refundServices;
            _branchServices = branchServices;
        }
        // GET: Refund
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }

        public async Task<bool> loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            return true;
        }


        public async Task<JsonResult> LoadRefundmentData(LoanRefundQuery query)
        {
            try
            {
                var data = await _refundServices.GeRefundDataTableAsync(query);

                var reconciliations = JsonConvert.DeserializeObject<List<LoanRefundDto>>(
                    JsonConvert.SerializeObject(data.data));


                return Json(new
                {

                    draw = data.DataTableOptions.draw ?? "1",
                    recordsTotal = data.DataTableOptions.recordsTotal,
                    recordsFiltered = data.DataTableOptions.recordsFiltered,
                    data = reconciliations,
                    success = true,
                    message = " DataTable loaded successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.DataTableOptions?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }
    }
}