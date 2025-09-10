document.addEventListener('DOMContentLoaded', function () {
    LoadPendingTransactions();
});

function LoadPendingTransactions() {
    LoadDataTableNew('WithdrawalNotification', 'myDataTable', "InitializeData", null, '_PendingRequestData', 0, "pendinglisting", 'datalistingview');
}
function ValidateRequest(id)
{
    EditResetModal(id, 'modal', 'modalContent', 'WithdrawalNotification', 'InitializeData', '_ValidatePendingRequest', 'get_to_approve', 'PENDING SAVING WITHDRAWAL', 'modalLabel')
}

function AjaxPostAndUpdateWithdrawalFromRequest(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {


        alertify.confirm("Information", "Are you sure you want to approve the selected operation? ",
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        if (response.success) {

                            LoadPendingTransactions();
                        }
                        else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else {
                                appalert(response.message, 2, 1);
                            }

                        }

                    }
                    , error: function (err) {
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig["contentType"] = false;
                    ajaxConfig["processData"] = false;
                }
                console.log(ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );
    }
    return false;


}
