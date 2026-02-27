using CBS.BusinessService.Accounting;
using CBS.BusinessService.AccountingV2.AccountingYear;
using CBS.BusinessService.AccountingV2.GLSystemReconciliation;
using CBS.BusinessService.AccountingV2.JournalHead;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.MembersAccountSettings;
using CBS.BusinessService.Session;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2.AccountingYear;
using CBS.FrontDesk.Data.Entity.AccountingV2.GLSystemReconciliation;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.DownLoadDTO;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace CBS.FrontDesk.UI.Controllers.CustomerManagement
{
    [CheckSessionTimeOutAttribute]

    public class IndividualController : BaseController
    {
        // GET: Individual
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly AccountServices _accountServices;
        private readonly MemberAccountActivationServices _memberAccountActivationServices;
        private readonly BranchServices _branchServices;
        private readonly CountryServices _countryServices;
        private readonly LocationAggregateService _locationService;
        public IndividualController(IndividualProfileServices individualProfileServices, MemberAccountActivationServices memberAccountActivationServices = null, BranchServices branchServices = null, AccountServices accountServices = null, CountryServices countryServices = null, LocationAggregateService locationService = null)
        {
            _individualProfileServices = individualProfileServices;
            _memberAccountActivationServices = memberAccountActivationServices;
            _branchServices = branchServices;
            _accountServices=accountServices;
            _countryServices=countryServices;
            _locationService=locationService;
        }
        public async Task<ActionResult> List()
        {

            ViewBag.Branches=await _branchServices.GetBranches();
            return View();


        }
        public async Task<ActionResult> Reporting()
        {

            ViewBag.Branches=await _branchServices.GetBranches();
            return View();


        }

        public async Task<ActionResult> CustomerProfile(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            ViewBag.MemberStatus = customer.CustomerList.CustomerId.Contains("PRM") ? "MEMBER REFERENCE NUMBER" : "MEMBER ACCOUNT NUMBER";
            var statuses = await _individualProfileServices.GetAllAsync();
            ViewBag.MemberStatuses = statuses?
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = $"[{s.Name}] - [{s.Description}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

            var politicalstatus = await _individualProfileServices.GetAllPoliticalAsync(); // however you fetch them

            ViewBag.politicalstatuses = politicalstatus?
                .Select(s => new SelectListItem
                {
                    Value = s.Value,
                    Text = $"[{s.Text}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();
            return View(customer);
        }
        public async Task<ActionResult> MyMembers()
        {

            return View();
        }
        public async Task<ActionResult> Account(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            return View(customer);
        }

        public async Task<ActionResult> MemberStatus()
        {

            return View();
        }

        public async Task<ActionResult> MemberStatusTable()
        {

            return View();
        }

        public async Task<JsonResult> GetMemberStatuses()
        {
            var statuses = await _individualProfileServices.GetAllAsync();

            return Json(new
            {
                data = statuses
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string serviceOption = null, string path = null)
        {
            ViewBag.KEY = KEY;
            if (KEY != "null")
            {
                var customer = await InitializeCustomerData(KEY);
                return PartialView(partialView, customer);
            }
            else
            {
                if (serviceOption == "branch")
                {
                    var data = await _individualProfileServices.GetIndividualProfileByBranch();
                    return PartialView(partialView, data);
                }
                else if (serviceOption == "all")
                {
                    //var data = await _individualProfileServices.GetMembers();
                    return PartialView(partialView, null);
                }

            }
            return PartialView(partialView, new List<IndividualProfile>());
        }
        public async Task<ActionResult> MembersReportingDownload(ReportQuerTemplate request)
        {
            // Clear ModelState errors for properties you don't want to validate
            //request.QueryParameter = "all";
            // Manually add the validation errors for `GetTillStatusQuery`
            string DateFrom = request.DateFrom;
            TryValidateModel(request, nameof(request));

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select((e, index) => $"{index + 1}. {e.ErrorMessage}")
                    .ToList();

                string error = string.Join("<br/>", errors);
                return Json(new { success = false, message = error });
            }
            string ParamTitle = $"{request.LegalFormStatus} Members With {request.MembersStatusType}";
            if (request.DateFrom == null)
            {
                request.DateFrom = DateTime.Now.ToString();
                request.DateTo = DateTime.Now.ToString();
            }
            else if (request.LegalFormStatus == "Moral_Person" && DateFrom==null)
            {
                ParamTitle = $"Moral Members";

            }
            else if (request.LegalFormStatus == "Moral_Person" && DateFrom != null)
            {
                ParamTitle = $"Moral Members within dated period {request.DateFrom} to {request.DateTo}";

            }
            else if (request.LegalFormStatus == "Physical_Person" && DateFrom == null)
            {
                ParamTitle = $"Physical Members";

            }
            else if (request.LegalFormStatus == "Physical_Person" && DateFrom != null)
            {
                ParamTitle = $"Physical Members within dated period {request.DateFrom} to {request.DateTo}";

            }
            else if (request.LegalFormStatus == "Physical_Person")
            {
                ParamTitle = $"All Members both Moral and Physical persons";

            }
            else if (request.LegalFormStatus == "Physical_Person")
            {
                ParamTitle = $"All Members both Moral and Physical persons";

            }



            // Call your service to get the data
            var response = await _individualProfileServices.GetAllMembersByParameters(request);
            var data = response.ToList(); // Convert to list if needed

            // Set session variables based on the request
            //if (request.ByBranch)
            //{
            //    if (data.Any())
            //    {
            //        this.HttpContext.Session["RPTBranchName"] = data.FirstOrDefault()?.BranchName;
            //    }
            //}
            //else
            //{
            //    this.HttpContext.Session["RPTBranchName"] = "For All Branches";
            //}

            // Check if data is available
            if (data == null || !data.Any())
            {
                return Json(new { success = false, message = "No data available for the selected query criteria." });

            }
            // If data is available, proceed with setting session variables and redirecting to the report
            this.HttpContext.Session["rptSource"] = data;
            this.HttpContext.Session["param_size"] = "member_listing";
            this.HttpContext.Session["DateFrom"] = request.DateFrom;
            this.HttpContext.Session["DateTo"] = request.DateTo;
            this.HttpContext.Session["ParamTitle"] = ParamTitle;
            this.HttpContext.Session["rptType"] = "ReportWithParameter";
            this.HttpContext.Session["ReportName"] = "Membersrpt.rpt";
            this.HttpContext.Session["rptpath"] = $"~/AppFiles/Reporting/Members/MembersListing/Membersrpt.rpt";
            this.HttpContext.Session["rpttitle"] = $"Members";

            // Construct the URL to redirect to the PDF
            string url = Url.Action("ReportWithParameter", "Reports"); // Adjust the controller Name if different

            // Set the ViewBag variables for the URL to open the report
            ViewBag.UrlToOpen = url;
            ViewBag.CurrentUrl = "/DailyTellerAssignation/TillCashStatus"; // The current URL
            ViewBag.ErrorMessage = string.Empty;

            // Return the view that opens the report in a new window
            return Json(new { success = true, message = "Success." });
        }


        public async Task<ActionResult> MembersReporting()
        {
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View();
        }
        public async Task<ActionResult> Create()
        {
            var customer = new IndividualProfile();
            await PopulateAggregatesInViewBag();


            var statuses = await _individualProfileServices.GetAllAsync(); // however you fetch them

            ViewBag.MemberStatuses = statuses?
                .Select(s => new SelectListItem
                {
                    Value = s.Name,
                    Text = $"[{s.Name}] - [{s.Description}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();

            var politicalstatus = await _individualProfileServices.GetAllPoliticalAsync(); // however you fetch them

            ViewBag.politicalstatuses = politicalstatus?
                .Select(s => new SelectListItem
                {
                    Value = s.Value,
                    Text = $"[{s.Text}]"
                })
                .OrderBy(x => x.Text)
                .ToList()
                ?? new List<SelectListItem>();
            return View(customer);
        }

       

        [HttpGet]
        public async Task<JsonResult> MemberStatusDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "Member Status ID is required." }, JsonRequestBehavior.AllowGet);

            try
            {
                var entry = await _individualProfileServices.GetMemberStausByIdAsync(id);

                if (entry == null)
                    return Json(new { success = false, message = "Member Status not found." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        entry.Id,
                        entry.Name,
                        entry.Description,
                        entry.IsDefault
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred." }, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> Updates(MemberStatus model)
        {

            if (model == null)
                return Json(new { success = false, message = "Invalid or empty model." });

            try
            {
                    // Id present → update existing record
                    var result = await _individualProfileServices.UpdateAsync(model);

                    if (result == null)
                        return Json(new { success = false, message = "No response from service." });

                    return Json(new
                    {
                        success = result.Result,
                        message = Messaging.MessageResult(result),
                        data = result.Data
                    });
                
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ Error: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<ActionResult> Create(IndividualProfile model)
        {
            if (ModelState.IsValid)
            {
                var data = await _individualProfileServices.Create(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Where(e => e.ErrorMessage != null)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Convert the list of error messages to a single string with each message on a new line
                string errorMessage = string.Join("\n", errorMessages);

                // Pass the error message as the message
                return Json(new { success = false, status = false, message = errorMessage });
            }
        }

        private async Task<IndividualCustomerProfile> InitializeCustomerData(string KEY)
        {
            var agrAggregates = await _individualProfileServices.GetAggregates();
            await PopulateAggregatesInViewBag(agrAggregates);
            var results = await _individualProfileServices.GetCustomer(KEY, agrAggregates);
            ViewBag.MemberAccounts = _individualProfileServices.MembersAccounts(results.CustomerAccounts.ToList());
            return results;
        }

        private async Task PopulateAggregatesInViewBag(Aggregrate agrAggregates = null)
        {
            if (agrAggregates == null)
            {
                agrAggregates = await _individualProfileServices.GetAggregates();
            }
            ViewBag.Banks = agrAggregates.Banks;
            ViewBag.Branches = agrAggregates.Branches;
            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.Countries = agrAggregates.Countries;
            ViewBag.Regions = agrAggregates.Regions;
            ViewBag.Divisions = agrAggregates.Divisions;
            ViewBag.Subdivisions = agrAggregates.Subdivisions;
            ViewBag.Towns = agrAggregates.Towns;
            ViewBag.Savings = agrAggregates.Savings;
            ViewBag.Organizations = agrAggregates.Organizations;
            ViewBag.bankingRelationships = agrAggregates.CustomerDefaultEnum.bankingRelationships;
            ViewBag.genders = agrAggregates.CustomerDefaultEnum.genders;
            ViewBag.membershipApprovalStatuses = agrAggregates.CustomerDefaultEnum.membershipApprovalStatuses;
            ViewBag.activeStatuses = agrAggregates.CustomerDefaultEnum.activeStatuses;
            ViewBag.workingStatuses = agrAggregates.CustomerDefaultEnum.workingStatuses;
            ViewBag.legalForms = agrAggregates.CustomerDefaultEnum.legalForms;
            ViewBag.formalOrInformalSectors = agrAggregates.CustomerDefaultEnum.formalOrInformalSectors;
            ViewBag.maritalStatuses = agrAggregates.CustomerDefaultEnum.maritalStatuses;
            ViewBag.languages = _individualProfileServices.GetLanguages();
            ViewBag.Categories = agrAggregates.CustomerDefaultEnum.customerCategories;
            ViewBag.relationships = agrAggregates.CustomerDefaultEnum.relationships;

        }


        [HttpGet]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await _individualProfileServices.GetDataTable(GetDataTableOptions(), searchCriteria);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadDataSearch(string searchCriterial = "All")
        {
            try
            {
                var dataTable = await _individualProfileServices.GetDataTable(GetDataTableOptions(), searchCriterial);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //"url": "/Saving/LoadDataSearch?Search=" + search,
        [HttpPost]
        public async Task<ActionResult> Update(IndividualCustomerProfile model)
        {

            if (model.option == "Profile")
            {
                var data = await _individualProfileServices.UpdateProfile(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "Status")
            {
                var data = await _individualProfileServices.ActivateDeactivate(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "Status")
            {
                var data = await _individualProfileServices.ActivateDeactivate(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "UpdateMemberAccount")
            {
                var data = await _memberAccountActivationServices.Update(model.MemberAccountActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.option == "AddMemberAccount")
            {

                var data = await _memberAccountActivationServices.Create(model.MemberAccountActivation);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "updatePISCA")
            {
                var command = new TagPISCollectionProfileCommand
                {
                    CustomerId = model.CustomerList.CustomerId,
                    TagAsPISCollectionProfile = model.CustomerList.TagAsPISCollectionProfile
                };

                var data = await _individualProfileServices.TagMemberProfilePIS(command);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }

            else if (model.option == "upload")
            {///NextofkingsPhoto,NextofkingsSignature,CustomerPhoto,CustomerSignature,CustomerOtherDocument
                model.CustomerDocumentRequest.CustomerID = model.CustomerList.CustomerId;
                model.CustomerDocumentRequest.ServiceTypeType = "ClientManagement";
                var data = await _individualProfileServices.UploadFiles(model.CustomerDocumentRequest);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "bank_info")
            {
                var data = await _individualProfileServices.UpdateBankInfo(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "cardsignaturespecement")
            {
                var data = await _individualProfileServices.CreateCardSignatureSpecimenDetail(model.CardSignatureSpecimen);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "nextofking")
            {
                var data = await _individualProfileServices.CreateMembershipNextOfKingsMember(model.MembershipNextOfKingsMember);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "membershipstatus")
            {
                var data = await _individualProfileServices.UpdateMembershipStatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "legalstaus")
            {
                var data = await _individualProfileServices.UpdateLegalSector(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "maritalstatus")
            {
                var data = await _individualProfileServices.UpdateMaritalStatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "employmentdetail")
            {
                var data = await _individualProfileServices.UpdateEmployementStatus(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
            }
            else if (model.option == "addaccount")
            {
                var data = await _individualProfileServices.AddCustomerAccount(model.AddCustomerAccount);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            else if (model.option == "ResetPin")
            {
                var data = await _individualProfileServices.ResetPin(model);
                return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });

            }
            //MemberAccountActivation


            return Json(new { success = false, status = false, message = "Fill the required fields." });
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _individualProfileServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> TagMemberPISCA(TagPISCollectionProfileCommand command)
        {
            var result = await _individualProfileServices.TagMemberProfilePIS(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }


        public async Task<ActionResult> RemoveAccount(string id)
        {
            var data = await _accountServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AccountDetails(string accountid)
        {
            var customerAccount = await _accountServices.GetAccount(accountid);
            return PartialView("_AccountDetails", new IndividualCustomerProfile { CustomerAccount = customerAccount });
        }

        [HttpPost]
        public async Task<ActionResult> LoadMembersData(GetCustomersForDataTableQuery query)
        {
            try
            {

                var dataTable = await _individualProfileServices.GetDataTableAsync(query, "MyMembers");

                var customerList = JsonConvert.DeserializeObject<List<CustomerLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );
                var branches = await _branchServices.GetBranches();
                var customers = _individualProfileServices.MapToDtoOrdered(customerList, branches.ToList()); // Optional: for client-side sorting/grouping

                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = customers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading member data.");
            }
        }
        [HttpPost]
        public async Task<ActionResult> LoadMembers(ExportCustomersQueryFilter query)
        {
            // filter.Options.start, filter.Options.length, filter.Options.sortColumnName, etc.
            var dataTable = await _individualProfileServices.GetDataTableAsyncTwo(query);
            var customerList = JsonConvert.DeserializeObject<List<CustomerLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );


            return Json(new
            {
                draw = query.Options?.draw ?? "1",
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = customerList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> DownloadMembersData(ExportCustomersQueryFilter query)
        {
            try
            {
                var fileDownload = await _individualProfileServices.DownloadCustomers(query);

                // Handle null or empty payloads
                if (fileDownload == null || fileDownload.FileData == null || fileDownload.FileData.Length == 0)
                {
                    // 204 avoids a broken file download; caller can decide how to message this
                    return new HttpStatusCodeResult((int)HttpStatusCode.NoContent, "No data to export.");
                }

                // Safe defaults
                var contentType = string.IsNullOrWhiteSpace(fileDownload.ContentType)
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : fileDownload.ContentType;

                var fileName = string.IsNullOrWhiteSpace(fileDownload.FileName)
                    ? $"MembersExport_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx"
                    : fileDownload.FileName;

                return File(fileDownload.FileData, contentType, fileName);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError, "Error exporting member data.");
            }
        }


        [HttpGet]
        public JsonResult GetRegionsByCountry(string countryId)
        {
            var regions = _locationService.GetRegionsByCountry(countryId);
            return Json(regions.Select(r => new { r.Id, r.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDivisionsByRegion(string regionId)
        {
            var divisions = _locationService.GetDivisionsByRegion(regionId);
            return Json(divisions.Select(d => new { d.Id, d.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubDivisionsByDivision(string divisionId)
        {
            var subDivisions = _locationService.GetSubDivisionsByDivision(divisionId);
            return Json(subDivisions.Select(s => new { s.Id, s.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTownsBySubDivision(string subDivisionId)
        {
            var towns = _locationService.GetTownsBySubDivision(subDivisionId);
            return Json(towns.Select(t => new { t.Id, t.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMemberStatus(MemberStatus model)
        {
            try
            {
                // 3️⃣ Call service
                var response = await _individualProfileServices.CreatstatusAsync(model);

                // 🔴 Null safety
                if (response == null)
                {
                    return Json(new
                    {
                        success = false,
                        statusCode = 502,
                        message = "No response from individualProfileServices service."
                    });
                }

                // ✅ SAME RESPONSE CONTRACT AS CloseYear
                return Json(new
                {
                    success = response.IsSuccess,
                    statusCode = response.IsSuccess ? 200 : 400,
                    message = response.Message,
                    data = response.ApiResponseData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    statusCode = 500,
                    message = $"Push record failed: {ex.Message}"
                });
            }
        }


    }
}