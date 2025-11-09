using CBS.FrontDesk.Data.Entity.Accounting_V2.TrialBalance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.MockData
{
    public class TrialBalance6ColumnsMock
    {
        public static TrialBalanceResponseDto GetMockData()
        {
            return new TrialBalanceResponseDto
            {
                Lines = new List<TrialBalanceDto>
                {
                    new TrialBalanceDto
                    {
                        AccountNumber = "100000000000",
                        AccountName = "Fully paid shares2",
                        OpeningDR = 727964220.00m,
                        OpeningCR = 0.00m,
                        PeriodDR = 1479213476.00m,
                        PeriodCR = 751249256.00m,
                        ClosingDR = 727964220.00m,
                        ClosingCR = 0.00m
                    },
                    new TrialBalanceDto
                    {
                        AccountNumber = "111000000000",
                        AccountName = "LEGAL RESERVES",
                        OpeningDR = 0.00m,
                        OpeningCR = 727964220.00m,
                        PeriodDR = 751249256.00m,
                        PeriodCR = 1479213476.00m,
                        ClosingDR = 0.00m,
                        ClosingCR = 727964220.00m
                    }
                }
            };
        }
    
    }
}
