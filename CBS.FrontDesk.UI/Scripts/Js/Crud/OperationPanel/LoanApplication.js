/* =========================================================
 *  LOAN APPLICATION UI SCRIPT (SAFE PATCHED VERSION)
 *  - Fixes infinite loop on load_loan_products
 *  - Fixes Targets loading by term + purpose
 *  - Keeps your structure & functions (no breaking changes)
 *  - Makes Select2 rebind safe inside modal and normal pages
 * ========================================================= */
function LoadRefinancing(KEY, path, affectedID) {
    var loandiv = document.getElementById('loandiv');
    var showLoanDiv = (KEY === "Refinancing" || KEY === "Reschedule" || KEY === "Restructure");
    var dataPath = KEY;

    if (showLoanDiv) {
        KEY = document.getElementById('customerid').value;
        loandiv.style.display = "block";

        // Dynamically update the header with dataPath
        dataPath = "Select Loan To " + dataPath;
        $('#repaymentHeader').html(dataPath);
        enableSelect2
        var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
        FillDropDownAjaxCallParam(url, affectedID, dataPath);
    } else {
        loandiv.style.display = "none";
    }


    // Call function to toggle input fields and divs based on application type
    toggleInputFields();
}

"use strict";

// =========================
// UI Loading Guards (SAFE ADD)
// =========================
window.__tscUiLoading = window.__tscUiLoading || 0;

function beginUiLoad() { window.__tscUiLoading++; }
function endUiLoad() { window.__tscUiLoading = Math.max(0, (window.__tscUiLoading || 0) - 1); }
function isUiLoading() { return (window.__tscUiLoading || 0) > 0; }

// Prevent rapid repeated calls to load_loan_products (SAFE ADD)
window.__tscLastLoanProductsUrl = window.__tscLastLoanProductsUrl || "";
window.__tscLastLoanProductsAt = window.__tscLastLoanProductsAt || 0;
function shouldSkipLoanProducts(url) {
    var now = Date.now();
    if (url && url === window.__tscLastLoanProductsUrl && (now - window.__tscLastLoanProductsAt) < 800) {
        return true;
    }
    window.__tscLastLoanProductsUrl = url || "";
    window.__tscLastLoanProductsAt = now;
    return false;
}


function getRequestedAmount() {
    // 1) preferred id
    var $a = $("#RequestedAmount");
    if ($a.length) return parseMoney($a.val());

    // 2) fallback by name (MVC generated)
    $a = $('[name="AddLoanApplicationCommand.Amount"]');
    if ($a.length) return parseMoney($a.val());

    return 0;
}

// bind (works even if element is dynamically rendered)
$(document).on("input change keyup", "#RequestedAmount, [name='AddLoanApplicationCommand.Amount']", function () {
    updateRefinancingComputation();
});


// =========================
// Document Ready Blocks (kept)
// =========================
$(document).ready(function () {
    // Run once on page load
    toggleInputFields();

    // When Application Type changes
    $("#LoanApplicationType").on("change", function () {
        toggleInputFields();
    });

    // NOTE: Your radio change remains commented
});

$(document).ready(function () {
    $('#loan_identification, #loan_configuration, #loan_financials, #loan_dates')
        .addClass('show')
        .prev('.accordion-header')
        .find('.accordion-button')
        .removeClass('collapsed');

    // Trigger the download on button click
    $("#btnData").click(function () {
        DownloadLoans('All');
    });

    // Initial check and setting up event listener for checkbox state changes
    toggleLoanOverride();

    $('#IsOverRightOldLoanInterestAndBalance').change(function () {
        toggleLoanOverride();
    });

    function toggleLoanOverride() {
        if ($('#IsOverRightOldLoanInterestAndBalance').is(':checked')) {
            $('#loanoveride').show();
        } else {
            $('#loanoveride').hide();
            $('#NewBalance').val(0);
            $('#NewInterest').val(0);
            $('#NewVAT').val(0);
            $('#NewPenalty').val(0);
        }
    }

    $('#IsOverRightOldLoanInterestAndBalance').trigger('change');
});

// =========================
// Select helpers (kept + fixed)
// =========================
window.resetSelect = function resetSelect(id, placeholder) {
    var $s = $("#" + id);
    if (!$s.length) return;

    // mark as UI loading to avoid chained change => product loop
    beginUiLoad();

    $s.prop("disabled", false).removeAttr("disabled");
    $s.empty().append('<option value="">' + (placeholder || "--- Select ---") + '</option>');
    $s.val("");

    // IMPORTANT: trigger after options set, but we are guarded
    $s.trigger("change.select2").trigger("change");

    rebindSelect2(id);

    // end guard async-safe
    setTimeout(endUiLoad, 0);
};

window.rebindSelect2 = function rebindSelect2(id) {
    if (!window.$ || !$.fn.select2) return;

    var $el = $("#" + id);
    if (!$el.length) return;

    // detect modal parent if exists
    var $modal = $("#loanProductModal");
    var hasModal = $modal.length > 0;

    // keep UI stable while rebinding
    beginUiLoad();

    if ($el.data("select2")) {
        try { $el.select2("destroy"); } catch (e) { }
    }

    var opt = { width: "100%" };
    if (hasModal) opt.dropdownParent = $modal;

    $el.select2(opt);
    $el.trigger("change.select2");

    setTimeout(endUiLoad, 0);
};

window.enableSelect2 = function enableSelect2(id) {
    var $el = $("#" + id);
    if (!$el.length) return;

    beginUiLoad();

    $el.prop("disabled", false).removeAttr("disabled");

    if ($el.hasClass("select2-hidden-accessible")) {
        try { $el.select2("destroy"); } catch (e) { }
    }

    $el.select2({ width: "100%" });

    setTimeout(endUiLoad, 0);
};

window.rebuildSelect2 = function rebuildSelect2(id, placeholder) {
    var $el = $("#" + id);
    if (!$el.length) return;

    beginUiLoad();

    $el.prop("disabled", false).removeAttr("disabled");

    if ($el.hasClass("select2-hidden-accessible")) {
        try { $el.select2("destroy"); } catch (e) { }
    }

    // prefer modal dropdown parent if inside modal
    var $modal = $("#loanProductModal");
    var opt = {
        width: "100%",
        placeholder: placeholder || "--- Select ---",
        allowClear: true
    };
    if ($modal.length) opt.dropdownParent = $modal;

    $el.select2(opt);

    setTimeout(endUiLoad, 0);
};

window.setSelectLoading = function setSelectLoading(id, loadingText) {
    var $s = $("#" + id);
    if (!$s.length) return;

    beginUiLoad();

    $s.prop("disabled", true);
    $s.empty().append('<option value="">' + (loadingText || "Loading...") + '</option>');
    $s.val("");
    $s.trigger("change.select2").trigger("change");

    rebindSelect2(id);

    setTimeout(endUiLoad, 0);
};
function loadRefinancingLoan(loanId) {
    $.get(`/MemberOperation/GetLoanForRefinancing?Key=${loanId}`, function (response) {
        if (response) {
            populateLoanModal(response);
            $('#loanDetailsCard').collapse('show');


        } else {
            appalert("No loan data found.", 2, 1);
        }
    }).fail(function (xhr) {
        appalert("Failed to load loan data: " + xhr.statusText, 0, 1);
    });
    // ✅ keep computation section in sync
    updateRefinancingComputation();
}
// =========================
// Helpers
// =========================
window.gp = function gp(o, camel, pascal) {
    if (!o) return null;
    var v = o[camel];
    if (v !== undefined && v !== null) return v;
    v = o[pascal];
    if (v !== undefined && v !== null) return v;
    return null;
};

function esc(s) {
    return (s == null ? "" : ("" + s))
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

// =========================
// Existing functions (kept)
// =========================
window.LoanProductsProperties = function LoanProductsProperties(KEY, path, affectedID) {
    GetLoanApplication(KEY);
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, 'RepaymentCircle', "---Select Option---");
};

// ✅ FIXED: correct path name used in your controller ("load_loan_term_by_pcmfpurpose")
window.GetTermByPcmfPurpose = function GetTermByPcmfPurpose(key, affectedID) {
    affectedID = affectedID || "LoanTermId";
    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(key || "") +
        "&path=load_loan_term_by_pcmfpurpose";

    beginUiLoad();
    FillDropDownAjaxCallParam(url, affectedID, "---Select pcmf loan term---");
    setTimeout(function () {
        rebindSelect2(affectedID);
        endUiLoad();
    }, 0);
};
// =========================
// ✅ Refinancing cache (SAFE ADD)
// =========================
window.__refinancingOldLoan = window.__refinancingOldLoan || {
    LoanId: null,
    Capital: 0,
    Interest: 0,
    Vat: 0,
    Penalty: 0
};
function parseMoney(v) {
    if (v === null || v === undefined) return 0;
    var s = (v + "").replace(/,/g, "").trim();
    var n = Number(s || 0);
    return isNaN(n) ? 0 : n;
}

function fmtXaf(n) {
    n = parseMoney(n);
    return n.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 }) + "";
}

function getRequestedAmount() {
    var $a = $("#RequestedAmount");
    if ($a.length) return parseMoney($a.val());

    $a = $('[name="AddLoanApplicationCommand.Amount"]');
    if ($a.length) return parseMoney($a.val());

    return 0;
}

// ✅ optional: if you have a fee/charges field later, plug it here
function getChargesAmount() {
    // Example hook (replace when you have real fee computation):
    // return parseMoney($('#TotalFee').val());
    return 0;
}

function updateRefinancingComputation() {
    var loanApplicationType = ($("#LoanApplicationType").val() || "");
    var isRefinancing = loanApplicationType === "Refinancing";

    var requested = getRequestedAmount();
    var shouldShow = isRefinancing && requested > 0;

    $("#refinancingComputationCard").toggle(shouldShow);
    if (!shouldShow) return;

    var old = window.__refinancingOldLoan || {};
    var oldCapital = parseMoney(old.Capital);
    var oldInterest = parseMoney(old.Interest);
    var oldVat = parseMoney(old.Vat);
    var oldPenalty = parseMoney(old.Penalty);

    // Settlement is what will be deducted to close the old loan
    var oldSettlementTotal = oldCapital + oldInterest + oldVat + oldPenalty;

    // New loan amount (recapitalization + top-up)
    var newLoanAmount = requested + oldCapital;

    // Charges (if any)
    var charges = getChargesAmount();

    // Net to member after settlement + charges
    var netToMember = newLoanAmount - (oldSettlementTotal + charges);

    // --------- fill UI ----------
    $("#rc_oldCapital").text(fmtXaf(oldCapital));
    $("#rc_oldInterest").text(fmtXaf(oldInterest));
    $("#rc_oldVat").text(fmtXaf(oldVat));
    $("#rc_oldPenalty").text(fmtXaf(oldPenalty));
    $("#rc_oldTotalSettlement").text(fmtXaf(oldSettlementTotal));

    $("#rc_requested").text(fmtXaf(requested));
    $("#rc_oldCapital2").text(fmtXaf(oldCapital));
    $("#rc_newLoanAmount").text(fmtXaf(newLoanAmount));
    $("#rc_newLoanAmount2").text(fmtXaf(newLoanAmount));

    $("#rc_payCapital").text(fmtXaf(oldCapital));
    $("#rc_payInterest").text(fmtXaf(oldInterest));
    $("#rc_payVat").text(fmtXaf(oldVat));
    $("#rc_payPenalty").text(fmtXaf(oldPenalty));

    $("#rc_charges").text(fmtXaf(charges));
    $("#rc_netToMember").text(fmtXaf(netToMember));

    $("#rc_netWarn").toggle(netToMember <= 0);
}

