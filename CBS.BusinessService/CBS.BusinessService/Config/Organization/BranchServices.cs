using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
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

namespace CBS.BusinessService.Config
{
    public class BranchServices : BaseService, IBranchServices
    {
        private readonly ApiCallerHelper _BranchConfigApiHelper;

        public BranchServices()
        {
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBranch = await GetBranch(id);
                var inResponse = await _BranchConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBranch.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBranch, false, $"{objBranch.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<Branch>> GetBranches()
        {
            try
            {
                var response = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                var branches = response?.ApiResponseData?.Data ?? new List<Branch>();

                // Check if the user is in the head office
                if (!IsHeadOffice())
                {
                    string currentBranchId = GetBranchID();
                    branches = branches.Where(b => b.Id == currentBranchId).ToList();
                }
                else
                {
                    // Add "All" option as the default
                    var defaultBranch = new Branch
                    {
                        Id = "All",
                        BranchCode = "All",
                        Name = "All Branches"
                    };

                    branches.Insert(0, defaultBranch);
                }
                

                // Format the Name and order by BranchCode
                return branches
                    .Select(branch =>
                    {
                        branch.Name = $"[{branch.BranchCode}] [{branch.Name}]";
                        return branch;
                    })
                    .OrderBy(branch => branch.BranchCode)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw;
            }
        }
        // these service send all branch inrespective of the branch
        public async Task<IEnumerable<Branch>> GetCounterpartyBranches()
        {
            try
            {
                var response = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                var branches = response?.ApiResponseData?.Data ?? new List<Branch>();

                // Always add "All" option as the default
                var defaultBranch = new Branch
                {
                    Id = "All",
                    BranchCode = "All",
                    Name = "All Branches"
                };

                branches.Insert(0, defaultBranch);

                // Format the Name and order by BranchCode
                return branches
                    .Select(branch =>
                    {
                        branch.Name = $"[{branch.BranchCode}] [{branch.Name}]";
                        return branch;
                    })
                    .OrderBy(branch => branch.BranchCode)
                    .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<SelectList> GetBranchesByBankId(string id)
        {
            try
            {
                var couApiResponse = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(string.Format(APICallHelper.GetAllBranch));
                var branches = new List<Branch>();

                if (IsHeadOffice())
                {
                    branches = couApiResponse.ApiResponseData.Data
                        .Where(x => x.BankId == id)
                        .ToList();
                    // Add "All" option as the default
                    var defaultBranch = new Branch
                    {
                        Id = "All",
                        BranchCode = "All",
                        Name = "All Branches"
                    };

                    branches.Insert(0, defaultBranch);
                }
                else
                {
                    branches = couApiResponse.ApiResponseData.Data
                        .Where(x => x.BankId == id && x.Id == GetBranchID())
                        .ToList();
                }

               

                // Format and order branches
                var formattedBranches = branches
                    .Select(branch => new
                    {
                        Id = branch.Id,
                        Name = $"[{branch.BranchCode}] [{branch.Name}]"
                    })
                    .OrderBy(branch => branch.Name)
                    .ToList();

                return new SelectList(formattedBranches, "Id", "Name");
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        



        private SelectList ProcessApiResponseResponse(List<Branch> branches)
        {
            var values = branches.Select(a => new Branch
            {
                Name = $"{a.BranchCode} {a.Name}",
                Id = a.Id
            });
            var defaultSelectedValue = "default-value";
            return new SelectList(values.ToList(), "Id", "Name", defaultSelectedValue);

        }

        public async Task<IEnumerable<Branch>> GetLiaison()
        {
            try
            {
                // Call the API to get all branches
                var response = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);

                // Validate response and data
                var branches = response?.ApiResponseData?.Data ?? new List<Branch>();

                // Format and return sorted list
                return branches
                    .Select(branch =>
                    {
                        branch.Name = $"[{branch.BranchCode}] [{branch.Name}]";
                        return branch;
                    })
                    .OrderBy(branch => branch.BranchCode)
                    .ToList();
            }
            catch (Exception ex)
            {
                // Log the error if a logging service is available
                // _logger.LogError(ex, "Error while fetching liaison branches.");

                throw new ApplicationException("An error occurred while retrieving liaison branches.", ex);
            }
        }

        public async Task<Branch> GetBranch(string id)
        {
            try
            {
                var cusResponseObject = await _BranchConfigApiHelper.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                if (cusResponseObject.ApiResponseData != null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new Branch();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<Branch> GetBranchByBankID(string bankid)
        {
            try
            {
                var cusResponseObject = await _BranchConfigApiHelper.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.GetBranchesByBankID, bankid));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Branch model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                var response = await _BranchConfigApiHelper.PostAsync<ServiceResponse<Branch>>(APICallHelper.CreateBranch, model);
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
        public async Task<ExecutionMessages> Update(Branch model)
        {
            try
            {

                var branch = await GetBranch(model.Id);
                if (branch != null)
                {
                    branch.BranchCode = model.BranchCode;
                    branch.Name = model.Name;
                    branch.Address = model.Address;
                    branch.ImmatriculationNumber = model.ImmatriculationNumber;
                    branch.RegistrationNumber = model.RegistrationNumber;
                    branch.Capital = model.Capital;
                    branch.DateOfCreation = model.DateOfCreation;
                    branch.Email = model.Email;
                    branch.Telephone = model.Telephone;
                    branch.Location = model.Location;
                    branch.TaxPayerNUmber = model.TaxPayerNUmber;
                    branch.PBox = model.PBox;
                    branch.WebSite = model.WebSite;
                    branch.BankInitial = model.BankInitial;
                    branch.Motto = model.Motto;
                    branch.IsHeadOffice = model.IsHeadOffice;
                    branch.ActiveStatus = model.ActiveStatus;
                    branch.HeadOfficeTelehoneNumber = model.HeadOfficeTelehoneNumber;
                    branch.HeadOfficeAddress = model.HeadOfficeAddress;
                    var response = await _BranchConfigApiHelper.PutAsync<ServiceResponse<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Branch, model.Id), branch);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{branch.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{branch.Name}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> UploadBranchLogo(CustomerDocumentRequest attachedToLoan)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    { "BranchID", GetBranchID() }
                };
                var response = await _BranchConfigApiHelper.PostFilesAndParamsAsync<ServiceResponse<Bank>>(APICallHelper.UpdateBranchLogo, additionalParams, attachedToLoan.AttachedFiles);
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

    }
}
