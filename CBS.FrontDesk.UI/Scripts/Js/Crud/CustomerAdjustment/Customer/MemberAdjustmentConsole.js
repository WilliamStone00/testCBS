$(document).ready(function () {
    $('#applyFilterBtn').click(function () {
        loadMemberAdjustmentRequests();
    });

    $('#resetFilterBtn').click(function () {
        resetMemberAdjustmentFilters();
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
function loadMemberAdjustmentRequests() {
    var dataTable = document.getElementById('myDataTable');
    console.log(dataTable);
    $(dataTable).DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/MemberAdjustmentConsole/LoadAdjustmentRequestDataTable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectMemberAdjustmentFilters(); // keep if same

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
                console.log(JSON.stringify(filters));
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
            { data: 'BranchName' },
            {
                data: 'OldBalance',
                render: formatCurrency1
            },
            {
                data: 'NewBalance',
                render: formatCurrency1
            },
            { data: 'RequestedBy' },
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
                            onclick="loadMemberAdjustmentDetailModal('${row.Id}')">
                            <i class="mdi mdi-eye-outline"></i>
                        </button>

                        ${status !== 'approved' ? `
                            <button type="button"
                                class="btn btn-sm btn-outline-success"
                                title="Approve Request"
                                data-id="${row.Id}"
                                data-customer-id="${row.CustomerId}"
                                data-customer-name="${row.CustomerName}"
                                data-branch-id="${row.BranchId}"
                                data-account-id="${row.AccountId}"
                                data-branch-name="${row.BranchName}"
                                data-old-firstname="${row.OldFirstName}"
                                data-new-firstname="${row.NewFirstName}"
                                data-old-lastname="${row.OldLastName}"
                                data-new-lastname="${row.NewLastName}"
                                data-old-balance="${row.OldBalance}"
                                data-new-balance="${row.NewBalance}"
                                data-old-memberstatus="${row.OldMemberStatus}"
                                data-new-memberstatus="${row.NewMemberStatus}"
                                data-old-membershipstatus="${row.OldMemberShipStatus}"
                                data-new-membershipstatus="${row.NewMemberShipStatus}"
                                data-old-category="${row.OldMemberCategory}"
                                data-new-category="${row.NewMemberCategory}"
                                data-reason="${row.Reason}"
                                onclick="approveMemberRequest(this)">
                                <i class="mdi mdi-check-circle-outline"></i>
                            </button>` : ''}

                        <button type="button"
                            class="btn btn-sm btn-outline-danger"
                            title="Reject Request"
                            onclick="rejectMemberRequest('${row.Id}')">
                            <i class="mdi mdi-close-circle-outline"></i>
                        </button>
                    </div>`;
                }
            }
        ]
    });
}

function loadMemberAdjustmentDetailModal(id) {
    // Show loader
    $('#memberAdjustmentLoader').removeClass('d-none');

    $.get(`/MemberAdjustmentConsole/GetRequestDetailPartial?id=${id}`, function (html) {
        // Inject the response HTML into the modal container
        $('#memberAdjustmentDetailsContainer').html(html);

        // Show the modal
        $('#memberAdjustmentModal').modal('show');
    })
        .fail(function () {
            appAlertError("❌ Failed to load adjustment detail. Please try again later.");
        })
        .always(function () {
            // Hide loader
            $('#memberAdjustmentLoader').addClass('d-none');
        });
}


function printMemberAdjustmentDetail() {
    const modalContent = document.querySelector('#memberAdjustmentDetailModal .modal-body').innerHTML;
    const printWindow = window.open('', '', 'width=1000,height=700');
    printWindow.document.write(`<html><head><title>Member Adjustment Detail</title>`);
    printWindow.document.write('<link rel="stylesheet" href="/css/site.css"/>');
    printWindow.document.write('</head><body>');
    printWindow.document.write(modalContent);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.print();
}

// 📤 Filter Collector
function collectMemberAdjustmentFilters() {
    return {
        BranchId: $('#branchId').val(),
        Status: $('#status').val(),
        CustomerId: $('#customerId').val(),
        RequestedBy: $('#requestedBy').val(),
        ApprovedBy: $('#approvedBy').val(),
        StartDate: formatDate1($('#startDate').val(), true),
        EndDate: formatDate1($('#endDate').val(), false),
        Options: { searchValue: "" }
    };
}

// 🔄 Reset Filters
function resetMemberAdjustmentFilters() {
    $('#branchId, #status, #customerId, #requestedBy, #approvedBy, #startDate, #endDate').val('');
    loadMemberAdjustmentRequests();
}

// ✅ Approve Handler
function approveMemberRequest(button) {
    const $btn = $(button);
    const id = $btn.data("id");

    // Map fields from DTO
    const fields = [
        { label: "First Name", old: $btn.data("old-firstname"), new: $btn.data("new-firstname") },
        { label: "Last Name", old: $btn.data("old-lastname"), new: $btn.data("new-lastname") },
        { label: "Balance", old: $btn.data("old-balance"), new: $btn.data("new-balance") },
        { label: "Member Status", old: $btn.data("old-memberstatus"), new: $btn.data("new-memberstatus") },
        { label: "Membership Status", old: $btn.data("old-membershipstatus"), new: $btn.data("new-membershipstatus") },
        { label: "Member Category", old: $btn.data("old-category"), new: $btn.data("new-category") }
    ];

    // Detect changed fields
    const changedFields = fields.filter(f =>
        f.old !== undefined && f.new !== undefined &&
        f.old.toString() !== f.new.toString()
    );

    if (changedFields.length === 0) {
        appAlertInfo("No changes detected between original and proposed values.");
        return;
    }

    // Build summary table
    const tableRows = changedFields.map(f => `
        <tr>
            <td>${f.label}</td>
            <td class="text-danger">${f.old || ''}</td>
            <td class="text-success fw-bold">${f.new || ''}</td>
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
        'Approve Member Adjustment Request',
        `
        <p>Are you sure you want to <strong>approve</strong> this member adjustment request?</p>
        <p><strong>Detected Changes:</strong></p>
        ${summaryTable}
        `,
        function () {
            $.post('/MemberAdjustmentConsole/approveMemberRequest', { requestId: id }, function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    loadMemberAdjustmentRequests();
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
function rejectMemberRequest(id) {
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
                $.post('/MemberAdjustmentConsole/rejectMemberRequest', { requestId: id, reason: reason }, function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        loadMemberAdjustmentRequests();
                    } else {
                        toastr.error(response.message || "Rejection failed.");
                    }
                });
            } else {
                appalert("⚠️ Rejection reason must be at least 20 characters.", 2, 1);
                rejectMemberRequest(id); // 🔁 Retry input
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
function formatDate1(dateStr, isStart) {
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
function formatCurrency1(data) {
    return `<div class="text-end">${data != null ? parseFloat(data).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0'}</div>`;
}
