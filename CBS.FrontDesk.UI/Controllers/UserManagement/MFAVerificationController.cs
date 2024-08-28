using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Service;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    [AllowAnonymous]
    public class MFAVerificationController : BaseController
    {
        private readonly IUserManagementServices _userManagementServices;
        private readonly AuthenticationServices _authenticationServices;

        public MFAVerificationController(IUserManagementServices helper, AuthenticationServices authenticationServices = null)
        {
            _userManagementServices = helper;
            _authenticationServices = authenticationServices;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Index(string KEY = "KEY", string email = "default", string fullName = "None", string returnUrl = "")
        {
            if (VerifyIfSessionExist("MFA"))
            {
                var data = new MFAActivation { Code = null, Email = email, FullName = fullName, ReturnUrl = returnUrl, Id = Guid.Parse(KEY) };
                return View(data);
            }

            return Redirect("~/Authentication/Logout");
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken] // Anti-CSRF Token Validation
        public async Task<ActionResult> MFACodeVerification(MFAActivation mFAActivation)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (VerifyIfSessionExist("MFA"))
                    {
                        var executionMessages = await _userManagementServices.MFACodeVerification(mFAActivation);
                        if (executionMessages.Result)
                        {
                            RemoveSessionName("MFA");
                            if (!string.IsNullOrEmpty(mFAActivation.ReturnUrl))
                            {
                                return Json(new { success = true, url = mFAActivation.ReturnUrl,message= Messaging.MessageResult(executionMessages) });
                            }
                        }
                        return Json(new { success = executionMessages.Result, status = executionMessages.MessageStatus, message = Messaging.MessageResult(executionMessages) });

                    }

                    return Json(new { success = false, message = "Session expired.", url = "~/Authentication/Logout" });


                }
                return Json(new { success = false, status = false, message = "Invalid data inputed." });

            }
            catch (Exception ex)
            {
                // Log the error details for internal review
                // LogException(ex);
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }

        }
    }
}