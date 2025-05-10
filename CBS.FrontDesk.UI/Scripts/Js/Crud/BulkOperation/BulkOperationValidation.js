
$('#approveBulkOperation').click(function () {
    var simulationId = $(this).data('simulation-id') || '';
    console.log("SimulationId", simulationId);

    // Call the function with the simulation ID and default status
    showConfirmBulkOperationModal(simulationId, 'Approved');
});

$('#reviewBulkOperation').click(function () {
    var simulationId = $(this).data('simulation-id') || '';
    console.log("SimulationId", simulationId);

    // Call the function with the simulation ID and default status
    showConfirmBulkOperationModal(simulationId, 'Reviewed');
});


function showConfirmBulkOperationModal(simulationId,status) {
    // Set the simulation ID
    $('#bulkOperationSimulationId').val(simulationId);
    $('#bulkOperationSimulationIdDisplay').text(simulationId);
    $('#approvalStatus').val(status),

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
    const formData = {
        BulkOperationSimulationId: $('#bulkOperationSimulationId').val(),
        ApprovalStatus: $('#approvalStatus').val(),
        ApprovalStatusDescription: $('#approvalStatusDescription').val(),
        ApprovalBy: $('#approvalBy').val()
    };

    // Here you would typically make an AJAX call to your API
    console.log('Submitting bulk operation confirmation:', formData);

    approveBulkOperation(formData);

  /*  // Simulate API call
    setTimeout(function () {
        // Hide loader
        $('#confirmBulkOperationLoader').addClass('d-none');

        // Close modal
        $('#confirmBulkOperationModal').modal('hide');

        // Show success message
        alert('Bulk operation confirmed successfully!');
    }, 1500);*/
});

function approveBulkOperation(formData) {
    // Determine confirmation message based on status
    let confirmationMessage;
    let actionVerb;

    switch (formData.ApprovalStatus.toLowerCase()) {
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

    // --- Confirmation and Submit
    alertify.confirm("Confirmation", confirmationMessage,
        function () {
            const ajaxConfig = {
                type: 'POST',
                url: 'BulkOperation/Validation',
                data: formData,
                success: function (response) {
                    console.log("Response:", response);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        setTimeout(() => {
                            if (response.redirectUrl) {
                                window.location.href = response.redirectUrl;
                            } else {
                                location.reload();
                            }
                        }, 1500);
                    } else {
                        appalert(response.message || `❌ Failed to ${actionVerb} the operation.`, 2, 1);
                    }
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
}