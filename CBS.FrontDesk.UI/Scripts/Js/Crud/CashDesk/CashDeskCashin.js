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
// Enforce only ONE loan can be checked; keep it once on the page.
$(document).on('change', '#loanRepaymentTable .loan-confirmation-checkbox', function () {
    if (this.checked) {
        $('#loanRepaymentTable .loan-confirmation-checkbox').not(this).prop('checked', false);
    }
    // keep your totals live
    if (typeof updateTotals === 'function') updateTotals();
});

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

function calculateTableTotal() {
    var total = 0;
    $('.total-span').each(function () {
        total += parseFloat($(this).text()) || 0;
    });

    // Calculate balance
    var totalNotes = parseFloat($("#totalNoteAmount").val()) || 0;
    var balance = totalNotes - total;

    // Update balance in the table footer with separators (no currency)
    var formattedBalance = balance.toLocaleString('en-US'); // No currency, just thousand separators
    $('#tableBalance').text(formattedBalance);

    // Change color of balance text if negative
    if (balance < 0) {
        $('#tableBalance').addClass('text-danger');
    } else {
        $('#tableBalance').removeClass('text-danger');
    }

    // Remove any existing icon
    $('#tableTotal .total-icon').remove();

    // Compare total with totalNotes and update the icon accordingly
    var iconClass, iconColor;
    if (total === totalNotes) {
        // Equal to totalNotes, show a green checkmark
        iconClass = 'fas fa-check-circle';
        iconColor = 'text-success';
    } else if (total < totalNotes) {
        // Less than totalNotes, show a warning exclamation mark
        iconClass = 'fas fa-exclamation-circle';
        iconColor = 'text-warning';
    } else {
        // Greater than totalNotes, show a red X
        iconClass = 'fas fa-times-circle';
        iconColor = 'text-danger';
    }

    // Append the total value to the tableTotal cell with separators (no currency)
    var formattedTotal = total.toLocaleString('en-US'); // No currency, just thousand separators
    $('#tableTotal').html(`<span>${formattedTotal}</span>`);
    // Append the icon after the total value
    $('#tableTotal').append(` <i class="${iconClass} total-icon ${iconColor}"></i>`);

    // Enable or disable the button based on the comparison
    if (total === totalNotes) {
        $('#submit').prop('disabled', false); // Enable the button
    } else {
        $('#submit').prop('disabled', true); // Disable the button
    }
}

