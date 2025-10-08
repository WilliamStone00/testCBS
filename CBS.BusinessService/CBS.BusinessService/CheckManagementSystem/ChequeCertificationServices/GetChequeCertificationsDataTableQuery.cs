using System;
using CBS.FrontDesk.Data.Entity.DataTable;

namespace CBS.BusinessService.CheckManagementSystem.ChequeCertification
{
    public class GetChequeCertificationsDataTableQuery
    {
        public DataTableOptions Options { get; set; }

        public string MemberReference { get; set; }
        public string ChequeCertificationID { get; set; }
        public string AccountNumber { get; set; }
        public string CertificationStatus { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}