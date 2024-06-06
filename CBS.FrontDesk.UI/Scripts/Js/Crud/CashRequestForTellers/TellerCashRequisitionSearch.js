




function SearchByDates(partialView, divToloadPV,controller) {
    if (!validateDates()) {
        return;
    }

    var branchid = $('#branchInput').val();
    var datefrom = $('#dateFromInput').val();
    var dateto = $('#dateToInput').val();
    LoadDataMain(controller, null, divToloadPV, "myDataTable", "InitializeData", branchid, "search", "search", null, datefrom, dateto, null, null, 0, partialView);
}





