


function LoadTellerrsAssigned() {
    var branchid = $('#branchInput').val();
    LoadDataMain("DailyTellerAssignation", null, "datalistingview", "myDataTable", "InitializeData", branchid, "search", "search", null, null, null, null, null, 0, "_Data");
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

function DeleteAssignedTeller(controller, KEY, tableID, partialView, order, divToLoadTheData) {

    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        LoadDataTableNew(controller, tableID, "InitializeData", $("#branchInput").val(), partialView, order,"search" , divToLoadTheData)
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }

                }, error: function (err) {

                    appalert(err.statusText, 3, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);

        });


}


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("DailyTellerAssignation", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}



