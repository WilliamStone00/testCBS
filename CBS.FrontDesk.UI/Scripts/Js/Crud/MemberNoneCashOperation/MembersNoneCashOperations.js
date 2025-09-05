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
    var loan_amount_input = parseFloat($row.find('.loan-amount-input').val()) || 0;
    var total = amount + fee + penalty + interest + loan_amount_input;
    $row.find('.total-span').text(total.toFixed(2));
    calculateTableTotal();
});

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

function getAccountingDateISO() {
    // try id, then name, then any date input with the known name
    const $el = $('#BulkDeposit_AccountingDate').length
        ? $('#BulkDeposit_AccountingDate')
        : $("[name='BulkDeposit.AccountingDate']");
    return ($el.val() || '').trim(); // already yyyy-MM-dd from <input type="date">
}

function collectDeposits() {
    var deposits = [];
    const accountingDate = getAccountingDateISO(); // <-- robust read

    $('#myDataTableT tbody tr').each(function () {
        const $tr = $(this);
        if ($tr.find('.row-select').prop('checked')) {
            const deposit = {};
            deposit.AccountNumber = $tr.find('td:eq(0)').text().trim();
            deposit.AccountType = $tr.find('td:eq(1)').text().trim();
            deposit.Amount = parseFloat($tr.find('.amount-input').val()) || 0;
            deposit.Fee = parseFloat($tr.find('.fee-input').val()) || 0;
            deposit.Penalty = parseFloat($tr.find('.penalty-input').val()) || 0;
            deposit.Interest = parseFloat($tr.find('.interest-input').val()) || 0;
            deposit.Total = parseFloat($tr.find('.total-span').text()) || 0;
            deposit.isDepositDoneByAccountOwner = $tr.find('.row-select').prop('checked') === true;
            deposit.IsChargesInclussive = $tr.find('.check-inclussive').prop('checked') === true;
            deposit.OperationType = $('#OperationType').val();
            deposit.IsSWS = false;
            deposit.CustomerId = $('#customerId').val();
            deposit.MemberName = $('#memberName').val();
            deposit.LoanApplicationId = $tr.find('.loan-application-id').val();
            deposit.Period = $tr.find('.period').val();
            deposit.ChartOfAccountId = $('#account_number').val();
            deposit.ChartOfAccountName = $('#account_number option:selected').text().trim();
            deposit.BookingDirection = $("input[name='AddMembersNoneCashOperationCommand.BookingDirection']:checked").val();
            deposit.Note = $('#Note').val();

            // 📅 Date & Branch
            deposit.AccountingDate = accountingDate;        // yyyy-MM-dd
            deposit.BranchId = $('#branchInput').val();

            // MoMo
            deposit.IsMobileMoneyOperation = $('#isMobileMoneyOperationChk').is(':checked');
            deposit.NoneMemberMobileReference = $('#noneMemberMobileReference').val();
            deposit.MobileMoneyPath = $("input[name='momoPath']:checked").val() || '';   // <-- NEW
            deposits.push(deposit);
        }
    });

    return deposits;
}


function collectCurrencyNotes() {
    return {
        note10000: parseFloat($('#Notes_note10000').val()),
        note5000: parseFloat($('#Notes_note5000').val()),
        note2000: parseFloat($('#Notes_note2000').val()),
        note1000: parseFloat($('#Notes_note1000').val()),
        note500: parseFloat($('#Notes_note500').val()),
        coin500: parseFloat($('#Notes_coin500').val()),
        coin100: parseFloat($('#Notes_coin100').val()),
        coin50: parseFloat($('#Notes_coin50').val()),
        coin25: parseFloat($('#Notes_coin25').val()),
        coin10: parseFloat($('#Notes_coin10').val()),
        coin5: parseFloat($('#Notes_coin5').val()),
        coin1: parseFloat($('#Notes_coin1').val())
    };
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
function Reprint() {
    ReportView("CashDesk", null, "GetReport", null, null, "receipts", "ReportParameterLess");

}
// Example: adjust your confirmTransaction to support HTML bodies
function confirmTransaction(title, message, ajaxUrl, data, operationType, isHtml = false, okText = 'OK', cancelText = 'Cancel') {
    var dlg = alertify.confirm('', function () { // empty here…
        $.ajax({
            url: ajaxUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response && response.success) {
                    successCallback(response, operationType);
                } else {
                    if (!response) alert("Your session is expired.");
                    else failureCallback(response);
                }
            },
            error: function () {
                appalert("Your session is expired Or An error occurred while processing the transaction. Please try again later.", 0, 1);
            }
        });
    }, function () {
        appalert('Transaction cancelled', 3, 1);
    });

    // ✅ ensure custom title & HTML body
    dlg.set('title', title);
    if (isHtml) dlg.setContent(message); else dlg.setContent($('<div>').text(message).html());

    // ✅ custom buttons
    dlg.set('labels', { ok: okText, cancel: cancelText });
    dlg.set('closable', false);
}

