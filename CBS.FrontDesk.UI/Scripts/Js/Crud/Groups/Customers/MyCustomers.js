
$(document).ready(function () {

    LoadDataGen('Individual', 'myDataTable', '_IndividualData', 0, 'datalistingview', null,'branch')
});


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}







function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Transactions", "InitializeData");
}
function LoadUsers() {
    LoadDataGen('Individual', 'myDataTable', '_IndividualData', 0, 'datalistingview', "KEY")



}






