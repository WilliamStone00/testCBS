
$(document).ready(function () {
    LoadMainServices();
    LoadSubServices();

    $("#btnDatasubservices").click(function () {
        LoadSubServices();
    });
    $("#btnDataservices").click(function () {
        LoadMainServices();

    });

});
function LoadSubServices() {
    LoadDataMain("Frontend", "SystemSubService", "datalistingview_ss", "myDataTable_ss", "InitializeData", "SystemSubService", "List");

}
function LoadMainServices() {
    LoadDataMain("Frontend", "SystemService", "datalistingview_s", "myDataTable_s", "InitializeData", "SystemService", "List");

}

function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}





