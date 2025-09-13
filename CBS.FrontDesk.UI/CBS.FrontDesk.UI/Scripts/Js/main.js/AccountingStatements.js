$(document).ready(function () {
    // Function to set the date to January 1st of current year
    function setDateToJanuaryFirst() {
        var currentYear = new Date().getFullYear();
        var januaryFirst = currentYear + '-01-01T00:00';
        $('#fromDate').val(januaryFirst);
    }

   
    // Cache elements
    const $auditCheckbox = $('#ActivateAuditId');
    const $branchList = $('#ListOfBranchToHide');
    const $branchSelect = $('#SystemQuery_BranchIds'); // Your select2 element
    $('#ListOfBranchToHide').hide();
    $('#SystemQuery_BranchIds').hide();
    $('#lunchBalanceSheetBuilder').hide();
    //// Initialize - hide branch list if checkbox is checked
    toggleBranchList(!$auditCheckbox.is(':checked'));

    // Handle checkbox change DeleteBtn
    $auditCheckbox.change(function () {
        toggleBranchList($(this).is(':checked'));
    });
    function toggleBranchList(shouldHide) {
        if (shouldHide) {
            $branchList.slideUp(300);
            // Clear selection when hiding
            $branchSelect.val(null).trigger('change');
        } else {
            $branchList.slideDown(300);
        }
    }
    LoadDownloadedReportByUser("myDataTable");
    LoadBranchAndAccountDataSetDT("BranchDataTable");
    LoadAccountDataSetDT("AccountDataTable");
    $('#AccountToHide').hide();

    $("#btnData").click(function () {
        LoadData();
    });
    // Initialize date pickers 

    $(document).on('change', '#SystemQuery_BranchId', function () {
        // Get the selected value AccountNumber
        var selectedValue = $(this).val();
        $("#selectedBranchID").val(selectedValue);
        var selectedReportType = $("#SystemQuery_ReportType").val();

        if (selectedReportType == "LL") {
            // Load another dropdown based on the selected value
            loadBranchLiasonAccount(selectedValue);
        } else if(selectedReportType == "GL") {
            loadBranchAccounts(selectedValue);
        }

    });

    $(document).on('change', '#SystemQuery_ReportType', function () {
        var selectedValue = $(this).val();
        $('#lunchBalanceSheetBuilder').hide();
        $('#AccountToHide').hide();
        // Check if the selected value matches the specific value
        if (selectedValue === 'GL') {
            // Show the element
            $('#AccountToHide').show();
            $('#fromDate').val('');
            var selectedId = $("#selectedBranchID").val();
            loadBranchAccounts(selectedId);
        } else if (selectedValue === 'PANDL') {
            $('#lunchBalanceSheetBuilder').show();
            setDateToJanuaryFirst();
        } else if (selectedValue === 'BS') {
            $('#lunchBalanceSheetBuilder').show();
            setDateToJanuaryFirst();
        } else if (selectedValue === 'TB6' || selectedValue === 'TB4') {
            // Call the function when document is ready
            setDateToJanuaryFirst();

        } else {

            $('#fromDate').val('');
            $('#AccountToHide').hide();
        }

    });
    $('#DeleteBtn a').on('click', function (e) {
        // Optional: prevent the default link behavior if needed
        e.preventDefault();

        // Hide the entire <th> element with ID DeleteBtn LunchBalanceSheetBuilder
        $('#DeleteBtn').hide();

 
    });
});

function LoadBranchAndAccountDataSetDT(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "25%" },

        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}
 
function launchBalanceSheetBuilder() {
    try {
        const model = collectSystemQueryData();

        // Validate required fields
        if (!model.BranchId || !model.FromDate || !model.ToDate) {
            appalert('Branch ID, From Date, and To Date are required fields.', 2, 1);
            return;
        }

        $.ajax({
            url: '/AccountingStatements/GenerateBalanceSheet',
            type: 'POST',  // Fixed capitalization (HTTP methods are conventionally uppercase)
            contentType: 'application/json',
            data: JSON.stringify({  // Ensure data is properly stringified for JSON
                BranchId: model.BranchId,
                FromDate: model.FromDate,
                ToDate: model.ToDate,
                FileType: model.FileType
            }),
            success: function (response) {
                console.log(response);
                if (!response) {
                    appalert('No response received from the server.', 2, 1);
                    return;
                }

                if (response.status) {
                    appalert(response.message, 1, 1);
                    buildBuilderInterface(response.TrialBalance, response.Assets, response.Liabilities,);
                } else {
                    appalert(response.message || 'Operation failed without a specific error message.', 2, 1);
                }
            },
            error: function (xhr, status, error) {
                const errorMessage = xhr.responseJSON?.message || error || 'An unknown error occurred.';
                appalert(`Server error: ${errorMessage}`, 0, 3);  // Added clarity in error message
            }
        });
    } catch (error) {
        console.error('Error in launchBalanceSheetBuilder:', error);
        appalert(`An unexpected error occurred: ${error.message}`, 0, 3);
    }
}
function collectSystemQueryData() {
    try {
        const systemQueryData = {
            BranchId: $('#SystemQuery_BranchId').val(),
            BranchIds: $('#SystemQuery_BranchIds').val(), // Multi-select
            AccountIds: $('#SystemQuery_AccountId').val(), // Multi-select
            ReportType: $('#SystemQuery_ReportType').val(),
            FileType: $('[name="SystemQuery.FileType"]').val(),
            FromDate: $('#fromDate').val(),
            ToDate: $('#toDate').val(),
            ActivateAudit: $('#ActivateAuditId').is(':checked')
        };

        console.log("Collected SystemQuery Data:", systemQueryData);
        return systemQueryData;
    } catch (error) {
        console.error('Error in collectSystemQueryData:', error);
        throw error; // Re-throw to be caught by the calling function
    }
}
function clearBalanceSheetBuilder() {
    $("#trialBalance .accordion-table-container").empty();
    $("#balanceSheet .balance-sheet-container .accordion-table-container").empty();
}

// Builds the interactive interface for the balance sheet builder
function clearBalanceSheetBuilder() {
    $("#trialBalance .accordion-table-container").empty();
    $(".balance-sheet-asset-section .accordion-table-container").empty();
    $(".balance-sheet-liabilities-section .accordion-table-container").empty();
    $("#balance_status").remove();
    $("#status_message").remove();
}

