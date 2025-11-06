using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.PendingAccount;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
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

namespace CBS.BusinessService.Accounting_V2.AffiliateAccounts
{
    public class AffiliateAccountService : BaseService
    {
        private readonly List<AffiliateAccountDto> _mockAffiliates;
        private readonly ApiCallerHelper _apiCallerHelper;

            public AffiliateAccountService()
            {
                //change the base url to the actual base url
                string baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new ConfigurationErrorsException("The 'AccountingV2BaseUrl' appSetting is missing or empty in Web.config.");
                }
                _apiCallerHelper = new ApiCallerHelper(baseUrl);
            }

            public async Task<IEnumerable<AffiliateAccountDto>> GetAsync()
            {
                try
                {
                    // CORRECTED: The helper returns an ApiResponse which contains the ServiceResponse
                    var response = await _apiCallerHelper.GetAsync<ServiceResponse<List<AffiliateAccountDto>>>(APICallHelper.GetAllAffiliateAccount);

                    // CORRECTED: Access the final payload via .ApiResponseData.Data
                    if (response.IsSuccess && response.ApiResponseData?.Data != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return new List<AffiliateAccountDto>();
                }
                catch (Exception ex)
                {
                    // In a real scenario, log 'ex'
                    throw;
            }
        }

        public async Task<CustomDataTable2> GetcategoryDataTableAsync(AffiliateAccountQuery query)
        {
            try
            {
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.AffiliateAccountdatatable, query);

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

        public async Task<IEnumerable<AffiliateAccountDto>> GetAllAffiliateAccounts1()
        {
            try
            {               

                var query = new AffiliateAccountQuery
                {
                    
                    AffiliateId = "1"
                };

                // Reuse existing method which calls the API datatable endpoint
                var dataTable = await GetcategoryDataTableAsync(query);

                // Convert datatable.data to strongly-typed list safely
                List<AffiliateAccountDto> branchList = new List<AffiliateAccountDto>();

                if (dataTable?.data is JToken token)
                {
                    branchList = token.ToObject<List<AffiliateAccountDto>>() ?? new List<AffiliateAccountDto>();
                }
                else if (dataTable?.data != null)
                {
                    // Fallback if data is plain object
                    branchList = JsonConvert.DeserializeObject<List<AffiliateAccountDto>>(JsonConvert.SerializeObject(dataTable.data))
                                 ?? new List<AffiliateAccountDto>();
                }

                // Optional: server may already have enforced branch filtering. If you still want to
                // double-check client-side for non-HO users, do case-insensitive compare:
                if (!IsHeadOffice())
                {
                    var currentBranch = GetBranchID();
                    branchList = branchList.Where(a => string.Equals(a.Id, currentBranch, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else
                {
                    // Ensure "All" entry exists for HO users
                    var defaultAffiliate = new AffiliateAccountDto { Id = "All", Name = "All Affiliates", Code = "ALL" };
                    if (!branchList.Any(x => string.Equals(x.Id, defaultAffiliate.Id, StringComparison.OrdinalIgnoreCase)))
                        branchList.Insert(0, defaultAffiliate);
                }

                var formatted = branchList
                    .Select(a => new AffiliateAccountDto
                    {
                        Id = a.Id,
                        Code = a.Code,
                        Name = $"[{a.Code}] - {a.Name}".Trim()
                    })
                    .OrderBy(a => a.Name) // nicer UX for dropdown; change if you prefer Id
                    .ToList();

                return formatted;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<AffiliateAccountDto>> GetAllAffiliateAccounts()
        {
            try
            {
                AffiliateAccountQuery query = new AffiliateAccountQuery()
                {
                    Options = new DataTableOptions() { lang = GetUserLanguage() },
                    AffiliateId = "1"

                };
                var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.AffiliateAccountdatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess || response.ApiResponseData == null)
                {
                    return new List<AffiliateAccountDto>();
                }


                var affiliateAccounts = JsonConvert.DeserializeObject<List<AffiliateAccountDto>>(JsonConvert.SerializeObject(response.ApiResponseData.Data.data));

                return affiliateAccounts;
            }
            catch (Exception ex)
            {
                return new List<AffiliateAccountDto>();
            }
        }



        //tree structure 
        public async Task<AffiliateAccountDto> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);

                var lan = GetLanguage();

                string formattedUrl = string.Format(APICallHelper.GetAffiliateAccountById, encodedId,lan);
                // formattedUrl => "/api/v1/get-checkbook-category/123" (no colon)

                var response = await _apiCallerHelper.GetAsync<ServiceResponse<AffiliateAccountDto>>(formattedUrl);

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

        //***************************************** details ****************************

        //public async Task<AffiliateAccountDto> GetByIdAsync(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id)) return null;

        //    try
        //    {
        //        var encoded = Uri.EscapeDataString(id);
        //        var url = string.Format(APICallHelper.GetAffiliateAccountById, encoded);
        //        var resp = await _apiCallerHelper.GetAsync<ServiceResponse<AffiliateAccountDto>>(url);
        //        if (resp != null && resp.IsSuccess)
        //        {
        //            return resp.ApiResponseData?.Data;
        //        }
        //        return null;
        //    }
        //    catch
        //    {
        //        // log if you have logging here
        //        throw;
        //    }
        //}

        //public async Task<IEnumerable<AffiliateAccountDto>> GetAsync()
        //{
        //    try
        //    {
        //        var url = APICallHelper.GetAffiliates; // replace with real constant
        //        var resp = await _apiCallerHelper.GetAsync<ServiceResponse<IEnumerable<AffiliateAccountDto>>>(url);
        //        if (resp != null && resp.IsSuccess)
        //        {
        //            return resp.ApiResponseData?.Data ?? Enumerable.Empty<AffiliateAccountDto>();
        //        }
        //        return Enumerable.Empty<AffiliateAccountDto>();
        //    }
        //    catch
        //    {
        //        // log if needed
        //        throw;
        //    }
        //}

        // ---------- Sync tree/chain helpers (use flat list when available for efficiency) ----------

        public List<AffiliateAccountDto> GetParentChain(string id, IEnumerable<AffiliateAccountDto> flatList = null)
        {
            var chain = new List<AffiliateAccountDto>();
            if (string.IsNullOrWhiteSpace(id)) return chain;

            var list = (flatList ?? Enumerable.Empty<AffiliateAccountDto>()).ToDictionarySafe(a => a.Id);
            if (list == null || !list.ContainsKey(id))
            {
                // If no flat list or id missing, return empty (caller can call async version)
                return chain;
            }

            var current = list[id];
            while (current != null)
            {
                chain.Add(current);
                if (string.IsNullOrWhiteSpace(current.ParentId)) break;
                if (!list.TryGetValue(current.ParentId, out current)) break;
            }

            chain.Reverse();
            return chain;
        }

        public TreeNodeaccounting BuildChildrenTree(string id, IEnumerable<AffiliateAccountDto> flatList = null)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var list = (flatList ?? Enumerable.Empty<AffiliateAccountDto>()).ToList();
            if (!list.Any()) return null;

            // ParentId -> children list (allow null keys but we only use non-null lookup keys)
            //var lookup = list.GroupBy(a => a.ParentId)
            //                 .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Code).ToList());

            var lookup = list
              .GroupBy(a => a.ParentId)
              .Where(g => g.Key != null)
              .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Code).ToList());

            AffiliateAccountDto rootItem = list.FirstOrDefault(a => a.Id == id);
            if (rootItem == null) return null;

            TreeNodeaccounting Build(string nodeId)
            {
                var item = list.FirstOrDefault(a => a.Id == nodeId);
                if (item == null) return null;
                var node = new TreeNodeaccounting { Item = item, Children = new List<TreeNodeaccounting>() };

                if (lookup.TryGetValue(nodeId, out var children))
                {
                    foreach (var c in children)
                    {
                        var childNode = Build(c.Id);
                        if (childNode != null) node.Children.Add(childNode);
                    }
                }

                node.Count = node.Children?.Count ?? 0; // direct children count (change if you need subtree count)
                return node;
            }

            return Build(id);
        }

        // ---------- Async wrappers if caller didn't fetch the flat list ----------

        public async Task<List<AffiliateAccountDto>> GetParentChainAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return new List<AffiliateAccountDto>();

            // Prefer walking via GetByIdAsync to avoid fetching all records
            var chain = new List<AffiliateAccountDto>();
            var current = await GetByIdAsync(id);
            if (current == null) return chain;

            while (current != null)
            {
                chain.Add(current);
                if (string.IsNullOrWhiteSpace(current.ParentId)) break;
                try
                {
                    current = await GetByIdAsync(current.ParentId);
                }
                catch
                {
                    break;
                }
            }

            chain.Reverse();
            return chain;
        }

        public async Task<TreeNodeaccounting> BuildChildrenTreeAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var flat = (await GetAsync())?.ToList();
            if (flat == null || !flat.Any()) return null;

            return BuildChildrenTree(id, flat);
        }
    


