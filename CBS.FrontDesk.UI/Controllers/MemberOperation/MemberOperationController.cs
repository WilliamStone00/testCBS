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

        public MemberOperationController(IndividualProfileServices individualProfileServices, LoanProductServices loanProductServices = null, LoanPurposeServices loanPurposeServices = null, LoanApplicationServices loanApplicationServices = null, LoanServices loanServices = null, LoanAmortizationServices loanAmortizationServices = null)
        {
            _individualProfileServices = individualProfileServices;
            _loanProductServices = loanProductServices;
            _loanPurposeServices = loanPurposeServices;
            _loanApplicationServices = loanApplicationServices;
            _loanServices = loanServices;
            _loanAmortizationServices = loanAmortizationServices;
        }
        public async Task<ActionResult> Members()
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
        public async Task<ActionResult> Loan(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }
        public async Task<ActionResult> LoanApplicationValidation(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            var productEnumAgregates = await _loanProductServices.GetLoanProductEnumAggregates();
            ViewBag.LoanApplicationStatus = productEnumAgregates.LoanStatuses;
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
                    else if (path == "application_detail")
                    {
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(KEY);
                        return PartialView(partialView, new MemberOperationPanel { LoanApplication = loanApplication });
                    }
                    else if (path == "loan_detail")
                    {
                        ViewBag.PaymentModes = productEnumAgregates.PaymentModes;
                        var loan = await _loanServices.GetLoan(KEY);
                        var loanList = new List<Loan>(); // Replace Loan with the actual type of the loan object
                        loanList.Add(loan);
                        var loanApplication = await _loanApplicationServices.GetLoanApplication(loan.LoanApplicationId);
                        var loanAmortizations = await _loanAmortizationServices.GetLoanAmortizationByLoanID(loan.Id);
                        return PartialView(partialView, new MemberOperationPanel { Loan = loan, LoanApplication = loanApplication, LoanAmortizations = loanAmortizations.ToList(), SelectLoans = loanList.ToList() });
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
            model.LoanApplication.CustomerId = model.Customer.customerId;
            var data = await _loanApplicationServices.Create(model.LoanApplication);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {
            var agrAggregates = await _individualProfileServices.GetAggregates();
            await PopulateAggregatesInViewBag(agrAggregates);
            var results = await _individualProfileServices.GetCustomer(KEY, agrAggregates);
            ViewBag.MemberAccounts = _individualProfileServices.MembersAccounts(results.CustomerAccounts.ToList());
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
            ViewBag.PaymentModes = productEnumAgregates.LoanTypes;

        }



        [HttpPost]
        public async Task<ActionResult> Update(MemberOperationPanel model)
        {

            var data = await _loanApplicationServices.ValidaLoanApplication(model.LoanApplication);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _individualProfileServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            var listing = await _loanProductServices.GetLoanProductRepayments(Key, path);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetObject(string Key)
        {
            var data = await _loanProductServices.GetLoanProduct(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}