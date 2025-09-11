using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.DataContext;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.UI.DataSet;
using CBS.FrontDesk.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Filters
{
    public class UserAuditFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                string actionName = filterContext.ActionDescriptor.ActionName;
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var request = filterContext.HttpContext.Request;

                FrontEndAuditTB objaudit = new FrontEndAuditTB();

                if (HttpContext.Current.Session["UserName"] == null)
                {
                    objaudit.UserName = "N/A";
                    objaudit.LoginStatus = "U"; // Unauthorized access attempt
                }
                else
                {
                    objaudit.UserName = HttpContext.Current.Session["FullName"].ToString();
                    objaudit.LoginStatus = "A"; // Active user session
                }

                objaudit.UsersAuditID = Guid.NewGuid().ToString(); // Generate a unique ID for the audit

                // Deserialize Roles from Session
                if (HttpContext.Current.Session["Roles"] is string[] rolesArray)
                {
                    objaudit.UserRole = string.Join(",", rolesArray); // Join roles with comma separator
                }
                else
                {
                    objaudit.UserRole = "N/A";
                }

                objaudit.BranchName = HttpContext.Current.Session["BranchName"]?.ToString() ?? "N/A";
                objaudit.SessionID = HttpContext.Current.Session.SessionID;
                objaudit.IPAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? request.UserHostAddress;
                objaudit.PageAccessed = request.RawUrl;
                objaudit.LoggedInAt = DateTime.Now;
                objaudit.LoggedOutAt = DateTime.MaxValue;
                objaudit.ControllerName = controllerName;
                objaudit.ActionName = actionName;
                objaudit.RequestMethod = request.HttpMethod; // Capture request method
                objaudit.UserAgent = request.UserAgent; // Capture user agent
                objaudit.Referer = request.UrlReferrer?.ToString() ?? "N/A"; // Capture referer
                if (request.AppRelativeCurrentExecutionFilePath.Contains("UserManagement/FLoginChangePassword") && request.HttpMethod=="GET" || request.AppRelativeCurrentExecutionFilePath.Contains("~/Authentication/Login"))
                {
                    //~/Authentication/Login
                }
                else
                {
                    objaudit.SerializedRequestData = SerializeRequestData(request);

                }
                // Capture AJAX post data
                if (request.Headers["X-Requested-With"] == "XMLHttpRequest" && request.ContentType.Contains("application/json"))
                {
                    using (var reader = new StreamReader(request.InputStream))
                    {
                        reader.BaseStream.Position = 0; // Reset position
                        string jsonData = reader.ReadToEnd();
                        objaudit.AjaxPostData = jsonData;
                    }
                }

                if (actionName == "Logout")
                {
                    objaudit.LoggedOutAt = DateTime.Now;
                    objaudit.LoginStatus = "L"; // User logged out
                }

                // Run the database save operation as a background task
                Task.Run(() => SaveAuditRecord(objaudit));

                // Finishes executing the Action as normal 
                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                // Log exception to the database
                Task.Run(() => LogExceptionToDatabase(ex));

                // Serialize the exception and add it to the response
                filterContext.Result = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(ex),
                    ContentType = "application/json"
                };
            }
        }

        private void SaveAuditRecord(FrontEndAuditTB objaudit)
        {
            using (var context = new CBSDbContext())
            {
               // context.FrontEndAuditLoggs.Add(objaudit);
               //context.SaveChanges();
            }
        }

        private void LogExceptionToDatabase(Exception ex)
        {
            // Implement logging logic here
        }

        // Method to serialize the request data into a string
        private string SerializeRequestData(HttpRequestBase request)
        {
            var requestData = new
            {
                //Headers = request.Headers.AllKeys.ToDictionary(k => k, k => request.Headers[k]),
                Form = request.Form.AllKeys.ToDictionary(k => k, k => request.Form[k]),
                QueryString = request.QueryString.AllKeys.ToDictionary(k => k, k => request.QueryString[k])
                // Add more properties as needed
            };

            // Serialize the request data into JSON string
            string serializedData = JsonConvert.SerializeObject(requestData, Formatting.Indented);

            return serializedData;
        }
    }
    public static class StatusHelper
    {
        public static readonly Dictionary<string, string> StatusClasses = new Dictionary<string, string>
    {
        { "Pending", "badge bg-warning text-dark" },
        { "Approved", "badge bg-success" },
        { "Rejected", "badge bg-danger" },
        { "In Progress", "badge bg-info" },
        { "Cancelled", "badge bg-secondary" },
        // Add more status classes as needed
    };
    }

}