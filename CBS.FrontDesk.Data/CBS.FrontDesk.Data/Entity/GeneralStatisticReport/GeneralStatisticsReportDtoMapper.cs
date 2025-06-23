using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.NLoan.Data.Dto.DataSetLoanPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.GeneralStatisticReport
{
    public static class GeneralStatisticsReportDtoMapper
    {
        public static GeneralStatisticsReportRPT AddBranchGetReportRPTMapper(GeneralStatisticsReportRPT rpt, Branch branch)
        {
            var headOffice = branch?.Bank ?? new Bank(); // fallback if branch.Bank is null

            // Branch Info
            rpt.Logo = branch?.LogoUrl ?? headOffice.LogoUrl;
            rpt.BranchName = branch?.Name;
            rpt.BranchCode = branch?.BranchCode;
            rpt.BranchAddress = branch?.Address;
            rpt.BranchTelephone = branch?.Telephone;

            // Head Office Info
            rpt.HeadOfficeName = headOffice.Name;
            rpt.HeadOfficeAddress = headOffice.Address;
            rpt.HeadOfficeTelephone = headOffice.Telephone;
            rpt.HeadOfficeEmail = headOffice.Email;
            rpt.HeadOfficeWebSite = headOffice.WebSite;
            rpt.HeadOfficeInitial = headOffice.BankInitial;
            rpt.HeadOfficeCode = headOffice.BankCode;

            // Inject branch/head office info into each member detail
            if (rpt.MembersDetails != null && rpt.MembersDetails.Any())
            {
                foreach (var customerBasic in rpt.MembersDetails)
                {
                    customerBasic.BranchName = branch?.Name;
                    customerBasic.BranchCode = branch?.BranchCode;
                    customerBasic.BranchTelephone = branch?.Telephone;
                    customerBasic.BranchAddress = branch?.Address;
                    customerBasic.Logo = headOffice.LogoUrl;

                    customerBasic.HeadOfficeName = headOffice.Name;
                    customerBasic.HeadOfficeAddress = headOffice.Address;
                    customerBasic.HeadOfficeTelephone = headOffice.Telephone;
                    customerBasic.HeadOfficeEmail = headOffice.Email;
                    customerBasic.HeadOfficeWebSite = headOffice.WebSite;
                    customerBasic.HeadOfficeInitial = headOffice.BankInitial;
                    customerBasic.HeadOfficeCode = headOffice.BankCode;
                }
            }

            return rpt;
        }

        public static List<GeneralStatisticsFlatRow> FlattenToRows(GeneralStatisticsReportRPT rpt)
        {
            var rows = new List<GeneralStatisticsFlatRow>();

            if (rpt == null) return rows;

            // Account types: SAVINGS, DEPOSITS, SHARES
            rows.AddRange(rpt.AccountTypeSummaries.Select(x => new GeneralStatisticsFlatRow
            {
                Title = x.AccountType,
                MenCount = x.MenCount,
                WomenCount = x.WomenCount,
                GroupsCount = x.GroupsCount,
                MenAmount = x.MenAmount,
                WomenAmount = x.WomenAmount,
                GroupsAmount = x.GroupsAmount,
                DataGroup = "Account"
            }));

            // Loan summaries: OUTSTANDING, GRANTED THIS MONTH
            rows.AddRange(rpt.LoanSummaries.Select(x => new GeneralStatisticsFlatRow
            {
                Title = x.Category,
                MenCount = x.MenCount,
                WomenCount = x.WomenCount,
                GroupsCount = x.GroupsCount,
                MenAmount = x.MenAmount,
                WomenAmount = x.WomenAmount,
                GroupsAmount = x.GroupsAmount,
                DataGroup = "LoanSummary"
            }));

            // Loan types: MEDICAL, CONTRACT, EDUCATION etc.
            rows.AddRange(rpt.LoanTypeSummaries.Select(x => new GeneralStatisticsFlatRow
            {
                Title = x.Category,
                MenCount = x.MenCount,
                WomenCount = x.WomenCount,
                GroupsCount = x.GroupsCount,
                MenAmount = x.MenAmount,
                WomenAmount = x.WomenAmount,
                GroupsAmount = x.GroupsAmount,
                DataGroup = "LoanType"
            }));

            // Optionally include categories or targets
            rows.AddRange(rpt.LoanCategorySummaries.Select(x => new GeneralStatisticsFlatRow
            {
                Title = x.Category,
                MenCount = x.MenCount,
                WomenCount = x.WomenCount,
                GroupsCount = x.GroupsCount,
                MenAmount = x.MenAmount,
                WomenAmount = x.WomenAmount,
                GroupsAmount = x.GroupsAmount,
                DataGroup = "LoanCategory"
            }));

            rows.AddRange(rpt.LoanTargetSummaries.Select(x => new GeneralStatisticsFlatRow
            {
                Title = x.Category,
                MenCount = x.MenCount,
                WomenCount = x.WomenCount,
                GroupsCount = x.GroupsCount,
                MenAmount = x.MenAmount,
                WomenAmount = x.WomenAmount,
                GroupsAmount = x.GroupsAmount,
                DataGroup = "LoanTarget"
            }));

            return rows;
        }
    }
}
