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

var vatMap = {};

function previewVat(loanId, interestValue, vatRate, loanAmount) {
    var interestAmount = parseFloat(interestValue) || 0;
    var vat = 0;

    // Only apply VAT if loan amount >= 2,000,000
    if (parseFloat(loanAmount) >= 2000000) {
        vat = Math.round(interestAmount * (parseFloat(vatRate) / 100));
    }

    // Store VAT per loan
    vatMap[loanId] = {
        original: interestAmount,
        vat: vat
    };

    // Recalculate and update total VAT in footer
    let totalVat = 0;
    for (const key in vatMap) {
        if (vatMap.hasOwnProperty(key)) {
            totalVat += vatMap[key].vat || 0;
        }
    }

    const vatFooter = document.getElementById("calculatedVat");
    if (vatFooter) {
        vatFooter.innerText = totalVat.toLocaleString('en-US', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });
    }
}
function applyVatAdjustment(loanId) {
    const interestInput = document.getElementById("interest-" + loanId);
    const vatCheckbox = document.getElementById("vatExclusive-" + loanId);

    // Validate existence
    if (!interestInput || !vatMap[loanId]) return;

    const { original, vat } = vatMap[loanId];

    // Only adjust if VAT checkbox is NOT checked
    if (vatCheckbox && !vatCheckbox.checked) {
        const netInterest = Math.round(original - vat);
        interestInput.value = netInterest.toFixed(0);
    }
}
function AddNote() {
    EditResetModal(null, 'modal', 'modalContent', 'CashDesk', 'InitializeData', '_Note', 'new_depositor', 'NOTE', 'modalLabel')
}
function AddDepositor() {
    EditResetModal(null, 'modal', 'modalContent', 'CashDesk', 'InitializeData', '_DepositerForm', 'new_depositor', 'Depositor information', 'modalLabel')
    $('#DepositorIDIssueDate, #DepositorIDExpiryDate').on('input', function () {
        var value = $(this).val();
        if (value.length === 4 || value.length === 7) {
            $(this).val(value + '/');
        }
    });

    // Format telephone mask
    $('#DepositorTelephone').on('input', function () {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length > 3) {
            value = value.replace(/(\d{3})(\d)/, '$1-$2');
        }
        if (value.length > 6) {
            value = value.replace(/(\d{3})(\d{2})(\d)/, '$1-$2-$3');
        }
        $(this).val(value);
    });
}

$(document).on('input', '.amount-input, .fee-input, .interest-input, .penalty-input, .loan-amount-input', function () {
    var $row = $(this).closest('tr');

    var amount = parseFloat($row.find('.amount-input').val()) || 0;
    var fee = parseFloat($row.find('.fee-input').val()) || 0;
    var interest = parseFloat($row.find('.interest-input').val()) || 0;
    var penalty = parseFloat($row.find('.penalty-input').val()) || 0;
    var loanAmount = parseFloat($row.find('.loan-amount-input').val()) || 0;
    var vat = parseFloat($('#calculatedVat').text()) || 0;

    // ✅ Add VAT only if checkbox is CHECKED
    var isVatExclusive = $row.find('.vat-exclusive-check').prop('checked');

    var total = amount + fee + interest + penalty + loanAmount;
    if (isVatExclusive) {
        total += vat;
    }

    $row.find('.total-span').text(total.toFixed(2));
    calculateTableTotal();
});

