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


// 🔹 Bind on DOM ready
document.addEventListener("DOMContentLoaded", function () {
    bindInterestFieldEvents();
});

// 🔹 Rebind if dynamic content is injected
const observer = new MutationObserver(() => {
    bindInterestFieldEvents();
});

const datalist = document.getElementById("datalistingview");
if (datalist) {
    observer.observe(datalist, { childList: true, subtree: true });
}

//function updateTotals() {
//    let totalDebited = 0;

//    // Validate Account Table: Amount entered must not exceed balance
//    document.querySelectorAll("#memberAccountsTable tr").forEach(row => {
//        let balanceElement = row.cells[3]; // Balance column
//        let amountInput = row.querySelector(".amount-input");
//        let statusIndicator = row.querySelector(".status-indicator");

//        if (balanceElement && amountInput && statusIndicator) {
//            let balance = parseFloat(balanceElement.innerText.replace(/[^\d.-]/g, '')) || 0;
//            let amount = parseFloat(amountInput.value) || 0;

//            if (amount > balance) {
//                statusIndicator.innerHTML = " ❌"; // Red X for invalid entry
//                statusIndicator.style.color = "red";
//            } else if (amount > 0) {
//                statusIndicator.innerHTML = " ✅"; // Green Tick if valid
//                statusIndicator.style.color = "green";
//            } else {
//                statusIndicator.innerHTML = ""; // Clear indicator if no amount entered
//            }

//            totalDebited += amount;
//        }
//    });

//    // Update the Total Debited Amount
//    document.getElementById("totalDebitedAmount").innerText = formatCurrency(totalDebited);

//    let totalRepayment = 0;
//    let totalVAT = 0;

//    // Update each loan row's total dynamically
//    document.querySelectorAll("#loanRepaymentTable tr").forEach(row => {
//        let loanId = row.id.replace("row-", "");
//        let loanAmountElement = row.cells[2]; // Loan Amount Column

//        let loanAmount = parseFloat(loanAmountElement.innerText.replace(/[^\d.-]/g, '')) || 0;
//        let capitalInput = document.getElementById("capital-" + loanId);
//        let interestInput = document.getElementById("interest-" + loanId);
//        let penaltyInput = document.getElementById("penalty-" + loanId);
//        let vatInput = document.getElementById("vat-" + loanId); // ✅ NEW VAT FIELD

//        let capital = capitalInput ? parseFloat(capitalInput.value) || 0 : 0;
//        let interest = interestInput ? parseFloat(interestInput.value) || 0 : 0;
//        let penalty = penaltyInput ? parseFloat(penaltyInput.value) || 0 : 0;

//        // ✅ Calculate VAT (19% on Interest ONLY if Loan Amount >= 2,000,000 XAF)
//        let vat = loanAmount >= 2000000 ? interest * 0.1925 : 0;
//        vat = parseFloat(vat.toFixed(0)); // Format VAT correctly
//        vatInput.value = formatCurrencyNoName(vat); // ✅ Update VAT input field
//        totalVAT += vat;

//        let rowTotal = capital + interest + penalty + vat;
//        document.getElementById("total-" + loanId).innerText = formatCurrencyNoName(rowTotal);

//        totalRepayment += rowTotal;
//    });

//    document.getElementById("totalRepaymentAmount").innerText = formatCurrencyNoName(totalRepayment);
//    document.getElementById("calculatedVat").innerText = formatCurrencyNoName(totalVAT); // Display total VAT

//    let remainingBalance = totalDebited - totalRepayment;
//    document.getElementById("remainingBalance").innerText = formatCurrency(remainingBalance);

