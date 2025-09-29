class DataTableManager {
    constructor() {
        this.dataTable = null;
        this.init();
    }

    init() {
        this.initializeDataTable();
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
                        render: (data) => data ? new Date(data).toLocaleDateString() : ''
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
                ]
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
        $('#actionForm').on('submit', (e) => this.submitAction(e));

        // Apply filters if they exist
        $('#applyListFilterBtn')?.on('click', () => this.refresh());
    }

    getStatusBadge(status) {
        const statusMap = {
            'pending': 'bg-warning text-dark',
            'approved': 'bg-success',
            'rejected': 'bg-danger',
            'delivered': 'bg-info',
            'review': 'bg-secondary'
        };
        const cssClass = statusMap[status?.toLowerCase()] || 'bg-secondary';
        return `<span class="badge ${cssClass}">${status}</span>`;
    }

    renderActionDropdown(row) {
        return `
            <div class="dropdown">
                <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" 
                        data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="mdi mdi-cog"></i> Actions
                </button>
                <ul class="dropdown-menu">
                    <li>
                        <a class="dropdown-item" href="#" onclick="dataTableManager.showDetails('${row.Id}')">
                            <i class="mdi mdi-eye-outline me-2 text-info"></i>Details
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item" href="#" onclick="dataTableManager.editRequest('${row.Id}')">
                            <i class="mdi mdi-pencil-outline me-2 text-primary"></i>Edit
                        </a>
                    </li>
                    <li><hr class="dropdown-divider"></li>
                    <li>
                        <a class="dropdown-item text-warning" href="#" onclick="dataTableManager.showActionModal('review', '${row.Id}', '${row.customerName}')">
                            <i class="mdi mdi-clock-outline me-2"></i>Review
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
        $.get('/ChequeRequest/InitializeData', {
            KEY: requestId,
            partialView: '_ChequeRequestForm',
            path: 'get'
        }, (html) => {
            $('#requestFormSection').html(html).show();
            $('#searchSection').hide();
            if (window.formManager) {
                window.formManager.setEditMode(requestId);
            }
        });
    }

    showActionModal(action, requestId, customerName) {
        $('#actionType').val(action);
        $('#actionRequestId').val(requestId);
        $('#actionModalLabel').text(`${action.charAt(0).toUpperCase() + action.slice(1)} Request - ${customerName}`);
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
                this.showAlert(response.message, 'success');
                $('#actionModal').modal('hide');
                this.refresh();
            } else {
                this.showAlert(response.message, 'error');
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