using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ErrorHandler
{
    public class ErrorController : Controller
    {
        public ActionResult Index(string message)
        {
            ViewBag.ErrorMessage = message ?? "An error occurred. Please try again later.";
            return View();
        }

        public ActionResult BadRequest(string message)
        {
            ViewBag.ErrorMessage = message ?? "Bad Request";
            return View();
        }

        public ActionResult Unauthorized(string message)
        {
            ViewBag.ErrorMessage = message ?? "Unauthorized";
            return View();
        }

        public ActionResult Forbidden(string message)
        {
            ViewBag.ErrorMessage = message ?? "Forbidden";
            return View();
        }

        public ActionResult NotFound(string message)
        {
            ViewBag.ErrorMessage = message ?? "Page Not Found";
            return View();
        }

        public ActionResult InternalServer(string message)
        {
            ViewBag.ErrorMessage = message ?? "Internal Server Error";
            return View();
        }

        public ActionResult ServiceUnavailable(string message)
        {
            ViewBag.ErrorMessage = message ?? "Service Unavailable";
            return View();
        }
    }



}