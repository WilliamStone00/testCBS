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
            xhrFields: { responseType: 'blob' },
            success: function (blob, status, xhr) {
                const contentType = xhr.getResponseHeader("Content-Type") || "";

                if (contentType.includes("application/json")) {
                    // Means controller returned a JSON error, not a file
                    const reader = new FileReader();
                    reader.onload = function () {
                        try {
                            const result = JSON.parse(reader.result);
                            appalert(result.message || "An error occurred", 3, 1);
                        } catch (err) {
                            appalert("⚠ Failed to parse error response.", 3, 1);
                        }
                    };
                    reader.readAsText(blob);
                } else {
                    // It's a valid file, download it
                    const disposition = xhr.getResponseHeader('Content-Disposition');
                    let filename = "LoanPortfolioReport_" + new Date().toISOString().replace(/[:.]/g, '') + ".pdf";
                    if (disposition && disposition.indexOf('filename=') !== -1) {
                        filename = disposition.split('filename=')[1].replace(/['"]/g, '');
                    }

                    const link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = filename;
                    link.click();

                    appalert("✅ Report generated and download started.", 1, 1);
                }
            },
            error: function () {
                appalert("❌ Server error occurred. Please try again.", 3, 1);
            },
            complete: function () {
                btn.prop('disabled', false).html('<i class="mdi mdi-file-chart-outline me-1"></i> Generate Report');
            }
        });
    });
});
