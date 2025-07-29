using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Data
{

    public class AccountBalanInitConfiguration
    {
        public InitInfoDto InitInfoDto { get; set; }
        public GetInfoDto GetInfoDto { get; set; }
        public List<AccountInitDto> AccountInitDtos { get; set; }
        public List<ZeroOutMemberBalanceSimulationDto> ZeroOutMemberBalanceSimulationDtos { get; set; }
        public List<ZeroOutMemberBalanceSimulationDetailDto> ZeroOutMemberBalanceSimulationDetailDtos { get; set; }
         public List<ZeroOutMemberBalanceSimulationCommand> ZeroOutMemberBalanceSimulationCommand { get; set; }
        public string ServiceOption { get; set; }
        

    }
    public class GetInfoDto
    {
        public string BranchId { get; set; }
        public string ProductId { get; set; }
    }
    public class InitInfoDto
    {
        public string ScenarioId { get; set; } = "";
        public string ProductId { get; set; }

        public decimal SubmittedMemberBalance { get; set; }
        public decimal SubmittedGLBalance { get; set; }

        public string BranchId { get; set; }
    }
    public class AccountInitDto
    {
        public string Id { get; set; }
        public string ScenarioId { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string AccountType { get; set; }

        public decimal SubmittedMemberBalance { get; set; }
        public decimal ActualMemberBalance { get; set; }
        public decimal SubmittedGLBalance { get; set; }
        public decimal ActualGLBalance { get; set; }
        public decimal Difference { get; set; }

        public string TargetGLAccount { get; set; }
        public string SuspenseGLAccount { get; set; }
        public string Narration { get; set; }

        public string SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; }

        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
    }

    public class ZeroOutMemberBalanceSimulationDto
    {
        public string Id { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountToCredit { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string ZeroOutMemberBalanceId { get; set; }
        public string TransferStatus { get; set; }
        public string TransferMessage { get; set; }
        public DateTime TransferDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public virtual ICollection<ZeroOutMemberBalanceSimulationDetailDto> ZeroOutMemberBalanceSimulationDetails { get; set; }


        public static List<ZeroOutMemberBalanceSimulationDto>  GenerateZeroOutMemberBalanceSimulations()
        {
            var simulations = new List<ZeroOutMemberBalanceSimulationDto>();
            var random = new Random();

            var memberNames = new[] { "John Doe", "Jane Smith", "Michael Johnson", "Sarah Wilson", "David Brown",
                             "Lisa Davis", "Robert Miller", "Emily Garcia", "James Rodriguez", "Maria Martinez" };

            var accountTypes = new[] { "Savings", "Current", "Fixed Deposit", "Loan", "Investment" };
            var branchNames = new[] { "Main Branch", "Downtown Branch", "Eastside Branch", "Westside Branch", "Central Branch" };
            var branchCodes = new[] { "MB001", "DB002", "EB003", "WB004", "CB005" };
            var transferStatuses = new[] { "Pending", "Completed", "Failed", "In Progress" };
            var approvalStatuses = new[] { "Pending", "Approved", "Rejected", "Under Review" };
            var transferMessages = new[] { "Transfer successful", "Insufficient funds", "Account blocked", "Processing...", "Approved for transfer" };

            for (int i = 1; i <= 10; i++)
            {
                var balance = (decimal)(random.NextDouble() * 50000 + 1000); // Random balance between 1000-51000
                var amountToCredit = balance * 0.8m; // 80% of balance
                var totalBalance = balance + (decimal)(random.NextDouble() * 5000);
                var netBalance = totalBalance - amountToCredit;

                var branchIndex = random.Next(branchNames.Length);
                var createdDate = DateTime.Now.AddDays(-random.Next(1, 30));
                var approvalDate = createdDate.AddHours(random.Next(1, 48));
                var transferDate = approvalDate.AddMinutes(random.Next(30, 240));

                // Generate 2-4 detail records for each simulation
                var detailCount = random.Next(2, 5);
                var details = new List<ZeroOutMemberBalanceSimulationDetailDto>();
                var zeroOutId = Guid.NewGuid().ToString();

                for (int j = 1; j <= detailCount; j++)
                {
                    var detailBalance = (decimal)(random.NextDouble() * 10000 + 500);
                    var detailAmountToCredit = detailBalance * 0.75m;
                    var detailTotalBalance = detailBalance + (decimal)(random.NextDouble() * 1000);
                    var detailNetBalance = detailTotalBalance - detailAmountToCredit;

                    var detail = new ZeroOutMemberBalanceSimulationDetailDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        MemberName = memberNames[i - 1],
                        MemberReference = $"MEM{i:D6}",
                        AccountNumber = $"ACC{random.Next(100000, 999999)}-{j}",
                        AccountType = accountTypes[random.Next(accountTypes.Length)],
                        Balance = Math.Round(detailBalance, 2),
                        AmountToCredit = Math.Round(detailAmountToCredit, 2),
                        BranchId = Guid.NewGuid().ToString(),
                        BranchName = branchNames[branchIndex],
                        BranchCode = branchCodes[branchIndex],
                        TotalBalance = Math.Round(detailTotalBalance, 2),
                        NetBalance = Math.Round(detailNetBalance, 2),
                        ZeroOutMemberBalanceId = zeroOutId,
                        TransferStatus = transferStatuses[random.Next(transferStatuses.Length)],
                        TransferMessage = transferMessages[random.Next(transferMessages.Length)],
                        TransferDate = transferDate.AddMinutes(random.Next(-30, 30)),
                        ApprovalStatus = approvalStatuses[random.Next(approvalStatuses.Length)],
                        ApprovalDate = approvalDate.AddMinutes(random.Next(-15, 15)),
                        CreatedDate = createdDate.AddMinutes(random.Next(-10, 10)),
                        CreatedBy = $"User{random.Next(1, 100)}"
                    };

                    details.Add(detail);
                }

                var simulation = new ZeroOutMemberBalanceSimulationDto
                {
                    Id = Guid.NewGuid().ToString(),
                    MemberName = memberNames[i - 1],
                    MemberReference = $"MEM{i:D6}",
                    AccountNumber = $"ACC{random.Next(100000, 999999)}",
                    AccountType = accountTypes[random.Next(accountTypes.Length)],
                    Balance = Math.Round(balance, 2),
                    AmountToCredit = Math.Round(amountToCredit, 2),
                    BranchId = Guid.NewGuid().ToString(),
                    BranchName = branchNames[branchIndex],
                    BranchCode = branchCodes[branchIndex],
                    TotalBalance = Math.Round(totalBalance, 2),
                    NetBalance = Math.Round(netBalance, 2),
                    ZeroOutMemberBalanceId = zeroOutId,
                    TransferStatus = transferStatuses[random.Next(transferStatuses.Length)],
                    TransferMessage = transferMessages[random.Next(transferMessages.Length)],
                    TransferDate = transferDate,
                    ApprovalStatus = approvalStatuses[random.Next(approvalStatuses.Length)],
                    ApprovalDate = approvalDate,
                    CreatedDate = createdDate,
                    CreatedBy = $"User{random.Next(1, 100)}",
                    ZeroOutMemberBalanceSimulationDetails = details
                };

                simulations.Add(simulation);
            }

            return simulations;
        }

 
}


    public class ZeroOutMemberBalanceSimulationDetailDto
    {
        public string Id { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public decimal AmountToCredit { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal NetBalance { get; set; }
        public string ZeroOutMemberBalanceId { get; set; }
        public string TransferStatus { get; set; }
        public string TransferMessage { get; set; }
        public DateTime TransferDate { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class ZeroOutMemberBalanceSimulationCommand
    {
        public string AccountType { get; set; }
        public string StartAccount { get; set; }
        public string EndAccount { get; set; }
        public string Description { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string  BankName { get; set; }
        public string BranchName { get; set; }
        public string CreatedBy { get; set; }
    }
    public class ZeroOutMemberBalanceConfirmationCommand
    {
        public string SimulationId { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovalStatusDescription { get; set; }
        public string ApprovalBy { get; set; }
    }
}
