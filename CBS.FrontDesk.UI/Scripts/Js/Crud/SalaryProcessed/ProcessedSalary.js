/* Processed Salary – index page JS */
$.fn.dataTable.ext.errMode = 'console';

let salaryTable = null;

$(document).ready(function () {
    initFilterToggles();
});

/* -------------------- Buttons -------------------- */
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

/* -------------------- Utilities -------------------- */
function htmlEncode(s) {
    return (s ?? '').toString()
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
function asMoney(v) {
    if (v === null || v === undefined || v === '') return '—';
    return Number(v).toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}
function ensureTableHeaderMatches(tableSelector, cols) {
    const $table = $(tableSelector);
    let $thead = $table.find('thead');
    if ($thead.length === 0) { $table.prepend('<thead></thead>'); $thead = $table.find('thead'); }
    const ths = cols.map(c => `<th class="${c.className ? c.className : ''}">${c.title || ''}</th>`).join('');
    $thead.html(`<tr>${ths}</tr>`);
}
function isDateRangeValid() {
    const s = $('#startDate').val(), e = $('#endDate').val();
    if (s && e) {
        if (new Date(s) > new Date(e)) { alert("⚠️ 'Start date' cannot be after 'End date'."); return false; }
    }
    return true;
}

/* -------------------- Filters I/O -------------------- */
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

/* -------------------- Export -------------------- */
function downloadProcessedSalary() {
    const query = $.param(collectFilterData());
    window.location.href = `/SalaryProcessed/Download?${query}`;
}

/* -------------------- Reset & toggles -------------------- */
function resetFilterForm() {
    ['byBranch', 'byOperators', 'byDate', 'withActiveCodeOnly', 'withoutActiveCodeOnly', 'nonMembersOnly']
        .forEach(id => $('#' + id).prop('checked', false));
    $('#branchInput, #uploadedBy, #executedBy, #memberReference, #memberName, #status').val('');
    $('#startDate, #endDate').val('');
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

/* -------------------- DataTable -------------------- */
function loadProcessedSalaryDataTable() {
    const cols = [
        { data: 'MemberName', title: 'Names' },                         // 0
        { data: 'MemberReference', title: 'M.Reference' },              // 1
        { data: 'Salary', title: 'G.Salary', render: asMoney, className: 'text-end' }, // 2
        { data: 'Saving', title: 'Saving', render: asMoney, className: 'text-end' },   // 3
        { data: 'Deposit', title: 'Deposit', render: asMoney, className: 'text-end' }, // 4
        { data: 'Shares', title: 'Shares', render: asMoney, className: 'text-end' },   // 5
        { data: 'Charges', title: 'Charges', render: asMoney, className: 'text-end' }, // 6
        { data: 'NetSalary', title: 'N.Salary', render: asMoney, className: 'text-end' }, // 7
        { data: 'CreatedDate', title: 'Date', render: d => d ? moment(d).format('DD/MM/YYYY HH:mm') : '—' }, // 8
        {
            data: 'Status',
            title: 'Status',
            className: 'text-center status-col',
            render: d => d
                ? '<span class="badge bg-success">Paid</span>'
                : '<span class="badge bg-warning text-dark">Pending</span>'
        }, // 9
        {
            data: null,
            title: 'Action',
            orderable: false,
            searchable: false,
            className: 'text-center',
            render: function (row) {
                const id = row.Id;
                const isPaid = !!row.Status;

                const name = htmlEncode(row.MemberName || '');
                const refPref = (row.MemberReference || row.NonMemberReference || '').toString();
                const ref = htmlEncode(refPref);
                const branchText = htmlEncode(row.BranchName || '');

                const detailBtn = `
                  <button type="button" class="btn btn-sm btn-outline-primary me-1 js-salary-detail"
                          data-id="${id}" data-name="${name}" data-ref="${ref}"
                          data-branch="${branchText}" data-status="${isPaid}">
                    <i class="mdi mdi-eye-outline me-1"></i> Detail
                  </button>`;

                if (isPaid) {
                    return `<div class="d-inline-flex">${detailBtn}</div>`;
                }

                // === Non-member registration state (from your screenshot) ===
                const nonMemberRefRaw = (row.NonMemberReference || row.NonMemberId || '').toString().trim();
                const isRegistered = nonMemberRefRaw.length > 0;

                // === Temp pay code state ===
                const hasTempId = !!row.LastTempPayCodeId;
                const tempStatus = (row.LastTempPayCodeStatus || '').toString().trim().toLowerCase();
                const expiresAt = row.LastTempPayCodeExpiresAt ? moment(row.LastTempPayCodeExpiresAt) : null;
                const isExpired = expiresAt ? !expiresAt.isAfter(moment()) : false; // treat null as not expired
                // Consider these as "active"/usable statuses; adjust if your API differs
                const isStatusActive = tempStatus === 'active' || tempStatus === 'issued' || tempStatus === 'generated';
                const hasActiveCode = hasTempId && isStatusActive && !isExpired;

                let actionBtn = '';

                if (!isRegistered) {
                    // 1) Not registered → Activate (create non-member)
                    actionBtn = `
                    <button type="button" class="btn btn-sm btn-outline-success js-nonmember-activate"
                            data-id="${id}"
                            data-name="${name}"
                            data-ref="${ref}"
                            data-branch="${branchText}"
                            data-branch-id="${htmlEncode(row.BranchId || '')}"
                            data-branch-code="${htmlEncode(row.BranchCode || '')}">
                      <i class="mdi mdi-account-plus-outline me-1"></i> Activate
                    </button>`;
                } else if (hasActiveCode) {
                    // 2) Registered + active code → Revoke
                    actionBtn = `
                    <button type="button" class="btn btn-sm btn-outline-danger js-salary-revoke"
                            data-temp-id="${htmlEncode(row.LastTempPayCodeId || '')}"
                            data-branch-id="${htmlEncode(row.BranchId || '')}"
                            data-name="${name}"
                            data-ref="${htmlEncode(nonMemberRefRaw)}"
                            data-branch="${branchText}">
                      <i class="mdi mdi-block-helper me-1"></i> Revoke
                    </button>`;
                } else {
                    // 3) Registered + NO active code (no code / revoked / cancelled / used / expired) → Generate
                    actionBtn = `
                    <button type="button" class="btn btn-sm btn-outline-success js-generate-paycode"
                            data-id="${id}"
                            data-name="${name}"
                            data-ref="${htmlEncode(nonMemberRefRaw)}"
                            data-branch="${branchText}"
                            data-branch-id="${htmlEncode(row.BranchId || '')}"
                            data-branch-code="${htmlEncode(row.BranchCode || '')}">
                      <i class="mdi mdi-qrcode me-1"></i> Generate Payment Code
                    </button>`;
                }

                return `<div class="d-inline-flex">${detailBtn}${actionBtn}</div>`;
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
        order: [[8, 'desc']], // Date
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
            { width: '26%', targets: 0 },  // Names
            { width: '8%', targets: 1 },  // M.Reference
            { width: '8%', targets: 2 },  // G.Salary
            { width: '8%', targets: 3 },  // Saving
            { width: '8%', targets: 4 },  // Deposit
            { width: '8%', targets: 5 },  // Shares
            { width: '8%', targets: 6 },  // Charges
            { width: '8%', targets: 7 },  // N.Salary
            { width: '16%', targets: 8 },  // Date
            { width: '4%', targets: 9 },  // Status
            { width: '8%', targets: 10 }  // Action
        ],
        initComplete: function () {
            dt.columns.adjust().responsive.recalc();
        }
    });

    return dt;
}

/* -------------------- Modal helpers -------------------- */
function renderErrorBlock(title, message, xhr) {
    const statusLine = xhr ? ` [${xhr.status || '—'} ${xhr.statusText || ''}]` : '';
    const body = xhr && xhr.responseText
        ? $('<div/>').text(xhr.responseText).html()
        : '';
    return `
    <div class="alert alert-danger d-flex align-items-start" role="alert">
      <i class="mdi mdi-alert-octagon-outline me-2 fs-4"></i>
      <div>
        <div class="fw-bold">${htmlEncode(title)}${statusLine}</div>
        <div class="mb-1">${htmlEncode(message || 'Unexpected error occurred.')}</div>
        ${body ? `<details class="small"><summary>Details</summary><pre class="mt-2 mb-0 small bg-light p-2 border rounded" style="max-height:240px;overflow:auto;">${body}</pre></details>` : ''}
      </div>
    </div>`;
}

/* Open details:
   - On success: inject HTML and SHOW modal.
   - On error:   inject error panel and SHOW modal (no spinner left and nothing leaks to main page). */
function openSalaryDetail(id, name, memberRef, statusBool, branchText) {
    // prepare header values up front (used for both success and error)
    const header = {
        name: name || '—',
        ref: memberRef || '—',
        branch: branchText || '—',
        statusBool: typeof statusBool === 'boolean' ? statusBool : null
    };

    // spinner while we fetch
    $('#salaryDetailBody').html('<div class="text-center py-4"><div class="spinner-border" role="status"></div></div>');
    $('#salaryDetailErrorArea').addClass('d-none').empty();

    // set header chips immediately
    $('#salaryDetailName').text(header.name);
    $('#salaryDetailRef').text(header.ref);
    $('#salaryDetailBranch').text(header.branch);
    const $chip = $('#salaryDetailStatus');
    if (header.statusBool === true) $chip.attr('class', 'badge bg-success').text('Paid');
    else if (header.statusBool === false) $chip.attr('class', 'badge bg-warning text-dark').text('Pending Withdrawal');
    else $chip.attr('class', 'badge bg-secondary').text('—');

    const modalEl = document.getElementById('salaryDetailModal');
    const modal = new bootstrap.Modal(modalEl);

    $.ajax({
        url: `/SalaryProcessed/DetailsPv?id=${encodeURIComponent(id)}`,
        type: 'GET',
        dataType: 'html',
        cache: false,
        timeout: 30000
    })
        .done(function (html) {
            $('#salaryDetailBody').html(html);
            $('#salaryDetailErrorArea').addClass('d-none').empty();
            modal.show();
        })
        .fail(function (xhr, textStatus) {
            const msg = textStatus === 'timeout'
                ? 'The request timed out. Please try again.'
                : 'Could not retrieve the detail view.';
            // Put the error block INSIDE the modal body and show the modal
            $('#salaryDetailBody').html(renderErrorBlock('Request failed', msg, xhr));
            $('#salaryDetailErrorArea').addClass('d-none').empty(); // keep a single error place (body)
            modal.show();
        });
}

/* Registration (kept as fallback if global showInPopup exists) */
// Activate (always use the same salary modal)
// Make sure this runs once (e.g., after your DataTable init)
$(document)
    .off('click.nmopen', '#myDataTable .js-nonmember-activate')
    .on('click.nmopen', '#myDataTable .js-nonmember-activate', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $b = $(this);
        const id = $b.data('id');
        const name = $b.data('name') || '—';
        const ref = $b.data('ref') || '—';
        const branchText = $b.data('branch') || '—';
        const branchId = $b.data('branchId') || '';     // <-- camelCase
        const branchCode = $b.data('branchCode') || '';   // <-- camelCase

        openRegisterNonMember(id, { name, ref, branchText, branchId, branchCode });
    });

// Revoke (from table)
$(document).on('click', '.js-salary-revoke', function () {
    const $b = $(this);
    openRevokeTempCode({
        tempId: $b.data('temp-id'),
        branchId: $b.data('branch-id'),
        name: $b.data('name'),
        ref: $b.data('ref'),
        branchText: $b.data('branch')
    });
});
// Load the registration PV inside #salaryDetailModal


/* Delegated buttons inside table */
$(document).on('click', '.js-salary-detail', function () {
    const $b = $(this);
    openSalaryDetail(
        $b.data('id'),
        $b.data('name') || '—',
        $b.data('ref') || '—',
        String($b.data('status')) === 'true',
        $b.data('branch') || '—'
    );
});



// ---------- Modal header/tips switching ----------
function setSalaryModalMode(mode /* 'detail' | 'register' */) {
    const $content = $('#salaryDetailModalContent');
    $content.attr('data-mode', mode);

    if (mode === 'register') {
        $('#salaryDetailTitleText').text('Register / Activate Non-Member');
        $('#salaryDetailStatus').attr('class', 'badge align-middle ms-2 bg-warning text-dark').text('Pending');
        $('#salaryTipsTitleText').text('Registration Tips');
        $('#salaryTipsBody').html(`
      <ul class="mb-0" style="list-style:none; padding-left:0;">
        <li class="mb-2"><i class="mdi mdi-account-plus-outline text-success me-2"></i>
          <strong>KYC:</strong> First/Last name and ID details should match the beneficiary’s document.
        </li>
        <li class="mb-2"><i class="mdi mdi-key-variant text-tsc-dark me-2"></i>
          <strong>TempPayCode:</strong> Expires at the selected time (defaults to now + 48h UTC).
        </li>
        <li class="mb-2"><i class="mdi mdi-bank text-muted me-2"></i>
          <strong>Branch:</strong> Comes from the salary extract and cannot be changed here.
        </li>
      </ul>
    `);
    } else {
        $('#salaryDetailTitleText').text('Processed Salary – Detail');
        // Badge is set based on the record; don't override here.
        $('#salaryTipsTitleText').text('Salary Detail Tips');
        $('#salaryTipsBody').html(`
      <ul class="mb-0" style="list-style:none; padding-left:0;">
        <li class="mb-2"><i class="mdi mdi-cash-multiple text-success me-2"></i>
          <strong>G.Salary vs N.Salary:</strong> N.Salary is after Savings, Shares, Charges and Standing Order.
        </li>
        <li class="mb-2"><i class="mdi mdi-percent-outline text-tsc-dark me-2"></i>
          <strong>Remaining:</strong> What’s left after all deductions.
        </li>
        <li class="mb-2"><i class="mdi mdi-file-document-edit-outline text-tsc me-2"></i>
          <strong>Loans:</strong> Main / Exceptional / Elected / Special / Micro sections render only when values exist.
        </li>
        <li class="mb-2"><i class="mdi mdi-key-variant text-muted me-2"></i>
          <strong>TempPayCode:</strong> For non-member payout activation, shows last code + expiry.
        </li>
      </ul>
    `);
    }
}

function setSalaryModalHeader(meta) {
    $('#salaryDetailName').text(meta?.name || '—');
    $('#salaryDetailRef').text(meta?.ref || '—');
    $('#salaryDetailBranch').text(meta?.branchText || '—');

    if (typeof meta?.statusBool === 'boolean') {
        if (meta.statusBool) {
            $('#salaryDetailStatus').attr('class', 'badge align-middle ms-2 bg-success').text('Paid');
        } else {
            $('#salaryDetailStatus').attr('class', 'badge align-middle ms-2 bg-warning text-dark').text('Pending');
        }
    }
}

function showSalaryLoader(show) {
    $('#salaryDetailLoader').toggleClass('d-none', !show);
}

// Centralized loader + fetcher.
// showErrorInModal=true: show the modal even if error, with the error content.
// false: don't show the modal if the fetch failed (useful if you prefer surfacing errors elsewhere).
function loadIntoSalaryModal(url, mode, headerMeta, showErrorInModal = true) {
    setSalaryModalMode(mode);
    setSalaryModalHeader(headerMeta);
    $('#salaryDetailBody').html('<div class="position-relative" style="min-height:3rem;"></div>');
    showSalaryLoader(true);

    $.ajax({
        url,
        type: 'GET',
        dataType: 'html',
        cache: false,
        timeout: 30000
    })
        .done(function (html) {
            showSalaryLoader(false);
            $('#salaryDetailBody').html(html);

            const modalEl = document.getElementById('salaryDetailModal');
            const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

            // Initialize Select2 and cascading selects for the newly injected PV
            initSelect2InSalaryModal();
            //wireNmCascadingSelects();

            // Make sure it stays correct even if the modal adjusts focus after show
            $(modalEl).off('shown.bs.modal.init2').on('shown.bs.modal.init2', function () {
                initSelect2InSalaryModal();
            });

            modal.show();
        })
        .fail(function (xhr) {
            showSalaryLoader(false);
            const safe = $('<div>').text(xhr.responseText || '').html();
            const errHtml = `
      <div class="alert alert-danger d-flex align-items-start m-3" role="alert">
        <i class="mdi mdi-alert-octagon-outline me-2 fs-4"></i>
        <div>
          <div class="fw-bold">Request failed</div>
          <div class="small mt-1">${safe || 'Unexpected error occurred.'}</div>
        </div>
      </div>`;
            if (showErrorInModal) {
                $('#salaryDetailBody').html(errHtml);
                const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('salaryDetailModal'));
                modal.show();
            } else {
                console.error('Salary modal fetch failed', xhr.status, xhr.statusText);
            }
        });
}