function calculateVat(interestInput, vatRate) {
    // Get the entered interest amount from the input
    var interestAmount = parseFloat(interestInput.value) || 0;

    // Calculate the VAT based on the interest amount and the vatRate from the specific row
    var calculatedVat = interestAmount * (vatRate / 100);

    // Format the calculated VAT with thousand separators (no currency symbol)
    var formattedVat = calculatedVat.toLocaleString('en-US');

    // Display the formatted VAT in the "calculatedVat" footer cell
    document.getElementById("calculatedVat").innerText = formattedVat;
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
// Collect exactly ONE checked loan; return { items: [...], loanTotal, anySelected, errorMsg }
function collectLoanRepaymentSelection() {
    const result = { items: [], loanTotal: 0, anySelected: false, errorMsg: "" };

    const checked = $('#loanRepaymentTable .loan-confirmation-checkbox:checked');
    const count = checked.length;

    if (count === 0) return result; // no selection is allowed when IncludeLoanRepayment is OFF
    if (count > 1) {
        result.errorMsg = "Please select only one loan row to repay.";
        return result;
    }

    const $row = checked.closest('tr');
    const loanId = ($row.attr('id') || '').replace('row-', '');

    // Parse numbers from inputs/labels (keep everything numeric; dialog formats later)
    const capital = parseFloat($row.find('.capital-input').val()) || 0;
    const interest = parseFloat($row.find('.interest-input').val()) || 0;
    const vat = parseFloat(String($row.find('.vat-input').val()).replace(/,/g, '')) || 0;
    const penalty = parseFloat($row.find('.penalty-input').val()) || 0;

    // Prefer the numeric unformatted total-span; if blank, recompute
    const totalSpan = String($row.find('.total-span').text()).replace(/,/g, '').trim();
    const rowTotal = totalSpan ? (parseFloat(totalSpan) || 0) : (capital + interest + vat + penalty);

    result.items.push({
        Id: loanId,
        LoanId: loanId,
        Amount: capital,          // maps to Principal on server
        Interest: interest,
        VAT: vat,                 // your server maps VAT -> Tax
        Penalty: penalty,
        Total: rowTotal
    });

    result.anySelected = true;
    result.loanTotal = rowTotal;

    return result;
}

function collectDeposits() {
    const deposits = [];
    const totalInfo = calculateTotalAmount();
    const alphaNumber = $('#CustomerAlphaNumber').val();
    const customerId = $('#customerId').val();
    const operationType = $('#OperationType').val();
    const checkName = $('#CheckName')?.val() || '';
    const checkNumber = $('#CheckNumber')?.val() || '';
    const note = $('#Note')?.val() || '';
    const globalVat = parseFloat($('#calculatedVat').text().replace(/,/g, '')) || 0;

    const hideBalance = $('#hideBalanceCheckbox').is(':checked'); // ✅ FETCH OUTSIDE the loop once

    $('#myDataTableT tbody tr').each(function () {
        const isChecked = $(this).find('td input[type="checkbox"]').prop('checked'); // ✅ target only inside table cell, not global checkbox!

        if (!isChecked) return;

        const deposit = {
            AccountNumber: $(this).find('td:eq(0)').text().trim(),
            AccountType: $(this).find('td:eq(1)').text().trim(),
            Balance: parseFloat($(this).find('td:eq(2)').text()) || 0,
            Amount: parseFloat($(this).find('.amount-input').val()) || 0,
            Fee: parseFloat($(this).find('.fee-input').val()) || 0,
            Penalty: parseFloat($(this).find('.penalty-input')?.val()) || 0,
            Interest: parseFloat($(this).find('.interest-input')?.val()) || 0,
            Total: parseFloat($(this).find('.total-span').text()) || 0,
            //Total: totalInfo.total //parseFloat($(this).find('.total-span').text()) || 0,
            Note: note,
            CheckName: checkName,
            CheckNumber: checkNumber,
            CustomerAlphaNumber: alphaNumber,
            CustomerId: customerId,
            OperationType: operationType,
            isDepositDoneByAccountOwner: isChecked,
            IsChargesInclussive: $(this).find('.check-inclussive').prop('checked'),
            IsSWS: true,
            PaymentMethod: 'Cash',
            PaymentChannel: 'Web_Portal',
            LoanApplicationId: $(this).find('.loan-application-id')?.val() || '',
            Period: $(this).find('.period')?.val() || '',
            Vat: globalVat,
            HideBalance: hideBalance // ✅ correctly from the top hide balance checkbox!
        };

        deposits.push(deposit);
    });

    console.log("✅ Collected Deposits:", deposits);
    return deposits;
}



function autoCheckDeposits() {
    $('#myDataTableT tbody tr').each(function () {
        const amount = parseFloat($(this).find('.amount-input').val()) || 0;

        // Only check CONF checkbox if amount > 0
        if (amount > 0) {
            $(this).find('td:eq(6) .form-check-input').prop('checked', true); // 7th column = CONF
        } else {
            $(this).find('td:eq(6) .form-check-input').prop('checked', false);
        }
    });
}


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
function Reprint(redirectUrl) {
    //window.open(redirectUrl, '_blank');
    ReportView("CashDesk", null, "GetReport", null, null, "receipts", "ReportParameterLessWithSubReports");
}
function ReprintLoan() {
    ReportView("CashDesk", null, "GetReport", null, null, "loan", "ReportParameterLessWithSubReports");


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
                        appalert(response.message, 1, 1);
                        resetDepositorForm();
                        $("#depositerModal").modal("hide"); // Hide if not already hidden
                        Reprint(response.redirectUrl);
                        location.reload();
                        //successCallback(response, operationType);
                        
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
   
    // Clean up
    resetDepositorForm();
    $("#depositerModal").modal("hide"); // Hide if not already hidden
    location.reload();
    const customerId = $("#customerId").val();

     //✅ Reload the entire page
    location.reload();

    const actions = {
        CashIn: "cashin",
        Withdrawal: "cashout",
        WithdrawalSWS: "cashoutsws",
        SavingWithdrawalFormFee: "withdrawalnotification",
        LoanRepayment: "repayment",
        LoanFee: "loanapplicationfeepayment"
    };

    const operation = actions[operationType];
    if (operation) {
        GetMemberData(customerId, '_OperationDesk', 'datalistingview', operation);
    } else {
        console.warn("❗ No operation handler defined for:", operationType);
    }
}


function failureCallback(response) {
    const message = response?.message || "⚠️ An unexpected error occurred or session expired.";
    appalert(message, 3, 1);

    // Optional: Re-enable confirm button if it was disabled
    $("#confirmDepositorBtn").prop("disabled", false);
    $("#depositorLoader").addClass("d-none");
}
// Returns summary {rowsHtml, totalAmount, totalFee, anySelected}
function buildDepositsSummaryFromCheckedRows() {
    let totalAmount = 0, totalFee = 0, anySelected = false;
    let rows = '';

    $('#myDataTableT tbody tr').each(function () {
        const $row = $(this);
        const isChecked = $row.find('td input[type="checkbox"]').prop('checked');
        if (!isChecked) return;

        anySelected = true;
        const accountType = $row.find('td:eq(1)').text().trim();
        const amount = parseFloat($row.find('.amount-input').val()) || 0;
        const fee = parseFloat($row.find('.fee-input').val()) || 0;

        totalAmount += amount;
        totalFee += fee;

        rows += `
          <tr>
            <td>${accountType}</td>
            <td class="text-end">${amount.toLocaleString('en-US')}</td>
            <td class="text-end">${fee.toLocaleString('en-US')}</td>
          </tr>`;
    });

    return {
        rowsHtml: rows,
        totalAmount,
        totalFee,
        anySelected
    };
}

// Returns summary {rowCount, rowsHtml, totalCapital, totalInterest, totalVat, totalPenalty, total, anySelected}
function buildLoanSummaryFromCheckedRow() {
    let rows = '';
    let totalCapital = 0, totalInterest = 0, totalVat = 0, totalPenalty = 0, total = 0;
    let rowCount = 0, anySelected = false;

    $('#loanRepaymentTable tr').each(function () {
        const $r = $(this);
        const $chk = $r.find('.loan-confirmation-checkbox');
        if ($chk.length === 0 || !$chk.prop('checked')) return;

        anySelected = true;
        rowCount++;

        const capital = parseFloat($r.find('.capital-input').val()) || 0;
        const interest = parseFloat($r.find('.interest-input').val()) || 0;
        const vat = parseFloat(String($r.find('.vat-input').val()).replace(/,/g, '')) || 0;
        const penalty = parseFloat($r.find('.penalty-input').val()) || 0;
        const rowTotal = parseFloat(String($r.find('.total-span').text()).replace(/,/g, '')) || (capital + interest + vat + penalty);

        totalCapital += capital;
        totalInterest += interest;
        totalVat += vat;
        totalPenalty += penalty;
        total += rowTotal;

        // NOTE: Date column removed per request
        rows += `
          <tr>
            <td class="text-end">${capital.toLocaleString('en-US')}</td>
            <td class="text-end">${interest.toLocaleString('en-US')}</td>
            <td class="text-end">${vat.toLocaleString('en-US')}</td>
            <td class="text-end">${penalty.toLocaleString('en-US')}</td>
            <td class="text-end fw-semibold">${rowTotal.toLocaleString('en-US')}</td>
          </tr>`;
    });

    return {
        rowCount,
        rowsHtml: rows,
        totalCapital, totalInterest, totalVat, totalPenalty, total,
        anySelected
    };
}

function PostCashIn() {
    const operation = $("#currentselectedOperation").val();
    const isNewSubscription = operation === "newsubcription";

    // Daily Collector context
    const hasDcControls = $('input[name="DailyCollectorCollectApproach"]').length > 0;
    const approach = hasDcControls ? $('input[name="DailyCollectorCollectApproach"]:checked').val() : null;
    const isManualApproach = hasDcControls && approach === "Manual";
    const isDeviceApproach = hasDcControls && approach === "Device";
    const isDailyCollectorContext = $("#ManualEntryDailyCollectorId").length > 0;

    if (!validateCustomerAlphaNumber()) return;
    if (!checkTotalNotes()) return;

    // DC Manual requires an approved batch
    let dcBatchId = null, dcBatchText = "";
    if (isDailyCollectorContext && isManualApproach) {
        dcBatchId = $("#ManualEntryDailyCollectorId").val();
        dcBatchText = $("#ManualEntryDailyCollectorId option:selected").text() || "";
        if (!dcBatchId) {
            appalert("⚠️ Please select an approved Daily Collector batch to clear before proceeding.", 3, 1);
            return;
        }
    }

    // Notes amount entered by teller
    const totalNotes = parseFloat($("#totalNoteAmount").val());

    // ✅ Member deposits (checked rows only)
    const deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("⚠️ Please select at least one account with a valid amount.", 3, 1);
        return;
    }

    // Sum member deposits (amount + fee are shown separately in dialog)
    const depositsTotal = deposits.reduce((s, d) => s + (parseFloat(d.Total) || 0), 0);

    // ✅ Loan repayment block (only if toggle is ON)
    const includeLoan = $("#includeLoanRepaymentCheckbox").is(":checked");
    const loanSel = collectLoanRepaymentSelection();
    if (includeLoan) {
        if (loanSel.errorMsg) { appalert(loanSel.errorMsg, 3, 1); return; }
        if (!loanSel.anySelected) {
            appalert("⚠️ Include Loan Repayment is ON. Please select one loan row to repay.", 3, 1);
            return;
        }
    }

    const loanTotal = includeLoan ? (loanSel.loanTotal || 0) : 0;

    // ✅ Combined total for validation against notes
    const combinedTotal = depositsTotal + loanTotal;

    // Validate totals vs notes (reusing your helper)
    const totalInfo = { total: combinedTotal, anyRowsSelected: deposits.length > 0 || loanSel.anySelected };
    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    // Auto-check deposit rows if needed
    autoCheckDeposits();

    // Attach DC context (first deposit object retains the envelope)
    if (isDailyCollectorContext) {
        deposits[0].IsDailyCollector = true;
        deposits[0].CollectionType = isManualApproach ? "Manual" : "Device";
        if (isManualApproach) {
            deposits[0].ManualEntryDailyCollectorId = dcBatchId;
        } else {
            delete deposits[0].ManualEntryDailyCollectorId;
        }
    }

    // ✅ Attach loan selection to payload without breaking existing server shape
    if (includeLoan) {
        deposits[0].IncludeLoanRepayment = true;
        deposits[0].BulkOperationsForLoanRepayments = loanSel.items;
    } else {
        deposits[0].IncludeLoanRepayment = false;
        deposits[0].BulkOperationsForLoanRepayments = [];
    }

    // 👤 Member info for dialog
    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    // ===== Build confirmation dialog (no currency suffix) =====
    // Deposits table rows/summary
    let depRowsHtml = "", depAmountSum = 0, depFeeSum = 0;
    $('#myDataTableT tbody tr').each(function () {
        const $r = $(this);
        const isChecked = $r.find('td input[type="checkbox"]').prop('checked');
        if (!isChecked) return;

        const accountType = $r.find('td:eq(1)').text().trim();
        const amount = parseFloat($r.find('.amount-input').val()) || 0;
        const fee = parseFloat($r.find('.fee-input').val()) || 0;

        depAmountSum += amount;
        depFeeSum += fee;

        depRowsHtml += `
          <tr>
            <td>${accountType}</td>
            <td class="text-end">${amount.toLocaleString('en-US')}</td>
            <td class="text-end">${fee.toLocaleString('en-US')}</td>
          </tr>`;
    });

    const depositsTableHtml = `
      <table class="table table-sm table-bordered w-100 confirm-table">
        <thead class="table-light">
          <tr>
            <th>ACCOUNT TYPE</th>
            <th class="text-end">AMOUNT</th>
            <th class="text-end">FEE</th>
          </tr>
        </thead>
        <tbody>
          ${depRowsHtml || `<tr><td colspan="3" class="text-muted">No member account selected.</td></tr>`}
        </tbody>
        <tfoot>
          <tr class="fw-bold text-dark">
            <td class="text-end">Total</td>
            <td class="text-end">${depAmountSum.toLocaleString('en-US')}</td>
            <td class="text-end">${depFeeSum.toLocaleString('en-US')}</td>
          </tr>
        </tfoot>
      </table>`;

    // ---- VAT mode description (for user understanding) ----
    const vatRadio = document.querySelector('input[name="vatMode"]:checked');
    const vatMode = vatRadio ? vatRadio.value : null; // 'exclusive' | 'inclusive' | null
    const vatLabel = vatMode === 'exclusive' ? 'VAT Exclusive' : 'VAT Inclusive';
    const vatRateText = vatMode === 'exclusive' ? '19.25%' : '16.1425%';

    // Loan table (no Date column) + VAT info line
    let loanTableHtml = "";
    if (includeLoan && loanSel.anySelected) {
        const li = loanSel.items[0];
        const showNoVatNote = (li.VAT || 0) === 0;

        const vatInfoHtml = showNoVatNote
            ? `<div class="small text-muted mb-1">
                 VAT mode: <strong>${vatLabel}</strong> — no VAT applied (threshold not met or VAT rate is 0). 
                 VAT applies only on <strong>interest</strong> when loan ≥ 2,000,000 & VAT rate &gt; 0.
               </div>`
            : `<div class="small text-muted mb-1">
                 VAT mode: <strong>${vatLabel} (${vatRateText})</strong> — applied on <strong>interest only</strong> for loans ≥ 2,000,000 with VAT rate &gt; 0.
               </div>`;

        loanTableHtml = `
          <div class="mt-3">
            <p class="mb-1 fw-bold text-primary">Loan Repayment</p>
            ${vatInfoHtml}
            <table class="table table-sm table-bordered w-100 confirm-table">
              <thead class="table-light">
                <tr>
                  <th class="text-end">CAPITAL</th>
                  <th class="text-end">INTEREST</th>
                  <th class="text-end">VAT</th>
                  <th class="text-end">PENALTY</th>
                  <th class="text-end">TOTAL</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="text-end">${(li.Amount || 0).toLocaleString('en-US')}</td>
                  <td class="text-end">${(li.Interest || 0).toLocaleString('en-US')}</td>
                  <td class="text-end">${(li.VAT || 0).toLocaleString('en-US')}</td>
                  <td class="text-end">${(li.Penalty || 0).toLocaleString('en-US')}</td>
                  <td class="text-end fw-semibold">${(li.Total || 0).toLocaleString('en-US')}</td>
                </tr>
              </tbody>
              <tfoot>
                <tr class="fw-bold text-dark">
                  <td class="text-end" colspan="4">Loan Total</td>
                  <td class="text-end">${loanTotal.toLocaleString('en-US')}</td>
                </tr>
              </tfoot>
            </table>
          </div>`;
    }

    const finalTotalsHtml = `
      <div class="mt-2">
        <table class="table table-sm table-bordered w-100 confirm-table">
          <tbody>
            <tr>
              <td class="text-end fw-bold">Deposits Total</td>
              <td class="text-end">${depositsTotal.toLocaleString('en-US')}</td>
            </tr>
            <tr>
              <td class="text-end fw-bold">Loan Total</td>
              <td class="text-end">${loanTotal.toLocaleString('en-US')}</td>
            </tr>
            <tr class="table-active">
              <td class="text-end fw-bold">Grand Total</td>
              <td class="text-end fw-bold">${combinedTotal.toLocaleString('en-US')}</td>
            </tr>
          </tbody>
        </table>
      </div>`;

    const modalTablesCss = `
      <style>
        .confirm-table{table-layout:fixed}
        .confirm-table th,.confirm-table td{white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
      </style>`;

    const confirmationTitle = isNewSubscription
        ? "🧾 CONFIRM MEMBER ONBOARDING DEPOSIT"
        : (isDailyCollectorContext
            ? (isManualApproach
                ? "🧾 CONFIRM DAILY COLLECTOR CASH CLEARANCE (EOD)"
                : "🧾 CONFIRM DAILY COLLECTOR DEVICE COLLECTION")
            : "💰 CONFIRM CASH-IN OPERATION");

    const opLead = isNewSubscription
        ? `You're about to complete a <strong>MEMBER ONBOARDING DEPOSIT</strong> of`
        : (isDailyCollectorContext
            ? (isManualApproach
                ? `You're about to post a <strong>DAILY COLLECTOR CASH CLEARANCE</strong> totaling`
                : `You're about to post a <strong>DAILY COLLECTOR DEVICE COLLECTION</strong> totaling`)
            : `You're about to perform a <strong>CASH-IN</strong> of`);

    const dcContextHtml = (isDailyCollectorContext && isManualApproach)
        ? `
          <div class="mt-2 p-2 border rounded bg-light">
            <p class="mb-1"><strong>Approved Batch:</strong> ${dcBatchText}</p>
            <p class="mb-0">
              <strong>Note:</strong> This operation clears the Daily Collector’s cash for end-of-day.
              A paired <em>Credit → Debit</em> posting is expected so the collector’s transit closes to
              <strong>0</strong> and the till can be closed.
            </p>
          </div>`
        : "";

    const message = `
      ${modalTablesCss}
      <div class="text-start">
        <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
        <p>${opLead} <b>${combinedTotal.toLocaleString('en-US')}</b>.</p>
        ${dcContextHtml}
        <p class="mt-3 fw-bold text-primary mb-1">Member Accounts</p>
        ${depositsTableHtml}
        ${loanTableHtml}
        ${finalTotalsHtml}
      </div>`;

    // ===== Show confirm dialog & post =====
    alertify.confirm(
        confirmationTitle,
        message,
        function () {
            // Prep depositor modal (unchanged)
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            $("#depositorModalTitle").html(
                isNewSubscription
                    ? `<i class="mdi mdi-account-plus-outline me-2"></i> MEMBER ONBOARDING DEPOSITOR`
                    : (isDailyCollectorContext
                        ? (isManualApproach
                            ? `<i class="mdi mdi-clipboard-check-outline me-2"></i> DAILY COLLECTOR CLEARANCE – DEPOSITOR`
                            : `<i class="mdi mdi-nfc-tap-variant me-2"></i> DEVICE COLLECTION – DEPOSITOR`)
                        : `<i class="mdi mdi-account-card-details-outline me-2"></i> DEPOSITOR INFORMATION REQUIRED`)
            );

            $("#depositorModalDescription").text(
                isNewSubscription
                    ? `As part of the member onboarding process, please record the depositor’s full identity for compliance.`
                    : (isDailyCollectorContext
                        ? (isManualApproach
                            ? `Please record the depositor’s full information for audit. This clearance will reconcile Daily Collector cash and may close the collector till (Credit → Debit pair).`
                            : `Please record the depositor’s information for device-based collection (C-Money/POS).`)
                        : `To complete this transaction, please enter the depositor's full information for regulatory and audit compliance.`)
            );

            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            setTimeout(() => $("#depositerModal").modal("show"), 100);

            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                deposits[0].Depositer = depositor;
                deposits[0].currencyNotes = collectCurrencyNotes();

                // 🚀 Post to your existing endpoint with enriched payload
                PostTransaction('/CashDesk/PostRequestCash', deposits, 'CashIn');
            });
        },
        function () {
            appalert("🚫 Operation cancelled.", 2, 1);
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
}





