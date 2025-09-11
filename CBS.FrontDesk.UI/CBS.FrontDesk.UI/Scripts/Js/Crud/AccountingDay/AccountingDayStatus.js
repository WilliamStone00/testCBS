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
        dataType: 'html',        // <-- make it explicit
        cache: false,            // <-- avoid stale/empty caches
        success: function (html) {
            const content = (html || '').trim();
            $('#modalContent').html(content || "<div class='text-danger'>No details returned.</div>");
            setTimeout(() => {
                const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('accountingDayModal'));
                modal.show();
            }, 0);
        },

        error: function () {
            $('#modalContent').html("<div class='text-danger'>Failed to load details.</div>");
            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('accountingDayModal'));
            modal.show();
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
    // Validation
    let isValid = true;
    let errors = [];

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
        appalert(errors.join('\n'), 2, 1);
        return;
    }

    let queryParameter = $('input[name="QueryParameter"]:checked').val();
    let branchId = $('#branchInput').val();
    let dateFrom = $('#dateFromInput').val();
    let dateTo = $('#dateToInput').val();

    // Update legend
    $('#select_base').text(
        `Accounting Days Histories : Query Parameter: ${queryParameter}, Date Between: ${dateFrom} & ${dateTo}`
    );

    let formData = {
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
                appalert("Validation Errors: " + response.message, 2, 1);
                $('#dataTableContainer').hide();
                $('#dataNotFound').show();
                $('#queryFieldset').show();
                return;
            }

            if (response.data && response.data.length > 0) {

                $('#myDataTable').DataTable({
                    data: response.data,
                    destroy: true,
                    autoWidth: false,
                    columns: [
                        { data: 'BranchName', title: 'Branch Name', width: '42%' },
                        { data: 'StrDate', title: 'Open Day', width: '15%' },
                        { data: 'StrOpenedAt', title: 'Time', width: '23%' },
                        {
                            data: 'IsClosed',
                            title: 'Status',
                            width: '8%',
                            render: function (val) {
                                return val
                                    ? '<span class="badge bg-orange">Closed</span>'
                                    : '<span class="badge bg-green">Open</span>';
                            }
                        },
                        {
                            data: null,
                            title: 'Action',
                            width: '12%',
                            orderable: false,
                            render: function (data, type, row) {
                                const detailsBtn =
                                    `<button type="button" class="btn btn-info btn-sm me-1"
                   title="View details"
                   onclick="loadAccountingDayDetails('${row.Id}')">
             Details
           </button>`;

                                // Show CLOSE only when the day is OPEN
                                const maybeCloseBtn = row.IsClosed
                                    ? '' // closed => no close button
                                    : `<button type="button" class="btn btn-warning btn-sm"
                     title="Close accounting day"
                     onclick="closeAccountingDay('${row.Id}')">
               Close
             </button>`;

                                return detailsBtn + maybeCloseBtn;
                            }
                        }
                    ]
                });


                $('#dataTableContainer').show();
                $('#dataNotFound').hide();
            } else {
                $('#dataTableContainer').hide();
                $('#dataNotFound').show();
            }

            $('#queryFieldset').show();
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
                    //$('#accountingDayModal').modal('hide');
                    loadAccountingDayDetails(id);
                    //viewInDataTable()
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

