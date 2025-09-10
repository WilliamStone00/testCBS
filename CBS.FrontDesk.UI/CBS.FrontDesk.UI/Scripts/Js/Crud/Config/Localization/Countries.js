/**
 * App user list (jquery)
 */


$(document).ready(function () {
    $("#btnData").click(function () {
        LoadData();
    });
    LoadDT("jkjkjkj",0)
});

function LoadData() {
    LoadDataTableNew("Country", "myDataTable", "InitializeData", null, "_Data", 1, "list","datalistingview");

}
function AddORUpdate(KEY, divToLoadData,partialView,path) {
    EditResetMain(KEY, partialView, divToLoadData, "Country", "InitializeData", null, path,null);
}

