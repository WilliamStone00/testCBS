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
//function collectDeposits() {
//    var deposits = [];

//    var alphaNumber = $('#CustomerAlphaNumber').val();
//    var customerId = $('#customerId').val();
//    var operationType = $('#OperationType').val();
//    var checkName = $('#CheckName')?.val() || '';
//    var checkNumber = $('#CheckNumber')?.val() || '';
//    var note = $('#Note')?.val() || '';

//    $('#myDataTableT tbody tr').each(function () {
//        if ($(this).find('.form-check-input').prop('checked')) {
//            var deposit = {};

//            deposit.AccountNumber = $(this).find('td:eq(0)').text().trim();
//            deposit.AccountType = $(this).find('td:eq(1)').text().trim();
//            deposit.Balance = parseFloat($(this).find('td:eq(2)').text()) || 0;

//            deposit.Amount = parseFloat($(this).find('.amount-input').val()) || 0;
//            deposit.Fee = parseFloat($(this).find('.fee-input').val()) || 0;
//            deposit.Penalty = parseFloat($(this).find('.penalty-input')?.val()) || 0;
//            deposit.Interest = parseFloat($(this).find('.interest-input')?.val()) || 0;
//            deposit.Total = parseFloat($(this).find('.total-span').text()) || 0;

//            deposit.Note = note;
//            deposit.CheckName = checkName;
//            deposit.CheckNumber = checkNumber;

//            deposit.CustomerAlphaNumber = alphaNumber;
//            deposit.CustomerId = customerId;
//            deposit.OperationType = operationType;

//            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
//            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');

//            deposit.IsSWS = true;
//            deposit.PaymentMethod = 'Cash';
//            deposit.PaymentChannel = 'Web_Portal';

//            // Optional fields if present
//            deposit.LoanApplicationId = $(this).find('.loan-application-id')?.val() || '';
//            deposit.Period = $(this).find('.period')?.val() || '';

//            deposits.push(deposit);
//        }
//    });

//    console.log("Collected Deposits:", deposits); // Debug output
//    return deposits;
//}


//function collectDeposits() {
//    var deposits = [];

//    $('#myDataTableT tbody tr').each(function () {
//        if ($(this).find('.form-check-input').prop('checked')) {
//            var deposit = {};
//            deposit.AccountNumber = $(this).find('td:eq(0)').text();
//            deposit.Amount = parseFloat($(this).find('.amount-input').val());
//            deposit.Fee = parseFloat($(this).find('.fee-input').val());
//            deposit.Penalty = parseFloat($(this).find('.penalty-input')?.val() || 0);
//            deposit.Interest = parseFloat($(this).find('.interest-input')?.val() || 0);
//            deposit.Total = parseFloat($(this).find('.total-span').text()) || 0;
//            deposit.AccountType = $(this).find('td:eq(1)').text();
//            deposit.Note = $('#Note').val();
//            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
//            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');
//            deposit.OperationType = $('#OperationType').val();
//            deposit.CheckName = $('#CheckName').val();
//            deposit.CheckNumber = $('#CheckNumber').val();
//            deposit.IsSWS = true;
//            deposit.CustomerId = $('#customerId').val();
//            deposit.LoanApplicationId = $(this).find('.loan-application-id')?.val();
//            deposit.Period = $(this).find('.period')?.val();
//            deposit.PaymentMethod = 'Cash';
//            deposit.PaymentChannel = 'Web_Portal';
//            deposit.CustomerAlphaNumber = $('#CustomerAlphaNumber').val(); // ✅ correct assignment

//            deposits.push(deposit);
//        }
//    });

