(function () {

    function todayStr() {
        const d = new Date();
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        const day = String(d.getDate()).padStart(2, "0");
        return `${y}-${m}-${day}`;
    }

    function toDate(v) {
        if (!v) return null;
        const d = new Date(v + "T00:00:00");
        return isNaN(d.getTime()) ? null : d;
    }

    function maxStr(a, b) {
        // a/b are "yyyy-MM-dd"
        return (a && b) ? (a > b ? a : b) : (a || b || "");
    }

    function syncIssueExpiryRules(showAlert) {
        const $issue = $("#DepositorIDIssueDate");
        const $exp = $("#DepositorIDExpiryDate");
        if (!$issue.length || !$exp.length) return;

        const t = todayStr();

        const issueVal = ($issue.val() || "").trim();
        const expVal = ($exp.val() || "").trim();

        // expiry must be >= today AND > issueDate (not equal)
        // => min = max(today, issue+1day) when issue exists, else min=today
        let minExp = t;

        if (issueVal) {
            const issueD = toDate(issueVal);
            if (issueD) {
                const nextDay = new Date(issueD);
                nextDay.setDate(nextDay.getDate() + 1);

                const y = nextDay.getFullYear();
                const m = String(nextDay.getMonth() + 1).padStart(2, "0");
                const d = String(nextDay.getDate()).padStart(2, "0");
                const issuePlusOne = `${y}-${m}-${d}`;

                minExp = maxStr(t, issuePlusOne);
            }
        }

        $exp.attr("min", minExp);

        // If expiry already set but invalid => clear it
        if (expVal) {
            const expD = toDate(expVal);
            const minD = toDate(minExp);

            if (expD && minD && expD < minD) {
                $exp.val("");
                if (showAlert) {
                    appalert("Expiry date must be at least tomorrow after issue date and not less than today.", 3, 1);
                }
            }
        }
    }

    $(document).ready(function () {
        syncIssueExpiryRules(false);

        $(document).on("change input", "#DepositorIDIssueDate", function () {
            syncIssueExpiryRules(true);
        });

        $(document).on("change input", "#DepositorIDExpiryDate", function () {
            syncIssueExpiryRules(true);
        });
    });

})();




$(document).ready(function () {
    // Event delegation for handling button clicks
    $(document).on("click", ".btn", function () {
        // Remove underline and blue color from all buttons
        $(".btn").removeClass("clicked");
        // Add underline and blue color to the clicked button
        $(this).addClass("clicked");
    });

    // Handle checkbox state changes
    // Format date mask



});

var vatMap = {};

function previewVat(loanId, interestValue, vatRate, loanAmount) {
    var interestAmount = parseFloat(interestValue) || 0;
    var vat = 0;

    // Only apply VAT if loan amount >= 2,000,000
    if (parseFloat(loanAmount) >= 2000000) {
        vat = Math.round(interestAmount * (parseFloat(vatRate) / 100));
    }

    // Store VAT per loan
    vatMap[loanId] = {
        original: interestAmount,
        vat: vat
    };

    // Recalculate and update total VAT in footer
    let totalVat = 0;
    for (const key in vatMap) {
        if (vatMap.hasOwnProperty(key)) {
            totalVat += vatMap[key].vat || 0;
        }
    }

    const vatFooter = document.getElementById("calculatedVat");
    if (vatFooter) {
        vatFooter.innerText = totalVat.toLocaleString('en-US', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });
    }
}
function applyVatAdjustment(loanId) {
    const interestInput = document.getElementById("interest-" + loanId);
    const vatCheckbox = document.getElementById("vatExclusive-" + loanId);

    // Validate existence
    if (!interestInput || !vatMap[loanId]) return;

    const { original, vat } = vatMap[loanId];

    // Only adjust if VAT checkbox is NOT checked
    if (vatCheckbox && !vatCheckbox.checked) {
        const netInterest = Math.round(original - vat);
        interestInput.value = netInterest.toFixed(0);
    }
}
function AddNote() {
    EditResetModal(null, 'modal', 'modalContent', 'CashDesk', 'InitializeData', '_Note', 'new_depositor', 'NOTE', 'modalLabel')
}
function AddDepositor() {
    EditResetModal(null, 'modal', 'modalContent', 'CashDesk', 'InitializeData', '_DepositerForm', 'new_depositor', 'Depositor information', 'modalLabel')
    $('#DepositorIDIssueDate, #DepositorIDExpiryDate').on('input', function () {
        var value = $(this).val();
        if (value.length === 4 || value.length === 7) {
            $(this).val(value + '/');
        }
    });

    // Format telephone mask
    $('#DepositorTelephone').on('input', function () {
        var value = $(this).val().replace(/\D/g, '');
        if (value.length > 3) {
            value = value.replace(/(\d{3})(\d)/, '$1-$2');
        }
        if (value.length > 6) {
            value = value.replace(/(\d{3})(\d{2})(\d)/, '$1-$2-$3');
        }
        $(this).val(value);
    });
}

$(document).on('input', '.amount-input, .fee-input, .interest-input, .penalty-input, .loan-amount-input', function () {
    var $row = $(this).closest('tr');

    var amount = parseFloat($row.find('.amount-input').val()) || 0;
    var fee = parseFloat($row.find('.fee-input').val()) || 0;
    var interest = parseFloat($row.find('.interest-input').val()) || 0;
    var penalty = parseFloat($row.find('.penalty-input').val()) || 0;
    var loanAmount = parseFloat($row.find('.loan-amount-input').val()) || 0;
    var vat = parseFloat($('#calculatedVat').text()) || 0;

    // ✅ Add VAT only if checkbox is CHECKED
    var isVatExclusive = $row.find('.vat-exclusive-check').prop('checked');

    var total = amount + fee + interest + penalty + loanAmount;
    if (isVatExclusive) {
        total += vat;
    }

    $row.find('.total-span').text(total.toFixed(2));
    calculateTableTotal();
});

