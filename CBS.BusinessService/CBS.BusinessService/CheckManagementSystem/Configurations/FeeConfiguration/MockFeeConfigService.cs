using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using global::CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;

namespace CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration
{

    
    
        public class MockFeeConfigService : BaseService
        {
            private static readonly List<FeeConfig> _mockFeeConfigs = new List<FeeConfig>
        {
            new FeeConfig
            {
                Id = "FeeConfig961205778227",
                IsCentralized = true,
                BranchId = null,
                BranchName = "Central Branch",
                Description = "Configuration centralisée",
                FeeType = "CashIn",
                AcceptPercentage = true,
                PercentageApplied = 2.5,
                AcceptRange = false,
                IsActive = true,
                FeeTypeRanges = new List<Range> { new Range { FromAmount = 0, ToAmount = 1000, Fee = 10 } }
            },
            new FeeConfig
            {
                Id = "FeeConfig731204065348",
                IsCentralized = false,
                BranchId = "BR123",
                BranchName = "Main Branch",
                Description = "Configuration par branche",
                FeeType = "CashOut",
                AcceptPercentage = false,
                PercentageApplied = null,
                AcceptRange = true,
                IsActive = true,
                FeeTypeRanges = new List<Range>
                {
                    new Range { FromAmount = 0, ToAmount = 500, Fee = 5 },
                    new Range { FromAmount = 500.01m, ToAmount = 2000, Fee = 15 }
                }
            },
            new FeeConfig
            {
                Id = "FeeConfigABCDEFGHIJKL",
                IsCentralized = true,
                BranchId = null,
                BranchName = "Central Branch",
                Description = "Frais de carnet de chèques",
                FeeType = "CheckFee",
                AcceptPercentage = false,
                PercentageApplied = null,
                AcceptRange = true,
                IsActive = true,
                FeeTypeRanges = new List<Range>
                {
                    new Range { FromAmount = 0, ToAmount = 10000, Fee = 2500 },
                    new Range { FromAmount = 10001, ToAmount = 50000, Fee = 5000 }
                }
            }
        };

            private static readonly object _mockLock = new object();

            public Task<IEnumerable<FeeConfig>> GetConfigsAsync(string feeType = null, string branchId = null, bool? centralized = null)
            {
                return GetConfigsMockAsync(feeType, branchId, centralized);
            }

            public Task<IEnumerable<FeeConfig>> GetConfigsMockAsync(string feeType = null, string branchId = null, bool? centralized = null)
            {
                IEnumerable<FeeConfig> query;
                lock (_mockLock)
                {
                    query = _mockFeeConfigs.ToList();
                }

                if (centralized.HasValue) query = query.Where(c => c.IsCentralized == centralized.Value);
                if (!string.IsNullOrEmpty(feeType)) query = query.Where(c => c.FeeType.Equals(feeType, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(branchId)) query = query.Where(c => c.BranchId == branchId);

                return Task.FromResult(query.ToList().AsEnumerable());
            }

            public Task<IEnumerable<FeeConfig>> GetAllConfigsAsSummaryAsync()
            {
                List<FeeConfig> summaryList;
                lock (_mockLock)
                {
                    summaryList = _mockFeeConfigs.Select(c => new FeeConfig
                    {
                        Id = c.Id,
                        FeeType = c.FeeType,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        IsCentralized = c.IsCentralized,
                        BranchName = c.BranchName
                    }).ToList();
                }
                return Task.FromResult(summaryList.AsEnumerable());
            }

            public Task<List<StringValues>> GetFeeTypesMockAsync()
            {
                var data = new List<StringValues>
            {
                new StringValues { Text = "CashIn", Value = "CashIn" },
                new StringValues { Text = "CashOut", Value = "CashOut" },
                new StringValues { Text = "CheckFee", Value = "CheckFee" },
                new StringValues { Text = "InterBranch", Value = "InterBranch" }
            };
                return Task.FromResult(data);
            }

            public Task<FeeConfig> GetByIdAsync(string id) => GetByIdMockAsync(id);

            public Task<FeeConfig> GetByIdMockAsync(string id)
            {
                if (string.IsNullOrWhiteSpace(id)) return Task.FromResult<FeeConfig>(null);
                lock (_mockLock)
                {
                    var cfg = _mockFeeConfigs.FirstOrDefault(c => c.Id == id);
                    return Task.FromResult(cfg);
                }
            }

            public Task<ExecutionMessages> CreateAsync(FeeConfig model) => CreateMockAsync(model);

            public Task<ExecutionMessages> CreateMockAsync(FeeConfig model)
            {
                model.Id = "FeeConfig" + Guid.NewGuid().ToString("N");
                lock (_mockLock)
                {
                    _mockFeeConfigs.Add(model);
                }
                GetExecutionMessages(model, true, "FeeConfig", MessagesResults.Success, ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "Created");
                return Task.FromResult(ExecutionMessage);
            }

            public Task<ExecutionMessages> UpdateAsync(FeeConfig model) => UpdateMockAsync(model);

            public Task<ExecutionMessages> UpdateMockAsync(FeeConfig model)
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Id))
                {
                    GetExecutionMessages(model, false, "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
                    return Task.FromResult(ExecutionMessage);
                }

                lock (_mockLock)
                {
                    var existing = _mockFeeConfigs.FirstOrDefault(c => c.Id == model.Id);
                    if (existing != null)
                    {
                        existing.FeeType = model.FeeType;
                        existing.Description = model.Description;
                        existing.IsCentralized = model.IsCentralized;
                        existing.BranchId = model.BranchId;
                        existing.BranchName = model.BranchName;
                        existing.AcceptPercentage = model.AcceptPercentage;
                        existing.PercentageApplied = model.PercentageApplied;
                        existing.AcceptRange = model.AcceptRange;
                        existing.IsActive = model.IsActive;
                        existing.FeeTypeRanges = model.FeeTypeRanges ?? new List<Range>();

                        GetExecutionMessages(existing, true, "FeeConfig", MessagesResults.Success, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Updated");
                    }
                    else
                    {
                        GetExecutionMessages(model, false, "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    }
                }
                return Task.FromResult(ExecutionMessage);
            }

            public Task<ExecutionMessages> DeleteAsync(string id) => DeleteMockAsync(id);

            public Task<ExecutionMessages> DeleteMockAsync(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Invalid id.");
                    return Task.FromResult(ExecutionMessage);
                }

                lock (_mockLock)
                {
                    var itemToRemove = _mockFeeConfigs.FirstOrDefault(c => c.Id == id);
                    if (itemToRemove != null)
                    {
                        _mockFeeConfigs.Remove(itemToRemove);
                        GetExecutionMessages(null, true, $"FeeConfig:{id}", MessagesResults.Success, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, "Deleted successfully.");
                    }
                    else
                    {
                        GetExecutionMessages(null, false, $"FeeConfig:{id}", MessagesResults.Failed, ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, "Item not found.");
                    }
                }
                return Task.FromResult(ExecutionMessage);
            }
        }
    }

