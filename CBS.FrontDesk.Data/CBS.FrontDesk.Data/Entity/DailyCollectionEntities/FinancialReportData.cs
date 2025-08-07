using System;
using System.Collections.Generic;

public class FinancialReportResponse
{
    public FinancialReportData Data { get; set; }
    public List<string> Errors { get; set; }
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
}

public class FinancialReportData
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalCollectedAmount { get; set; }
    public decimal TotalFeeCollected { get; set; }
    public decimal TotalCollectorShare { get; set; }
    public decimal TotalBranchShare { get; set; }
    public int TotalCashInCount { get; set; }
    public int TotalMembersServed { get; set; }
    public string MostFrequentSaver { get; set; }
    public string TopSaverName { get; set; }
    public decimal TopSaverAmount { get; set; }
    public decimal AverageCollectionPerSaver { get; set; }
    public int DaysWorked { get; set; }
    public int TotalSubscriptionCount { get; set; }
    public decimal TotalSubscriptionAmount { get; set; }
    public int TotalCashOutCount { get; set; }
    public decimal TotalCashOutAmount { get; set; }
    public decimal NetBalance { get; set; }
    public decimal CollectorSharePercentage { get; set; }
    public decimal BranchSharePercentage { get; set; }
    public string HighestCashInSaverName { get; set; }
    public decimal HighestCashInAmount { get; set; }
    public string HighestCashOutSaverName { get; set; }
    public decimal HighestCashOutAmount { get; set; }
    public string HighestCashInBranchName { get; set; }
    public string HighestCashInBranchCode { get; set; }
    public decimal HighestCashInBranchAmount { get; set; }
    public string HighestCashOutBranchName { get; set; }
    public string HighestCashOutBranchCode { get; set; }
    public decimal HighestCashOutBranchAmount { get; set; }
    public List<DailySummary> DailySummaries { get; set; }
    public List<Transaction> Transactions { get; set; }
    public List<TopSaver> TopSavers { get; set; }
    public List<TopCollector> TopCollectors { get; set; }
    public List<TopBranch> TopBranches { get; set; }
    public List<BranchDashboard> BranchDashboards { get; set; }


}
public static class FinancialReportDataGenerator
{
    public static FinancialReportData GenerateTestData()
    {
        var reportData = new FinancialReportData
        {
            Year = 2024,
            Month = 11,
            TotalCollectedAmount = 2850000.00m,
            TotalFeeCollected = 142500.00m,
            TotalCollectorShare = 85500.00m,
            TotalBranchShare = 57000.00m,
            TotalCashInCount = 1247,
            TotalMembersServed = 892,
            MostFrequentSaver = "John Doe",
            TopSaverName = "Mary Johnson",
            TopSaverAmount = 125000.00m,
            AverageCollectionPerSaver = 3194.40m,
            DaysWorked = 22,
            TotalSubscriptionCount = 156,
            TotalSubscriptionAmount = 468000.00m,
            TotalCashOutCount = 89,
            TotalCashOutAmount = 534000.00m,
            NetBalance = 2784000.00m,
            CollectorSharePercentage = 3.0m,
            BranchSharePercentage = 2.0m,
            HighestCashInSaverName = "Mary Johnson",
            HighestCashInAmount = 125000.00m,
            HighestCashOutSaverName = "Robert Wilson",
            HighestCashOutAmount = 45000.00m,
            HighestCashInBranchName = "Downtown Branch",
            HighestCashInBranchCode = "DTB001",
            HighestCashInBranchAmount = 850000.00m,
            HighestCashOutBranchName = "Westside Branch",
            HighestCashOutBranchCode = "WSB003",
            HighestCashOutBranchAmount = 198000.00m,

            DailySummaries = GenerateDailySummaries(),
            Transactions = GenerateTransactions(),
            TopSavers = GenerateTopSavers(),
            TopCollectors = GenerateTopCollectors(),
            TopBranches = GenerateTopBranches(),
            BranchDashboards = GenerateBranchDashboards()
        };

        return reportData;
    }

