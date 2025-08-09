$(document).ready(function () {
    $('#applyFilterBtn').click(function () {
        loadAdjustmentRequests();
    });

    $('#resetFilterBtn').click(function () {
        resetAdjustmentFilters();
    });
    // Toggle branch filter
    $("#toggleBranch").on("change", function () {
        $("#branchFilterGroup").toggle(this.checked);
    });

    // Toggle date range filter
    $("#toggleDate").on("change", function () {
        $("#dateFilterGroup").toggle(this.checked);
    });
});

// 🔄 Load Adjustment Requests
function loadAdjustmentRequests() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/LoanAdjustmentConsole/LoadDataTable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectLoanAdjustmentFilters();
                filters.Options.draw = d.draw;
                filters.Options.start = d.start;
                filters.Options.length = d.length;
                filters.Options.skip = d.start;
                filters.Options.pageSize = d.length;
                filters.Options.sortColumnName = d.columns[d.order[0]?.column]?.data || "RequestedDate";
                filters.Options.sortColumnDirection = d.order[0]?.dir || "desc";
                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'RequestedDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                }
            },
            { data: 'CustomerName' },
            { data: 'CustomerId' },
            {
                data: 'OldBalance',
                render: formatCurrency
            },
            {
                data: 'NewBalance',
                render: formatCurrency
            },
            {
                data: 'Status',
                render: function (data) {
                    const badge = {
                        'pending': 'warning',
                        'approved': 'success',
                        'rejected': 'danger'
                    }[(data || '').toLowerCase()] || 'secondary';
                    return `<div class="text-center"><span class="badge bg-${badge}">${data || 'N/A'}</span></div>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    const status = (row.Status || '').toLowerCase();

                    return `
        <div class="btn-group" role="group">
            <button type="button"
                class="btn btn-sm btn-outline-primary"
                title="View Details"
                onclick="loadAdjustmentDetailModal('${row.Id}')">
                <i class="mdi mdi-eye-outline"></i>
            </button>

            ${status !== 'approved' ? `
                <button type="button"
                    class="btn btn-sm btn-outline-success"
                    title="Approve Request"
                    data-id="${row.Id}"
                    data-old-loan="${row.OldLoanAmount}"
                    data-new-loan="${row.NewLoanAmount}"
                    data-old-balance="${row.OldBalance}"
                    data-new-balance="${row.NewBalance}"
                    data-old-interest="${row.OldInterest}"
                    data-new-interest="${row.NewInterest}"
                    data-old-vat="${row.OldVat}"
                    data-new-vat="${row.NewVat}"
                    data-old-penalty="${row.OldPenalty}"
                    data-new-penalty="${row.NewPenalty}"
                    data-old-int-rate="${row.OldIntRate}"
                    data-new-int-rate="${row.NewIntRate}"
                    data-old-vat-rate="${row.OldVatRate}"
                    data-new-vat-rate="${row.NewVatRate}"
                    data-old-due="${row.OldDueAmount}"
                    data-new-due="${row.NewDueAmount}"
                    onclick="approveRequest(this)">
                    <i class="mdi mdi-check-circle-outline"></i>
                </button>` : ''}

            <button type="button"
                class="btn btn-sm btn-outline-danger"
                title="Reject Request"
                onclick="rejectRequest('${row.Id}')">
                <i class="mdi mdi-close-circle-outline"></i>
            </button>
        </div>`;
                }



            }


        ]
    });
}
function loadAdjustmentDetailModal(id) {
    // Show loader
    $('#loanAdjustmentLoader').removeClass('d-none');

    $.get(`/LoanAdjustmentConsole/GetRequestDetailPartial?id=${id}`, function (html) {
        // Inject the response HTML into the modal container
        $('#loanAdjustmentDetailsContainer').html(html);

        // Show the modal
        $('#loanAdjustmentModal').modal('show');
    })
        .fail(function () {
            appAlertError("❌ Failed to load adjustment detail. Please try again later.");
        })
        .always(function () {
            // Hide loader
            $('#loanAdjustmentLoader').addClass('d-none');
        });
}


function printAdjustmentDetail() {
    const modalContent = document.querySelector('#loanAdjustmentDetailModal .modal-body').innerHTML;
    const printWindow = window.open('', '', 'width=1000,height=700');
    printWindow.document.write(`<html><head><title>Loan Adjustment Detail</title>`);
    printWindow.document.write('<link rel="stylesheet" href="/css/site.css"/>');
    printWindow.document.write('</head><body>');
    printWindow.document.write(modalContent);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

// 📤 Filter Collector
function collectLoanAdjustmentFilters() {
    return {
        BranchId: $('#branchId').val(),
        Status: $('#status').val(),
        LoanId: $('#loanId').val(),
        CustomerId: $('#customerId').val(),
        RequestedBy: $('#requestedBy').val(),
        ApprovedBy: $('#approvedBy').val(),
        StartDate: formatDate($('#startDate').val(), true),
        EndDate: formatDate($('#endDate').val(), false),
        Options: { searchValue: "" }
    };
}

// 🔄 Reset Filters
function resetAdjustmentFilters() {
    $('#branchId, #status, #loanId, #customerId, #requestedBy, #approvedBy, #startDate, #endDate').val('');
    loadAdjustmentRequests();
}

// ✅ Approve Handler
function approveRequest(button) {
    const $btn = $(button);
    const id = $btn.data("id");

    const fields = [
        { label: "Loan Amount", old: $btn.data("old-loan"), new: $btn.data("new-loan") },
        { label: "Balance", old: $btn.data("old-balance"), new: $btn.data("new-balance") },
        { label: "Interest", old: $btn.data("old-interest"), new: $btn.data("new-interest") },
        { label: "VAT", old: $btn.data("old-vat"), new: $btn.data("new-vat") },
        { label: "Penalty", old: $btn.data("old-penalty"), new: $btn.data("new-penalty") },
        { label: "Interest Rate (%)", old: $btn.data("old-int-rate"), new: $btn.data("new-int-rate") },
        { label: "VAT Rate (%)", old: $btn.data("old-vat-rate"), new: $btn.data("new-vat-rate") },
        { label: "Due Amount", old: $btn.data("old-due"), new: $btn.data("new-due") }
    ];

    const changedFields = fields.filter(f =>
        f.old !== undefined && f.new !== undefined &&
        parseFloat(f.old) !== parseFloat(f.new)
    );

    if (changedFields.length === 0) {
        appAlertInfo("No changes detected between original and proposed values.");
        return;
    }

    // Build summary table
    const tableRows = changedFields.map(f => `
        <tr>
            <td>${f.label}</td>
            <td class="text-danger">${parseFloat(f.old).toLocaleString()}</td>
            <td class="text-success fw-bold">${parseFloat(f.new).toLocaleString()}</td>
        </tr>
    `).join('');

    const summaryTable = `
        <div class="table-responsive">
            <table class="table table-bordered table-sm table-striped mb-0">
                <thead class="table-light">
                    <tr>
                        <th>Field</th>
                        <th>Old Value</th>
                        <th>New Value</th>
                    </tr>
                </thead>
                <tbody>
                    ${tableRows}
                </tbody>
            </table>
        </div>
    `;

    alertify.confirm(
        'Approve Loan Adjustment Request',
        `
        <p>Are you sure you want to <strong>approve</strong> this loan adjustment request?</p>
        <p><strong>Detected Changes:</strong></p>
        ${summaryTable}
        `,
        function () {
            $.post('/LoanAdjustmentConsole/ApproveRequest', { requestId: id }, function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    loadAdjustmentRequests();
                } else {
                    toastr.error(response.message || "Approval failed.");
                }
            });
        },
        function () {
            toastr.info("Approval cancelled.");
        }
    ).set({
        labels: { ok: 'Yes, Approve', cancel: 'Cancel' },
        padding: true,
        closableByDimmer: false
    });
}



// ❌ Reject Handler
function rejectRequest(id) {
    alertify.confirm(
        'Reject Loan Adjustment Request',
        `
        <div class="text-start">
            <p>Please enter a reason for <strong class="text-danger">rejecting</strong> this adjustment request:</p>
            <textarea id="rejectReasonTextarea" class="form-control" rows="4" placeholder="Enter clear rejection reason (at least 20 characters)..."></textarea>
            <small class="text-muted mt-1 d-block">Minimum 20 characters required.</small>
        </div>
        `,
        function () {
            const reason = $('#rejectReasonTextarea').val().trim();
            if (reason.length >= 20) {
                $.post('/LoanAdjustmentConsole/RejectRequest', { requestId: id, reason: reason }, function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        loadAdjustmentRequests();
                    } else {
                        toastr.error(response.message || "Rejection failed.");
                    }
                });
            } else {
                appalert("⚠️ Rejection reason must be at least 20 characters.", 2, 1);
                rejectRequest(id); // 🔁 Retry input
            }
        },
        function () {
            toastr.info("❎ Rejection cancelled.");
        }
    ).set({
        labels: { ok: 'Submit Rejection', cancel: 'Cancel' },
        padding: true,
        closableByDimmer: false,
        resizable: false,
        movable: true
    });
}

// 📅 Format ISO Date
function formatDate(dateStr, isStart) {
    if (!dateStr) return null;
    const date = new Date(dateStr);
    if (isStart) {
        date.setHours(0, 0, 0, 0);
    } else {
        date.setHours(23, 59, 59, 999);
    }
    return date.toISOString();
}

// 💰 Format Number
function formatCurrency(data) {
    return `<div class="text-end">${data != null ? parseFloat(data).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0'}</div>`;
}