// Builds the interactive interface for the balance sheet builder
function buildBuilderInterface(trialBalanceData, assetsData, liabilitiesData) {
    clearBalanceSheetBuilder();

    let selectedTarget = null;
    let selectedAccounts = {}; // Track which accounts are used in each target cell
    let usedAccounts = new Set(); // Track which trial balance accounts have been used

    // --------------------- Trial Balance Table ---------------------
    const trialBalanceTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const tbThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Account Number"),
            $("<th>").text("Name"),
            $("<th>").text("Bal Dr"),
            $("<th>").text("Bal Cr")
        )
    );
    const tbTbody = $("<tbody>");

    trialBalanceData.forEach(item => {
        const row = $("<tr>");
        row.append($("<td>").text(item.accountNumber));
        row.append($("<td>").text(item.accountName));

        const debitCell = $("<td>").text(formatNumber(item.debitBalance)).addClass("selectable-cell").data({
            value: item.debitBalance,
            account: item.accountName,
            accountNumber: item.accountNumber
        });

        const creditCell = $("<td>").text(formatNumber(item.creditBalance)).addClass("selectable-cell").data({
            value: item.creditBalance,
            account: item.accountName,
            accountNumber: item.accountNumber
        });

        row.append(debitCell, creditCell);
        tbTbody.append(row);
    });

    trialBalanceTable.append(tbThead, tbTbody);
    $("#trialBalance .trialBalance-sheet-section .accordion-table-container").append(trialBalanceTable);

    // --------------------- Assets Table (5 columns) ---------------------
    const assetsTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const assetsThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Gross"),
            $("<th>").text("Amort"),
            $("<th>").text("Result")
        )
    );

    const assetsTbody = $("<tbody>");
    assetsData.forEach((item, index) => {
        const row = $("<tr>").attr("id", `asset_row_${index}`);
        row.append($("<td>").text(item.Reference));
        row.append($("<td>").text(item.Description));

        // Create interactive cells for gross and amort
        const grossCell = createEditableCell("gross", index);
        const amortCell = createEditableCell("amort", index);
        const resultCell = $("<td>").addClass("result-cell").attr("id", `asset_result_${index}`).text("0");

        row.append(grossCell, amortCell, resultCell);
        assetsTbody.append(row);
    });

    // Totals row for assets
    const assetTotalRow = $("<tr>").addClass("total-row").append(
        $("<td colspan='2'>").text("Total").addClass("fw-bold"),
        $("<td id='total_gross'>").text("0").addClass("fw-bold"),
        $("<td id='total_amort'>").text("0").addClass("fw-bold"),
        $("<td id='total_assets'>").text("0").addClass("fw-bold")
    );
    assetsTbody.append(assetTotalRow);

    assetsTable.append(assetsThead, assetsTbody);
    $(".balance-sheet-asset-section .accordion-table-container").append(assetsTable);

    // --------------------- Liabilities Table (3 columns) ---------------------
    const liabilitiesTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const liabilitiesThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Result")
        )
    );

    const liabilitiesTbody = $("<tbody>");
    liabilitiesData.forEach((item, index) => {
        const row = $("<tr>").attr("id", `liability_row_${index}`);
        row.append($("<td>").text(item.Reference));
        row.append($("<td>").text(item.Description));

        const resultCell = createEditableCell("liability", index);

        row.append(resultCell);
        liabilitiesTbody.append(row);
    });

    // Total row for liabilities
    const liabilityTotalRow = $("<tr>").addClass("total-row").append(
        $("<td colspan='2'>").text("Total").addClass("fw-bold"),
        $("<td id='total_liabilities'>").text("0").addClass("fw-bold")
    );
    liabilitiesTbody.append(liabilityTotalRow);

    liabilitiesTable.append(liabilitiesThead, liabilitiesTbody);
    $(".balance-sheet-liabilities-section .accordion-table-container").append(liabilitiesTable);

    // Add balance check status
    const balanceStatus = $("<div>").addClass("alert m-3").attr("id", "balance_status");
    $("#balanceSheet .balance-sheet-container").append(balanceStatus);

    // Helper function to format numbers as 0,###
    function formatNumber(num) {
        return new Intl.NumberFormat('en-US', {
            maximumFractionDigits: 0,
            useGrouping: true
        }).format(num || 0);
    }

    // Create and return an editable cell for the balance sheet
    function createEditableCell(type, index) {
        const cellId = `${type}_${index}`;
        const cell = $("<td>")
            .attr("id", cellId)
            .addClass("editable-cell")
            .text("0")
            .data({
                total: 0,
                accounts: {}  // Store accounts with their values
            });

        // Add click handler to select this cell as target
        cell.on("click", function (e) {
            // If the cell was already selected, show account removal dialog
            if (selectedTarget && selectedTarget.attr("id") === cellId) {
                showAccountRemovalDialog(cell);
            } else {
                $(".editable-cell").removeClass("selected-cell");
                $(this).addClass("selected-cell");
                selectedTarget = $(this);

                // Show visual feedback
                $("#status_message").remove();
                const statusMsg = $("<div>")
                    .attr("id", "status_message")
                    .addClass("alert alert-info m-3")
                    .text(`Selected ${type} cell. Click on Trial Balance entries to add values.`);
                $("#balanceSheet .accordion-body").prepend(statusMsg);
            }
            e.stopPropagation();
        });

        // Add right-click handler to show account details
        cell.on("contextmenu", function (e) {
            e.preventDefault();
            showAccountDetailsDialog(cell);
        });

        // Add hover effect to show account details
        cell.hover(
            function () {
                const accounts = $(this).data("accounts");
                if (Object.keys(accounts).length > 0) {
                    let tooltipContent = "Accounts:\n";
                    for (const accNum in accounts) {
                        tooltipContent += `${accNum} - ${accounts[accNum].name}: ${formatNumber(accounts[accNum].value)}\n`;
                    }
                    $(this).attr("title", tooltipContent);
                    $(this).tooltip({ html: true });
                    $(this).tooltip("show");
                }
            },
            function () {
                $(this).tooltip("hide");
            }
        );

        return cell;
    }

    // Function to display account details on right-click
    function showAccountDetailsDialog(cell) {
        // Remove any existing dialog
        $("#account_details_dialog").remove();

        const accounts = cell.data("accounts");
        if (Object.keys(accounts).length === 0) {
            return;
        }

        const dialog = $("<div>")
            .attr("id", "account_details_dialog")
            .addClass("card")
            .css({
                position: "absolute",
                top: cell.offset().top + cell.height() + "px",
                left: cell.offset().left + "px",
                zIndex: 1000,
                width: "300px"
            });

        const dialogHeader = $("<div>")
            .addClass("card-header")
            .text("Account Details - Click to Remove");

        const dialogBody = $("<div>").addClass("card-body p-0");
        const accountList = $("<ul>").addClass("list-group");

        for (const accNum in accounts) {
            const account = accounts[accNum];
            const listItem = $("<li>")
                .addClass("list-group-item")
                .text(`${accNum} - ${account.name}: ${formatNumber(account.value)}`)
                .css("cursor", "pointer")
                .data({
                    accountNumber: accNum,
                    value: account.value
                })
                .on("click", function () {
                    removeAccountFromCell(cell, $(this).data("accountNumber"), $(this).data("value"));
                    $("#account_details_dialog").remove();
                });

            accountList.append(listItem);
        }

        const closeBtn = $("<button>")
            .addClass("btn btn-sm btn-secondary mt-2")
            .text("Close")
            .on("click", function () {
                $("#account_details_dialog").remove();
            });

        dialogBody.append(accountList);
        dialog.append(dialogHeader, dialogBody, $("<div>").addClass("card-footer").append(closeBtn));

        $("body").append(dialog);

        // Close dialog when clicking outside
        $(document).on("click.accountDetailsDialog", function (e) {
            if (!$(e.target).closest("#account_details_dialog, #" + cell.attr("id")).length) {
                $("#account_details_dialog").remove();
                $(document).off("click.accountDetailsDialog");
            }
        });
    }

    // Function to display account removal dialog
    function showAccountRemovalDialog(cell) {
        // Remove any existing dialog
        $("#account_removal_dialog").remove();

        const accounts = cell.data("accounts");
        if (Object.keys(accounts).length === 0) {
            return;
        }

        const dialog = $("<div>")
            .attr("id", "account_removal_dialog")
            .addClass("card")
            .css({
                position: "absolute",
                top: cell.offset().top + cell.height() + "px",
                left: cell.offset().left + "px",
                zIndex: 1000,
                width: "300px"
            });

        const dialogHeader = $("<div>")
            .addClass("card-header")
            .text("Select account to remove");

        const dialogBody = $("<div>").addClass("card-body p-0");
        const accountList = $("<ul>").addClass("list-group");

        for (const accNum in accounts) {
            const account = accounts[accNum];
            const listItem = $("<li>")
                .addClass("list-group-item")
                .text(`${accNum} - ${account.name}: ${formatNumber(account.value)}`)
                .css("cursor", "pointer")
                .data({
                    accountNumber: accNum,
                    value: account.value
                })
                .on("click", function () {
                    removeAccountFromCell(cell, $(this).data("accountNumber"), $(this).data("value"));
                    $("#account_removal_dialog").remove();
                });

            accountList.append(listItem);
        }

        const closeBtn = $("<button>")
            .addClass("btn btn-sm btn-secondary mt-2")
            .text("Close")
            .on("click", function () {
                $("#account_removal_dialog").remove();
            });

        dialogBody.append(accountList);
        dialog.append(dialogHeader, dialogBody, $("<div>").addClass("card-footer").append(closeBtn));

        $("body").append(dialog);

        // Close dialog when clicking outside
        $(document).on("click.accountDialog", function (e) {
            if (!$(e.target).closest("#account_removal_dialog, #" + cell.attr("id")).length) {
                $("#account_removal_dialog").remove();
                $(document).off("click.accountDialog");
            }
        });
    }

    // Function to remove an account from a cell
    function removeAccountFromCell(cell, accountNumber, value) {
        const accounts = cell.data("accounts");
        if (accounts[accountNumber]) {
            let currentTotal = cell.data("total") || 0;
            currentTotal -= value;

            // Update cell data
            delete accounts[accountNumber];
            cell.data("accounts", accounts);
            cell.data("total", currentTotal);
            cell.text(formatNumber(currentTotal));

            // Mark the trial balance account as no longer used
            usedAccounts.delete(accountNumber);
            updateTrialBalanceDisplay();

            // Update calculations
            updateCalculations(cell);

            // Update row highlighting
            updateRowHighlighting();
        }
    }

    // Function to update the trial balance display (strikethrough used accounts)
    function updateTrialBalanceDisplay() {
        $(".selectable-cell").each(function () {
            const accountNumber = $(this).data("accountNumber");
            if (usedAccounts.has(accountNumber)) {
                $(this).addClass("used-account");
            } else {
                $(this).removeClass("used-account");
            }
        });
    }

    // Function to update row highlighting
    function updateRowHighlighting() {
        // Check asset rows
        $("tr[id^='asset_row_']").each(function () {
            const index = $(this).attr("id").split("_")[2];
            const gross = parseFloat($(`#gross_${index}`).text().replace(/,/g, '')) || 0;
            const amort = parseFloat($(`#amort_${index}`).text().replace(/,/g, '')) || 0;

            if (gross > 0 || amort > 0) {
                $(this).addClass("used-row");
            } else {
                $(this).removeClass("used-row");
            }
        });

        // Check liability rows
        $("tr[id^='liability_row_']").each(function () {
            const index = $(this).attr("id").split("_")[2];
            const value = parseFloat($(`#liability_${index}`).text().replace(/,/g, '')) || 0;

            if (value > 0) {
                $(this).addClass("used-row");
            } else {
                $(this).removeClass("used-row");
            }
        });
    }

    // --------------------- Click logic from Trial Balance to Balance Sheet ---------------------
    $(document).on("click", ".selectable-cell", function () {
        if (!selectedTarget) {
            alert("Please select a target cell in the Balance Sheet first");
            return;
        }

        // Check if this account is already used
        const sourceAccNum = $(this).data("accountNumber");
        if (usedAccounts.has(sourceAccNum)) {
            alert("This account has already been used in the balance sheet");
            return;
        }

        const sourceVal = parseFloat($(this).data("value")) || 0;
        if (sourceVal === 0) return; // Skip empty values

        const sourceAcc = $(this).data("account");

        let currentTotal = selectedTarget.data("total") || 0;
        let accounts = selectedTarget.data("accounts") || {};

        // Add the account
        accounts[sourceAccNum] = {
            name: sourceAcc,
            value: sourceVal
        };

        currentTotal += sourceVal;

        // Update the cell
        selectedTarget.data("total", currentTotal);
        selectedTarget.data("accounts", accounts);
        selectedTarget.text(formatNumber(currentTotal));

        // Mark account as used
        usedAccounts.add(sourceAccNum);
        updateTrialBalanceDisplay();

        // Update calculations
        updateCalculations(selectedTarget);

        // Update row highlighting
        updateRowHighlighting();
    });

    // Function to update all calculations based on changes
    function updateCalculations(changedCell) {
        const id = changedCell.attr("id");

        // Update specific result cell if gross or amort was changed
        if (id.startsWith("gross_") || id.startsWith("amort_")) {
            const index = id.split("_")[1];
            const gross = parseFloat($(`#gross_${index}`).text().replace(/,/g, '')) || 0;
            const amort = parseFloat($(`#amort_${index}`).text().replace(/,/g, '')) || 0;
            const result = gross - amort;

            $(`#asset_result_${index}`).text(formatNumber(result));
        }

        // Update totals
        calculateTotals();

        // Check if balance sheet balances
        checkBalance();
    }

    // Calculate all totals
    function calculateTotals() {
        // Asset totals
        let totalGross = 0;
        let totalAmort = 0;
        let totalAssets = 0;

        // Calculate gross total
        $("td[id^='gross_']").each(function () {
            totalGross += parseFloat($(this).text().replace(/,/g, '')) || 0;
        });

        // Calculate amortization total
        $("td[id^='amort_']").each(function () {
            totalAmort += parseFloat($(this).text().replace(/,/g, '')) || 0;
        });

        // Calculate asset results
        $("td[id^='asset_result_']").each(function () {
            totalAssets += parseFloat($(this).text().replace(/,/g, '')) || 0;
        });

        // Update asset totals
        $("#total_gross").text(formatNumber(totalGross));
        $("#total_amort").text(formatNumber(totalAmort));
        $("#total_assets").text(formatNumber(totalAssets));

        // Liability totals
        let totalLiabilities = 0;

        // Calculate liability total
        $("td[id^='liability_']").each(function () {
            totalLiabilities += parseFloat($(this).text().replace(/,/g, '')) || 0;
        });

        // Update liability total
        $("#total_liabilities").text(formatNumber(totalLiabilities));
    }

    // Check if assets equal liabilities
    function checkBalance() {
        const totalAssets = parseFloat($("#total_assets").text().replace(/,/g, '')) || 0;
        const totalLiabilities = parseFloat($("#total_liabilities").text().replace(/,/g, '')) || 0;
        const difference = Math.abs(totalAssets - totalLiabilities);

        const balanceStatus = $("#balance_status");

        if (difference < 0.01) { // Allow for small rounding errors
            balanceStatus.removeClass("alert-danger").addClass("alert-success")
                .text("✓ Balance Sheet is balanced: Assets = Liabilities");
        } else {
            balanceStatus.removeClass("alert-success").addClass("alert-danger")
                .text(`⚠ Balance Sheet is NOT balanced: Difference = ${formatNumber(difference)}`);
        }
    }

    // Add click handler to document to clear selection when clicking elsewhere
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".editable-cell, .selectable-cell, #account_removal_dialog, #account_details_dialog").length) {
            $(".editable-cell").removeClass("selected-cell");
            selectedTarget = null;
            $("#status_message").remove();
        }
    });

    // Add button to calculate P&L result (EP50/RP50)
    const calculatePLBtn = $('<button class="btn btn-outline-primary m-3" id="calculatePL"><i class="mdi mdi-calculator me-1"></i>Calculate P&L Result</button>');
    $("#balanceSheet .accordion-body").append(calculatePLBtn);

    // Calculate P&L result handler
    $("#calculatePL").on("click", function () {
        calculatePLResult();
    });

    // Function to calculate P&L result
    function calculatePLResult() {
        // Sum all credit balances of accounts starting with 7
        let credits7 = 0;
        let debits6 = 0;
        let accounts67 = [];

        // Process all accounts in trial balance
        trialBalanceData.forEach(item => {
            const accNum = item.accountNumber.toString();

            // Handle accounts starting with 7 (sum credits)
            if (accNum.startsWith('7')) {
                credits7 += parseFloat(item.creditBalance) || 0;
                accounts67.push(accNum);
            }

            // Handle accounts starting with 6 (sum debits)
            if (accNum.startsWith('6')) {
                debits6 += parseFloat(item.debitBalance) || 0;
                accounts67.push(accNum);
            }
        });

        // Calculate the difference
        const plResult = credits7 - debits6;

        // Find the relevant cells in either assets or liabilities
        let ep50Cell = null;
        let rp50Cell = null;

        // Look for EP50 and RP50 in assets and liabilities
        $("tr").each(function () {
            const firstCell = $(this).find("td:first-child");
            if (firstCell.text() === "EP50") {
                const index = $(this).attr("id").split("_")[2];
                ep50Cell = $(`#liability_${index}`);
            }
            if (firstCell.text() === "RP50") {
                const index = $(this).attr("id").split("_")[2];
                rp50Cell = $(`#liability_${index}`);
            }
        });

        // Reset both cells first
        if (ep50Cell) {
            ep50Cell.text("0").data({
                total: 0,
                accounts: {}
            });
        }

        if (rp50Cell) {
            rp50Cell.text("0").data({
                total: 0,
                accounts: {}
            });
        }

        // Place the result in the appropriate cell
        if (plResult > 0 && ep50Cell) {
            ep50Cell.text(formatNumber(plResult));
            ep50Cell.data("total", plResult);
            ep50Cell.data("accounts", { "P&L": { name: "Profit & Loss", value: plResult } });

            // Update calculations
            updateCalculations(ep50Cell);

            // Mark EP50 row as used
            ep50Cell.closest("tr").addClass("used-row");
        } else if (plResult < 0 && rp50Cell) {
            const absValue = Math.abs(plResult);
            rp50Cell.text(formatNumber(absValue));
            rp50Cell.data("total", absValue);
            rp50Cell.data("accounts", { "P&L": { name: "Profit & Loss", value: absValue } });

            // Update calculations
            updateCalculations(rp50Cell);

            // Mark RP50 row as used
            rp50Cell.closest("tr").addClass("used-row");
        }

        // Mark all accounts starting with 6 or 7 as used
        accounts67.forEach(accNum => {
            usedAccounts.add(accNum);
        });

        // Update trial balance display
        updateTrialBalanceDisplay();

        // Show confirmation message
        alert(`P&L calculated successfully. Result: ${formatNumber(plResult)}`);
    }

    // Add print and reset buttons
    if (!$("#printBalanceSheet").length) {
        const printBtn = $('<button class="btn btn-primary m-3" id="printBalanceSheet"><i class="mdi mdi-printer me-1"></i>Print Balance Sheet</button>');
        const resetBtn = $('<button class="btn btn-outline-secondary m-3" id="resetBalanceSheet"><i class="mdi mdi-refresh me-1"></i>Reset</button>');

        const btnContainer = $('<div class="d-flex justify-content-end"></div>').append(resetBtn, printBtn);
        $("#balanceSheet .accordion-body").append(btnContainer);

        // Print button handler
        $("#printBalanceSheet").on("click", function () {
            // Show loading indicator
            const loadingIndicator = $('<div id="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div><div class="mt-2">Generating report...</div></div>');
            $("body").append(loadingIndicator);

            // Collect balance sheet data into BalanceSheetAccount objects
            const balanceSheetData = collectBalanceSheetData();

            // Post data to backend for Crystal Report generation
            $.ajax({
                url: "/AccountingStatements/GenerateBSReport",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(balanceSheetData),
                success: function (response) {
                    // Remove loading indicator
                    $("#loading-overlay").remove();

                    // Handle the response - typically a URL to the generated report
                    if (response && response.reportUrl) {
                        // Open the report in a new window/tab
                        window.open(response.reportUrl, "_blank");
                    } else {
                        // Display the report in the current window
                        window.location.href = response.reportUrl || response;
                    }
                },
                error: function (xhr, status, error) {
                    // Remove loading indicator
                    $("#loading-overlay").remove();

                    // Show error message
                    alert("Error generating report: " + (xhr.responseJSON?.message || error || "Unknown error"));
                }
            });
        });

        // Function to collect balance sheet data
        function collectBalanceSheetData() {
            const balanceSheetAccounts = [];

            // Collect asset data - include ALL rows, even those with zero values
            $("tr[id^='asset_row_']").each(function () {
                const index = $(this).attr("id").split("_")[2];
                const reference = $(this).find("td:first-child").text();
                const description = $(this).find("td:nth-child(2)").text();
                const gross = parseFloat($(`#gross_${index}`).text().replace(/,/g, '')) || 0;
                const amort = parseFloat($(`#amort_${index}`).text().replace(/,/g, '')) || 0;
                const result = parseFloat($(`#asset_result_${index}`).text().replace(/,/g, '')) || 0;

                // Add all rows, including those with zero values
                balanceSheetAccounts.push({
                    Gross: gross,
                    Amort_Dep: amort,
                    Reference: reference,
                    Description: description,
                    Result: result,
                    Category: "Assets"
                });
            });

            // Collect liability data - include ALL rows, even those with zero values
            $("tr[id^='liability_row_']").each(function () {
                const index = $(this).attr("id").split("_")[2];
                const reference = $(this).find("td:first-child").text();
                const description = $(this).find("td:nth-child(2)").text();
                const result = parseFloat($(`#liability_${index}`).text().replace(/,/g, '')) || 0;

                // Add all rows, including those with zero values
                balanceSheetAccounts.push({
                    Gross: result, // For liabilities, we store the value in both Gross and Result
                    Amort_Dep: 0,
                    Reference: reference,
                    Description: description,
                    Result: result,
                    Category: "Liabilities"
                });
            });

            return balanceSheetAccounts;
        }

        // Reset button handler
        $("#resetBalanceSheet").on("click", function () {
            if (confirm("Are you sure you want to reset all values?")) {
                $(".editable-cell").each(function () {
                    $(this).text("0").data({
                        total: 0,
                        accounts: {}
                    });
                });

                $(".result-cell").text("0");
                $("#total_gross, #total_amort, #total_assets, #total_liabilities").text("0");

                // Clear used accounts
                usedAccounts.clear();
                updateTrialBalanceDisplay();

                // Clear row highlighting
                $("tr[id^='asset_row_'], tr[id^='liability_row_']").removeClass("used-row");

                checkBalance();
                selectedTarget = null;
                $(".editable-cell").removeClass("selected-cell");
                $("#status_message").remove();
            }
        });
    }

    // Apply some CSS for better UI
    $("<style>")
        .prop("type", "text/css")
        .html(`
            .selectable-cell { cursor: pointer; }
            .selectable-cell:hover { background-color: #f8f9fa; }
            .editable-cell { cursor: pointer; background-color: #f8f9fa; }
            .editable-cell:hover { background-color: #e9ecef; }
            .selected-cell { background-color: #cfe2ff !important; }
            .total-row { background-color: #f3f3f3; }
            .result-cell { background-color: #f3f3f3; }
            .used-account { text-decoration: line-through; color: #999; pointer-events: none; }
            .used-row { background-color: #e9ecef; }
            #loading-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-color: rgba(255, 255, 255, 0.8);
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                z-index: 9999;
            }
        `)
        .appendTo("head");

    // Initially show both sections
    $("#trialBalance").addClass("show");
    $("#balanceSheet").addClass("show");
}
// Builds the interactive interface for the balance sheet builder
function buildBuilderInterface0(trialBalanceData, assetsData, liabilitiesData) {
    clearBalanceSheetBuilder();

    let selectedTarget = null;
    let selectedAccounts = {}; // Track which accounts are used in each target cell
    let usedAccounts = new Set(); // Track which trial balance accounts have been used

    // --------------------- Trial Balance Table ---------------------
    const trialBalanceTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const tbThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Account Number"),
            $("<th>").text("Name"),
            $("<th>").text("Bal Dr"),
            $("<th>").text("Bal Cr")
        )
    );
    const tbTbody = $("<tbody>");

    trialBalanceData.forEach(item => {
        const row = $("<tr>");
        row.append($("<td>").text(item.accountNumber));
        row.append($("<td>").text(item.accountName));

        const debitCell = $("<td>").text(item.debitBalance).addClass("selectable-cell").data({
            value: item.debitBalance,
            account: item.accountName,
            accountNumber: item.accountNumber
        });

        const creditCell = $("<td>").text(item.creditBalance).addClass("selectable-cell").data({
            value: item.creditBalance,
            account: item.accountName,
            accountNumber: item.accountNumber
        });

        row.append(debitCell, creditCell);
        tbTbody.append(row);
    });

    trialBalanceTable.append(tbThead, tbTbody);
    $("#trialBalance .trialBalance-sheet-section .accordion-table-container").append(trialBalanceTable);

    // --------------------- Assets Table (5 columns) ---------------------
    const assetsTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const assetsThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Gross"),
            $("<th>").text("Amort"),
            $("<th>").text("Result")
        )
    );

    const assetsTbody = $("<tbody>");
    assetsData.forEach((item, index) => {
        const row = $("<tr>").attr("id", `asset_row_${index}`);
        row.append($("<td>").text(item.Reference));
        row.append($("<td>").text(item.Description));

        // Create interactive cells for gross and amort
        const grossCell = createEditableCell("gross", index);
        const amortCell = createEditableCell("amort", index);
        const resultCell = $("<td>").addClass("result-cell").attr("id", `asset_result_${index}`).text("0.00");

        row.append(grossCell, amortCell, resultCell);
        assetsTbody.append(row);
    });

    // Totals row for assets
    const assetTotalRow = $("<tr>").addClass("total-row").append(
        $("<td colspan='2'>").text("Total").addClass("fw-bold"),
        $("<td id='total_gross'>").text("0.00").addClass("fw-bold"),
        $("<td id='total_amort'>").text("0.00").addClass("fw-bold"),
        $("<td id='total_assets'>").text("0.00").addClass("fw-bold")
    );
    assetsTbody.append(assetTotalRow);

    assetsTable.append(assetsThead, assetsTbody);
    $(".balance-sheet-asset-section .accordion-table-container").append(assetsTable);

    // --------------------- Liabilities Table (3 columns) ---------------------
    const liabilitiesTable = $("<table>").addClass("table table-bordered table-sm mb-0");
    const liabilitiesThead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Result")
        )
    );

    const liabilitiesTbody = $("<tbody>");
    liabilitiesData.forEach((item, index) => {
        const row = $("<tr>").attr("id", `liability_row_${index}`);
        row.append($("<td>").text(item.Reference));
        row.append($("<td>").text(item.Description));

        const resultCell = createEditableCell("liability", index);

        row.append(resultCell);
        liabilitiesTbody.append(row);
    });

    // Total row for liabilities
    const liabilityTotalRow = $("<tr>").addClass("total-row").append(
        $("<td colspan='2'>").text("Total").addClass("fw-bold"),
        $("<td id='total_liabilities'>").text("0.00").addClass("fw-bold")
    );
    liabilitiesTbody.append(liabilityTotalRow);

    liabilitiesTable.append(liabilitiesThead, liabilitiesTbody);
    $(".balance-sheet-liabilities-section .accordion-table-container").append(liabilitiesTable);

    // Add balance check status
    const balanceStatus = $("<div>").addClass("alert m-3").attr("id", "balance_status");
    $("#balanceSheet .balance-sheet-container").append(balanceStatus);

    // Create and return an editable cell for the balance sheet
    function createEditableCell(type, index) {
        const cellId = `${type}_${index}`;
        const cell = $("<td>")
            .attr("id", cellId)
            .addClass("editable-cell")
            .text("0.00")
            .data({
                total: 0,
                accounts: {}  // Store accounts with their values
            });

        // Add click handler to select this cell as target
        cell.on("click", function (e) {
            // If the cell was already selected, show account removal dialog
            if (selectedTarget && selectedTarget.attr("id") === cellId) {
                showAccountRemovalDialog(cell);
            } else {
                $(".editable-cell").removeClass("selected-cell");
                $(this).addClass("selected-cell");
                selectedTarget = $(this);

                // Show visual feedback
                $("#status_message").remove();
                const statusMsg = $("<div>")
                    .attr("id", "status_message")
                    .addClass("alert alert-info m-3")
                    .text(`Selected ${type} cell. Click on Trial Balance entries to add values.`);
                $("#balanceSheet .accordion-body").prepend(statusMsg);
            }
            e.stopPropagation();
        });

        // Add right-click handler to show account details
        cell.on("contextmenu", function (e) {
            e.preventDefault();
            showAccountDetailsDialog(cell);
        });

        // Add hover effect to show account details
        cell.hover(
            function () {
                const accounts = $(this).data("accounts");
                if (Object.keys(accounts).length > 0) {
                    let tooltipContent = "Accounts:<br>";
                    for (const accNum in accounts) {
                        tooltipContent += `${accNum} - ${accounts[accNum].name}: ${accounts[accNum].value.toFixed(2)}<br>`;
                    }
                    $(this).attr("title", tooltipContent);
                    $(this).tooltip({ html: true });
                    $(this).tooltip("show");
                }
            },
            function () {
                $(this).tooltip("hide");
            }
        );

        return cell;
    }

    // Function to display account details on right-click
    function showAccountDetailsDialog(cell) {
        // Remove any existing dialog
        $("#account_details_dialog").remove();

        const accounts = cell.data("accounts");
        if (Object.keys(accounts).length === 0) {
            return;
        }

        const dialog = $("<div>")
            .attr("id", "account_details_dialog")
            .addClass("card")
            .css({
                position: "absolute",
                top: cell.offset().top + cell.height() + "px",
                left: cell.offset().left + "px",
                zIndex: 1000,
                width: "300px"
            });

        const dialogHeader = $("<div>")
            .addClass("card-header")
            .text("Account Details - Click to Remove");

        const dialogBody = $("<div>").addClass("card-body p-0");
        const accountList = $("<ul>").addClass("list-group");

        for (const accNum in accounts) {
            const account = accounts[accNum];
            const listItem = $("<li>")
                .addClass("list-group-item")
                .text(`${accNum} - ${account.name}: ${account.value.toFixed(2)}`)
                .css("cursor", "pointer")
                .data({
                    accountNumber: accNum,
                    value: account.value
                })
                .on("click", function () {
                    removeAccountFromCell(cell, $(this).data("accountNumber"), $(this).data("value"));
                    $("#account_details_dialog").remove();
                });

            accountList.append(listItem);
        }

        const closeBtn = $("<button>")
            .addClass("btn btn-sm btn-secondary mt-2")
            .text("Close")
            .on("click", function () {
                $("#account_details_dialog").remove();
            });

        dialogBody.append(accountList);
        dialog.append(dialogHeader, dialogBody, $("<div>").addClass("card-footer").append(closeBtn));

        $("body").append(dialog);

        // Close dialog when clicking outside
        $(document).on("click.accountDetailsDialog", function (e) {
            if (!$(e.target).closest("#account_details_dialog, #" + cell.attr("id")).length) {
                $("#account_details_dialog").remove();
                $(document).off("click.accountDetailsDialog");
            }
        });
    }

    // Function to display account removal dialog
    function showAccountRemovalDialog(cell) {
        // Remove any existing dialog
        $("#account_removal_dialog").remove();

        const accounts = cell.data("accounts");
        if (Object.keys(accounts).length === 0) {
            return;
        }

        const dialog = $("<div>")
            .attr("id", "account_removal_dialog")
            .addClass("card")
            .css({
                position: "absolute",
                top: cell.offset().top + cell.height() + "px",
                left: cell.offset().left + "px",
                zIndex: 1000,
                width: "300px"
            });

        const dialogHeader = $("<div>")
            .addClass("card-header")
            .text("Select account to remove");

        const dialogBody = $("<div>").addClass("card-body p-0");
        const accountList = $("<ul>").addClass("list-group");

        for (const accNum in accounts) {
            const account = accounts[accNum];
            const listItem = $("<li>")
                .addClass("list-group-item")
                .text(`${accNum} - ${account.name}: ${account.value.toFixed(2)}`)
                .css("cursor", "pointer")
                .data({
                    accountNumber: accNum,
                    value: account.value
                })
                .on("click", function () {
                    removeAccountFromCell(cell, $(this).data("accountNumber"), $(this).data("value"));
                    $("#account_removal_dialog").remove();
                });

            accountList.append(listItem);
        }

        const closeBtn = $("<button>")
            .addClass("btn btn-sm btn-secondary mt-2")
            .text("Close")
            .on("click", function () {
                $("#account_removal_dialog").remove();
            });

        dialogBody.append(accountList);
        dialog.append(dialogHeader, dialogBody, $("<div>").addClass("card-footer").append(closeBtn));

        $("body").append(dialog);

        // Close dialog when clicking outside
        $(document).on("click.accountDialog", function (e) {
            if (!$(e.target).closest("#account_removal_dialog, #" + cell.attr("id")).length) {
                $("#account_removal_dialog").remove();
                $(document).off("click.accountDialog");
            }
        });
    }

    // Function to remove an account from a cell
    function removeAccountFromCell(cell, accountNumber, value) {
        const accounts = cell.data("accounts");
        if (accounts[accountNumber]) {
            let currentTotal = cell.data("total") || 0;
            currentTotal -= value;

            // Update cell data
            delete accounts[accountNumber];
            cell.data("accounts", accounts);
            cell.data("total", currentTotal);
            cell.text(currentTotal.toFixed(2));

            // Mark the trial balance account as no longer used
            usedAccounts.delete(accountNumber);
            updateTrialBalanceDisplay();

            // Update calculations
            updateCalculations(cell);

            // Update row highlighting
            updateRowHighlighting();
        }
    }

    // Function to update the trial balance display (strikethrough used accounts)
    function updateTrialBalanceDisplay() {
        $(".selectable-cell").each(function () {
            const accountNumber = $(this).data("accountNumber");
            if (usedAccounts.has(accountNumber)) {
                $(this).addClass("used-account");
            } else {
                $(this).removeClass("used-account");
            }
        });
    }

    // Function to update row highlighting
    function updateRowHighlighting() {
        // Check asset rows
        $("tr[id^='asset_row_']").each(function () {
            const index = $(this).attr("id").split("_")[2];
            const gross = parseFloat($(`#gross_${index}`).text()) || 0;
            const amort = parseFloat($(`#amort_${index}`).text()) || 0;

            if (gross > 0 || amort > 0) {
                $(this).addClass("used-row");
            } else {
                $(this).removeClass("used-row");
            }
        });

        // Check liability rows
        $("tr[id^='liability_row_']").each(function () {
            const index = $(this).attr("id").split("_")[2];
            const value = parseFloat($(`#liability_${index}`).text()) || 0;

            if (value > 0) {
                $(this).addClass("used-row");
            } else {
                $(this).removeClass("used-row");
            }
        });
    }

    // --------------------- Click logic from Trial Balance to Balance Sheet ---------------------
    $(document).on("click", ".selectable-cell", function () {
        if (!selectedTarget) {
            alert("Please select a target cell in the Balance Sheet first");
            return;
        }

        // Check if this account is already used
        const sourceAccNum = $(this).data("accountNumber");
        if (usedAccounts.has(sourceAccNum)) {
            alert("This account has already been used in the balance sheet");
            return;
        }

        const sourceVal = parseFloat($(this).data("value")) || 0;
        if (sourceVal === 0) return; // Skip empty values

        const sourceAcc = $(this).data("account");

        let currentTotal = selectedTarget.data("total") || 0;
        let accounts = selectedTarget.data("accounts") || {};

        // Add the account
        accounts[sourceAccNum] = {
            name: sourceAcc,
            value: sourceVal
        };

        currentTotal += sourceVal;

        // Update the cell
        selectedTarget.data("total", currentTotal);
        selectedTarget.data("accounts", accounts);
        selectedTarget.text(currentTotal.toFixed(2));

        // Mark account as used
        usedAccounts.add(sourceAccNum);
        updateTrialBalanceDisplay();

        // Update calculations
        updateCalculations(selectedTarget);

        // Update row highlighting
        updateRowHighlighting();
    });

    // Function to update all calculations based on changes
    function updateCalculations(changedCell) {
        const id = changedCell.attr("id");

        // Update specific result cell if gross or amort was changed
        if (id.startsWith("gross_") || id.startsWith("amort_")) {
            const index = id.split("_")[1];
            const gross = parseFloat($(`#gross_${index}`).text()) || 0;
            const amort = parseFloat($(`#amort_${index}`).text()) || 0;
            const result = gross - amort;

            $(`#asset_result_${index}`).text(result.toFixed(2));
        }

        // Update totals
        calculateTotals();

        // Check if balance sheet balances
        checkBalance();
    }

    // Calculate all totals
    function calculateTotals() {
        // Asset totals
        let totalGross = 0;
        let totalAmort = 0;
        let totalAssets = 0;

        // Calculate gross total
        $("td[id^='gross_']").each(function () {
            totalGross += parseFloat($(this).text()) || 0;
        });

        // Calculate amortization total
        $("td[id^='amort_']").each(function () {
            totalAmort += parseFloat($(this).text()) || 0;
        });

        // Calculate asset results
        $("td[id^='asset_result_']").each(function () {
            totalAssets += parseFloat($(this).text()) || 0;
        });

        // Update asset totals
        $("#total_gross").text(totalGross.toFixed(2));
        $("#total_amort").text(totalAmort.toFixed(2));
        $("#total_assets").text(totalAssets.toFixed(2));

        // Liability totals
        let totalLiabilities = 0;

        // Calculate liability total
        $("td[id^='liability_']").each(function () {
            totalLiabilities += parseFloat($(this).text()) || 0;
        });

        // Update liability total
        $("#total_liabilities").text(totalLiabilities.toFixed(2));
    }

    // Check if assets equal liabilities
    function checkBalance() {
        const totalAssets = parseFloat($("#total_assets").text()) || 0;
        const totalLiabilities = parseFloat($("#total_liabilities").text()) || 0;
        const difference = Math.abs(totalAssets - totalLiabilities);

        const balanceStatus = $("#balance_status");

        if (difference < 0.01) { // Allow for small rounding errors
            balanceStatus.removeClass("alert-danger").addClass("alert-success")
                .text("✓ Balance Sheet is balanced: Assets = Liabilities");
        } else {
            balanceStatus.removeClass("alert-success").addClass("alert-danger")
                .text(`⚠ Balance Sheet is NOT balanced: Difference = ${difference.toFixed(2)}`);
        }
    }

    // Add click handler to document to clear selection when clicking elsewhere
    $(document).on("click", function (e) {
        if (!$(e.target).closest(".editable-cell, .selectable-cell, #account_removal_dialog, #account_details_dialog").length) {
            $(".editable-cell").removeClass("selected-cell");
            selectedTarget = null;
            $("#status_message").remove();
        }
    });

    // Add button to calculate P&L result (EP50/RP50)
    const calculatePLBtn = $('<button class="btn btn-outline-primary m-3" id="calculatePL"><i class="mdi mdi-calculator me-1"></i>Calculate P&L Result</button>');
    $("#balanceSheet .accordion-body").append(calculatePLBtn);

    // Calculate P&L result handler
    $("#calculatePL").on("click", function () {
        calculatePLResult();
    });

    // Function to calculate P&L result
    function calculatePLResult() {
        // Sum all credit balances of accounts starting with 7
        let credits7 = 0;
        let debits6 = 0;
        let accounts67 = [];

        // Process all accounts in trial balance
        trialBalanceData.forEach(item => {
            const accNum = item.accountNumber.toString();

            // Handle accounts starting with 7 (sum credits)
            if (accNum.startsWith('7')) {
                credits7 += parseFloat(item.creditBalance) || 0;
                accounts67.push(accNum);
            }

            // Handle accounts starting with 6 (sum debits)
            if (accNum.startsWith('6')) {
                debits6 += parseFloat(item.debitBalance) || 0;
                accounts67.push(accNum);
            }
        });

        // Calculate the difference
        const plResult = credits7 - debits6;

        // Find the relevant cells in either assets or liabilities
        let ep50Cell = null;
        let rp50Cell = null;

        // Look for EP50 and RP50 in assets and liabilities
        $("tr").each(function () {
            const firstCell = $(this).find("td:first-child");
            if (firstCell.text() === "EP50") {
                const index = $(this).attr("id").split("_")[2];
                ep50Cell = $(`#liability_${index}`);
            }
            if (firstCell.text() === "RP50") {
                const index = $(this).attr("id").split("_")[2];
                rp50Cell = $(`#liability_${index}`);
            }
        });

        // Reset both cells first
        if (ep50Cell) {
            ep50Cell.text("0.00").data({
                total: 0,
                accounts: {}
            });
        }

        if (rp50Cell) {
            rp50Cell.text("0.00").data({
                total: 0,
                accounts: {}
            });
        }

        // Place the result in the appropriate cell
        if (plResult > 0 && ep50Cell) {
            ep50Cell.text(plResult.toFixed(2));
            ep50Cell.data("total", plResult);
            ep50Cell.data("accounts", { "P&L": { name: "Profit & Loss", value: plResult } });

            // Update calculations
            updateCalculations(ep50Cell);

            // Mark EP50 row as used
            ep50Cell.closest("tr").addClass("used-row");
        } else if (plResult < 0 && rp50Cell) {
            const absValue = Math.abs(plResult);
            rp50Cell.text(absValue.toFixed(2));
            rp50Cell.data("total", absValue);
            rp50Cell.data("accounts", { "P&L": { name: "Profit & Loss", value: absValue } });

            // Update calculations
            updateCalculations(rp50Cell);

            // Mark RP50 row as used
            rp50Cell.closest("tr").addClass("used-row");
        }

        // Mark all accounts starting with 6 or 7 as used
        accounts67.forEach(accNum => {
            usedAccounts.add(accNum);
        });

        // Update trial balance display
        updateTrialBalanceDisplay();

        // Show confirmation message
        alert(`P&L calculated successfully. Result: ${plResult.toFixed(2)}`);
    }

    // Add print and reset buttons
    if (!$("#printBalanceSheet").length) {
        const printBtn = $('<button class="btn btn-primary m-3" id="printBalanceSheet"><i class="mdi mdi-printer me-1"></i>Print Balance Sheet</button>');
        const resetBtn = $('<button class="btn btn-outline-secondary m-3" id="resetBalanceSheet"><i class="mdi mdi-refresh me-1"></i>Reset</button>');

        const btnContainer = $('<div class="d-flex justify-content-end"></div>').append(resetBtn, printBtn);
        $("#balanceSheet .accordion-body").append(btnContainer);

        // Print button handler
        $("#printBalanceSheet").on("click", function () {
            // Show loading indicator
            const loadingIndicator = $('<div id="loading-overlay"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div><div class="mt-2">Generating report...</div></div>');
            $("body").append(loadingIndicator);

         var balanceSheetData=  collectBalanceSheetData();
            // Collect balance sheet data into BalanceSheetAccount objects
            console.log(balanceSheetData);
            // Post data to backend for Crystal Report generation
            $.ajax({
                url: "/AccountingStatements/GenerateBSReport",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(balanceSheetData),
                success: function (response) {
                    // Remove loading indicator
                    $("#loading-overlay").remove();

                    // Handle the response - typically a URL to the generated report
                    if (response && response.reportUrl) {
                        // Open the report in a new window/tab
                        window.open(response.reportUrl, "_blank");
                    } else {
                        // Display the report in the current window
                        window.location.href = response.reportUrl || response;
                    }
                },
                error: function (xhr, status, error) {
                    // Remove loading indicator
                    $("#loading-overlay").remove();

                    // Show error message
                    alert("Error generating report: " + (xhr.responseJSON?.message || error || "Unknown error"));
                }
            });
        });

        // Function to collect balance sheet data
        function collectBalanceSheetData() {
            const balanceSheetAccounts = [];

            // Collect asset data
            $("tr[id^='asset_row_']").each(function () {
                const index = $(this).attr("id").split("_")[2];
                const reference = $(this).find("td:first-child").text();
                const description = $(this).find("td:nth-child(2)").text();
                const gross = parseFloat($(`#gross_${index}`).text()) || 0;
                const amort = parseFloat($(`#amort_${index}`).text()) || 0;
                const result = parseFloat($(`#asset_result_${index}`).text()) || 0;

                // Only add rows with values
                if (gross > 0 || amort > 0) {
                    balanceSheetAccounts.push({
                        Gross: gross,
                        Amort_Dep: amort,
                        Reference: reference,
                        Description: description,
                        Result: result,
                        Category: "Assets"
                    });
                }
            });

            // Collect liability data
            $("tr[id^='liability_row_']").each(function () {
                const index = $(this).attr("id").split("_")[2];
                const reference = $(this).find("td:first-child").text();
                const description = $(this).find("td:nth-child(2)").text();
                const result = parseFloat($(`#liability_${index}`).text()) || 0;

                // Only add rows with values
                if (result > 0) {
                    balanceSheetAccounts.push({
                        Gross: result, // For liabilities, we store the value in both Gross and Result
                        Amort_Dep: 0,
                        Reference: reference,
                        Description: description,
                        Result: result,
                        Category: "Liabilities"
                    });
                }
            });

            return balanceSheetAccounts;
        }

        // Reset button handler
        $("#resetBalanceSheet").on("click", function () {
            if (confirm("Are you sure you want to reset all values?")) {
                $(".editable-cell").each(function () {
                    $(this).text("0.00").data({
                        total: 0,
                        accounts: {}
                    });
                });

                $(".result-cell").text("0.00");
                $("#total_gross, #total_amort, #total_assets, #total_liabilities").text("0.00");

                // Clear used accounts
                usedAccounts.clear();
                updateTrialBalanceDisplay();

                // Clear row highlighting
                $("tr[id^='asset_row_'], tr[id^='liability_row_']").removeClass("used-row");

                checkBalance();
                selectedTarget = null;
                $(".editable-cell").removeClass("selected-cell");
                $("#status_message").remove();
            }
        });
    }

    // Apply some CSS for better UI
    $("<style>")
        .prop("type", "text/css")
        .html(`
            .selectable-cell { cursor: pointer; }
            .selectable-cell:hover { background-color: #f8f9fa; }
            .editable-cell { cursor: pointer; background-color: #f8f9fa; }
            .editable-cell:hover { background-color: #e9ecef; }
            .selected-cell { background-color: #cfe2ff !important; }
            .total-row { background-color: #f3f3f3; }
            .result-cell { background-color: #f3f3f3; }
            .used-account { text-decoration: line-through; color: #999; pointer-events: none; }
            .used-row { background-color: #e9ecef; }
            #loading-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-color: rgba(255, 255, 255, 0.8);
                display: flex;
                flex-direction: column;
                justify-content: center;
                align-items: center;
                z-index: 9999;
            }
        `)
        .appendTo("head");

    // Initially show both sections
    $("#trialBalance").addClass("show");
    $("#balanceSheet").addClass("show");
}