// ---------- Public openers (table + detail PV both use these) ----------
function openSalaryDetail(id, name, memberRef, statusBool, branchText) {
    const url = `/SalaryProcessed/DetailsPv?id=${encodeURIComponent(id)}`;
    const meta = { name, ref: memberRef, branchText, statusBool };
    loadIntoSalaryModal(url, 'detail', meta, /*showErrorInModal*/ true);
}

function openRegisterNonMember(salaryExtractId, meta) {
    const url = `/SalaryProcessed/RegistrationPv?salaryExtractId=${encodeURIComponent(salaryExtractId)}&branchId=${encodeURIComponent(meta.branchId || '')}&branchCode=${encodeURIComponent(meta.branchCode || '')}`;
    const header = {
        name: meta?.name || '—',
        ref: meta?.ref || '—',
        branchText: meta?.branchText || '—',
        statusBool: false  // registration is only possible when not paid
    };
    loadIntoSalaryModal(url, 'register', header, /*showErrorInModal*/ true);
}

// (Optional) Hook up the print button
$(document).on('click', '#salaryDetailPrintBtn', function () {
    const $btn = $(this);
    const $spin = $btn.find('.spinner-border');
    const $txt = $btn.find('.btn-text');
    $btn.prop('disabled', true); $spin.removeClass('d-none'); $txt.addClass('opacity-50');
    try {
        window.print();
    } finally {
        setTimeout(() => { $btn.prop('disabled', false); $spin.addClass('d-none'); $txt.removeClass('opacity-50'); }, 300);
    }
});

