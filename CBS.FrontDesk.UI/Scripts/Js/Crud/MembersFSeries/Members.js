



// Load Members into DataTable
function loadMemberData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/MembersFSeries/LoadMembersData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectMemberExportParams();

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

                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'CreateDate',
                name: 'CreateDate',
                render: function (data) {
                    return moment(data).format('DD/MM/YYYY');
                }
            },
            { data: 'FullName', name: 'FullName' },
            { data: 'CustomerId', name: 'CustomerId' },
            { data: 'Phone', name: 'Phone' },
            {
                data: 'CustomerType',
                name: 'CustomerType',
                render: function (data, type, row) {
                    const label = data || '';
                    const isApproved = row.MembershipApprovalStatus === 'Approved';
                    const badgeClass = isApproved ? 'bg-primary' : 'bg-warning text-dark';
                    return `<span class="badge ${badgeClass}">${label}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<a href='/MembersFSeries/F2MembersGeneratSituation?KEY=${row.CustomerId}' target='_blank' 
                    class='btn btn-sm btn-outline-dark' title='Check account situation'>
                    <i class="mdi mdi-account-cash-outline me-1 text-primary"></i> Account Situation
                </a>`;
                }
            }

        ]
    });
}


function exportMemberData() {
    const filters = collectMemberExportParams();
    const params = new URLSearchParams(filters).toString();
    window.location.href = `/Individual/DownloadMembers?${params}`;
}

function collectMemberExportParams() {
    return {
        CustomerId: $('#customerId').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        BranchId: $('#branchId').val(),
        Gender: $('#gender').val(),
        MaritalStatus: $('#maritalStatus').val(),
        WorkingStatus: $('#workingStatus').val(),
        MembershipApprovalStatus: $('#membershipApprovalStatus').val(),
        LegalForm: $('#legalForm').val(),
        CustomerType: $('#customerType').val(),
        AgeCategoryStatus: $('#ageCategoryStatus').val(),
        DateOfBirthFrom: $('#dobFrom').val(),
        DateOfBirthTo: $('#dobTo').val(),
        CreatedFrom: $('#createdFrom').val(),
        CreatedTo: $('#createdTo').val(),
        ShowAll: false,
        options: {}
    };
}

function resetMemberFilters() {
    $('#memberFilterForm').trigger('reset');
    $('.select2').val('').trigger('change');
    $('#demographicFilters, #approvalFilters, #dateFilters').addClass('d-none');
    loadMemberData();
}

$(document).ready(function () {
    loadMemberData();

    $('#applyFilterBtn').on('click', function (e) {
        e.preventDefault();
        loadMemberData();
    });

    $('#resetFilterBtn').on('click', function (e) {
        e.preventDefault();
        resetMemberFilters();
    });

    $('#exportBtn').on('click', function (e) {
        e.preventDefault();
        exportMemberData();
    });
});





