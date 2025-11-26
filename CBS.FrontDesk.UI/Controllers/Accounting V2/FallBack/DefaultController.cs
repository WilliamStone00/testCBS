using CBS.BusinessService.Accounting_V2.FallBack;
using CBS.FrontDesk.Data.Entity.Accounting_V2.FallBack;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.FallBack
{
    [CheckSessionTimeOut]
    public class AffiliateToBranchFallbackController : Controller
    {
        private readonly AffiliateToBranchFallbackService _fallbackService;

        public AffiliateToBranchFallbackController(AffiliateToBranchFallbackService fallbackService)
        {
            _fallbackService = fallbackService;
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoadFallbackData(FallbackLogQuery query)
        {
            try
            {
                var data = await _fallbackService.GetFallbackLogDataTableAsync(query);

                var fallbackLogs = JsonConvert.DeserializeObject<List<FallbackLogResponse>>(JsonConvert.SerializeObject(data.data));

                // Calculate reconciliation statistics
                int toBeReconciled = 0;
                int alreadyReconciled = 0;
                int total = fallbackLogs?.Count ?? 0;

                if (fallbackLogs != null)
                {
                    foreach (var log in fallbackLogs)
                    {
                        if (log.isResolved)
                            alreadyReconciled++;
                        else
                            toBeReconciled++;
                    }
                }

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = fallbackLogs,
                    statistics = new
                    {
                        toBeReconciled,
                        alreadyReconciled,
                        total
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    statistics = new
                    {
                        toBeReconciled = 0,
                        alreadyReconciled = 0,
                        total = 0
                    },
                    error = ex.Message
                });
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path == "details")
            {
                var data = await _fallbackService.GetByIdAsync(KEY);
                return PartialView(partialView, data);
            }
            else if (path == "reconcile")
            {
                var data = await _fallbackService.GetByIdAsync(KEY);
                var reconcileModel = new ReconcileFallbackRequest
                {
                    Id = data?.id,
                    SourceGlAccountNumber = data?.supposedGlAccountNumber,
                    SourceAmount = data?.amount ?? 0,
                    SourceSit = data?.operationCode,
                    SourceDescription = $"Affiliate: {data?.requestedAffiliateAccountIdOrBranchCode}",

                    DestinationGlAccountNumber = data?.fallbackBranchAccountNumber,
                    DestinationAmount = data?.amount ?? 0,
                    DestinationSit = data?.operationCode,
                    DestinationDescription = $"Fallback: {data?.fallbackBranchAccountName}"
                };
                return PartialView(partialView, reconcileModel);
            }
            else
            {
                return PartialView(partialView, new FallbackLogResponse());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reconcile(ReconcileFallbackRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed." });

            var resolveRequest = new ResolveFallbackRequest
            {
                Id = model.Id,
                SourceGlAccountNumber = model.SourceGlAccountNumber,
                SourceAmount = model.SourceAmount,
                SourceSit = model.SourceSit,
                SourceDescription = model.SourceDescription,
                DestinationGlAccountNumber = model.DestinationGlAccountNumber,
                DestinationAmount = model.DestinationAmount,
                DestinationSit = model.DestinationSit,
                DestinationDescription = model.DestinationDescription
            };

            var result = await _fallbackService.ResolveAsync(resolveRequest);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }
    }
}