function handleVatToggle(loanId) {
    const interestInput = document.getElementById(`interest-${loanId}`);
    const vatCheckbox = document.getElementById(`vatExclusive-${loanId}`);
    const vatElement = document.getElementById("calculatedVat");

    const capitalInput = document.getElementById(`capital-${loanId}`);
    const penaltyInput = document.getElementById(`penalty-${loanId}`);
    const totalSpan = document.getElementById(`total-${loanId}`);

    if (!interestInput || !vatCheckbox || !vatElement || !capitalInput || !penaltyInput || !totalSpan)
        return;

    const vat = parseFloat(vatElement.textContent) || 0;
    let interest = parseFloat(interestInput.value) || 0;
    const capital = parseFloat(capitalInput.value) || 0;
    const penalty = parseFloat(penaltyInput.value) || 0;

    const isExclusive = vatCheckbox.checked;

    // Update interest
    if (isExclusive) {
        interest += vat;
    } else {
        interest -= vat;
    }
    interestInput.value = interest.toFixed(2);

    // Recalculate total
    const total = interest + vat + penalty + capital;
    totalSpan.textContent = total.toFixed(2);

    // Format numbers with financial separators
    const formatCurrency = (num) =>
        num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    // Prepare message
    const message = `
🧾 ${isExclusive ? "VAT Exclusive Selected" : "VAT Inclusive Selected"}

  [Capital]     : ${formatCurrency(capital)}
  [Interest]    : ${formatCurrency(interest)}
  [VAT]         : ${formatCurrency(vat)}
  [Penalty]     : ${formatCurrency(penalty)}
  -------------------------------
  [Total Due]   : ${formatCurrency(total)}
`.trim();

    // Show notification
    if (typeof appalert === "function") {
        appalert(message, 2, 1);
    } else {
        alert(message);
    }
}
function calculateTableTotal() {
    var total = 0;     // total of all entered rows (and charge if enabled)
    var totalVat = 0;  // keep your VAT logic (currently 0)

    // helper
    var n = function (v) {
        var x = parseFloat(v);
        return Number.isFinite(x) ? x : 0;
    };

    // -------------------------------
    // 1) Sum row totals
    // -------------------------------
    $('#myDataTableT tbody tr').each(function () {
        var $row = $(this);

        var capital = n($row.find('.amount-input').val());
        var interest = n($row.find('.interest-input').val());
        var penalty = n($row.find('.penalty-input').val());

        // loan repayment rows may not have .fee-input (defensive)
        var fee = n($row.find('.fee-input').val());

        var rowTotal = capital + interest + penalty + fee;

        // update row total span (format optional)
        $row.find('.total-span').text(rowTotal);

        total += rowTotal;
    });

    // -------------------------------
    // 2) Add repayment collection charge (NEW)
    // -------------------------------
    var chargeApply = $("#repaymentChargeApply").is(":checked") === true;
    var chargeMode = ($("#repaymentChargeMode").val() || "ADD_TO_TOTAL").toString().trim().toUpperCase();
    var chargeAmount = n($("#repaymentCollectionCharge").val());

    var chargeAdded = 0;
    if (chargeApply && chargeAmount > 0 && chargeMode === "ADD_TO_TOTAL") {
        total += chargeAmount;
        chargeAdded = chargeAmount;
    }

    // keep hidden mirrors synced (safe for posting + other JS)
    $("#hRepaymentChargeAmount").val(chargeAmount);
    $("#hRepaymentChargeApply").val(chargeApply ? "1" : "0");
    $("#hRepaymentChargeMode").val(chargeMode);

    // -------------------------------
    // 3) Update footer totals
    // -------------------------------
    $('#tableTotal').text(total.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));
    $('#calculatedVat').text(totalVat.toLocaleString('en-US', { minimumFractionDigits: 1 }));

    // -------------------------------
    // 4) Update balance (if applicable)
    // -------------------------------
    var totalNotes = n($("#totalNoteAmount").val()); // if not present => 0
    var balance = totalNotes - total;
    $('#tableBalance').text(balance.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));

    // -------------------------------
    // 5) Optional: show included charge hint (if element exists)
    // -------------------------------
    // if ($('#includedChargeHint').length) {
    //     $('#includedChargeHint').text(chargeAdded > 0 ? chargeAdded.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }) : '');
    // }
}
function getRepaymentCollectionCharge() {
    var apply = $("#repaymentChargeApply").is(":checked");
    var mode = ($("#repaymentChargeMode").val() || "").toUpperCase();
    var amt = parseFloat($("#repaymentCollectionCharge").val()) || 0;

    if (!apply || amt <= 0) return 0;
    if (mode && mode !== "ADD_TO_TOTAL") return 0; // only add when mode is ADD_TO_TOTAL
    return amt;
}

// Bind event listeners to the input fields
// Always recompute when repayment charge changes (delegated binding)
$(document).on('input change',
    '#repaymentCollectionCharge, #repaymentChargeApply, #repaymentChargeMode',
    function () {
        calculateTableTotal();
    }
);

// Also recompute on row input
$(document).on('input',
    '#myDataTableT .amount-input, #myDataTableT .interest-input, #myDataTableT .penalty-input, #myDataTableT .fee-input',
    function () {
        calculateTableTotal();
    }
);

// Run once on load (important)
$(function () { calculateTableTotal(); });



