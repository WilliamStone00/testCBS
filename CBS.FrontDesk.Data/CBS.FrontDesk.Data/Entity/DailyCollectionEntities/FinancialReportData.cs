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