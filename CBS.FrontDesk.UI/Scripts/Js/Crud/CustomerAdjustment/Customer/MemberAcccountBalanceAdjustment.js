$(document).ready(function () {
    // Initialize Bootstrap Modal
    var modalEl = document.getElementById('memberAdjustmentModal');
    var modal = new bootstrap.Modal(modalEl);

    // Initialize when modal is shown
    $('#memberAdjustmentModal').on('shown.bs.modal', function () {
        // Set initial values
        var initialOption = $('#accountDropdown').find(':selected');
        $('#oldBalance').val(initialOption.data('balance') || "0.00");
        $('#accountId').val(initialOption.val() || "");

        // Bind change event
        $('#accountDropdown').off('change').on('change', function () {
            var selectedOption = $(this).find(':selected');
            $('#oldBalance').val(selectedOption.data('balance') || "0.00");
            $('#accountId').val(selectedOption.val() || "");
        });
    });
});