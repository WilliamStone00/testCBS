
$(document).ready(function () {
    $("#btnData").click(function () {
        LoadAlertProfilelist();
    });
});


function LoadAlertProfilelist() {
    LoadDataMain("Frontend", "AlertProfile", "datalistingview", "myDataTable", "InitializeData", null, "List", null, null);
}