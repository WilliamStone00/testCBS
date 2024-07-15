using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class BranchToBranchTransfer
    {
        [Required]
        public string AccountId { get; set; }
   
        public string Accountinfor { get; set; }
        public string Balance { get; set; }
        [Required]
        public string LiaisonId { get; set; }
        [Required]
        public string Amount { get; set; }
        public string Description { get; set; }
        public string ReferenceId { get; set; }
    }
}