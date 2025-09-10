
$(document).on('hidden.bs.modal', '#tempMembersModal', function () {
    $('#modalLoader').addClass('d-none');   // Modal-specific loader
    $('#loading').hide();                   // Global loader
    console.log('✅ Modal closed — loaders and states cleared.');
});

$(document).ready(function () {
    // Apply filters
    $(document).on('click', '#applyFilterBtn', function (e) {
        e.preventDefault();
        reloadMemberData();
    });

    // Reset filters
    $(document).on('click', '#resetFilterBtn', function () {
        $('#memberFilterForm')[0].reset();
    });

    // Load table on modal open
    $(document).on('shown.bs.modal', '#tempMembersModal', function () {
        $('#modalLoader').removeClass('d-none');
        loadMemberData();
    });
});

function openTempMembersModal() {
    $('#modalLoader').addClass('d-none');
    $('#tempMembersModal').modal('show');
}

function collectMemberExportParams() {
    return {
        CustomerId: $('#memberReferenceId').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        BranchId: $('#branchId').val(),
        Gender: $('#gender').val(),
        MaritalStatus: $('#maritalStatus').val(),
        WorkingStatus: $('#workingStatus').val(),
        MembershipApprovalStatus: $('#membershipApprovalStatus').val(),
        LegalForm: $('#legalForm').val(),
        CustomerType: 'PreRegistredMember',
        AgeCategoryStatus: $('#ageCategoryStatus').val(),
        DateOfBirthFrom: $('#dobFrom').val(),
        DateOfBirthTo: $('#dobTo').val(),
        CreatedFrom: $('#createdFrom').val(),
        CreatedTo: $('#createdTo').val(),
        ShowAll: false,
        options: {}
    };
}

function reloadMemberData() {
    $('#modalLoader').removeClass('d-none');
    loadMemberData();
}

function loadMemberData() {
    const $table = $('#tempMembersTable');
    const $loader = $('#modalLoader');

    if ($.fn.DataTable.isDataTable($table)) {
        $table.DataTable().clear().destroy();
    }

    const table = $table.DataTable({
        serverSide: true,
        processing: false,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/Individual/LoadMembersData',
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
            },
            dataSrc: function (json) {
                $loader.addClass('d-none');
                return Array.isArray(json.data) ? json.data : [];
            },
            error: function (xhr) {
                $loader.addClass('d-none');
                console.error('❌ DataTable error:', xhr.responseText);
                alert('Failed to load member data.');
            }
        },
        columns: [
            {
                data: 'CreateDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY') : '';
                }
            },
            { data: 'FullName' },
            { data: 'CustomerId' },
            { data: 'Phone' },
            {
                data: 'CustomerType',
                render: function (data, type, row) {
                    const badge = row.MembershipApprovalStatus === 'Approved'
                        ? 'bg-primary'
                        : 'bg-warning text-dark';
                    return `<span class="badge ${badge}">${data}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<button class="btn btn-sm btn-success" onclick="selectTempMember('${row.CustomerId}', '${row.FullName}')">
        <i class="fas fa-check-circle"></i> Select
    </button>`;
                }
            }
        ],
        initComplete: function () {
            setTimeout(() => {
                table.columns.adjust();
            }, 100);
        }
    });
}

function selectTempMember(customerId, fullName) {
    alert(`Selected: ${fullName} (${customerId})`);
    $('#tempMembersModal').modal('hide');

    // Show loader while loading onboarding summary
    $('#preMemberOnboardingContainer').html(`
    <div class="text-center p-3">
        <div class="spinner-border text-primary" role="status"></div>
        <p class="fw-bold text-primary mt-2">Loading onboarding summary...</p>
    </div>`);
    $('#manualSearchInput').val(customerId);
    GetMemberData(customerId, '_OperationDesk', 'datalistingview', 'newsubcription');
}
