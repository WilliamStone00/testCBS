function Search(controller, tableDiv, partialView, datalistViewDIV, filterOption) {
    // if (e) e.preventDefault(); // Prevent default if the event is passed

    const jsonData = {
        ServiceOption: $('#ServiceOption').val(),
        Action: $('#Action').val(),
        BranchId: $('select[name="QueryModel.BranchId"]').val(),
        FromDate: $('#fromDate').val(),
        ToDate: $('#toDate').val(),
        SelectedBranchID: $('#selectedBranchID').val()
    };
    var jsonDataj = JSON.stringify(jsonData);
    console.log("Collected Form Data:", jsonData);

    LoadhData(
        controller,  //'ManuallyJournalEntry',
        tableDiv, //  'myDataTable',
        partialView,// '_PendingEntries',
        datalistViewDIV, //'datalistingview_pendingEntries',
        jsonDataj,
        filterOption,// 'FilteringOption',
        $('select[name="QueryModel.FilteringOption"]').val()
    );

    return false;
}
function LoadhData(controller, tableID, partialView, datalistingview, KEY, serviceOption, path = "list") {
    path = serviceOption;
    // Call LoadDataTableNewVersion with predefined action "InitializeData" and other parameters
    LoadDataTableNewVersion(
        controller,
        tableID, 
        "InitializeData",
        KEY,
        partialView,
        path,
        datalistingview,
        serviceOption
    );
}