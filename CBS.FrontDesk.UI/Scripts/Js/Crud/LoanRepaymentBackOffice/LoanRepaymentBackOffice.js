$(document).ready(function () {
    // Event delegation for handling button clicks
    $(document).on("click", ".btn", function () {
        // Remove underline and blue color from all buttons
        $(".btn").removeClass("clicked");
        // Add underline and blue color to the clicked button
        $(this).addClass("clicked");
    });

    // Handle checkbox state changes
    // Format date mask



});



function updateTotals() {
    let totalDebited = 0;

    // 🔹 Validate account entries
    document.querySelectorAll("#memberAccountsTable tr").forEach(row => {
        let balanceElement = row.cells[3];
        let amountInput = row.querySelector(".amount-input");
        let statusIndicator = row.querySelector(".status-indicator");

        if (balanceElement && amountInput && statusIndicator) {
            let balance = parseFloat(balanceElement.innerText.replace(/[^\d.-]/g, '')) || 0;
            let amount = parseFloat(amountInput.value) || 0;

            if (amount > balance) {
                statusIndicator.innerHTML = " ❌";
                statusIndicator.style.color = "red";
            } else if (amount > 0) {
                statusIndicator.innerHTML = " ✅";
                statusIndicator.style.color = "green";
            } else {
                statusIndicator.innerHTML = "";
            }

            totalDebited += amount;
        }
    });

    document.getElementById("totalDebitedAmount").innerText = formatCurrency(totalDebited);

    let totalRepayment = 0;
    let totalVAT = 0;

    // 🔹 Loop through each loan and just READ values (no recalculation!)
    document.querySelectorAll("#loanRepaymentTable tr").forEach(row => {
        let loanId = row.id.replace("row-", "");

        let capital = parseFloat(document.getElementById("capital-" + loanId)?.value?.replace(/,/g, '') || 0);
        let interest = parseFloat(document.getElementById("interest-" + loanId)?.value?.replace(/,/g, '') || 0);
        let penalty = parseFloat(document.getElementById("penalty-" + loanId)?.value?.replace(/,/g, '') || 0);
        let vat = parseFloat(document.getElementById("vat-" + loanId)?.value?.replace(/,/g, '') || 0);

        // ✅ Do NOT recalculate anything. Just use values as-is
        let rowTotal = capital + interest + penalty + vat;

        const totalField = document.getElementById("total-" + loanId);
        if (totalField) {
            totalField.innerText = formatCurrencyNoName(rowTotal);
        }

        totalRepayment += rowTotal;
        totalVAT += vat;
    });

    // 🔹 Update footer totals
    document.getElementById("totalRepaymentAmount").innerText = formatCurrencyNoName(totalRepayment);
    document.getElementById("calculatedVat").innerText = formatCurrencyNoName(totalVAT);

    let remainingBalance = totalDebited - totalRepayment;
    document.getElementById("remainingBalance").innerText = formatCurrency(remainingBalance);

    let balanceIndicator = document.getElementById("balanceIndicator");
    if (balanceIndicator) {
        if (remainingBalance === 0) {
            balanceIndicator.innerHTML = " ✅";
            balanceIndicator.style.color = "green";
        } else {
            balanceIndicator.innerHTML = " ❌";
            balanceIndicator.style.color = "red";
        }
    }
}

function bindInterestFieldEvents() {
    document.querySelectorAll(".interest-input").forEach(input => {
        input.replaceWith(input.cloneNode(true)); // Remove old events
    });

    document.querySelectorAll(".interest-input").forEach(input => {
        const loanId = input.id.replace("interest-", "");
        const vatRate = parseFloat(input.dataset.vatrate || "0");
        const loanAmount = parseFloat(input.dataset.loanamount || "0");

        // 🔹 On INPUT: show calculated VAT only
        input.addEventListener("input", function () {
            const rawInterest = parseFloat(this.value) || 0;
            let vat = 0;
            if (loanAmount >= 2000000) {
                vat = Math.round(rawInterest * (vatRate / 100));
            }

            const vatInput = document.getElementById("vat-" + loanId);
            if (vatInput) {
                vatInput.value = formatCurrencyNoName(vat);
            }

            updateTotals();
        });

        // 🔹 On BLUR: apply VAT adjustment to interest field
        input.addEventListener("blur", function () {
            const rawInterest = parseFloat(this.value) || 0;
            let vat = 0;
            if (loanAmount >= 2000000) {
                vat = Math.round(rawInterest * (vatRate / 100));
            }

            const netInterest = Math.max(rawInterest - vat, 0);
            this.value = netInterest.toFixed(0);

            const vatInput = document.getElementById("vat-" + loanId);
            if (vatInput) {
                vatInput.value = formatCurrencyNoName(vat);
            }

            updateTotals();
        });
    });
}


