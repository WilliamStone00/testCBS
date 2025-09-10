
function GetFee(KEY) {
    $.ajax({
        type: "GET",
        url: '/FeeRange/GetFee?KEY=' + KEY,
        success: function (data) {
            // Check if the FeeBase is Percentage or Range and show/hide the corresponding divs
            console.log(data.FeeBase);
            if (data.FeeBase === "Percentage") {
                $('#percentage').show();
                $('#range').hide();
            } else {
                $('#percentage').hide();
                $('#range').show();
            }
            $('#select_base').html("Configuration option: " + data.FeeBase);
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



