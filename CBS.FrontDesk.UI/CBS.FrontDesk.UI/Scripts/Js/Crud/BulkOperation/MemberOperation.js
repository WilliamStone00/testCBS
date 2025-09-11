
function initializeMemberDataTable(data) {
    $('#memberDataTable').DataTable({
        data: data,
        columns: [
            {
                data: 'SourceAccountNumber',
                title: 'Source Account',
                render: (data) => data
            },
            {
                data: 'SourceAccountType',
                title: 'Source Type',
                render: (data) => data
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
                render: (data) => data
            },
            {
                data: 'DestinationAccountType',
                title: 'Destination Type',
                render: (data) => data
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
                    return `<span class="badge ${row.ApprovalStatusBadge}">${data}</span>`;
                }
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
                        <div class="text-center">
                            <a href="/BulkOperation/TransferDataDetails?KEY=${id}" class="btn btn-sm btn-outline-primary" title="Transfer Details">
                                <i class="fas fa-user-cog me-1"></i> View Account Transfer Details
                            </a>
                        </div>`;
                }
            }
        ],
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        dom: '<"top"lf>rt<"bottom"ip><"clear">',
        language: {
            emptyTable: "No members found in this operation",
            info: "Showing _START_ to _END_ of _TOTAL_ members",
            infoEmpty: "Showing 0 to 0 of 0 members",
            infoFiltered: "(filtered from _MAX_ total members)",
            lengthMenu: "Show _MENU_ members per page",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        },
        initComplete: function () {
            $('.dataTables_length select').addClass('form-select form-select-sm');
            $('.dataTables_filter input').addClass('form-control form-control-sm');
        }
    });
}



$(document).on('page-loaded', function () {
    if ($('#memberDataTable').length) {
        initializeMemberDataTable(window.bulkOperationData);
    }
});

// Initialize immediately if already loaded
$(function () {
    if ($('#memberDataTable').length) {
        initializeMemberDataTable(window.bulkOperationData);
    }
});

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-NG', {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(amount);
}