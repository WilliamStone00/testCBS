function initializeDataTable(ajaxUrl) {
    return $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
        "ajax": {
            "url": ajaxUrl,
            "type": "POST",
            "datatype": "json"
        },
        "columns": [
            { "data": "customerName", "name": "customerName", "autoWidth": true },
            { "data": "customerCode", "name": "customerCode", "autoWidth": true },
            { "data": "phone", "name": "phone", "autoWidth": true },
            { "data": "customerId", "name": "customerId", "autoWidth": true },
            { "data": "BranchName", "name": "BranchName", "autoWidth": true },
            {
                "data": "accountNumber",
                "orderable": false,
                "render": function (data) {
                    return "<a href='/Operation/AccountDetails?KEY=" + data + "' target='_blank'>Operations</a>";
                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": false, "orderable": true, "width": "30%" },
            { "targets": 1, "searchable": false, "orderable": true, "width": "8%" },
            { "targets": 2, "searchable": false, "orderable": true, "width": "10%" },
            { "targets": 3, "searchable": false, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": false, "orderable": true, "width": "28%" },
            { "targets": 5, "searchable": false, "orderable": true, "width": "10%" }
        ],
        "order": [[0, "asc"]],
        "bFilter": false, // Disable the search bar
    });
}

// Function to load accounts based on the specified path
function LoadAccounts(path) {
    var title = (path == "local") ? "LOCAL MEMBER'S ACCOUNT" : "ALL NETWORK MEMBER'S ACCOUNT";
    $("#titleaccount").html(title);
    initializeDataTable("/Saving/LoadData?Path=" + path);
}

// Function to search accounts based on the specified criteria
function SearchAccounts(search) {
    initializeDataTable("/Saving/LoadDataSearch?Search=" + search);
}

// Function to perform manual search
function manualSearch() {
    var searchText = $("#manualSearchInput").val();
    SearchAccounts(searchText);
}





//function LoadAccounts(path) {
//    if (path == "local") {
//        $("#titleaccount").html("LOCAL MEMBER'S ACCOUNT");
//    } else {
//        $("#titleaccount").html("ALL NETWORK MEMBER'S ACCOUNT");
//    }
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "info": true,
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
//        "ajax": {
//            "url": "/Saving/LoadData?Path=" + path,
//            "type": "POST",
//            "datatype": "json"
//        },
//        "columns": [
//            { "data": "customerName", "name": "customerName", "autoWidth": true },
//            { "data": "customerCode", "name": "customerCode", "autoWidth": true },
//            { "data": "phone", "name": "phone", "autoWidth": true },
//            { "data": "customerId", "name": "customerId", "autoWidth": true },
//            { "data": "BranchName", "name": "BranchName", "autoWidth": true },
//            {
//                "data": "customerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Operation/AccountDetails?KEY=" + data + "' target='_blank'>Operations</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": false, "orderable": true, "width": "30%" },
//            { "targets": 1, "searchable": false, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": false, "orderable": true, "width": "10%" },
//            { "targets": 3, "searchable": false, "orderable": true, "width": "15%" },
//            { "targets": 4, "searchable": false, "orderable": true, "width": "28%" },
//            { "targets": 5, "searchable": false, "orderable": true, "width": "10%" }
//        ],
//        "order": [[0, "asc"]],
//        "bFilter": false, // Disable the search bar
//    });
//}

//function SearchAccounts(search) {
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "info": true,
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
//        "ajax": {
//            "url": "/Saving/LoadDataSearch?Search=" + search,
//            "type": "POST",
//            "datatype": "json"
//        },
//        "columns": [
//            { "data": "customerName", "name": "customerName", "autoWidth": true },
//            { "data": "customerCode", "name": "customerCode", "autoWidth": true },
//            { "data": "phone", "name": "phone", "autoWidth": true },
//            { "data": "customerId", "name": "customerId", "autoWidth": true },
//            { "data": "BranchName", "name": "BranchName", "autoWidth": true },
//            {
//                "data": "customerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Operation/AccountDetails?KEY=" + data + "' target='_blank'>Operations</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": false, "orderable": true, "width": "30%" },
//            { "targets": 1, "searchable": false, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": false, "orderable": true, "width": "10%" },
//            { "targets": 3, "searchable": false, "orderable": true, "width": "15%" },
//            { "targets": 4, "searchable": false, "orderable": true, "width": "28%" },
//            { "targets": 5, "searchable": false, "orderable": true, "width": "10%" }
//        ],
//        "order": [[0, "asc"]],
//        "bFilter": false, // Disable the search bar
//    });
//}

//function manualSearch() {
//    // Implement manual search functionality here
//    var searchText = $("#manualSearchInput").val();
//    SearchAccounts(searchText);
//    // Use searchText to filter the data in your table or perform custom search logic
//}