// 🔹 Bind interest field events once DOM is fully loaded
document.addEventListener("DOMContentLoaded", function () {
    bindInterestFieldEvents(); // Initial bind for static content
});

// 🔹 Rebind only once for dynamically loaded content
const datalist = document.getElementById("datalistingview");
if (datalist) {
    let isBound = false;

    const observer = new MutationObserver((mutationsList, observerInstance) => {
        if (!isBound) {
            bindInterestFieldEvents();
            isBound = true;

            // Disconnect after first successful dynamic binding
            observerInstance.disconnect();
            console.log("✅ Dynamic content observed and bound. Observer disconnected.");
        }
    });

    observer.observe(datalist, { childList: true, subtree: true });
}







function collectDeposits() {
    var accountsToBeDebited = [];
    var loansToBeRefunded = [];
    var totalDebited = 0;
    var totalRepayment = 0;
    var selectedAccounts = 0;
    var selectedLoans = 0;
    var hasInvalidAccounts = false;
    var hasInvalidLoans = false;

    console.clear();

    // ✅ Step 1: Collect checked accounts
    $('#memberAccountsTable tr').each(function () {
        let checkbox = $(this).find('.form-check-input');
        if (!checkbox.is(':checked')) return;

        let amountInput = $(this).find('.amount-input');
        let statusIndicator = $(this).find('.status-indicator');
        let amount = parseFloat(amountInput.val()) || 0;
        let balance = parseFloat($(this).find('.balance-span').text().replace(/[^\d.-]/g, '')) || 0;
        let accountNumber = $(this).find('td:eq(1)').text().trim();
        let accountType = $(this).find('td:eq(2)').text().trim();
        let productId = $(this).find('td:eq(0)').text().trim();

        if (amount <= 0) {
            statusIndicator.html("❌").css("color", "red");
            hasInvalidAccounts = true;
            return;
        }

        if (amount > balance) {
            statusIndicator.html("❌").css("color", "red");
            hasInvalidAccounts = true;
        } else {
            statusIndicator.html("✅").css("color", "green");
        }

        accountsToBeDebited.push({
            AccountNumber: accountNumber,
            Amount: amount,
            ProductId: productId,
            AccountType: accountType
        });

        totalDebited += amount;
        selectedAccounts++;
    });

    if (selectedAccounts === 0) {
        appalert("❌ Please select at least one account and enter a valid amount.", 3, 1);
        return null;
    }

    if (hasInvalidAccounts) {
        appalert("❌ Some selected accounts have invalid debit amounts. Please correct them.", 3, 1);
        return null;
    }

    let transactionNote = getValidatedNote();
    if (!transactionNote) return null;

    // ✅ Step 2: Collect checked loans
    $('#loanRepaymentTable tr').each(function () {
        let checkbox = $(this).find('.loan-confirmation-checkbox');
        if (!checkbox.is(':checked')) return;

        let loanId = $(this).find('td:eq(0)').text().trim();
        let capital = parseFloat($(this).find('.capital-input').val()) || 0;
        let interest = parseFloat($(this).find('.interest-input').val()) || 0;
        let vat = parseFloat($(this).find('.vat-input').val().replace(/,/g, '')) || 0;
        let penalty = parseFloat($(this).find('.penalty-input').val()) || 0;
        let totalAmount = capital + interest + vat + penalty;

        $(this).find('.total-span').html(
            totalAmount > 0
                ? totalAmount.toLocaleString('en-US', { minimumFractionDigits: 0 })
                : "⚠️ Missing Values"
        ).css("color", totalAmount > 0 ? "" : "orange");

        if (totalAmount <= 0) {
            hasInvalidLoans = true;
            return;
        }

        loansToBeRefunded.push({
            LoanId: loanId,
            Capital: capital,
            Interest: interest,
            Penalty: penalty,
            Vat: vat,
            TotalAmount: totalAmount,
            LoanAmount: parseFloat($(this).find('td:eq(2)').text().trim().replace(/[^\d.-]/g, '')) || 0,
            MemberRefence: $('#customerId').val(),
            Note: transactionNote
        });

        totalRepayment += totalAmount;
        selectedLoans++;
    });

    $('#totalRepaymentAmount').text(totalRepayment.toLocaleString('en-US', { minimumFractionDigits: 0 }));

    if (selectedLoans === 0) {
        appalert("❌ Please select and enter values for at least one loan to process repayment.", 3, 1);
        return null;
    }

    if (selectedLoans > 1) {
        appalert("❌ Only one loan can be paid at an instant. Please deselect other loans.", 3, 1);
        return null;
    }

    if (hasInvalidLoans) {
        appalert("❌ Some selected loans have missing or invalid values. Please check before proceeding.", 3, 1);
        return null;
    }

    console.log("✅ FINAL VALIDATION PASSED. Ready to Submit!");
    return [{
        AccountToBeDebiteds: accountsToBeDebited,
        LoanToBeRefundeds: loansToBeRefunded
    }];
}


