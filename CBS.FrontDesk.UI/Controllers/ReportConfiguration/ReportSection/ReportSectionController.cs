using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection;
using CBS.FrontDesk.Data.Message;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportConfiguration
{
	public class ReportSectionController : BaseController
	{
		// GET: ReportSection
		private readonly ReportSectionService _services;
		private readonly ReportDefinitionService _definitionService;

		public ReportSectionController(ReportSectionService services, ReportDefinitionService definitionService)
		{
			_services = services;
			_definitionService = definitionService;
		}

		// GET: ReportSectiont
		public ActionResult Index()
		{
			return View(new ReportSection());
		}

		public async Task<ActionResult> Initilization(string path = "list", string partialView = null, string KEY = null)
		{
			// par défaut, s'il n'y a pas de vue partielle fournie, on charge _List
			partialView = partialView ?? (path == "list" ? "_List" : "_Create");

			switch (path)
			{
				case "list":
					var list = await _services.GetAll();
					return PartialView(partialView, list);

				case "new":
					var model = new ReportSection();
					ViewBag.ReportDefinitions = await _definitionService.GetAll();
					return PartialView($"~/Views/ReportConfiguration/ReportSection/{partialView}.cshtml", model);

				case "edit":
					if (string.IsNullOrEmpty(KEY))
						return new HttpStatusCodeResult(400, "Invalid report key");

					var report = await _services.GetById(KEY);
					if (report == null)
						return HttpNotFound("Report not found");

					ViewBag.ReportDefinitions = await _definitionService.GetAll();
					return PartialView($"~/Views/ReportConfiguration/ReportSection/{partialView}.cshtml", report);

				default:
					var all = await _services.GetAll();
					return PartialView("_List", all);
			}
		}

		public async Task<ActionResult> ReloadList()
		{
			var list = await _services.GetAll();
			return PartialView("~/Views/ReportConfiguration/ReportSection/_List.cshtml", list);
		}

		[HttpPost]
		public async Task<ActionResult> Create(ReportSection model)
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
				var data = await _services.Create(model);

				return Json(new
				{
					success = data.Result,
					status = data.MessageStatus,
					message = Messaging.MessageResult(data),
					reloadDataView = "Yes",
					controllerName = "ReportSection",
					option = "List",
					divLoaderList = "sectionContainer",
					tableName = "myDataTable_section",
					dataLoaderActionName = "ReloadList"
				});

			}
			else
			{
				// If the Id is not null, it's an update, so call the Update method
				return await Update(model);
			}
		}

		[HttpPost]
		public async Task<ActionResult> Update(ReportSection model)
		{
			var data = await _services.Update(model);
			return Json(new
			{
				success = data.Result,
				status = data.MessageStatus,
				message = Messaging.MessageResult(data),
				reloadDataView = "Yes",
				controllerName = "ReportSection",
				option = "List",
				divLoaderList = "sectionContainer",
				tableName = "myDataTable_section",
				dataLoaderActionName = "ReloadList"
			});

		}

		public async Task<ActionResult> Delete(string KEY)
		{
			var data = await _services.Delete(KEY);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> GetReportSection(string Key)
		{
			var data = await _services.GetById(Key);
			return Json(data, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> GetReportSectionPartialView(string Key)
		{
			var data = await _services.GetById(Key);
			if (data == null)
			{
				return HttpNotFound();
			}

			return PartialView("_ReportSectionDetailsPartial", data);
		}
	}
}