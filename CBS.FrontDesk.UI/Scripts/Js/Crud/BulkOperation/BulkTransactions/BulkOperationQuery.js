
$(document).ready(function () {
    console.log("Is he Ready...");
    initFilterToggles();
    loadBulkOperationDataTable();
    bindFilterActions();
});

function resetFilterForm() {
    $('#userName, #firstName, #lastName, #phoneNumber, #email, #role, #createdFrom, #createdTo').val('');
    $('#isActive, #isBlocked, #isVerified').val('');
    $('#branchInput').val('').trigger('change');
    $('#byBranch, #byUser, #byDate').prop('checked', false);
    $('#branchFilterSection, #userFilterSection, #dateRangeSection').hide();
}

// ✅ Filter toggle control
function initFilterToggles() {
    console.log('Does #byBranch exist?', $('#byBranch').length); // Should log 1
    $('#byBranch').change(function () {
        $('#branchFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#branchInput').val('').trigger('change');
    });

    $('#byUser').change(function () {
        $('#userFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#userName, #firstName, #lastName').val('');
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
        if (!isDateRangeValid()) return;
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

//$(document).ready(function () {
//    // Initialize date pickers
//    $('#dateFrom, #dateTo').datepicker({
//        format: 'dd/mm/yyyy',
//        autoclose: true,
//        todayHighlight: true
//    });

//    // Initialize DataTable
//    //initializeLoanDataTable();

//    // Reinitialize DataTable on search button click
//    $('#searchButton').on('click', function () {
//        initializeBulkOperationDataTable();
//    });

//    // Initially hide the filter section
//    $('#filterContent').hide();

//    // Toggle the visibility of the filter section
//    $('#toggleFilterButton').on('click', function () {
//        $('#filterContent').slideToggle(300, function () {
//            if ($(this).is(':visible')) {
//                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-up"></i> Hide Filters');
//            } else {
//                $('#toggleFilterButton').html('<i class="mdi mdi-chevron-down"></i> Show Filters');
//            }
//        });
//    });
//});

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
                //if (json.data.length > 0) {
                //    $('#bulkOperationDataCard').fadeIn();
                //} else {
                //    $('#bulkOperationDataCard').fadeOut();
                //}
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
                render: (data) => data
            },
            {
                data: 'SimulationType',
                title: 'Simulation Type',
                render: (data) => `${data}%`
            },
            {
                data: 'TotalVolume',
                title: 'Total Volume',
                render: (data) => formatCurrency(data)
            },
            {
                data: 'TotalMembers',
                title: 'Total Members',
                render: (data) => data
            },
            {
                data: 'ApprovalStatus',
                title: 'Approval Status',
                render: (data) => data
            },
            {
                data: 'ApprovalValidationDescription',
                title: 'Approval Description',
                render: (data) => data
            },
            {
                data: 'ApprovalValidationDate',
                title: 'Approval Date',
                render: (data) => data
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
            <div class="text-center">
                <a href="/BulkOperation/Details?KEY=${id}" class="btn btn-sm btn-outline-primary" title="Details Simulation">
                    <i class="fas fa-user-cog me-1"></i> View Simulation Details
                </a>
            </div>`;
                }
            }
        ],
        //language: {
        //    emptyTable: "No Bulk Oprations available for the selected criteria."
        //},
        //dom: 'rtip'
    });
}


// Function to format currency in NGN
function formatCurrency(amount) {
    return new Intl.NumberFormat('en-NG', {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(amount);
}

function viewBulkOperationDetails(data) {

}