// ✅ bind to amount input changes
$(document).on("input change keyup", "#RequestedAmount, [name='AddLoanApplicationCommand.Amount']", function () {
    updateRefinancingComputation();
});

// ✅ also refresh when application type changes
$(document).on("change", "#LoanApplicationType", function () {
    updateRefinancingComputation();
});

function populateLoanModal(data) {
    // 🧩 Identification
    $('#rl_productId').text(data.LoanProductId || '');
    $('#rl_productName').text(data.LoanProductName || '');
    $('#rl_loanType').text(data.LoanType || '');
    $('#rl_loanStatus').text(data.LoanStatus || '');
    $('#rl_loanCategory').text(data.LoanCategory || '');

    // ⚙️ Configuration
    $('#rl_loanTerm').text(data.LoanTermName || '');
    $('#rl_targetPopulation').text(data.LoanTarget || '');
    $('#rl_loanPurpose').text(data.LoanPurpose || '');
    $('#rl_repaymentPeriod').text(data.RepaymentCycle || '');
    $('#rl_repaymentMode').text(data.RepaymentMode || '');
    $('#rl_installments').text(data.NumberOfInstallments || '');
    $('#rl_duration').text(data.LoanDurarion || '');
    $('#rl_interestCalculationMethod').text(data.InterestCalculationMethod || '');

    // 💰 Financials
    $('#rl_interestRate').text((data.InterestRate || 0).toFixed(2) + '%');
    $('#rl_vatRate').text((data.VatRate || 0).toFixed(2) + '%');
    $('#rl_principal').text(formatXAF(data.Principal));
    $('#rl_interest').text(formatXAF(data.AccrualInterest));
    $('#rl_vat').text(formatXAF(data.Tax));
    $('#rl_penalty').text(formatXAF(data.Penalty));
    $('#rl_dueAmount').text(formatXAF(data.DueAmount));

    // 📅 Dates
    $('#rl_disbursementDate').text(formatDate(data.DisbursementDate));
    $('#rl_maturityDate').text(formatDate(data.MaturityDate));
    $('#rl_lastRepaymentDate').text(formatDate(data.LastRepaymentDate));
    $('#rl_disbursementChannel').text(data.DisbursementChannel || '');

    $('#rl_productCategoryId').text(data.ProductCategoryId || '');
    $('#rl_productCategoryName').text(data.ProductCategoryName || '');

    $('#oldLoanAmount').val(data.DueAmount);
    $('#oldLoanCapital').val(data.Principal);
    $('#oldLoanInterest').val(data.AccrualInterest);
    $('#oldLoanVAT').val(data.Tax);
    $('#oldLoanPenalty').val(data.Penalty);
    $('#oldLoanLoanId').val(data.Id);
    $('#oldLoanvatRate').val(data.VatRate);
    // ✅ Cache values for later computation (do NOT rely on disabled inputs)
    window.__refinancingOldLoan = {
        LoanId: data.Id || null,
        Capital: Number(data.Principal || 0),
        Interest: Number(data.AccrualInterest || 0),
        Vat: Number(data.Tax || 0),
        Penalty: Number(data.Penalty || 0)
    };

    // ✅ refresh computation UI immediately (if amount already typed)
    updateRefinancingComputation();

    // 🎨 Dynamic Styling Based on Loan Status
    const wrapper = $('#loanDetailsCardWrapper');
    const header = $('#loanDetailsHeader');
    const label = $('#loanStatusLabel');

    wrapper.removeClass('border-success border-danger border-warning');
    header.removeClass('bg-success-subtle bg-danger-subtle bg-warning-subtle text-success text-danger text-warning');

    if (!data.LoanStatus) return;

    const status = data.LoanStatus.toLowerCase();
    if (status.includes('delinquent') || status.includes('default')) {
        wrapper.addClass('border-danger');
        header.addClass('bg-danger-subtle text-danger');
        label.text("🚨 Delinquent Loan Summary");
    } else if (status.includes('pending')) {
        wrapper.addClass('border-warning');
        header.addClass('bg-warning-subtle text-warning');
        label.text("⚠️ Pending Loan Summary");
    } else {
        wrapper.addClass('border-success');
        header.addClass('bg-success-subtle text-success');
        label.text("✅ Active Loan Summary");
    }
}
window.GetLoanPurposes = function GetLoanPurposes() {
    var loanCategoryId = $("#LoanCategoryId").val();

    if (!loanCategoryId) {
        resetSelect("LoanTermId", "---Select pcmf loan term---");
        resetSelect("purposeId", "---Select Option---");
        resetSelect("TargetId", "--- Select Target ---");
        return;
    }

    // 1) Load terms
    GetTermByPcmfPurpose(loanCategoryId, "LoanTermId");

    // 2) Load purposes
    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(loanCategoryId || "") +
        "&path=get_puposes";

    beginUiLoad();
    FillDropDownAjaxCallParam(url, "purposeId", "---Select Option---");
    setTimeout(function () {
        rebindSelect2("purposeId");
        endUiLoad();
    }, 0);
};

window.LoanProductsPropertiesRefinancing = function LoanProductsPropertiesRefinancing(KEY, path, affectedID) {
    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
};

// =========================
// ✅ STOP LOOP: Guard + Dedupe load_loan_products
// =========================
window.GetProductByTarget = function GetProductByTarget(KEY, affectedID) {
    if (isUiLoading()) return;
    if (!KEY) return;

    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    var path = "load_loan_products";
    var loantermid = $("#LoanTermId").val();
    var loanCategoryid = $("#LoanCategoryId").val();

    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
        "&path=" + path +
        "&loanTermId=" + loantermid +
        "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;

    if (shouldSkipLoanProducts(url)) return;

    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
};

window.GetProductByTargetLoandingMainLoan = function GetProductByTargetLoandingMainLoan() {
    if (isUiLoading()) return;

    var affectedID = "loan_productid";
    var KEY = $("#TargetId").val();
    if (!KEY) return;

    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    var path = "load_loan_products";
    var loantermid = $("#LoanTermId").val();
    var loanCategoryid = $("#LoanCategoryId").val();

    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
        "&path=" + path +
        "&loanTermId=" + loantermid +
        "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;

    if (shouldSkipLoanProducts(url)) return;

    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
};

// ✅ FIXED: local var path + correct Key usage
window.GetConfiuredTargets = function GetConfiuredTargets(KEY, affectedID) {
    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
    var loanCategoryid = $("#LoanCategoryId").val();
    var path = "get_configurated_target"; // ✅ was global before

    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
        "&path=" + path +
        "&loanCategoryid=" + loanCategoryid +
        "&loanCategoryValue=" + loanCategoryValue;

    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
};

// =========================
// ✅ FIXED: Load targets by TermId + PurposeId with Hint display
// =========================
window.LoadTargetsByTermAndPurpose = function LoadTargetsByTermAndPurpose() {
    var termId = $("#LoanTermId").val();

    // ✅ FIX: purpose must come from purpose dropdown
    var purposeId = $("#LoanCategoryId").val() || $("#LoanCategoryId").val();

    resetSelect("TargetId", "--- Select Target ---");

    if (!purposeId || !termId) return;

    setSelectLoading("TargetId", "Loading targets...");

    var url = "/MemberOperation/Ajaxloader"
        + "?Key=" + encodeURIComponent(purposeId)
        + "&path=get_configurated_target"
        + "&loanTermId=" + encodeURIComponent(termId);

    beginUiLoad();

    $.get(url)
        .done(function (list) {
            var $target = $("#TargetId");
            $target.prop("disabled", false).removeAttr("disabled");

            $target.empty().append('<option value="">--- Select Target ---</option>');

            (list || []).forEach(function (x) {
                var id = gp(x, "id", "Id");
                var hint = gp(x, "hint", "Hint");
                var text = gp(x, "text", "Text");
                var label = (hint || text || "").trim();

                $target.append('<option value="' + esc(id || "") + '">' + esc(label) + '</option>');
            });

            // ✅ IMPORTANT: Re-init select2 in the same way you do everywhere else (modal safe)
            rebindSelect2("TargetId");

            // Clear selection silently
            $target.val("").trigger("change.select2").trigger("change");
        })
        .fail(function () {
            resetSelect("TargetId", "--- Select Target ---");
        })
        .always(function () {
            endUiLoad();
        });
};
function formatXAF(amount) {
    return (amount || 0).toLocaleString('en-US', {
        style: 'currency',
        currency: 'XAF',
        minimumFractionDigits: 0
    });
}
function formatDate(dateValue) {
    if (!dateValue) return '';

    const date = new Date(dateValue);
    if (isNaN(date)) return '';

    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    });
}
// =========================
// Keep your existing functions below (UNCHANGED)
// =========================
window.handleLoanSelection = window.handleLoanSelection || function handleLoanSelection(loanId) {
    if (!loanId || loanId.trim() === "") {
        $('#viewLoanDetailsBtn').prop('disabled', true);
        $('#loanDetailsCard').collapse('hide');
        return;
    }
    $('#viewLoanDetailsBtn').prop('disabled', false);
};

