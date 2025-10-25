class SearchManager {
    constructor() {
        this.init();
    }

    init() {
        $('#searchCustomerBtn').on('click', () => this.searchCustomer());
        $('#customerIdInput').on('keypress', (e) => {
            if (e.which === 13) this.searchCustomer();
        });
    }

    async searchCustomer() {
        const customerId = $('#customerIdInput').val().trim();
        if (!customerId) {
            appalert('Please enter a Customer ID', 3, 1);
            return;
        }

        const searchBtn = $('#searchCustomerBtn');
        const originalHtml = searchBtn.html();
        searchBtn.prop('disabled', true).html();

        try {
            // Single call to get the partial view HTML
            const formHtml = await $.get(getRequestFormUrl, { customerId: customerId });

            // Check if we got valid HTML (not an error page)
            if (formHtml && formHtml.includes('chequeRequestForm')) {
                // Display customer details from the form data
                this.displayCustomerDetailsFromForm(formHtml);

                // Inject the form HTML
                $('#requestFormSection').html(formHtml).show();

                // Initialize form manager if available
                if (window.formManager && typeof window.formManager.init === 'function') {
                    window.formManager.init();
                }
            } else {
                appalert('Customer not found or form could not be loaded', 3, 1);
                this.hideCustomerDetails();
                this.hideRequestForm();
            }

        } catch (err) {
            console.error('Search error:', err);
            appalert('An error occurred while searching for customer', 4, 1);
            this.hideCustomerDetails();
            this.hideRequestForm();
        } finally {
            searchBtn.prop('disabled', false).html(originalHtml);
        }
    }

    // Helper method to extract customer info from the form HTML
    displayCustomerDetailsFromForm(formHtml) {
        // Create a temporary element to parse the HTML
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = formHtml;

        // Try to find customer information in the form
        const customerIdInput = tempDiv.querySelector('#formCustomerId');
        const customerNameInput = tempDiv.querySelector('#formCustomerName');
        const customerSummary = tempDiv.querySelector('#formCustomerSummary');

        if (customerIdInput && customerIdInput.value) {
            const customerName = customerNameInput ? customerNameInput.value : 'Customer';
            $('#customerSummary').text(`${customerName} (${customerIdInput.value})`);
            $('#accountSummary').text('Accounts loaded in form');
            $('#customerDetailsCard').show();
        }
    }

    hideCustomerDetails() {
        $('#customerDetailsCard').hide();
    }

    hideRequestForm() {
        $('#requestFormSection').hide().empty();
    }

    resetSearch() {
        $('#customerIdInput').val('');
        this.hideCustomerDetails();
        this.hideRequestForm();
    }
}

window.searchManager = new SearchManager();


