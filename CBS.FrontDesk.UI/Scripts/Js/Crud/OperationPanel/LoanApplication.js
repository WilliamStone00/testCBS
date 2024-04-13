$(document).ready(function () {

    //$('#myDataTable_customer_loan').DataTable();
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

});

function LoadDropDown(KEY, path,affectedID) {
    GetLoanApplication(KEY);
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
    
}
function LoadProductDetails(KEY) {

    EditResetMain(KEY, '_LoadProductDetails', '_loanproductdetailDiv', 'MemberOperation','InitializeData')

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
            $('#saving').html("Enter balance saving rate between:" + data.MinimumSavingAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumSavingAccountBalanceRateForTheRequestAmount+"%");
            $('#share').html("Enter required share amount between:" + data.MinimumShareAccountBalanceForTheRequestAmount + " and " + data.MaximumShareAccountBalanceForTheRequestAmount + "");
            $('#salary').html("Enter Salary rate between:" + data.MinimumSalaryAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount + "%");
            $('#fee').html("Enter processing fee rate between:" + data.MinimumProcessingFeeRate + "% and " + data.MaximumProcessingFeeRate + "%.");
            $('#inspectionfee').html("Enter inspection fee between:" + data.MinimumInspectionFeeRate + "% and " + data.MaximumInspectionFeeRate + "%.");

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
                    LoadLocalSchedule();                }
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



