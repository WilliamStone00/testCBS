// Global variable declaration
let selectedMonth = '';
const Url = '/DailyAgentManagement/RetrieveDailyCollectionDashboardActivitiesAsync';

$(document).ready(function () {
    // Month change handler
    $('#month').on('change', function () {
        selectedMonth = $(this).val(); // format will be "YYYY-MM"
        console.log('Month changed to:', selectedMonth);
    });

    // Branch change handler
    $(document).on('change', '#DashboardActivities_BranchId', function () {
        var branchId = $(this).val();
        loadAgentsByBranch(branchId, selectedMonth);
    });
});

async function loadDashBoardData() {
    // Show loading indicator
    $('.load-btn').prop('disabled', true).html('⏳ Loading...');

    // Clear any previous validation messages
    $('.text-danger').empty();

    // Collect form data
    var formData = {
        Month: $('#month').val(),
        BranchId: $('select[name="DashboardActivities.BranchId"]').val(),
        CollectorId: $('select[name="DashboardActivities.CollectorId"]').val(),
    };

    // Basic validation
    if (!formData.Month) {
        showValidationError('Month', 'Please select a month and year');
        $('.load-btn').prop('disabled', false).html('Load Data');
        return;
    }

    console.log('Form data:', formData);

    try {
        const dashboardUpdater = new DashboardUpdater(Url);
        await dashboardUpdater.fetchAndUpdateData(formData);
        showSuccessMessage('Data loaded successfully!');
    } catch (error) {
        console.error('Error loading dashboard data:', error);
        showErrorMessage(error.message || 'An error occurred while loading data');
    } finally {
        $('.load-btn').prop('disabled', false).html('Load Data');
    }
}

// Dashboard Auto-Update Functions
class DashboardUpdater {
    constructor(apiEndpoint) {
        this.apiEndpoint = apiEndpoint;
        this.updateInterval = 30 * 60 * 1000; // 30 minutes in milliseconds
        this.intervalId = null;
        this.isUpdating = false;
        this.lastUpdateTime = null;
        this.errorCount = 0;
        this.maxErrors = 3;
    }

    async fetchAndUpdateData(formData) {
        if (this.isUpdating) {
            console.log('Update already in progress, skipping...');
            return;
        }

        this.isUpdating = true;
        this.showLoadingState();

        try {
            const data = await this.fetchData(formData);
            this.updateDashboard(data);
            this.errorCount = 0; // Reset error count on successful update
            this.lastUpdateTime = new Date();
            this.showSuccessState();
            console.log('Dashboard updated successfully at:', this.lastUpdateTime);
            return data;
        } catch (error) {
            this.errorCount++;
            this.handleError(error);

            // Stop auto-update if too many errors
            if (this.errorCount >= this.maxErrors) {
                this.stopAutoUpdate();
                this.showErrorState('Too many failed attempts. Auto-update stopped.');
                throw error;
            }
        } finally {
            this.isUpdating = false;
            this.hideLoadingState();
        }
    }

    async fetchData(formData) {
        console.log("Fetching data with:", formData);

        // Create URL with query parameters
        const url = new URL(this.apiEndpoint, window.location.origin);
        Object.keys(formData).forEach(key => {
            if (formData[key]) { // Only add non-empty values
                url.searchParams.append(key, formData[key]);
            }
        });

        // Create AbortController for timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 seconds timeout

        try {
            const response = await fetch(url.toString(), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Cache-Control': 'no-cache'
                },
                signal: controller.signal
            });