// Optional: Single VAT calculation function for real-time updates
// VAT calculation function
function calculateVat(interestInput, vatRate) {
    // Parse the interest value from the input
    var interest = parseFloat(interestInput.value) || 0;

    // Calculate VAT for the current row
    var vat = interest * vatRate / 100;

    // Format VAT for display with thousands separators and no decimal places
    var formattedVat = vat.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    // Recalculate the total VAT for all rows
    calculateTotalVat();

    // Find the element with id 'calculatedVat' and update its content
    var vatElement = document.getElementById('calculatedVat');
    if (vatElement) {
        vatElement.textContent = formattedVat;
    } else {
        console.error('Element with id "calculatedVat" not found in the DOM.');
    }
}

function calculateTotalVat() {
    var totalVat = 0;

    // Iterate over all interest inputs and calculate the total VAT
    $('#myDataTableT tbody tr').each(function () {
        var interest = parseFloat($(this).find('.interest-input').val()) || 0;
        var vatRate = parseFloat($(this).find('.vat-rate').text()) || 0; // Assuming a hidden cell for VAT rate
        totalVat += interest * vatRate / 100;
    });

    // Format the total VAT for display
    var formattedTotalVat = totalVat.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    // Update the VAT footer cell
    var vatFooterElement = document.getElementById('calculatedVat');
    if (vatFooterElement) {
        vatFooterElement.textContent = formattedTotalVat;
    } else {
        console.error('Footer element with id "calculatedVat" not found in the DOM.');
    }
}






function checkTotalNotes() {
    var totalNotes = parseFloat($("#totalNoteAmount").val());

    if (totalNotes === 0) {
        appalert("Please enter cash in the denomination box.", 3, 1);
        return false;
    }

    return true;
}

function calculateTotalAmount() {
    var total = 0;
    var anyRowsSelected = false;

    $('#myDataTableT tbody tr').each(function () {
        if ($(this).find('.form-check-input').prop('checked')) {
            anyRowsSelected = true;
            total += parseFloat($(this).find('.total-span').text());
        }
    });

    return { total: total, anyRowsSelected: anyRowsSelected };
}


function validateTotalAmount(total, totalNotes) {
    if (!total.anyRowsSelected) {
        appalert("Please select at least one account to perform operation.", 3, 1);
        return false;
    }

    if (total.total === 0) {
        appalert("Please enter an amount.", 3, 1);
        return false;
    }

    if (total.total !== totalNotes) {
        appalert("Amount entered must equal the total of notes entered. Make sure you have checked/unchecked corresponding accounts. Please reevaluate and enter again.", 3, 1);
        return false;
    }

    return true;
}

function applyPayment() {
    var loanId = document.getElementById('modalLoanId').value;
    var amount = parseFloat(document.getElementById('modalAmount').value) || 0;
    var interest = parseFloat(document.getElementById('modalInterest').value) || 0;
    var penalty = parseFloat(document.getElementById('modalPenalty').value) || 0;

    // Update the row with the values entered from the modal
    var capitalInput = document.getElementById('capital-' + loanId);
    var interestInput = document.getElementById('interest-' + loanId);
    var penaltyInput = document.getElementById('penalty-' + loanId);
    var totalSpan = document.getElementById('total-' + loanId);

    // Check if elements are found
    if (!capitalInput || !interestInput || !penaltyInput || !totalSpan) {
        console.error("One or more elements not found. Check your IDs and ensure they match.");
        return;
    }

    // Update the row with entered values
    capitalInput.value = amount;
    interestInput.value = interest;
    penaltyInput.value = penalty;

    // Calculate total (capital + interest + penalty)
    var total = amount + interest + penalty;
    totalSpan.innerText = total;
    calculateTableTotal();
    // Calculate VAT based on the entered interest
    calculateVat(interestInput, selectedVatRate);

    // Close the modal
    var paymentModal = bootstrap.Modal.getInstance(document.getElementById('paymentModal'));
    paymentModal.hide();
}
function firstNonEmptyValue(selectors) {
    // selectors: "#a, #b, #c"
    var parts = selectors.split(",").map(s => s.trim()).filter(Boolean);
    for (var i = 0; i < parts.length; i++) {
        var $el = $(parts[i]);
        if (!$el.length) continue;
        var v = $el.val();
        v = (v === undefined || v === null) ? "" : String(v).trim();
        if (v) return v;
    }
    // fallback: return empty string (not null) so binder keeps it
    return "";
}
function parseNumber(v) {
    var x = parseFloat(v);
    return Number.isFinite(x) ? x : 0;
}

// reads numeric from a td (handles commas)
function tdNumber($row, index) {
    var t = ($row.find('td:eq(' + index + ')').text() || "").replace(/,/g, '').trim();
    return parseNumber(t);
}

// Try to read VAT total from footer (#calculatedVat).
// If footer is "0.0" or not currency, still ok.
function readVatFooter() {
    if (!$("#calculatedVat").length) return 0;
    var t = ($("#calculatedVat").text() || "").replace(/[^0-9.\-]/g, '').trim();
    return parseNumber(t);
}

