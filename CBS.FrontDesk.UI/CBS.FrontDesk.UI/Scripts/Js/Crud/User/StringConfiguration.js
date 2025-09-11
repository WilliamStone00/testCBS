
$(document).ready(function () {
    $("#btnData").click(function () {
        LoadStrings("strings");
    });
    $("#btnsetvalues").click(function () {
        LoadStrings("set_value");
    });
});


function LoadStrings(path) {
    LoadDataMain("Frontend", "StringConfiguration", "datalistingview", "myDataTable", "InitializeData", null, "List", path, null);
}

