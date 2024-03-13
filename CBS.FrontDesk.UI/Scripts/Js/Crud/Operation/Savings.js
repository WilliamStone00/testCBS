
$(document).ready(function () {

    LoadAccounts();
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

});

function LoadAccounts() {

    $("#myDataTable").DataTable({
        "destroy": true,
        "processing": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
        "filter": true,
        "ajax": {
            "url": "/Saving/LoadData",
            "type": "POST",
            "datatype": "json"
        },

        "columns": [
            { "data": "customerName", "name": "customerName", "autoWidth": true },
            { "data": "productName", "name": "productName", "autoWidth": true },
            { "data": "profileType", "name": "profileType", "autoWidth": true },
            { "data": "accountNumber", "name": "accountNumber", "autoWidth": true },
            { "data": "customerId", "name": "customerId", "autoWidth": true },
            { "data": "status", "name": "status", "autoWidth": true },
            {
                "data": "accountNumber", "orderable": "false", "render": function (data) {
                    //return "<a href='/Operation/AccountDetails?KEY=" + data + "'target='_blanck'" + data + " detail'> Detail</a>";
                    return "<a href='/Operation/AccountDetails?KEY=" + data + "' target='_blank'>Operations</a>";

                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "31%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "18%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 6, "searchable": true, "orderable": true, "width": "10%" }


        ],
        "order": [[0, "asc"]]
    });



}


//function DownloadLoans(path) {
//    var datefrom = $("#mdatefromexport").val();
//    var dateto = $("#mdatetoexport").val()
//    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
//    DownloadFile(url);

//}