//    return deposits;
//}
function collectDeposits() {
    var deposits = [];

    var alphaNumber = $('#CustomerAlphaNumber').val();
    var customerId = $('#customerId').val();
    var operationType = $('#OperationType').val();
    var checkName = $('#CheckName')?.val() || '';
    var checkNumber = $('#CheckNumber')?.val() || '';
    var note = $('#Note')?.val() || '';

    // ✅ Capture the global calculated VAT from the footer element
    var globalVat = parseFloat($('#calculatedVat').text().replace(/,/g, '')) || 0;

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            var deposit = {};

            deposit.AccountNumber = $(this).find('td:eq(0)').text().trim();
            deposit.AccountType = $(this).find('td:eq(1)').text().trim();
            deposit.Balance = parseFloat($(this).find('td:eq(2)').text()) || 0;

            deposit.Amount = parseFloat($(this).find('.amount-input').val()) || 0;
            deposit.Fee = parseFloat($(this).find('.fee-input').val()) || 0;
            deposit.Penalty = parseFloat($(this).find('.penalty-input')?.val()) || 0;
            deposit.Interest = parseFloat($(this).find('.interest-input')?.val()) || 0;
            deposit.Total = parseFloat($(this).find('.total-span').text()) || 0;

            deposit.Note = note;
            deposit.CheckName = checkName;
            deposit.CheckNumber = checkNumber;

            deposit.CustomerAlphaNumber = alphaNumber;
            deposit.CustomerId = customerId;
            deposit.OperationType = operationType;

            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');

            deposit.IsSWS = true;
            deposit.PaymentMethod = 'Cash';
            deposit.PaymentChannel = 'Web_Portal';

            // Optional fields if present
            deposit.LoanApplicationId = $(this).find('.loan-application-id')?.val() || '';
            deposit.Period = $(this).find('.period')?.val() || '';

            // ✅ Add captured VAT
            deposit.Vat = globalVat;

            deposits.push(deposit);
        }
    });

    console.log("✅ Collected Deposits with VAT:", deposits);
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
                        successCallback(response, operationType);
                        Reprint();
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
        case 'CashIn':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'cashin');

            break;
        case 'Withdrawal':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'cashout');
            break;
        case 'WithdrawalSWS':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'cashoutsws');
            break;
        case 'SavingWithdrawalFormFee':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'withdrawalnotification');
            break;
        case 'LoanRepayment':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'repayment');
            break;
        case 'LoanFee':
            GetMemberData($("#customerId").val(), '_OperationDesk', 'datalistingview', 'loanapplicationfeepayment');
            break;
        default:
            break;
    }
}

function failureCallback(response) {
    appalert(response.message || "Your session is expired Or An error occurred while processing the transaction", 3, 1);
}

function PostCashIn() {
    if (!validateCustomerAlphaNumber()) return;
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

    var message = "";
    message += "Are you sure you want to perform a cash-in of " + totalInfo.total + " to the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-In Operation', message, '/CashDesk/PostRequestCash', deposits, 'CashIn');
}
function validateCustomerAlphaNumber() {
    var alphaNumber = $("#CustomerAlphaNumber").val()?.trim();

    // 1. Required field
    if (!alphaNumber) {
        appalert(
            "The Member's custom account number is required. This number uniquely identifies the member from the previous system and is necessary for linking their records in Trust Soft Credit (TSC).",
            3, 1
        );
        return false;
    }

    // 2. Must be numeric only
    if (!/^\d+$/.test(alphaNumber)) {
        appalert(
            "The Member's custom account number must contain digits only. Please enter the exact number as provided in the legacy system to ensure correct member linkage in TSC.",
            3, 1
        );
        return false;
    }

    // 3. Max length check
    if (alphaNumber.length > 7) {
        appalert(
            "The Member's custom account number must not exceed 7 digits. This is to ensure compatibility with the format used during migration from the legacy system.",
            3, 1
        );
        return false;
    }

    return true;
}


