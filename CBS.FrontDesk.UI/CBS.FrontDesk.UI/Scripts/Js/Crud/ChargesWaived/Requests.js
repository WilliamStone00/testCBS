document.addEventListener('DOMContentLoaded', function () {
    LoadAllTransactions();
});

function LoadAllTransactions() {
    LoadDataTableNew('ChargesWaived', 'myDataTable', "InitializeData", null, '_RequestsData', 0, "list", 'datalistingview');
}
function ViewDetail(id) {
    EditResetModal(id, 'modal', 'modalContent', 'ChargesWaived', 'InitializeData', '_RequestDetail', 'detail', 'WAIVER DETAIL', 'modalLabel')
}