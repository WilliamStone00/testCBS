
(function ($) {
    'use strict';

    // --- Configuration for your global appalert function ---
    const ALERT_STATE = { SUCCESS: 1, WARNING: 2, INFO: 3, DANGER: 4 };
    const ALERT_TYPE = { TOASTR: 1 };

    // --- Read URLs from the main page's container once ---
    const workspace = $('#notificationConfigWorkspace');
    const URLS = {
        init: workspace.data('init-url'),
        delete: workspace.data('delete-url')
        // The save URL will be read from the form's 'action' attribute
    };

    /**
     * This is the single entry point. It runs when the page is ready.
     */
    $(document).ready(function () {
        // 1. Initialize static UI elements (like select2 plugins)
        $('.select2').select2({ width: '100%' });

        // 2. Wire up all event handlers for the page
        setupStaticEventHandlers();
        setupDelegatedEventHandlers();
    });

    /**
     * Wires up event handlers for static elements that exist on page load.
     */
    function setupStaticEventHandlers() {
        $('#isCentralized').on('change', function () {
            $('#branch-selection-container').toggle(!this.checked);
            if (this.checked) {
                $('#branchId').val('').trigger('change');
            }
        });

        $('#loadConfigBtn').on('click', loadConfiguration);
    }

    /**
     * Wires up DELEGATED event handlers for dynamic content that will be loaded into the container.
     */
    function setupDelegatedEventHandlers() {
        const container = $('#config-form-container');

        // Handles the SAVE action via form submission
        container.on('submit', '#notificationConfigForm', function (e) {
            e.preventDefault();
            handleSave(this);
        });

        // Handles DELETE and RESET buttons by their specific IDs
        container.on('click', '#deleteConfigBtn', handleDelete);
        container.on('click', '#resetConfigBtn', loadConfiguration);

        // Handles clicking on placeholder tags
        container.on('click', '.placeholder-tag', insertPlaceholder);
    }

    // --- WORKFLOW FUNCTIONS ---

    function loadConfiguration() {
        const isCentralized = $('#isCentralized').is(':checked');
        const branchId = isCentralized ? null : $('#branchId').val();
        const notificationType = $('#notificationType').val();

        if (!isCentralized && !branchId) { appalert('Please select a branch.', ALERT_STATE.WARNING, ALERT_TYPE.TOASTR); return; }
        if (!notificationType) { appalert('Please select a notification type.', ALERT_STATE.WARNING, ALERT_TYPE.TOASTR); return; }

        const container = $('#config-form-container');


        container.load(URLS.init, {
            partialView: '_ConfigForm',
            isCentralized: isCentralized,
            branchId: branchId,
            notificationType: notificationType
        });
    }

    function handleSave(form) {
        const $form = $(form);
        if ($.validator && $.validator.unobtrusive && !$form.valid()) {
            return;
        }

        const $submitBtn = $form.find('button[type="submit"]');
        const originalHtml = $submitBtn.html();

        alertify.confirm("Confirm Save", "Are you sure you want to save these changes?",
            function () { 
                $submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Saving...');

                $.ajax({
                    url: $form.attr('action'),
                    type: 'POST',
                    data: $form.serialize(), // Reliably collects all form data
                    success: function (response) {
                        if (response.success) {
                            appalert(response.message, ALERT_STATE.SUCCESS, ALERT_TYPE.TOASTR);
                            loadConfiguration(); // Reload the form on success
                        } else {
                            appalert(response.message, ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
                        }
                    },
                    error: function () { appalert('An unexpected server error occurred.', ALERT_STATE.DANGER, ALERT_TYPE.TOASTR); },
                    complete: function () { $submitBtn.prop('disabled', false).html(originalHtml); }
                });
            },
            function () { appalert('Save cancelled.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR); }
        );
    }

    function handleDelete() {
        const configId = $('#Id').val();
        if (!configId) {
            appalert('This template has not been saved yet.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR);
            return;
        }
        alertify.confirm('Confirm Deletion', 'This action is permanent. Are you sure?',
            function () { // onOk
                const token = $('input[name="__RequestVerificationToken"]').val();
                $.ajax({
                    url: URLS.delete,
                    type: 'POST', // Use POST for secure deletion
                    data: { __RequestVerificationToken: token, KEY: configId },
                    success: function (response) {
                        if (response.success) {
                            appalert(response.message, ALERT_STATE.SUCCESS, ALERT_TYPE.TOASTR);
                            $('#config-form-container').empty(); // Clear the form on success
                        } else {
                            appalert(response.message, ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
                        }
                    },
                    error: function () { appalert('An unexpected error occurred during deletion.', ALERT_STATE.DANGER, ALERT_TYPE.TOASTR); }
                });
            },
            function () { appalert('Deletion cancelled.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR); }
        );
    }

    function insertPlaceholder() {
        const textarea = document.getElementById('templateBody');
        const placeholderText = $(this).text();
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const text = textarea.value;

        // Add $ before the placeholder text
        const textToInsert = `$${placeholderText}`;

        textarea.value = text.substring(0, start) + textToInsert + text.substring(end);
        textarea.focus();
        textarea.selectionStart = textarea.selectionEnd = start + textToInsert.length;
    }


}(jQuery));
