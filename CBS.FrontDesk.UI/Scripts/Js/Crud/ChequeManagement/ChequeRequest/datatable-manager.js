// wwwroot/Scripts/Js/Crud/ChequeManagement/ChequeRequest/datatable-manager.js
class DataTableManager {
    constructor() {
        this.dataTable = null;
        this.init();
    }

    init() {
        this.bindEvents();
    }

    initializeDataTable() {
        if ($.fn.DataTable) {
            this.dataTable = $('#myDataTable').DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: '/ChequeRequest/LoadRequestsForDataTable',
                    type: 'POST',
                    data: (d) => this.buildQuery(d)
                },
                columns: [
                    { data: 'customerName', name: 'customerName' },
                    { data: 'categoryName', name: 'categoryName' },
                    {
                        data: 'requestDate',
                        name: 'requestDate',
                        render: (data) => data ? new Date(data).toLocaleDateString() : 'N/A'
                    },
                    {
                        data: 'status',
                        name: 'status',
                        render: (data) => this.getStatusBadge(data)
                    },
                    {
                        data: null,
                        orderable: false,
                        className: 'text-center',
                        render: (data, type, row) => this.renderActionDropdown(row)
                    }
                ],
                language: {
                    emptyTable: "No cheque requests found",
                    zeroRecords: "No matching records found"
                }
            });
        }
    }

    buildQuery(dataTableParams) {
        return {
            query: {
                Options: {
                    draw: dataTableParams.draw,
                    start: dataTableParams.start,
                    pageSize: dataTableParams.length,
                    sortColumnName: dataTableParams.columns[dataTableParams.order[0]?.column]?.data,
                    sortDirection: dataTableParams.order[0]?.dir || 'asc'
                },
                CustomerFilter: $('#customerFilter')?.val() || '',
                CategoryFilter: $('#categoryFilter')?.val() || '',
                StatusFilter: $('#statusFilter')?.val() || ''
            }
        };
    }

    bindEvents() {
        // Action form submission
        $(document).on('submit', '#actionForm', (e) => this.submitAction(e));
    }

    getStatusBadge(status) {
        const statusMap = {
            'pending': { class: 'bg-warning text-dark', text: 'Pending' },
            'approved': { class: 'bg-success', text: 'Approved' },
            'rejected': { class: 'bg-danger', text: 'Rejected' },
            'delivered': { class: 'bg-info', text: 'Delivered' },
            'review': { class: 'bg-secondary', text: 'Under Review' }
        };

        const statusInfo = statusMap[status?.toLowerCase()] || { class: 'bg-secondary', text: status };
        return `<span class="badge ${statusInfo.class}">${statusInfo.text}</span>`;
    }

    renderActionDropdown(row) {
        // Determine which actions are available based on status
        const status = (row.status || '').toLowerCase();
        let actionsHtml = '';

        // Details action - always available
        actionsHtml += `
            <li>
                <a class="dropdown-item" href="#" onclick="dataTableManager.showDetails('${row.Id}')">
                    <i class="mdi mdi-eye-outline me-2 text-info"></i>Details
                </a>
            </li>
        `;

        // Edit action - only for pending requests
        if (status === 'pending') {
            actionsHtml += `
                <li>
                    <a class="dropdown-item" href="#" onclick="dataTableManager.editRequest('${row.Id}')">
                        <i class="mdi mdi-pencil-outline me-2 text-primary"></i>Edit
                    </a>
                </li>
                <li><hr class="dropdown-divider"></li>
            `;
        }

        // Status actions based on current status
        if (status === 'pending') {
            actionsHtml += `
                <li>
                    <a class="dropdown-item text-warning" href="#" onclick="dataTableManager.showActionModal('review', '${row.Id}', '${row.customerName}')">
                        <i class="mdi mdi-clock-outline me-2"></i>Mark for Review
                    </a>
                </li>
                <li>
                    <a class="dropdown-item text-success" href="#" onclick="dataTableManager.showActionModal('approve', '${row.Id}', '${row.customerName}')">
                        <i class="mdi mdi-check-circle-outline me-2"></i>Approve
                    </a>
                </li>
                <li>
                    <a class="dropdown-item text-danger" href="#" onclick="dataTableManager.showActionModal('reject', '${row.Id}', '${row.customerName}')">
                        <i class="mdi mdi-close-circle-outline me-2"></i>Reject
                    </a>
                </li>
            `;
        } else if (status === 'approved') {
            actionsHtml += `
                <li>
                    <a class="dropdown-item text-info" href="#" onclick="dataTableManager.showActionModal('deliver', '${row.Id}', '${row.customerName}')">
                        <i class="mdi mdi-truck-check-outline me-2"></i>Mark as Delivered
                    </a>
                </li>
            `;
        }

        return `
            <div class="dropdown">
                <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" 
                        data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="mdi mdi-cog"></i> Actions
                </button>
                <ul class="dropdown-menu">
                    ${actionsHtml}
                </ul>
            </div>
        `;
    }

    showDetails(requestId) {
        $.get('/ChequeRequest/InitializeData', {
            KEY: requestId,
            partialView: '_RequestDetails',
            path: 'get'
        }, (html) => {
            $('#detailsModalBody').html(html);
            $('#detailsModal').modal('show');
        });
    }

    editRequest(requestId) {
        // Load the form in edit mode
        if (window.formManager) {
            window.formManager.setEditMode(requestId);
        }
    }

    showActionModal(action, requestId, customerName) {
        $('#actionType').val(action);
        $('#actionRequestId').val(requestId);

        // Set appropriate modal title based on action
        const actionTitles = {
            'approve': 'Approve Request',
            'reject': 'Reject Request',
            'review': 'Mark for Review',
            'deliver': 'Mark as Delivered'
        };

        $('#actionModalLabel').text(`${actionTitles[action] || 'Take Action'} - ${customerName}`);
        $('#actionNote').val('');
        $('#actionModal').modal('show');
    }

    async submitAction(e) {
        e.preventDefault();

        const formData = new FormData(e.target);
        const actionData = {
            requestId: formData.get('requestId'),
            action: formData.get('action'),
            note: formData.get('note')
        };

        try {
            const response = await $.ajax({
                url: '/ChequeRequest/TakeAction',
                type: 'POST',
                data: actionData
            });

            if (response.success) {
                this.showAlert('Action completed successfully', 'success');
                $('#actionModal').modal('hide');
                this.refresh();
            } else {
                this.showAlert(response.message || 'Action failed', 'error');
            }
        } catch (error) {
            this.showAlert('Error submitting action', 'error');
        }
    }

    refresh() {
        if (this.dataTable) {
            this.dataTable.ajax.reload();
        }
    }

    showAlert(message, type) {
        const alertType = type === 'error' ? 4 : type === 'warning' ? 2 : 1;
        if (typeof appalert === 'function') {
            appalert(message, alertType, 1);
        } else {
            alert(message);
        }
    }
}