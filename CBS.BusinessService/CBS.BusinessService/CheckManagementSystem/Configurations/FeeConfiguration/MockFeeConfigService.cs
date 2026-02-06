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
                id = "FeeConfig961205778227",
                isCentralized = true,
                branchId = null,
                branchName = "Central Branch",
                description = "Configuration centralisée",
                feeType = "CashIn",
                acceptPercentage = true,
                percentageApplied = 25,
                acceptRange = false,
                feeTypeRanges = new List<Range> { new Range { amountFrom = 0, amountTo = 1000, value = 10 } }
            },
            new FeeConfig
            {
                id = "FeeConfig731204065348",
                isCentralized = false,
                branchId = "BR123",
                branchName = "Main Branch",
                description = "Configuration par branche",
                feeType = "CashOut",
                acceptPercentage = false,
                percentageApplied = null,
                acceptRange = true,
                feeTypeRanges = new List<Range>
                {
                    new Range { amountFrom = 0, amountTo = 500, value = 5 },
                    new Range { amountFrom = 500.01m, amountTo = 2000, value = 15 }
                }
            },
            new FeeConfig
            {
                id = "FeeConfigABCDEFGHIJKL",
                isCentralized = true,
                branchId = null,
                branchName = "Central Branch",
                description = "Frais de carnet de chèques",
                feeType = "CheckFee",
                acceptPercentage = false,
                percentageApplied = null,
                acceptRange = true,
                feeTypeRanges = new List<Range>
                {
                    new Range { amountFrom = 0, amountTo = 10000, value = 2500 },
                    new Range { amountFrom = 10001, amountTo = 50000, value = 5000 }
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

                if (centralized.HasValue) query = query.Where(c => c.isCentralized == centralized.Value);
                if (!string.IsNullOrEmpty(feeType)) query = query.Where(c => c.feeType.Equals(feeType, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(branchId)) query = query.Where(c => c.branchId == branchId);

                return Task.FromResult(query.ToList().AsEnumerable());
            }

            public Task<IEnumerable<FeeConfig>> GetAllConfigsAsSummaryAsync()
            {
                List<FeeConfig> summaryList;
                lock (_mockLock)
                {
                    summaryList = _mockFeeConfigs.Select(c => new FeeConfig
                    {
                        id = c.id,
                        feeType = c.feeType,
                        description = c.description,
                        isCentralized = c.isCentralized,
                        branchName = c.branchName
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
                    var cfg = _mockFeeConfigs.FirstOrDefault(c => c.id == id);
                    return Task.FromResult(cfg);
                }
            }

            public Task<ExecutionMessages> CreateAsync(FeeConfig model) => CreateMockAsync(model);

            public Task<ExecutionMessages> CreateMockAsync(FeeConfig model)
            {
                model.id = "FeeConfig" + Guid.NewGuid().ToString("N");
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
                if (model == null || string.IsNullOrWhiteSpace(model.id))
                {
                    GetExecutionMessages(model, false, "FeeConfig", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Invalid model or Id.");
                    return Task.FromResult(ExecutionMessage);
                }

                lock (_mockLock)
                {
                    var existing = _mockFeeConfigs.FirstOrDefault(c => c.id == model.id);
                    if (existing != null)
                    {
                        existing.feeType = model.feeType;
                        existing.description = model.description;
                        existing.isCentralized = model.isCentralized;
                        existing.branchId = model.branchId;
                        existing.branchName = model.branchName;
                        existing.acceptPercentage = model.acceptPercentage;
                        existing.percentageApplied = model.percentageApplied;
                        existing.acceptRange = model.acceptRange;
                        existing.feeTypeRanges = model.feeTypeRanges ?? new List<Range>();

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
                    var itemToRemove = _mockFeeConfigs.FirstOrDefault(c => c.id == id);
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

