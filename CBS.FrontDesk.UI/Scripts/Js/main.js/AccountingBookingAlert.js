$(document).ready(function () {

    var configurationChecker = {
        checkInterval: 30 * 60 * 1000, // 30 minutes in milliseconds
        intervalId: null,
        bannerDismissed: false,

        init: function () {
            this.checkConfiguration();
            //this.checkFeeOrInComeConfiguration();
            this.startPeriodicCheck();
            this.bindEvents();
        },

        startPeriodicCheck: function () {
            var self = this;
            this.intervalId = setInterval(function () {
                self.checkConfiguration();
            }, this.checkInterval);
        },

        checkConfiguration: function () {
            var self = this;

            fetch('/AccountingConfiguration/CheckAccountConfigurationStatus', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! Status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log(data);
                    if (data.success && data.hasUnconfiguredProducts) {
                        self.showBanner(data.unconfiguredCount);
                    } else {
                        self.hideBanner();
                    }
                })
                .catch(error => {
                    console.warn('Configuration check failed:', error);
                });
        },

        //checkFeeOrInComeConfiguration: function () {
        //    var self = this;

        //    $.ajax({
        //        url: '/AccountingConfiguration/CheckFeeOrIncomeStatus',
        //        type: 'GET',
        //        dataType: 'json',
        //        success: function (response) {
        //            console.log(response);
        //            if (response.success && response.hasUnconfiguredProducts) {
        //                self.showBanner(response.unconfiguredCount);
        //            } else {
        //                self.hideBanner();
        //            }
        //        },
        //        error: function (xhr, status, error) {
        //            console.warn('Configuration check failed:', error);
        //        }
        //    });
        //},

        showBanner: function (count) {
            if (this.bannerDismissed) return;

            var message = count === 1
                ? '1 product is not configured with accounts.'
                :'⚠ '+ count + ' products are not configured some accounting entries might not impact your trial balance...';

            $('#bannerMessage').text(message);
            $('#configurationBanner').addClass('show');
        },

        hideBanner: function () {
            $('#configurationBanner').removeClass('show');
            this.bannerDismissed = false;
        },

        dismissBanner: function () {
            this.hideBanner();
            this.bannerDismissed = true;

            //// Reset dismissal after 1 hour so banner can show again if needed
            //setTimeout(() => {
            //    this.bannerDismissed = false;
            //}, 60 * 60 * 1000);
        },

        bindEvents: function () {
            var self = this;

            // Close banner button
            $('#closeBanner').click(function (e) {
                e.preventDefault();
                self.dismissBanner();
            });

            // Show unconfigured products modal
            $('#showUnconfiguredProducts').click(function (e) {
                e.preventDefault();
                self.showUnconfiguredProductsModal();
            });

            // Show all accounts button
            $('#showAllAccountsBtn').click(function (e) {
                e.preventDefault();
                self.showAllUnconfiguredAccounts();
            });

            // Handle product clicks in modal
            $(document).on('click', '.product-header', function () {
                var productId = $(this).data('product-id');
                productIdConfig = productId;
                self.showProductDetails(productId);
            });
        },

        showUnconfiguredProductsModal: function () {
            $('#modalContent').html('<div class="loading-spinner"><i class="glyphicon glyphicon-refresh glyphicon-spin"></i> Loading unconfigured products...</div>');
            $('#unconfiguredModal').modal('show');

            $.ajax({
                url: '/AccountingConfiguration/GetUnconfiguredProducts',
                type: 'GET',
                dataType: 'json',
                success: function (response) {
                    if (response.success) {
                        configurationChecker.renderUnconfiguredProducts(response.products);
                    } else {
                        $('#modalContent').html('<div class="error-message">Error loading products: ' + (response.error || 'Unknown error') + '</div>');
                    }
                },
                error: function (xhr, status, error) {
                    $('#modalContent').html('<div class="error-message">Failed to load unconfigured products. Please try again.</div>');
                }
            });
        },

        renderUnconfiguredProducts: function (products) {
            if (!products || products.length === 0) {
                $('#modalContent').html('<div class="alert alert-info">No unconfigured products found.</div>');
                return;
            }

            var html = '<div class="products-list">';

            products.forEach(function (product) {
                html += '<div class="product-item">';
                html += '<div class="product-header" data-product-id="' + product.productAccountBookId + '">';
                html += '<h5 class="product-name">' + $('<div>').text(product.productName).html() + '</h5>';
                html += '<div class="account-count">' + product.unconfiguredAccountsCount + ' unconfigured account(s) - Click to view details</div>';
                html += '</div>';
                html += '</div>';
            });

            html += '</div>';
            $('#modalContent').html(html);
        },

        showProductDetails: function (productId) {
            $('#productDetailsContent').html('<div class="loading-spinner"><i class="glyphicon glyphicon-refresh glyphicon-spin"></i> Loading product details...</div>');
            $('#productDetailsModal').modal('show');

            $.ajax({
                url: 'AccountingConfiguration/GetProductUnconfiguredAccounts',
                type: 'GET',
                dataType: 'json',
                data: { productId: productId },
                success: function (response) {
          
                    if (response.success) {
                        console.log(response);
                        configurationChecker.renderProductDetails(response);
                    } else {
                        $('#productDetailsContent').html('<div class="error-message">Error loading product details: ' + (response.error || 'Unknown error') + '</div>');
                    }
                },
                error: function (xhr, status, error) {
                    $('#productDetailsContent').html('<div class="error-message">Failed to load product details. Please try again.</div>');
                }
            });
        },

        renderProductDetails: function (productData) {
            $('#productIdConfig').text(productData.productAccountBookId);
            console.log(productData.productAccountBookId);
            $('#productDetailsModalLabel').text(`[${productData.productCode}] ${productData.productName} Configurations`);
            var html = '<h5>Product: ' + $('<div>').text('[' + productData.productCode + '] ' + productData.productName).html() + '</h5>';

            if (!productData.accountingBookDetails || productData.accountingBookDetails.length === 0) {
                html += '<div class="alert alert-info">All accounts not yet configured for this product.</div>';
            } else {
                html += '<ul class="account-list">';

                productData.accountingBookDetails.forEach(function (account) {
                    html += '<li class="account-item">';
                    html += '<div class="account-name">' + $('<div>').text(account.productName).html() + '</div>';
                    html += '<span class="account-type">' + $('<div>').text(account.productType).html() + '</span>';
                    html += '<div class="account-config">' + $('<div>').text(account.description).html() + '</div>';
                    html += '</li>';
                });

                html += '</ul>';
            }

            $('#productDetailsContent').html(html);
        },

        showAllUnconfiguredAccounts: function () {
            $('#modalContent').html('<div class="loading-spinner"><i class="glyphicon glyphicon-refresh glyphicon-spin"></i> Loading all unconfigured accounts...</div>');

            $.ajax({
                url: '@Url.Action("GetAllUnconfiguredAccounts", "ProductConfiguration")',
                type: 'GET',
                dataType: 'json',
                success: function (response) {
                    if (response.success) {
                        configurationChecker.renderAllUnconfiguredAccounts(response.accounts);
                    } else {
                        $('#modalContent').html('<div class="error-message">Error loading accounts: ' + (response.error || 'Unknown error') + '</div>');
                    }
                },
                error: function (xhr, status, error) {
                    $('#modalContent').html('<div class="error-message">Failed to load all unconfigured accounts. Please try again.</div>');
                }
            });
        },

        renderAllUnconfiguredAccounts: function (accounts) {
            if (!accounts || accounts.length === 0) {
                $('#modalContent').html('<div class="alert alert-info">No unconfigured accounts found.</div>');
                return;
            }
        }
    }

    // 🔥 Add this line to initialize the checker
    configurationChecker.init();

    $('#closeBanner1, #closeBanner').on('click', function (e) {
        e.preventDefault();
        $('#unconfiguredModal').modal('hide');

    });
    $('#closeBanner3, #closeBanner2').on('click', function (e) {
        e.preventDefault();

        $('#productDetailsModal').modal('hide');
    });
});

let productIdConfig = "0";
function GoToProductConfiguration() {
    // Open a new window/tab and navigate to the product configuration page /OldLoanAccountingMaping
    var productId = $('#productIdConfig').val();
    console.log(productId);

    $.ajax({
        url: 'AccountingConfiguration/GetProductUnconfiguredAccounts',
        type: 'GET',
        dataType: 'json',
        data: { productId: productIdConfig },
        success: function (response) {

            if (response.success) {
                if (response.productType==="LoanProduct") {
                    window.open('/OldLoanAccountingMaping', '_blank');
                } else {
                    window.open('/TransactionConfiguration/OrdinaryAccounts', '_blank');
                }
                configurationChecker.renderProductDetails(response);
            } else {
                $('#productDetailsContent').html('<div class="error-message">Error loading product details: ' + (response.error || 'Unknown error') + '</div>');
            }
        },
        error: function (xhr, status, error) {
            $('#productDetailsContent').html('<div class="error-message">Failed to load product details. Please try again.</div>');
        }
    });

}