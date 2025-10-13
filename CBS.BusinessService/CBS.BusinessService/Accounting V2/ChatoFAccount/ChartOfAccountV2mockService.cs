using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2
{
    public class ChartOfAccountsV2mockService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;
        private static readonly List<HoPcmfAccount> _mockAccountData;

        static ChartOfAccountsV2mockService()
        {
            var seedNowUtc = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

            _mockAccountData = new List<HoPcmfAccount>
            {
                new HoPcmfAccount { Id = "86326916", Code = "0", NameEn = "ROOT_ACCOUNT", NameFr = "ROOT_ACCOUNTkk", Class = "0", ParentId = null, Path = "/0/", Depth = 0, PostingAllowed = true, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "01589560", Code = "1", NameEn = "CAPITAL FUNDS ACCOUNTS", NameFr = "CAPITAL FUNDS ACCOUNTS", Class = "1", ParentId = null, Path = "/1/", Depth = 0, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "54231400", Code = "10", NameEn = "CAPITAL, SHARES AND ALLOTMENTS", NameFr = "CAPITAL, PARTS SOCIALES ET DOTATIONS", Class = "1", ParentId = "01589560", Path = "/1/10/", Depth = 1, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "70781597", Code = "100", NameEn = "Subscribed shares called", NameFr = "Parts sociales souscrites appelées", Class = "1", ParentId = "54231400", Path = "/1/10/100/", Depth = 2, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "47390320", Code = "1000", NameEn = "Subscribed shares called paid in", NameFr = "Parts sociales souscrites appelées versées", Class = "1", ParentId = "70781597", Path = "/1/10/100/1000/", Depth = 3, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "90858811", Code = "26929", NameEn = "Attached claims on other permanent financial investment network", NameFr = "Créances rattachées sur autres immobilisations financières réseau", Class = "2", ParentId = "16590310", Path = "/2/26/269/2692/26929/", Depth = 4, PostingAllowed = true, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "01589560", Code = "2", NameEn = "CAPITAL FUNDS ACCOUNTS", NameFr = "CAPITAL FUNDS ACCOUNTS", Class = "1", ParentId = null, Path = "/1/", Depth = 0, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "54231400", Code = "20", NameEn = "CAPITAL, SHARES AND ALLOTMENTS", NameFr = "CAPITAL, PARTS SOCIALES ET DOTATIONS", Class = "1", ParentId = "01589560", Path = "/1/10/", Depth = 1, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "70781597", Code = "200", NameEn = "Subscribed shares called", NameFr = "Parts sociales souscrites appelées", Class = "1", ParentId = "54231400", Path = "/1/10/100/", Depth = 2, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "47390320", Code = "2000", NameEn = "Subscribed shares called paid in", NameFr = "Parts sociales souscrites appelées versées", Class = "1", ParentId = "70781597", Path = "/1/10/100/1000/", Depth = 3, PostingAllowed = false, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "90858811", Code = "46929", NameEn = "Attached claims on other permanent financial investment network", NameFr = "Créances rattachées sur autres immobilisations financières réseau", Class = "2", ParentId = "16590310", Path = "/2/26/269/2692/26929/", Depth = 4, PostingAllowed = true, CreatedDate = seedNowUtc, ModifiedDate = seedNowUtc, IsDeleted = false }
            };
        }

        public ChartOfAccountsV2mockService()
        {
            var baseUrl = ConfigurationManager.AppSettings["AccountingBaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<List<AccountTreeNode>> GetAccountTreeForJsTreeAsync(string branchId = null)
        {
            try
            {
                // Simulate API delay
                await Task.Delay(500);

                var flatList = await GetAllAccountsFromApiAsync();
                if (flatList == null || !flatList.Any())
                    return new List<AccountTreeNode>();

                // Build tree structure properly
                var nodeDictionary = flatList.ToDictionary(
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
                        IsDeleted = acc.IsDeleted,
                        Children = new List<HoPcmfAccountTreeDto>()
                    });

                var rootNodes = new List<HoPcmfAccountTreeDto>();

                foreach (var acc in flatList)
                {
                    if (string.IsNullOrEmpty(acc.ParentId))
                    {
                        // This is a root node
                        rootNodes.Add(nodeDictionary[acc.Id]);
                    }
                    else if (nodeDictionary.ContainsKey(acc.ParentId))
                    {
                        // Add as child to parent
                        nodeDictionary[acc.ParentId].Children.Add(nodeDictionary[acc.Id]);
                    }
                }

                var result = rootNodes.Select(ConvertToJsTreeNode).ToList();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChartOfAccountsV2Service.GetAccountTreeForJsTreeAsync error: {ex}");
                return new List<AccountTreeNode>();
            }
        }

        public async Task<HoPcmfAccount> GetAccountByIdAsync(string accountId)
        {
            await Task.Delay(100); // Simulate API call
            return _mockAccountData.FirstOrDefault(a =>
                string.Equals(a.Id, accountId, StringComparison.OrdinalIgnoreCase));
        }

        public Task<ExecutionMessages> UpdateAccountNameAsync(string accountId, string newNameEn, string newNameFr)
        {
            var account = _mockAccountData.FirstOrDefault(a =>
                string.Equals(a.Id, accountId, StringComparison.OrdinalIgnoreCase));

            if (account != null)
            {
                account.NameEn = newNameEn ?? account.NameEn;
                account.NameFr = newNameFr ?? account.NameFr;
                account.ModifiedDate = DateTime.UtcNow;

                GetExecutionMessages(account, true, "Account Name", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                    "Account name updated successfully.");
            }
            else
            {
                GetExecutionMessages(null, false, "Account Name", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                    "Account not found.");
            }

            return Task.FromResult(ExecutionMessage);
        }

        public async Task<HoPcmfAccount> GetAccountDetailsAsync(string accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null) return null;

            return new HoPcmfAccount
            {
                Id = account.Id,
                Code = account.Code,
                NameEn = account.NameEn,
                NameFr = account.NameFr,
                Class = account.Class,
                PostingAllowed = account.PostingAllowed,
                Depth = account.Depth,
                Path = account.Path,
                CreatedDate = account.CreatedDate,
                ModifiedDate = account.ModifiedDate,
                ParentId = account.ParentId
            };
        }

        private AccountTreeNode ConvertToJsTreeNode(HoPcmfAccountTreeDto node)
        {
            var jsNode = new AccountTreeNode
            {
                id = node.Id,
                text = $"{node.Code} - {node.NameEn}",
                icon = GetIconForNode(node),
                state = new State
                {
                    opened = node.Depth < 2,
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
                children = node.Children?.Any() == true ?
                    node.Children.Select(ConvertToJsTreeNode).ToList() :
                    new List<AccountTreeNode>()
            };

            return jsNode;
        }

        private string GetIconForNode(HoPcmfAccountTreeDto node)
        {
            if (node.PostingAllowed)
            {
                return "fas fa-file-alt text-success";
            }
            return node.Depth == 0 ? "fas fa-server text-warning" : "fas fa-folder text-primary";
        }

        private Task<List<HoPcmfAccount>> GetAllAccountsFromApiAsync()
        {
            return Task.FromResult(_mockAccountData.ToList());
        }
    }

}