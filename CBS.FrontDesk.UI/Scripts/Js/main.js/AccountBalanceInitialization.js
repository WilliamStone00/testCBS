

$(document).ready(function () {
 
});
function submitAccountData() {
    const container = document.getElementById("datalistingview_AccountInitialize");
    var ddddatascope = getAccountInitializeFormData();
    var datascope = createAccountBalanInitConfiguration(ddddatascope);

    $.ajax({
        url: '/AccountBalanceInitialization/Create',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(datascope),
        success: function (response) {
            if (response.status===true) {
                loadAccountList(datascope.BranchId, datascope.ProductId);
            } else {
                appalert("This Account Initialization failed ", 1, 2);
            }
         
        }
    });
}
function getAccountInitializeFormData() {
    return {
        branchId: document.getElementById('InitInfoDto_BranchId')?.value || "",
        productId: document.getElementById('InitInfoDto_ProductId')?.value || "",
        submittedMemberBalance: document.getElementById('SubmittedMemberBalance')?.value || "0",
        submittedGLBalance: document.getElementById('SubmittedGLBalance')?.value || "0",
        scenarioId: document.getElementById('InitInfoDto_ScenarioId')?.value || ""
    };
}
function createAccountBalanInitConfiguration(data) {
    return {
        initInfoDto: {
            scenarioId: data.scenarioId,
            productId: data.productId,
            submittedMemberBalance: data.submittedMemberBalance,
            submittedGLBalance: data.submittedGLBalance,
            branchId: data.branchId
        },
        accountInitDtos: [
            // You can dynamically push items into this array as needed
            {
                accountId: "",
                accountNumber: "",
                accountName: "",
                openingBalance: 0
            }
        ]
    };
}
function GetAccountList(BranchId, ProductId) {
    $.getJSON('/AccountBalanceInitialization/GetAccountList?BranchId=' + BranchId + '&ProductId=' + ProductId, function (data) {
        const list = data.MakeAccountPostingCommands;
        const tbody = $('#depositTable tbody');
        tbody.empty();

        list.forEach((item, index) => {
            const totalAmount = item.AmountCollection.reduce((acc, a) => acc + a.Amount, 0);
            tbody.append(`
                <tr>
                    <td>${index + 1}</td>
                    <td>${item.TransactionReferenceId}</td>
                    <td>${item.AccountNumber}</td>
                    <td>${item.AccountHolder}</td>
                    <td>XAF ${totalAmount.toLocaleString()}</td>
                    <td>${item.BranchCode}</td>
                    <td>${formatDate(item.TransactionDate)}</td>
                    <td>
                        <button class="btn btn-sm btn-outline-primary" onclick='showDetails(${JSON.stringify(item)})'>🔍 View</button>
                    </td>
                </tr>
            `);
        });
    });
}

function showDetails(item) {
    $('#modalRef').text(item.TransactionReferenceId);
    $('#modalDate').text(formatDate(item.TransactionDate));
    $('#modalAccount').text(item.AccountNumber);
    $('#modalHolder').text(item.AccountHolder);
    $('#modalProduct').text(`${item.ProductName} (${item.ProductId})`);
    $('#modalBranch').text(`${item.BranchCode}`);

    let amountRows = '';
    item.AmountCollection.forEach(ac => {
        amountRows += `
            <tr>
                <td>${ac.EventAttributeName}</td>
                <td>${ac.LiaisonEventName}</td>
                <td>${ac.Amount.toLocaleString()}</td>
                <td>${ac.IsPrincipal ? '✔️' : '—'}</td>
                <td>${ac.Naration}</td>
            </tr>
        `;
    });
    $('#modalAmounts').html(amountRows);

    new bootstrap.Modal(document.getElementById('postingDetailsModal')).show();
}

function formatDate(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString('fr-FR', {
        year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit'
    });
}

