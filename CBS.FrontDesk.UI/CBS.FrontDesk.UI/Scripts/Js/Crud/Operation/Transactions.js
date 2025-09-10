$(document).ready(function () {
    $("#btnData").click(function () {
        LoadData();
    });
    GetTransactionHistory(null, 'datalistingview', '_TransactionData', 'transactions','myDataTable','0');
});

function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable,order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}






