using CBS.BusinessService.AccountingDayObject;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.AccountOpeningRulePerBranchP;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{

    [CheckSessionTimeOutAttribute]
    public class AccountOpeningRulePerBranchController : BaseController
    {
        // GET: AccountOpeningRulePerBranch
        private readonly AccountOpeningRulePerBranchServices _services;
        private readonly SavingProductServices _savingProductServices;
        private readonly BranchServices _branchServices;
        public AccountOpeningRulePerBranchController(AccountOpeningRulePerBranchServices services, BranchServices branchServices = null, SavingProductServices savingProductServices = null)
        {
            _services = services;
            _branchServices = branchServices;
            _savingProductServices=savingProductServices;
        }

        public async Task<ActionResult> Index()
        {
            // Fetch the list of branches from the service
            await Loaders();
            return View(new AccountOpeningRulePerBranch());
        }


        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(AccountOpeningRulePerBranch model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, status = "ValidationError", message = string.Join(" ", errors) });
            }

            try
            {
                if (model.Id == null)
                {
                    var data = await _services.Create(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else if (model.Id !=null)
                {
                    var data = await _services.Update(model);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else
                {
                    return Json(new { success = false, status = "InvalidAction", message = "Invalid action specified." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "Error", message = $"An error occurred: {ex.Message}" });
            }
        }
      
        public ActionResult Listing()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetAllAccountOpeningRules(GetAllAccountOpeningRulesQuery allAccountOpeningRulesQuery)
        {
            try
            {
                // Step 1: Optional validation (if applicable)
                var validationContext = new ValidationContext(allAccountOpeningRulesQuery, null, null);
                var validationResults = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(allAccountOpeningRulesQuery, validationContext, validationResults, true);

                if (!isValid)
                {
                    var errors = validationResults.Select(v => v.ErrorMessage).ToList();
                    return Json(new { success = false, status = "ValidationError", message = string.Join(" ", errors) });
                }

                // Step 2: Get data
                var data = await _services.GetAccountOpeningRulePerBranches(allAccountOpeningRulesQuery);

                // Step 3: Return as JSON
                return Json(new { success = true, data = data.ToList() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, status = "ServerError", error = ex.Message });
            }
        }

        public async Task<ActionResult> Loaders()
        {
            var branches = await _branchServices.GetBranches();
            var SavingProducts = await _savingProductServices.GetSavingProductsDropdownAsync();
            ViewBag.Branches=branches.ToList();
            ViewBag.SavingProducts=SavingProducts.ToList();
            return View();
        }
        [HttpGet]
        public async Task<JsonResult> GetById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "Invalid rule ID." });

                var rule = await _services.GetAccountOpeningRule(id);
                await Loaders();
                if (rule == null)
                    return Json(new { success = false, message = "Rule not found." });

                return Json(new { success = true, data = rule }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "Invalid rule ID." });

                var result = await _services.Delete(id);
                return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }

}