function handleVatToggle(loanId) {
    const interestInput = document.getElementById(`interest-${loanId}`);
    const vatCheckbox = document.getElementById(`vatExclusive-${loanId}`);
    const vatElement = document.getElementById("calculatedVat");

    const capitalInput = document.getElementById(`capital-${loanId}`);
    const penaltyInput = document.getElementById(`penalty-${loanId}`);
    const totalSpan = document.getElementById(`total-${loanId}`);

    if (!interestInput || !vatCheckbox || !vatElement || !capitalInput || !penaltyInput || !totalSpan)
        return;

    const vat = parseFloat(vatElement.textContent) || 0;
    let interest = parseFloat(interestInput.value) || 0;
    const capital = parseFloat(capitalInput.value) || 0;
    const penalty = parseFloat(penaltyInput.value) || 0;

    const isExclusive = vatCheckbox.checked;

    // Update interest
    if (isExclusive) {
        interest += vat;
    } else {
        interest -= vat;
    }
    interestInput.value = interest.toFixed(2);

    // Recalculate total
    const total = interest + vat + penalty + capital;
    totalSpan.textContent = total.toFixed(2);

    // Format numbers with financial separators
    const formatCurrency = (num) =>
        num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    // Prepare message
    const message = `
🧾 ${isExclusive ? "VAT Exclusive Selected" : "VAT Inclusive Selected"}

  [Capital]     : ${formatCurrency(capital)}
  [Interest]    : ${formatCurrency(interest)}
  [VAT]         : ${formatCurrency(vat)}
  [Penalty]     : ${formatCurrency(penalty)}
  -------------------------------
  [Total Due]   : ${formatCurrency(total)}
`.trim();

    // Show notification
    if (typeof appalert === "function") {
        appalert(message, 2, 1);
    } else {
        alert(message);
    }
}
function calculateTableTotal() {
    var total = 0; // Total of all rows
    var totalVat = 0; // Total VAT for all rows

    // Iterate over each row in the table
    $('#myDataTableT tbody tr').each(function () {
        // Extract values for Capital, Interest, and Penalty
        var capital = parseFloat($(this).find('.amount-input').val()) || 0;
        var interest = parseFloat($(this).find('.interest-input').val()) || 0;
        var penalty = parseFloat($(this).find('.penalty-input').val()) || 0;
        var fee = parseFloat($(this).find('.fee-input').val()) || 0;
         //Calculate VAT (interest × vatRate)
        //var vatRate = parseFloat($(this).find('.interest-input').data('vat-rate')) || 0; // Get VAT rate from data attribute
        //var vat = interest * (vatRate / 100);

        // Row total
        var rowTotal = capital + interest + penalty + fee;

        // Update row total and VAT display in the row (optional)
        $(this).find('.total-span').text(rowTotal);

        // Accumulate totals
        total += rowTotal;
        //totalVat += vat;
    });

    // Update footer totals
    $('#tableTotal').text(total.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));
    $('#calculatedVat').text(totalVat.toLocaleString('en-US', { minimumFractionDigits: 1 })); // Update VAT in the footer

    // Update balance (if applicable)
    var totalNotes = parseFloat($("#totalNoteAmount").val()) || 0; // Assuming total funds are available
    var balance = totalNotes - total;
    $('#tableBalance').text(balance.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));
}
// Bind event listeners to the input fields
$(document).ready(function () {
    // Recalculate totals whenever inputs change
    $('#myDataTableT').on('input', '.amount-input, .interest-input, .penalty-input', function () {
        calculateTableTotal();
    });
});

// Optional: Single VAT calculation function for real-time updates
// VAT calculation function
function calculateVat(interestInput, vatRate) {
    // Parse the interest value from the input
    var interest = parseFloat(interestInput.value) || 0;

    // Calculate VAT for the current row
    var vat = interest * vatRate / 100;

    // Format VAT for display with thousands separators and no decimal places
    var formattedVat = vat.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    // Recalculate the total VAT for all rows
    calculateTotalVat();

    // Find the element with id 'calculatedVat' and update its content
    var vatElement = document.getElementById('calculatedVat');
    if (vatElement) {
        vatElement.textContent = formattedVat;
    } else {
        console.error('Element with id "calculatedVat" not found in the DOM.');
    }
}