function PostTransaction(ajaxUrl, data, operationType) {
    // Show loader and disable confirm
    $("#depositorLoader").removeClass("d-none");
    $("#confirmDepositorBtn").prop("disabled", true);

    $.ajax({
        url: ajaxUrl,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            if (response && response.success) {
                // ✅ Success: hide modal, notify and reprint
                //$("#depositerModal").modal("hide");
                //appalert(`✅ ${response.message}`, 1, 2);
                //successCallback(response, operationType);
                //Reprint();
                appalert(response.message, 1, 1);
                resetDepositorForm();
                $("#depositerModal").modal("hide"); // Hide if not already hidden
                Reprint(response.redirectUrl);
                location.reload();
            } else {
                // ❌ Failure: show error inside modal
                const errorMsg = response?.message || "An unknown error occurred.";
                appalert(`❌ ${errorMsg}`, 3, 1);

                // Keep the modal open and re-enable the confirm button
                $("#depositorLoader").addClass("d-none");
                $("#confirmDepositorBtn").prop("disabled", false);
            }
        },
        error: function (xhr, status, error) {
            // 🔴 AJAX/network error
            appalert("🚫 Session expired or a network error occurred. Please try again.", 0, 1);
            $("#depositorLoader").addClass("d-none");
            $("#confirmDepositorBtn").prop("disabled", false);
        }
    });
}

