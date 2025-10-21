using BusinessServices;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Affiliate;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.Accounting_V2.HoPcmfAccount;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.AffiliateAccounts
{
    public class AffiliateAccountMockService : BaseService
    {
        private readonly List<Affiliateresponse> _mockAffiliates;
        private readonly List<HoPcmfAccount> _mockAccounts;
        private readonly DateTime _seedNowUtc;
       
        public AffiliateAccountMockService()
        {
            _seedNowUtc = DateTime.UtcNow;
            _mockAccounts = GenerateMockAccounts();
            _mockAffiliates = GenerateMockAffiliates();
            
        }

        private List<HoPcmfAccount> GenerateMockAccounts()
        {
            return new List<HoPcmfAccount>
            {
                new HoPcmfAccount { Id = "86326916", Code = "0", NameEn = "ROOT_ACCOUNT", NameFr = "ROOT_ACCOUNT", Class = "0", ParentId = null, Path = "/0/", Depth = 0, PostingAllowed = true, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "01589560", Code = "1", NameEn = "CAPITAL FUNDS ACCOUNTS", NameFr = "CAPITAL FUNDS ACCOUNTS", Class = "1", ParentId = null, Path = "/1/", Depth = 0, PostingAllowed = false, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "54231400", Code = "10", NameEn = "CAPITAL, SHARES AND ALLOTMENTS", NameFr = "CAPITAL, PARTS SOCIALES ET DOTATIONS", Class = "1", ParentId = "01589560", Path = "/1/10/", Depth = 1, PostingAllowed = false, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "70781597", Code = "100", NameEn = "Subscribed shares called", NameFr = "Parts sociales souscrites appelées", Class = "1", ParentId = "54231400", Path = "/1/10/100/", Depth = 2, PostingAllowed = false, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "47390320", Code = "1000", NameEn = "Subscribed shares called paid in", NameFr = "Parts sociales souscrites appelées versées", Class = "1", ParentId = "70781597", Path = "/1/10/100/1000/", Depth = 3, PostingAllowed = false, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "81208436", Code = "10000", NameEn = "Shares of category", NameFr = "Parts sociales EMF 1ère catégorie", Class = "1", ParentId = "47390320", Path = "/1/10/100/1000/10000/", Depth = 4, PostingAllowed = true, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "11993592", Code = "10009", NameEn = "Shares of apex body", NameFr = "Parts sociales organe faîtier", Class = "1", ParentId = "47390320", Path = "/1/10/100/1000/10009/", Depth = 4, PostingAllowed = true, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "95726915", Code = "11", NameEn = "RESERVES", NameFr = "RESERVES", Class = "1", ParentId = "01589560", Path = "/1/11/", Depth = 1, PostingAllowed = false, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false },
                new HoPcmfAccount { Id = "70869153", Code = "111", NameEn = "Legal reserves", NameFr = "Réserves légales", Class = "1", ParentId = "95726915", Path = "/1/11/111/", Depth = 2, PostingAllowed = true, CreatedDate = _seedNowUtc, ModifiedDate = _seedNowUtc, IsDeleted = false }
            };
        }

        private List<Affiliateresponse> GenerateMockAffiliates()
        {
            return new List<Affiliateresponse>
            {
                new Affiliateresponse
                {
                    Id = "01589560",
                    Code = "1",
                    Class = "1",
                    Depth = 0,
                    PostingAllowed = true,
                    ParentId = null,
                    Path = "/1/",
                    Name = "CAPITAL FUNDS ACCOUNTS",
                    NameEn = "CAPITAL FUNDS ACCOUNTS",
                    NameFr = "COMPTES DE FONDS DE CAPITAL",
                    IsActive = true,
                    IsHeadOffice = true,
                    CreatedDate = _seedNowUtc,
                    ModifiedDate = _seedNowUtc
                },
                new Affiliateresponse
                {
                    Id = "54231400",
                    Code = "10",
                    Class = "1",
                    Depth = 1,
                    PostingAllowed = false,
                    Path = "/1/10/",
                    ParentId = "01589560",
                    Name = "CAPITAL, SHARES AND ALLOTMENTS",
                    NameEn = "CAPITAL, SHARES AND ALLOTMENTS",
                    NameFr = "CAPITAL, PARTS SOCIALES ET DOTATIONS",
                    IsActive = true,
                    IsHeadOffice = false,
                    CreatedDate = _seedNowUtc,
                    ModifiedDate = _seedNowUtc
                },
                new Affiliateresponse
                {
                    Id = "95726915",
                    Code = "11",
                    Class = "1",
                    PostingAllowed = true,
                    Path = "/1/11/",
                    Depth = 1,
                    ParentId = "01589560",
                    Name = "RESERVES",
                    NameEn = "RESERVES",
                    NameFr = "RESERVES",
                    IsActive = true,
                    IsHeadOffice = false,
                    CreatedDate = _seedNowUtc,
                    ModifiedDate = _seedNowUtc
                },
                new Affiliateresponse
                {
                    Id = "63866761",
                    Code = "14",
                    Class = "1",
                    PostingAllowed = false,
                    Path = "/1/11/",
                    Depth = 2,
                    ParentId = "01589560",
                    Name = "REGULATED PROVISIONS AND RESERVES",
                    NameEn = "REGULATED PROVISIONS AND RESERVES",
                    NameFr = "PROVISIONS ET RESERVES REGLEMENTEES",
                    IsActive = false,
                    IsHeadOffice = false,
                    CreatedDate = _seedNowUtc,
                    ModifiedDate = _seedNowUtc
                },
                new Affiliateresponse
                {
                    Id = "70869153",
                    Code = "14",
                    Class = "1",
                    PostingAllowed = false,
                    Path = "/1/11/",
                    Depth = 2,
                    ParentId = "01589560",
                    Name = "REGULATED PROVISIONS AND RESERVES",
                    NameEn = "REGULATED PROVISIONS AND RESERVES",
                    NameFr = "PROVISIONS ET RESERVES REGLEMENTEES",
                    IsActive = false,
                    IsHeadOffice = false,
                    CreatedDate = _seedNowUtc,
                    ModifiedDate = _seedNowUtc
                },

            };
        }

         public async Task<IEnumerable<Affiliateresponse>> GetAsync()
        {
            // Simulate async operation
            await Task.Delay(10);
            return _mockAffiliates.Where(a => a.IsActive).ToList();
        }


        // Simplified version
        public async Task<CustomDataTable> GetcategoryDataTableAsync(AffiliateAccountQuery query)
        {
            await Task.Delay(10);

            var filteredAffiliates = _mockAffiliates.AsQueryable();

            // Apply basic filtering
            if (!string.IsNullOrEmpty(query.Code))
            {
                filteredAffiliates = filteredAffiliates.Where(a => a.Code.Contains(query.Code));
            }

            if (!string.IsNullOrEmpty(query.Name))
            {
                filteredAffiliates = filteredAffiliates.Where(a => a.Name.Contains(query.Name));
            }
                       
            filteredAffiliates = filteredAffiliates.OrderBy(a => a.Code);

            // Apply pagination with defaults
            var start = 0;
            var length = 10;

            if (query.Options != null)
            {
                start = query.Options.start;
                length = query.Options.length > 0 ? query.Options.length : 10;

            }

            var totalRecords = filteredAffiliates.Count();
            var pagedData = filteredAffiliates
                .Skip(start)
                .Take(length)
                .ToList();


            return new CustomDataTable
            {
                data = pagedData,
                draw = Convert.ToInt32(query.Options?.draw ?? "1"),
                recordsTotal = _mockAffiliates.Count,
                recordsFiltered = totalRecords
            };
        }

        // inside AffiliateAccountMockService

        public async Task<List<Affiliateresponse>> GetAllAsync()
        {
            await Task.Delay(5);
            // return full list (including inactive if you want) - use ToList to protect underlying list
            return _mockAffiliates.ToList();
        }

        public List<Affiliateresponse> GetParentChain(string id, IEnumerable<Affiliateresponse> flatList = null)
        {
            var list = (flatList ?? _mockAffiliates).ToDictionary(a => a.Id);
            var chain = new List<Affiliateresponse>();
            if (string.IsNullOrEmpty(id) || !list.ContainsKey(id)) return chain;

            var current = list[id];
            while (current != null)
            {
                chain.Add(current);
                if (string.IsNullOrEmpty(current.ParentId)) break;
                if (!list.TryGetValue(current.ParentId, out current)) break;
            }
            chain.Reverse(); 
            return chain;
        }

        public TreeNode BuildChildrenTree(string id, IEnumerable<Affiliateresponse> flatList = null)
        {

            var list = (flatList ?? _mockAffiliates).ToList();
            if (!list.Any() || string.IsNullOrEmpty(id)) return null;

            // Exclude groups where ParentId == null to avoid a null dictionary key
            var lookup = list
                .GroupBy(a => a.ParentId)
                .Where(g => g.Key != null)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Code).ToList());

            Affiliateresponse rootItem = list.FirstOrDefault(a => a.Id == id);
            if (rootItem == null) return null;

            TreeNode Build(string nodeId)
            {
                var item = list.FirstOrDefault(a => a.Id == nodeId);
                if (item == null) return null;
                var node = new TreeNode { Item = item };

                if (lookup.TryGetValue(nodeId, out var children))
                {
                    foreach (var c in children)
                    {
                        var childNode = Build(c.Id);
                        if (childNode != null) node.Children.Add(childNode);
                    }
                }

                return node;
            }

            return Build(id);
        }

        public async Task<Affiliateresponse> GetByIdAsync(string id)
        {
            await Task.Delay(10);

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("id is required", nameof(id));

            return _mockAffiliates.FirstOrDefault(a => a.Id == id);
        }

        public async Task<ExecutionMessages> CreateAsync(AffiliateCommand model)
        {
            await Task.Delay(10);

            try
            {
                // Simulate validation
                if (string.IsNullOrEmpty(model.Name))
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, "Name is required");
                    return ExecutionMessage;
                }

                if (_mockAffiliates.Any(a => a.Code == model.Code))
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),
                        null, "Affiliate with this code already exists");
                    return ExecutionMessage;
                }

                // Create new affiliate
                var newAffiliate = new Affiliateresponse
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Code = model.Code,
                    Name = model.Name,
                    NameEn = model.NameEn,
                    NameFr = model.NameFr,
                    IsActive = true,
                    IsHeadOffice = model.IsHeadOffice,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                _mockAffiliates.Add(newAffiliate);

                GetExecutionMessages(model, true, model.Name, MessagesResults.Success,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(),
                    null, "Affiliate created successfully");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateAsync(AffiliateCommand model)
        {
            await Task.Delay(10);

            try
            {
                var existingAffiliate = _mockAffiliates.FirstOrDefault(a => a.Id == model.Id);
                if (existingAffiliate == null)
                {
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                        null, "Affiliate not found");
                    return ExecutionMessage;
                }

                // Update properties
                existingAffiliate.Name = model.Name;
                existingAffiliate.NameEn = model.NameEn;
                existingAffiliate.NameFr = model.NameFr;
                existingAffiliate.Code = model.Code;
                existingAffiliate.IsHeadOffice = model.IsHeadOffice;
                existingAffiliate.ModifiedDate = DateTime.UtcNow;

                GetExecutionMessages(model, true, model.Name, MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, "Affiliate updated successfully");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model.Name, MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeleteAsync(string affiliateId)
        {
            await Task.Delay(10);

            try
            {
                var affiliate = _mockAffiliates.FirstOrDefault(a => a.Id == affiliateId);
                if (affiliate == null)
                {
                    GetExecutionMessages(null, false, $"Affiliate ID: {affiliateId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(),
                        null, "Affiliate not found");
                    return ExecutionMessage;
                }

                // Soft delete by setting IsActive to false
                affiliate.IsActive = false;
                affiliate.ModifiedDate = DateTime.UtcNow;

                GetExecutionMessages(null, true, $"Affiliate ID: {affiliateId}", MessagesResults.Success,
                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(),
                    null, "Affiliate deactivated successfully");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Affiliate ID: {affiliateId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<IEnumerable<Affiliateresponse>> GetAffiliatesAsync()
        {
            await Task.Delay(10);

            try
            {
                var affiliates = _mockAffiliates.Where(a => a.IsActive).ToList();

                // Simulate branch filtering logic
                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option for Head Office users
                    var defaultAffiliate = new Affiliateresponse
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    affiliates.Insert(0, defaultAffiliate);
                }

                // Format name for display and order by Code
                return affiliates
                    .Select(a =>
                    {
                        a.Name = $"[{a.Code}] - {a.Name} {(a.IsHeadOffice ? "(Head Office)" : string.Empty)}";
                        return a;
                    })
                    .OrderBy(a => a.Id)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log exception in real scenario
                throw;
            }
        }

       
        // Additional helper methods for testing
        public List<HoPcmfAccount> GetMockAccounts() => _mockAccounts;
        public List<Affiliateresponse> GetMockAffiliates() => _mockAffiliates;

        public void ResetData()
        {
            _mockAffiliates.Clear();
            _mockAffiliates.AddRange(GenerateMockAffiliates());
        }

        public async Task<IEnumerable<Affiliateresponse>> GetAffiliatesFromMockAsync()
        {
            try
            {
                // Pull active affiliates from the mock service (mock simulates async I/O)
                var affiliates = (await GetAsync()).ToList(); // GetAsync returns only IsActive

                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    affiliates = affiliates.Where(a => a.Id == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option for Head Office users (same behavior as endpoint version)
                    var defaultAffiliate = new Affiliateresponse
                    {
                        Id = "All",
                        Name = "All Affiliates",
                        Code = "ALL",
                        IsActive = true
                    };
                    if (!affiliates.Any(x => string.Equals(x.Id, defaultAffiliate.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        affiliates.Insert(0, defaultAffiliate);
                    }
                }

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
                // Consider logging
                throw;
            }
        }

    }
}