function calculateTotalVat() {
    var totalVat = 0;

    // Iterate over all interest inputs and calculate the total VAT
    $('#myDataTableT tbody tr').each(function () {
        var interest = parseFloat($(this).find('.interest-input').val()) || 0;
        var vatRate = parseFloat($(this).find('.vat-rate').text()) || 0; // Assuming a hidden cell for VAT rate
        totalVat += interest * vatRate / 100;
    });

    // Format the total VAT for display
    var formattedTotalVat = totalVat.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    // Update the VAT footer cell
    var vatFooterElement = document.getElementById('calculatedVat');
    if (vatFooterElement) {
        vatFooterElement.textContent = formattedTotalVat;
    } else {
        console.error('Footer element with id "calculatedVat" not found in the DOM.');
    }
}






function checkTotalNotes() {
    var totalNotes = parseFloat($("#totalNoteAmount").val());

    if (totalNotes === 0) {
        appalert("Please enter cash in the denomination box.", 3, 1);
        return false;
    }

    return true;
}

function calculateTotalAmount() {
    var total = 0;
    var anyRowsSelected = false;

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            anyRowsSelected = true;
            total += parseFloat($(this).find('.total-span').text());
        }
    });

    return { total: total, anyRowsSelected: anyRowsSelected };
}


function validateTotalAmount(total, totalNotes) {
    if (!total.anyRowsSelected) {
        appalert("Please select at least one account to perform operation.", 3, 1);
        return false;
    }

    if (total.total === 0) {
        appalert("Please enter an amount.", 3, 1);
        return false;
    }

    if (total.total !== totalNotes) {
        appalert("Amount entered must equal the total of notes entered. Make sure you have checked/unchecked corresponding accounts. Please reevaluate and enter again.", 3, 1);
        return false;
    }

    return true;
}

function applyPayment() {
    var loanId = document.getElementById('modalLoanId').value;
    var amount = parseFloat(document.getElementById('modalAmount').value) || 0;
    var interest = parseFloat(document.getElementById('modalInterest').value) || 0;
    var penalty = parseFloat(document.getElementById('modalPenalty').value) || 0;

    // Update the row with the values entered from the modal
    var capitalInput = document.getElementById('capital-' + loanId);
    var interestInput = document.getElementById('interest-' + loanId);
    var penaltyInput = document.getElementById('penalty-' + loanId);
    var totalSpan = document.getElementById('total-' + loanId);

    // Check if elements are found
    if (!capitalInput || !interestInput || !penaltyInput || !totalSpan) {
        console.error("One or more elements not found. Check your IDs and ensure they match.");
        return;
    }

    // Update the row with entered values
    capitalInput.value = amount;
    interestInput.value = interest;
    penaltyInput.value = penalty;

    // Calculate total (capital + interest + penalty)
    var total = amount + interest + penalty;
    totalSpan.innerText = total;
    calculateTableTotal();
    // Calculate VAT based on the entered interest
    calculateVat(interestInput, selectedVatRate);

    // Close the modal
    var paymentModal = bootstrap.Modal.getInstance(document.getElementById('paymentModal'));
    paymentModal.hide();
}
function collectDeposits() {
    const deposits = [];

    // helpers
    const n = v => {
        const x = parseFloat(v);
        return Number.isFinite(x) ? x : 0;
    };
    const tv = sel => {
        const $el = $(sel);
        if (!$el.length) return null;
        const v = $el.val();
        return (v === undefined || v === null) ? null : String(v).trim();
    };

    // one-time values
    const accountingDate = $('#BulkDeposit_AccountingDate').val();

    // NEW: branch + GL + source type
    const branchId = $('#branchInput').val();
    const branchName = $('#branchInput option:selected').text().trim();
    const chartOfAccountId = $('#account_number').val();
    const chartOfAccountName = $('#account_number option:selected').text().trim();
    const sourceType = $("input[name='BulkDeposit.SourceType']:checked").val();

    // read depositor fields with fallback for old IDs (typos)
    const depositorName = tv('#DepositorName');
    const depositorPhoneNumber = tv('#DepositorTelephone, #DepositerTelephone'); // fallback
    const depositorIDNumber = tv('#DepositorIDNumber');
    const depositorIssueDate = tv('#DepositorIDIssueDate');
    const depositorExpiryDate = tv('#DepositorIDExpiryDate');
    const depositorNumberPlaceOfIssue = tv('#DepositorIDNumberPlaceOfIssue');
    const depositorNote = tv('#DepositorNote, #DepositerNote'); // fallback

    $('#myDataTableT tbody tr').each(function () {
        const $row = $(this);
        const rowChecked = $row.find('.form-check-input').first().prop('checked');
        if (!rowChecked) return;

        const deposit = {
            // table/operation data
            AccountNumber: $row.find('td:eq(0)').text().trim(),
            Amount: n($row.find('.amount-input').val()),
            Fee: n($row.find('.fee-input').val()),
            Penalty: n($row.find('.penalty-input').val()),
            Interest: n($row.find('.interest-input').val()),
            Total: n($row.find('.total-span').text()),
            AccountType: $row.find('td:eq(1)').text().trim(),
            isDepositDoneByAccountOwner: rowChecked,
            IsChargesInclussive: $row.find('.check-inclussive').prop('checked') === true,
            OperationType: $('#OperationType').val(),
            CheckName: tv('#CheckName'),
            CheckNumber: tv('#CheckNumber'),
            IsSWS: true,
            CustomerId: $('#customerId').val(),

            // NEW: momocash / bulk-deposit context
            SourceType: sourceType,
            BranchId: branchId,
            BranchName: branchName,
            ChartOfAccountId: chartOfAccountId,
            ChartOfAccountName: chartOfAccountName,

            LoanApplicationId: $row.find('.loan-application-id').val(),
            Period: $row.find('.period').val(),
            AccountingDate: accountingDate,

            // depositor fields — use EXACT C# property names
            DepositorName: depositorName,
            DepositorPhoneNumber: depositorPhoneNumber,
            DepositorIDNumber: depositorIDNumber,
            DepositorIssueDate: depositorIssueDate,
            DepositorExpiryDate: depositorExpiryDate,
            DepositorNumberPlaceOfIssue: depositorNumberPlaceOfIssue,
            DepositorNote: depositorNote
        };

        deposits.push(deposit);
    });

    return deposits;
}