function getValidatedNote() {
    let noteInput = $('#Note').val().trim();
    let memberName = $('#memberName').val().trim(); // Member's Name
    let memberId = $('#customerId').val().trim(); // Member ID
    let branchName = $('#branchName').val().trim(); // Branch Name
    let branchCode = $('#branchCode').val().trim(); // Branch Code
    let accountantName = $('#accountantName').val().trim(); // Accountant's Name

    // Fallback if values are missing
    memberName = memberName || "the member";
    branchName = branchName || "their respective branch";
    branchCode = branchCode || "N/A";
    accountantName = accountantName || "the accountant";

    let defaultNote = `This back office operation for loan repayment is due to the fact that ${memberName} (ID: ${memberId}) from ${branchName} (Branch Code: ${branchCode}) failed to pay their loan as planned. The repayment process has been manually initiated by ${accountantName} to ensure compliance with financial obligations and to maintain the integrity of the institution's loan portfolio.`;

    // ✅ If no note is provided, use the default note
    if (!noteInput) {
        $('#Note').val(defaultNote);
        return defaultNote;
    }

    // ✅ Count words in the user's note
    let wordCount = noteInput.split(/\s+/).length;

    if (wordCount < 20) {
        appalert("❌ The transaction note must contain at least 20 words. Please provide more details.", 3, 1);
        return null;
    }

    return noteInput;
}




