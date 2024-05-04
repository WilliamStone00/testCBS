
function AjaxPost(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    LoadCollateral();                }
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



function showConfirmMessage(KEY, tableID) {
    DeleteDataConfiguration("Collateral", KEY, tableID, "_Data", 0, "datalistingview");

}

function LoadCollateral() {
    LoadDataGen('Collateral', 'myDataTable', '_Data', 0, 'datalistingview', "KEY",null,'list')
}



