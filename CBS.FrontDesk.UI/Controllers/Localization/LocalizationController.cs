using CBS.BusinessService.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Language
{
    public class LocalizationController : BaseController
    {
        private readonly IUserManagementServices _userManagementServices;

        public LocalizationController(
            IUserManagementServices userManagementServices)
        {
          
            _userManagementServices = userManagementServices;
        }

        public async Task<ActionResult> SetLanguage(string lang)
        {
            if (!string.IsNullOrWhiteSpace(lang))
            {
                // Set session for UI logic
                Session["SelectedLanguage"] = lang;

                // Set cookie for request culture
                var cookie = new HttpCookie("TSC_Lang", lang)
                {
                    Expires = DateTime.Now.AddYears(1)
                };
                Response.Cookies.Add(cookie);

                // Update user's preferred language if logged in
                var userDto = GetUserDto();
                if (userDto != null && Guid.TryParse(userDto.id, out Guid userId))
                {
                    var user = await _userManagementServices.GetUser(userId);
                    if (user != null)
                    {
                        user.UserPreferedLanguage = lang;
                        await _userManagementServices.UpdateUserProfile(user);
                        userDto.UserPreferedLanguage = lang;
                        Session["AuthUser"] = userDto;
                    }
                }
            }

            string returnUrl = Request.UrlReferrer?.ToString() ?? "/";
            return Redirect(returnUrl);
        }
    }

}