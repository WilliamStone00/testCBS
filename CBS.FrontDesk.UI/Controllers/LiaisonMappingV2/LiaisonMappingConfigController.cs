using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.AccountingV2.LiaisonMappingV2;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMappingV2;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.LiaisonMappingV2
{
	public class LiaisonMappingConfigController : BaseController
	{

		// GET: LiaisonMapping
		private readonly LiaisonMappingServices _services;
		private readonly BranchServices _branchServices;
		private readonly BranchAccountService _branchAccountService;

		public LiaisonMappingConfigController(LiaisonMappingServices services, BranchServices branchServices = null, BranchAccountService branchAccountService = null)
		{
			_services = services;
			_branchServices = branchServices;
			_branchAccountService = branchAccountService;
		}

		public async Task<ActionResult> Index()
		{
			await GetValues();
			return View(new LiaisonMapping());
		}
		public ActionResult Initilization()
		{
			return View(new LiaisonMapping());
		}
		[HttpPost]
		public async Task<ActionResult> Create(LiaisonMapping model)
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
		public async Task<ActionResult> Update(LiaisonMapping model)
		{
			var data = await _services.Update(model);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
		}

		//private async Task GetValues()
		//{
		//	var branches = await _branchServices.GetBranches();
		//          var CounterpartyBranches = await _branchServices.GetCounterpartyBranches();
		//          var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(null);
		//	var newlistng = _branchAccountService.DropDownGen(accounts.ToList());
		//	ViewBag.Branches = branches;
		//          ViewBag.CounterpartyBranches = CounterpartyBranches;
		//          ViewBag.Accounts = newlistng;
		//}
		//public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
		//{
		//	if (path == "list")
		//	{
		//		var data = await _services.GetAll();
		//		return PartialView(partialView, data);
		//	}

		//	else if (path == "new")
		//	{
		//		await GetValues();
		//		return PartialView(partialView, new LiaisonMapping());
		//	}
		//	else
		//	{
		//		await GetValues();
		//		ViewBag.Key = KEY;
		//		var LiaisonMapping = await _services.GetById(KEY);
		//		return PartialView(partialView, LiaisonMapping);
		//	}
		//}

		private async Task GetValues(string branchId = null)
		{
			var branches = await _branchServices.GetBranches();
			var counterpartyBranches = await _branchServices.GetCounterpartyBranches();

			// Si branchId est fourni, charger uniquement les comptes de cette branche
			var accounts = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);
			var accountsList = _branchAccountService.DropDownGen(accounts.ToList());

			ViewBag.Branches = branches;
			ViewBag.CounterpartyBranches = counterpartyBranches;
			ViewBag.Accounts = accountsList;
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
				// Nouveau mapping : charger toutes les branches et tous les comptes
				await GetValues();
				return PartialView(partialView, new LiaisonMapping());
			}
			else
			{
				// Édition : récupérer l'enregistrement existant
				var liaisonMapping = await _services.GetById(KEY);
				if (liaisonMapping == null)
					return HttpNotFound();

				// Charger uniquement les comptes de la branche sélectionnée
				await GetValues(liaisonMapping.BranchId);

				ViewBag.Key = KEY;
				return PartialView(partialView, liaisonMapping);
			}
		}

		public async Task<ActionResult> Delete(string KEY)
		{
			var data = await _services.Delete(KEY);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> GetLiaisonMapping(string Key)
		{
			var data = await _services.GetById(Key);
			return Json(data, JsonRequestBehavior.AllowGet);
		}




		public async Task<ActionResult> GetLiaisonMappingPartialView(string Key)
		{
			var data = await _services.GetById(Key);
			if (data == null)
			{
				return HttpNotFound();
			}

			return PartialView("_LiaisonMappingDetailsPartial", data);
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
				var listing = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(Key);
				var newlistng = _branchAccountService.DropDownGen(listing.ToList());
				return Json(newlistng, JsonRequestBehavior.AllowGet);
			}
			return Json(null, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public async Task<ActionResult> List()
		{
			await GetValues();
			return View();
		}

		// Note: use [FromBody] so model binder reads the JSON DataTables sends.
		[HttpPost]
		public async Task<JsonResult> LoadLiaisonMappingData(LiaisonMappingQuery query)
		{
			try
			{
				var data = await _services.GetLiaisonMappingDataTable(query);

				var chequeBookscat = JsonConvert.DeserializeObject<List<LiaisonMapping>>(JsonConvert.SerializeObject(data.data));

				return Json(new
				{
					draw = data.draw,
					recordsTotal = data.recordsTotal,
					recordsFiltered = data.recordsFiltered,
					data = chequeBookscat
				});
			}
			catch (Exception ex)
			{
				// return a DataTables-compatible empty result on error
				return Json(new
				{
					draw = query?.Options?.draw ?? "1",
					recordsTotal = 0,
					recordsFiltered = 0,
					data = new List<object>(),
					error = ex.Message
				});
			}
		}
	}
}