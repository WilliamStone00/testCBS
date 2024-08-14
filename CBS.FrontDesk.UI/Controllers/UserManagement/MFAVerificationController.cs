using Antlr.Runtime;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Service;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.UserManagement
{
    public class MFAVerificationController : BaseController
    {
        //GET: MFAVerification
        //GET: TwoStepaccountverification
        private readonly IUserManagementServices _userManagementServices;
        private readonly AuthenticationServices _authenticationServices;
        string url = "/";
        public MFAVerificationController(IUserManagementServices helper, AuthenticationServices authenticationServices = null)
        {
            _userManagementServices = helper;
            _authenticationServices = authenticationServices;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Index(string serviceoption = "None", string KEY = "KEY", string secrete = "none", string usersecreteid = "secrete", string path = null, string email = "default", string fullName = "None", string returnUrl = "")
        {
            if (!VerifyCookies("CBS4U_MFA"))
            {
                var data = new MFAActivation { Code = null, Email = email, FullName = fullName, ReturnUrl = returnUrl, Id = Guid.Parse(KEY) };
                return View(data);
            }

            return Redirect("~/Authentication/Logout");
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> MFACodeVerification(MFAActivation mFAActivation)
        {

            if (!VerifyCookies("CBS4U_MFA"))
            {
                var executionMessages = await _userManagementServices.MFACodeVerification(mFAActivation);
                if (executionMessages.Result)
                {
                    //var userDto = (UserDto)executionMessages.Data;
                    RemoveSessionName("MFA");
                    if (!string.IsNullOrEmpty(mFAActivation.ReturnUrl))
                    {
                        return Json(new { success = true, url = mFAActivation.ReturnUrl });
                    }
                }
                return Json(new { success = executionMessages.Result, status = executionMessages.MessageStatus, message = Messaging.MessageResult(executionMessages) });
            }

            return Json(new { success = false, message = "Session expired.", url = "~/Authentication/Logout" });
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ActionResult> MFACodeVerification(MFAActivation mFAActivation, string returnUrl = "")
        //{



        //    if (!VerifyCookies("CBS4U_MFA"))
        //    {
        //        var data = await _userManagementServices.MFACodeVerification(mFAActivation);
        //        if (data.Result)
        //        {
        //            return RedirectToLocal(returnUrl);
        //        }
        //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

        //    }
        //    url = "~/Authentication/Logout";
        //    return Json(new { success = false, message = "Session expired.", url = url, state = "Expired" }, JsonRequestBehavior.AllowGet);



        //}
        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    return RedirectToAction("Index", "Home");
        //}
    }
}