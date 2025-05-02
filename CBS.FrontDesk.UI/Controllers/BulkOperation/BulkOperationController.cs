using CBS.BusinessService;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.BulkOPerations;
using CBS.FrontDesk.Data.Entity.LoanConf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.BulkOperations
{

    [CheckSessionTimeOutAttribute]
    public class BulkOperationController : BaseController
    {
         private readonly BranchServices _branchServices;
         private readonly BulkOperationService _bulkOperationService;



        public BulkOperationController(BranchServices branchServices, BulkOperationService bulkOperationService)
        {
            _branchServices = branchServices;
            _bulkOperationService = bulkOperationService;
        }

        // GET: BulkOperation
        public async Task<ActionResult> Index()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }


        public async Task<ActionResult> LoadBulkOperationData(string searchCriteria, string dateFrom= null, string dateTo=null, string operationStatus = "Pending", string branchid = null)
        {

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrWhiteSpace(dateFrom))
            {
                startDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", null);
            }

            if (!string.IsNullOrWhiteSpace(dateTo))
            {
                endDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);
            }


            var query = new GetBulkOperationDataTableQuery
            {
                DataTableOptions = PostDataTableOptions(),
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MinValue,
                BranchId = branchid,
                Status = operationStatus,
            };

            var dataTable = await _bulkOperationService.GetBulkOperationDataTableAsync(query, searchCriteria);
            var loanList = JsonConvert.DeserializeObject<List<MemberAccountsBulkOperations>>(JsonConvert.SerializeObject(dataTable.data));

            return Json(new
            {
                draw = query.DataTableOptions.draw,
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = loanList
            }, JsonRequestBehavior.AllowGet);
        }

    }
}