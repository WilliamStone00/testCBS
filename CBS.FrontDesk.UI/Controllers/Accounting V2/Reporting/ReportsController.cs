using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.ReportData;
using CBS.FrontDesk.Data.Entity.DataTable;
using CrystalDecisions.Shared;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.Reporting
{
    //public sealed class ReportsController : BaseController
    //{
    //    private readonly IMediator _mediator;
    //    public ReportsController(IMediator mediator) => _mediator = mediator;


    //    public ActionResult Index()
    //    {
    //        var vm = new ReportsFilterVm
    //        {
    //            Branches = new SelectList(/* load from DB */ new[] { new { Id = "", Name = "All" } }, "Id", "Name")
    //        };
    //        return View(vm);
    //    }


    //    [HttpPost]
    //    public async Task<ActionResult> LoadGlJournals([FromBody] DataTableQuery<ReportsFilterVm> q)
    //    {
    //        var cmd = new GenerateReportCommand
    //        {
    //            Kind = ReportKind.GLJournals,
    //            Filter = Map(q.Filters, q.Options),
    //            AccountNumbers = q.Filters.AccountNumbers,
    //            AccountNumber = q.Filters.AccountNumber
    //        };
    //        var resp = await _mediator.Send(cmd);
    //        return Json(ToDataTables(resp));
    //    }


    //    [HttpPost]
    //    public async Task<ActionResult> LoadTrialBalance([FromBody] DataTableQuery<ReportsFilterVm> q)
    //    {
    //        var cmd = new GenerateReportCommand { Kind = ReportKind.TrialBalance, Filter = Map(q.Filters, q.Options) };
    //        var resp = await _mediator.Send(cmd);
    //        return Json(ToDataTables(resp));
    //    }


    //    [HttpPost]
    //    public async Task<ActionResult> LoadAccountStatement([FromBody] DataTableQuery<ReportsFilterVm> q)
    //    {
    //        var cmd = new GenerateReportCommand
    //        {
    //            Kind = ReportKind.AccountStatement,
    //            Filter = Map(q.Filters, q.Options),
    //            AccountNumbers = q.Filters.AccountNumbers,
    //            AccountNumber = q.Filters.AccountNumber
    //        };
    //        var resp = await _mediator.Send(cmd);
    //        return Json(ToDataTables(resp));
    //    }


    //    // Similar for Temp/EoD/Fin/BS


    //    public async Task<ActionResult> Export(string type, string format, string json)
    //    {
    //        var filters = System.Text.Json.JsonSerializer.Deserialize<ReportsFilterVm>(json);
    //        var cmd = new GenerateReportCommand { Kind = MapKind(type), Filter = Map(filters, null) };
    //        var resp = await _mediator.Send(cmd);
    //        // Use your ReportExportService to stream file
    //        var (bytes, contentType, fileName) = await _exporter.ToCsvAsync(resp.Result, type, HttpContext.RequestAborted);
    //        return File(bytes, contentType, fileName);
    //    }
    //}
}