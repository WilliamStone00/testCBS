
function GetMemberDataLoanSimulation() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val();
    AddORUpdateGen(memberId, 'datalistingview', '_LoanRepaymentSimulationForm', 'repayment', "CashDesk");
}



(() => {
  // --- VAT constants ---
  const THRESHOLD = 2000000;      // VAT applies only when LoanAmount >= 2,000,000
    const RATE_EXC  = 19.25;        // Exclusive: VAT = interest * 19.25%
    const RATE_INC  = 16.1425;      // Inclusive: VAT portion inside gross interest

  const toNum = v => (v == null ? 0 : (parseFloat(String(v).replace(/,/g, '')) || 0));
  const getMode = () => {
    const r = document.querySelector('input[name="vatMode"]:checked');
    return r ? r.value : null;
  };

  // When user types interest: store gross, and if a mode is selected, recompute VAT immediately for this row
  window.handleInterestInput = (id) => {
    const iEl = document.getElementById(`interest-${id}`);
    if (!iEl) return;
    iEl.dataset.gross = String(toNum(iEl.value));        // remember what user typed as baseline
    const mode = getMode();

    if (mode) {
        applyVatForRow(id, mode);                          // recompute VAT now (no shrinking bug)
    } else {
      const vEl = document.getElementById(`vat-${id}`);
    if (vEl) vEl.innerText = "0";
    updateRow(id);
    updateSummaryCards();
    }
  };

  // Capital/Penalty changes: just re-total
  window.onCapitalPenaltyChanged = (id) => {
        updateRow(id);
    updateSummaryCards();
  };

    // Core: compute VAT for a single row given a mode
    function applyVatForRow(id, mode) {
    const iEl = document.getElementById(`interest-${id}`);
    const vEl = document.getElementById(`vat-${id}`);
    if (!iEl || !vEl) return;

    const loanAmt = toNum(iEl.dataset.loanamount);
    let gross = toNum(iEl.dataset.gross);
    if (!gross) {                       // fallback if user never typed yet
        gross = toNum(iEl.value);
    iEl.dataset.gross = String(gross);
    }

    let vat = 0;

    if (loanAmt >= THRESHOLD) {
      if (mode === 'inclusive') {
        // VAT portion inside gross; reduce interest to net
        vat = Math.round(gross * (RATE_INC / 100));
    iEl.value = Math.max(0, Math.round(gross - vat));
      } else {
        // Exclusive: interest remains gross
        iEl.value = Math.round(gross);
    vat = Math.round(gross * (RATE_EXC / 100));
      }
    } else {
        // Below threshold: no VAT, show interest as typed
        iEl.value = Math.round(gross);
    vat = 0;
    }

    vEl.innerText = vat.toLocaleString('en-US');
    updateRow(id);
  }

  // Apply VAT to all rows
  window.applyVatModeToAllRows = () => {
    const mode = getMode();
    document.querySelectorAll('#loanRepaymentTable tr').forEach(row => {
      const id = row.id.replace('row-', '');
    applyVatForRow(id, mode);
    });
    updateSummaryCards();
  };

    // Per-row total (capital + interest + vat + penalty)
    function updateRow(id) {
    const interest = toNum(document.getElementById(`interest-${id}`)?.value);
    const vat      = toNum(document.getElementById(`vat-${id}`)?.innerText);
    const capital  = toNum(document.getElementById(`capital-${id}`)?.value);
    const penalty  = toNum(document.getElementById(`penalty-${id}`)?.value);

    const total = capital + interest + vat + penalty;
    const span  = document.getElementById(`total-${id}`);
    if (span) span.innerText = total.toLocaleString('en-US');
  }

    // Optional: update the cards (summaryCapital, summaryInterest, summaryPenalty, summaryVat, summaryTotal)
    function updateSummaryCards() {
        let sumCap=0, sumInt=0, sumVat=0, sumPen=0, sumTotal=0;

    document.querySelectorAll('#loanRepaymentTable tr').forEach(row => {
      const id   = row.id.replace('row-', '');
    const cap  = toNum(document.getElementById(`capital-${id}`)?.value);
    const intr = toNum(document.getElementById(`interest-${id}`)?.value);
    const vat  = toNum(document.getElementById(`vat-${id}`)?.innerText);
    const pen  = toNum(document.getElementById(`penalty-${id}`)?.value);
    const tot  = toNum(document.getElementById(`total-${id}`)?.innerText);

    sumCap += cap; sumInt += intr; sumVat += vat; sumPen += pen; sumTotal += tot;
    });

    const set = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = `${val.toLocaleString('en-US')} XAF`; };
    set('summaryCapital',  sumCap);
    set('summaryInterest', sumInt);
    set('summaryPenalty',  sumPen);
    set('summaryVat',      sumVat);

    const totalEl = document.getElementById('summaryTotal');
    if (totalEl) totalEl.textContent = `${sumTotal.toLocaleString('en-US')} XAF`;
  }

  // Init (ensure Inclusive selected and applied once)
  document.addEventListener('DOMContentLoaded', () => {
        // Wire VAT radios (re-apply even on same click)
        document.querySelectorAll('input[name="vatMode"]').forEach(r => {
            r.addEventListener('change', applyVatModeToAllRows);
            r.addEventListener('click', applyVatModeToAllRows);
        });

    // First pass: if inclusive radio exists, ensure it's checked and applied
    const inc = document.getElementById('vatModeInclusive');
    if (inc) inc.checked = true;

    // If rows already have values, apply the mode now
    applyVatModeToAllRows();
  });
})();

