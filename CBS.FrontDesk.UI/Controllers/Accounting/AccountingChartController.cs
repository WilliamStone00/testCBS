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
        private readonly AccountCategoryServices _AccountCategoryServices;

        public AccountingChartController(ChartOfAccountServices services, AccountCategoryServices accountCategoryServices)
        {
            _Services = services;
            _AccountCategoryServices = accountCategoryServices;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            GetListAsync();
           /*JsonRequestBehavior = JsonRequestBehavior.AllowGet*/
         
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(ChartOfAccount modeldto)
        {
            if (ModelState.IsValid)
            {
                string accNum = (modeldto.AccountNumber.Length==1)? modeldto.AccountNumber:modeldto.AccountNumber.Substring(0, modeldto.AccountNumber.Length - 1);
                var mode = await _Services.GetChartOfAccountByAccountNumber(accNum);
                if (mode==null)
                {
                    if (accNum.Length==1) 
                    {
                        ChartOfAccountDto model = new ChartOfAccountDto
                        {
                            RootParentId = "",
                            LabelEn = modeldto.LabelEn,
                            LabelFr = modeldto.LabelFr,
                            IsBalanceAccount = modeldto.IsBalanceSheetAccount,
                            AccountNumber = modeldto.AccountNumber,
                            CanBeNegative = modeldto.CanBeNegative,
                            IsDebit = modeldto.OperationDirection == "DEBIT",
                            AccountCartegoryId = modeldto.AccountCartegoryId
                        };

                        var data = await _Services.Create(model);
                        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                    }
                    else
                    {
                        return Json(new { success = false, status = "failed", message = "There no root account " + accNum + " inside the chartofAccount" });
                    }
                   
                }
                else
                {
                    ChartOfAccountDto model = new ChartOfAccountDto
                    {
                        RootParentId = mode.Id,
                        LabelEn = modeldto.LabelEn,
                        LabelFr = modeldto.LabelFr,
                        IsBalanceAccount = modeldto.IsBalanceSheetAccount,
                        AccountNumber = modeldto.AccountNumber,
                        CanBeNegative = modeldto.CanBeNegative,
                        IsDebit = modeldto.OperationDirection == "DEBIT",
                        AccountCartegoryId = modeldto.AccountCartegoryId
                    };

                    var data = await _Services.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
               
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
                    AccountNumber = modeldto.AccountNumber,
                      LabelFr = modeldto.LabelFr,                
                    CanBeNegative = modeldto.CanBeNegative,
                    IsDebit = modeldto.OperationDirection == "DEBIT",
                    AccountCartegoryId = modeldto.AccountCartegoryId
                };
                var data = await _Services.Update(modeldto);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
           await  GetListAsync();
            if (path == "list" )
            {
                var treeData = await _Services.GetAllChartOfAccountTreeNodes();

                  return Json(treeData, JsonRequestBehavior.AllowGet);
            }
            else if (path == "new")
            {
                var account = await _Services.GetChartOfAccountByAccountNumber(KEY);
                 account =(account == null )?new ChartOfAccount(): account;
                return PartialView(partialView, account);
            }
            else if (path == "Transit")
            {
                var OperationEventAttribute = await _Services.GetChartOfAccountById(KEY);

                return PartialView(partialView, OperationEventAttribute);
            }else if (path == "Cartegory")
            {
                var treeData = await _AccountCategoryServices.GetAccountClassCategory(KEY);

                return Json(treeData, JsonRequestBehavior.AllowGet);
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
        public async Task GetListAsync()
        {
            ViewBag.AccountCartegories = await _AccountCategoryServices.GetAccountCategory();
            ViewBag.OperationDirection = BuildMenuViewBag();

        }
        private dynamic BuildMenuViewBag()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem { Text = $"DEBIT", Value = "DEBIT" });

            list.Add(new SelectListItem { Text = $"CREDIT", Value = "CREDIT" });


            return list;
        }
    }
}