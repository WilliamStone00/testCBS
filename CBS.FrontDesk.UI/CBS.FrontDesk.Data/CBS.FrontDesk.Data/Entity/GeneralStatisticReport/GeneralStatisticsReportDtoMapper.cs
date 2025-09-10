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
        public static List<GeneralStatisticsFlatRow> FlattenToRowsWithGroupAndGrandTotals(GeneralStatisticsReportRPT rpt)
        {
            var allData = new List<GeneralStatisticsFlatRow>();

            // 1️⃣ Add MembersWithAccounts
            if (rpt.MembersWithAccounts != null)
            {
                allData.Add(new GeneralStatisticsFlatRow
                {
                    Title = "Members",
                    DataGroup = "1. MEMBERS OVERVIEW",
                    MenCount = rpt.MembersWithAccounts.Men,
                    WomenCount = rpt.MembersWithAccounts.Women,
                    GroupsCount = rpt.MembersWithAccounts.Groups,
                    MenAmount = 0,
                    WomenAmount = 0,
                    GroupsAmount = 0
                });
            }

            // 2️⃣ Local helper to add grouped sections
            void AddGroup<T>(List<T> source, string groupName) where T : class
            {
                if (source == null || !source.Any()) return;

                var groupRows = new List<GeneralStatisticsFlatRow>();
                int totalGroupCount = 0;
                decimal totalGroupAmount = 0;

                foreach (var item in source)
                {
                    switch (item)
                    {
                        case AccountTypeGenderSummary acc:
                            totalGroupCount += acc.MenCount + acc.WomenCount + acc.GroupsCount;
                            totalGroupAmount += acc.MenAmount + acc.WomenAmount + acc.GroupsAmount;
                            break;
                        case LoanCategoryGenderSummary loan:
                            totalGroupCount += loan.MenCount + loan.WomenCount + loan.GroupsCount;
                            totalGroupAmount += loan.MenAmount + loan.WomenAmount + loan.GroupsAmount;
                            break;
                        case LoanBreakdownItem purpose:
                            totalGroupCount += purpose.Men + purpose.Women + purpose.Groups;
                            totalGroupAmount += purpose.MenAmount + purpose.WomenAmount + purpose.GroupsAmount;
                            break;
                    }
                }

                foreach (var item in source)
                {
                    if (item is AccountTypeGenderSummary acc)
                    {
                        groupRows.Add(new GeneralStatisticsFlatRow
                        {
                            Title = acc.AccountType,
                            MenCount = acc.MenCount,
                            WomenCount = acc.WomenCount,
                            GroupsCount = acc.GroupsCount,
                            MenAmount = acc.MenAmount,
                            WomenAmount = acc.WomenAmount,
                            GroupsAmount = acc.GroupsAmount,
                            DataGroup = groupName,
                            TotalCountPercentage = totalGroupCount > 0
                                ? Math.Round((double)(acc.MenCount + acc.WomenCount + acc.GroupsCount) / totalGroupCount * 100, 2)
                                : 0,
                            TotalAmountPercentage = totalGroupAmount > 0
                                ? Math.Round((double)(acc.MenAmount + acc.WomenAmount + acc.GroupsAmount) / (double)totalGroupAmount * 100, 2)
                                : 0
                        });
                    }
                    else if (item is LoanCategoryGenderSummary loan)
                    {
                        groupRows.Add(new GeneralStatisticsFlatRow
                        {
                            Title = loan.Category,
                            MenCount = loan.MenCount,
                            WomenCount = loan.WomenCount,
                            GroupsCount = loan.GroupsCount,
                            MenAmount = loan.MenAmount,
                            WomenAmount = loan.WomenAmount,
                            GroupsAmount = loan.GroupsAmount,
                            DataGroup = groupName,
                            TotalCountPercentage = totalGroupCount > 0
                                ? Math.Round((double)(loan.MenCount + loan.WomenCount + loan.GroupsCount) / totalGroupCount * 100, 2)
                                : 0,
                            TotalAmountPercentage = totalGroupAmount > 0
                                ? Math.Round((double)(loan.MenAmount + loan.WomenAmount + loan.GroupsAmount) / (double)totalGroupAmount * 100, 2)
                                : 0
                        });
                    }
                    else if (item is LoanBreakdownItem purpose)
                    {
                        groupRows.Add(new GeneralStatisticsFlatRow
                        {
                            Title = purpose.Category,
                            MenCount = purpose.Men,
                            WomenCount = purpose.Women,
                            GroupsCount = purpose.Groups,
                            MenAmount = purpose.MenAmount,
                            WomenAmount = purpose.WomenAmount,
                            GroupsAmount = purpose.GroupsAmount,
                            DataGroup = groupName,
                            TotalCountPercentage = totalGroupCount > 0
                                ? Math.Round((double)(purpose.Men + purpose.Women + purpose.Groups) / totalGroupCount * 100, 2)
                                : 0,
                            TotalAmountPercentage = totalGroupAmount > 0
                                ? Math.Round((double)(purpose.MenAmount + purpose.WomenAmount + purpose.GroupsAmount) / (double)totalGroupAmount * 100, 2)
                                : 0
                        });
                    }
                }

                allData.AddRange(groupRows);
            }

            // 3️⃣ Add all grouped sections
            AddGroup(rpt.AccountTypeSummaries, "2. MEMBER'S ACCOUNT");
            AddGroup(rpt.LoanSummaries, "3. OUT STANDING SUMMARY");
            AddGroup(rpt.LoanCategorySummaries, "4. LOAN BY CATEGORY");
            AddGroup(rpt.LoanBreakdowns, "5. LOAN BY PURPOSE");
            AddGroup(rpt.LoanTypeSummaries, "6. LOAN BY TYPES");
            AddGroup(rpt.LoanTargetSummaries, "7. LOAN BY TARGET");
            AddGroup(rpt.LoanDeliquentStatusByGenderSummaries, "8. LOAN BY DELINQUENCY");
            AddGroup(rpt.ParSummaries, "9. LOAN BY PAR (BUCKET)");

            // 4️⃣ Liquidity Ratio Computation
            var totalLoan = rpt.LoanSummaries?.Sum(x => x.TotalAmount) ?? 0;
            var totalSavings = rpt.AccountTypeSummaries
                ?.Where(x => x.AccountType?.ToLowerInvariant().Contains("saving") == true)
                .Sum(x => x.TotalAmount) ?? 0;

            var ratio = totalSavings > 0 ? Math.Round((double)(totalLoan / totalSavings) * 100, 2) : 0;
            var diff = totalSavings - totalLoan;

            var liquidityComment = ratio <= 75
                ? "✅ Compliant: Savings exceed loans, providing a strong liquidity buffer."
                : "❗ Warning: Loan-to-savings ratio exceeds 75%. Review liquidity exposure.";

            allData.Add(new GeneralStatisticsFlatRow
            {
                DataGroup = "10. LIQUIDITY OVERVIEW",
                MenAmount = totalLoan,
                WomenAmount = totalSavings,
                GroupsAmount = diff,
                TotalAmountPercentage = ratio,
                TotalCountPercentage = 0,
                Title = $"Liquidity Ratio: {ratio}% - {liquidityComment}"
            });

            return allData;
        }

        //public static List<GeneralStatisticsFlatRow> FlattenToRowsWithGroupAndGrandTotals(GeneralStatisticsReportRPT rpt)
        //{
        //    var allData = new List<GeneralStatisticsFlatRow>();

        //    // 1️⃣ Add MembersWithAccounts
        //    if (rpt.MembersWithAccounts != null)
        //    {
        //        allData.Add(new GeneralStatisticsFlatRow
        //        {
        //            Title = "Members",
        //            DataGroup = "1. MEMBERS OVERVIEW",
        //            MenCount = rpt.MembersWithAccounts.Men,
        //            WomenCount = rpt.MembersWithAccounts.Women,
        //            GroupsCount = rpt.MembersWithAccounts.Groups,
        //            MenAmount = 0,
        //            WomenAmount = 0,
        //            GroupsAmount = 0
        //        });
        //    }

        //    // 2️⃣ Local helper to add grouped sections
        //    void AddGroup<T>(List<T> source, string groupName) where T : class
        //    {
        //        if (source == null || !source.Any()) return;

        //        var groupRows = new List<GeneralStatisticsFlatRow>();
        //        int totalGroupCount = 0;
        //        decimal totalGroupAmount = 0;

        //        foreach (var item in source)
        //        {
        //            switch (item)
        //            {
        //                case AccountTypeGenderSummary acc:
        //                    totalGroupCount += acc.MenCount + acc.WomenCount + acc.GroupsCount;
        //                    totalGroupAmount += acc.MenAmount + acc.WomenAmount + acc.GroupsAmount;
        //                    break;
        //                case LoanCategoryGenderSummary loan:
        //                    totalGroupCount += loan.MenCount + loan.WomenCount + loan.GroupsCount;
        //                    totalGroupAmount += loan.MenAmount + loan.WomenAmount + loan.GroupsAmount;
        //                    break;
        //                case LoanBreakdownItem purpose:
        //                    totalGroupCount += purpose.Men + purpose.Women + purpose.Groups;
        //                    totalGroupAmount += purpose.MenAmount + purpose.WomenAmount + purpose.GroupsAmount;
        //                    break;
        //            }
        //        }

        //        foreach (var item in source)
        //        {
        //            if (item is AccountTypeGenderSummary acc)
        //            {
        //                groupRows.Add(new GeneralStatisticsFlatRow
        //                {
        //                    Title = acc.AccountType,
        //                    MenCount = acc.MenCount,
        //                    WomenCount = acc.WomenCount,
        //                    GroupsCount = acc.GroupsCount,
        //                    MenAmount = acc.MenAmount,
        //                    WomenAmount = acc.WomenAmount,
        //                    GroupsAmount = acc.GroupsAmount,
        //                    DataGroup = groupName,
        //                    TotalCountPercentage = totalGroupCount > 0
        //                        ? Math.Round((double)(acc.MenCount + acc.WomenCount + acc.GroupsCount) / totalGroupCount * 100, 2)
        //                        : 0,
        //                    TotalAmountPercentage = totalGroupAmount > 0
        //                        ? Math.Round((double)(acc.MenAmount + acc.WomenAmount + acc.GroupsAmount) / (double)totalGroupAmount * 100, 2)
        //                        : 0
        //                });
        //            }
        //            else if (item is LoanCategoryGenderSummary loan)
        //            {
        //                groupRows.Add(new GeneralStatisticsFlatRow
        //                {
        //                    Title = loan.Category,
        //                    MenCount = loan.MenCount,
        //                    WomenCount = loan.WomenCount,
        //                    GroupsCount = loan.GroupsCount,
        //                    MenAmount = loan.MenAmount,
        //                    WomenAmount = loan.WomenAmount,
        //                    GroupsAmount = loan.GroupsAmount,
        //                    DataGroup = groupName,
        //                    TotalCountPercentage = totalGroupCount > 0
        //                        ? Math.Round((double)(loan.MenCount + loan.WomenCount + loan.GroupsCount) / totalGroupCount * 100, 2)
        //                        : 0,
        //                    TotalAmountPercentage = totalGroupAmount > 0
        //                        ? Math.Round((double)(loan.MenAmount + loan.WomenAmount + loan.GroupsAmount) / (double)totalGroupAmount * 100, 2)
        //                        : 0
        //                });
        //            }
        //            else if (item is LoanBreakdownItem purpose)
        //            {
        //                groupRows.Add(new GeneralStatisticsFlatRow
        //                {
        //                    Title = purpose.Category,
        //                    MenCount = purpose.Men,
        //                    WomenCount = purpose.Women,
        //                    GroupsCount = purpose.Groups,
        //                    MenAmount = purpose.MenAmount,
        //                    WomenAmount = purpose.WomenAmount,
        //                    GroupsAmount = purpose.GroupsAmount,
        //                    DataGroup = groupName,
        //                    TotalCountPercentage = totalGroupCount > 0
        //                        ? Math.Round((double)(purpose.Men + purpose.Women + purpose.Groups) / totalGroupCount * 100, 2)
        //                        : 0,
        //                    TotalAmountPercentage = totalGroupAmount > 0
        //                        ? Math.Round((double)(purpose.MenAmount + purpose.WomenAmount + purpose.GroupsAmount) / (double)totalGroupAmount * 100, 2)
        //                        : 0
        //                });
        //            }
        //        }

        //        allData.AddRange(groupRows);
        //    }

        //    // 3️⃣ Add all grouped sections
        //    AddGroup(rpt.AccountTypeSummaries, "2. MEMBER'S ACCOUNT");
        //    AddGroup(rpt.LoanSummaries, "3. OUT STANDING SUMMARY");
        //    AddGroup(rpt.LoanCategorySummaries, "4. LOAN BY CATEGORY");
        //    AddGroup(rpt.LoanBreakdowns, "5. LOAN BY PURPOSE");
        //    AddGroup(rpt.LoanTypeSummaries, "6. LOAN BY TYPES");
        //    AddGroup(rpt.LoanTargetSummaries, "7. LOAN BY TARGET");

        //    // 4️⃣ Liquidity Ratio Computation (💰 Loans vs Savings)
        //    var totalLoan = rpt.LoanSummaries?.Sum(x => x.TotalAmount) ?? 0;
        //    var totalSavings = rpt.AccountTypeSummaries
        //        ?.Where(x => x.AccountType?.ToLowerInvariant().Contains("saving") == true)
        //        .Sum(x => x.TotalAmount) ?? 0;

        //    var ratio = totalSavings > 0 ? Math.Round((double)(totalLoan / totalSavings) * 100, 2) : 0;
        //    var diff = totalSavings - totalLoan;

        //    var liquidityComment = ratio <= 75
        //        ? "✅ Compliant: Savings exceed loans, providing a strong liquidity buffer."
        //        : "❗ Warning: Loan-to-savings ratio exceeds 75%. Review liquidity exposure.";

        //    allData.Add(new GeneralStatisticsFlatRow
        //    {

        //        DataGroup = "8. LIQUIDITY OVERVIEW",
        //        MenAmount = totalLoan,
        //        WomenAmount = totalSavings,
        //        GroupsAmount = diff,
        //        TotalAmountPercentage = ratio,
        //        // Use Title or TotalCountPercentage optionally to encode compliance comment
        //        TotalCountPercentage = 0, // Not applicable, reused field optional
        //                                  // You can add a new `Comment` field to the FlatRow model for cleaner design
        //                                  // e.g., Comment = liquidityComment
        //        Title = $"Liquidity Ratio: {ratio}% - {liquidityComment}"
        //    });
        //    // Example: assuming you already have monthly data grouped

        //    return allData;
        //}


        public static List<LiquidityRatioChartRow> GenerateLiquidityRatioChartData(GeneralStatisticsReportRPT rpt)
        {
            if (rpt == null || rpt.AccountTypeSummaries == null || rpt.LoanSummaries == null)
                return new List<LiquidityRatioChartRow>();

            decimal totalSavings = rpt.AccountTypeSummaries
                .Where(x => x.AccountType?.ToLower().Contains("saving") == true)
                .Sum(x => x.TotalAmount);

            decimal totalLoan = rpt.LoanSummaries
                .Where(x => x.Category?.ToLower().Contains("outstanding") == true)
                .Sum(x => x.TotalAmount);

            double ratio = totalSavings > 0 ? Math.Round((double)(totalLoan / totalSavings) * 100, 2) : 0;
            decimal difference = totalSavings - totalLoan;

            string complianceStatus = ratio <= 75
                ? "Compliant: Savings exceed loans, providing a strong liquidity buffer."
                : "Non-Compliant: Loan-to-savings ratio exceeds 75%. Liquidity risk is high.";

            string insightAdvice = GetLiquidityInsightByRatio(ratio);

            return new List<LiquidityRatioChartRow>
            {
                new LiquidityRatioChartRow
                {
                    TotalLoanBalance = totalLoan,
                    TotalSavings = totalSavings,
                    LiquidityRatio = ratio,
                    Difference = difference,
                    ComplianceComment = complianceStatus,
                    LiquidityInsightAdvice = insightAdvice
                }
            };
        }

        private static string GetLiquidityInsightByRatio(double ratio)
        {
            if (ratio <= 30)
                return "✅ Excellent liquidity health. Your loan portfolio is very well supported by savings. Consider reviewing investment opportunities or controlled loan expansion.";

            else if (ratio <= 50)
                return "✅ Very strong liquidity. You have ample savings buffer. Maintain current loan growth pace but continue promoting saving schemes.";

            else if (ratio <= 65)
                return "✅ Good liquidity. There is still enough savings to cover loans. Monitor growth carefully and reinforce loan recovery discipline.";

            else if (ratio <= 75)
                return "🟡 Acceptable range. Loan levels are approaching the recommended threshold. Avoid overexposure by increasing savings mobilization efforts.";

            else if (ratio <= 85)
                return "⚠️ Attention required. Savings buffer is narrowing. Strengthen savings strategies and avoid high-risk loans. Enforce repayment rigorously.";

            else if (ratio <= 95)
                return "❗ Liquidity imbalance emerging. Your loans are almost equal to savings. Hold off on aggressive loan disbursement and intensify recovery drives.";

            else if (ratio <= 105)
                return "🚨 Critical threshold. Your loan levels are now exceeding savings. Liquidity is under pressure. Suspend large loans and engage in urgent savings mobilization.";

            else if (ratio <= 120)
                return "🚨 Severe liquidity risk. The institution is likely over-leveraged. Prioritize emergency liquidity action—freeze disbursements and accelerate recoveries.";

            else
                return "🔥 Extreme danger. Loans far exceed savings. This exposes the institution to systemic risk. Immediate strategic intervention is required from leadership.";
        }


    }


}