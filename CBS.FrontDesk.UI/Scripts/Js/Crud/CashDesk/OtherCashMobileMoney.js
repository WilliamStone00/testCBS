
function GetMember() {
    var id = $('#manualSearchInput').val();
    var url = "/CashDesk/Ajaxloader?Key=" + id + "&path=getmember";
    $.ajax({
        type: "GET",
        url: url,
        success: function (data) {
            // Check if the FeeBase is Percentage or Range and show/hide the corresponding divs
            $('#customerId').val(data.CustomerId);
            $('#Name').val(data.FirstName + " " + data.LastName)
            //$('#select_base').html("Configuration option: " + data.FeeBase);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
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

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            var deposit = {};
            deposit.CustomerId = $('#customerId').val();
            deposit.TellerCode = $('#TellerCode').val();
            deposit.Amount = parseFloat($(this).find('.amount-input').val());
            deposit.Total = parseFloat($(this).find('.total-span').text());
            deposit.OperationType = $('#OperationType').val();
            deposit.MemberName = $('#MemberName').val();
            deposit.CNI = $('#CNI').val();
            deposit.TelephoneNumber = $('#TelephoneNumber').val();
            deposit.SourceType = $("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked").val();
            deposit.BookingDirection = $("input[name='AddOtherTransactionMobileMoneyCommand.BookingDirection']:checked").val();
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




function PostMobileMoney() {

    // Check if at least one table row is checked
    var checkedRows = $("#myDataTableT tbody input[type='checkbox']:checked");
    if (checkedRows.length === 0) {
        appalert("Please select the confirmation option from the table", 3, 1);
        return;
    }

    // Check if one of the radio buttons is selected
    var sourceType = $("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked").val();
    if (!sourceType) {
        appalert("Please select operator type, Either Mobile Money MTN OR Mobile Money Orange", 3, 1);
        return;
    }
    // Check if one of the radio buttons is selected
    var bookingDirection = $("input[name='AddOtherTransactionMobileMoneyCommand.BookingDirection']:checked").val();
    if (!bookingDirection) {
        appalert("Please select operation type, Either Cash-in OR Cash-out operation", 3, 1);
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
        appalert("Please select at least one account to perform the cash-in.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    var message = "";
    // Check if MemberAccount radio button is checked
    if ($('#deposit').prop('checked')) {
        message += "Are you sure you want to perform a cashin of " + totalInfo.total + " to " + $("#TelephoneNumber").val() + "?\n";
    } else {
        message += "Are you sure you want to perform a cashout of " + totalInfo.total + " from " + $("#TelephoneNumber").text() + "?\n";
    }
    confirmTransaction('Confirm Mobile-Money Operation', message, '/CashDesk/PostRequestCash', deposits);
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








