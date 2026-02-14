using CBS.API.Helper;
using CBS.BusinessService.CheckManagementSystem.ChequeClearance;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Operations.ChequeCancelation
{
    public class ChequeCancellationController : BaseController
    {
        private readonly ChequeCancellationService _chequeCancellationService;

        private readonly BranchServices _branchServices;

        public ChequeCancellationController(
            ChequeCancellationService chequeCancellationService,

            BranchServices branchServices)
        {
            _chequeCancellationService = chequeCancellationService;

            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await Load();
            return View(new CancellationRequest());
        }

        private async Task Load()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

        }

        // GET: Load partial views for listing, forms, details




        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(CancellationRequest model)
        {
            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 400,
                    message = "Invalid or empty model.",
                    data = (object)null
                });
            }

            try
            {
                ExecutionMessages response;

                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    response = await _chequeCancellationService.CreateAsync(model);
                }
                else
                {
                    response = await _chequeCancellationService.UpdateAsync(model);
                }

                return Json(new
                {
                    success = response.Result,   // use Result if that's your success flag
                    statusCode = response.Result ? 200 : 400,
                    message = response.MessageString,
                    data = response.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"❌ Error: {ex.Message}",
                    data = (object)null
                });
            }
        }




    }
}