//function collectDeposits() {
//    var deposits = [];
//    // Get the accounting date value once
//    var accountingDate = $('#BulkDeposit_AccountingDate').val();
//    $('#myDataTableT tbody tr').each(function () {
//        if ($(this).find('.form-check-input').prop('checked')) {
//            var deposit = {};
//            deposit.AccountNumber = $(this).find('td:eq(0)').text();
//            deposit.Amount = parseFloat($(this).find('.amount-input').val());
//            deposit.Fee = parseFloat($(this).find('.fee-input').val());
//            deposit.Penalty = parseFloat($(this).find('.penalty-input').val());
//            deposit.Interest = parseFloat($(this).find('.interest-input').val());
//            deposit.Total = parseFloat($(this).find('.total-span').text());
//            deposit.AccountType = $(this).find('td:eq(1)').text();
//            deposit.Note = $('#Note').val();
//            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
//            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');
//            deposit.OperationType = $('#OperationType').val();
//            deposit.CheckName = $('#CheckName').val();
//            deposit.CheckNumber = $('#CheckNumber').val();
//            deposit.IsSWS = true;
//            deposit.CustomerId = $('#customerId').val();
//            deposit.SourceType = $("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked").val();
//            deposit.LoanApplicationId = $(this).find('.loan-application-id').val();
//            deposit.Period = $(this).find('.period').val();
//            // ✅ Add accounting date
//            deposit.AccountingDate = accountingDate;
//            deposits.push(deposit);
//        }
//    });


//    return deposits;
//}

