
function AjaxPostCollateral(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    LoadCollaterals();                }
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
    DeleteDataConfiguration("MemberOperation", KEY, tableID, "_AddLoanCollateralData", 0, "datalistingview_collateral_data");

}


function LoadCollaterals() {
    var KEY = $("#loanapplicationid").val();

    LoadDataGen('MemberOperation', 'myDataTable_collateral', '_AddLoanCollateralData', 0, 'datalistingview_collateral_data', KEY, 'applications','get_collateral_listing')
}





