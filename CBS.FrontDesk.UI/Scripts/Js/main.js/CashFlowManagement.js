$(document).ready(function () {

    LoadCashReplenishmentDataDT("ListOfCashReplenishmentData")
});
function refreshPage() {
    // Refresh the current page
    window.location.reload();
    console.log("Page Refreshed");
    // Redirect to the home page
    window.location.href = "/CashFlowManagement/GetAllCashReplenimentRequestData";
}
function refreshPageHome() {
    // Refresh the current page
    window.location.reload();
    console.log("Page Refreshed");
    // Redirect to the home page
    window.location.href = "/home";
}

function LoadCashReplenishmentDataDT(tableID) {

     
        var T = '#' + tableID;
        var dataThumbView = $(T).DataTable({
            responsive: false,
        "columns": [
            //{ "data": "ReferenceId", "name": "ReferenceId", "autoWidth": true },
            //{ "data": "Amount", "name": "Amount", "autoWidth": true },
            //{ "data": "RequestMessage", "name": "RequestMessage", "autoWidth": true },
            //{ "data": "IssuedBy", "name": "IssuedBy", "autoWidth": true },
            //{ "data": "IssuedDate", "name": "IssuedDate", "autoWidth": true },
            //{
            //    "data": "Id", "orderable": "false", "render": function (data) {
            //        return "<a href='/CashFlowManagement/GetCashReplenimentRequest?KEY=" + data + " class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Click to Approve</a>";
            //    }
            //}
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "15%" },

        ],
            
            oLanguage: {
                sLengthMenu: "_MENU_",
                sSearch: ""
            },
            aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


            order: [[0, "asc"]],
            bInfo: true,
            pageLength: 10

        });
    }





