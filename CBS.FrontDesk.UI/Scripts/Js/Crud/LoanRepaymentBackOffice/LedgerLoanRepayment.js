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
function calculateVatOnlyLedger(loanId) {
    const interestInput = document.getElementById(`interest-${loanId}`);
    const vatInput = document.getElementById(`vat-${loanId}`);
    const loanAmountEl = document.querySelector(`#row-${loanId} .loan-amount`);

    const rawInterest = parseFloat((interestInput.value || "0").replace(/,/g, '')) || 0;
    const loanAmount = parseFloat(loanAmountEl?.getAttribute("data-loan-amount") || "0");
    const vatRate = 19.25;

    let vat = 0;
    if (loanAmount >= 2000000) {
        vat = Math.round(rawInterest * (vatRate / 100));
    }

    // Store raw interest
    interestInput.setAttribute("data-original", rawInterest.toFixed(0));

    // Update VAT field
    if (vatInput) {
        vatInput.value = vat.toLocaleString('en-US');
    }

    updateLedgerRowTotal(loanId);
}

function adjustInterestWithVatLedger(loanId) {
    const interestInput = document.getElementById(`interest-${loanId}`);
    const vatInput = document.getElementById(`vat-${loanId}`);

    const originalInterest = parseFloat(interestInput.getAttribute("data-original") || "0") || 0;
    const vat = parseFloat((vatInput?.value || "0").replace(/,/g, '')) || 0;

    const netInterest = Math.max(originalInterest - vat, 0);
    interestInput.value = netInterest.toFixed(0);

    updateLedgerRowTotal(loanId);
}

function updateLedgerRowTotal(loanId) {
    const capital = parseFloat(document.getElementById(`capital-${loanId}`)?.value) || 0;
    const interest = parseFloat(document.getElementById(`interest-${loanId}`)?.value) || 0;
    const vat = parseFloat(document.getElementById(`vat-${loanId}`)?.value.replace(/,/g, '')) || 0;
    const penalty = parseFloat(document.getElementById(`penalty-${loanId}`)?.value) || 0;

    const total = capital + interest + vat + penalty;

    const totalSpan = document.getElementById(`total-${loanId}`);
    if (totalSpan) {
        totalSpan.innerText = total.toLocaleString('en-US', { minimumFractionDigits: 0 });
    }

    calculateLedgerGrandTotal();
}

function calculateLedgerGrandTotal() {
    let grandTotal = 0;
    document.querySelectorAll(".total-span").forEach(el => {
        const val = parseFloat(el.innerText.replace(/,/g, '')) || 0;
        grandTotal += val;
    });

    document.getElementById("grandTotalRepayment").innerText = grandTotal.toLocaleString('en-US', {
        minimumFractionDigits: 0
    });
}
function calculateVatAndTotals(loanId) {
    console.log("Calculating for loanId:", loanId);
    var capitalInput = document.getElementById(`capital-${loanId}`);
    var interestInput = document.getElementById(`interest-${loanId}`);
    var penaltyInput = document.getElementById(`penalty-${loanId}`);
    var vatInput = document.getElementById(`vat-${loanId}`);

    if (!capitalInput || !interestInput || !penaltyInput || !vatInput) {
        console.error("One or more input elements are missing for loanId:", loanId);
        return;
    }

    var capital = parseFloat(capitalInput.value) || 0;
    var interest = parseFloat(interestInput.value) || 0;
    var penalty = parseFloat(penaltyInput.value) || 0;

    var loanAmountEl = document.querySelector(`#row-${loanId} .loan-amount`);
    if (!loanAmountEl) {
        console.error("Loan amount element missing for loanId:", loanId);
        return;
    }
    var loanAmount = parseFloat(loanAmountEl.getAttribute('data-loan-amount')) || 0;

    var vat = loanAmount >= 2000000 ? interest * 0.1925 : 0;
    vatInput.value = vat.toFixed(2);

    var total = capital + interest + penalty + vat;
    var totalSpan = document.getElementById(`total-${loanId}`);
    if (totalSpan) {
        totalSpan.innerText = total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    calculateGrandTotal();
}

function calculateGrandTotal() {
    var totalElements = document.querySelectorAll('.total-span');
    var grandTotal = 0;

    totalElements.forEach(function (el) {
        var val = parseFloat(el.innerText.replace(/,/g, '')) || 0; // remove commas before parsing
        grandTotal += val;
    });

    document.getElementById('grandTotalRepayment').innerText = grandTotal.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}



function collectDeposits() {
    var accountsToBeDebited = [];
    var loansToBeRefunded = [];
    var totalDebited = 0;
    var totalRepayment = 0;
    var selectedLoans = 0;
    var hasInvalidLoans = false;

    console.clear();

    // Member and branch info
    var memberReference = $('#customerId').val();
    var memberName = $('#memberName').val();
    var branchName = $('#branchName').val();
    var branchCode = $('#branchCode').val();
    var accountantName = $('#accountantName').val();

    // Chart of account (ledger source)
    var chartOfAccountId = $('#account_number').val();
    var chartOfAccountName = $('#account_number option:selected').text().trim();

    if (!chartOfAccountId || chartOfAccountId === "0") {
        appalert("❌ Please select a source ledger account (GL) before proceeding.", 3, 1);
        $('#account_number').focus();
        return null;
    }

    let transactionNote = getValidatedNote();
    if (!transactionNote) return null;

    // 🟩 Collect Loans
    $('#loanRepaymentTable tr').each(function () {
        let checkbox = $(this).find('.form-check-input');
        let loanId = $(this).find('td:eq(0)').text().trim();
        let loanAmountContracted = parseFloat($(this).find('.loan-amount').data('loan-amount')) || 0;

        let capital = parseFloat($(this).find('.capital-input').val()) || 0;
        let interest = parseFloat($(this).find('.interest-input').val()) || 0;
        let penalty = parseFloat($(this).find('.penalty-input').val()) || 0;
        let vat = parseFloat($(this).find('.vat-input').val().replace(/,/g, '')) || 0;

        // ✅ Use existing VAT & interest from input
        let totalAmount = capital + interest + vat + penalty;

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
            LoanAmountContracted: loanAmountContracted,
            MemberRefence: memberReference,
            MemberName: memberName,
            BranchName: branchName,
            BranchCode: branchCode,
            AccountantName: accountantName,
            Note: transactionNote
        });

        totalRepayment += totalAmount;
        selectedLoans++;
    });

    // Final validations
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

    console.log("✅ All validations passed. Ready to submit.");

    return [{
        MemberReference: memberReference,
        MemberName: memberName,
        BranchName: branchName,
        BranchCode: branchCode,
        AccountantName: accountantName,
        Note: transactionNote,
        LedgerChartOfAccountId: chartOfAccountId,
        ChartOfAccountName: chartOfAccountName,
        AccountsToBeDebited: accountsToBeDebited,
        LoansToBeRefunded: loansToBeRefunded,
        TotalDebited: totalDebited,
        TotalRepayment: totalRepayment
    }];
}


