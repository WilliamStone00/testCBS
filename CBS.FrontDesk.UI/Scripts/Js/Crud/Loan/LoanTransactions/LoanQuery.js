$(document).ready(function () {
    $('#branchFilterSection, #dateRangeSection, #extraFiltersSection, #parFilterSection').hide();

    $('#byBranch').change(function () {
        $('#branchFilterSection').slideToggle(this.checked);
    });

    $('#byDate').change(function () {
        $('#dateRangeSection').slideToggle(this.checked);
    });

    $('#moreOptions').change(function () {
        $('#extraFiltersSection').slideToggle(this.checked);
    });

    $('#byPar').change(function () {
        $('#parFilterSection').slideToggle(this.checked);
    });

    $('#applyFilterBtn').click(function () {
        loadLoanData();
    });

    $('#resetFilterBtn').click(function () {
        resetLoanFilters();
    });

    $('#exportBtn').click(function () {
        exportLoanData();
    });
});


function loadLoanData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/Loan/LoadLoanData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectLoanFilters();
                filters.dataTableOptions.draw = d.draw;
                filters.dataTableOptions.start = d.start;
                filters.dataTableOptions.length = d.length;
                filters.dataTableOptions.skip = d.start;
                filters.dataTableOptions.pageSize = d.length;
                filters.dataTableOptions.sortColumnName = d.columns[d.order[0]?.column]?.data || "CreatedDate";
                filters.dataTableOptions.sortColumnDirection = d.order[0]?.dir || "desc";
                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'LoanDate',
                render: function (data) {
                    return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                }
            },
            { data: 'CustomerName' },
            { data: 'CustomerId' },
            {
                data: 'LoanAmount',
                render: function (data) {
                    return `<div class="text-end">${data != null ? parseFloat(data).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0'}</div>`;
                }
            },
            {
                data: 'InterestRate',
                render: function (data) {
                    return `<div class="text-end">${data != null ? parseFloat(data).toFixed(1) : '0.0'}%</div>`;
                }
            },
            {
                data: 'AccrualInterest',
                render: function (data) {
                    return `<div class="text-end">${data != null ? parseFloat(data).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0'}</div>`;
                }
            },
            {
                data: 'Balance',
                render: function (data) {
                    return `<div class="text-end">${data != null ? parseFloat(data).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0'}</div>`;
                }
            },
            {
                data: 'LoanStatus',
                render: function (data) {
                    let badgeClass = 'secondary';
                    switch ((data || '').toLowerCase()) {
                        case 'open': badgeClass = 'primary'; break;
                        case 'closed': badgeClass = 'dark'; break;
                        case 'refinanced': badgeClass = 'warning'; break;
                        case 'restructured': badgeClass = 'info'; break;
                        case 'rescheduled': badgeClass = 'success'; break;
                    }

                    return `
            <div class="text-center">
                <span class="badge bg-${badgeClass} text-uppercase">${data || 'UNKNOWN'}</span>
            </div>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `
            <div class="text-center dropdown">
                <button class="btn btn-sm btn-info dropdown-toggle" type="button" id="actionDropdown${row.Id}" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="mdi mdi-eye"></i> Action
                </button>
                <ul class="dropdown-menu" aria-labelledby="actionDropdown${row.Id}">
                   
                    <li>
                        <a class="dropdown-item" href="/MembersFSeries/LoanDetails?loanId=${row.Id}" target="_blank">
                            <i class="mdi mdi-file-document-outline text-info me-1"></i> Detail
                        </a>
                    </li>
                </ul>
            </div>
        `;
                }
            }

        ],
        drawCallback: function (settings) {
            const dataCount = settings.json?.data?.length || 0;
            if (dataCount > 0) {
                $('#loanDataCard').slideDown(); // show with animation
            } else {
                $('#loanDataCard').slideUp(); // hide when no data
            }
        }

    });
}
function collectLoanFilters() {
    return {
        startDate: $('#byDate').is(':checked') ? formatDateTime($('#startDate').val(), 'start') : null,
        endDate: $('#byDate').is(':checked') ? formatDateTime($('#endDate').val(), 'end') : null,
        byDate: $('#byDate').is(':checked'),
        byBranch: $('#byBranch').is(':checked'),
        byPar: $('#byPar').is(':checked'),
        moreOptions: $('#moreOptions').is(':checked'),

        isInterbranch: $('#isInterbranch').is(':checked'),

        branchId: $('#branchInput').val(),
        loanId: $('#loanId').val(),
        memberId: $('#memberId').val(),
        status: $('#status').val(),
        disburmentStatus: $('#disburmentStatus').val(),
        deliquentStatus: $('#deliquentStatus').val(),

        // 🔽 Extra filters
        loanCategory: $('#loanCategory').val(),
        loanTypes: $('#loanTypes').val(),
        isMigratedLoan: $('#isMigratedLoan').val(),
        loanTarget: $('#loanTarget').val(),
        loanTypeCategory: $('#loanTypeCategory').val(),
        delinquentDays: $('#delinquentDays').val(),
        parId: $('#parId').val(),

        // DataTable default options (will be filled in server side)
        dataTableOptions: { searchValue: "" }
    };
}


function resetLoanFilters() {
    $('#startDate, #endDate, #loanId, #memberId, #loanTarget, #loanTypeCategory, #delinquentDays').val('');
    $('#status, #disburmentStatus, #deliquentStatus, #branchInput, #isMigratedLoan, #loanTypes, #loanCategory, #parId').val('');
    $('.select2').val('').trigger('change');
    $('#byBranch, #byDate, #moreOptions, #byPar, #isInterbranch')
        .prop('checked', false);
    $('#branchFilterSection, #dateRangeSection, #extraFiltersSection, #parFilterSection').slideUp();
    loadLoanData();
}

function exportLoanData() {
    const filters = collectLoanFilters();
    const params = new URLSearchParams();

    if (filters.byDate) {
        params.append("startDate", filters.startDate);
        params.append("endDate", filters.endDate);
    }

    if (filters.byBranch) {
        params.append("branchId", filters.branchId);
    }

    if (filters.loanId) params.append("loanId", filters.loanId);
    if (filters.memberId) params.append("memberId", filters.memberId);
    if (filters.status) params.append("status", filters.status);
    if (filters.disburmentStatus) params.append("disburmentStatus", filters.disburmentStatus);
    if (filters.deliquentStatus) params.append("deliquentStatus", filters.deliquentStatus);
    if (filters.isMigratedLoan) params.append("isMigratedLoan", filters.isMigratedLoan);
    if (filters.loanTypes) params.append("loanTypes", filters.loanTypes);
    if (filters.loanCategory) params.append("loanCategory", filters.loanCategory);
    if (filters.loanTarget) params.append("loanTarget", filters.loanTarget);
    if (filters.loanTypeCategory) params.append("loanTypeCategory", filters.loanTypeCategory);
    if (filters.delinquentDays) params.append("delinquentDays", filters.delinquentDays);
    if (filters.parId) params.append("parId", filters.parId);

    // ✅ NEW
    if (filters.isInterbranch) {
        params.append("isInterbranch", true);
    }

    window.location.href = `/Loan/DownloadLoanData?${params.toString()}`;
}

function formatDateTime(dateString, type) {
    if (!dateString) return null;
    const date = new Date(dateString);
    if (type === 'start') {
        date.setHours(0, 0, 0, 0);
    } else if (type === 'end') {
        date.setHours(23, 59, 59, 999);
    }
    return date.toISOString(); // C# API will parse this correctly
}
