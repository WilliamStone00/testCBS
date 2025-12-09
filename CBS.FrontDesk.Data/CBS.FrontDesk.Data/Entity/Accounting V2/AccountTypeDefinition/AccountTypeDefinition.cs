using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.AccountTypeDefinition
{
    public class AccountTypeDefinitionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string AccountTypeGroupId { get; set; }
        public AccountTypeGroupDto AccountTypeGroup { get; set; }
        public string ParentId { get; set; }
        public AccountTypeDefinitionDto Parent { get; set; }
        public List<AccountTypeDefinitionDto> Children { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemStandard { get; set; }
        public bool IsUserStandard { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AccountTypeGroupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<AccountTypeDefinitionDto> AccountTypes { get; set; }
    }

    public class AccountTypeDefinitionCommand
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string AccountTypeGroupId { get; set; }
        public string ParentId { get; set; }
        public bool IsUserStandard { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}