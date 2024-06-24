
$(document).ready(function () {

    LoadLoans("All");
});

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}


function manualSearch() {
    LoadLoans($('#manualSearchInput').val())
}
function LoadLoans(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/Loan/LoadData",
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
            { "data": "DisbursementDate", "name": "DisbursementDate", "autoWidth": true },
            { "data": "CustomerId", "name": "CustomerId", "autoWidth": true },
            { "data": "Principal", "name": "Principal", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "InterestRate", "name": "InterestRate", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "AccrualInterest", "name": "AccrualInterest", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Paid", "name": "Paid", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Balance", "name": "Balance", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            {
                "data": "Id", "orderable": false, "render": function (data) {
                    return `<a href='/Loan/Details?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View loan details of ${data}.'>Details</a>`;
                }
            }
        ],

        //"order": [[0, "desc"]],
        //"orderFixed": [[0, "desc"]]
    });
}
