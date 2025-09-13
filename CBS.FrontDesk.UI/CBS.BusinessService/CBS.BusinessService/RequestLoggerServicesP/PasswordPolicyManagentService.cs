using BusinessServices;
using CBS.API.Helper.APICallHelper;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Concurrent;
using DocumentFormat.OpenXml.Bibliography;
using CBS.FrontDesk.Data.Entity.Config;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Helper.Helper;
using CBS.BusinessService.UserManagement;
using MongoDB.Driver.Linq;
using DocumentFormat.OpenXml.EMMA;
using CBS.FrontDesk.Data.UserManagement;

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class PasswordPolicyManagentService : BaseService
    {
        private readonly ApiCallerHelper _identityServerBaseUrl;
        private readonly BranchServices _branchServices;
        public PasswordPolicyManagentService()
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            _branchServices=new BranchServices();
        }

        /// <summary>
        /// Adds a blocked user to the system.
        /// </summary>
        public async Task<ExecutionMessages> Add(IdleTime idleTime)
        {
            try
            {
                var command = await MapToCommandAsync(idleTime);
                var response = await _identityServerBaseUrl.PostAsync<ResponseObject<IdleTime>>(
                    APICallHelper.AddPasswordPolicyManagement, command);

                if (response.IsSuccess )
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> Update(IdleTime idleTime)
        {
            try
            {
                var command = await MapToCommandAsync(idleTime);

                var response = await _identityServerBaseUrl.PutAsync<ResponseObject<IdleTime>>(
                    APICallHelper.UpdatePasswordPolicyManagement, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }
        /// <summary>
        /// Deletes a blocked user by ID.
        /// </summary>

        public async Task<AddOrUpdateIdleTimeCommand> MapToCommandAsync(IdleTime idleTime)
        {
            var command = new AddOrUpdateIdleTimeCommand
            {
                IdleDuration = idleTime.IdleDuration,
                IsCentral = idleTime.IsCentral,
                NumberOfDaysRequiredToResetPassword = idleTime.NumberOfDaysRequiredToResetPassword,
                NumberOfDaysToBlockUserIfInactive = idleTime.NumberOfDaysToBlockUserIfInactive,
                NumberOfLoginAttemps = idleTime.NumberOfLoginAttemps,
                PasswordPolicies = idleTime.PasswordPolicies,
                Id = idleTime.Id // include in case of update
            };

            if (!idleTime.IsCentral && !string.IsNullOrWhiteSpace(idleTime.BranchId))
            {
                var branch = await _branchServices.GetBranch(idleTime.BranchId);
                if (branch != null)
                {
                    command.BranchId = branch.Id;
                    command.BranchCode = branch.BranchCode;
                    command.BranchName = branch.DisplayName;
                }
            }
            else
            {
                command.BranchId = string.Empty;
                command.BranchCode = string.Empty;
                command.BranchName = string.Empty;
            }

            return command;
        }


        public async Task<ExecutionMessages> DeleteAsync(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.DeleteAsync<ServiceResponse<bool>>(
                    string.Format(APICallHelper.Get_Delete_PasswordPolicyManagement, id));

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, id, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, id, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, id, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        /// <summary>
        /// Retrieves a blocked user by ID.
        /// </summary>
        public async Task<IdleTime> GetByIdAsync(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<IdleTime>>(
                    string.Format(APICallHelper.Get_Delete_PasswordPolicyManagement, id));

                return response?.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve RateLimitedUser by ID", ex);
            }
        }

        /// <summary>
        /// Retrieves all blocked users.
        /// </summary>
        public async Task<List<IdleTime>> GetAllAsync()
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<List<IdleTime>>>(
                    APICallHelper.GetAllPasswordPolicyManagements);

                return response?.ApiResponseData.Data ?? new List<IdleTime>();
            }
            catch (Exception ex)
            {
                throw new Exception("❌ Failed to retrieve all RateLimitedUser records", ex);
            }
        }
    }

}
