$(document).ready(function () {
    var apiUrl = "/ManuallyJournalEntry/GetAccountMFIChartOfAccount";

    // Function to fetch Chart of Accounts dynamically
    function fetchChartOfAccounts(callback) {
        $.ajax({
            url: apiUrl,
            type: "GET",
            dataType: "json",
            beforeSend: function () {
                console.log("Fetching Chart of Accounts...");
            },
            success: function (data) {
                var options = `<option value="">Select Chart of Account</option>`;
                $.each(data, function (index, item) {
                    options += `<option value="${item.Value}">${item.Text}</option>`;
                });
                callback(options);
            },
            error: function () {
                console.error("Failed to load Chart of Accounts.");
                callback(""); // Return empty options if API fails
            }
        });
    }

    var bookingDirectionOptions = `
        <option value="Credit">Credit</option>
        <option value="Debit">Debit</option>
    `;

    // Event listener for editing cells (handles new and existing rows)
    $("#AccountingRulebasketTable tbody").on("click", "td", function () {
        var $cell = $(this);
        var columnIndex = $cell.index();
        var $row = $cell.closest("tr");
        var identifier = $row.data("identifier") || $row.find("td").eq(0).text().trim();

        if ($cell.find("select").length > 0) return;

        var oldValue = $cell.text().trim();
        $cell.attr("data-old-value", oldValue);
        $cell.attr("data-identifier", identifier);

        if (columnIndex === 0) {
            fetchChartOfAccounts(function (options) {
                createDropdown($cell, options);
            });
        } else if (columnIndex === 1) {
            createDropdown($cell, bookingDirectionOptions);
        }
    });

    // Function to create and insert dropdowns dynamically
    function createDropdown($cell, options) {
        var currentValue = $cell.attr("data-old-value");
        var $dropdown = $(
            `<select class="form-control form-control-sm editable-dropdown">${options}</select>`
        );

        $dropdown.val(currentValue);
        $cell.html($dropdown);
        $dropdown.select2({ width: "100%", dropdownParent: $cell, placeholder: "Select an option", allowClear: true });
        $dropdown.focus();

        $dropdown.on("change blur", function () {
            saveChanges($cell, $dropdown);
        });
    }

    // Function to save changes and send updates to the server
    function saveChanges($cell, $dropdown) {
        var newValue = $dropdown.find(":selected").text();
        var newId = $dropdown.val();
        var oldValue = $cell.attr("data-old-value");
        var identifier = $cell.attr("data-identifier");
        var columnIndex = $cell.index();
        var updateUrl = columnIndex === 0 ? "/ManuallyJournalEntry/updateChartOfAccount" : "/ManuallyJournalEntry/updateBookingDirection";

        $cell.text(newValue);
        $cell.data("id", newId);

        console.log("Updating:", { identifier, oldValue, newValue });

        $.ajax({
            url: updateUrl,
            type: "POST",
            data: JSON.stringify({ identifier: identifier, oldValue: oldValue, newValue: newValue }),
            contentType: "application/json",
            success: function () {
                console.log("Updated successfully:", { identifier, oldValue, newValue });
            },
            error: function () {
                console.error("Failed to update:", { identifier, oldValue, newValue });
            }
        });
    }

    // Function to add new entry dynamically
    $("#addEntry").on("click", function () {
        fetchChartOfAccounts(function (chartOfAccountOptions) {
            var newRowId = "row-" + new Date().getTime();

            const newRow = `
                <tr">
                    <td width="75%" class="data-old-id">
                        <select class="form-control form-control-sm editable-dropdown">${chartOfAccountOptions}</select>
                    </td>
                    <td width="25%" class="data-old-id">
                        <select class="form-control form-control-sm editable-dropdown">${bookingDirectionOptions}</select>
                    </td>
                </tr>
            `;

            $("#AccountingRulebasketTable tbody").append(newRow);

            $("#AccountingRulebasketTable tbody tr:last-child select").select2({ width: "100%", placeholder: "Select an option", allowClear: true });

            console.log("New row added with identifier:", newRowId);
        });
    });
});

function saveForm() {
    var formData = {
        ServiceOption: $("#ServiceOptionn").val(),
        AccountingEventRule: {
            Id: $("#AccountingEventRule_Id").val(),
            EventName: $("#AccountingEventRule_EventName").val(),
            IsDoubleValidationNeeded: $("#AccountingRule_IsValidationNeed").val(),
            ListOfEligibleBranchId: $("#ListOfEligibleBranchId").val(),
            EntryType: $("#EntryType").val(),
            LevelOfExecution: $("#LevelOfExecution").val(),
            AccountingRules: []
        }
    };

    // Collect data from the table
    $("#AccountingRulebasketTable tbody tr").each(function () {
        var $row = $(this);
        var chartOfAccountId = $row.find("td:eq(0)").data("id") || $row.find("td:eq(0)").text().trim();
        var bookingDirection = $row.find("td:eq(1)").text().trim();

        if (chartOfAccountId) { // Ensure valid data is collected
            formData.AccountingEventRule.AccountingRules.push({
                MFI_ChartOfAccountId: chartOfAccountId,
                BookingDirection: bookingDirection
            });
        }
    });

    console.log("Submitting Form Data: ", formData); // Debugging output

    // Send data to the server
    $.ajax({
        url: "/ManuallyJournalEntry/UpdateAccountingRule",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(formData),
        beforeSend: function () {
            $("#saveButton").prop("disabled", true).text("Saving...");
        },
        success: function (response) {
            appalert("Accounting Rule Saved Successfully!",1,1);
            console.log("Save Success:", response);
        },
        error: function (xhr) {
            alert("Failed to save. Please try again.");
            console.error("Save Error:", xhr.responseText);
        },
        complete: function () {
            $("#saveButton").prop("disabled", false).text("Save");
        }
    });
}
