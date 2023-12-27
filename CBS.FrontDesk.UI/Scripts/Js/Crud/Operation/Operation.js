$(document).ready(function () {
    $("#btnData").click(function () {
        LoadData();
    });
});

function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable,order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function GetObject(KEY, divToLoadData, partialView, path) {
    EditResetMain(KEY, partialView, divToLoadData, "Operation", "InitializeData", null, null, path);

}
    function PostData(form) {
        var partialView = $("#partialView").val();
        var accountNumber = $("#accountNumber").val();
        var divID = $("#divID").val();
        var controller = $("#controller").val();
        var actionMethod = $("#actionMethod").val();
        $.validator.unobtrusive.parse(form);
        if ($(form).valid()) {
            var ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {

                    if (response.success) {
                        appalert(response.message, 1, 1);
                        //PageReload();
                        EditResetMain(accountNumber, partialView, divID, controller, actionMethod);
                    }
                    else
                        if (response.message === undefined) {
                            alert("Your session is expired.")
                        }
                        else {
                            appalert(response.message, 3, 1);
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