function validateDepositor(depositor) {
    let isValid = true;
    let messages = [];

    const fields = [
        { id: "#DepositorName", value: depositor.DepositorName, label: "Depositor Name" },
        { id: "#DepositerTelephone", value: depositor.DepositerTelephone, label: "Phone Number" },
        { id: "#DepositorIDNumber", value: depositor.DepositorIDNumber, label: "CNI / ID Number" },
        { id: "#DepositorIDIssueDate", value: depositor.DepositorIDIssueDate, label: "Issue Date" },
        { id: "#DepositorIDExpiryDate", value: depositor.DepositorIDExpiryDate, label: "Expiry Date" },
        { id: "#DepositorIDNumberPlaceOfIssue", value: depositor.DepositorIDNumberPlaceOfIssue, label: "Place of Issue" }
    ];

    // General field validation
    fields.forEach(field => {
        const $input = $(field.id);
        const $feedback = $input.siblings(".invalid-feedback");

        if (!field.value || field.value.trim() === "") {
            $input.addClass("is-invalid");
            $feedback.text(`Please enter ${field.label.toLowerCase()}.`);
            messages.push(`- ${field.label} is required.`);
            isValid = false;
        } else {
            $input.removeClass("is-invalid");
            $feedback.text("");
        }
    });

    // Date validation
    const issueDate = new Date(depositor.DepositorIDIssueDate);
    const expiryDate = new Date(depositor.DepositorIDExpiryDate);

    const $issueInput = $("#DepositorIDIssueDate");
    const $expiryInput = $("#DepositorIDExpiryDate");

    if (!depositor.DepositorIDIssueDate || isNaN(issueDate)) {
        $issueInput.addClass("is-invalid");
        $issueInput.siblings(".invalid-feedback").text("Please enter a valid issue date.");
        messages.push("- Issue Date is required and must be valid.");
        isValid = false;
    } else {
        $issueInput.removeClass("is-invalid");
    }

    if (!depositor.DepositorIDExpiryDate || isNaN(expiryDate)) {
        $expiryInput.addClass("is-invalid");
        $expiryInput.siblings(".invalid-feedback").text("Please enter a valid expiry date.");
        messages.push("- Expiry Date is required and must be valid.");
        isValid = false;
    } else {
        $expiryInput.removeClass("is-invalid");
    }

    if (!isNaN(issueDate) && !isNaN(expiryDate)) {
        if (expiryDate <= issueDate) {
            $expiryInput.addClass("is-invalid");
            $expiryInput.siblings(".invalid-feedback").text("Expiry date must be after issue date.");
            messages.push("- Expiry date must be after issue date.");
            isValid = false;
        }
    }

    if (!isValid && messages.length > 0) {
        appalert(`❌ Please correct the following fields:\n\n${messages.join("\n")}`, 3, 1);
    }

    return isValid;
}