function getValidatedNote() {
    let noteInput = $('#Note').val().trim();
    let memberName = $('#memberName').val().trim();
    let memberId = $('#customerId').val().trim();
    let branchName = $('#branchName').val().trim();
    let branchCode = $('#branchCode').val().trim();
    let accountantName = $('#accountantName').val().trim();

    // ✅ Get selected ledger account details from dropdown
    let selectedLedgerId = $('#account_number').val();
    let selectedLedgerName = $('#account_number option:selected').text().trim();

    // Set safe defaults if fields are missing
    memberName = memberName || "the member";
    branchName = branchName || "their respective branch";
    branchCode = branchCode || "N/A";
    accountantName = accountantName || "the accountant";
    selectedLedgerName = selectedLedgerName || "the designated ledger account";
    selectedLedgerId = selectedLedgerId || "N/A";

    let defaultNote = `This loan repayment transaction for ${memberName} (Member ID: ${memberId}) from ${branchName} (Branch Code: ${branchCode}) has been processed using ledger account '${selectedLedgerName}' (Ledger ID: ${selectedLedgerId}). This repayment may also represent a standing order instruction from the bank, executed manually by ${accountantName} to ensure proper reconciliation and compliance with the institution's loan portfolio management policies.`;

    // ✅ If no note is provided, auto-fill
    if (!noteInput) {
        $('#Note').val(defaultNote);
        console.log("✅ Auto-filled transaction note for ledger-based loan repayment.");
        return defaultNote;
    }

    // ✅ Validate word count
    let wordCount = noteInput.split(/\s+/).length;
    if (wordCount < 20) {
        appalert("❌ The transaction note must contain at least 20 words. Please provide more detailed justification.", 3, 1);
        $('#Note').addClass('border border-danger');
        $('html, body').animate({ scrollTop: $('#Note').offset().top - 100 }, 300);
        return null;
    }

    // Remove error styling if valid
    $('#Note').removeClass('border border-danger');
    return noteInput;
}




