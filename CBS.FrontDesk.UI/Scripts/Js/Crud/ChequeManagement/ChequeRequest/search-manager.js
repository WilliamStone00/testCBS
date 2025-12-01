// Scripts/Js/Crud/ChequeManagement/ChequeRequest/search-manager.js
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
        searchBtn.prop('disabled', true).html('<i class="mdi mdi-loading mdi-spin"></i> Searching...');

        try {
            // Single call to get the partial view with pre-populated data
            const formHtml = await $.get(getRequestFormUrl, { customerId: customerId });

            // Check if we got valid HTML
            if (formHtml && formHtml.includes('chequeRequestForm')) {
                // Inject the form HTML directly below search
                $('#requestFormSection').html(formHtml).show();

                // Initialize form manager
                if (window.formManager) {
                    window.formManager.init();
                }

                // Optional: Scroll to form for better UX
                $('html, body').animate({
                    scrollTop: $('#requestFormSection').offset().top - 20
                }, 500);

                appalert('Customer found! Please fill out the request form below.', 1, 1);

            } else {
                appalert('Customer not found. Please check the Customer ID and try again.', 3, 1);
                this.hideForm();
            }

        } catch (err) {
            console.error('Search error:', err);
            appalert('An error occurred while searching for customer. Please try again.', 4, 1);
            this.hideForm();
        } finally {
            searchBtn.prop('disabled', false).html(originalHtml);
        }
    }

    hideForm() {
        $('#requestFormSection').hide().empty();
    }

    resetSearch() {
        $('#customerIdInput').val('');
        this.hideForm();
    }
}

window.SearchManager = SearchManager;