function validateCustomerAlphaNumber() {
    const inputEl = $("#CustomerAlphaNumber");
    const alphaNumber = inputEl.val()?.trim();

    // Remove any previous error class
    inputEl.removeClass("is-invalid");

    // 1. Required field
    if (!alphaNumber) {
        inputEl.addClass("is-invalid");
        appalert(
            "❗ <strong>Member's Custom Account Number is Required</strong><br/>This number uniquely identifies the member from the previous system and is necessary for linking their records in <strong>Trust Soft Credit (TSC)</strong>.",
            3, 1
        );
        return false;
    }

    // 2. Must be numeric only
    if (!/^\d+$/.test(alphaNumber)) {
        inputEl.addClass("is-invalid");
        appalert(
            "❗ <strong>Only Digits Allowed</strong><br/>Please enter the custom account number using digits only, as registered in the legacy system.",
            3, 1
        );
        return false;
    }

    // 3. Max length check
    if (alphaNumber.length > 7) {
        inputEl.addClass("is-invalid");
        appalert(
            "❗ <strong>Maximum Length Exceeded</strong><br/>The Member's custom account number must not exceed <strong>7 digits</strong> to match migration format.",
            3, 1
        );
        return false;
    }

    // Remove error styling if all validations pass
    inputEl.removeClass("is-invalid");
    return true;
}


