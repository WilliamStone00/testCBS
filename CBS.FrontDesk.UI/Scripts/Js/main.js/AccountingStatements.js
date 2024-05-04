$(document).ready(function () {
    $('#AccountToHide').hide();
    $("#btnData").click(function () {
        LoadData();
    });
    $(document).on('change', '#SystemQuery_BranchId', function () {
        // Get the selected value AccountNumber
        var selectedValue = $(this).val();

        var selectedReportType = $("#SystemQuery_ReportType").val();

        if (selectedReportType == "LL") {
            // Load another dropdown based on the selected value
            loadBranchLiasonAccount(selectedValue);
        }

    });

    $(document).on('change', '#SystemQuery_ReportType', function () {
        var selectedValue = $(this).val();

        // Check if the selected value matches the specific value
        if (selectedValue === 'GL') {
            // Show the element
            $('#AccountToHide').show();
        } else {
            // Hide the element
            $('#AccountToHide').hide();
        }

    });


});

function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("AccountingStatements", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}
function AjaxPostSearch(form) {
    var fileType = $("#SystemQuery_FileType").val();
    var reportType = $("#SystemQuery_ReportType").val();
    alert(fileType + reportType);
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                appalert(response.message, 2, 1);
                if (fileType === "EXCEL") {
                    if (reportType === "TB4") {
                        window.open("/Reports/PrintTrialBalance4Column", "_blank");
                    } else if (reportType === "TB6") {

                        window.open("/Reports/PrintTrialBalance6Column", "_blank");
                    }
                } else {
                    window.open("/Reports/DownloadExcelFile", "_blank");
                }

                // Updated URL



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


function loadBranchLiasonAccount(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingStatements/GetAllLiasionAccount',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#SystemQuery_AccountNumber').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#SystemQuery_AccountNumber').append($('<option>').text(item.Value).attr('value', item.Text));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}