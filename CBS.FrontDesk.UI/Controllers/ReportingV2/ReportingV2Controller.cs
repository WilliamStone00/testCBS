using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportDefinition;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLine;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportLineMapping;
using CBS.FrontDesk.Data.Entity.AccountingV2.ReportingV2.ReportSection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.ReportingV2
{
    public class ReportingV2Controller : Controller
    {
        // GET: ReportingV2
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
                    var result= PartialView($"~/Views/{partialView}.cshtml", new List<ReportDefinition>());
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