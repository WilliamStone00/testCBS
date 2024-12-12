using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounts
{
    public class TellerServices : BaseService
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;
        private readonly BranchServices _branchServices;

        public TellerServices()
        {
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objTeller = await GetTeller(id);
                var inResponse = await _savingConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Teller, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objTeller.name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objTeller, false, $"{objTeller.name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Teller>> GetTellers()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<Teller>>>(APICallHelper.GetAllTeller);
                var branches = await _branchServices.GetBranches();

                if (IsHeadOffice())
                {

                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var data = from teller in couApiResponse.ApiResponseData.Data
                                   join branch in branches on teller.branchId equals branch.Id
                                   select new Teller
                                   {
                                       id = teller.id,
                                       isPrimary = teller.isPrimary,
                                       name = teller.name,
                                       code = teller.code,
                                       bankId = teller.bankId,
                                       PerformCashIn = teller.PerformCashIn,
                                       PerformCashOut = teller.PerformCashOut,
                                       PerformTransfer = teller.PerformTransfer,
                                       TellerType = teller.TellerType,
                                       branchId = teller.branchId,
                                       MinimumAmountToManage = teller.MinimumAmountToManage,
                                       MaximumAmountToManage = teller.MaximumAmountToManage,
                                       MinimumDepositAmount = teller.MinimumDepositAmount,
                                       MaximumDepositAmount = teller.MaximumDepositAmount,
                                       MinimumWithdrawalAmount = teller.MinimumWithdrawalAmount,
                                       MaximumWithdrawalAmount = teller.MaximumWithdrawalAmount,
                                       MinimumTransferAmount = teller.MinimumTransferAmount,
                                       MaximumTransferAmount = teller.MaximumTransferAmount,
                                       Branch = branch,
                                       MapMobileMoneyToNoneMemberMobileMoneyReference = teller.MapMobileMoneyToNoneMemberMobileMoneyReference,
                                       inUseStatus = teller.inUseStatus,
                                       activeStatus = teller.activeStatus,
                                       Transactions = teller.Transactions
                                   };

                        return data;
                    }
                }
                else
                {
                    if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                    {
                        var tellers = couApiResponse.ApiResponseData.Data.Where(x => x.branchId == GetBranchID())
                                                                        .Select(teller =>
                                                                        {
                                                                            teller.Branch = branches.FirstOrDefault(b => b.Id == teller.branchId);
                                                                            return teller;
                                                                        });
                        return tellers;
                    }
                }

                return new List<Teller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<StringValues>> GetTellersStringValuesAsync()
        {
            try
            {
                var data = await GetTellers();
                var stringValues = data.Select(a => new StringValues
                {
                    Text = $"[{a.name}] [{a.Branch.Name}] [{(a.isPrimary ? "Primary" : "Sub")}-Teller]",
                    Value = $"{a.id}",
                }).ToList();

                return stringValues;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<SelectList> GetTellersDroupDownListByBranchId(string id = null)
        {
            try
            {

                var data = await GetTellers();
                var values = data.Where(x => x.branchId == id).Select(a => new StringValues
                {
                    Text = $"[{a.name}] [{a.Branch.Name}] [{(a.isPrimary ? "Primary" : "Sub")}-Till]",
                    Value = $"{a.id}",
                });
                var defaultSelectedValue = "default-value";
                return new SelectList(values.ToList(), "Value", "Text", defaultSelectedValue);



            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<Teller>> GetTellersPrimary()
        {
            try
            {
                var couApiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<Teller>>>(APICallHelper.GetAllTeller);
                if (couApiResponse != null)
                {
                    var results = couApiResponse.ApiResponseData.Data.Where(x => x.isPrimary == true);
                    return results;
                }
                return new List<Teller>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Teller> GetTeller(string id)
        {
            try
            {
                var cusResponseObject = await _savingConfigApiHelper.GetAsync<ResponseObject<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Teller, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Teller model)
        {
            try
            {
                model.bankId = GetBankID();
                //model.branchId = GetBranchID();
                // Make an API call to create an individual profile
                var response = await _savingConfigApiHelper.PostAsync<ServiceResponse<Teller>>(APICallHelper.CreateTeller, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.name}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(Teller model)
        {
            try
            {

                var Teller = await GetTeller(model.id);
                if (Teller != null)
                {
                    Teller.code = model.code;
                    Teller.name = model.name;
                    Teller.MinimumAmountToManage = model.MinimumAmountToManage;
                    Teller.MaximumAmountToManage = model.MaximumAmountToManage;
                    Teller.MinimumWithdrawalAmount = model.MinimumWithdrawalAmount;
                    Teller.MaximumWithdrawalAmount = model.MaximumWithdrawalAmount;
                    Teller.MaximumTransferAmount = model.MaximumTransferAmount;
                    Teller.MinimumTransferAmount = model.MinimumTransferAmount;
                    Teller.MaximumDepositAmount = model.MaximumDepositAmount;
                    Teller.MinimumDepositAmount = model.MinimumDepositAmount;
                    Teller.isPrimary = model.isPrimary;
                    Teller.activeStatus = model.activeStatus;
                    Teller.inUseStatus = model.inUseStatus;
                    Teller.PerformTransfer = model.PerformTransfer;
                    Teller.PerformCashIn = model.PerformCashIn;
                    Teller.PerformCashOut = model.PerformCashOut;
                    Teller.TellerType = model.TellerType;
                    Teller.OperationType = model.OperationType;
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Teller, model.id), Teller);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{Teller.name}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
        public async Task<ExecutionMessages> UpdateMobileMoneyConfiguration(MobileMoneyTellerConfigurationCommand model, string action)
        {
            try
            {
                string option = null;
                var Teller = await GetTeller(model.Id);
                if (Teller != null)
                {
                    if (action == "operation_event")
                    {
                        Teller.OperationEventCode = model.OperationEventCode;
                    }
                    else if (action == "topup_accounting_Configuration")
                    {
                        Teller.FromAuxillaryAccountNumber_A = model.FromAuxillaryAccountNumber_A;
                        Teller.ToBranchFloatAccountNumberAuxillary_A = model.ToBranchFloatAccountNumberAuxillary_A;
                        Teller.FromHeadOfficeAccountNumber_B = model.FromHeadOfficeAccountNumber_B;
                        Teller.ToBranchFloatAccountNumberHeadOffice_B = model.ToBranchFloatAccountNumberHeadOffice_B;
                        Teller.FromBranchAccountNumber_C = model.FromBranchAccountNumber_C;
                        Teller.ToBranchFloatAccountNumberBranch_C = model.ToBranchFloatAccountNumberBranch_C;
                        Teller.FromBranchFloatAccountNumber_D = model.FromBranchFloatAccountNumber_D;
                        Teller.ToHeadOfficeFloatAccountNumber_D = model.ToHeadOfficeFloatAccountNumber_D;
                    }
                    else if (action == "float_number_profile")
                    {
                        Teller.MobileMoneyFloatNumber = model.MobileMoneyFloatNumber;
                        Teller.MobileMoneyUserKeepingThePhone = model.MobileMoneyUserKeepingThePhone;
                        Teller.AccountNumber = model.AccountNumber;
                        Teller.MapMobileMoneyToNoneMemberMobileMoneyReference = model.MapMobileMoneyToNoneMemberMobileMoneyReference;
                        option = action;
                    }
                    else if (action == "balance_alert")
                    {
                        Teller.MobileMoneyMaximumBalanceAlertLevel = model.MobileMoneyMaximumBalanceAlertLevel;
                        Teller.MobileMoneyMinimumBalanceAlertLevel = model.MobileMoneyMinimumBalanceAlertLevel;
                    }
                    else if (action == "sms_alert_profile")
                    {
                        Teller.PhoneNumberToRecieveAlert = model.PhoneNumberToRecieveAlert;
                        Teller.MobileMoneyAlertMessageInFrench = model.MobileMoneyAlertMessageInFrench;
                        Teller.MobileMoneyAlertMessageInEnglish = model.MobileMoneyAlertMessageInEnglish;
                    }
                    var branch = await _branchServices.GetBranch(Teller.branchId);
                    var mobileMoneyTellerConfiguration = new MobileMoneyTellerConfigurationCommand
                    {
                        Id = Teller.id,
                        FromAuxillaryAccountNumber_A = Teller.FromAuxillaryAccountNumber_A,
                        ToBranchFloatAccountNumberAuxillary_A = Teller.ToBranchFloatAccountNumberAuxillary_A,
                        FromHeadOfficeAccountNumber_B = Teller.FromHeadOfficeAccountNumber_B,
                        ToBranchFloatAccountNumberHeadOffice_B = Teller.ToBranchFloatAccountNumberHeadOffice_B,
                        FromBranchAccountNumber_C = Teller.FromBranchAccountNumber_C,
                        ToBranchFloatAccountNumberBranch_C = Teller.ToBranchFloatAccountNumberBranch_C,
                        FromBranchFloatAccountNumber_D = Teller.FromBranchFloatAccountNumber_D,
                        ToHeadOfficeFloatAccountNumber_D = Teller.ToHeadOfficeFloatAccountNumber_D,
                        OperationEventCode = Teller.OperationEventCode,
                        MobileMoneyAlertMessageInEnglish = Teller.MobileMoneyAlertMessageInEnglish,
                        MobileMoneyAlertMessageInFrench = Teller.MobileMoneyAlertMessageInFrench,
                        MobileMoneyMinimumBalanceAlertLevel = Teller.MobileMoneyMinimumBalanceAlertLevel,
                        MobileMoneyFloatNumber = Teller.MobileMoneyFloatNumber,
                        MobileMoneyUserKeepingThePhone = Teller.MobileMoneyUserKeepingThePhone,
                        MobileMoneyMaximumBalanceAlertLevel = Teller.MobileMoneyMaximumBalanceAlertLevel,
                        PhoneNumberToRecieveAlert = Teller.PhoneNumberToRecieveAlert,
                        AccountNumber = Teller.AccountNumber,
                        MapMobileMoneyToNoneMemberMobileMoneyReference = Teller.MapMobileMoneyToNoneMemberMobileMoneyReference,
                        Option = option,
                        BranchCode = branch.BranchCode,
                    };
                    var response = await _savingConfigApiHelper.PutAsync<ServiceResponse<Teller>>(string.Format(APICallHelper.MobileMoneyTellerConfiguration, Teller.id), mobileMoneyTellerConfiguration);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, null, MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{Teller.name}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
        public MobileMoneyTellerConfigurationCommand MapTellerToMobileMoneyCommand(Teller teller)
        {
            if (teller == null)
            {
                throw new ArgumentNullException(nameof(teller), "Teller cannot be null.");
            }

            // Map properties from the Teller object to the MobileMoneyTellerConfigurationCommand
            var mobileMoneyTellerConfigCommand = new MobileMoneyTellerConfigurationCommand
            {
                Id = teller.id,
                OperationEventCode = teller.OperationEventCode,
                AccountNumber = teller.AccountNumber,
                MobileMoneyUserKeepingThePhone = teller.MobileMoneyUserKeepingThePhone,
                MobileMoneyFloatNumber = teller.MobileMoneyFloatNumber,
                MobileMoneyMinimumBalanceAlertLevel = teller.MobileMoneyMinimumBalanceAlertLevel,
                MobileMoneyMaximumBalanceAlertLevel = teller.MobileMoneyMaximumBalanceAlertLevel,
                FromAuxillaryAccountNumber_A = teller.FromAuxillaryAccountNumber_A,
                ToBranchFloatAccountNumberAuxillary_A = teller.ToBranchFloatAccountNumberAuxillary_A,
                FromHeadOfficeAccountNumber_B = teller.FromHeadOfficeAccountNumber_B,
                ToBranchFloatAccountNumberHeadOffice_B = teller.ToBranchFloatAccountNumberHeadOffice_B,
                FromBranchAccountNumber_C = teller.FromBranchAccountNumber_C,
                ToBranchFloatAccountNumberBranch_C = teller.ToBranchFloatAccountNumberBranch_C,
                FromBranchFloatAccountNumber_D = teller.FromBranchFloatAccountNumber_D,
                ToHeadOfficeFloatAccountNumber_D = teller.ToHeadOfficeFloatAccountNumber_D,
                PhoneNumberToRecieveAlert = teller.PhoneNumberToRecieveAlert,
                MobileMoneyAlertMessageInFrench = teller.MobileMoneyAlertMessageInFrench,
                MobileMoneyAlertMessageInEnglish = teller.MobileMoneyAlertMessageInEnglish,
                MapMobileMoneyToNoneMemberMobileMoneyReference = teller.MapMobileMoneyToNoneMemberMobileMoneyReference,
            };

            return mobileMoneyTellerConfigCommand;
        }

    }

}
