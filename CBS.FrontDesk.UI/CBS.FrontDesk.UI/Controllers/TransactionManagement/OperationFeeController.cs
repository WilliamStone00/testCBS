using CBS.FrontDesk.Data.Message;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]
    public class OperationFeeController : BaseController
    {
        // GET: OperationFee
        private readonly OperationFeeServices _FeeServices;
        private readonly AccountingServices _accountingServices;
        public OperationFeeController(OperationFeeServices FeeServices, AccountingServices accountingServices)
        {
            _FeeServices = FeeServices;
            _accountingServices = accountingServices;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.Key = null;
                await GetList();
                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("InternalServer", "Error");
            }
         
        }
        [HttpPost]
        public async Task<ActionResult> Create(OperationFee model)
        {
            if (model.Id == null)
            {
                var data = await _FeeServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(OperationFee model)
        {
            var data = await _FeeServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
   
            await GetList();
            if (path == "list")
            {
                var data = await _FeeServices.GetFees();
                return PartialView(partialView, data);
            }

            else if (path == "new")
            {
                ViewBag.Key = null;
                return PartialView(partialView, new OperationFee());
            }
            else
            {
                ViewBag.Key = KEY;
                var Fee = await _FeeServices.GetFee(KEY);
                return PartialView(partialView, Fee);

            }
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _FeeServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            ViewBag.Languages = _FeeServices.GetLanguages();
            ViewBag.Roles = await _accountingServices.GetAccountingRoles();
            return true;
        }
    }
}