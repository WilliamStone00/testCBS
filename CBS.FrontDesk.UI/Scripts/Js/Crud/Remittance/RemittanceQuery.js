
//$(document).ready(function () {
//    // Initialize date pickers
//    //$('#dateFrom, #dateTo').datepicker({
//    //    format: 'dd/mm/yyyy',
//    //    autoclose: true,
//    //    todayHighlight: true
//    //});

//    // Reinitialize DataTable on search button click
//    $('#searchButton').on('click', function () {
//        console.log("Search button clicked");
//        initializeRemittanceDataTable();
//    });

//    // Initially hide the filter section
//    $('#filterContent').hide();

//    // Toggle the visibility of the filter section
//    $('#toggleFilterButton').on('click', function () {
//        $('#filterContent').slideToggle(300, function () {
//            if ($(this).is(':visible')) {
//                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-up"></i> Hide Filters');
//            } else {
//                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-down"></i> Show Filters');
//            }
//        });
//    });

//    // Reset Button Click Event
//    $("#resetButton").click(function () {
//        console.log("Reset button clicked");
//        // Clear input fields
//        $("#dateFrom").val("");
//        $("#dateTo").val("");
//        $("#queryValue").val("");

//        // Reset select fields
//        $("#queryParameter").val("all").trigger("change");
//        $("#remittanceStatus").val("all").trigger("change");
//        $("#branchSelection").val("all").trigger("change");

//        // Refresh the DataTable to load all data
//        initializeRemittanceDataTable();
//    });
//});

//// Ensure the function is correctly defined
//function initializeRemittanceDataTable() {
//    //if ($.fn.DataTable.isDataTable('#remittanceDataTable')) {
//    //    $('#remittanceDataTable').DataTable().destroy(); // Destroy previous instance
//    //    $('#remittanceDataTable').empty(); // Clear old content
//    //}

//    $('#remittanceDataTable').DataTable({
//        destroy: true,
//        serverSide: true,
//        order: [[0, 'desc']], // Default sorting by date in descending order
//        ajax: {
//            url: '/Remittance/LoadRemittanceData',
//            type: 'POST',
//            data: getRemittanceSearchParameters,
//            dataSrc: function (json) {
//                if (json.data && json.data.length > 0) {
//                    $('#remittanceDataCard').fadeIn();
//                } else {
//                    $('#remittanceDataCard').fadeOut();
//                }
//                return json.data || [];
//            }
//        },
//        columns: [
//            { data: 'TransactionDate', title: 'Date', render: (data) => data ? moment(data).format('DD/MM/YYYY HH:mm:ss') : 'N/A' },
//            { data: 'SenderName', title: 'Sender Name' },
//            { data: 'ReceiverName', title: 'Receiver Name' },
//            { data: 'TransactionReference', title: 'Transaction Reference' },
//            { data: 'InitiatingBranch', title: 'Initiating Branch' },
//            { data: 'ReceivingBranch', title: 'Receiving Branch', render: (data) => data ? data : 'N/A' },
//            { data: 'Amount', title: 'Amount', render: (data) => data ? formatCurrency(data) : '0 FCFA' },
//            { data: 'Status', title: 'Status', render: (data) => `<span class="badge ${getRemittanceStatusClass(data)}">${data}</span>` },
//            { data: 'Id', title: 'Action', render: (data) => `<button class="btn btn-info btn-sm" onclick="viewRemittanceDetails('${data}')"><i class="mdi mdi-eye"></i> Detail</button>` }
//        ],
//        language: { emptyTable: "No remittance transactions available for the selected criteria." },
//        dom: 'rtip' // Hide default search input
//    });
//}


$(document).ready(function () {
   

    // Initialize DataTable
    initializeRemittanceDataTable();

    // Search Button Click Event
    $('#searchButton').on('click', function () {
        console.log("Search button clicked");
        initializeRemittanceDataTable();
    });

    // Initially hide the filter section
    $('#filterContent').hide();

    // Toggle Filter Visibility
    $('#toggleFilterButton').on('click', function () {
        $('#filterContent').slideToggle(300, function () {
            $('#toggleFilterButton').html($(this).is(':visible') ? '<i class="mdi mdi-chevron-up"></i> Hide Filters' : '<i class="mdi mdi-chevron-down"></i> Show Filters');
        });
    });

    // Reset Button Click Event
    $("#resetButton").click(function () {
        console.log("Reset button clicked");

        // Clear input fields
        $("#dateFrom").val("");
        $("#dateTo").val("");
        $("#queryValue").val("");

        // Reset select fields
        $("#queryParameter").val("all").trigger("change");
        $("#remittanceStatus").val("all").trigger("change");
        $("#branchSelection").val("all").trigger("change");

        // Refresh DataTable
        initializeRemittanceDataTable();
    });
});

