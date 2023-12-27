
$(document).ready(function () {

    LoadUsers();
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

});


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
    LoadDataGen('UserManagement', 'myDataTable', '_Data', 0, 'datalistingview')
    //$("#myDataTable").DataTable({
    //    "destroy": true,
    //    "processing": true,
    //    "serverSide": true,
    //    "info": true,
    //    "stateSave": true,
    //    "lengthMenu": [[10, 20, 100, 500, 1000, 2000, 5000, 10000], [10, 20, 100, 500, 1000, 2000, 5000, 10000]],
    //    "filter": true,
    //    "ajax": {
    //        "url": "/UserManagement/LoadData",
    //        "type": "POST",
    //        "datatype": "json"
    //    },
       
    //    "columns": [
    //        { "data": "userName", "name": "userName", "autoWidth": true },
    //        { "data": "name", "name": "name", "autoWidth": true },
    //        { "data": "phoneNumber", "name": "phoneNumber", "autoWidth": true },
    //        { "data": "strlastLoginDate", "name": "strlastLoginDate", "autoWidth": true },
    //        { "data": "status", "name": "status", "autoWidth": true },
    //        {
    //            "data": "id", "orderable": "false", "render": function (data) {
    //                return "<a href='/UserManagement/UserProfile?KEY=" + data + "'target='_blanck' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Detail</a>";
    //            }
    //        }
    //    ],
    //    "columnDefs": [
    //        { "targets": 0, "searchable": true, "orderable": true, "width": "25%" },
    //        { "targets": 1, "searchable": true, "orderable": true, "width": "25%" },
    //        { "targets": 2, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 3, "searchable": true, "orderable": true, "width": "20%" },
    //        { "targets": 4, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 5, "searchable": true, "orderable": true, "width": "10%" }

    //    ],
    //    "order": [[0, "asc"]]
    //});



}


function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



