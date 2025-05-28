using CBS.API.Helper;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Session
{
    // Controller: SessionController.cs
    public class SessionController : Controller
    {
        private readonly LocalSession _localSession;
        private static int _cachedTimeout = 5; // Default in case DB call fails

        public SessionController()
        {
            _localSession = new LocalSession();
        }
        [HttpGet]
        public ActionResult KeepAlive()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetIdleTimeout()
        {
            const string cacheKey = "TSC_SessionTimeoutConfig";

            // 🧠 Check cache first
            if (HttpRuntime.Cache[cacheKey] is SessionTimeoutConfigDto cachedDto)
            {
                return Json(cachedDto, JsonRequestBehavior.AllowGet);
            }

            // 🧠 Default values
            int timeout = 15;
            int warning = 2;

            var idleTime = await _localSession.GetCurrentIdletimeByBranch();

            if (idleTime != null && idleTime.TotalMinutes > 0)
            {
                timeout = (int)idleTime.TotalMinutes;
            }

            var config = new SessionTimeoutConfigDto
            {
                Timeout = timeout,
                Warning = warning
            };

            // 🧠 Cache the DTO for 10 minutes
            HttpRuntime.Cache.Insert(
                cacheKey,
                config,
                null,
                DateTime.Now.AddHours(24),
                Cache.NoSlidingExpiration
            );

            return Json(config, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Ping()
        {
            return Content("Pong");
        }

        [HttpGet]
        public ActionResult ExtendSessionTimeout()
        {
            Session.Timeout = _cachedTimeout;
            return Json(new { success = true, timeout = Session.Timeout }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RegisterIdleEvent()
        {
            // Register idle timeout event to DB (for tracking or analytics)
            return new HttpStatusCodeResult(200);
        }


        public ActionResult Locked()
        {
            Session["SessionUnlocked"] = false; // Ensure it's locked
            ViewBag.Username = Session["UserName"]?.ToString();
            return View(new SessionAuth { UserName = ViewBag.Username, SessionRecoveryCode = string.Empty });
        }

        [HttpPost]
        public JsonResult ValidateRecoveryCode(string recoveryCode)
        {
            string userId = Session["UserID"]?.ToString();
            string username = Session["Username"]?.ToString();
            string sessionCode = Session["SessionCode"]?.ToString();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(recoveryCode) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(sessionCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid recovery session parameters. Logging out...",
                    redirect = true // 👈 this flag tells the frontend to redirect
                });
            }

            var sessionResult = _localSession.GetCurrentUserSession(sessionCode, username);

            if (sessionResult == null)
            {
               
                return Json(new
                {
                    success = false,
                    message = "Session expired. Logging out...",
                    redirect = true // 👈 same for expired
                });
            }

            Session["SessionUnlocked"] = true;
            return Json(new { success = true });
        }

    }
}