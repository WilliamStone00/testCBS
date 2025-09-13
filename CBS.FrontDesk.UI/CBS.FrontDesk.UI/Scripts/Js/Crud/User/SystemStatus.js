
$(document).ready(function () {
        $("#btnData").click(function () {
            LoadDataMain("Frontend", "SystemStatus", "datalistingview", "myDataTable", "InitializeData", "SystemStatus", "List");
        });

    });

function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}





