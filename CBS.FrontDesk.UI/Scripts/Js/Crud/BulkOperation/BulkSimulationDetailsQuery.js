$(document).ready(function () {
    var simulationId = $('#myDataTable').data('simulation-id') || '';
    console.log("SimulationId", simulationId);

    // Add loading indicator
   /* $('#myDataTable').wrap('<div class="position-relative"></div>')
        .parent().append('<div class="datatable-loader"><div class="spinner-border"></div></div>');
*/
    loadBulkOperationDataTable(simulationId);
});

function loadBulkOperationDataTable(simulationId) {
    $('#myDataTable').DataTable({
        destroy: true,
        serverSide: true,
        processing: true,
        order: [[0, 'desc']],
        ajax: {
            url: '/BulkOperation/LoadBulkOperationDetailsData',
            type: 'POST',
            data: function (d) {
                d.simulationId = simulationId;
               
                return d;
            },
            dataSrc: function (json) {
                return json.data;
            }
        },
        columns: [
            {
                data: 'MemberName',
                title: 'Member Name',
                render: (data) => data || 'N/A'
            },
            {
                data: 'MemberReference',
                title: 'Member Ref',
                render: (data) => data || 'N/A'
            },
            {
                data: 'SourceAccountNumber',
                title: 'Source Account',
                render: (data) => data || 'N/A'
            },
            {
                data: 'SourceAccountType',
                title: 'Source Type',
                render: (data) => data || 'N/A'
            },
            {
                data: 'SourceAccountBalance',
                title: 'Source Balance',
                className: 'text-end',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'DestinationAccountNumber',
                title: 'Destination Account',
                render: (data) => data || 'N/A'
            },
            {
                data: 'DestinationAccountType',
                title: 'Destination Type',
                render: (data) => data || 'N/A'
            },
            {
                data: 'AmountToDebit',
                title: 'Amount To Debit',
                className: 'text-end',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'ApprovalStatus',
                title: 'Status',
                render: function (data, type, row) {
                    let badgeClass = '';
                    switch (data) {
                        case 'Pending':
                            badgeClass = 'bg-warning text-dark';
                            break;
                        case 'Review':
                            badgeClass = 'bg-info text-white';
                            break;
                        case 'Approved':
                            badgeClass = 'bg-success text-white';
                            break;
                        default:
                            badgeClass = 'bg-secondary text-white';
                    }
                    return `<span class="badge ${badgeClass}">${data || 'N/A'}</span>`;
                }
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
                        <div class="text-center">
                            <a href="#" 
                               class="btn btn-sm btn-outline-primary details-btn" 
                               title="Transfer Details"
                               data-id="${id}">
                                <i class="fas fa-user-cog me-1"></i>View
                            </a>
                        </div>`;
                }
            }
        ],
        language: {
            emptyTable: "No Bulk Oprations available for the selected criteria."
        },
        dom: 'rtip',
        initComplete: function () {
            // Add click event handler for details buttons
            $('#myDataTable').on('click', '.details-btn', function (e) {
                e.preventDefault();
                var KEY = $(this).data('id');
                console.log(KEY);
                var modal = new bootstrap.Modal(document.getElementById('transferDetailsModal'));
                var contentDiv = document.getElementById('transferDetailsContent');
                // Show loading indicator
                showLoading(modal, contentDiv, `GetBulkOperationDetails : ${KEY}`);

                // Fetch detailed data
                $.ajax({
                    url: '/BulkOperation/GetBulkOperationDetails',
                    type: 'GET',
                    data: { KEY: KEY },
                    success: function (data) {
                        contentDiv.innerHTML = data;
                    },
                    error: function () {
                        contentDiv.innerHTML = `
                    <div class="alert alert-danger">
                        Failed to load transfer details. Please try again.
                    </div>`;
                    }
                });
            });
        }
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-NG', {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(amount);
}

function showLoading(modal,contentDiv,message) {
       // Show loading state
    contentDiv.innerHTML = `
            <div class="loading-container">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>`;

    modal.show();
}

function hideLoading() {
    const loadingModal = bootstrap.Modal.getInstance(document.getElementById('loadingModal'));
    if (loadingModal) {
        loadingModal.hide();
    }
}

function showDetailsPopup(details) {
    // Format dates
    const transferDate = new Date(details.TransferDate).toLocaleString();
    const approvalDate = details.ApprovalDate ? new Date(details.ApprovalDate).toLocaleString() : 'N/A';

    // Create the HTML content for the popup
    const content = `
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-6">
                    <h5>Member Information</h5>
                    <table class="table table-sm">
                        <tr><th>Member Name:</th><td>${details.MemberName || 'N/A'}</td></tr>
                        <tr><th>Member Reference:</th><td>${details.MemberReference || 'N/A'}</td></tr>
                        <tr><th>Branch:</th><td>${details.BranchName || 'N/A'} (${details.BranchCode || 'N/A'})</td></tr>
                    </table>
                    
                    <h5>Source Account</h5>
                    <table class="table table-sm">
                        <tr><th>Account Number:</th><td>${details.SourceAccountNumber || 'N/A'}</td></tr>
                        <tr><th>Account Type:</th><td>${details.SourceAccountType || 'N/A'}</td></tr>
                        <tr><th>Balance:</th><td>${formatCurrency(details.SourceAccountBalance)}</td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <h5>Destination Account</h5>
                    <table class="table table-sm">
                        <tr><th>Account Number:</th><td>${details.DestinationAccountNumber || 'N/A'}</td></tr>
                        <tr><th>Account Type:</th><td>${details.DestinationAccountType || 'N/A'}</td></tr>
                        <tr><th>Balance:</th><td>${formatCurrency(details.DestinationBalance)}</td></tr>
                    </table>
                    
                    <h5>Transfer Details</h5>
                    <table class="table table-sm">
                        <tr><th>Amount:</th><td>${formatCurrency(details.AmountToDebit)}</td></tr>
                        <tr><th>Status:</th><td>${details.TransferStatus || 'N/A'}</td></tr>
                        <tr><th>Transfer Date:</th><td>${transferDate}</td></tr>
                        <tr><th>Approval Status:</th><td>${details.ApprovalStatus || 'N/A'}</td></tr>
                        <tr><th>Approval Date:</th><td>${approvalDate}</td></tr>
                        ${details.TransferErrorMessage ? `<tr><th>Error:</th><td class="text-danger">${details.TransferErrorMessage}</td></tr>` : ''}
                    </table>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-12">
                    <h5>Balance Information</h5>
                    <table class="table table-sm">
                        <tr><th>Total Balance:</th><td>${formatCurrency(details.TotalBalance)}</td></tr>
                        <tr><th>Net Balance:</th><td>${formatCurrency(details.NetBalance)}</td></tr>
                    </table>
                </div>
            </div>
        </div>
    `;

    // Set the content and show the modal
    document.getElementById('detailsModalBody').innerHTML = content;
    const detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
    detailsModal.show();

    // Show the popup using SweetAlert (or you can use Bootstrap modal)
  /*  Swal.fire({
        title: 'Transfer Details',
        html: content,
        width: '90%',
        confirmButtonText: 'Close',
        customClass: {
            popup: 'text-start'
        }
    });*/
}

