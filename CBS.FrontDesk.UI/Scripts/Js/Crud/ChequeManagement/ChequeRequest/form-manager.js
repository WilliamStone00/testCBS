class FormManager {
    constructor() {
        this.isEditMode = false;
        this.init();
    }

    init() {
        $('#backToSearchBtn').on('click', () => this.backToSearch());
        this.interceptGenericAjax();
    }

    interceptGenericAjax() {
        // Store original function if it exists
        if (typeof window.AjaxPostAndUpdate === 'function') {
            const originalAjaxPostAndUpdate = window.AjaxPostAndUpdate;

            window.AjaxPostAndUpdate = function (form) {
                if (!window.formManager.validateForm()) {
                    return false;
                }
                return originalAjaxPostAndUpdate.call(this, form);
            };
        }
    }

    populateForm(customerData) {
        if (!customerData || !customerData.customerDto) return;

        const customer = customerData.customerDto;
        const accounts = customerData.accountDtos || [];

        $('#formCustomerId').val(customer.customerId);
        $('#formCustomerName').val(`${customer.firstName} ${customer.lastName}`);
        $('#formCustomerSummary').text(`${customer.customerId} - ${customer.firstName} ${customer.lastName}`);

        this.populateAccountDropdowns(accounts);
    }

    populateAccountDropdowns(accounts) {
        const $check = $('#checkBookAccountDropdown');
        const $sub = $('#subscriptionPaymentAccountDropdown');

        $check.empty().append('<option value="">Select Account</option>');
        $sub.empty().append('<option value="">Select Account</option>');

        if (accounts && accounts.length > 0) {
            accounts.filter(acc => acc.status && acc.status.toLowerCase() === 'active').forEach(account => {
                const text = `${account.accountNumber} - ${account.accountName} (${account.accountType})`;
                $check.append(new Option(text, account.accountNumber));
                $sub.append(new Option(text, account.accountNumber));
            });
        }
    }

    setEditMode(requestData) {
        this.isEditMode = true;
        $('#formTitle').text('Edit Cheque Book Request');
        $('#submitButtonText').text('Update Request');

        // Populate form with existing request data
        if (requestData) {
            // Implementation for editing existing request
        }
    }

    validateForm() {
        if (!$('#formCustomerId').val()) {
            this.showAlert('Please select a customer first', 'warning');
            return false;
        }

        if (!$('#categoryId').val()) {
            this.showAlert('Please select a cheque book category', 'warning');
            return false;
        }

        if (!$('#checkBookAccountDropdown').val()) {
            this.showAlert('Please select an account for the cheque book', 'warning');
            return false;
        }

        return true;
    }

    backToSearch() {
        if (window.searchManager) {
            window.searchManager.backToSearch();
        } else {
            $('#requestFormSection').hide();
            $('#searchSection').show();
        }
        this.resetForm();
    }

    resetForm() {
        this.isEditMode = false;
        const form = document.getElementById('chequeRequestForm');
        if (form) form.reset();
        $('#formTitle').text('Create Cheque Book Request');
        $('#submitButtonText').text('Submit Request');
        $('#formCustomerSummary').text('Customer will appear here');
    }

    showAlert(message, type) {
        const alertType = type === 'error' ? 4 : type === 'warning' ? 2 : 1;
        if (typeof appalert === 'function') {
            appalert(message, alertType, 1);
        } else {
            alert(message);
        }
    }
}