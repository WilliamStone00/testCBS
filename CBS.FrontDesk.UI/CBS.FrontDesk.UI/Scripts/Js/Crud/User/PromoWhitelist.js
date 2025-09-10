
$(document).ready(function () {
    $("#btnData").click(function () {
        LoadPromoWhitelist();
    });
});


function LoadPromoWhitelist() {
    LoadDataMain("Frontend", "PromoWhitelist", "datalistingview", "myDataTable", "InitializeData", null, "List", null, null);
}