     //***************************************** end of details ****************************

        // real endpoint version
        public async Task<IEnumerable<AffiliateAccountDto>> GetAffiliatesFromEndpointAsync()
        {
            try
            {
                var Affiliate = "1";
                string formattedUrl = string.Format(APICallHelper.GetAllAccountDropAffiliate, Affiliate);
                var response = await _apiCallerHelper.GetAsync<ResponseObject<List<AffiliateAccountDto>>>(formattedUrl);
                var affiliates = response?.ApiResponseData?.Data ?? new List<AffiliateAccountDto>();

                if (!IsHeadOffice())
                {
                    // Filter only the affiliate that matches current branch (if applicable)
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option at the top for Head Office users
                    var defaultAffiliate = new AffiliateAccountDto
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    // ensure we don't duplicate if API already returns such entry
                    if (!affiliates.Any(x => string.Equals(x.Id, defaultAffiliate.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        affiliates.Insert(0, defaultAffiliate);
                    }
                }

                // Format name for display and order by Code
                return affiliates
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name} {(a.IsHeadOffice ? "(Head Office)" : string.Empty)}".Trim();
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



        public async Task<ExecutionMessages> CreateAsync(PendingAccountRequest model)
        {
                try
                {
                    var response = await _apiCallerHelper.PostAsync<ServiceResponse<PendingAccountRequest>>(APICallHelper.CreateAffiliateAccount, model);

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

            public async Task<ExecutionMessages> UpdateAsync(PendingAccountRequest model)
            {
                try
                {
                    var catid = model.Id;
                    string formattedUrl = string.Format(APICallHelper.UpdateAffiliateAccount, catid);
                    var response = await _apiCallerHelper.PutAsync<ServiceResponse<AffiliateCommand>>(formattedUrl, model);

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
                    string formattedUrl = string.Format(APICallHelper.DeactivateAffiliateAccount, categoryId);
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



public static class EnumerableExtensions
{
    public static Dictionary<string, T> ToDictionarySafe<T>(this IEnumerable<T> items, Func<T, string> keySelector)
    {
        if (items == null) return new Dictionary<string, T>();
        var dict = new Dictionary<string, T>();
        foreach (var i in items)
        {
            try
            {
                var key = keySelector(i);
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!dict.ContainsKey(key)) dict[key] = i;
            }
            catch { /* ignore items that throw */ }
        }
        return dict;
    }
}
