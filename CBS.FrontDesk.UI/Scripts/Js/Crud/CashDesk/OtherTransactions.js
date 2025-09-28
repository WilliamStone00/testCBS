$(document).ready(function () {
    // Initial call to set the correct visibility on page load
    toggleAccountDropdown();

    // Hide the manualSearchInputButton on page load
    document.getElementById("manualSearchInputbutton").style.display = "none";

    // Attach toggle function to radio buttons to trigger on change
    document.getElementById("memberAccountOption").addEventListener("change", toggleAccountDropdown);
    document.getElementById("cashCollectionOption").addEventListener("change", toggleAccountDropdown);
});
// Function to toggle visibility of the "Member Account" dropdown and related inputs
function toggleAccountDropdown() {
    var isCashCollected = document.getElementById("cashCollectionOption").checked;
    var accountDropdown = document.getElementById("memberAccountDropdown");
    var manualSearchInput = document.getElementById("manualSearchInput");
    var manualSearchInputButton = document.getElementById("manualSearchInputbutton");

    // Show or hide the Member Account dropdown based on the selected option
    accountDropdown.style.display = isCashCollected ? "none" : "block";

    //// Show or hide the manual search input and button based on the selected option
    //manualSearchInput.style.display = isCashCollected ? "none" : "block";
    //manualSearchInputButton.style.display = isCashCollected ? "none" : "block";

    // Reset the dropdown if Member Account is selected
    if (!isCashCollected) {
        var accountNumberDropdown = document.getElementById("account_number");
        accountNumberDropdown.selectedIndex = 0; // Reset dropdown to the first option
    }
}


function GetMember() {
    var id = $('#manualSearchInput').val().trim();

    // Check if manualSearchInput is empty
    if (!id) {
        appalert("Please enter a member reference number.", 3, 1);
        return;
    }

    var url = "/CashDesk/Ajaxloader?Key=" + id + "&path=getmember";
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            // Check if the FeeBase is Percentage or Range and show/hide the corresponding divs
            $('#customerId').val(data.CustomerId);
            $('#Name').val(data.FirstName + " " + data.LastName);
            //$('#select_base').html("Configuration option: " + data.FeeBase);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}


function LoadMembersAccounts(affectedId) {
    $('#customerId').val('');
    $('#Name').val('');
    var id = $('#manualSearchInput').val().trim();

    // Check if manualSearchInput is empty
    if (!id) {
        appalert("Please enter a member reference number.", 3, 1);
        return;
    }

    var SourceType = $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val();
    console.log(SourceType);
    if (SourceType == "Cash_Collected") {
    } else {
        var id = $('#manualSearchInput').val();
        var url = "/CashDesk/Ajaxloader?Key=" + id;
        FillDropDownAjaxCallParam(url, affectedId, "---Select account---");
        GetMember();
    }

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
    var total = 0;
    $('.total-span').each(function () {
        total += parseFloat($(this).text());
    });

    // Calculate balance
    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var balance = totalNotes - total;

    // Update balance in the table footer
    var formattedBalance = balance.toLocaleString('en-US', { style: 'currency', currency: 'XAF' });
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

    // Append the total value to the tableTotal cell
    var formattedTotal = total.toLocaleString('en-US', { style: 'currency', currency: 'XAF' });
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
        const diff = totalNotes - total.total;
        appalert(`Amount mismatch! 
Selected Accounts = ${total.total.toLocaleString()} 
Denominations = ${totalNotes.toLocaleString()} 
Difference = ${diff.toLocaleString()}`, 3, 1);
        return false;
    }

    return true;
}


function collectDeposits() {
    var deposits = [];
    var externalBranchId = $("#ExternalBranchId").val();
    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            var deposit = {};
            // Update the deposit object with values from form inputs in the current row
            deposit.AccountNumber = $('#account_number').val(); // Assuming this is the selected account number
            deposit.Amount = parseFloat($(this).find('.amount-input').val());
            deposit.Total = parseFloat($(this).find('.total-span').text());
            deposit.Note = $('#DepositerNote').val();
            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.OperationType = $('#OperationType').val();
            deposit.Period = $('#Name').val(); // Assuming this is the selected customer ID
            deposit.CustomerId = $('#customerId').val(); // Assuming this is the selected customer ID
            deposit.SourceType = $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val(); // Get the selected source type
            deposit.EventCode = $('#BulkDeposit_OtherTransaction_EventCode').val(); // Assuming this is the selected event code value
            // Push the updated deposit object to the deposits array
            // ✅ Only assign ExternalBranchId if inter-branch is checked
            deposit.ExternalBranchId = externalBranchId;
            deposits.push(deposit);
        }
    });

    return deposits;
}


