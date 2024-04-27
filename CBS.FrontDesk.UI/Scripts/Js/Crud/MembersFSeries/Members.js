
$(document).ready(function () {
    LoadMembers();
});

function GetMembersData(Key, partialView, divToloadPV, path) {

    AddORUpdateGen(Key, divToloadPV, partialView, path, "MembersFSeries");
}

function LoadMembers() {
    LoadDataGen('MembersFSeries', 'myDataTable', '_MembersData', 0, 'datalistingview', null, 'all')

}

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



