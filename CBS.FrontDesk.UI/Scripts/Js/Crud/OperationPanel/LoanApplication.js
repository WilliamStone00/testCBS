/* =========================================================
 *  LOAN APPLICATION UI SCRIPT (SAFE PATCHED VERSION)
 *  - Fixes infinite loop on load_loan_products
 *  - Fixes Targets loading by term + purpose
 *  - Keeps your structure & functions (no breaking changes)
 *  - Makes Select2 rebind safe inside modal and normal pages
 * ========================================================= */

(function () {
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
        if (!KEY) return;

        $.ajax({
            type: "GET",
            url: '/MemberOperation/GetLoanProduct?Key=' + encodeURIComponent(KEY),
            success: function (data) {
                if (!data) return;

                // ----------------------------
                // Helpers
                // ----------------------------
                function formatCurrency(amount) {
                    var n = Number(amount || 0);
                    return n.toLocaleString('en-US', { style: 'currency', currency: 'XAF', minimumFractionDigits: 0 });
                }

                function fmtMonthRange(term) {
                    if (!term) return "";
                    var min = term.MinInMonth || term.minInMonth || 0;
                    var max = term.MaxInMonth || term.maxInMonth || 0;
                    if (!min && !max) return "";
                    return min + " - " + max + " Month(s)";
                }

                function pcmfTag(p) {
                    // Your PCMFLoanProduct has:
                    // PcmfSection, PcmfGroupCode, PcmfPopulation, PcmfBaseCode
                    var section = (p.PcmfSection || p.pcmfSection || "").toString();
                    var group = (p.PcmfGroupCode ?? p.pcmfGroupCode);
                    var baseCode = (p.PcmfBaseCode ?? p.pcmfBaseCode);
                    var pop = (p.PcmfPopulation || p.pcmfPopulation || "").toString();

                    var parts = [];
                    if (section || group) parts.push((section || "N/A") + "-" + (group != null ? group : "N/A"));
                    if (baseCode != null) parts.push("Base: " + baseCode);
                    if (pop) parts.push("Pop: " + pop);

                    return parts.length ? ("[PCMF: " + parts.join(" / ") + "]") : "";
                }

                // ----------------------------
                // UI updates (keep your existing ids)
                // ----------------------------
                var policy = data.Policy || data.policy || {};

                // ✅ These fields exist on policy (per your class)
                $('#amount').html(
                    "Enter amount from: " + formatCurrency(policy.LoanMinimumAmount) +
                    " to " + formatCurrency(policy.LoanMaximumAmount) + " " + pcmfTag(data)
                );

                // Duration: take from Term if present, else fallback to policy duration fields if you have them
                var term = data.Term || data.term;
                var termRange = fmtMonthRange(term);

                if (termRange) {
                    $('#loanduration').html("Loan duration is: " + termRange);
                } else if (policy.MinimumDurationPeriod != null || policy.MaximumDurationPeriod != null) {
                    $('#loanduration').html("Loan duration is between: " + (policy.MinimumDurationPeriod || 0) +
                        " to " + (policy.MaximumDurationPeriod || 0) + " Months");
                } else {
                    $('#loanduration').html("");
                }

                // Interest / VAT (if your policy returns these)
                if (policy.MinimumInterestRate != null || policy.MaximumInterestRate != null) {
                    $('#interest').html("Enter interest between: " + (policy.MinimumInterestRate || 0) +
                        "% and " + (policy.MaximumInterestRate || 0) + "%.");
                } else {
                    $('#interest').html("");
                }

                // Repayment cycles (PCMFLoanProduct has RepaymentCycles list)
                var cycles = data.RepaymentCycles || data.repaymentCycles || [];
                if (cycles && cycles.length) {
                    $('#installment').html("Repayment cycles: " + cycles.join(", "));
                } else if (policy.MinimumNumberOfRepayment != null || policy.MaximumNumberOfRepayment != null) {
                    $('#installment').html("Minimum repayment installment is: " + (policy.MinimumNumberOfRepayment || 0) +
                        " and Maximum is " + (policy.MaximumNumberOfRepayment || 0));
                } else {
                    $('#installment').html("");
                }

                // These depend on whether your policy contains them (keeping your original ids)
                if (policy.MinimumSavingAccountBalanceRateForTheRequestAmount != null || policy.MaximumSavingAccountBalanceRateForTheRequestAmount != null) {
                    $('#saving').html("Enter balance saving rate between: " +
                        (policy.MinimumSavingAccountBalanceRateForTheRequestAmount || 0) + "% and " +
                        (policy.MaximumSavingAccountBalanceRateForTheRequestAmount || 0) + "%");
                } else {
                    $('#saving').html("");
                }

                if (policy.MinimumShareAccountBalanceForTheRequestAmount != null || policy.MaximumShareAccountBalanceForTheRequestAmount != null) {
                    $('#share').html("Enter required share amount between: " +
                        formatCurrency(policy.MinimumShareAccountBalanceForTheRequestAmount) + " and " +
                        formatCurrency(policy.MaximumShareAccountBalanceForTheRequestAmount));
                } else {
                    $('#share').html("");
                }

                if (policy.MinimumSalaryAccountBalanceRateForTheRequestAmount != null || policy.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount != null) {
                    $('#salary').html("Enter Salary rate between: " +
                        (policy.MinimumSalaryAccountBalanceRateForTheRequestAmount || 0) + "% and " +
                        (policy.MaximumMaximumSalaryAccountBalanceRateForTheRequestAmount || 0) + "%");
                } else {
                    $('#salary').html("");
                }

                if (policy.MinimumProcessingFeeRate != null || policy.MaximumProcessingFeeRate != null) {
                    $('#fee').html("Enter processing fee rate between: " +
                        (policy.MinimumProcessingFeeRate || 0) + "% and " +
                        (policy.MaximumProcessingFeeRate || 0) + "%.");
                } else {
                    $('#fee').html("");
                }

                if (policy.MinimumInspectionFeeRate != null || policy.MaximumInspectionFeeRate != null) {
                    $('#inspectionfee').html("Enter inspection fee between: " +
                        (policy.MinimumInspectionFeeRate || 0) + "% and " +
                        (policy.MaximumInspectionFeeRate || 0) + "%.");
                } else {
                    $('#inspectionfee').html("");
                }

                if (policy.MinimumChargesToAppliedInPercentage != null || policy.MaximumChargesToAppliedPercentage != null) {
                    $('#chargeparcentages').html("Enter charge percentage between: " +
                        (policy.MinimumChargesToAppliedInPercentage || 0) + " % and " +
                        (policy.MaximumChargesToAppliedPercentage || 0) + "%");
                } else {
                    $('#chargeparcentages').html("");
                }

                if (policy.MinimumChargesStartDayAfterLoanDueDate != null || policy.MaximumChargesStartDayAfterLoanDueDate != null) {
                    $('#chargedayranges').html("Enter in days when charges start between: " +
                        (policy.MinimumChargesStartDayAfterLoanDueDate || 0) + " to " +
                        (policy.MaximumChargesStartDayAfterLoanDueDate || 0) + " days");
                } else {
                    $('#chargedayranges').html("");
                }

                if (policy.MinimumInterestWaiver != null || policy.MaximumInterestWaiver != null) {
                    $('#waiverranges').html("Enter in percentage interest to waive between: " +
                        (policy.MinimumInterestWaiver || 0) + "% and " +
                        (policy.MaximumInterestWaiver || 0) + "%.");
                } else {
                    $('#waiverranges').html("");
                }

                // Down payment rule
                if (policy.MinimumDownPaymentPercentage != null) {
                    $('#downpaymentrate').html("Does this application require down payment? Minimum rate is [" +
                        (policy.MinimumDownPaymentPercentage || 0) + "%].");

                    var requiredDownPayment = (policy.MinimumDownPaymentPercentage || 0) > 0;
                    var $downPaymentCheckbox = $('input[name="AddLoanApplicationCommand.RequiredDownPaymentCoverageRate"]');
                    $downPaymentCheckbox.prop('checked', requiredDownPayment);
                    $downPaymentCheckbox.prop('disabled', true);
                } else {
                    $('#downpaymentrate').html("");
                }

                // Paid fee before processing (if present in policy; keep your original behavior)
                var isPaidFeeBeforeProcessing = policy.IsPaidFeeBeforeProcessing === true;
                var $paidFeeCheckbox = $('input[name="AddLoanApplicationCommand.IsPaidFeeBeforeProcessing"]');
                var $processingLabel = $('label[for="FeePaidBeforeProcessing"]');
                var loanProductName = data.ProductName || data.ProductCode || "this loan product";

                if (policy.IsPaidFeeBeforeProcessing != null) {
                    if (isPaidFeeBeforeProcessing) {
                        $paidFeeCheckbox.prop('checked', true);
                        $paidFeeCheckbox.prop('disabled', true);
                        $processingLabel.html("A partial fee must be paid at the cash desk before the loan (" + loanProductName + ") can be processed.");
                        $('#beforeProcessingDiv').show();
                    } else {
                        $paidFeeCheckbox.prop('checked', false);
                        $paidFeeCheckbox.prop('disabled', false);
                        $processingLabel.html("(" + loanProductName + ") is not configured for partial fee payment before processing.");
                        // keep your decision whether to hide div or not
                    }
                }
            },
            error: function (err) {
                appalert(err.statusText, 1, 3);
            }
        });
    }

    // --- keep populateLoanModal, styleLoanStatus, formatXAF, formatDate, etc exactly as you had ---
    // (No changes below this point to avoid breaking existing behaviors)

})();


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