function openRevokeTempCode(meta) {
    // header
    $('#salaryDetailTitle, #salaryDetailModalLabel').text('Revoke TempPayCode');
    $('#salaryDetailName').text(meta.name || '—');
    $('#salaryDetailRef').text(meta.ref || '—');
    $('#salaryDetailBranch').text(meta.branchText || '—');
    $('#salaryDetailStatus').attr('class', 'badge bg-info').text('Active Code');

    // loader
    $('#salaryDetailBody').html(`
        <div class="text-center py-4">
            <div class="spinner-border" role="status"></div>
            <div class="small text-muted mt-2">Loading revoke form…</div>
        </div>
    `);

    $.ajax({
        url: `/SalaryProcessed/RevokePv?tempPayCodeId=${encodeURIComponent(meta.tempId)}&branchId=${encodeURIComponent(meta.branchId || '')}`,
        type: 'GET',
        dataType: 'html',
        cache: false,
        timeout: 30000
    })
        .done(function (html) {
            $('#salaryDetailBody').html(html);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('salaryDetailModal')).show();
        })
        .fail(function (xhr) {
            const safe = $('<div>').text(xhr.responseText || '').html();
            $('#salaryDetailBody').html(`
            <div class="alert alert-danger d-flex align-items-start m-3">
                <i class="mdi mdi-alert-octagon-outline me-2 fs-4"></i>
                <div>
                    <div class="fw-bold">Could not load revoke view</div>
                    <div class="small mt-1">${safe || 'Unexpected error occurred.'}</div>
                </div>
            </div>
        `);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('salaryDetailModal')).show();
        });
}


