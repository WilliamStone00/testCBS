$(document).ready(function () {
    //loadCMoneyMemberData();

    $('#applyFilterBtn').click(function () {
        loadCMoneyMemberData();
    });

    $('#resetFilterBtn').click(function () {
        resetFilters();
    });

    $('#exportBtn').click(function () {
        exportCMoneyMemberData();
    });

    $('#byBranch').on('change', function () {
        if ($(this).is(':checked')) {
            $('#branchFilterSection').slideDown();
        } else {
            $('#branchFilterSection').slideUp();
        }
    });

    $('#byUser').on('change', function () {
        if ($(this).is(':checked')) {
            $('#userFilterSection').slideDown();
        } else {
            $('#userFilterSection').slideUp();
        }
    });

    $('#byDate').on('change', function () {
        if ($(this).is(':checked')) {
            $('#dateRangeSection').slideDown();
        } else {
            $('#dateRangeSection').slideUp();
        }
    });

    // Trigger initial show/hide state
    $('#byBranch, #byUser, #byDate').trigger('change');
});

function loadCMoneyMemberData() {
    $('#myDataTable').DataTable({
        //processing: true,
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[5, 'desc']],
        ajax: {
            url: '/CMoneyMembership/LoadCMoneyMemberData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectExportParams();
                filters.dataTableOptions.draw = d.draw;
                filters.dataTableOptions.start = d.start;
                filters.dataTableOptions.length = d.length;
                filters.dataTableOptions.skip = d.start;
                filters.dataTableOptions.pageSize = d.length;
                filters.dataTableOptions.sortColumnName = d.columns[d.order[0].column].data;
                filters.dataTableOptions.sortColumnDirection = d.order[0].dir;
                return JSON.stringify(filters);
            }
        },
        columns: [
            { data: 'Name' },
            { data: 'CustomerId' },
            { data: 'PhoneNumber' },
            { data: 'ActivatedBy' },
            { data: 'BranchCode' },
            {
                data: 'ActivationDate',
                render: function (data) {
                    return moment(data).format('DD/MM/YYYY HH:mm');
                }
            },
            {
                data: 'IsActive',
                render: function (data) {
                    return data
                        ? '<span class="badge bg-success">Active</span>'
                        : '<span class="badge bg-danger">Deactivated</span>';
                }
            },
            { data: 'LoginId' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<a href='/CMoneyMembership/ManageProfile?KEY=${row.LoginId}' target='_blank' class='btn btn-sm btn-info'>
                            <i class="mdi mdi-eye"></i> View
                        </a>`;
                }
            }
        ]
    });
}

function exportCMoneyMemberData() {
    const startDate = $('#activationDateFrom').val();
    const endDate = $('#activationDateTo').val();
    //const lastPaymentStartDate = $('#lastPaymentStartDate').val();
    //const lastPaymentEndDate = $('#lastPaymentEndDate').val();
    const byDate = $('#byDate').is(':checked');
    const byBranch = $('#byBranch').is(':checked');
    const byUser = $('#byUser').is(':checked');
    const isActive = $('#activationStatus').val() === 'true';
    const isDeactivated = $('#activationStatus').val() === 'false';
    const branchId = $('#branchInput').val();
    const customerId = $('#customerId').val();
    const loginId = $('#loginId').val();
    const phoneNumber = $('#phoneNumber').val();
    const name = $('#name').val();
    const activatedBy = $('#activatedBy').val();
    const hasChangeDefaultPin = $('#hasChangedDefaultPin').val();
    const isSubscribed = $('#isSubscribed').val();
    const failedAttemptsMin = $('#failedAttemptsMin').val();
    const failedAttemptsMax = $('#failedAttemptsMax').val();

    const url = `/CMoneyMembership/DownloadCMoneyActivations?startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}&byDate=${byDate}&byBranch=${byBranch}&byUser=${byUser}&isActive=${isActive}&isDeactivated=${isDeactivated}&branchId=${encodeURIComponent(branchId)}&customerId=${encodeURIComponent(customerId)}&loginId=${encodeURIComponent(loginId)}&phoneNumber=${encodeURIComponent(phoneNumber)}&name=${encodeURIComponent(name)}&activatedBy=${encodeURIComponent(activatedBy)}&hasChangeDefaultPin=${encodeURIComponent(hasChangeDefaultPin)}&isSubscribed=${encodeURIComponent(isSubscribed)}&failedAttemptsMin=${encodeURIComponent(failedAttemptsMin)}&failedAttemptsMax=${encodeURIComponent(failedAttemptsMax)}`;

    // Trigger download
    window.location.href = url;
}


function collectExportParams() {
    return {
        startDate: convertToDate($('#activationDateFrom').val()),
        endDate: convertToDate($('#activationDateTo').val()),
        lastPaymentStartDate: convertToDate($('#lastPaymentStartDate').val()),
        lastPaymentEndDate: convertToDate($('#lastPaymentEndDate').val()),
        byDate: $('#byDate').is(':checked'),
        byBranch: $('#byBranch').is(':checked'),
        byUser: $('#byUser').is(':checked'),
        isActive: $('#activationStatus').val() === 'true',
        isDeactivated: $('#activationStatus').val() === 'false',
        branchId: $('#branchInput').val(), // ✅ updated from `parameterString` to `branchId`
        customerId: $('#customerId').val(),
        loginId: $('#loginId').val(),
        phoneNumber: $('#phoneNumber').val(),
        name: $('#name').val(),
        activatedBy: $('#activatedBy').val(),
        hasChangeDefaultPin: convertToBool($('#hasChangedDefaultPin').val()),
        isSubscribed: convertToBool($('#isSubscribed').val()),
        failedAttemptsMin: parseInt($('#failedAttemptsMin').val()) || null,
        failedAttemptsMax: parseInt($('#failedAttemptsMax').val()) || null,
        dataTableOptions: { searchValue: "" }
    };
}

function resetFilters() {
    // Clear text inputs and numbers
    $('#activationDateFrom').val('');
    $('#activationDateTo').val('');
    //$('#lastPaymentStartDate').val('');
    //$('#lastPaymentEndDate').val('');
    $('#customerId').val('');
    $('#loginId').val('');
    $('#phoneNumber').val('');
    $('#name').val('');
    $('#activatedBy').val('');
    $('#failedAttemptsMin').val('');
    $('#failedAttemptsMax').val('');

    // Reset all selects to default
    $('#activationStatus').val('');
    $('#hasChangedDefaultPin').val('');
    $('#isSubscribed').val('');
    $('#branchInput').val('');

    // If using select2:
    $('.select2').val('').trigger('change');

    // Uncheck checkboxes
    $('#byBranch').prop('checked', false);
    $('#byUser').prop('checked', false);
    $('#byDate').prop('checked', false);

    // Hide conditional sections
    $('#branchFilterSection').slideUp();
    $('#userFilterSection').slideUp();
    $('#dateRangeSection').slideUp();

    // Reload table with cleared filters
    loadCMoneyMemberData();
}


function convertToDate(dateStr) {
    if (dateStr) {
        const parts = dateStr.split('/');
        return `${parts[2]}-${parts[1]}-${parts[0]}T00:00:00`;
    }
    return null;
}

function convertToBool(value) {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return null;
}
