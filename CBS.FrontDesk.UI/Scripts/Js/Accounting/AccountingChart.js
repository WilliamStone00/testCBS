(function () {
    // Configuration
    const config = {
        treeContainer: '#accountsTree',
        loadBtn: '#refreshTree',
        expandBtn: '#expandAll',
        collapseBtn: '#collapseAll',
        formSelector: '#editAccountForm',
        noSelection: '#noSelection',
        detailsLoading: '#detailsLoading',
        detailsPane: '#accountInfo',
        closeDetailsBtn: '#closeDetails',
        closeAccountBtn: '#closeAccountBtn',
        treeLoading: '#treeLoading'
    };

    // URLs
    const urls = {
        tree: '/ChartOfAccountV2/GetTreeData',
        accountDetails: '/ChartOfAccountV2/GetAccountDetails',
        update: '/ChartOfAccountV2/UpdateAccountName',
        close: '/ChartOfAccountV2/CloseAccount'
    };

    // State
    let currentLanguage = 'en';
    let currentTree = null;

    // Initialize
    $(document).ready(function () {
        initializeEvents();
        loadChartOfAccounts(); // Auto-load on page ready
    });

    function initializeEvents() {
        $(document).on('click', config.loadBtn, loadChartOfAccounts);
        $(document).on('click', config.expandBtn, expandAllNodes);
        $(document).on('click', config.collapseBtn, collapseAllNodes);
        $(document).on('click', config.closeDetailsBtn, closeDetailsPanel);
        $(document).on('click', config.closeAccountBtn, closeAccount);
        $(document).on('submit', config.formSelector, handleFormSubmit);
        $(document).on('change', '#languageToggle', toggleLanguage);
    }

    function loadChartOfAccounts() {
        showElement(config.treeLoading);
        hideElement(config.treeContainer);

        $.ajax({
            url: urls.tree,
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                initializeOrRefreshTree(data);
                hideElement(config.treeLoading);
                showElement(config.treeContainer);
            },
            error: function (xhr, status, err) {
                console.error('LoadChartOfAccounts error:', err);
                showError('Failed to load chart of accounts. Please try again.');
                hideElement(config.treeLoading);
                showElement(config.treeContainer);
            }
        });
    }

    function initializeOrRefreshTree(data) {
        if ($.jstree && $(config.treeContainer).data('jstree')) {
            currentTree = $(config.treeContainer).jstree(true);
            currentTree.settings.core.data = data;
            currentTree.refresh();
        } else {
            currentTree = $(config.treeContainer).jstree({
                core: {
                    data: data,
                    multiple: false,
                    themes: {
                        name: 'default',
                        responsive: true,
                        dots: true,
                        icons: true
                    },
                    check_callback: true
                },
                plugins: ['wholerow', 'types'],
                types: {
                    default: { icon: 'jstree-folder' },
                    file: { icon: 'jstree-file' }
                }
            });

            // Node selection event
            currentTree.on('select_node.jstree', function (e, data) {
                const node = data.node;
                if (node && node.id) {
                    loadAccountDetails(node.id, node);
                }
            });

            // Node loaded event - fix icons
            currentTree.on('loaded.jstree', function () {
                fixTreeIcons();
            });
        }
    }

    function loadAccountDetails(accountId, node = null) {
        showElement(config.detailsLoading);
        hideElement(config.noSelection);
        hideElement(config.detailsPane);

        $.ajax({
            url: urls.accountDetails,
            type: 'GET',
            data: { id: accountId },
            success: function (response) {
                if (response && response.success) {
                    displayAccountDetails(response.data, node);
                } else {
                    showError(response.message || 'Failed to load account details.');
                    showElement(config.noSelection);
                }
                hideElement(config.detailsLoading);
            },
            error: function (xhr) {
                console.error('LoadAccountDetails error:', xhr);
                showError('Error loading account details.');
                hideElement(config.detailsLoading);
                showElement(config.noSelection);
            }
        });
    }

    function displayAccountDetails(account, node) {
        // Fill form fields
        $('#accountId').val(account.id);
        $('#accountCode').val(account.code);
        $('#accountNameEn').val(account.nameEn);
        $('#accountNameFr').val(account.nameFr);
        $('#accountClass').val(account.class);
        $('#accountDepth').val(account.depth);
        $('#postingAllowed').prop('checked', account.postingAllowed);

        // Show/hide close account button based on business rules
        toggleCloseAccountButton(account);

        // Show details panel
        hideElement(config.noSelection);
        hideElement(config.detailsLoading);
        showElement(config.detailsPane);
        showElement(config.closeDetailsBtn);

        // Update UI based on account type
        updateUIForAccountType(account);
    }

    function toggleCloseAccountButton(account) {
        // Business logic: Only show close button for certain account types/depths
        const canClose = account.depth > 0 && account.postingAllowed;
        if (canClose) {
            showElement(config.closeAccountBtn);
        } else {
            hideElement(config.closeAccountBtn);
        }
    }

    function updateUIForAccountType(account) {
        // Add visual indicators based on account properties
        const form = $(config.formSelector);
        form.removeClass('border-warning border-danger border-success');

        if (!account.postingAllowed) {
            form.addClass('border-warning');
        } else if (account.depth === 0) {
            form.addClass('border-success');
        }
    }

    function handleFormSubmit(e) {
        e.preventDefault();

        const accountId = $('#accountId').val();
        if (!accountId) {
            showError('No account selected.');
            return false;
        }

        const payload = {
            Id: accountId,
            NameEn: $('#accountNameEn').val().trim(),
            NameFr: $('#accountNameFr').val().trim()
        };

        if (!payload.NameEn) {
            showError('English name is required.');
            return false;
        }

        $.ajax({
            url: urls.update,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (response) {
                if (response && response.success) {
                    updateTreeAfterEdit(accountId, payload.NameEn);
                    showSuccess(response.message || 'Account updated successfully.');
                } else {
                    showError(response.message || 'Update failed.');
                }
            },
            error: function (xhr) {
                console.error('Update error', xhr);
                showError('Error updating account. Please try again.');
            }
        });

        return false;
    }

    function updateTreeAfterEdit(accountId, newName) {
        if (currentTree) {
            const node = currentTree.get_node(accountId);
            if (node) {
                const codePart = node.text.split(' - ')[0];
                const newText = `${codePart} - ${newName}`;
                currentTree.rename_node(accountId, newText);

                // Update node data
                node.original['data-name-en'] = newName;
                node.li_attr['data-name-en'] = newName;
            }
        }
    }

    function closeAccount() {
        const accountId = $('#accountId').val();
        if (!accountId) return;

        if (!confirm('Are you sure you want to close this account? This action may be irreversible.')) {
            return;
        }

        $.ajax({
            url: urls.close,
            type: 'POST',
            data: { id: accountId },
            success: function (response) {
                if (response && response.success) {
                    showSuccess(response.message);
                    closeDetailsPanel();
                    loadChartOfAccounts(); // Refresh tree
                } else {
                    showError(response.message || 'Failed to close account.');
                }
            },
            error: function (xhr) {
                console.error('Close account error:', xhr);
                showError('Error closing account.');
            }
        });
    }

    function closeDetailsPanel() {
        hideElement(config.detailsPane);
        hideElement(config.closeDetailsBtn);
        hideElement(config.detailsLoading);
        showElement(config.noSelection);

        if (currentTree) {
            currentTree.deselect_all();
        }
    }

    function expandAllNodes() {
        if (currentTree) {
            currentTree.open_all();
        }
    }

    function collapseAllNodes() {
        if (currentTree) {
            currentTree.close_all();
        }
    }

    function toggleLanguage() {
        currentLanguage = currentLanguage === 'en' ? 'fr' : 'en';
        $('#languageLabel').text(currentLanguage === 'en' ? 'English' : 'French');
        // Implement language switching logic here
    }

    function fixTreeIcons() {
        // Ensure all tree nodes have proper icons
        $(config.treeContainer).find('.jstree-node').each(function () {
            const $node = $(this);
            const $icon = $node.find('.jstree-icon');

            if ($icon.length === 0 || !$icon.attr('class').includes('jstree-themeicon')) {
                const postingAllowed = $node.find('.jstree-anchor').data('postingallowed');
                const depth = $node.find('.jstree-anchor').data('depth') || 0;

                let iconClass = postingAllowed ? 'jstree-file' : 'jstree-folder';
                if (depth === 0) iconClass = 'jstree-root text-warning';

                $node.find('.jstree-anchor').prepend(`<i class="${iconClass}"></i> `);
            }
        });
    }

    // Utility functions
    function showElement(selector) {
        $(selector).show();
    }

    function hideElement(selector) {
        $(selector).hide();
    }

    function showSuccess(message) {
        $('#successMessage').text(message || 'Operation completed successfully.');
        $('#successModal').modal('show');
    }

    function showError(message) {
        $('#errorMessage').text(message || 'An error occurred.');
        $('#errorModal').modal('show');
    }
})();