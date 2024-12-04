
$(document).ready(function () {
    LoadOpenBranches()
});

function LoadOpenBranches() {
    LoadDataGen('Dashboard', 'myDataTable', '_OpenedBranchesData', 0, 'datalistingview', "KEY", 'open_branches', 'open_branches')
}



