//// Function to fetch and update the dashboard every 5 seconds
//async function fetchDashboardData() {
//    try {
//        const response = await fetch('/Dashboard/GetLiveDashboard');

//        if (!response.ok) {
//            throw new Error(`HTTP error! Status: ${response.status}`);
//        }

//        const data = await response.json();

//        // Assuming your data has fields like totalCashInAmount, totalCashOutAmount, etc.
//        document.getElementById('totalCashInAmount').textContent = data.TotalCashInAmount;
//        document.getElementById('totalCashOutAmount').textContent = data.TotalCashOutAmount;
//        document.getElementById('newMembers').textContent = data.NewMembers;
//        document.getElementById('totalNumberOfCashIn').textContent = data.NumberOfCashIn;
//        document.getElementById('loanDisbursements').textContent = data.LoanDisbursements;
//    } catch (error) {
//        console.error("Error fetching data:", error.message);
//        alert(`Failed to fetch data: ${error.message}`);
//    }
//}

//// Call the function every 5 seconds
//setInterval(fetchDashboardData, 5000);

//// Initial call to load data immediately on page load
//fetchDashboardData();


document.addEventListener("DOMContentLoaded", function () {
    async function fetchDashboardData() {
        var branchid = $("#branch_id").val();
        try {
            const response = await fetch('/Dashboard/GetLiveOpenedBranchDashboard?branchid=' + branchid);

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
                        // Format numbers as currency
                        element.textContent = currencyFormatter.format(value);
                    } else {
                        // Display strings or non-currency values directly
                        element.textContent = value ?? '';
                    }
                }
            }

            // Mapping values from GeneralDailyDashboardDto to HTML elements
            setElementText('id', data.Id);
            setElementText('branchId', data.BranchId);
            setElementText('branchName', data.BranchName);
            setElementText('branchCode', data.BranchCode);
            setElementText('numberOfCashIn', data.NumberOfCashIn);
            setElementText('numberOfCashOut', data.NumberOfCashOut);
            setElementText('totalCashInAmount', data.TotalCashInAmount, true);
            setElementText('totalCashOutAmount', data.TotalCashOutAmount, true);
            setElementText('newMembers', data.NewMembers);
            setElementText('closedAccounts', data.ClosedAccounts);
            setElementText('activeAccounts', data.ActiveAccounts);
            setElementText('dormantAccounts', data.DormantAccounts);
            setElementText('loanDisbursements', data.LoanDisbursements, true);
            setElementText('loanRepayments', data.LoanRepayments, true);
            setElementText('serviceFeesCollected', data.ServiceFeesCollected, true);
            setElementText('interestPaid', data.InterestPaid, true);
            setElementText('vat', data.Vat, true);
            setElementText('penalties', data.Penalties, true);
            setElementText('dailyExpenses', data.DailyExpenses, true);
            setElementText('ordinaryShares', data.OrdinaryShares, true);
            setElementText('preferenceShares', data.PreferenceShares, true);
            setElementText('savings', data.Savings, true);
            setElementText('deposits', data.Deposits, true);
            setElementText('cashInHand57', data.CashInHand57, true);
            setElementText('cashInHand56', data.CashInHand56, true);
            setElementText('mtnMobileMoney', data.MTNMobileMoney, true);
            setElementText('numberOfCashOutMTN', data.NumberOfCashOutMTN);
            setElementText('numberOfCashOutOrange', data.NumberOfCashOutOrange);
            setElementText('numberOfCashInMTN', data.NumberOfCashInMTN);
            setElementText('mobileMoneyCashOut', data.MobileMoneyCashOut);
            
            setElementText('numberOfLoanFee', data.NumberOfLoanFee);
            setElementText('numberOfLoanDisbursementFee', data.NumberOfLoanDisbursementFee);
            setElementText('numberOfCashInOrange', data.NumberOfCashInOrange);
            setElementText('orangeMoney', data.OrangeMoney, true);
            setElementText('dailyCollectionCashOut', data.DailyCollectionCashOut, true);

            setElementText('dailyCollectionCashIn', data.DailyCollectionCashIn, true);
            setElementText('momocashCollection', data.MomocashCollection, true);
            setElementText('transfer', data.Transfer, true);
            setElementText('primaryTillOpenOfDayBalance', data.PrimaryTillOpenOfDayBalance, true);
            setElementText('subTillTillOpenOfDayBalance', data.SubTillTillOpenOfDayBalance, true);
            setElementText('date', data.Date);
            setElementText('accountingDate', data.AccountingDate);
            setElementText('subTillBalance', data.SubTillBalance, true);
            setElementText('primaryTillBalance', data.PrimaryTillBalance, true);
            setElementText('cashReplenishmentSubTill', data.CashReplenishmentSubTill, true);
            setElementText('cashReplenishmentPrimaryTill', data.CashReplenishmentPrimaryTill, true);

        } catch (error) {
            console.error("Error fetching data:", error.message);
            appalert(`Failed to fetch data: ${error.message}`, 0, 1);
        }
    }

    // Call fetchDashboardData every 5 seconds
    setInterval(fetchDashboardData, 5000);

    // Initial call
    fetchDashboardData();
});
