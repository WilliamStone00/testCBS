$(document).ready(function () {
    var simulationId = $('#myDataTable').data('simulation-id') || '';
    console.log("SimulationId", simulationId);

    // Add loading indicator
   /* $('#myDataTable').wrap('<div class="position-relative"></div>')
        .parent().append('<div class="datatable-loader"><div class="spinner-border"></div></div>');
*/
    loadBulkOperationDetailsDataTable(simulationId);
});

function loadBulkOperationDetailsDataTable(simulationId) {
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


