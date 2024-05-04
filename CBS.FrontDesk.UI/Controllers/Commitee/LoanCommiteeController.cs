using CBS.BusinessService.LoanCommitee;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity.LoanCommitee;
using CBS.FrontDesk.Data.Message;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Commitee
{
    [CheckSessionTimeOutAttribute]

    public class LoanCommiteeController : BaseController
    {
        // GET: LoanCommitee

        private readonly LoanCommiteeGroupServices _loanCommiteeGroupServices;
        private readonly LoanCommiteeMemberServices _loanCommiteeMember;
        private readonly UserManagementServices _userManagementServices;

        public LoanCommiteeController(LoanCommiteeGroupServices loanCommiteeGroupServices, LoanCommiteeMemberServices loanCommiteeMember, UserManagementServices userManagementServices)
        {
            _loanCommiteeGroupServices = loanCommiteeGroupServices;
            _loanCommiteeMember = loanCommiteeMember;
            _userManagementServices = userManagementServices;
        }

        public async Task<ActionResult> Index()
        {
            await GetList();
            return View(new LoanCommitee());
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(LoanCommitee model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model.ServiceOption, model);
            }
            else
            {
                serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
            }

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, LoanCommitee model)
        {
            if (serviceOption == "commitee_group")
            {
                return () => _loanCommiteeGroupServices.Create(model.LoanCommiteeGroup);
            }
            else if (serviceOption == "commitee_member")
            {
                return () => _loanCommiteeMember.Create(model.LoanCommiteeMember);
            }
            
            else
            {
                return null;
            }
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, LoanCommitee model)
        {
            if (serviceOption == "commitee_group")
            {
                return () => _loanCommiteeGroupServices.Update(model.LoanCommiteeGroup);
            }
            else if (serviceOption == "commitee_member")
            {
                return () => _loanCommiteeMember.Update(model.LoanCommiteeMember);
            }
            
            else
            {
                return null;
            }
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {
            await GetList();
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
            if (serviceOption == "LoanCommiteeGroup")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _loanCommiteeGroupServices.GetLoanCommiteeGroups();
                        var sysData = new LoanCommitee { LoanCommiteeGroups = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeGroup = new LoanCommiteeGroup() });
                }
                else
                {
                    ViewBag.Key = key;
                    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeGroup = await _loanCommiteeGroupServices.GetLoanCommiteeGroup(key) });
                }
            }
            else if (serviceOption == "LoanCommiteeMember")
            {
                if (path == "list")
                {
                    return async () =>
                    {
                        var data = await _loanCommiteeMember.GetLoanCommiteeMembers();
                        var sysData = new LoanCommitee { LoanCommiteeMembers = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = new LoanCommiteeMember() });
                }
                else
                {
                    ViewBag.Key = key;
                    return async () => PartialView(partialView, new LoanCommitee { LoanCommiteeMember = await _loanCommiteeMember.GetLoanCommiteeMember(key) });
                }

            }
            
            return null;
        }
        public async Task<ActionResult> Delete(string id)
        {
            var data = await _loanCommiteeMember.Delete(id);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
        public async Task<bool> GetList()
        {
            ViewBag.Groups = await _loanCommiteeGroupServices.GetLoanCommiteeGroups();
            var users = await _userManagementServices.GetUserDropDownList();
            ViewBag.Users = users.ToList();
            return true;
        }
    }
}