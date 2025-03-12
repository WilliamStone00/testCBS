using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.RemittanceP
{
    //[CheckSessionTimeOutAttribute]
    public class RemittanceController : BaseController
    {
        // GET: Remittance
        private readonly RemittanceServices _services;
        private readonly BranchServices _branchServices;
        private readonly AccountServices _accountServices;

        public RemittanceController(RemittanceServices services, BranchServices branchServices = null, AccountServices accountServices = null)
        {
            _services = services;
            _branchServices = branchServices;
            _accountServices=accountServices;
        }
        //Validation
        public async Task<ActionResult> Remittances(GetAllRemittanceQuery remittanceQuery)
        {
            var remittances = await _services.GetRemittances(remittanceQuery);
            return View(remittances);
        }
        public async Task<ActionResult> PendingRequest(string branchid)
        {
            if (branchid==null)
            {
                branchid=Session["BranchId"].ToString();
            }
            var remittanceQuery = new GetAllRemittanceQuery { Approved=false, ByDateRange=false, QueryParameter="sourcebranchid", QueryValue= branchid, BranchId=branchid, DataTableOptions=GetDataTableOptions(), Status="Pending" };
            var pending = await _services.GetRemittances(remittanceQuery);
            return View(pending.ToList());
        }
        //Pending
        public async Task<ActionResult> RequestValidationForm(string remittanceid)
        {
            // Fetch the remittance record using the provided remittance ID
            var remittance = await _services.GetRemittance(remittanceid);
            // Update the approval comment to reflect validation and approval process
            remittance.ApprovalComment = $"Validated and Approved: Remittance request has been reviewed and authorized by [{Session["FullName"].ToString()}]. The sender [{remittance.SenderName}], may now proceed to the cash desk for payment processing.";
            // Return the updated remittance record to the view

            var validationOfRemittance = new ValidationOfRemittanceCommand { ApprovalComment=remittance.ApprovalComment, Id=remittance.Id, Status=remittance.Status, Remittance=remittance };

            return View(validationOfRemittance);
        }

        public async Task<ActionResult> RemittanceQuery()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new Remittance());
        }
        [HttpGet]
        public async Task<ActionResult> Details(string KEY = null)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return RedirectToAction("Index"); // Redirect to list page if KEY is not provided
            }

            var remittance = await _services.GetRemittance(KEY);
            if (remittance == null)
            {
                return View("NotFound"); // Show a Not Found view if loan is null
            }
            return View(remittance);
        }
        public async Task<ActionResult> Request()
        {
            await GetList();
            return View(new AddRemittanceCommand());
        }
        [HttpPost]
        public async Task<ActionResult> Create(AddRemittanceCommand model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();
                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }
            var data = await _services.Create(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
        [HttpPost]
        public async Task<ActionResult> Validate(ValidationOfRemittanceCommand model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();
                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }
            var data = await _services.Validate(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        }
        //Validate

        public async Task<bool> GetList()
        {
            var Branches = await _branchServices.GetBranches();
            var RemittanceTypes = new List<StringValues>();
            var languages = _branchServices.GetLanguages().ToList();

            ViewBag.Branches = Branches.ToList();
            ViewBag.Languages = languages;
            ViewBag.RemittanceTypes = RemittanceTypes;
            return true;
        }
        public async Task<ActionResult> GetRemittanceAccount(string branchid, string accountType)
        {

            var data = await _accountServices.GetRemittanceAccountByTypeQuery(branchid, accountType);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> Download(
    string queryParameter = "sourcebranchid",
    string queryValue = null,
    string dateFrom = null,
    string dateTo = null,
    string status = "all",
    string branchId = null)
        {
            try
            {
                Branch branch = null;

                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // Retrieve branch details efficiently
                if (!string.IsNullOrWhiteSpace(branchId))
                {
                    branch = await _branchServices.GetBranch(branchId);
                }
                else if (!_branchServices.IsHeadOffice())
                {
                    branch = await _branchServices.GetBranch(_branchServices.GetBranchID());
                }

                branch = new Branch(); // Ensure valid branch

                // Construct query object
                var getRemittanceDataTableQuery = new GetAllRemittanceQuery
                {
                    DataTableOptions = new DataTableOptions
                    {

                        start = 0,
                        searchValue = !string.IsNullOrWhiteSpace(queryValue) ? queryValue : "all"
                    },
                    DateFrom = startDate ?? DateTime.MinValue,
                    DateTo = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    QueryParameter = queryParameter,
                    QueryValue = queryValue
                };
                getRemittanceDataTableQuery.DataTableOptions = GetDataTableOptions();

                // Fetch remittance data
                var dataTable = await _services.GetDataTableAsync(getRemittanceDataTableQuery, queryValue);
                var remittances = JsonConvert.DeserializeObject<List<Remittance>>(JsonConvert.SerializeObject(dataTable.data));

                if (remittances == null || remittances.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No data available for export.");
                }

                // Get exported by user
                string exportedBy = Session["FullName"]?.ToString() ?? "Unknown";

                // Generate Excel file
                //var exportFile = RemittanceExcelGenerator.GenerateRemittanceExcel(
                //    remittances,
                //    branch,
                //    exportedBy,
                //    fileTitle: "REMITTANCE QUERY",
                //    dateFrom,
                //    dateTo
                //);

                //return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
                return null;
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting data.");
            }
        }


        [HttpPost]
        public async Task<ActionResult> LoadRemittanceData(
            string queryParameter = "sourcebranchid",
            string queryValue = null,
            string dateFrom = null,
            string dateTo = null,
            string status = "all",
            string branchId = null
        )
        {
            try
            {
                // Debug: Log request parameters

                // Validate input before proceeding
                if (string.IsNullOrWhiteSpace(queryValue) && queryParameter != "all")
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid query value.");
                }

                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // ✅ Construct Query Object
                var query = new GetAllRemittanceQuery
                {
                    DataTableOptions = PostDataTableOptions(),
                    DateFrom = startDate ?? DateTime.MinValue,
                    DateTo = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    QueryParameter = queryParameter,
                    QueryValue = queryValue
                };

                // Debug: Log constructed query

                // ✅ Fetch Data
                var dataTable = await _services.GetDataTableAsync(query, queryValue);

                if (dataTable == null || dataTable.data == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No data available.");
                }

                var remittanceList = JsonConvert.DeserializeObject<List<Remittance>>(JsonConvert.SerializeObject(dataTable.data));


                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = remittanceList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, $"Error loading remittance data: {ex.Message}");
            }
        }

        public async Task<ActionResult> GetRemittanceCharge(string accountNumber, string accountType, string amount, string chargeType, string transferType)
        {
            try
            {
                // Initialize the query object
                var getRemittanceCharge = new GetRemittanceChargeQuery
                {
                    Amount = string.IsNullOrEmpty(amount) ? 0 : Convert.ToDecimal(amount),
                    RemittanceType = accountType,
                    TransfterType = transferType,
                    SenderAccountNumber = accountNumber,
                    ChargeType = chargeType
                };

                // Fetch remittance charge data
                var data = await _services.GetRemittanceCharge(getRemittanceCharge);

                // Add session data (Transaction Manager Name)
                data.GlobalStatus = Session["FullName"]?.ToString();

                // Handle null or invalid FeeType
                if (data == null || string.IsNullOrEmpty(data.FeeType))
                {
                    return Json(new
                    {
                        success = false,
                        message = data.FeeName
                    }, JsonRequestBehavior.AllowGet);
                }

                // Return success response
                return Json(new
                {
                    success = true,
                    amount = data.Amount,
                    charge = data.Charge,
                    feeName = data.FeeName,
                    percentageValue = data.PercentageValue,
                    remittanceType = data.RemittanceType,
                    feeType = data.FeeType,
                    serviceCharge = data.ServiceCharge,
                    totalCharges = data.TotalCharges,
                    initailAmount = getRemittanceCharge.Amount,
                    globalStatus = data.GlobalStatus
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Handle server errors
                return Json(new { success = false, message = "An error occurred: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Ajaxloader(string Key, string transfterType)
        {
            var data = await _accountServices.GetAllRemittanceAccounts(Key);
            var listing = await _accountServices.GetAllRemittanceAccountsDroupDown(data.ToList(), transfterType);
            return Json(listing, JsonRequestBehavior.AllowGet);

        }

    }

}