//    let balanceIndicator = document.getElementById("balanceIndicator");
//    if (remainingBalance === 0) {
//        balanceIndicator.innerHTML = " ✅";
//        balanceIndicator.style.color = "green";
//    } else {
//        balanceIndicator.innerHTML = " ❌";
//        balanceIndicator.style.color = "red";
//    }
//}




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

    // ✅ Step 1: Collect Accounts
    $('#memberAccountsTable tr').each(function () {
        let checkbox = $(this).find('.form-check-input');
        let amountInput = $(this).find('.amount-input');
        let statusIndicator = $(this).find('.status-indicator');
        let amount = parseFloat(amountInput.val()) || 0;
        let balance = parseFloat($(this).find('.balance-span').text().replace(/[^\d.-]/g, '')) || 0;
        let accountNumber = $(this).find('td:eq(1)').text().trim();
        let accountType = $(this).find('td:eq(2)').text().trim();
        let productId = $(this).find('td:eq(0)').text().trim();

        if (amount > 0) {
            if (amount > balance) {
                statusIndicator.html("❌").css("color", "red");
                hasInvalidAccounts = true;
            } else {
                statusIndicator.html("✅").css("color", "green");
                checkbox.prop('checked', true);
            }

            accountsToBeDebited.push({
                AccountNumber: accountNumber,
                Amount: amount,
                ProductId: productId,
                AccountType: accountType
            });

            totalDebited += amount;
            selectedAccounts++;
        }
    });

    if (selectedAccounts === 0) {
        appalert("❌ Please enter an amount and select at least one account from the account table.", 3, 1);
        return null;
    }

    if (hasInvalidAccounts) {
        appalert("❌ Some accounts have invalid debit amounts (greater than balance). Please correct them.", 3, 1);
        return null;
    }

    let transactionNote = getValidatedNote();
    if (!transactionNote) return null;

    // ✅ Step 2: Collect Loans
    $('#loanRepaymentTable tr').each(function () {
        let checkbox = $(this).find('.form-check-input');
        let loanId = $(this).find('td:eq(0)').text().trim();

        // ✅ Read values directly from table inputs
        let capital = parseFloat($(this).find('.capital-input').val()) || 0;
        let interest = parseFloat($(this).find('.interest-input').val()) || 0;
        let vat = parseFloat($(this).find('.vat-input').val().replace(/,/g, '')) || 0;
        let penalty = parseFloat($(this).find('.penalty-input').val()) || 0;

        // ✅ Use values as-is, do not recalculate VAT or interest
        let totalAmount = capital + interest + vat + penalty;

        // ✅ Show total in the row
        $(this).find('.total-span').html(totalAmount.toLocaleString('en-US', {
            minimumFractionDigits: 0
        })).css("color", "");

        if (totalAmount > 0) {
            checkbox.prop('checked', true);
        } else {
            $(this).find('.total-span').html("⚠️ Missing Values").css("color", "orange");
            hasInvalidLoans = true;
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
        appalert("❌ Please enter an amount and select at least one loan to process repayment.", 3, 1);
        return null;
    }

    if (selectedLoans > 1) {
        appalert("❌ Only one loan can be paid at an instant. Please deselect other loans.", 3, 1);
        return null;
    }

    if (hasInvalidLoans) {
        appalert("❌ Some loans have missing values. Please enter valid amounts before proceeding.", 3, 1);
        return null;
    }

    //if (Math.round(totalDebited) !== Math.round(totalRepayment)) {
    //    appalert(`❌ The sum of Capital, Interest, and Penalty (including VAT) (${formatCurrency(totalRepayment)}) must be equal to the total amount to be debited (${formatCurrency(totalDebited)}).`, 3, 1);
    //    return null;
    //}

    console.log("✅ FINAL VALIDATION PASSED. Ready to Submit!");
    return [{ AccountToBeDebiteds: accountsToBeDebited, LoanToBeRefundeds: loansToBeRefunded }];
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

    if (note === "") {
        appalert("❌ Please enter a note for this transaction.", 3, 1);
        return;
    }

    if (wordCount < 20) {
        appalert("❌ The note must contain at least 20 words.", 3, 1);
        return;
    }

    var accountsToBeDebited = deposits[0].AccountsToBeDebited || [];
    var loansToBeRefunded = deposits[0].LoansToBeRefunded[0];

    var totalDebited = parseFloat(accountsToBeDebited.reduce((sum, acc) => sum + acc.Amount, 0)) || 0;
    var totalLoanAmount = parseFloat(loansToBeRefunded.TotalAmount) || 0;

    var capital = parseFloat(loansToBeRefunded.Capital) || 0;
    var interest = parseFloat(loansToBeRefunded.Interest) || 0;
    var penalty = parseFloat(loansToBeRefunded.Penalty) || 0;
    var vat = parseFloat(loansToBeRefunded.Vat) || 0; // ✅ Read VAT from collected data
    var loanAmountContracted = parseFloat(loansToBeRefunded.LoanAmountContracted) || 0;

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







