//using CBS.BusinessService.AccountingV2.ReportingV2;
//using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine;
//using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
//using CBS.FrontDesk.Data.Message;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace CBS.FrontDesk.UI.Controllers.ReportConfiguration
//{
//	public class ReportLineMappingController : BaseController
//	{
//		// GET: ReportLineMapping
//		private readonly ReportLineMappingService _services;
//		private readonly ReportLineService _lineService;
//		public ReportLineMappingController(ReportLineMappingService services, ReportLineService lineService)
//		{
//			_services = services;
//			_lineService = lineService;
//		}

//		// GET: ReportLineMappingt
//		public ActionResult Index()
//		{
//			return View(new ReportLineMapping());
//		}

//		public async Task<ActionResult> Initilization(string path = "list", string partialView = null, string KEY = null)
//		{
//			// par défaut, s'il n'y a pas de vue partielle fournie, on charge _List
//			partialView = partialView ?? (path == "list" ? "_List" : "_Create");

//			switch (path)
//			{
//				case "list":
//					var list = await _services.GetAll();
//					return PartialView(partialView, list);

//				case "new":
//					ViewBag.ReportLines = await _lineService.GetAll();
//					return PartialView($"~/Views/ReportConfiguration/ReportLineMapping/{partialView}.cshtml", new ReportLineMapping());

//				case "edit":
//					if (string.IsNullOrEmpty(KEY))
//						return new HttpStatusCodeResult(400, "Invalid report key");

//					var report = await _services.GetById(KEY);
//					if (report == null)
//						return HttpNotFound("Report not found");

//					ViewBag.ReportLines = await _lineService.GetAll();
//					return PartialView($"~/Views/ReportConfiguration/ReportLineMapping/{partialView}.cshtml", report);

//				default:
//					var all = await _services.GetAll();
//					return PartialView("_List", all);
//			}
//		}

//		public async Task<ActionResult> ReloadList()
//		{
//			var list = await _services.GetAll();
//			return PartialView("~/Views/ReportConfiguration/ReportLineMapping/_List.cshtml", list);
//		}

//		[HttpPost]
//		public async Task<ActionResult> Create(ReportLineMapping model)
//		{

//			// Validate the model state
//			if (!ModelState.IsValid)
//			{
//				// If model validation fails, return validation errors as JSON response
//				return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
//			}

//			// If the Id is null, it's a new holiday entry, so call the Create service
//			if (model.Id == null)
//			{
//				// Adding Created Date
//				var data = await _services.Create(model);
//				return Json(new
//				{
//					success = data.Result,
//					status = data.MessageStatus,
//					message = Messaging.MessageResult(data),
//					reloadDataView = "Yes",
//					controllerName = "ReportLineMapping",
//					option = "List",
//					divLoaderList = "mappingContainer",
//					tableName = "myDataTable_line_mapping",
//					dataLoaderActionName = "ReloadList"
//				});
//			}
//			else
//			{
//				// If the Id is not null, it's an update, so call the Update method
//				return await Update(model);
//			}
//		}

//		[HttpPost]
//		public async Task<ActionResult> Update(ReportLineMapping model)
//		{
//			var data = await _services.Update(model);

//			return Json(new
//			{
//				success = data.Result,
//				status = data.MessageStatus,
//				message = Messaging.MessageResult(data),
//				reloadDataView = "Yes",
//				controllerName = "ReportLineMapping",
//				option = "List",
//				divLoaderList = "mappingContainer",
//				tableName = "myDataTable_line_mapping",
//				dataLoaderActionName = "ReloadList"
//			});
//		}

//		public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
//		{
//			if (path == "list")
//			{
//				var data = await _services.GetAll();
//				return PartialView(partialView, data);
//			}

//			else if (path == "new")
//			{
//				return PartialView(partialView, new ReportLineMapping());
//			}
//			else
//			{
//				ViewBag.Key = KEY;
//				var ReportLineMapping = await _services.GetById(KEY);
//				return PartialView(partialView, ReportLineMapping);

//			}
//		}

//		public async Task<ActionResult> Delete(string KEY)
//		{
//			var data = await _services.Delete(KEY);
//			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
//		}

//		public async Task<ActionResult> GetReportLineMapping(string Key)
//		{
//			var data = await _services.GetById(Key);
//			return Json(data, JsonRequestBehavior.AllowGet);
//		}

