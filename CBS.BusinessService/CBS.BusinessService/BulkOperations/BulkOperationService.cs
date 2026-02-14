using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.BulkOPeration;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.BulkOperations
{
    public  class BulkOperationService : BaseService
    {
        private readonly ApiCallerHelper _transactionConfigApiHelper;
        private readonly BranchServices _branchServices;
        private readonly SavingProductServices _savingProductServices;
        private readonly IndividualProfileServices _individualProfileServices;
        private readonly BranchAccountService _branchAccountService;
        private string operationType;

        public BulkOperationService(BranchServices branchServices = null, IndividualProfileServices individualProfileServices = null, SavingProductServices savingProductServices = null, BranchAccountService branchAccountService=null)
        {
            _transactionConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = branchServices;
            _individualProfileServices = individualProfileServices;
            _savingProductServices = savingProductServices;
            _branchAccountService = branchAccountService;
            operationType = "INCOME";
        }



        public async Task<CustomDataTable<List<BulkOperationDataDetails>>> GetBulkOperationDetailsDataTableAsync(GetAllSimulationDetailBySimulationIdRequestQuery query)
        {
            // Make API call to fetch the DataTable result
            var couApiResponse = await _transactionConfigApiHelper.PostAsync<ResponseObject<CustomDataTable<List<BulkOperationDataDetails>>>>(APICallHelper.BulkOperationDetailsDataTablePaggination,query);

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable<List<BulkOperationDataDetails>>(
                draw: Convert.ToInt32(0),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<BulkOperationDataDetails>(), // No data
                dataTableOptions: new DataTableOptions()
            );
        }

        public async Task<ApiResponse< ServiceResponse<BulkCashOperationFileSummary>>> ProcessBulkCashOperationFileAsync(HttpPostedFileBase file)
        {

            return await _transactionConfigApiHelper.UploadBulkCashPaymentFileAsync<ServiceResponse<BulkCashOperationFileSummary>>(file,APICallHelper.UploadBulkCashOperations);
         
        }
        public async Task<ApiResponse< ServiceResponse<BulkCashOperationFileSummary>>> GetBulkCashOperationFileDetailsAsync(string fileId)
        {

            return await _transactionConfigApiHelper.GetAsync<ServiceResponse<BulkCashOperationFileSummary>>(string.Format(APICallHelper.BulkCashOperations_GetById,fileId));
         
        }

        public async Task<List<SavingProduct>> GetSavingProducts()
        {
            var savingProducts = await _savingProductServices.GetSavingProducts();

            // Using HashSet for faster lookups (O(1) complexity)
            var validProductTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "deposit",
                "savings",
                "salary",
                "preference shares"
            };

            // Filter products where Name exists in validProductTypes
            return savingProducts
                .Where(product => product.Name != null &&
                       validProductTypes.Contains(product.Name.Trim()))
                .ToList();
        }

        public async Task<CustomDataTable<List<BulkOperationData>>> GetBulkOperationDataTableAsync(GetBulkOperationDataTableQuery loansDataTableQuery)
        {
         /*   loansDataTableQuery.DataTableOptions.searchValue = searchCriterial;
            loansDataTableQuery.DataTableOptions.search = searchCriterial;
            loansDataTableQuery.DataTableOptions.sortColumnName = "LoanDate";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId = GetBranchID();
            }*/
            // Make API call to fetch the DataTable result
            var couApiResponse = await _transactionConfigApiHelper.PostAsync<ResponseObject<CustomDataTable<List<BulkOperationData>>>>(
                APICallHelper.BulkOperationDataTablePaggination,
                loansDataTableQuery
            );

           // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable<List<BulkOperationData>>(
                draw: Convert.ToInt32(loansDataTableQuery.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<BulkOperationData>(), // No data
                dataTableOptions: loansDataTableQuery.Options
            );
        }

        public async Task<BulkOperationDataDetails> GetBulkOperationDetailsById(string id)
        {
                try
                {
                    var response = await _transactionConfigApiHelper.GetAsync<ResponseObject<BulkOperationDataDetails>>(string.Concat(APICallHelper.BulkOperationDataDetails, "/" ,id));
                    if (response.ApiResponseData != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    // Log and handle exception
                    throw ex;
                }
        }
        
        public async Task<BulkOperationData> GetBulkOperationById(string id)
        {
                try
                {
                    var response = await _transactionConfigApiHelper.GetAsync<ResponseObject<BulkOperationData>>(string.Concat(APICallHelper.GetAllBulkOperations, "/" ,id));
                    if (response.ApiResponseData != null)
                    {
                        return response.ApiResponseData.Data;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    // Log and handle exception
                    throw ex;
                }
        }

        public async Task<ExecutionMessages> DeleteBulkOperationById(string id,string simulationType)
        {
            try
            {
                var response = await _transactionConfigApiHelper.GetAsync<ResponseObject<BulkOperationData>>(string.Concat(APICallHelper.DeleteBulkOperations, "/", id));
                if (response.ApiResponseData != null && response.ApiResponseData.StatusCode==200)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{id}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(id, false, simulationType, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
                // Log and handle exception
                
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> SimulateOperation(SimulateBulkOperation model)
        {
            try
            {
           
                Branch branch= model.Branches.Where(x=>x.Id==model.BranchId).FirstOrDefault();

                if (model.ContributionAmount>0)
                {
                    var eventName = await _branchAccountService.GetAllBranchAccountsFromDataTableAsync(model.BranchId);

                    var selectedEvent = eventName.Where(x => x.Id == model.DestinationAccountId).FirstOrDefault();

                    BulkOperationContributionSimulationCommand simulateModel= new BulkOperationContributionSimulationCommand(model, selectedEvent.Name, branch);
                    // Make an API call to create an individual profile
                    var response = await _transactionConfigApiHelper.PostAsync<ServiceResponse<CreateBulkOperationSimulation>>(APICallHelper.SimulateBulkAccountContribution, simulateModel);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{simulateModel.SimulationType}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.SimulationType, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }

                }
                else
                {

                  

                    BulkOperationAccountTopupSimulationCommand simulateModel = new BulkOperationAccountTopupSimulationCommand(model, branch);
                    // Make an API call to create an individual profile
                    var response = await _transactionConfigApiHelper.PostAsync<ServiceResponse<CreateBulkOperationSimulation>>(APICallHelper.SimulateBulkAccountTopup, simulateModel);
                    if (response.ApiResponseData != null)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{simulateModel.SimulationType}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.SimulationType, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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

        public async Task<ExecutionMessages> ValidateOperation(ConfirmBulkOperationCommand command)
        {
            try
            {
                  
                    // Make an API call to create an individual profile
                    var response = await _transactionConfigApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.ConfirmBulkOperation, command);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{command.ApprovalStatus}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(command, false, command.ApprovalStatus, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        
        
        
        
        public async Task<ExecutionMessages> SimulateBulkCreditOrDebitOperation(SimulateBulkCreditOrDebitOperationCommand command)
        {
            try
            {
                  
                    // Make an API call to create an individual profile
                    var response = await _transactionConfigApiHelper.PostAsync<ServiceResponse<CreateBulkOperationSimulation>>(APICallHelper.SimulateBulkCreditOrDebitOperation, command);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{command.SimulationType}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(command, false, command.SimulationType, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        
        public async Task<ExecutionMessages> BulkCashInOrCashOutSimulation(SimulateBulkCreditOrDebitOperationCommand command)
        {
            try
            {
                  
                    // Make an API call to create an individual profile
                    var response = await _transactionConfigApiHelper.PostAsync<ServiceResponse<CreateBulkOperationSimulation>>(APICallHelper.SimulateBulkAccountCashInOrCashOut, command);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{command.SimulationType}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(command, false, command.SimulationType, MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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

        public string GetBankID()
        {
            var data = HttpContext.Current?.Session?["BankID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? string.Empty : data;
        }

        public bool IsHeadOffice()
        {
            var value = HttpContext.Current?.Session?["IsHeadOffice"];
            return value is bool booleanValue && booleanValue;
        }

        public string GetBankCode()
        {
            var data = HttpContext.Current?.Session?["BankCode"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBankCode" : data;
        }
        public string GetLanguage()
        {
            var data = HttpContext.Current?.Session?["SelectedLanguage"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "en" : data;
        }
        public string GetRoleId()
        {
            var data = HttpContext.Current?.Session?["RoleId"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoRoleId" : data;
        }

        public string GetBranchCode()
        {
            var data = HttpContext.Current?.Session?["BranchCode"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBranchCode" : data;
        } 
        
        
        public string GetConnectedUser()
        {
            var data = HttpContext.Current?.Session?["FullName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "N0 Name" : data;
        }

        public string GetBranchName()
        {
            var data = HttpContext.Current?.Session?["BranchName"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? "NoBranchName" : data;
        }

        public string GetBranchID()
        {
            var data = HttpContext.Current?.Session?["BranchID"]?.ToString();
            return string.IsNullOrWhiteSpace(data) ? string.Empty : data;
        }

      
    }
}
