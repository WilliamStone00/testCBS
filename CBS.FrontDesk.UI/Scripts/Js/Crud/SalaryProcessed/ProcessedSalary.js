
$.fn.dataTable.ext.errMode = 'console';

let salaryTable = null;

$(document).ready(function () {
    initFilterToggles();
});

// ------------------------- Button handlers (delegated) -------------------------
$(document).on('click', '#applyFilterBtn', function (e) {
    e.preventDefault();
    if (!isDateRangeValid()) return;

    if ($.fn.dataTable.isDataTable('#myDataTable')) {
        salaryTable.ajax.reload();
    } else {
        salaryTable = loadProcessedSalaryDataTable();
    }
});

$(document).on('click', '#resetFilterBtn', function (e) {
    e.preventDefault();
    resetFilterForm();
    if ($.fn.dataTable.isDataTable('#myDataTable')) {
        salaryTable.ajax.reload();
    }
});

$(document).on('click', '#exportBtn', function (e) {
    e.preventDefault();
    if (!isDateRangeValid()) return;
    downloadProcessedSalary();
});

// ------------------------- Table header guard -------------------------
function ensureTableHeaderMatches(tableSelector, cols) {
    const $table = $(tableSelector);
    let $thead = $table.find('thead');
    if ($thead.length === 0) {
        $table.prepend('<thead></thead>');
        $thead = $table.find('thead');
    }
    const ths = cols.map(c => `<th class="${c.className ? c.className : ''}">${c.title || ''}</th>`).join('');
    $thead.html(`<tr>${ths}</tr>`);
}

// ------------------------- DataTable init -------------------------
function loadProcessedSalaryDataTable() {
    const cols = [
        { data: 'MemberName', title: 'Names' },                        // 0
        { data: 'MemberReference', title: 'M.Reference' },             // 1
        { data: 'Salary', title: 'G.Salary', render: asMoney, className: 'text-end' },   // 2
        { data: 'Saving', title: 'Saving', render: asMoney, className: 'text-end' },     // 3
        { data: 'Deposit', title: 'Deposit', render: asMoney, className: 'text-end' },   // 4
        { data: 'Shares', title: 'Shares', render: asMoney, className: 'text-end' },     // 5
        { data: 'Charges', title: 'Charges', render: asMoney, className: 'text-end' },   // 6

        // 🔁 Net Salary now placed AFTER Charges
        { data: 'NetSalary', title: 'N.Salary', render: asMoney, className: 'text-end' }, // 7

        {
            data: 'CreatedDate', title: 'Date',                                 // 8
            render: d => d ? moment(d).format('DD/MM/YYYY HH:mm') : '—'
        },
        {
            data: 'Status', title: 'Status', className: 'text-center status-col',        // 9
            render: d => d
                ? '<span class="badge bg-success">Paid</span>'
                : '<span class="badge bg-warning text-dark">Pending</span>'
        },
        {
            data: null, title: 'Action', orderable: false, searchable: false, className: 'text-center', // 10
            render: row => {
                const id = row.Id, isPaid = !!row.Status;
                const detailBtn = `
                    <button type="button" class="btn btn-sm btn-outline-primary me-1"
                            title="Details"
                            onclick="openSalaryDetail(
                             '${id}',
                              ${JSON.stringify(row.MemberName || '')},
                              ${JSON.stringify(row.MemberReference || row.NonMemberReference || '')},
                              ${row.Status},
                              ${JSON.stringify(row.BranchName || '')}
                            )">
                        <i class="mdi mdi-eye-outline me-1"></i> Detail
                    </button>`;
                const activateBtn = isPaid ? '' : `
                    <button type="button" class="btn btn-sm btn-outline-success"
                            title="Activate / Register non-member"
                            onclick="openRegisterNonMember('${id}', '${row.BranchId || ''}', '${row.BranchCode || ''}')">
                        <i class="mdi mdi-account-plus-outline me-1"></i> Activate
                    </button>`;
                return `<div class="d-inline-flex">${detailBtn}${activateBtn}</div>`;
            }
        }
    ];

    ensureTableHeaderMatches('#myDataTable', cols);

    const dt = $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        responsive: true,
        autoWidth: false,
        order: [[8, 'desc']], // Executed On now at index 8
        ajax: {
            url: '/SalaryProcessed/LoadData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectFilterData();
                filters.DataTableOptions = {
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    skip: d.start,
                    pageSize: d.length,
                    sortColumnName: d.columns[d.order[0].column].data,
                    sortColumnDirection: d.order[0].dir
                };
                return JSON.stringify(filters);
            },
            error: function (xhr) {
                console.error('LoadData failed:', xhr.status, xhr.responseText);
                alert('Failed to load data. Please try again.');
            }
        },
        columns: cols,
        columnDefs: [
            { width: '26%', targets: 0 }, // Names (priority)
            { width: '8%', targets: 1 }, // Member Reference
            { width: '8%', targets: 2 }, // Gross Salary
            { width: '8%', targets: 3 },  // Saving
            { width: '8%', targets: 4 },  // Deposit
            { width: '8%', targets: 5 },  // Shares
            { width: '8%', targets: 6 },  // Charges
            { width: '8%', targets: 7 }, // Net Salary
            { width: '16%', targets: 8 }, // Executed On
            { width: '4%', targets: 9 }, // Status (shrink)
            { width: '8%', targets: 10 } // Action
        ],
        initComplete: function () {
            dt.columns.adjust().responsive.recalc();
        }
    });

    return dt;
}

