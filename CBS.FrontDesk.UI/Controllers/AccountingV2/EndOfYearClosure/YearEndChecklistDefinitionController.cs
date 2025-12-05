using CBS.BusinessService.AccountingV2.EndOfYearClosure;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure;
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

        [HttpPost]
        public async Task<JsonResult> LoadYearEndChecklistDefinition(YearEndChecklistStatusQuery query)
        {
            try
            {
                var data = await _YearEndChecklistDefinitionService.GetYearEndChecklistDefinitionDataTableAsync(query);

                // Deserialize DataTable payload into strongly-typed list
                var EndChecklist = JsonConvert.DeserializeObject<List<Data.Entity.AccountingV2.EndOfYearClosure.EndOfYearTask>>(
                    JsonConvert.SerializeObject(data.data));

                return Json(new
                {

                    //draw = data.Options.draw ?? "1",
                    //recordsTotal = data.Options.recordsTotal,
                    //recordsFiltered = data.Options.recordsFiltered,
                    data = EndChecklist,
                    success = true,
                    message = "Display DataTable for End of Year check Definition status   successfully"
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
    }
}