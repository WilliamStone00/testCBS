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

// Bind change event to the checkbox
// Add an event listener to the checkbox
//<a href="#" onclick="EditResetModal(null,'modal','modalContent','CashDesk','InitializeData','_DepositerForm','new_depositor','Depositor information','modalLabel')" data-toggle="modal" data-target="#depositerModal" class="btn btn-icon btn-label-primary btn-sm btn-fab demo btn-space customer-details" data-bs-toggle="tooltip" title="View customer details">
//    <i class="fas fa-user"></i> <!-- Customer details icon -->
//    <i class="fas fa-money-check"></i> <!-- Depositor icon -->
//</a>

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
    calculateBalance();
    var total = 0;
    $('.total-span').each(function () {
        total += parseFloat($(this).text());
    });
    var totalNotes = parseFloat($("#totalNoteAmount").val());

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

function collectDeposits() {
    var deposits = [];

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            var deposit = {};
            deposit.AccountNumber = $(this).find('td:eq(0)').text();
            deposit.Amount = parseFloat($(this).find('.amount-input').val());
            deposit.Fee = parseFloat($(this).find('.fee-input').val());
            deposit.Penalty = parseFloat($(this).find('.penalty-input').val());
            deposit.Interest = parseFloat($(this).find('.interest-input').val());
            deposit.Total = parseFloat($(this).find('.total-span').text());
            deposit.AccountType = $(this).find('td:eq(1)').text();
            deposit.Note = $('#Note').val();
            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.OperationType = $('#OperationType').val();
            deposit.CustomerId = $('#customerId').val();
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

//function PostCashIn() {
//    if (!checkTotalNotes()) return false;

//    var totalNotes = parseFloat($("#totalNoteAmount").val());
//    var total = calculateTotalAmount();

//    if (!validateTotalAmount(total, totalNotes)) return;

//    var deposits = collectDeposits();
//    if (deposits.length === 0) {
//        appalert("Please select at least one account to perform the cash-in.", 3, 1);
//        return;
//    }

//    deposits[0].currencyNotes = collectCurrencyNotes();
//    deposits[0].Depositer = collectDepositorInfo();

//    var message = "WARNING!!!\n";
//    message += "Are you sure you want to perform a cash-in of " + total + " to the selected account numbers?\n";
//    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
//    confirmTransaction('Confirm Cash-In Operation', message, '/CashDesk/PostRequestCashIn', deposits);
//}
function PostCashIn() {
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
    deposits[0].Depositer = collectDepositorInfo();

    var message = "WARNING!!!\n";
    message += "Are you sure you want to perform a cash-in of " + totalInfo.total + " to the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-In Operation', message, '/CashDesk/PostRequestCash', deposits);
}

// Similarly update PostCashOut() and PostLoanRepayment() functions

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

    var message = "WARNING!!!\n";
    message += "Are you sure you want to perform a cash-out of " + totalInfo.total + " from the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-Out Operation', message, '/CashDesk/PostRequestCash', deposits);
}

