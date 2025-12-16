using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberNoneCashOperationsP;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.UI.Controllers.CashDeskOperations
{
    [CheckSessionTimeOutAttribute]

    public class AccountToAccountTransferController : BaseController
    {
        private readonly AccountToAccountTransferService _transferService;
        private readonly MemberNoneCashOperationServices _memberNoneCashOperationServices;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;

        public AccountToAccountTransferController(AccountToAccountTransferService cashDeskService = null, BranchAccountService chartOfAccountServices = null, MemberNoneCashOperationServices memberNoneCashOperationServices = null, BranchServices branchServices = null)
        {
            _transferService = cashDeskService;
            this._branchAccountService = chartOfAccountServices;
            _memberNoneCashOperationServices=memberNoneCashOperationServices;
            _branchServices=branchServices;
        }
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Transfters()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }


        [HttpGet]
        public async Task<JsonResult> GetReceiverDetails(string receiverRef)
        {
            var receiver = await _transferService.GetReceiverInfo(receiverRef);
            if (receiver == null)
                return Json(new { success = false, message = "Receiver not found." }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                success = true,
                data = new
                {
                    customerId = receiver.Customer.CustomerId,
                    name = receiver.Customer.name,
                    branch = receiver.Branch.Name,
                    branchCode = receiver.Branch.BranchCode,
                    phone= receiver.Customer.Phone,
                    cni = receiver.Customer.IDNumber,
                    accounts = receiver.Accounts.Select(a => new
                    {
                        accountType = a.AccountType,
                        balance = a.Balance, // ✅ raw numeric value
                        accountNumber = a.AccountNumber
                    })
                }
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            try
            {
                ViewBag.KEY = KEY;
                //if (path == "transactions")
                //{
                //    var account = await _acountServices.GetTransactionsAsync();
                //    return PartialView(partialView, account);
                //}
                //else if (path == "confirmation_request")
               if (path == "details")
                {
                    var transfer = await _transferService.GetTransfer(KEY);
                    var accountTransferDto = new AccountToAccountTransferDto { TransfterRequest = transfer };
                    return PartialView(partialView, accountTransferDto);
                }
                else if (path == "pending_request")
                {

                    var transfers = await _transferService.GetPendingTransfers();
                    var accountTransferDto = new AccountToAccountTransferDto { TransfterRequests = transfers };
                    return PartialView(partialView, accountTransferDto);
                }
                else if (path == "all_transfer_request")
                {
                    var transfers = await _transferService.GetTransfers();
                    var accountTransferDto = new AccountToAccountTransferDto { TransfterRequests = transfers };
                    return PartialView(partialView, accountTransferDto);
                }
                else
                {
                    //var transfer = await _overdraftFacilityConfigServices.GetTransfer(KEY);
                    //var receiver = await _overdraftFacilityConfigServices.GetReceiverInfo(transfer.c);
                    //var conf = await _overdraftFacilityConfigServices.GetSavingConfigurationAggregates();
                    //ViewBag.Source = account;
                    //ViewBag.Destination = account;
                    //ViewBag.TransferTypes = conf.transferTypes;
                    //return PartialView(partialView, new Account());
                    return null;
                }
               

            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = ex.Message; // Store error message
                return RedirectToAction("Index", "Error"); // Redirect to error page
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadTransferData(GetTransfersDataTableQuery tableQuery)
        {
            try
            {
                var dataTable = await _transferService.GetDataTableAsync(tableQuery);

                // Convert object data to strongly typed TransferDto list
                var transferList = JsonConvert.DeserializeObject<List<Transfer>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                return Json(new
                {
                    draw = dataTable.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = transferList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Optional: log ex here
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading transfer data.");
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetSenderAccountsPartial(string senderRef)
        {
            var dto = await _transferService.GetAccountToAccountTransfer(senderRef);
            if (dto == null || dto.Sender == null || !dto.Sender.Accounts.Any())
            {
                return Content("<div class='text-danger p-3'>Sender not found or has no eligible accounts.</div>");
            }

            return PartialView("_SendersForm", dto);
        }

        [HttpGet]
        public async Task<JsonResult> GetSenderDetails(string senderRef)
        {
            var dto = await _transferService.GetAccountToAccountTransfer(senderRef);
            if (dto == null || dto.Sender == null || !dto.Sender.Accounts.Any())
            {
                return Json(new { success = false, message = "Sender not found or no accounts available." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = dto }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitTransfer(TransferRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .ToList();

                var errorMessageString = string.Join(" | ", errorMessages);

                return Json(new
                {
                    success = false,
                    status = false,
                    message = $"❌ Validation Failed: {errorMessageString}"
                });
            }

            try
            {
                var result = await _transferService.SubmitTransferRequest(request);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    status = false,
                    message = $"❌ An unexpected error occurred: {ex.Message}"
                });
            }
        }


        public bool ValidateTransfer(AccountToAccountTransferDto dto, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (dto.Transfer.Amount <= 0)
            {
                errorMessage = "Transfer amount must be greater than zero.";
                return false;
            }

            var senderAccount = dto.Sender.Accounts
                .FirstOrDefault(a => a.AccountNumber == dto.Transfer.SenderAccountNumber);

            if (senderAccount == null)
            {
                errorMessage = "Sender account not found.";
                return false;
            }

            if (senderAccount.Balance < dto.Transfer.Amount)
            {
                errorMessage = "Sender account does not have sufficient balance.";
                return false;
            }

            var receiverAccount = dto.Receiver.Accounts
                .FirstOrDefault(a => a.AccountNumber == dto.Transfer.ReceiverAccountNumber);

            if (receiverAccount == null)
            {
                errorMessage = "Receiver account not found.";
                return false;
            }

            return true;
        }



        [HttpPost]
        public async Task<ActionResult> ApproveTransfer(TransferConfirmation command)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .ToList();

                var errorMessageString = string.Join(" | ", errorMessages);

                return Json(new
                {
                    success = false,
                    status = false,
                    message = $"❌ Validation Failed: {errorMessageString}"
                });
            }

            try
            {
                var result = await _transferService.TransferConfirmation(command);

                return Json(new
                {
                    success = result.Result,
                    status = result.MessageStatus,
                    message = Messaging.MessageResult(result)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    status = false,
                    message = $"❌ An unexpected error occurred: {ex.Message}"
                });
            }
        }


        public async Task<ActionResult> DeleteOperation(string KEY)
        {
            var data = await _memberNoneCashOperationServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public async Task<ActionResult> GetReport(string path)
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"MainReport.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Payment/MainReport.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceipts";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> GetReportOtherPayment()
        {
            this.HttpContext.Session["rptType"] = "ReportParameterLess";
            this.HttpContext.Session["ReportName"] = $"OtherTransactionReciept.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Transactions/Reciepts/OtherTransactionReciept.rpt";
            this.HttpContext.Session["rpttitle"] = $"MemberReceiptsOtherPayment";
            return Json(new { success = true, status = false, message = "Parameters OK." }, JsonRequestBehavior.AllowGet);

        }
        public async Task<bool> GetChartOfAccounts()
        {
            var listing = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(null);
            var accountsDto = _branchAccountService.DropDownGen(listing.ToList());
            ViewBag.chartOfAccounts = accountsDto.ToList();
            return true;
        }
    }
}