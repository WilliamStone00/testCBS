using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.API
{
 
    public class ApiKey
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string KeyPrefix { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public string RawKey { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokedBy { get; set; }
        public string RevocationReason { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    // Entities/CreateApiKeyRequest.cs
    public class CreateApiKeyRequest
    {
        public string UserName { get; set; } = string.Empty;
        public int Length { get; set; } = 64;
    }

    // Entities/RenewApiKeyRequest.cs
    public class RenewApiKeyRequest
    {
        public string Id { get; set; } = string.Empty;
        public string newValidityPeriod { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    // Entities/RevokeApiKeyRequest.cs
    public class RevokeApiKeyRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    // Entities/ChangeStatusRequest.cs
    public class ChangeStatusRequest
    {
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class ApiKeyQuery
    {
        public DataTableOptions Options { get; set; }
        public ApiKeyQuery() { Options = new DataTableOptions(); }

        public string UserName { get; set; }
        public string Id { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public bool SortDesc { get; set; }
    }
}