// Example data for testing
function loadExampleData() {
    const trialBalanceData = [
        { accountNumber: "1000", accountName: "Cash", debitBalance: 50000, creditBalance: 0 },
        { accountNumber: "1100", accountName: "Accounts Receivable", debitBalance: 25000, creditBalance: 0 },
        { accountNumber: "1200", accountName: "Inventory", debitBalance: 30000, creditBalance: 0 },
        { accountNumber: "1300", accountName: "Equipment", debitBalance: 80000, creditBalance: 0 },
        { accountNumber: "1310", accountName: "Accumulated Depreciation", debitBalance: 0, creditBalance: 15000 },
        { accountNumber: "1400", accountName: "Building", debitBalance: 200000, creditBalance: 0 },
        { accountNumber: "1410", accountName: "Accumulated Depreciation - Building", debitBalance: 0, creditBalance: 25000 },
        { accountNumber: "2000", accountName: "Accounts Payable", debitBalance: 0, creditBalance: 35000 },
        { accountNumber: "2100", accountName: "Notes Payable", debitBalance: 0, creditBalance: 50000 },
        { accountNumber: "2200", accountName: "Long-term Debt", debitBalance: 0, creditBalance: 120000 },
        { accountNumber: "3000", accountName: "Common Stock", debitBalance: 0, creditBalance: 100000 },
        { accountNumber: "3100", accountName: "Retained Earnings", debitBalance: 0, creditBalance: 40000 }
    ];

    const assetsData = [
        { Reference: "A1", Description: "Current Assets" },
        { Reference: "A2", Description: "Property, Plant & Equipment" },
        { Reference: "A3", Description: "Intangible Assets" },
        { Reference: "A4", Description: "Other Assets" }
    ];

    const liabilitiesData = [
        { Reference: "L1", Description: "Current Liabilities" },
        { Reference: "L2", Description: "Long-term Liabilities" },
        { Reference: "E1", Description: "Equity" }
    ];

    buildBuilderInterface(trialBalanceData, assetsData, liabilitiesData);
}

