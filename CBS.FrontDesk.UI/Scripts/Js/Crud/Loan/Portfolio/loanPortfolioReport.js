// Generate Report Handler
$('#generateReportBtn').on('click', function () {
    let btn = $(this);
    btn.prop('disabled', true)
        .html('<i class="mdi mdi-loading mdi-spin"></i> Generating...');

    const payload = buildLoanPortfolioReportPayload();

    console.log("===== LOAN REPORT PAYLOAD =====", payload);

    $.ajax({
        url: '/Loan/LoanPortfolioStatistics',
        type: 'POST',
        data: payload,
        success: function (response) {
            if (response.success && response.redirectUrl) {
                window.open(response.redirectUrl, '_blank');
                if (typeof appalert === 'function') {
                    appalert("✅ Viewer launched in a new tab.", 1, 1);
                } else {
                    alert("Report generated successfully!");
                }
            } else {
                if (typeof appalert === 'function') {
                    appalert(response.message || "❌ Failed to launch report viewer.", 3, 1);
                } else {
                    alert(response.message || "Failed to generate report");
                }
            }
        },
        error: function (xhr) {
            const errorMsg = "❌ Server error: " + (xhr.responseText || "Unexpected error.");
            if (typeof appalert === 'function') {
                appalert(errorMsg, 3, 1);
            } else {
                alert(errorMsg);
            }
        },
        complete: function () {
            btn.prop('disabled', false)
                .html('<i class="mdi mdi-file-chart-outline me-1"></i> Generate Report');
        }
    });
});

function buildLoanPortfolioReportPayload() {
    const isMainReport = $('#toggleReportType').is(':checked');
    const isLoanReport = $('#toggleloanreports').is(':checked');
    const isSSFReport = $('#toggleSSF').is(':checked');
    const isDurationReport = $('#toggleDuration').is(':checked');
    const isPARReport = $('#togglePAR').is(':checked');

    // Determine which report type is active
    let reportType = null;
    let mainReportType = null;

    if (isMainReport) {
        mainReportType = $('#MainReportType').val();
    } else if (isLoanReport) {
        reportType = $('#ReportType').val();
    } else if (isSSFReport) {
        reportType = $('#SSFReportType').val();
    } else if (isDurationReport) {
        reportType = $('#DurationType').val();
    } else if (isPARReport) {
        reportType = $('#PARType').val();
    }

    const subReports = $('#SubReportType').val() || [];

    // Build payload matching GenerateLoanPortfolioReportCommand
    const payload = {
        // Base properties
        BranchId: $('#byBranch').is(':checked') ? $('#BranchId').val() : null,
        StartDate: $('#byDate').is(':checked') ? $('#StartDate').val() : null,
        EndDate: $('#byDate').is(':checked') ? $('#EndDate').val() : null,
        ReportDownloadType: 'PDF', // Default to PDF

        // Report type properties
        ReportType: reportType,
        MainReportType: mainReportType,

        // Sub reports (comma-separated for multi-select)
        SubReportType: subReports.length > 0 ? subReports.join(',') : null,

        // Query parameter filtering
        FilterByParam: isMainReport && $('#MainReportType').val() === 'All',
        QueryParam: (isMainReport && $('#MainReportType').val() === 'All') ? $('#QueryParam').val() : null,
        QueryParamValue: (isMainReport && $('#MainReportType').val() === 'All') ? $('#QueryParamValue').val() : null
    };

    // Remove null/empty values to keep payload clean
    Object.keys(payload).forEach(key => {
        if (payload[key] === null || payload[key] === undefined || payload[key] === '') {
            delete payload[key];
        }
    });

    return payload;
}