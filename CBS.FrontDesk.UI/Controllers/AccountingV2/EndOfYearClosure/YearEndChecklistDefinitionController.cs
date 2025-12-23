using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AccountingV2.EndOfYearClosure
{
    public class YearEndChecklistDefinitionController : Controller
    {

        private readonly BranchServices _branchServices;
        private readonly YearEndChecklistDefinitionService _YearEndChecklistDefinitionService;



        public YearEndChecklistDefinitionController(BranchServices branchServices, YearEndChecklistDefinitionService yearEndChecklistDefinitionService)
        {

            _branchServices = branchServices;
            _YearEndChecklistDefinitionService = yearEndChecklistDefinitionService;


        }
        // GET: YearEndChecklistDefinition
        public async Task<ActionResult> Index()
        {
            await loader();
            return View();
        }
        public async Task<bool> loader()
        {

            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            return true;
        }

        [HttpGet]
        public async Task<ActionResult> New()
        {
            await loader();
            return PartialView("_EndofYearConfiguration", new EndOfYearConfig());
        }

        [HttpPost]
        public async Task<JsonResult> LoadYearEndChecklistDefinition(YearEndChecklistStatusQuery query)
        {
            try
            {
                var data = await _YearEndChecklistDefinitionService.GetYearEndChecklistDefinitionDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var EndChecklist = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.EndOfYearClosure.EndOfYearConfig>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    draw = data.Options.draw ?? "1",
                    recordsTotal = data.Options.recordsTotal,
                    recordsFiltered = data.Options.recordsFiltered,
                    data = EndChecklist,
                    success = true
                    
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return DataTables-compatible empty result on error
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
        public async Task<ActionResult> GetDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(400, "ID is required");

            await loader();
            try
            {
                var entry = await _YearEndChecklistDefinitionService.GetEndofYearConfig(id);
                if (entry == null)
                    return HttpNotFound("accounting year not found");

               
                // Return the partial view that will be injected into the modal
                return PartialView("_EndofYearConfiguration", entry);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }




        public async Task<ActionResult> CreateOrUpdate(EndOfYearConfig model)
        {
           
            if (model == null)
                return Json(new { success = false, message = "Invalid or empty model." });

            try
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    // No Id → create new record
                    var execMessage = await _YearEndChecklistDefinitionService.Create(model);

                    if (execMessage == null)
                        return Json(new { success = false, message = "No response from service." });

                    if (!execMessage.Result)
                        return Json(new
                        {
                            success = false,
                            message = execMessage.MessageString ?? "Failed to save Accounting Year.",
                            data = execMessage.Data
                        });

                    return Json(new
                    {
                        success = true,
                        message = execMessage.MessageString ?? "Accounting Year created successfully.",
                        data = execMessage.Data
                    });
                }
                else
                {
                    // Id present → update existing record
                    var result = await _YearEndChecklistDefinitionService.UpdateAsync(model);

                    if (result == null)
                        return Json(new { success = false, message = "No response from service." });

                    return Json(new
                    {
                        success = result.Result,
                        message = Messaging.MessageResult(result),
                        data = result.Data
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }



       

    }
}