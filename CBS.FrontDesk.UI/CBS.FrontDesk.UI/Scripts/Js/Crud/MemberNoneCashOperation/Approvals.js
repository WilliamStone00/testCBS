$(document).ready(function () {

    // Load pending operations on page load
    LoadDataGen('MemberNoneCashOperation', 'myDataTable', '_PendingOperationsData', 0, 'datalistingview', null, null, 'pending_operations');

    // Helper function to show loading spinner inside a container
    function showLoadingSpinner(container) {
        $(container).html(
            '<div class="d-flex justify-content-center align-items-center p-5">' +
            '<div class="spinner-border text-primary" role="status">' +
            '<span class="visually-hidden">Loading...</span>' +
            '</div>' +
            '</div>'
        );
    }

    // Helper function for handling AJAX errors
    function handleAjaxError(container, message) {
        $(container).html(`<p class="text-danger">${message}</p>`);
    }

    // View Details Handler
    $(document).on('click', '.view-details', function () {
        var operationId = $(this).data('operation-id');
        showLoadingSpinner('#operationDetailContainer');

        // Fetch details via AJAX
        $.ajax({
            url: '/MemberNoneCashOperation/InitializeData',
            type: 'GET',
            data: {
                KEY: operationId,
                path: 'detail',
                partialView: '_OperationDetail'
            },
            success: function (result) {
                $('#operationDetailContainer').html(result);
                $('#operationDetailModal').modal('show');
            },
            error: function () {
                handleAjaxError('#operationDetailContainer', 'Failed to load operation details. Please try again.');
            }
        });
    });

    // Load Validation Form Handler
    window.loadValidationForm = function (operationId) {
        showLoadingSpinner('#validateOperationContainer');

        $.ajax({
            url: '/MemberNoneCashOperation/InitializeData',
            type: 'GET',
            data: {
                KEY: operationId,
                path: 'get_validation',
                partialView: '_ValidateMemberNoneCashOperation'
            },
            success: function (result) {
                $('#validateOperationContainer').html(result);
                $('#validateOperationModal').modal('show');
            },
            error: function () {
                handleAjaxError('#validateOperationContainer', 'Failed to load validation form. Please try again.');
            }
        });
    };

    // Submit Validation Form
    $(document).on('submit', '#validateOperationForm', function (e) {
        e.preventDefault();

        $.ajax({
            url: '/MemberNoneCashOperation/SubmitValidation',
            type: 'POST',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    $('#validateOperationModal').modal('hide');
                    appalert(response.message, 1, 1);
                    location.reload();  // Refresh the table to reflect changes
                } else {
                    appalert(response.message, 3, 1);
                }
            },
            error: function () {
                appalert('An error occurred while processing the validation.', 3, 1);
            }
        });
    });

});
