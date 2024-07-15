$(document).ready(function () {

    //$('#myDataTable_customer_loan').DataTable();
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

});

function LoadDropDown(KEY, path, affectedID) {
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

    if (path === "loanrepayment_cycles") {
        GetLoanApplication(KEY);
        var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
        FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
    }
}



function LoadProductDetails(KEY) {

    EditResetMain(KEY, '_LoadProductDetails', '_loanproductdetailDiv', 'MemberOperation', 'InitializeData')

}

function GetLoanApplication(KEY) {
    $.ajax({
        type: "GET",
        url: '/MemberOperation/GetLoanProduct?KEY=' + KEY,
        success: function (data) {
            $('#amount').html("Enter amount from:" + data.LoanMinimumAmount + " to " + data.LoanMaximumAmount);
            $('#loanduration').html("Loan duration is between:" + data.MinimumDurationPeriod + " to " + data.MaximumDurationPeriod + " " + data.LoanDurationPeriod);
            $('#interest').html("Enter interest between:" + data.MinimumInterestRate + "% and " + data.MaximumInterestRate + "%. Calculated on daily bases: " + data.LoanInterestPeriod);
            $('#installment').html("Minimum repayment installment is:" + data.MinimumNumberOfRepayment + " and Maximum is " + data.MaximumNumberOfRepayment);
            $('#saving').html("Enter balance saving rate between:" + data.MinimumSavingAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumSavingAccountBalanceRateForTheRequestAmount + "%");
            $('#share').html("Enter required share amount between:" + data.MinimumShareAccountBalanceForTheRequestAmount + " and " + data.MaximumShareAccountBalanceForTheRequestAmount + "");
            $('#salary').html("Enter Salary rate between:" + data.MinimumSalaryAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount + "%");
            $('#fee').html("Enter processing fee rate between:" + data.MinimumProcessingFeeRate + "% and " + data.MaximumProcessingFeeRate + "%.");
            $('#inspectionfee').html("Enter inspection fee between:" + data.MinimumInspectionFeeRate + "% and " + data.MaximumInspectionFeeRate + "%.");
            $('#chargeparcentages').html("Enter charge percentage between:" + data.MinimumChargesToAppliedInPercentage + " % and " + data.MaximumChargesToAppliedPercentage + "%");
            $('#chargedayranges').html("Enter in days when charges starts between:" + data.MinimumChargesStartDayAfterLoanDueDate + " to " + data.MaximumChargesStartDayAfterLoanDueDate + "days");
            $('#waiverranges').html("Enter in percentage interest to waive between:" + data.MinimumInterestWaiver + "% and " + data.MaximumInterestWaiver + "%.");
            $('#downpaymentrate').html("Does this application require down payment? Minimum rate is [" + data.MinimumDownPaymentPercentage + "%].");

            //InspectionFee
        }, error: function (err) {

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



