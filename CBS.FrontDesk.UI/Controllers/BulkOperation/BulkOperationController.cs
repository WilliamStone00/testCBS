using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.BulkOperation
{

    [CheckSessionTimeOutAttribute]
    public class BulkOperationController : BaseController
    {
         private readonly BranchServices _branchServices;
         private readonly BulkOperationService _bulkOperationService;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;
        private readonly SavingProductServices _savingProductServices;



        public BulkOperationController(BranchServices branchServices, BulkOperationService bulkOperationService, ChartOfAccountServicesAnnex chartOfAccountServices, SavingProductServices savingProductServices)
        {
            _branchServices = branchServices;
            _bulkOperationService = bulkOperationService;
            this.chartOfAccountServices = chartOfAccountServices;
            _savingProductServices = savingProductServices;
        }

        public async Task<ActionResult> Index()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            var savingProduct = await _savingProductServices.GetSavingProducts();
            var savingOrdinaryProduct = savingProduct.Where(x => x.ProductCategory == "OrdinaryAccount").ToList();
            var Branches = await _branchServices.GetBranches();
            return View(new SimulateBulkOperation()
            {
                BulkOperationSelectionModel = new BulkOperationSelectionModel()
                {
                    SelectedOperationType = "specific"
                },
                SavingProducts = savingOrdinaryProduct,
                Branches = Branches.ToList()
            });
        }

        // GET: BulkOperation
        public async Task<ActionResult> Listing()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }

        public async Task<ActionResult> Details(string KEY)
        {
            var bulkOperationData = await _bulkOperationService.GetBulkOperationById(KEY);
            bulkOperationData.ApprovalStatusBadge = GetBadge(bulkOperationData.ApprovalStatus);
            var addDetailStatusBadge = bulkOperationData.BulkOperationSimulationDetails.Select(x =>
            {
                x.ApprovalStatusBadge = GetBadge(x.ApprovalStatus);
                x.TransferStatusBadge = GetBadge(x.TransferStatus);
                return x;
            }).ToList();
            bulkOperationData.BulkOperationSimulationDetails = addDetailStatusBadge;
            return View(bulkOperationData);
        }

        public string GetBadge(string status)
        {
            switch (status)
            {
                case "Pending": return "bg-warning text-dark";
                case "Review": return "bg-info text-white";
                case "Approved": return "bg-success text-white";
                default: return "bg-secondary text-white";
              }

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
            var loanList = JsonConvert.DeserializeObject<List<Data.Entity.BulkOperation.BulkOperationData>>(JsonConvert.SerializeObject(dataTable.data));

            return Json(new
            {
                draw = query.DataTableOptions.draw,
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = loanList
            }, JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public async Task<ActionResult> Simulate(SimulateBulkOperation model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            var Branches = await _branchServices.GetBranches();
            model.Branches=Branches.ToList();
            serviceAction =  async () =>await  _bulkOperationService.SimulateOperation(model);

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

    }
}