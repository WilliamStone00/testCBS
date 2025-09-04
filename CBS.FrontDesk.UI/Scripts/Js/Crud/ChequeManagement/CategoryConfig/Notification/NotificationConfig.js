// Location: ~/Scripts/Js/YourPath/NotificationConfig.js

$(document).ready(function () {
    // Set up the initial event listeners on the static parts of the page.
    setupNotificationConfigToggles();
    $('#loadConfigBtn').on('click', loadConfiguration);

    // Set up delegated event handlers for the dynamic form.
    setupDynamicFormHandlers();
});


function setupNotificationConfigToggles() {
    $('#isCentralized').on('change', function () {
        if (this.checked) {
            $('#branch-selection-container').slideUp();
            $('#branchId').val('').trigger('change');
        } else {
            $('#branch-selection-container').slideDown();
        }
    });
}


function loadConfiguration() {
    var isCentralized = $('#isCentralized').is(':checked');
    var branchId = isCentralized ? null : $('#branchId').val();
    var notificationType = $('#notificationType').val();

    // Validation
    if (!isCentralized && !branchId) {
        appalert('Please select a branch for non-centralized configuration.', 2, 1);
        return;
    }
    if (!notificationType) {
        appalert('Please select a notification type.', 2, 1);
        return;
    }

    // Use jQuery's .load() to call the InitializeData action and inject the HTML.
    $('#config-form-container').load(
        '/NotificationConfig/InitializeData',
        {
            partialView: '_ConfigForm',
            isCentralized: isCentralized,
            branchId: branchId,
            notificationType: notificationType
        }
    );
}


function setupDynamicFormHandlers() {
    var container = $('#config-form-container');

    // Handle the "Reset" button click by simply re-loading the original data.
    container.on('click', '#resetConfigBtn', function () {
        loadConfiguration();
    });

    // Handle the "Save" button click.
    container.on('click', '#saveConfigBtn', function () {
        saveConfiguration();
    });

    // Handle the "Delete" button click.
    container.on('click', '#deleteConfigBtn', function () {
        deleteConfiguration();
    });
}


function saveConfiguration() {
    var form = $('#notificationConfigForm');
    if (form.length === 0 || !form.valid()) {
        // Ensure form exists and is valid (if using jQuery validation).
        return;
    }

    var formData = form.serialize();
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/NotificationConfig/CreateOrUpdate',
        type: 'POST',
        data: formData + "&__RequestVerificationToken=" + token,
        success: function (response) {
            if (response.success) {
                appalert(response.message, 1, 1);
                // On success, reload the configuration to get the updated state.
                loadConfiguration();
            } else {
                appalert(response.message, 2, 1);
            }
        },
        error: function (err) {
            appalert('An error occurred while saving: ' + err.statusText, 0, 1);
        }
    });
}


function deleteConfiguration() {
    var configId = $('#Id').val(); // The ID is in a hidden field in the form.
    if (!configId) {
        appalert('This template has not been saved yet.', 3, 1);
        return;
    }

    if (confirm('Are you sure you want to delete this notification template?')) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/NotificationConfig/Delete',
            type: 'POST',
            data: {
                KEY: configId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    appalert(response.message, 1, 1);
                    // On successful deletion, clear the form from the page.
                    $('#config-form-container').empty();
                } else {
                    appalert(response.message, 2, 1);
                }
            },
            error: function (err) {
                appalert('An error occurred during deletion: ' + err.statusText, 0, 1);
            }
        });
    }
}