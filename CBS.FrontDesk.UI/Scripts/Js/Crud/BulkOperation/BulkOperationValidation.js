
$('#approveBulkOperation').click(function () {
    var simulationId = $(this).data('simulation-id') || '';
    console.log("SimulationId", simulationId);

    // Call the function with the simulation ID and default status
    showConfirmBulkOperationModal(simulationId);
});


function showConfirmBulkOperationModal(simulationId) {
    // Set the simulation ID
    $('#bulkOperationSimulationId').val(simulationId);
    $('#bulkOperationSimulationIdDisplay').text(simulationId);

    // Reset form
    $('#confirmBulkOperationForm')[0].reset();

    // Show modal
    $('#confirmBulkOperationModal').modal('show');
}


// Handle confirm button click
$('#confirmBulkOperationBtn').click(function () {
    // Show loader
    $('#confirmBulkOperationLoader').removeClass('d-none');

    // Get form data
 
    let BulkOperationSimulationId = $('#bulkOperationSimulationId').val();
    let ApprovalStatus = $('#approvalStatus').val();
    let ApprovalStatusDescription = $('#approvalStatusDescription').val();
    let ApprovalBy = $('#approvalBy').val();


    // Here you would typically make an AJAX call to your API
    console.log('Submitting bulk operation confirmation:', formData);

    let confirmationMessage;
    let actionVerb;
    console.log(BulkOperationSimulationId, ApprovalStatus, ApprovalStatusDescription, ApprovalBy);

  
    switch (ApprovalStatus.toLowerCase()) {
        case 'approved':
            actionVerb = 'approve';
            confirmationMessage = `Are you sure you want to approve this bulk operation?`;
            break;
        case 'reviewed':
            actionVerb = 'send for review';
            confirmationMessage = `Are you sure you want to send this bulk operation for review?`;
            break;
        default:
            actionVerb = 'set as pending';
            confirmationMessage = `Are you sure you want to mark this bulk operation as pending?`;
    }
    console.log(actionVerb);

    // --- Confirmation and Submit
    alertify.confirm("Confirmation", confirmationMessage,
        function () {
            const ajaxConfig = {
                url: 'BulkOperation/Validation',
                type: 'POST',
                data: function (d) {
                    d.stimulationId = BulkOperationSimulationId;
                    d.approvalStatus = ApprovalStatus;
                    d.description = ApprovalStatusDescription;
                    d.approvedBy = ApprovalBy;
                },
                success: function (response) {
                    console.log("Response:", response);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                    } else {
                        appalert(response.message || `❌ Failed to ${actionVerb} the operation.`, 2, 1);
                    }
                    $('#confirmBulkOperationForm')[0].reset();
                },
                error: function (err) {
                    console.log("Error:", err);
                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.statusText || `❌ An error occurred while trying to ${actionVerb}.`, 0, 1);
                    }
                }
            };

            $.ajax(ajaxConfig);
        },
        function () {
            appalert(`Operation to ${actionVerb} was cancelled.`, 3, 1);
        }
    );
});