function collectDeposits() {
    const deposits = [];

    const n = v => {
        const x = parseFloat(v);
        return Number.isFinite(x) ? x : 0;
    };

    const operationType = $('#OperationType').val();
    const accountingDate = $('#BulkDeposit_AccountingDate').val();

    const branchId = $('#branchInput').val();
    const branchName = ($('#branchInput option:selected').text() || "").trim();
    const chartOfAccountId = $('#account_number').val();
    const chartOfAccountName = ($('#account_number option:selected').text() || "").trim();

    // operator source type (support both names safely)
    const $sourceInput =
        $("input[name='BulkDeposit.SourceType']:checked")
            .add("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked");

    const sourceType = $sourceInput.val() || null;
    const operatorLabel = ($sourceInput.closest('label').text() || "").trim() || sourceType;

    // Representative (Depositor) fields (FIRST NON EMPTY)
    const depositorName = firstNonEmptyValue('#DepositorName');
    const depositorPhoneNumber = firstNonEmptyValue('#DepositorTelephone, #DepositerTelephone');
    const depositorIDNumber = firstNonEmptyValue('#DepositorIDNumber');
    const depositorIssueDate = firstNonEmptyValue('#DepositorIDIssueDate');
    const depositorExpiryDate = firstNonEmptyValue('#DepositorIDExpiryDate');
    const depositorNumberPlaceOfIssue = firstNonEmptyValue('#DepositorIDNumberPlaceOfIssue');
    const depositorNote = firstNonEmptyValue('#DepositorNote, #DepositerNote');

    // Collection charge (operation-level)
    const chargeApply = $("#repaymentChargeApply").length ? $("#repaymentChargeApply").is(":checked") : false;
    const chargeMode = ($("#repaymentChargeMode").val() || "ADD_TO_TOTAL").toUpperCase();
    const chargeAmount = n($("#repaymentCollectionCharge").val());
    const repaymentCharge = (chargeApply && chargeAmount > 0 && chargeMode === "ADD_TO_TOTAL") ? chargeAmount : 0;

    $('#myDataTableT tbody tr').each(function () {
        const $row = $(this);

        // ✅ CONF checkbox (first checkbox in row)
        const rowChecked = $row.find('td .form-check-input').first().prop('checked');
        if (!rowChecked) return;

        const deposit = {
            OperationType: operationType,
            CustomerId: $('#customerId').val(),
            AccountingDate: accountingDate,

            BranchId: branchId,
            BranchName: branchName,
            ChartOfAccountId: chartOfAccountId,
            ChartOfAccountName: chartOfAccountName,

            SourceType: sourceType,
            SourceTypeLabel: operatorLabel,

            // keep your flag naming
            isDepositDoneByAccountOwner: rowChecked,

            // ✅ IMPORTANT: send both flat AND nested object for model binding
            DepositorName: depositorName,
            DepositorTelephone: depositorPhoneNumber,
            DepositorNote: depositorNote,
            DepositorIDNumber: depositorIDNumber,
            DepositorIDIssueDate: depositorIssueDate,
            DepositorIDExpiryDate: depositorExpiryDate,
            DepositorIDNumberPlaceOfIssue: depositorNumberPlaceOfIssue,

            Depositer: { // <-- This is what your C# screenshot is binding to
                DepositorName: depositorName,
                DepositorTelephone: depositorPhoneNumber,
                DepositorNote: depositorNote,
                DepositorIDNumber: depositorIDNumber,
                DepositorIDIssueDate: depositorIssueDate,
                DepositorIDExpiryDate: depositorExpiryDate,
                DepositorIDNumberPlaceOfIssue: depositorNumberPlaceOfIssue
            }
        };

        // ✅ LOAN REPAYMENT
        if (operationType === "LoanRepaymentMomocashCollection") {
            const loanId = ($row.find('td:eq(0)').text() || "").trim();

            const capital = n($row.find('.amount-input').val());
            const interest = n($row.find('.interest-input').val());
            const penalty = n($row.find('.penalty-input').val());

            const vatExclusive = $row.find('.vat-exclusive-check').prop('checked') === true;

            // Hidden VAT rate column in your table:
            // <td style="display:none;">@d.VatRate</td>
            // Based on your header order, VatRate is td:eq(5) (0=LoanId,1=Date,2=Amount(hidden),3=Balance,4=Int,5=VRate,6=DueA)
            const vatRate = tdNumber($row, 5); // e.g. 19.25

            // loan table has no fee-input; keep defensive anyway
            const rowFee = n($row.find('.fee-input').val());

            // ✅ VAT amount rule:
            // - If VAT is Exclusive => VAT is added on top of interest (typical)
            // - If VAT is Inclusive => VAT is included in interest => VAT amount can be derived if you want,
            //   but safest is 0 (meaning "already included") unless your backend expects the extracted VAT portion.
            let vatAmount = 0;

            if (vatExclusive && vatRate > 0 && interest > 0) {
                vatAmount = interest * (vatRate / 100);
            }

            // ✅ Fee includes repayment charge (since one loan per operation)
            deposit.Fee = rowFee + repaymentCharge;

            // ✅ Total includes VAT (if exclusive) + charge
            // If VAT is inclusive, do NOT add VAT again.
            deposit.Total = (capital + interest + penalty + rowFee) + repaymentCharge + vatAmount;

            deposit.LoanId = loanId;

            // Your server naming:
            deposit.Principal = capital;   // you renamed capital -> Principal
            deposit.Interest = interest;
            deposit.Penalty = penalty;
            deposit.Amount = capital;
            deposit.Capital = capital;
            
            // VAT fields
            deposit.VatRate = vatRate;
            deposit.IsVatExclusive = vatExclusive;
            deposit.VatAmount = vatAmount;           // ✅ per-loan VAT
            deposit.VAT = vatAmount;           // ✅ per-loan VAT
            deposit.Tax = vatAmount;           // ✅ per-loan VAT
            deposit.TotalVatAmount = readVatFooter(); // ✅ operation VAT (optional but helpful)

            // Optional: charge separately too
            deposit.CollectionCharge = repaymentCharge;

            deposits.push(deposit);
            return;
        }


        // ✅ other operations unchanged
        deposit.AccountNumber = ($row.find('td:eq(0)').text() || "").trim();
        deposit.AccountType = ($row.find('td:eq(1)').text() || "").trim();
        deposit.Amount = n($row.find('.amount-input').val());
        deposit.Fee = n($row.find('.fee-input').val());
        deposit.Penalty = n($row.find('.penalty-input').val());
        deposit.Interest = n($row.find('.interest-input').val());
        deposit.Total = n($row.find('.total-span').text());
        deposit.IsChargesInclussive = $row.find('.check-inclussive').prop('checked') === true;

        deposits.push(deposit);
    });

    return deposits;
}