//function populateLoanModal(data) {
//    // 🧩 Identification
//    $('#rl_productId').text(data.LoanProductId || '');
//    $('#rl_productName').text(data.LoanProductName || '');
//    $('#rl_loanType').text(data.LoanType || '');
//    $('#rl_loanStatus').text(data.LoanStatus || '');
//    $('#rl_loanCategory').text(data.LoanCategory || '');

//    // ⚙️ Configuration
//    $('#rl_loanTerm').text(data.LoanTermName || '');
//    $('#rl_targetPopulation').text(data.LoanTarget || '');
//    $('#rl_loanPurpose').text(data.LoanPurpose || '');
//    $('#rl_repaymentPeriod').text(data.RepaymentCycle || '');
//    $('#rl_repaymentMode').text(data.RepaymentMode || '');
//    $('#rl_installments').text(data.NumberOfInstallments || '');
//    $('#rl_duration').text(data.LoanDurarion || '');
//    $('#rl_interestCalculationMethod').text(data.InterestCalculationMethod || '');

//    // 💰 Financials
//    $('#rl_interestRate').text((data.InterestRate || 0).toFixed(2) + '%');
//    $('#rl_vatRate').text((data.VatRate || 0).toFixed(2) + '%');
//    $('#rl_principal').text(formatXAF(data.Principal));
//    $('#rl_interest').text(formatXAF(data.AccrualInterest));
//    $('#rl_vat').text(formatXAF(data.Tax));
//    $('#rl_penalty').text(formatXAF(data.Penalty));
//    $('#rl_dueAmount').text(formatXAF(data.DueAmount));

