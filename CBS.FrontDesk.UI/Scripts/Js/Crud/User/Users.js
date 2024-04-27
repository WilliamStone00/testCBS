
$(document).ready(function () {
    LoadUsers();
});


function UserDetail(id) {
    EditResetModal(id, 'modal', 'modalContent', 'UserManagement', 'InitializeData', '_UserDetails', 'userdetail', 'USER DETAIL', 'modalLabel')
}

function LoadUsers() {
    LoadDataGen('UserManagement', 'myDataTable', '_Data', 0, 'datalistingview', "KEY",null,'List')
}