function PostLoanRepayment() {
    var deposits = collectDeposits();
    if (!deposits) return;

    var note = $('#Note').val().trim();
    var wordCount = note.split(/\s+/).filter(word => word.length > 0).length;
    console.log("🧾 Raw deposits object returned from collectDeposits():", deposits);

    if (!deposits) {
        console.error("❌ collectDeposits() returned null or undefined.");
        return;
    }
    console.log("✅ collectDeposits - Final structure being returned:", {
        AccountToBeDebiteds: accountsToBeDebited,
        LoanToBeRefundeds: loansToBeRefunded
    });

    if (note === "") {
        appalert("❌ Please enter a note for this transaction.", 3, 1);
        return;
    }

    if (wordCount < 20) {
        appalert("❌ The note must contain at least 20 words.", 3, 1);
        return;
    }
    var accountsToBeDebited = deposits[0].AccountToBeDebiteds || [];

    if (!deposits[0].LoanToBeRefundeds || deposits[0].LoanToBeRefundeds.length === 0) {
        appalert("❌ No loan selected for repayment. Please check the loan section.", 3, 1);
        return;
    }

    /*var loansToBeRefunded = deposits[0].LoansToBeRefunded[0];*/
    if (!Array.isArray(deposits[0].LoanToBeRefundeds)) {
        console.error("🚨 LoanToBeRefundeds is not an array:", deposits[0].LoanToBeRefundeds);
        appalert("❌ Invalid loan data returned. Please verify form entries.", 3, 1);
        return;
    }

    if (deposits[0].LoanToBeRefundeds.length === 0) {
        console.error("🚨 LoanToBeRefundeds array is empty.");
        appalert("❌ No loan has been entered for repayment.", 3, 1);
        return;
    }

    var loansToBeRefunded = deposits[0].LoanToBeRefundeds[0];
    var totalDebited = parseFloat(accountsToBeDebited.reduce((sum, acc) => sum + acc.Amount, 0)) || 0;
    var totalLoanAmount = parseFloat(loansToBeRefunded.TotalAmount) || 0;

    var capital = parseFloat(loansToBeRefunded.Capital) || 0;
    var interest = parseFloat(loansToBeRefunded.Interest) || 0;
    var penalty = parseFloat(loansToBeRefunded.Penalty) || 0;
    var vat = parseFloat(loansToBeRefunded.Vat) || 0; // ✅ Read VAT from collected data
    var loanAmountContracted = parseFloat(loansToBeRefunded.LoanAmount) || 0;

    console.log("🔹 Loan Amount Contracted:", loanAmountContracted);
    console.log("🔹 Total Debited:", totalDebited);
    console.log("🔹 Total Loan Amount:", totalLoanAmount);
    console.log("🔹 Capital:", capital);
    console.log("🔹 Interest:", interest);
    console.log("🔹 Penalty:", penalty);
    console.log("🔹 VAT:", vat);

    if (Math.round(totalDebited) !== Math.round(totalLoanAmount)) {
        appalert(`❌ Error: The total debited amount (${formatCurrency(totalDebited)}) must match the total loan repayment amount (${formatCurrency(totalLoanAmount)}).`, 3, 1);
        return;
    }

    // 🧾 Accounts breakdown
    var accountsSummary = `<br><strong>Accounts to be Debited:</strong><br>`;
    accountsToBeDebited.forEach(account => {
        accountsSummary += `✔️ ${account.AccountType} (${account.AccountNumber}) : ${formatCurrency(account.Amount)}<br>`;
    });
    accountsSummary += `<hr><strong>Total Amount to be Debited: ${formatCurrency(totalDebited)}</strong><br>`;

    // 📋 Loan breakdown
    var loanSummary = `
            <br><strong>Loan Repayment Details:</strong><br>
            🔹 Contracted Loan Amount: ${formatCurrency(loanAmountContracted)}<br>
            🔹 Capital: ${formatCurrency(capital)}<br>
            🔹 Interest: ${formatCurrency(interest)}<br>
            🔹 Penalty: ${formatCurrency(penalty)}<br>
            ${vat > 0 ? `🔹 VAT (19.25% of Interest): ${formatCurrency(vat)}<br>` : ""}
            <hr>
            <strong>Total Loan Amount to be Paid: ${formatCurrency(Math.round(totalLoanAmount))}</strong><br>
        `;

    var message = `Are you sure you want to proceed with this loan repayment?<br>
                       ${accountsSummary} ${loanSummary}`;

    confirmTransaction(
        'Confirm Loan Repayment Operation',
        message,
        '/LoanRepaymentBackOffice/PostRequestCash',
        deposits,
        'LoanRepaymentMomocashCollection'
    );
}
function formatCurrencyNoName(amount) {
    if (isNaN(amount) || amount === undefined) {
        console.warn("🚨 formatCurrency() received an invalid amount:", amount);
        return "0.0 XAF"; // Default value
    }
    return amount.toLocaleString('fr-FR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
}
function formatCurrency(amount) {
    if (isNaN(amount) || amount === undefined) {
        console.warn("🚨 formatCurrency() received an invalid amount:", amount);
        return "0.0 XAF"; // Default value
    }
    return amount.toLocaleString('fr-FR', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + " XAF";
}



function Reprint() {
    ReportView("LoanRepaymentBackOffice", null, "GetReport", null, null, "receipts", "ReportParameterLess");

}
function confirmTransaction(title, message, ajaxUrl, data, operationType) {
    console.log("🔍 Data to be sent:", JSON.stringify(data, null, 2)); // Debugging - Log full JSON data before sending

    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    accountToBeDebiteds: data[0].AccountToBeDebiteds,
                    loanToBeRefundeds: data[0].LoanToBeRefundeds
                }), // Ensure correct structure
                success: function (response) {
                    if (response && response.success) {
                        successCallback(response, operationType);
                    } else {
                        if (!response) {
                            alert("Your session is expired.");
                        } else {
                            failureCallback(response);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    appalert("Your session is expired or an error occurred while processing the transaction. Please try again later.", 0, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

function successCallback(response, operationType) {
    appalert(response.message, 1, 1);
    resetDepositorForm();
    switch (operationType) {
        case 'CashInMomocashCollection':
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'cashin');
            break;
        case 'LoanRepaymentMomocashCollection':
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'repayment');
            break;
        default:
            break;
    }
}

function failureCallback(response) {
    appalert(response.message || "Your session is expired Or An error occurred while processing the transaction", 3, 1);
}






function getSelectedAccountNumbers() {
    var selectedAccountNumbers = [];
    $('#myDataTableT tbody tr').each(function () {
        // Check if the checkbox in this row is selected
        if ($(this).find('.form-check-input').prop('checked')) {
            // Get the account number from the current row
            var accountNumber = $(this).find('td:eq(0)').text();
            // Add the account number to the array
            selectedAccountNumbers.push(accountNumber);
        }
    });
    // Join the array elements into a comma-separated string
    return selectedAccountNumbers.join(', ');
}





function GetObject(KEY, divToLoadData, partialView, path) {
    if (KEY.trim() === "") {
        alert("KEY is empty. Please provide a valid KEY.");
        return; // Exit the function
    }
    AddORUpdateGen(KEY, divToLoadData, partialView, path, "LoanRepaymentBackOffice");


}
function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "LoanRepaymentBackOffice");

}
function GetMember() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    GetMemberData(memberId, '_AccountAndLoanListingForm', 'datalistingview', operation);
    /*GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);


    AddORUpdateGen(Key, divToloadPV, partialView, path, "LoanRepaymentBackOffice");
    //calculateBalance();
}






function calculateVatOnly(loanId) {
    const interestInput = document.getElementById("interest-" + loanId);
    const vatInput = document.getElementById("vat-" + loanId);

    const rawInterest = parseFloat((interestInput.value || "0").replace(/,/g, '')) || 0;
    const vatRate = parseFloat(interestInput.dataset.vatrate || "0");
    const loanAmount = parseFloat(interestInput.dataset.loanamount || "0");

    let vat = 0;
    if (loanAmount >= 2000000) {
        vat = Math.round(rawInterest * (vatRate / 100));
    }

    vatInput.value = vat.toLocaleString('en-US');

    // 🔐 Store the original value
    interestInput.dataset.original = rawInterest.toFixed(0);

    updateGrandTotals();
}



function calculateVatAndAdjustInterest(loanId) {
    const interestInput = document.getElementById("interest-" + loanId);
    const vatInput = document.getElementById("vat-" + loanId);
    const capitalInput = document.getElementById("capital-" + loanId);
    const penaltyInput = document.getElementById("penalty-" + loanId);
    const totalField = document.getElementById("total-" + loanId);

    const originalInterest = parseFloat(interestInput.dataset.original || "0") || 0;
    const vat = parseFloat((vatInput?.value || "0").replace(/,/g, '')) || 0;
    const capital = parseFloat(capitalInput?.value || "0") || 0;
    const penalty = parseFloat(penaltyInput?.value || "0") || 0;

    const netInterest = Math.max(originalInterest - vat, 0);

    interestInput.value = netInterest.toFixed(0);

    // ✅ Update row total cell
    const rowTotal = capital + netInterest + vat + penalty;
    if (totalField) {
        totalField.innerText = formatCurrencyNoName(rowTotal);
    }

    updateGrandTotals(); // Recalculate footer totals
}



function updateGrandTotals() {
    let totalRepayment = 0;
    let totalVat = 0;
    let totalDebited = 0;

    // 🔹 Sum account entries
    document.querySelectorAll(".amount-input").forEach(input => {
        const val = parseFloat(input.value) || 0;
        totalDebited += val;
    });

    // 🔹 Sum loan repayments
    document.querySelectorAll("#loanRepaymentTable tr").forEach(row => {
        const loanId = row.id.replace("row-", "");

        const interest = parseFloat(document.getElementById("interest-" + loanId)?.value || 0); // already net!
        const vat = parseFloat(document.getElementById("vat-" + loanId)?.value.replace(/,/g, '') || 0);
        const capital = parseFloat(document.getElementById("capital-" + loanId)?.value || 0);
        const penalty = parseFloat(document.getElementById("penalty-" + loanId)?.value || 0);

        const rowTotal = capital + interest + vat + penalty; // ✅ no double VAT deduction
        totalRepayment += rowTotal;
        totalVat += vat;
    });

    // 🔹 Update UI
    document.getElementById("totalRepaymentAmount").innerText = formatCurrencyNoName(totalRepayment);
    document.getElementById("calculatedVat").innerText = formatCurrencyNoName(totalVat);
    document.getElementById("totalDebitedAmount").innerText = formatCurrency(totalDebited);

    const balance = totalDebited - totalRepayment;
    document.getElementById("remainingBalance").innerText = formatCurrency(balance);

    const balanceIndicator = document.getElementById("balanceIndicator");
    if (balanceIndicator) {
        if (balance === 0) {
            balanceIndicator.innerHTML = " ✅";
            balanceIndicator.style.color = "green";
        } else {
            balanceIndicator.innerHTML = " ❌";
            balanceIndicator.style.color = "red";
        }
    }
}
