


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}







function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Transactions", "InitializeData");
}
function LoadUsers() {
    LoadDataGen('Individual', 'myDataTable', '_IndividualData', 0, 'datalistingview', "KEY")
    //$("#myDataTable").DataTable({
    //    "destroy": true,
    //    "processing": true,
    //    "serverSide": true,
    //    "info": true,
    //    "stateSave": true,
    //    "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
    //    "filter": true,
    //    "ajax": {
    //        "url": "/Individual/LoadData",
    //        "type": "POST",
    //        "datatype": "json"
    //    },

    //    "columns": [
    //        { "data": "name", "name": "userName", "autoWidth": true },
    //        { "data": "phone", "name": "email", "autoWidth": true },
    //        { "data": "address", "name": "name", "autoWidth": true },
    //        { "data": "idNumber", "name": "phoneNumber", "autoWidth": true },
    //        { "data": "town", "name": "address", "autoWidth": true },
    //        { "data": "branch", "name": "strlastLoginDate", "autoWidth": true },
    //        { "data": "status", "name": "status", "autoWidth": true },
    //        {
    //            "data": "customerId", "orderable": "false", "render": function (data) {
    //                return "<a href='/Individual/CustomerProfile?KEY=" + data + "'target='_blanck' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Profile</a>";
    //            }
    //        }
    //    ],
    //    "columnDefs": [
    //        { "targets": 0, "searchable": true, "orderable": true, "width": "25%" },
    //        { "targets": 1, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
    //        { "targets": 3, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 4, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 5, "searchable": true, "orderable": true, "width": "15%" },
    //        { "targets": 6, "searchable": true, "orderable": true, "width": "5%" },
    //        { "targets": 7, "searchable": true, "orderable": true, "width": "10%" },

    //    ],
    //    "order": [[0, "asc"]]
    //});



}



// Load Members into DataTable
function loadMemberData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
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
                data: 'CustomerType', // important: this should match what you want to sort by
                name: 'CustomerType',
                render: function (data, type, row) {
                    const label = CustomerType || '';
                    const isApproved = row.MembershipApprovalStatus === 'Approved';
                    const badgeClass = isApproved ? 'bg-primary' : 'bg-warning text-dark';
                    return `<span class="badge ${badgeClass}">${label}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<a href='/Individual/CustomerProfile?KEY=${row.CustomerId}' target='_blank' class='btn btn-sm btn-info'>
                                <i class="mdi mdi-account-circle"></i> Profile
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