function PostLoanRepayment() {
    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("Loan repayment can only be done from one account only. Please deselect other accounts.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "WARNING!!!\n";
    message += "Are you sure you want to perform loan repayment of " + totalInfo.total + " to the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Loan Repayment Operation', message, '/CashDesk/PostRequestCash', deposits);
}



//function PostCashIn() {
//    // Check if any rows are selected
//    var anyRowsSelected = false;
//    $('#myDataTableT tbody tr').each(function () {
//        if ($(this).find('.form-check-input').prop('checked')) {
//            anyRowsSelected = true;
//            return false; // Exit the loop early since at least one row is selected
//        }
//    });

//    // If no rows are selected, notify the user and exit the function
//    if (!anyRowsSelected) {
//        appalert("Please select at least one account to perform the cash-in.", 3, 1);
//        return;
//    }
//    var total = 0;
//    // Calculate total cash-in amount
//    $('#myDataTableT tbody tr').each(function () {
//        // Check if the checkbox in this row is selected
//        if ($(this).find('.form-check-input').prop('checked')) {
//            // Add the total amount of the checked row to the total
//            total += parseFloat($(this).find('.total-span').text());
//        }
//    });
//    // Check if total cash-in amount is zero
//    if (total === 0) {
//        appalert("Please enter an amount to cash in.", 3, 1);
//        return; // Exit the function
//    }

//    var totalNotes = parseFloat($("#totalNoteAmount").val());

//    // Check if total amount entered matches total notes entered
//    if (total !== totalNotes) {
//        appalert("Amount entered must equal the total of notes entered. Please reevaluate and enter again.", 3, 1);
//        return; // Exit the function
//    }

//    // Prepare the array to hold deposit objects
//    var deposits = [];

//    // Loop through each selected row in the table to collect deposit information
//    $('#myDataTableT tbody tr').each(function () {
//        // Check if the checkbox in this row is selected
//        if ($(this).find('.form-check-input').prop('checked')) {
//            var deposit = {};

//            // Get the values from the row
//            deposit.AccountNumber = $(this).find('td:eq(0)').text();
//            deposit.Amount = parseFloat($(this).find('.amount-input').val());
//            deposit.Fee = parseFloat($(this).find('.fee-input').val());
//            deposit.Penalty = parseFloat($(this).find('.penalty-input').val());
//            deposit.Interest = parseFloat($(this).find('.interest-input').val());
//            deposit.Total = parseFloat($(this).find('.total-span').text());
//            deposit.AccountType = $(this).find('td:eq(1)').text();
//            deposit.Note = $(this).find('.form-control').val();
//            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');

//            // Push the deposit object into the array
//            deposits.push(deposit);
//        }
//    });

//    // Add the total notes denomination to the first deposit object
//    if (deposits.length > 0) {
//        deposits[0].currencyNotes = {
//            note10000: parseFloat($('#Notes_note10000').val()),
//            note5000: parseFloat($('#Notes_note5000').val()),
//            note2000: parseFloat($('#Notes_note2000').val()),
//            note1000: parseFloat($('#Notes_note1000').val()),
//            note500: parseFloat($('#Notes_note500').val()),
//            coin500: parseFloat($('#Notes_coin500').val()),
//            coin100: parseFloat($('#Notes_coin100').val()),
//            coin50: parseFloat($('#Notes_coin50').val()),
//            coin25: parseFloat($('#Notes_coin25').val()),
//            coin10: parseFloat($('#Notes_coin10').val()),
//            coin5: parseFloat($('#Notes_coin5').val()),
//            coin1: parseFloat($('#Notes_coin1').val())
//        };
//    }

//    // Add depositer's information to the first deposit object
//    if (deposits.length > 0) {
//        var depositer = {
//            DepositorName: $('#DepositorName').val(),
//            DepositerTelephone: $('#DepositerTelephone').val(),
//            DepositorIDNumber: $('#DepositorIDNumber').val(),
//            DepositorIDIssueDate: $('#DepositorIDIssueDate').val(),
//            DepositorIDExpiryDate: $('#DepositorIDExpiryDate').val(),
//            DepositorIDNumberPlaceOfIssue: $('#DepositorIDNumberPlaceOfIssue').val(),
//            DepositerNote: $('#DepositerNote').val()
//        };
//        deposits[0].Depositer = depositer;
//    }

//    // Prepare the data to be sent in the AJAX request
//    var data = JSON.stringify(deposits);

//    // Construct the message string with the relevant details
//    var message = "WARNING!!!\n";
//    message += "Are you sure you want to perform a cash-in of " + total + " to the selected account numbers?\n";
//    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
//    // Add other relevant details as needed

//    // Show confirmation dialog
//    alertify.confirm("Cash-In Confirmation", message,
//        function () {
//            // User clicked OK, proceed with cash-in
//            $.ajax({
//                url: '/CashDesk/PostRequestCashIn',
//                type: 'POST',
//                contentType: 'application/json',
//                data: data,
//                success: function (response) {
//                    if (response.success) {
//                        appalert(response.message, 1, 1);
//                        //ReportView("Operation", null, "GetReport", null, null, "receipts", "ReportParameterLess");
//                    } else {
//                        if (response.message === undefined) {
//                            alert("Your session is expired.");
//                        } else {
//                            appalert(response.message, 3, 1);
//                        }
//                    }
//                },
//                error: function (xhr, status, error) {
//                    appalert(error, 0, 1);
//                }
//            });
//        },
//        function () {
//            // User clicked Cancel, do nothing
//            appalert('Transaction cancelled', 3, 1);
//        }
//    );
//}

//function PostCashOut() {
//    // Check if any rows are selected
//    var anyRowsSelected = false;
//    $('#myDataTableT tbody tr').each(function () {
//        if ($(this).find('.form-check-input').prop('checked')) {
//            anyRowsSelected = true;
//            return false; // Exit the loop early since at least one row is selected
//        }
//    });

//    // If no rows are selected, notify the user and exit the function
//    if (!anyRowsSelected) {
//        appalert("Please select at least one account to perform the cash-out.", 3, 1);
//        return;
//    }

//    // If no rows are selected or more than one row is selected, notify the user and exit the function
//    var selectedRowCount = $('#myDataTableT tbody tr').find('.form-check-input:checked').length;
//    if (selectedRowCount !== 1) {
//        if (selectedRowCount === 0) {
//            appalert("Please select at least one account to perform the cash-out.", 3, 1);
//        } else {
//            appalert("Cash-out can only be done from one account only. Please deselect other accounts.", 3, 1);
//        }
//        return;
//    }

//    var total = 0;
//    // Calculate total cash-out amount
//    $('#myDataTableT tbody tr').each(function () {
//        // Check if the checkbox in this row is selected
//        if ($(this).find('.form-check-input').prop('checked')) {
//            // Add the total amount of the checked row to the total
//            total += parseFloat($(this).find('.total-span').text());
//        }
//    });

//    // Check if total cash-in amount is zero
//    if (total === 0) {
//        appalert("Please enter an amount to cash in.", 3, 1);
//        return; // Exit the function
//    }

//    var totalNotes = parseFloat($("#totalNoteAmount").val());

//    // Check if total amount entered matches total notes entered
//    if (total !== totalNotes) {
//        appalert("Amount entered must equal the total of notes entered. Please reevaluate and enter again.", 3, 1);
//        return; // Exit the function
//    }

//    // Prepare the array to hold deposit objects
//    var deposits = [];

//    // Loop through each selected row in the table to collect deposit information
//    $('#myDataTableT tbody tr').each(function () {
//        // Check if the checkbox in this row is selected
//        if ($(this).find('.form-check-input').prop('checked')) {
//            var deposit = {};

//            // Get the values from the row
//            deposit.AccountNumber = $(this).find('td:eq(0)').text();
//            deposit.Amount = parseFloat($(this).find('.amount-input').val());
//            deposit.Fee = parseFloat($(this).find('.fee-input').val());
//            deposit.Penalty = parseFloat($(this).find('.penalty-input').val());
//            deposit.Interest = parseFloat($(this).find('.interest-input').val());
//            deposit.Total = parseFloat($(this).find('.total-span').text());
//            deposit.AccountType = $(this).find('td:eq(1)').text();
//            deposit.Note = $(this).find('.form-control').val();
//            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');

//            // Push the deposit object into the array
//            deposits.push(deposit);
//        }
//    });

//    // Add the total notes denomination to the first deposit object
//    if (deposits.length > 0) {
//        deposits[0].currencyNotes = {
//            note10000: parseFloat($('#Notes_note10000').val()),
//            note5000: parseFloat($('#Notes_note5000').val()),
//            note2000: parseFloat($('#Notes_note2000').val()),
//            note1000: parseFloat($('#Notes_note1000').val()),
//            note500: parseFloat($('#Notes_note500').val()),
//            coin500: parseFloat($('#Notes_coin500').val()),
//            coin100: parseFloat($('#Notes_coin100').val()),
//            coin50: parseFloat($('#Notes_coin50').val()),
//            coin25: parseFloat($('#Notes_coin25').val()),
//            coin10: parseFloat($('#Notes_coin10').val()),
//            coin5: parseFloat($('#Notes_coin5').val()),
//            coin1: parseFloat($('#Notes_coin1').val())
//        };
//    }

//    // Add depositer's information to the first deposit object
//    if (deposits.length > 0) {
//        var depositer = {
//            DepositorName: $('#DepositorName').val(),
//            DepositerTelephone: $('#DepositerTelephone').val(),
//            DepositorIDNumber: $('#DepositorIDNumber').val(),
//            DepositorIDIssueDate: $('#DepositorIDIssueDate').val(),
//            DepositorIDExpiryDate: $('#DepositorIDExpiryDate').val(),
//            DepositorIDNumberPlaceOfIssue: $('#DepositorIDNumberPlaceOfIssue').val(),
//            DepositerNote: $('#DepositerNote').val()
//        };
//        deposits[0].Depositer = depositer;
//    }

//    // Prepare the data to be sent in the AJAX request
//    var data = JSON.stringify(deposits);

//    // Construct the message string with the relevant details
//    var message = "WARNING!!!\n";
//    message += "Are you sure you want to perform a cash-out of " + total + " from the selected account numbers?\n";
//    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
//    // Add other relevant details as needed

//    // Show confirmation dialog
//    alertify.confirm("Cash-Out Confirmation", message,
//        function () {
//            // User clicked OK, proceed with cash-in
//            $.ajax({
//                url: '/CashDesk/PostRequestCashIn',
//                type: 'POST',
//                contentType: 'application/json',
//                data: data,
//                success: function (response) {
//                    if (response.success) {
//                        appalert(response.message, 1, 1);
//                        //ReportView("Operation", null, "GetReport", null, null, "receipts", "ReportParameterLess");
//                    } else {
//                        if (response.message === undefined) {
//                            alert("Your session is expired.");
//                        } else {
//                            appalert(response.message, 3, 1);
//                        }
//                    }
//                },
//                error: function (xhr, status, error) {
//                    appalert(error, 0, 1);
//                }
//            });
//        },
//        function () {
//            // User clicked Cancel, do nothing
//            appalert('Transaction cancelled', 3, 1);
//        }
//    );
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
    calculateBalance();
}
function GetMemberData(Key, partialView, divToloadPV, path) {

    AddORUpdateGen(Key, divToloadPV, partialView, path, "CashDesk");
    calculateBalance();
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
