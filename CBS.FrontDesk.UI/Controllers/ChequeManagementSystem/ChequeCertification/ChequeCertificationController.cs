using CBS.BusinessService.CheckManagementSystem.ChequeCertification;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeCertification;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem
{
    public class ChequeCertificationController : Controller
    {
        private readonly ChequeCertificationService _certService;
        private readonly MockChequeCertificationService _mockChequeCertificationService;

        public ChequeCertificationController(ChequeCertificationService certService, MockChequeCertificationService mockChequeCertificationService)
        {
            _certService = certService;
            _mockChequeCertificationService = mockChequeCertificationService;
        }

        // Index
        public ActionResult Index()
        {
            return View();
        }

        // Create
        [HttpPost]
        public async Task<ActionResult> Create(ChequeCertificationDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await _certService.CreateAsync(model);
                return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
            }
            return Json(new { success = false, status = false, message = "Invalid model." });
        }

        // Generic initializer (list or form)
        [HttpGet]
        public async Task<ActionResult> InitializeData(
            string chequeCertificationID = null,
            string partialView = null,
            string path = null,
            string serviceOption = null)
        {
            // ROUTE 1: return summary list
            if (path == "list")
            {
                var data = await _mockChequeCertificationService.GetAllAsync();
                return PartialView(partialView ?? "_ChequeCertificationDataTable", data);
            }

            // ROUTE 2: load data for the form / details
            ChequeCertificationDto model = null;

            if (!string.IsNullOrWhiteSpace(serviceOption))
            {
                InitOptions options = null;
                try { options = JsonConvert.DeserializeObject<InitOptions>(serviceOption); }
                catch { options = new InitOptions(); }

                // Load by ChequeCertificationID when provided
                if (!string.IsNullOrWhiteSpace(chequeCertificationID))
                {
                    model = await _mockChequeCertificationService.GetByIdAsync(chequeCertificationID);
                }

                // fallback if model not found
                if (model == null)
                {
                    model = new ChequeCertificationDto
                    {
                        ChequeCertificationID = chequeCertificationID,
                        BranchId = options?.branchId,
                        //CreatedDate = DateTime.Now,
                        CertificationStatus = "Pending"
                    };
                }
            }

            if (model == null) model = new ChequeCertificationDto();

            // Return the form partial (not the data table) for form route
            return PartialView(partialView ?? "_ChequeCertificationForm", model);
        }

        // GetById - returns JSON (if you need an API-style lookup)
        [HttpGet]
        public async Task<ActionResult> GetById(string chequeCertificationID)
        {
            var data = await _mockChequeCertificationService.GetByIdAsync(chequeCertificationID);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Details - returns a view (or partial) with the DTO
        [HttpGet]
        public async Task<ActionResult> Details(string chequeCertificationID)
        {
            var data = await _mockChequeCertificationService.GetByIdAsync(chequeCertificationID);
             return PartialView("Details", data); // optionally partial
           // return View(data);
        }

        // The following action methods (Review, Validate, Reject, Delete) - use the ChequeCertificationID parameter
        //[HttpPost]
        //public async Task<ActionResult> Review(string chequeCertificationID)
        //{
        //    var result = await _certService.ReviewAsync(chequeCertificationID);
        //    return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        //}

        //[HttpPost]
        //public async Task<ActionResult> Validate(string chequeCertificationID)
        //{
        //    var result = await _certService.ValidateAsync(chequeCertificationID);
        //    return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        //}

        //[HttpPost]
        //public async Task<ActionResult> Reject(string chequeCertificationID)
        //{
        //    var result = await _certService.RejectAsync(chequeCertificationID);
        //    return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        //}

        [HttpPost]
        public async Task<ActionResult> Delete(string chequeCertificationID)
        {
            var result = await _mockChequeCertificationService.DeleteAsync(chequeCertificationID);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}