// ---------- helpers used by the modal ----------
function initSelect2InSalaryModal() {
    const $modal = $('#salaryDetailModal');

    // Initialize/destroy-safe
    $modal.find('select.select2, select.use-select2').each(function () {
        const $el = $(this);
        if ($el.data('select2')) $el.select2('destroy');
        $el.select2({
            dropdownParent: $modal,                   // <— key for modals
            width: '100%',
            allowClear: true,
            placeholder: $el.attr('data-placeholder') || '--- Select ---'
        });
    });
}

// Optional: wire cascading geo selects if present in the PV
//function wireNmCascadingSelects() {
//    const $modal = $('#salaryDetailModal');

//    // local helpers that only operate within the modal
//    const populateDropdown = (selector, data, placeholder) => {
//        const $dd = $modal.find(selector);
//        $dd.empty().append($('<option>').val('').text(placeholder || '--- Select ---'));
//        (data || []).forEach(item => $dd.append($('<option>').val(item.Id).text(item.Name)));
//        if ($dd.hasClass('select2') || $dd.hasClass('use-select2')) $dd.trigger('change.select2');
//    };
//    const resetDropdowns = (selectors) => {
//        selectors.forEach(sel => {
//            const $dd = $modal.find(sel);
//            $dd.empty().append('<option value="">--- Select ---</option>');
//            if ($dd.hasClass('select2') || $dd.hasClass('use-select2')) $dd.trigger('change.select2');
//        });
//    };

