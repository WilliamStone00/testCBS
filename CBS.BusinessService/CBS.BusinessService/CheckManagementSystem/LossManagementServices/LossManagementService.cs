//using BusinessServices;
//using CBS.API.Helper;
//using CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
//using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
//using CBS.FrontDesk.Data.Entity.DataTable;
//using CBS.FrontDesk.Data.Entity.ManualDailycollection;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.Helper;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Mvc;
//using static CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing.ChequeBookQuery;

//namespace CBS.BusinessService.Config
//{
//    public class LossManagementService : BaseService
//    {
//        private readonly ApiCallerHelper _lossManagementApiHelper;

//        public LossManagementService()
//        {
//            _lossManagementApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"].ToString());
//        }

//        public async Task<CustomerCheckbooksDto> GetCustomerCheckbooks(string memberRef)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CustomerCheckbooksDto>>(
//                    string.Format(APICallHelper.GetCustomerCheckbooks, memberRef));

//                if (!response.IsSuccess)
//                {
//                    GetExecutionMessages(null, false, "Customer Checkbooks", MessagesResults.Failed,
//                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
//                    return null;
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, "Customer Checkbooks", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
//                return null;
//            }
//        }

//        public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
//                    APICallHelper.GetChequeBooksDataTableloss, query);

//                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
//                if (!response.IsSuccess)
//                {
//                    throw new Exception($"API call failed: {response.Message}");
//                }

//                if (response.ApiResponseData == null)
//                {
//                    throw new Exception("API returned null data");
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                // Log the original exception
//                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

//                // Re-throw to trigger fallback
//                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
//            }
//        }

     

//        public async Task<CustomDataTable> GetChequeleavesDataTableAsync(CheckLeafQuery query)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
//                    APICallHelper.GetCheckLeavesByCheckbook, query);

//                // ⚠️ CRITICAL: If API call fails or returns unsuccessful, THROW exception
//                if (!response.IsSuccess)
//                {
//                    throw new Exception($"API call failed: {response.Message}");
//                }

//                if (response.ApiResponseData == null)
//                {
//                    throw new Exception("API returned null data");
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                // Log the original exception
//                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

//                // Re-throw to trigger fallback
//                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
//            }
//        }
//        public async Task<List<CheckLeafDto>> GetCheckLeavesByCheckbookId(string checkbookId)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<List<CheckLeafDto>>>(
//                    string.Format(APICallHelper.GetCheckLeavesByCheckbook, checkbookId));

//                if (response == null || !response.IsSuccess)
//                    return null;

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                // You can log or rethrow depending on your app’s conventions
//                GetExecutionMessages(null, false, "GetCheckLeavesByCheckbookId", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
//                return null;
//            }
//        }


//        public async Task<CheckbookDetailDto> GetCheckbookDetails(string KEY)
//        {
//            try
//            {
//                // ✅ Expecting a single object with nested list
//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckbookDetailDto>>(
//                    string.Format(APICallHelper.GetCheckbookDetails, KEY));

//                if (!response.IsSuccess)
//                {
//                    GetExecutionMessages(
//                        null,
//                        false,
//                        "Checkbook Details",
//                        MessagesResults.Failed,
//                        ExecutionProcessOption.DefaultFailedMessages,
//                        SystemMessageStatus.Failed.ToString(),
//                        null,
//                        response.Message
//                    );
//                    return null;
//                }

//                // ✅ The "data" property contains the single object
//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(
//                    null,
//                    false,
//                    "Checkbook Details",
//                    MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch,
//                    SystemMessageStatus.Failed.ToString(),
//                    ex
//                );
//                return null;
//            }
//        }


//        //return response.IsSuccess ? response.ApiResponseData.Data : null;
//        //    var response = new ResponseObject<CheckbookDetailDto>(
//        //                        data: new CheckbookDetailDto
//        //                        {
//        //                            CheckBookId = KEY,
//        //                            Category = "Personal",
//        //                            Status = "Active",
//        //                            CustomerId = "0030005128",
//        //                            AccountNumber = "703935936432828",
//        //                            BranchCode = "003",
//        //                            CheckLeaves = new List<CheckLeafDto>
//        //                            {
//        //                                new CheckLeafDto
//        //                                {
//        //                                    CheckLeafId = KEY+"001",
//        //                                    Status = "Unused",
//        //                                    CheckBookId = KEY
//        //                                },
//        //                                new CheckLeafDto
//        //                                {
//        //                                    CheckLeafId = KEY+"002",
//        //                                    Status = "Issued",
//        //                                    CheckBookId = KEY
//        //                                },
//        //                                new CheckLeafDto
//        //                                {
//        //                                    CheckLeafId = KEY+"003",
//        //                                    Status = "Cleared",
//        //                                    CheckBookId = KEY
//        //                                }
//        //                            }
//        //                        },
//        //                        statusCode: 200,
//        //                        description: "Request processed successfully",
//        //                        message: "Checkbook details retrieved",
//        //                        status: "Success",
//        //                        errors: new List<string>() // no errors
//        //                    );


