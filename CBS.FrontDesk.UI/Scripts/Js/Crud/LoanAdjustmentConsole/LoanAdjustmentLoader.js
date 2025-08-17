$(document).ready(function () {
    // 🔽 Toggle filter sections
    $('#branchFilterSection, #dateRangeSection').hide();

    $('#byBranch').change(() => $('#branchFilterSection').slideToggle($('#byBranch').is(':checked')));
    $('#byDate').change(() => $('#dateRangeSection').slideToggle($('#byDate').is(':checked')));

    // 🔎 Apply filters
    $('#applyFilterBtn').click(loadLoanData);
    $('#resetFilterBtn').click(resetLoanFilters);
        // 👮 Validation buttons
    $('#approveRequestBtn').click(() => approveLoanAdjustment());
    $('#rejectRequestBtn').click(() => rejectLoanAdjustment());
    // Recalculate new due amount when any new value changes
    $('#NewBalance, #NewInterest, #NewVat, #NewPenalty').on('input', function () {
        calculateNewDueAmount();
    });

    // Initial calculation on page load (optional)
    calculateNewDueAmount();
    $('#NewLoanAmount, #NewBalance, #NewInterest, #NewVat, #NewPenalty, #newDueAmount, #NewIntRate, #NewVatRate')
        .on('input change', checkForLoanAdjustmentsChange);


});

// 🧠 Load DataTable
function renderLoanStatusBadge(status) {
    if (!status) return `<span class="badge bg-secondary text-uppercase">UNKNOWN</span>`;

    const normalized = status.toLowerCase();
    const badgeMap = {
        open: 'success',
        closed: 'secondary',
        refinanced: 'primary',
        pending: 'warning',
        restructured: 'info',
        rescheduled: 'dark'
    };

    const color = badgeMap[normalized] || 'secondary';
    const label = status.toUpperCase();

    return `<span class="badge rounded-pill bg-${color} px-3 py-2 text-uppercase">${label}</span>`;
}

function loadLoanData() {
    let isFirstDraw = true;

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
                filters.dataTableOptions = {
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    skip: d.start,
                    pageSize: d.length,
                    sortColumnName: d.columns[d.order[0]?.column]?.data || "CreatedDate",
                    sortColumnDirection: d.order[0]?.dir || "desc"
                };
                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'LoanDate',
                render: data => data ? moment(data).format('DD/MM/YYYY HH:mm') : ''
            },
            { data: 'CustomerName' },
            { data: 'CustomerId' },
            {
                data: 'LoanAmount',
                render: data => `<div class="text-end">${formatCurrency(data)}</div>`
            },
            {
                data: 'InterestRate',
                render: data => `<div class="text-end">${parseFloat(data || 0).toFixed(1)}%</div>`
            },
            {
                data: 'AccrualInterest',
                render: data => `<div class="text-end">${formatCurrency(data)}</div>`
            },
            {
                data: 'Balance',
                render: data => `<div class="text-end">${formatCurrency(data)}</div>`
            },
            {
                data: 'LoanStatus',
                render: data => renderLoanStatusBadge(data)
            },
            {
                data: null,
                orderable: false,
                render: function (_, __, row) {
                    return `
    <div class="text-center dropdown">
        <button class="btn btn-sm btn-info dropdown-toggle" type="button" data-bs-toggle="dropdown">
            <i class="mdi mdi-eye"></i> Action
        </button>
        <ul class="dropdown-menu">
            <li>
                <a class="dropdown-item" href="/Loan/Details?KEY=${row.Id}" target="_blank">
                    <i class="mdi mdi-file-document-outline text-info me-1"></i> Details
                </a>
            </li>
            <li>
                <a class="dropdown-item text-warning" href="#"
                   onclick="openLoanAdjustmentModal('request', {
                       LoanId: '${row.Id}',
                       CustomerId: '${row.CustomerId}',
                       BranchId: '${row.BranchId}',
                       CustomerName: '${row.CustomerName}', 
                       LoanAmount: ${row.LoanAmount},
                       Balance: ${row.Balance},
                       AccrualInterest: ${row.AccrualInterest},
                       Tax: ${row.Vat},
                       Penalty: ${row.Penalty},
                       DueAmount: ${row.DueAmount},
                       VatRate: ${row.VatRate},
                       InterestRate: ${row.InterestRate},
                       LoanDate: '${row.LoanDate}',
                       DisbursementDate: '${row.DisbursementDate}',
                       LoanStatus: '${row.LoanStatus}',
                       NextInstallmentDate: '${row.NextInstallmentDate}',
                       Paid: ${row.Paid ?? 0}
                   })">
                   <i class="mdi mdi-pencil-outline me-1"></i> Initiate Loan Adjustment
                </a>
            </li>
        </ul>
    </div>`;
                }
            }
        ],

        drawCallback: function (settings) {
            const hasData = settings.json?.data?.length > 0;

            if (isFirstDraw) {
                $('#loanDataCard').toggle(hasData); // No animation
                isFirstDraw = false;
            } else {
                hasData ? $('#loanDataCard').slideDown() : $('#loanDataCard').slideUp();
            }
        }


    });
}

