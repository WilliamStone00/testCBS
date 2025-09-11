


function LoadProductFeeMapping(key) {
    LoadDataGen('SavingProductFee', 'myDataTable_savingProductFee', '_SavingProductFeeMapperData', 0, 'datalistingview_feeMapping', key, null, 'savingFeeMapping')
}


function AjaxPostSavingProductFeeMapping(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    var key = $("#savingproductid").val();
                    LoadProductFeeMapping(key);
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


