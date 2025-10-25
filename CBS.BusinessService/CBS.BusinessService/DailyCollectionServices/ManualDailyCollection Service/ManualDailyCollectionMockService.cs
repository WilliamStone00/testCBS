//using BusinessServices;
//using CBS.API.Helper; // You might need this for ApiResponse
//using CBS.FrontDesk.Data.Entity.ManualDailycollection;
//using CBS.FrontDesk.Data.Message;
//using CBS.FrontDesk.Helper;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;

//// IMPORTANT: Make sure this class implements the full interface
//namespace CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service
//{
//    public class ManualDailyCollectionMockService : BaseService
//    {
//        // Our in-memory "database" of uploaded files
//        private static readonly List<FileUploadResponse> _mockFiles = new List<FileUploadResponse>
//        {
//            new FileUploadResponse
//            {
//                //id = "mock-file-1",
//                BranchId = "BR001",
//                CollectorId = "COL100",
//                FileUploadId = "UPLOAD001",
//                TotalAmount = 5000,
//                TotalMembers = 3,
//                UploadedBy = "John Doe",
//                FileDetails = new List<ManualDailyCollectionFileDetail>
//                {
//                    new ManualDailyCollectionFileDetail { MemberReference="M001", AccountNumber="12345", MemberName="Alice", Amount=2000 },
//                    new ManualDailyCollectionFileDetail { MemberReference="M002", AccountNumber="67890", MemberName="Bob", Amount=1500 },
//                    new ManualDailyCollectionFileDetail { MemberReference="M003", AccountNumber="54321", MemberName="Charlie", Amount=1500 },
//                }
//            },
//            new FileUploadResponse
//            {
//                //id = "mock-file-2",
//                BranchId = "BR002",
//                CollectorId = "COL200",
//                FileUploadId = "UPLOAD002",
//                TotalAmount = 12500,
//                TotalMembers = 2,
//                UploadedBy = "Jane Smith",
//                FileDetails = new List<ManualDailyCollectionFileDetail>
//                {
//                    new ManualDailyCollectionFileDetail { MemberReference="M004", AccountNumber="11223", MemberName="David", Amount=7500 },
//                    new ManualDailyCollectionFileDetail { MemberReference="M005", AccountNumber="44556", MemberName="Eve", Amount=5000 },
//                }
//            }
//        };

//        public Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadManualEntryFileAsync(HttpPostedFileBase file)
//        {
//            var fakeResponse = new FileUploadResponse
//            {
//                //id = Guid.NewGuid().ToString(),
//                BranchId = "BR002",
//                CollectorId = "COL200",
//                FileUploadId = "UPLOAD" + DateTime.Now.Ticks,
//                TotalAmount = 10000,
//                TotalMembers = 2,
//                UploadedBy = "Mock Collector",
//                FileDetails = new List<ManualDailyCollectionFileDetail>
//                {
//                    new ManualDailyCollectionFileDetail { MemberReference="M010", AccountNumber="11111", MemberName="Mock Member1", Amount=6000 },
//                    new ManualDailyCollectionFileDetail { MemberReference="M011", AccountNumber="22222", MemberName="Mock Member2", Amount=4000 }
//                }
//            };

//            _mockFiles.Add(fakeResponse);

//            return Task.FromResult(new ApiResponse<ServiceResponse<FileUploadResponse>>
//            {
//                IsSuccess = true,
//                ApiResponseData = ServiceResponse<FileUploadResponse>.ReturnResultWith200(fakeResponse, "Mock upload successful")
//            });
//        }

//        public Task<FileUploadResponse> GetFileDetailsByIdAsync(string fileUploadId)
//        {
//            var file = _mockFiles.FirstOrDefault(f => f.FileUploadId == fileUploadId);
//            return Task.FromResult(file);
//        }

//        public Task<List<FileUploadResponse>> GetFilesByProcessingStatusAsync(string processingStatus)
//        {
//            // For now, we ignore the Status and return all files. 
//            // In a more advanced mock, you could add a "Status" property to FileUploadResponse and filter by it here.
//            return Task.FromResult(_mockFiles.ToList());
//        }

//        // --- NEW METHOD ADDED HERE ---
//        /// <summary>
//        /// MOCK: Returns all files currently in our in-memory list.
//        /// This is what the controller's LoadAllFiles action will call.
//        /// </summary>
//        public Task<List<FileUploadResponse>> GetAllFilesAsync()
//        {
//            // Simulate an async operation and return a copy of the list.
//            return Task.FromResult(_mockFiles.ToList());
//        }
//        // --- END OF NEW METHOD ---

//        public Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId)
//        {
//            var file = _mockFiles.FirstOrDefault(f => f.FileUploadId == fileUploadId);
//            if (file != null)
//            {
//                _mockFiles.Remove(file);
//                GetExecutionMessages(null, true, fileUploadId, MessagesResults.Success,
//                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(),
//                    null, "Mock deleted successfully.");
//            }
//            else
//            {
//                GetExecutionMessages(null, false, fileUploadId, MessagesResults.Failed,
//                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(),
//                    null, "File not found in mock store.");
//            }

//            return Task.FromResult(ExecutionMessage);
//        }

       
//    }
//}