    private static List<DailySummary> GenerateDailySummaries()
    {
        return new List<DailySummary>
        {
            new DailySummary
            {
                Date = new DateTime(2024, 11, 1),
                CashInCount = 45,
                CashInAmount = 115000.00m,
                CashOutCount = 3,
                CashOutAmount = 18000.00m,
                SubscriptionCount = 8,
                SubscriptionAmount = 24000.00m,
                CollectorCommission = 3450.00m,
                BranchCommission = 2300.00m,
                TotalFeeCollected = 5750.00m,
                TotalCollectorShare = 3450.00m
            },
            new DailySummary
            {
                Date = new DateTime(2024, 11, 2),
                CashInCount = 52,
                CashInAmount = 128000.00m,
                CashOutCount = 4,
                CashOutAmount = 22000.00m,
                SubscriptionCount = 6,
                SubscriptionAmount = 18000.00m,
                CollectorCommission = 3840.00m,
                BranchCommission = 2560.00m,
                TotalFeeCollected = 6400.00m,
                TotalCollectorShare = 3840.00m
            },
            new DailySummary
            {
                Date = new DateTime(2024, 11, 3),
                CashInCount = 48,
                CashInAmount = 122000.00m,
                CashOutCount = 2,
                CashOutAmount = 15000.00m,
                SubscriptionCount = 7,
                SubscriptionAmount = 21000.00m,
                CollectorCommission = 3660.00m,
                BranchCommission = 2440.00m,
                TotalFeeCollected = 6100.00m,
                TotalCollectorShare = 3660.00m
            }
        };
    }

    private static List<Transaction> GenerateTransactions()
    {
        return new List<Transaction>
        {
            new Transaction
            {
                Date = new DateTime(2024, 11, 1, 9, 15, 0),
                MemberReference = "MBR001234",
                MemberName = "Alice Smith",
                OperationType = "CashIn",
                Amount = 15000.00m,
                Fee = 750.00m,
                AccountNumber = "ACC789012",
                TransactionReference = "TXN20241101001",
                CollectorShare = 450.00m,
                BranchName = "Downtown Branch",
                BranchCode = "DTB001",
                BranchId = "BR001"
            },
            new Transaction
            {
                Date = new DateTime(2024, 11, 1, 10, 30, 0),
                MemberReference = "MBR005678",
                MemberName = "David Brown",
                OperationType = "CashOut",
                Amount = 8000.00m,
                Fee = 400.00m,
                AccountNumber = "ACC345678",
                TransactionReference = "TXN20241101002",
                CollectorShare = 240.00m,
                BranchName = "Eastside Branch",
                BranchCode = "ESB002",
                BranchId = "BR002"
            },
            new Transaction
            {
                Date = new DateTime(2024, 11, 1, 14, 20, 0),
                MemberReference = "MBR009876",
                MemberName = "Sarah Davis",
                OperationType = "Subscription",
                Amount = 3000.00m,
                Fee = 150.00m,
                AccountNumber = "ACC123456",
                TransactionReference = "TXN20241101003",
                CollectorShare = 90.00m,
                BranchName = "Downtown Branch",
                BranchCode = "DTB001",
                BranchId = "BR001"
            },
            new Transaction
            {
                Date = new DateTime(2024, 11, 2, 8, 45, 0),
                MemberReference = "MBR002468",
                MemberName = "Michael Wilson",
                OperationType = "CashIn",
                Amount = 25000.00m,
                Fee = 1250.00m,
                AccountNumber = "ACC987654",
                TransactionReference = "TXN20241102001",
                CollectorShare = 750.00m,
                BranchName = "Westside Branch",
                BranchCode = "WSB003",
                BranchId = "BR003"
            },
            new Transaction
            {
                Date = new DateTime(2024, 11, 2, 11, 15, 0),
                MemberReference = "MBR013579",
                MemberName = "Emma Johnson",
                OperationType = "CashIn",
                Amount = 12000.00m,
                Fee = 600.00m,
                AccountNumber = "ACC246810",
                TransactionReference = "TXN20241102002",
                CollectorShare = 360.00m,
                BranchName = "Northside Branch",
                BranchCode = "NSB004",
                BranchId = "BR004"
            }
        };
    }

    private static List<TopSaver> GenerateTopSavers()
    {
        return new List<TopSaver>
        {
            new TopSaver { Name = "Mary Johnson", Code = "SAV001", TotalAmount = 125000.00m },
            new TopSaver { Name = "James Miller", Code = "SAV002", TotalAmount = 98500.00m },
            new TopSaver { Name = "Patricia Garcia", Code = "SAV003", TotalAmount = 87200.00m },
            new TopSaver { Name = "Robert Anderson", Code = "SAV004", TotalAmount = 76800.00m },
            new TopSaver { Name = "Jennifer Martinez", Code = "SAV005", TotalAmount = 69300.00m }
        };
    }

    private static List<TopCollector> GenerateTopCollectors()
    {
        return new List<TopCollector>
        {
            new TopCollector { Name = "Thomas Wilson", Code = "COL001", TotalAmount = 28500.00m },
            new TopCollector { Name = "Linda Thompson", Code = "COL002", TotalAmount = 24200.00m },
            new TopCollector { Name = "Christopher Lee", Code = "COL003", TotalAmount = 19800.00m },
            new TopCollector { Name = "Angela White", Code = "COL004", TotalAmount = 13000.00m }
        };
    }

