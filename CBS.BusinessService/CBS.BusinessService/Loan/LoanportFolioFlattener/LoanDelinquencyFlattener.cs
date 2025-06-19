using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanPortFolioDataSet;
using CBS.NLoan.Data.Dto.DataSetLoanPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.LoanportFolioFlattener
{
    public static class LoanDelinquencyFlattener
    {
        public static LoanDelinquencyReportResultRPT FlattenAll(LoanDelinquencyReportResult report, Branch branch)
        {
            var headOffice = branch?.Bank ?? new Bank(); // assuming Bank is a class

            var rpt = new LoanDelinquencyReportResultRPT();
            rpt.PortfolioDetails=report.PortfolioDetails;
            // Branch Info
            rpt.Logo = branch.LogoUrl ?? headOffice.LogoUrl;
            rpt.BranchName = branch.Name;
            rpt.BranchCode = branch.BranchCode;
            rpt.BranchAddress = branch.Address;
            rpt.BranchTelephone = branch.Telephone;
            // Head Office Info
            rpt.HeadOfficeName = headOffice.Name;
            rpt.HeadOfficeAddress = headOffice.Address;
            rpt.HeadOfficeTelephone = headOffice.Telephone;
            rpt.HeadOfficeEmail = headOffice.Email;
            rpt.HeadOfficeWebSite = headOffice.WebSite;
            rpt.HeadOfficeInitial = headOffice.BankInitial;
            rpt.HeadOfficeCode = headOffice.BankCode;
            rpt.Flat_AgeAndGender = report.ByAgeAndGender.Select(x => new FlatAgeGenderRecord
            {
                Ageing = x.Ageing,

                MaleCount = x.Male.Num,
                MaleAmount = x.Male.Amount,
                MalePercentage = x.Male.Percentage,
                MaleInterestCount = x.Male.InterestNum,
                MaleInterest = x.Male.Interest,
                MaleIPercentage = x.Male.IPercentage,

                FemaleCount = x.Female.Num,
                FemaleAmount = x.Female.Amount,
                FemalePercentage = x.Female.Percentage,
                FemaleInterestCount = x.Female.InterestNum,
                FemaleInterest = x.Female.Interest,
                FemaleIPercentage = x.Female.IPercentage,

                GroupCount = x.Group.Num,
                GroupAmount = x.Group.Amount,
                GroupPercentage = x.Group.Percentage,
                GroupInterestCount = x.Group.InterestNum,
                GroupInterest = x.Group.Interest,
                GroupIPercentage = x.Group.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();

            rpt.Flat_AgeAndLoanType = report.ByAgeAndLoanType.Select(x => new FlatAgeLoanTypeRecord
            {
                Ageing = x.Ageing,

                MainLoanCount = x.MainLoans.Num,
                MainLoanAmount = x.MainLoans.Amount,
                MainLoanPercentage = x.MainLoans.Percentage,
                MainLoanInterestCount = x.MainLoans.InterestNum,
                MainLoanInterest = x.MainLoans.Interest,
                MainLoanIPercentage = x.MainLoans.IPercentage,

                ExceptionalLoanCount = x.ExceptionalLoans.Num,
                ExceptionalLoanAmount = x.ExceptionalLoans.Amount,
                ExceptionalLoanPercentage = x.ExceptionalLoans.Percentage,
                ExceptionalLoanInterestCount = x.ExceptionalLoans.InterestNum,
                ExceptionalLoanInterest = x.ExceptionalLoans.Interest,
                ExceptionalLoanIPercentage = x.ExceptionalLoans.IPercentage,

                SSFCount = x.SSF.Num,
                SSFAmount = x.SSF.Amount,
                SSFPercentage = x.SSF.Percentage,
                SSFInterestCount = x.SSF.InterestNum,
                SSFInterest = x.SSF.Interest,
                SSFIPercentage = x.SSF.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();

            rpt.Flat_ByZone = report.ByZone.Select(x => new FlatZoneRecord
            {
                Zone = x.Zone,

                MaleCount = x.Male.Num,
                MaleAmount = x.Male.Amount,
                MalePercentage = x.Male.Percentage,
                MaleInterestCount = x.Male.InterestNum,
                MaleInterest = x.Male.Interest,
                MaleIPercentage = x.Male.IPercentage,

                FemaleCount = x.Female.Num,
                FemaleAmount = x.Female.Amount,
                FemalePercentage = x.Female.Percentage,
                FemaleInterestCount = x.Female.InterestNum,
                FemaleInterest = x.Female.Interest,
                FemaleIPercentage = x.Female.IPercentage,

                GroupCount = x.Group.Num,
                GroupAmount = x.Group.Amount,
                GroupPercentage = x.Group.Percentage,
                GroupInterestCount = x.Group.InterestNum,
                GroupInterest = x.Group.Interest,
                GroupIPercentage = x.Group.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();

            rpt.Flat_ByLoanTerm = report.ByLoanTerm.Select(x => new FlatLoanTermRecord
            {
                Term = x.Term,

                MaleCount = x.Male.Num,
                MaleAmount = x.Male.Amount,
                MalePercentage = x.Male.Percentage,
                MaleInterestCount = x.Male.InterestNum,
                MaleInterest = x.Male.Interest,
                MaleIPercentage = x.Male.IPercentage,

                FemaleCount = x.Female.Num,
                FemaleAmount = x.Female.Amount,
                FemalePercentage = x.Female.Percentage,
                FemaleInterestCount = x.Female.InterestNum,
                FemaleInterest = x.Female.Interest,
                FemaleIPercentage = x.Female.IPercentage,

                GroupCount = x.Group.Num,
                GroupAmount = x.Group.Amount,
                GroupPercentage = x.Group.Percentage,
                GroupInterestCount = x.Group.InterestNum,
                GroupInterest = x.Group.Interest,
                GroupIPercentage = x.Group.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();

            rpt.Flat_ByTargetGroup = report.ByTargetGroup.Select(x => new FlatTargetGroupRecord
            {
                TargetGroup = x.TargetGroup,

                MaleCount = x.Male.Num,
                MaleAmount = x.Male.Amount,
                MalePercentage = x.Male.Percentage,
                MaleInterestCount = x.Male.InterestNum,
                MaleInterest = x.Male.Interest,
                MaleIPercentage = x.Male.IPercentage,

                FemaleCount = x.Female.Num,
                FemaleAmount = x.Female.Amount,
                FemalePercentage = x.Female.Percentage,
                FemaleInterestCount = x.Female.InterestNum,
                FemaleInterest = x.Female.Interest,
                FemaleIPercentage = x.Female.IPercentage,

                GroupCount = x.Group.Num,
                GroupAmount = x.Group.Amount,
                GroupPercentage = x.Group.Percentage,
                GroupInterestCount = x.Group.InterestNum,
                GroupInterest = x.Group.Interest,
                GroupIPercentage = x.Group.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();

            rpt.Flat_ByCategory = report.ByCategory.Select(x => new FlatCategoryRecord
            {
                Category = x.Category,

                MaleCount = x.Male.Num,
                MaleAmount = x.Male.Amount,
                MalePercentage = x.Male.Percentage,
                MaleInterestCount = x.Male.InterestNum,
                MaleInterest = x.Male.Interest,
                MaleIPercentage = x.Male.IPercentage,

                FemaleCount = x.Female.Num,
                FemaleAmount = x.Female.Amount,
                FemalePercentage = x.Female.Percentage,
                FemaleInterestCount = x.Female.InterestNum,
                FemaleInterest = x.Female.Interest,
                FemaleIPercentage = x.Female.IPercentage,

                GroupCount = x.Group.Num,
                GroupAmount = x.Group.Amount,
                GroupPercentage = x.Group.Percentage,
                GroupInterestCount = x.Group.InterestNum,
                GroupInterest = x.Group.Interest,
                GroupIPercentage = x.Group.IPercentage,

                TotalCount = x.Total.Num,
                TotalAmount = x.Total.Amount,
                TotalPercentage = x.Total.Percentage,
                TotalInterestCount = x.TotalInterest.InterestNum,
                TotalInterest = x.TotalInterest.Interest,
                TotalIPercentage = x.TotalInterest.IPercentage
            }).ToList();
            // ✅ Inject branch and head office info into each loan record
            if (report.PortfolioDetails != null && report.PortfolioDetails.Any())
            {
                foreach (var loan in report.PortfolioDetails)
                {
                    // Set branch data
                    loan.BranchName = branch?.Name;
                    loan.BranchCode = branch?.BranchCode;
                    loan.BranchTelephone = branch?.Telephone;
                    loan.BranchAddress = branch?.Address;
                    loan.Logo = headOffice.LogoUrl;
                    // Set head office info from inherited HeadOffice
                    loan.HeadOfficeName = headOffice?.Name;
                    loan.HeadOfficeAddress = headOffice?.Address;
                    loan.HeadOfficeTelephone = headOffice?.Telephone;
                    loan.HeadOfficeEmail = headOffice?.Email;
                    loan.HeadOfficeWebSite = headOffice?.WebSite;
                    loan.HeadOfficeInitial = headOffice?.BankInitial;
                    loan.HeadOfficeCode = headOffice?.BankCode;
                }

                rpt.PortfolioDetails = report.PortfolioDetails;
            }
            return rpt;
        }

        /// <summary>
        /// Enriches the original report with branch and head office info.
        /// </summary>
        public static LoanDelinquencyReportDto FlattenAll(LoanDelinquencyReportDto report, Branch branch)
        {
            var headOffice = branch?.Bank ?? new Bank(); // fallback

            report.Logo = branch.LogoUrl ?? headOffice.LogoUrl;
            report.BranchName = branch.Name;
            report.BranchCode = branch.BranchCode;
            report.BranchAddress = branch.Address;
            report.BranchTelephone = branch.Telephone;

            report.HeadOfficeName = headOffice.Name;
            report.HeadOfficeAddress = headOffice.Address;
            report.HeadOfficeTelephone = headOffice.Telephone;
            report.HeadOfficeEmail = headOffice.Email;
            report.HeadOfficeWebSite = headOffice.WebSite;
            report.HeadOfficeInitial = headOffice.BankInitial;
            report.HeadOfficeCode = headOffice.BankCode;

            return report;
        }

        /// <summary>
        /// Converts the structured report into flat rows for Crystal Reports.
        /// </summary>
        public static List<FlatLoanDelinquencyRow> FlattenToFlatRows(LoanDelinquencyReportDto report)
        {
            var flatRows = new List<FlatLoanDelinquencyRow>();

            // Helper for shared metadata
            FlatLoanDelinquencyRow PopulateShared(FlatLoanDelinquencyRow row)
            {
                row.ReportDate = report.ReportDate;

                row.BranchName = report.BranchName;
                row.BranchCode = report.BranchCode;
                row.BranchAddress = report.BranchAddress;
                row.BranchTelephone = report.BranchTelephone;

                row.HeadOfficeName = report.HeadOfficeName;
                row.HeadOfficeAddress = report.HeadOfficeAddress;
                row.HeadOfficeTelephone = report.HeadOfficeTelephone;
                row.HeadOfficeEmail = report.HeadOfficeEmail;
                row.HeadOfficeWebSite = report.HeadOfficeWebSite;
                row.HeadOfficeInitial = report.HeadOfficeInitial;
                row.HeadOfficeCode = report.HeadOfficeCode;
                row.LogoUrl = report.Logo;

                return row;
            }

            foreach (var item in report.CategorySummaries)
            {
                flatRows.Add(PopulateShared(new FlatLoanDelinquencyRow
                {
                    Section = "CATEGORY",
                    Category = item.Category,
                    TotalCount = item.TotalCount,
                    MaleCount = item.MaleCount,
                    FemaleCount = item.FemaleCount,
                    GroupCount = item.GroupCount,
                    TotalBalance = item.TotalBalance,
                    MaleBalance = item.MaleBalance,
                    FemaleBalance = item.FemaleBalance,
                    GroupBalance = item.GroupBalance,
                    Percentage = item.PercentageOfTotalLoans
                }));
            }

            foreach (var g in report.GenderSummariesAfter60Days)
            {
                flatRows.Add(PopulateShared(new FlatLoanDelinquencyRow
                {
                    Section = "GENDER",
                    Gender = g.Gender,
                    TotalAmount = g.TotalAmount,
                    DelinquentAmount = g.DelinquentAmount,
                    DelinquentLoanCount = g.DelinquentLoanCount,
                    Percentage = g.PercentageOfTotalDelinquency
                }));
            }

            foreach (var lt in report.LoanTypeSummariesAfter60Days)
            {
                flatRows.Add(PopulateShared(new FlatLoanDelinquencyRow
                {
                    Section = "LOAN_TYPE",
                    LoanType = lt.LoanType,
                    TotalAmount = lt.TotalAmount,
                    MaleBalance = lt.MaleAmount,
                    FemaleBalance = lt.FemaleAmount,
                    GroupBalance = lt.GroupAmount,
                    Percentage = lt.PercentageOfTotalDelinquency
                }));
            }

            return flatRows;
        }
    }

}
