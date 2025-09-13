
function GetMember() {
    console.log(id);
    var id = $('#manualSearchInput').val();
    var url = "/CashDesk/Ajaxloader?Key=" + id + "&path=getmember";
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            // Check if the FeeBase is Percentage or Range and show/hide the corresponding divs
            $('#customerId').val(data.CustomerId);
            $('#Name').val(data.FirstName +" "+ data.LastName)
            //$('#select_base').html("Configuration option: " + data.FeeBase);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}

function Reprint() {
    ReportView("CashDesk", null, "GetReportOtherPayment", null, null, "receipts", "ReportParameterLess");

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
        appalert("Amount entered must equal the total of notes entered. Make sure you have checked/unchecked corresponding accounts. Please reevaluate and enter again.", 3, 1);
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
            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.OperationType = $('#OperationType').val();
            deposit.CustomerId = $('#customerId').val(); // Assuming this is the selected customer ID
            deposit.Period = $('#Name').val(); // Assuming this is the selected customer ID
            deposit.Note = $('#Naration').val(); // Assuming this is the selected customer ID
            deposit.SourceType = $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val(); // Get the selected source type
            deposit.EventCode = $('#BulkDeposit_OtherTransaction_EventCode').val(); // Assuming this is the selected event code value
            // Push the updated deposit object to the deposits array
            deposit.ExternalBranchId = externalBranchId;
            deposits.push(deposit);
        }
    });
    
    return deposits;
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





function PostExpense() {

    // Check if at least one table row is checked
    var checkedRows = $("#myDataTableT tbody input[type='checkbox']:checked");
    if (checkedRows.length === 0) {
        appalert("Please select the confirmation option from the table", 3, 1);
        return;
    }

    // Check if one of the radio buttons is selected
    var sourceType = $("input[name='BulkDeposit.OtherTransaction.SourceType']:checked").val();
    if (!sourceType) {
        appalert("Please select source type, Either It's a Member OR Staff/None Member", 3, 1);
        return;
    }
    // Check if amount entered is greater than 0 and not negative
    var amountInputs = $("#myDataTableT tbody input.amount-input");
    var isValidAmount = true;
    amountInputs.each(function () {
        var amount = parseFloat($(this).val());
        if (isNaN(amount) || amount <= 0) {
            isValidAmount = false;
            return false; // Exit the loop early
        }
    });
    if (!isValidAmount) {
        appalert("Please enter a valid amount greater than 0");
        return;
    }


    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("Please select at least one record to perform the operation.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    var message = "";
    // Check if MemberAccount radio button is checked
    message += "Are you sure you want to process a transaction of " + totalInfo.total + " for the service: " + $("#BulkDeposit_OtherTransaction_EventCode option:selected").text() + "?\n";

    confirmTransaction('Confirm Expense Operation', message, '/CashDesk/PostRequestCash', deposits);
}


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












