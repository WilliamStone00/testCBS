$(document).ready(function () {
    $('input[name="operationType"]').change(function () {
        if ($(this).val() === 'specific') {
            $('#mainview1').show();
            $('#mainview2').hide();
        } else {
            $('#mainview1').hide();
            $('#mainview2').show();
        }
    });

    // Initialize view based on selected radio
    if ($('input[name="operationType"]:checked').val() === 'specific') {
        $('#mainview1').show();
        $('#mainview2').hide();
    } else {
        $('#mainview1').hide();
        $('#mainview2').show();
    }
});