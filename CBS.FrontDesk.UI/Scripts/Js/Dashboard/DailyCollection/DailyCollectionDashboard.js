
$(document).ready(function () {

    $(document).ready(function () {
        $('#month').on('change', function () {
            selectedMonth = $(this).val(); // format will be "YYYY-MM"
            console.log('Month changed to:', selectedMonth);
        });
    });
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
    const dashboardUpdater = new DashboardUpdater('/DailyAgentManagement/RetrieveDailyCollectionDashboardActivitiesAsync');
    // Basic validation

    if (!formData.Month) {
        showValidationError('Month', 'Please select a month and year');
        return;
    }
    console.log(formData);
    // AJAX POST request
    $.ajax({
        url: '/DailyAgentManagement/RetrieveDailyCollectionDashboardActivitiesAsync', // Replace with your actual endpoint
        type: 'GET',
        data: {
            Month: formData.Month,
            BranchId: formData.BranchId,
            CollectorId: formData.CollectorId,
        },

        dataType: 'json',

        beforeSend: function () {

            // Optional: Add loading spinner or overlay
        },
        success: function (response) {
            console.log('Success:', response.data);
            // Handle successful response
            if (response.success) {
                // Update your UI with the returned data      
                dashboardUpdater.updateDashboard(response.data);
                dashboardUpdater.errorCount = 0; // Reset error count on successful update
                dashboardUpdater.lastUpdateTime = new Date();
                dashboardUpdater.showSuccessState();
                showSuccessMessage('Data loaded successfully!');

            } else {

                // Handle server-side validation errors

                if (response.errors) {

                    displayValidationErrors(response.errors);

                } else {

                    showErrorMessage(response.message || 'An error occurred while loading data');

                }

            }

        },

        error: function (xhr, status, error) {

            console.group('AJAX Error - Attempt ' + (this.retryCount || 1));

            console.log('Status:', status);

            console.log('Error:', error);

            console.log('HTTP Status Code:', xhr.status);

            console.log('Response Text:', xhr.responseText);

            console.groupEnd();

            // Initialize retry count

            this.retryCount = this.retryCount || 0;

            // Retry for network errors or server errors (but not client errors)

            if ((xhr.status === 0 || xhr.status >= 500) && this.retryCount < 3) {

                this.retryCount++;

                console.log('Retrying request... Attempt ' + this.retryCount);

                // Retry after a delay

                setTimeout(() => {

                    $.ajax(this);

                }, 1000 * this.retryCount); // Exponential backoff

                return;

            }

            // Handle the error after retries are exhausted

            let errorMessage = getErrorMessage(xhr, status, error);

            showErrorMessage(errorMessage);

        },

        complete: function () {

       

        }

    });

}


// Dashboard Auto-Update Functions
class DashboardUpdater {
    constructor(apiEndpoint) {
        this.apiEndpoint = apiEndpoint;
        this.updateInterval = 30 * 60 * 1000; // 10 minutes in milliseconds
        this.intervalId = null;
        this.isUpdating = false;
        this.lastUpdateTime = null;
        this.errorCount = 0;
        this.maxErrors = 3;

        // Initialize the updater
        this.init();
    }

    init() {
        // Start auto-update
        this.startAutoUpdate();

        // Add event listeners for visibility change (pause when tab is hidden)
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.pauseAutoUpdate();
            } else {
                this.resumeAutoUpdate();
            }
        });

        // Add manual refresh button if it exists
        const refreshBtn = document.getElementById('refreshBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.fetchAndUpdateData());
        }
    }

    async fetchData() {
        var formData = {

            Month: $('#month').val(),

            BranchId: $('select[name="DashboardActivities.BranchId"]').val(),

            CollectorId: $('select[name="DashboardActivities.CollectorId"]').val(),

        };
        console.log("Month:" + formData.Month + " BranchId:" + formData.BranchId + " CollectorId:" + formData.CollectorId);
        try {
            const response = await fetch(this.apiEndpoint, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Cache-Control': 'no-cache'
                },
                data: {

                    Month: formData.Month,

                    BranchId: formData.BranchId,

                    CollectorId: formData.CollectorId,

                },
                timeout: 30000 // 30 seconds timeout
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            if (data.status !== 'SUCCESS') {
                throw new Error(data.message || 'API returned unsuccessful status');
            }

            return data.data;
        } catch (error) {
            console.error('Error fetching data:', error);
            throw error;
        }
    }

    async fetchAndUpdateData() {
        if (this.isUpdating) {
            console.log('Update already in progress, skipping...');
            return;
        }

        this.isUpdating = true;
        this.showLoadingState();

        try {
            const data = await this.fetchData();
            this.updateDashboard(data);
            this.errorCount = 0; // Reset error count on successful update
            this.lastUpdateTime = new Date();
            this.showSuccessState();

            console.log('Dashboard updated successfully at:', this.lastUpdateTime);
        } catch (error) {
            this.errorCount++;
            this.handleError(error);

            // Stop auto-update if too many errors
            if (this.errorCount >= this.maxErrors) {
                this.stopAutoUpdate();
                this.showErrorState('Too many failed attempts. Auto-update stopped.');
            }
        } finally {
            this.isUpdating = false;
            this.hideLoadingState();
        }
    }
   

