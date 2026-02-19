using CBS.API.Helper;
using CBS.BusinessService;
using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.BulkOperations;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AffiliateAccount;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using ZXing;
using static CBS.FrontDesk.Data.Entity.SalaryManagement.EndDateAfterStartDateAttribute;

namespace CBS.FrontDesk.UI.Controllers.StandingOrderP
{
    [CheckSessionTimeOutAttribute]

    public class StandingOrderController : BaseController
    {
        // GET: StandingOrder
        private readonly IndividualProfileServices _services;
        private readonly StandingOrderServices _standingOrderServices;
        private readonly BranchServices _branchServices;

        public StandingOrderController(IndividualProfileServices services, StandingOrderServices standingOrderServices, BranchServices branchServices)
        {
            _services = services;
            _standingOrderServices = standingOrderServices;
            _branchServices = branchServices;
        }


        public async Task loader()
        {
            ViewBag.Branches =await _branchServices.GetBranches();
            ViewBag.SourceAccountTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Savings Account", Value = "Savings" },
                new SelectListItem { Text = "Loan Account", Value = "Loan" },
                new SelectListItem { Text = "Salary Account", Value = "Salary" }
            };
            ViewBag.DestinationAccountTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Savings Account", Value = "Savings" },
                new SelectListItem { Text = "Loan Account", Value = "Loan" },
                new SelectListItem { Text = "Salary Account", Value = "Salary" }
            };
            ViewBag.Frequencies = new List<SelectListItem>
            {
                new SelectListItem { Text = "Daily", Value = "Daily" },
                new SelectListItem { Text = "Weekly", Value = "Weekly" },
                new SelectListItem { Text = "Monthly", Value = "Monthly" },
                new SelectListItem { Text = "Quarterly", Value = "Quarterly" },
                new SelectListItem { Text = "Bi-Annually", Value = "Bi-Annually" },
                new SelectListItem { Text = "Annually", Value = "Annually" }
            };
        }

        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Listing()
        {
           await  loader();
            return View();
        }
     
      
       
        [HttpPost]
        public async Task<ActionResult> Create(StandingOrderCarrier model)
        {

            if (ModelState.IsValid)
            {
                if (model.Action=="insert")
                {
                    var data = await _standingOrderServices.Create(model.AddOrUpdateStandingOrderCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), activationid = data.SessionID });
                }
                else
                {
                    var data = await _standingOrderServices.Update(model.AddOrUpdateStandingOrderCommand);
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data), activationid = data.SessionID });

                }

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
        public async Task<ActionResult> Update(StandingOrderCarrier model)
        {
            if (ModelState.IsValid)
            {
                var data = await _standingOrderServices.Update(model.AddOrUpdateStandingOrderCommand);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            // Convert ModelState errors to a readable string
            var errors = ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .SelectMany(ms => ms.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var errorMessage = string.Join("<br />", errors);

            return Json(new { success = false, status = false, message = errorMessage });
        }

        [HttpGet]
        public async Task<JsonResult> GetCustomerAccounts(string customerId)
        {
            try
            {
                var customerData = await _standingOrderServices.GetCustomerAccountDropdownAsync(customerId);
                // var customerData = await _ipsClaimService.GetinfoAsync(customerId);

                return Json(new
                {
                    success = true,
                    customer = customerData.CustomerDto,
                    accounts = customerData.AccountSelectList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
           
            
            if (path=="search")
            {
                ViewBag.Key=null;
                var data = await _services.GetCustomerLight(KEY);
                if (data!=null)
                {
                    var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(KEY);

                    return PartialView(partialView, new StandingOrderCarrier { Customer = data.CustomerList, AddOrUpdateStandingOrderCommand=new AddOrUpdateStandingOrderCommand { MemberId=data.CustomerList.CustomerId, MemberName=$"{data.CustomerList.FirstName} {data.CustomerList.LastName}"}, StandingOrders=standingOrders .ToList()});

                }
                else
                {
                    ViewBag.message = $"{KEY} was not found in the database.";

                    return PartialView("_DataNotFound", new StandingOrderCarrier());
                }
            }
            else if (path=="list")
            {
                var standingOrder = await _standingOrderServices.GetAllStandingOrder();
                var StandingOrderCarrier = new StandingOrderCarrier { StandingOrders=standingOrder.ToList()};
                return PartialView(partialView, StandingOrderCarrier);
            }
            else if (path=="my_orders")
            {
                var data = await _services.GetCustomerLight(KEY);
                if (data!=null)
                {
                    var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(KEY);
                    return PartialView(partialView, new StandingOrderCarrier {StandingOrders=standingOrders.ToList(), Customer=data.CustomerList });

                }
                else
                {
                    ViewBag.message = $"{KEY} was not found in the database.";

                    return PartialView("_DataNotFound", new StandingOrderCarrier());
                }
            }
            else
            {
                ViewBag.Key=KEY;
                var standingOrder = await _standingOrderServices.GetStandingOrder(KEY);
                var addOrUpdateStanding=_standingOrderServices.MapStandingOrderToCommand(standingOrder);
                var data = await _services.GetCustomerLight(standingOrder.MemberId);
                var StandingOrderCarrier = new StandingOrderCarrier { AddOrUpdateStandingOrderCommand=addOrUpdateStanding, StandingOrder=standingOrder, Customer=data.CustomerList };
                return PartialView(partialView, StandingOrderCarrier);
            }
        }
        public async Task<ActionResult> GetStandingOrderPartialView(string Key)
        {
            var data = await _standingOrderServices.GetStandingOrder(Key);
            if (data == null)
            {
                return HttpNotFound();
            }
            var standingOrders = await _standingOrderServices.GetStandingOrderByMemberId(data.MemberId);
            StandingOrderCarrier standingOrder = new StandingOrderCarrier();
            standingOrder.StandingOrders=standingOrders.ToList();
            standingOrder.StandingOrder=data;
            return PartialView("_StandingOrderDetailsPartial", standingOrder);
        }
        public async Task<ActionResult> Delete(string KEY)
        {
            var data = await _standingOrderServices.Delete(KEY);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet]
        //public async Task<ActionResult> LoadData(string searchCriteria = "All")
        //{
        //    try
        //    {
        //        var dataTable = await _standingOrderServices.GetDataTable(GetDataTableOptions(), searchCriteria);
        //        return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}
        //[HttpPost]
        //public async Task<ActionResult> LoadDataSearch(string searchCriterial = "All")
        //{
        //    try
        //    {
        //        var dataTable = await _standingOrderServices.GetDataTable(GetDataTableOptions(), searchCriterial);
        //        return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        public ActionResult StandingOrderRegistration()
        {
         
            return View();
        }

        

        public async Task<ActionResult> DownloadStandingOrderMemberRegistrationTemplate()
        {
            try
            {
                const string fileName = "StandingOrderCreationTemplateUploadFile.xlsx";
                string directoryPath = Server.MapPath("~/AppFiles/StandingOrder");

                // Validate directory exists
                if (!Directory.Exists(directoryPath))
                {
                    return Json(new { success = false, status = false, message = "Standing Order template directory not found" });
                }

                string filePath = Path.Combine(directoryPath, fileName);

                // Validate file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return Json(new { success = false, status = false, message = "Template file not found" });
                }

                // Read file asynchronously
                byte[] fileBytes;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    fileBytes = new byte[fileStream.Length];
                    await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                }

                // Return the file
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, status = false, message = "Access denied to template file" });
            }
            catch (IOException ex)
            {
                return Json(new { success = false, status = false, message = $"Error reading template file: {ex.Message}" });
            }
            catch (Exception ex)
            {

                // Log the exception here
                return Json(new { success = false, status = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        public async Task<ActionResult> StandingOrderMemberRegistrationUploadFile(HttpPostedFileBase file)
        {
            try
            {
                // Validation (keep your existing validation code)

                // Process the file
                var result = await _standingOrderServices.ProcessStandingOrderMemberRegistrationUploadFileAsync(file);

                if (result == null || !result.IsSuccess || result.ApiResponseData == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = result == null ? "Failed to process file" : result.Message ?? "Failed to process file",
                        error = (result == null || result.ApiResponseData == null) ? null : result.ApiResponseData.Errors // Include any additional error details
                    });
                }


                // Return proper JSON structure
                return Json(new
                {
                    draw = Request.Form["draw"] ?? "1",
                    recordsTotal = result.ApiResponseData.Data?.FileDetails?.Count ?? 0,
                    recordsFiltered = result.ApiResponseData.Data?.FileDetails?.Count ?? 0,
                    data = result.ApiResponseData.Data ?? new StandingOrderMemberRegistrationUploadSummary(),
                    success = true,
                    message = "File processed successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the error
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your file.",
                    error = ex.Message // Only include in development
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateStandingOrdersFromUpload(RegisterStandingOrderUploadMain model)
        {

           
            var result = await _standingOrderServices.RegisterListOfStandingOrdersAsync(model);
            if (result == null || !result.IsSuccess || result.ApiResponseData == null)
            {
                return Json(new
                {
                    success = false,
                    message = result == null ? "Bulk SO Registration Failed" : result.Message ?? "Bulk  SO Registration Failed",
                    error = (result == null || result.ApiResponseData == null) ? null : result.ApiResponseData.Errors // Include any additional error details
                });
            }


            // Return proper JSON structure
            return Json(new
            {
                draw = Request.Form["draw"] ?? "1",
                recordsTotal = result.ApiResponseData.Data?.RowCreationResult?.Count ?? 0,
                recordsFiltered = result.ApiResponseData.Data?.RowCreationResult?.Count ?? 0,
                data = result.ApiResponseData.Data?.RowCreationResult ?? new List<StandingOrderUploadRowResult>(),
                success = true,
                message = " SO Registration processed successfully"
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> CreateStandingOrdersFromManual(RegisterStandingOrderUploadMain model)
        {

           
            var result = await _standingOrderServices.RegisterListOfStandingOrdersAsync(model);
            if (result == null || !result.IsSuccess || result.ApiResponseData == null)
            {
                return Json(new
                {
                    success = false,
                    message = result == null ? "Bulk SO Registration Failed" : result.Message ?? "Bulk  SO Registration Failed",
                    error = (result == null || result.ApiResponseData == null) ? null : result.ApiResponseData.Errors // Include any additional error details
                });
            }


            // Return proper JSON structure
            return Json(new
            {
                draw = Request.Form["draw"] ?? "1",
                recordsTotal = result.ApiResponseData.Data?.RowCreationResult?.Count ?? 0,
                recordsFiltered = result.ApiResponseData.Data?.RowCreationResult?.Count ?? 0,
                data = result.ApiResponseData.Data?.RowCreationResult ?? new List<StandingOrderUploadRowResult>(),
                success = true,
                message = " SO Registration processed successfully"
            }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public async Task<JsonResult> LoadStandingOrderData(StandingOrderDataTableQuery query)
        {
            //await loader();
            try
            {

                if(!_standingOrderServices.IsHeadOffice())
                {
                    query.BranchId = _standingOrderServices.GetBranchID();
                }

                var data = await _standingOrderServices.GetStandingOrderDataTableAsync(query);

                var standingOrders = JsonConvert.DeserializeObject<List<StandingOrder>>(JsonConvert.SerializeObject(data.data));

                return Json(new
                {
                    draw = data.DataTableOptions.draw,
                    recordsTotal = data.recordsTotal,
                    recordsFiltered = data.recordsFiltered,
                    data = standingOrders
                });
            }
            catch (Exception ex)
            {
                // return a DataTables-compatible empty result on error
                return Json(new
                {
                    draw = query?.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

    }

}