//    // 📅 Dates
//    $('#rl_disbursementDate').text(formatDate(data.DisbursementDate));
//    $('#rl_maturityDate').text(formatDate(data.MaturityDate));
//    $('#rl_lastRepaymentDate').text(formatDate(data.LastRepaymentDate));
//    $('#rl_disbursementChannel').text(data.DisbursementChannel || '');

//    $('#rl_productCategoryId').text(data.ProductCategoryId || '');
//    $('#rl_productCategoryName').text(data.ProductCategoryName || '');

//    $('#oldLoanAmount').val(data.DueAmount);
//    $('#oldLoanCapital').val(data.Principal);
//    $('#oldLoanInterest').val(data.AccrualInterest);
//    $('#oldLoanVAT').val(data.Tax);
//    $('#oldLoanPenalty').val(data.Penalty);
//    $('#oldLoanLoanId').val(data.Id);
//    $('#oldLoanvatRate').val(data.VatRate);

//    // 🎨 Dynamic Styling Based on Loan Status
//    const wrapper = $('#loanDetailsCardWrapper');
//    const header = $('#loanDetailsHeader');
//    const label = $('#loanStatusLabel');

//    wrapper.removeClass('border-success border-danger border-warning');
//    header.removeClass('bg-success-subtle bg-danger-subtle bg-warning-subtle text-success text-danger text-warning');

//    if (!data.LoanStatus) return;

//    const status = data.LoanStatus.toLowerCase();
//    if (status.includes('delinquent') || status.includes('default')) {
//        wrapper.addClass('border-danger');
//        header.addClass('bg-danger-subtle text-danger');
//        label.text("🚨 Delinquent Loan Summary");
//    } else if (status.includes('pending')) {
//        wrapper.addClass('border-warning');
//        header.addClass('bg-warning-subtle text-warning');
//        label.text("⚠️ Pending Loan Summary");
//    } else {
//        wrapper.addClass('border-success');
//        header.addClass('bg-success-subtle text-success');
//        label.text("✅ Active Loan Summary");
//    }
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



