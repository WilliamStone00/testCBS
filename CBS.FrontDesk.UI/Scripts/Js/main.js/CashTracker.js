

function SetCashMovementTracker(BranchId,RefrenceId,TransactionType,movementType) {
    $('#cashTrackerReferenceTitle').empty();
    // Append text to the modal title
    $('#cashTrackerReferenceTitle').append('Set Cash Tracker On CashMovement With Reference :' + RefrenceId + ' ');
    $('#expectedReference').val(RefrenceId);
    $.ajax({
        url: '/CashMovementTracker/GetCashTrackingConfiguration',
        type: 'GET',
        dataType: 'json',
        data: { branchId: BranchId, referenceId: RefrenceId, transType: TransactionType, movementType: MovementType },
        success: function (data) {
   
            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
    loadBranchUser(BranchId);
}

function ActivateCashMovementTracker(controller, KEY, tableID, partialView, order, divToLoadTheData, serviceOption) {

    alertify.confirm("TRUST SOFT CREDIT", "You about to set a cash tracker on Cash Movement Reference: " + KEY + " \nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/ActivateCashMovementTracker?KEY=" + KEY + "&serviceOption=" + serviceOption;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        LoadDataTableNew(controller, tableID, "InitializeData", KEY, partialView, order, "list", divToLoadTheData)
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }

                }, error: function (err) {

                    appalert(err.statusText, 3, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);

        }

    );


}
function loadBranchUser(BranchId) {

    $.ajax({
        url: '/CashMovementTracker/GetAllBranchUser',
        type: 'GET',
        dataType: 'json',
        data: { branchId: BranchId },
        success: function (data) {
            $('#loadBranchUser').empty();
            $.each(data, function (index, item) {
                $('#loadBranchUser').append($('<option>').text(item.Value).attr('value', item.Text));
            });

            // Add new options based on the fetched data
            console.log(data); 
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}