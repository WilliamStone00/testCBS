using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.BusinessService.Accounting;

using MvcSiteMapProvider.Reflection;
using System.Data.Entity.Core.Metadata.Edm;
using CBS.BusinessService.Config;
using System.Linq.Expressions;
using CBS.BusinessService.MembersAccountSettings;

namespace CBS.FrontDesk.UI.Controllers.CustomerManagement
{
    //[SessionTimeoutFilterAttribute]
    public class IndividualController : BaseController
    {
        // GET: Individual
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly MemberAccountActivationServices _memberAccountActivationServices;

        public IndividualController(IndividualProfileServices individualProfileServices, MemberAccountActivationServices memberAccountActivationServices = null)
        {
            _individualProfileServices = individualProfileServices;
            _memberAccountActivationServices = memberAccountActivationServices;
        }
        public async Task<ActionResult> List()
        {

            //var data = await _individualProfileServices.GetIndividualProfile();
            return View();


        }

        public async Task<ActionResult> CustomerProfile(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }
        public async Task<ActionResult> MyMembers()
        {
         
            return View();
        }
        public async Task<ActionResult> Account(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null,string serviceOption = null, string path = null)
        {
            ViewBag.KEY = KEY;
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
                    var data = await _individualProfileServices.GetMembers();
                    return PartialView(partialView, data);
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
        public async Task<ActionResult> Create(IndividualProfile model)
        {
            if (ModelState.IsValid)
            {
                var data = await _individualProfileServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Where(e => e.ErrorMessage != null)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Convert the list of error messages to a single string with each message on a new line
                string errorMessage = string.Join("\n", errorMessages);

                // Pass the error message as the message
                return Json(new { success = false, status = false, message = errorMessage });
            }
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {
            var agrAggregates = await _individualProfileServices.GetAggregates();
            await PopulateAggregatesInViewBag(agrAggregates);
            var results= await _individualProfileServices.GetCustomer(KEY, agrAggregates);
            ViewBag.MemberAccounts= _individualProfileServices.MembersAccounts(results.CustomerAccounts.ToList());
            return results;
        }

        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
            ViewBag.Banks = agrAggregates.Banks;
            ViewBag.Branches = agrAggregates.Branches;
            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.Countries = agrAggregates.Countries;
            ViewBag.Regions = agrAggregates.Regions;
            ViewBag.Divisions = agrAggregates.Divisions;
            ViewBag.Subdivisions = agrAggregates.Subdivisions;
            ViewBag.Towns = agrAggregates.Towns;
            ViewBag.Savings = agrAggregates.Savings;
            ViewBag.Organizations = agrAggregates.Organizations;
            ViewBag.bankingRelationships = agrAggregates.CustomerDefaultEnum.bankingRelationships;
            ViewBag.genders = agrAggregates.CustomerDefaultEnum.genders;
            ViewBag.membershipApprovalStatuses = agrAggregates.CustomerDefaultEnum.membershipApprovalStatuses;
            ViewBag.activeStatuses = agrAggregates.CustomerDefaultEnum.activeStatuses;
            ViewBag.workingStatuses = agrAggregates.CustomerDefaultEnum.workingStatuses;
            ViewBag.legalForms = agrAggregates.CustomerDefaultEnum.legalForms;
            ViewBag.formalOrInformalSectors = agrAggregates.CustomerDefaultEnum.formalOrInformalSectors;
            ViewBag.maritalStatuses = agrAggregates.CustomerDefaultEnum.maritalStatuses;
            ViewBag.languages = _individualProfileServices.GetLanguages();
            ViewBag.Categories = agrAggregates.CustomerDefaultEnum.customerCategories;
            ViewBag.relationships = agrAggregates.CustomerDefaultEnum.relationships;
            
        }


        [HttpPost]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await _individualProfileServices.GetDataTable(GetDataTableOptions());
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadDataSearch(string Search = "All")
        {
            try
            {
                var dataTable = await _individualProfileServices.GetDataTable(GetDataTableOptions());
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //"url": "/Saving/LoadDataSearch?Search=" + search,
        [HttpPost]
        public async Task<ActionResult> Update(IndividualCustomerProfile model)
        {

            if (model.option == "Profile")
            {
                var data = await _individualProfileServices.UpdateProfile(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "Status")
            {
                var data = await _individualProfileServices.ActivateDeactivate(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "Status")
            {
                var data = await _individualProfileServices.ActivateDeactivate(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "UpdateMemberAccount")
            {
                var data = await _memberAccountActivationServices.Update(model.MemberAccountActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.option == "AddMemberAccount")
            {
               
                var data = await _memberAccountActivationServices.Create(model.MemberAccountActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }

            else if (model.option == "upload")
            {///NextofkingsPhoto,NextofkingsSignature,CustomerPhoto,CustomerSignature,CustomerOtherDocument
                model.CustomerDocumentRequest.CustomerID = model.CustomerList.customerId;
                model.CustomerDocumentRequest.ServiceTypeType = "ClientManagement";
                var data = await _individualProfileServices.UploadFiles(model.CustomerDocumentRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "bank_info")
            {
                var data = await _individualProfileServices.UpdateBankInfo(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "cardsignaturespecement")
            {
                var data = await _individualProfileServices.CreateCardSignatureSpecimenDetail(model.CardSignatureSpecimen);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "nextofking")
            {
                var data = await _individualProfileServices.CreateMembershipNextOfKingsMember(model.MembershipNextOfKingsMember);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "membershipstatus")
            {
                var data = await _individualProfileServices.UpdateMembershipstatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "legalstaus")
            {
                var data = await _individualProfileServices.UpdateLegalSector(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "maritalstatus")
            {
                var data = await _individualProfileServices.UpdateMaritalStatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                
            }
            else if (model.option == "employmentdetail")
            {
                var data = await _individualProfileServices.UpdateEmployementStatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.option == "addaccount")
            {
                var data = await _individualProfileServices.AddCustomerAccount(model.AddCustomerAccount);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "ResetPin")
            {
                var data = await _individualProfileServices.ResetPin(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            //MemberAccountActivation


            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _individualProfileServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

     
    }
}