using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.ChequeClearance
{

    public class MockCheckClearanceService
    {
        private readonly object _lock = new object();


        private readonly List<OptionRequest> _mockClearances = new List<OptionRequest>
{
    new OptionRequest
    {
        ChequeClearanceId = " Cheque001",
        External = true,
        BranchId = "B001",
        CheckBookNumber = "CB001",
        CheckBookPageNumber = "1",
        AccountNumber = "ACC1001", // shown because DepositToAccount = true
        DepositToAccount = true,
        SameDayProcessing = false,
        NationalIdNumber = "NID1001",
        ChequeBranchId = "BR001",
        ReceivingBranchId = "RB001",
        MemberReference = "MEM-1001",
        NationalIdCreationDate = new DateTime(2020, 5, 10),
        NationalIdExpirationDate = new DateTime(2030, 5, 10),
       ChequeCreationDate = new DateTime(2025, 9, 18, 14, 30, 0), // 18 Sep 2025, 14:30:00
        ChequeExpirationDate = new DateTime(2026, 3, 18, 17, 45, 0), // 18 Mar 2026, 17:45:00

        ChequeAmount = 1500.50m,
        Status = "Pending"
    },
    new OptionRequest
    {
        ChequeClearanceId = " Cheque002",
        External = false,
        BranchId = "B002",
        CheckBookNumber = "CB002",
        CheckBookPageNumber = "2",
        AccountNumber = null, // not shown because DepositToAccount = false
        DepositToAccount = false,
        SameDayProcessing = true,
        NationalIdNumber = "NID1002",
        ChequeBranchId = "BR002",
        ReceivingBranchId = "RB002",
        MemberReference = "MEM-1002",
        NationalIdCreationDate = new DateTime(2021, 2, 15),
        NationalIdExpirationDate = new DateTime(2031, 2, 15),
        ChequeCreationDate = new DateTime(2025, 9, 15, 14, 30, 0),
        ChequeExpirationDate = new DateTime(2026, 3, 15,17, 45, 0),
        ChequeAmount = 2500.00m,
        Status = "Disbursed"
    },
    new OptionRequest
    {
        ChequeClearanceId = " Cheque003",
        External = true,
        BranchId = "B003",
        CheckBookNumber = "CB003",
        CheckBookPageNumber = "1",
        AccountNumber = "ACC1003", // shown because DepositToAccount = true
        DepositToAccount = true,
        SameDayProcessing = false,
        NationalIdNumber = "NID1003",
        ChequeBranchId = "BR003",
        ReceivingBranchId = "RB003",
        MemberReference = "MEM-1003",
        NationalIdCreationDate = new DateTime(2020, 11, 5),
        NationalIdExpirationDate = new DateTime(2030, 11, 5),
        ChequeCreationDate = new DateTime(2025, 9, 12, 14, 30, 0),
        ChequeExpirationDate = new DateTime(2026, 3, 12,17, 45, 0),
        ChequeAmount = 0m,
        Status = "Rejected"
    },
    new OptionRequest
    {
        ChequeClearanceId = " Cheque004",
        External = false,
        BranchId = "B001",
        CheckBookNumber = "CB004",
        CheckBookPageNumber = "3",
        AccountNumber = null, // not shown because DepositToAccount = false
        DepositToAccount = false,
        SameDayProcessing = true,
        NationalIdNumber = "NID1004",
        ChequeBranchId = "BR001",
        ReceivingBranchId = "RB001",
        MemberReference = "MEM-1004",
        NationalIdCreationDate = new DateTime(2022, 6, 20),
        NationalIdExpirationDate = new DateTime(2032, 6, 20),
        ChequeCreationDate = new DateTime(2025, 9, 22, 14, 30, 0),
        ChequeExpirationDate = new DateTime(2026, 3, 22, 17, 45, 0),
        ChequeAmount = 500.75m,
        Status = "Approved"
    },
    new OptionRequest
    {
        ChequeClearanceId = " Cheque005",
        External = true,
        BranchId = "B002",
        CheckBookNumber = "CB005",
        CheckBookPageNumber = "4",
        AccountNumber = null, // not shown because DepositToAccount = false
        DepositToAccount = false,
        SameDayProcessing = true,
        NationalIdNumber = "NID1005",
        ChequeBranchId = "BR001",
        ReceivingBranchId = "RB001",
        MemberReference = "MEM-1005",
        NationalIdCreationDate = new DateTime(2022, 6, 20),
        NationalIdExpirationDate = new DateTime(2032, 6, 20),
        ChequeCreationDate = new DateTime(2025, 9, 22, 14, 30, 0),
        ChequeExpirationDate = new DateTime(2026, 3, 22, 17, 45, 0),
        ChequeAmount = 600.75m,
        Status = "Reviewed"
    }
};

        public Task<IEnumerable<OptionRequest>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_mockClearances.AsEnumerable());
            }
        }
        public Task<OptionRequest> GetByIdAsync(string chequeClearanceId)
        {
            var clearance = _mockClearances.FirstOrDefault(c => c.ChequeClearanceId == chequeClearanceId);
            return Task.FromResult(clearance);
        }
        public async Task<OptionRequest> GetByBranchAndBookAsync(bool isNotfromfi, string branchId, string CheckBookNumber, string pageNumber)
        {
            /*lock (_lock)
            {*/
                var result = _mockClearances.FirstOrDefault(c =>
                   
                    c.BranchId == branchId &&
                    c.CheckBookNumber == CheckBookNumber &&
                    c.CheckBookPageNumber == pageNumber);

                return result;
            //}
        }

        public Task<OptionRequest> CreateAsync(OptionRequest model)
        {
            lock (_lock)
            {
                _mockClearances.Add(model);
                return Task.FromResult(model);
            }
        }

       
    }


}