//        //    return response.StatusCode==200 ? response.Data : null;
//        //}
//        //catch (Exception ex)
//        //{
//        //    // Log exception
//        //    throw ex;
//        //}



//        public async Task<CheckLeafDetailDto> GetCheckLeafDetails(string KEY)
//        {
//            try
//            {

//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckLeafDetailDto>>(
//                   string.Format(APICallHelper.GetCheckbookDetails, KEY));

//                //var response = new ResponseObject<CheckbookDetailDto>(
//                //                    data: new CheckbookDetailDto
//                //                    {
//                //                        CheckBookId = KEY,
//                //                        Category = "Personal",
//                //                        Status = "Active",
//                //                        CustomerId = "0030005128",
//                //                        AccountNumber = "703935936432828",
//                //                        BranchCode = "003",
//                //                        CheckLeaves = new List<CheckLeafDto>
//                //                        {
//                //                            new CheckLeafDto
//                //                            {
//                //                                CheckLeafId = KEY+"001",
//                //                                Status = "Unused",
//                //                                CheckBookId = KEY
//                //                            },
//                //                            new CheckLeafDto
//                //                            {
//                //                                CheckLeafId = KEY+"002",
//                //                                Status = "Issued",
//                //                                CheckBookId = KEY
//                //                            },
//                //                            new CheckLeafDto
//                //                            {
//                //                                CheckLeafId = KEY+"003",
//                //                                Status = "Cleared",
//                //                                CheckBookId = KEY
//                //                            }
//                //                        }
//                //                    },
//                //                    statusCode: 200,
//                //                    description: "Request processed successfully",
//                //                    message: "Checkbook details retrieved",
//                //                    status: "Success",
//                //                    errors: new List<string>() // no errors
//                //                );


//                if (!response.IsSuccess)
//                {
//                    // Use consistent error handling
//                    GetExecutionMessages(null, false, "Check Leaf Details", MessagesResults.Failed,
//                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
//                    return null;
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, "Check Leaf Details", MessagesResults.Error, ExecutionProcessOption.TryCatch,
//                    SystemMessageStatus.Failed.ToString(), ex);
//                return null;
//            }
//        }

//        public async Task<LossRequestDto> RequestLossForCheckbook(string KEY)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
//                    string.Format(APICallHelper.RequestLossForCheckbook, KEY));

//                if (!response.IsSuccess)
//                {
//                    GetExecutionMessages(null, false, "Loss Request Checkbook", MessagesResults.Failed,
//                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
//                    return null;
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, "Loss Request Checkbook", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
//                return null;
//            }
//        }



//        public async Task<LossRequestDto> RequestLossForCheckLeaf(string KEY)
//        {
//            try
//            {
//                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
//                    string.Format(APICallHelper.RequestLossForCheckLeaf, KEY));

//                if (!response.IsSuccess)
//                {
//                    GetExecutionMessages(null, false, "Loss Request Check Leaf", MessagesResults.Failed,
//                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
//                    return null;
//                }

//                return response.ApiResponseData.Data;
//            }
//            catch (Exception ex)
//            {
//                GetExecutionMessages(null, false, "Loss Request Check Leaf", MessagesResults.Error,
//                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
//                return null;
//            }
//        }

////        public async Task<LossRequestDto> GetLossRequestDetails(string key)
////        {
////            var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
////                $"{APICallHelper.LossRequestDetails}?key={key}"
////            );

////            if (response.IsSuccess && response.ApiResponseData != null)
////            {
////                return response.ApiResponseData.Data;
////            }

////            return null;
////        }

