using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.RoleGlResolution
{
    public class RoleGlResolutionModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Scope is required")]
        public string Scope { get; set; } = "BRANCH"; // GLOBAL | AFFILIATE | BRANCH

        public string ScopeId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string RoleId { get; set; }

        [Required(ErrorMessage = "Branch Account is required")]
        public string BranchAccountId { get; set; }

        // For display purposes
        public string ScopeName { get; set; }
        public string RoleName { get; set; }
        public string BranchAccountName { get; set; }
    }

    public class RoleGlResolutionDto
    {
        public string Id { get; set; } 
        public string Scope { get; set; } = "BRANCH"; // GLOBAL | AFFILIATE | BRANCH
        public string ScopeId { get; set; }
        public string ScopeName { get; set; }
        public string RoleId { get; set; } 
        public string RoleName { get; set; }
        public string BranchAccountId { get; set; }
        public string BranchAccountName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GetRoleGlResolutionsDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public string Scope { get; set; }
        public string RoleId { get; set; }
        public string BranchId { get; set; }
    }

    public class CreateOrUpdateRoleGlResolutionCommand
    {
        public string Id { get; set; }
        public string Scope { get; set; }
        public string ScopeId { get; set; }
        public string RoleId { get; set; }
        public string BranchAccountId { get; set; }
    }
}
