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

namespace CBS.BusinessService.CustomerManagement
{


    public class IndividualProfileServices : BaseService
    {
        private readonly ApiCallerHelper _customerApiHelper;
        private readonly ApiCallerHelper _bankConfigApiHelper;
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly ApiCallerHelper _identityServerBaseUrl;

        private readonly BranchServices _branchServices;
        private readonly BankServices _bankServices;

        private readonly MemberAccountActivationServices _memberAccountActivationServices;
        private readonly MemberAccountActivationPolicyServices _memberAccountActivationPolicyServices;

        public IndividualProfileServices(BranchServices branchServices = null, MemberAccountActivationServices memberAccountActivationServices = null, MemberAccountActivationPolicyServices memberAccountActivationPolicyServices = null, BankServices bankServices = null)
        {
            _customerApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            //_bankConfigApiHelper = new ApiCallerHelper("https://localhost:7085/");
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            _branchServices = branchServices;
            _memberAccountActivationServices = memberAccountActivationServices;
            _memberAccountActivationPolicyServices = memberAccountActivationPolicyServices;
            _bankServices = bankServices;
        }
        //https://localhost:7085/
        // Other existing methods...
        //public async Task<ExecutionMessages> UploadFiles(CustomerDocumentRequest documentRequest)
        //{
        //    try
        //    {

        //        // Check if files are attached
        //        if (documentRequest.AttachedFiles[0] == null)
        //        {
        //            // Handle case where no files are attached
        //            return GetExecutionMessages(documentRequest, false, documentRequest.DocumentType, MessagesResults.Failed,
        //      ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
        //      null);
        //        }
        //        var additionalParams = new Dictionary<string, string>
        //        {
        //            { "Id", documentRequest.CustomerID },
        //            { "documentType", documentRequest.DocumentType },
        //            { "serviceType", documentRequest.ServiceTypeType },
        //        };





        //        var response = await _bankConfigApiHelper.PostFilesAndParamsAsync<ServiceResponse<DocumentUploadResponse>>(APICallHelper.UploadFile, additionalParams, documentRequest.AttachedFiles);
        //        if (response.ApiResponseData != null)
        //        {
        //            GetExecutionMessages(response, true, documentRequest.DocumentType, MessagesResults.Success,
        //                ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
        //                null);
        //            return ExecutionMessage;
        //        }
        //        GetExecutionMessages(documentRequest, false, documentRequest.DocumentType, MessagesResults.Failed,
        //            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
        //            null);

        //    }
        //    catch (Exception ex)
        //    {
        //        GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
        //            SystemMessageStatus.Failed.ToString(), ex);
        //    }
        //    return ExecutionMessage;
        //}
        public async Task<ExecutionMessages> UploadFiles(CustomerDocumentRequest documentRequest)
        {
            try
            {
                // Check if files are attached
                if (documentRequest.AttachedFiles[0] == null)
                {
                    // Handle case where no files are attached
                    return GetExecutionMessages(documentRequest, false, "File", MessagesResults.Failed,
                  ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
              null);
                }
                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", documentRequest.CustomerID },
                    { "DocumentId", "N/A" },
                    { "DocumentType", documentRequest.DocumentType },
                    { "ServiceType", documentRequest.ServiceTypeType },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString()},
                    { "CallBackEndPoint", APICallHelper.AttachedDocumentsRemotelyMembers},
                    { "RemoteFilePath", $"MembersImages/{documentRequest.DocumentType}" },
                };
                var response = await _identityServerBaseUrl.PostFilesAndParamsAsync<DocumentUploadResponse>(APICallHelper.AttachedDocuments, additionalParams, documentRequest.AttachedFiles);
                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(documentRequest, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);

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
                if (inResponse.ApiResponseData != null && inResponse.IsSuccess)
                {

                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, $"{customer.FirstName} {customer.LastName}", MessagesResults.Failed,
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

        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true)
        {
            var customerParam = new PagginationResource
            {
                OrderBy = "CustomerId",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                BranchId = GetBranchID(),
                IsByBranch = isByBranch,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            Func<Task<List<IndividualProfile>>> getDataFunc = async () => (await GetMembers(customerParam)).ToList();
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc);
            return dataTable;

        }

        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<IndividualProfile>>> getDataFunc)
        {
            List<IndividualProfile> data = (await getDataFunc()).ToList();
            //var filteredData = DatatableHelper.FilterData(data, dataTableOptions);

            // Handle potential null values safely
            var paginationMetadata = data?.FirstOrDefault()?.PaginationMetadata ?? new PaginationMetadata
            {
                TotalCount = 0
            };

            // Construct CustomDataTable using pagination metadata
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                paginationMetadata.TotalCount,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }


