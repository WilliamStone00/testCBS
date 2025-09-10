$(document).ready(function () {
    $(document).ready(function () {
        $('#myDataTableT').DataTable();
    });
    $("#btnData").click(function () {
        LoadData();
    });
    var depositerDiv = document.getElementById("depositer");
    depositerDiv.style.display = "none";
});
function toggleDepositer(isChecked) {
    var depositerDiv = document.getElementById("depositer");
    if (isChecked) {
        // Checkbox is checked, hide the depositer div
        depositerDiv.style.display = "none";
    } else {
        // Checkbox is unchecked, show the depositer div
        depositerDiv.style.display = "block";
    }
}
function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function GetObject(KEY, divToLoadData, partialView, path) {
    EditResetMain(KEY, partialView, divToLoadData, "Operation", "InitializeData", null, null, path);
    calculateBalance();
}
function PostData(form) {
    var partialView = $("#partialView").val();
    var amount = document.getElementById("lblDepositRequest_amount").innerText;
    //var amount = parseFloat(amountText);

    var operation = $("#OperationType").val();
    var accountNumber = $("#accountnumber").val();
    var divID = $("#divID").val();
    var controller = $("#controller").val();
    var actionMethod = $("#actionMethod").val();
    var path = $("#path").val();
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {

        alertify.confirm("WARNING!!!", "Are you sure you want to perform a " + operation + " of " + amount + " to Account Number " + accountNumber + ", Account Name: " + $("#accountname").val() + " ?",
            function () {

                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {

                        if (response.success) {
                            appalert(response.message, 1, 1);
                            location.reload();
                            ReportView("Operation", null, "GetReport", null, null, "receipts", "ReportParameterLess");
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
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );

    }
    return false;

}


function GetLoan(KEY) {
    $.ajax({
        type: "GET",
        url: '/Operation/GetLoan?KEY=' + KEY,
        success: function (data) {
            var balance = parseFloat(data.Balance).toFixed(1); // Format Balance with 1 decimal place
            var paid = parseFloat(data.Paid).toFixed(1); // Format Paid with 1 decimal place

            // Format numbers with commas as thousands separators
            balance = parseFloat(balance).toLocaleString('en-US');
            paid = parseFloat(paid).toLocaleString('en-US');

            // Assuming #balance and #paid are HTML input elements
            $('#balance').val(balance);
            $('#paid').val(paid);
            $('#loanid').val(data.Id);
            //$('#balance').val(data.Balance);
            //$('#paid').val(data.Paid);
        }, error: function (err) {
            appalert(err.statusText, 3, 0);
        }
    });
}








function AjaxPostAndUpdate(form) {


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
                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            }
                            else {
                                appalert(response.message, 1, 1);

                            }
                            if (response.option === 'Update' && response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                            else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                                EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                            }
                            else if (response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
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
