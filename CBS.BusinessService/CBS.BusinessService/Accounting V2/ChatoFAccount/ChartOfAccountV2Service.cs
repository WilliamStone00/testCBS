using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.BranchAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2
{
    /// <summary>
    /// Real service implementation that calls the Accounting V2 REST API (via ApiCallerHelper)
    /// based on the structure used in ChartOfAccountServices and the mock implementation.
    /// </summary>
    public class ChartOfAccountsV2Service : BaseApiServices
    {
        private readonly ApiCallerHelper _apiHelper;


        public ChartOfAccountsV2Service()
        {
            //change the base url to the actual base url
            var baseUrl = ConfigurationManager.AppSettings["AccountingV2BaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        /// <summary>
        /// Fetches a flat list of accounts from the API and returns a jsTree-compatible nested list.
        /// </summary>
        public async Task<List<AccountTreeNode>> GetAccountTreeForJsTreeAsync(string branchId = null)
        {
            try
            {
                // Call the API to get all accounts (flat)
                // Expecting ResponseObject<List<HoPcmfAccount>> or similar; adjust generic type if your API returns DTOs
                var apiResponse = await _apiHelper.GetAsync<ResponseObject<List<HoPcmfAccount>>>(APICallHelper.GetAllAccountsEndpoint);

                if (apiResponse == null || !apiResponse.IsSuccess || apiResponse.ApiResponseData?.Data == null)
                {
                    return new List<AccountTreeNode>();
                }

                var flatList = apiResponse.ApiResponseData.Data;

                // Build dictionary of DTO -> tree DTO (so we can attach children)
                var nodeDict = flatList.ToDictionary(
                    acc => acc.Id,
                    acc => new HoPcmfAccountTreeDto
                    {
                        Id = acc.Id,
                        Code = acc.Code,
                        NameEn = acc.NameEn,
                        NameFr = acc.NameFr,
                        Class = acc.Class,
                        ParentId = acc.ParentId,
                        PostingAllowed = acc.PostingAllowed,
                        Depth = acc.Depth,
                        Path = acc.Path,
                        CreatedDate = acc.CreatedDate,
                        ModifiedDate = acc.ModifiedDate,
                        IsDeleted = acc.IsDeleted,
                        Children = new List<HoPcmfAccountTreeDto>()
                    });

                // Collect roots, attach children to parents when parent exists
                var roots = new List<HoPcmfAccountTreeDto>();
                foreach (var acc in flatList)
                {
                    if (string.IsNullOrWhiteSpace(acc.ParentId) || !nodeDict.ContainsKey(acc.ParentId))
                    {
                        // If parent is missing -> treat as root (orphan), but still allow client to show/hide under root
                        roots.Add(nodeDict[acc.Id]);
                    }
                    else
                    {
                        nodeDict[acc.ParentId].Children.Add(nodeDict[acc.Id]);
                    }
                }

                // Convert to jsTree nodes
                var result = roots.Select(ConvertToJsTreeNode).ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChartOfAccountsV2Service.GetAccountTreeForJsTreeAsync error: {ex}");
                return new List<AccountTreeNode>();
            }
        }

        /// <summary>
        /// Returns the account entity fetched from the API (raw).
        /// </summary>
        public async Task<HoPcmfAccountTreeDto> GetAccountByIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId)) return null;
            var lang = GetLanguage();
            try
            {
                if (string.IsNullOrWhiteSpace(accountId)) throw new ArgumentNullException(nameof(accountId));
                lang = string.IsNullOrWhiteSpace(lang) ? "en" : lang;

                // URL-encode path segments to be safe
                var idEscaped = Uri.EscapeDataString(accountId);
                var langEscaped = Uri.EscapeDataString(lang);

                var url = string.Format(APICallHelper.GetAccountByIdEndpoint, idEscaped, langEscaped);

                var response = await _apiHelper.GetAsync<ResponseObject<HoPcmfAccountTreeDto>>(url);

                if (response != null && response.IsSuccess && response.ApiResponseData != null)
                    return response.ApiResponseData.Data;

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAccountByIdAsync error: {ex}");
                throw;
            }
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetHoPcmfCoaQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.COAdatatable, query);

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
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

        public string GetClassNumericValue(string textValue)
        {
            if (string.IsNullOrWhiteSpace(textValue))
                return "0";

            switch (textValue.Trim().ToUpperInvariant())
            {
                case "EQUITY":
                    return "1";

                case "FIXED ASSETS":
                    return "2";

                case "MEMBERS":
                    return "3";

                case "THIRD PARTIES":
                    return "4";

                case "TREASURY":
                    return "5";

                case "EXPENSES":
                    return "6";

                case "REVENUE":
                    return "7";

                case "OUTSIDE ORDINARY ACTIVITIES (OOA)":
                    return "8";

                case "MANAGEMENT":
                    return "9";

                default:
                    return "0"; // fallback for unrecognized or invalid text
            }
        }


        public string GetClassTextValue(string numericValue)
        {
            switch (numericValue)
            {
                case "1":
                    return "EQUITY";

                case "2":
                    return "FIXED ASSETS";

                case "3":
                    return "MEMBERS";

                case "4":
                    return "THIRD PARTIES";

                case "5":
                    return "TREASURY";

                case "6":
                    return "EXPENSES";

                case "7":
                    return "REVENUE";

                case "8":
                    return "OUTSIDE ORDINARY ACTIVITIES (OOA)";

                case "9":
                    return "MANAGEMENT";

                default:
                    return "UNKNOWN"; // fallback for invalid or unmapped values
            }
        }



        public List<StringValues> GetAllClass()
        {
            return new List<StringValues>()
            {
                new StringValues("EQUITY","1"),
                new StringValues("FIXED ASSETS","2"),
                new StringValues("MEMBERS", "3"),
                new StringValues("THIRD PARTIES","4"),
                new StringValues("TREASURY", "5"),
                new StringValues("EXPENSES", "6"),
                new StringValues("REVENUE","7"),
                new StringValues("OUTSIDE ORDINARY ACTIVITIES (OOA)", "8"),
                new StringValues("MANAGEMENT","9"),

            };
        }
        public List<StringValues> GetAllClass2()
        {
            return new List<StringValues>()
            {
                new StringValues("EQUITY","EQUITY"),
                new StringValues("FIXED ASSETS","FIXED ASSETS"),
                new StringValues("MEMBERS", "MEMBERS"),
                new StringValues("THIRD PARTIES","THIRD PARTIES"),
                new StringValues("TREASURY", "TREASURY"),
                new StringValues("EXPENSES", "EXPENSES"),
                new StringValues("REVENUE","REVENUE"),
                new StringValues("OUTSIDE ORDINARY ACTIVITIES (OOA)", "OUTSIDE ORDINARY ACTIVITIES (OOA)"),
                new StringValues("MANAGEMENT","MANAGEMENT"),

            };
        }

        public async Task<List<StringValues>> GetAllPCMFAccounts()
        {
            try
            {
                GetHoPcmfCoaQuery query = new GetHoPcmfCoaQuery()
                {
                    Options = new DataTableOptions() { lang=GetUserLanguage() }

                };
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.COAdatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess || response.ApiResponseData == null)
                {
                    return new List<StringValues>();
                }


                var serialise = JsonConvert.DeserializeObject<List<HoPcmfAccountTreeDto>>(
                    JsonConvert.SerializeObject(response.ApiResponseData.Data.data));

                // Convert HoPcmfAccount to StringValues
                var stringValuesList = serialise.Select(account => new StringValues
                {
                    Text = account.Code+"-"+ account.Name, // or account.NameFr based on your language preference
                    Value = account.Id     // or account.Code depending on what you want as value
                }).ToList();

                return stringValuesList;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                return new List<StringValues>();
            }
        }
        public async Task<List<StringValues>> GetAllPCMFAccountsByClass(string cls)
        {
            try
            {
                GetHoPcmfCoaQuery query = new GetHoPcmfCoaQuery()
                {
                    Options = new DataTableOptions() { lang=GetUserLanguage() },
                    Class=cls

                };

                query.Options.pageSize = 30000;
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.COAdatatable, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess || response.ApiResponseData == null)
                {
                    return new List<StringValues>();
                }


                var serialise = JsonConvert.DeserializeObject<List<HoPcmfAccountTreeDto>>(
                    JsonConvert.SerializeObject(response.ApiResponseData.Data.data));

                // Convert HoPcmfAccount to StringValues
                var stringValuesList = serialise.Select(account => new StringValues
                {
                    Text = account.Code+"-"+ account.Name, // or account.NameFr based on your language preference
                    Value = account.Id     // or account.Code depending on what you want as value
                }).ToList();

                return stringValuesList;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                return new List<StringValues>();
            }
        }

        /// <summary>
        /// Maps a collection of HoPcmfAccountTreeDto to a new list where AccountNumber
        /// is parsed from Code. If Code is null/empty or not an integer, AccountNumber becomes 0.
        /// Mapping is applied recursively to children.
        /// </summary>
        public  List<HoPcmfAccountTreeDto> MapCodesToAccountNumbers(List<HoPcmfAccountTreeDto> source)
        {
            if (source == null) return new List<HoPcmfAccountTreeDto>();
            return source.Select(MapNode).ToList();
        }

        private  HoPcmfAccountTreeDto MapNode(HoPcmfAccountTreeDto node)
        {
            if (node == null) return new HoPcmfAccountTreeDto();

            return new HoPcmfAccountTreeDto
            {
                Id = node.Id,
                Code = node.Code,
                // parse Code to AccountNumber, fallback to 0 when empty or non-integer
                AccountNumber = ParseCodeToInt(node.Code),
                Name = node.Name,
                NameEn = node.NameEn,
                NameFr = node.NameFr,
                Class = node.Class,
                ParentId = node.ParentId,
                PostingAllowed = node.PostingAllowed,
                Depth = node.Depth,
                Path = node.Path,
                CreatedDate = node.CreatedDate,
                ModifiedDate = node.ModifiedDate,
                IsDeleted = node.IsDeleted,
                Children = node.Children == null
                    ? new List<HoPcmfAccountTreeDto>()
                    : node.Children.Select(MapNode).ToList()
            };
        }

        private static int ParseCodeToInt(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return 0;
            return int.TryParse(code, out var value) ? value : 0;
        }

        /// <summary>
        /// Calls the API to update an account name. Returns ExecutionMessages (same pattern as BaseApiServices).
        /// </summary>
        public async Task<ExecutionMessages> UpdateAccountNameAsync(UpdateAccountNameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                GetExecutionMessages(null, false, "Account Name", MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Invalid account id");
                return ExecutionMessage;
            }

            try
            {                
                // If APICallHelper has a dedicated endpoint for this action, use it; otherwise use the generic update endpoint
                var encodedId = Uri.EscapeDataString(request.Id);
                string endpoint = string.Format(APICallHelper.UpdateAccountNameEndpoint, encodedId); 
                // Use PutAsync or PostAsync depending on your API. Here we try PutAsync as a safe default for updates.
                var response = await _apiHelper.PutAsync<ServiceResponse<HoPcmfAccount>>(endpoint, request);
                if (response != null && response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Account Name", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.Message ?? "Account updated.");
                }
                else
                {
                    GetExecutionMessages(response, false, "Account Name", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response?.Message ?? "Failed to update account.");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Account Name", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        /// <summary>
        /// Returns a filtered/detailed account DTO suitable for the UI.
        /// </summary>
        public async Task<HoPcmfAccountTreeDto> GetAccountDetailsAsync(string accountId)
        {
            var acc = await GetAccountByIdAsync(accountId);
            if (acc == null) return null;

            // Map to a lighter DTO if needed; currently returning same entity
            return new HoPcmfAccountTreeDto
            {
                Id = acc.Id,
                Code = acc.Code,
                NameEn = acc.NameEn,
                NameFr = acc.NameFr,
                Class = acc.Class,
                PostingAllowed = acc.PostingAllowed,
                Depth = acc.Depth,
                Path = acc.Path,
                CreatedDate = acc.CreatedDate,
                ModifiedDate = acc.ModifiedDate,
                ParentId = acc.ParentId,
                IsDeleted = acc.IsDeleted
            };
        }

        /// <summary>
        /// Fetches raw flat account list from the API (used by other services).
        /// </summary>
        public async Task<List<HoPcmfAccount>> GetAllAccountsFromApiAsync()
        {
            try
            {
                var apiResponse = await _apiHelper.GetAsync<ResponseObject<List<HoPcmfAccount>>>(APICallHelper.GetAllAccountsEndpoint);
                if (apiResponse != null && apiResponse.IsSuccess && apiResponse.ApiResponseData != null)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<HoPcmfAccount>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAccountsFromApiAsync error: {ex}");
                return new List<HoPcmfAccount>();
            }
        }

        // --------------------------
        // PRIVATE HELPERS
        // --------------------------

        /// <summary>
        /// Convert a HoPcmfAccountTreeDto to the jsTree AccountTreeNode expected by the front-end.
        /// Sets node.type and children=false for leaves to prevent caret rendering.
        /// Sets state.opened = false by default so roots are closed initially (frontend controls expansion).
        /// </summary>
        private AccountTreeNode ConvertToJsTreeNode(HoPcmfAccountTreeDto node)
        {
            if (node == null) return null;

            // Determine if this node has children
            var hasChildren = node.Children != null && node.Children.Any();

            // Determine postingAllowed/leaf
            var isLeaf = node.PostingAllowed;

            var jsNode = new AccountTreeNode
            {
                id = node.Id,
                text = $"{node.Code} - {node.NameEn}",
                // prefer type property (read by jsTree types plugin)
                // json `type` is expected by jsTree; include it in the object if your AccountTreeNode supports it
                // If AccountTreeNode has no 'type' property, keep it in li_attr and let client normalize.
                state = new State
                {
                    opened = false, // closed by default
                    disabled = false,
                    selected = false
                },
                li_attr = new Dictionary<string, object>
                {
                    { "data-name-en", node.NameEn ?? string.Empty },
                    { "data-name-fr", node.NameFr ?? string.Empty },
                    { "data-code", node.Code ?? string.Empty },
                    { "data-class", node.Class ?? string.Empty },
                    { "data-path", node.Path ?? string.Empty },
                    { "data-postingallowed", node.PostingAllowed.ToString().ToLower() },
                    { "data-depth", node.Depth.ToString() }
                },
                // children: if leaf, set to false so jsTree does not show caret; else set children list
                children = node.Children?.Any() == true ? node.Children.Select(ConvertToJsTreeNode).ToList() : new List<AccountTreeNode>()
            };

            // If AccountTreeNode type property exists at runtime, assign it via reflection (best-effort)
            try
            {
                var typeProp = jsNode.GetType().GetProperty("type");
                if (typeProp != null && typeProp.CanWrite)
                {
                    typeProp.SetValue(jsNode, isLeaf ? "file" : "folder");
                }
            }
            catch
            {
                // ignore - not critical
            }

            return jsNode;
        }

        /// <summary>
        /// Utility to choose an icon class for the node (used by earlier mock). Front-end types mapping preferred.
        /// </summary>
        private string GetIconForNode(HoPcmfAccountTreeDto node)
        {
            if (node.PostingAllowed)
                return "fas fa-file-alt text-success";

            // depth 0 could be root server icon
            return node.Depth == 0 ? "fas fa-server text-warning" : "fas fa-folder text-primary";
        }

        public async Task<ApiResponse<ServiceResponse<ChartOfAccountFileUpload>>> COAUpload(ChartOfAccountFileUpload model)
        {
            if (model?.File == null)
                throw new ArgumentNullException(nameof(model.File), "Please select a file to upload.");

            // ✅ API endpoint for Shared Month upload
            var endpoint = APICallHelper.SharedMonthUpload;

            // 🚀 Upload only the file
            var result = await _apiHelper.UploadCOAFileAsync<
                ServiceResponse<ChartOfAccountFileUpload>>(
                    model.File,
                    endpoint
            );

            return result;
        }

    }


}