// Uncomment to test with example data
// $(document).ready(function() {
//     loadExampleData();
// });



 
/*Add on this for the user friendly expirience.

For the trial balance strike the trial balance Debit or Credit balance when is already used and make the cell red

For the balance sheet  Let the rows of balance sheet
1 Highlight the cell on the balance sheet that is currently by treated with a light yellow color.
2 Change the row color to light green when is already treated

Let display on hover be more friendly like this
[EP01:Members share]
------------------------
    1.AccountName[1200000]
2.AccountName[23000000]

Also Makesure that a cell on the trial balance should not used more than one time on your balance sheet.  

When click on a cell already processed i can see the dropdown list of cells already added and i scroll to an item an delete the item and the appropriate reverse operation takes place. 
*/

function LoadAccountDataSetDT(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [
            //{ "data": "ReferenceId", "name": "ReferenceId", "autoWidth": true },
            //{ "data": "Amount", "name": "Amount", "autoWidth": true },
            //{ "data": "RequestMessage", "name": "RequestMessage", "autoWidth": true },
            //{ "data": "IssuedBy", "name": "IssuedBy", "autoWidth": true },
            //{ "data": "IssuedDate", "name": "IssuedDate", "autoWidth": true },
            //{
            //    "data": "Id", "orderable": "false", "render": function (data) {
            //        return "<a href='/CashFlowManagement/GetCashReplenimentRequest?KEY=" + data + " class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Click to Approve</a>";
            //    }
            //}
        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "20%" }

           

        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}

