$(document).ready(function () {
    // Bind the event to trigger when the radio buttons are clicked
    $("input[name='AddLoanApplicationCommand.LoanCategory']").on('change', function () {

        var loanTermId = $("#LoanTermId").val();
        GetConfiuredTargets(loanTermId,'TargetId');
        GetProductByTargetLoandingMainLoan();  // Call the function when the radio button is checked
    });
});


$(document).ready(function () {

    // Trigger the download on button click
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

    // Initial check and setting up event listener for checkbox state changes
    toggleLoanOverride();

    $('#IsOverRightOldLoanInterestAndBalance').change(function () {
        toggleLoanOverride();
    });

    function toggleLoanOverride() {
        if ($('#IsOverRightOldLoanInterestAndBalance').is(':checked')) {
            $('#loanoveride').show();
        } else {
            $('#loanoveride').hide();
            // Reset the fields if the checkbox is unchecked
            $('#NewBalance').val(0);
            $('#NewInterest').val(0);
            $('#NewVAT').val(0);
            $('#NewPenalty').val(0);
        }
    }

    // Trigger change event on page load to set the correct initial state
    $('#IsOverRightOldLoanInterestAndBalance').trigger('change');

});



function LoanProductsProperties(KEY, path, affectedID) {
    GetLoanApplication(KEY);
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");

    //GetLoanPurposes()
}
function GetProductByTarget(KEY, affectedID) {
    // Fetch the selected radio button based on the 'id' attribute
    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    console.log(loanCategoryValue); // Logs 'MainLoan' or 'SpecialSavingFacilityLoan'

    var path = "load_loan_products";

    // Fetch other necessary parameters
    var loantermid = $("#LoanTermId").val();
    var loanCategoryid = $("#LoanCategoryId").val();

    // Construct the URL with updated isSSF value
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
        "&path=" + path +
        "&loanTermId=" + loantermid +
        "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;
    console.log(url);  // Logs the constructed URL

    // Make the AJAX call to populate the dropdown
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
}
function GetProductByTargetLoandingMainLoan() {
    // Fetch the selected radio button based on the 'id' attribute
    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    console.log(loanCategoryValue); // Logs 'MainLoan' or 'SpecialSavingFacilityLoan'
    var affectedID = "loan_productid";
    var KEY = $("#TargetId").val();
    var path = "load_loan_products";
    // Fetch other necessary parameters
    var loantermid = $("#LoanTermId").val();
    var loanCategoryid = $("#LoanCategoryId").val();
    // Construct the URL with updated isSSF value
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
        "&path=" + path +
        "&loanTermId=" + loantermid +
        "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;
    console.log(url);  // Logs the constructed URL

    // Make the AJAX call to populate the dropdown
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
}
function GetConfiuredTargets(KEY, affectedID) {
    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    console.log(KEY);
    console.log(affectedID);
    path = "get_configurated_target";
    var loanCategoryid = $("#LoanCategoryId").val();
    console.log(loanCategoryid);
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path + "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
}
function GetLoanPurposes() {
    path = "get_puposes";
    var loanCategoryid = $("#LoanCategoryId").val();
    var url = "/MemberOperation/Ajaxloader?Key=" + loanCategoryid + "&path=" + path;
    FillDropDownAjaxCallParam(url, "purposeId", "---Select Option---");
}

function GetLoanPurposes(key) {
    path = "get_puposes";
    var url = "/MemberOperation/Ajaxloader?Key=" + key + "&path=" + path;
    FillDropDownAjaxCallParam(url, "purposeId", "---Select Option---");
}

function LoadRefinancing(KEY, path, affectedID) {
    var loandiv = document.getElementById('loandiv');
    var showLoanDiv = (KEY === "Refinancing" || KEY === "Reschedule" || KEY === "Restructure");
    var dataPath = KEY;

    if (showLoanDiv) {
        KEY = document.getElementById('customerid').value;
        loandiv.style.display = "block";
        dataPath = "Select loan to " + dataPath;
        $('#loanlable').html(dataPath);
        var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
        FillDropDownAjaxCallParam(url, affectedID, dataPath);
    } else {
        loandiv.style.display = "none";
    }

    // Call function to toggle input fields and divs based on application type
    toggleInputFields();
}

