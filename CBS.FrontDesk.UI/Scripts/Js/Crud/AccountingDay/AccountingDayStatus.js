function AjaxPostAndUpdatePrintTillStatus(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);

    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        alertify.confirm("Confirmation", "Are you sure you want to perform this action?",
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: formData,
                    success: function (response) {
                        console.log("Response:", response);
                        if (response.success) {
                            appalert(response.message, 1, 1);
                            var url = "/Reports/ReportWithParameter"
                            window.open(url, "_blank");
                        } else {
                            appalert(response.message, 2, 1);
                        }
                    },
                    error: function (err) {
                        console.log("Error:", err);
                        if (err.status === 401) { // Unauthorized
                            window.location.href = '/Authentication/Login';
                        } else {
                            appalert(err.statusText, 0, 1);
                        }
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig.contentType = false;
                    ajaxConfig.processData = false;
                }

                console.log("AJAX Config:", ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);
            }
        );
    }
    return false;
}

function loadAccountingDayDetails(accountingDayId) {
    $.ajax({
        url: '/AccountingDay/GetAccountingDayDetails',
        type: 'GET',
        data: { id: accountingDayId },
        success: function (result) {
            $('#modalContent').html(result);
            $('#accountingDayModal').modal('show');
        },
        error: function (xhr, status, error) {
            $('#modalContent').html('<p class="text-danger">An error occurred while loading the details. Please try again later.</p>');
            $('#accountingDayModal').modal('show');
        }
    });
}

function toggleBranchDiv(enable) {
    var branchDiv = document.getElementById("branchDiv");
    var branchInput = document.getElementById("branchInput");

    if (enable) {
        branchDiv.style.display = "block";
        branchInput.disabled = false;
    } else {
        branchDiv.style.display = "none";
        branchInput.disabled = true;
    }
}

// Set initial state on page load
window.onload = function () {
    document.getElementById("byBranchRadio").checked = true;
    toggleBranchDiv(true);
};


function viewInDataTable() {
    // Perform client-side validation
    var isValid = true;
    var errors = [];

    // Example validation checks
    if (!$('input[name="QueryParameter"]:checked').val()) {
        isValid = false;
        errors.push('Query Parameter is required.');
    }
    if (!$('#dateFromInput').val()) {
        isValid = false;
        errors.push('Date From is required.');
    }
    if (!$('#dateToInput').val()) {
        isValid = false;
        errors.push('Date To is required.');
    }

    if (!isValid) {
        // Display validation errors
        appalert(errors.join('\n'), 2, 1);
        return;
    }

    var queryParameter = $('input[name="QueryParameter"]:checked').val();
    var branchId = $('#branchInput').val();
    var dateFrom = $('#dateFromInput').val();
    var dateTo = $('#dateToInput').val();

    // Update fieldset legend with query information
    var legendText = `Accounting Days Histories : Query Parameter: ${queryParameter}, Date Between: ${dateFrom} & ${dateTo}`;
    $('#select_base').text(legendText);

    var formData = {
        QueryParameter: queryParameter,
        BranchId: branchId,
        DateFrom: dateFrom,
        DateTo: dateTo
    };

    $.ajax({
        url: '/AccountingDay/GetAccountingDays',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success === false && response.status === 'ValidationError') {
                // Display validation errors
                appalert("Validation Errors: " + response.message, 2, 1);
                $('#dataTableContainer').hide();
                $('#dataNotFound').show();
                $('#queryFieldset').show();
            } else if (response.data && response.data.length > 0) {
                // Data is available, initialize DataTable
                $('#myDataTable').DataTable({
                    data: response.data,
                    destroy: true,
                    autoWidth: false,  // Disable auto width to respect the specified widths
                    columns: [
                        { data: 'BranchName', title: 'Branch Name', width: '46%' }, 
                        { data: 'StrDate', title: 'Open Day', width: '15%' },       
                        { data: 'StrOpenedAt', title: 'Time', width: '23%' },     
                        {
                            data: 'IsClosed',
                            title: 'Status',
                            width: '8%',                                          
                            render: function (data) {
                                return data ? '<span class="badge bg-orange">Closed</span>' : '<span class="badge bg-green">Open</span>';
                            }
                        },
                        {
                            data: 'Id',
                            title: 'Action',
                            width: '8%',                                          
                            render: function (data) {
                                return `<a href="#" onclick="loadAccountingDayDetails('${data}')" class="btn btn-info btn-sm" data-toggle="tooltip" data-placement="top" title="View details">Details</a>`;
                            }
                        }
                    ]
                });

                // Show DataTable and hide "data not found" message
                $('#dataTableContainer').show();
                $('#dataNotFound').hide();
                $('#queryFieldset').show();
            } else {
                // No data found, hide DataTable and show "data not found" message
                $('#dataTableContainer').hide();
                $('#dataNotFound').show();
                $('#queryFieldset').show();
            }
        },
        error: function (xhr, status, error) {
            appalert("An error occurred: " + error, 2, 1);
        }
    });
}

function closeAccountingDay(id) {

    alertify.confirm("Accounting Day Closing Confirmation", "Are you sure you want to close this accounting day?",
        function () {
            $.ajax({
                url: '/AccountingDay/CloseAccountingDay',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    appalert(result.message, 1, 1);
                    $('#accountingDayModal').modal('hide');
                    loadAccountingDayDetails(id);
                    viewInDataTable()
                },
                error: function (xhr, status, error) {
                    alert("An error occurred: " + error);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );

    
}


//alertify.confirm("Confirmation", "Are you sure you want to close this accounting day?",
//    function () {

//    },
//    function () {
//        appalert('Transaction cancelled', 3, 1);
//    }
//);



function reopenAccountingDay(id) {
    alertify.confirm("Accounting Day Re-Open Confirmation", "Are you sure you want to reopen this accounting day?",
        function () {
            $.ajax({
                url: '/AccountingDay/ReopenAccountingDay',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    appalert(result.message, 1, 1);
                    $('#accountingDayModal').modal('hide');
                    loadAccountingDayDetails(id);
                    viewInDataTable()
                    
                },
                error: function (xhr, status, error) {
                    alert("An error occurred: " + error);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
   
}

function removeAccountingDay(id) {
    alertify.confirm("Accounting Day Delete Confimation", "Are you sure you want to remove this accounting day?",
        function () {
            $.ajax({
                url: '/AccountingDay/RemoveAccountingDay',
                type: 'POST',
                data: { id: id },
                success: function (result) {
                    appalert(result.message, 1, 1);
                    $('#accountingDayModal').modal('hide');
                    viewInDataTable();
                },
                error: function (xhr, status, error) {
                    alert("An error occurred: " + error);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