function Reprint() {
    ReportView("CashDesk", null, "GetReportOtherPayment", null, null, "receipts", "ReportParameterLess");

}
function confirmTransaction(title, message, ajaxUrl, data) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        ReportView("CashDesk", null, "GetReportOtherPayment", null, null, "receipts", "ReportParameterLess");
                        window.PageReload();
                    } else {
                        if (response.message === undefined) {
                            alert("Your session is expired.");
                        } else {
                            appalert(response.message, 3, 1);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    appalert(error, 0, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}




function PostCashOut() {
    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("Cash-out can only be done from one account only. Please deselect other accounts.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "";
    message += "Are you sure you want to perform a cash-out of " + totalInfo.total + " from the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-Out Operation', message, '/CashDesk/PostRequestCash', deposits, 'Withdrawal');
}



// ===================== Context & UI =====================
function getFormMode() {
    // "OtherCashIn" or "OtherCashOut" (set in a hidden input by the view/controller)
    return ($('#FormContext').val() || 'OtherCashIn');
}
function isCashIn() { return getFormMode() === 'OtherCashIn'; }

// Apply context: title + source label
function applyContextUI() {
    const cashIn = isCashIn();
    $('#contextTitle').text(cashIn ? '💰 Other Cash-In' : '💸 Other Cash-Out');
    $('#sourceCollectLabel').text(cashIn ? 'Collect Cash' : 'Payout Cash');
}
$(function () { applyContextUI(); });

// ===================== Helpers =========================
function money(n) {
    const v = parseFloat(n || 0);
    return v.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}
function getSourceTypeRaw() {
    return $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val() || "";
}
function getSourceTypeFinal() {
    // In Cash-Out, if user chose "Cash_Collected", post "Payout_Cash"
    const raw = getSourceTypeRaw(); // "Cash_Collected" | "Member_Account"
    if (!isCashIn() && raw === 'Cash_Collected') return 'Payout_Cash';
    return raw;
}
function getExternalBranchId() { return $("#isInterBranchCheck").prop("checked") ? ($("#ExternalBranchId").val() || "") : ""; }
function getDenominationTotal() { return parseFloat($("#totalNoteAmount").val() || "0") || 0; }
function getName() { return ($("#Name").val() || "").trim(); }
function getCNI() { return ($("#CNI").val() || "").trim(); }
function getTelephone() { return ($("#TelephoneNumber").val() || "").trim(); }
function getTopNarration() { return ($("#DepositerNote").val() || "").trim(); }
function getCustomerId() { return $("#customerId").val() || ""; }
function getAccountNumber() { return $("#account_number").val() || ""; }
function getAccountingDate() { const v = $('.accounting-date').val(); return v || new Date().toISOString().slice(0, 10); }

function collectAllocationsFromTable() {
    const rows = [];
    $("#myDataTableT tbody tr").each(function () {
        const $tr = $(this);
        if (!$tr.find(".alloc-check").prop("checked")) return;
        const accountId = $tr.find(".accountid-select").val() || "";
        const amount = parseFloat($tr.find(".alloc-amount").val() || "0") || 0;
        const naration = ($tr.find(".alloc-note").val() || "").trim();
        if (accountId && amount > 0) rows.push({ AccountId: accountId, Amount: amount, Naration: naration, _row: $tr });
    });
    return rows;
}
function sumAllocations(list) { return list.reduce((s, r) => s + (r.Amount || 0), 0); }
function collectCurrencyNotesSafe() {
    try { return (typeof collectCurrencyNotes === 'function') ? collectCurrencyNotes() : null; }
    catch { return null; }
}

// ===================== Build payload ===================
function buildOtherTxnPayload() {
    const lines = collectAllocationsFromTable();
    const amountTotal = sumAllocations(lines);
    const notesTotal = getDenominationTotal();

    const isOut = !isCashIn();
    const useOtherSrc = isOut ? $('#IsOtherSource').prop('checked') : false;
    const otherSrcId = isOut && useOtherSrc ? ($('#OtherSourceOfAccountId').val() || '') : null;

    const cmd = {
        Amount: amountTotal,
        Direction: isCashIn() ? 'Credit' : 'Debit',
        Name: getName(),
        Naration: getTopNarration(),
        TransactionType: isCashIn() ? 'Income' : 'Expense',
        SourceType: getSourceTypeFinal(),
        CustomerId: getCustomerId(),
        AccountNumber: getAccountNumber(),
        ExternalBranchId: getExternalBranchId(),
        AccountAmountCollections: lines.map(x => ({ AccountId: x.AccountId, Amount: x.Amount, Naration: x.Naration })),
        CurrencyNotesRequest: collectCurrencyNotesSafe(),
        AccountingDate: getAccountingDate(),

        // NEW
        CNI: getCNI(),
        TelephoneNumber: getTelephone(),
        IsOtherSource: useOtherSrc,
        OtherSourceOfAccountId: otherSrcId
    };

    return { cmd, amountTotal, notesTotal, lines };
}
// ===================== Confirmation (ASH) ==============
function renderConfirmationHtml(summary) {
    const { cmd, amountTotal, notesTotal, lines } = summary;

    // Header (ash)
    const header = `
    <div style="background:#eeeeee;border:1px solid #ddd;border-radius:6px; padding:10px 12px; width:100%;">
      <div style="display:flex;flex-wrap:wrap;gap:16px;font-size:0.95rem;">
        <div><b>Context:</b> ${isCashIn() ? 'Other Cash-In' : 'Other Cash-Out'}</div>
        <div><b>Transaction Type:</b> ${cmd.TransactionType}</div>
        <div><b>Direction:</b> ${cmd.Direction}</div>
        <div><b>Source:</b> ${cmd.SourceType || '(not set)'}</div>
        ${cmd.ExternalBranchId ? `<div><b>Destination Branch:</b> ${cmd.ExternalBranchId}</div>` : ''}
        <div><b>Accounting Date:</b> ${cmd.AccountingDate}</div>
        ${cmd.Name ? `<div><b>Name:</b> ${cmd.Name}</div>` : ''}
        ${cmd.CNI ? `<div><b>CNI:</b> ${cmd.CNI}</div>` : ''}
      </div>
      ${cmd.TelephoneNumber ? `<div style="margin-top:6px;"><b>Telephone:</b> ${cmd.TelephoneNumber}</div>` : ''}
      ${cmd.Naration ? `<div style="margin-top:6px;"><b>Narration:</b> ${cmd.Naration.replace(/</g, '&lt;')}</div>` : ''}
    </div>`;

    // Rows
    const rows = lines.map((r, i) => {
        const text = r._row?.find(".accountid-select option:selected").text() || r.AccountId;
        return `
      <tr>
        <td style="padding:8px; border-top:1px solid #e5e5e5;">${i + 1}</td>
        <td style="padding:8px; border-top:1px solid #e5e5e5;">${text}</td>
        <td style="padding:8px; border-top:1px solid #e5e5e5;">${(r.Naration || '').replace(/</g, '&lt;')}</td>
        <td style="padding:8px; border-top:1px solid #e5e5e5; text-align:right;">${money(r.Amount)}</td>
      </tr>`;
    }).join('');

    // Table (ash header, full width)
    const table = `
    <div style="border:1px solid #ddd; border-radius:6px; margin-top:10px; overflow:hidden; width:100%;">
      <table style="width:100%; border-collapse:separate; border-spacing:0;">
        <thead>
          <tr style="background:#f2f2f2; color:#333; border-bottom:1px solid #ddd;">
            <th style="padding:8px; text-align:left; width:36px; border-right:1px solid #e5e5e5;">#</th>
            <th style="padding:8px; text-align:left; border-right:1px solid #e5e5e5;">Account</th>
            <th style="padding:8px; text-align:left; border-right:1px solid #e5e5e5;">Note</th>
            <th style="padding:8px; text-align:right; width:120px;">Amount</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
        <tfoot>
          <tr style="background:#fafafa;">
            <td colspan="3" style="padding:10px 8px; text-align:right; border-top:1px solid #e5e5e5;"><b>Total (Selected):</b></td>
            <td style="padding:10px 8px; text-align:right; border-top:1px solid #e5e5e5;"><b>${money(amountTotal)}</b></td>
          </tr>
          <tr style="background:#fafafa;">
            <td colspan="3" style="padding:8px; text-align:right; border-top:1px solid #e5e5e5;">Total Notes (Denominations):</td>
            <td style="padding:8px; text-align:right; border-top:1px solid #e5e5e5;">${money(notesTotal)}</td>
          </tr>
        </tfoot>
      </table>
    </div>`;

    // Wrap in a container that expands to dialog width
    return `<div style="width:100%; max-width:100%;">${header}${table}</div>`;
}

// ===================== Validation ======================
function validateOtherTxn(summary) {
    const { cmd, amountTotal, notesTotal, lines } = summary;

    if (!lines.length) { appalert("Please select at least one account (check the 'Select' column).", 3, 1); return false; }
    if (amountTotal <= 0) { appalert("Please enter a valid amount (> 0) on the selected rows.", 3, 1); return false; }
    if (!cmd.SourceType) { appalert("Please select a Source Type.", 3, 1); return false; }
    if (!cmd.Name || !cmd.Name.trim()) { appalert("Name cannot be empty.", 3, 1); return false; }

    // Phone sanity check (optional but helpful)
    const phoneDigits = (cmd.TelephoneNumber || '').replace(/\D/g, '');
    if (phoneDigits && phoneDigits.length < 8) {
        appalert("Please check the telephone number.", 3, 1);
        return false;
    }

    if (!(notesTotal > 0)) { appalert("Please enter cash in the denomination box.", 3, 1); return false; }
    if (Math.abs(amountTotal - notesTotal) > 0.0001) {
        const diff = notesTotal - amountTotal;
        appalert(`Amount mismatch!
Selected Accounts = ${money(amountTotal)}
Denominations = ${money(notesTotal)}
Difference = ${money(diff)}`, 3, 1);
        return false;
        // NEW: if expense and "other source" is checked, require the GL
        const isOut = ($('#FormContext').val() || 'OtherCashIn') === 'OtherCashOut';
        if (isOut && cmd.IsOtherSource && !cmd.OtherSourceOfAccountId) {
            appalert("Please select the Source GL for this expense.", 3, 1);
            return false;
        }
    }
    return true;
}

// ===================== Submit (both modes) =============
function PostOtherCashIn() {
    const summary = buildOtherTxnPayload();
    if (!validateOtherTxn(summary)) return;

    const html = renderConfirmationHtml(summary);
    const title = isCashIn() ? "Confirm Other Cash-In" : "Confirm Other Cash-Out";

    alertify.confirm(
        title,
        html,
        function onOk() {
            $.ajax({
                url: "/CashDesk/OtherCashinPosting",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(summary.cmd),
                success: function (response) {
                    if (response && response.success) {
                        appalert(response.message || "Transaction posted successfully.", 1, 1);
                        //if (response.redirectUrl) window.open(response.redirectUrl, "_blank");
                        if (window.PageReload) window.PageReload();
                    } else {
                        const msg = (response && response.message) ? response.message : "Operation failed.";
                        appalert(msg, 3, 1);
                    }
                },
                error: function (xhr, status, err) {
                    appalert(err || "Failed to post transaction.", 0, 1);
                }
            });
        },
        function onCancel() { appalert("Transaction cancelled.", 3, 1); }
    );
}

// Make available to inline onclick (non-module script)
window.PostOtherCashIn = PostOtherCashIn;







//function PostOtherCashIn() {
//    // ✅ Check confirmation in table rows
//    var checkedRows = $("#myDataTableT tbody input[type='checkbox']:checked");
//    if (checkedRows.length === 0) {
//        appalert("Please confirm at least one row in the table.", 3, 1);
//        return;
//    }

//    // ✅ Ensure a Source Type is selected
//    var sourceType = $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val();
//    if (!sourceType) {
//        appalert("Please select a source type.", 3, 1);
//        return;
//    }

//    // ✅ Check valid Amount entries
//    var amountInputs = $("#myDataTableT tbody input.amount-input");
//    var isValidAmount = true;
//    amountInputs.each(function () {
//        var amount = parseFloat($(this).val());
//        if (isNaN(amount) || amount <= 0) {
//            isValidAmount = false;
//            return false;
//        }
//    });
//    if (!isValidAmount) {
//        appalert("Please enter a valid amount greater than 0.", 3, 1);
//        return;
//    }

//    // ✅ Validate denominations
//    if (!checkTotalNotes()) return false;

//    // ✅ Validate amount vs denominations
//    var totalNotes = parseFloat($("#totalNoteAmount").val());
//    var totalInfo = calculateTotalAmount();
//    if (!validateTotalAmount(totalInfo, totalNotes)) return;

//    // ✅ Prepare data object
//    var deposits = collectDeposits();
//    if (deposits.length === 0) {
//        appalert("Please provide at least one account entry.", 3, 1);
//        return;
//    }

//    // ✅ Fetch form data
//    var eventCode = document.getElementById("BulkDeposit_OtherTransaction_EventCode").value;
//    var memberName = document.getElementById("Name").value;

//    if (!eventCode) {
//        appalert("Please select an Event Item.", 3, 1);
//        return;
//    }

//    if (!memberName || memberName.trim() === "") {
//        appalert("Member name cannot be empty.", 3, 1);
//        return;
//    }

//    // ✅ Add currency notes breakdown
//    deposits[0].currencyNotes = collectCurrencyNotes();

//    // ✅ Construct confirmation message
//    var message = `Are you sure you want to record a cash-in of ${totalInfo.total} for the service: ` +
//        `${$("#BulkDeposit_OtherTransaction_EventCode option:selected").text()}?
//Name: ${memberName}`;

//    // ✅ Final call
//    confirmTransaction('Confirm Cash-In Operation', message, '/CashDesk/PostRequestCash', deposits);
//}





function checkTotalNotes() {
    var totalNotes = parseFloat($("#totalNoteAmount").val());

    if (totalNotes === 0) {
        appalert("Please enter cash in the denomination box.", 3, 1);
        return false;
    }

    return true;
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
    calculateBalance();
}








