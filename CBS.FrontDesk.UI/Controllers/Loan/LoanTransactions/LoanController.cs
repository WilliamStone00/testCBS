using CBS.BusinessService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Config;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.MemberOperation;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.LoanTransactions
{
    [CheckSessionTimeOutAttribute]

    public class LoanController : BaseController
    {
        // GET: Loan

        private readonly LoanServices _LoanServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly UserManagementServices _userManagementServices;
        private readonly BranchServices _branchServices;
        public LoanController(LoanServices LoanServices, LoanCommiteeMemberServices loanCommiteeMember, UserManagementServices userManagementServices, BranchServices branchServices = null)
        {
            _LoanServices = LoanServices;
            _loanCommiteeMember = loanCommiteeMember;
            _userManagementServices = userManagementServices;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            
            return View();
        }
        public async Task<ActionResult> Configuration()
        {

            return View();
        }
        //
        public async Task<ActionResult> Details(string KEY=null)
        {
            var loan = await _LoanServices.GetLoanWithCustomerAndBranch(KEY);
            return View(loan);
        }
        // Action to handle file download
        public async Task<ActionResult> DownloadFile(string fileId=null)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
                var Branches = await _branchServices.GetBranches();
                ViewBag.Branches = Branches;
                return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
            }

            try
            {
                // Call the service to download the file
                var response = await _LoanServices.DownloadFile(fileId);

                if (response != null)
                {
                    // If response is successful, return the file
                    return File(response.FileData, response.ContentType, response.FileName);
                }
                else
                {
                    // If the response is null or contains errors, return an error view
               
                    return View("Error", new HandleErrorInfo(new Exception(response.ErrorMessage), "ControllerName", "ActionName"));
                }
            }
            catch (Exception ex)
            {
                // Handle exception and return an error view
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return View("Error", new HandleErrorInfo(ex, "ControllerName", "ActionName"));
            }
        }

        public async Task<ActionResult> LoanReportGeneration()
        {
            var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
            var Branches = await _branchServices.GetBranches();
            ViewBag.Branches = Branches;
            return View(new Loan {FileDownloadInfoLoans= downloadInfoLoans.ToList()});
        }
        [HttpPost]
        public async Task<ActionResult> DownloadFile(InitiateLoanDownloadCommand initiateLoanDownloadCommand)
        {
            var data = await _LoanServices.InitiateBulkDownloadLoansBranch(initiateLoanDownloadCommand);
            ViewBag.Message = Messaging.MessageResult(data);
            ViewBag.Status = data.Result;
            var downloadInfoLoans = await _LoanServices.GetAllFileDownloadInfoLoanPerUser();
            var Branches = await _branchServices.GetBranches();
          
            ViewBag.Branches = Branches;
            return View(new Loan { FileDownloadInfoLoans = downloadInfoLoans.ToList() });
        }
        [HttpPost]
        public async Task<ActionResult> LoadData(string searchCriteria = "All")
        {
            try
            {
                var dataTable = await _LoanServices.GetDataTable(PostDataTableOptions(), searchCriteria,true);
                return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {
         
            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }

            return HttpNotFound(); // Or return a default view for handling unknown paths
        }


        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)

        {
            if (serviceOption == "Loan")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        //var data = await _LoanServices.GetLoans();
                        var sysData = new MemberOperationPanel { Loans = null };
                        return PartialView(partialView, sysData);
                    };
                }
               
            }
            else if (serviceOption == "LoanCommiteeMember")
            {
                //if (path == "list")
                //{
                //    return async () =>
                //    {
                //        var data = await _loanCommiteeMember.GetLoanCommiteeMembers();
                //        var sysData = new LoanCommitee { LoanCommiteeMembers = data.ToList() };
                //        return PartialView(partialView, sysData);
                //    };
                //}
                //else if (path == "new")
                //{
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = new LoanCommiteeMember() });
                //}
                //else
                //{
                //    ViewBag.Key = key;
                //    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = await _loanCommiteeMember.GetLoanCommiteeMember(key) });
                //}

            }
            
            return null;
        }

        public async Task<bool> GetList()
        {
            //ViewBag.Groups = await _LoanServices.GetLoans();
            var users = await _userManagementServices.GetUserDropDownList();
            ViewBag.Users = users.ToList();
            return true;
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _LoanServices.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}