// 📄 Collect filters
function collectLoanFilters() {
    return {
        startDate: $('#byDate').is(':checked') ? formatDateTime($('#startDate').val(), 'start') : null,
        endDate: $('#byDate').is(':checked') ? formatDateTime($('#endDate').val(), 'end') : null,
        byDate: $('#byDate').is(':checked'),
        byBranch: $('#byBranch').is(':checked'),
        branchId: $('#branchInput').val(),
        loanId: $('#loanId').val(),
        memberId: $('#memberId').val(),
        status: $('#adjustmentStatus').val(),
        dataTableOptions: { searchValue: "" }
    };
}

// 🔄 Reset filters
function resetLoanFilters() {
    $('#startDate, #endDate, #loanId, #memberId').val('');
    $('#branchInput, #adjustmentStatus').val('');
    $('.select2').val('').trigger('change');
    $('#byBranch, #byDate').prop('checked', false);
    $('#branchFilterSection, #dateRangeSection').slideUp();
    loadLoanData();
}

// ⏱ Format datetime for backend
function formatDateTime(dateString, type) {
    if (!dateString) return null;
    const date = new Date(dateString);
    type === 'start' ? date.setHours(0, 0, 0, 0) : date.setHours(23, 59, 59, 999);
    return date.toISOString();
}

// 💲 Currency formatter
function formatCurrency(value) {
    return value && !isNaN(value) ? parseFloat(value).toLocaleString(undefined, { minimumFractionDigits: 1 }) : '0.0';
}
function calculateNewDueAmount() {
    const balance = parseFloat($('#NewBalance').val()) || 0;
    const interest = parseFloat($('#NewInterest').val()) || 0;
    const vat = parseFloat($('#NewVat').val()) || 0;
    const penalty = parseFloat($('#NewPenalty').val()) || 0;

    const total = balance + interest + vat + penalty;

    // Set raw value for form submission
    $('#newDueAmount').val(total.toFixed(2));

    // Show nicely formatted currency string
    $('#dueAmountFormatted').text(
        total.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
    );
}



$('#NewLoanAmount, #NewBalance, #NewInterest, #NewVat, #NewPenalty').on('input', calculateNewDueAmount);




