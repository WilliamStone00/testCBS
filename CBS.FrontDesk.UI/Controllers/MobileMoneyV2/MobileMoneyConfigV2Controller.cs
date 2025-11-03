using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.MobileMoneyV2;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.MobileMoneyV2;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.MobileMoneyV2
{

	//[CheckSessionTimeOutAttribute]

	public class MobileMoneyConfigV2Controller : BaseController
	{
		// GET: MobileMoney
		private readonly MobileMoneyServices _services;
		private readonly BranchServices _branchServices;
		private readonly BranchAccountService _branchAccountService;


		public MobileMoneyConfigV2Controller(MobileMoneyServices services, BranchServices branchServices = null, BranchAccountService branchAccountService = null)
		{
			_services = services;
			_branchServices = branchServices;
			_branchAccountService = branchAccountService;
		}

		public async Task<ActionResult> Index()
		{
			await GetValues();
			return View(new MobileMoney());
		}
		public ActionResult Initilization()
		{
			return View(new MobileMoney());
		}
		[HttpPost]
		public async Task<ActionResult> Create(MobileMoney model)
		{

			// Validate the model state
			if (!ModelState.IsValid)
			{
				// If model validation fails, return validation errors as JSON response
				return Json(new { success = false, message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
			}

			// If the Id is null, it's a new holiday entry, so call the Create service
			if (model.Id == null)
			{
				// Adding Created Date
				model.CreatedDate = BaseUtilities.UtcToLocal();

				var data = await _services.Create(model);
				return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
			}
			else
			{
				// If the Id is not null, it's an update, so call the Update method
				return await Update(model);
			}
		}

		[HttpPost]
		public async Task<ActionResult> Update(MobileMoney model)
		{
			var data = await _services.Update(model);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
		}
	
		private async Task GetValues()
		{
			var branches = await _branchServices.GetBranches();
			var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync("");
			var newlistng = _branchAccountService.DroupDownGen(accounts.ToList());
			ViewBag.Branches = branches;
			ViewBag.Accounts = newlistng;
		}
		public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
		{
			if (path == "list")
			{
				var data = await _services.GetAll();
				return PartialView(partialView, data);
			}

			else if (path == "new")
			{
				await GetValues();
				return PartialView(partialView, new MobileMoney());
			}
			else
			{
				await GetValues();
				ViewBag.Key = KEY;
				var MobileMoney = await _services.GetById(KEY);
				return PartialView(partialView, MobileMoney);

			}
		}
		public async Task<ActionResult> Delete(string KEY)
		{
			var data = await _services.Delete(KEY);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> GetMobileMoney(string Key)
		{
			var data = await _services.GetById(Key);
			return Json(data, JsonRequestBehavior.AllowGet);
		}
		public async Task<ActionResult> GetMobileMoneyPartialView(string Key)
		{
			var data = await _services.GetById(Key);
			if (data == null)
			{
				return HttpNotFound();
			}

			return PartialView("_MobileMoneyDetailsPartial", data);
		}
		public async Task<ActionResult> GetBranch(string Key)
		{
			var branch = await _branchServices.GetBranch(Key);
			return Json(branch, JsonRequestBehavior.AllowGet);
		}
		public async Task<ActionResult> Ajaxloader(string Key, string path)
		{
			if (Key != null)
			{
				var listing =  await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(Key);
				var newlistng=_branchAccountService.DroupDownGen(listing.ToList());
				return Json(newlistng, JsonRequestBehavior.AllowGet);
			}
			return Json(null, JsonRequestBehavior.AllowGet);
		}
	}
}