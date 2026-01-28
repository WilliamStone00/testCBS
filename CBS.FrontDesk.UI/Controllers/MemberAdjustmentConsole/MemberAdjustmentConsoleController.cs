 using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.Config.Localization;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.LoanP.LoanAdjustmentP;
using CBS.BusinessService.MemberP.MemberAdjustment;
using CBS.BusinessService.MembersAccountSettings;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.LoanAdjustmentP;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.MemberAdjustmentConsole
{
    public class MemberAdjustmentConsoleController : BaseController
    {
        // GET: Individual
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly AccountServices _accountServices;
        private readonly MemberAccountActivationServices _memberAccountActivationServices;
        private readonly BranchServices _branchServices;
        private readonly CountryServices _countryServices;
        private readonly LocationAggregateService _locationService;
        private readonly MemberAdjustmentService _memberAdjustmentService;
        public MemberAdjustmentConsoleController(IndividualProfileServices individualProfileServices, MemberAccountActivationServices memberAccountActivationServices = null, BranchServices branchServices = null, AccountServices accountServices = null, CountryServices countryServices = null, LocationAggregateService locationService = null, MemberAdjustmentService memberAdjustmentService = null)
        {
            _individualProfileServices = individualProfileServices;
            _memberAccountActivationServices = memberAccountActivationServices;
            _branchServices = branchServices;
            _accountServices = accountServices;
            _countryServices = countryServices;
            _locationService = locationService;
            _memberAdjustmentService = memberAdjustmentService;
        }
        public async Task<ActionResult> Index()
        {

            ViewBag.Branches = await _branchServices.GetBranches();
            return View();


        } 
        
        public async Task<ActionResult> MemberAdjustment()
        {

            ViewBag.Branches = await _branchServices.GetBranches();
            return View();


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


        [HttpGet]
        public async Task<ActionResult> GetMemberNamesDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.NameAdjustment, model);
            return PartialView("_MemberNameAdjustmentModalBody", request);
        }  
        
        [HttpGet]
        public async Task<ActionResult> GetMemberActiveStatusDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.MemberActiveStatusAdjustment, model);
            return PartialView("_MemberActiveStatusAdjustmentModalBody", request);
        } 
        
        [HttpGet]
        public async Task<ActionResult> GetMemberMembershipStatusDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.MemberMembershipStatusAdjustment, model);
            return PartialView("_MemberMembershipStatusAdjustmentModalBody", request);
        } 
        
        [HttpGet]
        public async Task<ActionResult> GetMemberStatusDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.MemberActivationAdjustment, model);
            return PartialView("_MemberStatusAdjustmentModalBody", request);
        }
        
        [HttpGet]
        public async Task<ActionResult> GetMemberReferenceDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.MemberReferenceAdjustment, model);
            return PartialView("_MemberReferenceAdjustmentModalBody", request);
        }

        [HttpGet]
        public async Task<ActionResult> GetMemberCategoryDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.MemberCategoryAdjustment, model);
            return PartialView("_MemberCategoryAdjustmentModalBody", request);
        } 
        
        
        [HttpGet]
        public async Task<ActionResult> GetMemberAccountBalanceDetailPartial(string id)
        {
            var model =  await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.AccountBalanceAdjustment, model);
            return PartialView("_MemberAccountBalanceAdjustmentModalBody", request);
        }

        // Block Amount Adjustment
        [HttpGet]
        public async Task<ActionResult> GetMemberBlockAmountDetailPartial(string id)
        {
            var model = await InitializeCustomerData(id);
            var request = _memberAdjustmentService.initialiseMemberAdjustmentModel(AdjustmentType.BlockAmountAdjustment, model);
            return PartialView("_MemberBlockAmountAdjustementModal", request);
        }

        [HttpPost]
        public async Task<ActionResult> MemberAdjustmentRequest(MemberAdjustmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data." });
            }
            if (model.AdjustmentType == AdjustmentType.BlockAmountAdjustment.ToString())
            {
                model.OldBalance = null;
            }
            if (model.DailySavingReExecutionFileId != null)
            {
                model.AdjustmentType = "DailySavingReExecutionAdjustment";
                //model.AdjustmentType ==  AdjustmentType.BlockAmountAdjustment.ToString();
            }


            model.RequestedBy = Session["FullName"]?.ToString();

            var command= _memberAdjustmentService.ConvertMemberAjustmentModel(model);
            var result = await _memberAdjustmentService.SubmitMemberAdjustmentRequestAsync(command);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        

        [HttpPost]
        public async Task<ActionResult> LoadAdjustmentRequestDataTable(GetMemberAdjustmentRequestsDataTableQuery query)
        {
            var dataTable = await _memberAdjustmentService.GetDataTableAsync(query);
            var requestList = JsonConvert.DeserializeObject<List<MemberAdjustmentRequestDetailsDto>>(JsonConvert.SerializeObject(dataTable.data));

            return Json(new
            {
                draw = query.Options.draw,
                recordsTotal = dataTable.recordsTotal,
                recordsFiltered = dataTable.recordsFiltered,
                data = requestList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetRequestDetailPartial(string id)
        {
            var model = await _memberAdjustmentService.GetMemberAdjustmentRequestAsync(id);
            if (model != null && model.Request!= null && model.Request.OldBalance!=0)
            {
                bool isCredit = model.Request.BalanceSenseDifference == "CREDIT";
                if (isCredit)
                {
                    model.Request.NewBalance = model.Request.OldBalance + model.Request.NewBalance;
                }
                else
                {
                    model.Request.NewBalance = model.Request.OldBalance - model.Request.NewBalance;
                }

            }
            return PartialView("_MemberAdjustmentDetailBody", model);
        }

        [HttpPost]
        public async Task<ActionResult> ApproveRequest(ApproveMemberAdjustmentRequestCommand command)
        {
            var result = await _memberAdjustmentService.ApproveMemberAdjustmentRequestAsync(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

        [HttpPost]
        public async Task<ActionResult> RejectRequest(RejectMemberAdjustmentRequestCommand command)
        {
            command.RejectedBy = Session["UserId"]?.ToString();
            var result = await _memberAdjustmentService.RejectMemberAdjustmentRequestAsync(command);
            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

        public async Task<ActionResult> CustomerProfile(string KEY = null, string ReadOptions = null, string path = null, string group = null)
        {
            ViewBag.KEY = KEY;
            var customer = await InitializeCustomerData(KEY);
            ViewBag.MemberStatus = customer.CustomerList.CustomerId.Contains("PRM") ? "MEMBER REFERENCE NUMBER" : "MEMBER ACCOUNT NUMBER";
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
            else if (request.LegalFormStatus == "Moral_Person" && DateFrom == null)
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
            return View(customer);
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

        [HttpGet]
        public async Task<ActionResult> DownloadMembersData(GetCustomersForDataTableQuery query)
        {
            try
            {
                // Ensure we fetch all relevant records
                query.Options = new DataTableOptions
                {
                    pageSize = 10000,
                    start = 0,
                    skip = 0
                };

                var dataTable = await _individualProfileServices.GetDataTableAsync(query, "MyMembers");

                var customerList = JsonConvert.DeserializeObject<List<CustomerLightDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                var branches = await _branchServices.GetBranches();

                var customers = _individualProfileServices.MapToDtoOrdered(customerList, branches.ToList());

                string exportedBy = Session["FullName"]?.ToString() ?? "System Export";

                //var exportFile = ExportUtilityCustomer.GenerateCustomerExcel(
                //    customers,
                //    exportedBy,
                //    query.CreatedFrom?.ToString("dd/MM/yyyy"),
                //    query.CreatedTo?.ToString("dd/MM/yyyy")
                //);

                //return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
                return null;
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting member data.");
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
    }
}