// 📦 Launch Shared Modal
function openLoanAdjustmentModal(mode, model) {
    $('#loanAdjustmentModal form')[0].reset();
    $('.text-danger').text('');

    // Hidden POST fields
    $('#LoanId').val(model.LoanId);
    $('#CustomerId').val(model.CustomerId);
    $('#BranchId').val(model.BranchId);

    // Footer
    $('#footerCustomerName').text(model.CustomerName || 'N/A');
    $('#footerCustomerId').text(model.CustomerId || 'N/A');
    $('#footerLoanId').text(model.LoanId || 'N/A');

    // 🧾 Old Labels
    $('#oldLoanAmountLabel').val(formatCurrency(model.LoanAmount ?? 0));
    $('#oldBalanceLabel').val(formatCurrency(model.Balance ?? 0));
    $('#oldInterestLabel').val(formatCurrency(model.AccrualInterest ?? 0));
    $('#oldVatLabel').val(formatCurrency(model.Tax ?? 0));
    $('#oldPenaltyLabel').val(formatCurrency(model.Penalty ?? 0));
    $('#oldDueAmountLabel').val(formatCurrency(model.DueAmount ?? 0));
    $('#oldVatRateLabel').val(model.VatRate ?? 0);
    $('#oldIntRateLabel').val(model.InterestRate ?? 0);
    $('#oldLoanDateLabel').val(formatDate(model.LoanDate));
    $('#oldDisbursementDateLabel').val(formatDate(model.DisbursementDate));
    $('#oldLoanStatusLabel').val(model.LoanStatus || "N/A");
    $('#oldNextInstallmentLabel').val(formatDate(model.NextInstallmentDate));
    $('#oldPaidLabel').val(formatCurrency(model.Paid ?? 0));

    // ✏️ New Fields
    $('#NewLoanAmount').val(model.LoanAmount ?? 0);
    $('#NewBalance').val(model.Balance ?? 0);
    $('#NewInterest').val(model.AccrualInterest ?? 0);
    $('#NewVat').val(model.Tax ?? 0);
    $('#NewPenalty').val(model.Penalty ?? 0);
    $('#newDueAmount').val(model.DueAmount ?? 0);
    $('#NewVatRate').val(model.VatRate ?? 0);
    $('#NewIntRate').val(model.InterestRate ?? 0);
    $('#NewLoanDate').val(model.LoanDate ? model.LoanDate.split('T')[0] : '');
    $('#NewDisbursementDate').val(model.DisbursementDate ? model.DisbursementDate.split('T')[0] : '');
    $('#NewLoanStatus').val(model.LoanStatus || "Open");
    $('#NewNextInstallmentDate').val(model.NextInstallmentDate ? model.NextInstallmentDate.split('T')[0] : '');
    $('#NewPaid').val(model.Paid ?? 0);

    // Buttons
    $('#submitRequestBtn').toggleClass('d-none', mode !== 'request');
    $('#approveRequestBtn, #rejectRequestBtn').toggleClass('d-none', mode !== 'validation');

    $('#loanAdjustmentModal').modal('show');
}

//function openLoanAdjustmentModal(mode, model) {
//    // Reset form & errors
//    $('#loanAdjustmentModal form')[0].reset();
//    $('.text-danger').text('');

//    // 🧾 Hidden POST fields
//    $('#LoanId').val(model.LoanId);
//    $('#CustomerId').val(model.CustomerId);
//    $('#BranchId').val(model.BranchId);

//    // 👤 Footer summary
//    $('#footerCustomerName').text(model.CustomerName || 'N/A');
//    $('#footerCustomerId').text(model.CustomerId || 'N/A');
//    $('#footerLoanId').text(model.LoanId || 'N/A');

//    // 💼 Old values – Display
//    $('#oldLoanAmountLabel').val(formatCurrency(model.LoanAmount ?? 0));
//    $('#oldBalanceLabel').val(formatCurrency(model.Balance ?? 0));
//    $('#oldInterestLabel').val(formatCurrency(model.AccrualInterest ?? 0));
//    $('#oldVatLabel').val(formatCurrency(model.Tax ?? 0));
//    $('#oldPenaltyLabel').val(formatCurrency(model.Penalty ?? 0));
//    $('#oldDueAmountLabel').val(formatCurrency(model.DueAmount ?? 0));
//    $('#oldVatRateLabel').val(model.VatRate ?? 0);
//    $('#oldIntRateLabel').val(model.InterestRate ?? 0); // ✅ Fix ID name
//    // 🆕 NEW FIELDS - OLD VALUES
//    $('#oldLoanDateLabel').val(formatDate(mode.LoanDate));
//    $('#oldDisbursementDateLabel').val(formatDate(mode.DisbursementDate));
//    $('#oldLoanStatusLabel').val(mode.LoanStatus || "N/A");
//    $('#oldNextInstallmentLabel').val(formatDate(mode.NextInstallmentDate));
//    $('#oldPaidLabel').val(formatCurrency(mode.Paid));


//    // 🆕 Pre-fill new fields
//    $('#NewLoanAmount').val(model.LoanAmount ?? 0);
//    $('#NewBalance').val(model.Balance ?? 0);
//    $('#NewInterest').val(model.AccrualInterest ?? 0);
//    $('#NewVat').val(model.Tax ?? 0);
//    $('#NewPenalty').val(model.Penalty ?? 0);
//    $('#newDueAmount').val(model.DueAmount ?? 0);
//    $('#NewVatRate').val(model.VatRate ?? 0);
//    $('#NewIntRate').val(model.InterestRate ?? 0);

//    // 🎯 Control buttons
//    $('#submitRequestBtn').toggleClass('d-none', mode !== 'request');
//    $('#approveRequestBtn, #rejectRequestBtn').toggleClass('d-none', mode !== 'validation');