function toggleInputFields() {
    var loanApplicationType = document.getElementById('LoanApplicationType').value;
    var isReschedule = loanApplicationType === "Reschedule";

    // List of input fields to toggle
    var inputFields = ["NewBalance", /*"NewInterest", "NewVAT", "NewPenalty"*/];

    // Toggle readonly attribute based on loan application type
    inputFields.forEach(function (fieldId) {
        var field = document.getElementById(fieldId);
        if (isReschedule) {
            field.setAttribute('readonly', 'readonly');
        } else {
            field.removeAttribute('readonly');
        }
    });

    // Update the panel title and convert to uppercase
    var panelTitle = document.getElementById('panelTitle');
    switch (loanApplicationType) {
        case "Reschedule":
            panelTitle.innerHTML = "RESCHEDULELING LOAN APPLICATION FORM".toUpperCase();
            break;
        case "Refinancing":
            panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM".toUpperCase();
            break;
        case "Restructure":
            panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM".toUpperCase();
            break;
        default:
            panelTitle.innerHTML = "NEW LOAN APPLICATION FORM".toUpperCase();
            break;
    }

    // Update the icon based on the loan application type
    var iconElement = document.querySelector('#accordionPopoutIconThree i');
    switch (loanApplicationType) {
        case "Reschedule":
            iconElement.className = "mdi mdi-calendar-refresh me-2";
            break;
        case "Refinancing":
            iconElement.className = "mdi mdi-cash-refund me-2";
            break;
        case "Restructure":
            iconElement.className = "mdi mdi-account-cog me-2";
            break;
        default: // New Loan Application
            iconElement.className = "mdi mdi-file me-2"; // Update this line with the new icon class
            break;
    }

    // List of div IDs to hide/show based on "Reschedule"
    var divsToToggle = [
        "RiskMitigationDiv",
        "RAmountDiv",
        "loanTypeDiv",
        "loanProductDiv",
        "WaiverDiv",
        "TargetPopulationDiv",
        "LoanCategoryDive",
        "RepaymentDiv",
        "IncludeChargeDiv",
        "ApplyInterestWaiverDiv",
        "purposeAndActivitiesDiv"
    ];

    // Hide or show divs based on "Reschedule" status
    divsToToggle.forEach(function (divId) {
        var divElement = document.getElementById(divId);
        if (isReschedule) {
            divElement.style.display = "none";
        } else {
            divElement.style.display = "block";
        }
    });
}


function LoadProductDetails(KEY) {

    EditResetMain(KEY, '_LoadProductDetails', '_loanproductdetailDiv', 'MemberOperation', 'InitializeData')

}

