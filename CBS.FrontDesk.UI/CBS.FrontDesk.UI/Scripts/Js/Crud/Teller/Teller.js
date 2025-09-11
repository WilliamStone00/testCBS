

$(document).ready(function () {
   
    LoadDT("myDataTable_teller",0)
});


function LoadDropDown(KEY, path, affectedID) {
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");

}