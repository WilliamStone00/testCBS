using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class AccountingChartController : BaseController
    {
        // GET: ChartOfAccount
 
        private readonly ChartOfAccountServices _Services;
        public AccountingChartController(ChartOfAccountServices services)
        {
            _Services = services;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            GetList();
           /*JsonRequestBehavior = JsonRequestBehavior.AllowGet*/
         
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(ChartOfAccount modeldto)
        {
            if (ModelState.IsValid)
            {
                string accNum = modeldto.AccountNumber.Substring(0, modeldto.AccountNumber.Length - 1);
                var mode = await _Services.GetChartOfAccountByAccountNumber(accNum);
                ChartOfAccountDto model = new ChartOfAccountDto
                {
                    RootParentId= mode.Id,
                    LabelEn = modeldto.LabelEn,
                    LabelFr = modeldto.LabelFr,
                    IsBalanceAccount = modeldto.IsBalanceSheetAccount,
                    AccountNumber = modeldto.AccountNumber
                };
               
                var data = await _Services.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        [HttpPost]
        public async Task<ActionResult> Update(ChartOfAccount modeldto)
        {
            if (ModelState.IsValid)
            {
                ChartOfAccountDto model = new ChartOfAccountDto
                {
                    RootParentId = modeldto.ParentAccountId,
                    LabelEn = modeldto.LabelEn,
                    IsBalanceAccount = modeldto.IsBalanceSheetAccount,
                    AccountNumber = modeldto.AccountNumber
                };
                var data = await _Services.Update(modeldto);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            GetList();
            if (path == "list" )
            {
                var treeData = await _Services.GetAllChartOfAccountTreeNodes();

                  return Json(treeData, JsonRequestBehavior.AllowGet);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new ChartOfAccount());
            }
            else if (path == "Transit")
            {
                var OperationEventAttribute = await _Services.GetChartOfAccountById(KEY);

                return PartialView(partialView, OperationEventAttribute);
            }
            else
            {
                if (string.IsNullOrEmpty(KEY))
                {
                    return PartialView("_AcccountChartErrorMessage", new ChartOfAccount());
                }
                else
                {
                    var OperationEventAttribute = await _Services.GetChartOfAccountByAccountNumber(KEY);
                    return PartialView(partialView, OperationEventAttribute);
                }
              
               //return Json(OperationEventAttribute, JsonRequestBehavior.AllowGet);

            }
        }
        public async Task<ActionResult> Operation(string KEY,string partialView)
        {
            var OperationEventAttribute = await _Services.GetChartOfAccountById(KEY);
       

            return PartialView(partialView, OperationEventAttribute);
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _Services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public void GetList()
        {

            //ViewBag.Branches = _Services.GetAllBranchesByBankId(_Services.BankId);

        }
    }
}