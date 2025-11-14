using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportingV2
{
    public class ReportingV2Controller : Controller
    {
        // GET: ReportingV2
        private readonly ReportDefinitionService _reportDefinitionService;
		public ReportingV2Controller(ReportDefinitionService reportDefinitionService)
		{
			_reportDefinitionService = reportDefinitionService;
		}
		public ActionResult Index()
        {
            return View();
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            ViewBag.Key = KEY;

            switch (path)
            {
                case "definition":
                    var definitions =await _reportDefinitionService.GetAll();
					var result= PartialView($"~/Views/{partialView}.cshtml", definitions);
                    return result;

                case "section":
                    return PartialView($"~/Views/{partialView}.cshtml", new List<ReportSection>());

                case "line":
                    return PartialView($"~/Views/{partialView}.cshtml", new List<ReportLine>());

                case "line_mapping":
                    return PartialView($"~/Views/{partialView}.cshtml", new List<ReportLineMapping>());

                default:
                    return PartialView($"~/Views/{partialView}.cshtml");
            }
        }


    }
}