        public async Task<IEnumerable<IndividualProfile>> GetMembers(PagginationResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.SearchByAnyCriterialQuery}?{queryString}";
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(fullUrl);



                //GetCustomersWithPagination
                //var individualProfiles = await _customerApiHelper.PostAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.SearchByAnyCriterialQuery, resource);

                var aggregates = await GetAggregates();
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join b in aggregates.Branches on a.BranchId equals b.Id
                            join t in aggregates.Towns on a.TownId equals t.Id into townJoin
                            from t in townJoin.DefaultIfEmpty() // Left join operation
                            select TransformToCustomerList(a, b, t)).ToList();

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<CustomerListingDS>> GetAllMembersByParameters(ReportQuerTemplate resource)
        {
            try
            {
                var fullUrl = $"{APICallHelper.GetAllByParameters}";
                var apiResponse = await _customerApiHelper.PostAsync<ResponseObject<List<CustomerListingDto>>>(fullUrl, resource);
                var branches = await _branchServices.GetBranches(); // Get list of branches
                var bank = await _bankServices.GetBank(GetBankID()); // Get list of branches
                var data = MapToCustomerListingDS(apiResponse.ApiResponseData.Data.ToList(), branches.ToList(), bank);
                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public List<CustomerListingDS> MapToCustomerListingDS(List<CustomerListingDto> customerListingDtos, List<Branch> branches, Bank bank)
        {
            return customerListingDtos.Select(c =>
            {
                // Find the corresponding branch based on BranchCode or other logic
                var branch = branches.FirstOrDefault(b => b.Id == c.BranchId);
                return new CustomerListingDS
                {
                    Id = c.Id,
                    Name = c.Name,
                    BankingRelationship = c.BankingRelationship,
                    RegistrationDate = c.RegistrationDate,
                    IDNumber = c.IDNumber,
                    IDNumberIssueDate = c.IDNumberIssueDate,
                    MembershipApprovalStatus = c.MembershipApprovalStatus,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    CustomerBranchCode = branch.BranchCode,
                    // Branch-specific details
                    BranchName = branch.Name,
                    BranchCode = branch.BranchCode,
                    BranchAddress = branch.Address,
                    BranchTelephone = branch.Telephone,
                    Logo = bank.LogoUrl,

                    // Head Office details
                    HeadOfficeName = bank.Name,
                    HeadOfficeAddress = bank.Address,
                    HeadOfficeTelephone = bank.Telephone,
                    HeadOfficeEmail = bank.Email,
                    HeadOfficeWebSite = bank.WebSite,
                    HeadOfficeInitial = bank.BankInitial,
                    HeadOfficeCode = bank.BankCode
                };
            }).ToList();
        }
        public async Task<IEnumerable<StringValues>> GetMembers(List<IndividualProfile> individuals)
        {
            try
            {
                var stringValuesList = individuals
                         .Select(x => new StringValues($"{x.name}-[{x.branch}]", x.CustomerId))
                         .ToList();
                return stringValuesList;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<IndividualProfile>> GetIndividualProfileByBranch()
        {
            try
            {




                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(string.Format(APICallHelper.GetCustomersByBranchID, GetBranchID()));
                var aggregates = await GetAggregatesBankConfiguration();

                var data = (from a in individualProfiles.ApiResponseData.Data
                            join b in aggregates.Branches on a.BranchId equals b.Id
                            join t in aggregates.Towns on a.TownId equals t.Id into townJoin
                            from t in townJoin.DefaultIfEmpty() // Left join operation
                            select TransformToCustomerList(a, b, t)).ToList();

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }


        public async Task<IEnumerable<IndividualProfile>> GetAllIndividualProfileLight()
        {
            try
            {
                var individualProfiles = await _customerApiHelper.GetAsync<ResponseObject<List<IndividualProfile>>>(APICallHelper.AllCustomers);

                var individuals = individualProfiles.ApiResponseData.Data.ToList();


                var aggregates = await GetAggregatesBankConfiguration();
                var data = (from a in individuals
                            join b in aggregates.Branches on a.BranchId equals b.Id
                            join t in aggregates.Towns on a.TownId equals t.Id into townJoin
                            from t in townJoin.DefaultIfEmpty() // Left join operation
                            select TransformToCustomerList(a, b, t)).ToList();
                return data;

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private async Task<AccountBalance> GetCustomerBalance(string CustomerId)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<AccountBalance>>(string.Format(APICallHelper.GetCustomerBalance, CustomerId));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<CustomerAccount>> GetCustomerAccounts(string CustomerId)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<CustomerAccount>>>(string.Format(APICallHelper.GetCustomerAccounts, CustomerId));
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
            var customer = new IndividualProfile
            {
                name = $"{a.FirstName} {a.LastName}",
                Phone = a.Phone,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                TownId = a.TownId,
                town = t == null ? "N/A" : t.Name,
                branch = b == null ? "N/A" : b.Name,
                BranchId = a.BranchId,
                Active = a.Active,
                ActiveStatus = a.Active ? "Active" : "In-Active",
                Address = a.Address,
                BankId = a.BankId,
                CountryId = a.CountryId,
                CustomerId = a.CustomerId,
                DateOfBirth = a.DateOfBirth,
                DivisionId = a.DivisionId,
                EconomicActivitiesId = a.EconomicActivitiesId,
                Gender = a.Gender,
                IDNumber = a.IDNumber,
                IsUseOnLineMobileBanking = a.IsUseOnLineMobileBanking,
                OrganizationId = a.OrganizationId,
                CustomerPackageId = a.CustomerPackageId,
                Fax = a.Fax,
                photoUrl = a.photoUrl,
                signatureUrl = a.signatureUrl,
                BankingRelationship = a.BankingRelationship,
                CardSignatureSpecimens = a.CardSignatureSpecimens,
                CustomerDocuments = a.CustomerDocuments,
                pin = a.pin,
                RegionId = a.RegionId,
                SubDivisionId = a.SubDivisionId,
                TaxIdentificationNumber = a.TaxIdentificationNumber,
                bankCode = a.bankCode,
                FormalOrInformalSector = a.FormalOrInformalSector,
                IsMemberOfACompany = a.IsMemberOfACompany,
                LegalForm = a.LegalForm,
                MembershipApprovalStatus = a.MembershipApprovalStatus,
                branchCode = a.branchCode,
                CustomerCategory = a.CustomerCategory,
                CustomerCategoryId = a.CustomerCategoryId,
                EmployerAddress = a.EmployerAddress,
                EmployerName = a.EmployerName,
                EmployerTelephone = a.EmployerTelephone,
                Income = a.Income,
                WorkingStatus = a.WorkingStatus,
                IDNumberIssueAt = a.IDNumberIssueAt,
                IDNumberIssueDate = a.IDNumberIssueDate,
                ImageNoVirtualPath = a.ImageNoVirtualPath,
                ImageVirtualNoSignaturePath = a.ImageVirtualNoSignaturePath,
                ImageVirtualPath = a.ImageVirtualPath,
                ImageVirtualSignaturePath = a.ImageVirtualSignaturePath,
                IsMemberOfAGroup = a.IsMemberOfAGroup,
                Language = a.Language,
                MaritalStatus = a.MaritalStatus,
                Occupation = a.Occupation,
                POBox = a.POBox,
                MembershipAllocatedNumber = a.MembershipAllocatedNumber,
                MembershipApplicantDate = a.MembershipApplicantDate,
                MembershipApplicantProposedByReferral1 = a.MembershipApplicantProposedByReferral1,
                MembershipApplicantProposedByReferral2 = a.MembershipApplicantProposedByReferral2,
                MembershipApprovalBy = a.MembershipApprovalBy,
                MembershipApprovedDate = a.MembershipApprovedDate,
                MembershipApprovedSignatureUrl = a.MembershipApprovedSignatureUrl,
                MembershipNextOfKings = a.MembershipNextOfKings,
                mobileOrOnLineBankingLoginState = a.mobileOrOnLineBankingLoginState,
                NumberOfKids = a.NumberOfKids,
                PlaceOfBirth = a.PlaceOfBirth,
                IsDailyCollector = a.IsDailyCollector,
                SecretAnswer = a.SecretAnswer,
                SecretQuestion = a.SecretQuestion,
                SpouseAddress = a.SpouseAddress,
                SpouseContactNumber = a.SpouseContactNumber,
                SpouseName = a.SpouseName,
                SpouseOccupation = a.SpouseOccupation,
                BankName = b == null ? "N/A" : b.Bank.Name,
                CustomerCode = a.CustomerCode,
                VillageOfOrigin = a.VillageOfOrigin,
                PaginationMetadata = a.PaginationMetadata
            };

            return customer;
        }
        public async Task<IndividualCustomerProfile> GetCustomer(string id, Aggregrate aggregrates)
        {
            try
            {

                var cusResponseObject = await GetSingleCustomer(id);
                var accountBalance = await GetCustomerBalance(id);
                var accounts = await GetCustomerAccounts(id);
                //var policies = await _memberAccountActivationPolicyServices.GetMemberAccountActivationPolicys();
                var memberAccountActivation = await _memberAccountActivationServices.GetMemberAccountActivationByMemberId(id);
                var data = (from a in new List<IndividualProfile> { cusResponseObject }
                            join b in aggregrates.Branches on a.BranchId equals b.Id
                            join t in aggregrates.Towns on a.TownId equals t.Id into townJoin
                            from t in townJoin.DefaultIfEmpty() // Left join operation
                            select TransformToCustomerList(a, b, t)).ToList();
                var addaccount = new AddCustomerAccount { customerId = id };
                var nextOfKingsMember = new MembershipNextOfKing { CustomerId = id };
                var cardSignatureSpecimen = new CardSignatureSpecimen { CustomerId = id };
                var result = new IndividualCustomerProfile(data.First(), aggregrates, accountBalance, accounts, addaccount, nextOfKingsMember, cardSignatureSpecimen);
                result.SavingProducts = aggregrates.Savings;
                result.AddCustomerAccount.CustomerName = result.CustomerList.name;
                //var policy = new MemberRegistrationFeePolicy();
                //if (policies.Any())
                //{
                //    policy = policies.Where(x => x.LegalForm == cusResponseObject.LegalForm).FirstOrDefault();
                //}
                //if (policy == null)
                //{
                //    policy = new MemberRegistrationFeePolicy();
                //}
                //if (memberAccountActivation == null)
                //{
                //    result.MemberAccountActivation = new MemberAccountActivation
                //    {
                //        CustomerId = id,
                //        EntranceFee = policy.MaximumEntrancenFee,
                //        ByeLawFee = policy.MaximumByeLawsFee,
                //        LoanPolicyFee = policy.MaximumLoanPolicyFee,
                //        MemberAccountActivationPolicyId = policy.Id,
                //        Balance = policy.MaximumEntrancenFee + policy.MaximumByeLawsFee + policy.MaximumLoanPolicyFee,
                //        AmountPaid = 0,
                //        BuildingContribution = policy.MaximumBuildingContribution
                //    };
                //    result.option = "AddMemberAccount";
                //}
                //else
                //{
                //    result.MemberAccountActivation = memberAccountActivation;
                //    result.option = "UpdateMemberAccount";
                //}

                return result;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<IndividualCustomerProfile> GetCustomerLight(string id)
        {
            try
            {

                var cusResponseObject = await GetSingleCustomer(id);
                var Branch = await _branchServices.GetBranch(cusResponseObject.BranchId);
                var data = TransformToCustomerList(cusResponseObject, Branch, null);
                var result = new IndividualCustomerProfile(data);
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
        public async Task<Aggregrate> GetAggregatesBankConfiguration()
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
                    aggregates.Savings = savingsResponse?.ApiResponseData == null ? new List<SavingProduct>() : savingsResponse.ApiResponseData.Data.Where(x => !x.IsUsedForTellerProvisioning && x.ActiveStatus).ToList();
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
        public async Task<IndividualProfile> GetSingleCustomer(string id)
        {
            try
            {
                var cusResponseObject = await _customerApiHelper.GetAsync<ResponseObject<IndividualProfile>>(string.Format(APICallHelper.GetCustomerByID, id));
                if (cusResponseObject.ApiResponseData != null)
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
                    GetExecutionMessages(inResponse, true, $"{inResponse.ApiResponseData.Data.product.Name} Account.", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(model, false, "Account", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);

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

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.DivisionId = objCustomerProfile.CustomerList.DivisionId;
                customer.SubDivisionId = objCustomerProfile.CustomerList.SubDivisionId;
                customer.RegionId = objCustomerProfile.CustomerList.RegionId;
                customer.TownId = objCustomerProfile.CustomerList.TownId;
                customer.CountryId = objCustomerProfile.CustomerList.CountryId;
                customer.IsDailyCollector = objCustomerProfile.CustomerList.IsDailyCollector;
                customer.DateOfBirth = objCustomerProfile.CustomerList.DateOfBirth;
                customer.FirstName = objCustomerProfile.CustomerList.FirstName;
                customer.LastName = objCustomerProfile.CustomerList.LastName;
                customer.IDNumberIssueAt = objCustomerProfile.CustomerList.IDNumberIssueAt;
                customer.IDNumberIssueDate = objCustomerProfile.CustomerList.IDNumberIssueDate;
                customer.IDNumber = objCustomerProfile.CustomerList.IDNumber;
                customer.Email = objCustomerProfile.CustomerList.Email;
                customer.Address = objCustomerProfile.CustomerList.Address;
                customer.BankName = GetBranchName();
                customer.Language = objCustomerProfile.CustomerList.Language;
                customer.VillageOfOrigin = objCustomerProfile.CustomerList.VillageOfOrigin;
                customer.Gender = objCustomerProfile.CustomerList.Gender;
                customer.Phone = objCustomerProfile.CustomerList.Phone;
                customer.TaxIdentificationNumber = objCustomerProfile.CustomerList.TaxIdentificationNumber;
                customer.EconomicActivitiesId = objCustomerProfile.CustomerList.EconomicActivitiesId;
                customer.Occupation = objCustomerProfile.CustomerList.Occupation;
                customer.CustomerCategoryId = objCustomerProfile.CustomerList.CustomerCategoryId;
                customer.Fax = objCustomerProfile.CustomerList.Fax;
                customer.POBox = objCustomerProfile.CustomerList.POBox;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.OrganizationId = objCustomerProfile.CustomerList.OrganizationId;
                customer.BranchId = objCustomerProfile.CustomerList.BranchId;
                customer.BankId = objCustomerProfile.CustomerList.BankId;
                customer.MembershipApprovalStatus = objCustomerProfile.CustomerList.MembershipApprovalStatus;
                customer.LegalForm = objCustomerProfile.CustomerList.LegalForm;
                customer.FormalOrInformalSector = objCustomerProfile.CustomerList.FormalOrInformalSector;
                customer.BankingRelationship = objCustomerProfile.CustomerList.BankingRelationship;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.MembershipApprovalStatus = objCustomerProfile.CustomerList.MembershipApprovalStatus;
                customer.MembershipApprovalBy = GetUserFullName();
                customer.MembershipApprovedDate = DateTime.Now.ToString();
                if (customer.VillageOfOrigin == null)
                {
                    customer.VillageOfOrigin = "N/A";
                }
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.LegalForm = objCustomerProfile.CustomerList.LegalForm;
                customer.FormalOrInformalSector = objCustomerProfile.CustomerList.FormalOrInformalSector;
                customer.BankingRelationship = objCustomerProfile.CustomerList.BankingRelationship;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                var activation = new CustomerActivation { activate = objCustomerProfile.CustomerList.Active, customerId = objCustomerProfile.CustomerList.CustomerId };
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.ActivateOrDiactivateCustomer, activation);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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
                var tel = CleanTelephoneNumber(model.Phone);
                model.bankCode = GetBankCode();
                model.branchCode = GetBranchCode();
                model.BranchId = GetBranchID();
                model.BankId = GetBankID();
                model.LegalForm = "Physical_Person";
                model.EmployerTelephone = tel;
                model.MembershipApprovalStatus = "Awaits_Validation";
                model.BankName = GetBranchName();
                model.Email = model.Email ?? "fluxdefault@trustcredit.com";
                model.MembershipApplicantDate = DateTime.Now.ToString();
                var response = await _customerApiHelper.PostAsync<ServiceResponse<IndividualProfile>>(APICallHelper.CreateIndividualProfile, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.FirstName} {model.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
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

        public async Task<ExecutionMessages> CreateMembershipNextOfKingsMember(MembershipNextOfKing model)
        {
            try
            {

                model.BranchId = GetBranchID();
                var response = await _customerApiHelper.PostAsync<ServiceResponse<MembershipNextOfKing>>(APICallHelper.CreateMembershipNextOfKing, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
                var response = await _customerApiHelper.PostAsync<ServiceResponse<CardSignatureSpecimen>>(APICallHelper.CreateCardSignatureSpecimen, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.ResetPin, customer.Phone), new ResetPinCode { Phone = customer.Phone });
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.MaritalStatus = objCustomerProfile.CustomerList.MaritalStatus;
                customer.SpouseName = objCustomerProfile.CustomerList.SpouseName;
                customer.SpouseAddress = objCustomerProfile.CustomerList.SpouseAddress;
                customer.SpouseContactNumber = objCustomerProfile.CustomerList.SpouseContactNumber;
                customer.SpouseOccupation = objCustomerProfile.CustomerList.SpouseOccupation;
                customer.NumberOfKids = objCustomerProfile.CustomerList.NumberOfKids;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
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

                var customer = await GetSingleCustomer(objCustomerProfile.CustomerList.CustomerId);
                customer.WorkingStatus = objCustomerProfile.CustomerList.WorkingStatus;
                customer.EmployerName = objCustomerProfile.CustomerList.EmployerName;
                customer.EmployerTelephone = objCustomerProfile.CustomerList.EmployerTelephone;
                customer.EmployerAddress = objCustomerProfile.CustomerList.EmployerAddress;
                customer.Income = objCustomerProfile.CustomerList.Income;
                var inResponse = await _customerApiHelper.PutAsync<ServiceResponse<IndividualProfile>>(string.Format(APICallHelper.UpdateIndividualProfile, customer.CustomerId), customer);
                if (inResponse.IsSuccess)
                {
                    // Handle success scenario
                    GetExecutionMessages(inResponse, true, $"{customer.FirstName} {customer.LastName}", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(customer, false, customer.FirstName, MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, null);

                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public Task TillCashStatus(ReportQuerTemplate request)
        {
            throw new NotImplementedException();
        }
    }


}