            // Clear timeout if request completes
            clearTimeout(timeoutId);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Server returned ${response.status}: ${errorText}`);
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || 'API request was not successful');
            }

            return data.data || data; // Handle different response structures
        } catch (error) {
            // Clear timeout in case of error
            clearTimeout(timeoutId);
            console.error('Fetch error:', error);
            throw error;
        }
    }

    updateDashboard(data) {
        if (!data) {
            console.error('No data provided to update dashboard');
            return;
        }

        console.log('Updating dashboard with:', data);

        try {
            // Update key metrics
            this.updateMetrics(data);

            // Update charts if they exist
            if (window.dailyTrendChart || window.branchChart) {
                this.updateCharts(data);
            }

            // Update tables
            this.updateTables(data);

            // Update branch performance
            if (data.branchDashboards) {
                this.updateBranchPerformance(data);
            }

            // Update recent transactions
            if (data.transactions) {
                this.updateRecentTransactions(data);
            }

            // Update last updated timestamp
            this.updateTimestamp();
        } catch (error) {
            console.error('Error updating dashboard:', error);
            throw error;
        }
    }

    updateMetrics(data) {
        const metrics = {
            totalCollectedAmount: data.totalCollectedAmount || 0,
            totalFeeCollected: data.totalFeeCollected || 0,
            totalCashInCount: data.totalCashInCount || 0,
            totalMembersServed: data.totalMembersServed || 0,
            netBalance: data.netBalance || 0,
            averageCollectionPerSaver: data.averageCollectionPerSaver || 0
        };

        Object.keys(metrics).forEach(key => {
            const element = document.querySelector(`[data-metric="${key}"]`);
            if (element) {
                element.textContent = this.formatNumber(metrics[key]);
                this.animateValue(element);
            }
        });
    }

    updateCharts(data) {
        // Safely exit if data is not valid
        if (!data || typeof data !== 'object') return;

        // === Daily Trend Chart ===
        if (window.dailyTrendChart && data.dailySummaries) {
            const dailyData = this.prepareDailyTrendData(data.dailySummaries);
            window.dailyTrendChart.data.labels = dailyData.labels;
            window.dailyTrendChart.data.datasets[0].data = dailyData.cashIn;
            window.dailyTrendChart.data.datasets[1].data = dailyData.cashOut;
            window.dailyTrendChart.update();
        }

        // === Branch Performance Chart ===
        if (window.branchChart && data.topBranches) {
            const branchData = this.prepareBranchData(data.topBranches);
            window.branchChart.data.labels = branchData.labels;
            window.branchChart.data.datasets[0].data = branchData.amounts;
            window.branchChart.update();
        }
    }

    updateTables(data) {
        // Update top savers table
        if (data.topSavers) {
            this.updateTopSaversTable(data.topSavers);
        }

        // Update daily summary table
        if (data.dailySummaries) {
            this.updateDailySummaryTable(data.dailySummaries);
        }
    }

// Global variable declaration
let selectedMonth = '';
const Url = '/DailyAgentManagement/RetrieveDailyCollectionDashboardActivitiesAsync';

$(document).ready(function () {
    // Month change handler
    $('#month').on('change', function () {
        selectedMonth = $(this).val(); // format will be "YYYY-MM"
        console.log('Month changed to:', selectedMonth);
    });

    // Branch change handler
    $(document).on('change', '#DashboardActivities_BranchId', function () {
        var branchId = $(this).val();
        loadAgentsByBranch(branchId, selectedMonth);
    });
});

async function loadDashBoardData() {
    // Show loading indicator
    $('.load-btn').prop('disabled', true).html('⏳ Loading...');

    // Clear any previous validation messages
    $('.text-danger').empty();

    // Collect form data
    var formData = {
        Month: $('#month').val(),
        BranchId: $('select[name="DashboardActivities.BranchId"]').val(),
        CollectorId: $('select[name="DashboardActivities.CollectorId"]').val(),
    };

    // Basic validation
    if (!formData.Month) {
        showValidationError('Month', 'Please select a month and year');
        $('.load-btn').prop('disabled', false).html('Load Data');
        return;
    }

    console.log('Form data:', formData);

    try {
        const dashboardUpdater = new DashboardUpdater(Url);
        await dashboardUpdater.fetchAndUpdateData(formData);
        showSuccessMessage('Data loaded successfully!');
    } catch (error) {
        console.error('Error loading dashboard data:', error);
        showErrorMessage(error.message || 'An error occurred while loading data');
    } finally {
        $('.load-btn').prop('disabled', false).html('Load Data');
    }
}

// Dashboard Auto-Update Functions
class DashboardUpdater {
    constructor(apiEndpoint) {
        this.apiEndpoint = apiEndpoint;
        this.updateInterval = 30 * 60 * 1000; // 30 minutes in milliseconds
        this.intervalId = null;
        this.isUpdating = false;
        this.lastUpdateTime = null;
        this.errorCount = 0;
        this.maxErrors = 3;
    }

    async fetchAndUpdateData(formData) {
        if (this.isUpdating) {
            console.log('Update already in progress, skipping...');
            return;
        }

        this.isUpdating = true;
        this.showLoadingState();

        try {
            const data = await this.fetchData(formData);
            this.updateDashboard(data);
            this.errorCount = 0; // Reset error count on successful update
            this.lastUpdateTime = new Date();
            this.showSuccessState();
            console.log('Dashboard updated successfully at:', this.lastUpdateTime);
            return data;
        } catch (error) {
            this.errorCount++;
            this.handleError(error);

            // Stop auto-update if too many errors
            if (this.errorCount >= this.maxErrors) {
                this.stopAutoUpdate();
                this.showErrorState('Too many failed attempts. Auto-update stopped.');
                throw error;
            }
        } finally {
            this.isUpdating = false;
            this.hideLoadingState();
        }
    }

    async fetchData(formData) {
        console.log("Fetching data with:", formData);

        // Create URL with query parameters
        const url = new URL(this.apiEndpoint, window.location.origin);
        Object.keys(formData).forEach(key => {
            if (formData[key]) { // Only add non-empty values
                url.searchParams.append(key, formData[key]);
            }
        });

        // Create AbortController for timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 30000); // 30 seconds timeout

        try {
            const response = await fetch(url.toString(), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Cache-Control': 'no-cache'
                },
                signal: controller.signal
            });

            // Clear timeout if request completes
            clearTimeout(timeoutId);

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Server returned ${response.status}: ${errorText}`);
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || 'API request was not successful');
            }

            return data.data || data; // Handle different response structures
        } catch (error) {
            // Clear timeout in case of error
            clearTimeout(timeoutId);
            console.error('Fetch error:', error);
            throw error;
        }
    }

    updateDashboard(data) {
        if (!data) {
            console.error('No data provided to update dashboard');
            return;
        }

        console.log('Updating dashboard with:', data);

        try {
            // Update key metrics
            this.updateMetrics(data);

            // Update charts if they exist
            if (window.dailyTrendChart || window.branchChart) {
                this.updateCharts(data);
            }

            // Update tables
            this.updateTables(data);

            // Update branch performance
            if (data.branchDashboards) {
                this.updateBranchPerformance(data);
            }

            // Update recent transactions
            if (data.transactions) {
                this.updateRecentTransactions(data);
            }

            // Update last updated timestamp
            this.updateTimestamp();
        } catch (error) {
            console.error('Error updating dashboard:', error);
            throw error;
        }
    }

    updateMetrics(data) {
        const metrics = {
            totalCollectedAmount: data.totalCollectedAmount || 0,
            totalFeeCollected: data.totalFeeCollected || 0,
            totalCashInCount: data.totalCashInCount || 0,
            totalMembersServed: data.totalMembersServed || 0,
            netBalance: data.netBalance || 0,
            averageCollectionPerSaver: data.averageCollectionPerSaver || 0
        };

        Object.keys(metrics).forEach(key => {
            const element = document.querySelector(`[data-metric="${key}"]`);
            if (element) {
                element.textContent = this.formatNumber(metrics[key]);
                this.animateValue(element);
            }
        });
    }

    updateCharts(data) {
        // Safely exit if data is not valid
        if (!data || typeof data !== 'object') return;

        // === Daily Trend Chart ===
        if (window.dailyTrendChart && data.dailySummaries) {
            const dailyData = this.prepareDailyTrendData(data.dailySummaries);
            window.dailyTrendChart.data.labels = dailyData.labels;
            window.dailyTrendChart.data.datasets[0].data = dailyData.cashIn;
            window.dailyTrendChart.data.datasets[1].data = dailyData.cashOut;
            window.dailyTrendChart.update();
        }

        // === Branch Performance Chart ===
        if (window.branchChart && data.topBranches) {
            const branchData = this.prepareBranchData(data.topBranches);
            window.branchChart.data.labels = branchData.labels;
            window.branchChart.data.datasets[0].data = branchData.amounts;
            window.branchChart.update();
        }
    }

    updateTables(data) {
        // Update top savers table
        if (data.topSavers) {
            this.updateTopSaversTable(data.topSavers);
        }

        // Update daily summary table
        if (data.dailySummaries) {
            this.updateDailySummaryTable(data.dailySummaries);
        }
    }

    updateTopSaversTable(topSavers) {
        const tbody = document.querySelector('.table-container table tbody');
        if (!tbody) return;

        tbody.innerHTML = '';
        (topSavers.slice(0, 3)?.forEach(saver => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${saver.name || 'N/A'}</td>
                <td>${saver.code || 'N/A'}</td>
                <td class="amount">${this.formatNumber(saver.totalAmount || 0)}</td>
            `;
            tbody.appendChild(row);
        });
    }

    updateDailySummaryTable(dailySummaries) {
        const tables = document.querySelectorAll('.table-container');
        const dailySummaryTable = tables[1]?.querySelector('table tbody');
        if (!dailySummaryTable) return;

        dailySummaryTable.innerHTML = '';
        (dailySummaries.slice(0, 5))?.forEach(summary => {
            const date = new Date(summary.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })}</td>
                <td class="amount">${this.formatNumber(summary.cashInAmount || 0)}</td>
                <td class="amount negative">${this.formatNumber(summary.cashOutAmount || 0)}</td>
                <td class="amount">${this.formatNumber((summary.cashInAmount || 0) - (summary.cashOutAmount || 0))}</td>
            `;
            dailySummaryTable.appendChild(row);
        });
    }

    updateBranchPerformance(data) {
        const branchContainer = document.querySelector('.branch-performance');
        if (!branchContainer) return;

        branchContainer.innerHTML = '';
        (data.branchDashboards.slice(0, 3))?.forEach(branch => {
            if (branch.branchName) {
                const branchCard = document.createElement('div');
                branchCard.className = 'branch-card';
                branchCard.innerHTML = `
                    <div class="branch-name">${branch.branchName} (${branch.branchCode || 'N/A'})</div>
                    <div class="branch-metrics">
                        <div class="branch-metric">
                            <span>Total Collections:</span>
                            <span class="amount">${this.formatNumber(branch.totalCollectedAmount || 0)} XAF</span>
                        </div>
                        <div class="branch-metric">
                            <span>Transactions:</span>
                            <span>${branch.totalCashInCount || 0}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Members Served:</span>
                            <span>${branch.totalMembersServed || 0}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Fees Collected:</span>
                            <span class="amount">${this.formatNumber(branch.totalFeeCollected || 0)} XAF</span>
                        </div>
                    </div>
                `;
                branchContainer.appendChild(branchCard);
            }
        });
    }

    updateRecentTransactions(data) {
        const transactionTable = document.querySelector('.table-container.full-width table tbody');
        if (!transactionTable) return;

        transactionTable.innerHTML = '';
        (data.transactions.slice(0, 5))?.forEach(transaction => {
            const date = new Date(transaction.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}</td>
                <td>${transaction.memberName || 'N/A'}</td>
                <td><span class="status-badge ${this.getStatusClass(transaction.operationType)}">${transaction.operationType || 'N/A'}</span></td>
                <td class="amount ${transaction.operationType === 'CashOut' ? 'negative' : ''}">${this.formatNumber(transaction.amount || 0)}</td>
                <td>${this.formatNumber(transaction.fee || 0)}</td>
                <td>${transaction.branchName ? transaction.branchName.split(' ').pop() : 'N/A'}</td>
            `;
            transactionTable.appendChild(row);
        });
    }

    prepareDailyTrendData(dailySummaries) {
        const labels = [];
        const cashIn = [];
        const cashOut = [];

        if (!Array.isArray(dailySummaries)) {
            return { labels: [], cashIn: [], cashOut: [] };
        }

        dailySummaries.slice(0, 30).reverse().forEach(summary => {
            try {
                const date = new Date(summary.date);
                const formattedDate = `${date.toLocaleString('default', { month: 'short' })} ${date.getDate().toString().padStart(2, '0')}`;
                labels.push(formattedDate);
                cashIn.push(Number(summary.cashInAmount) || 0);
                cashOut.push(Number(summary.cashOutAmount) || 0);
            } catch (e) {
                console.error('Error processing daily summary:', e);
            }
        });

        return { labels, cashIn, cashOut };
    }

    prepareBranchData(topBranches) {
        const labels = [];
        const amounts = [];

        if (Array.isArray(topBranches)) {
            topBranches.forEach(branch => {
                labels.push(branch.name?.split(' ').pop() || 'Branch');
                amounts.push(branch.totalAmount || 0);
            });
        }

        return { labels, amounts };
    }

    getStatusClass(operationType) {
        if (!operationType) return 'status-unknown';

        switch (operationType.toLowerCase()) {
            case 'cashin':
                return 'status-cashin';
            case 'cashout':
                return 'status-cashout';
            case 'onboardingfee':
                return 'status-onboarding';
            default:
                return 'status-unknown';
        }
    }

    formatNumber(number) {
        return new Intl.NumberFormat('en-US').format(Number(number) || 0);
    }

    animateValue(element) {
        if (!element) return;

        element.style.transform = 'scale(1.1)';
        element.style.transition = 'transform 0.2s ease';
        setTimeout(() => {
            element.style.transform = 'scale(1)';
        }, 200);
    }

    updateTimestamp() {
        const now = new Date();
        const timeString = now.toLocaleTimeString();
        const dateString = now.toLocaleDateString();

        const dateInfo = document.querySelector('.date-info');
        if (dateInfo) {
            dateInfo.innerHTML = `
                <div>${now.toLocaleString('default', { month: 'long' })} ${now.getFullYear()} Report</div>
                <div>Last Updated: ${dateString} ${timeString}</div>
            `;
        }
    }

    showLoadingState() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'block';
        }
    }

    hideLoadingState() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'none';
        }
    }

    showSuccessState() {
        this.showNotification('Dashboard updated successfully!', 'success');
    }

    showErrorState(message) {
        this.showNotification(message || 'Failed to update dashboard', 'error');
    }

    showNotification(message, type = 'info') {
        // Implementation remains the same as original
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'error' ? '#e74c3c' : type === 'success' ? '#27ae60' : '#3498db'};
            color: white;
            padding: 15px 20px;
            border-radius: 5px;
            z-index: 1001;
            max-width: 300px;
            opacity: 0;
            transform: translateY(-20px);
            transition: all 0.3s ease;
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 100);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }

    handleError(error) {
        console.error('Dashboard update error:', error);
        let errorMessage = 'Failed to update dashboard';

        if (error.name === 'TypeError' && error.message.includes('fetch')) {
            errorMessage = 'Network error. Please check your connection.';
        } else if (error.message.includes('timeout')) {
            errorMessage = 'Request timeout. Please try again.';
        } else if (error.message) {
            errorMessage = error.message;
        }

        this.showErrorState(errorMessage);
        return Promise.reject(error);
    }

    startAutoUpdate() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }

        // Initial fetch
        this.fetchAndUpdateData();

        // Set up interval for subsequent updates
        this.intervalId = setInterval(() => {
            this.fetchAndUpdateData();
        }, this.updateInterval);

        console.log(`Auto-update started. Updates every ${this.updateInterval / 60000} minutes.`);
    }

    stopAutoUpdate() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        console.log('Auto-update stopped.');
    }

    pauseAutoUpdate() {
        this.stopAutoUpdate();
        console.log('Auto-update paused (tab hidden).');
    }

    resumeAutoUpdate() {
        this.startAutoUpdate();
        console.log('Auto-update resumed (tab visible).');
    }

    getStatus() {
        return {
            isRunning: this.intervalId !== null,
            lastUpdateTime: this.lastUpdateTime,
            errorCount: this.errorCount,
            nextUpdateIn: this.intervalId ? this.updateInterval - (Date.now() - (this.lastUpdateTime || Date.now())) : null
        };
    }
}

// Helper function to show error messages
function showErrorMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.error(message);
    } else {
        console.error(message);
        alert(message);
    }
}

// Helper function to show success messages
function showSuccessMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.success(message);
    } else {
        console.log(message);
    }
}

// Helper function to show validation errors
function showValidationError(field, message) {
    $(`[data-valmsg-for="${field}"]`).text(message);
}

function loadAgentsByBranch(branchId, month, preselectedAgentId = null) {
    console.log('Loading agents for branch:', branchId, 'month:', month);

    // Get all possible agent dropdown selectors
    var $agentDropdowns = $('#DailyCollectorCollectorId, #CollectorCollectorId, select[name="DashboardActivities.CollectorId"]');

    // If no branch selected, reset all dropdowns and return
    if (!branchId) {
        $agentDropdowns.each(function () {
            $(this).empty().append(
                $('<option>').val('').text('--- Select Agent ---')
            ).trigger('change');

            // Handle Select2 if present
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy').select2();
            }
        });
        return;
    }

    // Show loading state
    $agentDropdowns.each(function () {
        $(this).empty().append(
            $('<option>').val('').text('Loading agents...')
        ).trigger('change');
    });

    // Load agents by branch and month
    $.ajax({
        url: '/DailyAgentManagement/GetActiveAgentBYBranch',
        type: 'GET',
        dataType: 'json',
        data: {
            branchId: branchId,
            month: month
        },
        success: function (response) {
            console.log('Received agents response:', response);

            // Handle different response structures
            var agentData = response.data || response;

            if (!Array.isArray(agentData)) {
                console.error('Expected array of agents, got:', typeof agentData);
                showErrorMessage('Invalid agent data received');
                return;
            }

            // Clear and populate all agent dropdowns
            $agentDropdowns.each(function () {
                var $dropdown = $(this);

                // Clear existing options
                $dropdown.empty().append(
                    $('<option>').val('').text('--- Select Agent ---')
                );

                // Add agent options
                $.each(agentData, function (index, item) {
                    // Handle different data structures
                    var value = item.Value || item.value || item.id || '';
                    var text = item.Text || item.text || item.name || 'Unknown Agent';

                    $dropdown.append(
                        $('<option>').val(value).text(text)
                    );
                });

                // Reinitialize Select2 if needed
                if ($dropdown.hasClass('select2-hidden-accessible') || $dropdown.data('select2')) {
                    $dropdown.select2('destroy').select2();
                }

                // Optional: Preselect agent
                if (preselectedAgentId) {
                    $dropdown.val(preselectedAgentId).trigger('change');
                }
            });

            console.log('Agent dropdowns populated successfully');
        },
        error: function (xhr, status, error) {
            console.error('Error loading agents:', error);
            console.log('XHR Status:', xhr.status);
            console.log('Response Text:', xhr.responseText);

            // Reset dropdowns to error state
            $agentDropdowns.each(function () {
                $(this).empty().append(
                    $('<option>').val('').text('Error loading agents')
                ).trigger('change');
            });

            showErrorMessage('Failed to load agents for selected branch');
        }
    });
}

// Initialize dashboard when DOM is ready
$(document).ready(function () {
    // Initialize the dashboard updater
    const dashboardUpdater = new DashboardUpdater(Url);

    // Make the updater globally accessible for debugging
    window.dashboardUpdater = dashboardUpdater;

    // Add manual controls if needed
    if ($('#dashboardControls').length === 0) {
        $('body').append(`
            <div id="dashboardControls" style="position: fixed; bottom: 20px; right: 20px; z-index: 1000;">
                <button id="refreshBtn" class="btn btn-primary">Refresh Now</button>
                <button id="pauseBtn" class="btn btn-danger">Pause Updates</button>
                <button id="resumeBtn" class="btn btn-success">Resume Updates</button>
            </div>
        `);

        $('#refreshBtn').click(() => dashboardUpdater.fetchAndUpdateData());
        $('#pauseBtn').click(() => dashboardUpdater.stopAutoUpdate());
        $('#resumeBtn').click(() => dashboardUpdater.startAutoUpdate());
    }
});

    updateDailySummaryTable(dailySummaries) {
        const tables = document.querySelectorAll('.table-container');
        const dailySummaryTable = tables[1]?.querySelector('table tbody');
        if (!dailySummaryTable) return;

        dailySummaryTable.innerHTML = '';
        (dailySummaries.slice(0, 5))?.forEach(summary => {
            const date = new Date(summary.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })}</td>
                <td class="amount">${this.formatNumber(summary.cashInAmount || 0)}</td>
                <td class="amount negative">${this.formatNumber(summary.cashOutAmount || 0)}</td>
                <td class="amount">${this.formatNumber((summary.cashInAmount || 0) - (summary.cashOutAmount || 0))}</td>
            `;
            dailySummaryTable.appendChild(row);
        });
    }

    updateBranchPerformance(data) {
        const branchContainer = document.querySelector('.branch-performance');
        if (!branchContainer) return;

        branchContainer.innerHTML = '';
        (data.branchDashboards.slice(0, 3))?.forEach(branch => {
            if (branch.branchName) {
                const branchCard = document.createElement('div');
                branchCard.className = 'branch-card';
                branchCard.innerHTML = `
                    <div class="branch-name">${branch.branchName} (${branch.branchCode || 'N/A'})</div>
                    <div class="branch-metrics">
                        <div class="branch-metric">
                            <span>Total Collections:</span>
                            <span class="amount">${this.formatNumber(branch.totalCollectedAmount || 0)} XAF</span>
                        </div>
                        <div class="branch-metric">
                            <span>Transactions:</span>
                            <span>${branch.totalCashInCount || 0}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Members Served:</span>
                            <span>${branch.totalMembersServed || 0}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Fees Collected:</span>
                            <span class="amount">${this.formatNumber(branch.totalFeeCollected || 0)} XAF</span>
                        </div>
                    </div>
                `;
                branchContainer.appendChild(branchCard);
            }
        });
    }

    updateRecentTransactions(data) {
        const transactionTable = document.querySelector('.table-container.full-width table tbody');
        if (!transactionTable) return;

        transactionTable.innerHTML = '';
        (data.transactions.slice(0, 5))?.forEach(transaction => {
            const date = new Date(transaction.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}</td>
                <td>${transaction.memberName || 'N/A'}</td>
                <td><span class="status-badge ${this.getStatusClass(transaction.operationType)}">${transaction.operationType || 'N/A'}</span></td>
                <td class="amount ${transaction.operationType === 'CashOut' ? 'negative' : ''}">${this.formatNumber(transaction.amount || 0)}</td>
                <td>${this.formatNumber(transaction.fee || 0)}</td>
                <td>${transaction.branchName ? transaction.branchName.split(' ').pop() : 'N/A'}</td>
            `;
            transactionTable.appendChild(row);
        });
    }

    prepareDailyTrendData(dailySummaries) {
        const labels = [];
        const cashIn = [];
        const cashOut = [];

        if (!Array.isArray(dailySummaries)) {
            return { labels: [], cashIn: [], cashOut: [] };
        }

        dailySummaries.slice(0, 30).reverse().forEach(summary => {
            try {
                const date = new Date(summary.date);
                const formattedDate = `${date.toLocaleString('default', { month: 'short' })} ${date.getDate().toString().padStart(2, '0')}`;
                labels.push(formattedDate);
                cashIn.push(Number(summary.cashInAmount) || 0);
                cashOut.push(Number(summary.cashOutAmount) || 0);
            } catch (e) {
                console.error('Error processing daily summary:', e);
            }
        });

        return { labels, cashIn, cashOut };
    }

    prepareBranchData(topBranches) {
        const labels = [];
        const amounts = [];

        if (Array.isArray(topBranches)) {
            topBranches.forEach(branch => {
                labels.push(branch.name?.split(' ').pop() || 'Branch');
                amounts.push(branch.totalAmount || 0);
            });
        }

        return { labels, amounts };
    }

    getStatusClass(operationType) {
        if (!operationType) return 'status-unknown';

        switch (operationType.toLowerCase()) {
            case 'cashin':
                return 'status-cashin';
            case 'cashout':
                return 'status-cashout';
            case 'onboardingfee':
                return 'status-onboarding';
            default:
                return 'status-unknown';
        }
    }

    formatNumber(number) {
        return new Intl.NumberFormat('en-US').format(Number(number) || 0);
    }

    animateValue(element) {
        if (!element) return;

        element.style.transform = 'scale(1.1)';
        element.style.transition = 'transform 0.2s ease';
        setTimeout(() => {
            element.style.transform = 'scale(1)';
        }, 200);
    }

    updateTimestamp() {
        const now = new Date();
        const timeString = now.toLocaleTimeString();
        const dateString = now.toLocaleDateString();

        const dateInfo = document.querySelector('.date-info');
        if (dateInfo) {
            dateInfo.innerHTML = `
                <div>${now.toLocaleString('default', { month: 'long' })} ${now.getFullYear()} Report</div>
                <div>Last Updated: ${dateString} ${timeString}</div>
            `;
        }
    }

    showLoadingState() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'block';
        }
    }

    hideLoadingState() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'none';
        }
    }

    showSuccessState() {
        this.showNotification('Dashboard updated successfully!', 'success');
    }

    showErrorState(message) {
        this.showNotification(message || 'Failed to update dashboard', 'error');
    }

    showNotification(message, type = 'info') {
        // Implementation remains the same as original
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${type === 'error' ? '#e74c3c' : type === 'success' ? '#27ae60' : '#3498db'};
            color: white;
            padding: 15px 20px;
            border-radius: 5px;
            z-index: 1001;
            max-width: 300px;
            opacity: 0;
            transform: translateY(-20px);
            transition: all 0.3s ease;
        `;
        notification.textContent = message;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateY(0)';
        }, 100);

        // Remove after 3 seconds
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }

    handleError(error) {
        console.error('Dashboard update error:', error);
        let errorMessage = 'Failed to update dashboard';

        if (error.name === 'TypeError' && error.message.includes('fetch')) {
            errorMessage = 'Network error. Please check your connection.';
        } else if (error.message.includes('timeout')) {
            errorMessage = 'Request timeout. Please try again.';
        } else if (error.message) {
            errorMessage = error.message;
        }

        this.showErrorState(errorMessage);
        return Promise.reject(error);
    }

    startAutoUpdate() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
        }

        // Initial fetch
        this.fetchAndUpdateData();

        // Set up interval for subsequent updates
        this.intervalId = setInterval(() => {
            this.fetchAndUpdateData();
        }, this.updateInterval);

        console.log(`Auto-update started. Updates every ${this.updateInterval / 60000} minutes.`);
    }

    stopAutoUpdate() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        console.log('Auto-update stopped.');
    }

    pauseAutoUpdate() {
        this.stopAutoUpdate();
        console.log('Auto-update paused (tab hidden).');
    }

    resumeAutoUpdate() {
        this.startAutoUpdate();
        console.log('Auto-update resumed (tab visible).');
    }

    getStatus() {
        return {
            isRunning: this.intervalId !== null,
            lastUpdateTime: this.lastUpdateTime,
            errorCount: this.errorCount,
            nextUpdateIn: this.intervalId ? this.updateInterval - (Date.now() - (this.lastUpdateTime || Date.now())) : null
        };
    }
}

