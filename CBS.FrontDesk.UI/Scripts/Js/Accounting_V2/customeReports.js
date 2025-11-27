
////    const submitBtn = document.querySelector('#submitBtn');
// const dateFrom = document.querySelector('.datefrom_custome');
//    const dateTo = document.querySelector('.dateto_custome');
    
//const reportType = document.querySelector('.reportype_custome');
//const sourceMode = document.querySelector('.source_custome');
//const accountNumbers = [];
//const branches = [];




//const isInterbranch = document.querySelector('#IsInterbranch');
// const isInterLaison = document.querySelector('.filterByAccount_custome');
//const consolidated = document.querySelector('#IsConsolidated');




//const branch = document.querySelector('.branches_custome');
//const accountDropdown = document.getElementById('AccountId');
//const account_group = document.querySelector('.account_group');
    
//    branch.onchange = function (e) {
//        axios.post('/CustomeReports/loadAccountById/', { branchId: e.target.value }).then((response) => {
//            const data = response.data.results;
//            accountDropdown.innerHTML = "";
//            accountDropdown.innerHTML = '<option value="">--- Select Account ---</option>';

//            account_group.classList.remove('d-none');
//            data.forEach(function (item) {
//                const option = document.createElement("option");
//                option.value = item.id;     
//                option.text = item.Name;
//                accountDropdown.appendChild(option);
//            })
//        })
//    }




////function Reprint(controller) {
////    //window.open(redirectUrl, '_blank');\

////    ReportView(controller, null, "GetReport", null, null, "receipts", "ReportParameterLess");
////}



////submitBtn.addEventListener('click', (e) => {
////        e.preventDefault();

////        const ValuereportType = trim(reportType.value);
////        const selectedBranches = trim(branch.value)!=="" ? Array.from(branch.selectedOptions).map(option => option.value) : []
           
////        const selectedAccounts = filterByAccount.checked == true ? Array.from(filterByAccount.selectedOptions).map(option => option.value) : [];

////            //trim(filterByAccount.value) !== "" ? Array.from(filterByAccount.selectedOptions).map(option => option.value) : [];

////        const payload = {
////            dateFrom: dateFrom.value || "2025-11-02",
////            dateTo: dateTo.value || "2025-11-09",
////            branchId: selectedBranches[0] || null,
////            selectedBranchIds: selectedBranches,
////            consolidated: consolidated.checked,
////            excludeLiaisonInternal: excludeLiaisonInternal.checked,
////            sourceMode: sourceMode.value,
////            accountNumbers: 
////            filteredAccounts: accountNumbers, // ✅ your account list
////            language: "en",
////            paging: { page: 0, pageSize: 0 },
////            sorting: { sortBy: "", sortDir: "" }
////        };


        

        

////        switch (ValuereportType) {
////            case "account statement":

////                axios.post('/AccntStatement/GenerateReport/', payload).then((response) => {
////                    console.log(response);
////                    Reprint("AccntStatement")
////                })
////                break;

////            case "trial balance 6 columns":
////                axios.post('/TrialBalance/GenerateTrialBalance/', payload).then((response) => {
////                    Reprint("TrialBalance")
////                })
////                break;
////            case "je":
////                axios.post('/JournalEntries/GenerateJournalEntries/', payload).then((response) => {
////                    console.log(response);
////                    Reprint("JournalEntries")
////                })
////                break;

////            default:
////                // fallback if none match
////                break;
////        }
        



////    });


////function trim(val) {
////    return (val || "").toLowerCase().trim();
////}