function successCallback(response, operationType) {
    appalert(response.message, 1, 1);
    resetDepositorForm();
    switch (operationType) {
        case 'MemberNoneCash':
            GetMemberData($("#customerId").val(), '_MemberNonCashDesk', 'datalistingview', 'cashin');
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

function PostOperation() {
    // Build from checked rows only
    var deposits = collectDeposits();

    // 1) Must have a selected row
    if (!deposits || deposits.length === 0) {
        appalert("Please select at least one account to perform operation.", 3, 1);
        return;
    }
    if (deposits.length !== 1) {
        appalert("Please select exactly one account to perform the operation.", 3, 1);
        return;
    }

    // 2) Totals (from selected row only)
    var selectedTotalAmount = deposits.reduce((sum, d) => sum + Number(d.Amount || 0) + Number(d.Fee || 0), 0);
    var totalInfo = calculateTotalAmount(); // { total: ... }
    if (Math.abs(Number(totalInfo.total || 0) - selectedTotalAmount) > 0.009) {
        appalert("The total amount does not match the sum of the selected account amounts and fees.", 3, 1);
        return;
    }

    // 3) Booking direction
    var bookingDirection = $("input[name='AddMembersNoneCashOperationCommand.BookingDirection']:checked").val();
    if (!bookingDirection) {
        appalert("Please select booking direction. Either Debit or Credit.", 3, 1);
        return;
    }

    // 4) Branch validation
    var branchId = $('#branchInput').val();
    var branchText = $('#branchInput option:selected').text().trim();
    if (!branchId || branchText.startsWith('---')) {
        appalert("Please select a valid Branch.", 3, 1);
        if ($('#branchInput').hasClass('select2')) $('#branchInput').select2('open'); else $('#branchInput').focus();
        return;
    }

    // 5) Accounting date validation (required, not in the future)
    var accountingDateRaw = $('#BulkDeposit_AccountingDate').val(); // yyyy-MM-dd
    if (!accountingDateRaw) {
        appalert("Please choose an Accounting Date.", 3, 1);
        $('#BulkDeposit_AccountingDate').focus();
        return;
    }
    var m = accountingDateRaw.match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (!m) {
        appalert("Invalid Accounting Date format.", 3, 1);
        $('#BulkDeposit_AccountingDate').focus();
        return;
    }
    var acctDate = new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]));
    var today = new Date(); today.setHours(0, 0, 0, 0);
    if (acctDate > today) {
        appalert("Accounting Date cannot be in the future.", 3, 1);
        $('#BulkDeposit_AccountingDate').focus();
        return;
    }

    // 6) MoMo controls (checkbox-based)
    var isMomo = $('#isMobileMoneyOperationChk').is(':checked');
    var momoCategory = $("input[name='momoCategory']:checked").val() || '';
    var momoPath = $("input[name='momoPath']:checked").val() || '';                 // <-- NEW
    var momoRefVal = $('#noneMemberMobileReference').val();
    var momoRefText = $('#noneMemberMobileReference option:selected').text().trim();

    if (isMomo) {
        if (!momoCategory) {
            appalert('Please select a MM/OM Service type/Category (MTN or Orange).', 3, 1);
            return;
        }
        if (!momoPath) {
            appalert('Please select a MM/OM operation flow.', 3, 1);                        // <-- NEW
            return;
        }
        if (!momoRefVal) {
            appalert('Please select the MM/OM Collection transit account corresponding to the chosen reference.', 3, 1);
            $('#noneMemberMobileReference').focus();
            return;
        }
    }

    // 7) Corresponding GL — ONLY validate when MoMo is OFF
    var correspondingAccountId = '';
    var correspondingAccountName = '';
    if (!isMomo) {
        correspondingAccountId = $('#account_number').val();
        correspondingAccountName = $('#account_number option:selected').text().trim();
        if (!correspondingAccountId || correspondingAccountId === '---Select GL---') {
            appalert("Please select a valid Corresponding Account (GL).", 3, 1);
            $('#account_number').focus();
            return;
        }
    }
    // If MoMo is ON, we leave GL empty; UI already hides/disables it.

    // 8) Note required
    var note = $('#Note').val().trim();
    if (!note) {
        appalert("Please enter a reason for the operation in the Note field.", 3, 1);
        $('#Note').focus();
        return;
    }

    // 9) Build confirmation with the SAME selected data
    var memberAccountNames = deposits.map(d => (d.AccountType || '').toString().trim()).filter(Boolean).join(', ');
    var memberAccounts = deposits.map(d => (d.AccountNumber || '').toString().trim()).filter(Boolean).join(', ');

    var htmlMsg = buildConfirmHtml({
        bookingDirection: bookingDirection.toUpperCase(),
        branchName: branchText,
        accountingDate: accountingDateRaw,
        correspondingAccountName: correspondingAccountName,
        correspondingAccountId: correspondingAccountId,
        isMomo: isMomo,
        momoCategory: momoCategory,
        momoRefText: momoRefText || '',
        momoPath: momoPath,                                 // <-- NEW
        memberAccountNames: memberAccountNames,
        memberAccounts: memberAccounts,
        total: selectedTotalAmount
    });


    // 10) Confirm
    confirmTransaction(
        'Member–GL Confirmation',
        htmlMsg,
        '/MemberNoneCashOperation/PostRequestCash',
        deposits,
        'CashInMomocashCollection',
        true,               // HTML body
        'Yes, Proceed',     // OK
        'Cancel'            // Cancel
    );
}
function getAccountingDateValue() {
    // Try common Razor/MVC id + name + class fallbacks
    const $byId = $('#BulkDeposit_AccountingDate');
    const $byName = $("input[name='BulkDeposit.AccountingDate']");
    const $byCls = $('input.accounting-date[type="date"]');

    let raw = ($byId.val() || $byName.val() || $byCls.val() || '').trim();
    return raw || ''; // '' if not found
}