//    // Avoid duplicate bindings
//    $modal.off('change.nmgeo');

//    // Country -> Regions
//    $modal.on('change.nmgeo', '#nmCountry', function () {
//        const countryId = $(this).val();
//        resetDropdowns(['#nmRegion', '#nmDivision', '#nmSubDivision', '#nmTown']);
//        if (!countryId) return;
//        populateDropdown('#nmRegion', [], 'Loading...');
//        $.getJSON('/Individual/GetRegionsByCountry', { countryId })
//            .done(d => populateDropdown('#nmRegion', d, '--- Select Region ---'))
//            .fail(() => populateDropdown('#nmRegion', [], 'Error loading regions'));
//    });

//    // Region -> Divisions
//    $modal.on('change.nmgeo', '#nmRegion', function () {
//        const regionId = $(this).val();
//        resetDropdowns(['#nmDivision', '#nmSubDivision', '#nmTown']);
//        if (!regionId) return;
//        populateDropdown('#nmDivision', [], 'Loading...');
//        $.getJSON('/Individual/GetDivisionsByRegion', { regionId })
//            .done(d => populateDropdown('#nmDivision', d, '--- Select Division ---'))
//            .fail(() => populateDropdown('#nmDivision', [], 'Error loading divisions'));
//    });

//    // Division -> SubDivisions
//    $modal.on('change.nmgeo', '#nmDivision', function () {
//        const divisionId = $(this).val();
//        resetDropdowns(['#nmSubDivision', '#nmTown']);
//        if (!divisionId) return;
//        populateDropdown('#nmSubDivision', [], 'Loading...');
//        $.getJSON('/Individual/GetSubDivisionsByDivision', { divisionId })
//            .done(d => populateDropdown('#nmSubDivision', d, '--- Select Sub-Division ---'))
//            .fail(() => populateDropdown('#nmSubDivision', [], 'Error loading sub-divisions'));
//    });

//    // SubDivision -> Towns
//    $modal.on('change.nmgeo', '#nmSubDivision', function () {
//        const subDivisionId = $(this).val();
//        resetDropdowns(['#nmTown']);
//        if (!subDivisionId) return;
//        populateDropdown('#nmTown', [], 'Loading...');
//        $.getJSON('/Individual/GetTownsBySubDivision', { subDivisionId })
//            .done(d => populateDropdown('#nmTown', d, '--- Select Town ---'))
//            .fail(() => populateDropdown('#nmTown', [], 'Error loading towns'));
//    });
//}