function PostFEE() {
    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("Please select at least one fee label to perform the payment", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "";
    message += "Are you sure you want to pay loan fee of " + totalInfo.total + "?\n";
    confirmTransaction('Loan Fee Operation', message, '/CashDesk/PostRequestCash', deposits, 'LoanFee');
}

function PostWithdrawalFromFee() {
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

    var message = "";
    message += "Are you sure you want to confirm the payment of " + totalInfo.total + " as withdrawal form fee?\n";
    confirmTransaction('Payment of S.W.F', message, '/CashDesk/PostRequestCash', deposits, 'SavingWithdrawalFormFee');
}

// Similarly update PostCashOut() and PostLoanRepayment() functions


function PostCashOut() {
    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    //if (deposits.length !== 1) {
    //    appalert("Cash-out can only be done from one account only. Please deselect other accounts.", 3, 1);
    //    return;
    //}

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "";
    message += "Are you sure you want to perform a cash-out of " + totalInfo.total + " from the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-Out Operation', message, '/CashDesk/PostRequestCash', deposits, 'Withdrawal');
}
function PostCashOutSWS() {
    // Validate CheckName and CheckNumber
    var checkName = $('#CheckName').val();
    var checkNumber = $('#CheckNumber').val();

    if (!checkName || !checkNumber) {
        appalert("Check Name and Check Number are required.", 3, 1);
        return false;
    }

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
    message += "Are you sure you want to perform a SWS cash-out of " + totalInfo.total + " from the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm SWS Cash-Out Operation', message, '/CashDesk/PostRequestCash', deposits, 'WithdrawalSWS');
}


function PostLoanRepayment() {
    if (!checkTotalNotes()) return false;

    var totalNotes = parseFloat($("#totalNoteAmount").val());
    var totalInfo = calculateTotalAmount();

    if (!validateTotalAmount(totalInfo, totalNotes)) return;

    var deposits = collectDeposits();
    if (deposits.length !== 1) {
        appalert("Only one loan can be paid at an instant. Please deselect other accounts.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    var message = "";
    message += "Are you sure you want to perform loan repayment of " + totalInfo.total + "?";
    confirmTransaction('Confirm Loan Repayment Operation', message, '/CashDesk/PostRequestCash', deposits, 'LoanRepayment');
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
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    GetMemberData(memberId, '_OperationDesk', 'datalistingview', operation);
    /*GetMemberData(memberId, '_OperationDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);
    var spanElement = document.getElementById('cashDeskOperations');

    // Default style
    spanElement.style.fontWeight = "bold";
    spanElement.style.textDecoration = "underline";
    spanElement.style.textDecorationThickness = "2px";

    if (path == "cashin") {
        spanElement.innerText = "CASH-IN OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "cashout") {
        spanElement.innerText = "CASH-OUT OPERATIONS";
        spanElement.style.color = "red";
    }
    else if (path == "cashoutsws") {
        spanElement.innerText = "SWS CASH-OUT OPERATIONS";
        spanElement.style.color = "red";
    }
    else if (path == "repayment") {
        spanElement.innerText = "LOAN REPAYMENT OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "withdrawalnotification") {
        spanElement.innerText = "SAVING WITHDRAWAL NOTIFICATION PAYMENT";
        spanElement.style.color = "red";
    }
    else if (path == "loanapplicationfeepayment") {
        spanElement.innerText = "LOAN APPLICATION FEE PAYMENT";
        spanElement.style.color = "green";
    }
    else if (path == "search") {
        spanElement.innerText = "MEMBER'S INFORMATION";
        spanElement.style.color = "blue";
    }
    else {
        spanElement.innerText = "CASH OPERATIONS";
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

// Function to apply the payment values from the modal to the table row
//function applyPayment() {
//    var loanId = document.getElementById('modalLoanId').value;
//    var amount = parseFloat(document.getElementById('modalAmount').value) || 0;
//    var interest = parseFloat(document.getElementById('modalInterest').value) || 0;
//    var penalty = parseFloat(document.getElementById('modalPenalty').value) || 0;

//    // Update the row with the values entered from the modal
//    var capitalInput = document.getElementById('capital-' + loanId);
//    var interestInput = document.getElementById('interest-' + loanId);
//    var penaltyInput = document.getElementById('penalty-' + loanId);
//    var totalSpan = document.getElementById('total-' + loanId);

//    // Check if elements are found
//    if (!capitalInput || !interestInput || !penaltyInput || !totalSpan) {
//        console.error("One or more elements not found. Check your IDs and ensure they match.");
//        return;
//    }

//    // Update the row with entered values
//    capitalInput.value = amount;
//    interestInput.value = interest;
//    penaltyInput.value = penalty;

//    // Calculate total (capital + interest + penalty)
//    var total = amount + interest + penalty;
//    totalSpan.innerText = total;
//    calculateTableTotal();
//    // Calculate VAT based on the entered interest
//    //calculateVat(interestInput, selectedVatRate);

//    // Close the modal
//    var paymentModal = bootstrap.Modal.getInstance(document.getElementById('paymentModal'));
//    paymentModal.hide();
//}
//function calculateTableTotal() {
//    let total = 0;
//    let totalVat = 0;

//    // Sum all visible row totals (capital + interest + penalty)
//    $('.total-span').each(function () {
//        total += parseFloat($(this).text()) || 0;
//    });

//    // Sum all VATs from hidden inputs or inputs with class 'vat-input'
//    $('.vat-input').each(function () {
//        totalVat += parseFloat($(this).value || $(this).val()) || 0;
//    });

//    // Grand total includes VAT
//    const grandTotal = total + totalVat;

//    // Display total VAT in footer
//    $('#calculatedVat').text(totalVat.toLocaleString('en-US'));

//    // Compute and display balance
//    const totalNotes = parseFloat($("#totalNoteAmount").val()) || 0;
//    const balance = totalNotes - grandTotal;
//    $('#tableBalance').text(balance.toLocaleString('en-US'));

//    // Color balance if negative
//    $('#tableBalance').toggleClass('text-danger', balance < 0);

//    // Set comparison icon
//    $('#tableTotal .total-icon').remove();
//    let iconClass = '', iconColor = '';
//    if (grandTotal === totalNotes) {
//        iconClass = 'fas fa-check-circle'; iconColor = 'text-success';
//    } else if (grandTotal < totalNotes) {
//        iconClass = 'fas fa-exclamation-circle'; iconColor = 'text-warning';
//    } else {
//        iconClass = 'fas fa-times-circle'; iconColor = 'text-danger';
//    }

//    // Display total with icon
//    $('#tableTotal').html(`<span>${grandTotal.toLocaleString('en-US')}</span> <i class="${iconClass} total-icon ${iconColor}"></i>`);

//    // Enable/disable button
//    $('#submit').prop('disabled', grandTotal !== totalNotes);
//}

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
    if (!interestInput || !vatMap[loanId]) return;

    const { original, vat } = vatMap[loanId];
    const netInterest = Math.round(original - vat);
    interestInput.value = netInterest.toFixed(0);
}


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