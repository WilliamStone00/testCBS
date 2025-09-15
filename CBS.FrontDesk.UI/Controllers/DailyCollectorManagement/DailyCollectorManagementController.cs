using CBS.API.Helper;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices;
using CBS.BusinessService.Session;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DailyCollectionData;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
 
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.UI.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.IdentityModel.Tokens;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
// 
namespace CBS.FrontDesk.UI.Controllers
{
    //
    public class DailyAgentManagementController : BaseController
    {
        private BranchServices _branchService;
        private AgentServices _agentServices;
        public AccountingServices _accountServices;
        private UserManagementServices _userServices;
        private AgentAccountServices _agentAccountServices;
        private DailyCollectionMigrationServices _dailyCollectionMigrationServices;
        private DailyCollectionEndOfDayServices _dailyCollectionEndOfDayServices;
        public DailyAgentManagementController()
        {
            _branchService = new BranchServices();
            _agentServices = new AgentServices();
            _userServices = new UserManagementServices();
            _agentAccountServices = new AgentAccountServices();
            _dailyCollectionMigrationServices = new DailyCollectionMigrationServices();
            _accountServices = new AccountingServices();
            _dailyCollectionEndOfDayServices = new DailyCollectionEndOfDayServices();
        }
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new DailyAgentManagement());
        }
        public async Task<ActionResult> CommissionPayment()
        {
            await GetList();
            return View(new DailyAgentManagement());
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }
   
        private async Task<ActionResult> GetServiceAction(string path, string partialView, string KEY, string serviceOption)
        {
            if (serviceOption == "agent")
            {
                if (path == "list")
                {
                    var data = await _agentServices.GetAgents();

                    var sysData = new DailyAgentManagement { Agents = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {


                    return PartialView(partialView, new DailyAgentManagement { Agent = new Data.Entity.DailyCollectionData.Agent() });

                }
                else
                {

                    var data = await _agentServices.GetAgentById(KEY);
                    return PartialView(partialView, new DailyAgentManagement { Agent = data });

                }


            }
            else
            {
                if (path == "list")
                {
                    var data = await _agentAccountServices.GetAgentAccounts();

                    var sysData = new DailyAgentManagement { AgentAccounts = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {


                    return PartialView(partialView, new DailyAgentManagement { AgentAccount = new Data.Entity.DailyCollectionData.AgentAccount() });

                }
                else
                {
                    var data = await _agentAccountServices.GetAgentAccountById(KEY);
                    return PartialView(partialView, new DailyAgentManagement { AgentAccount = data });

                }

            }
        }
        public async Task<ActionResult> UploadDailyCollectorOperation()
        {
            ViewBag.Branches = BuildMenuISViewBag((await _branchService.GetBranches()).ToList());
            ViewBag.BankName = _branchService.GetBankName();
            return View(new UploadDailyCollectorData { });
        }
        public async Task<ActionResult> ListOfUploadedFiles()
        {
          
            return View(new FileUploadCollectorDto { });
        }

        public async Task<ActionResult> UploadDailySavers()
        {
            ViewBag.Branches = BuildMenuISViewBag((await _branchService.GetBranches()).ToList());
            ViewBag.BankName = _branchService.GetBankName();
            return View(new UploadDailyCollectorData ());
        }
        private dynamic BuildMenuISViewBag(List<CBS.FrontDesk.Data.Entity.Config.Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {

                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $" {item.Name}" });


            }
            return selectListItems;

        }
        [HttpPost]
        public async Task<ActionResult> UploadDailyCollectorModelXXX(UploadDailyCollectorData model)
        {
            try
            {
                // Step 1: Validate Model State (Data Annotation Checks)
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input. Please fill all required fields." }, JsonRequestBehavior.AllowGet);
                }

                // Step 2: Check if file was uploaded and has content
                if (model.FormFile == null || model.FormFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty." }, JsonRequestBehavior.AllowGet);
                }

                // Step 3: Validate Excel file extension
                string fileExtension = Path.GetExtension(model.FormFile.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    return Json(new { success = false, message = "Invalid file format. Please upload a .xlsx or .xls file." }, JsonRequestBehavior.AllowGet);
                }
                var modek = (await _userServices.GetUsers()).Where(x => x.id.ToString() == model.CollectorId).FirstOrDefault();
                model.CollectorName = $"{modek.firstName} {modek.lastName}";
                // Step 4: (Optional) Save the file to a temp location or process directly from stream
                string fileName = Path.GetFileName(model.FormFile.FileName);
                 await _dailyCollectionMigrationServices.UploadFileVoid(model);

                // Step 5: (Placeholder) Validate Excel structure and content here
                // You can use a library like ClosedXML or ExcelDataReader here


                // Example response for now:
                var Data = new { totalVolume= "", dailyCollectorGL="", totalMembers =""};
                return Json(new {  data=Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Step 6: Log the exception and return error response
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"An error occurred while processing the file: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

      




        [HttpPost]
        public async Task<ActionResult> UploadDailyCollectorModelcccc(UploadDailyCollectorData model)
        {

            List<DailySaverRequest> dataList = new List<DailySaverRequest>();
            var branches = await _branchService.GetBranches();
            var branch = branches.Where(x => x.Id == model.BranchId).FirstOrDefault();
            try
            {
                // Step 1: Validate Model State (Data Annotation Checks)
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input. Please fill all required fields." }, JsonRequestBehavior.AllowGet);
                }

                // Step 2: Check if file was uploaded and has content
                if (model.FormFile == null || model.FormFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty." }, JsonRequestBehavior.AllowGet);
                }

                // Step 3: Validate Excel file extension
                string fileExtension = Path.GetExtension(model.FormFile.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    return Json(new { success = false, message = "Invalid file format. Please upload a .xlsx or .xls file." }, JsonRequestBehavior.AllowGet);
                }
                var modek = (await _userServices.GetUsers()).Where(x => x.id.ToString() == model.CollectorId).FirstOrDefault();
                model.CollectorName = $"{modek.firstName} {modek.lastName}";
                // Step 4: (Optional) Save the file to a temp location or process directly from stream
                string fileName = Path.GetFileName(model.FormFile.FileName);
                #region MyRegion
                var response = await _dailyCollectionMigrationServices.UploadFile(model);

                if (response.Result)
                {
                    var serviceResponse = (ServiceResponseDailySaverUploadResult)response.Data;
                    this.HttpContext.Session["rptSource" + Session.SessionID] = serviceResponse.Data;
                    var data = new
                    {
                        DataList = serviceResponse.Data.CollectorMembers.ToList(),
                        Summary = new
                        {
                            actualVolume = Convert.ToDecimal(serviceResponse.Data.GLAccountBalance),
                            totalVolume = Convert.ToDecimal(serviceResponse.Data.CollectorMembers.Sum(x => x.AccountBalance)),
                            isexhausive = serviceResponse.Data.IsExhausive,
                            dailyCollectorGL = serviceResponse.Data.AccountNumber + "-" + serviceResponse.Data.AccountName,
                            totalMembers = serviceResponse.Data.CollectorMembers.Count(),
                            absentMembers = serviceResponse.Data.AbsentMembers,
                            cashDifference = Math.Abs(Convert.ToDecimal(serviceResponse.Data.GLAccountBalance) - Convert.ToDecimal(serviceResponse.Data.CollectorMembers.Sum(x => x.AccountBalance))),
                            recordId = _accountServices.GetUserId() + "@" + model.CollectorId
                        }
                    };

                    return Json(new
                    {
                        success = true,
                        status = "success",
                        message = "File was uploaded successfully",
                        Data = data
                    });

                }
                else
                {
                    return Json(new { success = false, status = "Error", message = "The file structure does not respect the expected file format", Data = "nullable" });

                }

                #endregion

                //if (response.Result)
                //{
                //    var serviceResponse = (ServiceResponseDailySaverUploadResult)response.Data;
                //    this.HttpContext.Session["rptSource" + Session.SessionID] = serviceResponse.Data;
                //    var data = new
                //    {
                //        DataList = serviceResponse.Data.CollectorMembers.ToList(),
                //        Summary = new
                //        {
                //            actualVolume = Convert.ToDecimal(serviceResponse.Data.GLAccountBalance),
                //            totalVolume = Convert.ToDecimal(serviceResponse.Data.CollectorMembers.Sum(x => x.AccountBalance)),
                //            isexhausive = serviceResponse.Data.IsExhausive,
                //            dailyCollectorGL = serviceResponse.Data.AccountNumber + "-" + serviceResponse.Data.AccountName,
                //            totalMembers = serviceResponse.Data.CollectorMembers.Count(),
                //            absentMembers = serviceResponse.Data.AbsentMembers,
                //            cashDifference = Math.Abs(Convert.ToDecimal(serviceResponse.Data.GLAccountBalance) - Convert.ToDecimal(serviceResponse.Data.CollectorMembers.Sum(x => x.AccountBalance))),
                //            recordId = _accountServices.GetUserId() + "@" + model.CollectorId
                //        }
                //    };

                //    return Json(new
                //    {
                //        success = true,
                //        status = "success",
                //        message = "File was uploaded successfully",
                //        Data = data
                //    });

                //}
                //else
                //{
                //    return Json(new { success = false, status = "Error", message = "The file structure does not respect the expected file format", Data = "nullable" });

                //}



            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json($"An error occurred: {ex.Message}", JsonRequestBehavior.AllowGet);
            }

        }
        private List<AddDailySaverMinCommand> CreateBatchLinq(List<BusinessService.DailyCollectionServices.DailySaverRequest> responseData)
        {
            return responseData?
                .Where(request => request != null) // Filter out null entries
                .Select(request => new AddDailySaverMinCommand
                {
                    DailySaverId = request.DailySaverId,
                    IsNewCustomer = request.IsNewCustomer,
                    FirstName = request.FirstName,
                    BankCode = request.BankCode,
                    BranchName = request.BranchName,
                    BranchCode = request.BranchCode
                })
                .ToList() ?? new List<AddDailySaverMinCommand>();
        }
        [HttpPost]
        public async Task<JsonResult> PostAgentGLForInitialization()
        {
            var sessionKey = "rptSource" + _dailyCollectionMigrationServices.GetUserID();
            var modelData = this.HttpContext.Session[sessionKey] as DailySaverUploadTempResult;

            if (modelData == null)
            {
                return Json(new { success = false, message = "Session expired or report data not found." }, JsonRequestBehavior.AllowGet);
            }

            var result = await _dailyCollectionMigrationServices.PostAgentGLForInitialization(modelData);

            if (result != null)
            {
                return Json(new { success = true, data = result, message = "Collector Account Initialize successfully processed." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Error occurred while Initializing Collector Account." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> UploadDailyCollectorModel(UploadDailyCollectorData model)
        {
            List<DailySaverRequest> dataList = new List<DailySaverRequest>();
            var branches = await _branchService.GetBranches();
            var branch = branches.Where(x => x.Id == model.BranchId).FirstOrDefault();

            try
            {
                // Step 1: Validate Model State (Data Annotation Checks)
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input. Please fill all required fields." }, JsonRequestBehavior.AllowGet);
                }

                // Step 2: Check if file was uploaded and has content
                if (model.FormFile == null || model.FormFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty." }, JsonRequestBehavior.AllowGet);
                }

                // Step 3: Validate Excel file extension
                string fileExtension = Path.GetExtension(model.FormFile.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    return Json(new { success = false, message = "Invalid file format. Please upload a .xlsx or .xls file." }, JsonRequestBehavior.AllowGet);
                }

                var modek = (await _userServices.GetUsers()).Where(x => x.id.ToString() == model.CollectorId).FirstOrDefault();
                model.CollectorName = $"{modek.firstName} {modek.lastName}";

                var accountId =await _accountServices.GetAccount(model.AccountId);
                if (accountId == null)
                {
                    return Json(new { success = false, message = "Invalid account selected. Please select a valid account." }, JsonRequestBehavior.AllowGet);
                }

                // Read Excel file and get all data
                var responseData = await _dailyCollectionMigrationServices.ReadExcelFileAsync(
                    model.FormFile,
                     branch.Name,
                    branch.BranchCode,
                    _dailyCollectionMigrationServices.GetUserName(),

                    model.AccountId,
                    branch.Id,
                    model.CollectorId
                );

            
                var batchModel = new DailySaverUpload
                {
                    AccountId = model.AccountId,
                    BranchId = branch.Id,
                    CollectorId= model.CollectorId,
                    CashDifferenceAccountId= model.AccountId,
                    CollectorName= model.CollectorName,
                    ProductId="",
                    DailySaverList = responseData
                };
                var tempResult00 = await _dailyCollectionMigrationServices.Create(batchModel);
                var temp1 = (ResponseObject<DailySaverUploadTempResult>)tempResult00.Data;
                var tempResult = temp1.Data;

                //var serviceResponse = (ServiceResponseDailySaverUploadResult)response.Data;
                //this.HttpContext.Session["rptSource" + Session.SessionID] = serviceResponse.Data;
                var sessionKey = "rptSource" + _agentServices.GetUserID();
                  this.HttpContext.Session[sessionKey] = temp1.Data;
                var data = new
                {
                    DataList =BuildDailyCollectorMembersAsync(responseData),
                    Summary = new
                    {
                        actualVolume = Convert.ToDecimal(tempResult.GLAccountBalance),
                        totalVolume = Convert.ToDecimal(responseData.Sum(x => x.Amount)),
                        isexhausive = false,
                        dailyCollectorGL = tempResult.CollectorGL,
                        totalMembers = tempResult.TotalMembers,
                        absentMembers = "Still Processing" ,
                        cashDifference = Convert.ToDecimal(tempResult.GLAccountBalance) - Convert.ToDecimal(responseData.Sum(x => x.Amount)),//Math.Abs(Convert.ToDecimal(serviceResponse.Data.GLAccountBalance) - Convert.ToDecimal(serviceResponse.Data.CollectorMembers.Sum(x => x.AccountBalance))),
                        id = tempResult.Id// _accountServices.GetUserId() + "@" + model.CollectorId
                    }
                };

                return Json(new
                {
                    success = true,
                    status = "success",
                    message = $"Branch {tempResult.BranchName} | Collector {tempResult.CollectorName} | Total Members: {tempResult.TotalMembers} | Total Amount: {tempResult.TotalAmount} | Collector GL: {tempResult.CollectorGL} (Balance {tempResult.GLAccountBalance}) | Expected processing time: {TimeSpan.FromMilliseconds(200 * responseData.Count)}.",

                    Data = data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new
                {
                    success = false,
                    message = $"An error occurred: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<DailyCollectorMemberAccounts> BuildDailyCollectorMembersAsync(List<DailySaverRequest> responseData)
        {
            List <DailyCollectorMemberAccounts> dailyCollectors = new List<DailyCollectorMemberAccounts>();
            foreach (var item in responseData)
            {
                dailyCollectors.Add(new DailyCollectorMemberAccounts { AccountBalance = item.Amount, Name = item.FirstName, MemberReference = item.DailySaverId });
            }
            return dailyCollectors;
        }

        private List<DailySaverRequest> ReadExcelFile(Stream stream,string BranchCode,string branchName, string accountId,string username, string branchId)
        {
            int i = 0;
            var dataList = new List<DailySaverRequest>();
            try
            {
                // Reset stream position to beginning
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        throw new InvalidOperationException("No worksheet found in the Excel file.");
                    }

                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                    if (lastRow < 2)
                    {
                        throw new InvalidOperationException("Excel file must contain at least a header row and one data row.");
                    }

                    // Skip header row, start from row 2
                    for (int row = 2; row <= lastRow; row++)
                    {


                        var currentRow = worksheet.Row(row);
                        // Skip empty rows
                        if (currentRow.IsEmpty())
                            continue;

                        // Alternative way to read with better type checking
                        var record = new DailySaverRequest
                        {
                            AccountNumber = GetCellValueAsString(currentRow.Cell(1)),
                            FirstName = GetCellValueAsString(currentRow.Cell(2)),
                            Username = username,
                            BranchCode = BranchCode,
                            BranchId = branchId,//GetCellValueAsString(currentRow.Cell(4)),
                            DailySaverId = PrepareDailySaverIDFormat(GetCellValueAsString(currentRow.Cell(1)), BranchCode),
                            BranchName = branchName,
                            IsNewCustomer = false, // Default value, adjust as needed
                            BankCode = "012",
                            AccountId = accountId,
                            Amount = GetCellValueAsDecimal(currentRow.Cell(5)),
                            HasBeenProcessed = false
                        };
                        dataList.Add(record);
                    }
                }
           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
                throw;
            }
            return dataList;
        }
        public string PrepareDailySaverIDFormat(string id, string branchCode)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID must not be null or empty.");

            if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
                throw new ArgumentException("Branch code must be exactly 3 characters long.  " + branchCode);

            // Get last 5 characters of the ID or pad with '0' to the left if shorter
            string formattedIdPart = id.Length > 5
                ? id.Substring(id.Length - 5)
                : id.PadLeft(5, '0');

            // Combine branchCode + "DS" + 5-character ID
            return branchCode + "DS" + formattedIdPart;
        }
        public string GetCellValueAsString(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return string.Empty;

            return cell.GetString()?.Trim() ?? string.Empty;
        }

        // Basic version - returns 0 for invalid values
        public decimal GetCellValueAsDecimal(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty())
                return 0m;

            // Try to get as double first (Excel's native numeric type)
            if (cell.TryGetValue(out double doubleValue))
            {
                return Convert.ToDecimal(doubleValue);
            }

            // If not a number, try to parse the string representation
            var stringValue = cell.GetString()?.Trim();
            if (string.IsNullOrEmpty(stringValue))
                return 0m;

            // Try parsing as decimal with culture-invariant format
            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            // Try parsing with current culture (handles localized number formats)
            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }

            // If all parsing attempts fail, return 0
            return 0m;
        }
        [HttpPost]
        public async Task<ActionResult> UploadDailyCollectorEndOfdayOperation(UploadDailyCollectorOperationData model)
        {
            try
            {
                // Step 1: Validate Model State (Data Annotation Checks)
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid input. Please fill all required fields." }, JsonRequestBehavior.AllowGet);
                }

                // Step 2: Check if file was uploaded and has content
                if (model.ExcelFile == null || model.ExcelFile.ContentLength == 0)
                {
                    return Json(new { success = false, message = "No file uploaded or file is empty." }, JsonRequestBehavior.AllowGet);
                }

                // Step 3: Validate Excel file extension
                string fileExtension = Path.GetExtension(model.ExcelFile.FileName);
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    return Json(new { success = false, message = "Invalid file format. Please upload a .xlsx or .xls file." }, JsonRequestBehavior.AllowGet);
                }

                // Step 4: (Optional) Save the file to a temp location or process directly from stream
                //   string fileName = Path.GetFileName(model.ExcelFile.FileName);
                 var data=   await _dailyCollectionMigrationServices.UploadFile(model);
             
                return Json(new { data = data.Data, success = true, message = "File uploaded and validated successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Step 6: Log the exception and return error response
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"An error occurred while processing the file: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }



        private dynamic BuildOperationTypes()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select Option" });

            //selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Subscription", Value = $"SUB" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Deposit", Value = $"CashIn" });

            return selectListItems;

        }
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildDailyCollectorAsync(List<DailyCollectorInfo> listOfCollector)
        {
        
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in listOfCollector)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.name , Value = item.userId.ToString() });

            }

            return selectListItems;

        }
        private async Task GetList()
        {
            var BranList = (await _branchService.GetBranches()).ToList();
            ViewBag.Branches = BuildBranch(BranList);
            ViewBag.OperationTypes = BuildOperationTypes();
            ViewBag.DailyCollectors =await BuildDailyCollectorAsync(GetSampleDailyCollectors());
            ViewBag.Accounts = new List<Account>();
        }
        public List<DailyCollectorInfo> GetSampleDailyCollectors()
        {
            return new List<DailyCollectorInfo>
    {
        new DailyCollectorInfo { userId = "COL001", name = "Alice Nkom" },
        new DailyCollectorInfo { userId = "COL002", name = "Jean Dupont" },
        new DailyCollectorInfo { userId = "COL003", name = "Fatou Bayo" }
    };
        }
        private dynamic BuildBranch(List<CBS.FrontDesk.Data.Entity.Config.Branch> listOfItems)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "", Value = $"Select BranchCode" });
            foreach (var item in listOfItems)
            {
                if (!item.BranchCode.Equals("000"))
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = $"{item.Name}", Value = item.Id });
                }

            }
            return selectListItems;

        }
        private List<StringValues> BuildDropDown(List<Data.Account> ListOfData)
        {
            List<StringValues> list = new List<StringValues>();
            foreach (var item in ListOfData)
            {

                list.Add(new StringValues { Text = $"{item.TempData}-{item.AccountName}", Value = item.Id });

            }

            return list;
        }

        public async Task<ActionResult> GetBranchAccount(string BranchId)
        {


            try
            {
                var modelist = await _accountServices.GetAllAccountForABranch(BranchId);
                modelist = modelist.Where(x=>x.AccountNumber=="3841").ToList();
                return Json(BuildDropDown(modelist), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> PayDailyCollectorCommission(CollectorDto collector)
        {
            var sessionKey = "rptSource" + _agentServices.GetUserID();
            var modelData = this.HttpContext.Session[sessionKey] as PayDailyCollectorCommission;

            if (modelData == null)
            {
                return Json(new { success = false, message = "Session expired or report data not found." }, JsonRequestBehavior.AllowGet);
            }

            var result = await _agentServices.PayAgentActivitiesAsync(modelData);

            if (result != null)
            {
                return Json(new { success = true, data = result, message = "Payment successfully processed." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Error occurred while processing payment." }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpGet]
        public async Task<JsonResult> RetrieveDailyCollectionDashboardActivitiesAsync(string Month, string BranchId, string CollectorId)
        {
            try
            {
         

                return Json(new
                {
                    success = true,
                    data = FinancialReportDataGenerator.GenerateTestData() ,
                    JsonRequestBehavior.AllowGet
                });
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetActiveAgentBYBranch(string BranchId, string month)
        {
            int Year = 0; int Month = 0;
            try
            {
                // Validate input
                if (BranchId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                if (!string.IsNullOrEmpty(month))
                {
                    var monthParts = month.Split('-');
                    if (monthParts.Length == 2)
                    {
                        if (int.TryParse(monthParts[0], out int year) && int.TryParse(monthParts[1], out int months))
                        {
                            Year = year;
                            Month = months;
                        }
                        else
                        {
                            throw new FormatException($"Invalid month format: {month}. Expected format: YYYY-MM");
                        }
                    }
                    else
                    {
                        throw new FormatException($"Invalid month format: {month}. Expected format: YYYY-MM");
                    }
                }
                var activities = await _agentServices.GetAgentActiveAgent(BranchId, Month, Year);
                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }

                var dee = await BuildDailyCollectorAsync(activities);
                return Json(new
                {
              
                    data = dee
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet]
        public async Task<JsonResult> RetrieveAgentActivitiesAsync(string Month, string BranchId, string CollectorId, string OperationType, string MemberReference)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                var activities = await _agentServices.GetAgentActivitiesAsync(new CollectorSalaryInfo
                {
                    Month = Month,
                    BranchId = BranchId,
                    CollectorId = CollectorId,
                    OperationType = OperationType,
                    MemberReference = MemberReference
                });

                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    this.HttpContext.Session["rptSource" + _agentServices.GetUserID()] = activities.ConvertToPayDailyCollectorCommission();
                }

                return Json(new
                {
                    success = true,
                    data = activities
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetAgentAllAgentByBranch( string branchId)
        {
            try
            {
            
                // Example: Fetch data from service/repository
                var users = await _userServices.GetUSerRoles();

                var activities =   users.Where(x=>x.branchId==branchId&&x.RoleName== "Daily_Collector_Agent");

                if (!activities.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = await BuildAgentByBranch(activities.ToList())
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildAgentByBranch(List<UserRoleDto> listOfCollector)
        {

            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            //if (listOfCollector.Count()>1)
            //{
            foreach (var item in listOfCollector)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.FirstName == null ? item.FirstName : item.LastName == null ? "Name Not Define" : item.LastName, Value = item.UserId.ToString() });

            }
            //}
            //else
            //{
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text ="Collector 1",Value = "Collector1" });
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 2", Value = "Collector2" });
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 3", Value = "Collector3" });
            //}


            return selectListItems;

        }
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildAgentByBranch(List<AgentDto> listOfCollector)
        {

            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            //if (listOfCollector.Count()>1)
            //{
                foreach (var item in listOfCollector)
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.FirstName == null ? item.FirstName : item.LastName == null ? "Name Not Define" : item.LastName, Value = item.Id.ToString() });

                }
            //}
            //else
            //{
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text ="Collector 1",Value = "Collector1" });
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 2", Value = "Collector2" });
            //    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 3", Value = "Collector3" });
            //}


            return selectListItems;

        }
        [HttpGet]
        public async Task<JsonResult> GetListOfUploadedFiles()
        {
            try
            {
                var dataw = await _dailyCollectionEndOfDayServices.GetUploadedFiles();


                return Json(new
                {
                    success = true,
                    data = dataw

                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetFileUploadApproval(string Id)
        {
            try
            {
               
                    var dataw = _dailyCollectionEndOfDayServices.GetUploadedFile(Id);


                    return View(dataw);
                 
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<JsonResult> RetrieveFinancialActivitiesStatics(string Month, string BranchId, string CollectorId)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid parameters supplied."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Example: Fetch data from service/repository
                var activities = await _agentServices.GetAgentActivitiesAsync(new CollectorSalaryInfo
                {
                    Month = Month,
                    BranchId = BranchId,
                    CollectorId = CollectorId,

                });

                if (activities == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No activities found for the provided parameters."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    this.HttpContext.Session["rptSource" + _agentServices.GetUserID()] = activities.ConvertToPayDailyCollectorCommission();
                }

                return Json(new
                {
                    success = true,
                    data = activities
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, DailyAgentManagement model)
        {
            if (serviceOption == "agent")
            {

                return () => _agentServices.Create(model.Agent);

            }
            else if (serviceOption == "agentAccount")
            {
                return () => _agentAccountServices.Create(model.AgentAccount);
            }

            else
            {
                return null;
            }
        }
    }
}