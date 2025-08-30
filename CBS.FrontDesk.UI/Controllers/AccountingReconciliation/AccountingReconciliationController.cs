using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.AccountingReconciliation
{
    public class AccountingReconciliationController : BaseController
    {
        private AccountingReconciliationServices _accountingReconciliationServices { get; set; }
        private BranchServices _branchServices { get; set; }
        public AccountingReconciliationController()
        {
            _accountingReconciliationServices = new AccountingReconciliationServices();
            _branchServices = new BranchServices();
        }
        // GET: 
        public async Task<ActionResult> Index()
        {
            await GetList();
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await GetDataTable(GetDataTableOptions(), searchCriteria);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task GetList()
        {
            var listBranch = await _branchServices.GetBranches();
            ViewBag.Branches = BuildDropDown(GenerateBranchBranchCode(listBranch.ToList()));

        }

        public DataTableOptions GetDataTableOptions()
        {
            var queryParams = HttpContext.Request.QueryString;

            // Ensure draw is not null or empty, default to "1" if missing
            var draw = !string.IsNullOrWhiteSpace(queryParams["draw"]) ? queryParams["draw"] : "1";

            // Retrieve the sort column name and provide a default (e.g., "Timestamp") if missing
            var sortColumnIndex = queryParams["order[0][column]"];
            var sortColumnName = !string.IsNullOrWhiteSpace(sortColumnIndex) && queryParams[$"columns[{sortColumnIndex}][name]"] != null
                ? queryParams[$"columns[{sortColumnIndex}][name]"]
                : "Timestamp";  // Default column if not provided

            // Default sort direction to "asc" if missing or invalid
            var sortDirection = queryParams["order[0][dir]"]?.ToLower() == "desc" ? "desc" : "asc";

            DataTableOptions dataTableOptions = new DataTableOptions
            {
                draw = draw,
                start = int.TryParse(queryParams["start"], out var start) ? start : 0,
                length = int.TryParse(queryParams["length"], out var length) ? length : 10,
                sortColumnName = sortColumnName,
                sortColumnDirection = sortDirection,
                searchValue = queryParams["search[value]"] ?? string.Empty,
                sortDirection = sortDirection
            };

            dataTableOptions.pageSize = dataTableOptions.length;
            dataTableOptions.skip = dataTableOptions.start;
            dataTableOptions.recordsTotal = 0;

            return dataTableOptions;
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true)
        {
           
            var TransactionParam = new PaginatedResource
            {
                OrderBy = "TransactionDate",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                BranchId = _branchServices. GetBranchID(),      
                IsByBranch = isByBranch,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            Func<Task<List<TransactionTracker>>> getDataFunc = async () => (await _accountingReconciliationServices.GetTransactionTrackers(TransactionParam, ( await _branchServices.GetBranches()).ToList())).ToList();
            var dataTable = await _accountingReconciliationServices.GenerateDataTable(dataTableOptions, getDataFunc);
            return dataTable;

        }
 
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "FilteringBankDepositOption")
            {
                return await HandleFilteringBankDepositOption(KEY, partialView);
            }
            else
            {
                return HandleSessionData(KEY, partialView);
            }
        }
       
        private async Task<ActionResult> HandleFilteringBankDepositOption(string key, string partialView)
        {
            try
            {
                // Deserialize the filter
                var settings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                    Culture = CultureInfo.InvariantCulture
                };

                var queryFilter = JsonConvert.DeserializeObject<QueryFilter>(key, settings);

                // Get transaction data
                var dataModel = await _accountingReconciliationServices.GetTransactionTrackers(queryFilter);

                if (!dataModel.Result)
                {
                    return HandleNoDataScenario(partialView);
                }

                // Get branch data
                var listBranch = await _branchServices.GetBranches();
                ViewBag.IsAuthourized = true;

                // Extract transaction data safely
                var transactionData = ExtractTransactionData(dataModel.Data);

                // Process and store in session
                var transactionTrackerList = GetTransactionTrackerWithBranchInformationImmutable(listBranch, transactionData);
                StoreInSession(transactionTrackerList);

                return PartialView(partialView, new TransactionTrackerConfiguration
                {
                    TransactionTrackers = transactionTrackerList ?? new List<TransactionTracker>()
                });
            }
            catch (JsonException jsonEx)
            {
                return HandleError(partialView, "Invalid date format or filter parameters");
            }
            catch (Exception ex)
            {
                return HandleError(partialView, "You must select a date range you estimated the data was inputed");
            }
        }

        private ActionResult HandleSessionData(string key, string partialView)
        {
            try
            {
                var sessionKey = "TransactionTracker" + _accountingReconciliationServices.GetUserID();
                var transactionTrackerList =(List<TransactionTracker>) HttpContext.Session[sessionKey];

                if (transactionTrackerList == null)
                {
                    return HandleError(partialView, "Session data not found. Please refresh the data.");
                }

                var selectedTransaction = transactionTrackerList.Find(x => x.Id.ToString() == key);

                return PartialView(partialView, new TransactionTrackerConfiguration
                {
                    TransactionTracker = selectedTransaction
                });
            }
            catch (Exception ex)
            {
                return HandleError(partialView, "Error retrieving transaction data");
            }
        }

        [HttpPost]
        public  async Task<ActionResult> RepostingDataAsync(string objectAsString)
        {
            try
            {
                var sessionKey = "TransactionTrackerData:" + _accountingReconciliationServices.GetUserID();
                var data = (TransactionTracker)HttpContext.Session[sessionKey];
                var datac = await _accountingReconciliationServices.PostPayload(new { TransactionTracker = data, CommandDataType = data.CommandDataType, CommandJsonObject = objectAsString });

                return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

            }
            catch (Exception ex)
            {


                return Json(new { success = false, status ="Success", message =ex.Message });

            }

        }

        private List<TransactionTracker> ExtractTransactionData(object data)
        {
            // .NET 4.7.2 compatible version using traditional if-else
            if (data is ResponseObject<List<TransactionTracker>> responseObj)
            {
                var stringData = JsonConvert.SerializeObject(responseObj);
                var jsonResponse = JObject.Parse(stringData);
                var message = jsonResponse["Data"]?.ToString();
                var listDaa= JsonConvert.DeserializeObject<List<TransactionTracker>>(message);
                return listDaa;
            }

            if (data is List<TransactionTracker> directList)
            {
                return directList;
            }

            // Default case - return empty list
            return new List<TransactionTracker>();
        }

        private ActionResult HandleNoDataScenario(string partialView)
        {
            ViewBag.IsAuthourized = false;
            ViewBag.Error = $"{_accountingReconciliationServices.GetUserFullName()}, All looks good there is no pending or unreconciled transaction";

            return PartialView(partialView, new TransactionTrackerConfiguration
            {
                TransactionTrackers = new List<TransactionTracker>()
            });
        }

        private ActionResult HandleError(string partialView, string errorMessage)
        {
            ViewBag.IsAuthourized = false;
            ViewBag.Error = $"{_accountingReconciliationServices.GetUserFullName()}, {errorMessage}";

            return PartialView(partialView, new TransactionTrackerConfiguration
            {
                TransactionTrackers = new List<TransactionTracker>()
            });
        }
        public async Task<JsonResult> ReconciliationData(string id)
        {
            var sessionKey = "TransactionTracker:" + _accountingReconciliationServices.GetUserID();
            var dataList = (List<TransactionTracker>)HttpContext.Session[sessionKey];
            var model = dataList.Find(x => x.Id == id);
            var listResponse = new List<TransactionTracker>
            { };
            listResponse.Add(model);
            StoreInSession(model);
            return Json( new {data= model, listData= listResponse },JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetReconciliationData(string id)
        {
            var sessionKey = "TransactionTracker:" + _accountingReconciliationServices.GetUserID();
             var dataList=(List<TransactionTracker>) HttpContext.Session[sessionKey];
            var model = dataList.Find(x=> x.Id == id);  
            return View( new TransactionTrackerConfiguration
            {
                TransactionTracker = model
            });
        }

        private void StoreInSession(List<TransactionTracker> data)
        {
            var sessionKey = "TransactionTracker:" + _accountingReconciliationServices.GetUserID();
            HttpContext.Session[sessionKey]= data;
        }
        private void StoreInSession(TransactionTracker data)
        {
            var sessionKey = "TransactionTrackerData:" + _accountingReconciliationServices.GetUserID();
            HttpContext.Session[sessionKey] = data;
        }
        // Extension methods for session handling (add to a separate static class)

        private List<TransactionTracker> GetTransactionTrackerWithBranchInformationImmutable(
            IEnumerable<Branch> listBranch,
            List<TransactionTracker> transactionTrackers)
        {
            if (transactionTrackers == null || !transactionTrackers.Any())
                return new List<TransactionTracker>();

            if (listBranch == null || !listBranch.Any())
                return new List<TransactionTracker>(transactionTrackers);

            // Create dictionaries for faster branch lookup
            var branchDictionary = listBranch.ToDictionary(
                branch => branch.BranchCode,
                branch => branch,
                StringComparer.OrdinalIgnoreCase);

            var branchByIdDictionary = listBranch.ToDictionary(
                branch => branch.Id,
                branch => branch,
                StringComparer.OrdinalIgnoreCase);

            return transactionTrackers.Select(tracker =>
            {
                Branch matchedBranch = null;

                // Try to match by BranchCode first, then by BranchId
                if (!string.IsNullOrWhiteSpace(tracker.BranchCode))
                    branchDictionary.TryGetValue(tracker.BranchCode, out matchedBranch);

                if (matchedBranch == null && !string.IsNullOrWhiteSpace(tracker.BranchId))
                    branchByIdDictionary.TryGetValue(tracker.BranchId, out matchedBranch);

                // Create a new TransactionTracker with enriched branch information
                return new TransactionTracker
                {
                    Id = tracker.Id,
                    CommandDataType = tracker.CommandDataType,
                    CommandJsonObject = tracker.CommandJsonObject,
                    TransactionReferenceId = tracker.TransactionReferenceId,
                    HasPassed = tracker.HasPassed,
                    NumberOfRetry = tracker.NumberOfRetry,
                    DatePassed = tracker.DatePassed,
                    TransactionDate = tracker.TransactionDate,
                    DestinationUrl = tracker.DestinationUrl,
                    SourceUrl = tracker.SourceUrl,
                    UserFullName = tracker.UserFullName,
                    CreatedDate = tracker.CreatedDate,
                    ErrorOrSuccessMessage = tracker.ErrorOrSuccessMessage,

                    // Enrich with branch information
                    BranchOffice = matchedBranch?.Name ?? "Unknown Branch",
                    BranchCode =tracker.BranchCode,
                    BranchId =  tracker.BranchId
                };
            }).ToList();
        }

        //[HttpPost]
        //public async Task<ActionResult> AddOrUpdate(CashDemandDataEntity model)
        //{
        //    if (model.ServiceOption.Equals("CashInfusionModel"))
        //    {
        //        if (model.Action.Equals("insert"))
        //        {


        //            var datac = await _accountingEntryServices.CashReplenishmentRequest(model.CashInfusionModel);

        //            return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

        //        }
        //        else
        //        {

        //            model.CashInfusionModel.Id = model.Id;
        //            var datac = await _accountingEntryServices.Update(model.CashInfusionModel);
        //            return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

        //        }
        //    }
        //    else if (model.ServiceOption.Equals("RedirectedForBankCashOut"))
        //    {
        //        var datac = await _accountingEntryServices.RedirectedForBankCashOutRequest(model.RedirectedForBankCashOut);

        //        return Json(new { success = datac.Result, status = datac.MessageStatus, message = Messaging.MessageResult(datac) });

        //    }
        //}
        private IEnumerable<StringValues> GenerateBranchBranchCode(List<Branch> branches)
        {
            List<StringValues> stringValues = new List<StringValues>();
            //var collections = branches.Where(x => x.IsHavingBank == true);
            foreach (var branch in branches)
            {

                stringValues.Add(new StringValues(branch.Id, branch.Name));
            }
            return stringValues;
        }

        private List<SelectListItem> BuildDropDown(IEnumerable<StringValues> stringValues)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in stringValues)
            {

                list.Add(new SelectListItem { Text = item.Text, Value = item.Value });

            }

            return list;
        }
    }
}