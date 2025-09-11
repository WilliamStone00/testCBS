

$(document).ready(function () {
    SwitchShow();
});



function AjaxPostingForSearch(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);
    const container = document.getElementById("datalistingview_AccountInitialize");
    const initFormData = getAccountInitializeFormData();
    const payload = createAccountBalanInitConfiguration(initFormData);
    const confirmMsg = `
Are you sure you which to proccced  with this operation

Do you want to continue?`;
    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        alertify.confirm("OPERATION CONFIRMATION", confirmMsg,
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        console.log("Response:", response);
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

                        } else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else {
                                appalert(response.message, 2, 1);
                            }
                        }
                        GetAccountList(initFormData.branchId, initFormData.productId);

                    },
                    error: function (err) {
                        console.log("Error:", err);

                        if (err.status === 401) { // Unauthorized
                            // Session has expired, redirect to the login page
                            window.location.href = '/Authentication/Login'; // Adjust the URL as needed
                        } else {
                            appalert(err.statusText, 0, 1);
                        }
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig.contentType = false;
                    ajaxConfig.processData = false;
                }

                console.log("AJAX Config:", ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);
            }
        );
    }
    return false;
}


function AjaxPosting(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);
    const container = document.getElementById("datalistingview_AccountInitialize");
    const initFormData = getAccountInitializeFormData();
    const payload = createAccountBalanInitConfiguration(initFormData);
    const confirmMsg = `
You are about to initialize the following account:

- Branch: ${initFormData.branchId}
- Product Type: ${initFormData.productId}
- Member Balance: ${initFormData.submittedMemberBalance}
- GL Balance: ${initFormData.submittedGLBalance}
- Scenario: ${initFormData.scenarioId}

Do you want to continue?`;
    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        alertify.confirm("ACCOUNT INITIALIZATION CONFIRMATION", confirmMsg,
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        console.log("Response:", response);
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

                        } else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else {
                                appalert(response.message, 2, 1);
                            }
                        }
                        GetAccountList(initFormData.branchId, initFormData.productId, "_AccountInitializationData");

                    },
                    error: function (err) {
                        console.log("Error:", err);

                        if (err.status === 401) { // Unauthorized
                            // Session has expired, redirect to the login page
                            window.location.href = '/Authentication/Login'; // Adjust the URL as needed
                        } else {
                            appalert(err.statusText, 0, 1);
                        }
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig.contentType = false;
                    ajaxConfig.processData = false;
                }

                console.log("AJAX Config:", ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);
            }
        );
    }
    return false;
}
function LoadDataTableNew(controller, tableID, action, KEY, partialView, order, path, diveToloadtheData, serviceOption) {
    var encodedURL = '/' + controller + '/' + action +
        '?KEY=' + encodeURIComponent(KEY) +
        '&partialView=' + encodeURIComponent(partialView) +
        '&serviceOption=' + encodeURIComponent(serviceOption) +
        '&path=' + encodeURIComponent(path);


    $.ajax({
        type: "GET",
        url: encodedURL,
        success: function (data) {
            $('#' + diveToloadtheData).html(data);
            console.log($('#DataTablePosition').length);
            LoadDT(tableID, order);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}
function submitAccountData() {
    const container = document.getElementById("datalistingview_AccountInitialize");
    const initFormData = getAccountInitializeFormData();
    const payload = createAccountBalanInitConfiguration(initFormData);

    const confirmMsg = `
You are about to initialize the following account:

- Branch: ${initFormData.branchId}
- Product Type: ${initFormData.productId}
- Member Balance: ${initFormData.submittedMemberBalance}
- GL Balance: ${initFormData.submittedGLBalance}
- Scenario: ${initFormData.scenarioId}

Do you want to continue?`;

    if (!window.confirm(confirmMsg)) {
        return; // Cancel submission
    }

    $.ajax({
        url: '/AccountBalanceInitialization/Create',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (response) {
            if (response.status === true) {
                GetAccountList(payload.branchId, payload.productId);
            } else {
                appalert("This Account Initialization failed", 1, 2);
            }
        },
        error: function () {
            appalert("An error occurred during submission.", 1, 2);
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
function Switch() {
    $('#datalistingview_AccountInitializationData').show();
    $('#datalistingview_AccountInitialize').hide();
}
function SwitchShow() {
    $('#datalistingview_AccountInitializationData').hide();
    $('#datalistingview_AccountInitialize').show();
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
    $.ajax({
        url: '/AccountBalanceInitialization/GetAccountList',
        method: 'GET',
        data: { BranchId, ProductId },
        dataType: 'json',
        success: function (list) {
            const tbody = $('#depositTable tbody').empty();


            // Reload partial view section if necessary
            if (list.status === "Exist") {
                appalert(list.message, 3, 1);

            }
            else if (list.status === "Failed") {
                appalert(list.message, 2, 1);
            }
            else {
                for (let i = 0; i < list.length; i++) {
                    const item = list[i];
                    const row = `
        <tr>
            <td>${i + 1}</td>
            <td>${item.BranchCode} - ${item.BranchName}</td>
            <td>${item.ProductType}</td>
            <td>XAF ${item.ActualGLBalance.toLocaleString()}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary" onclick='showDetails(${JSON.stringify(item)})'>🔍 View</button>
            </td>
        </tr>
    `;
                    tbody.append(row);
                }
            }
      
        },
        error: function (xhr, status, error) {
            console.error("Failed to fetch account list:", error);
        }
    });
}

//function GetAccountList(BranchId, ProductId) {
//    $.getJSON('/AccountBalanceInitialization/GetAccountList?BranchId=' + BranchId + '&ProductId=' + ProductId,
//        function (data) {
//            console.log(data);
//            const list = data;
//            const tbody = $('#depositTable tbody');
//            tbody.empty();

//            list.forEach((item, index) => {
//                const totalAmount = item.ActualGLBalance;
//                tbody.append(`
//        <tr>
//            <td>${index + 1}</td>
//            <td>${item.BranchCode} - ${item.BranchName}</td>
//            <td>${item.ProductType}</td>
//            <td>XAF ${totalAmount.toLocaleString()}</td>
//            <td>
//                <button class="btn btn-sm btn-outline-primary" onclick="showDetails(${JSON.stringify(item)})">🔍 View</button>
//            </td>
//        </tr>
//    `);
//            });

//            // Replace the partial view content
//            $("#datalistingview_AccountInitialize").load("/AccountBalanceInitialization/_AccountInitializationData #datalistingview_AccountInitialize");
//        });
//}

function GetAccountListqqq(BranchId, ProductId) {
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