function collectDepositorInfo() {
    return {
        DepositorName: $('#DepositorName').val(),
        DepositerTelephone: $('#DepositerTelephone').val(),
        DepositorIDNumber: $('#DepositorIDNumber').val(),
        DepositorIDIssueDate: $('#DepositorIDIssueDate').val(),
        DepositorIDExpiryDate: $('#DepositorIDExpiryDate').val(),
        DepositorIDNumberPlaceOfIssue: $('#DepositorIDNumberPlaceOfIssue').val(),
        DepositerNote: $('#DepositerNote').val()
    };
}
function resetDepositorForm() {
    $('#DepositorName').val('');
    $('#DepositerTelephone').val('');
    $('#DepositorIDNumber').val('');
    $('#DepositorIDIssueDate').val('');
    $('#DepositorIDExpiryDate').val('');
    $('#DepositorIDNumberPlaceOfIssue').val('');
    $('#DepositerNote').val('');
    $('#Note').val('')
}
function Reprint() {
    ReportView("CashDesk", null, "GetReport", null, null, "receipts", "ReportParameterLess");

}
function confirmTransaction(title, message, ajaxUrl, data, operationType) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
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
                    appalert("Your session is expired Or An error occurred while processing the transaction. Please try again later.", 0, 1);
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
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'cashin_momokash_collection');
            break;
        case 'LoanRepaymentMomocashCollection':
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'repayment_momokash_collection');
            break;
        default:
            break;
    }
}
function formatAmount(val) {
    var n = parseFloat(val);
    if (!Number.isFinite(n)) n = 0;
    return n.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function failureCallback(response) {
    appalert(response.message || "Your session is expired Or An error occurred while processing the transaction", 3, 1);
}

function PostCashIn() {
    var deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("Please select at least one account to perform the cash-in.", 3, 1);
        return;
    }

    // 1) Require Branch selection
    var branchId = $('#branchInput').val();
    var branchName = $('#branchInput option:selected').text().trim() || "-";
    if (!branchId) {
        appalert("Please select the branch for this Momocash collection.", 3, 1);
        return;
    }

    // 2) Require Momocash collection GL account
    var momoCollectionAccountId = $('#account_number').val();
    var momoCollectionAccountName = $('#account_number option:selected').text().trim() || "-";
    if (!momoCollectionAccountId) {
        appalert("Please select the Momocash collection GL account.", 3, 1);
        return;
    }

    // 3) Validate total vs per-row input & build row summaries
    var totalInfo = calculateTotalAmount();

    var selectedTotalAmount = 0;
    var rowSummaries = [];   // for table
    var depositsTotal = 0;
    var loanTotal = 0;
    var totalFee = 0;

    $('#myDataTableT tbody tr').each(function () {
        var $row = $(this);
        var checked = $row.find('.form-check-input').first().prop('checked');
        if (!checked) return;

        var accountNumber = $row.find('td:eq(0)').text().trim();
        var accountType = $row.find('td:eq(1)').text().trim();
        var amount = parseFloat($row.find('.amount-input').val()) || 0;
        var fee = parseFloat($row.find('.fee-input').val()) || 0;
        var total = amount + fee;

        selectedTotalAmount += total;
        totalFee += fee;

        // classify deposit vs loan (defensive – your cash-in table excludes loans already)
        if (accountType.toLowerCase().includes("loan")) {
            loanTotal += total;
        } else {
            depositsTotal += total;
        }

        rowSummaries.push({
            accountNumber: accountNumber,
            accountType: accountType,
            amount: amount,
            fee: fee,
            total: total
        });
    });

    if (totalInfo.total !== selectedTotalAmount) {
        appalert("The total amount does not match the sum of the selected account amounts and fees.", 3, 1);
        return;
    }

    // 4) Attach currency notes & depositor
    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    // 5) Attach branch + GL to payload
    deposits[0].BranchId = branchId;
    deposits[0].BranchName = branchName;
    deposits[0].ChartOfAccountId = momoCollectionAccountId;
    deposits[0].ChartOfAccountName = momoCollectionAccountName;

    // 6) Require operator (MTN / Orange)
    var $sourceInput =
        $("input[name='BulkDeposit.SourceType']:checked")
            .add("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked");

    var sourceType = $sourceInput.val();
    if (!sourceType) {
        appalert("Please select operator type: either MTN Mobile Money or Orange Money.", 3, 1);
        return;
    }

    var operatorLabel = $sourceInput.closest('label').text().trim() || sourceType;
    deposits[0].SourceType = sourceType;
    deposits[0].SourceTypeLabel = operatorLabel;

    // 7) Extra context for confirmation
    var accountingDate = $('#BulkDeposit_AccountingDate').val() || "-";
    var memberName = $('#memberName').val() || "-";
    var memberAccountNumber = $('#memberAccountNumber').val() || "";

    if (!memberAccountNumber && rowSummaries.length > 0) {
        // fallback: use first selected account number
        memberAccountNumber = rowSummaries[0].accountNumber || "";
    }

    // 8) Build HTML table for confirm dialog
    var rowsHtml = rowSummaries.map(function (r) {
        return (
            '<tr>' +
            '<td>' + escapeHtml(r.accountType) + '</td>' +
            '<td class="text-end">' + formatAmount(r.amount) + '</td>' +
            '<td class="text-end">' + formatAmount(r.fee) + '</td>' +
            '</tr>'
        );
    }).join('');

    var html =
        '<div class="momo-confirm">' +
        '<p><strong>Member:</strong> ' + escapeHtml(memberName) + '</p>' +
        (memberAccountNumber
            ? '<p><strong>Member Account Number:</strong> ' + escapeHtml(memberAccountNumber) + '</p>'
            : '') +
        '<p>You\'re about to perform a <strong>CASH-IN of ' + formatAmount(totalInfo.total) + '.</strong></p>' +
        '<p><strong>Branch:</strong> ' + escapeHtml(branchName) + '<br />' +
        '<strong>Operator:</strong> ' + escapeHtml(operatorLabel) + '<br />' +
        '<strong>Collection GL:</strong> ' + escapeHtml(momoCollectionAccountName) + '<br />' +
        '<strong>Accounting Date:</strong> ' + escapeHtml(accountingDate) + '</p>' +

        '<h6 class="text-success mt-3 mb-2">Member Accounts</h6>' +
        '<table class="table table-sm table-bordered mb-2">' +
        '<thead>' +
        '<tr>' +
        '<th>Account Type</th>' +
        '<th class="text-end">Amount</th>' +
        '<th class="text-end">Fee</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>' +
        rowsHtml +
        '<tr class="fw-bold bg-light">' +
        '<td>Total</td>' +
        '<td class="text-end">' + formatAmount(depositsTotal + loanTotal) + '</td>' +
        '<td class="text-end">' + formatAmount(totalFee) + '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +

        '<table class="table table-sm table-bordered mb-0">' +
        '<tbody>' +
        '<tr>' +
        '<td><strong>Deposits Total</strong></td>' +
        '<td class="text-end">' + formatAmount(depositsTotal) + '</td>' +
        '</tr>' +
        '<tr>' +
        '<td><strong>Loan Total</strong></td>' +
        '<td class="text-end">' + formatAmount(loanTotal) + '</td>' +
        '</tr>' +
        '<tr class="fw-bold bg-light">' +
        '<td><strong>Grand Total</strong></td>' +
        '<td class="text-end">' + formatAmount(totalInfo.total) + '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +
        '</div>';

    // 9) Ask for confirmation (message is now HTML)
    confirmTransaction(
        '💰 CONFIRM CASH-IN OPERATION',
        html,
        '/CashDesk/PostRequestCash',
        deposits,
        'CashInMomocashCollection'
    );
}

