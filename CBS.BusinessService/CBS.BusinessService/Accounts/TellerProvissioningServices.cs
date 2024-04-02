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
    public class TellerProvissioningServices:BaseService
    {
        private readonly TellerServices _tellerServices;
        private readonly RoleServices _role;
        private readonly UserManagementServices _userManagementServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        public TellerProvissioningServices()
        {
            _tellerServices=new TellerServices();
            _role= new RoleServices();
            _userManagementServices =new UserManagementServices();
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }
        public bool IsCurrencySumValid(CurrencyNotes currencyNotes, int amount)
        {
            int totalNotesValue = currencyNotes.note10000 * 10000 +
                                  currencyNotes.note5000 * 5000 +
                                  currencyNotes.note2000 * 2000 +
                                  currencyNotes.note1000 * 1000 +
                                  currencyNotes.note500 * 500 +
                                  currencyNotes.coin500 * 500 +
                                  currencyNotes.coin100 * 100 +
                                  currencyNotes.coin50 * 50 +
                                  currencyNotes.coin25 * 25 +
                                  currencyNotes.coin10 * 10 +
                                  currencyNotes.coin5 * 5 +
                                  currencyNotes.coin1;

            return totalNotesValue == amount;
        }
        public async Task<ExecutionMessages> PrimaryTellerProvision(PrimaryTellerProvissioning model)
        {
            try
            {
                if (IsCurrencySumValid(model.currencyNotes, model.amount))
                {
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
                else
                {
                    GetExecutionMessages(model, false, $"{model.amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

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
                if (IsCurrencySumValid(model.currencyNotes, ConverToInteger(model.initialAmount.ToString())))
                {
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
                else
                {
                    GetExecutionMessages(model, false, $"{model.initialAmount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

                }
                // Make an API call to create an individual profile

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
                var pTellers= (from a in await _tellerServices.GetTellers() where a.isPrimary select a);
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





                var pTellers = (from a in await _userManagementServices.GetUSerRoles()  where a.IsTeller select new Teller
                                { 
                                 name=$"{a.RoleName}-{a.FirstName} {a.LastName}", id=a.UserId.ToString(),
                                }).ToList();

                if (!pTellers.Any())
                {
                    pTellers.Add(new Teller { name = $"Primary Teller-Default Admin", id = "4b352b37-332a-40c6-ab05-e38fcf109719" });
                }
                return pTellers.ToList();

            }
            catch (Exception ex)
            {
                // Log and handle exception
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
