using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CustomerManagement
{
    public class GroupServices : BaseService
    {
        private readonly ApiCallerHelper _customerConfigApiHelper;
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public GroupServices()
        {
            _customerConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objGroup = await GetGroup(id);
                var inResponse = await _customerConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_Group, id), id));
                if (inResponse.ApiResponseData != null)
                {

                    GetExecutionMessages(inResponse, true, $"{objGroup.GroupName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objGroup, false, $"{objGroup.GroupName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<IEnumerable<Group>> GetGroups()
        {
            try
            {
                var couApiResponse = await _customerConfigApiHelper.GetAsync<ResponseObject<List<Group>>>(APICallHelper.GetAllGroups);
                if (couApiResponse.ApiResponseData != null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<Group>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool IsByBranch = true)
        {
            var customerParam = new GroupResource
            {
                PageSize = dataTableOptions.pageSize,
                OrderBy = "GroupName",
                Skip = dataTableOptions.skip,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
                IsByBranch = IsByBranch,
                BranchId = GetBranchID(),
            };
            Func<Task<List<Group>>> getDataFunc = async () => (await GetGroups(customerParam)).ToList();
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc);
            return dataTable;

        }

        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<Group>>> getDataFunc)
        {
            List<Group> data = (await getDataFunc()).ToList();
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


        public async Task<IEnumerable<Group>> GetGroups(GroupResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.SearchByAnyCriterialGroupQuery}?{queryString}";
                var apiResponse = await _customerConfigApiHelper.GetAsync<ResponseObject<List<Group>>>(fullUrl);

                var data = apiResponse.ApiResponseData.Data;

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<Group> GetGroup(string id)
        {
            try
            {
                var cusResponseObject = await _customerConfigApiHelper.GetAsync<ResponseObject<Group>>(string.Format(APICallHelper.Get_Update_Delete_Group, id));
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
        public async Task<GroupManagement> GetGroupManagement(string groupId)
        {
            try
            {
                var cusResponseObject = await _customerConfigApiHelper.GetAsync<ResponseObject<Group>>(string.Format(APICallHelper.Get_Update_Delete_Group, groupId));

                if (cusResponseObject?.ApiResponseData != null && cusResponseObject.ApiResponseData.Data != null)
                {
                    var group = cusResponseObject.ApiResponseData.Data;
                    var groupManagement = new GroupManagement
                    {
                        Group = group,


                        GroupCustomers = group.GroupCustomers,
                        GroupDocuments = group.GroupDocuments,
                        GroupDocument = new GroupDocument { GroupId = groupId },
                        AddGroupCustomerCommand = new AddGroupCustomerCommand { GroupId = groupId, commit = false, CustomerIds = new List<string>() },
                        Customer = group.GroupCustomers.Where(x => x.CustomerId == group.GroupLeaderId).FirstOrDefault().Customer,
                    };
                    return groupManagement;
                }
                else
                {
                    // Handle the case when no data is returned or ApiResponseData is null
                    return new GroupManagement();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ExecutionMessages> Create(Group model)
        {
            try
            {
                model.BranchCode = GetBranchCode();
                model.BankCode = GetBankCode();
                model.BranchId = GetBranchID();
                model.BankId = GetBankID();
                model.BankName = GetBankName();
                model.BranchName = GetBranchName();
                model.PhotoSource = "N/A";
                model.GroupLeaderId = "N/A";
                // Make an API call to create an individual profile
                var response = await _customerConfigApiHelper.PostAsync<ServiceResponse<Group>>(APICallHelper.CreateGroup, model);
                if (response.ApiResponseData != null)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.GroupName}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.GroupName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(GroupManagement group)
        {
            try
            {
                if (group.Option == "addmembers")
                {
                    var response = await _customerConfigApiHelper.PostAsync<ServiceResponse<List<GroupCustomer>>>(APICallHelper.AddMemberToGroup, group.AddGroupCustomerCommand);
                    if (response.ApiResponseData != null)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{group.AddGroupCustomerCommand.CustomerIds.Count()} members", MessagesResults.Success,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(group.AddGroupCustomerCommand, false, "", MessagesResults.Failed,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }

                }
                else if (group.Option == "groupprofile")
                {
                    var model = group.Group;
                    var Group = await GetGroup(model.GroupId);
                    if (Group != null)
                    {
                        Group.GroupName = model.GroupName;
                        Group.GroupTypeId = model.GroupTypeId;
                        Group.Active = model.Active;
                        Group.RegistrationNumber = model.RegistrationNumber;
                        Group.TaxPayerNumber = model.TaxPayerNumber;
                        var response = await _customerConfigApiHelper.PutAsync<ServiceResponse<Group>>(string.Format(APICallHelper.Get_Update_Delete_Group, model.GroupId), Group);
                        if (response.ApiResponseData != null)
                        {
                            // Successful creation
                            GetExecutionMessages(response, true, $"{model.GroupName}", MessagesResults.Success,
                                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                            return ExecutionMessage;
                        }
                        else
                        {
                            // Failed creation
                            GetExecutionMessages(model, false, model.GroupName, MessagesResults.Failed,
                                ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                        }
                    }
                }
                else if (group.Option == "adddocument")
                {
                    var doc_file = group.GroupDocument;
                    if (doc_file.AttachedFiles[0] == null)
                    {
                        // Handle case where no files are attached
                        return GetExecutionMessages(doc_file, false, doc_file.DocumentType, MessagesResults.Failed,
                  ExecutionProcessOption.NoFileWasSelected, SystemMessageStatus.Failed.ToString(), null,
                  null);
                    }
                    var additionalParams = new Dictionary<string, string>
                {
                    { "Id", doc_file.GroupId },
                    { "documentType", "GroupDocument" },
                    { "serviceType", "ClientManagement" },
                };
                    var response = await _bankConfigApiHelper.PostFilesAndParamsAsync<ServiceResponse<DocumentUploadResponse>>(APICallHelper.UploadFile, additionalParams, doc_file.AttachedFiles);
                    if (response.ApiResponseData != null)
                    {
                        GetExecutionMessages(response, true, doc_file.DocumentType, MessagesResults.Success,
                            ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                            null);
                        return ExecutionMessage;
                    }
                    GetExecutionMessages(doc_file, false, doc_file.DocumentType, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                        null);
                }
                else if (group.Option == "removemember")
                {

                    var inResponse = await _customerConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.RemoveMemberFromGroup, group.Key));
                    if (inResponse.ApiResponseData != null)
                    {

                        GetExecutionMessages(inResponse, true, $"Member", MessagesResults.Success,
                            ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                        return ExecutionMessage;

                    }
                    else
                    {
                        // Handle failure scenario
                        GetExecutionMessages(null, false, $"Member", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
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

    }

}
