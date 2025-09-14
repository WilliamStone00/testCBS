using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System.Net.Http.Headers;
using CBS.FrontDesk.Data.Entity;
using CBS.BusinessService.Config;
using CBS.BusinessService.MembersAccountSettings;
using CBS.BusinessService.MembersAccountSettings.policy;
using System.Net.Http;
using Newtonsoft.Json;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNet.SignalR.Hosting;
using System.Threading;

namespace CBS.BusinessService.CustomerManagement
{


    public class DailySaverServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly BranchServices _branchServices;
        private readonly IndividualProfileServices _individualProfileServices;


        public DailySaverServices(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices=individualProfileServices;
        }


        public async Task<Aggregrate> GetAggregates()
        {
            try
            {
               return await _individualProfileServices.GetAggregates();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public List<CustomerLightDto> MapToDtoOrdered(List<CustomerLightDto> entities, List<Branch> branches)
        {
            if (entities == null) return new List<CustomerLightDto>();

            return entities
                .Select(x =>
                {
                    var branch = branches.FirstOrDefault(b => b.Id == x.BranchId);

                    return new CustomerLightDto
                    {
                        CustomerId = x.CustomerId,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        FullName = $"{x.FirstName} {x.LastName}",
                        Matricule = x.Matricule,
                        MobileLoginId = x.MobileLoginId,
                        LegalForm = x.LegalForm,
                        MembershipApprovalStatus = x.MembershipApprovalStatus,
                        Gender = x.Gender,
                        CustomerType=x.CustomerType,
                        Phone = x.Phone,
                        BranchId = x.BranchId,
                        BranchName = branch?.Name ?? "Unknown",
                        BranchCode = branch?.BranchCode ?? "N/A",
                        BankId = x.BankId,
                        Language = x.Language,
                        Active = x.Active,
                        CreateDate = x.CreateDate,
                        AccountConfirmationNumber = x.AccountConfirmationNumber
                    };
                })
                .ToList();
        }
       
       
        public async Task<ExecutionMessages> Create(DailySaverCreateVm model)
        {
            try
            {
                var branch=await _branchServices.GetBranch(model.BranchId);
                var tel = CleanTelephoneNumber(model.Phone);
                model.BankName = GetBranchName();
                model.BranchCode=branch.BranchCode;
                model.BranchName=branch.Name;
                model.BankCode=branch.Bank.BankCode;
                model.Email = model.Email ?? "fluxdefault@trustcredit.com";
                var response = await _customerApiHelper.PostAsync<ServiceResponse<IndividualProfile>>(APICallHelper.CreateDailySaverCustomer, model);
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
