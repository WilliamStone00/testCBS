document.addEventListener("DOMContentLoaded", function () {

    // Initial call to fetch data after the page loads
    fetchDashboardDataForOrdinaryAccounts();
    // Initial call to fetch data after the page loads
    fetchDashboardDataForMember();


    fetchDashboardDataForLoan();
    fetchDashboardDataForAccounting();


});
async function fetchDashboardDataForOrdinaryAccounts() {
    try {
        const response = await fetch('/Dashboard/GetOrdinaryAccountsDashboard');

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();

        // Define the currency formatter for XAF
        const currencyFormatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'XAF',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0,
        });

        // Safely update HTML elements if they exist, applying the currency format for currency fields
        function setElementText(id, value, isCurrency = false) {
            const element = document.getElementById(id);
            if (element) {
                if (typeof value === 'number' && isCurrency) {
                    element.textContent = currencyFormatter.format(value);
                } else {
                    element.textContent = value ?? '';
                }
            }
        }

        // Map MainDashboardOrdinaryAccounts fields to HTML elements
        setElementText('totalAccounts', data.TotalAccounts);
        setElementText('totalActiveAccounts', data.TotalActieAccounts);
        setElementText('totalInactiveAccounts', data.TotalInactiveAccounts);
        setElementText('totalOrdinaryShares', data.TotalOrdinaryShares);
        setElementText('totalVolumeOfOrdinaryShares', data.TotalVolumeOfOrdinaryShares, true);
        setElementText('totalPreferenceShares', data.TotalPreferenceShares);
        setElementText('totalVolumeOfPreferenceShares', data.TotalVolumeOfPreferenceShares, true);
        setElementText('totalSavings', data.TotalSavings);
        setElementText('totalVolumeOfSavings', data.TotalVolumeOfSavings, true);
        setElementText('totalDeposits', data.TotalDeposits);
        setElementText('totalVolumeOfDeposits', data.TotalVolumeOfDeposits, true);
        setElementText('totalDailyCollections', data.TotalDailyCollections);
        setElementText('totalVolumeOfDailyCollections', data.TotalVolumeOfDailyCollections, true);
        setElementText('totalGav', data.TotalGav);
        setElementText('totalVolumeOfGav', data.TotalVolumeOfGav, true);
        setElementText('totalVolumeOfBlockedAccounts', data.TotalVolumeOfBlockedAccounts, true);
        setElementText('branchName', data.BranchName);
        setElementText('totalBranches', data.TotalBranches);
        setElementText('totalMembers', data.TotalMembers);
        setElementText('totalNumberOfActiveAccounts', data.TotalNumberOfActiveAccounts);
        setElementText('totalNumberOfInActiveAccounts', data.TotalNumberOfInActiveAccounts);
        setElementText('branchId', data.BranchId);
        setElementText('totalBalance', data.TotalBalance, true);
        setElementText('totalBalanceWithoutBlocked', data.TotalBalanceWithoutBlocked, true);
        setElementText('totalBlockedAmount', data.TotalBlockedAmount, true);

    } catch (error) {
        console.error("Error fetching data:", error.message);
        appalert(`Failed to fetch data: ${error.message}`, 0, 1);
    }
}
async function fetchDashboardDataForMember() {
    try {
        const response = await fetch('/Dashboard/GetMemberDashboard');

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();

        // Helper function to set text content with formatting
        function setElementText(id, value) {
            const element = document.getElementById(id);
            if (element) {
                // Format the value to 0 decimal places if it's a number
                const formattedValue = typeof value === 'number'
                    ? new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(value)
                    : value ?? '';
                element.textContent = formattedValue;
            }
        }

        // Map MainDashboardMembers fields to HTML elements with formatted values
        setElementText('mtotalMembers', data.TotalMembers);
        setElementText('mbranchName', data.BranchName);
        setElementText('mtotalPhysicalMembers', data.TotalPhysicalMembers);
        setElementText('mtotalMoralMembers', data.TotalMoralMembers);
        setElementText('mtotalBranches', data.TotalBranches);
        
    } catch (error) {
        console.error("Error fetching data:", error.message);
        alert(`Failed to fetch data: ${error.message}`);
    }
}
async function fetchDashboardDataForAccounting() {
    try {
        const response = await fetch('/Dashboard/GetAccountingsDashboard');

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();
        console.log(data)
        // Define the currency formatter for XAF
        const currencyFormatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'XAF',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0,
        });

        // Function to safely set element text with optional currency formatting
        function setElementText(id, value, isCurrency = false) {
            const element = document.getElementById(id);
            if (element) {
                if (typeof value === 'number' && isCurrency) {
                    element.textContent = currencyFormatter.format(value);
                } else {
                    element.textContent = value ?? '';
                }
            }
        }

        // Map MainAccountingDashboardStatistics fields to HTML elements
        setElementText('branchName', data.BranchName);
        setElementText('branchId', data.BranchId);
        setElementText('branchCode', data.BranchCode);
        setElementText('dashboardAccountType', data.DashboardAccountType);
        setElementText('cashInHandBalance', data.CashInHandBalance, true);
        setElementText('cashInBankBalance', data.CashInBankBalance, true);
        setElementText('totalSharesBalance', data.TotalSharesBalance, true);
        setElementText('preferenceShareBalance', data.PreferenceShareBalance, true);
        setElementText('ordinarySharesBalance', data.OrdinarySharesBalance, true);
        setElementText('depositBalance', data.DepositBalance, true);
        setElementText('savingsBalance', data.SavingsBalance, true);
        setElementText('gavBalance', data.GavBalance, true);
        setElementText('mmTNMobileMoneyBalance', data.MTNMobileMoneyBalance, true);
        setElementText('dailyCollectionsBalance', data.DailyCollectionsBalance, true);
        setElementText('mtnMobileMoneyMasterBalance', data.MTNMobileMoneyMasterBalance, true);
        setElementText('orangeMoneyMasterBalance', data.OrangeMoneyMasterBalance, true);
        setElementText('orangeMoneyBalance', data.OrangeMoneyBalance, true);
        setElementText('totalExpenseBalance', data.TotalExpenseBalance, true);
        setElementText('totalIncomeBalance', data.TotalIncomeBalance, true);
        setElementText('totalLiquidity', data.TotalLiquidity, true);
        console.log(setElementText('mtnMobileMoneyMasterBalance', data.MTNMobileMoneyMasterBalance, true))
        console.log(setElementText('mmTNMobileMoneyBalance', data.MTNMobileMoneyMasterBalance, true))
    } catch (error) {
        console.error("Error fetching data:", error.message);
        appalert(`Failed to fetch data: ${error.message}`, 0, 1);
    }
}

