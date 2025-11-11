using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Message;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportingV2
{
	public class ReportLineMappingController : BaseController
	{
		// GET: ReportLineMapping
		private readonly ReportLineMappingService _services;

		public ReportLineMappingController(ReportLineMappingService services)
		{
			_services = services;
		}

		// GET: ReportLineMappingt
		public ActionResult Index()
		{
			return View(new ReportLineMapping());
		}

		public ActionResult Initilization()
		{
			return View(new ReportLineMapping());
		}

		[HttpPost]
		public async Task<ActionResult> Create(ReportLineMapping model)
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
		public async Task<ActionResult> Update(ReportLineMapping model)
		{
			var data = await _services.Update(model);
			return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
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
				return PartialView(partialView, new ReportLineMapping());
			}
			else
			{
				ViewBag.Key = KEY;
				var ReportLineMapping = await _services.GetById(KEY);
				return PartialView(partialView, ReportLineMapping);

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
	}
}