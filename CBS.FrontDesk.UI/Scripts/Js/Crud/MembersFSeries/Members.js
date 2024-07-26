
$(document).ready(function () {
    //LoadMembers("All");
});

function GetMembersData(Key, partialView, divToloadPV, path) {

    AddORUpdateGen(Key, divToloadPV, partialView, path, "MembersFSeries");
}

//function LoadMembers() {
//    LoadDataGen('MembersFSeries', 'myDataTable', '_MembersData', 0, 'datalistingview', null, 'all')

//}

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}


function manualSearch() {
    LoadMembers($('#manualSearchInput').val())
}

function LoadMembers(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/MembersFSeries/LoadData",
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
            { "data": "name", "name": "FirstName", "autoWidth": true },
            { "data": "CustomerId", "name": "CustomerId", "autoWidth": true },
            { "data": "Phone", "name": "Phone", "autoWidth": true },
            { "data": "branch", "name": "Branch", "autoWidth": true },
            {
                "data": "CustomerId", "orderable": false, "render": function (data) {
                    return "<a href='/MembersFSeries/F2MembersGeneratSituation?KEY=" + data + "' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='Select to view F-Series reports'>F2-F5:Series</a>";
                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
        ],
        "order": [[0, "asc"]],
        "orderFixed": [[0, "asc"]],
    });
}
