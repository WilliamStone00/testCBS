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

    let confirmationMessage;
    let actionVerb;

    switch (ApprovalStatus.toLowerCase()) {
        case 'approved':
            actionVerb = 'approve';
            confirmationMessage = `Are you sure you want to approve this bulk operation?`;
            break;
        case 'reviewed':
            actionVerb = 'send for review';
            confirmationMessage = `Are you sure you want to send this bulk operation for review?`;
            break;
        case 'rejected':
            actionVerb = 'reject';
            confirmationMessage = `Are you sure you want to reject this bulk operation?`;
            break;
        default:
            actionVerb = 'set as pending';
            confirmationMessage = `Are you sure you want to mark this bulk operation as pending?`;
    }

    // Confirmation prompt
    alertify.confirm("Confirmation", confirmationMessage,
        function () {
            $.ajax({
                url: '/BulkOperation/Validation',
                type: 'POST',
                data: {
                    stimulationId: BulkOperationSimulationId,
                    approvalStatus: ApprovalStatus,
                    description: ApprovalStatusDescription,
                    approvedBy: ApprovalBy
                },
                success: function (response) {
                    $('#confirmBulkOperationLoader').addClass('d-none');

                    if (response.success) {
                        appalert(response.message, 1, 1);
                        $('#confirmBulkOperationModal').modal('hide');
                        $('#confirmBulkOperationForm')[0].reset();
                    } else {
                        appalert(response.message || `❌ Failed to ${actionVerb} the operation.`, 2, 1);
                    }
                },
                error: function (err) {
                    $('#confirmBulkOperationLoader').addClass('d-none');

                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.statusText || `❌ An error occurred while trying to ${actionVerb}.`, 0, 1);
                    }
                }
            });
        },
        function () {
            $('#confirmBulkOperationLoader').addClass('d-none');
            appalert(`Operation to ${actionVerb} was cancelled.`, 3, 1);
        }
    );
});