using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Data.UserManagement;
using CBS.BusinessService.UserManagement;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;

namespace CBS.BusinessService.Accounts
{
    public class TellerProvissioningServices : BaseService
    {
        private readonly TellerServices _tellerServices;
        private readonly RoleServices _role;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _brancheServices;
        public TellerProvissioningServices()
        {
            _tellerServices = new TellerServices();
            _role = new RoleServices();
            _userManagementServices = new UserManagementServices();
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _brancheServices = new BranchServices();
        }

        public async Task<ExecutionMessages> PrimaryTellerProvision(PrimaryTellerProvissioning model)
        {
            try
            {
                model.amount = ComputeDenomination(model.currencyNotes);
                if (model.amount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }

                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<OpeningOfTheDayResponse>>(APICallHelper.PrimaryTellerProvisioning, model);
                if (response.IsSuccess)
                {

                    GetExecutionMessages(response, true, $"{model.amount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }



            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> SubTellerProvision(SubTellerProvissioning model)
        {
            try
            {
                model.initialAmount = ComputeDenomination(model.currencyNotes);
                if (model.initialAmount <= 0)
                {
                    GetExecutionMessages(model, false, $"{model.initialAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Amount entered be greater than 0");
                    return ExecutionMessage;
                }
                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<OpeningOfTheDayResponse>>(APICallHelper.SubTellerProvisioning, model);
                if (response.IsSuccess)
                {

                    GetExecutionMessages(response, true, $"{model.initialAmount}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.initialAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }



            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<List<Teller>> GetPrimaryTellers()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var branches = await _brancheServices.GetBranches();
                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    join b in branches on a.branchId equals b.Id
                                    where a.isPrimary
                                    select new Teller
                                    {
                                        activeStatus = a.activeStatus,
                                        bankId = a.bankId,
                                        branchId = a.branchId,
                                        code = a.code,
                                        id = a.id,
                                        inUsedByUserId = a.inUsedByUserId,
                                        name = $"{a.name}-{b.Name}"

                                    }).ToList();
                    return pTellers.ToList();
                }
                else
                {

                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    where a.isPrimary && a.branchId == GetBranchID()
                                    select a).ToList();
                    return pTellers.ToList();
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Teller>> GetSubTellers()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var branches = await _brancheServices.GetBranches();
                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    join b in branches on a.branchId equals b.Id
                                    where !a.isPrimary
                                    select new Teller
                                    {
                                        activeStatus = a.activeStatus,
                                        bankId = a.bankId,
                                        branchId = a.branchId,
                                        code = a.code,
                                        id = a.id,
                                        inUsedByUserId = a.inUsedByUserId,
                                        name = $"{a.name}-{b.Name}"

                                    }).ToList();
                    return pTellers.ToList();
                }
                else
                {

                    var pTellers = (from a in await _tellerServices.GetTellers()
                                    where !a.isPrimary && a.branchId == GetBranchID()
                                    select a).ToList();
                    return pTellers.ToList();
                }


            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Teller>> GetSubTellers(string primaryTellerId)
        {
            try
            {
                // Retrieve the primary teller
                var teller = await _tellerServices.GetTeller(primaryTellerId);

                // Ensure the primary teller exists
                if (teller == null)
                {
                    // Handle the case where the primary teller is not found
                    return new List<Teller>(); // Or throw an exception or handle appropriately
                }

                // Retrieve all tellers
                var allTellers = await GetSubTellers();

                // Filter tellers by branch ID
                var branchTellers = allTellers.Where(x => x.branchId == teller.branchId).ToList();
                return branchTellers;
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception or handle as needed
            }
        }

        public async Task<List<Teller>> GetUserTellerRole()
        {
            try
            {
                List<Teller> pTellers;

                if (IsHeadOffice())
                {
                    // Get all user roles and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    var branches = await _brancheServices.GetBranches();
                    pTellers = (from a in userRoles
                                join b in branches on a.branchId equals b.Id
                                where a.IsTeller
                                select new Teller
                                {
                                    name = $"{a.RoleName}-{a.FirstName} {a.LastName} {b.Name}",
                                    id = a.UserId.ToString(),
                                }).ToList();

                    // If no tellers are found, add a default one
                    if (!pTellers.Any())
                    {
                        pTellers.Add(new Teller { name = $"Primary Teller-Default Admin", id = "4b352b37-332a-40c6-ab05-e38fcf109719" });
                    }
                }
                else
                {
                    // Get user roles for the current branch and filter for tellers
                    var userRoles = await _userManagementServices.GetUSerRoles();
                    pTellers = (from a in userRoles
                                where a.IsTeller && a.branchId == GetBranchID()
                                select new Teller
                                {
                                    name = $"{a.RoleName}-{a.FirstName} {a.LastName}",
                                    id = a.UserId.ToString(),
                                }).ToList();
                }

                return pTellers;
            }
            catch (Exception ex)
            {
                // Log and rethrow exception
                throw ex;
            }
        }
        public async Task<List<Teller>> GetUserTellerRole(string primaryTellerId)
        {
            try
            {
                // Retrieve the primary teller
                var teller = await _tellerServices.GetTeller(primaryTellerId);

                // Ensure the primary teller exists
                if (teller == null)
                {
                    // Handle the case where the primary teller is not found
                    return new List<Teller>(); // Or throw an exception or handle appropriately
                }

                // Retrieve all users with teller role
                var allTellerUsers = await GetUserTellerRole();

                // Filter users by branch ID
                var users = allTellerUsers.Where(x => x.branchId == teller.branchId).ToList();
                return users;
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception or handle as needed
            }
        }


        //public async Task<List<Teller>> GetUserRole()
        //{
        //    try
        //    {
        //        var pTellers = (from a in await _userManagementServices.GetUserList()
        //                        join r in await _role.GetRoles() on a.roleID equals r.Id
        //                        where r.IsTeller
        //                        select new Teller
        //                        {
        //                            name = $"{r.Name}-{a.firstName} {a.lastName}",
        //                            id = a.id.ToString(),
        //                        }).ToList();

        //        if (!pTellers.Any())
        //        {
        //            pTellers.Add(new Teller { name = $"Primary Teller-Default Admin", id = "4b352b37-332a-40c6-ab05-e38fcf109719" });
        //        }
        //        return pTellers.ToList();

        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}
    }
}