window.toggleLoanDetailsCard = window.toggleLoanDetailsCard || function toggleLoanDetailsCard() {
    $('#loanDetailsCard').collapse('toggle');
};
// ✅ PCMF version of GetLoanApplication (works with your new /MemberOperation/GetLoanProduct controller)
// Expects PCMFLoanProduct returned by PcmfGetLoanProduct(id)
function GetLoanApplication(KEY) {

    // =========================
    // Helpers (shared)
    // =========================
    function num(v) {
        if (v === null || v === undefined) return 0;
        var s = (v + "").replace(/,/g, "").trim();
        var n = Number(s || 0);
        return isNaN(n) ? 0 : n;
    }

    function fcfa(v) {
        var n = num(v);
        return n.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + " FCFA";
    }

    function normalizeRange(min, max) {
        min = num(min);
        max = num(max);
        if (max <= 0 && min > 0) max = min;
        if (max < min) { var t = max; max = min; min = t; }
        return { min: min, max: max };
    }

    function pcmfTagHtml(p) {
        var section = (p.PcmfSection || p.pcmfSection || "").toString();
        var group = (p.PcmfGroupCode ?? p.pcmfGroupCode);
        var baseCode = (p.PcmfBaseCode ?? p.pcmfBaseCode);
        var pop = (p.PcmfPopulation || p.pcmfPopulation || "").toString();

        var parts = [];
        if (baseCode != null) parts.push("Base: " + baseCode);
        if (section || group != null) parts.push((section || "N/A") + "-" + (group != null ? group : "N/A"));
        if (pop) parts.push("Pop: " + pop);

        return parts.length
            ? "<span class='badge bg-success'>PCMF: " + parts.join(" | ") + "</span>"
            : "";
    }

    function $byName(name) { return $('[name="' + name + '"]'); }
    function getVal(name) { var $el = $byName(name); return $el.length ? num($el.val()) : 0; }
    function setVal(name, value) { var $el = $byName(name); if ($el.length) $el.val(value); }
    function isChecked(name) { var $el = $byName(name); return $el.length ? ($el.prop("checked") === true) : false; }
    function setChecked(name, v) { var $el = $byName(name); if ($el.length) $el.prop("checked", v === true); }

    // Feedback line under input (no green borders; only tick icon)
    function markLimit($input, ok, msg) {
        if (!$input || !$input.length) return;

        $input.toggleClass("is-invalid", !ok);

        var id = $input.attr("id") || ($input.attr("name") || "").replace(/[\[\]\.]/g, "_");
        var fbId = "fb_" + id;

        var $fb = $("#" + fbId);
        if (!$fb.length) {
            $fb = $('<div class="tsc-limit-feedback"></div>').attr("id", fbId);
            $input.closest(".form-floating").append($fb);
        }

        if (!msg) { $fb.text(""); return; }

        if (ok) {
            $fb.removeClass("tsc-bad")
                .html('<span class="tsc-tick">✔</span> ' + msg);
        } else {
            $fb.addClass("tsc-bad")
                .html('<span class="tsc-tick">✖</span> ' + msg);
        }
    }

    function validateRange(v, min, max, label, fmt) {
        v = num(v);
        min = num(min);
        max = num(max);

        if (min === 0 && max === 0) return { ok: true, msg: "" };
        if (max <= 0 && min > 0) max = min;
        if (max < min) { var t = max; max = min; min = t; }

        var ok = (v >= min && v <= max);
        var showV = fmt ? fmt(v) : v;
        var showMin = fmt ? fmt(min) : min;
        var showMax = fmt ? fmt(max) : max;

        return {
            ok: ok,
            msg: ok
                ? (label + " OK (" + showV + ")")
                : (label + " out of range. Allowed: " + showMin + " - " + showMax)
        };
    }

    // =========================
    // Reset on loan product change
    // =========================
    function resetControls() {
        $(".tsc-limit-feedback").remove();
        $(".is-invalid").removeClass("is-invalid");

        // Safe if box doesn't exist
        $("#pcmfCoverageBox").hide();
        $("#pcmfTagInline").html("");

        // Reset main editable fields
        setVal('AddLoanApplicationCommand.Amount', "");
        setVal('AddLoanApplicationCommand.LoanDuration', "");
        setVal('AddLoanApplicationCommand.InterestRate', "");

        // Reset computed/locked fields (will be refilled)
        setVal('AddLoanApplicationCommand.ShareAccountCoverageAmount', "");
        setVal('AddLoanApplicationCommand.SavingAccountCoverageRate', "");

        // Reset option toggles
        setChecked('AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount', false);
        setChecked('AddLoanApplicationCommand.IsDepositAccountCoverageAmount', false);
        setChecked('AddLoanApplicationCommand.IsSalryAccount', false);

        setVal('AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount', "");
        setVal('AddLoanApplicationCommand.DepositAccountCoverageAmount', "");
        setVal('AddLoanApplicationCommand.SalaryAccountCoverageRate', "");

        // Hide optional amount inputs
        $byName('AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount').closest(".form-floating").hide();
        $byName('AddLoanApplicationCommand.DepositAccountCoverageAmount').closest(".form-floating").hide();
        $byName('AddLoanApplicationCommand.SalaryAccountCoverageRate').closest(".form-floating").hide();

        // Remove contribution lines if any
        $('[id^="dp_"]').remove();
    }

    if (!KEY) return;

    $.ajax({
        type: "GET",
        url: '/MemberOperation/GetLoanProduct?Key=' + encodeURIComponent(KEY),
        success: function (data) {
            if (!data) return;

            resetControls();

            var policy = data.Policy || data.policy || {};
            var term = data.Term || data.term || {};

            // =========================
            // Limits from policy/term
            // =========================
            var loanAmtR = normalizeRange(policy.LoanMinimumAmount, policy.LoanMaximumAmount);

            var termMin = num(term.MinInMonth || term.minInMonth);
            var termMax = num(term.MaxInMonth || term.maxInMonth);
            var termR = normalizeRange(termMin, termMax);

            var shareR = normalizeRange(
                policy.MinimumShareAccountBalanceForTheRequestAmount,
                policy.MaximumShareAccountBalanceForTheRequestAmount
            );

            var savMinRate = num(policy.MinimumSavingAccountBalanceRateForTheRequestAmount);
            var savMaxRate = num(policy.MaximumSavingAccountBalanceRateForTheRequestAmount);
            if (savMaxRate <= 0 && savMinRate > 0) savMaxRate = savMinRate;
            if (savMaxRate < savMinRate) { var x = savMaxRate; savMaxRate = savMinRate; savMinRate = x; }
            var savRateR = { min: savMinRate, max: savMaxRate };

            var intR = normalizeRange(policy.MinimumInterestRate, policy.MaximumInterestRate);

            var dpRate = num(policy.MinimumDownPaymentPercentage);

            // =========================
            // 1) Labels + PCMF
            // =========================
            $("#amount").html("Enter amount from: " + fcfa(loanAmtR.min) + " to " + fcfa(loanAmtR.max) + " " + pcmfTagHtml(data));

            if (termR.min || termR.max) {
                $("#loanduration").html("Loan duration is between: " + termR.min + " to " + termR.max + " Month(s)");
            }

            if (intR.min || intR.max) {
                $("#interest").html("Enter interest between: " + intR.min + "% and " + intR.max + "%.");
            }

            $("#share").html("Enter required share amount between: " + fcfa(shareR.min) + " and " + fcfa(shareR.max));
            $("#saving").html("Enter balance saving rate between: " + savRateR.min + "% and " + savRateR.max + "%");

            $("#downpaymentrate").html("Does this application require down payment? Minimum rate is [" + dpRate + "%].");
            $("#downPaymentCheckbox").prop("checked", dpRate > 0).prop("disabled", true);

            // =========================
            // 2) Auto-fill + lock share/savings
            // =========================
            var shareField = 'AddLoanApplicationCommand.ShareAccountCoverageAmount';
            var savingRateField = 'AddLoanApplicationCommand.SavingAccountCoverageRate';

            if (shareR.min > 0) setVal(shareField, shareR.min);
            if (savRateR.min > 0) setVal(savingRateField, savRateR.min);

            $byName(shareField).prop("readonly", true);
            $byName(savingRateField).prop("readonly", true);

            // =========================
            // 3) Computations (define FIRST, before usage)
            // =========================
            function principal() { return getVal('AddLoanApplicationCommand.Amount'); }
            function savingRate() { return getVal(savingRateField); }

            function requiredSavingsAmount() {
                var p = principal();
                var r = savingRate();
                if (p <= 0 || r <= 0) return 0;
                return (p * r) / 100.0;
            }

            function requiredDownPaymentAmount() {
                var p = principal();
                if (p <= 0 || dpRate <= 0) return 0;
                return (p * dpRate) / 100.0;
            }

            function coverageTotalExcludingShares() {
                var total = 0;

                total += requiredSavingsAmount();

                if (isChecked('AddLoanApplicationCommand.IsDepositAccountCoverageAmount'))
                    total += getVal('AddLoanApplicationCommand.DepositAccountCoverageAmount');

                if (isChecked('AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount'))
                    total += getVal('AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount');

                // ✅ salary excluded
                return total;
            }

            function ensureUnderInputLine($input, key) {
                if (!$input || !$input.length) return null;

                var id = $input.attr("id") || ($input.attr("name") || "").replace(/[\[\]\.]/g, "_");
                var lineId = "dp_" + key + "_" + id;

                var $line = $("#" + lineId);
                if (!$line.length) {
                    $line = $('<div class="small text-muted mt-1"></div>').attr("id", lineId);
                    $input.closest(".form-floating").append($line);
                }
                return $line;
            }

            function renderContributionLines() {
                var total = coverageTotalExcludingShares();

                // savings contribution
                var reqSav = requiredSavingsAmount();
                var $sav = $byName(savingRateField);
                var $savLine = ensureUnderInputLine($sav, "sav");
                if ($savLine) {
                    $savLine.html("Added to Down Payment (Savings): <strong>" + fcfa(reqSav) + "</strong> &nbsp; | &nbsp; Total Coverage: <strong>" + fcfa(total) + "</strong>");
                }

                // deposit contribution
                var depToggle = 'AddLoanApplicationCommand.IsDepositAccountCoverageAmount';
                var depAmount = 'AddLoanApplicationCommand.DepositAccountCoverageAmount';
                var depVal = isChecked(depToggle) ? getVal(depAmount) : 0;

                var $dep = $byName(depAmount);
                var $depLine = ensureUnderInputLine($dep, "dep");
                if ($depLine) {
                    $depLine.html(isChecked(depToggle)
                        ? ("Added to Down Payment (Deposit): <strong>" + fcfa(depVal) + "</strong> &nbsp; | &nbsp; Total Coverage: <strong>" + fcfa(total) + "</strong>")
                        : "");
                }

                // preference contribution
                var prefToggle = 'AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount';
                var prefAmount = 'AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount';
                var prefVal = isChecked(prefToggle) ? getVal(prefAmount) : 0;

                var $pref = $byName(prefAmount);
                var $prefLine = ensureUnderInputLine($pref, "pref");
                if ($prefLine) {
                    $prefLine.html(isChecked(prefToggle)
                        ? ("Added to Down Payment (Preference): <strong>" + fcfa(prefVal) + "</strong> &nbsp; | &nbsp; Total Coverage: <strong>" + fcfa(total) + "</strong>")
                        : "");
                }
            }

            function renderCalculator() {
                var p = principal();
                var sr = savingRate();
                var rs = requiredSavingsAmount();
                var rd = requiredDownPaymentAmount();
                var cov = coverageTotalExcludingShares();

                var show = (p > 0) || (dpRate > 0);
                $("#pcmfCoverageBox").toggle(show);

                $("#pcmfTagInline").html(pcmfTagHtml(data));
                $("#pcmfPrincipalText").text(p > 0 ? fcfa(p) : "-");
                $("#pcmfSavingRateText").text(sr > 0 ? (sr + "%") : "-");
                $("#pcmfSavingRequiredText").text(rs > 0 ? fcfa(rs) : fcfa(0));
                $("#pcmfDownRateText").text(dpRate > 0 ? (dpRate + "%") : "0%");
                $("#pcmfDownRequiredText").text(rd > 0 ? fcfa(rd) : fcfa(0));
                $("#pcmfCoverageTotalText").text(cov > 0 ? fcfa(cov) : fcfa(0));

                var ok = cov >= rd;

                $("#pcmfCoverageStatus")
                    .removeClass("text-success text-danger")
                    .addClass(ok ? "text-success" : "text-danger")
                    .html(ok
                        ? ('<span class="tsc-tick">✔</span> Down payment covered (excluding shares): <strong>' + fcfa(cov) + "</strong> / " + fcfa(rd))
                        : ('<span class="tsc-tick">✖</span> Down payment NOT covered (excluding shares): <strong>' + fcfa(cov) + "</strong> / " + fcfa(rd))
                    );
            }

            // =========================
            // 4) Live Limit Validation
            // =========================
            function validateAll() {
                var $amt = $byName('AddLoanApplicationCommand.Amount');
                markLimit($amt, validateRange($amt.val(), loanAmtR.min, loanAmtR.max, "Requested Amount", fcfa).ok,
                    validateRange($amt.val(), loanAmtR.min, loanAmtR.max, "Requested Amount", fcfa).msg);

                var $dur = $byName('AddLoanApplicationCommand.LoanDuration');
                if (termR.min || termR.max) {
                    var vDur = validateRange($dur.val(), termR.min, termR.max, "Loan Duration", function (x) { return num(x) + " Month(s)"; });
                    markLimit($dur, vDur.ok, vDur.msg);
                }

                var $int = $byName('AddLoanApplicationCommand.InterestRate');
                if (intR.min || intR.max) {
                    var vInt = validateRange($int.val(), intR.min, intR.max, "Interest Rate", function (x) { return num(x) + "%"; });
                    markLimit($int, vInt.ok, vInt.msg);
                }

                var $share = $byName(shareField);
                var vShare = validateRange($share.val(), shareR.min, shareR.max, "Share Coverage Amount", fcfa);
                markLimit($share, vShare.ok, vShare.msg);

                var $sav = $byName(savingRateField);
                var vSav = validateRange($sav.val(), savRateR.min, savRateR.max, "Saving Coverage Rate", function (x) { return num(x) + "%"; });
                markLimit($sav, vSav.ok, vSav.msg);
            }

            // =========================
            // 5) Toggle behaviors
            // =========================
            function bindToggle(toggleName, amountName) {
                var $t = $byName(toggleName);
                var $wrap = $byName(amountName).closest(".form-floating");
                if (!$t.length) return;

                $wrap.toggle($t.prop("checked") === true);

                $t.off("change.pcmfToggle").on("change.pcmfToggle", function () {
                    $wrap.toggle(this.checked === true);
                    renderCalculator();
                    renderContributionLines();
                });
            }

            // Disable salary fully (remove from UI + calc)
            (function disableSalaryCoverage() {
                var salToggle = 'AddLoanApplicationCommand.IsSalryAccount';
                var salAmount = 'AddLoanApplicationCommand.SalaryAccountCoverageRate';

                var $t = $byName(salToggle);
                if ($t.length) $t.prop("checked", false).prop("disabled", true);

                var $wrap = $byName(salAmount).closest(".form-floating");
                if ($wrap.length) {
                    $byName(salAmount).val("");
                    $wrap.hide();
                }
            })();

            bindToggle('AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount', 'AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount');
            bindToggle('AddLoanApplicationCommand.IsDepositAccountCoverageAmount', 'AddLoanApplicationCommand.DepositAccountCoverageAmount');
            // ✅ DO NOT bind salary

            // =========================
            // 6) Watch inputs (live)
            // =========================
            var watch = [
                'AddLoanApplicationCommand.Amount',
                'AddLoanApplicationCommand.LoanDuration',
                'AddLoanApplicationCommand.InterestRate',

                'AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount',
                'AddLoanApplicationCommand.DepositAccountCoverageAmount',

                'AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount',
                'AddLoanApplicationCommand.IsDepositAccountCoverageAmount'
            ];

            watch.forEach(function (name) {
                var $el = $byName(name);
                if (!$el.length) return;

                $el.off("input.pcmf change.pcmf")
                    .on("input.pcmf change.pcmf", function () {
                        validateAll();
                        renderCalculator();
                        renderContributionLines();
                    });
            });

            // initial
            validateAll();
            renderCalculator();
            renderContributionLines();

            // =========================
            // 7) Fee before processing behaviour (keep yours)
            // =========================
            var isPaidFeeBeforeProcessing = policy.IsPaidFeeBeforeProcessing === true;
            var $paidFeeCheckbox = $byName("AddLoanApplicationCommand.IsPaidFeeBeforeProcessing");
            var $processingLabel = $('label[for="FeePaidBeforeProcessing"]');
            var loanProductName = data.ProductName || data.ProductCode || "this loan product";

            if (policy.IsPaidFeeBeforeProcessing != null) {
                if (isPaidFeeBeforeProcessing) {
                    $paidFeeCheckbox.prop('checked', true).prop('disabled', true);
                    $processingLabel.html("A partial fee must be paid at the cash desk before the loan (" + loanProductName + ") can be processed.");
                    $('#beforeProcessingDiv').show();
                } else {
                    $paidFeeCheckbox.prop('checked', false).prop('disabled', false);
                    $processingLabel.html("(" + loanProductName + ") is not configured for partial fee payment before processing.");
                }
            }
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}



function toggleInputFields() {
    var loanApplicationType = document.getElementById('LoanApplicationType').value;
    var isReschedule = loanApplicationType === "Reschedule";
    var isRefinancing = loanApplicationType === "Refinancing";

    // Toggle readonly on specific inputs for Reschedule only
    var inputFields = ["NewBalance"];
    inputFields.forEach(function (fieldId) {
        var field = document.getElementById(fieldId);
        if (isReschedule) {
            field.setAttribute('readonly', 'readonly');
        } else {
            field.removeAttribute('readonly');
        }
    });

    // Update panel title
    var panelTitle = document.getElementById('panelTitle');
    switch (loanApplicationType) {
        case "Reschedule":
            panelTitle.innerHTML = "RESCHEDULELING LOAN APPLICATION FORM";
            break;
        case "Refinancing":
            panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM";
            break;
        case "Restructure":
            panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM";
            break;
        default:
            panelTitle.innerHTML = "NEW LOAN APPLICATION FORM";
            break;
    }

    // Update icon
    var iconElement = document.querySelector('#accordionPopoutIconThree i');
    switch (loanApplicationType) {
        case "Reschedule":
            iconElement.className = "mdi mdi-calendar-refresh me-2";
            break;
        case "Refinancing":
            iconElement.className = "mdi mdi-cash-refund me-2";
            break;
        case "Restructure":
            iconElement.className = "mdi mdi-account-cog me-2";
            break;
        default:
            iconElement.className = "mdi mdi-file me-2";
            break;
    }

    // Common divs to toggle (already present)
    var divsToToggle = [
        "RiskMitigationDiv",
        "RAmountDiv",
        "loanTypeDiv",
        "loanProductDiv",
        "TargetPopulationDiv",
        "LoanCategoryDive",
        "RepaymentDiv",
        "purposeAndActivitiesDiv"
    ];

    divsToToggle.forEach(function (divId) {
        var divElement = document.getElementById(divId);
        if (isReschedule) {
            divElement.style.display = "none";
        } else {
            divElement.style.display = "block";
        }
    });

    // ✅ Additional logic for Refinancing: hide product selection-related fields
    var refinancingFields = [
        //"LoanCategorySelect",       // dropdown for Loan Product Category
        //"LoanTermSelect",           // dropdown for Loan Term
        "LoanCategoryDive",     // radio buttons
        //"TargetPopulationDiv",  // dropdown for target
        //"loanProductDiv",       // dropdown for loan product
        "loanTypeDiv"           // dropdown for loan type
    ];

    refinancingFields.forEach(function (id) {
        var element = document.getElementById(id);
        if (element) {
            element.style.display = isRefinancing ? "none" : "block";
        }
    });
    updateRefinancingComputation();

}







//$(document).ready(function () {

//    // Run once on page load
//    toggleInputFields();

//    // When Application Type changes (if not already handled)
//    $("#LoanApplicationType").on("change", function () {
//        toggleInputFields();
//    });

//    //// Bind radio change
//    //$("input[name='AddLoanApplicationCommand.LoanCategory']").on('change', function () {

//    //    var loanTermId = $("#LoanTermId").val();
//    //    if (!loanTermId) return;

//    //    GetConfiuredTargets(loanTermId, 'TargetId');
//    //    GetProductByTargetLoandingMainLoan();
//    //});
//});



//$(document).ready(function () {
//    $('#loan_identification, #loan_configuration, #loan_financials, #loan_dates')
//        .addClass('show')
//        .prev('.accordion-header')
//        .find('.accordion-button')
//        .removeClass('collapsed');

//    // Trigger the download on button click
//    $("#btnData").click(function () {
//        DownloadLoans('All');
//    });

//    // Initial check and setting up event listener for checkbox state changes
//    toggleLoanOverride();

//    $('#IsOverRightOldLoanInterestAndBalance').change(function () {
//        toggleLoanOverride();
//    });

//    function toggleLoanOverride() {
//        if ($('#IsOverRightOldLoanInterestAndBalance').is(':checked')) {
//            $('#loanoveride').show();
//        } else {
//            $('#loanoveride').hide();
//            // Reset the fields if the checkbox is unchecked
//            $('#NewBalance').val(0);
//            $('#NewInterest').val(0);
//            $('#NewVAT').val(0);
//            $('#NewPenalty').val(0);
//        }
//    }

//    // Trigger change event on page load to set the correct initial state
//    $('#IsOverRightOldLoanInterestAndBalance').trigger('change');

//});



//function LoanProductsProperties(KEY, path, affectedID) {
//    GetLoanApplication(KEY);
//    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
//    FillDropDownAjaxCallParam(url, 'RepaymentCircle', "---Select Option---");

//    //GetLoanPurposes()
//}
//function GetTermByPcmfPurpose(key, affectedID) {
//    // defaults
//    affectedID = affectedID || "LoanTermId";

//    var path = "load_loan_terms_by_pcmf_purpose";
//    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(key || "") +
//        "&path=" + encodeURIComponent(path);

//    FillDropDownAjaxCallParam(url, affectedID, "---Select pcmf loan term---");
//}

//function GetLoanPurposes() {
//    var loanCategoryId = $("#LoanCategoryId").val();

//    // reset if empty
//    if (!loanCategoryId) {
//        resetSelect("LoanTermId", "---Select pcmf loan term---");
//        resetSelect("purposeId", "---Select Option---");
//        return;
//    }

//    // 1) Load terms for this category (PCMF purpose key / category)
//    GetTermByPcmfPurpose(loanCategoryId, "LoanTermId");

//    // 2) Load purposes for this category
//    var path = "get_puposes";
//    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(loanCategoryId || "") +
//        "&path=" + encodeURIComponent(path);

//    FillDropDownAjaxCallParam(url, "purposeId", "---Select Option---");
//}

//function LoanProductsPropertiesRefinancing(KEY, path, affectedID) {
//    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
//    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");

//    //GetLoanPurposes()
//}
//function GetProductByTarget(KEY, affectedID) {
//    // Fetch the selected radio button based on the 'id' attribute
//    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
//    console.log(loanCategoryValue); // Logs 'MainLoan' or 'SpecialSavingFacilityLoan'

//    var path = "load_loan_products";

//    // Fetch other necessary parameters
//    var loantermid = $("#LoanTermId").val();
//    var loanCategoryid = $("#LoanCategoryId").val();

//    // Construct the URL with updated isSSF value
//    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
//        "&path=" + path +
//        "&loanTermId=" + loantermid +
//        "&loanCategoryid=" + loanCategoryid +
//        "&loanCategoryValue=" + loanCategoryValue;
//    console.log(url);  // Logs the constructed URL

//    // Make the AJAX call to populate the dropdown
//    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
//}
//function GetProductByTargetLoandingMainLoan() {
//    // Fetch the selected radio button based on the 'id' attribute
//    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
//    console.log(loanCategoryValue); // Logs 'MainLoan' or 'SpecialSavingFacilityLoan'
//    var affectedID = "loan_productid";
//    var KEY = $("#TargetId").val();
//    var path = "load_loan_products";
//    // Fetch other necessary parameters
//    var loantermid = $("#LoanTermId").val();
//    var loanCategoryid = $("#LoanCategoryId").val();
//    // Construct the URL with updated isSSF value
//    var url = "/MemberOperation/Ajaxloader?Key=" + KEY +
//        "&path=" + path +
//        "&loanTermId=" + loantermid +
//        "&loanCategoryid=" + loanCategoryid +
//        "&loanCategoryValue=" + loanCategoryValue;
//    console.log(url);  // Logs the constructed URL

//    // Make the AJAX call to populate the dropdown
//    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
//}
//function GetConfiuredTargets(KEY, affectedID) {
//    var loanCategoryValue = $("input[name='AddLoanApplicationCommand.LoanCategory']:checked").attr('id');
//    console.log(KEY);
//    console.log(affectedID);
//    path = "get_configurated_target";
//    var loanCategoryid = $("#LoanCategoryId").val();
//    console.log(loanCategoryid);
//    var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path + "&loanCategoryid=" + loanCategoryid +
//        "&loanCategoryValue=" + loanCategoryValue;
//    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
//}

//function handleLoanSelection(loanId) {
//    if (!loanId || loanId.trim() === "") {
//        $('#viewLoanDetailsBtn').prop('disabled', true);
//        $('#loanDetailsCard').collapse('hide');
//        return;
//    }

//    $('#viewLoanDetailsBtn').prop('disabled', false);
//}

//function toggleLoanDetailsCard() {
//    $('#loanDetailsCard').collapse('toggle');
//}





//function styleLoanStatus(status) {
//    const $badge = $('#rl_loanStatus');
//    $badge.text(status).removeClass().addClass('badge px-3 py-1');

//    switch ((status || '').toLowerCase()) {
//        case 'approved':
//            $badge.addClass('bg-success');
//            break;
//        case 'pending':
//            $badge.addClass('bg-warning text-dark');
//            break;
//        case 'rejected':
//        case 'delinquent':
//            $badge.addClass('bg-danger');
//            break;
//        case 'open':
//            $badge.addClass('bg-primary text-white'); // 💡 Or use a custom class like 'bg-open-green'
//            break;
//        default:
//            $badge.addClass('bg-secondary');
//    }
//}



//function formatXAF(amount) {
//    return (amount || 0).toLocaleString('en-US', {
//        style: 'currency',
//        currency: 'XAF',
//        minimumFractionDigits: 0
//    });
//}

//function formatDate(dateValue) {
//    if (!dateValue) return '';

//    const date = new Date(dateValue);
//    if (isNaN(date)) return '';

//    return date.toLocaleDateString('en-GB', {
//        day: '2-digit',
//        month: '2-digit',
//        year: 'numeric'
//    });
//}




//// ==========================
//// Select helpers
//// ==========================
//function resetSelect(id, placeholder) {
//    var $s = $("#" + id);
//    $s.prop("disabled", false);
//    $s.empty().append('<option value="">' + placeholder + '</option>');
//    $s.val("");
//    $s.trigger("change.select2").trigger("change");
//    rebindSelect2(id);
//}

//function rebindSelect2(id) {
//    if (!window.$ || !$.fn.select2) return;

//    var $modal = $("#loanProductModal");
//    var $el = $("#" + id);
//    if (!$el.length) return;

//    if ($el.data("select2")) {
//        try { $el.select2("destroy"); } catch (e) { }
//    }

//    $el.select2({ width: "100%", dropdownParent: $modal });
//    $el.trigger("change.select2");
//}

//function enableSelect2(id) {
//    var $el = $("#" + id);

//    // enable
//    $el.prop("disabled", false);

//    // destroy old select2 instance if any
//    if ($el.hasClass("select2-hidden-accessible")) {
//        try { $el.select2("destroy"); } catch (e) { }
//    }

//    // re-init
//    $el.select2({ width: "100%" });
//}

//function GetLoanPurposes(categoryId, purposeSelectId) {
//    purposeSelectId = purposeSelectId || "purposeId";

//    // reset downstream
//    resetSelect(purposeSelectId, "--- Select Purpose ---");
//    resetSelect("LoanTermId", "--- Select Loan Term ---");
//    resetSelect("TargetId", "--- Select Target ---");

//    // re-enable UI after reset
//    enableSelect2(purposeSelectId);
//    enableSelect2("LoanTermId");
//    enableSelect2("TargetId");

//    if (!categoryId) return;

//    // 1) load terms
//    GetTermByPcmfPurpose(categoryId, "LoanTermId");

//    // 2) load purposes
//    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(categoryId) +
//        "&path=get_puposes";

//    FillDropDownAjaxCallParam(url, purposeSelectId, "--- Select Purpose ---");

//    // ✅ rebind select2 so user can select newly loaded items
//    enableSelect2(purposeSelectId);
//}

//function GetTermByPcmfPurpose(key, affectedID) {
//    affectedID = affectedID || "LoanTermId";

//    var url = "/MemberOperation/Ajaxloader?Key=" + encodeURIComponent(key || "") +
//        "&path=load_loan_term_by_pcmfpurpose";

//    FillDropDownAjaxCallParam(url, affectedID, "--- Select Loan Term ---");

//    // ✅ rebind select2 so user can select newly loaded items
//    enableSelect2(affectedID);
//}
//function setSelectLoading(id, loadingText) {
//    var $s = $("#" + id);
//    $s.prop("disabled", true);
//    $s.empty().append('<option value="">' + (loadingText || "Loading...") + '</option>');
//    $s.val("");
//    $s.trigger("change.select2").trigger("change");
//    rebindSelect2(id);
//}
//// ✅ Load targets using BOTH termId + purposeId
//function LoadTargetsByTermAndPurpose() {
//    var termId = $("#LoanTermId").val();

//    // ✅ FIX: get purpose from purpose dropdown
//    var purposeId = $("#LoanCategoryId").val(); // change if your id is "purposeId"

//    resetSelect("TargetId", "--- Select Target ---");

//    if (!purposeId || !termId) return;

//    setSelectLoading("TargetId", "Loading targets...");

//    function esc(s) {
//        return (s == null ? "" : ("" + s))
//            .replace(/&/g, "&amp;")
//            .replace(/</g, "&lt;")
//            .replace(/>/g, "&gt;")
//            .replace(/"/g, "&quot;")
//            .replace(/'/g, "&#39;");
//    }

//    var url = "/MemberOperation/Ajaxloader"
//        + "?Key=" + encodeURIComponent(purposeId)
//        + "&path=get_configurated_target"
//        + "&loanTermId=" + encodeURIComponent(termId);

//    $.get(url)
//        .done(function (list) {
//            var $target = $("#TargetId");

//            // ✅ hard enable before rebuilding
//            $target.prop("disabled", false).removeAttr("disabled");

//            // ✅ reset options
//            $target.empty().append('<option value="">--- Select Target ---</option>');

//            function esc(s) {
//                return (s == null ? "" : ("" + s))
//                    .replace(/&/g, "&amp;")
//                    .replace(/</g, "&lt;")
//                    .replace(/>/g, "&gt;")
//                    .replace(/"/g, "&quot;")
//                    .replace(/'/g, "&#39;");
//            }

//            (list || []).forEach(function (x) {
//                var id = gp(x, "id", "Id");
//                var hint = gp(x, "hint", "Hint");
//                var text = gp(x, "text", "Text");
//                var label = (hint || text || "").trim();

//                $target.append('<option value="' + esc(id || "") + '">' + esc(label) + '</option>');
//            });

//            // ✅ re-init select2 cleanly (this is what makes it clickable again)
//            rebuildSelect2("TargetId", "--- Select Target ---");

//            // optional: clear selection
//            $target.val("").trigger("change");
//        })
//        .fail(function () {
//            resetSelect("TargetId", "--- Select Target ---");
//        });

//}
//function rebuildSelect2(id, placeholder) {
//    var $el = $("#" + id);

//    // enable
//    $el.prop("disabled", false).removeAttr("disabled");

//    // destroy if already initialized
//    if ($el.hasClass("select2-hidden-accessible")) {
//        try { $el.select2("destroy"); } catch (e) { }
//    }

//    // init again
//    $el.select2({
//        width: "100%",
//        placeholder: placeholder || "--- Select ---",
//        allowClear: true
//    });
//}

//// ==========================
//// Helpers
//// ==========================
//function gp(o, camel, pascal) {
//    if (!o) return null;
//    var v = o[camel];
//    if (v !== undefined && v !== null) return v;
//    v = o[pascal];
//    if (v !== undefined && v !== null) return v;
//    return null;
//}
//function LoadRefinancing(KEY, path, affectedID) {
//    var loandiv = document.getElementById('loandiv');
//    var showLoanDiv = (KEY === "Refinancing" || KEY === "Reschedule" || KEY === "Restructure");
//    var dataPath = KEY;

//    if (showLoanDiv) {
//        KEY = document.getElementById('customerid').value;
//        loandiv.style.display = "block";

//        // Dynamically update the header with dataPath
//        dataPath = "Select Loan To " + dataPath;
//        $('#repaymentHeader').html(dataPath);
//        enableSelect2
//        var url = "/MemberOperation/Ajaxloader?Key=" + KEY + "&path=" + path;
//        FillDropDownAjaxCallParam(url, affectedID, dataPath);
//    } else {
//        loandiv.style.display = "none";
//    }


//    // Call function to toggle input fields and divs based on application type
//    toggleInputFields();
//}
//function toggleInputFields() {
//    var typeEl = document.getElementById('LoanApplicationType');
//    var loanApplicationType = typeEl ? typeEl.value : "";

//    var isReschedule = loanApplicationType === "Reschedule";
//    var isRefinancing = loanApplicationType === "Refinancing";
//    var isRestructure = loanApplicationType === "Restructure";

//    // ---------------------------
//    // 1) Toggle readonly fields (Reschedule only)
//    // ---------------------------
//    ["NewBalance"].forEach(function (fieldId) {
//        var field = document.getElementById(fieldId);
//        if (!field) return;

//        if (isReschedule) field.setAttribute('readonly', 'readonly');
//        else field.removeAttribute('readonly');
//    });

//    // ---------------------------
//    // 2) Update panel title
//    // ---------------------------
//    var panelTitle = document.getElementById('panelTitle');
//    if (panelTitle) {
//        switch (loanApplicationType) {
//            case "Reschedule":
//                panelTitle.innerHTML = "RESCHEDULING LOAN APPLICATION FORM";
//                break;
//            case "Refinancing":
//                panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM";
//                break;
//            case "Restructure":
//                panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM";
//                break;
//            default:
//                panelTitle.innerHTML = "NEW LOAN APPLICATION FORM";
//                break;
//        }
//    }

//    // ---------------------------
//    // 3) Update icon
//    // ---------------------------
//    var iconElement = document.querySelector('#accordionPopoutIconThree i');
//    if (iconElement) {
//        switch (loanApplicationType) {
//            case "Reschedule":
//                iconElement.className = "mdi mdi-calendar-refresh me-2";
//                break;
//            case "Refinancing":
//                iconElement.className = "mdi mdi-cash-refund me-2";
//                break;
//            case "Restructure":
//                iconElement.className = "mdi mdi-account-cog me-2";
//                break;
//            default:
//                iconElement.className = "mdi mdi-file me-2";
//                break;
//        }
//    }

//    // ---------------------------
//    // 4) Reschedule hides these UI blocks
//    // ---------------------------
//    var rescheduleHide = [
//        "LoanCategorySelect",
//        "LoanTermSelect",
//        "LoanCategoryDive",
//        "TargetPopulationDiv",
//        "loanProductDiv",
//        "loanTypeDiv",
//        "RAmountDiv",
//        "RiskMitigationDiv",
//        "RepaymentDiv",
//        "purposeAndActivitiesDiv"
//    ];

//    rescheduleHide.forEach(function (id) {
//        var el = document.getElementById(id);
//        if (!el) return;
//        el.style.display = isReschedule ? "none" : "block";
//    });

//    // ---------------------------
//    // 4b) Hide Interest section (Reschedule only)
//    // ---------------------------
//    var interestDiv = document.getElementById("InterestDiv");
//    if (interestDiv) {
//        interestDiv.style.display = isReschedule ? "none" : "block";
//    }

//    // ---------------------------
//    // 5) Refinancing hides some product-selection fields
//    // ---------------------------
//    var refinancingHide = [
//        "LoanCategoryDive",
//        "loanTypeDiv"
//    ];

//    refinancingHide.forEach(function (id) {
//        var el = document.getElementById(id);
//        if (!el) return;
//        if (isReschedule) return;
//        el.style.display = isRefinancing ? "none" : "block";
//    });

//    // ---------------------------
//    // 6) Loan selection block
//    // ---------------------------
//    var loanDiv = document.getElementById("loandiv");
//    if (loanDiv) {
//        var showLoanDiv =
//            loanApplicationType === "Refinancing" ||
//            loanApplicationType === "Reschedule" ||
//            loanApplicationType === "Restructure";

//        loanDiv.style.display = showLoanDiv ? "block" : "none";
//    }

//    // ---------------------------
//    // 7) Update loan selection header text (SAFE ADD)
//    // ---------------------------
//    var repaymentHeader = document.getElementById("repaymentHeader");
//    if (repaymentHeader) {
//        if (isRefinancing) {
//            repaymentHeader.innerText = "SELECT LOAN TO REFINANCE";
//        } else if (isReschedule) {
//            repaymentHeader.innerText = "SELECT LOAN TO RESCHEDULE";
//        } else if (isRestructure) {
//            repaymentHeader.innerText = "SELECT LOAN TO RESTRUCTURE";
//        }
//    }

//    // ---------------------------
//    // 8) Toggle guideline sections (SAFE ADD)
//    // ---------------------------
//    var refinancingGuidelines = document.getElementById("refinancingGuidelines");
//    var reschedulingGuidelines = document.getElementById("reschedulingGuidelines");

//    if (refinancingGuidelines) {
//        refinancingGuidelines.style.display = isRefinancing ? "block" : "none";
//    }

//    if (reschedulingGuidelines) {
//        reschedulingGuidelines.style.display = isReschedule ? "block" : "none";
//    }
//}

////function toggleInputFields() {
////    var loanApplicationType = document.getElementById('LoanApplicationType').value;
////    var isReschedule = loanApplicationType === "Reschedule";
////    var isRefinancing = loanApplicationType === "Refinancing";

////    // Toggle readonly on specific inputs for Reschedule only
////    var inputFields = ["NewBalance"];
////    inputFields.forEach(function (fieldId) {
////        var field = document.getElementById(fieldId);
////        if (isReschedule) {
////            field.setAttribute('readonly', 'readonly');
////        } else {
////            field.removeAttribute('readonly');
////        }
////    });

////    // Update panel title
////    var panelTitle = document.getElementById('panelTitle');
////    switch (loanApplicationType) {
////        case "Reschedule":
////            panelTitle.innerHTML = "RESCHEDULELING LOAN APPLICATION FORM";
////            break;
////        case "Refinancing":
////            panelTitle.innerHTML = "REFINANCING LOAN APPLICATION FORM";
////            break;
////        case "Restructure":
////            panelTitle.innerHTML = "RESTRUCTURING LOAN APPLICATION FORM";
////            break;
////        default:
////            panelTitle.innerHTML = "NEW LOAN APPLICATION FORM";
////            break;
////    }

////    // Update icon
////    var iconElement = document.querySelector('#accordionPopoutIconThree i');
////    switch (loanApplicationType) {
////        case "Reschedule":
////            iconElement.className = "mdi mdi-calendar-refresh me-2";
////            break;
////        case "Refinancing":
////            iconElement.className = "mdi mdi-cash-refund me-2";
////            break;
////        case "Restructure":
////            iconElement.className = "mdi mdi-account-cog me-2";
////            break;
////        default:
////            iconElement.className = "mdi mdi-file me-2";
////            break;
////    }

////    // Common divs to toggle (already present)
////    var divsToToggle = [
////        "RiskMitigationDiv",
////        "RAmountDiv",
////        "loanTypeDiv",
////        "loanProductDiv",
////        "TargetPopulationDiv",
////        "LoanCategoryDive",
////        "RepaymentDiv",
////        "purposeAndActivitiesDiv"
////    ];

////    divsToToggle.forEach(function (divId) {
////        var divElement = document.getElementById(divId);
////        if (isReschedule) {
////            divElement.style.display = "none";
////        } else {
////            divElement.style.display = "block";
////        }
////    });

////    // ✅ Additional logic for Refinancing: hide product selection-related fields
////    var refinancingFields = [
////        //"LoanCategorySelect",       // dropdown for Loan Product Category
////        //"LoanTermSelect",           // dropdown for Loan Term
////        "LoanCategoryDive",     // radio buttons
////        //"TargetPopulationDiv",  // dropdown for target
////        //"loanProductDiv",       // dropdown for loan product
////        "loanTypeDiv"           // dropdown for loan type
////    ];

////    refinancingFields.forEach(function (id) {
////        var element = document.getElementById(id);
////        if (element) {
////            element.style.display = isRefinancing ? "none" : "block";
////        }
////    });
////}



//function LoadProductDetails(KEY) {

//    EditResetMain(KEY, '_LoadProductDetails', '_loanproductdetailDiv', 'MemberOperation', 'InitializeData')

//}

//function GetLoanApplication(KEY) {
//    $.ajax({
//        type: "GET",
//        url: '/MemberOperation/GetLoanProduct?KEY=' + KEY,
//        success: function (data) {
//            // Helper function to format amounts to XAF currency
//            function formatCurrency(amount) {
//                return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'XAF', minimumFractionDigits: 1 }).format(amount);
//            }

//            // Update the HTML elements based on the data received
//            $('#amount').html("Enter amount from: " + formatCurrency(data.LoanMinimumAmount) + " to " + formatCurrency(data.LoanMaximumAmount));
//            $('#loanduration').html("Loan duration is between: " + data.MinimumDurationPeriod + " to " + data.MaximumDurationPeriod + " Months");
//            $('#interest').html("Enter interest between: " + data.MinimumInterestRate + "% and " + data.MaximumInterestRate + "%. Calculated on a daily basis.");
//            $('#installment').html("Minimum repayment installment is: " + data.MinimumNumberOfRepayment + " and Maximum is " + data.MaximumNumberOfRepayment);
//            $('#saving').html("Enter balance saving rate between: " + data.MinimumSavingAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumSavingAccountBalanceRateForTheRequestAmount + "%");
//            $('#share').html("Enter required share amount between: " + formatCurrency(data.MinimumShareAccountBalanceForTheRequestAmount) + " and " + formatCurrency(data.MaximumShareAccountBalanceForTheRequestAmount));
//            $('#salary').html("Enter Salary rate between: " + data.MinimumSalaryAccountBalanceRateForTheRequestAmount + "% and " + data.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount + "%");
//            $('#fee').html("Enter processing fee rate between: " + data.MinimumProcessingFeeRate + "% and " + data.MaximumProcessingFeeRate + "%.");
//            $('#inspectionfee').html("Enter inspection fee between: " + data.MinimumInspectionFeeRate + "% and " + data.MaximumInspectionFeeRate + "%.");
//            $('#chargeparcentages').html("Enter charge percentage between: " + data.MinimumChargesToAppliedInPercentage + " % and " + data.MaximumChargesToAppliedPercentage + "%");
//            $('#chargedayranges').html("Enter in days when charges start between: " + data.MinimumChargesStartDayAfterLoanDueDate + " to " + data.MaximumChargesStartDayAfterLoanDueDate + " days");
//            $('#waiverranges').html("Enter in percentage interest to waive between: " + data.MinimumInterestWaiver + "% and " + data.MaximumInterestWaiver + "%.");
//            $('#downpaymentrate').html("Does this application require down payment? Minimum rate is [" + data.MinimumDownPaymentPercentage + "%].");

//            const requiredDownPayment = data.MinimumDownPaymentPercentage > 0;

//            // Set the checkbox state for Down Payment
//            const $downPaymentCheckbox = $('input[name="AddLoanApplicationCommand.RequiredDownPaymentCoverageRate"]');
//            $downPaymentCheckbox.prop('checked', requiredDownPayment);
//            $downPaymentCheckbox.prop('disabled', true);

//            // Handle IsPaidFeeBeforeProcessing logic
//            const $paidFeeCheckbox = $('input[name="AddLoanApplicationCommand.IsPaidFeeBeforeProcessing"]');
//            const $processingLabel = $('label[for="FeePaidBeforeProcessing"]');
//            const loanProductName = data.ProductName || "this loan product"; // Use the loan product name if available

//            if (data.IsPaidFeeBeforeProcessing) {
//                $paidFeeCheckbox.prop('checked', true); // Check the checkbox
//                $paidFeeCheckbox.prop('disabled', true); // Disable the checkbox
//                $processingLabel.html(`A partial fee must be paid at the cash desk before the loan (${loanProductName}) can be processed.`);
//                $('#beforeProcessingDiv').show(); // Ensure the div is visible
//            } else {
//                $paidFeeCheckbox.prop('checked', false); // Uncheck the checkbox
//                $paidFeeCheckbox.prop('disabled', false); // Enable the checkbox
//                $processingLabel.html(`(${loanProductName}) is not configured for partial fee payment before processing.`);
//                //$('#beforeProcessingDiv').hide(); // Hide the div
//            }
//        },
//        error: function (err) {
//            appalert(err.statusText, 1, 3);
//        }
//    });
//}

//function loadRefinancingLoan(loanId) {
//    $.get(`/MemberOperation/GetLoanForRefinancing?Key=${loanId}`, function (response) {
//        if (response) {
//            populateLoanModal(response);
//            $('#loanDetailsCard').collapse('show');

//            // ✅ Only call this after successful response
//        //    GetLoanPurposes(response.ProductCategoryId);
//        //    GetLoanApplication(response.LoanProductId)
//        //    LoanProductsPropertiesRefinancing(response.LoanProductId, 'loanrepayment_cycles', 'RepaymentCircle')
//        } else {
//            appalert("No loan data found.", 2, 1);
//        }
//    }).fail(function (xhr) {
//        appalert("Failed to load loan data: " + xhr.statusText, 0, 1);
//    });
//}

////function GetLoan(loanid) {
////    if (!loanid) {
////        console.error("Loan ID is required.");
////        return;
////    }

////    $.ajax({
////        type: "GET",
////        url: `/MemberOperation/GetLoanForRefinancing?Key=${loanid}`,
////        success: function (response) {
////            // Adjust to match the actual response format
////            if (response.success === false) {
////                console.error(response.message);
////                alert(response.message);
////                return;
////            }

////            const data = response.data || response; // Use raw data if no 'data' property exists

////            // Populate form fields with the loan data
////            $('#oldLoanAmount').val(data.DueAmount);
////            $('#oldLoanCapital').val(data.Principal);
////            $('#oldLoanInterest').val(data.AccrualInterest);
////            $('#oldLoanVAT').val(data.Tax);
////            $('#oldLoanPenalty').val(data.Penalty);
////            $('#oldLoanLoanId').val(data.Id);
////            $('#oldLoanvatRate').val(data.VatRate);
////        },
////        error: function (xhr, status, error) {
////            console.error("Error fetching loan data:", error, "Response:", xhr.responseText);
////            appalert("An error occurred while fetching loan details. Please try again." + xhr.responseText + " Error: " + error + ". Status: " + status , 0, 1);
////        }
////    });
////}

//function calculateVATAndTotal() {
//    // Get the values from the input fields
//    const oldLoanCapital = parseFloat($('#oldLoanCapital').val()) || 0;
//    const oldLoanInterest = parseFloat($('#oldLoanInterest').val()) || 0;
//    const oldLoanPenalty = parseFloat($('#oldLoanPenalty').val()) || 0;
//    const oldLoanvatRate = parseFloat($('#oldLoanvatRate').val()) || 0;

//    // Calculate VAT (assume VAT rate is 15% for this example)
//    const oldLoanVAT = oldLoanInterest * oldLoanvatRate;

//    // Update the VAT field
//    $('#oldLoanVAT').val(oldLoanVAT.toFixed(2));

//    // Calculate the total loan amount
//    const totalAmount = oldLoanCapital + oldLoanInterest + oldLoanPenalty + oldLoanVAT;

//    // Update the total amount field
//    $('#oldLoanAmount').val(totalAmount.toFixed(2));
//}

//function EditReset(KEY, partialView) {
//    EditResetMain(KEY, partialView, "mainview", "Individual", "InitializeData");
//}


//function AjaxPostLoanScedule(form) {

//    $.validator.unobtrusive.parse(form);
//    if ($(form).valid()) {
//        var ajaxConfig = {
//            type: 'POST',
//            url: form.action,
//            data: new FormData(form),
//            success: function (response) {

//                if (response.success) {
//                    appalert(response.message, 1, 1);
//                    LoadLocalSchedule();
//                }
//                else {
//                    appalert(response.message, 2, 1);

//                }

//            }
//            , error: function (err) {
//                appalert(err.statusText, 0, 1);
//            }
//        };

//        if ($(form).attr('enctype') === "multipart/form-data") {
//            ajaxConfig["contentType"] = false;
//            ajaxConfig["processData"] = false;
//        }
//        $.ajax(ajaxConfig);

//    }
//    return false;

//}



//function showConfirmMessage(KEY, ServiceOption, tableID) {
//    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

//}

//function LoadLocalSchedule() {
//    LoadDataGen('MemberOperation', null, '_LoanSimulationScheduleData', 0, 'amortization_schedule_data_div', "KEY", 'applications', 'loan_schedule')
//}




//function GetLoanApplication(KEY) {
//    if (!KEY) return;

//    // =========================
//    // Small utils (GLOBAL inside this function)
//    // =========================
//    function num(v) {
//        if (v === null || v === undefined) return 0;
//        var s = (v + "").replace(/,/g, "").trim();
//        var n = Number(s || 0);
//        return isNaN(n) ? 0 : n;
//    }

//    function money(v) {
//        return num(v).toLocaleString('en-US', {
//            style: 'currency',
//            currency: 'XAF',
//            minimumFractionDigits: 0
//        });
//    }

//    function normalizeRange(min, max) {
//        min = num(min);
//        max = num(max);

//        // ✅ max missing => show min-min
//        if (max <= 0 && min > 0) max = min;

//        // ✅ swapped
//        if (max < min) { var t = max; max = min; min = t; }

//        return { min: min, max: max };
//    }

//    function closestFeedbackHost($input) {
//        // Works for textboxes in form-floating or any container
//        return $input.closest(".form-floating, .form-floating-outline, .input-group, .mb-3, .col-md-12, .col-md-6, .col-md-3, .col-12").first();
//    }

//    function markLimit($input, ok, msg) {
//        if (!$input || !$input.length) return;

//        // Reset classes
//        $input.removeClass("is-valid is-invalid tsc-ok-input");

//        if (ok === true) {
//            // ✅ do NOT use bootstrap is-valid (neon). Use our subtle class.
//            $input.addClass("tsc-ok-input");
//        } else if (ok === false) {
//            $input.addClass("is-invalid");
//        }

//        // feedback block
//        var id = $input.attr("id") || ($input.attr("name") || "").replace(/[\[\]\.]/g, "_");
//        var fbId = "fb_" + id;

//        var $fb = $("#" + fbId);
//        if (!$fb.length) {
//            $fb = $('<div class="d-block small mt-1"></div>').attr("id", fbId);

//            // attach under closest container
//            $input.closest(".form-floating, .form-floating-outline, .mb-3, .col-md-12, .col-md-6, .col-md-3, .col-12")
//                .first()
//                .append($fb);
//        }

//        $fb.removeClass("tsc-ok-text tsc-bad-text");
//        $fb.addClass(ok ? "tsc-ok-text" : "tsc-bad-text");
//        $fb.text(msg || "");
//    }


//    function validateNumberRange(value, range, label, unitSuffix) {
//        var v = num(value);
//        var min = num(range.min);
//        var max = num(range.max);

//        // no constraint at all
//        if (min === 0 && max === 0) return { ok: true, msg: "" };

//        // max missing => treat as min only
//        if (max <= 0 && min > 0) max = min;

//        // swapped
//        if (max < min) { var t = max; max = min; min = t; }

//        var ok = (v >= min && v <= max);
//        var msg = ok
//            ? ("✅ " + label + " OK (" + v + (unitSuffix || "") + ")")
//            : ("❌ " + label + " out of range. Allowed: " + min + " - " + max + (unitSuffix || ""));

//        return { ok: ok, msg: msg };
//    }

//    function pcmfTag(p) {
//        var section = (p.PcmfSection || p.pcmfSection || "").toString();
//        var group = (p.PcmfGroupCode ?? p.pcmfGroupCode);
//        var baseCode = (p.PcmfBaseCode ?? p.pcmfBaseCode);
//        var pop = (p.PcmfPopulation || p.pcmfPopulation || "").toString();

//        var parts = [];
//        if (baseCode != null) parts.push("Base: " + baseCode);
//        if (section || group != null) parts.push((section || "N/A") + "-" + (group != null ? group : "N/A"));
//        if (pop) parts.push("Pop: " + pop);

//        return parts.length
//            ? "<span class='badge bg-primary ms-1'>PCMF: " + parts.join(" | ") + "</span>"
//            : "";
//    }

//    function setValByName(name, value) {
//        var $el = $('[name="' + name + '"]');
//        if (!$el.length) return;
//        $el.val(value).trigger("change");
//    }

//    function getValByName(name) {
//        var $el = $('[name="' + name + '"]');
//        if (!$el.length) return 0;
//        return num($el.val());
//    }

//    function isCheckedByName(name) {
//        var $el = $('[name="' + name + '"]');
//        return $el.length ? ($el.prop("checked") === true) : false;
//    }

//    // =========================
//    // Ensure computation panel exists (inject once)
//    // Put it under RiskMitigationDiv to be visible to user.
//    // =========================
//    function ensurePcmfCoverageBox() {
//        if ($("#pcmfCoverageBox").length) return;

//        // inject just before the first <hr> inside RiskMitigationDiv body (fallback append)
//        var $riskBody = $("#RiskMitigationDiv .accordion-body .row.g-4").first();
//        if (!$riskBody.length) return;

//        var html =
//            '<div class="col-12" id="pcmfCoverageBox" style="display:none;">' +
//            '  <div class="card border shadow-sm">' +
//            '    <div class="card-header d-flex align-items-center justify-content-between">' +
//            '      <div class="fw-semibold">Down Payment Coverage Calculator</div>' +
//            '      <div id="pcmfTagInline"></div>' +
//            '    </div>' +
//            '    <div class="card-body">' +
//            '      <div class="row g-3">' +
//            '        <div class="col-md-6"><div class="small text-muted">Principal (Requested)</div><div id="pcmfPrincipalText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-md-6"><div class="small text-muted">Saving Coverage Rate</div><div id="pcmfSavingRateText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-md-6"><div class="small text-muted">Required Savings (Principal × Saving %)</div><div id="pcmfSavingRequiredText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-md-6"><div class="small text-muted">Down Payment Rate</div><div id="pcmfDownRateText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-md-6"><div class="small text-muted">Required Down Payment (Principal × DP %)</div><div id="pcmfDownRequiredText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-md-6"><div class="small text-muted">Coverage Total (excluding shares)</div><div id="pcmfCoverageTotalText" class="fw-semibold">-</div></div>' +
//            '        <div class="col-12"><div id="pcmfCoverageStatus" class="fw-semibold mt-2"></div></div>' +
//            '      </div>' +
//            '      <div class="text-muted small mt-2">Note: Shares are validated but excluded from down payment coverage computation.</div>' +
//            '    </div>' +
//            '  </div>' +
//            '</div>';

//        // place it near top of Risk Mitigation section
//        $riskBody.prepend(html);
//    }

//    // =========================
//    // Ajax load product
//    // =========================
//    $.ajax({
//        type: "GET",
//        url: '/MemberOperation/GetLoanProduct?Key=' + encodeURIComponent(KEY),
//        success: function (data) {
//            if (!data) return;

//            ensurePcmfCoverageBox();

//            // ----------------------------
//            // Extract policy/term safely
//            // ----------------------------
//            var policy = data.Policy || data.policy || {};
//            var term = data.Term || data.term || {};

//            // ----------------------------
//            // Labels (Loan Product) + PCMF
//            // ----------------------------
//            var loanAmtRange = normalizeRange(policy.LoanMinimumAmount, policy.LoanMaximumAmount);
//            $("#amount").html(
//                "Enter amount from: " + money(loanAmtRange.min) +
//                " to " + money(loanAmtRange.max) +
//                " " + pcmfTag(data)
//            );

//            // Duration label
//            var termMin = num(term.MinInMonth || term.minInMonth);
//            var termMax = num(term.MaxInMonth || term.maxInMonth);
//            if (termMax <= 0 && termMin > 0) termMax = termMin;
//            if (termMax < termMin) { var tm = termMax; termMax = termMin; termMin = tm; }
//            if (termMin || termMax) {
//                $("#loanduration").html("Loan duration is between: " + (termMin || 0) + " to " + (termMax || 0) + " Month(s)");
//            }

//            // Interest label
//            if (policy.MinimumInterestRate != null || policy.MaximumInterestRate != null) {
//                $("#interest").html(
//                    "Enter interest between: " + (num(policy.MinimumInterestRate) || 0) +
//                    "% and " + (num(policy.MaximumInterestRate) || 0) + "%."
//                );
//            }

//            // ----------------------------
//            // Risk mitigation labels
//            // ----------------------------
//            var shareR = normalizeRange(policy.MinimumShareAccountBalanceForTheRequestAmount, policy.MaximumShareAccountBalanceForTheRequestAmount);
//            $("#share").html("Enter required share amount between: " + money(shareR.min) + " and " + money(shareR.max));

//            var savMinRate = num(policy.MinimumSavingAccountBalanceRateForTheRequestAmount);
//            var savMaxRate = num(policy.MaximumSavingAccountBalanceRateForTheRequestAmount);
//            if (savMaxRate <= 0 && savMinRate > 0) savMaxRate = savMinRate;
//            if (savMaxRate < savMinRate) { var x = savMaxRate; savMaxRate = savMinRate; savMinRate = x; }
//            $("#saving").html("Enter balance saving rate between: " + savMinRate + "% and " + savMaxRate + "%");

//            // Salary label (optional)
//            if (policy.MinimumSalaryAccountBalanceRateForTheRequestAmount != null || policy.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount != null) {
//                $("#salary").html("Enter Salary rate between: " +
//                    (num(policy.MinimumSalaryAccountBalanceRateForTheRequestAmount) || 0) + "% and " +
//                    (num(policy.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount) || 0) + "%");
//            }

//            // ----------------------------
//            // Down payment label + checkbox lock
//            // ----------------------------
//            var dpRate = num(policy.MinimumDownPaymentPercentage);
//            $("#downpaymentrate").html("Does this application require down payment? Minimum rate is [" + dpRate + "%].");

//            var requiredDownPayment = dpRate > 0;
//            $("#downPaymentCheckbox").prop("checked", requiredDownPayment).prop("disabled", true);

//            // ----------------------------
//            // Field names from your Razor
//            // ----------------------------
//            var principalField = 'AddLoanApplicationCommand.Amount';
//            var durationField = 'AddLoanApplicationCommand.LoanDuration';
//            var interestField = 'AddLoanApplicationCommand.InterestRate';

//            var shareField = 'AddLoanApplicationCommand.ShareAccountCoverageAmount';
//            var savingRateField = 'AddLoanApplicationCommand.SavingAccountCoverageRate';

//            var prefToggle = 'AddLoanApplicationCommand.IsPreferenceShareAccountCoverageAmount';
//            var prefAmount = 'AddLoanApplicationCommand.PreferenceShareAccountCoverageAmount';

//            var depToggle = 'AddLoanApplicationCommand.IsDepositAccountCoverageAmount';
//            var depAmount = 'AddLoanApplicationCommand.DepositAccountCoverageAmount';

//            var salToggle = 'AddLoanApplicationCommand.IsSalryAccount';
//            // ⚠️ In your form it is SalaryAccountCoverageRate (but user enters it as amount/standing order)
//            var salAmount = 'AddLoanApplicationCommand.SalaryAccountCoverageRate';

//            // ----------------------------
//            // Auto-fill (baseline) when empty/0
//            // ----------------------------
//            if (getValByName(shareField) <= 0 && shareR.min > 0) setValByName(shareField, shareR.min);
//            if (getValByName(savingRateField) <= 0 && savMinRate > 0) setValByName(savingRateField, savMinRate);

//            // ----------------------------
//            // Ranges for validation
//            // ----------------------------
//            var amountRange = loanAmtRange;
//            var durationRange = { min: termMin, max: termMax };

//            var intMin = num(policy.MinimumInterestRate);
//            var intMax = num(policy.MaximumInterestRate);
//            if (intMax <= 0 && intMin > 0) intMax = intMin;
//            if (intMax < intMin) { var it = intMax; intMax = intMin; intMin = it; }
//            var interestRange = { min: intMin, max: intMax };

//            var savingRange = { min: savMinRate, max: savMaxRate };
//            var shareRange = shareR;

//            // ----------------------------
//            // Computations (EXCLUDE shares)
//            // ----------------------------
//            function readPrincipal() { return getValByName(principalField); }
//            function readSavingRate() { return getValByName(savingRateField); }

//            function computedRequiredSavingsAmount() {
//                var p = readPrincipal();
//                var r = readSavingRate();
//                if (p <= 0 || r <= 0) return 0;
//                return (p * r) / 100.0;
//            }

//            function requiredDownPaymentAmount() {
//                var p = readPrincipal();
//                if (p <= 0 || dpRate <= 0) return 0;
//                return (p * dpRate) / 100.0;
//            }

//            function coverageTotalExcludingShares() {
//                var total = 0;

//                // ✅ Savings required amount (computed)
//                total += computedRequiredSavingsAmount();

//                // ✅ Optional coverage amounts ONLY when toggled
//                if (isCheckedByName(depToggle)) total += getValByName(depAmount);
//                if (isCheckedByName(salToggle)) total += getValByName(salAmount);
//                if (isCheckedByName(prefToggle)) total += getValByName(prefAmount);

//                // ❌ Shares excluded
//                return total;
//            }

//            // ----------------------------
//            // Validation + render
//            // ----------------------------
//            function validateAllLimits() {
//                var $principal = $('[name="' + principalField + '"]');
//                var $duration = $('[name="' + durationField + '"]');
//                var $interest = $('[name="' + interestField + '"]');

//                var $savingRate = $('[name="' + savingRateField + '"]');
//                var $shareAmt = $('[name="' + shareField + '"]');

//                // Requested amount
//                if ($principal.length) {
//                    var r1 = validateNumberRange($principal.val(), amountRange, "Requested Amount", " FCFA");
//                    markLimit($principal, r1.ok, r1.msg);
//                }

//                // Duration (months)
//                if ($duration.length && (durationRange.min || durationRange.max)) {
//                    var r2 = validateNumberRange($duration.val(), durationRange, "Loan Duration", " Month(s)");
//                    markLimit($duration, r2.ok, r2.msg);
//                }

//                // Interest (%)
//                if ($interest.length && (interestRange.min || interestRange.max)) {
//                    var r3 = validateNumberRange($interest.val(), interestRange, "Interest Rate", "%");
//                    markLimit($interest, r3.ok, r3.msg);
//                }

//                // Saving % (rate)
//                if ($savingRate.length && (savingRange.min || savingRange.max)) {
//                    var r4 = validateNumberRange($savingRate.val(), savingRange, "Saving Coverage Rate", "%");
//                    markLimit($savingRate, r4.ok, r4.msg);
//                }

//                // Shares amount (validated, but excluded from computation)
//                if ($shareAmt.length && (shareRange.min || shareRange.max)) {
//                    var r5 = validateNumberRange($shareAmt.val(), shareRange, "Share Coverage Amount", " FCFA");
//                    markLimit($shareAmt, r5.ok, r5.msg);
//                }
//            }

//            function renderComputation() {
//                var p = readPrincipal();
//                var savingRate = readSavingRate();

//                var reqSavings = computedRequiredSavingsAmount();
//                var reqDown = requiredDownPaymentAmount();
//                var cov = coverageTotalExcludingShares();

//                var showBox = (p > 0) || (dpRate > 0);
//                $("#pcmfCoverageBox").toggle(showBox);

//                $("#pcmfTagInline").html(pcmfTag(data));
//                $("#pcmfPrincipalText").text(p > 0 ? money(p) : "-");
//                $("#pcmfSavingRateText").text(savingRate > 0 ? (savingRate + "%") : "-");
//                $("#pcmfSavingRequiredText").text(reqSavings > 0 ? money(reqSavings) : money(0));
//                $("#pcmfDownRateText").text(dpRate > 0 ? (dpRate + "%") : "0%");
//                $("#pcmfDownRequiredText").text(reqDown > 0 ? money(reqDown) : money(0));
//                $("#pcmfCoverageTotalText").text(cov > 0 ? money(cov) : money(0));

//                // if dpRate is 0 => always OK
//                var ok = (dpRate <= 0) ? true : (cov >= reqDown);

//                $("#pcmfCoverageStatus")
//                    .removeClass("text-success text-danger")
//                    .addClass(ok ? "text-success" : "text-danger")
//                    .html(ok
//                        ? ("✅ Down payment covered (excluding shares): " + money(cov) + " / " + money(reqDown))
//                        : ("❌ Down payment NOT covered (excluding shares): " + money(cov) + " / " + money(reqDown))
//                    );
//            }

//            function refreshAll() {
//                validateAllLimits();
//                renderComputation();
//            }

//            // ----------------------------
//            // Live updates (include amount, duration, interest, saving, share, toggles & amounts)
//            // ----------------------------
//            var watchNames = [
//                principalField,
//                durationField,
//                interestField,

//                shareField,
//                savingRateField,

//                depAmount,
//                salAmount,
//                prefAmount,

//                depToggle,
//                salToggle,
//                prefToggle
//            ];

//            watchNames.forEach(function (name) {
//                var $el = $('[name="' + name + '"]');
//                if (!$el.length) return;

//                $el.off("input.pcmfcalc change.pcmfcalc")
//                    .on("input.pcmfcalc change.pcmfcalc", function () {
//                        refreshAll();
//                    });
//            });

//            // Initial run
//            refreshAll();

//            // ----------------------------
//            // Fee before processing (keep your logic)
//            // ----------------------------
//            var isPaidFeeBeforeProcessing = policy.IsPaidFeeBeforeProcessing === true;
//            var $paidFeeCheckbox = $('[name="AddLoanApplicationCommand.IsPaidFeeBeforeProcessing"]');
//            var $processingLabel = $('label[for="FeePaidBeforeProcessing"]');
//            var loanProductName = data.ProductName || data.ProductCode || "this loan product";

//            if (policy.IsPaidFeeBeforeProcessing != null) {
//                if (isPaidFeeBeforeProcessing) {
//                    $paidFeeCheckbox.prop('checked', true).prop('disabled', true);
//                    $processingLabel.html("A partial fee must be paid at the cash desk before the loan (" + loanProductName + ") can be processed.");
//                    $('#beforeProcessingDiv').show();
//                } else {
//                    $paidFeeCheckbox.prop('checked', false).prop('disabled', false);
//                    $processingLabel.html("(" + loanProductName + ") is not configured for partial fee payment before processing.");
//                }
//            }
//        },
//        error: function (err) {
//            appalert(err.statusText, 1, 3);
//        }
//    });
//}

// --- keep populateLoanModal, styleLoanStatus, formatXAF, formatDate, etc exactly as you had ---
// (No changes below this point to avoid breaking existing behaviors)