updateDashboard(data)
{
    console.log('Data to update dashboard:' + data);
        // Update key metrics
        this.updateMetrics(data);

        // Update charts
        this.updateCharts(data);

        // Update tables
        this.updateTables(data);

        // Update branch performance
        this.updateBranchPerformance(data);

        // Update recent transactions
        this.updateRecentTransactions(data);

        // Update last updated timestamp
        this.updateTimestamp();
    }

    updateMetrics(data) {
        const metrics = {
            totalCollectedAmount: data.totalCollectedAmount,
            totalFeeCollected: data.totalFeeCollected,
            totalCashInCount: data.totalCashInCount,
            totalMembersServed: data.totalMembersServed,
            netBalance: data.netBalance,
            averageCollectionPerSaver: data.averageCollectionPerSaver
        };

        Object.keys(metrics).forEach(key => {
            const element = document.querySelector(`[data-metric="${key}"]`) ||
                document.querySelector('.metric-value');
            if (element) {
                element.textContent = this.formatNumber(metrics[key]);
                this.animateValue(element);
            }
        });

        // Update specific metric cards
        const metricCards = document.querySelectorAll('.metric-card');
        if (metricCards.length >= 6) {
            metricCards[0].querySelector('.metric-value').textContent = this.formatNumber(data.totalCollectedAmount);
            metricCards[1].querySelector('.metric-value').textContent = this.formatNumber(data.totalFeeCollected);
            metricCards[2].querySelector('.metric-value').textContent = data.totalCashInCount;
            metricCards[3].querySelector('.metric-value').textContent = data.totalMembersServed;
            metricCards[4].querySelector('.metric-value').textContent = this.formatNumber(data.netBalance);
            metricCards[5].querySelector('.metric-value').textContent = this.formatNumber(data.averageCollectionPerSaver);
        }
    }
 
    updateCharts(data) {
        // Safely exit if data is not valid
        if (!data || typeof data !== 'object') return;

        // === Daily Trend Chart ===
        if (window.dailyTrendChart) {
            const summaries = Array.isArray(data.dailySummaries) ? data.dailySummaries : [];
            const dailyData = this.prepareDailyTrendData(summaries);

            window.dailyTrendChart.data.labels = dailyData.labels;
            window.dailyTrendChart.data.datasets[0].data = dailyData.cashIn;
            window.dailyTrendChart.data.datasets[1].data = dailyData.cashOut;
            window.dailyTrendChart.update();
        }

        // === Branch Performance Chart ===
        if (window.branchChart) {
            const branches = Array.isArray(data.topBranches) ? data.topBranches : [];
            const branchData = this.prepareBranchData(branches);

            window.branchChart.data.labels = branchData.labels;
            window.branchChart.data.datasets[0].data = branchData.amounts;
            window.branchChart.update();
        }
    }

    updateTables(data) {
        // Update top savers table
        this.updateTopSaversTable(data.topSavers);

        // Update daily summary table
        this.updateDailySummaryTable(data.dailySummaries);
    }

    updateTopSaversTable(topSavers) {
        const tbody = document.querySelector('.table-container table tbody');
        if (!tbody) return;

        tbody.innerHTML = '';
        topSavers.slice(0, 3).forEach(saver => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${saver.name}</td>
                <td>${saver.code}</td>
                <td class="amount">${this.formatNumber(saver.totalAmount)}</td>
            `;
            tbody.appendChild(row);
        });
    }

    updateDailySummaryTable(dailySummaries) {
        const tables = document.querySelectorAll('.table-container');
        const dailySummaryTable = tables[1]?.querySelector('table tbody');
        if (!dailySummaryTable) return;

        dailySummaryTable.innerHTML = '';
        dailySummaries.slice(0, 5).forEach(summary => {
            const date = new Date(summary.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })}</td>
                <td class="amount">${this.formatNumber(summary.cashInAmount)}</td>
                <td class="amount negative">${this.formatNumber(summary.cashOutAmount)}</td>
                <td class="amount">${this.formatNumber(summary.cashInAmount - summary.cashOutAmount)}</td>
            `;
            dailySummaryTable.appendChild(row);
        });
    }

    updateBranchPerformance(data) {
        const branchContainer = document.querySelector('.branch-performance');
        if (!branchContainer) return;

        branchContainer.innerHTML = '';
        data.branchDashboards.slice(0, 3).forEach(branch => {
            if (branch.branchName) { // Skip empty branch names
                const branchCard = document.createElement('div');
                branchCard.className = 'branch-card';
                branchCard.innerHTML = `
                    <div class="branch-name">${branch.branchName} (${branch.branchCode})</div>
                    <div class="branch-metrics">
                        <div class="branch-metric">
                            <span>Total Collections:</span>
                            <span class="amount">${this.formatNumber(branch.totalCollectedAmount)} XAF</span>
                        </div>
                        <div class="branch-metric">
                            <span>Transactions:</span>
                            <span>${branch.totalCashInCount}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Members Served:</span>
                            <span>${branch.totalMembersServed}</span>
                        </div>
                        <div class="branch-metric">
                            <span>Fees Collected:</span>
                            <span class="amount">${this.formatNumber(branch.totalFeeCollected)} XAF</span>
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
        data.transactions.slice(0, 5).forEach(transaction => {
            const date = new Date(transaction.date);
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${date.getDate()} ${date.toLocaleString('default', { month: 'short' })} ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}</td>
                <td>${transaction.memberName}</td>
                <td><span class="status-badge ${this.getStatusClass(transaction.operationType)}">${transaction.operationType}</span></td>
                <td class="amount ${transaction.operationType === 'CashOut' ? 'negative' : ''}">${this.formatNumber(transaction.amount)}</td>
                <td>${this.formatNumber(transaction.fee)}</td>
                <td>${transaction.branchName ? transaction.branchName.split(' ').pop() : 'N/A'}</td>
            `;
            transactionTable.appendChild(row);
        });
    }

   
    prepareDailyTrendData(dailySummaries) {
        const labels = [];
        const cashIn = [];
        const cashOut = [];

        if (!Array.isArray(dailySummaries) || dailySummaries.length === 0) {
            return { labels: [], cashIn: [], cashOut: [] };
        }

        dailySummaries.slice(0, 30).reverse().forEach(summary => {
            const timestamp = parseInt(summary.date.match(/\d+/)[0]); // Parse "/Date(...)"
            const date = new Date(timestamp);

            const formattedDate = `${date.toLocaleString('default', { month: 'short' })} ${date.getDate().toString().padStart(2, '0')}`;
            labels.push(formattedDate);

            cashIn.push(Number(summary.cashInAmount) || 0);
            cashOut.push(Number(summary.cashOutAmount) || 0);
        });

        return { labels, cashIn, cashOut };
    }

    prepareBranchData(topBranches) {
        const labels = [];
        const amounts = [];

        topBranches.forEach(branch => {
            labels.push(branch.name.split(' ').pop()); // Get last word (simplified name)
            amounts.push(branch.totalAmount);
        });

        return { labels, amounts };
    }

    getStatusClass(operationType) {
        switch (operationType) {
            case 'CashIn':
                return 'status-cashin';
            case 'CashOut':
                return 'status-cashout';
            case 'OnboardingFee':
                return 'status-onboarding';
            default:
                return 'status-cashin';
        }
    }

    formatNumber(number) {
        return new Intl.NumberFormat('en-US').format(number);
    }

    animateValue(element) {
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

        // Update the header date info
        const dateInfo = document.querySelector('.date-info');
        if (dateInfo) {
            dateInfo.innerHTML = `
                <div>July 2025 Report</div>
                <div>Last Updated: ${dateString} ${timeString}</div>
            `;
        }
    }

    showLoadingState() {
        const loadingIndicator = document.getElementById('loadingIndicator');
        if (loadingIndicator) {
            loadingIndicator.style.display = 'block';
        } else {
            // Create loading indicator if it doesn't exist
            const loader = document.createElement('div');
            loader.id = 'loadingIndicator';
            loader.innerHTML = `
                <div style="position: fixed; top: 20px; right: 20px; background: #3498db; color: white; padding: 10px 20px; border-radius: 5px; z-index: 1000;">
                    <i class="fas fa-spinner fa-spin"></i> Updating...
                </div>
            `;
            document.body.appendChild(loader);
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

        if (error.name === 'TypeError' && error.message.includes('fetch')) {
            this.showErrorState('Network error. Please check your connection.');
        } else if (error.message.includes('timeout')) {
            this.showErrorState('Request timeout. Please try again.');
        } else {
            this.showErrorState('Failed to update dashboard. Please try again.');
        }
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

        console.log('Auto-update started. Updates every 10 minutes.');
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

// Usage Example and Initialization
document.addEventListener('DOMContentLoaded', function () {
    // Initialize the dashboard updater
    const API_ENDPOINT = '/DailyAgentManagement/RetrieveDailyCollectionDashboardActivitiesAsync'; // Replace with your actual API endpoint
    const dashboardUpdater = new DashboardUpdater(API_ENDPOINT);

    // Make the updater globally accessible for debugging
    window.dashboardUpdater = dashboardUpdater;

    // Add manual controls (optional)
    const controlsHtml = `
        <div id="dashboardControls" style="position: fixed; bottom: 20px; right: 20px; z-index: 1000;">
            <button id="refreshBtn" style="background: #3498db; color: white; border: none; padding: 10px 15px; margin: 5px; border-radius: 5px; cursor: pointer;">
                Refresh Now
            </button>
            <button id="pauseBtn" style="background: #e74c3c; color: white; border: none; padding: 10px 15px; margin: 5px; border-radius: 5px; cursor: pointer;">
                Pause Updates
            </button>
            <button id="resumeBtn" style="background: #27ae60; color: white; border: none; padding: 10px 15px; margin: 5px; border-radius: 5px; cursor: pointer;">
                Resume Updates
            </button>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', controlsHtml);

    // Add event listeners for manual controls
    document.getElementById('pauseBtn').addEventListener('click', () => {
        dashboardUpdater.stopAutoUpdate();
    });

    document.getElementById('resumeBtn').addEventListener('click', () => {
        dashboardUpdater.startAutoUpdate();
    });

    // Console commands for debugging
    console.log('Dashboard updater initialized. Available commands:');
    console.log('- dashboardUpdater.getStatus() - Get current status');
    console.log('- dashboardUpdater.fetchAndUpdateData() - Manual update');
    console.log('- dashboardUpdater.stopAutoUpdate() - Stop auto-updates');
    console.log('- dashboardUpdater.startAutoUpdate() - Start auto-updates');
});

// Service Worker for background updates (optional)
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js').then(function (registration) {
        console.log('Service Worker registered successfully');
    }).catch(function (error) {
        console.log('Service Worker registration failed:', error);
    });
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

// Corrected loadAgentsByBranch function
function loadAgentsByBranch(branchId, month, preselectedAgentId = null) {
    console.log('Loading agents for branch:', branchId, 'month:', month);

    // Get all possible agent dropdown selectors
    var $agentDropdowns = $('#DailyCollectorCollectorId, #CollectorCollectorId, select[name="DashboardActivities.CollectorId"]');

    // If no branch selected, reset all dropdowns and return
    if (!branchId) {
        $agentDropdowns.each(function () {
            $(this).empty().append(
                $('<option>').val('').text('--- Select Agent ---')
            );

            // Handle Select2 if present
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy');
                $(this).select2();
            }
        });
        return;
    }

    // Show loading state
    $agentDropdowns.each(function () {
        $(this).empty().append(
            $('<option>').val('').text('Loading agents...')
        );
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
                toastr.error('Invalid agent data received');
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
                    var value = item.Value;
                    var text = item.Text;
                    console.log( value);
                    console.log(text);
                 
                        $dropdown.append(
                            $('<option>').val(value).text(text)
                        );
                  
                });

                // Reinitialize Select2 if present
                if ($dropdown.hasClass('select2-hidden-accessible')) {
                    $dropdown.select2('destroy');
                }

                // Initialize Select2 if the class exists
                if ($dropdown.hasClass('select2') || $dropdown.data('select2')) {
                    $dropdown.select2();
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
                );
            });

            toastr.error('Failed to load agents for selected branch');
        }
    });
}