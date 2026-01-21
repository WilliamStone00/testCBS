using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Message;
using CBS.BusinessService.Accounting_V2.Translations;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using Newtonsoft.Json;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Translations;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.Translation
{

    [CheckSessionTimeOut]
    public class TranslationPlaceholderController : BaseController
    {
        //private readonly MockTranslationPlaceholderService _translationPlaceholderService;
        private readonly TranslationPlaceholderService _translationPlaceholderService;
        private readonly TranslationPlaceholderExportGenerator _exportGenerator;

        public TranslationPlaceholderController(TranslationPlaceholderService TranslationPlaceholderService, TranslationPlaceholderExportGenerator translationPlaceholderExportGenerator, MockTranslationPlaceholderService mockTranslationPlaceholderService)
        {
            // _translationPlaceholderService = mockTranslationPlaceholderService;
            _translationPlaceholderService = TranslationPlaceholderService;
            _exportGenerator = translationPlaceholderExportGenerator;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            await PopulateViewBag();
            return View(new TranslationPlaceholder());
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            await PopulateViewBag();
            return View();
        }

        private async Task PopulateViewBag()
        {
            // Populate View dropdown options
            ViewBag.Views = new List<SelectListItem>
            {
                new SelectListItem { Value = "Global View", Text = "Global View" },
                new SelectListItem { Value = "Main View", Text = "Main View" },
                new SelectListItem { Value = "JAVASCRIPT View", Text = "JAVASCRIPT View" },
                new SelectListItem { Value = "Controller View", Text = "Controller View" }
            };
        }

        [HttpPost]
        public async Task<JsonResult> LoadTranslationPlaceholderData(TranslationPlaceholderQuery query)
        {
            try
            {

                var data = await _translationPlaceholderService.GetTranslationPlaceholdersDataTableAsync(query);

                var placeholders = JsonConvert.DeserializeObject<List<TranslationPlaceholder>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = placeholders
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
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            await PopulateViewBag();

            if (path == "list")
            {
                //var data = await _translationPlaceholderService.GetTranslationPlaceholdersAsync();
                //return PartialView(partialView, data);
            }
            else if (path == "new")
            {
                return PartialView(partialView, new TranslationPlaceholder());
            }
            else if (path == "edit")
            {
                var data = await _translationPlaceholderService.GetTranslationPlaceholderByIdAsync(KEY);
                if (data == null)
                {
                    return PartialView("_NotFound", $"Translation placeholder with ID '{KEY}' not found.");
                }
                return PartialView(partialView ?? "_TranslationPlaceholderForm", data);
            }
            else if (!string.IsNullOrEmpty(KEY))
            {
                var data = await _translationPlaceholderService.GetTranslationPlaceholderByIdAsync(KEY);
                if (data == null)
                {
                    return PartialView("_NotFound", $"Translation placeholder with ID '{KEY}' not found.");
                }
                return PartialView(partialView ?? "_TranslationPlaceholderDetailsPartial", data);
            }

            return PartialView("_Error", "Invalid request parameters.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(TranslationPlaceholder model)
        {
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Validation failed. Please check all required fields." });

                var result = await _translationPlaceholderService.CreateTranslationPlaceholderAsync(model);
                return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
            }
            else
            {
                return await Update(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(TranslationPlaceholder model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Validation failed. Please check all required fields." });

            var result = await _translationPlaceholderService.UpdateTranslationPlaceholderAsync(model);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
                return Json(new { success = false, message = "Invalid ID provided." });

            var result = await _translationPlaceholderService.DeactivateTranslationPlaceholderAsync(KEY);
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }



        // Updated controller method
        [HttpPost]
        public async Task<ActionResult> Export(ExportTranslationPlaceholderRequest request)
        {
            try
            {
                Console.WriteLine($"Export request received: {request?.TableData?.Count ?? 0} items");

                // Validate input
                if (request?.TableData == null || !request.TableData.Any())
                {
                    Console.WriteLine("No data in export request");
                    return Json(new { success = false, message = "No data available to export." });
                }

                Console.WriteLine($"Processing export for {request.TableData.Count} translation placeholders");

                // Get export information
                var exportedBy = !string.IsNullOrEmpty(request.ExportedBy)
                    ? request.ExportedBy
                    : (User?.Identity?.Name ?? "System");

                var exportDate = request.ExportDate != default ? request.ExportDate : DateTime.Now;

                // Generate Excel using the provided table data
                Console.WriteLine("Generating Excel file...");
                var excelBytes = _exportGenerator.GenerateExcel(
                    request.TableData,
                    exportedBy,
                    request.Query?.StartDate,
                    request.Query?.EndDate);

                Console.WriteLine($"Excel generated successfully: {excelBytes.Length} bytes");

                // Return the file
                var fileName = $"TranslationPlaceholders_{exportDate:yyyyMMdd_HHmmss}.xlsx";

                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Export failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = $"Export failed: {ex.Message}"
                });
            }
        }

   
    }
}
