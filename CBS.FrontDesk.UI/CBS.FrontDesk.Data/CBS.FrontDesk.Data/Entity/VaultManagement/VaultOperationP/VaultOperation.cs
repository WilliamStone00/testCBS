using System;

namespace CBS.FrontDesk.Data.Entity.VaultManagement
{

    public class VaultOperation
    {
        public string Id { get; set; }
        public string VaultId { get; set; }
        public string OperationType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string DoneBy { get; set; }
        public string BranchId { get; set; }
        public string Reference { get; set; }
        public DateTime OperationDate { get; set; }
        public Vault Vault { get; set; }
    }

}
