using CBS.API.Helper;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
  
    public class ChartOfAccountManagementPositionService : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get; private set; }
        public string BankId { get; private set; }
        public string OrganizationId { get; private set; }
        private AccountingServices _accountingServices { get; set; }
        private ChartOfAccountServices _chartOfAccountServices { get;  set; }
        public ChartOfAccountManagementPositionService()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            //this.BranchId = GetBranchID();
            //this.BankId = GetBankID();
            //this.OrganizationId = GetOrganizationID();
            _chartOfAccountServices = new ChartOfAccountServices();
            _accountingServices = new AccountingServices();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objAccountCategory = await GetChartOfAccountManagementPosition(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccountManagementPosition, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objAccountCategory.Description}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objAccountCategory, false, $"{objAccountCategory.Description}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<ChartofAccountManagementPosition>> DownloadChartOfAccount()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<ChartofAccountManagementPosition>>>(APICallHelper.DownloadChartOfAccountManagementPositionUrl);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<ChartofAccountManagementPosition>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<ChartofAccountManagementPosition>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<List<ProductAccountingBook>> GetProductAccountingBook(string id)
        {
            try
            {

                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<ProductAccountingBook>>>(string.Format(APICallHelper.GetProductAccountingBookUrl, id));// await _savingConfigApiHelper.GetAsync<ResponseObject<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
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
        public async Task<List<AccountProduct>> GetProductAccountingBookByproductname(string name)
        {
            try
            {

                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<AccountProduct>>>(string.Format(APICallHelper.GetProductAccountingBookbyproductnameUrl,name));// await _savingConfigApiHelper.GetAsync<ResponseObject<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
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
        public async Task<List<ProductAccountingChart>> GetProductAccountingBookByproducttype(string productType)
        {
            try
            {

                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<ProductAccountingChart>>>(string.Format(APICallHelper.GetProductAccountingBookbyproductTypeUrl, productType));// await _savingConfigApiHelper.GetAsync<ResponseObject<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
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
        public async Task<IEnumerable<ChartofAccountManagementPosition>> GetChartOfAccountManagementPositions()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<ChartofAccountManagementPosition>>>(APICallHelper.GetAllChartOfAccountManagementPosition);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<ChartofAccountManagementPosition>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<ChartofAccountManagementPosition>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<ChartofAccountManagementPosition> GetChartOfAccountManagementPosition(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<ChartofAccountManagementPosition>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccountManagementPosition, id));
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

        public async Task<ExecutionMessages> Update(ChartofAccountManagementPosition model)
        {
            try
            {

                var AccountCategory = await _chartOfAccountServices.GetChartOfAccountById(model.ChartOfAccountId);
                if (AccountCategory != null)
                {
                    model.RootDescription = AccountCategory.LabelEn;

                 
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<ChartofAccountManagementPosition>>(string.Format(APICallHelper.Get_Update_Delete_ChartOfAccountManagementPosition, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Description}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.Description, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Create(ChartofAccountManagementPosition model)
        {
            try
            {

                // Make an API call to create an individual profile

                var models = await _chartOfAccountServices.GetChartOfAccountById(model.ChartOfAccountId);
                model.RootDescription = models.LabelEn;
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<ChartofAccountManagementPosition>>(APICallHelper.CreateChartOfAccountManagementPosition, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Description}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Description, MessagesResults.Failed,
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


        public async Task<List<ChartofAccountManagementPosition>> Get_ChartOfAccountManagementPosition(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<ChartofAccountManagementPosition>>>(string.Format(APICallHelper.Get_ChartOfAccountManagementPosition, id));
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

        public async Task<CBS.FrontDesk.Data.Account> GetChartOfAccountManagementPositionServiceByIdandBranchIDAsync(string mFI_ChartOfAccountId, string AccountOwnerId)
        {
            CBS.FrontDesk.Data.Account account = new CBS.FrontDesk.Data.Account();
            var modelist =await _accountingServices.GetAllAccountForABranch(AccountOwnerId);
            if (modelist == null)
            {
                throw new ArgumentNullException($"There is no branch account created for this branch:{_accountingServices.GetBranchName()}");
            }
            else
            {
               var list = modelist.Where(pi=>pi.ChartOfAccountManagementPositionId == mFI_ChartOfAccountId);

                if (list.Any()) 
                { 
                   account = list.First();
                }
                else
                {
                    var modell = await this.GetChartOfAccountManagementPosition(mFI_ChartOfAccountId);
                    var model = new CBS.FrontDesk.Data.Account
                    {
                        AccountNumberManagementPosition = modell.PositionNumber,
                        AccountName = modell.Description + " " + GetBranchName(),// .BranchName,
                        AccountNumber = modell.AccountNumber,

                        AccountNumberNetwok = (modell.AccountNumber.PadRight(6, '0') + modell.PositionNumber.PadRight(3, '0') + GetBranchCode() + GetBranchCode()).PadRight(6, '0'),
                        AccountNumberCU = (modell.AccountNumber.PadRight(6, '0') + modell.PositionNumber.PadRight(3, '0') + GetBranchCode()).PadRight(9, '0'),
                        AccountCategoryId = "XXX",
                        AccountTypeId = "",
                        AccountOwnerId = GetBranchID(),
                        ChartOfAccountManagementPositionId = modell.Id,
                        BranchCode = GetBranchCode(),
                        IsNormalCreation = false,

                    };
                    account=(CBS.FrontDesk.Data.Account) (await  _accountingServices.Create(model)).Data;
                    //throw new ArgumentNullException($"There is no account {(await this.GetChartOfAccountManagementPosition(mFI_ChartOfAccountId)).AccountNumber} present in the system for {_accountingServices.GetBranchName()}");
                }
                return account;
            }
        }

        public async Task<IExecutionMessages> Update(string id, string category)
        {
         
            try
            {
                var Id = id.Split('-')[0];

                var mfi_chart = await this.GetChartOfAccountManagementPosition(Id);
              
                if (mfi_chart != null)
                {
                   var model= await _chartOfAccountServices.GetChartOfAccountById(mfi_chart.ChartOfAccountId);
                    model.AccountCartegoryId = category;

                   return  await _chartOfAccountServices.UpdateAccountCategory(model);
                }
                else
                {
                    return new ExecutionMessages
                    {
                        MessagesResults = 0,
                        MessageString = "Excution failed for " + id,
                        MessageStatus =MessagesResults.NoteFound.ToString(),
                        Data= null
                       
                    }; 
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