function PostFEE() {
    if (!checkTotalNotes()) return false;

    const totalNotes = parseFloat($("#totalNoteAmount").val());
    const totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    autoCheckDeposits();

    const deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("⚠️ Please select at least one loan fee label to process payment.", 3, 1);
        return;
    }

    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    let feeBreakdown = `
        <table class="table table-sm table-bordered w-100 mt-2">
            <thead class="table-light">
                <tr>
                    <th>Fee Label</th>
                    <th>Amount</th>
                </tr>
            </thead>
            <tbody>
    `;

    $('#myDataTableT tbody tr').each(function () {
        const feeLabel = $(this).find('td:eq(3)').text().trim();
        const amount = parseFloat($(this).find('.amount-input').val()) || 0;

        if (amount > 0) {
            feeBreakdown += `
                <tr>
                    <td>${feeLabel}</td>
                    <td>${amount.toLocaleString('en-US')} FCFA</td>
                </tr>
            `;
        }
    });

    feeBreakdown += `
            </tbody>
            <tfoot>
                <tr class="fw-bold text-dark">
                    <td class="text-end">Total</td>
                    <td>${totalInfo.total.toLocaleString('en-US')} FCFA</td>
                </tr>
            </tfoot>
        </table>
    `;

    const confirmationTitle = "🧾 CONFIRM LOAN FEE PAYMENT";
    const message = `
        <div class="text-start">
            <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
            <p>You are about to pay a <strong>LOAN APPLICATION FEE</strong> of <b>${totalInfo.total.toLocaleString('en-US')} FCFA</b>.</p>
            <p class="mt-3 fw-bold text-primary">Fee Summary</p>
            ${feeBreakdown}
        </div>
    `;

    alertify.confirm(
        confirmationTitle,
        message,
        function () {
            // 🧼 Clear depositor form
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            // 🎯 Set modal titles
            $("#depositorModalTitle").html(`<i class="mdi mdi-credit-card-outline me-2"></i> LOAN FEE DEPOSITOR`);
            $("#depositorModalDescription").text(`Please enter the depositor's full information before submitting the loan fee payment.`);

            // 👤 Set footer info
            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            // 🧾 Show the modal
            setTimeout(() => $("#depositerModal").modal("show"), 100);

            // ✅ Handle confirm button
            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                // Disable UI
                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                // Fill data
                deposits[0].Depositer = depositor;
                deposits[0].currencyNotes = collectCurrencyNotes();

                // Call AJAX
                PostTransaction('/CashDesk/PostRequestCash', deposits, 'LoanFee');
            });
        },
        function () {
            alertify.message("🚫 Loan fee payment cancelled.");
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
}


function PostWithdrawalFromFee() {
    if (!checkTotalNotes()) return false;

    const totalNotes = parseFloat($("#totalNoteAmount").val());
    const totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    autoCheckDeposits();
    const deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("⚠️ Please select at least one notification row to proceed.", 3, 1);
        return;
    }

    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    // 📊 Build notification fee summary table
    let feeTable = `
        <table class="table table-sm table-bordered w-100 mt-2">
            <thead class="table-light">
                <tr>
                    <th>Amount Requested</th>
                    <th>Form Fee</th>
                    <th>IDW</th>
                    <th>Grace</th>
                </tr>
            </thead>
            <tbody>
    `;

    $('#myDataTableT tbody tr').each(function () {
        const amount = parseFloat($(this).find('.amount-input').val()) || 0;
        const fee = parseFloat($(this).find('.fee-input').val()) || 0;
        const idw = $(this).find('.doiw').text();
        const dogp = $(this).find('.dogp').text();

        if (amount > 0 || fee > 0) {
            feeTable += `
                <tr>
                    <td>${amount.toLocaleString('en-US')} FCFA</td>
                    <td>${fee.toLocaleString('en-US')} FCFA</td>
                    <td>${idw}</td>
                    <td>${dogp}</td>
                </tr>
            `;
        }
    });

    feeTable += `
            </tbody>
            <tfoot>
                <tr class="fw-bold text-dark">
                    <td class="text-end" colspan="3">Total</td>
                    <td>${totalInfo.total.toLocaleString('en-US')} FCFA</td>
                </tr>
            </tfoot>
        </table>
    `;

    const confirmationTitle = "🔔 CONFIRM S.W.N PAYMENT";
    const message = `
        <div class="text-start">
            <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
            <p>You are about to pay a <strong>Saving Withdrawal Notification Fee</strong> of <b>${totalInfo.total.toLocaleString('en-US')} FCFA</b>.</p>
            ${feeTable}
        </div>
    `;

    alertify.confirm(
        confirmationTitle,
        message,
        function () {
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            $("#depositorModalTitle").html(`<i class="mdi mdi-bell-ring-outline me-2"></i> S.W.N DEPOSITOR INFO`);
            $("#depositorModalDescription").text(`Please provide the depositor's information to confirm this notification fee payment.`);

            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            setTimeout(() => $("#depositerModal").modal("show"), 100);

            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                deposits[0].Depositer = depositor;
                deposits[0].currencyNotes = collectCurrencyNotes();

                PostTransaction('/CashDesk/PostRequestCash', deposits, 'SavingWithdrawalFormFee');
            });
        },
        function () {
            alertify.message("🚫 Operation cancelled.");
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
}

// Similarly update PostCashOut() and PostLoanRepayment() functions


