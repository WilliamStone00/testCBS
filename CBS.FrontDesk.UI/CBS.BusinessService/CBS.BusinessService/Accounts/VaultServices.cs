
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class VaultServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public VaultServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objHolyDay = await GetVault(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Vault, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objHolyDay.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objHolyDay, false, $"{objHolyDay.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Vault>> GetVaults()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var couApiResponse = await _transactionApiHelper.GetAsync<ServiceResponse<List<Vault>>>(APICallHelper.GetAllVault);
                    if (couApiResponse.IsSuccess)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                    return new List<Vault>();
                }
                else
                {
                   return await GetVaultByBranchIds(GetBranchID());
                }
               
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<Vault>> GetVaultByBranchIds(string branchid)
        {
            try
            {
                var branchVaault = new List<Vault>();
                var couApiResponse = await _transactionApiHelper.GetAsync<ServiceResponse<Vault>>(string.Format(APICallHelper.GetAllVaultByBranch, branchid));
                if (couApiResponse.IsSuccess)
                {
                    branchVaault.Add(couApiResponse.ApiResponseData.Data);
                    return branchVaault;
                }
                return new List<Vault>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Vault> GetVault(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<Vault>>(string.Format(APICallHelper.Get_Vault, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(AddVaultCommand model)
        {
            try
            {
               
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<Vault>>(APICallHelper.CreateVault, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public AddVaultCommand MapVaultToAddVaultCommand(Vault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault), "The vault object cannot be null.");
            }

            return new AddVaultCommand
            {
                Id = vault.Id,
                Name = vault.Name,
                BranchId = vault.BranchId,
                BranchCode = vault.BranchCode,
                Description = vault.Description,
                Location = vault.Location,
                Address = vault.Address,
                Diamention = vault.Diamention,
                MaximumCapacity = vault.MaximumCapacity,
                IsActive = vault.IsActive
            };
        }
        public async Task<ExecutionMessages> Update(AddVaultCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<Vault>>(APICallHelper.Update_Vault, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> VaultInitialization(VaultInitializationCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.VaultInitialize, model);
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
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
    }

}
