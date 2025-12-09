using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeGroup
{
    public class AccountTypeGroupResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<object> AccountTypes { get; set; } = new List<object>();

        // For display purposes
        public string StatusBadge => IsActive ? "Active" : "Inactive";
        public string StatusClass => IsActive ? "bg-success" : "bg-danger";
    }

    public class AccountTypeGroupRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}