using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Application;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.LoanCommitee;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.MemberOperation
{
    [CheckSessionTimeOutAttribute]

    public class MemberOperationController : BaseController
    {
        // GET: MemberOperation
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly LoanPurposeServices _loanPurposeServices;
        private readonly LoanApplicationServices _loanApplicationServices;
        private readonly LoanServices _loanservices;
        private readonly LoanAmortizationServices _loanAmortizationServices;
        private readonly LoanCommiteeValidationHistoryServices _loanCommiteeValidationHistoryServices;
        private readonly LoanApplicationCollateralServices _loanApplicationCollateralServices;
        private readonly LoanGuarantorServices _loanGuarantorServices;
        private readonly LoanProductCollateralServices _loanProductCollateralServices;
        private readonly AttachedDocumentServices _attachedDocumentServices;
        private readonly DocumentServices _documentServices;
        private readonly LoanTermServices _loanTermServices;
        private readonly BranchServices _branchServices;
        private readonly EconomicActivityServices _economicActivityServices;

        public MemberOperationController(IndividualProfileServices individualProfileServices, LoanProductServices loanProductServices = null, LoanPurposeServices loanPurposeServices = null, LoanApplicationServices loanApplicationServices = null, LoanServices loanServices = null, LoanAmortizationServices loanAmortizationServices = null, LoanCommiteeValidationHistoryServices loanCommiteeValidationHistoryServices = null, LoanApplicationCollateralServices loanApplicationCollateralServices = null, LoanGuarantorServices loanGuarantorServices = null, LoanProductCollateralServices loanProductCollateralServices = null, AttachedDocumentServices services = null, DocumentServices documentServices = null, LoanTermServices loanTermServices = null, BranchServices branchServices = null, EconomicActivityServices economicActivityServices = null)
        {
            _individualProfileServices = individualProfileServices;
            _loanProductServices = loanProductServices;
            _loanPurposeServices = loanPurposeServices;
            _loanApplicationServices = loanApplicationServices;
            _loanservices = loanServices;
            _loanAmortizationServices = loanAmortizationServices;
            _loanCommiteeValidationHistoryServices = loanCommiteeValidationHistoryServices;
            _loanApplicationCollateralServices = loanApplicationCollateralServices;
            _loanGuarantorServices = loanGuarantorServices;
            _loanProductCollateralServices = loanProductCollateralServices;
            _attachedDocumentServices = services;
            _documentServices = documentServices;
            _loanTermServices = loanTermServices;
            _branchServices=branchServices;
            _economicActivityServices=economicActivityServices;
        }
        public async Task<ActionResult> Members()
        {

            return View();


        }
        public async Task<ActionResult> PendingDisbursment()
        {

            return View();


        }

        public async Task<ActionResult> OperationPanel(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            //var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            //ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            var customer = await InitializeCustomerData(KEY);
            ViewBag.LoanProducts = new List<StringValues>(); /*await _loanProductServices.GetLoanProductsDropDown()*/;
            ViewBag.LoanFees = await _loanProductServices.GetFees();
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.MembersLoan = ViewBag.LoanFees;
            ViewBag.KEY = KEY;
            await PopulateAggregatesInViewBag();
            //var customer = await InitializeCustomerData(KEY);
            return View(new MemberOperationPanel { LoanApplication = new LoanApplication { CustomerId = KEY }, AddLoanApplicationCommand = new AddLoanApplicationCommand { CustomerId = KEY }, Customer = customer.CustomerList });


            //var CustomerLoans = await _loanservices.GetLoanByCustomerID(KEY);
        }
        public async Task<ActionResult> MyMembers()
        {

            return View();
        }
        public async Task<ActionResult> LoanSimulation()
        {
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;

            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
            ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
            ViewBag.InterestCalculationPeriod = productEnumAgregates.LoanInterestPeriods;
            ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
            ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
            ViewBag.AmortizationTypes = productEnumAgregates.AmortizationTypes;

            return View(new MemberOperationPanel());
        }
        //
        public async Task<ActionResult> Loan(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }
        public async Task<ActionResult> LoanApplicationApproval(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            //var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            //ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            var loanApplications = await _loanApplicationServices.GetLoanApplications("Validated");
            return View(new MemberOperationPanel { LoanApplications = loanApplications.ToList() });
        }
        public async Task<ActionResult> LoanApplicationForCommitees(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {

            //var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            //ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
            var loanApplications = await _loanApplicationServices.GetLoanApplications("Pending");
            return View(new MemberOperationPanel { LoanApplications = loanApplications.ToList() });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string serviceOption = null, string path = null)
         {
            ViewBag.KEY = KEY;
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            if (path == "loanapplication")
            {
                ViewBag.LoanProducts = await _loanProductServices.GetLoanProductsDropDown();
                ViewBag.LoanFees = await _loanProductServices.GetFees();
                ViewBag.MembersLoan = ViewBag.LoanApplicationStatus;
                ViewBag.KEY = KEY;
                await PopulateAggregatesInViewBag();
                //var customer = await InitializeCustomerData(KEY);
                return PartialView(partialView, new MemberOperationPanel { LoanApplication = new LoanApplication { CustomerId = KEY }, AddLoanApplicationCommand = new AddLoanApplicationCommand { CustomerId = KEY } });

            }
            else
            {
                if (serviceOption == "applications")
                {
                    if (path == "list_of_members_loan_application")
                    {
                        var loanApplications = await _loanApplicationServices.GetLoanApplicationByCustomerID(KEY);
                        return PartialView(partialView, new MemberOperationPanel { LoanApplications = loanApplications.ToList() });

                    }
                    else if (path == "loan_for_disbursed")
                    {


                        var loan = await _loanservices.GetLoan(KEY);
                        var Accounts = await _individualProfileServices.GetCustomerAccounts(loan.CustomerId);
                        var documentAttachedToLoans = loan.LoanApplication.DocumentAttachedToLoans;
                        var loanApplication = loan.LoanApplication;
                        ViewBag.CustomerAccounts = _individualProfileServices.MembersAccounts(Accounts.ToList()).ToList();
                        ViewBag.DisbursmentStatuses = productEnumAgregates.DisbursmentStatuses;
                        var loand = new AddLoanDisbumentCommand
                        {
                            //AccountNumber = Accounts.FirstOrDefault().accountNumber,
                            //Comment = $"Loan successfully disbursed to Customer number: [{loan.CustomerId}] and Account number: [{Accounts.FirstOrDefault().accountNumber}]. Documentation updated. For inquiries, contact Loan Manager {loan.LoanManager}.",
                            LoanId = loan.Id,
                            Status = "Disbursed"
                        };
                        var collaterals = await _loanProductCollateralServices.GetAllLaonApplicationCollateralByApplicationIdQuery(KEY);
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        var loanCommiteeValidationHistories = loanApplication.LoanCommiteeValidations;
                        return PartialView(partialView, new MemberOperationPanel { Loan= loan, AddLoanDisbumentCommand = loand, LoanCommiteeValidationHistory = new Data.Entity.LoanCommitee.LoanCommiteeValidationHistory { LoanApplicationId = loanApplication.Id }, LoanCommiteeValidationHistories = loanCommiteeValidationHistories.ToList(), LoanCollateras = collaterals.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "perding_disbursement")
                    {
                        var data = await _loanservices.GetPendingDisbursementLoans();
                        return PartialView(partialView, new MemberOperationPanel { Loans = data.ToList() });
                    }
                    else if (path == "application_detail" || path == "loan_approval")
                    {
                        ViewBag.Value = "0";
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        string defaultComment = $"Loan Rejected By {Session["FullName"].ToString()}. Dated: {DateTime.Now}";
                        string defaultStatus = ApprovalStatus.Rejected.ToString();
                        if (path=="loan_approval")
                        {
                            defaultComment=$"Loan {ApprovalStatus.Approved.ToString()} By {Session["FullName"].ToString()}. Dated: {DateTime.Now}";
                            defaultStatus = ApprovalStatus.Approved.ToString();
                        }
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        return PartialView(partialView, new MemberOperationPanel { DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id, OTPCode="0000", ApprovalStatus=defaultStatus, ApprovalComment=defaultComment } });
                    }
                    else if (path == "upload_document")
                    {
                        ViewBag.Value = "0";
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        return PartialView(partialView, new MemberOperationPanel { DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanApplication = loanApplication });
                    }
                    else if (path == "get_collateral_listing")
                    {
                        ViewBag.Value = "0";
                        var collaterals = await _loanProductCollateralServices.GetAllLaonApplicationCollateralByApplicationIdQuery(KEY);
                        return PartialView(partialView, new MemberOperationPanel { LoanCollateras = collaterals.ToList() });
                    }
                    else if (path == "get_collateral")
                    {
                        ViewBag.KEY = KEY;
                        var applicationCollateral = await _loanApplicationCollateralServices.GetLoanCollateral(KEY);
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(applicationCollateral.LoanApplicationId);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        return PartialView(partialView, new MemberOperationPanel { LoanCollatera = applicationCollateral, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "get_guarantor")
                    {
                        ViewBag.KEY = KEY;
                        var guarantor = await _loanGuarantorServices.GetLoanGuarantor(KEY);
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(guarantor.LoanApplicationId);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);

                        return PartialView(partialView, new MemberOperationPanel { LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "application_detail")
                    {
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.Id);
                        var updateLoanApplication = new UpdateLoanApplicationStatusCommand { Id=loanApplication.Id };

                        return PartialView(partialView, new MemberOperationPanel { LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = updateLoanApplication });
                    }

                    //loan_commitee_validation_history
                    else if (path == "loan_commitee_validation_history")
                    {
                        ViewBag.Value = "0";
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
                        ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
                        ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
                        ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
                        ViewBag.InterestCalculationPeriod = productEnumAgregates.LoanInterestPeriods;
                        ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
                        ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
                        ViewBag.AmortizationTypes = productEnumAgregates.AmortizationTypes;
                        var amortization = new LoanParameters();

                        if (loanApplication != null && loanApplication.LoanProduct != null && loanApplication.LoanProduct.LoanProductRepaymentCycles != null)
                        {
                            var repaymentCycle = loanApplication.LoanProduct.LoanProductRepaymentCycles.FirstOrDefault(x => x.Id == loanApplication.RepaymentCircle);

                            if (repaymentCycle != null)
                            {
                                amortization = new LoanParameters
                                {
                                    InterestRate = loanApplication.InterestRate,
                                    AmortizationType = loanApplication.AmortizationType,
                                    Amount = loanApplication.Amount,
                                    LoanDuration = loanApplication.LoanDuration,
                                    RepaymentCycle = repaymentCycle.RepaymentCycle
                                };
                            }
                            else
                            {
                                // Handle the case where repaymentCycle is null
                                // You may throw an exception, log an error, or set default values for amortization
                            }
                        }
                        else
                        {
                            // Handle the case where loanApplication or loanProduct or loanProductRepaymentCycles is null
                            // You may throw an exception, log an error, or set default values for amortization
                        }

                        //var amortization =new LoanParameters { InterestRate= loanApplication.InterestRate,
                        //  AmortizationType=loanApplication.AmortizationType, Amount=loanApplication.Amount,
                        // LoanDuration= loanApplication.LoanDuration, RepaymentCycle=loanApplication.LoanProduct.LoanProductRepaymentCycles.FirstOrDefault(x=>x.Id== loanApplication.RepaymentCircle).RepaymentCycle,
                        //};
                        var collaterals = await _loanProductCollateralServices.GetAllLaonApplicationCollateralByApplicationIdQuery(KEY);
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        var loanCommiteeValidationHistories = loanApplication.LoanCommiteeValidations;
                        return PartialView(partialView, new MemberOperationPanel { LoanParameter = amortization, LoanCommiteeValidationHistory = new Data.Entity.LoanCommitee.LoanCommiteeValidationHistory { LoanApplicationId = loanApplication.Id }, LoanCommiteeValidationHistories = loanCommiteeValidationHistories.ToList(), LoanCollateras = collaterals.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "create_loan_simulation")
                    {
                        ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
                        ViewBag.LoanInterestMethods = productEnumAgregates.LoanInterestMethods;
                        ViewBag.LoanInterestTypes = productEnumAgregates.LoanInterestTypes;
                        ViewBag.InterestCalculationPeriod = productEnumAgregates.LoanInterestPeriods;
                        ViewBag.LoanDurationPeriods = productEnumAgregates.LoanDurationPeriods;
                        ViewBag.RepaymentCycles = productEnumAgregates.RepaymentCycles;
                        ViewBag.AmortizationTypes = productEnumAgregates.AmortizationTypes;
                        return PartialView(partialView, new MemberOperationPanel { LoanParameter = new LoanParameters() });
                    }
                    else if (path == "loan_detail")
                    {
                        ViewBag.PaymentModes = productEnumAgregates.PaymentModes;
                        var loan = await _loanservices.GetLoan(KEY);
                        var loanList = new List<CBS.FrontDesk.Data.Entity.LoanConf.Loan>(); // Replace Loan with the actual type of the loan object
                        loanList.Add(loan);
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(loan.LoanApplicationId);
                        var loanAmortizations = await _loanAmortizationServices.GetLoanAmortizationByLoanID(loan.Id);
                        ViewBag.Value = "0";

                        ViewBag.KEY = null;
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.CustomerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        return PartialView(partialView, new MemberOperationPanel { LoanAmortizations = loanAmortizations.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id }, SelectLoans = loanList.ToList() });
                    }
                    else if (path == "loan_schedule")
                    {
                        // Retrieve the data from the session
                        var loanScheduleData = Session["loan_schedule"];
                        List<LoanAmortization> loanSchedule = new List<LoanAmortization>();
                        // Check if the data is not null and is of the expected type
                        if (loanScheduleData != null && loanScheduleData is List<LoanAmortization>)
                        {
                            // Cast the data to the appropriate type
                            loanSchedule = (List<LoanAmortization>)loanScheduleData;

                        }


                        return PartialView(partialView, new MemberOperationPanel { LoanAmortizations = loanSchedule.ToList() });
                    }
                }
                else
                {
                    if (KEY != "null")
                    {
                        var customer = await InitializeCustomerData(KEY);
                        return PartialView(partialView, customer);
                    }

                    else
                    {
                        if (serviceOption == "branch")
                        {
                            var data = await _individualProfileServices.GetIndividualProfileByBranch();
                            return PartialView(partialView, data);
                        }
                        else if (serviceOption == "all")
                        {
                            //var data = await _individualProfileServices.GetMembers();
                            return PartialView(partialView, new MemberOperationPanel { Customers = null });
                        }


                    }
                }

            }

            return PartialView(partialView, new List<IndividualProfile>());
        }

        public async Task<ActionResult> Create()
        {
            var customer = new IndividualProfile();
            await PopulateAggregatesInViewBag();
            return View(customer);
        }
        [HttpPost]
        public async Task<ActionResult> Create(MemberOperationPanel model)
        {
            if (model.ServiceOption == "loan_schedule")
            {
                var data = await _loanAmortizationServices.GenerateLoanAmortizationSchedule(model.LoanParameter);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.ServiceOption == "loan_commitee_validation_history")
            {
                var data = await _loanCommiteeValidationHistoryServices.Create(model.LoanCommiteeValidationHistory);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.ServiceOption == "pending_loan_disbursement")
            {
                var data = await _loanservices.ApprovePendingDisbursement(model.AddLoanDisbumentCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.ServiceOption == "document")
            {

                var data = await _attachedDocumentServices.UploadFiles(model.DocumentAttachedToLoan);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.ServiceOption == "guarantor")
            {
                if (model.Path == "update")
                {
                    var data = await _loanGuarantorServices.Update(model.LoanGuarantor);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                else
                {
                    var data = await _loanGuarantorServices.Create(model.LoanGuarantor);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }

            }
            else if (model.ServiceOption == "collateral")
            {
                if (model.Path == "update")
                {
                    var data = await _loanApplicationCollateralServices.Update(model.LoanCollatera);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }
                else
                {
                    var data = await _loanApplicationCollateralServices.Create(model.LoanCollatera);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

                }

            }
            else
            {

                if (model.AddLoanApplicationCommand.LoanApplicationType == "Reschedule")
                {
                    var loan = await _loanservices.GetLoan(model.AddLoanApplicationCommand.LoanId);
                    model.AddLoanApplicationCommand.AmortizationType = loan.LoanApplication.AmortizationType;
                    model.AddLoanApplicationCommand.LoanCategory = loan.LoanApplication.LoanCategory;
                    model.AddLoanApplicationCommand.InterestRate = loan.LoanApplication.InterestRate;
                    model.AddLoanApplicationCommand.EconomicActivityId = loan.LoanApplication.EconomicActivityId;
                    model.AddLoanApplicationCommand.LoanProductId = loan.LoanApplication.LoanProductId;
                    model.AddLoanApplicationCommand.RepaymentCircle = loan.LoanApplication.RepaymentCircle;
                    model.AddLoanApplicationCommand.LoanPurposeId = loan.LoanApplication.LoanPurposeId;
                    model.AddLoanApplicationCommand.LoanTarget = loan.LoanApplication.LoanTarget;
                    model.AddLoanApplicationCommand.LoanType = loan.LoanApplication.LoanType;
                    model.AddLoanApplicationCommand.LoanId = loan.Id;
                    model.AddLoanApplicationCommand.Amount = loan.Balance;
                    model.AddLoanApplicationCommand.OldLoanPayment = model.AddLoanApplicationCommand.OldLoanPayment = new OldLoanPayment
                    {
                        LoanId = loan.Id,
                        Amount = loan.Balance,
                        Capital = loan.Balance,
                        Interest = loan.AccrualInterest,
                        Penalty = loan.Penalty,
                        VAT = loan.Tax
                    };
                    //model.AddLoanApplicationCommand.Amount = model.AddLoanApplicationCommand.NewBalance + model.AddLoanApplicationCommand.NewVAT + model.AddLoanApplicationCommand.NewInterest + model.AddLoanApplicationCommand.NewPenalty;
                    var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
                   return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else if (model.AddLoanApplicationCommand.LoanApplicationType == "Refinancing")
                {
                    var loan = await _loanservices.GetLoan(model.AddLoanApplicationCommand.LoanId);
                    model.AddLoanApplicationCommand.AmortizationType = loan.LoanApplication.AmortizationType;
                    model.AddLoanApplicationCommand.LoanTarget = loan.LoanApplication.LoanTarget;
                    model.AddLoanApplicationCommand.LoanType = loan.LoanApplication.LoanType;
                    model.AddLoanApplicationCommand.OldLoanPayment=new OldLoanPayment
                    {
                        LoanId=loan.Id,
                        Amount=loan.DueAmount,
                        Capital=loan.Balance,
                        Interest=loan.AccrualInterest,
                        Penalty=loan.Penalty,
                        VAT=loan.Tax
                    };
                    var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                else
                {
                    if (!ModelState.IsValid)
                    {
                        return JsonValidationErrorResponse();

                    }
                    if (model.AddLoanApplicationCommand.LoanApplicationType=="Normal")
                    {
                        model.AddLoanApplicationCommand.LoanId="N/A";
                        model.AddLoanApplicationCommand.OldLoanPayment.LoanId="N/A";
                    }
                    if (model.AddLoanApplicationCommand.LoanApplicationType == "Restructure")
                    {
                        model.AddLoanApplicationCommand.Amount = model.AddLoanApplicationCommand.OldLoanPayment.Capital + model.AddLoanApplicationCommand.OldLoanPayment.Interest + model.AddLoanApplicationCommand.OldLoanPayment.Penalty + model.AddLoanApplicationCommand.OldLoanPayment.VAT;

                    }
                    // Proceed with processing the valid command
                    var data = await _loanApplicationServices.Create(model.AddLoanApplicationCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }



            }
        }

  

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {

            var results = await _individualProfileServices.GetCustomerLight(KEY);
            return results;
        }

        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            //if (agrAggregates == null)
            //{
            //    agrAggregates = await _individualProfileServices.GetAggregates();
            //}
            var loanpurpose = await _loanPurposeServices.GetAllLoanPurpose();
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            var loanTerms = await _loanProductServices.GetProductTermOrDurationFromConfiguredProduct();
            var categories = await _loanProductServices.GetProductCategoryFromConfiguredProduct();
            ViewBag.LoanTypes = productEnumAgregates.LoanTypes;

            ViewBag.EconomicActivities = await _economicActivityServices.GetEconomicActivities();
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



        [HttpPost]
        public async Task<ActionResult> Update(MemberOperationPanel model)
        {

            var data = await _loanApplicationServices.ValidaLoanApplication(model.UpdateLoanApplicationStatus);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        [HttpPost]
        public async Task<ActionResult> GenerateOTP(MemberOperationPanel model)
        {

            var data = await _loanApplicationServices.GenerateOTP(model.AddOTPNotificationCommand);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        public async Task<ActionResult> Delete(string KEY, string path)
        {
            if (path=="attached_document")
            {
                var data = await _attachedDocumentServices.Delete(KEY);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
            }
            var data1 = await _individualProfileServices.Delete(KEY);
            return Json(new { success = data1.Result, status = data1.MessageStatus, message = Messaging.MessageResult(data1) }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> Ajaxloader(string Key, string path, string loanTermId, string loanCategoryid, string loanCategoryValue)
        {
            bool isSSF = false;
            if (loanCategoryValue == "SpecialSavingFacilityLoan")
            {
                isSSF = true;
            }
            if (Key != null)
            {
                if (path == "loanrepayment_cycles")
                {
                    var listing = await _loanProductServices.GetLoanProductRepayments(Key, path);
                    return Json(listing, JsonRequestBehavior.AllowGet);
                }
                else if (path == "load_loan_products")
                {
                    var listing = await _loanProductServices.GetLoanProductsDropDown(Key, loanTermId, loanCategoryid, isSSF);
                    return Json(listing, JsonRequestBehavior.AllowGet);

                }
                else if (path == "get_configurated_target")
                {
                    var listing = await _loanProductServices.GetTargetsConfiguredForProductByTermOrDuration(Key, loanCategoryid, isSSF);
                    return Json(listing, JsonRequestBehavior.AllowGet);

                }

                else if (path == "get_puposes")
                {
                    var listing = await _loanPurposeServices.GetAllLoanPurpose(Key);
                    return Json(listing, JsonRequestBehavior.AllowGet);

                }
                // 
                else
                {
                    var loans = await _loanservices.GetAllMembersCurrents(Key);
                    var listing = _loanservices.GetAllMembersCurrentsDropdown(loans).ToList();
                    return Json(listing, JsonRequestBehavior.AllowGet);

                }
                //
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetLoanProduct(string Key)
        {
            var data = await _loanProductServices.GetLoanProduct(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetLoan(string Key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid loan key.");
                }

                var data = await _loanservices.GetLoan(Key);
                if (data == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while fetching loan details.");
            }
        }

        public async Task<ActionResult> GetLoanForRefinancing(string Key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Key))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid loan key.");
                }

                var data = await _loanservices.GetLoan(Key);

                if (data == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var newData = _loanservices.MapLoan(data);
                return Json(newData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred while fetching loan details.");
            }
        }
        public async Task<ActionResult> GetObject(string Key)
        {
            var data = await _loanProductServices.GetLoanProduct(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}