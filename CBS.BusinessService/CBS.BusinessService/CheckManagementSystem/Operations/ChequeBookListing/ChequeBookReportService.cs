using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.Report;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing
{
    public class ChequeBookReportService : BaseService
    {
        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly BranchServices _branchServices;

        public ChequeBookReportService(BranchServices branchServices)
        {
            string baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            _apiCallerHelper = new ApiCallerHelper(baseUrl);
            _branchServices = branchServices;
        }

        // ============================================================
        // API CALL
        // ============================================================

        public async Task<List<ChequeBook>> GetChequeBooksAsync(ChequeBookQuery query)
        {
            var response = await _apiCallerHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetChequeBooksDataTable, query);

            if (!response.IsSuccess || response.ApiResponseData?.Data == null)
                return new List<ChequeBook>();

            return JsonConvert.DeserializeObject<List<ChequeBook>>(
                JsonConvert.SerializeObject(response.ApiResponseData.Data.data));
        }

        // ============================================================
        // PUBLIC ENTRY (USED BY CONTROLLER / CRYSTAL)
        // ============================================================

        public async Task<List<ChequeBookReportRow>> BuildChequeBookReportAsync(
            ChequeBookQuery query)
        {
            var chequeBooks = await GetChequeBooksAsync(query);
            return BuildFlatRows(chequeBooks, query);
        }

        // ============================================================
        // FLATTEN DATA (ONE ROW = ONE CHEQUEBOOK)
        // ============================================================

        private List<ChequeBookReportRow> BuildFlatRows(
            List<ChequeBook> chequeBooks,
            ChequeBookQuery query)
        {
            var rows = new List<ChequeBookReportRow>();

            var summary = CalculateSummary(chequeBooks, query);
            var now = DateTime.Now;
            var user = GetUserFullName();

            int rowNo = chequeBooks.Count;

            var bankname = GetBankName();
            var bankcode = GetBankCode();
            var branchname = GetBranchName();
            var branchcode = GetBranchCode();
            var printedby = GetUserFullName();
           
            foreach (var cb in chequeBooks)
            {
                rows.Add(new ChequeBookReportRow
                {
                    // ================= HEADER =================
                    BankName = bankname,
                    BankCode = bankcode,
                    BranchName = branchname,
                    BranchCode = branchcode,
                    PrintedBy = printedby,
                    PrintedOn = now,
                    ReportTitle = "CHECKBOOK REGISTRY REPORT",

                    // ================= SUMMARY =================
                    TotalBranches = summary.TotalBranches,
                    TotalChequeBooksIssued = summary.TotalChequeBooksIssued,
                    TotalAccountsLinked = summary.TotalAccountsLinked,
                    TotalLeaves = summary.TotalLeaves,
                    TotalLeavesUsed = summary.TotalLeavesUsed,
                    TotalBalance = summary.TotalBalance,
                    ApprovedChequeBooks = summary.ApprovedChequeBooks,
                    UnclearedChequeBooks = summary.UnclearedChequeBooks,
                    PendingChequeBooks = summary.PendingChequeBooks,
                    ReportPeriodFrom = summary.ReportPeriodFrom,
                    ReportPeriodTo = summary.ReportPeriodTo,
             

                    CreatedDate = cb.CreatedDate.ToString("dd/MM/yyyy"),
                    Time = cb.CreatedDate.ToString("HH:mm:ss"),
                    // ================= DETAIL =================
                    RowNumber = rowNo,
                    CustomerName = cb.CustomerName ?? string.Empty,
                    AccountNumber = cb.AccountNumber ?? string.Empty,
                    CategoryName = cb.CategoryName ?? string.Empty,
                    NumberOfLeaves = cb.NumberOfLeaves,
                    StatusDescription = GetStatusDescription(cb.Status),
                    Year = cb.CreatedDate.Year.ToString()
                });
            }

            // Ensure Crystal prints headers even when no data
            if (!rows.Any())
            {
                rows.Add(new ChequeBookReportRow
                {
                    BankName = bankname,
                    BankCode = bankcode,
                    BranchName = branchname,
                    BranchCode = branchcode,
                    PrintedBy = printedby,
                    PrintedOn = now,
                    ReportTitle = "CHECKBOOK REGISTRY REPORT",
                    ReportPeriodFrom = query.StartDate ?? DateTime.MinValue,
                    ReportPeriodTo = query.EndDate ?? DateTime.MinValue
                });
            }

            return rows;
        }

        // ============================================================
        // SUMMARY CALCULATION
        // ============================================================

        private ChequeBookReportRow CalculateSummary(
            List<ChequeBook> chequeBooks,
            ChequeBookQuery query)
        {
            return new ChequeBookReportRow
            {
                TotalBranches = chequeBooks.Select(x => x.BranchId).Distinct().Count(),
                TotalChequeBooksIssued = chequeBooks.Count,
                TotalAccountsLinked = chequeBooks.Select(x => x.AccountNumber).Distinct().Count(),
                TotalLeaves = chequeBooks.Sum(x => x.NumberOfLeaves),
                TotalLeavesUsed = chequeBooks.Sum(x => x.UsedLeaves),
                TotalBalance = chequeBooks.Sum(x => x.Balance),
                ApprovedChequeBooks = chequeBooks.Count(x =>
                    x.Status == "APPROVED" || x.Status == "APPROVED_AND_PAID"),
                UnclearedChequeBooks = chequeBooks.Count(x => x.Status == "UNCLEARED"),
                PendingChequeBooks = chequeBooks.Count(x =>
                    x.Status == "PENDING" || x.Status == "UNDER_REVIEW"),
                ReportPeriodFrom = query.StartDate ?? DateTime.MinValue,
                ReportPeriodTo = query.EndDate ?? DateTime.MinValue
            };
        }

        private string GetStatusDescription(string status)
        {
            if (string.IsNullOrEmpty(status))
                return "Unknown";

            switch (status)
            {
                case "APPROVED":
                    return "Approved";
                case "APPROVED_AND_PAID":
                    return "Appr.and paid";
                case "UNCLEARED":
                    return "Awaiting";
                case "PENDING":
                    return "Pending";
                case "BLOCKED":
                    return "Blocked";
                default:
                    return status;
            }
        }
    }
}