function PostLoanRepayment() {
    var deposits = collectDeposits();

    if (!deposits) {
        return; // Stop execution if deposits collection failed
    }

    // Validate Note field
    var note = $('#Note').val().trim();
    var wordCount = note.split(/\s+/).filter(word => word.length > 0).length;

    if (note === "") {
        appalert("❌ Please enter a note for this transaction.", 3, 1);
        return;
    }

    if (wordCount < 20) {
        appalert("❌ The transaction note must contain at least 20 words.", 3, 1);
        return;
    }

    // ✅ Control that ledger must be selected
    var ledgerAccountId = $('#account_number').val();
    var ledgerAccountName = $('#account_number option:selected').text().trim();

    if (!ledgerAccountId || ledgerAccountId === "" || ledgerAccountId === "0") {
        appalert("❌ Please select a valid ledger account (GL) before proceeding.", 3, 1);
        $('#account_number').focus();
        return;
    }

    var accountsToBeDebited = deposits[0].AccountsToBeDebited;
    var loan = deposits[0].LoansToBeRefunded[0];

    var totalDebited = parseFloat(accountsToBeDebited.reduce((sum, acc) => sum + acc.Amount, 0)) || 0;
    var totalLoanAmount = parseFloat(loan.TotalAmount) || 0;
    var capital = parseFloat(loan.Capital) || 0;
    var interest = parseFloat(loan.Interest) || 0;
    var penalty = parseFloat(loan.Penalty) || 0;
    var loanAmountContracted = parseFloat(loan.LoanAmountContracted) || 0;
    var vat = (loanAmountContracted >= 2000000) ? (interest * 0.1925) : 0;
    vat = parseFloat(vat.toFixed(0));

    var memberName = $('#memberName').val().trim();
    var memberId = $('#customerId').val().trim();
    var branchName = $('#branchName').val().trim();
    var branchCode = $('#branchCode').val().trim();
    var accountantName = $('#accountantName').val().trim();

    // Loan Repayment Breakdown
    var loanSummary = `
        <br><strong>Loan Repayment Details:</strong><br>
        🔹 Contracted Loan Amount: ${formatCurrency(loanAmountContracted)}<br>
        🔹 Capital: ${formatCurrency(capital)}<br>
        🔹 Interest: ${formatCurrency(interest)}<br>
        🔹 Penalty: ${formatCurrency(penalty)}<br>
        ${vat > 0 ? `🔹 VAT (19.25% of Interest): ${formatCurrency(vat)}<br>` : ""}
        <hr>
        <strong>Total Loan Repayment Amount: ${formatCurrency(Math.round(totalLoanAmount))}</strong><br>
    `;

    // Posting context details
    var postingContext = `
        <hr>
        <strong>Posting Context:</strong><br>
        📍 Member: <strong>${memberName}</strong> (${memberId})<br>
        🏦 Branch: <strong>${branchName}</strong> (Code: ${branchCode})<br>
        🗄️ Ledger Account Used: <strong>${ledgerAccountName}</strong> (${ledgerAccountId})<br>
        👤 Posted by: <strong>${accountantName}</strong><br>
        ${note.toLowerCase().includes('standing order') ? 'ℹ️ <strong>Note:</strong> This transaction relates to a standing order repayment from the bank.<br>' : ''}
    `;

    var message = `Are you sure you want to proceed with this loan repayment?<br>
                   ${loanSummary} ${postingContext}`;

    // Confirmation and posting
    confirmTransaction(
        'Confirm Ledger-Based Loan Repayment Operation',
        message,
        '/LoanRepaymentBackOffice/PostRequestCash',
        deposits,
        'LoanRepaymentGLAccountNoneCash', ledgerAccountId
    );
}
appalert("❌ Some loans have missing values. Please enter valid amounts before proceeding.", 3, 1);
function confirmTransaction(title, message, ajaxUrl, data, operationType, ledgerAccountId) {
    console.log("🔍 Preparing to send data for operation:", operationType);
    console.log("📤 Data to be sent:", JSON.stringify(data, null, 2));

    alertify.confirm(title, message,
        function () {
            appalert('⏳ Processing transaction... Please wait.',1,1);
            $('.ajs-button.ajs-ok').attr('disabled', true).text('Processing...');

            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    accountToBeDebiteds: data[0].AccountsToBeDebited,
                    loanToBeRefundeds: data[0].LoansToBeRefunded,
                    operationType: "LoanRepaymentGLAccountNoneCash",
                    ledgerChartOfAccountId: ledgerAccountId
                }),
                success: function (response) {
                    console.log("✅ Response received:", response);
                    if (response && response.success) {
                        alertify.success('✅ Loan repayment posted successfully.');
                        successCallback(response, operationType);
                    } else {
                        if (!response) {
                            appalert("❌ Your session has expired. Please log in again.", 3, 1);
                        } else {
                            failureCallback(response);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    console.error("❌ Error while posting transaction:", error);
                    appalert("❌ An unexpected error occurred while processing the loan repayment. Please try again later.", 0, 1);
                },
                complete: function () {
                    $('.ajs-button.ajs-ok').attr('disabled', false).text('OK');
                }
            });
        },
        function () {
            appalert('🚫 Transaction was cancelled.', 3, 1);
        }
    );
}




function Reprint() {
    ReportView("LoanRepaymentBackOffice", null, "GetReport", null, null, "receipts", "ReportParameterLess");

}

function successCallback(response, operationType) {
    appalert(response.message, 1, 1);
    resetDepositorForm();
    switch (operationType) {
        case 'LoanRepaymentGLAccountNoneCash':
            GetMemberData($("#customerId").val(), '_LedgerLoanRepaymentForm', 'datalistingview', 'loan_repayment_gl');
            break;
        case 'LoanRepaymentMomocashCollection':
            GetMemberData($("#customerId").val(), '_LedgerLoanRepaymentForm', 'datalistingview', 'loan_repayment_gl');
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
    GetMemberData(memberId, '_LedgerLoanRepaymentForm', 'datalistingview', 'loan_repayment_gl');
    /*GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);


    AddORUpdateGen(Key, divToloadPV, partialView, path, "LoanRepaymentBackOffice");
    //calculateBalance();
}







