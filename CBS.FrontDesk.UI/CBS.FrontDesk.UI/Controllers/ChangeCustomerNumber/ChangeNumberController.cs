
using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.ChangeNumber;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.ChangeNumber;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.ChangeCustomerNumber
{
    [CheckSessionTimeOutAttribute]

    public class ChangeNumberController : BaseController
    {
        // GET: Vault
        private readonly ChangeNumberServices _services;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly CMoneyMemberServices _cMoneyMemberServices;

        public ChangeNumberController(ChangeNumberServices services, BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null, CMoneyMemberServices cMoneyMemberServices = null)
        {
            _services = services;
            _branchServices = branchServices;
            _individualProfileServices=individualProfileServices;
            _cMoneyMemberServices=cMoneyMemberServices;
        }

        public ActionResult Index()
        {
            
            return View(new ChangeNumberCarrier());
        }
        public async Task<ActionResult> PendingRequests()
        {
            var data = await _services.GetChangePhonumberHistory("Pending");
            return View(new ChangeNumberCarrier { PhoneNumberChangeHistories=data.ToList() });
        }
        //
        public ActionResult Initilization()
        {
            return View(new ChangeNumberCarrier());
        }
        [HttpPost]
        public async Task<ActionResult> Create(ChangeNumberCarrier model)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Concatenate all validation errors into a single string
                string errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                // Return the validation error as a single string message
                return Json(new { success = false, message = "Validation failed: " + errorMessages });
            }

            // If the Id is null, it's a new number change request, so call the Create service
            if (model.Option != "approved")
            {
                var data = await _services.Create(model.ChangePhoneNumberRequestCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                // If it's an approval update, call the Update method
                return await Update(model.ApprovePhoneNumberRequestCommand);
            }
        }


        [HttpPost]
        public async Task<ActionResult> SendOTP(string phoneNumber,string customerId)
        {
            if (ModelState.IsValid)
            {
                var generateMemberActivationOTP = new GenerateMemberActivationOTPCommand { CustomerId=customerId, PhoneNumber =phoneNumber };
                var data = await _cMoneyMemberServices.GenerateOTP(generateMemberActivationOTP);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), phonenumber = data.SessionID });
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
        public async Task<ActionResult> Update(ApprovePhoneNumberRequestCommand model)
        {
            var data = await _services.Update(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }
       
        private async Task GetValues()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
        {
            if (path == "pending")
            {
                var data = await _services.GetChangePhonumberHistory(path);
                return PartialView(partialView, new ChangeNumberCarrier { PhoneNumberChangeHistories=data.ToList()});
            }

            else if (path == "new")
            {
                return PartialView(partialView, new ChangeNumberCarrier());
            }
            else
            {
           
                var phoneNumberChange = await _services.PhoneNumberChangeHistory(KEY);
                
                return PartialView(partialView, new ChangeNumberCarrier { PhoneNumberChangeHistory=phoneNumberChange, ApprovePhoneNumberRequestCommand=new ApprovePhoneNumberRequestCommand { PhoneNumberChangeHistoryId=phoneNumberChange.Id, Approved=true } });

            }
        }
        [HttpGet]
        public async Task<ActionResult> SearchMember(string memberReference)
        {
            var member = await _individualProfileServices.GetCustomerLight(memberReference);

            if (member == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var model = new ChangePhoneNumberRequestCommand
            {
                CustomerId = member.CustomerList.CustomerId,
                OldPhoneNumber = member.CustomerList.Phone
            };

            return PartialView("_RequestPhoneChange", new ChangeNumberCarrier { Customer=member.CustomerList, ChangePhoneNumberRequestCommand=model});
        }

        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _services.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> LoadApproveView(string id)
        {
            var data = await _services.PhoneNumberChangeHistory(id);

            if (data == null)
            {
                // If no record is found, return a NotFound partial view or an error message
                return PartialView("_NotFound");
            }

            var model = new ChangeNumberCarrier
            {
                ApprovePhoneNumberRequestCommand = new ApprovePhoneNumberRequestCommand
                {
                    Approved = false, // Default approval state
                    PhoneNumberChangeHistoryId = data.Id,
                    ApprovalComment = null
                },
                PhoneNumberChangeHistory = data
            };

            return PartialView("_ApprovePhoneChange", model);
        }


        public async Task<ActionResult> GetVaultPartialView(string Key)
        {
            var data = await _services.PhoneNumberChangeHistory(Key);
            if (data == null)
            {
                return HttpNotFound();
            }

            return PartialView("_ChangeNumberDetailsPartial", new ChangeNumberCarrier { PhoneNumberChangeHistory=data, ApprovePhoneNumberRequestCommand=new ApprovePhoneNumberRequestCommand { PhoneNumberChangeHistoryId=data .Id, Approved=true} });
        }
        public async Task<ActionResult> GetBranch(string Key)
        {
            var branch = await _branchServices.GetBranch(Key);
            return Json(branch, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Ajaxloader(string Key, string path)
        {
            if (Key != null)
            {
                var listing = await _branchServices.GetBranchesByBankId(Key);
                return Json(listing, JsonRequestBehavior.AllowGet);

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }

}