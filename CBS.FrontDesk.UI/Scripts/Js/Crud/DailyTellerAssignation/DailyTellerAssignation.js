


function SearchByDates(partialView, divToloadPV) {
    if (!validateDates()) {
        return;
    }

    var branchid = $('#branchInput').val();
    var datefrom = $('#dateFromInput').val();
    var dateto = $('#dateToInput').val();
    LoadDataMain("DailyTellerAssignation", null, divToloadPV, "myDataTable", "InitializeData", branchid, "search", "search", null, datefrom, dateto, null, null, 0, partialView);
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
    DeleteData("DailyTellerAssignation", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}