//    // 🪟 Show modal
//    $('#loanAdjustmentModal').modal('show');
//}



// ✅ Approve
function approveLoanAdjustment() {
    const loanId = $('#loanAdjustmentForm input[name="LoanId"]').val();
    $.post('/LoanAdjustmentConsole/ApproveRequest', { LoanId: loanId }, function (response) {
        if (response.success) {
            toastr.success(response.message);
            $('#loanAdjustmentModal').modal('hide');
            loadLoanData();
        } else {
            toastr.error(response.message);
        }
    });
}

// ❌ Reject
function rejectLoanAdjustment() {
    const loanId = $('#loanAdjustmentForm input[name="LoanId"]').val();
    $.post('/LoanAdjustmentConsole/RejectRequest', { LoanId: loanId }, function (response) {
        if (response.success) {
            toastr.success(response.message);
            $('#loanAdjustmentModal').modal('hide');
            loadLoanData();
        } else {
            toastr.error(response.message);
        }
    });
}
function normalize(value) {
    return parseFloat((value || "0").toString().replace(/,/g, '').trim());
}

function checkForLoanAdjustmentsChange() {
    const changes = [
        { old: $('#oldLoanAmountLabel').text(), new: $('#NewLoanAmount').val() },
        { old: $('#oldBalanceLabel').text(), new: $('#NewBalance').val() },
        { old: $('#oldInterestLabel').text(), new: $('#NewInterest').val() },
        { old: $('#oldVatLabel').text(), new: $('#NewVat').val() },
        { old: $('#oldPenaltyLabel').text(), new: $('#NewPenalty').val() },
        { old: $('#oldDueAmountLabel').text(), new: $('#newDueAmount').val() },
        { old: $('#oldIntRateLabel').val(), new: $('#NewIntRate').val() },
        { old: $('#oldVatRateLabel').val(), new: $('#NewVatRate').val() }
    ];

    const hasChange = changes.some(pair => normalize(pair.old) !== normalize(pair.new));
    $('#submitRequestBtn').prop('disabled', !hasChange);
}