function PostCashOut() {
    if (!checkTotalNotes()) return false;

    const totalNotes = parseFloat($("#totalNoteAmount").val());
    const totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    autoCheckDeposits();
    const deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("⚠️ Please select at least one account with a valid amount.", 3, 1);
        return;
    }

    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    // Build table of accounts
    let accountSummaryHtml = `
        <table class="table table-sm table-bordered w-100 mt-2">
            <thead class="table-light">
                <tr><th>Account Type</th><th>Amount</th><th>Fee</th></tr>
            </thead><tbody>
    `;

    $('#myDataTableT tbody tr').each(function () {
        const accountType = $(this).find('td:eq(1)').text().trim();
        const amount = parseFloat($(this).find('.amount-input').val()) || 0;
        const fee = parseFloat($(this).find('.fee-input').val()) || 0;

        if (amount > 0 || fee > 0) {
            accountSummaryHtml += `
                <tr>
                    <td>${accountType}</td>
                    <td>${amount.toLocaleString('en-US')} FCFA</td>
                    <td>${fee.toLocaleString('en-US')} FCFA</td>
                </tr>`;
        }
    });

    accountSummaryHtml += `
        <tr class="table-light fw-bold">
            <td>Total</td>
            <td colspan="2">${totalInfo.total.toLocaleString('en-US')} FCFA</td>
        </tr></tbody></table>`;

    const message = `
        <div class="text-start">
            <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
            <p>You are about to <strong>perform a CASH-OUT</strong> of <b>${totalInfo.total.toLocaleString('en-US')} FCFA</b>.</p>
            <p class="mt-3 fw-bold text-danger">Accounts Summary</p>
            ${accountSummaryHtml}
        </div>
    `;

    alertify.confirm("🧾 CONFIRM CASH-OUT OPERATION", message,
        function () {
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            $("#depositorModalTitle").html(`<i class="mdi mdi-cash-remove me-2"></i> CASH-OUT DEPOSITOR`);
            $("#depositorModalDescription").text(`To complete this cash withdrawal, please provide the depositor's full identity.`);

            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            setTimeout(() => $("#depositerModal").modal("show"), 100);

            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                deposits[0].Depositer = depositor;
                deposits[0].currencyNotes = collectCurrencyNotes();

                PostTransaction('/CashDesk/PostRequestCash', deposits, 'Withdrawal');
            });
        },
        function () {
            alertify.message("🚫 Cash-out operation cancelled.");
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
}
function PostCashOutSWS() {
    const checkName = $('#CheckName').val();
    const checkNumber = $('#CheckNumber').val();

    if (!checkName || !checkNumber) {
        appalert("⚠️ Check Name and Check Number are required.", 3, 1);
        return false;
    }

    if (!checkTotalNotes()) return false;

    const totalNotes = parseFloat($("#totalNoteAmount").val());
    const totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    autoCheckDeposits();

    const deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("⚠️ Cash-out can only be done from one account only. Please deselect other accounts.", 3, 1);
        return;
    }

    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    let accountSummaryHtml = `
        <table class="table table-sm table-bordered w-100 mt-2">
            <thead class="table-light">
                <tr>
                    <th>Account Type</th>
                    <th>Amount</th>
                    <th>Fee</th>
                </tr>
            </thead>
            <tbody>
    `;

    $('#myDataTableT tbody tr').each(function () {
        const accountType = $(this).find('td:eq(1)').text().trim();
        const amount = parseFloat($(this).find('.amount-input').val()) || 0;
        const fee = parseFloat($(this).find('.fee-input').val()) || 0;

        if (amount > 0 || fee > 0) {
            accountSummaryHtml += `
                <tr>
                    <td>${accountType}</td>
                    <td>${amount.toLocaleString('en-US')} FCFA</td>
                    <td>${fee.toLocaleString('en-US')} FCFA</td>
                </tr>
            `;
        }
    });

    accountSummaryHtml += `
            <tr>
                <td colspan="2" class="text-end fw-bold">TOTAL</td>
                <td class="fw-bold text-primary">${totalInfo.total.toLocaleString('en-US')} FCFA</td>
            </tr>
        </tbody>
    </table>`;

    const message = `
        <div class="text-start">
            <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
            <p>You are about to perform a <strong>SPECIAL WITHDRAWAL SLIP (SWS) CASH-OUT</strong> of <b>${totalInfo.total.toLocaleString('en-US')} FCFA</b>.</p>
            <p class="mt-3 fw-bold text-primary">Account Summary</p>
            ${accountSummaryHtml}
        </div>
    `;

    alertify.confirm("💳 CONFIRM SWS CASH-OUT OPERATION", message,
        function () {
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            $("#depositorModalTitle").html(`<i class="mdi mdi-cash-minus me-2"></i> DEPOSITOR - SWS CASH-OUT`);
            $("#depositorModalDescription").text(`Please fill in the depositor's information to finalize the SWS withdrawal.`);

            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            setTimeout(() => $("#depositerModal").modal("show"), 100);

            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                deposits[0].Depositer = depositor;
                deposits[0].currencyNotes = collectCurrencyNotes();

                PostTransaction('/CashDesk/PostRequestCash', deposits, 'WithdrawalSWS');
            });
        },
        function () {
            alertify.message("🚫 Operation cancelled.");
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
}


function PostLoanRepayment() {
    if (!checkTotalNotes()) return false;

    const totalNotes = parseFloat($("#totalNoteAmount").val());
    const totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    const deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("Only one loan can be paid at an instant. Please deselect other accounts.", 3, 1);
        return;
    }

    const deposit = deposits[0];
    const memberName = $("#customerName").length ? $("#customerName").text().trim() : "Unknown Member";
    const customerId = $("#customerId").val() || "N/A";

    // Extract breakdown
    const capital = deposit.Amount || 0;
    const interest = deposit.Interest || 0;
    const penalty = deposit.Penalty || 0;
    const vat = deposit.Vat || 0;

    const confirmationTitle = "💼 CONFIRM LOAN REPAYMENT";
    const message = `
        <div class="text-start">
            <p><strong>Member:</strong> ${memberName}<br><strong>Member Account Number:</strong> ${customerId}</p>
            <p>You're about to perform a <strong>LOAN REPAYMENT</strong> of <b>${totalInfo.total.toLocaleString('en-US')} FCFA</b>.</p>
            <table class="table table-sm table-bordered w-100 mt-2">
                <thead class="table-light"><tr>
                    <th>Capital</th><th>Interest</th><th>VAT</th><th>Penalty</th>
                </tr></thead>
                <tbody>
                    <tr>
                        <td>${capital.toLocaleString('en-US')} FCFA</td>
                        <td>${interest.toLocaleString('en-US')} FCFA</td>
                        <td>${vat.toLocaleString('en-US')} FCFA</td>
                        <td>${penalty.toLocaleString('en-US')} FCFA</td>
                    </tr>
                </tbody>
            </table>
        </div>
    `;

    alertify.confirm(
        confirmationTitle,
        message,
        function () {
            // Reset depositor modal
            $('#depositorForm input, #depositorForm textarea').val('').removeClass('is-invalid');

            $("#depositorModalTitle").html(`<i class="mdi mdi-bank-transfer me-2"></i> LOAN REPAYMENT DEPOSITOR`);
            $("#depositorModalDescription").text(`To finalize loan repayment, please record the depositor’s full identity.`);

            $("#depositorCustomerName").text(memberName);
            $("#depositorCustomerId").text(customerId);

            setTimeout(() => $("#depositerModal").modal("show"), 100);

            $("#confirmDepositorBtn").off("click").on("click", function () {
                const depositor = collectDepositorInfo();
                if (!validateDepositor(depositor)) {
                    appalert("❗ All depositor fields are required to proceed.", 3, 1);
                    return;
                }

                $("#confirmDepositorBtn").prop("disabled", true);
                $("#depositorLoader").removeClass("d-none");

                deposit.Depositer = depositor;
                deposit.currencyNotes = collectCurrencyNotes();

                PostTransaction('/CashDesk/PostRequestCash', [deposit], 'LoanRepayment');
            });
        },
        function () {
            alertify.message("🚫 Loan repayment cancelled.");
        }
    ).set('labels', { ok: 'Yes, Continue', cancel: 'Cancel' });
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
    const memberId = $('#manualSearchInput').val().trim().toUpperCase(); // Normalize input
    console.log("🔍 Member’s Account Number Entered:", memberId);

    // Handle empty or missing input
    if (!memberId) {
        appalert(`
            ⚠️ <strong>Member’s Account Number Required</strong><br/>
            You must enter a valid <strong>Member’s Account Number</strong> to proceed with the Cash-In operation.
            <br/><br/>
            <strong>Format Guide:</strong>
            <ul style="margin-bottom: 0;">
                <li>🔹 <strong>Full Member:</strong> <code>3-digit Branch Code</code> + <code>7-digit Member Code</code> (e.g., <code>1010000456</code>)</li>
                <li>🔹 <strong>Pre-Registered Member:</strong> <code>3-digit Branch Code</code> + <code>PRM</code> + <code>Serial</code> (e.g., <code>101PRM0001</code>)</li>
            </ul>
            <br/>
            Please ensure the format is correct and try again.
        `, 3, 1);
        console.warn("❌ No valid Member’s Account Number provided.");
        return;
    }

    let operation;

    // Handle pre-registered members
    if (memberId.includes("PRM")) {
        operation = "newsubcription";
        console.log("🆕 New Subscription Mode Activated for PRM Member");
    } else {
        operation = "cashin";
        console.log("💰 Normal Cash-In Mode Activated for Full Member");
    }

    // Save operation context
    $("#currentselectedOperation").val(operation);

    // Trigger data load
    GetMemberData(memberId, '_OperationDesk', 'datalistingview', operation);
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

