$(document).ready(function () {
    // Reset Button Click Event
    $("#resetButton").click(function () {
        // Clear input fields
        $("#dateFrom").val("");
        $("#dateTo").val("");
        $("#searchCriteria").val("");

        // Reset select fields
        $("#loanCategory").val("all").trigger("change");
        $("#approvalStatus").val("all").trigger("change");
        $("#loanTarget").val("all").trigger("change");
        $("#branchInput").val("all").trigger("change");

        // Refresh the DataTable to load all data
        initializeLoanDataTable();
    });
});


// Function to export loan data
function exportLoanData() {
    alert("Export functionality to be implemented.");
    // Replace the alert with your actual export logic.
}
function confirmDelete(loanApplicationid) {
    alertify.confirm(
        "DELETE WARNING!!!",
        `<p class="text-danger">
            <strong>Are you sure you want to delete this loan application?</strong>
        </p>
        <p>
            <strong>Warning:</strong> Deleting this loan application will result in the **permanent loss** of the following dependencies:
        </p>
        <ul>
            <li><strong>Loan Records:</strong> Approved loans, disbursed loans, amortization schedules</li>
            <li><strong>Financial Transactions:</strong> Loan payments, accrued interest, penalties, fees</li>
            <li><strong>Guarantors & Collateral:</strong> Guarantor information and collateral assets</li>
            <li><strong>Approval & Validation:</strong> Loan committee approvals and validation history</li>
            <li><strong>Documents & Notifications:</strong> Attached documents and notification logs</li>
            <li><strong>Delinquency & Restructuring:</strong> Loan delinquency status, refinanced loans</li>
        </ul>
        <p class="fw-bold text-danger">
            This action <strong>CANNOT</strong> be undone! Please proceed with caution.
        </p>`,
        function () {
            $.ajax({
                type: "GET",
                url: `/LoanApplication/Delete?id=${loanApplicationid}`,
                success: function (response) {
                    if (response.success) {
                       
                        appalert(response.message, 1, 1);
                        setTimeout(() => {
                            window.location.href = "/LoanApplication/Index"; // Redirect to Loan List
                        }, 1500);
                    } else {
                        appalert(response.message, 2, 1);
                    }
                },
                error: function (err) {
                    appalert("Error: " + err.statusText, 1, 1);
                    
                }
            });
        },
        function () {
            appalert("Transaction cancelled", 2, 1);
        }
    ).set({ labels: { ok: "Yes, Delete", cancel: "Cancel" } });
}

function initializeLoanDataTable() {
    if ($.fn.DataTable.isDataTable('#myDataTable')) {
        $('#myDataTable').DataTable().destroy(); // Destroy previous instance
        $('#myDataTable').empty(); // Clear old content
    }

    $('#myDataTable').DataTable({
        destroy: true,
        serverSide: true,
        order: [[0, 'desc']], // Default sorting by ApplicationDate in descending order
        ajax: {
            url: '/LoanApplication/LoadLoanData',
            type: 'POST',
            data: getSearchParameters,
            dataSrc: function (json) {
                if (json.data && json.data.length > 0) {
                    $('#loanApplicationDataCard').fadeIn();
                } else {
                    $('#loanApplicationDataCard').fadeOut();
                }
                return json.data || []; // Ensure an array is returned
            }
        },
        columns: [
            {
                data: 'ApplicationDate',
                title: 'Date',
                render: (data) => data ? moment(data).format('DD/MM/YYYY HH:mm:ss') : 'N/A'
            },
            { data: 'CustomerName', title: 'Name' },
            { data: 'CustomerId', title: 'Reference' },
            {
                data: 'Amount',
                title: 'Amount',
                render: (data) => data ? formatCurrency(data) : '0 FCFA'
            },
            {
                data: 'InterestRate',
                title: 'I.Rate',
                render: (data) => data != null ? `${data.toFixed(2)}%` : 'N/A'
            },
            {
                data: 'ApprovalStatus',
                title: 'A.Status',
                render: function (data) {
                    return `<span class="badge ${getStatusClass(data)}">${data}</span>`;
                }
            },
            {
                data: 'Id',
                title: 'Action',
                render: (data) => data ? `
                    <button class="btn btn-info btn-sm" onclick="viewLoanDetails('${data}')">
                        <i class="mdi mdi-eye"></i> Detail
                    </button>
                ` : 'N/A'
            }
        ],
        language: {
            emptyTable: "No loan application available for the selected criteria."
        },
        dom: 'rtip'  // Hide the default search input
    });
}

