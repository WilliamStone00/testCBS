$(document).ready(function () {

    //$('#myDataTable_customer_loan').DataTable();
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

});

function LoadDropDown(KEY, path,affectedID) {

    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID,"---Select Option---")

}
function LoadProductDetails(KEY) {

    EditResetMain(KEY, '_LoadProductDetails', '_loanproductdetailDiv', 'MemberOperation','InitializeData')

}


function EditReset(KEY, partialView) {
    EditResetMain(KEY, partialView, "mainview", "Individual", "InitializeData");
}





function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

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
    //        { "targets": 0, "searchable": true, "orderable": true, "width": "30%" },
    //        { "targets": 1, "searchable": true, "orderable": true, "width": "10%" },
    //        { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
    //        { "targets": 3, "searchable": true, "orderable": true, "width": "20%" },
    //        { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
    //        { "targets": 5, "searchable": true, "orderable": true, "width": "10%" },


    //    ],
    //    "order": [[0, "asc"]]
    //});



}





