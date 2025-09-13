using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DDOSControlP
{
    [CheckSessionTimeOutAttribute]

    public class AdvancedSheildController : BaseController
    {
        // GET: AdvancedSheild
        private readonly RateLimitConfigService _services;
        private readonly CountryServices _countryServices;
        public AdvancedSheildController(RateLimitConfigService membersServices, CountryServices countryServices)
        {
            _services = membersServices;
            _countryServices=countryServices;
        }
        public async Task<ActionResult> Index()
        {
            ViewBag.CountriesCode= await _countryServices.GetCountries();
            return View(new RateLimitConfig());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(RateLimitConfig model,
            string WhitelistedHeaders,
            string WhitelistedCidrs,
            string SuspiciousIndicators,
            string ExcludedPaths,
            string ExcludedSubstrings,
            string ExcludedExtensions,
            string BadUserAgents, string AllowedOrigins, string TelemetryHeaderPrefixes, string HighRiskHeaderKeys, string ProtectedPathRefererRequired, string SensitiveHeaderMissingCheckPaths, string SpoofedForwardedForIndicators,string AllowedHeaderValuePattern,string SuspiciousFormKeyPatterns,string BlockedFileExtensions)
        {
            // Helpers
            List<string> ParseList(string input) =>
                input?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(x => x.Trim())
                      .Where(x => !string.IsNullOrWhiteSpace(x))
                      .Distinct()
                      .ToList() ?? new List<string>();

            // Parse form inputs
            model.WhitelistedHeaders = ParseList(WhitelistedHeaders);
            model.WhitelistedCidrs = ParseList(WhitelistedCidrs);
            model.ExcludedPaths = ParseList(ExcludedPaths);
            model.ExcludedSubstrings = ParseList(ExcludedSubstrings);
            model.ExcludedExtensions = ParseList(ExcludedExtensions);
            model.SuspiciousIndicators = ParseList(SuspiciousIndicators);
            model.BadUserAgents = ParseList(BadUserAgents);
            model.AllowedOrigins = ParseList(AllowedOrigins);
            model.TelemetryHeaderPrefixes = ParseList(TelemetryHeaderPrefixes);
            model.HighRiskHeaderKeys = ParseList(HighRiskHeaderKeys);
            model.ProtectedPathRefererRequired = ParseList(ProtectedPathRefererRequired);
            model.SensitiveHeaderMissingCheckPaths = ParseList(SensitiveHeaderMissingCheckPaths);
            model.SpoofedForwardedForIndicators = ParseList(SpoofedForwardedForIndicators);
            model.AllowedHeaderValuePattern = ParseList(AllowedHeaderValuePattern);

            model.SuspiciousFormKeyPatterns = ParseList(SuspiciousFormKeyPatterns);
            model.BlockedFileExtensions = ParseList(BlockedFileExtensions);
            // CIDR validation
            var invalidCidrs = new List<string>();
            foreach (var cidr in model.WhitelistedCidrs)
            {
                var parts = cidr.Split('/');
                if (parts.Length != 2 ||
                    !IPAddress.TryParse(parts[0], out _) ||
                    !int.TryParse(parts[1], out int prefixLength) ||
                    prefixLength < 0 || prefixLength > 32)
                {
                    invalidCidrs.Add(cidr);
                }
            }

            if (invalidCidrs.Any())
            {
                var invalidString = string.Join(", ", invalidCidrs);
                return Json(new
                {
                    success = false,
                    status = false,
                    message = $"The following CIDRs are invalid: {invalidString}"
                });
            }

            Func<Task<ExecutionMessages>> serviceAction = model.Action == "insert"
                ? GetInsertServiceAction(model)
                : GetUpdateServiceAction(model);

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(RateLimitConfig model)
        {
            return () => _services.AddOrUpdateAsync(model);

        }
        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(RateLimitConfig model)
        {
            return () => _services.AddOrUpdateAsync(model);
        }



        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }

        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key)
        {
            if (path == "list")
            {
                return async () =>
                {
                    var data = await _services.GetAllAsync();
                    return PartialView(partialView, data.ToList());

                };
            }
            else if (path == "new")
            {
                return async () => PartialView(partialView, new RateLimitConfig());
            }
            else
            {
                return async () =>
                {


                    ViewBag.CountriesCode= await _countryServices.GetCountries();
                    var data = await _services.GetRateLimitConfig(key);
                    return PartialView(partialView, data);

                };
            }
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.DeleteAsync(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);

        }
    }
}