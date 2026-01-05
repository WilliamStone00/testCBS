using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Application;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.LoanManagementP;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using Irony.Parsing;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanManagementP
{
    //[CheckSessionTimeOutAttribute]

    public class LoanManagementController : BaseController
    {
        // GET: Loan

        private readonly LoanApplicationServices _LoanServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly LoanPurposeServices _loanPurposeServices;
        private readonly LoanTermServices _loanTermServices;

        public LoanManagementController(LoanApplicationServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null, LoanProductServices loanProductServices = null, LoanPurposeServices loanPurposeServices = null, LoanTermServices loanTermServices = null)
        {
            _LoanServices = LoanServices;
            _loanCommiteeMember = loanCommiteeMember;
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
            _loanProductServices = loanProductServices;
            _loanPurposeServices = loanPurposeServices;
            _loanTermServices = loanTermServices;
        }
        //EditLoanApplication
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /LoanManagement/SearchMembers?q=jean
        [HttpGet]
        public JsonResult SearchMembers(string q)
        {
            q = (q ?? "").Trim();

            // TODO: replace with real service call
            var data = new[]
            {
        new {
            id = "1",
            name = "Jean Mbarga",
            code = "0030005129",
            phoneNumber = "699001122",
            branchCode = "003",
            branchName = "BUEA MAIN",
            balance = 150000,
            currency = "FCFA"
        },
        new {
            id = "2",
            name = "Marie Kouam",
            code = "0030005128",
            phoneNumber = "677445566",
            branchCode = "001",
            branchName = "DOUALA CENTRAL",
            balance = 280000,
            currency = "FCFA"
        },
        new {
            id = "3",
            name = "Paul Ndongo",
            code = "0030005130",
            phoneNumber = "655778899",
            branchCode = "002",
            branchName = "YAOUNDE CITY",
            balance = 95000,
            currency = "FCFA"
        }
    }
            .Where(x =>
                (x.name ?? "").ToLower().Contains(q.ToLower()) ||
                (x.code ?? "").ToLower().Contains(q.ToLower()) ||
                (x.phoneNumber ?? "").ToLower().Contains(q.ToLower()) ||
                (x.branchCode ?? "").ToLower().Contains(q.ToLower())
            )
            .Take(10)
            .ToList();

            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMemberLoans(string memberId)
        {
            var loans = new[]
            {
        new
        {
            Id = "LN-1001",
            Name = "Classic Loan",
            Code = "LN-1001",
            Status = "Active",
            Currency = "FCFA",
            CreatedOn = new DateTime(2025, 12, 20),

            // ✅ add these
            Facility = "CLASSIC",
            LoanProductId = "P-CL-001",

            Amount = 500000m,
            Balance = 320000m,
            AccrualInterest = 12500m,
            VAT = 2375m,
            Penalty = 0m,
            DelinquentInterest = 0m,
            DelinquentAmount = 0m,
            DelinquentDays = 0m,

            TotalDueAmount = 320000m + 12500m + 2375m
        },
        new
        {
            Id = "LN-1002",
            Name = "SSF Loan",
            Code = "LN-1002",
            Status = "Delinquent",
            Currency = "FCFA",
            CreatedOn = new DateTime(2025, 12, 27),

            // ✅ add these
            Facility = "SSF",
            LoanProductId = "P-SSF-001",

            Amount = 200000m,
            Balance = 45000m,
            AccrualInterest = 7800m,
            VAT = 1482m,
            Penalty = 2500m,
            DelinquentInterest = 9200m,
            DelinquentAmount = 15000m,
            DelinquentDays = 26m,

            TotalDueAmount = 45000m + 7800m + 1482m + 2500m + 9200m + 15000m
        }
    }
            .OrderByDescending(x => x.CreatedOn)
            .ToList();

            return Json(new { success = true, data = loans }, JsonRequestBehavior.AllowGet);
        }
        // -------------------------------------------------
        // Member Lookup (by MemberReference / Code)
        // Used for CoObligor / Shotee / Inter-Cooperation
        // -------------------------------------------------
        [HttpGet]
        public async Task<JsonResult> LookupMember(string memberReference)
        {
            try
            {
                memberReference = (memberReference ?? "").Trim();

                if (string.IsNullOrWhiteSpace(memberReference))
                {
                    return Json(new { success = false, message = "memberReference is required." }, JsonRequestBehavior.AllowGet);
                }

                // ✅ TODO: Replace with real call
                // Example (recommended):
                // var member = await _individualProfileServices.GetCustomerByMemberReference(memberReference);

                // -------------------------
                // MOCKED MEMBER RESPONSE
                // -------------------------
                var mockedMembers = new[]
                {
            new {
                MemberId = "M-001",
                MemberReference = "0030005129",
                Name = "Jean Mbarga",
                PhoneNumber = "699001122",
                Email = "jean.mbarga@mail.com",
                Address = "Buea - Molyko",
                IdCardNumber = "CNI-123456",
                IssueDate = "2022-05-10",
                ExpireDate = "2032-05-10",
                BranchId = "B-003",
                BranchCode = "003",
                BranchName = "BUEA MAIN"
            },
            new {
                MemberId = "M-002",
                MemberReference = "0030005128",
                Name = "Marie Kouam",
                PhoneNumber = "677445566",
                Email = "marie.kouam@mail.com",
                Address = "Douala - Akwa",
                IdCardNumber = "CNI-998877",
                IssueDate = "2021-02-01",
                ExpireDate = "2031-02-01",
                BranchId = "B-001",
                BranchCode = "001",
                BranchName = "DOUALA CENTRAL"
            }
        };

                var member = mockedMembers.FirstOrDefault(x =>
                    string.Equals(x.MemberReference, memberReference, StringComparison.OrdinalIgnoreCase));

                if (member == null)
                {
                    return Json(new { success = false, message = "Member not found." }, JsonRequestBehavior.AllowGet);
                }

                // Keep field names aligned with the JS I gave you earlier
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        memberId = member.MemberId,
                        memberReference = member.MemberReference,
                        name = member.Name,
                        phoneNumber = member.PhoneNumber,
                        email = member.Email,
                        address = member.Address,
                        idCardNumber = member.IdCardNumber,
                        issueDate = member.IssueDate,
                        expireDate = member.ExpireDate,
                        branchId = member.BranchId,
                        branchCode = member.BranchCode,
                        branchName = member.BranchName
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lookup failed: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // -------------------------------------------------
        // Relationship dropdown (Guarantor relationship)
        // -------------------------------------------------
        [HttpGet]
        public JsonResult GetRelationships()
        {
            // TODO: load from DB/config if needed
            var data = new[]
            {
        new { Id="REL-PARENT", Name="Parent" },
        new { Id="REL-SPOUSE", Name="Spouse" },
        new { Id="REL-SIBLING", Name="Sibling" },
        new { Id="REL-FRIEND", Name="Friend" },
        new { Id="REL-EMPLOYER", Name="Employer" },
        new { Id="REL-EMPLOYEE", Name="Employee" },
        new { Id="REL-BUSINESS_PARTNER", Name="Business Partner" },
        new { Id="REL-GUARDIAN", Name="Guardian" },
        new { Id="REL-OTHER", Name="Other" },
    }.ToList();

            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        // -------------------------------------------------
        // Collateral Types dropdown
        // -------------------------------------------------
        [HttpGet]
        public JsonResult GetCollateralTypes()
        {
            // TODO: replace with DB/service
            var data = new[]
            {
        new { Id="COL-LAND", Name="Land Title" },
        new { Id="COL-HOUSE", Name="House / Building" },
        new { Id="COL-VEHICLE", Name="Vehicle" },
        new { Id="COL-MACHINE", Name="Machine / Equipment" },
        new { Id="COL-STOCK", Name="Stock / Inventory" },
        new { Id="COL-SALARY", Name="Salary Assignment" },
        new { Id="COL-OTHER", Name="Other" },
    }.ToList();

            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        // -------------------------------------------------
        // Get Member Accounts (by memberId)
        // Used for CoObligor/Shotee account selection & blocking
        // -------------------------------------------------
        [HttpGet]
        public async Task<JsonResult> GetMemberAccountsByMemberId(string memberId)
        {
            try
            {
                memberId = (memberId ?? "").Trim();

                if (string.IsNullOrWhiteSpace(memberId))
                {
                    return Json(new { success = false, message = "memberId is required." }, JsonRequestBehavior.AllowGet);
                }

                // ✅ TODO: Replace with real service call
                // Example:
                // var accounts = await _accountServices.GetMemberAccounts(memberId);

                // -------------------------
                // MOCKED ACCOUNTS RESPONSE
                // -------------------------
                var accounts = new[]
                {
                    new { Id="A-SAV-001", AccountNumber="SAV-001", AccountType="Savings",          AccountName="Main Savings",          Status="Active",  Balance=150000m, Currency="FCFA" },
                    new { Id="A-CUR-002", AccountNumber="CUR-002", AccountType="Current",          AccountName="Current Account",       Status="Active",  Balance=325000m, Currency="FCFA" },
                    new { Id="A-DEP-020", AccountNumber="DEP-020", AccountType="Deposit",          AccountName="Deposit Account",       Status="Dormant", Balance=85000m,  Currency="FCFA" },
                    new { Id="A-DSV-030", AccountNumber="DSV-030", AccountType="DailySavings",     AccountName="Daily Savings",         Status="Active",  Balance=22000m,  Currency="FCFA" },
                    new { Id="A-SHR-010", AccountNumber="SHR-010", AccountType="OrdinaryShares",   AccountName="Ordinary Shares",       Status="Active",  Balance=100000m, Currency="FCFA" },
        }
                .Where(a => !string.Equals(a.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(a => a.Balance)
                .ToList();

                return Json(new { success = true, data = accounts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to load accounts: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // -------------------------------------------------
        // 1) Institution Custom Loan Types
        // -------------------------------------------------
        [HttpGet]
        public JsonResult GetInstitutionLoanTypes()
        {
            // TODO: replace with DB/service
            // These are institution-defined "Loan Types" for internal statistics/reporting
            var types = new[]
            {
            new { Id = "LT-001", Name = "Agriculture Loan Type" },
            new { Id = "LT-002", Name = "Small Business Loan Type" },
            new { Id = "LT-003", Name = "Salary Loan Type" },
            new { Id = "LT-004", Name = "Emergency Loan Type" },
        }.ToList();

            return Json(new { success = true, data = types }, JsonRequestBehavior.AllowGet);
        }

        // -------------------------------------------------
        // 2) Institution Loan Products (NOT PCMF products)
        //    - Each product contains the PCMF purpose under which it was registered
        //    - Each product contains policy constraints to guide the manager
        // -------------------------------------------------
        [HttpGet]
        public JsonResult GetInstitutionLoanProducts(string facility, string mode, string oldLoanId, string productId = null)
        {
            // facility: CLASSIC | SSF | OVERDRAFT | LOC
            // mode: NEW | REFINANCE | RESCHEDULE (optional)
            // oldLoanId: optional

            facility = (facility ?? "").Trim().ToUpperInvariant();
            mode = (mode ?? "NEW").Trim().ToUpperInvariant();

            // Defaults (institution policy baseline)
            const decimal DEFAULT_MIN_ORDINARY_SHARES_POLICY = 75000m;
            const decimal DEFAULT_MIN_SAVINGS_PERCENT = 20m;

            // Grace period is in MONTHS (not days)
            // If you want to keep it simple, use a standard range and override per product.
            const int DEFAULT_MIN_GRACE_MONTHS = 0;  // allow 0 month grace
            const int DEFAULT_MAX_GRACE_MONTHS = 3;  // typical max grace
                                                     // ✅ If productId not provided but oldLoanId is, you may derive it here (optional but useful)

            var all = new List<object>
    {
        // ============================================================
        // CLASSIC PRODUCTS
        // ============================================================

        new
        {
            Id = "P-CL-001",
            Name = "Classic Loan - Standard",
            Facility = "CLASSIC",

            InstitutionPurpose = "Working Capital (Institution Purpose)",
            PcmfPurpose = "Working Capital",

            MinAmount = 100000m,
            MaxAmount = 2000000m,
            MinInterest = 1.5m,
            MaxInterest = 3.5m,

            MinMonth = 6,
            MaxMonth = 24,
            Periods = new[] { 6, 12, 18, 24 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 3,

            MinSavings = 50000m,
            MinOrdinaryShares = 20000m,

            // ✅ Policy Enhancements
            RequireDownPayment = true,
            DownPaymentRatePercent = 10m,
            MinSavingsPercent = DEFAULT_MIN_SAVINGS_PERCENT,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = true,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        new
        {
            Id = "P-CL-002",
            Name = "Classic Loan - Education",
            Facility = "CLASSIC",

            InstitutionPurpose = "Education Support (Institution Purpose)",
            PcmfPurpose = "Education",

            MinAmount = 50000m,
            MaxAmount = 1500000m,
            MinInterest = 1.2m,
            MaxInterest = 3.0m,

            MinMonth = 3,
            MaxMonth = 18,
            Periods = new[] { 3, 6, 9, 12, 18 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 6, // education may allow longer grace

            MinSavings = 25000m,
            MinOrdinaryShares = 15000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 5m,
            MinSavingsPercent = DEFAULT_MIN_SAVINGS_PERCENT,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = true,
            RequireCollateral = false,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        new
        {
            Id = "P-CL-003",
            Name = "Classic Loan - Agriculture",
            Facility = "CLASSIC",

            InstitutionPurpose = "Agriculture & Farming (Institution Purpose)",
            PcmfPurpose = "Agriculture",

            MinAmount = 100000m,
            MaxAmount = 2500000m,
            MinInterest = 1.4m,
            MaxInterest = 3.2m,

            MinMonth = 6,
            MaxMonth = 30,
            Periods = new[] { 6, 12, 18, 24, 30 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 6,

            MinSavings = 60000m,
            MinOrdinaryShares = 25000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 10m,
            MinSavingsPercent = DEFAULT_MIN_SAVINGS_PERCENT,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = true,
            RequireCollateral = true,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        new
        {
            Id = "P-CL-004",
            Name = "Classic Loan - Asset Purchase",
            Facility = "CLASSIC",

            InstitutionPurpose = "Asset Purchase (Institution Purpose)",
            PcmfPurpose = "Asset Purchase",

            MinAmount = 200000m,
            MaxAmount = 4000000m,
            MinInterest = 1.6m,
            MaxInterest = 3.6m,

            MinMonth = 6,
            MaxMonth = 36,
            Periods = new[] { 6, 12, 18, 24, 36 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 3,

            MinSavings = 100000m,
            MinOrdinaryShares = 40000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 15m,
            MinSavingsPercent = DEFAULT_MIN_SAVINGS_PERCENT,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = true,
            RequireCoObligor = true,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        // ============================================================
        // SSF PRODUCTS (Loan within savings / salary-backed)
        // ============================================================

        new
        {
            Id = "P-SSF-001",
            Name = "SSF Loan - Salary Backed",
            Facility = "SSF",

            InstitutionPurpose = "Consumption (Institution Purpose)",
            PcmfPurpose = "Consumption",

            MinAmount = 50000m,
            MaxAmount = 1500000m,
            MinInterest = 1.0m,
            MaxInterest = 2.5m,

            MinMonth = 3,
            MaxMonth = 18,
            Periods = new[] { 3, 6, 9, 12, 18 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 2,

            MinSavings = 25000m,
            MinOrdinaryShares = 15000m,

            RequireDownPayment = false,
            DownPaymentRatePercent = 0m,

            MinSavingsPercent = 30m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = false,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = false,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        new
        {
            Id = "P-SSF-002",
            Name = "SSF Loan - Savings Pledge",
            Facility = "SSF",

            InstitutionPurpose = "Short-term Needs (Institution Purpose)",
            PcmfPurpose = "Consumption",

            MinAmount = 30000m,
            MaxAmount = 800000m,
            MinInterest = 0.9m,
            MaxInterest = 2.2m,

            MinMonth = 1,
            MaxMonth = 12,
            Periods = new[] { 1, 3, 6, 9, 12 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 1,

            MinSavings = 50000m,
            MinOrdinaryShares = 10000m,

            RequireDownPayment = false,
            DownPaymentRatePercent = 0m,

            MinSavingsPercent = 40m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = false,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = false,
            SupportsRefinance = true,
            SupportsReschedule = true
        },

        // ============================================================
        // OVERDRAFT PRODUCTS
        // ============================================================

        new
        {
            Id = "P-OD-001",
            Name = "Overdraft Facility - Standard",
            Facility = "OVERDRAFT",

            InstitutionPurpose = "Working Capital (Institution Purpose)",
            PcmfPurpose = "Working Capital",

            MinAmount = 100000m,
            MaxAmount = 3000000m,
            MinInterest = 1.8m,
            MaxInterest = 4.0m,

            MinMonth = 1,
            MaxMonth = 12,
            Periods = new[] { 1, 3, 6, 12 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = DEFAULT_MIN_GRACE_MONTHS,
            MaxGracePeriodMonths = DEFAULT_MAX_GRACE_MONTHS,

            MinSavings = 80000m,
            MinOrdinaryShares = 25000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 10m,
            MinSavingsPercent = 25m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = true,
            RequireCoObligor = false,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = false,
            SupportsRefinance = false,
            SupportsReschedule = false
        },

        new
        {
            Id = "P-OD-002",
            Name = "Overdraft Facility - Salary Account",
            Facility = "OVERDRAFT",

            InstitutionPurpose = "Short-term Cashflow Support (Institution Purpose)",
            PcmfPurpose = "Working Capital",

            MinAmount = 100000m,
            MaxAmount = 2000000m,
            MinInterest = 1.5m,
            MaxInterest = 3.5m,

            MinMonth = 1,
            MaxMonth = 12,
            Periods = new[] { 1, 3, 6, 12 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = DEFAULT_MIN_GRACE_MONTHS,
            MaxGracePeriodMonths = 2,

            MinSavings = 50000m,
            MinOrdinaryShares = 20000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 5m,
            MinSavingsPercent = 20m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = false,
            RequireCoObligor = true,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = false,
            SupportsRefinance = false,
            SupportsReschedule = false
        },

        // ============================================================
        // LOC PRODUCTS
        // ============================================================

        new
        {
            Id = "P-LOC-001",
            Name = "Line of Credit - Standard",
            Facility = "LOC",

            InstitutionPurpose = "Business Expansion (Institution Purpose)",
            PcmfPurpose = "Business Expansion",

            MinAmount = 200000m,
            MaxAmount = 5000000m,
            MinInterest = 1.6m,
            MaxInterest = 3.8m,

            MinMonth = 6,
            MaxMonth = 36,
            Periods = new[] { 6, 12, 18, 24, 36 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 3,

            MinSavings = 100000m,
            MinOrdinaryShares = 40000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 15m,
            MinSavingsPercent = 25m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = true,
            RequireCoObligor = true,
            RequireMortgage = false,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = false,
            SupportsReschedule = false
        },

        new
        {
            Id = "P-LOC-002",
            Name = "Line of Credit - Mortgage Backed",
            Facility = "LOC",

            InstitutionPurpose = "Business Expansion (Institution Purpose)",
            PcmfPurpose = "Business Expansion",

            MinAmount = 500000m,
            MaxAmount = 10000000m,
            MinInterest = 1.7m,
            MaxInterest = 4.2m,

            MinMonth = 12,
            MaxMonth = 48,
            Periods = new[] { 12, 18, 24, 36, 48 },

            // ✅ Grace Period Policy (MONTHS)
            MinGracePeriodMonths = 0,
            MaxGracePeriodMonths = 6,

            MinSavings = 150000m,
            MinOrdinaryShares = 50000m,

            RequireDownPayment = true,
            DownPaymentRatePercent = 20m,
            MinSavingsPercent = 30m,
            MinOrdinarySharesPolicy = DEFAULT_MIN_ORDINARY_SHARES_POLICY,

            RequireGuarantor = false,
            RequireCollateral = true,
            RequireCoObligor = false,
            RequireMortgage = true,
            RequireDocuments = true,

            RequiresPartialUpfront = true,
            SupportsRefinance = false,
            SupportsReschedule = false
        }
    };
            // ✅ filter by facility (optional)
            if (!string.IsNullOrWhiteSpace(facility))
            {
                all = all.Where(x =>
                {
                    var prop = x.GetType().GetProperty("Facility");
                    var val = prop?.GetValue(x)?.ToString();
                    return string.Equals(val, facility, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            // ✅ KEY: filter by productId (for RESCHEDULE/REFINANCE policy load)
            if (!string.IsNullOrWhiteSpace(productId))
            {
                all = all.Where(x =>
                {
                    var prop = x.GetType().GetProperty("Id");
                    var val = prop?.GetValue(x)?.ToString();
                    return string.Equals(val, productId, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            //// Facility filter
            //if (!string.IsNullOrWhiteSpace(facility))
            //{
            //    all = all.Where(x =>
            //    {
            //        var prop = x.GetType().GetProperty("Facility");
            //        var val = prop != null ? prop.GetValue(x) as string : null;
            //        return string.Equals(val, facility, StringComparison.OrdinalIgnoreCase);
            //    }).ToList();
            //}

            return Json(new { success = true, data = all }, JsonRequestBehavior.AllowGet);
        }


        //[HttpGet]
        //public JsonResult GetInstitutionScoreConfig(string facility)
        //{
        //    facility = (facility ?? "").Trim().ToUpperInvariant();

        //    // MOCKED: later load from DB/config by InstitutionId / BranchId / Facility / Product
        //    var cfg = new InstitutionScoreConfigDto
        //    {
        //        OverScoreBonus = 15,
        //        MediumRiskBuffer = 10,
        //        Bands = new List<ScoreBandDto>
        //        {
        //            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 40 },
        //            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 60 },
        //            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 75 },
        //        }
        //    };

        //    // Example tuning per facility
        //    if (facility == "OVERDRAFT" || facility == "LOC")
        //    {
        //        cfg.Bands = new List<ScoreBandDto>
        //        {
        //            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 50 },
        //            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 70 },
        //            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 85 },
        //        };
        //        cfg.OverScoreBonus = 10; // OD/LOC stricter
        //        cfg.MediumRiskBuffer = 8;
        //    }

        //    if (facility == "SSF")
        //    {
        //        // SSF (loan within savings) generally more secure:
        //        // keep required slightly lower, but still depend on policy
        //        cfg.Bands = new List<ScoreBandDto>
        //        {
        //            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 35 },
        //            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 55 },
        //            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 70 },
        //        };
        //    }

        //    return Json(new { success = true, data = cfg }, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public JsonResult GetInstitutionScoreConfig(string facility)
        {
            facility = (facility ?? "").Trim().ToUpperInvariant();

            var cfg = new InstitutionScoreConfigDto
            {
                OverScoreBonus = 15,
                MediumRiskBuffer = 10,

                // ✅ ADD THIS: what user selects in the multiselect
                Assessments = new List<ScoreAssessmentDto>
        {
            new ScoreAssessmentDto { Id="ASM-GUARANTOR_OK", Name="Strong guarantor / Co-obligor", Points=15 },
            new ScoreAssessmentDto { Id="ASM-COLLATERAL_OK", Name="Acceptable collateral provided", Points=20 },
            new ScoreAssessmentDto { Id="ASM-INCOME_OK", Name="Stable income / salary evidence", Points=15 },
            new ScoreAssessmentDto { Id="ASM-HISTORY_OK", Name="Good repayment history", Points=10 },
            new ScoreAssessmentDto { Id="ASM-LOW_RISK_AREA", Name="Low risk sector / activity", Points=10 },
        },

                Bands = new List<ScoreBandDto>
        {
            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 40 },
            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 60 },
            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 75 },
        }
            };

            if (facility == "OVERDRAFT" || facility == "LOC")
            {
                cfg.Bands = new List<ScoreBandDto>
        {
            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 50 },
            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 70 },
            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 85 },
        };
                cfg.OverScoreBonus = 10;
                cfg.MediumRiskBuffer = 8;
            }

            if (facility == "SSF")
            {
                cfg.Bands = new List<ScoreBandDto>
        {
            new ScoreBandDto { MaxAmount = 200000m, RequiredScore = 35 },
            new ScoreBandDto { MaxAmount = 1000000m, RequiredScore = 55 },
            new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 70 },
        };
            }

            return Json(new { success = true, data = cfg }, JsonRequestBehavior.AllowGet);
        }

        // -------------------------------------------------
        // Existing endpoints (you already have)
        // -------------------------------------------------

        //[HttpGet]
        //public JsonResult GetLoanCharges(string productId)
        //{
        //    // TODO: Replace with real service
        //    var charges = new[]
        //    {
        //    new { Id="C-APPR", Name="Appraisal Fee", Amount=5000m, Mode="BEFORE_APPRAISAL" },
        //    new { Id="C-FILE", Name="File Opening Fee", Amount=2500m, Mode="BEFORE_APPRAISAL" },
        //    new { Id="C-PROC", Name="Processing Fee", Amount=8000m, Mode="AFTER_DISBURSEMENT" }
        //}.ToList();

        //    if (!string.IsNullOrWhiteSpace(productId) && productId.StartsWith("P-SSF", StringComparison.OrdinalIgnoreCase))
        //    {
        //        charges.RemoveAll(c => c.Id == "C-FILE");
        //    }

        //    return Json(new { success = true, data = charges }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public JsonResult GetMemberAccounts(string memberId)
        {
            // TODO: Replace with real account service
            var accounts = new[]
            {
        new { Id="A-SAV-001", AccountNumber="SAV-001", AccountType="Savings",            AccountName="Main Savings",          Status="Active",  Balance=150000m, Currency="FCFA" },
        new { Id="A-CUR-002", AccountNumber="CUR-002", AccountType="Current",            AccountName="Current Account",       Status="Active",  Balance=325000m, Currency="FCFA" },

        new { Id="A-SHR-010", AccountNumber="SHR-010", AccountType="OrdinaryShares",     AccountName="Ordinary Shares",       Status="Active",  Balance=100000m,  Currency="FCFA" },
        new { Id="A-PSH-011", AccountNumber="PSH-011", AccountType="PreferenceShares",   AccountName="Preference Shares",     Status="Active",  Balance=75000m,  Currency="FCFA" },

        new { Id="A-DEP-020", AccountNumber="DEP-020", AccountType="Deposit",            AccountName="Deposit Account",       Status="Dormant", Balance=85000m,  Currency="FCFA" },

        new { Id="A-DSV-030", AccountNumber="DSV-030", AccountType="DailySavings",       AccountName="Daily Savings",         Status="Active",  Balance=22000m,  Currency="FCFA" },
        new { Id="A-DSV-031", AccountNumber="DSV-031", AccountType="DailySavings",       AccountName="Daily Savings (Agent)", Status="Active",  Balance=12000m,  Currency="FCFA" },

        new { Id="A-SAL-040", AccountNumber="SAL-040", AccountType="Salary",             AccountName="Salary Account",        Status="Active",  Balance=98000m,  Currency="FCFA" },

        new { Id="A-CUR-004", AccountNumber="CUR-004", AccountType="Current",            AccountName="Business Current",      Status="Inactive",Balance=0m,      Currency="FCFA" },
    }.ToList();

            return Json(new { success = true, data = accounts }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetInstitutionScoringModel(string facility)
        {
            facility = (facility ?? "").Trim().ToUpperInvariant();

            bool isOdLoc = facility == "OVERDRAFT" || facility == "LOC";
            bool isSsf = facility == "SSF";

            // ----------------------------------------------------
            // Weight Profile per Facility (C# 7.3 style)
            // ----------------------------------------------------
            decimal capacityW, historyW, collateralW, businessW, kycW, penaltyW;

            if (isOdLoc)
            {
                capacityW = 1.25m;
                historyW = 1.10m;
                collateralW = 0.90m;
                businessW = 1.35m;
                kycW = 1.10m;
                penaltyW = 1.20m;
            }
            else if (isSsf)
            {
                capacityW = 1.05m;
                historyW = 1.10m;
                collateralW = 1.35m;
                businessW = 0.85m;
                kycW = 1.10m;
                penaltyW = 1.15m;
            }
            else
            {
                capacityW = 1.15m;
                historyW = 1.20m;
                collateralW = 1.10m;
                businessW = 1.00m;
                kycW = 1.10m;
                penaltyW = 1.10m;
            }

            // ----------------------------------------------------
            // Complex Assessments (C# 7.3 target-typed new removed)
            // ----------------------------------------------------
            var assessments = new List<ScoringAssessmentDto>
    {
        // =========================
        // A) Capacity / Affordability
        // =========================
        new ScoringAssessmentDto { Id="SC-CAP-INCOME-PROOF", Name="Income Proven (payslip/bank inflow/contracts)", Points=25, Category="Capacity", Weight=capacityW, Mandatory=true, EvidenceHint="Payslips / Bank statement / Contracts", AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-DTI-LOW",      Name="DTI <= 35% (strong)",                              Points=25, Category="Capacity", Weight=capacityW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-DTI-MID",      Name="DTI 36% - 45% (acceptable)",                      Points=15, Category="Capacity", Weight=capacityW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-DTI-HIGH",     Name="DTI 46% - 55% (weak)",                            Points=5,  Category="Capacity", Weight=capacityW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-DSCR-OK",      Name="DSCR >= 1.5 (business/salary surplus)",           Points=20, Category="Capacity", Weight=capacityW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-EXPENSE-RATIO",Name="Expenses ratio reasonable vs income",              Points=10, Category="Capacity", Weight=capacityW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-CAP-SHOCK-BUFFER", Name="Client has buffer for shocks (savings/insurance/extra income)", Points=10, Category="Capacity", Weight=capacityW, AppliesTo="ALL" },

        // =========================
        // B) Stability
        // =========================
        new ScoringAssessmentDto { Id="SC-STAB-EMP-TENURE", Name="Employment/Business tenure >= 24 months", Points=15, Category="Stability", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-STAB-RESIDENCE",  Name="Stable residence & traceability",         Points=10, Category="Stability", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-STAB-DEPENDENTS", Name="Dependents load reasonable vs income",    Points=8,  Category="Stability", Weight=1.0m, AppliesTo="ALL" },

        // =========================
        // C) Repayment / Credit Behavior
        // =========================
        new ScoringAssessmentDto { Id="SC-HIST-REPAYMENT",   Name="Good internal repayment history",            Points=25, Category="History", Weight=historyW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-HIST-NO-ARREARS",  Name="No current arrears / clean delinquency",     Points=30, Category="History", Weight=historyW, Mandatory=true, EvidenceHint="Loan ledger / delinquency report", AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-HIST-RESTRUCTURE", Name="No recent restructuring/write-off history",   Points=15, Category="History", Weight=historyW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-HIST-EXT-CREDIT",  Name="External credit check clean (if available)",  Points=10, Category="History", Weight=historyW, AppliesTo="ALL" },

        // =========================
        // D) Relationship / Member Discipline
        // =========================
        new ScoringAssessmentDto { Id="SC-REL-ACCOUNT-AGE",     Name="Relationship tenure with MFI >= 12 months", Points=15, Category="Relationship", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-REL-SAVINGS-LVL",     Name="Savings level adequate vs facility",        Points=20, Category="Relationship", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-REL-SAVINGS-CONSIST", Name="Savings consistency (regular deposits)",    Points=20, Category="Relationship", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-REL-TRANSACTION",     Name="Healthy transaction pattern (inflow regularity)", Points=20, Category="Relationship", Weight=1.0m, AppliesTo="ALL" },

        // =========================
        // E) Collateral / Guarantee
        // =========================
        new ScoringAssessmentDto { Id="SC-COLL-GUAR-QUALITY", Name="Guarantor quality strong (income/character)", Points=20, Category="Collateral", Weight=collateralW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-COLL-COVERAGE",     Name="Collateral/guarantee coverage ratio adequate",Points=30, Category="Collateral", Weight=collateralW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-COLL-VAL",          Name="Collateral valuation verified & documented",  Points=20, Category="Collateral", Weight=collateralW, Mandatory=true, EvidenceHint="Valuation report / photos / proof of ownership", AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-COLL-LIQUIDITY",     Name="Collateral liquidity (easy to realize)",     Points=15, Category="Collateral", Weight=collateralW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-COLL-LEGAL",         Name="Collateral legal enforceability confirmed",  Points=15, Category="Collateral", Weight=collateralW, Mandatory=true, EvidenceHint="Title deed / registration / lien agreement", AppliesTo="ALL" },

        // =========================
        // F) KYC / Compliance / Field Verification
        // =========================
        new ScoringAssessmentDto { Id="SC-KYC-COMPLETE",   Name="KYC complete & valid",                    Points=20, Category="KYC", Weight=kycW, Mandatory=true, EvidenceHint="ID, address, photos, signatures", AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-DOCS-SUPPORT",   Name="All supporting documents provided",       Points=15, Category="KYC", Weight=kycW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-SITEVISIT-DONE", Name="Site/Home visit done & consistent",       Points=20, Category="KYC", Weight=kycW, Mandatory=true, EvidenceHint="Visit report + pictures + geo note", AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-AML-SCREEN",     Name="AML/PEP screening clear (if applicable)", Points=10, Category="KYC", Weight=kycW, Mandatory=true, AppliesTo="ALL" },

        // =========================
        // G) Business Strength (OD/LOC emphasized by weight)
        // =========================
        new ScoringAssessmentDto { Id="SC-BIZ-ACTIVE",   Name="Business active & verified",                    Points=20, Category="Business", Weight=businessW, AppliesTo="BUSINESS" },
        new ScoringAssessmentDto { Id="SC-BIZ-TURNOVER", Name="Turnover supports requested limit",             Points=35, Category="Business", Weight=businessW, AppliesTo="OD_LOC" },
        new ScoringAssessmentDto { Id="SC-BIZ-STOCK",    Name="Stock/inventory verified",                     Points=15, Category="Business", Weight=businessW, AppliesTo="BUSINESS" },
        new ScoringAssessmentDto { Id="SC-BIZ-MARKET",   Name="Market/customer base verified",                Points=10, Category="Business", Weight=businessW, AppliesTo="BUSINESS" },
        new ScoringAssessmentDto { Id="SC-BIZ-CYCLE",    Name="Cash conversion cycle understood & stable",     Points=10, Category="Business", Weight=businessW, AppliesTo="OD_LOC" },

        // =========================
        // H) Purpose / Risk Context
        // =========================
        new ScoringAssessmentDto { Id="SC-PURPOSE-CLEAR",        Name="Loan purpose clear & verified",        Points=15, Category="Purpose", Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-RISK-CONCENTRATION",   Name="No concentration risk (sector/group)",Points=10, Category="Risk",    Weight=1.0m, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-RISK-EXPOSURE-LOW",    Name="Low existing exposure",               Points=35, Category="Risk",    Weight=1.0m, AppliesTo="ALL" },

        // =========================
        // I) Penalties (negative scoring)
        // =========================
        new ScoringAssessmentDto { Id="SC-PEN-ARREARS-RECENT",      Name="Recent arrears within last 6 months", Points=0, MinPoints=-30, IsPenalty=true, Category="Penalties", Weight=penaltyW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-PEN-INCONSISTENT-INFO",   Name="Inconsistent info/documents found",   Points=0, MinPoints=-25, IsPenalty=true, Category="Penalties", Weight=penaltyW, Mandatory=true, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-PEN-UNTRACEABLE",         Name="Client/guarantor untraceable",        Points=0, MinPoints=-40, IsPenalty=true, Category="Penalties", Weight=penaltyW, Mandatory=true, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-PEN-SECTOR-HIGH",         Name="High-risk sector / volatile income",   Points=0, MinPoints=-15, IsPenalty=true, Category="Penalties", Weight=penaltyW, AppliesTo="ALL" },
        new ScoringAssessmentDto { Id="SC-PEN-FRAUD-FLAG",          Name="Fraud suspicion flag",                Points=0, MinPoints=-100,IsPenalty=true, Category="Penalties", Weight=penaltyW, Mandatory=true, AppliesTo="ALL" }
    };

            // ----------------------------------------------------
            // Bands (C# 7.3 if/else)
            // ----------------------------------------------------
            List<ScoreBandDto> bands;

            if (isOdLoc)
            {
                bands = new List<ScoreBandDto>
                {
                    new ScoreBandDto { MaxAmount = 200000m,    RequiredScore = 65 },
                    new ScoreBandDto { MaxAmount = 1000000m,   RequiredScore = 78 },
                    new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 88 }
                };
            }
            else if (isSsf)
            {
                bands = new List<ScoreBandDto>
                {
                    new ScoreBandDto { MaxAmount = 200000m,    RequiredScore = 50 },
                    new ScoreBandDto { MaxAmount = 1000000m,   RequiredScore = 65 },
                    new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 75 }
                };

                AddIfMissing(assessments, new ScoringAssessmentDto
                {
                    Id = "SC-SSF-SAV-PLEDGE",
                    Name = "Savings pledge coverage adequate vs amount",
                    Points = 40,
                    Category = "Collateral",
                    Weight = collateralW,
                    Mandatory = true,
                    AppliesTo = "SSF"
                });

                AddIfMissing(assessments, new ScoringAssessmentDto
                {
                    Id = "SC-SSF-BLOCKING",
                    Name = "Blocking / recovery setup confirmed",
                    Points = 20,
                    Category = "Collateral",
                    Weight = collateralW,
                    Mandatory = true,
                    AppliesTo = "SSF"
                });
            }
            else
            {
                bands = new List<ScoreBandDto>
                {
                    new ScoreBandDto { MaxAmount = 200000m,    RequiredScore = 55 },
                    new ScoreBandDto { MaxAmount = 1000000m,   RequiredScore = 70 },
                    new ScoreBandDto { MaxAmount = 999999999m, RequiredScore = 82 }
                };
            }

            // ----------------------------------------------------
            // Config
            // ----------------------------------------------------
            var cfg = new InstitutionScoreModelDto
            {
                Facility = facility,
                OverScoreBonus = isOdLoc ? 10 : 15,
                MediumRiskBuffer = isOdLoc ? 8 : 10,
                Bands = bands,
                Assessments = assessments
            };

            // Optional: if your DTO supports it
            // cfg.MandatoryChecks = assessments.Where(a => a.Mandatory).Select(a => a.Id).ToList();

            return Json(new { success = true, data = cfg }, JsonRequestBehavior.AllowGet);
        }

        // helper (keeps endpoint clean) - KEEP ONLY THIS ONE
        private static void AddIfMissing(List<ScoringAssessmentDto> list, ScoringAssessmentDto item)
        {
            if (list == null) return;
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.Id)) return;

            if (!list.Any(x => x != null && !string.IsNullOrWhiteSpace(x.Id) &&
                               string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase)))
            {
                list.Add(item);
            }
        }

        [HttpGet]


        public async Task<ActionResult> SubmitNewLoanApplication(NewLoanApplicationRequest model)
        {

            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            // mock async work
            await Task.Delay(1);

            // simple return
            return Json(new
            {
                success = true,
                status = "SUCCESS",
                message = "Mocked: Loan application submitted."
            });


        }
        public async Task<ActionResult> PostRefinanceLoan(RefinanceLoanApplicationRequest model)
        {
            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            // mock async work
            await Task.Delay(1);

            // simple return
            return Json(new
            {
                success = true,
                status = "SUCCESS",
                message = "Mocked: Loan application submitted."
            });
        }

        public async Task<ActionResult> PostRescheduleLoan(RescheduleLoanApplicationRequest model)
        {
            //var data = await _loanApplicationServices.Reschedule(model.RescheduleCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            // mock async work
            await Task.Delay(1);

            // simple return
            return Json(new
            {
                success = true,
                status = "SUCCESS",
                message = "Mocked: Loan application submitted."
            });
        }

        public async Task<ActionResult> PostOverdraft(OverdraftApplicationRequest model)
        {
            //var data = await _loanApplicationServices.CreateOverdraft(model.AddOverdraftCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            // mock async work
            await Task.Delay(1);

            // simple return
            return Json(new
            {
                success = true,
                status = "SUCCESS",
                message = "Mocked: Loan application submitted."
            });
        }

        public async Task<ActionResult> PostLineOfCredit(LineOfCreditApplicationRequest model)
        {
            //var data = await _loanApplicationServices.CreateLineOfCredit(model.AddLineOfCreditCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            //var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
            //return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            // mock async work
            await Task.Delay(1);

            // simple return
            return Json(new
            {
                success = true,
                status = "SUCCESS",
                message = "Mocked: Loan application submitted."
            });
        }

        [HttpGet]
        public JsonResult GetLoanCharges(string productId)
        {
            var charges = new List<LoanChargeDto>
    {
        new LoanChargeDto { Id="C-FORM", Name="Loan Forms Fee", Mode="BEFORE_APPRAISAL", CalcType=ChargeCalcType.Fixed, FixedAmount=1000m },
        new LoanChargeDto { Id="C-APPRAISAL", Name="Appraisal Fee", Mode="BEFORE_APPRAISAL", CalcType=ChargeCalcType.Range,
            Ranges = new List<ChargeRangeTier> {
                new ChargeRangeTier { FromAmount=10000m, ToAmount=50000m, FeeAmount=1000m },
                new ChargeRangeTier { FromAmount=50001m, ToAmount=200000m, FeeAmount=2500m },
                new ChargeRangeTier { FromAmount=200001m, ToAmount=500000m, FeeAmount=5000m },
                new ChargeRangeTier { FromAmount=500001m, ToAmount=1000000m, FeeAmount=8000m },
                new ChargeRangeTier { FromAmount=1000001m, ToAmount=5000000m, FeeAmount=15000m }
            }
        },
        new LoanChargeDto { Id="C-CRM", Name="CRM Fee", Mode="BEFORE_APPRAISAL", CalcType=ChargeCalcType.Fixed, FixedAmount=2000m },
        new LoanChargeDto { Id="C-FILE", Name="File Opening Fee", Mode="BEFORE_APPRAISAL", CalcType=ChargeCalcType.Fixed, FixedAmount=2500m },

        new LoanChargeDto { Id="C-PROC", Name="Processing Fee", Mode="AFTER_DISBURSEMENT", CalcType=ChargeCalcType.Rate, RatePercent=2.5m, MinFee=5000m, MaxFee=75000m },
        new LoanChargeDto { Id="C-INS", Name="Loan Protection / Insurance", Mode="AFTER_DISBURSEMENT", CalcType=ChargeCalcType.Rate, RatePercent=1.0m, MinFee=1000m },
        new LoanChargeDto { Id="C-LEGAL", Name="Legal / Agreement Fee", Mode="AFTER_DISBURSEMENT", CalcType=ChargeCalcType.Range,
            Ranges = new List<ChargeRangeTier> {
                new ChargeRangeTier { FromAmount=0m, ToAmount=200000m, FeeAmount=3000m },
                new ChargeRangeTier { FromAmount=200001m, ToAmount=1000000m, FeeAmount=7000m },
                new ChargeRangeTier { FromAmount=1000001m, ToAmount=5000000m, FeeAmount=15000m }
            }
        },
        new LoanChargeDto { Id="C-SMS", Name="SMS Notification Fee", Mode="AFTER_DISBURSEMENT", CalcType=ChargeCalcType.Fixed, FixedAmount=500m },
        new LoanChargeDto { Id="C-DISB", Name="Disbursement / Transfer Fee", Mode="AFTER_DISBURSEMENT", CalcType=ChargeCalcType.Fixed, FixedAmount=1500m }
    };

            if (!string.IsNullOrWhiteSpace(productId) &&
                productId.StartsWith("P-SSF", StringComparison.OrdinalIgnoreCase))
            {
                charges.RemoveAll(c => c.Id == "C-FILE");
            }

            return Json(new { success = true, data = charges }, JsonRequestBehavior.AllowGet);
        }

        private static decimal ComputeSystemCharge(LoanChargeDto c, decimal requestedAmount)
        {
            decimal fee = 0m;

            switch (c.CalcType)
            {
                case ChargeCalcType.Fixed:
                    fee = c.FixedAmount ?? 0m;
                    break;

                case ChargeCalcType.Rate:
                    var pct = c.RatePercent ?? 0m;
                    fee = (requestedAmount * pct) / 100m;
                    break;

                case ChargeCalcType.Range:
                    var tier = (c.Ranges ?? new List<ChargeRangeTier>())
                        .FirstOrDefault(x => requestedAmount >= x.FromAmount && requestedAmount <= x.ToAmount);
                    fee = tier?.FeeAmount ?? 0m;
                    break;
            }

            if (c.MinFee.HasValue && fee < c.MinFee.Value) fee = c.MinFee.Value;
            if (c.MaxFee.HasValue && fee > c.MaxFee.Value) fee = c.MaxFee.Value;

            return Math.Round(fee, 0, MidpointRounding.AwayFromZero);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateLoanApplication(UpdateLoanApplicationCommand model)
        {

            if (!ModelState.IsValid)
            {
                return JsonValidationErrorResponse();

            }

            var data = await _LoanServices.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        public async Task<ActionResult> EditLoanApplication(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            var loanApplication = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            if (loanApplication == null)
            {
                return View("NotFound"); // Show a Not Found view if loan is null
            }
            ViewBag.KEY = KEY;
            //var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            //ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            var customer = await InitializeCustomerData(loanApplication.CustomerId);
            ViewBag.LoanProducts = await _loanProductServices.GetLoanProductsDropDown();
            var fees = await _loanProductServices.GetFees();
            ViewBag.LoanFees = new MultiSelectList(
        fees,
        "value", // Replace with the property name for the value (e.g., ID)
        "Text",  // Replace with the property name for the display text (e.g., Name)
        selectedValues: null // Optionally, pass a list of selected values
    );
            ViewBag.MembersLoan = ViewBag.LoanFees;
            ViewBag.KEY = KEY;
            //await PopulateAggregatesInViewBag();
            var LoanApplicationToCommand = _LoanServices.MapLoanApplicationToCommand(loanApplication);
            LoanApplicationToCommand.Customer = customer.CustomerList;
            //var customer = await InitializeCustomerData(KEY);
            return View(LoanApplicationToCommand);


            //var CustomerLoans = await _loanservices.GetLoanByCustomerID(KEY);
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {

            var results = await _individualProfileServices.GetCustomerLight(KEY);
            return results;
        }
        [HttpGet]
        public async Task<ActionResult> Details(string KEY = null)
        {
            if (string.IsNullOrEmpty(KEY))
            {
                return RedirectToAction("Index"); // Redirect to list page if KEY is not provided
            }

            var loanApplication = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            if (loanApplication == null)
            {
                return View("NotFound"); // Show a Not Found view if loan is null
            }
            var customer = await InitializeCustomerData(loanApplication.CustomerId);
            loanApplication.Customer = customer;
            return View(loanApplication);
        }


        [HttpGet]
        public async Task<ActionResult> Download(
            string searchCriteria = "all",
            string dateFrom = null,
            string dateTo = null,
            string status = "Open",
            string deliquentStatus = "Current",
            string branchId = null,
            string loanCategory = "all",
            string loanTarget = "all",
            string approvalStatus = "all")
        {
            try
            {

                Branch branch = null;

                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // ✅ Retrieve branch details efficiently
                if (!string.IsNullOrWhiteSpace(branchId))
                {
                    branch = await _branchServices.GetBranch(branchId);
                }
                else if (!_branchServices.IsHeadOffice())
                {
                    branch = await _branchServices.GetBranch(_branchServices.GetBranchID());
                }

                // Ensure valid branch
                branch = new Branch();

                // ✅ Construct query object
                var getLoansDataTableQuery = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = new DataTableOptions
                    {
                        pageSize = 30000, // Export a large number of records
                        start = 0,
                        searchValue = !string.IsNullOrWhiteSpace(searchCriteria) ? searchCriteria : "all"
                    },
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    MemberId = "n/a",
                    LoanCategory = loanCategory,
                    LoanTarget = loanTarget,
                    ApprovalStatus = approvalStatus,
                };
                getLoansDataTableQuery.DataTableOptions = GetDataTableOptions();

                //if (string.IsNullOrWhiteSpace(searchCriteria))
                //{
                //    searchCriteria = "all";
                //}

                getLoansDataTableQuery.DataTableOptions.pageSize = 30000;
                getLoansDataTableQuery.DataTableOptions.start = 0;

                // ✅ Fetch loan data
                var dataTable = await _LoanServices.GetDataTableAsync(getLoansDataTableQuery, searchCriteria);
                var loans = JsonConvert.DeserializeObject<List<LoanApplication>>(JsonConvert.SerializeObject(dataTable.data));

                if (loans == null || loans.Count == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No data available for export.");
                }

                // ✅ GetAllowAnonymous exported by user
                string exportedBy = Session["FullName"]?.ToString() ?? "Unknown";

                // ✅ Generate Excel file
                var exportFile = LoanApplicationExcelGenerator.GenerateLoanApplicationExcel(
                    loans,
                    branch,
                    exportedBy,
                    fileTitle: "LOAN APPLICATION QUERY",
                    dateFrom,
                    dateTo
                );

                return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting data.");
            }
        }


        [HttpPost]
        public async Task<ActionResult> LoadLoanData(
            string searchCriteria = "all",
            string dateFrom = null,
            string dateTo = null,
            string status = "Open",
            string deliquentStatus = "Current",
            string branchId = null,
            string loanCategory = "all",
            string loanTarget = "all",
            string approvalStatus = "all"
        )
        {
            try
            {
                // Date Parsing with Improved Error Handling
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParseExact(dateFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStartDate))
                {
                    startDate = parsedStartDate;
                }

                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParseExact(dateTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEndDate))
                {
                    endDate = parsedEndDate.AddDays(1).AddTicks(-1); // Set end of the day
                }

                // Construct Query Object with Additional Parameters
                var query = new GetLoanApplicationsDataTableQuery
                {
                    DataTableOptions = PostDataTableOptions(),
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    BranchId = !string.IsNullOrWhiteSpace(branchId) ? branchId : "all",
                    Status = status,
                    MemberId = "n/a",
                    LoanCategory = loanCategory,
                    LoanTarget = loanTarget,
                    ApprovalStatus = approvalStatus,
                };

                // Fetch Data
                var dataTable = await _LoanServices.GetDataTableAsync(query, searchCriteria);
                var loanList = JsonConvert.DeserializeObject<List<LoanApplication>>(JsonConvert.SerializeObject(dataTable.data));

                return Json(new
                {
                    draw = query.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = loanList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading loan data: " + ex.Message);
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {

            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }


        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)

        {
            if (serviceOption == "Loan")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        //var data = await auditTrailServices.GetLoans();
                        var sysData = new MemberOperationPanel { Loans = null };
                        return PartialView(partialView, sysData);
                    };
                }

            }
            else if (serviceOption == "LoanCommiteeMember")
            {
                //if (path == "list")
                //{
                //    return async () =>
                //    {
                //        var data = await _loanCommiteeMember.GetLoanCommiteeMembers();
                //        var sysData = new LoanCommitee { LoanCommiteeMembers = data.ToList() };
                //        return PartialView(partialView, sysData);
                //    };
                //}
                //else if (path == "new")
                //{
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = new LoanCommiteeMember() });
                //}
                //else
                //{
                //    ViewBag.Key = key;
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = await _loanCommiteeMember.GetLoanCommiteeMember(key) });
                //}

            }

            return null;
        }
        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
            var loanpurpose = await _loanPurposeServices.GetAllLoanPurpose();
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            var loanTerms = await _loanProductServices.GetProductTermOrDurationFromConfiguredProduct();
            var categories = await _loanProductServices.GetProductCategoryFromConfiguredProduct();
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;

            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.CalculateInterestOn = productEnumAgregates.CalculateInterestOn;
            ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
            ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
            ViewBag.LoanStatuses = productEnumAgregates.LoanStatuses;
            ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
            ViewBag.LoanInterestPeriods = productEnumAgregates.LoanInterestPeriods;
            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.RefundOrders = productEnumAgregates.RefundOrders;
            ViewBag.LoanPurposes = loanpurpose;
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;
            ViewBag.AmortizationTypes = productEnumAgregates.AmortizationTypes;
            ViewBag.LoanApplicationTypes = productEnumAgregates.LoanApplicationTypes;
            ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
            ViewBag.LoanCategories = productEnumAgregates.LoanCategories;
            ViewBag.LoanTargets = productEnumAgregates.LoanTargets;
            ViewBag.LoanTerms = loanTerms;
            ViewBag.Categories = categories;
        }
        public async Task<bool> GetList()
        {
            //ViewBag.Groups = await auditTrailServices.GetLoans();
            //var users = await _userManagementServices.GetUserDropDownList();
            //ViewBag.Users = users.ToList();
            return true;
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _LoanServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}