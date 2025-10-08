using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBookQuery;

namespace CBS.BusinessService.Config
{
    public class LossManagementService : BaseService
    {
        private readonly ApiCallerHelper _lossManagementApiHelper;

        public LossManagementService()
        {
            _lossManagementApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"].ToString());
        }

        public async Task<CustomerCheckbooksDto> GetCustomerCheckbooks(string memberRef)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CustomerCheckbooksDto>>(
                    string.Format(APICallHelper.GetCustomerCheckbooks, memberRef));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        {
            try
            {
                var response = await _lossManagementApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetChequeBooksDataTableloss, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

     

        public async Task<CustomDataTable> GetChequeleavesDataTableAsync(CheckLeafQuery query)
        {
            try
            {
                var response = await _lossManagementApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetCheckLeavesByCheckbook, query);

                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }
        public async Task<CheckbookDetailDto> GetCheckLeavesByCheckbook(string KEY)
        {
            return await GetCheckbookDetails(KEY);

            //    try
            //    {
            //        var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckbookDetailDto>>(
            //            string.Format(APICallHelper.GetCheckLeavesByCheckbook, KEY));

            //        return response.IsSuccess ? response.ApiResponseData.Data : null;
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log exception
            //        throw ex;
            //    }
        }

        public async Task<CheckbookDetailDto> GetCheckbookDetails(string KEY)
        {
            try
            {

                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckbookDetailDto>>(
                   string.Format(APICallHelper.GetCheckbookDetails, KEY));
                return response.IsSuccess ? response.ApiResponseData.Data : null;
                //    var response = new ResponseObject<CheckbookDetailDto>(
                //                        data: new CheckbookDetailDto
                //                        {
                //                            CheckBookId = KEY,
                //                            Category = "Personal",
                //                            Status = "Active",
                //                            CustomerId = "0030005128",
                //                            AccountNumber = "703935936432828",
                //                            BranchCode = "003",
                //                            CheckLeaves = new List<CheckLeafDto>
                //                            {
                //                                new CheckLeafDto
                //                                {
                //                                    CheckLeafId = KEY+"001",
                //                                    Status = "Unused",
                //                                    CheckBookId = KEY
                //                                },
                //                                new CheckLeafDto
                //                                {
                //                                    CheckLeafId = KEY+"002",
                //                                    Status = "Issued",
                //                                    CheckBookId = KEY
                //                                },
                //                                new CheckLeafDto
                //                                {
                //                                    CheckLeafId = KEY+"003",
                //                                    Status = "Cleared",
                //                                    CheckBookId = KEY
                //                                }
                //                            }
                //                        },
                //                        statusCode: 200,
                //                        description: "Request processed successfully",
                //                        message: "Checkbook details retrieved",
                //                        status: "Success",
                //                        errors: new List<string>() // no errors
                //                    );


                //    return response.StatusCode==200 ? response.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
            
        }

        public async Task<CheckLeafDetailDto> GetCheckLeafDetails(string KEY)
        {
            try
            {

                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckLeafDetailDto>>(
                   string.Format(APICallHelper.GetCheckbookDetails, KEY));
                //var response = new ResponseObject<CheckbookDetailDto>(
                //                    data: new CheckbookDetailDto
                //                    {
                //                        CheckBookId = KEY,
                //                        Category = "Personal",
                //                        Status = "Active",
                //                        CustomerId = "0030005128",
                //                        AccountNumber = "703935936432828",
                //                        BranchCode = "003",
                //                        CheckLeaves = new List<CheckLeafDto>
                //                        {
                //                            new CheckLeafDto
                //                            {
                //                                CheckLeafId = KEY+"001",
                //                                Status = "Unused",
                //                                CheckBookId = KEY
                //                            },
                //                            new CheckLeafDto
                //                            {
                //                                CheckLeafId = KEY+"002",
                //                                Status = "Issued",
                //                                CheckBookId = KEY
                //                            },
                //                            new CheckLeafDto
                //                            {
                //                                CheckLeafId = KEY+"003",
                //                                Status = "Cleared",
                //                                CheckBookId = KEY
                //                            }
                //                        }
                //                    },
                //                    statusCode: 200,
                //                    description: "Request processed successfully",
                //                    message: "Checkbook details retrieved",
                //                    status: "Success",
                //                    errors: new List<string>() // no errors
                //                );


                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<LossRequestDto> RequestLossForCheckbook(string KEY)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
                    string.Format(APICallHelper.RequestLossForCheckbook, KEY));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // log error
                throw;
            }
        }


        public async Task<LossRequestDto> RequestLossForCheckLeaf(string KEY)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
                    string.Format(APICallHelper.RequestLossForCheckLeaf, KEY));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<ExecutionMessages> SubmitLossRequest(LossRequestDto request)
        {
            try
            {
                var response = await _lossManagementApiHelper.PostAsync<ServiceResponse<LossRequestDto>>(
                    APICallHelper.SubmitLossRequest, request);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Loss Request", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(request, false, "Loss Request", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
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