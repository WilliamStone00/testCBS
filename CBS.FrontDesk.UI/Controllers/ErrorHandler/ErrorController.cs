using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ErrorHandler
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            // Your default error view/page
            return View("Error");
        }

        public ActionResult BadRequest()
        {
            // Handle 400 Bad Request error
            return View();
        }

        public ActionResult Unauthorized()
        {
            // Handle 401 Unauthorized error
            return View();
        }

        public ActionResult Forbidden()
        {
            // Handle 403 Forbidden error
            return View();
        }

        public ActionResult NotFound()
        {
            // Handle 404 Not Found error
            return View();
        }

        public ActionResult InternalServer()
        {
            // Handle 500 Internal Server error
            return View();
        }

        public ActionResult ServiceUnavailable()
        {
            // Handle 503 Service Unavailable error
            return View();
        }

        // Add more methods for other error pages as needed
    }



}