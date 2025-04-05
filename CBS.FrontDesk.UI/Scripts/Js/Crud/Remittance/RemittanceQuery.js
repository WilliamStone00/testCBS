$(document).ready(function () {
    // Initial load state
    $('#remittanceDataCard').hide();

    // Search
    $('#searchButton').click(function () {
        $('#remittanceDataCard').show();
        loadRemittanceDataTable();
    });

    // Reset
    $('#resetButton').click(function () {
        resetRemittanceFilters();
    });

    // Export
    $('#exportButton').click(function () {
        exportRemittanceData();
    });

    // Toggle filter sections based on checkboxes
    $('#byDate').change(function () {
        $('#dateRangeSection').toggle(this.checked);
    });

    $('#byBranch').change(function () {
        $('#branchFilterSection').toggle(this.checked);
    });

    $('#byStatus').change(function () {
        $('#statusFilterSection').toggle(this.checked);
    });
});

function collectRemittanceFilters() {
    return {
        DateFrom: $('#dateFrom').val(),
        DateTo: $('#dateTo').val(),
        ByDateRange: $('#byDate').is(':checked'),
        Status: $('#remittanceStatus').val(),
        QueryParameter: $('#queryParameter').val(),
        QueryValue: $('#queryValue').val(),
        BranchId: $('#branchSelection').val()
    };
}

function loadRemittanceDataTable() {
    $('#remittanceDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        responsive: true,
        order: [[0, 'desc']], // Sort by InitiationDate
        ajax: {
            url: '/Remittance/LoadRemittanceData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectRemittanceFilters();

                filters.DataTableOptions = {
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    skip: d.start,
                    pageSize: d.length,
                    sortColumnName: d.columns[d.order[0].column].data,
                    sortColumnDirection: d.order[0].dir
                };

                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'InitiationDate',
                render: d => d ? moment(d).format('DD/MM/YYYY') : '—'
            },
            { data: 'SenderName' },
            { data: 'ReceiverName' },
            { data: 'TransactionReference' },
            { data: 'SourceBranchName' },
            { data: 'ReceivingBranchName' },
            {
                data: 'Amount',
                render: d => `XAF ${parseFloat(d).toLocaleString(undefined, { minimumFractionDigits: 2 })}`
            },
            {
                data: 'Status',
                render: function (status) {
                    let badgeClass = 'secondary';
                    switch (status?.toLowerCase()) {
                        case 'approved': badgeClass = 'success'; break;
                        case 'pending': badgeClass = 'warning'; break;
                        case 'rejected': badgeClass = 'danger'; break;
                        case 'paid': badgeClass = 'primary'; break;
                        case 'withdrawn': badgeClass = 'dark'; break;
                    }
                    return `<span class="badge bg-${badgeClass}">${status}</span>`;
                }
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
                        <div class="text-center">
                            <a href="/Remittance/Details/${id}" class="btn btn-sm btn-outline-info" title="View Details">
                                <i class="mdi mdi-eye me-1"></i> View
                            </a>
                        </div>`;
                }
            }
        ]
    });
}

function exportRemittanceData() {
    const filters = collectRemittanceFilters();
    const params = $.param(filters);
    window.location.href = `/Remittance/Download?${params}`;
}

function resetRemittanceFilters() {
    // Clear values
    $('#dateFrom, #dateTo, #queryValue').val('');
    $('#remittanceStatus, #queryParameter, #branchSelection').val('all');

    // Reset toggles
    $('#byDate, #byBranch, #byStatus').prop('checked', false);
    $('#dateRangeSection, #branchFilterSection, #statusFilterSection').hide();

    // Reset card and table
    $('#remittanceDataCard').hide();

    const table = $('#remittanceDataTable').DataTable();
    table.clear().draw();
    table.destroy();
}
