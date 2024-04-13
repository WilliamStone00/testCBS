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

namespace CBS.BusinessService.Accounts
{
    public class TellerProvissioningServices : BaseService
    {
        private readonly TellerServices _tellerServices;
        private readonly RoleServices _role;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        public TellerProvissioningServices()
        {
            _tellerServices = new TellerServices();
            _role = new RoleServices();
            _userManagementServices = new UserManagementServices();
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
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
                var pTellers = (from a in await _tellerServices.GetTellers() where a.isPrimary select a);
                return pTellers.ToList();

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
                var pTellers = (from a in await _tellerServices.GetTellers() where !a.isPrimary select a);
                return pTellers.ToList();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
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
                    pTellers = (from a in userRoles
                                where a.IsTeller
                                select new Teller
                                {
                                    name = $"{a.RoleName}-{a.FirstName} {a.LastName}",
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
