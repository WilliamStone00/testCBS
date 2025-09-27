//(function ($) {
//    'use strict';

//    const ALERT_STATE = { SUCCESS: 1, WARNING: 2, INFO: 3, DANGER: 4 };
//    const ALERT_TYPE = { TOASTR: 1 };

//    const workspace = $('#notificationConfigWorkspace');
//    const URLS = {
//        init: workspace.data('init-url'),
//        delete: workspace.data('delete-url'),
//        save: workspace.data('save-url')
//    };

//    // Global function to check if save button exists
//    window.checkSaveButton = function () {
//        const exists = $('#saveConfigBtn').length > 0;
//        console.log('Save button exists:', exists);
//        if (exists) {
//            console.log('Save button HTML:', $('#saveConfigBtn')[0].outerHTML);
//        }
//        return exists;
//    };

//    $(document).ready(function () {
//        console.log('Document ready - initializing');
//        $('.select2').select2({ width: '100%' });

//        // Setup static handlers
//        $('#isCentralized').on('change', function () {
//            $('#branch-selection-container').toggle(!this.checked);
//            if (this.checked) {
//                $('#branchId').val('').trigger('change');
//            }
//        });

//        $('#loadConfigBtn').on('click', loadConfiguration);

//        // Test button handlers
//        $('#testSave').on('click', function () {
//            const exists = window.checkSaveButton();
//            alert('Save button exists: ' + exists);
//        });

//        $('#testLoad').on('click', loadConfiguration);

//        // Setup global event delegation for dynamic content
//        setupGlobalEventDelegation();
//    });

//    function setupGlobalEventDelegation() {
//        console.log('Setting up global event delegation');

//        // Event delegation for ANY saveConfigBtn that exists now or in the future
//        $(document).on('click', '#saveConfigBtn', function (e) {e.preventDefault();
//            console.log('Save button clicked via global delegation');
//            handleSaveClick();
//        });

//        $(document).on('click', '#deleteConfigBtn', function (e) {
//            e.preventDefault();
//            console.log('Delete button clicked via global delegation');
//            handleDeleteClick();
//        });

//        $(document).on('click', '#resetConfigBtn', function (e) {
//            e.preventDefault();
//            console.log('Reset button clicked via global delegation');
//            loadConfiguration();
//        });

//        $(document).on('click', '.placeholder-tag', function (e) {
//            e.preventDefault();
//            console.log('Placeholder clicked via global delegation');
//            insertPlaceholder.call(this); // Pass the clicked element context
//        });
//    }

//    function loadConfiguration() {
//        console.log('=== LOAD CONFIGURATION STARTED ===');

//        const isCentralized = $('#isCentralized').is(':checked');
//        const branchId = isCentralized ? null : $('#branchId').val();
//        const notificationType = $('#notificationType').val();

//        console.log('Form values:', { isCentralized, branchId, notificationType });

//        if (!isCentralized && !branchId) {
//            appalert('Please select a branch.', ALERT_STATE.WARNING, ALERT_TYPE.TOASTR);
//            return;
//        }
//        if (!notificationType) {
//            appalert('Please select a notification type.', ALERT_STATE.WARNING, ALERT_TYPE.TOASTR);
//            return;
//        }

//        const container = $('#config-form-container');
//        console.log('Container found:', container.length > 0);

//        //container.html('<div class="text-center p-4"><div class="spinner-border text-primary"></div><p>Loading template...</p></div>');

//        // Use proper AJAX call instead of .load() for better debugging
//        $.ajax({
//            url: URLS.init,
//            method: 'GET',
//            data: {
//                partialView: '_ConfigForm',
//                isCentralized: isCentralized,
//                branchId: branchId,
//                notificationType: notificationType
//            },
//            success: function (response) {
//                console.log('=== AJAX SUCCESS ===');
//                console.log('Response length:', response.length);
//                container.html(response);

//                // Check if buttons exist after loading
//                console.log('Save button after load:', $('#saveConfigBtn').length);
//                console.log('Delete button after load:', $('#deleteConfigBtn').length);
//                console.log('Reset button after load:', $('#resetConfigBtn').length);

//                // Test if the save button is clickable
//                const saveBtn = $('#saveConfigBtn');
//                if (saveBtn.length) {
//                    console.log('Save button properties:', {
//                        id: saveBtn.attr('id'),
//                        type: saveBtn.attr('type'),
//                        disabled: saveBtn.prop('disabled'),
//                        html: saveBtn.html()
//                    });

//                    // Test direct click binding as backup
//                    saveBtn.off('click').on('click', function (e) {
//                        e.preventDefault();
//                        console.log('Save button clicked via direct binding');
//                        handleSaveClick();
//                    });
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error('=== AJAX ERROR ===', error);
//                container.html('<div class="alert alert-danger">Error loading template: ' + error + '</div>');
//            }
//        });
//    }

//    function handleSaveClick() {
//        console.log('=== HANDLE SAVE CLICK STARTED ===');

