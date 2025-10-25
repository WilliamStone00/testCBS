using BusinessServices;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonAccountConfiguration;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.LiaisonAccountConfiguration
{
    public class MockLiaisonAccountConfiguration : BaseService
    {
        // Mock storag
            // ✅ Mock in-memory data
            private static readonly List<LiaisonAccountConfigurationDto> _mockData = new List<LiaisonAccountConfigurationDto>
        {
            new LiaisonAccountConfigurationDto
            {
                BranchId = "BR001",
                BranchName = "Head Office",
                CashInHandAccountId = "ACC1001",
                CashInHandAccountName = "Cash In Hand - HO",
                VaultAccountId = "ACC1002",
                VaultAccountName = "Vault - HO",
                SurplusIncomeAccountId = "ACC1003",
                SurplusIncomeAccountName = "Surplus Income - HO",
                ShortageExpenseAccountId = "ACC1004",
                ShortageExpenseAccountName = "Shortage Expense - HO",
                RealTimeCashPosting = true,
                RevenueAccountId = "ACC1005",
                RevenueAccountName = "Revenue - HO",
                SourceBranchAccountId = "ACC1006",
                SourceBranchAccountName = "Source Branch - HO",
                DestinationBranchAccountId = "ACC1007",
                DestinationBranchAccountName = "Destination Branch - HO",
                HeadOfficeAccountId = "ACC1008",
                PartnerAccountId = "ACC1009",
                CamcculAccountId = "ACC1010",
                HeadOfficeLiaisonAccountId = "ACC1011",
                FormFeeIncomeAccountId = "ACC1012",
                Status = true
            },
            new LiaisonAccountConfigurationDto
            {
                BranchId = "BR002",
                BranchName = "Abuja Branch",
                CashInHandAccountId = "ACC2001",
                CashInHandAccountName = "Cash In Hand - Abuja",
                VaultAccountId = "ACC2002",
                VaultAccountName = "Vault - Abuja",
                SurplusIncomeAccountId = "ACC2003",
                SurplusIncomeAccountName = "Surplus Income - Abuja",
                ShortageExpenseAccountId = "ACC2004",
                ShortageExpenseAccountName = "Shortage Expense - Abuja",
                RealTimeCashPosting = false,
                RevenueAccountId = "ACC2005",
                RevenueAccountName = "Revenue - Abuja",
                SourceBranchAccountId = "ACC2006",
                SourceBranchAccountName = "Source Branch - Abuja",
                DestinationBranchAccountId = "ACC2007",
                DestinationBranchAccountName = "Destination Branch - Abuja",
                HeadOfficeAccountId = "ACC2008",
                PartnerAccountId = "ACC2009",
                CamcculAccountId = "ACC2010",
                HeadOfficeLiaisonAccountId = "ACC2011",
                FormFeeIncomeAccountId = "ACC2012",
                Status = true
            }
        };

            private async Task SimulateDelay() => await Task.Delay(300);

            public async Task<IEnumerable<LiaisonAccountConfigurationDto>> GetLiaisonAccountMappings()
            {
                await SimulateDelay();
                return _mockData;
            }

            public async Task<LiaisonAccountConfigurationDto> GetLiaisonAccountMapping(string branchId)
            {
                await SimulateDelay();
                return _mockData.FirstOrDefault(x => x.BranchId == branchId);
            }

            public async Task<ExecutionMessages> Create(LiaisonAccountConfigurationDto model)
            {
                await SimulateDelay();

                if (_mockData.Any(x => x.BranchId == model.BranchId))
                {
                    return new ExecutionMessages
                    {
                        Result = false,
                        MessageStatus = SystemMessageStatus.Failed.ToString(),
                       // MessageResult = "Mock: Configuration already exists for this branch."
                    };
                }

                _mockData.Add(model);

                return new ExecutionMessages
                {
                    Result = true,
                    MessageStatus = SystemMessageStatus.Success.ToString(),
                    //MessageResult = "Mock: Liaison configuration created successfully."
                };
            }

            public async Task<ExecutionMessages> Update(LiaisonAccountConfigurationDto model)
            {
                await SimulateDelay();

                var existing = _mockData.FirstOrDefault(x => x.BranchId == model.BranchId);
                if (existing == null)
                {
                    return new ExecutionMessages
                    {
                        Result = false,
                        MessageStatus = SystemMessageStatus.Failed.ToString(),
                       // MessageResult = "Mock: Configuration not found."
                    };
                }

                // Replace values
                var index = _mockData.IndexOf(existing);
                _mockData[index] = model;

                return new ExecutionMessages
                {
                    Result = true,
                    MessageStatus = SystemMessageStatus.Success.ToString(),
                   // MessageResult = "Mock: Liaison configuration updated successfully."
                };
            }

            public async Task<ExecutionMessages> Delete(string branchId)
            {
                await SimulateDelay();

                var existing = _mockData.FirstOrDefault(x => x.BranchId == branchId);
                if (existing == null)
                {
                    return new ExecutionMessages
                    {
                        Result = false,
                        MessageStatus = SystemMessageStatus.Failed.ToString(),
                       // MessageResult = "Mock: Configuration not found."
                    };
                }

                _mockData.Remove(existing);

                return new ExecutionMessages
                {
                    Result = true,
                    MessageStatus = SystemMessageStatus.Success.ToString(),
                    //MessageResult = "Mock: Liaison configuration deleted successfully."
                };
            }
    }
}

