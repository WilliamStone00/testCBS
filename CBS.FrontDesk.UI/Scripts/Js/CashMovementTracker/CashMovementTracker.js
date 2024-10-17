$(document).ready(function () {

 
    $(document).on('change', '#CashMovementTrackingConfiguration_MovementType', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        loadAppropriateDestination(selectedValue);
    });

});

function loadAppropriateDestination(selectedValue) {
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