// ------------------------- Helpers -------------------------
function asMoney(v) {
    if (v === null || v === undefined || v === '') return '—';
    return Number(v).toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

function isDateRangeValid() {
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();
    if (startDate && endDate) {
        const start = new Date(startDate);
        const end = new Date(endDate);
        if (start > end) {
            alert("⚠️ 'Start date' cannot be after 'End date'.");
            return false;
        }
    }
    return true;
}

// ------------------------- Filters mapping (GetProcessedSalaryDataTableQuery) -------------------------
function collectFilterData() {
    return {
        BranchId: $('#branchInput').val(),
        UploadedBy: $('#uploadedBy').val(),
        ExecutedBy: $('#executedBy').val(),
        WithActiveCodeOnly: $('#withActiveCodeOnly').is(':checked'),
        WithoutActiveCodeOnly: $('#withoutActiveCodeOnly').is(':checked'),
        NonMembersOnly: $('#nonMembersOnly').is(':checked'),
        MemberReference: $('#memberReference').val(),
        MemberName: $('#memberName').val(),
        Status: $('#status').val() === '' ? null : ($('#status').val() === 'true'),
        StartDate: $('#startDate').val(),
        EndDate: $('#endDate').val()
    };
}

// ------------------------- Export -------------------------
function downloadProcessedSalary() {
    const query = $.param(collectFilterData());
    window.location.href = `/SalaryProcessed/Download?${query}`;
}

// ------------------------- Reset & Toggles -------------------------
function resetFilterForm() {
    // toggles
    ['byBranch', 'byOperators', 'byDate', 'withActiveCodeOnly', 'withoutActiveCodeOnly', 'nonMembersOnly']
        .forEach(id => $('#' + id).prop('checked', false));

    // inputs
    $('#branchInput, #uploadedBy, #executedBy, #memberReference, #memberName, #status').val('');
    $('#startDate, #endDate').val('');

    // sections
    $('#branchFilterSection, #operatorsFilterSection, #dateRangeSection').hide();
}

function initFilterToggles() {
    toggleSections();

    $('#byBranch').change(function () {
        $('#branchFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#branchInput').val('').trigger('change');
    });

    $('#byOperators').change(function () {
        $('#operatorsFilterSection').slideToggle(this.checked);
        if (!this.checked) $('#uploadedBy, #executedBy').val('');
    });

    $('#byDate').change(function () {
        $('#dateRangeSection').slideToggle(this.checked);
        if (!this.checked) $('#startDate, #endDate').val('');
    });

    // Mutually exclusive TempPayCode toggles
    $('#withActiveCodeOnly').change(function () {
        if (this.checked) $('#withoutActiveCodeOnly').prop('checked', false);
    });
    $('#withoutActiveCodeOnly').change(function () {
        if (this.checked) $('#withActiveCodeOnly').prop('checked', false);
    });
}

function toggleSections() {
    $('#branchFilterSection').toggle($('#byBranch').is(':checked'));
    $('#operatorsFilterSection').toggle($('#byOperators').is(':checked'));
    $('#dateRangeSection').toggle($('#byDate').is(':checked'));
}

// ------------------------- Row action handlers -------------------------
function openSalaryDetail(id) {
    showInPopup(`/SalaryProcessed/DetailsPv?id=${encodeURIComponent(id)}`, 'Processed Salary Detail');
}

function openRegisterNonMember(id, branchId, branchCode) {
    showInPopup(
        `/SalaryProcessed/RegistrationPv?salaryExtractId=${encodeURIComponent(id)}&branchId=${encodeURIComponent(branchId || '')}&branchCode=${encodeURIComponent(branchCode || '')}`,
        'Activate / Register Non-Member'
    );
}

// Expose for inline onclick
window.openSalaryDetail = openSalaryDetail;
window.openRegisterNonMember = openRegisterNonMember;


// Utility: show a developer-friendly error panel (no modal)
function showDetailError(message, xhr) {
    const $area = $('#salaryDetailErrorArea');
    const statusLine = xhr
        ? ` [${xhr.status || '—'} ${xhr.statusText || ''}]`
        : '';
    const resp = xhr && xhr.responseText ? `<pre class="mt-2 mb-0 small bg-light p-2 border rounded" style="max-height:240px;overflow:auto;">${$('<div/>').text(xhr.responseText).html()}</pre>` : '';

    $area
        .removeClass('d-none')
        .html(`
            <div class="alert alert-danger d-flex align-items-start" role="alert">
                <i class="mdi mdi-alert-octagon-outline me-2 fs-4"></i>
                <div>
                    <div class="fw-bold">Failed to load salary detail${statusLine}</div>
                    <div>${message || 'Unexpected error occurred.'}</div>
                    ${resp}
                </div>
            </div>
        `);
}

// Opens the detail modal ONLY after successful PV retrieval
function openSalaryDetail(id, name, memberRef, statusBool, branchText) {
    // clear previous errors
    $('#salaryDetailErrorArea').addClass('d-none').empty();

    // prepare header bits (we'll set them right before showing)
    const headerData = {
        name: name || '—',
        ref: memberRef || '—',
        branch: branchText || '—',
        statusBool: typeof statusBool === 'boolean' ? statusBool : null
    };

    // fetch partial first; don't show modal yet
    const url = `/SalaryProcessed/DetailsPv?id=${encodeURIComponent(id)}`;

    $.ajax({
        url,
        type: 'GET',
        dataType: 'html',
        cache: false,
        timeout: 30000
    })
        .done(function (html) {
            // inject PV
            $('#salaryDetailBody').html(html);

            // finalize header
            $('#salaryDetailName').text(headerData.name);
            $('#salaryDetailRef').text(headerData.ref);
            $('#salaryDetailBranch').text(headerData.branch);

            const $status = $('#salaryDetailStatus');
            if (headerData.statusBool === true) {
                $status.attr('class', 'badge bg-success').text('Paid');
            } else if (headerData.statusBool === false) {
                $status.attr('class', 'badge bg-warning text-dark').text('Pending');
            } else {
                $status.attr('class', 'badge bg-secondary').text('—');
            }

            // show modal now that data is ready
            const modal = new bootstrap.Modal(document.getElementById('salaryDetailModal'));
            modal.show();
        })
        .fail(function (xhr, textStatus) {
            const msg = textStatus === 'timeout'
                ? 'The request timed out. Please try again.'
                : 'Could not retrieve the detail view.';
            showDetailError(msg, xhr);
        });
}

// Optional print handler (unchanged)
$(document).on('click', '#salaryDetailPrintBtn', function () {
    const printContents = document.getElementById('salaryDetailBody').innerHTML;
    const w = window.open('', '_blank', 'width=1024,height=768');
    w.document.write(`
      <html><head><title>Processed Salary Detail</title>
      <link href="/Content/bootstrap.css" rel="stylesheet" />
      </head><body>${printContents}</body></html>
    `);
    w.document.close();
    w.focus();
    w.print();
    setTimeout(() => w.close(), 200);
});