function getSelectedAccountNames() {
    var names = [];
    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            // column 1 is the Account/Product name in your table
            names.push($(this).find('td:eq(1)').text().trim());
        }
    });
    return names.join(', ');
}

function buildConfirmHtml(opts) {
    const esc = s => String(s ?? '').replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m]));
    const fmtNum = n => (Number(n || 0)).toLocaleString();

    function fmtDate(input) {
        if (!input) return '';
        let d;
        if (input instanceof Date) d = input;
        else if (typeof input === 'string') {
            const m = input.match(/^(\d{4})-(\d{2})-(\d{2})$/);
            if (m) return `${m[3]}/${m[2]}/${m[1]}`;
            d = new Date(input);
            if (isNaN(d)) return esc(input);
        } else return esc(String(input));
        return String(d.getDate()).padStart(2, '0') + '/' + String(d.getMonth() + 1).padStart(2, '0') + '/' + d.getFullYear();
    }
    function pathLabel(path) {                              // <-- NEW
        switch (path) {
            case 'FloatFromMembersAccountToMomoAccount':
                return 'Member account → MoMo Float (increase float)';
            case 'FromMembersAccountToMobilemoneyAndToMembersPhone':
                return 'Member account → MoMo Float → Member’s phone (wallet payout)';
            case 'FloatFromMobileMoneyToMembersAccount':
                return 'Member’s phone → MoMo Float → Member account (wallet to branch)';
            default:
                return path || '';
        }
    }
    // friendlier MoMo category label if canonical key is passed
    const niceMomoCategory = (() => {
        const v = String(opts.momoCategory || '').toLowerCase();
        if (!v) return '';
        if (v.includes('mtn')) return 'MTN Cameroon';
        if (v.includes('orange')) return 'Orange Money Cameroon';
        return opts.momoCategory; // already a friendly label
    })();

    const rows = [];

    rows.push(`
      <tr>
        <th style="width:200px;padding:6px 10px;text-align:left;">Operation</th>
        <td style="padding:6px 10px;"><strong>${esc(opts.bookingDirection)} member’s account</strong></td>
      </tr>
    `);

    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Branch</th>
        <td style="padding:6px 10px;">${esc(opts.branchName || '')}</td>
      </tr>
    `);

    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Accounting Date</th>
        <td style="padding:6px 10px;">${fmtDate(opts.accountingDate) || 'N/A'}</td>
      </tr>
    `);

    // Corresponding GL row: hide details when MoMo is selected
    if (opts.isMomo) {
        rows.push(`
          <tr>
            <th style="padding:6px 10px;text-align:left;">Corresponding Account (GL)</th>
            <td style="padding:6px 10px;"><em>Tills linked MM/OM Configurations</em></td>
          </tr>
        `);
    } else {
        rows.push(`
          <tr>
            <th style="padding:6px 10px;text-align:left;">Corresponding Account</th>
            <td style="padding:6px 10px;">
              <div><strong>${esc(opts.correspondingAccountName || 'N/A')}</strong></div>
              <div style="opacity:.8">GL Number: ${esc(opts.correspondingAccountId || 'N/A')}</div>
            </td>
          </tr>
        `);
    }

    // Mobile Money rows
    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Mobile Money</th>
        <td style="padding:6px 10px;">${opts.isMomo ? 'Yes' : 'No'}</td>
      </tr>
    `);

    if (opts.isMomo) {
        rows.push(`
          <tr>
            <th style="padding:6px 10px;text-align:left;">MoMo Category</th>
            <td style="padding:6px 10px;">${esc(niceMomoCategory)}</td>
          </tr>
        `);
        if (opts.momoPath) {
            rows.push(`
              <tr>
                <th style="padding:6px 10px;text-align:left;">MM/OM Operation flow</th>
                <td style="padding:6px 10px;">${esc(pathLabel(opts.momoPath))}</td>
              </tr>
            `);
        }
        if (opts.momoRefText) {
            rows.push(`
              <tr>
                <th style="padding:6px 10px;text-align:left;">Mobile Money Reference</th>
                <td style="padding:6px 10px;">${esc(opts.momoRefText)}</td>
              </tr>
            `);
        }
    }

    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Selected Member Account Name(s)</th>
        <td style="padding:6px 10px;">${esc(opts.memberAccountNames || '')}</td>
      </tr>
    `);

    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Selected Member Account(s)</th>
        <td style="padding:6px 10px;">${esc(opts.memberAccounts || '')}</td>
      </tr>
    `);

    rows.push(`
      <tr>
        <th style="padding:6px 10px;text-align:left;">Total Amount</th>
        <td style="padding:6px 10px;"><strong>${fmtNum(opts.total)}</strong></td>
      </tr>
    `);

    return `
      <div style="font-size:13px;line-height:1.35;">
        <table style="width:100%;border-collapse:collapse;">
          ${rows.join('')}
        </table>
        <div style="margin-top:10px;padding:8px 10px;border-left:3px solid #f0ad4e;background:#fff7e6;">
          <strong>Note:</strong> This operation will remain <em>PENDING</em> until it is validated by an authorized user.
          The selected accounts will be impacted only after validation.
        </div>
        <div style="margin-top:10px;"><strong>Are you ready to proceed with this operation?</strong></div>
      </div>
    `;
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
    AddORUpdateGen(KEY, divToLoadData, partialView, path, "MemberNoneCashOperation");


}
function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "MemberNoneCashOperation");
    //calculateBalance();
}
function GetMember() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    GetMemberData(memberId, '_MemberNonCashDesk', 'datalistingview', operation);
    /*GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);
    var spanElement = document.getElementById('cashDeskOperations');

    // Default style
    spanElement.style.fontWeight = "bold";
    spanElement.style.textDecoration = "underline";
    spanElement.style.textDecorationThickness = "2px";

    spanElement.innerText = "MEMBER'S OTHER NONE-CASH OPERATION";

    AddORUpdateGen(Key, divToloadPV, partialView, path, "MemberNoneCashOperation");
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

