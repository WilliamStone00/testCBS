using System;



using System.Web.Mvc;
using System.Web.Security;
using System.Web;
using CBS.FrontDesk.Service;
using CBS.API.Helper;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.Entity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using CBS.BusinessService.Session;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using System.Runtime.Caching;

namespace CBS.FrontDesk.UI.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationServices _helper;
        private readonly LocalSession _localSession;
        private static readonly MemoryCache _geoCache = MemoryCache.Default;

        public AuthenticationController(IAuthenticationServices helper, LocalSession localSession)
        {
            _helper = helper;
            _localSession=localSession;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        private async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse)
        {
            var secretKey = "6Lev7iYrAAAAAJGDWuMIHIoV3x43EcYMLGXVg2-M";
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}", null);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                return result.success == true;
            }
        }

        private const int MaxFailedAttempts = 5;
        private const string FailedAttemptsKey = "FailedLoginAttempts";

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(AuthRequest model, string returnUrl = "")
        {
            var result = new ExecutionMessages();
            var geo = await GetGeoLocation();
            model.GeoLocationResponse = geo;

            int failedAttempts = Session[FailedAttemptsKey] != null ? (int)Session[FailedAttemptsKey] : 0;

            // ✅ 1. If too many failures, trigger CAPTCHA
            if (failedAttempts >= 3)
            {
                var recaptchaResponse = Request["g-recaptcha-response"];
                var isCaptchaValid = await VerifyRecaptchaAsync(recaptchaResponse);

                if (!isCaptchaValid)
                {
                    ViewBag.Success = false;
                    ViewBag.Message = "🚧 Please complete CAPTCHA verification.";
                    return View(model);
                }
            }

            // ✅ 2. Validate credentials
            if (ModelState.IsValid)
            {
                result = await _helper.AuthenticateUser(model);

                if (result.Data is UserDto userDto)
                {
                    try
                    {
                        // 🛡️ Reset failed attempts
                        Session[FailedAttemptsKey] = 0;
                        TempData["ResetClientAttempts"] = true;

                        CreateToken(userDto, "TSC", userDto.expirationTime);

                        if (userDto.ChangePasswordOnFirstLogin)
                        {
                            Session["PWD"] = "PWD";
                            var url = Url.Action("FLoginChangePassword", "UserManagement", new
                            {
                                serviceoption = "USER",
                                KEY = userDto.id,
                                secrete = userDto.refreshToken,
                                usersecreteid = Guid.NewGuid(),
                                path = $"{userDto.firstName}_{userDto.lastName}",
                                userName = userDto.userName
                            });
                            Session["CHPWDUrl"] = url;
                            return RedirectToLocal(url);
                        }

                        if (userDto.isMFA)
                        {
                            Session["MFA"] = "MFA";
                            var url = Url.Action("Index", "MFAVerification", new
                            {
                                serviceoption = "MFA",
                                KEY = userDto.id,
                                secrete = userDto.refreshToken,
                                usersecreteid = Guid.NewGuid(),
                                email = userDto.email,
                                fullname = userDto.firstName,
                                returnUrl
                            });
                            Session["MFAUrl"] = url;
                            return Redirect(url);
                        }

                        ViewBag.Success = true;
                        ViewBag.StartSessionWarning = true;
                        ViewBag.Message = Messaging.MessageResult(result);
                        return RedirectToLocal(returnUrl);
                    }
                    catch (Exception ex)
                    {
                        LogFailedAttempt(model.UserName); // Optional: Log failed login to DB
                        IncrementLoginFailures();
                        ViewBag.Success = false;
                        ViewBag.Message = "⚠️ Login failed due to internal error. Please try again.";
                        return View("Login", model);
                    }
                }
            }

            // ❌ Final failure fallback
            LogFailedAttempt(model.UserName);
            IncrementLoginFailures();
            ViewBag.Success = false;
            ViewBag.Message = result.MessageString ?? "❌ Invalid login. Please check your credentials.";
            return View("Login", model);
        }

        // 🧠 Utility methods

        private void IncrementLoginFailures()
        {
            Session[FailedAttemptsKey] = ((int?)Session[FailedAttemptsKey] ?? 0) + 1;
        }

        private void LogFailedAttempt(string username)
        {
            // TODO: Log to DB or log file: IP, user, timestamp
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResolveMultipleSessions(SessionAuth model)
        {
            if (!ModelState.IsValid)
            {
                // Send back to the GET action with preserved parameters
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = "❌ Please provide a valid session recovery code.",
                    count = model.Counts
                });
            }

            try
            {
                var result = await _localSession.InvalidateAllActivetUsers(model);

                if (result.Result)
                {
                    TempData["Success"] = true;
                    TempData["Message"] = "✅ All other sessions have been terminated. Please login again.";
                    return RedirectToAction("Login", "Authentication");
                }

                // Redirect back with error message
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = result.MessageString ?? "⚠️ Failed to terminate other sessions. Please try again.",
                    count = model.Counts
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = "🚨 An error occurred. Please try again later or contact support.",
                    count = model.Counts
                });
            }
        }




        [AllowAnonymous]
        public ActionResult ResolveMultipleSessions(string username, string message, int count)
        {
            ViewBag.Username = username;
            ViewBag.Message = message;
            ViewBag.SessionCount = count;
            var sessionAuth = new SessionAuth { Counts=count, UserName=username, SessionRecoveryCode=string.Empty };
            return View(sessionAuth);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            // Redirect to a local URL if it's valid
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }


        [AllowAnonymous]
        public async Task<GeoLocationResponse> GetGeoLocation()
        {
            string ip = null;

            try
            {
                // Step 1: Get the IP only (no geo info) for caching key
                using (var tempClient = new HttpClient())
                {
                    var tempResponse = await tempClient.GetStringAsync("https://ipinfo.io/ip");
                    ip = tempResponse?.Trim();
                }

                if (string.IsNullOrWhiteSpace(ip))
                    ip = "unknown";

                // Step 2: Check cache first
                if (_geoCache.Contains(ip))
                {
                    System.Diagnostics.Debug.WriteLine($"🟢 Using cached geo for IP: {ip}");
                    return (GeoLocationResponse)_geoCache.Get(ip);
                }

                // Step 3: Fetch full geo data
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync("https://ipinfo.io/geo");

                    if ((int)response.StatusCode == 429)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Rate limit exceeded for ipinfo.io at IP: {ip}");
                        return new GeoLocationResponse { City = "Unknown", Country = "Unknown", Ip = ip };
                    }

                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var geo = JsonConvert.DeserializeObject<GeoLocationResponse>(content);

                    // Ensure IP is set
                    geo.Ip = ip;

                    // Step 4: Cache result for 2 hours
                    _geoCache.Set(geo.Ip, geo, DateTimeOffset.Now.AddHours(2));
                    System.Diagnostics.Debug.WriteLine($"✅ Cached geo for IP: {geo.Ip}");

                    return geo;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GeoLocation error: {ex.Message}");
                return new GeoLocationResponse { City = "Unknown", Country = "Unknown", Ip = ip ?? "unknown" };
            }
        }
        //[AllowAnonymous]
        //public async Task<GeoLocationResponse> GetGeoLocation()
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //            var response = await client.GetAsync("https://ipinfo.io/geo");

        //            if ((int)response.StatusCode == 429)
        //            {
        //                // Log rate limit hit and return dummy location
        //                System.Diagnostics.Debug.WriteLine("Rate limit exceeded for ipinfo.io");
        //                return new GeoLocationResponse { City = "Unknown", Country = "Unknown" };
        //            }

        //            response.EnsureSuccessStatusCode();

        //            var content = await response.Content.ReadAsStringAsync();
        //            return JsonConvert.DeserializeObject<GeoLocationResponse>(content);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle all network errors gracefully
        //        System.Diagnostics.Debug.WriteLine($"GeoLocation error: {ex.Message}");
        //        return new GeoLocationResponse { City = "Unknown", Country = "Unknown" };
        //    }
        //}

        [AllowAnonymous]
        public async Task<ActionResult> Logout()
        {
            await _helper.Logout();
            await PerformLogoutAsync();
            // 🛡️ Tell client to reset failed attempts
            TempData["ResetClientAttempts"] = true;
            return RedirectToAction("Login");
        }

        //public async Task<ActionResult> Logout()
        //{

        //    FormsAuthentication.SignOut();
        //    // List of all cookies to clear
        //    var cookieNames = new[] { "BranchObject", "AuthUser", "CBS4U", "CBS4U_MFA", "MFA", "PWD", "ASP.NET_SessionId", "EncryptedJWToken" };
        //    await _helper.Logout();

        //    foreach (var cookieName in cookieNames)
        //    {
        //        if (Request.Cookies[cookieName] != null)
        //        {
        //            var cookie = new HttpCookie(cookieName)
        //            {
        //                Expires = DateTime.Now.AddYears(-1), // Set expiration date in the past
        //                Value = string.Empty // Clear the value
        //            };
        //            Response.Cookies.Add(cookie);
        //        }
        //    }
        //    Session.Remove("EncryptedJWToken");

        //    // Abandon the session
        //    Session.Abandon();
        //    Session.Clear();
        //    Session.RemoveAll();
        //    // Redirect to login page
        //    return RedirectToAction("Login");
        //}
    }
}
