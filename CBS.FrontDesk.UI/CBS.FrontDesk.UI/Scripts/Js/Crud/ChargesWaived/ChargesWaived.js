
function LoadPendingTransactions() {
    LoadDataTableNew('ChargesWaived', 'myDataTable', "InitializeData", null, '_PendingRequestData', 0, "pending", 'datalistingview');
}




function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "ChargesWaived");
}
function GetMemberData(Key, partialView, divToloadPV, path) {
    // Modified AddORUpdateGen to accept a callback function
    AddORUpdateGen(Key, divToloadPV, partialView, path, "ChargesWaived", function () {
        LoadRequest();
    });
}

function LoadRequest() {
    var id = $("#customerid").val()
    LoadDataTableNew('ChargesWaived', 'myDataTable', "InitializeData", id, '_MemberRequestData', 0, "customerrequest", 'dataloader');
}








function AjaxPostAndUpdateWithdrawalFromRequest(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {


        alertify.confirm("WARNING!!!", "Are you sure you want to perform this action! ",
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        if (response.success) {
                           
                            LoadRequest();
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
