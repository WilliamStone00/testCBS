$(document).ready(function () {
    // Bind the event to trigger when the radio buttons are clicked
    $("input[name='AddLoanApplicationCommand.LoanCategory']").on('change', function () {

        var loanTermId = $("#LoanTermId").val();
        GetConfiuredTargets(loanTermId,'TargetId');
        GetProductByTargetLoandingMainLoan();  // Call the function when the radio button is checked
    });
});


$(document).ready(function () {
    $('#loan_identification, #loan_configuration, #loan_financials, #loan_dates')
        .addClass('show')
        .prev('.accordion-header')
        .find('.accordion-button')
        .removeClass('collapsed');

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
function LoanProductsPropertiesRefinancing(KEY, path, affectedID) {
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
function handleLoanSelection(loanId) {
    if (!loanId || loanId.trim() === "") {
        $('#viewLoanDetailsBtn').prop('disabled', true);
        $('#loanDetailsCard').collapse('hide');
        return;
    }

    $('#viewLoanDetailsBtn').prop('disabled', false);
}

function toggleLoanDetailsCard() {
    $('#loanDetailsCard').collapse('toggle');
}




function populateLoanModal(data) {
    // 🧩 Identification
    $('#rl_productId').text(data.LoanProductId || '');
    $('#rl_productName').text(data.LoanProductName || '');
    $('#rl_loanType').text(data.LoanType || '');
    $('#rl_loanStatus').text(data.LoanStatus || '');
    $('#rl_loanCategory').text(data.LoanCategory || '');

    // ⚙️ Configuration
    $('#rl_loanTerm').text(data.LoanTermName || '');
    $('#rl_targetPopulation').text(data.LoanTarget || '');
    $('#rl_loanPurpose').text(data.LoanPurpose || '');
    $('#rl_repaymentPeriod').text(data.RepaymentPeriod || '');
    $('#rl_repaymentMode').text(data.RepaymentMode || '');
    $('#rl_installments').text(data.NumberOfInstallments || '');
    $('#rl_interestCalculationMethod').text(data.InterestCalculationMethod || '');

    // 💰 Financials
    $('#rl_interestRate').text((data.InterestRate || 0).toFixed(2) + '%');
    $('#rl_vatRate').text((data.VatRate || 0).toFixed(2) + '%');
    $('#rl_principal').text(formatXAF(data.Principal));
    $('#rl_interest').text(formatXAF(data.AccrualInterest));
    $('#rl_vat').text(formatXAF(data.Tax));
    $('#rl_penalty').text(formatXAF(data.Penalty));
    $('#rl_dueAmount').text(formatXAF(data.DueAmount));

    // 📅 Dates
    $('#rl_disbursementDate').text(formatDate(data.DisbursementDate));
    $('#rl_maturityDate').text(formatDate(data.MaturityDate));
    $('#rl_lastRepaymentDate').text(formatDate(data.LastRepaymentDate));
    $('#rl_disbursementChannel').text(data.DisbursementChannel || '');

    $('#rl_productCategoryId').text(data.ProductCategoryId || '');
    $('#rl_productCategoryName').text(data.ProductCategoryName || '');

    $('#oldLoanAmount').val(data.DueAmount);
    $('#oldLoanCapital').val(data.Principal);
    $('#oldLoanInterest').val(data.AccrualInterest);
    $('#oldLoanVAT').val(data.Tax);
    $('#oldLoanPenalty').val(data.Penalty);
    $('#oldLoanLoanId').val(data.Id);
    $('#oldLoanvatRate').val(data.VatRate);

    // 🎨 Dynamic Styling Based on Loan Status
    const wrapper = $('#loanDetailsCardWrapper');
    const header = $('#loanDetailsHeader');
    const label = $('#loanStatusLabel');

    wrapper.removeClass('border-success border-danger border-warning');
    header.removeClass('bg-success-subtle bg-danger-subtle bg-warning-subtle text-success text-danger text-warning');

    if (!data.LoanStatus) return;

    const status = data.LoanStatus.toLowerCase();
    if (status.includes('delinquent') || status.includes('default')) {
        wrapper.addClass('border-danger');
        header.addClass('bg-danger-subtle text-danger');
        label.text("🚨 Delinquent Loan Summary");
    } else if (status.includes('pending')) {
        wrapper.addClass('border-warning');
        header.addClass('bg-warning-subtle text-warning');
        label.text("⚠️ Pending Loan Summary");
    } else {
        wrapper.addClass('border-success');
        header.addClass('bg-success-subtle text-success');
        label.text("✅ Active Loan Summary");
    }
}
function styleLoanStatus(status) {
    const $badge = $('#rl_loanStatus');
    $badge.text(status).removeClass().addClass('badge px-3 py-1');

    switch ((status || '').toLowerCase()) {
        case 'approved':
            $badge.addClass('bg-success');
            break;
        case 'pending':
            $badge.addClass('bg-warning text-dark');
            break;
        case 'rejected':
        case 'delinquent':
            $badge.addClass('bg-danger');
            break;
        case 'open':
            $badge.addClass('bg-primary text-white'); // 💡 Or use a custom class like 'bg-open-green'
            break;
        default:
            $badge.addClass('bg-secondary');
    }
}



function formatXAF(amount) {
    return (amount || 0).toLocaleString('en-US', {
        style: 'currency',
        currency: 'XAF',
        minimumFractionDigits: 0
    });
}

function formatDate(dateValue) {
    if (!dateValue) return '';

    const date = new Date(dateValue);
    if (isNaN(date)) return '';

    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
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

        // Dynamically update the header with dataPath
        dataPath = "Select Loan To " + dataPath;
        $('#repaymentHeader').html(dataPath);

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
    var isRefinancing = loanApplicationType === "Refinancing";

    // Toggle readonly on specific inputs for Reschedule only
    var inputFields = ["NewBalance"];
    inputFields.forEach(function (fieldId) {
        var field = document.getElementById(fieldId);
        if (isReschedule) {
            field.setAttribute('readonly', 'readonly');
        } else {
            field.removeAttribute('readonly');
        }
    });

    // Update panel title
    var panelTitle = document.getElementById('panelTitle');
    switch (loanApplicationType) {
        case "Reschedule":
            panelTitle.innerHTML = "RESCHEDULELING LOAN APPLICATION FORM";
            break;
        case "Refinancing":
            panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM";
            break;
        case "Restructure":
            panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM";
            break;
        default:
            panelTitle.innerHTML = "NEW LOAN APPLICATION FORM";
            break;
    }

    // Update icon
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
        default:
            iconElement.className = "mdi mdi-file me-2";
            break;
    }

    // Common divs to toggle (already present)
    var divsToToggle = [
        "RiskMitigationDiv",
        "RAmountDiv",
        "loanTypeDiv",
        "loanProductDiv",
        "TargetPopulationDiv",
        "LoanCategoryDive",
        "RepaymentDiv",
        "purposeAndActivitiesDiv"
    ];

    divsToToggle.forEach(function (divId) {
        var divElement = document.getElementById(divId);
        if (isReschedule) {
            divElement.style.display = "none";
        } else {
            divElement.style.display = "block";
        }
    });

    // ✅ Additional logic for Refinancing: hide product selection-related fields
    var refinancingFields = [
        "LoanCategorySelect",       // dropdown for Loan Product Category
        "LoanTermSelect",           // dropdown for Loan Term
        "LoanCategoryDive",     // radio buttons
        "TargetPopulationDiv",  // dropdown for target
        "loanProductDiv",       // dropdown for loan product
        "loanTypeDiv"           // dropdown for loan type
    ];

    refinancingFields.forEach(function (id) {
        var element = document.getElementById(id);
        if (element) {
            element.style.display = isRefinancing ? "none" : "block";
        }
    });
}

//function toggleInputFields() {
//    var loanApplicationType = document.getElementById('LoanApplicationType').value;
//    var isReschedule = loanApplicationType === "Reschedule";

//    // List of input fields to toggle
//    var inputFields = ["NewBalance", /*"NewInterest", "NewVAT", "NewPenalty"*/];

//    // Toggle readonly attribute based on loan application type
//    inputFields.forEach(function (fieldId) {
//        var field = document.getElementById(fieldId);
//        if (isReschedule) {
//            field.setAttribute('readonly', 'readonly');
//        } else {
//            field.removeAttribute('readonly');
//        }
//    });

//    // Update the panel title and convert to uppercase
//    var panelTitle = document.getElementById('panelTitle');
//    switch (loanApplicationType) {
//        case "Reschedule":
//            panelTitle.innerHTML = "RESCHEDULELING LOAN APPLICATION FORM".toUpperCase();
//            break;
//        case "Refinancing":
//            panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM".toUpperCase();
//            break;
//        case "Restructure":
//            panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM".toUpperCase();
//            break;
//        default:
//            panelTitle.innerHTML = "NEW LOAN APPLICATION FORM".toUpperCase();
//            break;
//    }

//    // Update the icon based on the loan application type
//    var iconElement = document.querySelector('#accordionPopoutIconThree i');
//    switch (loanApplicationType) {
//        case "Reschedule":
//            iconElement.className = "mdi mdi-calendar-refresh me-2";
//            break;
//        case "Refinancing":
//            iconElement.className = "mdi mdi-cash-refund me-2";
//            break;
//        case "Restructure":
//            iconElement.className = "mdi mdi-account-cog me-2";
//            break;
//        default: // New Loan Application
//            iconElement.className = "mdi mdi-file me-2"; // Update this line with the new icon class
//            break;
//    }

//    // List of div IDs to hide/show based on "Reschedule"
//    var divsToToggle = [
//        "RiskMitigationDiv",
//        "RAmountDiv",
//        "loanTypeDiv",
//        "loanProductDiv",
//    /*    "WaiverDiv",*/
//        "TargetPopulationDiv",
//        "LoanCategoryDive",
//        "RepaymentDiv",
//        //"ApplyInterestWaiverDiv",
//        "purposeAndActivitiesDiv"
//    ];

//    // Hide or show divs based on "Reschedule" status
//    divsToToggle.forEach(function (divId) {
//        var divElement = document.getElementById(divId);
//        console.log(divId);
//        console.log(divElement);
//        if (isReschedule) {
//            divElement.style.display = "none";
//        } else {
//            divElement.style.display = "block";
//        }
//    });
//}


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

function loadRefinancingLoan(loanId) {
    $.get(`/MemberOperation/GetLoanForRefinancing?Key=${loanId}`, function (response) {
        if (response) {
            populateLoanModal(response);
            $('#loanDetailsCard').collapse('show');

            // ✅ Only call this after successful response
            GetLoanPurposes(response.ProductCategoryId);
            GetLoanApplication(response.LoanProductId)
            LoanProductsPropertiesRefinancing(response.LoanProductId, 'loanrepayment_cycles', 'RepaymentCircle')
        } else {
            appalert("No loan data found.", 2, 1);
        }
    }).fail(function (xhr) {
        appalert("Failed to load loan data: " + xhr.statusText, 0, 1);
    });
}

//function GetLoan(loanid) {
//    if (!loanid) {
//        console.error("Loan ID is required.");
//        return;
//    }

//    $.ajax({
//        type: "GET",
//        url: `/MemberOperation/GetLoanForRefinancing?Key=${loanid}`,
//        success: function (response) {
//            // Adjust to match the actual response format
//            if (response.success === false) {
//                console.error(response.message);
//                alert(response.message);
//                return;
//            }

//            const data = response.data || response; // Use raw data if no 'data' property exists

//            // Populate form fields with the loan data
//            $('#oldLoanAmount').val(data.DueAmount);
//            $('#oldLoanCapital').val(data.Principal);
//            $('#oldLoanInterest').val(data.AccrualInterest);
//            $('#oldLoanVAT').val(data.Tax);
//            $('#oldLoanPenalty').val(data.Penalty);
//            $('#oldLoanLoanId').val(data.Id);
//            $('#oldLoanvatRate').val(data.VatRate);
//        },
//        error: function (xhr, status, error) {
//            console.error("Error fetching loan data:", error, "Response:", xhr.responseText);
//            appalert("An error occurred while fetching loan details. Please try again." + xhr.responseText + " Error: " + error + ". Status: " + status , 0, 1);
//        }
//    });
//}

function calculateVATAndTotal() {
    // Get the values from the input fields
    const oldLoanCapital = parseFloat($('#oldLoanCapital').val()) || 0;
    const oldLoanInterest = parseFloat($('#oldLoanInterest').val()) || 0;
    const oldLoanPenalty = parseFloat($('#oldLoanPenalty').val()) || 0;
    const oldLoanvatRate = parseFloat($('#oldLoanvatRate').val()) || 0;
    
    // Calculate VAT (assume VAT rate is 15% for this example)
    const oldLoanVAT = oldLoanInterest * oldLoanvatRate;

    // Update the VAT field
    $('#oldLoanVAT').val(oldLoanVAT.toFixed(2));

    // Calculate the total loan amount
    const totalAmount = oldLoanCapital + oldLoanInterest + oldLoanPenalty + oldLoanVAT;

    // Update the total amount field
    $('#oldLoanAmount').val(totalAmount.toFixed(2));
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



