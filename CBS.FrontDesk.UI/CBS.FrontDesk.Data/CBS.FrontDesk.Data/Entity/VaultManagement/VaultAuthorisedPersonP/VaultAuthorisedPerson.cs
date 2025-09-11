namespace CBS.FrontDesk.Data.Entity.VaultManagement
{
    public class VaultAuthorisedPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string VaultId { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public bool IsLeader { get; set; }
        public string Date { get; set; }
        public bool IsActive { get; set; }
        public virtual Vault Vault { get; set; }
    }
}
