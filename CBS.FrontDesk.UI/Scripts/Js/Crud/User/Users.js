$(document).ready(function () {
    initFilterToggles();
    bindFilterActions();
});
function loadBulkOperationDataTable() {
    return $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        responsive: true,
        order: [[5, 'desc']],
        ajax: {
            url: '/UserManagement/LoadUsers',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectFilterData();
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
            { data: 'FullName' },
            { data: 'UserName' },
            { data: 'RoleName' },
            {
                data: 'IsActive',
                render: d => d
                    ? '<span class="badge bg-success">Active</span>'
                    : '<span class="badge bg-danger">Inactive</span>'
            },
            { data: 'PhoneNumber' },
            {
                data: 'LastLoginDate',
                render: d => d ? moment(d).format('DD/MM/YYYY HH:mm') : '—'
            },
            {
                data: 'Id',
                orderable: false,
                searchable: false,
                render: function (id) {
                    return `
    <div class="text-center">
        <a href="/UserManagement/UserProfile?KEY=${id}" target="_blank" rel="noopener noreferrer" class="btn btn-sm btn-outline-primary" title="Manage User">
            <i class="fas fa-user-cog me-1"></i> Profile
        </a>
    </div>`;

                }
            }
        ]
    });
}

function isDateRangeValid() {
    const startDate = $('#createdFrom').val();
    const endDate = $('#createdTo').val();

    if (startDate && endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);
        if (start > end) {
            alert("⚠️ 'Start Date' cannot be after 'End Date'.");
            return false;
        }
    }
    return true;
}
// 📦 Collect filter values into object
// 📥 Collect filter values
function collectFilterData() {
    return {
        UserName: $('#userName').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        PhoneNumber: $('#phoneNumber').val(),
        Email: $('#email').val(),
        BranchId: $('#branchInput').val(),
        Role: $('#role').val(),
        IsActive: $('#isActive').val(),
        IsVerified: $('#isVerified').val(),
        StartDate: $('#createdFrom').val(),
        EndDate: $('#createdTo').val()
    };
}

// 📥 Export to Excel
function downloadUsers() {
    const query = $.param(collectFilterData());
    window.location.href = `/UserManagement/DownloadUsers?${query}`;
}

// 🧹 Reset filter inputs
function resetFilterForm() {
    $('#userName, #firstName, #lastName, #phoneNumber, #email, #role, #createdFrom, #createdTo').val('');
    $('#isActive, #isVerified').val('');
    $('#branchInput').val('').trigger('change');
    $('#byBranch, #byUser, #byDate').prop('checked', false);
    $('#branchFilterSection, #userFilterSection, #dateRangeSection').hide();
}


// ✅ Filter toggle control
function initFilterToggles() {
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
let userTable = null;

function bindFilterActions() {
    $('#applyFilterBtn').click(() => {
        if (!isDateRangeValid()) return;

        if (!userTable) {
            // First time — initialize table
            userTable = loadBulkOperationDataTable();
        } else {
            // Already initialized — reload with new filters
            userTable.ajax.reload();
        }
    });

    $('#resetFilterBtn').click(() => {
        resetFilterForm();
        if (userTable) {
            userTable.ajax.reload();
        }
    });

    $('#exportBtn').click(() => {
        if (!isDateRangeValid()) return;
        const query = $.param(collectFilterData());
        window.location.href = `/UserManagement/DownloadUsers?${query}`;
    });
}

function UserDetail(id) {
    EditResetModal(id, 'modal', 'modalContent', 'UserManagement', 'InitializeData', '_UserDetails', 'userdetail', 'USER DETAIL', 'modalLabel')
}
// 🧠 Unified User Operation Trigger
function performUserAction(option, userId = null) {
    let formUrl = `/UserManagement/${option}`;
    if (userId) formUrl += `?KEY=${userId}`;

    showInPopup(formUrl, `Perform: ${option.replace(/([A-Z])/g, ' $1')}`);
}
