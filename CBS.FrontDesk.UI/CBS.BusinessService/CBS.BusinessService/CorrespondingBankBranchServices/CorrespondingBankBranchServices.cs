using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.ThirdPartyBankAccount;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Services
{
  
    public class CorrespondingBankBranchServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;
        private readonly ApiCallerHelper _systemConfigApiHelper;

        public CorrespondingBankBranchServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _systemConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["SystemConfigurationBaseUrl"].ToString());
        }
        public async Task<List<System.Web.WebPages.Html.SelectListItem>> GetBanktypeAsync()
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "MICROFINANCE", Value = "MICROFINANCE" });
            selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = "BANK", Value = "BANK" });
            return selectListItems;
        }

     
        public async Task<List<StringValues>> GetValueOption(string switch_on, string Id = "0")
        {
            List<StringValues> selectListItems = new List<StringValues>();
            var LocationDto = await this.GetLocationInfo();
            switch (switch_on)
            {
                case "REGION":
                    {

                        foreach (var item in LocationDto.Regions)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "DIVISION":
                    {
                        var modelList = LocationDto.Divisions.Where(x => x.RegionId == Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "SUBDIVISION":
                    {
                        var modelList = LocationDto.Subdivisions.Where(x => x.DivisionId== Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "TOWN":
                    {
                        var modelList = LocationDto.Towns.Where(x => x.SubdivisionId == Id).ToList();
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;


                default: break;
            }

            return selectListItems;
        }
        public async Task<List<StringValues>> GetLocationValueOption(string switch_on, string Id = "0")
        {
            switch_on = Id;
            List<StringValues> selectListItems = new List<StringValues>();
            var LocationDto = await this.GetLocationInfo();
            switch (switch_on)
            {
                case "REGION":
                    {

                        foreach (var item in LocationDto.Regions)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "DIVISION":
                    {
                        var modelList = LocationDto.Divisions;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "SUBDIVISION":
                    {
                        var modelList = LocationDto.Subdivisions;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;
                case "TOWN":
                    {
                        var modelList = LocationDto.Towns;
                        foreach (var item in modelList)
                        {
                            selectListItems.Add(new StringValues { Text = item.Id, Value = item.Name });
                        }
                    }
                    break;


                default: break;
            }

            return selectListItems;
        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objCorrespondingBankBranch = await GetCorrespondingBankBranch(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_CorrespondingBankBranche, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCorrespondingBankBranch.BranchName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCorrespondingBankBranch, false, $"{objCorrespondingBankBranch.BranchName}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        {
            Func<Task<List<CorrespondingBankBranch>>> getDataFunc = async () => (await GetCorrespondingBankBranch()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<CorrespondingBankBranch>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<List<CorrespondingBankBranch>> GetCorrespondingBankBranch()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<CorrespondingBankBranch>>>(APICallHelper.GetAllCorrespondingBankBranche);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<CorrespondingBankBranch>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LocationDto> GetLocationInfo()
        {
            try
            {
                var couApiResponse = await _systemConfigApiHelper.GetAsync<ResponseObject<LocationDto>>(APICallHelper.GetlocationInforUrl);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new LocationDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CorrespondingBankBranch> GetCorrespondingBankBranch(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<CorrespondingBankBranch>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingBankBranche, id));
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

        public async Task<List<CorrespondingBankBranch>> GetCorrespondingBankBranchByBankId(string id)
        {
            try
            {
                var dataList = await GetCorrespondingBankBranch();
                return dataList.Where(x=>x.ThirdPartyInstitutionId==id).ToList();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
  
        public async Task<ExecutionMessages> Create(CorrespondingBankBranch model)
        {
            try
            {
                model.TownId = model.SubdivisionId;
                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<CorrespondingBankBranch>>(APICallHelper.CreateCorrespondingBankBranche, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.BranchName}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.BranchName, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(CorrespondingBankBranch model)
        {
            try
            {

                var CorrespondingBankBranch = await GetCorrespondingBankBranch(model.Id);
                if (CorrespondingBankBranch != null)
                {
                    CorrespondingBankBranch.BranchName = model.BranchName;
     
                    var response = await _bankConfigApiHelper.PutAsync<ServiceResponse<CorrespondingBankBranch>>(string.Format(APICallHelper.Get_Update_Delete_CorrespondingBankBranche, model.Id), CorrespondingBankBranch);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.BranchName}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.BranchName, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
