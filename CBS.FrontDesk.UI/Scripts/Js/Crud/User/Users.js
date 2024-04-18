
$(document).ready(function () {
    LoadUsers();
});




function LoadUsers() {
    LoadDataGen('UserManagement', 'myDataTable', '_Data', 0, 'datalistingview', "KEY",null,'List')
}
