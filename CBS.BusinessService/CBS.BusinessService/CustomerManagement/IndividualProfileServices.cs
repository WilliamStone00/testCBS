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
using CBS.FrontDesk.Data.Entity.CustomerManagement.Individual;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;

namespace CBS.BusinessService.CustomerManagement
{


    public class IndividualProfileServices : BaseService, IIndividualProfileServices
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _bankConfigApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;

        public IndividualProfileServices()
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        // Other existing methods...
        public async Task<ExecutionMessages> UploadFiles(CustomerDocumentRequest attachedToLoan)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    { "CustomerID", attachedToLoan.CustomerID },
                };
                var response = await _customerApiHelper.PostFilesAndParamsAsync<CustomerDocument>(APICallHelper.AttachedDocuments, additionalParams, attachedToLoan.AttachedFiles);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(attachedToLoan, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                    null);

            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var customer = await GetSingleCustomer(id);
                var inResponse = await _customerApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.DeleteCustomer, id));
                if (inResponse.IsSuccess)
                {
          
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.firstName} {customer.lastName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, $"{customer.firstName} {customer.lastName}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

       

        // Other methods refactored similarly...

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        {
            Func<Task<List<CustomerList>>> getDataFunc = async () => (await GetIndividualProfile()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<CustomerList>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<CustomerList>> GetIndividualProfile()
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<CustomerList>>>(APICallHelper.GetAllIndividualProfile);
                var aggregates = await GetAggregates();

                var data = (from a in individualProfiles.ApiResponseData.Data
                    join b in aggregates.Branches on a.branchId equals b.Id
                    join t in aggregates.Towns on a.townId equals t.Id
                    select TransformToCustomerList(a, b, t)).ToList();

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
       
        private async Task<AccountBalance> GetCustomerBalance(string customerID)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<AccountBalance>>(string.Format(APICallHelper.GetCustomerBalance, customerID));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private async Task<List<CustomerAccount>> GetCustomerAccounts(string customerID)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(string.Format(APICallHelper.GetCustomerAccounts, customerID));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private CustomerList TransformToCustomerList(CustomerList a, Branch b, Town t)
        {
            return new CustomerList
            {
                name = $"{a.firstName} {a.lastName}",
                phone = a.phone,
                firstName = a.firstName,
                lastName = a.lastName,
                email = a.email,
                townId = a.townId,
                town = t.Name,
                branch = b.Name,
                branchId = a.branchId,
                active = a.active,
                status = a.active ? "Active" : "Not Active",
                address = a.address,
                attempts = a.attempts,
                bankId = a.bankId, createdBy = a.createdBy, createdDate = a.createdDate,
                countryId = a.countryId,
                customerId = a.customerId,
                dateOfBirth = a.dateOfBirth,
                divisionId = a.divisionId,
                economicActivitesId = a.economicActivitesId,
                gender = a.gender,
                homePhone = a.homePhone,
                idNumber = a.idNumber,
                isPoliticallyExposed = a.isPoliticallyExposed,
                isUseOnlineMobileBanking = a.isUseOnlineMobileBanking,
                loanCycle = a.loanCycle,
                loginState = a.loginState,
                organisationId = a.organisationId,
                packageId = a.packageId,
                personalPhone = a.personalPhone,
                photoSource = a.photoSource,
                pin = a.pin,
                regionId = a.regionId, 
                subDivisionId = a.subDivisionId,
                taxIdentificationNumber = a.taxIdentificationNumber,
                zipCode = a.zipCode,
            };
        }
        public async Task<IndividualCustomerProfile> GetCustomer(string id)
        {
            try
            {
                //bool exists = SessionHelper.Exists(id);
                //if (exists)
                //{
                //    return SessionHelper.Retrieve<IndividualCustomerProfile>(id);
                //}

                var cusResponseObject = await GetSingleCustomer(id);
                var accountBalance = await GetCustomerBalance(id);
                var aggregates = await GetAggregates();
                var accounts = await GetCustomerAccounts(id);
                var data = (from a in new List<CustomerList> { cusResponseObject }
                    join b in aggregates.Branches on a.branchId equals b.Id
                    join t in aggregates.Towns on a.townId equals t.Id
                    select TransformToCustomerList(a, b, t)).ToList();

                var result = new IndividualCustomerProfile(data.First(), aggregates, accountBalance, accounts);
                //SessionHelper.SaveWithExpiration(id, result, TimeSpan.FromMinutes(1));
                return result;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<Aggregrate> GetAggregates()
        {
            try
            {

                //bool exists = SessionHelper.Exists("aggregates");
                //if (exists)
                //{
                //    return SessionHelper.Retrieve<Aggregrate>("aggregates");
                //}
                
                var subscriptionAggregatesResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<Aggregrate>>(APICallHelper.SubcriptionAggregates);
                if (subscriptionAggregatesResponse.IsSuccess)
                {
                    var aggregates = subscriptionAggregatesResponse?.ApiResponseData.Data ?? new Aggregrate();

                    // Fetching savings data
                    var savingsResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<Saving>>>(APICallHelper.GetSavingProducts);
                    aggregates.Savings = savingsResponse?.ApiResponseData==null? new List<Saving>(): savingsResponse.ApiResponseData.Data;
                    //SessionHelper.SaveWithExpiration("aggregates", aggregates, TimeSpan.FromMinutes(1));
                    return aggregates;
                }
                return new Aggregrate();    
          
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        private async Task<CustomerList> GetSingleCustomer(string id)
        {
            try
            {
                var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<CustomerList>>(string.Format(APICallHelper.GetCustomerByID, id));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<IndividualCustomerProfile> GetCustomerProfile(string id)
        {
            try
            {
                //var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<CustomerList>>(string.Format(APICallHelper.GetCustomerByID, id));
                //var account = await GetCustomerBalance(id);
                //var result = new IndividualCustomerProfile(cusResponseObject.Data, new Aggregrate(), account);
                var result = await GetSessionProfile(id);
                return result;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
       
        public async Task<ExecutionMessages> UpdateProfile(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.divisionId = objCustomerProfile.CustomerList.divisionId;
                customer.subDivisionId = objCustomerProfile.CustomerList.subDivisionId;
                customer.regionId = objCustomerProfile.CustomerList.regionId;
                customer.countryId = objCustomerProfile.CustomerList.countryId;
                customer.dateOfBirth = objCustomerProfile.CustomerList.dateOfBirth;
                customer.firstName = objCustomerProfile.CustomerList.firstName;
                customer.lastName = objCustomerProfile.CustomerList.lastName;
                customer.phone = objCustomerProfile.CustomerList.phone;
                customer.email = objCustomerProfile.CustomerList.email;
                customer.address = objCustomerProfile.CustomerList.address;
                customer.economicActivitesId = objCustomerProfile.CustomerList.economicActivitesId;
                customer.zipCode = objCustomerProfile.CustomerList.zipCode;
                customer.gender = objCustomerProfile.CustomerList.gender;
                customer.personalPhone = objCustomerProfile.CustomerList.personalPhone;
                
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.customerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.firstName} {customer.lastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.firstName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, null);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UpdateBankInfo(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.organisationId = objCustomerProfile.CustomerList.organisationId;
                customer.branchId = objCustomerProfile.CustomerList.branchId;
                customer.bankId = objCustomerProfile.CustomerList.bankId;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.customerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.firstName} {customer.lastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.firstName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, null);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> ActivateDeactivate(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.active = objCustomerProfile.CustomerList.active;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.customerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.firstName} {customer.lastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.firstName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, null);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> Create(IndividualProfile model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _customerApiHelper.PostAsync<ServiceResponse<IndividualProfile>>(APICallHelper.CreateIndividualProfile, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.firstName} {model.lastName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.firstName, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, null);
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

        public async Task<IndividualCustomerProfile> GetSessionProfile(string id)
        {
            var session = await GetCustomer(id);
            return session;
            // Check if the session contains the profile and it is of the correct type
            //if (HttpContext.Current.Session[id] is IndividualCustomerProfile sessionProfile)
            //{
            //    return sessionProfile;
            //}
            //else
            //{
            //    // Handle the case when the session doesn't contain the expected profile or the type is incorrect
            //    var session=await GetCustomer(id);
            //    return session; // or throw an exception, log, etc., based on your application's logic
            //}
        }


    }


}