function GetLoanApplication(KEY) {
    $.ajax({
        type: "GET",
        url: '/MemberOperation/GetLoanProduct?KEY=' + KEY,
        success: function (data) {
            // Helper function to format amounts to XAF currency
            function formatCurrency(amount) {
                return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'XAF', minimumFractionDigits: 1 }).format(amount);
            }

            // Update the HTML elements based on the data received
            $('#amount').html("Enter amount from: " + formatCurrency(data.LoanMinimumAmount) + " to " + formatCurrency(data.LoanMaximumAmount));
            $('#loanduration').html("Loan duration is between: " + data.MinimumDurationPeriod + " to " + data.MaximumDurationPeriod + " Months");
            $('#interest').html("Enter interest between: " + data.MinimumInterestRate + "% and " + data.MaximumInterestRate + "%. Calculated on a daily basis.");
            $('#installment').html("Minimum repayment installment is: " + data.MinimumNumberOfRepayment + " and Maximum is " + data.MaximumNumberOfRepayment);
            $('#saving').html("Enter balance saving rate between: " + data.MinimumSavingAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumSavingAccountBalanceRateForTheRequestAmount + "%");
            $('#share').html("Enter required share amount between: " + formatCurrency(data.MinimumShareAccountBalanceForTheRequestAmount) + " and " + formatCurrency(data.MaximumShareAccountBalanceForTheRequestAmount));
            $('#salary').html("Enter Salary rate between: " + data.MinimumSalaryAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount + "%");
            $('#fee').html("Enter processing fee rate between: " + data.MinimumProcessingFeeRate + "% and " + data.MaximumProcessingFeeRate + "%.");
            $('#inspectionfee').html("Enter inspection fee between: " + data.MinimumInspectionFeeRate + "% and " + data.MaximumInspectionFeeRate + "%.");
            $('#chargeparcentages').html("Enter charge percentage between: " + data.MinimumChargesToAppliedInPercentage + " % and " + data.MaximumChargesToAppliedPercentage + "%");
            $('#chargedayranges').html("Enter in days when charges start between: " + data.MinimumChargesStartDayAfterLoanDueDate + " to " + data.MaximumChargesStartDayAfterLoanDueDate + " days");
            $('#waiverranges').html("Enter in percentage interest to waive between: " + data.MinimumInterestWaiver + "% and " + data.MaximumInterestWaiver + "%.");
            $('#downpaymentrate').html("Does this application require down payment? Minimum rate is [" + data.MinimumDownPaymentPercentage + "%].");

            const requiredDownPayment = data.MinimumDownPaymentPercentage > 0;

            // Set the checkbox state for Down Payment
            const $downPaymentCheckbox = $('input[name="AddLoanApplicationCommand.RequiredDownPaymentCoverageRate"]');
            $downPaymentCheckbox.prop('checked', requiredDownPayment);
            $downPaymentCheckbox.prop('disabled', true);

            // Handle IsPaidFeeBeforeProcessing logic
            const $paidFeeCheckbox = $('input[name="AddLoanApplicationCommand.IsPaidFeeBeforeProcessing"]');
            const $processingLabel = $('label[for="FeePaidBeforeProcessing"]');
            const loanProductName = data.ProductName || "this loan product"; // Use the loan product name if available

            if (data.IsPaidFeeBeforeProcessing) {
                $paidFeeCheckbox.prop('checked', true); // Check the checkbox
                $paidFeeCheckbox.prop('disabled', true); // Disable the checkbox
                $processingLabel.html(`A partial fee must be paid at the cash desk before the loan (${loanProductName}) can be processed.`);
                $('#beforeProcessingDiv').show(); // Ensure the div is visible
            } else {
                $paidFeeCheckbox.prop('checked', false); // Uncheck the checkbox
                $paidFeeCheckbox.prop('disabled', false); // Enable the checkbox
                $processingLabel.html(`(${loanProductName}) is not configured for partial fee payment before processing.`);
                //$('#beforeProcessingDiv').hide(); // Hide the div
            }
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}


function GetLoan(loanid) {
    $.ajax({
        type: "GET",
        url: '/MemberOperation/GetLoan?Key=' + loanid,
        success: function (data) {
            // Assuming 'data' is an object containing the loan details
            $('#NewBalance').val(data.Balance);
            $('#NewInterest').val(data.AccrualInterest);
            $('#NewVAT').val(data.Tax);
            $('#NewPenalty').val(data.Penalty);

            // Show the loan div if hidden
            //$('#loandiv').show();
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}

function EditReset(KEY, partialView) {
    EditResetMain(KEY, partialView, "mainview", "Individual", "InitializeData");
}


function AjaxPostLoanScedule(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    LoadLocalSchedule();
                }
                else {
                    appalert(response.message, 2, 1);

                }

            }
            , error: function (err) {
                appalert(err.statusText, 0, 1);
            }
        };

        if ($(form).attr('enctype') === "multipart/form-data") {
            ajaxConfig["contentType"] = false;
            ajaxConfig["processData"] = false;
        }
        $.ajax(ajaxConfig);

    }
    return false;

}



function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}

function LoadLocalSchedule() {
    LoadDataGen('MemberOperation', null, '_LoanSimulationScheduleData', 0, 'amortization_schedule_data_div', "KEY", 'applications', 'loan_schedule')
}



