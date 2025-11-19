using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportDefinition;
using CBS.FrontDesk.Data.Message;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportConfiguration
{
	public class ReportDefinitionController : BaseController
	{
		private readonly ReportDefinitionService _services;

		public ReportDefinitionController(ReportDefinitionService services)
		{
			_services = services;
		}

		// ✅ Action principale utilisée en AJAX
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
					return PartialView($"~/Views/ReportConfiguration/ReportDefinition/{partialView}.cshtml", new ReportDefinition());

				case "edit":
					if (string.IsNullOrEmpty(KEY))
						return new HttpStatusCodeResult(400, "Invalid report key");

					var report = await _services.GetById(KEY);
					if (report == null)
						return HttpNotFound("Report not found");

					return PartialView($"~/Views/ReportConfiguration/ReportDefinition/{partialView}.cshtml", report);

				default:
					var all = await _services.GetAll();
					return PartialView("_List", all);
			}
		}

		public async Task<ActionResult> ReloadList()
		{
			var list = await _services.GetAll();
			return PartialView("~/Views/ReportConfiguration/ReportDefinition/_List.cshtml", list);
		}


		// ✅ Create / Update
		[HttpPost]
		public async Task<ActionResult> Create(ReportDefinition model)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				return Json(new { success = false, message = "Validation failed", errors });
			}

			if (string.IsNullOrEmpty(model.Id))
			{
				var data = await _services.Create(model);
				return Json(new
				{
					success = data.Result,
					status = data.MessageStatus,
					message = Messaging.MessageResult(data),
					reloadDataView = "Yes",
					controllerName = "ReportDefinition",
					option = "List",
					divLoaderList = "definitionContainer",
					tableName = "myDataTable",
					dataLoaderActionName = "ReloadList"
				});

			}
			else
			{
				return await Update(model);
			}
		}

		[HttpPost]
		public async Task<ActionResult> Update(ReportDefinition model)
		{
			var data = await _services.Update(model);
			return Json(new
			{
				success = data.Result,
				status = data.MessageStatus,
				message = Messaging.MessageResult(data),
				reloadDataView = "Yes",
				controllerName = "ReportDefinition",
				option = "List", 
				divLoaderList = "definitionContainer",
				tableName = "myDataTable",
				dataLoaderActionName = "ReloadList"
			});
		}



		// ✅ Delete
		public async Task<ActionResult> Delete(string KEY)
		{
			if (string.IsNullOrEmpty(KEY))
				return Json(new { success = false, message = "Invalid key" }, JsonRequestBehavior.AllowGet);

			var data = await _services.Delete(KEY);
			return Json(new
			{
				success = data.Result,
				status = data.MessageStatus,
				message = Messaging.MessageResult(data)
			}, JsonRequestBehavior.AllowGet);
		}

		// ✅ Get report by ID (JSON)
		public async Task<ActionResult> GetReportDefinition(string Key)
		{
			if (string.IsNullOrEmpty(Key))
				return Json(null, JsonRequestBehavior.AllowGet);

			var data = await _services.GetById(Key);
			return Json(data, JsonRequestBehavior.AllowGet);
		}

		// ✅ Get partial detail
		public async Task<ActionResult> GetReportDefinitionPartialView(string Key)
		{
			if (string.IsNullOrEmpty(Key))
				return HttpNotFound("Key is missing");

			var data = await _services.GetById(Key);
			if (data == null)
				return HttpNotFound("Report not found");

			return PartialView("_ReportDefinitionDetailsPartial", data);
		}
	}
}
