
function LoadLoanProducts() {
    LoadDataTableNew("LoanProduct", "myDataTable", "InitializeData", null, "_AccountMappingData", 0, "list", "datalistingview");

}


$(document).ready(function () {
    LoadLoanProducts();
});