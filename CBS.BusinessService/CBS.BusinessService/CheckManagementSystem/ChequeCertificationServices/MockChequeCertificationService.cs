using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeCertification;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.CheckManagementSystem.ChequeCertification
{
    /// <summary>
    /// In-memory mock implementation of ChequeCertificationService for UI testing.
    /// </summary>
    public class MockChequeCertificationService : BaseService
    {
        
        private static readonly List<ChequeCertificationDto> _mockData = new List<ChequeCertificationDto>
        {
            new ChequeCertificationDto
            {
                ChequeCertificationID = "CC-0001",
                MemberReference = "MEM-0001",
                BranchId = "BR001",
                ChequeBookId = "CB-100",
                ChequeleafId = "L-01",
                AccountNumber = "ACC-1000",
                Amount = 1250.00m,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                CertificationStatus = "Pending",
                Description = "Member requested certification for cheque issued on 2025-08-20"
            },
            new ChequeCertificationDto
            {
                ChequeCertificationID = "CC-0002",
                MemberReference = "MEM-0002",
                BranchId = "BR002",
                ChequeBookId = "CB-200",
                ChequeleafId = "L-09",
                AccountNumber = "ACC-2000",
                Amount = 5000.00m,
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                CertificationStatus = "Reviewed",
                Description = "Under review: large amount"
            },
            new ChequeCertificationDto
            {
                ChequeCertificationID = "CC-0003",
                MemberReference = "MEM-0003",
                BranchId = "BR001",
                ChequeBookId = "CB-300",
                ChequeleafId = "L-03",
                AccountNumber = "ACC-3000",
                Amount = 250.50m,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                CertificationStatus = "Approved",
                Description = "Previously validated"
            },
            new ChequeCertificationDto
            {
                ChequeCertificationID = "CC-0004",
                MemberReference = "MEM-0004",
                BranchId = "BR003",
                ChequeBookId = "CB-400",
                ChequeleafId = "L-11",
                AccountNumber = "ACC-4000",
                Amount = 999.99m,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                CertificationStatus = "Rejected",
                Description = "Insufficient funds"
            }
        };

        private static readonly object _lock = new object();

        // Return all certifications
        public Task<IEnumerable<ChequeCertificationDto>> GetAllAsync()
        {
            lock (_lock)
            {
                // return a copy to avoid accidental modifications from consumers
                var copy = _mockData.Select(x => Clone(x)).ToList().AsEnumerable();
                return Task.FromResult(copy);
            }
        }

        // Get single by ChequeCertificationID
        public Task<ChequeCertificationDto> GetByIdAsync(string chequeCertificationID)
        {
            if (string.IsNullOrWhiteSpace(chequeCertificationID))
                return Task.FromResult<ChequeCertificationDto>(null);

            lock (_lock)
            {
                var found = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, chequeCertificationID, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(found != null ? Clone(found) : null);
            }
        }

        // Create a new certification
        public Task<ExecutionMessages> CreateAsync(ChequeCertificationDto model)
        {
            try
            {
                if (model == null)
                {
                    GetExecutionMessages(null, false, "Create", MessagesResults.Failed, ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, "Model is null");
                    return Task.FromResult(ExecutionMessage);
                }

                lock (_lock)
                {
                    // Ensure unique ChequeCertificationID
                    if (string.IsNullOrWhiteSpace(model.ChequeCertificationID))
                    {
                        model.ChequeCertificationID = "CC-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    }
                    else
                    {
                        // avoid duplicates by ID
                        var exists = _mockData.Any(x => string.Equals(x.ChequeCertificationID, model.ChequeCertificationID, StringComparison.OrdinalIgnoreCase));
                        if (exists)
                        {
                            GetExecutionMessages(model, false, model.ChequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, "ChequeCertificationID already exists.");
                            return Task.FromResult(ExecutionMessage);
                        }
                    }

                    // Optionally ensure MemberReference exists; if missing assign a synthetic member reference
                    if (string.IsNullOrWhiteSpace(model.MemberReference))
                    {
                        model.MemberReference = "MEM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                    }

                    model.CreatedDate = DateTime.UtcNow;
                    model.CertificationStatus = string.IsNullOrWhiteSpace(model.CertificationStatus) ? "Pending" : model.CertificationStatus;
                    _mockData.Add(Clone(model));
                }

                GetExecutionMessages(model, true, model.ChequeCertificationID, MessagesResults.Success, ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "Created");
                return Task.FromResult(ExecutionMessage);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.ChequeCertificationID ?? "Create", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                return Task.FromResult(ExecutionMessage);
            }
        }

        // Update an existing certification (matched by ChequeCertificationID)
        public Task<ExecutionMessages> UpdateAsync(ChequeCertificationDto model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.ChequeCertificationID))
                {
                    GetExecutionMessages(model, false, "Update", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or ChequeCertificationID.");
                    return Task.FromResult(ExecutionMessage);
                }

                lock (_lock)
                {
                    var existing = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, model.ChequeCertificationID, StringComparison.OrdinalIgnoreCase));
                    if (existing == null)
                    {
                        GetExecutionMessages(model, false, model.ChequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                        return Task.FromResult(ExecutionMessage);
                    }

                    // update fields (keep CreatedDate)
                    existing.MemberReference = model.MemberReference;
                    existing.BranchId = model.BranchId;
                    existing.ChequeBookId = model.ChequeBookId;
                    existing.ChequeleafId = model.ChequeleafId;
                    existing.AccountNumber = model.AccountNumber;
                    existing.Amount = model.Amount;
                    existing.Description = model.Description;
                    // Allow manual status update if provided (but typically status flows through Review/Validate/Reject)
                    if (!string.IsNullOrWhiteSpace(model.CertificationStatus))
                        existing.CertificationStatus = model.CertificationStatus;
                }

                GetExecutionMessages(model, true, model.ChequeCertificationID, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Updated");
                return Task.FromResult(ExecutionMessage);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, model?.ChequeCertificationID ?? "Update", MessagesResults.Error, ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
                return Task.FromResult(ExecutionMessage);
            }
        }

        // Mark as Reviewed (by ChequeCertificationID)
        public Task<ExecutionMessages> ReviewAsync(string chequeCertificationID)
        {
            if (string.IsNullOrWhiteSpace(chequeCertificationID))
            {
                GetExecutionMessages(null, false, "Review", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid ChequeCertificationID.");
                return Task.FromResult(ExecutionMessage);
            }

            lock (_lock)
            {
                var item = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, chequeCertificationID, StringComparison.OrdinalIgnoreCase));
                if (item == null)
                {
                    GetExecutionMessages(null, false, chequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    return Task.FromResult(ExecutionMessage);
                }

                item.CertificationStatus = "Reviewed";
                GetExecutionMessages(item, true, chequeCertificationID, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Marked as Reviewed.");
                return Task.FromResult(ExecutionMessage);
            }
        }

       

        private IEnumerable<ChequeCertificationDto> ApplyFilters(
            IEnumerable<ChequeCertificationDto> data,
            GetChequeCertificationsDataTableQuery query)
        {
            var result = data.AsQueryable();

            if (!string.IsNullOrEmpty(query.MemberReference))
                result = result.Where(x => x.MemberReference.Contains(query.MemberReference));

            if (!string.IsNullOrEmpty(query.ChequeCertificationID))
                result = result.Where(x => x.ChequeCertificationID.Contains(query.ChequeCertificationID));

            if (!string.IsNullOrEmpty(query.CertificationStatus))
                result = result.Where(x => x.CertificationStatus == query.CertificationStatus);

            // Add more filters as needed...

            return result.ToList();
        }

        // Approve/Validate certification (by ChequeCertificationID)
        public Task<ExecutionMessages> ValidateAsync(string chequeCertificationID)
        {
            if (string.IsNullOrWhiteSpace(chequeCertificationID))
            {
                GetExecutionMessages(null, false, "Validate", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid ChequeCertificationID.");
                return Task.FromResult(ExecutionMessage);
            }

            lock (_lock)
            {
                var item = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, chequeCertificationID, StringComparison.OrdinalIgnoreCase));
                if (item == null)
                {
                    GetExecutionMessages(null, false, chequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    return Task.FromResult(ExecutionMessage);
                }

                item.CertificationStatus = "Approved";
                GetExecutionMessages(item, true, chequeCertificationID, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Validated/Approved.");
                return Task.FromResult(ExecutionMessage);
            }
        }

        // Reject certification (by ChequeCertificationID)
        public Task<ExecutionMessages> RejectAsync(string chequeCertificationID)
        {
            if (string.IsNullOrWhiteSpace(chequeCertificationID))
            {
                GetExecutionMessages(null, false, "Reject", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid ChequeCertificationID.");
                return Task.FromResult(ExecutionMessage);
            }

            lock (_lock)
            {
                var item = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, chequeCertificationID, StringComparison.OrdinalIgnoreCase));
                if (item == null)
                {
                    GetExecutionMessages(null, false, chequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    return Task.FromResult(ExecutionMessage);
                }

                item.CertificationStatus = "Rejected";
                GetExecutionMessages(item, true, chequeCertificationID, MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Rejected.");
                return Task.FromResult(ExecutionMessage);
            }
        }

        // Delete certification (by ChequeCertificationID)
        public Task<ExecutionMessages> DeleteAsync(string chequeCertificationID)
        {
            if (string.IsNullOrWhiteSpace(chequeCertificationID))
            {
                GetExecutionMessages(null, false, "Delete", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Invalid ChequeCertificationID.");
                return Task.FromResult(ExecutionMessage);
            }

            lock (_lock)
            {
                var existing = _mockData.FirstOrDefault(x => string.Equals(x.ChequeCertificationID, chequeCertificationID, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    GetExecutionMessages(null, false, chequeCertificationID, MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    return Task.FromResult(ExecutionMessage);
                }

                _mockData.Remove(existing);
                GetExecutionMessages(null, true, chequeCertificationID, MessagesResults.Success, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, "Deleted successfully.");
                return Task.FromResult(ExecutionMessage);
            }
        }

        // Helper to clone DTO (avoid exposing internal list instances)
        private ChequeCertificationDto Clone(ChequeCertificationDto src)
        {
            if (src == null) return null;
            return new ChequeCertificationDto
            {
                ChequeCertificationID = src.ChequeCertificationID,
                MemberReference = src.MemberReference,
                BranchId = src.BranchId,
                ChequeBookId = src.ChequeBookId,
                ChequeleafId = src.ChequeleafId,
                AccountNumber = src.AccountNumber,
                Amount = src.Amount,
                CreatedDate = src.CreatedDate,
                CertificationStatus = src.CertificationStatus,
                Description = src.Description
            };
        }
    }
}