// Function to Initialize DataTable
function initializeRemittanceDataTable() {
    console.log("Initializing DataTable...");

    if ($.fn.DataTable.isDataTable('#remittanceDataTable')) {
        $('#remittanceDataTable').DataTable().destroy(); // Destroy previous instance
        $('#remittanceDataTable').empty(); // Clear table content
    }

    $('#remittanceDataTable').DataTable({
        
        destroy: true,
        processing: true,
        serverSide: true,
        order: [[0, 'desc']], // Sort by date descending
        ajax: {
            url: '/Remittance/LoadRemittanceData',
            type: 'POST',
            data: getRemittanceSearchParameters
            //error: function (xhr, error, thrown) {
            //    console.error("AJAX Error: ", error, thrown);
            //    console.log(xhr.responseText);
            //}
        },
        columns: [
            { data: 'TransactionDate', title: 'Date', render: data => data ? moment(data).format('DD/MM/YYYY HH:mm:ss') : 'N/A' },
            { data: 'SenderName', title: 'Sender Name' },
            { data: 'ReceiverName', title: 'Receiver Name' },
            { data: 'TransactionReference', title: 'Transaction Reference' },
            { data: 'InitiatingBranch', title: 'Initiating Branch' },
            { data: 'ReceivingBranch', title: 'Receiving Branch', render: data => data ? data : 'N/A' },
            { data: 'Amount', title: 'Amount', render: data => data ? formatCurrency(data) : '0 FCFA' },
            { data: 'Status', title: 'Status', render: data => `<span class="badge ${getRemittanceStatusClass(data)}">${data}</span>` },
            { data: 'Id', title: 'Action', render: data => `<button class="btn btn-info btn-sm" onclick="viewRemittanceDetails('${data}')"><i class="mdi mdi-eye"></i> Detail</button>` }
        ],
        language: { emptyTable: "No remittance transactions found." },
        dom: 'rtip'
    });

    console.log("DataTable Initialized.");
}

// Function to Get Search Parameters
function getRemittanceSearchParameters(d) {
    d.queryParameter = $('#queryParameter').val() || "sourcebranchid";
    d.queryValue = $('#queryValue').val() || "";
    d.dateFrom = $('#dateFrom').val() || "";
    d.dateTo = $('#dateTo').val() || "";
    d.status = $('#remittanceStatus').val() || "all";
    d.branchId = ($('#branchSelection').val() === "---Select All---") ? "" : $('#branchSelection').val();

    console.log("Sending search parameters:", d);
}


// Ensure search parameters are correctly passed
//function getRemittanceSearchParameters(d) {
//    d.queryParameter = $('#queryParameter').val() || "all";
//    d.queryValue = $('#queryValue').val() || "";
//    d.dateFrom = $('#dateFrom').val() || "";
//    d.dateTo = $('#dateTo').val() || "";
//    d.status = $('#remittanceStatus').val() || "all";
//    d.branchId = ($('#branchSelection').val() === "---Select All---") ? "" : $('#branchSelection').val();
//}

function getRemittanceStatusClass(status) {
    switch (status) {
        case 'Approved':
            return 'bg-success text-white'; // Green
        case 'Paid':
            return 'bg-primary text-white'; // Blue
        case 'Withdrawn':
            return 'bg-info text-white'; // Cyan
        case 'Rejected':
            return 'bg-danger text-white'; // Red
        default:
            return 'bg-secondary text-white'; // Grey (for unknown statuses)
    }
}

//function getRemittanceSearchParameters(d) {
//    d.queryParameter = $('#queryParameter').val() || "";
//    d.queryValue = $('#queryValue').val() || "";
//    d.dateFrom = $('#dateFrom').val() || "";
//    d.dateTo = $('#dateTo').val() || "";
//    d.status = $('#remittanceStatus').val() || "all";
//    d.branchId = $('#branchSelection').val() && $('#branchSelection').val() !== "---Select All---" ? $('#branchSelection').val() : "";
//}

function exportRemittanceData() {
    // Collect filter criteria
    let queryParameter = $('#queryParameter').val() || "sourcebranchid";
    let queryValue = $('#queryValue').val() || "";
    let dateFrom = $('#dateFrom').val();
    let dateTo = $('#dateTo').val();
    let status = $('#remittanceStatus').val() || "all";
    let branchId = $('#branchSelection').val() === "---Select All---" ? "" : $('#branchSelection').val();

    // Validate date format (dd/MM/yyyy)
    const datePattern = /^\d{2}\/\d{2}\/\d{4}$/;
    if ((dateFrom && !datePattern.test(dateFrom)) || (dateTo && !datePattern.test(dateTo))) {
        appalert("Invalid date format! Use dd/MM/yyyy.", 2, 1);
        return;
    }

    // Confirm Export
    alertify.confirm(
        "Export Confirmation",
        "Are you sure you want to export remittance data based on selected filters?",
        function () {
            // Construct the download URL properly
            let url = `/Remittance/Download?` +
                `queryParameter=${encodeURIComponent(queryParameter)}` +
                `&queryValue=${encodeURIComponent(queryValue)}` +
                `&dateFrom=${encodeURIComponent(dateFrom)}` +
                `&dateTo=${encodeURIComponent(dateTo)}` +
                `&status=${encodeURIComponent(status)}` +
                `&branchid=${encodeURIComponent(branchId)}`;

            // Trigger download
            window.location.href = url;
        },
        function () {
            appalert("Export cancelled.", 2, 1);
        }
    ).set({ labels: { ok: "Yes, Export", cancel: "Cancel" } });
}
