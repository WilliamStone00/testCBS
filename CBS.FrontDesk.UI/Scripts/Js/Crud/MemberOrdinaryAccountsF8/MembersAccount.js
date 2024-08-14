//$(document).ready(function () {
//    LoadAccounts("All");
//});

function DownloadAccounts(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val();
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);
}

function manualSearch() {
    
    LoadAccounts($('#manualSearchInput').val(), $('#branchInput').val());
}

function loadByBbranch(id) {

    LoadAccounts($('#manualSearchInput').val(), id);
}

function LoadAccounts(search,branchid) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/MemberOrdinaryAccountsF8/LoadData",
            "type": "POST",
            "data": function (d) {
                d.searchCriteria = search;
                d.branchid = branchid;
            },
            "dataSrc": function (json) {
                if (json.error) {
                    console.error(json.error);
                    return [];
                }
                console.log("Data received:", json.data); // Log the data received
                return json.data;
            }
        },
        "columns": [
            { "data": "MemberName", "name": "MemberName", "autoWidth": true },
            { "data": "MemberReference", "name": "MemberReference", "autoWidth": true },
            { "data": "BranchCode", "name": "BranchCode", "autoWidth": true },
            { "data": "Saving", "name": "Saving", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Deposit", "name": "Deposit", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "PreferenceShare", "name": "PreferenceShare", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Share", "name": "Share", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Loan", "name": "Loan", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) }
        ]
    });

    // Initialize tooltips
    $(function () { $('[data-toggle="tooltip"]').tooltip(); });
}
