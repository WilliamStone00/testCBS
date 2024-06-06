
$(document).ready(function () {
    LoadUsers("All");
    /*LoadDataGen('Individual', 'myDataTable', '_IndividualData', 0, 'datalistingview', null, 'all')*/
});


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}



function manualSearch() {
    LoadUsers($('#manualSearchInput').val())
}



function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Transactions", "InitializeData");
}

//function LoadUsers(search) {
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "processing": false, // Display processing indicator during AJAX request
//        "paging": true, // Enable pagination
//        "info": true, // Enable table information display
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
//        "searching": false, // Enable search bar
//        "ajax": {
//            "url": "/Individual/LoadData",
//            "type": "GET",
//            "dataType": "json",
//            "data": function (d) {
//                // Pass additional parameters to the server
//                d.searchCriteria = search;
//            },
//            "dataSrc": function (json) {
//                // Set total records count from the server response
//                return json.data;
//            }
//        },
//        "columns": [
//            { "data": "name", "name": "userName", "autoWidth": true },
//            { "data": "CustomerId", "name": "email", "autoWidth": true },
//            { "data": "Phone", "name": "name", "autoWidth": true },
//            { "data": "branch", "name": "phoneNumber", "autoWidth": true },
//            { "data": "MembershipApprovalStatus", "name": "MembershipApprovalStatus", "autoWidth": true },
//            {
//                "data": "CustomerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Individual/CustomerProfile?KEY=" + data + "' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'>Profile</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
//            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
//            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },

//        ],
//        "order": [[0, "asc"]],
//        "orderFixed": [[0, "asc"]],
//        "createdRow": function (row, data, dataIndex) {
//            // Add a badge based on the membership approval status
//            var approvalStatus = data.MembershipApprovalStatus;
//            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
//            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
//            $('td:eq(5)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
//        }
//    });
//}

function LoadUsers(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/Individual/LoadData",
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
            { "data": "name", "name": "firstName", "autoWidth": true },
            { "data": "CustomerId", "name": "customerId", "autoWidth": true },
            { "data": "Phone", "name": "phone", "autoWidth": true },
            { "data": "branch", "name": "branch", "autoWidth": true },
            { "data": "MembershipApprovalStatus", "name": "membershipApprovalStatus", "autoWidth": true },
            {
                "data": "CustomerId", "orderable": false, "render": function (data)
                {
                    return `<a href='/Individual/CustomerProfile?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View ${data} detail'>Profile</a>`;
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
        "orderFixed": [[0, "asc"]],
        "createdRow": function (row, data, dataIndex) {
            var approvalStatus = data.MembershipApprovalStatus;
            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
            $('td:eq(4)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
        }
    });
}


//function LoadUsers(searchObj) {
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "info": true,
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
//        "searching": false, // Hide the search bar
//        "ajax": {
//            "url": "/Individual/LoadData?searchCriteria=" + searchObj,
//            "type": "GET",
//            "datatype": "json"
//        },
//        "columns": [
//            { "data": "name", "name": "FirstName", "autoWidth": true },
//            { "data": "CustomerId", "name": "CustomerId", "autoWidth": true },
//            { "data": "Phone", "name": "Phone", "autoWidth": true },
//            { "data": "branch", "name": "Branch", "autoWidth": true },
//            { "data": "MembershipApprovalStatus", "name": "MembershipApprovalStatus", "autoWidth": true },
//            {
//                "data": "CustomerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Individual/CustomerProfile?KEY=" + data + "' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'>Profile</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
//            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
//            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },
//        ],
//        "order": [[0, "asc"]],
//        "orderFixed": [[0, "asc"]],
//        "createdRow": function (row, data, dataIndex) {
//            // Add a badge based on the membership approval status
//            var approvalStatus = data.MembershipApprovalStatus;
//            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
//            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
//            $('td:eq(4)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
//        }
//    });
//}



function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



