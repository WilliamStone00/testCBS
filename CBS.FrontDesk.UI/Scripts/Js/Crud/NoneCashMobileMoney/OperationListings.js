$(document).ready(function () {


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


    // Delete Operation Handler
    window.deleteOperation = function (operationId) {
        alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this!.",
            function () {
                $.post('/MemberNoneCashOperation/DeleteOperation', { KEY: operationId })
                    .done(function (response) {
                        if (response.success) {
                            appalert(response.message, 1, 1);
                            location.reload();
                        } else {
                            appalert(response.message, 3, 1);
                        }
                    })
                    .fail(function () {
                        appalert('Failed to delete the operation. Please try again.', 3, 1);

                    });
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }
        );
    };
});
