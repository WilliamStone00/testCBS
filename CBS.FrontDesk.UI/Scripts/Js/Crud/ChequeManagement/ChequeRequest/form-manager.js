// Scripts/Js/Crud/ChequeManagement/ChequeRequest/form-manager.js
class FormManager {
    constructor() {
        this.init();
    }

    init() {
        // Initialize form-specific events here
        this.initFormEvents();
    }

    initFormEvents() {
        // Cancel button event - using event delegation for dynamically loaded content
        $(document).on('click', '#cancelFormBtn', () => {
            this.confirmCancel();
        });

        // Form validation or other form-specific logic can be added here
        
        // Initialize Select2 if used on the form
        this.initSelect2();
    }

    initSelect2() {
        // Initialize Select2 on dropdowns if Select2 is available
        if (typeof $.fn.select2 !== 'undefined') {
            $('.select2').select2({
                width: '100%',
                placeholder: "Select an option"
            });
        }
    }

    confirmCancel() {
        // Use alertify for cancellation confirmation
        alertify.confirm(
            'Cancel Request',
            'Are you sure you want to cancel this request? All entered data will be lost.',
            () => {
                // OK callback - proceed with cancellation
                this.hideForm();
            },
            () => {
                // Cancel callback - do nothing
                appalert('Cancellation aborted', 2, 1);
            }
        ).set('labels', { ok: 'Yes, Cancel', cancel: 'No, Keep Editing' });
    }

    hideForm() {
        $('#requestFormSection').hide().empty();
        $('#customerIdInput').val('').focus();
        appalert('Request form cancelled', 2, 1);
    }

    // Additional form utility methods can be added here
    validateForm() {
        // Custom form validation logic if needed
        return true;
    }

    resetForm() {
        const form = document.getElementById('chequeRequestForm');
        if (form) {
            form.reset();
            // Reset any dynamic elements if needed
        }
    }
}

window.FormManager = FormManager;