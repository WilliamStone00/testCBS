using CBS.BusinessService.CheckManagementSystem.Configurations.GlobalConfiguration;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.GlobalConfig;
using CBS.FrontDesk.Data.Message;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.GlobalConfiguration
{
	public class GlobalConfigController : BaseController
	{
		private readonly GlobalConfigServices _services;

		public GlobalConfigController(GlobalConfigServices services)
		{
			_services = services;
		}

		#region INDEX

		public ActionResult Index()
		{
			return View(new GlobalConfig());
		}

		public ActionResult Initialization()
		{
			return View(new GlobalConfig());
		}

		#endregion

		#region CREATE / UPDATE

		[HttpPost]
		public async Task<ActionResult> Create(GlobalConfig model)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					success = false,
					message = "Validation failed",
					errors = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList()
				});
			}

			// CREATE
			if (string.IsNullOrEmpty(model.Id))
			{
				var data = await _services.Create(model);
				return Json(new
				{
					success = data.Result,
					status = data.MessageStatus,
					message = Messaging.MessageResult(data)
				});
			}
			// UPDATE
			else
			{
				return await Update(model);
			}
		}

		[HttpPost]
		public async Task<ActionResult> Update(GlobalConfig model)
		{
			var data = await _services.Update(model);
			return Json(new
			{
				success = data.Result,
				status = data.MessageStatus,
				message = Messaging.MessageResult(data)
			});
		}

		#endregion

		#region INITIALIZE DATA (AJAX / PARTIAL VIEW)

		public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
		{
			if (path == "list")
			{
				var data = await _services.GetAll();
				return PartialView(partialView, data);
			}
			else if (path == "new")
			{
				return PartialView(partialView, new GlobalConfig());
			}
			else
			{
				ViewBag.Key = KEY;
				var config = await _services.GetById(KEY);
				return PartialView(partialView, config);
			}
		}

		#endregion

		#region DELETE

		public async Task<ActionResult> Delete(string KEY)
		{
			var data = await _services.Delete(KEY);
			return Json(new
			{
				success = data.Result,
				status = data.MessageStatus,
				message = Messaging.MessageResult(data)
			}, JsonRequestBehavior.AllowGet);
		}

		#endregion

		#region GET (AJAX)

		public async Task<ActionResult> GetGlobalConfig(string Key)
		{
			var data = await _services.GetById(Key);
			return Json(data, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> GetGlobalConfigPartialView(string Key)
		{
			var data = await _services.GetById(Key);
			if (data == null)
			{
				return HttpNotFound();
			}

			return PartialView("_GlobalConfigDetailsPartial", data);
		}

		#endregion
	}
}