$(document).ready(function () {

    LoadCashMovementDataDTHO("DataTableCashMovementTrackerDataHO")
    LoadCashMovementDataDTBranch("DataTableCashMovementTrackerDataBranch")
    $(document).on('change', '#CashMovementTrackingConfiguration_MovementType', function () {

        // Get the selected value
        var ZoneID = $("#zoneId").val();
        var selectedValue = $(this).val();
        loadLocalBranchesDestination(ZoneID);
        loadAppropriateDestination(selectedValue,ZoneID);
    });

});

function loadAppropriateDestination(selectedValue, ZoneID)
{
    var endPoint = selectedValue === "Branch-To-Bank" ? '/CashMovementTrackerConfiguration/GetBuildThirdPartyBank' : '/CashMovementTrackerConfiguration/GetBuildBranches';
    console.log(endPoint);
    $.ajax({
        url: endPoint,
        type: 'GET',
        dataType: 'json',
        //data: { branchId: BranchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo 
            $('#CashMovementTrackingConfiguration_To').empty();
            $.each(data, function (index, item) {
                $('#CashMovementTrackingConfiguration_To').append($('<option>').text(item.Value).attr('value', item.Text));
            });

            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function loadLocalBranchesDestination(selectedValue) {
    var endPoint = "CorrespondingBankManagement/GetZoneBankBranch"; //: '/CashMovementTrackerConfiguration/GetBuildBranches';
    console.log(endPoint);
    $.ajax({
        url: endPoint,
        type: 'GET',
        dataType: 'json',
        data: { ZoneID: selectedValue },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo 
            $('#CashMovementTrackingConfiguration_To').empty();
            $('#CashMovementTrackingConfiguration_From').empty();

            $.each(data, function (index, item) {
                $('#CashMovementTrackingConfiguration_To').append($('<option>').text(item.Value).attr('value', item.Text));
            });
     
            $.each(data, function (index, item) {
                $('#CashMovementTrackingConfiguration_From').append($('<option>').text(item.Value).attr('value', item.Text));
            });
            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function LoadCashMovementDataDTHO(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [
            //{ "data": "ReferenceId", "name": "ReferenceId", "autoWidth": true },
            //{ "data": "Amount", "name": "Amount", "autoWidth": true },
            //{ "data": "RequestMessage", "name": "RequestMessage", "autoWidth": true },
            //{ "data": "IssuedBy", "name": "IssuedBy", "autoWidth": true },
            //{ "data": "IssuedDate", "name": "IssuedDate", "autoWidth": true },
            //{
            //    "data": "Id", "orderable": "false", "render": function (data) {
            //        return "<a href='/CashFlowManagement/GetCashReplenimentRequest?KEY=" + data + " class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Click to Approve</a>";
            //    }
            //}
        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "22%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "23%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "10%" },
        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}

function LoadCashMovementDataDTBranch(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "32%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "33%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "10%" },
   
        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}