using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Buffers;
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


        public async Task<ActionResult> BulkCashOperation()
        {
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            ViewBag.eventNames = await _accountingServices.GetEventNames(operationType);
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();

            ViewBag.transferType = new List<StringValues>() { new StringValues { Text = "CashIn", Value = "CashIn" }, new StringValues { Text = "CashOut", Value = "CashOut" }, };
            ViewBag.operationScope = new List<StringValues>() { new StringValues { Text = "Internal", Value = "Internal" }, new StringValues { Text = "InterBranch", Value = "InterBranch" }, };
            var Branches = await _branchServices.GetBranches();
            var savingProduct = await _bulkOperationService.GetSavingProducts();
            var savingOrdinaryProduct = savingProduct.Where(x => x.ProductCategory == "OrdinaryAccount").ToList();
            ViewBag.branches = Branches.ToList();
            ViewBag.savingProducts = savingOrdinaryProduct;
            //var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            return View(new SimulateCashOutOrCashInBulkOperation() { Branches = Branches.ToList() });


        }

        public async Task<ActionResult> DownloadBulkOperationFileTemplate()
        {
            try
            {
                const string fileName = "BulkOperationFileUpload.xlsx";
                string directoryPath = Server.MapPath("~/AppFiles/BulkOperation");

                // Validate directory exists
                if (!Directory.Exists(directoryPath))
                {
                    return Json(new { success = false, status = false, message = "Bulk operation template directory not found" });
                }

                string filePath = Path.Combine(directoryPath, fileName);

                // Validate file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, status = false, message = "Template file not found" });
                }

                // Read file asynchronously
                byte[] fileBytes;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                }

                // Return the file
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, status = false, message = "Access denied to template file" });
            }
            catch (IOException ex)
            {
                return Json(new { success = false, status = false, message = $"Error reading template file: {ex.Message}" });
            }
            catch (Exception ex)
            {

                // Log the exception here
                return Json(new { success = false, status = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }


        public async Task<ActionResult> UploadBulkCashOperationFile(HttpPostedFileBase file)
        {
            try
            {
                // Validation (keep your existing validation code)

                // Process the file
                var result = await _bulkOperationService.ProcessBulkCashOperationFileAsync(file);

                if (result == null  || !result.IsSuccess  || result.ApiResponseData==null)
                {
                    return Json(new
                    {
                        success = false,
                        message = result == null ? "Failed to process file" : result.Message ?? "Failed to process file",
                        error = (result == null || result.ApiResponseData==null) ? null : result.ApiResponseData.Errors // Include any additional error details
                    });
                }


                // Return proper JSON structure
                return Json(new
                {
                    draw = Request.Form["draw"] ?? "1",
                    recordsTotal = result.ApiResponseData.Data?.BulkCashOperationFileDetails?.Count ?? 0,
                    recordsFiltered = result.ApiResponseData.Data?.BulkCashOperationFileDetails?.Count ?? 0,
                    data = result.ApiResponseData.Data?.BulkCashOperationFileDetails ?? new List<BulkCashOperationFileDetails>(),
                    success = true,
                    message = "File processed successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the error
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your file.",
                    error = ex.Message // Only include in development
                });
            }
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



        public async Task<ActionResult> Validation(string stimulationId, string approvalStatus, string description, string approvedBy)
        {

            ConfirmBulkOperationCommand command = new ConfirmBulkOperationCommand
            {
                BulkOperationSimulationId = stimulationId,
                ApprovalStatusDescription = description,
                ApprovalStatus = approvalStatus,
                ApprovalBy = approvedBy
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
                return PartialView("_TransferDetails", bulkOperationDetailsData);
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



        public async Task<ActionResult> LoadBulkOperationData(string searchCriteria, string dateFrom = null, string dateTo = null, string operationStatus = "", string branchid = null)
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


        public async Task<ActionResult> DeleteBulkOperation(string simulationId, string simulationType)
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

        [HttpPost]
        public async Task<ActionResult> SimulateCashOutOrCashInBulkOperation(SimulateCashOutOrCashInBulkOperation simulateCashOutOrCashIn)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data submitted." });
            }


            Func<Task<ExecutionMessages>> serviceAction = null;
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            var eventNames = await _accountingServices.GetEventNames(operationType);
            var Branches = await _branchServices.GetBranches();
            var savingProduct = await _bulkOperationService.GetSavingProducts();

            var currentEvent = chartOfAccounts.FirstOrDefault(x => x.Value == simulateCashOutOrCashIn.AccountCartId);
       

            List<SimulateBulkOperationDetailCommandDto> simulateBulkOperationDetails = new List<SimulateBulkOperationDetailCommandDto>();

            foreach (var data in simulateCashOutOrCashIn.Accounts)
            {
                var branch = Branches.Where(x => x.Id == data.BranchName).FirstOrDefault();

                var currentProduct = savingProduct.FirstOrDefault(x => x.Id == data.AccountType);

                var simulateBulkOperationDetail = new SimulateBulkOperationDetailCommandDto()
                {
                    AccountType = currentProduct.AccountType,
                    Amount = data.Amount,
                    BranchCode = branch?.BranchCode,
                    BranchId = branch?.Id,
                    BranchName = branch?.Name,
                    MemberReference = data.MemberReference
                };

                simulateBulkOperationDetails.Add(simulateBulkOperationDetail);
            }


            var mainBranch = Branches.Where(x => x.Id == simulateCashOutOrCashIn.BranchId).FirstOrDefault();
            string accountingDate = simulateCashOutOrCashIn.AccountDate.ToString("yyyy-MM-dd");

            var request = new SimulateBulkCreditOrDebitOperationCommand()
            {
                AccountChart = currentEvent?.Text,
                AccountChartId = currentEvent?.Value,
                AccountingDate = accountingDate,
                Description = simulateCashOutOrCashIn.SimulationDescription,
                OperationType = simulateCashOutOrCashIn.TransferType,
                SimulationType = simulateCashOutOrCashIn.OperationTitle,
                SimulateBulkOperationDetails = simulateBulkOperationDetails,
                MainBranchCode= mainBranch.BranchCode,
                MainBranchId= mainBranch.Id,
                MainBranchName= mainBranch.Name

            };


            // var obj= JsonConvert.DeserializeObject<List<SimulateCashOutOrCashInBulkOperation>>(JsonConvert.SerializeObject(model));
            // Process the simulation
            serviceAction = async () => await _bulkOperationService.SimulateBulkCreditOrDebitOperation(request);

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

        [HttpPost]
        public async Task<ActionResult> SimulateCashOutOrCashInBulkFileOperation(SimulateCashOutOrCashInBulkOperation simulateCashOutOrCashIn)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data submitted." });
            }


            Func<Task<ExecutionMessages>> serviceAction = null;
            var chartOfAccounts = await chartOfAccountServices.GetChartOfAccounts();
            var eventNames = await _accountingServices.GetEventNames(operationType);
            var Branches = await _branchServices.GetBranches();
            var savingProduct = await _bulkOperationService.GetSavingProducts();

            var currentEvent = chartOfAccounts.FirstOrDefault(x => x.Value == simulateCashOutOrCashIn.AccountCartId);

            List<SimulateBulkOperationDetailCommandDto> simulateBulkOperationDetails = new List<SimulateBulkOperationDetailCommandDto>();

            foreach (var data in simulateCashOutOrCashIn.AccountIIs)
            {
                var branch = Branches.Where(x => x.BranchCode == data.BranchCode).FirstOrDefault();
              //  var currentProduct = savingProduct.FirstOrDefault(x => x.Id == data.AccountType);
                var simulateBulkOperationDetail = new SimulateBulkOperationDetailCommandDto()
                {
                    AccountType = data.AccountType,
                    Amount = data.Amount,
                    BranchCode = branch?.BranchCode,
                    BranchId = branch?.Id,
                    BranchName = branch?.Name,
                    MemberReference = data.MemberReference
                };

                simulateBulkOperationDetails.Add(simulateBulkOperationDetail);
            }

            var mainBranch = Branches.Where(x => x.Id == simulateCashOutOrCashIn.BranchId).FirstOrDefault();
            string accountingDate = simulateCashOutOrCashIn.AccountDate.ToString("yyyy-MM-dd");
            var request = new SimulateBulkCreditOrDebitOperationCommand()
            {
                AccountChart = currentEvent?.Text,
                AccountChartId = currentEvent?.Value,
                AccountingDate = accountingDate,
                Description = simulateCashOutOrCashIn.SimulationDescription,
                OperationType = simulateCashOutOrCashIn.TransferType,
                SimulationType = simulateCashOutOrCashIn.OperationTitle,
                SimulateBulkOperationDetails = simulateBulkOperationDetails,
                MainBranchCode = mainBranch.BranchCode,
                MainBranchId = mainBranch.Id,
                MainBranchName = mainBranch.Name

            };


            // var obj= JsonConvert.DeserializeObject<List<SimulateCashOutOrCashInBulkOperation>>(JsonConvert.SerializeObject(model));
            // Process the simulation
            serviceAction = async () => await _bulkOperationService.SimulateBulkCreditOrDebitOperation(request);

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