using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;

namespace CBS.FrontDesk.UI.Controllers.Configuration
{
    [CheckSessionTimeOutAttribute]

    public class LoanProductController : BaseController
    {
        // GET: LoanProduct
        private readonly LoanProductServices _LoanProductServices;
        private readonly LoanProductCategoryServices _loanProductCategoryServices;
        private readonly LoanTermServices _loanTermServices;
        private readonly PenaltyServices _PenaltyServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        public LoanProductController(LoanProductServices LoanProductServices, ChartOfAccountServicesAnnex accountingServices, PenaltyServices penaltyServices, LoanProductCategoryServices loanProductCategoryServices, LoanTermServices loanTermServices)
        {
            _LoanProductServices = LoanProductServices;
            _accountingServices = accountingServices;
            _PenaltyServices = penaltyServices;
            _loanProductCategoryServices = loanProductCategoryServices;
            _loanTermServices = loanTermServices;
        }
        public async Task<ActionResult> Index()
        {
            await GetValues();
            return View(new LoanProductObject());
        }
        public ActionResult ProductListingPolicySet()
        {
            return View();
        }

        public async Task<ActionResult> Policy(string Key)
        {
            await GetValues();
            var LoanProduct = await _LoanProductServices.GetLoanProduct(Key);
            var loanProductObject = new LoanProductObject();
            loanProductObject.UpdateLoanProductCommand = _LoanProductServices.ProductMappingToUpdateObject(LoanProduct, "N/A", "N/A");
            return View(loanProductObject);

        }
        //Policy
        public async Task<ActionResult> LoanAccountMapping()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(LoanProductObject model)
        {
            if (model.ServiceOption == "insert")
            {
                if (ModelState.IsValid)
                {
                    var data = await _LoanProductServices.Create(model.AddLoanProductCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else
                {
                    var errorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Where(e => e.ErrorMessage != null)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    // Convert the list of error messages to a single string with each message on a new line
                    string errorMessage = string.Join("\n", errorMessages);

                    // Pass the error message as the message
                    return Json(new { success = false, status = false, message = errorMessage });
                }
            }
            else
            {
                return await Update(model);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(LoanProductObject model)
        {

           

            if (model.ServiceOption == "set_penalty")
            {
                var data = await _PenaltyServices.Create(model.Penalty);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else
            {
                model.UpdateLoanProductCommand.ServiceOption = model.ServiceOption;
                if (model.ServiceOption == "product")
                {
                    model.UpdateLoanProductCommand.ProductCode = model.AddLoanProductCommand.ProductCode;
                    model.UpdateLoanProductCommand.TargetType = model.AddLoanProductCommand.TargetType;
                    model.UpdateLoanProductCommand.ProductName = model.AddLoanProductCommand.ProductName;
                    model.UpdateLoanProductCommand.Id = model.AddLoanProductCommand.Id;
                    model.UpdateLoanProductCommand.Description = model.AddLoanProductCommand.Description;
                    model.UpdateLoanProductCommand.ActiveStatus = model.AddLoanProductCommand.ActiveStatus;
                    model.UpdateLoanProductCommand.ServiceOption = model.ServiceOption;
                    model.UpdateLoanProductCommand.LoanTermId = model.AddLoanProductCommand.LoanTermId;
                    model.UpdateLoanProductCommand.LoanProductCategoryId = model.AddLoanProductCommand.LoanProductCategoryId;
                    model.UpdateLoanProductCommand.IsProductWithSavingFacilities = model.AddLoanProductCommand.IsProductWithSavingFacilities;
                    model.UpdateLoanProductCommand.IsMortgage = model.AddLoanProductCommand.IsMortgage;

    }
                else if (model.ServiceOption == "duration")
                {
                    var selectedLoanTerm = await _loanTermServices.GetLoanTerm(model.UpdateLoanProductCommand.LoanTermId); // Fetch LoanTerm details
                    if (selectedLoanTerm == null)
                    {
                        return Json(new
                        {
                            success = false,
                            status = false,
                            message = "The selected loan term is invalid. Please select a valid loan term and try again Or Set the loan term."
                        });
                    }

                    // Validate the minimum duration period
                    if (model.UpdateLoanProductCommand.MinimumDurationPeriod < selectedLoanTerm.MinInMonth ||
                        model.UpdateLoanProductCommand.MinimumDurationPeriod > selectedLoanTerm.MaxInMonth)
                    {
                        return Json(new
                        {
                            success = false,
                            status = false,
                            message = $"The provided Minimum Duration Period of {model.UpdateLoanProductCommand.MinimumDurationPeriod} months does not fall within the allowable range of {selectedLoanTerm.MinInMonth} to {selectedLoanTerm.MaxInMonth} months for the selected loan term '{selectedLoanTerm.Name}'. Please review and adjust accordingly."
                        });
                    }

                    // Validate the maximum duration period
                    if (model.UpdateLoanProductCommand.MaximumDurationPeriod < selectedLoanTerm.MinInMonth ||
                        model.UpdateLoanProductCommand.MaximumDurationPeriod > selectedLoanTerm.MaxInMonth)
                    {
                        return Json(new
                        {
                            success = false,
                            status = false,
                            message = $"The provided Maximum Duration Period of {model.UpdateLoanProductCommand.MaximumDurationPeriod} months exceeds the permissible range of {selectedLoanTerm.MinInMonth} to {selectedLoanTerm.MaxInMonth} months for the selected loan term '{selectedLoanTerm.Name}'. Please review and adjust your input."
                        });
                    }

                    // Validate that minimum is not greater than maximum
                    if (model.UpdateLoanProductCommand.MinimumDurationPeriod > model.UpdateLoanProductCommand.MaximumDurationPeriod)
                    {
                        return Json(new
                        {
                            success = false,
                            status = false,
                            message = $"The Minimum Duration Period ({model.UpdateLoanProductCommand.MinimumDurationPeriod} months) cannot be greater than the Maximum Duration Period ({model.UpdateLoanProductCommand.MaximumDurationPeriod} months) for the selected loan term '{selectedLoanTerm.Name}'. Please ensure the values are entered correctly."
                        });
                    }
                }


                var data = await _LoanProductServices.Update(model.UpdateLoanProductCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }

        }
        public async Task<ActionResult> GetLoanProductAccountMapping()
        {

            List<ProductAccountingChart> AccountProductItems = await _accountingServices.GetProductAccountingBookByproducttype("Loan_Product");
            return View(new LoanProductObject { ProductAccountingCharts = AccountProductItems });


        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            ViewBag.Key = KEY;
            if (path == "list")
            {

                var data = await _LoanProductServices.GetLoanProducts();
                return PartialView(partialView, new LoanProductObject { LoanProducts = data.ToList() });
            }

            else if (path == "new")
            {
                ViewBag.Key = null;
                await GetValues();
                return PartialView(partialView, new LoanProductObject());
            }
            else
            {
                if (path == "set_penalty")
                {
                    var productEnumAgregates = await _LoanProductServices.GetLoanProductEnumAggregates();
                    ViewBag.PenaltyTypes = productEnumAgregates.PenaltyTypes;
                    ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
                    var penalty = await _PenaltyServices.GetPenalty(KEY);

                    return PartialView(partialView, penalty);
                }
                else if (path == "add_penalty")
                {
                    var productEnumAgregates = await _LoanProductServices.GetLoanProductEnumAggregates();
                    ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
                    ViewBag.PenaltyTypes = productEnumAgregates.PenaltyTypes;
                    var LoanProduct = await _LoanProductServices.GetLoanProduct(KEY);

                    //LoanProduct.Penalty.LoanProductId = LoanProduct.Id;
                    return PartialView(partialView, LoanProduct);
                }
                else if (path == "account_mapping")
                {
                    var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
                    ViewBag.ChartOfAccounts = chartOfAccounts;
                    var LoanProduct = await _LoanProductServices.GetLoanProduct(KEY);
                    var loanProductObject = new LoanProductObject();
                    loanProductObject.UpdateLoanProductCommand = _LoanProductServices.ProductMappingToUpdateObject(LoanProduct, "N/A", "N/A");
                    return PartialView(partialView, loanProductObject);
                }
                else
                {
                    await GetValues();
                    var LoanProduct = await _LoanProductServices.GetLoanProduct(KEY);
                    var loanProductObject = new LoanProductObject();
                    loanProductObject.UpdateLoanProductCommand = _LoanProductServices.ProductMappingToUpdateObject(LoanProduct, "N/A", "N/A");
                    loanProductObject.AddLoanProductCommand = new AddLoanProductCommand
                    {
                        ActiveStatus = LoanProduct.ActiveStatus,
                        Description = LoanProduct.Description,
                        Id = LoanProduct.Id,
                        ProductCode = LoanProduct.ProductCode,
                        ProductName = LoanProduct.ProductName,
                        TargetType = LoanProduct.TargetType,
                        IsProductWithSavingFacilities = LoanProduct.IsProductWithSavingFacilities,
                        LoanTermId = LoanProduct.LoanTermId,
                        LoanProductCategoryId = LoanProduct.LoanProductCategoryId

                    };
                    return PartialView(partialView, loanProductObject);
                }

            }
        }

        public async Task<bool> GetValues()
        {
            var agreggates = await _LoanProductServices.GetAgreggates();
            var productEnumAgregates = await _LoanProductServices.GetLoanProductEnumAggregates();
            var loanProductCategories = await _loanProductCategoryServices.GetLoanProductCategorys();
            var loanTerms = await _loanTermServices.GetLoanTerms();

            ViewBag.SheduleTypes = _LoanProductServices.GetScheduleTypes();
            ViewBag.Penalties = productEnumAgregates.Penalties;
            ViewBag.ProductCategories = loanProductCategories;
            ViewBag.LoanTerms = loanTerms;
            ViewBag.Fees = agreggates.Fees;
            ViewBag.DocumentPackes = agreggates.DocumentPackes;
            ViewBag.GuranteePackes = agreggates.GuranteePackes;
            ViewBag.Taxes = agreggates.Taxes;
            ViewBag.FundingLines = agreggates.FundingLines;
            ViewBag.InstallmentTypes = agreggates.InstallmentTypes;
            ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
            ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
            ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
            ViewBag.LoanStatuses = productEnumAgregates.LoanStatuses;
            ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
            ViewBag.LoanInterestPeriods = productEnumAgregates.LoanInterestPeriods;
            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.RefundOrders = productEnumAgregates.RefundOrders;
            ViewBag.PenaltyTypes = productEnumAgregates.PenaltyTypes;
            ViewBag.YesOrNo = productEnumAgregates.YesOrNo;
            ViewBag.LoanCategories = productEnumAgregates.LoanCategories;
            ViewBag.LoanTargets = productEnumAgregates.LoanTargets;
            //YesOrNo
            return true;
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _LoanProductServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}