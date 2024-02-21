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

using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.LoanConf;
using static CBS.FrontDesk.Data.Entity.CustomerManagement.IndividualProfile;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using System.Reflection;

namespace CBS.BusinessService.CustomerManagement
{


    public class IndividualProfileServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _bankConfigApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;


        public IndividualProfileServices()
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            //_bankConfigApiHelper = new ApiCallerHelper("https://localhost:7085/");
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }
        //https://localhost:7085/
        // Other existing methods...
        public async Task<ExecutionMessages> UploadFiles(CustomerDocumentRequest attachedToLoan)
        {
            try
            {

                // Check if files are attached
                if (attachedToLoan.AttachedFiles[0] == null)
                {
                    // Handle case where no files are attached
                    return GetExecutionMessages(attachedToLoan, false, attachedToLoan.DocumentType, MessagesResults.Failed,
              ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
              null);
                }
                var additionalParams = new Dictionary<string, string>
                {
                    { "Id", attachedToLoan.CustomerID },
                    { "documentType", attachedToLoan.DocumentType },
                    { "serviceType", attachedToLoan.ServiceTypeType },
                };
                var response = await _bankConfigApiHelper.PostFilesAndParamsAsync<ServiceResponse<DocumentUploadResponse>>(APICallHelper.UploadFile, additionalParams, attachedToLoan.AttachedFiles);
                if (response.ApiResponseData.Success)
                {
                    GetExecutionMessages(response, true, attachedToLoan.DocumentType, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(attachedToLoan, false, attachedToLoan.DocumentType, MessagesResults.Failed,
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
            Func<Task<List<IndividualProfile>>> getDataFunc = async () => (await GetIndividualProfile()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<IndividualProfile>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<IndividualProfile>> GetIndividualProfile()
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.GetAllIndividualProfile);
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
        private IndividualProfile TransformToCustomerList(IndividualProfile a, Branch b, Town t)
        {
            return new IndividualProfile
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
                activeStatus = a.active ? "Active" : "In-Active",
                address = a.address,
                bankId = a.bankId,
                countryId = a.countryId,
                customerId = a.customerId,
                dateOfBirth = a.dateOfBirth,
                divisionId = a.divisionId,
                economicActivitiesId = a.economicActivitiesId,
                gender = a.gender,
                idNumber = a.idNumber,
                isUseOnLineMobileBanking = a.isUseOnLineMobileBanking,
                organizationId = a.organizationId,
                packageId = a.packageId,
                fax = a.fax,
                photoUrl = a.photoUrl,
                signatureUrl = a.signatureUrl,
                bankingRelationship = a.bankingRelationship,
                cardSignatureSpecimens = a.cardSignatureSpecimens,
                customerDocuments = a.customerDocuments,
                pin = a.pin,
                regionId = a.regionId,
                subDivisionId = a.subDivisionId,
                taxIdentificationNumber = a.taxIdentificationNumber,
                bankCode = a.bankCode,
                formalOrInformalSector = a.formalOrInformalSector,
                isMemberOfACompany = a.isMemberOfACompany,
                legalForm = a.legalForm,
                membershipApprovalStatus = a.membershipApprovalStatus,
                branchCode = a.branchCode,
                customerCategory = a.customerCategory,
                customerCategoryId = a.customerCategoryId,
                customerPackageId = a.packageId,
                employerAddress = a.employerAddress,
                employerName = a.employerName,
                employerTelephone = a.employerTelephone,
                income = a.income,
                workingStatus = a.workingStatus,
                idNumberIssueAt = a.idNumberIssueAt,
                idNumberIssueDate = a.idNumberIssueDate,
                ImageNoVirtualPath = a.ImageNoVirtualPath,
                ImageVirtualNoSignaturePath = a.ImageVirtualNoSignaturePath,
                ImageVirtualPath = a.ImageVirtualPath,
                ImageVirtualSignaturePath = a.ImageVirtualSignaturePath,
                isMemberOfAGroup = a.isMemberOfAGroup,
                language = a.language,
                maritalStatus = a.maritalStatus,
                occupation = a.occupation,
                poBox = a.poBox,
                membershipAllocatedNumber = a.membershipAllocatedNumber,
                membershipApplicantDate = a.membershipApplicantDate,
                membershipApplicantProposedByReferral1 = a.membershipApplicantProposedByReferral1,
                membershipApplicantProposedByReferral2 = a.membershipApplicantProposedByReferral2,
                membershipApprovalBy = a.membershipApprovalBy,
                membershipApprovedDate = a.membershipApprovedDate,
                membershipApprovedSignatureUrl = a.membershipApprovedSignatureUrl,
                membershipNextOfKings = a.membershipNextOfKings,
                mobileOrOnLineBankingLoginState = a.mobileOrOnLineBankingLoginState,
                numberOfKids = a.numberOfKids,
                placeOfBirth = a.placeOfBirth,
                secretAnswer = a.secretAnswer,
                secretQuestion = a.secretQuestion,
                spouseAddress = a.spouseAddress,
                spouseContactNumber = a.spouseContactNumber,
                spouseName = a.spouseName,
                spouseOccupation = a.spouseOccupation,


            };
        }
        public async Task<IndividualCustomerProfile> GetCustomer(string id, Aggregrate aggregrates)
        {
            try
            {

                var cusResponseObject = await GetSingleCustomer(id);
                var accountBalance = await GetCustomerBalance(id);
                var accounts = await GetCustomerAccounts(id);
                var data = (from a in new List<IndividualProfile> { cusResponseObject }
                            join b in aggregrates.Branches on a.branchId equals b.Id
                            join t in aggregrates.Towns on a.townId equals t.Id
                            select TransformToCustomerList(a, b, t)).ToList();
                var addaccount = new AddCustomerAccount { customerId = id };
                var nextOfKingsMember = new MembershipNextOfKingsMember { customerId = id };
                var cardSignatureSpecimen = new CardSignatureSpecimen { customerId = id };
                var result = new IndividualCustomerProfile(data.First(), aggregrates, accountBalance, accounts, addaccount,nextOfKingsMember,cardSignatureSpecimen);
                result.SavingProducts = aggregrates.Savings;
                return result;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public IEnumerable<StringValues> MembersAccounts(List<CustomerAccount> accounts)
        {
            try
            {
                var menuMasters = from a in accounts
                                  select new StringValues
                                  {
                                      Value = a.accountNumber.ToString(),
                                      Text = $"{a.accountNumber}-{a.accountName}"
                                  };

                return menuMasters;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<SavingProduct>> GetSavingProducts()
        {
            try
            {

                var apiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<SavingProduct>>>(APICallHelper.GetSavingProducts);
                if (apiResponse.IsSuccess)
                {
                    return apiResponse.ApiResponseData.Data;
                }
                return new List<SavingProduct>();
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
                    var savingsResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<SavingProduct>>>(APICallHelper.GetSavingProducts);
                    aggregates.Savings = savingsResponse?.ApiResponseData == null ? new List<SavingProduct>() : savingsResponse.ApiResponseData.Data;
                    var customerDefaultEnum = await _customerApiHelper.GetAsync<ResponseObject<CustomerDefaultEnum>>(APICallHelper.GetCustomerDefaultEnums);
                    aggregates.CustomerDefaultEnum = customerDefaultEnum?.ApiResponseData == null ? new CustomerDefaultEnum() : customerDefaultEnum.ApiResponseData.Data;
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
        private async Task<IndividualProfile> GetSingleCustomer(string id)
        {
            try
            {
                var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<IndividualProfile>>(string.Format(APICallHelper.GetCustomerByID, id));
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new IndividualProfile();

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        //public async Task<IndividualCustomerProfile> GetCustomerProfile(string id)
        //{
        //    try
        //    {
        //        //var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<CustomerList>>(string.Format(APICallHelper.GetCustomerByID, id));
        //        //var account = await GetCustomerBalance(id);
        //        //var result = new IndividualCustomerProfile(cusResponseObject.Data, new Aggregrate(), account);
        //        var result = await GetSessionProfile(id);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}
        public async Task<ExecutionMessages> AddCustomerAccount(AddCustomerAccount model)
        {
            try
            {
                

                model.bankId = GetBankID();
                model.branchId = GetBranchID();
                var inResponse = await _transactionApiHelper.PostAsync<ServiceResponse<CustomerAccount>>(APICallHelper.AddCustomerSavingAccount, model);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{inResponse.ApiResponseData.Data.product.name} Account.", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, "Account", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, null);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UpdateProfile(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.divisionId = objCustomerProfile.CustomerList.divisionId;
                customer.subDivisionId = objCustomerProfile.CustomerList.subDivisionId;
                customer.regionId = objCustomerProfile.CustomerList.regionId;
                customer.townId = objCustomerProfile.CustomerList.townId;
                customer.countryId = objCustomerProfile.CustomerList.countryId;
                customer.dateOfBirth = objCustomerProfile.CustomerList.dateOfBirth;
                customer.firstName = objCustomerProfile.CustomerList.firstName;
                customer.lastName = objCustomerProfile.CustomerList.lastName;
                customer.idNumberIssueAt = objCustomerProfile.CustomerList.idNumberIssueAt;
                customer.idNumberIssueDate = objCustomerProfile.CustomerList.idNumberIssueDate;
                customer.idNumber = objCustomerProfile.CustomerList.idNumber;
                customer.email = objCustomerProfile.CustomerList.email;
                customer.address = objCustomerProfile.CustomerList.address;
                customer.bankName = GetBranchName();
                customer.language = objCustomerProfile.CustomerList.language;
                customer.gender = objCustomerProfile.CustomerList.gender;
                customer.phone = objCustomerProfile.CustomerList.phone;
                customer.taxIdentificationNumber = objCustomerProfile.CustomerList.taxIdentificationNumber;
                customer.economicActivitiesId = objCustomerProfile.CustomerList.economicActivitiesId;
                customer.occupation = objCustomerProfile.CustomerList.occupation;
                customer.customerCategoryId = objCustomerProfile.CustomerList.customerCategoryId;
                customer.fax = objCustomerProfile.CustomerList.fax;
                customer.poBox = objCustomerProfile.CustomerList.poBox;
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
                customer.organizationId = objCustomerProfile.CustomerList.organizationId;
                customer.branchId = objCustomerProfile.CustomerList.branchId;
                customer.bankId = objCustomerProfile.CustomerList.bankId;
                customer.membershipApprovalStatus = objCustomerProfile.CustomerList.membershipApprovalStatus;
                customer.legalForm = objCustomerProfile.CustomerList.legalForm;
                customer.formalOrInformalSector = objCustomerProfile.CustomerList.formalOrInformalSector;
                customer.bankingRelationship = objCustomerProfile.CustomerList.bankingRelationship;
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
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UpdateMembershipstatus(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.membershipApprovalStatus = objCustomerProfile.CustomerList.membershipApprovalStatus;
                customer.membershipApprovalBy = GetUserFullName();
                customer.membershipApprovedDate = DateTime.Now.ToString();
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
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UpdateLegalSector(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.legalForm = objCustomerProfile.CustomerList.legalForm;
                customer.formalOrInformalSector = objCustomerProfile.CustomerList.formalOrInformalSector;
                customer.bankingRelationship = objCustomerProfile.CustomerList.bankingRelationship;
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
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

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
            {//10005110050004
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                var activation = new CustomerActivation { activate = objCustomerProfile.CustomerList.active, customerId = objCustomerProfile.CustomerList.customerId };
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.ActivateOrDiactivateCustomer, activation);
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
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

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
                var tel = CleanTelephoneNumber(model.phone);
                model.bankCode = GetBankCode();
                model.branchCode = GetBranchCode();
                model.branchId = GetBranchID();
                model.bankId = GetBankID();
                model.employerTelephone=tel;
                model.bankName = GetBranchName();
                model.membershipApplicantDate = DateTime.Now.ToString();
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

        public async Task<ExecutionMessages> CreateMembershipNextOfKingsMember(MembershipNextOfKingsMember model)
        {
            try
            {
              
                model.branchId = GetBranchID();
                var response = await _customerApiHelper.PostAsync<ServiceResponse<MembershipNextOfKingsMember>>(APICallHelper.CreateMembershipNextOfKing, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.name, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> CreateCardSignatureSpecimenDetail(CardSignatureSpecimen model)
        {
            try
            {
                var apiResponse = await _transactionApiHelper.GetAsync<ResponseObject<Account>>(string.Format(APICallHelper.GetAccountByAccountNumber, model.accountNumber));



               model.branchMangerId= apiResponse.ApiResponseData.Data.createdBy;
                model.cardSignatureSpecimenDetails.Add(model.cardSignatureSpecimenDetail);
                model.branchId = GetBranchID();
                var response = await _customerApiHelper.PostAsync<ServiceResponse<CardSignatureSpecimen>>(APICallHelper.CreateCardSignatureSpecimenDetails, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.cardSignatureSpecimenDetail.name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.cardSignatureSpecimenDetail.name, MessagesResults.Failed,
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




        public async Task<ExecutionMessages> ResetPin(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.ResetPin, customer.customerId), customer);
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
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateMaritalStatus(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.maritalStatus = objCustomerProfile.CustomerList.maritalStatus;
                customer.spouseName = objCustomerProfile.CustomerList.spouseName;
                customer.spouseAddress = objCustomerProfile.CustomerList.spouseAddress;
                customer.spouseContactNumber = objCustomerProfile.CustomerList.spouseContactNumber;
                customer.spouseOccupation = objCustomerProfile.CustomerList.spouseOccupation;
                customer.numberOfKids = objCustomerProfile.CustomerList.numberOfKids;
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
        public async Task<ExecutionMessages> UpdateEmployementStatus(IndividualCustomerProfile objCustomerProfile)
        {
            try
            {

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.customerId);
                customer.workingStatus = objCustomerProfile.CustomerList.workingStatus;
                customer.employerName = objCustomerProfile.CustomerList.employerName;
                customer.employerTelephone = objCustomerProfile.CustomerList.employerTelephone;
                customer.employerAddress = objCustomerProfile.CustomerList.employerAddress;
                customer.income = objCustomerProfile.CustomerList.income;
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

    }


}