// Function to return the appropriate CSS class for status
function getStatusClass(status) {
    switch (status) {
        case 'Approved':
            return 'bg-success text-white'; // Green
        case 'Validated':
            return 'bg-primary text-white'; // Blue
        case 'Await_Initialization_Fee_Payment':
            return 'bg-warning text-dark'; // Yellow
        case 'Rejected':
            return 'bg-danger text-white'; // Red
        default:
            return 'bg-secondary text-white'; // Grey (for unknown statuses)
    }
}


function getSearchParameters(d) {
    d.searchCriteria = $('#searchCriteria').val() || "";
    d.dateFrom = $('#dateFrom').val() || "";
    d.dateTo = $('#dateTo').val() || "";
    d.status = $('#status').length ? $('#status').val() : "all"; // Ensure element exists
    d.deliquentStatus = $('#deliquentStatus').length ? $('#deliquentStatus').val() : "all";
    d.branchId = $('#branchInput').val() && $('#branchInput').val() !== "---Select All---" ? $('#branchInput').val() : "";
    d.loanCategory = $('#loanCategory').length ? $('#loanCategory').val() : "all";
    d.loanTarget = $('#loanTarget').length ? $('#loanTarget').val() : "all";
    d.approvalStatus = $('#approvalStatus').length ? $('#approvalStatus').val() : "all";
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
    let status = $('#approvalStatus').val() || "all";
    let deliquentStatus = $('#deliquentStatus').val() || "all";
    let branchId = $('#branchInput').val() === "---Select All---" ? "" : $('#branchInput').val();
    let loanCategory = $('#loanCategory').val() || "all";
    let loanTarget = $('#loanTarget').val() || "all";
    let approvalStatus = $('#approvalStatus').val() || "all";

    // Validate date format (dd/MM/yyyy)
    const datePattern = /^\d{2}\/\d{2}\/\d{4}$/;
    if ((dateFrom && !datePattern.test(dateFrom)) || (dateTo && !datePattern.test(dateTo))) {
       
        appalert("Invalid date format! Use dd/MM/yyyy.", 2, 1);
        return;
    }

    // Confirm Export
    alertify.confirm(
        "Export Confirmation",
        "Are you sure you want to export loan data based on selected filters?",
        function () {
            // Construct the download URL properly
            let url = `/LoanApplication/Download?` +
                `searchCriteria=${encodeURIComponent(searchCriteria)}` +
                `&dateFrom=${encodeURIComponent(dateFrom)}` +
                `&dateTo=${encodeURIComponent(dateTo)}` +
                `&status=${encodeURIComponent(status)}` +
                `&deliquentstatus=${encodeURIComponent(deliquentStatus)}` +
                `&branchid=${encodeURIComponent(branchId)}` +
                `&loanCategory=${encodeURIComponent(loanCategory)}` +
                `&loanTarget=${encodeURIComponent(loanTarget)}` +
                `&approvalStatus=${encodeURIComponent(approvalStatus)}`;

            // Trigger download
            window.location.href = url;
        },
        function () {
            appalert("Export cancelled.", 2, 1);
        }
    ).set({ labels: { ok: "Yes, Export", cancel: "Cancel" } });
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
    window.location.href = `/LoanApplication/Details?KEY=${loanId}`;
}



