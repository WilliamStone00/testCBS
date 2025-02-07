$(document).ready(function () {
    // Initialize the table on page load with no filters
    //LoadAudits("all");

    // Initialize datepickers (you can use any datepicker plugin like jQuery UI or Bootstrap Datepicker)
    $('#dateFrom, #dateTo').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        todayHighlight: true
    });

    // Trigger search when clicking the search button
    $('#searchButton').on('click', function () {
        let searchValue = $('#auditSearchInput').val();
        LoadAudits(searchValue);
    });
});
function LoadAudits(search) {
    const dateFrom = $('#dateFrom').val();
    const dateTo = $('#dateTo').val();
    const searchOption = $('input[name="searchOption"]:checked').val();

    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
        "searching": false,
        "ajax": {
            "url": "/AuditTrail/LoadData",
            "type": "POST",
            "data": {
                "searchCriteria": search,
                "dateFrom": dateFrom,
                "dateTo": dateTo,
                "searchOption": searchOption
            },
            "dataSrc": function (json) {
                if (json.error) {
                    console.error(json.error);
                    return [];
                }
                // Show the data listing view after data is loaded
                $('#datalistingview').show();
                return json.data;
            }
        },
        "columns": [
            {
                "data": "Id",
                "title": "Time",
                "render": function (id, type, row) {
                    const date = new Date(parseInt(row.Timestamp.replace(/\/Date\((\d+)\)\//, '$1')));
                    const formattedDate = date.toLocaleString('en-GB', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                        second: '2-digit'
                    });

                    return `<a href="/AuditTrail/Details/${id}" target="_blank" class="text-primary">
                                ${formattedDate}
                            </a>`;
                }
            },
            {
                "data": "Level",
                "title": "Level",
                "render": function (data) {
                    let badge = "";
                    switch (data) {
                        case "Information":
                            badge = `<span class="badge bg-info text-dark">
                                        <i class="mdi mdi-information-outline me-1"></i> ${data}
                                     </span>`;
                            break;
                        case "Warning":
                            badge = `<span class="badge bg-warning text-dark">
                                        <i class="mdi mdi-alert-outline me-1"></i> ${data}
                                     </span>`;
                            break;
                        case "Error":
                            badge = `<span class="badge bg-danger text-white">
                                        <i class="mdi mdi-alert-circle-outline me-1"></i> ${data}
                                     </span>`;
                            break;
                        default:
                            badge = `<span class="badge bg-secondary">
                                        <i class="mdi mdi-help-circle-outline me-1"></i> Unknown
                                     </span>`;
                    }
                    return badge;
                }
            },
            { "data": "DetailMessage", "title": "Response" },
            { "data": "StringifyObject", "title": "Requests" }
        ],
        "order": [[0, "desc"]]
    });
}

function downloadAuditTrail() {
    const searchCriteria = $('#auditSearchInput').val();
    const dateFrom = $('#dateFrom').val();
    const dateTo = $('#dateTo').val();
    const searchOption = $('input[name="searchOption"]:checked').val();

    // Build the export URL with search criteria and date range
    const url = `/AuditTrail/Download?searchCriteria=${encodeURIComponent(searchCriteria)}&dateFrom=${encodeURIComponent(dateFrom)}&dateTo=${encodeURIComponent(dateTo)}&searchOption=${encodeURIComponent(searchOption)}`;

    // Trigger download
    window.location.href = url;
}

//function LoadAudits(search) {
//    const dateFrom = $('#dateFrom').val();
//    const dateTo = $('#dateTo').val();

//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "info": true,
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 50, 100], [10, 20, 50, 100]],
//        "searching": false,
//        "ajax": {
//            "url": "/AuditTrail/LoadData",
//            "type": "POST",
//            "data": {
//                "searchCriteria": search,
//                "dateFrom": dateFrom,
//                "dateTo": dateTo
//            },
//            "dataSrc": function (json) {
//                if (json.error) {
//                    console.error(json.error);
//                    return [];
//                }
//                return json.data;
//            }
//        },
//        "columns": [
//            {
//                "data": "Id",
//                "title": "Time",
//                "render": function (id, type, row) {
//                    const date = new Date(parseInt(row.Timestamp.replace(/\/Date\((\d+)\)\//, '$1')));
//                    const formattedDate = date.toLocaleString('en-GB', {
//                        day: '2-digit',
//                        month: '2-digit',
//                        year: 'numeric',
//                        hour: '2-digit',
//                        minute: '2-digit',
//                        second: '2-digit'
//                    });

//                    return `<a href="/AuditTrail/Details/${id}" target="_blank" class="text-primary">
//                                ${formattedDate}
//                            </a>`;
//                }
//            },
//            {
//                "data": "Level",
//                "title": "Level",
//                "render": function (data) {
//                    let badge = "";
//                    switch (data) {
//                        case "Information":
//                            badge = `<span class="badge bg-info text-dark">
//                                        <i class="mdi mdi-information-outline me-1"></i> ${data}
//                                     </span>`;
//                            break;
//                        case "Warning":
//                            badge = `<span class="badge bg-warning text-dark">
//                                        <i class="mdi mdi-alert-outline me-1"></i> ${data}
//                                     </span>`;
//                            break;
//                        case "Error":
//                            badge = `<span class="badge bg-danger text-white">
//                                        <i class="mdi mdi-alert-circle-outline me-1"></i> ${data}
//                                     </span>`;
//                            break;
//                        default:
//                            badge = `<span class="badge bg-secondary">
//                                        <i class="mdi mdi-help-circle-outline me-1"></i> Unknown
//                                     </span>`;
//                    }
//                    return badge;
//                }
//            },
//            { "data": "DetailMessage", "title": "Response" },
//            { "data": "StringifyObject", "title": "Requests" }
//        ],
//        "order": [[0, "desc"]]
//    });
//}


//function downloadAuditTrail() {
//    const searchCriteria = $('#auditSearchInput').val();
//    const dateFrom = $('#dateFrom').val();
//    const dateTo = $('#dateTo').val();

//    // Build the export URL with search criteria and date range
//    const url = `/AuditTrail/Download?searchCriteria=${encodeURIComponent(searchCriteria)}&dateFrom=${encodeURIComponent(dateFrom)}&dateTo=${encodeURIComponent(dateTo)}`;

//    // Trigger download
//    window.location.href = url;
//}


// Function to view details
function viewDetails(id) {
    $.ajax({
        url: `/AuditTrail/Details/${id}`,
        type: "GET",
        success: function (response) {
            $('#detailsModalBody').html(response);
            $('#detailsModal').modal('show');
        },
        error: function () {
            alert("Error fetching details.");
        }
    });
}

// Function to delete an audit log entry
function deleteAudit(id) {
    if (confirm("Are you sure you want to delete this audit log?")) {
        $.ajax({
            url: `/AuditTrail/Delete/${id}`,
            type: "POST",
            success: function () {
                alert("Audit log deleted successfully.");
                $("#auditTrailTable").DataTable().ajax.reload();
            },
            error: function () {
                alert("Error deleting audit log.");
            }
        });
    }
}
