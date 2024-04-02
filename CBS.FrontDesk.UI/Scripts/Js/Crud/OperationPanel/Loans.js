
$(document).ready(function () {

    PendingDisbursementData();
});
function GetPendingLoans(id) {
    AddORUpdateGen(id, 'datalistingview', '_PendingDisbursmentForm', 'loan_for_disbursed', 'MemberOperation', 'applications')
}
function PendingDisbursementData() {
    LoadDataGen('MemberOperation', 'myDataTable', '_PendingDisbursementData', 0, 'datalistingview', null, 'applications', 'perding_disbursement')

}

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