//function collectDeposits() {
//    const deposits = [];

//    // helpers
//    const n = v => {
//        const x = parseFloat(v);
//        return Number.isFinite(x) ? x : 0;
//    };
//    const tv = sel => {
//        const $el = $(sel);
//        if (!$el.length) return null;
//        const v = $el.val();
//        return (v === undefined || v === null) ? null : String(v).trim();
//    };

//    // one-time values
//    const accountingDate = $('#BulkDeposit_AccountingDate').val();

//    // NEW: branch + GL + source type
//    const branchId = $('#branchInput').val();
//    const branchName = $('#branchInput option:selected').text().trim();
//    const chartOfAccountId = $('#account_number').val();
//    const chartOfAccountName = $('#account_number option:selected').text().trim();
//    const sourceType = $("input[name='BulkDeposit.SourceType']:checked").val();

//    // read depositor fields with fallback for old IDs (typos)
//    const depositorName = tv('#DepositorName');
//    const depositorPhoneNumber = tv('#DepositorTelephone, #DepositerTelephone'); // fallback
//    const depositorIDNumber = tv('#DepositorIDNumber');
//    const depositorIssueDate = tv('#DepositorIDIssueDate');
//    const depositorExpiryDate = tv('#DepositorIDExpiryDate');
//    const depositorNumberPlaceOfIssue = tv('#DepositorIDNumberPlaceOfIssue');
//    const depositorNote = tv('#DepositorNote, #DepositerNote'); // fallback

//    $('#myDataTableT tbody tr').each(function () {
//        const $row = $(this);
//        const rowChecked = $row.find('.form-check-input').first().prop('checked');
//        if (!rowChecked) return;

//        const deposit = {
//            // table/operation data
//            AccountNumber: $row.find('td:eq(0)').text().trim(),
//            Amount: n($row.find('.amount-input').val()),
//            Fee: n($row.find('.fee-input').val()),
//            Penalty: n($row.find('.penalty-input').val()),
//            Interest: n($row.find('.interest-input').val()),
//            Total: n($row.find('.total-span').text()),
//            AccountType: $row.find('td:eq(1)').text().trim(),
//            isDepositDoneByAccountOwner: rowChecked,
//            IsChargesInclussive: $row.find('.check-inclussive').prop('checked') === true,
//            OperationType: $('#OperationType').val(),
//            CheckName: tv('#CheckName'),
//            CheckNumber: tv('#CheckNumber'),
//            IsSWS: true,
//            CustomerId: $('#customerId').val(),

//            // NEW: momocash / bulk-deposit context
//            SourceType: sourceType,
//            BranchId: branchId,
//            BranchName: branchName,
//            ChartOfAccountId: chartOfAccountId,
//            ChartOfAccountName: chartOfAccountName,

//            LoanApplicationId: $row.find('.loan-application-id').val(),
//            Period: $row.find('.period').val(),
//            AccountingDate: accountingDate,

//            // depositor fields — use EXACT C# property names
//            DepositorName: depositorName,
//            DepositorPhoneNumber: depositorPhoneNumber,
//            DepositorIDNumber: depositorIDNumber,
//            DepositorIssueDate: depositorIssueDate,
//            DepositorExpiryDate: depositorExpiryDate,
//            DepositorNumberPlaceOfIssue: depositorNumberPlaceOfIssue,
//            DepositorNote: depositorNote
//        };

//        deposits.push(deposit);
//    });

//    return deposits;
//}



function collectDepositorInfo() {
    const tv = (sel) => {
        const $el = $(sel);
        if (!$el.length) return null;
        const v = $el.val();
        return (v === undefined || v === null) ? null : String(v).trim();
    };

    return {
        DepositorName: tv('#DepositorName'),
        DepositorPhoneNumber: tv('#DepositorTelephone, #DepositerTelephone'),
        DepositorIDNumber: tv('#DepositorIDNumber'),
        DepositorIssueDate: tv('#DepositorIDIssueDate'),
        DepositorExpiryDate: tv('#DepositorIDExpiryDate'),
        DepositorNumberPlaceOfIssue: tv('#DepositorIDNumberPlaceOfIssue'),
        DepositorNote: tv('#DepositorNote, #DepositerNote')
    };
}

