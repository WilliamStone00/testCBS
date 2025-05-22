using CBS.BusinessService.RequestLoggerServicesP;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ConnectedUserData
{
    public class ClientDataController : Controller
    {
        private readonly ClientDataService _dataService = new ClientDataService();

        /// <summary>
        /// Endpoint to log initial login data.
        /// </summary>
        /// <returns>JSON response</returns>
        [HttpPost]
        [Route("api/login-data")]
        public ActionResult LogLoginData()
        {
            return LogClientData("Login");
        }

        /// <summary>
        /// Endpoint to log main layout data.
        /// </summary>
        /// <returns>JSON response</returns>
        [HttpPost]
        [Route("api/client-data")]
        public ActionResult LogClientData()
        {
            return LogClientData("Main");
        }

        /// <summary>
        /// Generic method to handle client data logging.
        /// </summary>
        /// <param name="dataType">Data type ("Login" or "Main")</param>
        /// <returns>JSON response</returns>
        private ActionResult LogClientData(string dataType)
        {
            try
            {
                using (var reader = new StreamReader(Request.InputStream))
                {
                    var jsonData = reader.ReadToEnd();

                    if (string.IsNullOrEmpty(jsonData))
                        return Json(new { status = "error", message = "No data received" });

                    var clientData = JsonConvert.DeserializeObject<ClientDataModel>(jsonData);

                    if (clientData == null)
                        return Json(new { status = "error", message = "Invalid data format" });

                    // Set the data type
                    clientData.DataType = dataType;

                    // Validate and fallback
                    if (string.IsNullOrEmpty(clientData.IPAddress))
                        clientData.IPAddress = Request.UserHostAddress ?? "Unknown IP";

                    if (string.IsNullOrEmpty(clientData.UserAgent))
                        clientData.UserAgent = Request.UserAgent ?? "Unknown UserAgent";

                    // Log to database
                    _dataService.LogClientData(clientData);

                    return Json(new { status = "success" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging client data: {ex.Message}");
                return Json(new { status = "error", message = ex.Message });
            }
        }
    }
}