using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace CBS.FrontDesk.UI.Controllers.BulkOperation
{

    [CheckSessionTimeOutAttribute]
    public class BulkOperationController : BaseController
    {
         private readonly BranchServices _branchServices;
         private readonly BulkOperationService _bulkOperationService;
         private readonly AccountingServices _accountingServices;
        private readonly ChartOfAccountServicesAnnex chartOfAccountServices;
        private string operationType;



        public BulkOperationController(BranchServices branchServices, BulkOperationService bulkOperationService, ChartOfAccountServicesAnnex chartOfAccountServices, AccountingServices accountingEntryServices)
        {
            _branchServices = branchServices;
            _bulkOperationService = bulkOperationService;
            this.chartOfAccountServices = chartOfAccountServices;
            operationType = "INCOME";
            _accountingServices = accountingEntryServices;
        }

        public async Task<ActionResult> Index()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            ViewBag.eventNames = await _accountingServices.GetEventNames(operationType);
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            var savingProduct = await _bulkOperationService.GetSavingProducts();
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


        [HttpPost]
        public async Task<ActionResult> Simulate(SimulateBulkOperation model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            var Branches = await _branchServices.GetBranches();
            model.Branches = Branches.ToList();
            serviceAction = async () => await _bulkOperationService.SimulateOperation(model);

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



        // GET: BulkOperation
        public async Task<ActionResult> Listing()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }

        public async Task<ActionResult> Validation( string stimulationId, string approvalStatus,string description,string approvedBy)
        {

            ConfirmBulkOperationCommand command = new ConfirmBulkOperationCommand
            {
                BulkOperationSimulationId = stimulationId,
                ApprovalStatusDescription=description,
                ApprovalStatus=approvalStatus,
                ApprovalBy=approvedBy
            };
            if (string.IsNullOrEmpty(command.BulkOperationSimulationId))
            {
                return RedirectToAction("Listing", new { error = "Invalid operation ID" });
            }

            Func<Task<ExecutionMessages>> serviceAction = null;

            serviceAction = async () => await _bulkOperationService.ValidateOperation(command);

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

        public async Task<ActionResult> GetBulkOperationDetails(string KEY)
        {

            if (string.IsNullOrEmpty(KEY))
            {
                return RedirectToAction("Listing", new { error = "Invalid operation ID" });
            }

        

            try
            {
                var bulkOperationDetailsData = await _bulkOperationService.GetBulkOperationDetailsById(KEY);
                if (bulkOperationDetailsData == null)
                {
                    return HttpNotFound();
                }

                bulkOperationDetailsData.ApprovalStatusBadge = GetBadge(bulkOperationDetailsData.ApprovalStatus);
                return   PartialView("_TransferDetails", bulkOperationDetailsData);
            }
            catch (Exception ex)
            {
                // Log error
                return View("Error");
            }

        }

        public async Task<ActionResult> Details(string KEY)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return RedirectToAction("Listing", new { error = "Invalid operation ID" });
            }

            try
            {
                var bulkOperationData = await _bulkOperationService.GetBulkOperationById(KEY);
                if (bulkOperationData == null)
                {
                    return HttpNotFound();
                }

                bulkOperationData.ApprovalStatusBadge = GetBadge(bulkOperationData.ApprovalStatus);
                /*   var addDetailStatusBadge = bulkOperationData.BulkOperationSimulationDetails?.Select(x =>
                   {
                       x.ApprovalStatusBadge = GetBadge(x.ApprovalStatus);
                       x.TransferStatusBadge = GetBadge(x.TransferStatus);
                       return x;
                   }).ToList();*/

                // bulkOperationData.BulkOperationSimulationDetails = bulkOperationData.BulkOperationSimulationDetails ?? new List<BulkOperationDataDetails>();
                ViewBag.FullName = _bulkOperationService.GetUserFullName();
                return View(bulkOperationData);
            }
            catch (Exception ex)
            {
                // Log error
                return View("Error");
            }
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



        public async Task<ActionResult> LoadBulkOperationData(string searchCriteria, string dateFrom= null, string dateTo=null, string operationStatus="", string branchid = null)
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
        public async Task<ActionResult> LoadBulkOperationDetailsData(string simulationId)
        {
            try
            {

                var query = new GetAllSimulationDetailBySimulationIdRequestQuery
                {
                    Options = PostDataTableOptions(),
                    simulationId = simulationId
                };

                var data = await _bulkOperationService.GetBulkOperationDetailsDataTableAsync(query);

                return Json(new
                {
                    draw = query.Options.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = data.data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log error
                return Json(new { error = "Error loading data" });
            }
        }

        public async Task<ActionResult> DownloadBulkOperationExcel(string simulationId)
        {
            if (string.IsNullOrEmpty(simulationId))
            {
                return RedirectToAction("Listing", new { error = "Invalid operation ID" });
            }
            try
            {
                var bulkOperationData = await _bulkOperationService.GetBulkOperationById(simulationId);
                if (bulkOperationData == null)
                {
                    return HttpNotFound();
                }

                bulkOperationData.ApprovalStatusBadge = GetBadge(bulkOperationData.ApprovalStatus);
                // Define file path and branch name
                string branchName = Session["BranchName"].ToString();
                string fileName = $"BulkOperation_{bulkOperationData.SimulationType}_{branchName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedDate = DateTime.Now.ToString();
                string exportedBy = Session["FullName"].ToString();

                BulkOperationResultDetailExcelGenerator.GenerateBulkOperationExcel(bulkOperationData, branchName, filePath, exportedDate, exportedBy);

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                System.IO.File.Delete(filePath); // Clean up temporary file

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


            }
            catch (Exception ex)
            {
                // Log error
                // Log the exception and return an error response
                Console.WriteLine($"Error generating Excel file: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while generating the Excel file." }, JsonRequestBehavior.AllowGet);

            }

        }


        public async Task<ActionResult> DeleteBulkOperation(string simulationId,string simulationType)
        {
            if (string.IsNullOrEmpty(simulationId))
            {
                return RedirectToAction("Listing", new { error = "Invalid operation ID" });
            }

            try
            {
                var data = await _bulkOperationService.DeleteBulkOperationById(simulationId, simulationType);

                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            catch (Exception ex)
            {
                // Log error
                // Log the exception and return an error response
                Console.WriteLine($"Error generating Excel file: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while generating the Excel file." }, JsonRequestBehavior.AllowGet);

            }
        }


    }
}