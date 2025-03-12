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
            deposit.Amount = parseFloat($(this).find('.amount-input').val()) || 0;
            deposit.Fee = parseFloat($(this).find('.fee-input').val()) || 0;
            deposit.Penalty = parseFloat($(this).find('.penalty-input').val()) || 0;
            deposit.Interest = parseFloat($(this).find('.interest-input').val()) || 0;
            deposit.Total = parseFloat($(this).find('.total-span').text()) || 0;
            deposit.AccountType = $(this).find('td:eq(1)').text();
            deposit.isDepositDoneByAccountOwner = $(this).find('.form-check-input').prop('checked');
            deposit.IsChargesInclussive = $(this).find('.check-inclussive').prop('checked');
            deposit.OperationType = $('#OperationType').val();
            deposit.IsSWS = false;
            deposit.CustomerId = $('#customerId').val();
            deposit.MemberName = $('#MemberName').val();
            deposit.LoanApplicationId = $(this).find('.loan-application-id').val();
            deposit.Period = $(this).find('.period').val();
            deposit.ChartOfAccountName = $('#account_number option:selected').text().trim();

            // 🔹 Newly added fields to fix missing inputs
            deposit.SourceType = $("input[name='AddNoneCashMobileMoneyCommand.SourceType']:checked").val(); // Mobile Money Operator (MTN/Orange)
            deposit.TelephoneNumber = $('#TelephoneNumber').val().trim(); // Customer's Telephone Number
            deposit.Note = $('#Note').val().trim(); // Customer's Telephone Number
            deposit.TellerCode = $('#TellerCode').val().trim(); // Teller Code

            // Existing captured values
            deposit.BookingDirection = $("input[name='AddMembersNoneCashOperationCommand.BookingDirection']:checked").val();
            deposit.ChartOfAccountId = $('#account_number').val();

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
    ReportView("MobileMoneyBackOffice", null, "GetReport", null, null, "receipts", "ReportParameterLess");

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
        case 'MobileMoneyNoneCashIn':
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

function PostMobileMoney() {
    var deposits = collectDeposits();

    if (deposits.length === 0) {
        appalert("Please select at least one account to process cash-in.", 3, 1);
        return;
    }

    // Ensure exactly one account is selected
    var selectedRecords = $('#myDataTableT tbody .form-check-input:checked').length;
    if (selectedRecords !== 1) {
        appalert("You must select exactly one account for Mobile Money Cash-In.", 3, 1);
        return;
    }

    // Calculate total amount from the table
    var totalInfo = calculateTotalAmount();
    var selectedTotalAmount = 0;
    var isAmountZero = false; // Flag to check if any selected amount is 0

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            var amount = parseFloat($(this).find('.amount-input').val()) || 0;
            var fee = parseFloat($(this).find('.fee-input').val()) || 0;
            selectedTotalAmount += (amount + fee);

            // If amount is 0, prevent transaction
            if (amount <= 0) {
                isAmountZero = true;
            }
        }
    });

    // Ensure amount is greater than zero
    if (isAmountZero) {
        appalert("Cash-In Amount must be greater than zero.", 3, 1);
        return;
    }

    // Ensure total amount matches input values
    if (totalInfo.total !== selectedTotalAmount) {
        appalert("The total amount does not match the sum of the selected account's amount and fees.", 3, 1);
        return;
    }

    // Validate Mobile Money Source Selection
    var sourceType = $("input[name='AddNoneCashMobileMoneyCommand.SourceType']:checked").val();
    if (!sourceType) {
        appalert("Please select the Mobile Money Operator (MTN or Orange).", 3, 1);
        return;
    }

    // Validate Member Reference
    var memberReference = $('#customerId').val().trim();
    if (!memberReference) {
        appalert("Please enter a valid Member Reference.", 3, 1);
        return;
    }

    // Validate Representative Name
    var customerName = $('#MemberName').val().trim();
    if (!customerName) {
        appalert("Please enter the Representative's Name.", 3, 1);
        return;
    }

    // Validate Telephone Number
    var telephoneNumber = $('#TelephoneNumber').val().trim();
    if (!telephoneNumber.match(/^\d{9}$/)) {
        appalert("Please enter a valid 9-digit Telephone Number.", 3, 1);
        return;
    }

    // Validate Teller Code
    var tellerCode = $('#TellerCode').val().trim();
    if (!tellerCode) {
        appalert("Please enter a valid Teller Code.", 3, 1);
        return;
    }

    var sourceLabel = (sourceType === "MobileMoneyMTN") ? "MTN Mobile Money Float" : "Orange Mobile Money Float";

    var message = "You are about to process a Mobile Money Cash-In for the following details:\n\n";
    message += "Mobile Money Operator: " + sourceLabel + "\n";
    message += "Customer Name: " + customerName + "\n";
    message += "Member Reference: " + memberReference + "\n";
    message += "Telephone Number: " + telephoneNumber + "\n";
    message += "Teller Code: " + tellerCode + "\n";

    // Get selected amount and fee
    var selectedAmount = 0;
    var selectedFee = 0;

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            selectedAmount = parseFloat($(this).find('.amount-input').val()) || 0;
            selectedFee = parseFloat($(this).find('.fee-input').val()) || 0;
        }
    });

    var totalAmount = selectedAmount + selectedFee; // Ensure total amount includes fee

    // Append Amount, Fee, and Total to the message
    message += "Cash-In Amount: " + selectedAmount.toLocaleString() + " XAF\n";
    message += "Fee: " + selectedFee.toLocaleString() + " XAF\n";
    message += "Total Cash-In Amount: " + totalAmount.toLocaleString() + " XAF\n";
    message += "Account Number(s): " + getSelectedAccountNumbers() + "\n\n";
    message += "Please confirm this transaction before proceeding.";

    // Confirm transaction
    confirmTransaction('Confirm ' + sourceLabel + ' Cash-In', message, '/MobileMoneyBackOffice/PostRequestCash', deposits, 'MobileMoneyNoneCashIn');

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
    AddORUpdateGen(KEY, divToLoadData, partialView, path, "MobileMoneyBackOffice");


}
function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "MobileMoneyBackOffice");
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
    
    AddORUpdateGen(Key, divToloadPV, partialView, path, "MobileMoneyBackOffice");
    //calculateBalance();
}





