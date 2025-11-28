using CBS.BusinessService.Accounting_V2.AccntStatements;
using CBS.BusinessService.Accounting_V2.BalanceSheet;
using CBS.BusinessService.Accounting_V2.JournalEntries;
using CBS.BusinessService.Config;

using CBS.FrontDesk.Data.Entity.Accounting_V2.Queries;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.BalanceSheet
{
    public class BalanceSheetController : BaseController
    {
        private readonly BalanceSheetService _balanceSheetService;
        private readonly BranchServices _branchServices;

        public BalanceSheetController(BalanceSheetService balanceSheetService, BranchServices branchServices)
        {
            _balanceSheetService = balanceSheetService;
            _branchServices = branchServices;
        }

        /// <summary>
        /// Loads the Balance Sheet main page.
        /// </summary>
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Generates Balance Sheet report dataset based on user-selected filters.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GenerateBalanceSheet(AccountingV2ReportsFilter model)
        {
            try
            {
                // ==============================================================
                // STEP 1 — Retrieve balance sheet dataset based on filters
                // ==============================================================           
                var groups = await _balanceSheetService.GetBalanceSheetAsync(model);


                // Retrieve branch metadata for report header display
                var BranchInformation = await _branchServices.GetBranch(_branchServices.GetBranchID());

                // Stop processing if no entries were returned
                if (groups == null || !groups.Any())
                {
                    this.HttpContext.Session["rptSource"] = null;
                    this.HttpContext.Session["BranchInfo"] = null;

                    return Json(new
                    {
                        success = false,
                        message = "No data found for the selected filters."
                    }, JsonRequestBehavior.AllowGet);
                }

                // ==============================================================
                // STEP 2 — Build static report header (bank and branch details)
                // ==============================================================                
                var header = new BankHeaderInformation
                {
                    BankId = BranchInformation.Bank.Id,
                    BankBankCode = BranchInformation.Bank.BankCode,
                    BankCode = BranchInformation.Bank.BankCode,
                    BankName = BranchInformation.Bank.Name,
                    BankTelephone = BranchInformation.Bank.Telephone,
                    BankEmail = BranchInformation.Bank.Email,
                    BankAddress = BranchInformation.Bank.Address,
                    BankLogoUrl = BranchInformation.Bank.LogoUrl,
                    BankMotto = BranchInformation.Bank.Motto,
                    BankRegistrationNumber = BranchInformation.Bank.RegistrationNumber,
                    BankImmatriculationNumber = BranchInformation.Bank.ImmatriculationNumber,
                    BankPBox = BranchInformation.Bank.PBox,

                    BranchId = BranchInformation.Id,
                    BranchCode = BranchInformation.BranchCode,
                    BranchName = BranchInformation.Name,
                    BranchTelephone = BranchInformation.Telephone,
                    BranchEmail = BranchInformation.Email,
                    BranchAddress = BranchInformation.Address,
                    BranchLogoUrl = BranchInformation.LogoUrl,
                    BranchCapital = BranchInformation.Capital,
                    BranchRegistrationNumber = BranchInformation.RegistrationNumber,
                    BranchImmatriculationNumber = BranchInformation.ImmatriculationNumber,
                    BranchPBox = BranchInformation.PBox
                };

                // ==============================================================
                // STEP 3 — Transform balance sheet groups into flat rows
                // --------------------------------------------------------------
                // • Each group item becomes one report line
                // • Each row includes:
                //     - Balance sheet item values (Gross / Provision / NetN / NetN1)
                //     - Group and reference information
                //     - Bank + Branch header for report rendering
                // ==============================================================
                // 
                var data = groups
                .SelectMany(g => g.Items.Select(i => new BalanceSheetFlatItems
                {
                    GroupName = g.Name,
                    GroupReference = g.Refernce,
                    Reference = i.Ref,
                    Referernce = i.Ref,
                    Heading = i.Heading,
                    Gross = i.Gross ?? 0,
                    AmountProv = i.AmortProvision ?? 0,
                    NetN = i.NetN ?? 0,
                    NetN1 = i.NetN1 ?? 0,
                    PrintedBy = _balanceSheetService.GetUserFullName(),
                  
                }))
                .ToList();
                data.ForEach(item => ApplyHeader(item, header));

                // Save flattened dataset for Crystal Report viewer
                this.HttpContext.Session["rptSource"] = data;
                this.HttpContext.Session["BranchInfo"] = BranchInformation;

                return Json(new { success = true, message = "Journal Entries loaded." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Attach bank and branch header into one statement row.
        /// Crystal Reports requires header metadata available on each record.
        /// </summary>
        private void ApplyHeader(BalanceSheetFlatItems item, BankHeaderInformation header)
        {
            item.BankId = header.BankId;
            item.BankBankCode = header.BankBankCode;
            item.BankName = header.BankName;
            item.BankTelephone = header.BankTelephone;
            item.BankEmail = header.BankEmail;
            item.BankAddress = header.BankAddress;
            item.BankLogoUrl = header.BankLogoUrl;
            item.BankMotto = header.BankMotto;
            item.BankRegistrationNumber = header.BankRegistrationNumber;
            item.BankImmatriculationNumber = header.BankImmatriculationNumber;
            item.BankPBox = header.BankPBox;

            item.BranchId = header.BranchId;
            item.BranchCode = header.BranchCode;
            item.BranchName = header.BranchName;
            item.BranchTelephone = header.BranchTelephone;
            item.BranchEmail = header.BranchEmail;
            item.BranchAddress = header.BranchAddress;
            item.BranchLogoUrl = header.BranchLogoUrl;
            item.BranchCapital = header.BranchCapital;
            item.BranchRegistrationNumber = header.BranchRegistrationNumber;
            item.BranchImmatriculationNumber = header.BranchImmatriculationNumber;
            item.BranchPBox = header.BranchPBox;
        }



        /// <summary>
        /// Setup Crystal Report parameters for the Journal Entries report.
        /// </summary>
        [HttpPost]
        public ActionResult GetReport(string path)
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = "BalanceSheet.rpt";
            this.HttpContext.Session["rptpath"] = "~/AppFiles/Accountingv2Reporting/ReportRPT/BalanceSheet.rpt";
            this.HttpContext.Session["rpttitle"] = "BS";

            return Json(new
            {
                success = true,
                message = "Report parameters set successfully."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
