using CBS.BusinessService.AccountingV2.ReportingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ReportConfiguration
{
    public class ReportConfigurationController : Controller
    {
        // GET: ReportingV2
        private readonly ReportDefinitionService _reportDefinitionService;
        private readonly ReportSectionService _reportSectionService;
        private readonly ReportLineService _reportLineService;
        private readonly ReportLineMappingService _reportLineMappingService;
		public ReportConfigurationController(ReportDefinitionService reportDefinitionService, ReportSectionService reportSectionService, ReportLineService reportLineService, ReportLineMappingService reportLineMappingService)
		{
			_reportDefinitionService = reportDefinitionService;
			_reportSectionService = reportSectionService;
			_reportLineService = reportLineService;
			_reportLineMappingService = reportLineMappingService;
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
                    var sections = await _reportSectionService.GetAll();
                    return PartialView($"~/Views/{partialView}.cshtml", sections);

                case "line":
                    var lines = await _reportLineService.GetAll();
					return PartialView($"~/Views/{partialView}.cshtml", lines);

                case "line_mapping":
                    var lineMappings = await _reportLineMappingService.GetAll();
                    return PartialView($"~/Views/{partialView}.cshtml", lineMappings);

                default:
                    return PartialView($"~/Views/{partialView}.cshtml");
            }
        }


    }
}