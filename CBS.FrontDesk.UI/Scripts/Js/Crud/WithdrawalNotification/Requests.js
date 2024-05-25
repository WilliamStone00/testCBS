document.addEventListener('DOMContentLoaded', function () {
    LoadAllTransactions();
});

function LoadAllTransactions() {
    LoadDataTableNew('WithdrawalNotification', 'myDataTable', "InitializeData", null, '_RequestsData', 0, "list", 'datalistingview');
}
function ViewDetail(id) {
    EditResetModal(id, 'modal', 'modalContent', 'WithdrawalNotification', 'InitializeData', '_RequestDetail', 'detail', 'REQUEST DETAIL', 'modalLabel')
}