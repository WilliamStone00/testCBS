document.addEventListener('DOMContentLoaded', function () {
    const submitBtn = document.querySelector('#submitBtn');
    const dateFrom = document.querySelector('.datefrom_custome');
    const dateTo = document.querySelector('.dateto_custome');
    const branch = document.querySelector('.branches_custome');
    const reportType = document.querySelector('.reportype_custome');
    const sourceMode = document.querySelector('.source_custome');
    const excludeLiaisonInternal = document.querySelector('.excludeLiaisonInternal_custome');
    const filterByAccount = document.querySelector('.filterByAccount_custome');
    const consolidated = document.querySelector('.consolidated_custome');

    submitBtn.addEventListener('click', (e) => {
        e.preventDefault();

        const ValuereportType = trim(reportType.value);
        const selectedBranches = trim(branch.value)!=="" ? Array.from(branch.selectedOptions).map(option => option.value) : []
           
        const selectedAccounts = filterByAccount.checked == true ? Array.from(filterByAccount.selectedOptions).map(option => option.value) : [];

            //trim(filterByAccount.value) !== "" ? Array.from(filterByAccount.selectedOptions).map(option => option.value) : [];

        const payload = {
            dateFrom: dateFrom.value || "2025-11-02",
            dateTo: dateTo.value || "2025-11-09",
            branchId: selectedBranches[0] || null,
            selectedBranchIds: selectedBranches,
            consolidated: consolidated.checked,
            excludeLiaisonInternal: excludeLiaisonInternal.checked,
            sourceMode: sourceMode.value,
            filteredAccounts: selectedAccounts, // ✅ your account list
            language: "en",
            paging: { page: 0, pageSize: 0 },
            sorting: { sortBy: "", sortDir: "" }
        };

        console.log("Payload:", payload);

        // Example switch logic
        if (ValuereportType) {
            switch (ValuereportType) {
                case "trial balance 6 columns":
                    axios.post('/TrialBalance/GenerateTrialBalance', payload)
                        .then(res => console.log(res.data))
                        .catch(err => console.error(err));
                    break;

                case "balance sheet":
                    axios.post('/Reports/GetBalanceSheet', payload);
                    break;

                case "income statement":
                    axios.post('/Reports/GetIncomeStatement', payload);
                    break;

                default:
                    console.log("Unknown report type selected.");
                    break;
            }
        }
    });
});

function trim(val) {
    return (val || "").toLowerCase().trim();
}