function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("AccountingStatements", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}
function populateTrialBalance(data) {
    const table = $("<table>").addClass("table table-striped table-bordered table-sm mb-0");
    const thead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Account Number"),
            $("<th>").text("Name"),
            $("<th>").text("Bal Dr"),
            $("<th>").text("Bal Cr")
        )
    );
    const tbody = $("<tbody>");
    data.forEach(row => {
        tbody.append(
            $("<tr>").append(
                $("<td>").text(row.accountNumber),
                $("<td>").text(row.name),
                $("<td>").text(row.balDr),
                $("<td>").text(row.balCr)
            )
        );
    });
    table.append(thead, tbody);
    $(".trialBalance-sheet-container .accordion-table-container").append(table);
}

function populateAssets(data) {
    const table = $("<table>").addClass("table table-striped table-bordered table-sm mb-0");
    const thead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Gross"),
            $("<th>").text("Amort"),
            $("<th>").text("Result")
        )
    );
    const tbody = $("<tbody>");
    data.forEach(row => {
        tbody.append(
            $("<tr>").append(
                $("<td>").text(row.code),
                $("<td>").text(row.narration),
                $("<td>").text(row.gross),
                $("<td>").text(row.amort),
                $("<td>").text(row.result)
            )
        );
    });
    $(".balance-sheet-container .col-md-6:first .accordion-table-container").append(table);
}

