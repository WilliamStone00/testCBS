

function manualSearch() {
    LoadCMoneyMembers($('#manualSearchInput').val())
}

function LoadCMoneyMembers(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/CMoneyMembership/LoadData",
            "type": "GET",
            "data": {
                "searchCriteria": search
            },
            "dataSrc": function (json) {
                if (json.error) {
                    console.error(json.error);
                    return [];
                }
                return json.data;
            }
        },
        "columns": [
            { "data": "Name", "name": "name", "autoWidth": true },
            { "data": "CustomerId", "name": "customerId", "autoWidth": true },
            { "data": "PhoneNumber", "name": "phone", "autoWidth": true },
            { "data": "BranchName", "name": "branchName", "autoWidth": true },
            { "data": "LoginId", "name": "loginId", "autoWidth": true },
            {
                "data": "LoginId", "orderable": false, "render": function (data)
                {
                    return `<a href='/CMoneyMembership/ManageProfile?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View ${data} detail'>Profile</a>`;
                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },
        ],
        "order": [[0, "asc"]],
        "orderFixed": [[0, "asc"]]
    });
}