// Helper function to show error messages
function showErrorMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.error(message);
    } else {
        console.error(message);
        alert(message);
    }
}

// Helper function to show success messages
function showSuccessMessage(message) {
    if (typeof toastr !== 'undefined') {
        toastr.success(message);
    } else {
        console.log(message);
    }
}

// Helper function to show validation errors
function showValidationError(field, message) {
    $(`[data-valmsg-for="${field}"]`).text(message);
}

function loadAgentsByBranch(branchId, month, preselectedAgentId = null) {
    console.log('Loading agents for branch:', branchId, 'month:', month);

    // Get all possible agent dropdown selectors
    var $agentDropdowns = $('#DailyCollectorCollectorId, #CollectorCollectorId, select[name="DashboardActivities.CollectorId"]');

    // If no branch selected, reset all dropdowns and return
    if (!branchId) {
        $agentDropdowns.each(function () {
            $(this).empty().append(
                $('<option>').val('').text('--- Select Agent ---')
            ).trigger('change');

            // Handle Select2 if present
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy').select2();
            }
        });
        return;
    }

    // Show loading state
    $agentDropdowns.each(function () {
        $(this).empty().append(
            $('<option>').val('').text('Loading agents...')
        ).trigger('change');
    });

    // Load agents by branch and month
    $.ajax({
        url: '/DailyAgentManagement/GetActiveAgentBYBranch',
        type: 'GET',
        dataType: 'json',
        data: {
            branchId: branchId,
            month: month
        },
        success: function (response) {
            console.log('Received agents response:', response);

            // Handle different response structures
            var agentData = response.data || response;

            if (!Array.isArray(agentData)) {
                console.error('Expected array of agents, got:', typeof agentData);
                showErrorMessage('Invalid agent data received');
                return;
            }

            // Clear and populate all agent dropdowns
            $agentDropdowns.each(function () {
                var $dropdown = $(this);

                // Clear existing options
                $dropdown.empty().append(
                    $('<option>').val('').text('--- Select Agent ---')
                );

                // Add agent options
                $.each(agentData, function (index, item) {
                    // Handle different data structures
                    var value = item.Value || item.value || item.id || '';
                    var text = item.Text || item.text || item.name || 'Unknown Agent';

                    $dropdown.append(
                        $('<option>').val(value).text(text)
                    );
                });

                // Reinitialize Select2 if needed
                if ($dropdown.hasClass('select2-hidden-accessible') || $dropdown.data('select2')) {
                    $dropdown.select2('destroy').select2();
                }

                // Optional: Preselect agent
                if (preselectedAgentId) {
                    $dropdown.val(preselectedAgentId).trigger('change');
                }
            });

            console.log('Agent dropdowns populated successfully');
        },
        error: function (xhr, status, error) {
            console.error('Error loading agents:', error);
            console.log('XHR Status:', xhr.status);
            console.log('Response Text:', xhr.responseText);

            // Reset dropdowns to error state
            $agentDropdowns.each(function () {
                $(this).empty().append(
                    $('<option>').val('').text('Error loading agents')
                ).trigger('change');
            });

            showErrorMessage('Failed to load agents for selected branch');
        }
    });
}

// Initialize dashboard when DOM is ready
$(document).ready(function () {
    // Initialize the dashboard updater
    const dashboardUpdater = new DashboardUpdater(Url);

    // Make the updater globally accessible for debugging
    window.dashboardUpdater = dashboardUpdater;

    // Add manual controls if needed
    if ($('#dashboardControls').length === 0) {
        $('body').append(`
            <div id="dashboardControls" style="position: fixed; bottom: 20px; right: 20px; z-index: 1000;">
                <button id="refreshBtn" class="btn btn-primary">Refresh Now</button>
                <button id="pauseBtn" class="btn btn-danger">Pause Updates</button>
                <button id="resumeBtn" class="btn btn-success">Resume Updates</button>
            </div>
        `);

        $('#refreshBtn').click(() => dashboardUpdater.fetchAndUpdateData());
        $('#pauseBtn').click(() => dashboardUpdater.stopAutoUpdate());
        $('#resumeBtn').click(() => dashboardUpdater.startAutoUpdate());
    }
});