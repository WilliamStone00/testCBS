$(document).ready(function () {
    initializeCheckboxHandlers();
    initializeResetButton();
    loadSMSData();
});

function initializeCheckboxHandlers() {
    const checkboxMapping = {
        '#byBranch': '#branchFilterSection',
        '#bySender': '#senderFilterSection',
        '#byDate': '#dateRangeSection'
    };

    $.each(checkboxMapping, function (checkbox, section) {
        $(checkbox).on('change', function () {
            $(section).toggle(this.checked);
        });
    });
}

function initializeResetButton() {
    $('#resetFilterBtn').on('click', function () {
        const resetFields = [
            '#operationType', '#branchInput', '#status',
            '#senderId', '#msisdn', '#smsDateFrom', '#smsDateTo'
        ];

        resetFields.forEach(selector => $(selector).val('').trigger('change'));

        $('#byBranch, #bySender, #byDate').prop('checked', false);
        $('#branchFilterSection, #senderFilterSection, #dateRangeSection').hide();

        loadSMSData();
    });

    $('#applyFilterBtn').on('click', function () {
        loadSMSData();
    });

    
}
function loadSMSData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        ajax: {
            url: '/SMSActivityLog/LoadSMSActivityLogData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const requestData = JSON.stringify(prepareSMSDataParams(d));

                console.log("Request Data:", requestData); // ✅ Log request data for debugging

                return requestData;
            },
            error: function (xhr, status, error) {
                console.log("AJAX Error: ", xhr.responseText); // ✅ Log the error response
                console.log("Status: ", status);
                console.log("Error: ", error);

                $('#myDataTable').html(`
                    <div class="alert alert-danger">
                        <strong>Error:</strong> Failed to load data. Please try again later.
                    </div>
                `);
            }
        },
        columns: generateColumns()
    });
}

function prepareSMSDataParams(d) {
    const filters = collectSMSExportParams();

    filters.options = {
        draw: d.draw,
        start: d.start,
        length: d.length,
        skip: d.start,
        pageSize: d.length,
        searchValue: '',
        sortColumnName: d.columns[d.order[0].column].data,
        sortColumnDirection: d.order[0].dir
    };

    console.log("Prepared Filters:", filters); // ✅ Log the prepared filter object

    return filters;
}

function collectSMSExportParams() {
    return {
        OperationType: $('#operationType').val(),
        BranchId: $('#branchInput').val(),
        MemberName: $('#memberName').val(),
        MemberReference: $('#memberReference').val(),
        Msisdn: $('#msisdn').val(),
        From: $('#smsDateFrom').val(),
        To: $('#smsDateTo').val(),
        SendBy: $('#senderId').val(),
        Status: $('#status').val(),
        options: {}
    };
}

function generateColumns() {
    return [
        { data: 'OperationType', name: 'OperationType' },
        { data: 'SendBy', name: 'SendBy' },
        { data: 'MemberName', name: 'MemberName' },
        { data: 'Msisdn', name: 'Msisdn' },
        {
            data: 'Status',
            name: 'Status',
            render: function (data) {
                let statusClass = "badge bg-secondary"; // Default to pending
                let statusText = "Pending";

                if (data === "Successful") {
                    statusClass = "badge bg-primary";
                    statusText = "Success";
                }
                else if (data === "Failed") {
                    statusClass = "badge bg-danger";
                    statusText = "Failed";
                }

                return `<span class="${statusClass}">${statusText}</span>`;
            }
        },
        {
            data: 'Cost',
            name: 'Cost',
            render: function (data) {
                return data !== null && data !== undefined ? `XAF ${data.toFixed(1)}` : `XAF 0.0`;
            }
        },
        {
            data: 'CreatedDate',
            name: 'CreatedDate',
            render: function (data) {
                return moment(data).format('DD/MM/YYYY HH:mm:ss');
            }
        },
        {
            data: null,
            orderable: false,
            render: function (data, type, row) {
                return `<button class="btn btn-sm btn-primary" onclick="getSMSDetails('${row.Id}')">
                            <i class="mdi mdi-eye"></i> Details
                        </button>`;
            }
        }
    ];
}

function exportSMSData() {
    const filters = collectSMSExportParams();
    const params = new URLSearchParams(filters).toString();
    window.location.href = `/SMSActivityLog/SMSActivityLogDownload?${params}`;
}
function printSmsDetails() {
    const printContent = document.getElementById("smsDetailsContent").innerHTML;
    const printWindow = window.open('', '', 'width=800, height=600');
    printWindow.document.write(`
        <html>
            <head>
                <title>SMS Details</title>
                <link href="/Content/bootstrap.min.css" rel="stylesheet" />
                <link href="/Content/mdi/css/materialdesignicons.min.css" rel="stylesheet" />
            </head>
            <body>
                <div class="container mt-3">
                    ${printContent}
                </div>
            </body>
        </html>
    `);
    printWindow.document.close();
    printWindow.print();
    printWindow.close();
}
function getSMSDetails(id) {
    // Show Loader
    $('#smsLoader').removeClass('d-none');
    $('#smsDetailsContent').html('');

    $.ajax({
        url: `/SMSActivityLog/LoadSmsDetails?key=${id}`,
        type: 'GET',
        success: function (data) {
            // Hide Loader
            $('#smsLoader').addClass('d-none');

            // Load Content and Show Modal
            $('#smsDetailsContent').html(data);
            $('#smsDetailsModal').modal('show');

            // Update Transaction ID
            $('#smsTransactionId').text(id);
        },
        error: function () {
            $('#smsLoader').addClass('d-none');
            $('#smsDetailsContent').html('<p class="text-danger">Failed to load SMS details. Please try again.</p>');
        }
    });
}