function PostLoanRepayment() {
    //if (!checkTotalNotes()) return false;

    //var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    //if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("Only one loan can be paid at an instant. Please deselect other accounts.", 3, 1);
        return;
    }
    // Check if one of the radio buttons is selected
    var sourceType = $("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked").val();
    if (!sourceType) {
        appalert("Please select operator type, Either Mobile Money MTN OR Mobile Money Orange", 3, 1);
        return;
    }
    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "";
    message += "Are you sure you want to perform loan repayment of " + totalInfo.total + "?";
    confirmTransaction('Confirm Loan Repayment Operation', message, '/CashDesk/PostRequestCash', deposits, 'LoanRepaymentMomocashCollection');
}



//function PostLoanRepayment() {
//    if (!checkTotalNotes()) return false;

//    var totalNotes = parseFloat($("#totalNoteAmount").val());
//    var totalInfo = calculateTotalAmount();

//    if (!validateTotalAmount(totalInfo, totalNotes)) return;

//    var deposits = collectDeposits();
//    if (deposits.length !== 1) {
//        appalert("Only one loan can be paid at an instant. Please deselect other accounts.", 3, 1);
//        return;
//    }

//    deposits[0].currencyNotes = collectCurrencyNotes();
//    deposits[0].Depositer = collectDepositorInfo();

