using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class ManualEntryDailyCollectorDto
    {
        public string Id { get; set; }
        public string BranchCode { get; set; }

        public string CollectorId { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMember { get; set; }
        public int TotalBranchesCollected { get; set; }
        public string Status { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDateTime { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime ReviewedDateTime { get; set; }
        public string ReviewerStatement { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovalDateTime { get; set; }
        public string ApprovalStatement { get; set; }
        public string CashDeskStatus { get; set; }
        public string CashDeskDate { get; set; }
        public string CashDeskName { get; set; }
        public string CashDeskBranchId { get; set; }
        public string CashDeskBranchName { get; set; }
        public string CashDeskBranchCode { get; set; }
        public decimal TotalCashInAmount { get; set; }
        public bool CashInStatus { get; set; }
        public string ManualEntryDailyCollectorId { get; set; }

        public virtual ICollection<ManualEntryDailyCollectorDetailsDto> ManualEntryDailyCollectorDetailsDto { get; set; }
    }
}