//		public async Task<ActionResult> GetReportLineMappingPartialView(string Key)
//		{
//			var data = await _services.GetById(Key);
//			if (data == null)
//			{
//				return HttpNotFound();
//			}

//			return PartialView("_ReportLineMappingDetailsPartial", data);
//		}
//	}
//}


using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportConfiguration
{
    public class ReportLineMappingController : BaseController
    {
        private readonly ReportLineMappingService _services;
        private readonly ReportLineService _lineService;
        private readonly AffiliateAccountService _AffiliateAccountService;

        public ReportLineMappingController(
            ReportLineMappingService services,
            ReportLineService lineService,
            AffiliateAccountService affiliateAccountService)
        {
            _services = services;
            _lineService = lineService;
            _AffiliateAccountService = affiliateAccountService;
        }

        // GET: ReportLineMappingt
        public ActionResult Index()
        {
            return View(new ReportLineMapping());
        }

        public async Task<ActionResult> Initilization(string path = "list", string partialView = null, string KEY = null)
        {
            partialView = partialView ?? (path == "list" ? "_List" : "_Create");

            switch (path)
            {
                case "list":
                    var list = await _services.GetAll();
                    return PartialView(partialView, list);

                case "new":
                    await LoadViewBagsForForm();
                    return PartialView($"~/Views/ReportConfiguration/ReportLineMapping/{partialView}.cshtml", new ReportLineMapping());

                case "edit":
                    if (string.IsNullOrEmpty(KEY))
                        return new HttpStatusCodeResult(400, "Invalid report key");

                    var report = await _services.GetById(KEY);
                    if (report == null)
                        return HttpNotFound("Report not found");

                    await LoadViewBagsForForm(report);
                    return PartialView($"~/Views/ReportConfiguration/ReportLineMapping/{partialView}.cshtml", report);

                default:
                    var all = await _services.GetAll();
                    return PartialView("_List", all);
            }
        }

        private async Task LoadViewBagsForForm(ReportLineMapping model = null)
        {
            // Load report lines for dropdown
            ViewBag.ReportLines = await _lineService.GetAll();

            // Load COBAC structure data as SelectLists
            ViewBag.MainSections = GetMainSectionsSelectList();

            // Load sub-sections based on main section if model exists
            if (model != null && !string.IsNullOrEmpty(model.MainSection))
            {
                ViewBag.SubSections = GetSubSectionsSelectList(model.MainSection);
                ViewBag.CodePosteItems = GetCodePosteItemsSelectList(model.MainSection, model.SubSection);
            }
            else
            {
                ViewBag.SubSections = new List<SelectListItem>();
                ViewBag.CodePosteItems = new List<SelectListItem>();
            }

            // Load affiliate accounts for AccountCode match type as SelectList
            ViewBag.AffiliateAccounts = await _AffiliateAccountService.GetAllAffiliateAccounts();

            // If editing, load the appropriate selected items
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.AccountCode))
                {
                    ViewBag.SelectedAccountCode = model.AccountCode;
                }
                if (!string.IsNullOrEmpty(model.CodePoste))
                {
                    ViewBag.SelectedCodePoste = model.CodePoste;
                }
            }
        }

        public async Task<ActionResult> ReloadList()
        {
            var list = await _services.GetAll();
            return PartialView("~/Views/ReportConfiguration/ReportLineMapping/_List.cshtml", list);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ReportLineMapping model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            // Validate based on MatchType
            if (model.MatchType == ReportMatchType.AccountCode && string.IsNullOrEmpty(model.AccountCode))
            {
                return Json(new { success = false, message = "Account Code is required for AccountCode match type" });
            }

            if (model.MatchType == ReportMatchType.CodePoste && string.IsNullOrEmpty(model.CodePoste))
            {
                return Json(new { success = false, message = "Code Poste is required for CodePoste match type" });
            }

            if (model.Id == null)
            {
                var data = await _services.Create(model);
                return Json(new
                {
                    success = data.Result,
                    status = data.MessageStatus,
                    message = Messaging.MessageResult(data),
                    reloadDataView = "Yes",
                    controllerName = "ReportLineMapping",
                    option = "List",
                    divLoaderList = "mappingContainer",
                    tableName = "myDataTable_line_mapping",
                    dataLoaderActionName = "ReloadList"
                });
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Update(ReportLineMapping model)
        {
            // Validate based on MatchType
            if (model.MatchType == ReportMatchType.AccountCode && string.IsNullOrEmpty(model.AccountCode))
            {
                return Json(new { success = false, message = "Account Code is required for AccountCode match type" });
            }

            if (model.MatchType == ReportMatchType.CodePoste && string.IsNullOrEmpty(model.CodePoste))
            {
                return Json(new { success = false, message = "Code Poste is required for CodePoste match type" });
            }

            var data = await _services.Update(model);

            return Json(new
            {
                success = data.Result,
                status = data.MessageStatus,
                message = Messaging.MessageResult(data),
                reloadDataView = "Yes",
                controllerName = "ReportLineMapping",
                option = "List",
                divLoaderList = "mappingContainer",
                tableName = "myDataTable_line_mapping",
                dataLoaderActionName = "ReloadList"
            });
        }

        // API Endpoints for dynamic loading
        [HttpGet]
        public async Task<ActionResult> GetSubSections(string mainSection)
        {
            var subSections = GetSubSectionsSelectList(mainSection);
            return Json(subSections, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetCodePosteItems(string mainSection, string subSection = null)
        {
            var items = GetCodePosteItemsSelectList(mainSection, subSection);
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetMatchTypeField(short matchType, string selectedValue = null)
        {
            switch ((ReportMatchType)matchType)
            {
                case ReportMatchType.AccountCode:
                    var accounts = await _AffiliateAccountService.GetAllAffiliateAccounts();
                    var accountSelectList = new SelectList(accounts, "AccountCode", "AccountName", selectedValue);
                    ViewBag.SelectedAccountCode = selectedValue;
                    return PartialView("_AccountCodeField", accountSelectList);

                case ReportMatchType.CodePoste:
                    ViewBag.SelectedCodePoste = selectedValue;
                    return PartialView("_CodePosteField");

                default:
                    return Content("");
            }
        }

       

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetReportLineMapping(string Key)
        {
            var data = await _services.GetById(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetReportLineMappingPartialView(string Key)
        {
            var data = await _services.GetById(Key);
            if (data == null)
            {
                return HttpNotFound();
            }
            return PartialView("_ReportLineMappingDetailsPartial", data);
        }

        #region COBAC Data Methods - Returns SelectListItems

        private List<SelectListItem> GetMainSectionsSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "ASSETS (EP)", Value = "assets" },
                new SelectListItem { Text = "LIABILITIES (RP)", Value = "liabilities" },
                new SelectListItem { Text = "EXPENDITURE (ZP)", Value = "expenditure" },
                new SelectListItem { Text = "INCOME (LP)", Value = "income" }
            };
        }

        private List<SelectListItem> GetSubSectionsSelectList(string mainSection)
        {
            switch (mainSection?.ToLower())
            {
                case "assets":
                    return new List<SelectListItem>
                    {
                        new SelectListItem { Text = "CAPITAL", Value = "CAPITAL" },
                        new SelectListItem { Text = "CREDITS", Value = "CREDITS" },
                        new SelectListItem { Text = "GOODS INVENTORY", Value = "GOODS INVENTORY" },
                        new SelectListItem { Text = "THIRD PARTY", Value = "THIRD PARTY" },
                        new SelectListItem { Text = "CASH RECEIPTS", Value = "CASH RECEIPTS" },
                        new SelectListItem { Text = "ACCRUALS", Value = "ACCRUALS" },
                        new SelectListItem { Text = "RECIPROCAL", Value = "RECIPROCAL" },
                        new SelectListItem { Text = "TREASURY", Value = "TREASURY" }
                    };

                case "liabilities":
                    return new List<SelectListItem>
                    {
                        new SelectListItem { Text = "HERITAGE FUNDS", Value = "HERITAGE FUNDS" },
                        new SelectListItem { Text = "PROVISIONS & RISKS", Value = "PROVISIONS & RISKS" },
                        new SelectListItem { Text = "BORROWINGS", Value = "BORROWINGS" },
                        new SelectListItem { Text = "DEPOSITS", Value = "DEPOSITS" },
                        new SelectListItem { Text = "SHORT TERM DEBTS", Value = "SHORT TERM DEBTS" },
                        new SelectListItem { Text = "CASH RECEIPT", Value = "CASH RECEIPT" },
                        new SelectListItem { Text = "ACCRUALS", Value = "ACCRUALS" },
                        new SelectListItem { Text = "RECIPROCAL", Value = "RECIPROCAL" },
                        new SelectListItem { Text = "TREASURY", Value = "TREASURY" },
                        new SelectListItem { Text = "RESULTS", Value = "RESULTS" }
                    };

                case "expenditure":
                    return new List<SelectListItem>
                    {
                        new SelectListItem { Text = "FINANCIAL OPERATING", Value = "FINANCIAL OPERATING" },
                        new SelectListItem { Text = "PERSONNEL", Value = "PERSONNEL" },
                        new SelectListItem { Text = "GENERAL OPERATING", Value = "GENERAL OPERATING" },
                        new SelectListItem { Text = "TAXES & DUES", Value = "TAXES & DUES" },
                        new SelectListItem { Text = "AMORT & PROV", Value = "AMORT & PROV" },
                        new SelectListItem { Text = "EXCEPTIONAL", Value = "EXCEPTIONAL" },
                        new SelectListItem { Text = "NET INCOME", Value = "NET INCOME" }
                    };

                case "income":
                    return new List<SelectListItem>
                    {
                        new SelectListItem { Text = "FINANCIAL PROCEEDS", Value = "FINANCIAL PROCEEDS" },
                        new SelectListItem { Text = "OTHER PROCEEDS & GRANTS", Value = "OTHER PROCEEDS & GRANTS" },
                        new SelectListItem { Text = "EXCEPTIONAL", Value = "EXCEPTIONAL" },
                        new SelectListItem { Text = "TRANSFERS & PROV", Value = "TRANSFERS & PROV" },
                        new SelectListItem { Text = "LOSSES", Value = "LOSSES" }
                    };

                default:
                    return new List<SelectListItem>();
            }
        }

        private List<SelectListItem> GetCodePosteItemsSelectList(string mainSection, string subSection = null)
        {
            var items = new List<ChartItem>();

            switch (mainSection?.ToLower())
            {
                case "assets":
                    items = GetAssetItems();
                    break;
                case "liabilities":
                    items = GetLiabilityItems();
                    break;
                case "expenditure":
                    items = GetExpenditureItems();
                    break;
                case "income":
                    items = GetIncomeItems();
                    break;
                default:
                    return new List<SelectListItem>();
            }

            // Filter by sub-section if provided
            if (!string.IsNullOrEmpty(subSection))
            {
                var pattern = GetPatternForSubSection(mainSection, subSection);
                if (!string.IsNullOrEmpty(pattern))
                {
                    items = items.Where(i => System.Text.RegularExpressions.Regex.IsMatch(i.Ref, pattern)).ToList();
                }
            }

            return items.Select(i => new SelectListItem
            {
                Text = $"{i.Ref} - {i.Header}",
                Value = i.Ref
            }).ToList();
        }

        private List<ChartItem> GetAssetItems()
        {
            return new List<ChartItem>
            {
                new ChartItem("EP01", "Capital"),
                new ChartItem("EP02", "Capitalized costs"),
                new ChartItem("EP03", "Intangible fixed assets"),
                new ChartItem("EP04", "Lands"),
                new ChartItem("EP05", "Other tangible assets"),
                new ChartItem("EP06", "Fixed assets under construction, Advanced payments"),
                new ChartItem("EP07", "Deposits and guarantees"),
                new ChartItem("EP08", "Equity securities and government papers"),
                new ChartItem("EP09", "CREDITS TO MEMBERS"),
                new ChartItem("EP10", "Healthy long terms credits"),
                new ChartItem("EP11", "Healthy medium term credits"),
                new ChartItem("EP12", "Healthy short term credits"),
                new ChartItem("EP13", "Healthy debtor accounts"),
                new ChartItem("EP14", "Unpaid credits"),
                new ChartItem("EP15", "Capitalized credits"),
                new ChartItem("EP16", "Doubtful credits"),
                new ChartItem("EP17", "GOODS INVENTORY AND OTHER SIMILAR OPERATIONS"),
                new ChartItem("EP18", "THIRD PARTY ACCOUNTS"),
                new ChartItem("EP19", "Suppliers"),
                new ChartItem("EP20", "Staff"),
                new ChartItem("EP21", "State"),
                new ChartItem("EP22", "Members"),
                new ChartItem("EP23", "Sundry deebtors"),
                new ChartItem("EP24", "Sundry outstanding claims"),
                new ChartItem("EP25", "CASH RECEIPTS"),
                new ChartItem("EP26", "Securities to be collected"),
                new ChartItem("EP27", "Values on receipt in arrears"),
                new ChartItem("EP28", "ACCRUALS"),
                new ChartItem("EP29", "Costs recorded in advance"),
                new ChartItem("EP30", "Proceeds receivable"),
                new ChartItem("EP31", "Other adjustment operations"),
                new ChartItem("EP32", "RECIPROCAL ACCOUNTS"),
                new ChartItem("EP33", "Liaison headquarters and branches"),
                new ChartItem("EP34", "Liaison apex body and affiliated MFIs"),
                new ChartItem("EP35", "Liaison between affiliated MFIs"),
                new ChartItem("EP36", "Liaison between branches off network"),
                new ChartItem("EP37", "Internal liaison"),
                new ChartItem("EP38", "Sundry outstanding operations"),
                new ChartItem("EP39", "TREASURY ACCOUNTS"),
                new ChartItem("EP40", "Treasury securities"),
                new ChartItem("EP41", "Money market"),
                new ChartItem("EP42", "Demand and fixed term accounts in the apex body"),
                new ChartItem("EP43", "Demand and fixed term accounts in MFIs affiliated to the network"),
                new ChartItem("EP44", "Demand and fixed term accounts in other MFIs"),
                new ChartItem("EP45", "Demand and fixed term accounts in banks and financial institutions"),
                new ChartItem("EP46", "Other demand and fixed term accounts of correspondents"),
                new ChartItem("EP47", "Overdue claims on correspondents"),
                new ChartItem("EP48", "Cash"),
                new ChartItem("EP49", "RESULT AWAITING APPROVAL"),
                new ChartItem("EP50", "SURPLUS EXPENDITURE OVER REVENUE (1)")
            };
        }

        private List<ChartItem> GetLiabilityItems()
        {
            return new List<ChartItem>
            {
                new ChartItem("RP01", "HERITAGE FUNDS"),
                new ChartItem("RP02", "Subscribed capital called and endowment fund"),
                new ChartItem("RP03", "Subscribed partnership capital called unpaid"),
                new ChartItem("RP04", "Legal reserves"),
                new ChartItem("RP05", "Free reserves"),
                new ChartItem("RP06", "Required regulatory reserve"),
                new ChartItem("RP07", "Solidarity fund"),
                new ChartItem("RP08", "Other reserve funds"),
                new ChartItem("RP09", "Regulatory provisions and reserves"),
                new ChartItem("RP10", "Investment grant"),
                new ChartItem("RP11", "Carry forward"),
                new ChartItem("RP12", "Provisions for general risks"),
                new ChartItem("RP13", "Net income after certification"),
                new ChartItem("RP14", "PROVISIONS FOR RISKS AND COSTS"),
                new ChartItem("RP15", "BORROWINGS OFF NETWORK"),
                new ChartItem("RP16", "BORROWING FROM THE NETWORK"),
                new ChartItem("RP17", "L.T. borrowing"),
                new ChartItem("RP18", "M.T. borrowing"),
                new ChartItem("RP19", "DEPOSITS OF MEMBERS"),
                new ChartItem("RP20", "Special regime deposits"),
                new ChartItem("RP21", "Term deposits of affiliated MFIs"),
                new ChartItem("RP22", "Demand depsosits of affiliated MFIs"),
                new ChartItem("RP23", "Other deposits"),
                new ChartItem("RP24", "SHORT TERM DEBTS"),
                new ChartItem("RP25", "Suppliers"),
                new ChartItem("RP26", "Staff"),
                new ChartItem("RP27", "State"),
                new ChartItem("RP28", "Members"),
                new ChartItem("RP29", "Sundry creditors"),
                new ChartItem("RP30", "CASH RECEIPT ACCOUNTS"),
                new ChartItem("RP31", "ACCRUALS"),
                new ChartItem("RP32", "Costs payable"),
                new ChartItem("RP33", "Revenue recorded in advance"),
                new ChartItem("RP34", "Other adjustment operations"),
                new ChartItem("RP35", "RECIPROCAL ACCOUNTS"),
                new ChartItem("RP36", "Liaison headquarters and branches"),
                new ChartItem("RP37", "Liaison apex body and affiliated MFIs"),
                new ChartItem("RP38", "Liaison between affiliated MFIs"),
                new ChartItem("RP39", "Liaison between branches off network"),
                new ChartItem("RP40", "Internal liaison"),
                new ChartItem("RP41", "TREASURY ACCOUNTS"),
                new ChartItem("RP42", "Treasury securities"),
                new ChartItem("RP43", "Money market"),
                new ChartItem("RP44", "Demand and fixed term accounts of apex body"),
                new ChartItem("RP45", "Demand and fixed term accounts of MFIs affliated to network"),
                new ChartItem("RP46", "Demand and fixed term accounts of other MFIs"),
                new ChartItem("RP47", "Demand and fixed term accounts of banks and financial institutions"),
                new ChartItem("RP48", "Other demand and fixed term accounts of correspondents"),
                new ChartItem("RP49", "RESULT PENDING APPROVAL"),
                new ChartItem("RP50", "SURPLUS OF REVENUE OVER EXPENDITURE (1)")
            };
        }

        private List<ChartItem> GetExpenditureItems()
        {
            return new List<ChartItem>
            {
                new ChartItem("ZP01", "Operating costs"),
                new ChartItem("ZP02", "Interest on treasury and inter-bank operations"),
                new ChartItem("ZP03", "Interest on demand deposits of members"),
                new ChartItem("ZP04", "Interest on term deposits of members"),
                new ChartItem("ZP05", "Interest on loans"),
                new ChartItem("ZP06", "Commissions and expenses on fund transfer operations"),
                new ChartItem("ZP07", "Other bank commissions and expenses"),
                new ChartItem("ZP08", "Expenses linked to incidental operations"),
                new ChartItem("ZP09", "Personnel expenses"),
                new ChartItem("ZP10", "Personnel expenses"),
                new ChartItem("ZP11", "Fringe benefits"),
                new ChartItem("ZP12", "Other general operating costs"),
                new ChartItem("ZP13", "Office supplies"),
                new ChartItem("ZP14", "Water, electricity, gas and fuel"),
                new ChartItem("ZP15", "Rentals"),
                new ChartItem("ZP16", "Maintenance and repairs"),
                new ChartItem("ZP17", "Insurance premiums"),
                new ChartItem("ZP18", "Advertising, public relations and receptions"),
                new ChartItem("ZP19", "Transports and travels"),
                new ChartItem("ZP20", "Telecommunication"),
                new ChartItem("ZP21", "Training"),
                new ChartItem("ZP22", "Missions"),
                new ChartItem("ZP23", "Contributions to the professional association of MFIs"),
                new ChartItem("ZP24", "Board Meetings, General Meetings and session allowances"),
                new ChartItem("ZP25", "Other expenses incurred"),
                new ChartItem("ZP26", "Contribution to apex body"),
                new ChartItem("ZP27", "Taxes and dues"),
                new ChartItem("ZP28", "Amortizations and Provisions"),
                new ChartItem("ZP29", "Depreciation expenses"),
                new ChartItem("ZP30", "Provisions for customer claims"),
                new ChartItem("ZP31", "Other provisions"),
                new ChartItem("ZP32", "Exceptional expenses"),
                new ChartItem("ZP33", "Book value of assets transferred"),
                new ChartItem("ZP34", "Other exceptional expenses"),
                new ChartItem("ZP35", "Net income after certification")
            };
        }

        private List<ChartItem> GetIncomeItems()
        {
            return new List<ChartItem>
            {
                new ChartItem("LP01", "Financial operation proceeds"),
                new ChartItem("LP02", "Interest on treasury and inter-bank operations"),
                new ChartItem("LP03", "Interest on L.T. credits of members"),
                new ChartItem("LP04", "Interest on M.T. credits of members"),
                new ChartItem("LP05", "Interest on S.T credits of members"),
                new ChartItem("LP06", "Interest on debtor accounts of members"),
                new ChartItem("LP07", "Interest on loans"),
                new ChartItem("LP08", "Commissions and costs collected on fund transfer operations"),
                new ChartItem("LP09", "Other bank commissions and proceeds"),
                new ChartItem("LP10", "Proceeds from incidental operations"),
                new ChartItem("LP11", "Other proceeds and grants"),
                new ChartItem("LP12", "Membership fees"),
                new ChartItem("LP13", "Operating and balancing subsidy"),
                new ChartItem("LP14", "Sundry proceeds"),
                new ChartItem("LP15", "Exceptional proceeds"),
                new ChartItem("LP16", "Transfer of investment grant"),
                new ChartItem("LP17", "Proceeds from assets transfers"),
                new ChartItem("LP18", "Other exceptional proceeds"),
                new ChartItem("LP19", "Transfers, Amortizations and Provisions"),
                new ChartItem("LP20", "Amortization transfers"),
                new ChartItem("LP21", "Transfers of provisions for customer claims"),
                new ChartItem("LP22", "Other transfers of provisions"),
                new ChartItem("LP23", "Losses of the year")
            };
        }

        private string GetPatternForSubSection(string mainSection, string subSectionName)
        {
            var patterns = new Dictionary<string, Dictionary<string, string>>
            {
                ["assets"] = new Dictionary<string, string>
                {
                    ["CAPITAL"] = @"^(EP0[1-8]|EP49|EP50)$",
                    ["CREDITS"] = @"^(EP09|EP1[0-6])$",
                    ["GOODS INVENTORY"] = @"^EP17$",
                    ["THIRD PARTY"] = @"^EP1[8-9]|EP2[0-4]$",
                    ["CASH RECEIPTS"] = @"^EP2[5-7]$",
                    ["ACCRUALS"] = @"^EP2[8-9]|EP3[0-1]$",
                    ["RECIPROCAL"] = @"^EP3[2-8]$",
                    ["TREASURY"] = @"^EP3[9-9]|EP4[0-8]$"
                },
                ["liabilities"] = new Dictionary<string, string>
                {
                    ["HERITAGE FUNDS"] = @"^RP0[1-9]|RP1[0-3]$",
                    ["PROVISIONS & RISKS"] = @"^RP14$",
                    ["BORROWINGS"] = @"^RP1[5-8]$",
                    ["DEPOSITS"] = @"^RP1[9-9]|RP2[0-3]$",
                    ["SHORT TERM DEBTS"] = @"^RP2[4-9]$",
                    ["CASH RECEIPT"] = @"^RP30$",
                    ["ACCRUALS"] = @"^RP3[1-4]$",
                    ["RECIPROCAL"] = @"^RP3[5-9]|RP40$",
                    ["TREASURY"] = @"^RP4[1-8]$",
                    ["RESULTS"] = @"^RP49|RP50$"
                },
                ["expenditure"] = new Dictionary<string, string>
                {
                    ["FINANCIAL OPERATING"] = @"^ZP0[1-8]$",
                    ["PERSONNEL"] = @"^ZP0[9-9]|ZP1[0-1]$",
                    ["GENERAL OPERATING"] = @"^ZP1[2-9]|ZP2[0-6]$",
                    ["TAXES & DUES"] = @"^ZP27$",
                    ["AMORT & PROV"] = @"^ZP2[8-9]|ZP3[0-1]$",
                    ["EXCEPTIONAL"] = @"^ZP3[2-4]$",
                    ["NET INCOME"] = @"^ZP35$"
                },
                ["income"] = new Dictionary<string, string>
                {
                    ["FINANCIAL PROCEEDS"] = @"^LP0[1-9]|LP10$",
                    ["OTHER PROCEEDS & GRANTS"] = @"^LP1[1-4]$",
                    ["EXCEPTIONAL"] = @"^LP1[5-8]$",
                    ["TRANSFERS & PROV"] = @"^LP1[9-9]|LP2[0-2]$",
                    ["LOSSES"] = @"^LP23$"
                }
            };

            if (patterns.ContainsKey(mainSection?.ToLower()) &&
                patterns[mainSection.ToLower()].ContainsKey(subSectionName))
            {
                return patterns[mainSection.ToLower()][subSectionName];
            }

            return null;
        }

        #endregion
    }

    // Supporting classes
    public class ChartItem
    {
        public string Ref { get; set; }
        public string Header { get; set; }

        public ChartItem(string refCode, string header)
        {
            Ref = refCode;
            Header = header;
        }
    }
}