function populateLiabilities(data) {
    const table = $("<table>").addClass("table table-striped table-bordered table-sm mb-0");
    const thead = $("<thead>").append(
        $("<tr>").append(
            $("<th>").text("Code"),
            $("<th>").text("Narration"),
            $("<th>").text("Result")
        )
    );
    const tbody = $("<tbody>");
    data.forEach(row => {
        tbody.append(
            $("<tr>").append(
                $("<td>").text(row.code),
                $("<td>").text(row.narration),
                $("<td>").text(row.result)
            )
        );
    });
    $(".balance-sheet-container .col-md-6:last .accordion-table-container").append(table);
}
 
function getReportTitle(reportType) {
    if ((reportType === "TB6") || (reportType === "TB4")) {
        return "TRIAL BALANCE REPORT";
    } else if (reportType === "GL") {
        return "GENERAL LEDGER REPORT";
    } else if (reportType === "LL") {
        return "LIAISON LEDGER REPORT";
    } else if (reportType === "JE") {
        return "JOURNAL ENTRY REPORT";
    }
    // You can add more conditions here for other report types
    return reportType; // Return the original report type if no match
}

function AjaxPostSearch(form) {

    var model = CollectionOfData();

    console.log(model);

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {

        console.log(form);

        alertify.confirm("TRUST SOFT CREDIT", "Are you sure you want to  generate " + getReportTitle(model.ReportType) + " for the period of " + $("input[name='SystemQuery.FromDate']").val() + " to " + $("input[name='SystemQuery.ToDate']").val() + " !!! ",
            function () {


                var ajaxConfig = {
                    type: 'POST',
                    url: "/AccountingStatements/PostSearch",
                    data: new FormData(form),
                    success: function (response) {
      
                        appalert("Report [" + getReportTitle(model.ReportType) + "] has been generated successfully", 1, 1);
                        if (model.FileType.toLowerCase() === "pdf")
                        {
                            if (response.success)
                            {
                                openReportWindow(model.FileType, model.ReportType);
                                LoadDataGen('AccountingStatements', 'myDataTable', '_DownloadedReportData', 'DESC', 'datalistingview', 'xxx', 'Reports', 'list');
                            } else {
                                appalert(response.message, 0, 1);
                            }

                        } else {
                            LoadDataGen('AccountingStatements', 'myDataTable', '_DownloadedReportData', 'DESC', 'datalistingview', 'xxx', 'Reports', 'list');
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

function DeleteRecordPage(controller, KEY, partialview)
{
    console.log(controller + " " + KEY + " " + partialview);
    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?id=" + KEY;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        console.log(response);
                         LoadDataGen(controller, 'myDataTable', partialview, null, null, null, null, 'list');
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }

                }, error: function (err) {

                    appalert(err.statusText, 3, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);

        });


}


function formatDate(dateString) {
/*    if (!dateString) return 'Not defined';*/

    // Check if the date string is in the /Date(ticks)/ format
    const ticksRegex = /^\/Date\((-?\d+)\)\/$/;
    const match = dateString.match(ticksRegex);

    if (match) {
        // Convert ticks to milliseconds and create a Date object
        const ticks = parseInt(match[1], 10);
        const date = new Date(ticks);

        // Format the date
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');

        return `${day}-${month}-${year}`;
    } else {
        // If the date string is not in the /Date(ticks)/ format, treat it as a regular date string
        const date = new Date(dateString);
        if (isNaN(date.getTime())) {
            return 'Not defined';
        }

        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');

        return `${day}-${month}-${year}`;
    }
}

function DownLoadGL(FileType) {
    var branchId = $('#selectedForJEBranchId').val();
    var branchName = $('#selectedForJEBranchName').val();
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingStatements/GenerateGLByBranchId',
        type: 'Get',
        dataType: 'json',
        data: { branchId: branchId, fileType: FileType },
        success: function (data) {
            console.log(data);
            // Clear existing options in the OperationEventAttributeId combo
            appalert("GeneralLedger for " + branchName +" was created successfully" , 1, 1);
            if (FileType === "EXCEL") {
               
                window.open("/Reports/PrintAccountLedgerDtoInExcel", "_blank");
               
            } else {
                window.open("/Reports/DownloadExcelFile", "_blank");
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}


function DownLoadGLByAccount(FileType) {
    var branchId = $('#selectedForJEBranchId').val();
    var branchName = $('#selectedForJEBranchName').val();
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingStatements/GenerateGLByBranchId',
        type: 'Get',
        dataType: 'json',
        data: { branchId: branchId, fileType: FileType },
        success: function (data) {
            console.log(data);
            // Clear existing options in the OperationEventAttributeId combo
            appalert("GeneralLedger for " + branchName + " was created successfully", 1, 1);
            if (FileType === "EXCEL")
            {

                window.open("/Reports/PrintGeneralLedgerOfAccount", "_blank");

            } else {
                window.open("/Reports/DownloadExcelFile", "_blank");
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function LoadDownloadedReportByUser(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [
           
        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "10%" },
        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}

function DownLoadJE(fileType) {
    var BranchId = $('#selectedBranchId').val();
    var dateFrom = $('#selectedDateFromForJE').val();
    var dateTo = $('#selectedDateToForJE').val();
    console.log(BranchId +" - "+ dateFrom +" - "+ dateTo)
        $.ajax({
        url: '/AccountingStatements/GenerateJEByBranchId',
        type: 'Get',
        dataType: 'json',
            data: { branchId: BranchId, fileType: fileType, DateFrom: dateFrom, DateTo:dateTo },
        success: function (data) {
            console.log(data);
 
            appalert("Journal Entry for BranchId:" + BranchId + " was created successfully", 3, 1);
            if (fileType === "EXCEL") {

                window.open("/Reports/PrintJournalEntryDtoInExcel", "_blank");

            } else {
                window.open("/Reports/DownloadExcelFile", "_blank");
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function DownLoadLL(fileType) {
    var BranchId = $('#selectedBranchId').val();
    var BranchName = $('#selectedBranchName').val();
 
    console.log(BranchId + " - " + BranchName )
    $.ajax({
        url: '/AccountingStatements/GenerateLiaisonLedgerByBranchId',
        type: 'Get',
        dataType: 'json',
        data: { branchId: BranchId, fileType },
        success: function (data) {
            console.log(data);

            appalert("Liaison Ledger for :" + BranchName + " was created successfully", 3, 1);
            if (fileType === "EXCEL") {

                window.open("/Reports/DownloadExcelFile", "_blank");

            } else {
                window.open("/Reports/DownloadExcelFile", "_blank");
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function LoadLiaionLedgerByBranchID(branchId) {
    console.log(branchId);
    $.ajax({
        url: '/AccountingStatements/LiaisonLedgerPerBranch',
        type: 'Get',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            /* $('#largeModal').hide();*/
            $('#LiaisonLedgerHeading').empty();
            var description = $("#" + branchId + "-Name").text();
            var names = "Liaison Ledger for " + description + ".";
            console.log(names);
            $("#LiaisonLedgerHeading").text(names);
            // Append text to the modal title
            //$('#LiaisonLedgerHeading').append(names);
            var table;
            initializeDataTableForLiaisonLedger(data);

            $('#selectedBranchId').val(branchId);
            $('#').val(description);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function loadBranchGeneralLedgerByBranchId(branchId) {
    console.log(branchId);

    $.ajax({
        url: '/AccountingStatements/GetBranchAccounts',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {

            $('#exampleModalLabel3').empty();
            var description = $("#" + branchId + "-Name").text();
 
            // Append text to the modal title
            $('#exampleModalLabel3').append('General Ledger For ' + description);
            var table ;
            /** Populate the table with the fetched data*/
      
            initializeDataTableForGL(data);
    


       
            $('#selectedForJEBranchId').val(branchId);
            $('#selectedForJEBranchName').val(description);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function LoadJournalEntryByBranchID() {
    var branchId = $('#selectedForJEBranchId').val();
 
    const $fromDate = $('#FromDate');
    const $toDate = $('#ToDate');
    console.log(branchId + $fromDate.val() + $toDate.val());
    $.ajax({
        url: '/AccountingStatements/JournalEntriesPerBranch',
        type: 'Post',
        dataType: 'json',
        data: { branchId: branchId, toDate: $toDate.val(), fromDate: $fromDate.val() },
        success: function (data) {
           /* $('#largeModal').hide();*/
            $('#JournalEntryHeading').empty();
            var description = $("#" + branchId + "-Name").text();
            var names = description +"   Journal Entries From " + $fromDate.val() + " To " + $toDate.val() + ".";
            console.log(names);
            $("#JournalEntryHeading").text(names);
            // Append text to the modal title
            $('#exampleModalLabel3').append('JournalEntries ' + description);
            var table;
            initializeDataTableForJE(data);
    
            $('#selectedBranchId').val(branchId);
            $('#selectedDateFromForJE').val($fromDate.val());
            $('#selectedDateToForJE').val($toDate.val());
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}


 
 

function PostingDataToGenerateReport(title, message, ajaxUrl, data) {
    var formData = new FormData(data);
    formData.append("X-Requested-With", "XMLHttpRequest");

    alertify.confirm(title, message,
        function () {
            if ($(data).valid()) {
                sendAjaxRequest(ajaxUrl, formData);
            }
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

function sendAjaxRequest(url, formData) {
    var ajaxConfig = {
        type: 'POST',
        url: url,
        data: formData,
        contentType: false,
        processData: false,
        success: handleSuccess,
        error: handleError
    };

    $.ajax(ajaxConfig);
}

function handleSuccess(response) {
    appalert(response.message, 2, 1);
    openReportWindow(response.fileType, response.reportType);
}

function handleError(err) {
    appalert(err.statusText, 0, 1);
}

function openReportWindow(fileType, reportType) {
    var url;
    if (fileType === "EXCEL") {
        if (reportType === "TB4") {
            url = "/Reports/PrintTrialBalance4Column";
        } else if (reportType === "TB6") {
            url = "/Reports/PrintTrialBalance6Column";
        } else if (reportType === "BS") {
            url = "/Reports/PrintBalanceSheet";
        } else if (reportType === "GL") {
            url = "/Reports/PrintGeneralLedgerOfAccount";
        } else if (reportType === "JE") {
            url = "/Reports/PrintJournalEntryDtoInExcel";
        }
    } else
    {
        if (reportType === "BS" || reportType === "PANDL") {
            url = "/Reports/DownloadBSFile";
        } else {
            url = "/Reports/AccountingPDFReport?FileType=" + reportType;
        }
     

    }
    console.log(url);
    window.open(url, "_blank");
}
function LoadLiaionLedgerByBranchID(branchId) {
    console.log(branchId);
    $.ajax({
        url: '/AccountingStatements/LiaisonLedgerPerBranch',
        type: 'Get',
        dataType: 'json',
data: { branchId: branchId },
        success: function (data) {
            /* $('#largeModal').hide();*/
            $('#LiaisonLedgerHeading').empty();
            var description = $("#" + branchId + "-Name").text();
            var names = "Liaison Ledger for " + description +".";
            console.log(names);
            $("#LiaisonLedgerHeading").text(names);
            // Append text to the modal title
            //$('#LiaisonLedgerHeading').append(names);
            var table;
            initializeDataTableForLiaisonLedger(data);

            $('#selectedBranchId').val(branchId);
            $('#selectedBranchName').val(description);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function CollectionOfData() {
    var formData = {
        BranchId: $("#SystemQuery_BranchId").val(),
        FromDate: $("input[name='SystemQuery.FromDate']").val(),
        ToDate: $("input[name='SystemQuery.ToDate']").val(),
        ReportType: $("#SystemQuery_ReportType").val(),
        FileType: $("#SystemQuery_FileType").val(),
        AccountId: $("#SystemQuery_AccountId").val()
    };
    return formData;
}
function ShareBranchID(branchId)
{
    $('#journalEntryLabel').empty();
    var description = $("#" + branchId + "-Name").text();
            // Append text to the modal title
    $('#journalEntryLabel').append('Filter duration to view ' + description+' journal entries this data is limited to 3months');
    $('#selectedForJEBranchId').val(branchId);
  $('#selectedForJEBranchName').val(description);        
}
function initializeDataTableForGL(data) {
    console.log(data);
    if ($.fn.DataTable.isDataTable('#BranchAccountDataTable')) {
        // If the DataTable instance already exists, destroy it
        table.destroy();
    }

    if (data && data.length > 0) {
        // Create a new DataTable instance with the provided data
        table = $('#BranchAccountDataTable').DataTable({
            data: data,
            columns: [
                {
                    data: 'AccountNumber' 
                   
                },
                { data: 'AccountName' },
                { data: 'CurrentBalance' } 
             
            ]
        });
    } else {
        // Create an empty DataTable instance
        table = $('#BranchAccountDataTable').DataTable();
        table.clear().draw();
    }
}
function initializeDataTableForLiaisonLedger(data) {
    console.log(data);
    if ($.fn.DataTable.isDataTable('#LiaisonLadgerDataTable')) {
        // If the DataTable instance already exists, destroy it
        table.destroy();
    }

    if (data && data.length > 0) {
        // Create a new DataTable instance with the provided data
        table = $('#LiaisonLadgerDataTable').DataTable({
            data: data,
            columns: [
                { data: 'AccountNumber' },
                { data: 'AccountName' },
                { data: 'DebitBalance' },
                { data: 'CreditBalance' },
                { data: 'CurrentBalance' }
            ]
        });
    } else {
        // Create a new DataTable instance with an empty array and a custom rendering for the empty state
        table = $('#LiaisonLadgerDataTable').DataTable({
            data: [],
            columns: [
                { data: null, defaultContent: '' },
                { data: null, defaultContent: '' },
                { data: null, defaultContent: '' },
                { data: null, defaultContent: '' },
                { data: null, defaultContent: '' }
            ],
            language: {
                emptyTable: "No data found"
            }
        });
    }
}
 
function initializeDataTableForJE(data) {
    console.log(data);
    if ($.fn.DataTable.isDataTable('#JournalEntriesTable')) {
        // If the DataTable instance already exists, destroy it
        table.destroy();
    }

    if (data && data.length > 0) {
        // Create a new DataTable instance with the provided data
        table = $('#JournalEntriesTable').DataTable({
            data: data,
            columns: [
                { data: 'EntryDate' },
                { data: 'Reference' },
                { data: 'AccountNumber' },
                { data: 'Description' },
                { data: 'Debit' },
                { data: 'Credit' },
            ],
            lengthMenu: [[5, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [8, 15, 20, 100, 500, 1000, 2000, 5000, 10000, "All"]]
        });
    } else {
        // Create an empty DataTable instance
        console.log("Table length is empty " + data.length)
        table = $('#JournalEntriesTable').DataTable({
            data: [],
            columns: [
                { data: 'EntryDate' },
                { data: 'Reference' },
                { data: 'AccountNumber' },
                { data: 'Description' },
                { data: 'Debit' },
                { data: 'Credit' },
            ],
            language: {
                emptyTable: "No data found"
            }
        });
    }
}
function loadBranchLiasonAccount(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingStatements/GetAllLiasionAccount',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#SystemQuery_AccountNumber').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#SystemQuery_AccountNumber').append($('<option>').text(item.Value).attr('value', item.Text));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}



function loadBranchAccounts(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingStatements/GetAccountForABranch',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#SystemQuery_AccountId').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#SystemQuery_AccountId').append($('<option>').text(item.Value).attr('value', item.Text));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function loadBranchAccountJournalEntriesByBranchIdAndAccountId(accountId) {
    console.log(accountId); 
    var branchId = $('#selectedForJEBranchId').val();
    var branchName = $('#selectedForJEBranchName').val();
    console.log(branchId);
    console.log(branchName); 
    $.ajax({
        url: '/AccountingStatements/GetBranchAccountJournalEntriesForAnAccount',
        type: 'POST',
        dataType: 'json',
        data: { BranchId: branchId, AccountId: accountId },
        success: function (data) {
          
            $('#JournalEntryHeading').empty();
            var accountNumber = 'AccountNumber-' + accountId;
            var accountNumber2 = $('#' + accountNumber).text();
            // Append text to the modal title
            $('#JournalEntryHeading').append(branchName+' Journal Entries made in ' + accountNumber2);
            // Append text to the modal title
            initializeDataTableForJE(data);
            //if (data && data.length > 0) {
            //    // Define columns only if data is available
            //    table = $('#BranchAccountDataTable').DataTable({
            //        data: data,
            //        columns: [
            //            { data: 'AccountNumber' },
            //            { data: 'AccountName' },
            //            { data: 'CurrentBalance' },
            //            {
            //                data: null,
            //                render: function (data, type, row) {
            //                    var escapedAccountName = '';
            //                    if (row.AccountName) {
            //                        escapedAccountName = row.AccountName.replace(/'/g, "\\'");
            //                    }
            //                    return '<a href="#" class="btn btn-outline-primary" onclick="loadAccountingEntriesByAccountId(\'' + row.Id + '\')" data-toggle="tooltip" data-placement="top" data-bs-toggle="modal" data-bs-target="#largeModal" title="Select ' + escapedAccountName + ' to journal entry">Download Journal Entry</a>';
            //                }
            //            }
            //        ]
            //    });
            //} else {
            //    // Create an empty DataTable instance without columns
            //    table = $('#BranchAccountDataTable').DataTable();

            //    // Add a row with the "No data available" message
            //    //    table.clear().draw();
            //    //    table.row.add([{ colspan: 4, html: '<td style="text-align: center;">No data available</td>' }]).draw();
            //}



            $('#selectedForJEId').val(branchId);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function formatDate(dateString) {
    if (!dateString) return 'Not defined';

    // Check if the date string is in the /Date(ticks)/ format
    const ticksRegex = /^\/Date\((-?\d+)\)\/$/;
    const match = dateString.match(ticksRegex);

    if (match) {
        // Convert ticks to milliseconds and create a Date object
        const ticks = parseInt(match[1], 10);
        const date = new Date(ticks);

        // Format the date
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');

        return `${day}-${month}-${year}`;
    } else {
        // If the date string is not in the /Date(ticks)/ format, treat it as a regular date string
        const date = new Date(dateString);
        if (isNaN(date.getTime())) {
            return 'Not defined';
        }

        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');

        return `${day}-${month}-${year}`;
    }
}
function readAccountNumberCellValue(accountNumber) {
    var cellId = 'account-' + accountNumber;
    var $cell = $('#' + cellId);

    if ($cell.length) {
        var cellValue = $cell.text();
        return cellValue;
    } else {
        console.log('Cell with ID "' + cellId + '" not found.');
        return null;
    }
}