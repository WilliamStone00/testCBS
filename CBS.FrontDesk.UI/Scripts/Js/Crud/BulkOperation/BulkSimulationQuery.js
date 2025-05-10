
$(document).ready(function () {
    initFilterToggles();
    loadBulkOperationDataTable();
    bindFilterActions();
});

function resetFilterForm() {
    $(' #createdFrom, #createdTo').val('');
    $('#isActive, #isBlocked, #isVerified').val('');
    $('#branchInput').val('').trigger('change');
    $('#byBranch, #byUser, #byDate').prop('checked', false);
    $('#branchFilterSection, #statusFilterSection, #dateRangeSection').hide();
}

// ✅ Filter toggle control
function initFilterToggles() {
    console.log('Does #byBranch exist?', $('#byBranch').length); // Should log 1
    $('#byBranch').change(function () {
        $('#branchFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#branchInput').val('').trigger('change');
    });

    $('#byStatus').change(function () {
        $('#statusFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#statusInput').val('').trigger('change');
    });


    $('#byDate').change(function () {
        $('#dateRangeSection').slideToggle(this.checked);
        if (!this.checked) $('#createdFrom, #createdTo').val('');
    });
}

// 🎯 Bind button actions
function bindFilterActions() {
    const table = $('#myDataTable').DataTable();

    $('#applyFilterBtn').click(() => {
        //if (!isDateRangeValid()) return;
        table.ajax.reload();
    });

    $('#resetFilterBtn').click(() => {
        resetFilterForm();
        table.ajax.reload();
    });

    $('#exportBtn').click(() => {
        if (!isDateRangeValid()) return;
        const query = $.param(collectFilterData());
        window.location.href = `/UserManagement/DownloadUsers?${query}`;
    });
}

function getSearchParameters(d) {
    d.searchCriteria = $('#searchCriteria').val();
    d.dateFrom = $('#dateFrom').val();
    d.dateTo = $('#dateTo').val();
    d.status = $('#status').val();
    d.branchid = $('#branchInput').val();
}

function loadBulkOperationDataTable() {
    $('#myDataTable').DataTable({
        destroy: true,
        serverSide: true,
        order: [[0, 'desc']],
        ajax: {
            url: '/BulkOperation/LoadBulkOperationData',
            type: 'POST',
            data: getSearchParameters,
            dataSrc: function (json) {
                return json.data;
            }
        },
        columns: [
            {
                data: 'CreatedDate',
                title: 'Date',
                render: (data) => moment(data).format('DD/MM/YYYY HH:mm:ss')
            },
            {
                data: 'BranchName',
                title: 'Branch Name',
                render: (data) => data || 'N/A'
            },
            {
                data: 'SimulationType',
                title: 'Simulation Type',
                render: (data) => data || 'N/A'
            },
            {
                data: 'TotalMembers',
                title: 'Total Accounts',
                render: (data) => data
            },
            {
                data: 'TotalVolume',
                title: 'Total Volume',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'CreatedBy',
                title: 'Created By',
                render: (data) => data || 'N/A'
            },
            {
                data: 'ApprovalStatus',
                title: 'Approval Status',
                render: (data) => {
                    let badgeClass = '';
                    switch (data) {
                        case 'Pending':
                            badgeClass = 'bg-warning text-dark';
                            break;
                        case 'Reviewed':
                            badgeClass = 'bg-info text-white';
                            break;
                        case 'Approved':
                            badgeClass = 'bg-success text-white';
                            break;
                        default:
                            badgeClass = 'bg-secondary text-white';
                    }
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            {
                data: 'ApprovalBy',
                title: 'Approval By',
                render: (data) => data || 'N/A'
            },
            {
                data: 'ApprovalValidationDate',
                title: 'Approval Date',
                render: (data) => {
                    const csharpMinDate = new Date('0001-01-01T00:00:00');
                    if (!data || new Date(data).getTime() <= csharpMinDate.getTime()) {
                        return 'N/A';
                    }
                    return moment(data).format('DD/MM/YYYY HH:mm:ss');
                }
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
                        <div class="text-center">
                            <a href="/BulkOperation/Details?KEY=${id}" class="btn btn-sm btn-outline-primary" title="Detail Simulation">
                            <i class="fas fa-user-cog me-1"></i> View </a>
                        </div>`;
                }
            }
        ],
        language: {
            emptyTable: "No Bulk Oprations available for the selected criteria."
        },
        dom: 'rtip'
    });
}


// Function to format currency in NGN
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-NG', {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(amount);
}

//function viewBulkOperationDetails(data) {

//}
