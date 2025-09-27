// wwwroot/Scripts/Js/Crud/ChequeManagement/ChequeRequest/form-manager.js
class FormManager {
    constructor() {
        this.isEditMode = false;
        this.init();
    }

    init() {
        // Back to search button - event delegation since form is loaded dynamically
        $(document).on('click', '#backToSearchBtn', () => this.backToSearch());
        this.interceptGenericAjax();
    }

    interceptGenericAjax() {
        // Store original function if it exists
        if (typeof window.AjaxPostAndUpdate === 'function') {
            const originalAjaxPostAndUpdate = window.AjaxPostAndUpdate;

            // Override to add custom validation
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
        $('#formCustomerSummary').html(`
            <i class="mdi mdi-account-outline me-2"></i>
            <strong>${customer.customerId} - ${customer.firstName} ${customer.lastName}</strong>
        `);

        this.populateAccountDropdowns(accounts);
    }

    populateAccountDropdowns(accounts) {
        const $check = $('#checkBookAccountDropdown');
        const $sub = $('#subscriptionPaymentAccountDropdown');

        $check.empty().append('<option value="">Select Account</option>');
        $sub.empty().append('<option value="">Select Account</option>');

        if (accounts && accounts.length > 0) {
            const activeAccounts = accounts.filter(acc => acc.status && acc.status.toLowerCase() === 'active');

            activeAccounts.forEach(account => {
                const balance = account.balance ? parseFloat(account.balance).toLocaleString() : '0.00';
                const text = `${account.accountNumber} - ${account.accountName} (${account.accountType}) - ${balance}`;
                $check.append(new Option(text, account.accountNumber));
                $sub.append(new Option(text, account.accountNumber));
            });

            // Auto-select first account if available
            if (activeAccounts.length > 0) {
                $check.val(activeAccounts[0].accountNumber);
                $sub.val(activeAccounts[0].accountNumber);
            }
        }
    }

    setEditMode(requestId) {
        this.isEditMode = true;
        $('#formTitle').text('Edit Cheque Book Request');
        $('#submitButtonText').text('Update Request');

        // Load request data for editing
        this.loadRequestData(requestId);
    }

    loadRequestData(requestId) {
        $.get('/ChequeRequest/InitializeData', {
            KEY: requestId,
            partialView: '_ChequeRequestForm',
            path: 'get'
        }, (html) => {
            $('#requestFormSection').html(html);
            showRequestFormSection();
        });
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
        showSearchSection();
        this.resetForm();
    }

    resetForm() {
        this.isEditMode = false;
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