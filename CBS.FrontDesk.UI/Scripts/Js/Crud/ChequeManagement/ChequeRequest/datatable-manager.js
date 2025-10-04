class DataTableManager {
    constructor() {
        this.dataTable = null;
        this.initializeDataTable();
        this.bindEvents();
    }

    initializeDataTable() {
        if ($.fn.DataTable.isDataTable('#requestsDataTable')) {
            this.dataTable = $('#requestsDataTable').DataTable();
            return;
        }

        this.dataTable = $('#requestsDataTable').DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: '/ChequeRequest/LoadRequestsForDataTable',
                type: 'POST',
                data: (d) => {
                    return {
                        query: this.buildQuery(d)
                    };
                }
            },
            columns: [
                {
                    data: 'customerName',
                    name: 'CustomerName',
                    className: "ps-4"
                },
                {
                    data: 'categoryName',
                    name: 'CategoryName'
                },
                {
                    data: 'requestDate',
                    name: 'RequestDate',
                    render: (data) => data ? new Date(data).toLocaleDateString() : 'N/A'
                },
                {
                    data: 'status',
                    name: 'Status',
                    className: "text-center",
                    render: (data) => this.getStatusBadge(data)
                },
                {
                    data: null,
                    className: "text-center pe-4",
                    orderable: false,
                    render: (data, type, row) => this.renderActionButtons(row)
                }
            ],
            order: [[2, 'desc']],
            language: {
                emptyTable: "No cheque requests found",
                zeroRecords: "No matching requests found"
            }
        });
    }

    buildQuery(d) {
        const order = d.order && d.order[0] ? d.order[0] : {};
        const orderColumn = d.columns && d.columns[order.column] ? d.columns[order.column] : null;

        return {
            Options: {
                draw: d.draw,
                start: d.start,
                pageSize: d.length,
                sortColumnName: orderColumn ? orderColumn.data : 'requestDate',
                sortDirection: order.dir || 'desc'
            },
            CustomerName: $('#customerFilter').val() || '',
            CategoryName: $('#categoryFilter').val() || '',
            Status: $('#statusFilter').val() || '',
            StartDate: $('#startDateFilter').val() || null,
            EndDate: $('#endDateFilter').val() || null
        };
    }

    getStatusBadge(status) {
        const statusMap = {
            'pending': 'bg-warning text-dark',
            'approved': 'bg-success',
            'rejected': 'bg-danger',
            'delivered': 'bg-info',
            'reviewed': 'bg-secondary'
        };

        const statusClass = statusMap[(status || '').toLowerCase()] || 'bg-secondary';
        return `<span class="badge ${statusClass}">${status || '—'}</span>`;
    }

    renderActionButtons(row) {
        const status = (row.status || '').toLowerCase();
        const id = row.Id || row.id || '';
        const customerName = (row.customerName || '').replace(/'/g, "\\'");

        // Details button (always visible)
        const detailsBtn = `
            <button type="button" class="btn btn-sm btn-outline-info me-1" 
                onclick="dataTableManager.viewDetails('${id}')" 
                title="View Details">
                <i class="mdi mdi-eye-outline"></i>
            </button>
        `;

        let actionButtons = '';

        // Show action buttons based on status
        if (status === 'pending' || status === 'reviewed') {
            actionButtons = `
                <button type="button" class="btn btn-sm btn-success me-1" 
                    onclick="dataTableManager.showActionModal('approve', '${id}', '${customerName}')" 
                    title="Approve Request">
                    <i class="mdi mdi-check"></i>
                </button>
                <button type="button" class="btn btn-sm btn-danger" 
                    onclick="dataTableManager.showActionModal('reject', '${id}', '${customerName}')" 
                    title="Reject Request">
                    <i class="mdi mdi-close"></i>
                </button>
            `;
        }

        return `<div class="btn-group">${detailsBtn}${actionButtons}</div>`;
    }

    bindEvents() {
        // Apply filters
        $('#applyFilterBtn').on('click', () => {
            this.dataTable.ajax.reload();
        });

        // Reset filters
        $('#resetFilterBtn').on('click', () => {
            $('input[type="text"], select').val('');
            $('input[type="date"]').val('');
            $('input[type="checkbox"]').prop('checked', false);
            this.dataTable.ajax.reload();
        });

        // Toggle filter sections
        $('#byCustomerToggle').change(function () {
            $('#customerFilterSection').toggle(this.checked);
        });
        $('#byCategoryToggle').change(function () {
            $('#categoryFilterSection').toggle(this.checked);
        });
        $('#byStatusToggle').change(function () {
            $('#statusFilterSection').toggle(this.checked);
        });
        $('#byDateToggle').change(function () {
            $('#dateFilterSection').toggle(this.checked);
        });
    }

    viewDetails(requestId) {
        $.get('/ChequeRequest/InitializeData', {
            KEY: requestId,
            partialView: '_RequestDetails',
            path: 'get'
        }).done(function (html) {
            $('#detailsModalBody').html(html);
            new bootstrap.Modal(document.getElementById('detailsModal')).show();
        }).fail(function () {
            alert('Unable to load request details.');
        });
    }

    showActionModal(action, requestId, customerName) {
        const modalTitle = action === 'approve' ? 'Approve Request' : 'Reject Request';
        const modalBody = this.getActionModalBody(action, requestId, customerName);

        $('#actionModalLabel').text(`${modalTitle} - ${customerName}`);
        $('#actionModalBody').html(modalBody);

        // Bind form submission
        $('#actionModalBody').off('submit', '#actionForm').on('submit', '#actionForm', (e) => {
            e.preventDefault();
            this.submitAction(e.target);
        });

        new bootstrap.Modal(document.getElementById('actionModal')).show();
    }

    getActionModalBody(action, requestId, customerName) {
        const isApprove = action === 'approve';
        const noteRequired = isApprove ? 'required' : '';
        const placeholder = isApprove
            ? 'Please provide approval notes...'
            : 'Optional rejection reason...';

        return `
            <form id="actionForm">
                <input type="hidden" name="requestId" value="${requestId}" />
                <input type="hidden" name="action" value="${action}" />
                
                <div class="mb-3">
                    <label for="actionNote" class="form-label">
                        ${isApprove ? 'Approval Notes *' : 'Rejection Reason'}
                    </label>
                    <textarea id="actionNote" name="note" class="form-control" rows="4" 
                        ${noteRequired} placeholder="${placeholder}"></textarea>
                    ${isApprove ? '<div class="form-text text-warning">Approval notes are required</div>' : ''}
                </div>

                <div class="alert alert-info">
                    <i class="mdi mdi-information-outline me-2"></i>
                    You are about to <strong>${action}</strong> the cheque book request for <strong>${customerName}</strong>.
                </div>

                <div class="text-end">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-${isApprove ? 'success' : 'danger'}">
                        ${isApprove ? 'Approve Request' : 'Reject Request'}
                    </button>
                </div>
            </form>
        `;
    }

    submitAction(form) {
        const formData = $(form).serializeArray();
        const payload = {};

        formData.forEach(field => {
            payload[field.name] = field.value;
        });

        // Validation for approve action
        if (payload.action === 'approve' && !payload.note.trim()) {
            alert('Approval notes are required');
            return false;
        }

        const submitBtn = $(form).find('button[type="submit"]');
        const originalText = submitBtn.html();
        submitBtn.prop('disabled', true).html('<i class="mdi mdi-loading mdi-spin"></i> Processing...');

        $.ajax({
            url: '/ChequeRequest/TakeAction',
            type: 'POST',
            data: payload,
            success: (response) => {
                if (response.success) {
                    this.dataTable.ajax.reload();
                    $('#actionModal').modal('hide');
                    appalert(response.message, 1, 1);
                } else {
                    appalert(response.message, 2, 1);
                }
            },
            error: (xhr) => {
                appalert('Error processing action', 4, 1);
            },
            complete: () => {
                submitBtn.prop('disabled', false).html(originalText);
            }
        });

        return false;
    }

    refresh() {
        if (this.dataTable) {
            this.dataTable.ajax.reload();
        }
    }
}

// Initialize when document is ready
$(document).ready(function () {
    window.dataTableManager = new DataTableManager();
});