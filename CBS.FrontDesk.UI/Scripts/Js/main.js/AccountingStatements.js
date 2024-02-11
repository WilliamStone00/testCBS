$(document).ready(function () {
    $("#btnData").click(function () {
        LoadData();
    });
    GetTransactionHistory(null, 'datalistingview', '_GeneralLedgerData', 'export_generalLedger', 'myDataTable', '0');
});

function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("AccountingStatements", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function ExportFile(controller, serviceOption, action, KEY, ReadOptions, path, rptType, ReportName, reportoption, reportpath, fileTitle, datefrom, dateto) {
    appalert("Please wait, downloading file", 1);

    $.post(
        '/' + controller + '/' + action,
        {
            controller: controller,
            serviceOption: serviceOption,
            action: action,
            KEY: KEY,
            ReadOptions: ReadOptions,
            path: path,
            rptType: rptType,
            ReportName: ReportName,
            reportoption: reportoption,
            reportpath: reportpath,
            fileTitle: fileTitle,
            datefrom: datefrom,
            dateto: dateto
        },
        function () {
            window.open("/Reports/" + reportoption, "_blank"); // Updated URL
        }
    ).fail(function (err) {
        appalert(err.statusText, 1, 3);
    });
}