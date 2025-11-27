using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounting_V2.BranchAccountService
{
    public class BranchAccountService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;

        public BranchAccountService()
        {
            //change the base url to the actual base url
            string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
            }
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<IEnumerable<BranchAccountResponse>> GetAsync()
        {
            try
            {
                // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<BranchAccountResponse>>>(APICallHelper.GetAllBranchAccount);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                return new List<BranchAccountResponse>();
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }

        public async Task<CustomDataTable> GetDataTableAsync(BranchAccountQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.BranchAccountdatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"service unavailable: {ex.Message}", ex);
            }
        }

        //tree structure 
        public async Task<BranchAccountResponse> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var lan = GetUserLanguage();

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetBranchAccountById, encodedId, lan);
                // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<BranchAccountResponse>>(formattedUrl);

                //string formattedUrl = string.Format(APICallHelper.GetChequeBookCategoryById, id);
                //var response = await _apiCallerHelper.GetAsync<ServiceResponse<CategoryConfig>>(formattedUrl);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }
        public async Task<BRANCHTreeDto> GetByIdfordetailsAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var lan = GetLanguage();

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.GetBranchAccountById, encodedId, lan);
                // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<BRANCHTreeDto>>(formattedUrl);

                //string formattedUrl = string.Format(APICallHelper.GetChequeBookCategoryById, id);
                //var response = await _apiCallerHelper.GetAsync<ServiceResponse<CategoryConfig>>(formattedUrl);

                // CORRECTED: Access the final payload via .ApiResponseData.Data
                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // In a real scenario, log 'ex'
                throw;
            }
        }


        public List<SelectListItem> DropDownGen(List<BranchAccountResponse> branchAccounts)
        {
            if (branchAccounts is null) return new List<SelectListItem>();

            // Sort & distinct (optional: by Id)
            var items = branchAccounts
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Id))
                .GroupBy(a => a.Id)
                .Select(g => g.First())
                .OrderBy(a => a.Name ?? string.Empty)
                .Select(a => new SelectListItem
                {
                    // Show number if you have it; remove if not applicable
                    Text = string.IsNullOrWhiteSpace(a.Code) ? $"{a.Name}" : $"{a.Name}",
                    Value = a.Id
                })
                .ToList();

            return items;
        }
        public List<SelectListItem> DropDownGen1(List<BranchAccountResponse> branchAccounts)
        {
            if (branchAccounts is null) return new List<SelectListItem>();

            // Sort & distinct (optional: by Id)
            var items = branchAccounts
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Id))
                //.GroupBy(a => a.Id)
                //.Select(g => g.First())
                //.OrderBy(a => a.Name ?? string.Empty)
                .Select(a => new SelectListItem
                {
                    // Show number if you have it; remove if not applicable
                    Text = string.IsNullOrWhiteSpace(a.Code) ? $"{a.Name}" : $"{a.Name}",
                    Value = a.Id
                })
                .ToList();

            return items;
        }

        public List<AccountDto> DroupDownGenAccounts(List<BranchAccountResponse> branchAccounts)
        {
            try
            {
                List<AccountDto> stringValues;

                stringValues = (from a in branchAccounts
                                select new AccountDto
                                {
                                    AccountNumber = $"{a.Id}",
                                    AccountName = $"{a.Name}",
                                }).ToList();

                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }

        //get Branch Accounts for dro down 
        public async Task<IEnumerable<BranchAccountResponse>> GetAllBranchAccountsFromDataTableAsync(string branchId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Decide branchId first so the query sent to server is correct.
                if (IsHeadOffice())
                {
                    if (string.IsNullOrWhiteSpace(branchId))
                        branchId = null; // ensure API understands this convention
                                         // Head Office may keep a specific branchId if passed
                }
                else
                {
                    branchId = GetBranchID() ?? throw new InvalidOperationException("Current user's branch ID is not available.");
                }

                var query = new BranchAccountQuery
                {
                    BranchId = branchId
                };

                // Reuse existing method which calls the API datatable endpoint
                var dataTable = await GetDataTableAsync(query);

                // Convert datatable.data to strongly-typed list safely
                List<BranchAccountResponse> branchList = new List<BranchAccountResponse>();

                if (dataTable?.data is JToken token)
                {
                    branchList = token.ToObject<List<BranchAccountResponse>>() ?? new List<BranchAccountResponse>();
                }
                else if (dataTable?.data != null)
                {
                    // Fallback if data is plain object
                    branchList = JsonConvert.DeserializeObject<List<BranchAccountResponse>>(JsonConvert.SerializeObject(dataTable.data))
                                 ?? new List<BranchAccountResponse>();
                }

                var formatted = branchList
                    .Select(a => new BranchAccountResponse
                    {
                        Id = a.Id,
                        Code = a.Code,
                        Name = $"[{a.Code}] - {a.Name}".Trim()
                    })
                    .OrderBy(a => a.Code) // nicer UX for dropdown; change if you prefer Id
                    .ToList();

                return formatted;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        ////get Branch Accounts for dro down 
        //public async Task<IEnumerable<BranchAccountResponse>> GetAllBranchAccountsFromDataTableAsync(string branchId)
        //{
        //    try
        //    {
        //        // Create an "empty" query so server interprets as get all (everything null)
        //        var query = new BranchAccountQuery
        //        {
        //            BranchId = branchId 
        //        };

        //        // Reuse existing method which calls the API datatable endpoint
        //        var dataTable = await GetDataTableAsync(query);

        //        // dataTable.data is `object` so convert safely to the expected DTO list
        //        var branchList = JsonConvert.DeserializeObject<List<BranchAccountResponse>>(
        //            JsonConvert.SerializeObject(dataTable?.data)
        //        ) ?? new List<BranchAccountResponse>();

        //        // Optional: keep only active ones if needed
        //        // branchList = branchList.Where(a => a.IsActive).ToList();

        //        if (!IsHeadOffice())
        //        {
        //            // Filter only the affiliate that matches current branch (if applicable)
        //            string currentBranchId = GetBranchID();
        //            branchList = branchList.Where(a => a.Id == currentBranchId).ToList();
        //        }
        //        else
        //        {
        //            // Add "All" option at the top for Head Office users
        //            var defaultAffiliate = new BranchAccountResponse
        //            {
        //                Id = "All",
        //                Name = "All Affiliates",
        //                Code = "ALL"
        //            };

        //            if (!branchList.Any(x => string.Equals(x.Id, defaultAffiliate.Id, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                branchList.Insert(0, defaultAffiliate);
        //            }
        //        }

        //        // Format name for display and order (choose ordering you prefer)
        //        var formatted = branchList
        //            .Select(a =>
        //            {
        //                a.Name = $" {a.Name}".Trim();
        //                return a;
        //            })
        //            .OrderBy(a => a.Id)
        //            .ToList();

        //        return formatted;
        //    }
        //    catch (Exception ex)
        //    {
        //        // log properly (example: _logger.LogError(ex, "GetAllBranchAccountsFromDataTableAsync failed");)
        //        throw;
        //    }
        //}


        // real endpoint version
        public async Task<IEnumerable<BranchAccountResponse>> GetBranchAccountsByBranchIdAsync(string branchId)
        {
            try
            {

                if (IsHeadOffice())
                {
                    if (branchId == "")
                    {
                        branchId = "all";

                    }
                }
                else
                {
                    branchId = GetBranchID();
                }
                string lang = GetUserLanguage();
                // Call API
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<BranchAccountResponse>>>(string.Format(APICallHelper.GetAllBranchAccountsOfABranch, branchId, lang));

                if (!response.IsSuccess || response == null || response.ApiResponseData == null)
                {
                    return new List<BranchAccountResponse>();
                }

                var branchAcounts = response?.ApiResponseData?.Data ?? new List<BranchAccountResponse>();

                // Format name for display and order by Code
                return branchAcounts
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name}".Trim();
                        return a;
                    })
                    .OrderBy(a => a.Id)
                    .ToList();
            }
            catch (Exception)
            {
                // Consider logging: _logger.LogError(ex, "GetAffiliatesFromEndpointAsync failed");
                throw;
            }
        }

        //***************************************** MOCK *********************************************
        public async Task<CustomDataTable> GetcategoryDataTableAsync2(BranchAccountQuery query)
        {
            // Simulate latency
            await Task.Delay(10);

            try
            {
                // --- SAMPLE IN-MEMORY DATA (based on the JSON you posted) ---
                var seedDate = DateTime.UtcNow;
                var all = new List<BRANCHTreeDto>
        {
            new BRANCHTreeDto
            {
                Id = "BR0011000000",
                BranchId = "BR001",
                Code = "1000000",
                Name = "",
                Class = "EQUITY",
                AffiliateAccountId = "AFF-1000000",
                AffiliateAccountName = "Affiliate GL 1000000",
                ParentId = null,
                Path = "/",
                Depth = 0,
                PostingAllowed = true,
                CreatedDate = seedDate,
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            new BRANCHTreeDto
            {
                Id = "BR0021000000",
                BranchId = "BR002",
                Code = "1000000",
                Name = "",
                Class = "EQUITY",
                AffiliateAccountId = "AFF-1000000",
                AffiliateAccountName = "Affiliate GL 1000000",
                ParentId = null,
                Path = "/",
                Depth = 0,
                PostingAllowed = true,
                CreatedDate = seedDate,
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            new BRANCHTreeDto
            {
                Id = "888628649802897",
                BranchId = "BR001",
                Code = "100000000000",
                Name = "FULLY PAID SHARES",
                Class = "EQUITY",
                AffiliateAccountId = "010872885313163",
                AffiliateAccountName = "FULLY PAID SHARES",
                ParentId = null,
                Path = "/10/00/00/00/00/00/",
                Depth = 6,
                PostingAllowed = true,
                CreatedDate = DateTime.Parse("2025-10-21T11:56:41.6288479+01:00"),
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            new BRANCHTreeDto
            {
                Id = "744532399970759",
                BranchId = "BR001",
                Code = "113000000000",
                Name = "GENERAL RESERVES",
                Class = "EQUITY",
                AffiliateAccountId = "122676332422043",
                AffiliateAccountName = "GENERAL RESERVES",
                ParentId = null,
                Path = "/11/30/00/00/00/00/",
                Depth = 6,
                PostingAllowed = true,
                CreatedDate = DateTime.Parse("2025-10-21T11:56:41.6291028+01:00"),
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            new BRANCHTreeDto
            {
                Id = "922161461361713",
                BranchId = "BR001",
                Code = "114000000000",
                Name = "114000000000",
                Class = "EQUITY",
                AffiliateAccountId = null,
                AffiliateAccountName = null,
                ParentId = null,
                Path = "/11/40/00/00/00/00/",
                Depth = 6,
                PostingAllowed = true,
                CreatedDate = DateTime.Parse("2025-10-21T11:56:41.6291251+01:00"),
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            new BRANCHTreeDto
            {
                Id = "915531390038003",
                BranchId = "BR001",
                Code = "114000000110",
                Name = "BUILDING CONTRIBUTION",
                Class = "EQUITY",
                AffiliateAccountId = "537589946130068",
                AffiliateAccountName = "BUILDING CONTRIBUTION",
                ParentId = "922161461361713",
                Path = "/11/40/00/00/01/10/",
                Depth = 6,
                PostingAllowed = true,
                CreatedDate = DateTime.Parse("2025-10-21T11:56:41.6293741+01:00"),
                IsDeleted = false,
                Children = new List<BRANCHTreeDto>()
            },
            // add as many mock items as needed...
        };

                // --- Filtering based on BranchAccountQuery (defensive checks) ---
                var items = all.AsQueryable();

                if (query != null)
                {
                    // Example fields - adjust names if your BranchAccountQuery uses different property names
                    if (!string.IsNullOrWhiteSpace(query.BranchId))
                        items = items.Where(x => string.Equals(x.BranchId ?? string.Empty, query.BranchId, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrWhiteSpace(query.Code))

                        if (!string.IsNullOrWhiteSpace(query.Name))
                            items = items.Where(x => ((x.Name ?? x.NameEn ?? x.AffiliateAccountName) ?? string.Empty).IndexOf(query.Name, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (!string.IsNullOrWhiteSpace(query.Class))
                        items = items.Where(x => (x.Class ?? string.Empty).IndexOf(query.Class, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (!string.IsNullOrWhiteSpace(query.AffiliateAccountId))
                        items = items.Where(x => string.Equals(x.AffiliateAccountId ?? string.Empty, query.AffiliateAccountId, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrWhiteSpace(query.ParentId))
                        items = items.Where(x => string.Equals(x.ParentId ?? string.Empty, query.ParentId, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrWhiteSpace(query.PathContains))
                        items = items.Where(x => (x.Path ?? string.Empty).IndexOf(query.PathContains, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (query.DepthFrom.HasValue)
                        items = items.Where(x => x.Depth >= query.DepthFrom.Value);

                    if (query.DepthTo.HasValue)
                        items = items.Where(x => x.Depth <= query.DepthTo.Value);

                    if (query.PostingAllowed.HasValue)
                        items = items.Where(x => x.PostingAllowed == query.PostingAllowed.Value);


                }
                else
                {
                    // if query null, exclude deleted by default
                    items = items.Where(x => !x.IsDeleted);
                }

                // Global search coming from DataTable options (if provided)
                var opts = query?.Options;
                var globalSearch = opts?.searchValue?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(globalSearch))
                {
                    var s = globalSearch.ToLowerInvariant();
                    items = items.Where(x =>
                        ((x.Code ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.Name ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.NameEn ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.AffiliateAccountName ?? string.Empty).ToLowerInvariant().Contains(s))
                        || ((x.AffiliateAccountId ?? string.Empty).ToLowerInvariant().Contains(s))
                    );
                }

                // Sorting: default CreatedDate desc
                items = items.OrderByDescending(x => x.CreatedDate);

                var total = all.Count;
                var filteredCount = items.Count();

                // Paging (DataTables style)
                var start = opts?.start ?? 0;
                var length = opts?.length ?? 10;
                if (start < 0) start = 0;
                if (length <= 0) length = 10;

                var page = items.Skip(start).Take(length).ToList();

                // Build CustomDataTable result
                var result = new CustomDataTable
                {
                    draw = Convert.ToInt32(opts?.draw ?? "1"),
                    recordsTotal = total,
                    recordsFiltered = filteredCount,
                    data = page
                };

                return result;
            }
            catch (Exception ex)
            {
                // Bubble up to caller - controller will catch and return DataTables-safe response.
                throw new Exception($"Mock GetcategoryDataTableAsync failed: {ex.Message}", ex);
            }
        }

        //***************************************** END MOCK *********************************************


        public async Task<ExecutionMessages> CreateAsync(BranchAccountCommand model)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ServiceResponse<BranchAccountCommand>>(APICallHelper.CreateBranchAccount, model);

                // CORRECTED: Pass the ServiceResponse object to GetExecutionMessages
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(BranchAccountCommand model)
        {
            try
            {
                var catid = model.Id;
                string formattedUrl = string.Format(APICallHelper.UpdateBranchAccount, catid);
                var response = await _apiCallerHelper.PutAsync<ServiceResponse<BranchAccountCommand>>(formattedUrl, model);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, model.NameEn, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.ApiResponseData?.Message);
                }
                else
                {
                    GetExecutionMessages(model, false, model.NameEn, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.NameEn, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string categoryId)
        {
            try
            {
                string formattedUrl = string.Format(APICallHelper.DeactivatBranchAccount, categoryId);
                var response = await _apiCallerHelper.DeleteAsync<ServiceResponse<bool>>(formattedUrl);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(null, true, $"Category ID: {categoryId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData?.Message ?? "Category deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message ?? "Failed to deactivate category.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Category ID: {categoryId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

    }
}

