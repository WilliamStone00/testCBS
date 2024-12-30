using CBS.BusinessService;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.CMoney
{
    [CheckSessionTimeOutAttribute]

    public class CMoneyMembershipController : BaseController
    {
        // GET: CMoneyMembership
        private readonly IndividualProfileServices _services;
        private readonly CMoneyMemberServices _cMoneyMemberServices;
        private readonly BranchServices _branchServices;

        public CMoneyMembershipController(IndividualProfileServices services, CMoneyMemberServices cMoneyMemberServices, BranchServices branchServices)
        {
            _services = services;
            _cMoneyMemberServices=cMoneyMemberServices;
            _branchServices=branchServices;
        }

       
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Activation()
        {
            return View();
        }
        public async Task<ActionResult> ManageProfile(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var cMoneyMembersActivation = await _cMoneyMemberServices.GetCMoneyMembersActivationAccount(KEY);
            var customerProfile = await _services.GetCustomerLight(cMoneyMembersActivation.CustomerId);
            cMoneyMembersActivation.Customer=customerProfile.CustomerList;
            cMoneyMembersActivation.ReactivateCMoneyMemberCommand=new ReactivateCMoneyMemberCommand { CustomerId=cMoneyMembersActivation.CustomerId };
            cMoneyMembersActivation.ResetCMoneyMemberPinCommand=new ResetCMoneyMemberPinCommand { CustomerId=cMoneyMembersActivation.CustomerId };
            cMoneyMembersActivation.ResetPinWithSecurityCommand=new ResetPinWithSecurityCommand {  LoginId=cMoneyMembersActivation.LoginId, SecretAnswer=cMoneyMembersActivation.SecretAnswer, SecretQuestion=cMoneyMembersActivation.SecretQuestion };
            cMoneyMembersActivation.UpdateCMoneyMemberActivationCommand=new UpdateCMoneyMemberActivationCommand {  Id=cMoneyMembersActivation.Id, IsActive=cMoneyMembersActivation.IsActive, IsSubcribed=cMoneyMembersActivation.IsSubcribed, PhoneNumber=cMoneyMembersActivation.PhoneNumber };
            cMoneyMembersActivation.ManageSecretQuestionCommand=new ManageSecretQuestionCommand {  LoginId=cMoneyMembersActivation.LoginId, SecretQuestion=cMoneyMembersActivation.SecretQuestion, SecretAnswer=cMoneyMembersActivation.SecretAnswer };
            cMoneyMembersActivation.GenerateMemberActivationOTPCommand=new GenerateMemberActivationOTPCommand { CustomerId=cMoneyMembersActivation.CustomerId, PhoneNumber= cMoneyMembersActivation .PhoneNumber};
            cMoneyMembersActivation.ChangePhoneNumberCommand=new ChangePhoneNumberCommand { CustomerId=cMoneyMembersActivation.CustomerId, OldPhoneNumber= cMoneyMembersActivation.PhoneNumber, Language=cMoneyMembersActivation.Language, NewPhoneNumber=null, OTP=null, Reason=null };

            return View(cMoneyMembersActivation);
        }
        //CMoneyMemberProfile
        [HttpPost]
        public async Task<ActionResult> Create(CMoneyMembersActivationAccount model)
        {
            if (ModelState.IsValid)
            {
                if (model.Option=="upload_document")
                {
                    model.CustomerDocumentRequest.CustomerID = model.CustomerDocumentRequest.CustomerID;
                    model.CustomerDocumentRequest.ServiceTypeType = "ClientManagement";
                    var data = await _services.UploadFiles(model.CustomerDocumentRequest);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data),imagepath=data.SessionID });
                }
                else
                {
                    var branch = await _branchServices.GetBranch(model.AddCMoneyMemberActivationCommand.BranchId);
                    model.AddCMoneyMemberActivationCommand.BranchCode=branch.BranchCode;
                    var data = await _cMoneyMemberServices.Create(model.AddCMoneyMemberActivationCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), activationid = data.SessionID });

                }

            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }

        

        [HttpPost]
        public async Task<ActionResult> UploadImage(CMoneyMembersActivationAccount model)
        {
            if (model.CustomerDocumentRequest == null || string.IsNullOrEmpty(model.CustomerDocumentRequest.CustomerID))
            {
                return Json(new { success = false, status = false, message = "CustomerDocumentRequest is invalid or missing." });
            }

            model.CustomerDocumentRequest.ServiceTypeType = "ClientManagement";
            var data = await _services.UploadFiles(model.CustomerDocumentRequest);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), imagepath = data.SessionID });
        }


        [HttpPost]
        public async Task<ActionResult> GenerateOTP(CMoneyMembersActivationAccount model)
        {
            if (ModelState.IsValid)
            {
                var data = await _cMoneyMemberServices.GenerateOTP(model.GenerateMemberActivationOTPCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data),phonenumber= data.SessionID });
            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }
        [HttpPost]
        public async Task<ActionResult> Update(CMoneyMembersActivationAccount model)
        {
            if (ModelState.IsValid)
            {
                var data = await _cMoneyMemberServices.Update(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            if (path=="activation")
            {
                ViewBag.Languages=_services.GetLanguages();

                var data = await _services.GetCustomerLight(KEY);
                if (data!=null)
                {

                    var cMoneyMembersActivation = await _cMoneyMemberServices.GetCMoneyMembersActivationAccount(data.CustomerList.MobileLoginId);
                    if (cMoneyMembersActivation!=null)
                    {
                        data.CustomerList.IsUseOnLineMobileBanking=true;
                    }
                    else
                    {
                        data.CustomerList.IsUseOnLineMobileBanking=false;
                    }
                    return PartialView(partialView, new CMoneyMembersActivationAccount { Customer = data.CustomerList, AddCMoneyMemberActivationCommand=new AddCMoneyMemberActivationCommand { CustomerId=KEY, OTP=null, LoginId="Like: 0035129", BranchId=data.CustomerList.BranchId, PhoneNumber=data.CustomerList.Phone, BranchCode="N/A" }, GenerateMemberActivationOTPCommand=new GenerateMemberActivationOTPCommand { CustomerId=data.CustomerList.CustomerId, PhoneNumber=data.CustomerList.Phone }, CustomerDocumentRequest=new Data.Entity.CustomerManagement.CustomerDocumentRequest { CustomerID=data.CustomerList.CustomerId, DocumentType="CustomerPhoto", ServiceTypeType="ClientManagement" } });

                }
                else
                {
                    ViewBag.message = $"{KEY} was not found in the database.";

                    return PartialView("_DataNotFound", new CMoneyMembersActivationAccount());
                }
            }
            else if (path=="reload")
            {
                var data = await _cMoneyMemberServices.GetCMoneyMembersActivationAccount(KEY);
                return PartialView(partialView, new CMoneyMembersActivationAccount { Customer = data.Customer, AddCMoneyMemberActivationCommand=new AddCMoneyMemberActivationCommand { CustomerId=data.CustomerId, OTP=serviceOption, LoginId=data.LoginId } });

            }
            
            else
            {
                var branches = await _branchServices.GetBranches();
                ViewBag.Branches = branches;
                var activationAccount = await _cMoneyMemberServices.GetCMoneyMembersActivationAccount(KEY);
                activationAccount.ResetCMoneyMemberPinCommand=new ResetCMoneyMemberPinCommand { CustomerId=activationAccount.CustomerId };
                activationAccount.ResetPinWithSecurityCommand=new ResetPinWithSecurityCommand { LoginId=activationAccount.LoginId };
                activationAccount.UpdateCMoneyMemberActivationCommand=new UpdateCMoneyMemberActivationCommand { Id=activationAccount.Id, IsActive=activationAccount.IsActive, IsSubcribed=activationAccount.IsSubcribed, PhoneNumber=activationAccount.PhoneNumber };
                activationAccount.DeactivateCMoneyMemberCommand=new DeactivateCMoneyMemberCommand { CustomerId=activationAccount.CustomerId };
                activationAccount.ReactivateCMoneyMemberCommand=new ReactivateCMoneyMemberCommand { CustomerId=activationAccount.CustomerId };
                return PartialView(partialView, activationAccount);
            }
        }
        public async Task<ActionResult> GetActivatatedMember(string Key)
        {
            var data = await _cMoneyMemberServices.GetCMoneyMembersActivationAccount(Key);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await _cMoneyMemberServices.GetDataTable(GetDataTableOptions(), searchCriteria);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadDataSearch(string searchCriterial = "All")
        {
            try
            {
                var dataTable = await _cMoneyMemberServices.GetDataTable(GetDataTableOptions(), searchCriterial);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}