function resetDepositorForm() {
    $('#DepositorName').val('');
    $('#DepositerTelephone').val('');
    $('#DepositorIDNumber').val('');
    $('#DepositorIDIssueDate').val('');
    $('#DepositorIDExpiryDate').val('');
    $('#DepositorIDNumberPlaceOfIssue').val('');
    $('#DepositerNote').val('');
    $('#Note').val('')
}
function Reprint() {
    ReportView("CashDesk", null, "GetReport", null, null, "receipts", "ReportParameterLess");

}
function confirmTransaction(title, message, ajaxUrl, data, operationType) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response && response.success) {
                        successCallback(response, operationType);
                    } else {
                        if (!response) {
                            alert("Your session is expired.");
                        } else {
                            failureCallback(response);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    appalert("Your session is expired Or An error occurred while processing the transaction. Please try again later.", 0, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

function successCallback(response, operationType) {
    appalert(response.message, 1, 1);
    resetDepositorForm();
    switch (operationType) {
        case 'CashInMomocashCollection':
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'cashin_momokash_collection');
            break;
        case 'LoanRepaymentMomocashCollection':
            GetMemberData($("#customerId").val(), '_MomocashCollectionDesk', 'datalistingview', 'repayment_momokash_collection');
            break;
        default:
            break;
    }
}
function formatAmount(val) {
    var n = parseFloat(val);
    if (!Number.isFinite(n)) n = 0;
    return n.toLocaleString(undefined, { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

function escapeHtml(str) {
    if (str === null || str === undefined) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function failureCallback(response) {
    appalert(response.message || "Your session is expired Or An error occurred while processing the transaction", 3, 1);
}

function PostCashIn() {
    var deposits = collectDeposits();
    if (deposits.length === 0) {
        appalert("Please select at least one account to perform the cash-in.", 3, 1);
        return;
    }

    // 1) Require Branch selection
    var branchId = $('#branchInput').val();
    var branchName = $('#branchInput option:selected').text().trim() || "-";
    if (!branchId) {
        appalert("Please select the branch for this Momocash collection.", 3, 1);
        return;
    }

    // 2) Require Momocash collection GL account
    var momoCollectionAccountId = $('#account_number').val();
    var momoCollectionAccountName = $('#account_number option:selected').text().trim() || "-";
    if (!momoCollectionAccountId) {
        appalert("Please select the Momocash collection GL account.", 3, 1);
        return;
    }

    // 3) Validate total vs per-row input & build row summaries
    var totalInfo = calculateTotalAmount();

    var selectedTotalAmount = 0;
    var rowSummaries = [];   // for table
    var depositsTotal = 0;
    var loanTotal = 0;
    var totalFee = 0;

    $('#myDataTableT tbody tr').each(function () {
        var $row = $(this);
        var checked = $row.find('.form-check-input').first().prop('checked');
        if (!checked) return;

        var accountNumber = $row.find('td:eq(0)').text().trim();
        var accountType = $row.find('td:eq(1)').text().trim();
        var amount = parseFloat($row.find('.amount-input').val()) || 0;
        var fee = parseFloat($row.find('.fee-input').val()) || 0;
        var total = amount + fee;

        selectedTotalAmount += total;
        totalFee += fee;

        // classify deposit vs loan (defensive – your cash-in table excludes loans already)
        if (accountType.toLowerCase().includes("loan")) {
            loanTotal += total;
        } else {
            depositsTotal += total;
        }

        rowSummaries.push({
            accountNumber: accountNumber,
            accountType: accountType,
            amount: amount,
            fee: fee,
            total: total
        });
    });

    if (totalInfo.total !== selectedTotalAmount) {
        appalert("The total amount does not match the sum of the selected account amounts and fees.", 3, 1);
        return;
    }

    // 4) Attach currency notes & depositor
    deposits[0].currencyNotes = collectCurrencyNotes();
    //deposits[0].Depositer = collectDepositorInfo();

    // 5) Attach branch + GL to payload
    deposits[0].BranchId = branchId;
    deposits[0].BranchName = branchName;
    deposits[0].ChartOfAccountId = momoCollectionAccountId;
    deposits[0].ChartOfAccountName = momoCollectionAccountName;

    // 6) Require operator (MTN / Orange)
    var $sourceInput =
        $("input[name='BulkDeposit.SourceType']:checked")
            .add("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked");

    var sourceType = $sourceInput.val();
    if (!sourceType) {
        appalert("Please select operator type: either MTN Mobile Money or Orange Money.", 3, 1);
        return;
    }

    var operatorLabel = $sourceInput.closest('label').text().trim() || sourceType;
    deposits[0].SourceType = sourceType;
    deposits[0].SourceTypeLabel = operatorLabel;

    // 7) Extra context for confirmation
    var accountingDate = $('#BulkDeposit_AccountingDate').val() || "-";
    var memberName = $('#memberName').val() || "-";
    var memberAccountNumber = $('#memberAccountNumber').val() || "";

    if (!memberAccountNumber && rowSummaries.length > 0) {
        // fallback: use first selected account number
        memberAccountNumber = rowSummaries[0].accountNumber || "";
    }

    // 8) Build HTML table for confirm dialog
    var rowsHtml = rowSummaries.map(function (r) {
        return (
            '<tr>' +
            '<td>' + escapeHtml(r.accountType) + '</td>' +
            '<td class="text-end">' + formatAmount(r.amount) + '</td>' +
            '<td class="text-end">' + formatAmount(r.fee) + '</td>' +
            '</tr>'
        );
    }).join('');

    var html =
        '<div class="momo-confirm">' +
        '<p><strong>Member:</strong> ' + escapeHtml(memberName) + '</p>' +
        (memberAccountNumber
            ? '<p><strong>Member Account Number:</strong> ' + escapeHtml(memberAccountNumber) + '</p>'
            : '') +
        '<p>You\'re about to perform a <strong>CASH-IN of ' + formatAmount(totalInfo.total) + '.</strong></p>' +
        '<p><strong>Branch:</strong> ' + escapeHtml(branchName) + '<br />' +
        '<strong>Operator:</strong> ' + escapeHtml(operatorLabel) + '<br />' +
        '<strong>Collection GL:</strong> ' + escapeHtml(momoCollectionAccountName) + '<br />' +
        '<strong>Accounting Date:</strong> ' + escapeHtml(accountingDate) + '</p>' +

        '<h6 class="text-success mt-3 mb-2">Member Accounts</h6>' +
        '<table class="table table-sm table-bordered mb-2">' +
        '<thead>' +
        '<tr>' +
        '<th>Account Type</th>' +
        '<th class="text-end">Amount</th>' +
        '<th class="text-end">Fee</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>' +
        rowsHtml +
        '<tr class="fw-bold bg-light">' +
        '<td>Total</td>' +
        '<td class="text-end">' + formatAmount(depositsTotal + loanTotal) + '</td>' +
        '<td class="text-end">' + formatAmount(totalFee) + '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +

        '<table class="table table-sm table-bordered mb-0">' +
        '<tbody>' +
        '<tr>' +
        '<td><strong>Deposits Total</strong></td>' +
        '<td class="text-end">' + formatAmount(depositsTotal) + '</td>' +
        '</tr>' +
        '<tr>' +
        '<td><strong>Loan Total</strong></td>' +
        '<td class="text-end">' + formatAmount(loanTotal) + '</td>' +
        '</tr>' +
        '<tr class="fw-bold bg-light">' +
        '<td><strong>Grand Total</strong></td>' +
        '<td class="text-end">' + formatAmount(totalInfo.total) + '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +
        '</div>';

    // 9) Ask for confirmation (message is now HTML)
    confirmTransaction(
        '💰 CONFIRM CASH-IN OPERATION',
        html,
        '/CashDesk/PostRequestCash',
        deposits,
        'CashInMomocashCollection'
    );
}
function collectRepaymentChargeInfo() {
    var amount = parseFloat($("#repaymentCollectionCharge").val()) || 0;
    var apply = $("#repaymentChargeApply").is(":checked");
    var mode = $("#repaymentChargeMode").val() || "ADD_TO_TOTAL";
    var note = ($("#repaymentChargeNote").val() || "").trim();

    return {
        Apply: apply,
        Amount: amount,
        Mode: mode,       // ADD_TO_TOTAL | SEPARATE
        Note: note
    };
}

function PostLoanRepayment() {
    var deposits = collectDeposits();

    if (deposits.length !== 1) {
        appalert("Only one loan can be paid at an instant. Please select exactly one loan line.", 3, 1);
        return;
    }

    // 1) Require Branch selection
    var branchId = $('#branchInput').val();
    var branchName = $('#branchInput option:selected').text().trim() || "-";
    if (!branchId) {
        appalert("Please select the branch for this Momocash loan repayment collection.", 3, 1);
        return;
    }

    // 2) Require collection GL
    var momoCollectionAccountId = $('#account_number').val();
    var momoCollectionAccountName = $('#account_number option:selected').text().trim() || "-";
    if (!momoCollectionAccountId) {
        appalert("Please select the Momocash collection GL account.", 3, 1);
        return;
    }

    // 3) Require operator type
    var $sourceInput =
        $("input[name='BulkDeposit.SourceType']:checked")
            .add("input[name='AddOtherTransactionMobileMoneyCommand.SourceType']:checked");

    var sourceType = $sourceInput.val();
    if (!sourceType) {
        appalert("Please select operator type: either MTN Mobile Money or Orange Money.", 3, 1);
        return;
    }
    var operatorLabel = $sourceInput.closest('label').text().trim() || sourceType;

    // 4) totals from your footer
    var totalInfo = calculateTotalAmount(); // must return { total: number, ... }
    var tableTotal = parseFloat($("#tableTotal").text().replace(/[^\d.-]/g, "")) || totalInfo.total || 0;

    // 5) Loan repayment breakdown from selected row (the single deposit object)
    var d = deposits[0];
    var capital = parseFloat(d.Amount) || 0;
    var interest = parseFloat(d.Interest) || 0;
    var penalty = parseFloat(d.Penalty) || 0;
    var vatMode = d.IsVatExclusive ? "EXCLUSIVE" : "INCLUSIVE";
    var vatValue = parseFloat($("#calculatedVat").text().replace(/[^\d.-]/g, "")) || 0;

    if ((capital + interest + penalty) <= 0) {
        appalert("Please enter at least one value (capital / interest / penalty).", 3, 1);
        return;
    }

    // 6) Attach currency + depositor + charge info
    d.currencyNotes = collectCurrencyNotes();
    //d.Depositer = collectDepositorInfo(); // your existing function
    d.CollectionCharge = collectRepaymentChargeInfo();

    // 7) Attach branch + GL + operator to payload (top-level)
    d.BranchId = branchId;
    d.BranchName = branchName;
    d.ChartOfAccountId = momoCollectionAccountId;
    d.ChartOfAccountName = momoCollectionAccountName;

    d.SourceType = sourceType;
    d.SourceTypeLabel = operatorLabel;

    // 8) Confirmation context
    var accountingDate = $('#BulkDeposit_AccountingDate').val() || "-";
    var memberName = ($('#memberName').val() || "").trim() || "@Model.Customer.name";

    // 9) Build confirm HTML
    var chargeLine = "";
    if (d.CollectionCharge && d.CollectionCharge.Apply && (parseFloat(d.CollectionCharge.Amount) || 0) > 0) {
        chargeLine =
            '<tr>' +
            '<td><strong>Collection Charge</strong></td>' +
            '<td class="text-end">' + formatAmount(d.CollectionCharge.Amount) + '</td>' +
            '</tr>';
    }

    var depositorBlock = "";
    if (d.Depositer && (d.Depositer.DepositorName || d.Depositer.DepositorTelephone)) {
        depositorBlock =
            '<p class="mb-1"><strong>Representative:</strong> ' + escapeHtml(d.Depositer.DepositorName || "-") + '</p>' +
            '<p class="mb-2"><strong>Phone:</strong> ' + escapeHtml(d.Depositer.DepositorTelephone || "-") + '</p>';
    }

    var html =
        '<div class="momo-confirm">' +
        '<p class="mb-2"><strong>Member:</strong> ' + escapeHtml(memberName) + '</p>' +
        depositorBlock +
        '<p>You\'re about to perform a <strong>LOAN REPAYMENT of ' + formatAmount(tableTotal) + '.</strong></p>' +
        '<p class="mb-2">' +
        '<strong>Branch:</strong> ' + escapeHtml(branchName) + '<br />' +
        '<strong>Operator:</strong> ' + escapeHtml(operatorLabel) + '<br />' +
        '<strong>Collection GL:</strong> ' + escapeHtml(momoCollectionAccountName) + '<br />' +
        '<strong>Accounting Date:</strong> ' + escapeHtml(accountingDate) +
        '</p>' +

        '<h6 class="text-success mt-3 mb-2">Repayment Breakdown</h6>' +
        '<table class="table table-sm table-bordered mb-2">' +
        '<tbody>' +
        '<tr><td><strong>Capital</strong></td><td class="text-end">' + formatAmount(capital) + '</td></tr>' +
        '<tr><td><strong>Interest</strong></td><td class="text-end">' + formatAmount(interest) + '</td></tr>' +
        '<tr><td><strong>Penalty</strong></td><td class="text-end">' + formatAmount(penalty) + '</td></tr>' +
        '<tr><td><strong>VAT Mode</strong></td><td class="text-end">' + escapeHtml(vatMode) + '</td></tr>' +
        '<tr><td><strong>Calculated VAT</strong></td><td class="text-end">' + formatAmount(vatValue) + '</td></tr>' +
        chargeLine +
        '<tr class="fw-bold bg-light">' +
        '<td><strong>Grand Total</strong></td>' +
        '<td class="text-end">' + formatAmount(tableTotal) + '</td>' +
        '</tr>' +
        '</tbody>' +
        '</table>' +
        '</div>';

    // 10) Confirm and post
    confirmTransaction(
        '✅ CONFIRM LOAN REPAYMENT OPERATION',
        html,
        '/CashDesk/PostRequestCash',
        deposits,
        'LoanRepaymentMomocashCollection'
    );
}





function getSelectedAccountNumbers() {
    var selectedAccountNumbers = [];
    $('#myDataTableT tbody tr').each(function () {
        // Check if the checkbox in this row is selected
        if ($(this).find('.form-check-input').prop('checked')) {
            // Get the account number from the current row
            var accountNumber = $(this).find('td:eq(0)').text();
            // Add the account number to the array
            selectedAccountNumbers.push(accountNumber);
        }
    });
    // Join the array elements into a comma-separated string
    return selectedAccountNumbers.join(', ');
}



function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function GetObject(KEY, divToLoadData, partialView, path) {
    if (KEY.trim() === "") {
        alert("KEY is empty. Please provide a valid KEY.");
        return; // Exit the function
    }
    AddORUpdateGen(KEY, divToLoadData, partialView, path, "CashDesk");


}
function SearchByCustomerNumber(partialView, divToloadPV) {

    AddORUpdateGen($('#manualSearchInput').val(), divToloadPV, partialView, 'search', "CashDesk");
    //calculateBalance();
}
function GetMember() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', operation);
    /*GetMemberData(memberId, '_MomocashCollectionDesk', 'datalistingview', 'cashin');*/
}

function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);
    var spanElement = document.getElementById('cashDeskOperations');

    // Default style
    spanElement.style.fontWeight = "bold";
    spanElement.style.textDecoration = "underline";
    spanElement.style.textDecorationThickness = "2px";

    if (path == "cashin_momokash_collection") {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE-CASH-IN OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "repayment_momokash_collection") {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE-CASH LOAN REPAYMENT OPERATIONS";
        spanElement.style.color = "green";
    }
    else if (path == "search") {
        spanElement.innerText = "MEMBER'S INFORMATION";
        spanElement.style.color = "blue";
    }
    else {
        spanElement.innerText = "MOMO CASH COLLECTION >> NONE CASH OPERATIONS";
        spanElement.style.color = "blue"; // Default color for other paths
    }
    
    AddORUpdateGen(Key, divToloadPV, partialView, path, "CashDesk");
    //calculateBalance();
}

function GetLoan(KEY) {
    $.ajax({
        type: "GET",
        url: '/Operation/GetLoan?KEY=' + KEY,
        success: function (data) {
            var balance = parseFloat(data.Balance).toFixed(1); // Format Balance with 1 decimal place
            var paid = parseFloat(data.Paid).toFixed(1); // Format Paid with 1 decimal place

            // Format numbers with commas as thousands separators
            balance = parseFloat(balance).toLocaleString('en-US');
            paid = parseFloat(paid).toLocaleString('en-US');

            // Assuming #balance and #paid are HTML input elements
            $('#balance').val(balance);
            $('#paid').val(paid);
            $('#loanid').val(data.Id);
            //$('#balance').val(data.Balance);
            //$('#paid').val(data.Paid);
        }, error: function (err) {
            appalert(err.statusText, 3, 0);
        }
    });
}







function AjaxPostAndUpdate(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {


        alertify.confirm("WARNING!!!", "Are you sure you want to perform this action! ",
            function () {


                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {

                        if (response.success) {
                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            }
                            else {
                                appalert(response.message, 1, 1);

                            }
                            if (response.option === 'Update' && response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                            else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                                EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                            }
                            else if (response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                        }
                        else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else {
                                appalert(response.message, 2, 1);
                            }

                        }

                    }
                    , error: function (err) {
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig["contentType"] = false;
                    ajaxConfig["processData"] = false;
                }
                console.log(ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );
    }
    return false;


}
