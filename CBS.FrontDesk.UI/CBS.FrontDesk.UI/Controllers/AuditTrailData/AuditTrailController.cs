using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.AuditTrailP;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.AuditTralP;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.UI.Helper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.AuditTrailData
{
    [CheckSessionTimeOutAttribute]

    public class AuditTrailController : BaseController
    {
        // GET: Loan

        private readonly AuditTrailServices auditTrailServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly UserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        public AuditTrailController(AuditTrailServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, UserManagementServices userManagementServices, BranchServices branchServices = null)
        {
            auditTrailServices = LoanServices;
            _loanCommiteeMember = loanCommiteeMember;
            _userManagementServices = userManagementServices;
            _branchServices = branchServices;
        }

        public ActionResult Index()
        {

            return View();
        }
        public async Task<ActionResult> Configuration()
        {

            return View();
        }


        [HttpGet]
        public async Task<ActionResult> Download(string searchCriteria = "all", string dateFrom = null, string dateTo = null, string searchOption = "Action", bool filterByDatesOnly = true)
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                // Parse date strings if provided
                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    startDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", null);
                }

                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    endDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);  // Include the whole day
                }

                // Prepare the DataTable query
                var getAuditTrailsDataTable = new GetAuditTrailsDataTableQuery
                {
                    FilterByDatesOnly=filterByDatesOnly,
                    DataTableOptions = new DataTableOptions
                    {
                        pageSize = 10000,  // Export large number of records
                        start = 0,
                        searchValue = searchCriteria
                    },
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    Feild=searchOption,
                };
                getAuditTrailsDataTable.DataTableOptions=GetDataTableOptions();
                if (searchCriteria=="")
                {
                    searchCriteria= "all";
                }

                getAuditTrailsDataTable.DataTableOptions.pageSize = 10000;  // Export large number of records
                getAuditTrailsDataTable.DataTableOptions.start = 0;
                // Fetch the data
                var dataTable = await auditTrailServices.GetDataTableAsync(getAuditTrailsDataTable, searchCriteria);

                // Convert dataTable.data to List<AuditTrailDto>
                var auditTrailList = JsonConvert.DeserializeObject<List<AuditTrailDto>>(
                    JsonConvert.SerializeObject(dataTable.data)
                );

                // Generate Excel file
                var exportFile = ExportUtilityAuditTrail.ConvertToExcel(auditTrailList, startDate.Value, endDate.Value, searchCriteria);

                // Return file to client for download
                return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error exporting data.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> LoadData(string searchCriteria = "all", string dateFrom = null, string dateTo = null, string searchOption = "Action", bool filterByDateOnly = true)
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    startDate = DateTime.ParseExact(dateFrom, "dd/MM/yyyy", null);
                }

                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    endDate = DateTime.ParseExact(dateTo, "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);  // Include entire day
                }

                var getAuditTrailsDataTable = new GetAuditTrailsDataTableQuery
                {
                    FilterByDatesOnly=filterByDateOnly,
                    DataTableOptions = PostDataTableOptions(),
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue,
                    Feild=searchOption,
                };

                var dataTable = await auditTrailServices.GetDataTableAsync(getAuditTrailsDataTable, searchCriteria);
                var auditTrailList = JsonConvert.DeserializeObject<List<AuditTrailDto>>(
              JsonConvert.SerializeObject(dataTable.data)
          );
                return Json(new
                {
                    draw = getAuditTrailsDataTable.DataTableOptions.draw,
                    recordsTotal = dataTable.recordsTotal,
                    recordsFiltered = dataTable.recordsFiltered,
                    data = auditTrailList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading audit trail data.");
            }
        }



        public async Task<bool> GetList()
        {
            //ViewBag.Groups = await auditTrailServices.GetLoans();
            var users = await _userManagementServices.GetUserDropDownList();
            ViewBag.Users = users.ToList();
            return true;
        }
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            var data = await auditTrailServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            try
            {
                var auditTrail = await auditTrailServices.GetAuditTrailAsync(id);

                if (auditTrail == null)
                {
                    return HttpNotFound("AuditTrail entry not found.");
                }

                return View("Details", auditTrail);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error fetching details.");
            }
        }




    }
}