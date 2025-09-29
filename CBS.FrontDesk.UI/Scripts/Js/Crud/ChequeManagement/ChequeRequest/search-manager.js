// wwwroot/Scripts/Js/Crud/ChequeManagement/ChequeRequest/search-manager.js
class SearchManager {
    constructor() {
        this.currentCustomer = null;
        this.init();
    }

    init() {
        $('#searchCustomerBtn').on('click', () => this.searchCustomer());
        $('#customerIdInput').on('keypress', (e) => {
            if (e.which === 13) this.searchCustomer();
        });
        
        $('#proceedToRequestBtn').on('click', () => this.proceedToRequest());
    }

    async searchCustomer() {
        const branchId = $('#searchBranchId').val();
        const customerId = $('#customerIdInput').val().trim();

        if (!customerId) {
            this.showAlert('Please enter Customer ID', 'warning');
            return;
        }

        // Show loading state
        $('#searchCustomerBtn').prop('disabled', true).html('<i class="mdi mdi-loading mdi-spin"></i> Searching...');

        try {
            const response = await $.ajax({
                url: '/ChequeRequest/GetCustomerDetails',
                type: 'GET',
                data: { customerId: customerId, branchId: branchId }
            });

            if (response.success) {
                this.currentCustomer = response.data;
                this.displayCustomerDetails(response.data);
            } else {
                this.showAlert(response.message || 'Customer not found', 'error');
                $('#customerDetailsCard').hide();
            }
        } catch (error) {
            this.showAlert('Error searching for customer', 'error');
            $('#customerDetailsCard').hide();
        } finally {
            $('#searchCustomerBtn').prop('disabled', false).html('<i class="mdi mdi-magnify"></i> Search');
        }
    }

    displayCustomerDetails(customerData) {
        const customer = customerData.customerDto;
        const accounts = customerData.accountDtos || [];
        
        $('#customerSummary').html(`
            <strong>${customer.customerId}</strong> - ${customer.firstName} ${customer.lastName}
            <br><small class="text-muted">${customer.email} | ${customer.phone}</small>
        `);
        $('#accountSummary').text(`${accounts.length} account(s) available`);
        $('#customerDetailsCard').show();
    }

    proceedToRequest() {
        if (!this.currentCustomer) {
            this.showAlert('No customer data available', 'warning');
            return;
        }

        // Load the request form dynamically
        this.loadRequestForm();
    }

    loadRequestForm() {
        $.get('/ChequeRequest/InitializeData', {
            partialView: '_ChequeRequestForm',
            path: 'new'
        }, (html) => {
            $('#requestFormSection').html(html);
            showRequestFormSection();
            
            // Populate form with customer data
            if (window.formManager) {
                window.formManager.populateForm(this.currentCustomer);
            }
        });
    }

    resetSearch() {
        this.currentCustomer = null;
        $('#customerIdInput').val('');
        $('#customerDetailsCard').hide();
        $('#searchBranchId').val('').trigger('change');
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