////        public SubmitLossRequestCommand ConvertToLossRequestCommand(LossRequestDto dto)
////        {
////            return new SubmitLossRequestCommand
////            {
////                CheckBookID = dto.CheckBookID,
////                CheckLeafID = dto.CheckLeafID,
////                CustomerID = dto.CustomerID,
////                BranchID = dto.BranchID,
////                LossReason = dto.LossReason,
////                LossDate = dto.LossDate,
////                LossLocation = dto.LossLocation,
////                Description = dto.Description,
////                ClientSuggestion = dto.ClientSuggestion,
////                LossReportedBy = dto.LossReportedBy,
////                ThirdPartyName = dto.ThirdPartyName,
////                ThirdPartyNationalID = dto.ThirdPartyNationalID,
////                ThirdPartyIDExpiry = dto.ThirdPartyIDExpiry,
////                ThirdPartyDeliveryDate = dto.ThirdPartyDeliveryDate,
////                ThirdPartyIssueLocation = dto.ThirdPartyIssueLocation,
////                RequestedBy = GetUserFullName(), // Service handles user context
////                CreatedDate = DateTime.Now
////            };
////        }

////        public async Task<ExecutionMessages> SubmitLossRequestAsync(SubmitLossRequestCommand command)
////        {
////            try
////            {
////                var response = await _lossManagementApiHelper.PostAsync<ServiceResponse<LossRequestDto>>(
////                    APICallHelper.SubmitLossRequest, command);

////                if (response.IsSuccess)
////                {
////                    GetExecutionMessages(response, true, "Loss Request", MessagesResults.Success,
////                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
////                }
////                else
////                {
////                    GetExecutionMessages(command, false, "Loss Request", MessagesResults.Failed,
////                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
////                }

////                return ExecutionMessage;
////            }
////            catch (Exception ex)
////            {
////                GetExecutionMessages(null, false, "Loss Request", MessagesResults.Error,
////                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
////                return ExecutionMessage;
////            }
////        }

////        public async Task<ExecutionMessages> ApproveLossRequestAsync(ApproveLossRequestCommand command)
////        {
////            try
////            {
////                command.ApprovedBy = GetUserFullName();
////                var response = await _lossManagementApiHelper.PostAsync<ServiceResponse<bool>>(
////                    APICallHelper.ApproveLossRequest, command);

////                if (response.IsSuccess)
////                {
////                    GetExecutionMessages(response, true, "Loss Request Approval", MessagesResults.Success,
////                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
////                }
////                else
////                {
////                    GetExecutionMessages(command, false, "Loss Request Approval", MessagesResults.Failed,
////                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
////                }

////                return ExecutionMessage;
////            }
////            catch (Exception ex)
////            {
////                GetExecutionMessages(command, false, "Loss Request Approval", MessagesResults.Error,
////                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
////                return ExecutionMessage;
////            }
////        }

////        // NEW: Rejection method following Member Adjustment pattern
////        public async Task<ExecutionMessages> RejectLossRequestAsync(RejectLossRequestCommand command)
////        {
////            try
////            {
////                command.RejectedBy = GetUserFullName();
////                var response = await _lossManagementApiHelper.PostAsync<ServiceResponse<bool>>(
////                    APICallHelper.RejectLossRequest, command);

////                if (response.IsSuccess)
////                {
////                    GetExecutionMessages(response, true, "Loss Request Rejection", MessagesResults.Success,
////                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
////                }
////                else
////                {
////                    GetExecutionMessages(command, false, "Loss Request Rejection", MessagesResults.Failed,
////                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
////                }

////                return ExecutionMessage;
////            }
////            catch (Exception ex)
////            {
////                GetExecutionMessages(command, false, "Loss Request Rejection", MessagesResults.Error,
////                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
////                return ExecutionMessage;
////            }
////        }

////        // NEW: DataTable method following consistent pattern
////        public async Task<CustomDataTable> GetLossRequestsDataTableAsync(LossRequestQuery query)
////        {
////            try
////            {
////                if (!IsHeadOffice())
////                {
////                    query.BranchId = GetBranchID();
////                }

////                var response = await _lossManagementApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
////                    APICallHelper.LossRequestDataTable, query);

////                if (response.IsSuccess && response.ApiResponseData != null)
////                {
////                    return response.ApiResponseData.Data;
////                }

////                return new CustomDataTable(
////                    draw: Convert.ToInt32(query.Options.draw),
////                    recordsTotal: 0,
////                    recordsFiltered: 0,
////                    data: new List<object>(),
////                    dataTableOptions: query.Options
////                );
////            }
////            catch (Exception ex)
////            {
////                System.Diagnostics.Debug.WriteLine($"Loss Request DataTable Error: {ex.Message}");
////                throw new Exception($"Loss request service unavailable: {ex.Message}", ex);
////            }
////        }
////    }

//}
