
$(document).ready(function () {

    LoanLoans();
});

function LoanLoans() {
    LoadDataGen('Loan', 'myDataTable', '_LoanData', 0, 'datalistingview', null, 'Loan','list')

}

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



