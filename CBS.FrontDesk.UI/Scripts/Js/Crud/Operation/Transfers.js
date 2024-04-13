$(document).ready(function () {
    GetAllTransfers(null, 'datalistingview', '_TransferInitiatorData', 'all_transfer_request','myDataTableT','0');
});

function GetAllTransfers(KEY, divToLoadData, partialView, path, myDataTable,order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}






