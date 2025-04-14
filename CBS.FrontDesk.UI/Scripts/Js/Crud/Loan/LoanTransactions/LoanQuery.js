
// Function to export loan data
function exportLoanData() {
    alert("Export functionality to be implemented.");
    // Replace the alert with your actual export logic.
}

function initializeLoanDataTable() {
    $('#myDataTable').DataTable({
        destroy: true,
        serverSide: true,
        order: [[0, 'desc']],
        ajax: {
            url: '/Loan/LoadLoanData',
            type: 'POST',
            data: getSearchParameters,
            dataSrc: function (json) {
                if (json.data.length > 0) {
                    $('#loanDataCard').fadeIn();
                } else {
                    $('#loanDataCard').fadeOut();
                }
                return json.data;
            }
        },
        columns: [
            {
                data: 'LoanDate',
                title: 'Date',
                render: (data) => moment(data).format('DD/MM/YYYY HH:mm:ss')
            },
            {
                data: 'CustomerName',
                title: 'Name'
            },
            {
                data: 'CustomerId',
                title: 'M.REF'
            },
            {
                data: 'LoanAmount',
                title: 'Amount',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'InterestRate',
                title: 'Rate',
                render: (data) => `${data.toFixed(2)}%`
            },
            {
                data: 'AccrualInterest',
                title: 'int',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'Balance',
                title: 'Bal',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'DeliquentDays',
                title: 'D.Days'
            },
            {
                data: 'DeliquentAmount',
                title: 'D.AMT',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'DeliquentInterest',
                title: 'D.INT',
                render: (data) => formatCurrency(data)
            },
            {
                data: null,
                title: 'Due AMT',
                render: function (data, type, row) {
                    const totalDue = (row.DeliquentAmount || 0) + (row.DeliquentInterest || 0);
                    return formatCurrency(totalDue);
                }
            },
            {
                data: 'Id',
                title: 'Action',
                orderable: false,
                render: (data) => `
                    <button class="btn btn-info btn-sm" onclick="viewLoanDetails('${data}')">
                        <i class="mdi mdi-eye"></i> Detail
                    </button>
                `
            }
        ],
        language: {
            emptyTable: "No loans available for the selected criteria."
        },
        dom: 'rtip'
    });
}


//function initializeLoanDataTable() {
//    $('#myDataTable').DataTable({
//        destroy: true,
//        serverSide: true,
//        order: [[0, 'desc']],  // Default sorting by LoanDate in descending order
//        ajax: {
//            url: '/Loan/LoadLoanData',
//            type: 'POST',
//            data: getSearchParameters,
//            dataSrc: function (json) {
//                if (json.data.length > 0) {
//                    $('#loanDataCard').fadeIn();
//                } else {
//                    $('#loanDataCard').fadeOut();
//                }
//                return json.data;
//            }
//        },
//        columns: [
//            { data: 'LoanDate', title: 'Date', render: (data) => moment(data).format('DD/MM/YYYY HH:mm:ss') },
//            { data: 'CustomerName', title: 'Name' },
//            { data: 'CustomerId', title: 'Reference' },
//            { data: 'LoanAmount', title: 'Amount', render: (data) => formatCurrency(data) },
//            { data: 'InterestRate', title: 'I.Rate', render: (data) => `${data.toFixed(2)}%` },
//            { data: 'AccrualInterest', title: 'A.Int', render: (data) => formatCurrency(data) },
//            { data: 'Balance', title: 'Balance', render: (data) => formatCurrency(data) },
//            { data: 'DueAmount', title: 'D.Amount', render: (data) => formatCurrency(data) },
//            {
//                data: 'Id',
//                render: (data) => `
//                        <button class="btn btn-info btn-sm" onclick="viewLoanDetails('${data}')">
//                            <i class="mdi mdi-eye"></i> Detail
//                        </button>
//                    `
//            }
//        ],
//        language: {
//            emptyTable: "No loans available for the selected criteria."
//        },
//        dom: 'rtip'  // Hide the default search input
//    });
//}

// Function to pass parameters to server-side endpoint
function getSearchParameters(d) {
    d.searchCriteria = $('#searchCriteria').val();
    d.dateFrom = $('#dateFrom').val();
    d.dateTo = $('#dateTo').val();
    d.status = $('#status').val();
    d.deliquentstatus = $('#deliquentStatus').val();
    d.branchid = $('#branchInput').val();
}

$(document).ready(function () {
    // Initialize date pickers
    $('#dateFrom, #dateTo').datepicker({
        format: 'dd/mm/yyyy',
        autoclose: true,
        todayHighlight: true
    });

    // Initialize DataTable
    //initializeLoanDataTable();

    // Reinitialize DataTable on search button click
    $('#searchButton').on('click', function () {
        initializeLoanDataTable();
    });

    // Initially hide the filter section
    $('#filterContent').hide();

    // Toggle the visibility of the filter section
    $('#toggleFilterButton').on('click', function () {
        $('#filterContent').slideToggle(300, function () {
            if ($(this).is(':visible')) {
                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-up"></i> Hide Filters');
            } else {
                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-down"></i> Show Filters');
            }
        });
    });
});


function exportLoanData() {
    // Collect filter criteria
    let searchCriteria = $('#searchCriteria').val() || "all";
    let dateFrom = $('#dateFrom').val();
    let dateTo = $('#dateTo').val();
    let status = $('#status').val();
    let deliquentStatus = $('#deliquentStatus').val();
    let branchId = $('#branchInput').val() === "---Select All---" ? "" : $('#branchInput').val();

    // Construct the download URL with query parameters
    let url = `/Loan/Download?searchCriteria=${encodeURIComponent(searchCriteria)}&dateFrom=${encodeURIComponent(dateFrom)}&dateTo=${encodeURIComponent(dateTo)}&status=${encodeURIComponent(status)}&deliquentstatus=${encodeURIComponent(deliquentStatus)}&branchid=${encodeURIComponent(branchId)}`;

    // Trigger download
    window.location.href = url;
}


// Function to format currency in NGN
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-NG', {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(amount);
}


// Placeholder function for loan details view
function viewLoanDetails(loanId) {
    alert(`View loan details for Loan ID: ${loanId}`);
}
