$(document).ready(function () {
    $('.select2').select2({ width: '100%' });

    $('#generateReportBtn').on('click', function () {
        let btn = $(this);
        btn.prop('disabled', true).html('<i class="mdi mdi-loading mdi-spin"></i> Generating...');

        const subReports = $('#SubReportType').val();
        const payload = {
            BranchId: $('#BranchId').val(),
            StartDate: $('#StartDate').val(),
            EndDate: $('#EndDate').val(),
            SubReportType: subReports ? subReports.join(',') : 'All',
            ReportDownloadType: $('#ReportDownloadType').val(),
            MainReportType: $('#MainReportType').val()
        };

        $.ajax({
            url: '/Loan/LoanPortfolioStatistics',
            type: 'POST',
            data: payload,
            success: function (response) {
                if (response.success && response.redirectUrl) {
                    // ✅ Open the viewer tab only after success
                    window.open(response.redirectUrl, '_blank');
                    appalert("✅ Viewer launched in a new tab.", 1, 1);
                } else {
                    appalert(response.message || "❌ Failed to launch report viewer.", 3, 1);
                }
            },
            error: function (xhr) {
                appalert("❌ Server error: " + (xhr.responseText || "Unexpected error."), 3, 1);
            },
            complete: function () {
                btn.prop('disabled', false).html('<i class="mdi mdi-file-chart-outline me-1"></i> Generate Report');
            }
        });
    });
});