var selectedLoanId;
var selectedVatRate;

// Function to open modal and store selected row's LoanId and VatRate

//function updateTotal() {
//    // Get the raw input values without formatting
//    var amount = parseFloat(document.getElementById('modalAmount').value.replace(/,/g, '')) || 0;
//    var interest = parseFloat(document.getElementById('modalInterest').value.replace(/,/g, '')) || 0;
//    var penalty = parseFloat(document.getElementById('modalPenalty').value.replace(/,/g, '')) || 0;

//    // Calculate the total
//    var total = amount + interest + penalty;

//    // Display the formatted total (using XAF currency format)
//    document.getElementById('modalTotalAmount').value = `XAF ${total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
//}

// Apply formatting only when leaving the input field (onblur event)
function formatAmount(input) {
    var value = parseFloat(input.value.replace(/,/g, '')) || 0;
    input.value = value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}



// Function to open the payment modal and set the initial values
function openPaymentModal(loanId, accrualInterest, balance) {
    // Set the values in the modal
    console.log(loanId);
    document.getElementById('modalLoanId').value = loanId;
    document.getElementById('modalAccrualInterest').value = accrualInterest;
    document.getElementById('modalBalance').value = balance;

    // Clear previous values
    document.getElementById('modalAmount').value = '0';
    document.getElementById('modalInterest').value = '0';
    document.getElementById('modalPenalty').value = '0';
    document.getElementById('modalTotalAmount').value = '0';

    // Show the modal
    var paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
    paymentModal.show();
}

// Function to apply modal values to the table row
// Function to update the total amount in the modal
function updateTotal() {
    var amount = parseFloat(document.getElementById('modalAmount').value) || 0;
    var interest = parseFloat(document.getElementById('modalInterest').value) || 0;
    var penalty = parseFloat(document.getElementById('modalPenalty').value) || 0;

    // Calculate total (capital + interest + penalty)
    var total = amount + interest + penalty;

    // Format the total amount for display
    document.getElementById('modalTotalAmount').value = 'XAF ' + total.toLocaleString('en-US', { minimumFractionDigits: 2 });
}


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







//function applyVatAdjustment(loanId) {
//    const interestInput = document.getElementById("interest-" + loanId);
//    if (!interestInput || !vatMap[loanId]) return;

//    const { original, vat } = vatMap[loanId];
//    const netInterest = Math.round(original - vat);
//    interestInput.value = netInterest.toFixed(0);
//}


// VAT calculation function
//function calculateVat(interestInput, vatRate) {
//    // Parse the interest value from the input
//    var interest = parseFloat(interestInput.value) || 0;

//    // Calculate VAT based on the vatRate
//    var vat = interest * vatRate / 100;

//    // Format VAT with thousands separators and two decimal places
//    var formattedVat = vat.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

//    // Display the formatted VAT amount
//    document.getElementById('calculatedVat').innerText = `${formattedVat}`;
//}

// Function to open the loan details modal and populate it with data


function showLoanDetails(loan) {
    // Format amounts with thousands separators
    const formatCurrency = (amount) => {
        return `XAF ${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    };

    // Format percentage
    const formatPercentage = (value) => {
        const numberValue = parseFloat(value);
        if (isNaN(numberValue)) return 'N/A';
        return `${numberValue.toFixed(1)}%`;
    };

    // Populate modal fields with loan details
    document.getElementById('detailLoanDate').innerText = moment(loan.LoanDate).format('DD/MM/YYYY HH:mm:ss');
    document.getElementById('detailLoanAmount').innerText = formatCurrency(loan.LoanAmount);
    document.getElementById('detailBalance').innerText = formatCurrency(loan.Balance);
    document.getElementById('detailInterest').innerText = formatCurrency(loan.AccrualInterest);
    document.getElementById('detailDueAmount').innerText = formatCurrency(loan.DueAmount);
    document.getElementById('detailPenalty').innerText = formatCurrency(loan.Penalty);

    // Format and populate percentage fields
    document.getElementById('interestRate').innerText = formatPercentage(loan.InterestRate);
    document.getElementById('vatRate').innerText = formatPercentage(loan.VatRate);

    console.log(loan.VatRate)

    // Show the modal
    var loanDetailsModal = new bootstrap.Modal(document.getElementById('loanDetailsModal'));
    loanDetailsModal.show();
}

//function formatPercentage(value) {
//    // Ensure value is a number
//    const numberValue = parseFloat(value);
//    if (isNaN(numberValue)) return 'N/A';
//    return `${numberValue.toFixed(2)}%`;
//}