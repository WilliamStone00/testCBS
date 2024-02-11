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
using CBS.FrontDesk.UI.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Loan.Config;
using MvcSiteMapProvider.Reflection;

namespace CBS.FrontDesk.UI.Controllers.CustomerManagement
{
    //[SessionTimeoutFilterAttribute]
    public class IndividualController : BaseController
    {
        // GET: Individual
        private readonly IndividualProfileServices _individualProfileServices;

        public IndividualController(IndividualProfileServices individualProfileServices)
        {
            _individualProfileServices = individualProfileServices;
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
        public async Task<ActionResult> Account(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null)
        {
            ViewBag.KEY = KEY;
            if (KEY != "null")
            {
                var customer = await InitializeCustomerData(KEY);
                return PartialView(partialView, customer);
            }
            else
            {
                var data = await _individualProfileServices.GetIndividualProfile();

                return PartialView(partialView, data);
            }
            return PartialView(KEY, "");
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
            var data = await _individualProfileServices.Create(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {
            var agrAggregates = await _individualProfileServices.GetAggregates();
            await PopulateAggregatesInViewBag(agrAggregates);
            return await _individualProfileServices.GetCustomer(KEY, agrAggregates);
        }

        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            await GetList();
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
        
            else if (model.option == "upload")
            {///NextofkingsPhoto,NextofkingsSignature,CustomerPhoto,CustomerSignature,CustomerOtherDocument
                model.CustomerDocumentRequest.CustomerID = model.CustomerList.customerId;
                model.CustomerDocumentRequest.ServiceTypeType = "ClientManagement";
                var data = await _individualProfileServices.UploadFiles(model.CustomerDocumentRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "BankInfo")
            {
                var data = await _individualProfileServices.UpdateBankInfo(model);
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



            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _individualProfileServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> GetList()
        {
            ViewBag.Genders = _individualProfileServices.GetGender();
            return true;
        }
    }
}