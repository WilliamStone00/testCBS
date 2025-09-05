
(() => {
  // -------- Constants --------
  const LOAN_VAT_THRESHOLD = 2000000; // VAT applies only when loan amount >= 2,000,000
    const VAT_EXCLUSIVE_RATE = 19.25;   // VAT on net interest
    const VAT_INCLUSIVE_RATE = 16.1425; // VAT portion inside a VAT-inclusive amount

  // Safe number parser
  const toNumber = (v) => (v == null ? 0 : (parseFloat(String(v).replace(/,/g, '')) || 0));

  // VAT mode: 'exclusive' | 'inclusive' | null
  const getVatMode = () => {
    const r = document.querySelector('input[name="vatMode"]:checked');
    return r ? r.value : null;
  };

    // Called when user types interest — ONLY stores raw (gross) input, clears VAT
    function handleInterestInput(loanId) {
    const interestEl = document.getElementById(`interest-${loanId}`);
    if (!interestEl) return;

    const entered = toNumber(interestEl.value);
    interestEl.dataset.gross = String(entered);     // remember what user typed

    const vatEl = document.getElementById(`vat-${loanId}`);
    if (vatEl) vatEl.value = "0";                   // clear VAT until mode is applied

    updateRowTotal(loanId);
    updateGrandTotals();
  }

    // Apply VAT for a single row given a mode
    function applyVatForRow(loanId, mode) {
    const interestEl = document.getElementById(`interest-${loanId}`);
    const vatEl      = document.getElementById(`vat-${loanId}`);
    if (!interestEl || !vatEl) return;

    const loanAmount = toNumber(interestEl.dataset.loanamount);
    let gross = toNumber(interestEl.dataset.gross);
    if (!gross) {                                   // fallback if user never typed yet
        gross = toNumber(interestEl.value);
    interestEl.dataset.gross = String(gross);
    }

    let vat = 0;

    if (loanAmount >= LOAN_VAT_THRESHOLD && (mode === 'exclusive' || mode === 'inclusive')) {
      if (mode === 'exclusive') {
        // Interest field remains what user typed (gross)
        interestEl.value = Math.round(gross);
    vat = Math.round(gross * (VAT_EXCLUSIVE_RATE / 100));
      } else {
        // Inclusive: compute VAT inside gross, then reduce interest to net (once)
        vat = Math.round(gross * (VAT_INCLUSIVE_RATE / 100));
    const net = Math.max(0, Math.round(gross - vat));
    interestEl.value = net;
      }
    } else {
        // Below threshold or no mode selected => no VAT, interest shows gross
        interestEl.value = Math.round(gross);
    vat = 0;
    }

    vatEl.value = vat.toLocaleString('en-US');
    updateRowTotal(loanId);
  }

    // Apply the currently selected VAT mode to ALL rows
    function applyVatModeToAllRows() {
    const mode = getVatMode(); // may be null
    document.querySelectorAll('#loanRepaymentTable tr').forEach(row => {
      const loanId = row.id.replace('row-', '');
    applyVatForRow(loanId, mode);
    });
    updateGrandTotals();
  }

    // Update one row total (capital + interest + vat + penalty)
    function updateRowTotal(loanId) {
    const interest = toNumber(document.getElementById(`interest-${loanId}`)?.value);
    const vat      = toNumber(document.getElementById(`vat-${loanId}`)?.value);
    const capital  = toNumber(document.getElementById(`capital-${loanId}`)?.value);
    const penalty  = toNumber(document.getElementById(`penalty-${loanId}`)?.value);

    const total = capital + interest + vat + penalty;
    const span = document.getElementById(`total-${loanId}`);
    if (span) span.innerText = (typeof formatCurrencyNoName === 'function'
    ? formatCurrencyNoName(total)
    : total.toLocaleString('en-US'));
  }

    // Grand totals (bottom)
    function updateGrandTotals() {
        let totalRepayment = 0, totalVat = 0, totalDebited = 0;

    document.querySelectorAll(".amount-input").forEach(i => totalDebited += toNumber(i.value));

    document.querySelectorAll("#loanRepaymentTable tr").forEach(row => {
      const id  = row.id.replace("row-", "");
    const interest = toNumber(document.getElementById(`interest-${id}`)?.value);
    const vat      = toNumber(document.getElementById(`vat-${id}`)?.value);
    const capital  = toNumber(document.getElementById(`capital-${id}`)?.value);
    const penalty  = toNumber(document.getElementById(`penalty-${id}`)?.value);

    totalRepayment += capital + interest + vat + penalty;
    totalVat += vat;
    });

    const setText = (id, val, withName=false) => {
      const el = document.getElementById(id); if (!el) return;
    el.innerText = (withName && typeof formatCurrency === 'function')
    ? formatCurrency(val)
    : (typeof formatCurrencyNoName === 'function'
    ? formatCurrencyNoName(val)
    : val.toLocaleString('en-US'));
    };

    setText("totalRepaymentAmount", totalRepayment);
    setText("calculatedVat", totalVat);
    setText("totalDebitedAmount", totalDebited, true);

    const balance = totalDebited - totalRepayment;
    setText("remainingBalance", balance, true);

    const ind = document.getElementById("balanceIndicator");
    if (ind) {ind.innerHTML = (balance === 0 ? " ✅" : " ❌"); ind.style.color = (balance === 0 ? "green" : "red"); }
  }

    // Expose functions used by inline attributes
    window.handleInterestInput = handleInterestInput;
    window.applyVatModeToAllRows = applyVatModeToAllRows;
    window.updateTotals = function(loanId){ if (loanId) updateRowTotal(loanId); updateGrandTotals(); };

  // Wire listeners once DOM is ready
  document.addEventListener('DOMContentLoaded', () => {
        // Capital / Penalty / Interest listeners
        document.querySelectorAll('#loanRepaymentTable tr').forEach(row => {
            const id = row.id.replace('row-', '');
            document.getElementById(`capital-${id}`)?.addEventListener('input', () => { updateRowTotal(id); updateGrandTotals(); });
            document.getElementById(`penalty-${id}`)?.addEventListener('input', () => { updateRowTotal(id); updateGrandTotals(); });
            document.getElementById(`interest-${id}`)?.addEventListener('input', () => handleInterestInput(id));
        });

    // VAT mode toggle — apply on change AND on click (so re-click reapplies)
    document.querySelectorAll('input[name="vatMode"]').forEach(r => {
        r.addEventListener('change', applyVatModeToAllRows);
    r.addEventListener('click',  applyVatModeToAllRows);
    });
      // Make Inclusive the default and apply once on load
      const inclusiveRadio = document.getElementById('vatModeInclusive');
      if (inclusiveRadio) inclusiveRadio.checked = true;
      applyVatModeToAllRows();

    // Initial totals only; DO NOT auto-apply VAT (wait for user choice)
    updateGrandTotals();
  });
})();