    private static List<TopBranch> GenerateTopBranches()
    {
        return new List<TopBranch>
        {
            new TopBranch { Name = "Downtown Branch", Code = "DTB001", TotalAmount = 850000.00m },
            new TopBranch { Name = "Eastside Branch", Code = "ESB002", TotalAmount = 720000.00m },
            new TopBranch { Name = "Westside Branch", Code = "WSB003", TotalAmount = 680000.00m },
            new TopBranch { Name = "Northside Branch", Code = "NSB004", TotalAmount = 600000.00m }
        };
    }

    private static List<BranchDashboard> GenerateBranchDashboards()
    {
        return new List<BranchDashboard>
        {
            new BranchDashboard
            {
                BranchId = "BR001",
                BranchCode = "DTB001",
                BranchName = "Downtown Branch",
                TotalCollectedAmount = 850000.00m,
                TotalCashInCount = 387,
                TotalFeeCollected = 42500.00m,
                TotalCollectorShare = 25500.00m,
                TotalBranchShare = 17000.00m,
                TotalMembersServed = 298,
                AverageCollectionPerSaver = 2852.35m,
                TotalSubscriptionCount = 45,
                TotalSubscriptionAmount = 135000.00m
            },
            new BranchDashboard
            {
                BranchId = "BR002",
                BranchCode = "ESB002",
                BranchName = "Eastside Branch",
                TotalCollectedAmount = 720000.00m,
                TotalCashInCount = 324,
                TotalFeeCollected = 36000.00m,
                TotalCollectorShare = 21600.00m,
                TotalBranchShare = 14400.00m,
                TotalMembersServed = 245,
                AverageCollectionPerSaver = 2938.78m,
                TotalSubscriptionCount = 38,
                TotalSubscriptionAmount = 114000.00m
            },
            new BranchDashboard
            {
                BranchId = "BR003",
                BranchCode = "WSB003",
                BranchName = "Westside Branch",
                TotalCollectedAmount = 680000.00m,
                TotalCashInCount = 289,
                TotalFeeCollected = 34000.00m,
                TotalCollectorShare = 20400.00m,
                TotalBranchShare = 13600.00m,
                TotalMembersServed = 218,
                AverageCollectionPerSaver = 3119.27m,
                TotalSubscriptionCount = 42,
                TotalSubscriptionAmount = 126000.00m
            },
            new BranchDashboard
            {
                BranchId = "BR004",
                BranchCode = "NSB004",
                BranchName = "Northside Branch",
                TotalCollectedAmount = 600000.00m,
                TotalCashInCount = 247,
                TotalFeeCollected = 30000.00m,
                TotalCollectorShare = 18000.00m,
                TotalBranchShare = 12000.00m,
                TotalMembersServed = 131,
                AverageCollectionPerSaver = 4580.15m,
                TotalSubscriptionCount = 31,
                TotalSubscriptionAmount = 93000.00m
            }
        };
    }
}

// Usage example:
// var testData = FinancialReportDataGenerator.GenerateTestData();
 

public class DailySummary
{
    public DateTime Date { get; set; }
    public int CashInCount { get; set; }
    public decimal CashInAmount { get; set; }
    public int CashOutCount { get; set; }
    public decimal CashOutAmount { get; set; }
    public int SubscriptionCount { get; set; }
    public decimal SubscriptionAmount { get; set; }
    public decimal CollectorCommission { get; set; }
    public decimal BranchCommission { get; set; }
    public decimal TotalFeeCollected { get; set; }
    public decimal TotalCollectorShare { get; set; }
}

public class Transaction
{
    public DateTime Date { get; set; }
    public string MemberReference { get; set; }
    public string MemberName { get; set; }
    public string OperationType { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public string AccountNumber { get; set; }
    public string TransactionReference { get; set; }
    public decimal CollectorShare { get; set; }
    public string BranchName { get; set; }
    public string BranchCode { get; set; }
    public string BranchId { get; set; }
}

public class TopSaver
{
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TopCollector
{
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal TotalAmount { get; set; }
}

public class TopBranch
{
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal TotalAmount { get; set; }
}

public class BranchDashboard
{
    public string BranchId { get; set; }
    public string BranchCode { get; set; }
    public string BranchName { get; set; }
    public decimal TotalCollectedAmount { get; set; }
    public int TotalCashInCount { get; set; }
    public decimal TotalFeeCollected { get; set; }
    public decimal TotalCollectorShare { get; set; }
    public decimal TotalBranchShare { get; set; }
    public int TotalMembersServed { get; set; }
    public decimal AverageCollectionPerSaver { get; set; }
    public int TotalSubscriptionCount { get; set; }
    public decimal TotalSubscriptionAmount { get; set; }
}