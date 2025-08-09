using CBS.API.Helper;
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
using CBS.FrontDesk.UI.Models;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
// 
namespace CBS.FrontDesk.UI.Controllers
{
    //
    public class DailyAgentManagementController : BaseController
    {
        private BranchServices _branchService;
        private AgentServices _agentServices;
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
        //
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
        private dynamic BuildMenuISViewBag(List<Branch> listOfItems)
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
        public async Task<ActionResult> UploadDailyCollectorModel(UploadDailyCollectorData model)
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
                string fileName = Path.GetFileName(model.ExcelFile.FileName);
               await _dailyCollectionMigrationServices.UploadFile(model);

                // Step 5: (Placeholder) Validate Excel structure and content here
                // You can use a library like ClosedXML or ExcelDataReader here

                // Example response for now:
                return Json(new { success = true, message = "File uploaded and validated successfully." }, JsonRequestBehavior.AllowGet);
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
        private dynamic BuildBranch(List<Branch> listOfItems)
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
                // Validate input
                //if (string.IsNullOrEmpty(Month) || BranchId.IsNullOrEmpty() || CollectorId.IsNullOrEmpty())
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "Invalid parameters supplied."
                //    }, JsonRequestBehavior.AllowGet);
                //}

                // Example: Fetch data from service/repository
                //var activities = await _agentServices.GetAllActivitiesAsync((new DailyCollectionDashboardActivitiesQuery
                //{
                //    Month = Month,
                //    BranchId = BranchId,
                //    CollectorId = CollectorId,

                //}).ConvertToDailyCollectionActivitiesQuery());

                //if (activities == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No activities found for the provided parameters."
                //    }, JsonRequestBehavior.AllowGet);
                //}


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
                var activities = await _agentServices.GetAgentAllAgentByBranchIdAsync(branchId);

                if (activities == null)
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
                    data = await BuildAgentByBranch(activities)
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
        private async Task<List<System.Web.WebPages.Html.SelectListItem>> BuildAgentByBranch(List<AgentDto> listOfCollector)
        {

            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            if (listOfCollector.Count()>1)
            {
                foreach (var item in listOfCollector)
                {
                    selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.FirstName == null ? item.FirstName : item.LastName == null ? "Name Not Define" : item.LastName, Value = item.Id.ToString() });

                }
            }
            else
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text ="Collector 1",Value = "Collector1" });
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 2", Value = "Collector2" });
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "Collector 3", Value = "Collector3" });
            }


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