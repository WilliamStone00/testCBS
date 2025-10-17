using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
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
        public async Task<HoPcmfAccount> GetAccountByIdAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId)) return null;

            try
            {
                // If your APICallHelper endpoint is formatted like Get_Update_Delete_ChartOfAccount (id placeholder), use string.Format
                var url = string.Format(APICallHelper.GetAccountByIdEndpoint, accountId);
                var response = await _apiHelper.GetAsync<ResponseObject<HoPcmfAccount>>(url);

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

        /// <summary>
        /// Calls the API to update an account name. Returns ExecutionMessages (same pattern as BaseApiServices).
        /// </summary>
        public async Task<ExecutionMessages> UpdateAccountNameAsync(string accountId, string newNameEn, string newNameFr)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                GetExecutionMessages(null, false, "Account Name", MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Invalid account id");
                return ExecutionMessage;
            }

            try
            {
                // Prepare payload. Adjust the body type to match the API contract if different.
                var payload = new
                {
                    Id = accountId,
                    NameEn = newNameEn,
                    NameFr = newNameFr
                };

                // If APICallHelper has a dedicated endpoint for this action, use it; otherwise use the generic update endpoint
                string endpoint;
                if (!string.IsNullOrEmpty(APICallHelper.UpdateAccountNameEndpoint))
                {
                    endpoint = APICallHelper.UpdateAccountNameEndpoint; // e.g. "api/v2/accounts/{0}/name" or similar — implement in APICallHelper
                }
                else
                {
                    endpoint = string.Format(APICallHelper.GetAccountByIdEndpoint, accountId); // fallback: PUT to update resource
                }

                // Use PutAsync or PostAsync depending on your API. Here we try PutAsync as a safe default for updates.
                var response = await _apiHelper.PutAsync<ServiceResponse<bool>>(endpoint, payload);

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
        public async Task<HoPcmfAccount> GetAccountDetailsAsync(string accountId)
        {
            var acc = await GetAccountByIdAsync(accountId);
            if (acc == null) return null;

            // Map to a lighter DTO if needed; currently returning same entity
            return new HoPcmfAccount
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
    }
   

}
