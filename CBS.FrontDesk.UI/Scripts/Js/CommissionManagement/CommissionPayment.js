$(document).ready(function () {
    $("#toggleBtn").on("click", function () {
        const table = $(".members-table");

        table.slideToggle(300, function () {
            // Update button text after the animation completes
            if (table.is(":visible")) {
                $("#toggleBtn").text("Hide Member Activities");
            } else {
                $("#toggleBtn").text("Show Member Activities");
            }
        });
    });
});
function createDailyCollectionConfig(month, branch, collector, operationType, memberReference) {
    const dailyCollectionConfig = {
        Zone: {},
        CollectorSalaryInfo: {
            CollectorId: collector,
            BranchId: branch,
            Month: month,
            OperationType: operationType,
            MemberReference: memberReference
        },
        Zones: [],
        CommissionSetting: {
        },
        CommissionSettings: [],
        AgentDailyCashLimit: {
        },

        AgentDailyCashLimits: [

        ],

        ServiceOption: "AGENTACTIVITIES",
        Action: "",
        KEY: ""
    };
    return dailyCollectionConfig; 
}

function loadData() {
    // Show loading indicator
    $('.load-btn').prop('disabled', true).html('⏳ Loading...');

    // Clear any previous validation messages
    $('.text-danger').empty();

    // Collect form data
    var formData = {
        Month: $('#month').val(),
        BranchId: $('select[name="CollectorSalarySummary.BranchId"]').val(),
        OperationType: $('select[name="CollectorSalarySummary.OperationType"]').val(),
        CollectorId: $('select[name="CollectorSalarySummary.CollectorId"]').val(),
        MemberReference: $('#MemberReference').val()
    };

    // Basic validation
    if (!formData.Month) {
        showValidationError('Month', 'Please select a month and year');
        resetButton();
        return;
    }
    console.log(formData);
    // AJAX POST request
    $.ajax({
        url: '/DailyAgentManagement/RetrieveAgentActivitiesAsync', // Replace with your actual endpoint
        type: 'GET',
         data: {
        Month: formData.Month,
        BranchId: formData.BranchId,
        CollectorId: formData.CollectorId,
        OperationType: formData.OperationType,
        MemberReference: formData.MemberReference
    }, 
        dataType: 'json',
        beforeSend: function () {
            // Optional: Add loading spinner or overlay
            console.log('Sending data:', createDailyCollectionConfig(formData.Month, formData.BranchId, formData.CollectorId, formData.OperationType, formData.MemberReference) );
        },
        success: function (response) {
            console.log('Success:', response.data);

            // Handle successful response
            if (response.success) {
                // Update your UI with the returned data
                renderCollectorSalarySummary(response.data)
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
            // Reset button state
            resetButton();
        }
    });
}

function renderCollectorSalarySummary(data) {
    // Show container
    $("#results").removeClass("hidden");

    // 👤 Collector Info
    $("#collectorName").text(data.CollectorName || "N/A");
    $("#collectorId").text(data.CollectorId || "N/A");
    $("#branchInfo").text(`${data.BranchName} (${data.BranchCode})`);
    $("#periodInfo").text(data.Month || "N/A");
    $("#operationType").text(data.OperationType || "N/A");

    // 📊 Summary Stats
    $("#totalMembers").text(data.TotalMembersWithActivity);
    $("#totalCollected").text(`${data.TotalValueCollected.toLocaleString()} FCFA`);
    $("#totalFees").text(`${data.TotalFeeCharged.toLocaleString()} FCFA`);
    $("#totalDistribute").text(`${data.TotalAmountToDistribute.toLocaleString()} FCFA`);

    // 👥 Member Activity Table
    const $membersTableBody = $("#membersTableBody").empty();
    if (Array.isArray(data.MemberStats) && data.MemberStats.length > 0) {
        data.MemberStats.forEach(member => {
            const row = `
                <tr>
                    <td>${member.MemberId}</td>
                    <td>${member.MemberName}</td>
                    <td>${member.TotalActivityAmount.toLocaleString()} FCFA</td>
                    <td>${member.FeeCharged.toLocaleString()} FCFA</td>
                    <td>${formatDate(member.LastTransactionDate)}</td>
                </tr>
            `;
            $membersTableBody.append(row);
        });
    } else {
        $membersTableBody.append('<tr><td colspan="5">No activity found for this period.</td></tr>');
    }

    // 💼 Revenue Distribution by Stakeholder
    const $stakeholderGrid = $(".stakeholder-grid").empty();
    if (Array.isArray(data.SharedAmounts)) {
        data.SharedAmounts.forEach(item => {
            let icon = "💼";
            let title = item.Stakeholder;
            const name = item.Stakeholder.toLowerCase();

            if (name.includes("cam")) icon = "🏦";
            else if (name.includes("daily")) icon = "👤";
            else if (name.includes("branch")) title = "Branch Commission", icon = "🏢";
            else if (name.includes("incentive")) icon = "🎯";

            const card = `
                <div class="stakeholder-card">
                    <h4>${icon} ${title}</h4>
                    <div class="amount">${item.Amount.toLocaleString()} FCFA</div>
                    <div class="percentage">(${item.Percentage}%)</div>
                </div>
            `;
            $stakeholderGrid.append(card);
        });
    }
}

// Optional date formatter helper
function formatDate(dateStr) {
    if (!dateStr) return "—";

    // 1. Handle /Date(1752521491779)/
    const msMatch = dateStr.match(/\/Date\((\d+)(?:[+-]\d+)?\)\//);
    if (msMatch) {
        const ms = parseInt(msMatch[1]);
        const date = new Date(ms);
        return formatToFrench(date);
    }

    // 2. Handle US-style: M/D/YYYY h:mm:ss AM/PM
    const usMatch = dateStr.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4}) (\d{1,2}):(\d{2}):(\d{2}) (AM|PM)$/i);
    if (usMatch) {
        let [, month, day, year, hour, minute, second, meridiem] = usMatch;
        hour = parseInt(hour);
        if (meridiem.toUpperCase() === "PM" && hour < 12) hour += 12;
        if (meridiem.toUpperCase() === "AM" && hour === 12) hour = 0;

        const isoStr = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}T${hour.toString().padStart(2, '0')}:${minute}:${second}`;
        const date = new Date(isoStr);
        return !isNaN(date) ? formatToFrench(date) : "—";
    }

    // 3. Fallback to Date parser
    const fallback = new Date(dateStr);
    return !isNaN(fallback) ? formatToFrench(fallback) : "—";
}

// Formats a JS Date to "DD MMM YYYY" using French locale
function formatToFrench(date) {
    return date.toLocaleDateString("fr-FR", {
        year: "numeric",
        month: "short",
        day: "numeric"
    });
}



// Helper function to get appropriate error message
function getErrorMessage(xhr, status, error) {
    // Try to get message from response
    if (xhr.responseText) {
        const responseText = xhr.responseText.trim();

        if (!responseText.startsWith('<')) {
            try {
                const responseData = JSON.parse(responseText);
                if (responseData.message) return responseData.message;
                if (responseData.error) return responseData.error;
            } catch (e) {
                // Not JSON, use raw text if it's short
                if (responseText.length < 200) {
                    return responseText;
                }
            }
        }
    }

    // Default messages based on status
    const statusMessages = {
        0: 'Network error. Please check your connection.',
        400: 'Invalid request. Please check your input.',
        401: 'Authentication required. Please log in.',
        403: 'Access denied.',
        404: 'Resource not found.',
        408: 'Request timeout. Please try again.',
        500: 'Server error. Please try again later.',
        502: 'Bad gateway. Please try again later.',
        503: 'Service unavailable. Please try again later.',
        504: 'Gateway timeout. Please try again later.'
    };

    return statusMessages[xhr.status] || 'An unexpected error occurred. Please try again.';
}
// Helper function to reset button state
function resetButton() {
    $('.load-btn').prop('disabled', false).html('🔍 Load Data');
}

// Helper function to show validation error for specific field
function showValidationError(fieldName, message) {
    var validationElement = $('span[data-valmsg-for="CollectorSalarySummary.' + fieldName + '"]');
    if (validationElement.length === 0) {
        // Fallback: find by field name pattern
        validationElement = $('input[name*="' + fieldName + '"], select[name*="' + fieldName + '"]')
            .closest('.filter-group')
            .find('.text-danger');
    }
    validationElement.text(message);
}

// Helper function to display multiple validation errors
function displayValidationErrors(errors) {
    $.each(errors, function (field, messages) {
        if (Array.isArray(messages)) {
            showValidationError(field, messages.join(', '));
        } else {
            showValidationError(field, messages);
        }
    });
}

// Helper function to show success message
function showSuccessMessage(message) {
    // You can customize this based on your UI framework
    alert(message); // Replace with toast notification or custom modal
}

// Helper function to show error message
function showErrorMessage(message) {
    // You can customize this based on your UI framework
    alert(message); // Replace with toast notification or custom modal
}

// Helper function to display the loaded data
function displayData(data) {
    // Implement based on how you want to display the data
    // For example, populate a table, chart, or other UI elements
    console.log('Displaying data:', data);

    // Example: If you have a results container
    // $('#results-container').html(generateDataTable(data));
}



function updateDataByOperationType(operationType, operationData) {
    const data = operationData[operationType];

    if (!data) {
        appalert(`No data found for operation type "${operationType}"`, 2, 2);
        return;
    }

    document.getElementById('totalMembers').textContent = data.totalMembers;
    document.getElementById('totalCollected').textContent = data.totalCollected;
    document.getElementById('totalFees').textContent = data.totalFees;
    document.getElementById('totalDistribute').textContent = data.totalDistribute;

    const tableBody = document.getElementById('membersTableBody');
    tableBody.innerHTML = '';

    data.members.forEach(member => {
        const row = tableBody.insertRow();
        row.innerHTML = `
            <td>${member.id}</td>
            <td>${member.name}</td>
            <td class="amount">${member.amount} FCFA</td>
            <td class="amount">${member.fee} FCFA</td>
            <td>${member.date}</td>
        `;
    });

    const totalDistribute = parseFloat(data.totalDistribute.replace(/,/g, ''));
    updateStakeholderDistribution(totalDistribute);
}


function updateStakeholderDistribution(totalAmount) {
    const distributions = [
        { percentage: 50, amount: totalAmount * 0.5 },
        { percentage: 30, amount: totalAmount * 0.3 },
        { percentage: 15, amount: totalAmount * 0.15 },
        { percentage: 5, amount: totalAmount * 0.05 }
    ];

    const stakeholderCards = document.querySelectorAll('.stakeholder-card .amount');
    const percentageElements = document.querySelectorAll('.stakeholder-card .percentage');

    distributions.forEach((dist, index) => {
        if (stakeholderCards[index]) {
            stakeholderCards[index].textContent = formatNumber(Math.round(dist.amount)) + ' FCFA';
            percentageElements[index].textContent = `(${dist.percentage}%)`;
        }
    });
}


function processPayment() {
    const collectorName = document.getElementById('collectorName').textContent;
    const amount = document.getElementById('totalDistribute').textContent;

    if (confirm(`Are you sure you want to process payment of ${amount} FCFA to ${collectorName}?`)) {
        // Show processing animation
        const payBtn = document.querySelector('.payment-btn');
        payBtn.textContent = '⏳ Processing Payment...';
        payBtn.disabled = true;

        // Simulate payment processing
        setTimeout(() => {
            alert(`✅ Payment of ${amount} FCFA successfully processed to ${collectorName}!`);
            payBtn.textContent = '✅ Payment Completed';
            payBtn.style.background = '#28a745';
        }, 2000);
    }
}

// Format numbers with commas
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
 
// Initialize with current month
document.getElementById('month').value = new Date().toISOString().slice(0, 7);