function AjaxPostAndUpdateResetLoanValue(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);

    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }

    $.validator.unobtrusive.parse(form);
    if (!$(form).valid()) return false;

    function normalize(value) {
        return parseFloat((value || "0").replace(/,/g, '').trim());
    }

    function formatCurrency(val) {
        const num = normalize(val);
        return isNaN(num) ? val : num.toLocaleString(undefined, { minimumFractionDigits: 0 });
    }

    const values = [
        {
            field: "Loan Amount",
            old: $('#oldLoanAmountLabel').val(),
            new: $('#NewLoanAmount').val(),
            isChanged: () => normalize($('#oldLoanAmountLabel').val()) !== normalize($('#NewLoanAmount').val())
        },
        {
            field: "Balance",
            old: $('#oldBalanceLabel').val(),
            new: $('#NewBalance').val(),
            isChanged: () => normalize($('#oldBalanceLabel').val()) !== normalize($('#NewBalance').val())
        },
        {
            field: "Interest",
            old: $('#oldInterestLabel').val(),
            new: $('#NewInterest').val(),
            isChanged: () => normalize($('#oldInterestLabel').val()) !== normalize($('#NewInterest').val())
        },
        {
            field: "VAT",
            old: $('#oldVatLabel').val(),
            new: $('#NewVat').val(),
            isChanged: () => normalize($('#oldVatLabel').val()) !== normalize($('#NewVat').val())
        },
        {
            field: "Penalty",
            old: $('#oldPenaltyLabel').val(),
            new: $('#NewPenalty').val(),
            isChanged: () => normalize($('#oldPenaltyLabel').val()) !== normalize($('#NewPenalty').val())
        },
        {
            field: "Due Amount",
            old: $('#oldDueAmountLabel').val(),
            new: $('#newDueAmount').val(),
            isChanged: () => normalize($('#oldDueAmountLabel').val()) !== normalize($('#newDueAmount').val())
        },
        {
            field: "Interest Rate",
            old: $('#oldIntRateLabel').val(),
            new: $('#NewIntRate').val(),
            isChanged: () => normalize($('#oldIntRateLabel').val()) !== normalize($('#NewIntRate').val())
        },
        {
            field: "VAT Rate",
            old: $('#oldVatRateLabel').val(),
            new: $('#NewVatRate').val(),
            isChanged: () => normalize($('#oldVatRateLabel').val()) !== normalize($('#NewVatRate').val())
        }
    ];

    // === Validate constraints ===
    const loanAmount = normalize($('#NewLoanAmount').val());
    const balance = normalize($('#NewBalance').val());
    const interest = normalize($('#NewInterest').val());

    if (balance > loanAmount) {
        appalert("❌ Balance must not be greater than Loan Amount.", 2, 1);
        $('#NewBalance').focus();
        return false;
    }

    if (interest > loanAmount) {
        appalert("❌ Interest must not be greater than Loan Amount.", 2, 1);
        $('#NewInterest').focus();
        return false;
    }

    // === Reason validation continues ===
    const reasonInput = $('#Reason');
    let reason = reasonInput.val()?.trim() || '';
    const wordCount = reason.split(/\s+/).filter(w => w.length > 0).length;

    if (wordCount < 10) {
        const changedFields = values.filter(v => v.isChanged());

        const changedFieldsSummary = changedFields.length > 0
            ? changedFields.map(v => `${v.field}: ${formatCurrency(v.old)} → ${formatCurrency(v.new)}`).join(', ')
            : "No significant change detected but manual override requested";

        const defaultReason = `Requesting manual override for the following changes: ${changedFieldsSummary}. Kindly review and approve accordingly.`;

        alertify.confirm("Incomplete Reason",
            `<p>⚠️ Your reason must contain at least <strong>10 words</strong>.</p>
         <p>💡 Suggested reason based on detected changes:</p>
         <div class="border rounded bg-light p-2 mb-2 text-muted">${defaultReason}</div>
         <p>Would you like to use this reason?</p>`,
            function () {
                reasonInput.val(defaultReason);
                AjaxPostAndUpdateResetLoanValue(form); // Retry
            },
            function () {
                appalert('Please enter a more complete reason before continuing.', 2, 1);
            }
        ).set('labels', { ok: 'Use Suggested Reason', cancel: 'Let Me Edit' });

        return false;
    }


    let confirmMessage = `
        ⚠️ <strong>You are about to waive existing loan values and apply manual overrides.</strong><br/><br/>
        <u>Review the values that will be changed:</u><br/><br/>
        <table class="table table-bordered table-sm w-100">
            <thead><tr><th>Field</th><th>Old</th><th>New</th><th>Changed</th></tr></thead>
            <tbody>`;

    for (let v of values) {
        const changed = v.isChanged();
        const icon = changed ? '🔁' : '✅';
        const statusClass = changed ? 'text-warning fw-bold' : 'text-success';
        const newValClass = changed ? 'text-warning fw-bold' : '';

        confirmMessage += `
            <tr>
                <td>${v.field}</td>
                <td>${formatCurrency(v.old)}</td>
                <td class="${newValClass}">${formatCurrency(v.new)}</td>
                <td class="${statusClass}">${icon} ${changed ? 'Yes' : 'No'}</td>
            </tr>`;
    }

    confirmMessage += `
            </tbody>
        </table><br/>
        📝 Ensure all entries are accurate. This action will be <strong>logged</strong> and submitted for approval.<br/><br/>
        <strong>Do you wish to proceed with the adjustment?</strong>`;

    alertify.confirm("Confirm Loan Adjustment", confirmMessage,
        function () {
            const $btn = $('#submitRequestBtn');
            const originalText = $btn.html();
            $btn.prop('disabled', true).html(`<span class="spinner-border spinner-border-sm me-1"></span> Processing...`);

            const ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {
                    console.log("Response:", response);
                    $btn.prop('disabled', false).html(originalText);

                    if (response.success) {
                        appalert(response.message, 1, 1);
                        $('#loanAdjustmentModal').modal('hide');
                    } else {
                        appalert(response.message, 2, 1);
                    }
                },
                error: function (err) {
                    console.log("Error:", err);
                    $btn.prop('disabled', false).html(originalText);
                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.statusText, 0, 1);
                    }
                },
                contentType: false,
                processData: false
            };

            if ($(form).attr('enctype') !== "multipart/form-data") {
                delete ajaxConfig.contentType;
                delete ajaxConfig.processData;
            }

            $.ajax(ajaxConfig);
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    ).set('labels', { ok: 'Yes, Proceed', cancel: 'Cancel' });

    return false;
}