async function fetchDashboardDataForLoan() {
    try {
        const response = await fetch('/Dashboard/GetLoanDashboard');

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();

        // Define the currency formatter for XAF
        const currencyFormatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'XAF',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0,
        });

        // Function to safely set element text with optional currency formatting
        function setElementText(id, value, isCurrency = false) {
            const element = document.getElementById(id);
            if (element) {
                if (typeof value === 'number' && isCurrency) {
                    element.textContent = currencyFormatter.format(value);
                } else {
                    element.textContent = value ?? '';
                }
            }
        }

        // Map MainAccountingDashboardStatistics fields to HTML elements
        setElementTextInt('totalNumberOfLoans', data.TotalNumberOfLoans);
        setElementText('totalVolumeOfLoanGranted', data.TotalVolumeOfLoanGranted, true);
        setElementText('totalRemainingBalance', data.TotalRemainingBalance, true);

    } catch (error) {
        console.error("Error fetching data:", error.message);
        appalert(`Failed to fetch data: ${error.message}`, 0, 1);
    }
    function setElementTextInt(id, value) {
        const element = document.getElementById(id);
        if (element) {
            // Format the value to 0 decimal places if it's a number
            const formattedValue = typeof value === 'number'
                ? new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(value)
                : value ?? '';
            element.textContent = formattedValue;
        }
    }
}
