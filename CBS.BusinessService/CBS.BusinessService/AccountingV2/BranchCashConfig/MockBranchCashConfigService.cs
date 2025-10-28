using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.BranchCashConfigV
{
    public class MockBranchCashConfigService : BaseService
    {
        private readonly List<BranchCashConfig> _mockData;
        private int _mockIdCounter = 1000;

        public MockBranchCashConfigService()
        {
            _mockData = GenerateMockData();
        }

        private List<BranchCashConfig> GenerateMockData()
        {
            return new List<BranchCashConfig>
            {
                new BranchCashConfig
                {
                    Id = "BC-1001",
                    BranchId = "BR001",
                    CashInHandAccountId = "10001",
                    VaultAccountId = "10002",
                    SurplusIncomeAccountId = "469100",
                    ShortageExpenseAccountId = "569100",
                    RevenueAccountId = "401000",
                    FormFeeIncomeAccountId = "402000",
                    PartnerAccountId = "501000",
                    CamcculAccountId = "601000",
                    HeadOfficeLiaisonAccountId = "302000",
                    RealTimeCashPosting = true
                },
                new BranchCashConfig
                {
                    Id = "BC-1002",
                    BranchId = "BR002",
                    CashInHandAccountId = "10011",
                    VaultAccountId = "10012",
                    SurplusIncomeAccountId = "469100",
                    ShortageExpenseAccountId = "569100",
                    RevenueAccountId = "401000",
                    FormFeeIncomeAccountId = "402000",
                    RealTimeCashPosting = false
                },
                new BranchCashConfig
                {
                    Id = "BC-1003",
                    BranchId = "BR003",
                    CashInHandAccountId = "10021",
                    VaultAccountId = "10022",
                    SurplusIncomeAccountId = "469100",
                    ShortageExpenseAccountId = "569100",
                    RevenueAccountId = "401000",
                    FormFeeIncomeAccountId = "402000",
                    PartnerAccountId = "501100",
                    RealTimeCashPosting = true
                },
                new BranchCashConfig
                {
                    Id = "BC-1004",
                    BranchId = "BR004",
                    CashInHandAccountId = "10031",
                    VaultAccountId = "10032",
                    SurplusIncomeAccountId = "469100",
                    ShortageExpenseAccountId = "569100",
                    RevenueAccountId = "401000",
                    RealTimeCashPosting = false
                },
                new BranchCashConfig
                {
                    Id = "BC-1005",
                    BranchId = "BR005",
                    CashInHandAccountId = "10041",
                    VaultAccountId = "10042",
                    SurplusIncomeAccountId = "469100",
                    ShortageExpenseAccountId = "569100",
                    RevenueAccountId = "401000",
                    FormFeeIncomeAccountId = "402000",
                    RealTimeCashPosting = true
                }
            };
        }

        public async Task<IEnumerable<BranchCashConfig>> GetBranchCashConfigsAsync()
        {
            await Task.Delay(100);
            return _mockData;
        }

        public async Task<BranchCashConfig> GetBranchCashConfigByIdAsync(string id)
        {
            await Task.Delay(50);
            return _mockData.FirstOrDefault(c => c.Id == id);
        }

        public async Task<BranchCashConfig> GetBranchCashConfigByBranchIdAsync(string branchId)
        {
            await Task.Delay(50);
            return _mockData.FirstOrDefault(c => c.BranchId == branchId);
        }

        public async Task<IEnumerable<BranchCashConfig>> GetBranchCashConfigs()
        {
            await Task.Delay(100);
            var configs = _mockData.ToList();

            if (!IsHeadOffice())
            {
                string currentBranchId = GetBranchID();
                configs = configs.Where(c => c.BranchId == currentBranchId).ToList();
            }

            return configs.OrderBy(c => c.BranchId).ToList();
        }

        public async Task<ExecutionMessages> CreateBranchCashConfigAsync(BranchCashConfig model)
        {
            await Task.Delay(200);

            try
            {
                var existingConfig = _mockData.FirstOrDefault(c => c.BranchId == model.BranchId);
                if (existingConfig != null)
                {
                    GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Exist.ToString(), null,
                        "This branch already has a cash configuration.");
                    return ExecutionMessage;
                }

                model.Id = $"BC-{++_mockIdCounter}";
                _mockData.Add(model);

                GetExecutionMessages(model, true, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Success,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                    "Branch cash configuration created successfully.");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateBranchCashConfigAsync(BranchCashConfig model)
        {
            await Task.Delay(200);

            try
            {
                var existingConfig = _mockData.FirstOrDefault(c => c.Id == model.Id);
                if (existingConfig == null)
                {
                    GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.NotFound.ToString(), null,
                        "Configuration not found.");
                    return ExecutionMessage;
                }

                var index = _mockData.IndexOf(existingConfig);
                _mockData[index] = model;

                GetExecutionMessages(model, true, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                    "Branch cash configuration updated successfully.");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Branch Cash Config for Branch {model.BranchId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateBranchCashConfigAsync(string configId)
        {
            await Task.Delay(150);

            try
            {
                var config = _mockData.FirstOrDefault(c => c.Id == configId);
                if (config == null)
                {
                    GetExecutionMessages(null, false, $"Branch Cash Config ID: {configId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.NotFound.ToString(), null,
                        "Configuration not found.");
                    return ExecutionMessage;
                }

                _mockData.Remove(config);

                GetExecutionMessages(null, true, $"Branch Cash Config ID: {configId}", MessagesResults.Success,
                    ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                    "Branch cash configuration deactivated successfully.");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Branch Cash Config ID: {configId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<CustomDataTable> GetDataTableAsync(BranchCashConfigQueryDto branchCashConfigQueryDto)
        {
            await Task.Delay(300);

            var data = _mockData.Cast<object>().ToList();

            if (!string.IsNullOrEmpty(branchCashConfigQueryDto.BranchId))
            {
                data = data.Where(d => ((BranchCashConfig)d).BranchId == branchCashConfigQueryDto.BranchId).ToList();
            }

            if (branchCashConfigQueryDto.RealTimeCashPosting.HasValue)
            {
                data = data.Where(d => ((BranchCashConfig)d).RealTimeCashPosting == branchCashConfigQueryDto.RealTimeCashPosting.Value).ToList();
            }

            return new CustomDataTable(
                draw: Convert.ToInt32(branchCashConfigQueryDto.Options.draw),
                recordsTotal: _mockData.Count,
                recordsFiltered: data.Count,
                data: data,
                dataTableOptions: branchCashConfigQueryDto.Options
            );
        }

        public List<BranchCashConfig> MapToBranchCashConfigDownloadDtos(IEnumerable<BranchCashConfig> configs)
        {
            return configs.Select(MapToBranchCashConfigDownloadDto).ToList();
        }

        public BranchCashConfig MapToBranchCashConfigDownloadDto(BranchCashConfig config)
        {
            return new BranchCashConfig
            {
                Id = config.Id,
                BranchId = config.BranchId,
                CashInHandAccountId = config.CashInHandAccountId,
                VaultAccountId = config.VaultAccountId,
                SurplusIncomeAccountId = config.SurplusIncomeAccountId,
                RevenueAccountId = config.RevenueAccountId,
                ShortageExpenseAccountId = config.ShortageExpenseAccountId,
                RealTimeCashPosting = config.RealTimeCashPosting,
                PartnerAccountId = config.PartnerAccountId,
                CamcculAccountId = config.CamcculAccountId,
                HeadOfficeLiaisonAccountId = config.HeadOfficeLiaisonAccountId,
                FormFeeIncomeAccountId = config.FormFeeIncomeAccountId
            };
        }

        public List<BranchCashConfig> GetAllMockData() => _mockData;

        public async Task GetByIdAsync(string kEY)
        {
            throw new NotImplementedException();
        }
    }
}