//        // Validate form
//        const templateBody = $('#templateBody').val();
//        console.log('Template body value:', templateBody);

//        if (!templateBody || templateBody.trim() === '') {
//            appalert('Template body is required.', ALERT_STATE.WARNING, ALERT_TYPE.TOASTR);
//            return;
//        }

//        alertify.confirm("Confirm Save", "Are you sure you want to save these changes?",
//            function () { // onOk
//                console.log('User confirmed save');
//                const $saveBtn = $('#saveConfigBtn');
//                const originalHtml = $saveBtn.html();
//                $saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Saving...');

//                // Collect all form data with proper selectors
//                const formData = {
//                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
//                    Id: $('#Id').val() || '',
//                    IsCentralized: $('#IsCentralized').val() || 'false',
//                    BranchId: $('#BranchId').val() || '',
//                    NotificationType: $('#NotificationType').val() || '',
//                    IsActive: $('#IsActive').is(':checked'),
//                    TemplateBody: $('#templateBody').val(),
//                    Description: $('#description').val() || '',
//                    name: $('#name').val() || ''  // ADD THIS LINE
//                };

//                /*const formData = $('#notificationConfigForm').serialize();*/

//                console.log('Form data to send:', formData);

//                $.ajax({
//                    url: URLS.save,
//                    type: 'POST',
//                    data: formData,
//                    success: function (response) {
//                        console.log('Save response:', response);
//                        if (response && response.success) {
//                            appalert(response.message, ALERT_STATE.SUCCESS, ALERT_TYPE.TOASTR);
//                            loadConfiguration(); // Reload the form
//                        } else {
//                            appalert(response.message || 'Save failed.', ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
//                        }
//                    },
//                    error: function (xhr, status, error) {
//                        console.error('Save error:', error, xhr);
//                        appalert('An unexpected server error occurred: ' + error, ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
//                    },
//                    complete: function () {
//                        $saveBtn.prop('disabled', false).html(originalHtml);
//                    }
//                });
//            },
//            function () {
//                console.log('User cancelled save');
//                appalert('Save cancelled.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR);
//            }
//        );
//    }

//    function handleDeleteClick() {
//        console.log('=== HANDLE DELETE CLICK STARTED ===');

//        const configId = $('#Id').val();
//        console.log('Config ID for deletion:', configId);

//        if (!configId) {
//            appalert('This template has not been saved yet.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR);
//            return;
//        }

//        alertify.confirm('Confirm Deletion', 'This action cannot be undone. Are you sure?',
//            function () {
//                console.log('User confirmed deletion');
//                const token = $('input[name="__RequestVerificationToken"]').val();
//                $.ajax({
//                    url: URLS.delete,
//                    type: 'POST',
//                    data: { __RequestVerificationToken: token, KEY: configId },
//                    success: function (response) {
//                        console.log('Delete response:', response);
//                        if (response && response.success) {
//                            appalert(response.message, ALERT_STATE.SUCCESS, ALERT_TYPE.TOASTR);
//                            $('#config-form-container').empty();
//                        } else {
//                            appalert(response.message, ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
//                        }
//                    },
//                    error: function (xhr, status, error) {
//                        console.error('Delete error:', error);
//                        appalert('An error occurred during deletion.', ALERT_STATE.DANGER, ALERT_TYPE.TOASTR);
//                    }
//                });
//            },
//            function () {
//                console.log('User cancelled deletion');
//                appalert('Deletion cancelled.', ALERT_STATE.INFO, ALERT_TYPE.TOASTR);
//            }
//        );
//    }

//    function insertPlaceholder() {
//        console.log('=== INSERT PLACEHOLDER STARTED ===');

//        const placeholderText = $(this).attr('title') || $(this).text() || '';
//        console.log('Placeholder text:', placeholderText);

//        if (!placeholderText) return;

//        const textarea = document.getElementById('templateBody');
//        if (!textarea) {
//            console.error('Template textarea not found');
//            return;
//        }

//        try {
//            const start = textarea.selectionStart || 0;
//            const end = textarea.selectionEnd || 0;
//            const val = textarea.value || '';
//            textarea.value = val.substring(0, start) + placeholderText + val.substring(end);
//            textarea.focus();
//            const pos = start + placeholderText.length;
//            textarea.selectionStart = textarea.selectionEnd = pos;
//            console.log('Placeholder inserted successfully');
//        } catch (ex) {
//            console.error('Error inserting placeholder:', ex);
//            textarea.value = (textarea.value || '') + placeholderText;
//            textarea.focus();
//        }
//    }

//}(jQuery));

// This is the single, consolidated script that powers the entire Notification Configuration page.

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
        container.html('<div class="text-center p-4"><div class="spinner-border text-primary"></div></div>');

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
            function () { // onOk
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
        textarea.value = text.substring(0, start) + placeholderText + text.substring(end);
        textarea.focus();
        textarea.selectionStart = textarea.selectionEnd = start + placeholderText.length;
    }

}(jQuery));
