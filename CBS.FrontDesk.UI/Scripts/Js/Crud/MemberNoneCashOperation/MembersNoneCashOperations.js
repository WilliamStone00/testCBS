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
            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');
            deposit.OperationType = $('#OperationType').val();
            deposit.IsSWS = false;
            deposit.CustomerId = $('#customerId').val();
            deposit.LoanApplicationId = $(this).find('.loan-application-id').val();
            deposit.Period = $(this).find('.period').val();

            // New captured values
            deposit.BookingDirection = $("input[name='AddMembersNoneCashOperationCommand.BookingDirection']:checked").val();
            deposit.ChartOfAccountId = $('#account_number').val();
            deposit.Note = $('#Note').val();

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

function PostCashIn() {
    var deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("Please select at least one account to perform the cash-in.", 3, 1);
        return;
    }

    // Calculate total amount from user inputs in the table
    var totalInfo = calculateTotalAmount();

    // Collect the selected amounts from the input fields (Amount + Fee)
    var selectedTotalAmount = 0;
    $('#myDataTableT tbody tr').each(function () {
        var amount = parseFloat($(this).find('.amount-input').val()) || 0;
        var fee = parseFloat($(this).find('.fee-input').val()) || 0;
        selectedTotalAmount += (amount + fee);
    });

    // Compare calculated total with the user input
    if (totalInfo.total !== selectedTotalAmount) {
        appalert("The total amount does not match the sum of the selected account amounts and fees.", 3, 1);
        return;
    }

    deposits[0].currencyNotes = collectCurrencyNotes();
    deposits[0].Depositer = collectDepositorInfo();

    // Check if one of the radio buttons is selected
    var sourceType = $("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked").val();
    if (!sourceType) {
        appalert("Please select operator type, Either Mobile Money MTN OR Mobile Money Orange", 3, 1);
        return;
    }

    var message = "";
    message += "Are you sure you want to perform a cash-in of " + totalInfo.total + " to the selected account numbers?\n";
    message += "Account Numbers: " + getSelectedAccountNumbers() + "\n";
    confirmTransaction('Confirm Cash-In Operation', message, '/CashDesk/PostRequestCash', deposits, 'CashInMomocashCollection');
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