//    var message = "";
//    message += "Are you sure you want to perform loan repayment of " + totalInfo.total + "?";
//    confirmTransaction('Confirm Loan Repayment Operation', message, '/CashDesk/PostRequestCash', deposits, 'LoanRepayment');
//}

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



function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function GetObject(KEY, divToLoadData, partialView, path) {
    if (KEY.trim() === "") {
        alert("KEY is empty. Please provide a valid KEY.");
        return; // Exit the function
    }
    AddORUpdateGen(KEY, divToLoadData, partialView, path, "CashDesk");


}
function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "CashDesk");
    //calculateBalance();
}
function GetMember() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', operation);
    /*GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);
    var spanElement = document.getElementById('cashDeskOperations');

    // Default style
    spanElement.style.fontWeight = "bold";
    spanElement.style.textDecoration = "underline";
    spanElement.style.textDecorationThickness = "2px";

    if (path == "cashin_momokash_collection") {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE-CASH-IN OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "repayment_momokash_collection") {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE-CASH LOAN REPAYMENT OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "search") {
        spanElement.innerText = "MEMBER'S INFORMATION";
        spanElement.style.color = "blue";
    }
    else {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE CASH OPERATIONS";
        spanElement.style.color = "blue"; // Default color for other paths
    }
    
    AddORUpdateGen(Key, divToloadPV, partialView, path, "CashDesk");
    //calculateBalance();
}

function GetLoan(KEY) {
    $.ajax({
        type: "GET",
        url: '/Operation/GetLoan?KEY=' + KEY,
        success: function (data) {
            var balance = parseFloat(data.Balance).toFixed(1); // Format Balance with 1 decimal place
            var paid = parseFloat(data.Paid).toFixed(1); // Format Paid with 1 decimal place

            // Format numbers with commas as thousands separators
            balance = parseFloat(balance).toLocaleString('en-US');
            paid = parseFloat(paid).toLocaleString('en-US');

            // Assuming #balance and #paid are HTML input elements
            $('#balance').val(balance);
            $('#paid').val(paid);
            $('#loanid').val(data.Id);
            //$('#balance').val(data.Balance);
            //$('#paid').val(data.Paid);
        }, error: function (err) {
            appalert(err.statusText, 3, 0);
        }
    });
}







function AjaxPostAndUpdate(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {


        alertify.confirm("WARNING!!!", "Are you sure you want to perform this action! ",
            function () {


                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {

                        if (response.success) {
                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            }
                            else {
                                appalert(response.message, 1, 1);

                            }
                            if (response.option === 'Update' && response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                            else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                                EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                            }
                            else if (response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                        }
                        else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else {
                                appalert(response.message, 2, 1);
                            }

                        }

                    }
                    , error: function (err) {
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig["contentType"] = false;
                    ajaxConfig["processData"] = false;
                }
                console.log(ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );
    }
    return false;


}
