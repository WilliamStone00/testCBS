using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.BusinessService.Accounting;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.BusinessService.Config;
using CBS.BusinessService.Application;
using CBS.BusinessService;
using CBS.BusinessService.LoanCommitee;
using System.IO;

namespace CBS.FrontDesk.UI.Controllers.MemberOperation
{
    public class MemberOperationController : BaseController
    {
        // GET: MemberOperation
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly LoanProductServices _loanProductServices;
        private readonly LoanPurposeServices _loanPurposeServices;
        private readonly LoanApplicationServices _loanApplicationServices;
        private readonly LoanServices _loanServices;
        private readonly LoanAmortizationServices _loanAmortizationServices;
        private readonly LoanCommiteeValidationHistoryServices _loanCommiteeValidationHistoryServices;
        private readonly LoanApplicationCollateralServices _loanApplicationCollateralServices;
        private readonly LoanGuarantorServices _loanGuarantorServices;
        private readonly LoanProductCollateralServices _loanProductCollateralServices;
        private readonly AttachedDocumentServices _attachedDocumentServices;
        private readonly DocumentServices _documentServices;
        private readonly LoanServices _loanservices;
        public MemberOperationController(IndividualProfileServices individualProfileServices, LoanProductServices loanProductServices = null, LoanPurposeServices loanPurposeServices = null, LoanApplicationServices loanApplicationServices = null, LoanServices loanServices = null, LoanAmortizationServices loanAmortizationServices = null, LoanCommiteeValidationHistoryServices loanCommiteeValidationHistoryServices = null, LoanApplicationCollateralServices loanApplicationCollateralServices = null, LoanGuarantorServices loanGuarantorServices = null, LoanProductCollateralServices loanProductCollateralServices = null, AttachedDocumentServices services = null, DocumentServices documentServices = null, LoanServices loanservices = null)
        {
            _individualProfileServices = individualProfileServices;
            _loanProductServices = loanProductServices;
            _loanPurposeServices = loanPurposeServices;
            _loanApplicationServices = loanApplicationServices;
            _loanServices = loanServices;
            _loanAmortizationServices = loanAmortizationServices;
            _loanCommiteeValidationHistoryServices = loanCommiteeValidationHistoryServices;
            _loanApplicationCollateralServices = loanApplicationCollateralServices;
            _loanGuarantorServices = loanGuarantorServices;
            _loanProductCollateralServices = loanProductCollateralServices;
            _attachedDocumentServices = services;
            _documentServices = documentServices;
            _loanservices = loanservices;
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
            var customer = await InitializeCustomerData(KEY);
            var CustomerLoans = await _loanServices.GetLoanByCustomerID(KEY);
            return View(new MemberOperationPanel { Customer = customer.CustomerList, Loans = CustomerLoans.ToList() });
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
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
            var loanApplications = await _loanApplicationServices.GetLoanApplications();
            return View(new MemberOperationPanel { LoanApplications = loanApplications.ToList() });
        }
        public async Task<ActionResult> LoanApplicationForCommitees(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {

            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
            var loanApplications = await _loanApplicationServices.GetLoanApplications();
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
                ViewBag.KEY = KEY;
                await PopulateAggregatesInViewBag();
                var customer = await InitializeCustomerData(KEY);
                return PartialView(partialView, new MemberOperationPanel { Customer = customer.CustomerList });

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
                        var loan = await _loanServices.GetLoan(KEY);
                        var Accounts = await _individualProfileServices.GetCustomerAccounts(loan.CustomerId);
                        var documentAttachedToLoans = loan.LoanApplication.DocumentAttachedToLoans;
                        var loanApplication = loan.LoanApplication;
                        ViewBag.CustomerAccounts = _individualProfileServices.MembersAccounts(Accounts.ToList()).ToList();
                        ViewBag.DisbursmentStatuses = productEnumAgregates.DisbursmentStatuses;
                        var loand = new AddLoanDisbumentCommand
                        {
                             AccountNumber= Accounts.FirstOrDefault().accountNumber, Comment= $"Loan successfully disbursed to Customer number: [{loan.CustomerId}] and Account number: [{Accounts.FirstOrDefault().accountNumber}]. Documentation updated. For inquiries, contact Loan Manager {loan.LoanManager}.", LoanId=loan.Id, Status="Disbursed"
                        };
                        var collaterals = await _loanProductCollateralServices.GetAllLaonApplicationCollateralByApplicationIdQuery(KEY);
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        var loanCommiteeValidationHistories = loanApplication.LoanCommiteeValidations;
                        return PartialView(partialView, new MemberOperationPanel { AddLoanDisbumentCommand=loand, LoanCommiteeValidationHistory = new Data.Entity.LoanCommitee.LoanCommiteeValidationHistory { LoanApplicationId = loanApplication.Id }, LoanCommiteeValidationHistories = loanCommiteeValidationHistories.ToList(), LoanCollateras = collaterals.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "perding_disbursement")
                    {
                        var data = await _loanservices.GetPendingDisbursementLoans();
                        return PartialView(partialView, new MemberOperationPanel { Loans = data.ToList() });
                    }
                    else if (path == "application_detail")
                    {
                        ViewBag.Value = "0";
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        return PartialView(partialView, new MemberOperationPanel { DocumentAttachedToLoans= documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanCollatera = collateral, LoanGuarantor= guarantor, Customer= customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus=new UpdateLoanApplicationStatusCommand { Id=loanApplication.Id} });
                    }
                    else if (path == "upload_document")
                    {
                        ViewBag.Value = "0";
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        var attachedDoc =new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id};
                        return PartialView(partialView, new MemberOperationPanel { DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanApplication= loanApplication});
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
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        return PartialView(partialView, new MemberOperationPanel { LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
                    }
                    else if (path == "application_detail")
                    {
                        ViewBag.KEY = null;
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.Id);
                        return PartialView(partialView, new MemberOperationPanel { LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
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
                        var amortization=new LoanParameters { InterestRate= loanApplication.InterestRate,
                          AmortizationType=loanApplication.AmortizationType, Amount=loanApplication.Amount,
                         LoanDuration= loanApplication.LoanDuration, RepaymentCycle=loanApplication.LoanProduct.LoanProductRepaymentCycles.FirstOrDefault(x=>x.Id== loanApplication.RepaymentCircle).RepaymentCycle,
                        };
                        var collaterals = await _loanProductCollateralServices.GetAllLaonApplicationCollateralByApplicationIdQuery(KEY);
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        var loanCommiteeValidationHistories = loanApplication.LoanCommiteeValidations;
                        return PartialView(partialView, new MemberOperationPanel {LoanParameter=amortization, LoanCommiteeValidationHistory=new Data.Entity.LoanCommitee.LoanCommiteeValidationHistory { LoanApplicationId = loanApplication.Id }, LoanCommiteeValidationHistories = loanCommiteeValidationHistories.ToList(),LoanCollateras = collaterals.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id } });
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
                        var loan = await _loanServices.GetLoan(KEY);
                        var loanList = new List<Loan>(); // Replace Loan with the actual type of the loan object
                        loanList.Add(loan);
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(loan.LoanApplicationId);
                        var loanAmortizations = await _loanAmortizationServices.GetLoanAmortizationByLoanID(loan.Id);
                        ViewBag.Value = "0";

                        ViewBag.KEY = null;
                        var customer = await InitializeCustomerData(loanApplication.CustomerId);
                        var guarantor = new LoanGuarantor { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        var collateral = new LoanApplicationCollateral { LoanApplicationId = loanApplication.Id, CustomerId = customer.CustomerList.customerId };
                        ViewBag.LoanProductCollaterals = await _loanProductCollateralServices.GetLoanProductCollaterals(loanApplication.LoanProduct.Id);
                        ViewBag.DocumentTypes = await _documentServices.GetDocumentDropDown();
                        var documentAttachedToLoans = loanApplication.DocumentAttachedToLoans;
                        var attachedDoc = new DocumentAttachedToLoan { LoanApplicationId = loanApplication.Id };
                        return PartialView(partialView, new MemberOperationPanel {LoanAmortizations = loanAmortizations.ToList(), DocumentAttachedToLoans = documentAttachedToLoans.ToList(), DocumentAttachedToLoan = attachedDoc, LoanCollatera = collateral, LoanGuarantor = guarantor, Customer = customer.CustomerList, AddOTPNotificationCommand = new AddOTPNotificationCommand { CustomerId = loanApplication.CustomerId, LoanApplicationId = loanApplication.Id }, LoanApplication = loanApplication, UpdateLoanApplicationStatus = new UpdateLoanApplicationStatusCommand { Id = loanApplication.Id }, SelectLoans = loanList.ToList() });
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
                     

                        return PartialView(partialView, new MemberOperationPanel { LoanAmortizations = loanSchedule.ToList()});
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
                            var data = await _individualProfileServices.GetIndividualProfile();
                            return PartialView(partialView, new MemberOperationPanel { Customers = data.ToList() });
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
            if (model.ServiceOption== "loan_schedule")
            {
                var data = await _loanAmortizationServices.GenerateLoanAmortizationSchedule(model.LoanParameter);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if(model.ServiceOption == "loan_commitee_validation_history")
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
                if (model.Path=="update")
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
                model.LoanApplication.CustomerId = model.Customer.customerId;
                var data = await _loanApplicationServices.Create(model.LoanApplication);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {
           
            var results = await _individualProfileServices.GetCustomerLight(KEY);
            return results;
        }

        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
            var loanpurpose = await _loanPurposeServices.GetAllLoanPurpose();
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
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
            ViewBag.LoanCommiteeValidationStatuses = productEnumAgregates.LoanCommiteeValidationStatuses;
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
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _individualProfileServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key!=null)
            {
                var listing = await _loanProductServices.GetLoanProductRepayments(Key, path);
                return Json(listing, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetLoanProduct(string Key)
        {
            var data = await _loanProductServices.GetLoanProduct(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetObject(string Key)
        {
            var data = await _loanProductServices.GetLoanProduct(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}