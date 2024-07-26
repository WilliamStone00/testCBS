$(document).ready(function () {
    
    $('#LiaisonAccountOwnerToHide').hide();
    $('#DeterminationManagementAccountId').hide();
    $('#HasManagementAccount').change(function () {

        if ($(this).is(':checked')) {
            alert("ok");
            $('#DeterminationAccountId').hide();
            $('#DeterminationManagementAccountId').show();
        } else {
            $('#DeterminationAccountId').show();
            $('#DeterminationManagementAccountId').hide();
        }
    });

    // Trigger the processing simulation when needed
    $('#ReadUploadedFile').click(function () {
        simulateProcessing();
    });
    $('#document_type').change(function () {
        var selectedValue = $(this).val();

        // Check if the selected value is not empty
        if (selectedValue !== '') {
            // Show the second dropdown
            $('#document_Sub_type').show();
            // Clear previous options
            $('#document_Sub_type').empty();
            // Populate the second dropdown based on the selected value
            if (selectedValue === 'PANDL') {
                $('#document_Sub_type').append('<option value="incomeStatement">Income Statement</option>');
                $('#document_Sub_type').append('<option value="expenseStatement">Expense Statement</option>');
                $('#document_Sub_type').append('<option value="none">none</option>');
                // Show the element
                $('#itemsToHide1').hide();
                $('#itemsToHide2').hide();
                $('#itemsToHide3').hide();
                $('#itemsToHide4').hide();
                $('#itemsToShow1').show();
                $('#itemsToShow2').show();
            } else if (selectedValue === 'BS') {
                $('#document_Sub_type').append('<option value="asset">Asset</option>');
                $('#document_Sub_type').append('<option value="liability">Liability</option>');
                $('#document_Sub_type').append('<option value="none">none</option>');
                // Show the element
                $('#itemsToHide1').show();
                $('#itemsToHide2').show();
                $('#itemsToHide3').show();
                $('#itemsToHide4').show();
                $('#itemsToShow1').hide();
                $('#itemsToShow2').hide();
            }
        } else {
            // If the selected value is empty, hide the second dropdown
            $('#document_Sub_type').hide();
        }
    });

    $('#document_Sub_type').change(function () {
        var selectedValue = $(this).val();
        var IdModel = "";
        console.log(selectedValue);
        $.ajax({
            url: '/AccountingConfiguration/GetAccountNUMBERByDOCUMENTYPE',
            type: 'GET',
            dataType: 'json',
            data: { DocumentId: selectedValue },
            success: function (data) {
                // Clear existing options in the OperationEventAttributeId combo
                if (selectedValue === "asset" || selectedValue === "liability") {
                    IdModel = "#GrossChartOfAccountId";
                    $(IdModel).empty();
                    // Add new options based on the fetched data
                    $.each(data, function (index, item) {
                        $('#GrossChartOfAccountId').append($('<option>').text(item.Value).attr('value', item.Text));
                    });
                    $.each(data, function (index, item) {
                        $('#AmortizationChartOfAccountId').append($('<option>').text(item.Value).attr('value', item.Text));
                    });
                }
                if (selectedValue === "incomeStatement" || selectedValue === "expenseStatement") {
                    IdModel = "#incomeOrExpense";
                    $(IdModel).empty();
                    // Add new options based on the fetched data
                    $.each(data, function (index, item) {
                        $('#incomeOrExpense').append($('<option>').text(item.Value).attr('value', item.Text));
                    });
                }

            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        });
    });
    $("#loadChartAccount").click(function () {
        console.log("loadChartAccount");
        loadChartAccount();
    });

    $("#loadAccounts").click(function () {
        console.log("loadAccounts");
        loadAccounts();
    });
    $(document).on('change', '#OperationEventId', function () {
        var EventId = $(this).val();
        console.log("Operation Event Id selected: " + EventId);
        loadOperationEventAttributeIds(EventId);
    });
    $(document).on('change', '#Account_ChartOfAccountManagementPositionId', function () {
        var selectedOption = $(this).find('option:selected');
        var eventId = $(this).val();
        var selectedText = selectedOption.text();
        var chartNumber = getFirst6Characters(splitStringByHyphen(selectedText)[1]);

        console.log("Selected text:", selectedText);
        console.log("Chart number:", chartNumber);

        if (chartNumber === "451000") {
            $('#LiaisonAccountOwnerToHide').show();
            console.log("Showing Liaison Account Owner element");
        } else {
            $('#LiaisonAccountOwnerToHide').hide();
            console.log("Hiding Liaison Account Owner element");
        }

        loadAccountCategoryByChartNumber(eventId);
    });
    $(document).on('change', '#AccountOwnerId', function () {
        var selectedValue = $(this).val();

        var accNumber = $('#AccountNumberCU').val();
        accNumber = accNumber.replace('[BCD]', selectedValue);
        $('#AccountNumberCU').val(accNumber);
        $('#BranchCode').val(selectedValue);
    });
    $(document).on('change', '#LiasonAccountOwnerId', function () {
        var selectedValue = $(this).val();
        var accNumber = $('#AccountNumberCU').val();
        accNumber = replaceLastThreeChars($('#AccountNumberCU').val(), selectedValue);
        $('#AccountNumberCU').val(accNumber);
    });
    $('#AccountNumber').on('input', function () {
        var inputValue = $(this).val();
        if (inputValue === '451') {
            $('#LiasonAccountOwnerToHIde').show();
         
        } else {
            $('#LiasonAccountOwnerToHIde').hide();
        }
    });
/*    LoadDataForEventRule();*/
});

function splitStringByHyphen(inputString) {
    // Check if the input is a string
    if (typeof inputString !== 'string') {
        return "Error: Input must be a string";
    }

    // Split the string using the '-' character
    const result = inputString.split('-');

    // Return the resulting array
    return result;
}

function getFirst6Characters(str) {
    // Check if the string is defined and not null
    if (str == null || str == undefined) {
        return "";
    }

    // Convert input to string (in case it's not already a string)
    str = String(str);

    // Use the slice method to get the first 6 characters
    return str.slice(0, 7);
}
function replaceLastThreeChars(inputString, replacement) {
    // Check if the string is at least 3 characters long
    if (inputString.length < 3) {
        return inputString; // Return the original string if it's too short
    }

    // Remove the last 3 characters and append the replacement
    return inputString.slice(0, -3) + replacement;
}
function updateProgressBar(progress) {
    var progressBar = $('.progress-bar');
    progressBar.css('width', progress + '%');
    progressBar.attr('aria-valuenow', progress);
    progressBar.text(progress + '%');
}

function simulateProcessing() {
    var progress = 0;
    var interval = setInterval(function () {
        progress += 10;
        updateProgressBar(progress);

        if (progress >= 100) {
            clearInterval(interval);
            console.log('Processing complete!');
        }
    }, 1000); // Update every 1 second
}

function ApproveJournalEntry(response) {
    //comment_description
    var storedId = $("#selectedId").val();
    var comment = $("#comment_description").val();

    if (comment === "") {

        appalert("Please kindly enter your comment for this entry with referenceId:" + storedId + " before you continue", 3, 1);
        return;
    } else {
        var ServiceOption = "EntryTempData";
        var message = "WARNING!!!\n";
        message += "Are you sure you want to confirm this various account adjustment?\n";
        ApproveJournalEntryTransactions('Confirm Manual Entry Operation', message, '/ManuallyJournalEntry/ApproveEntries', ServiceOption, response, storedId, comment);

    }
}

function ReadExcelFile() {
    var formData = new FormData();
    var file = $("#uploadedFile")[0].files[0];
    var branchCode = $("#branchCodeDropdown").val(); // Assuming your dropdown has this ID

    if (branchCode === "") {

        appalert("PLEASE KINDLY SELECT YOUR BRANCH CODE", 2, 1);
        return;
    } else {
        console.log(file);
        formData.append("ExcelFile", file);
        formData.append("BranchId", branchCode); // Adding branch code to formData
        console.log(formData.get("ExcelFile"));
        console.log("Branch Code:", formData.get("BranchCode"));

        event.preventDefault();
        $.ajax({
            url: "/AccountingConfiguration/UploadAccountModel/",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                appalert("PLEASE KINDLY SELECT YOUR BRANCH CODE", 2, 1);

                console.log(response);
            },
            error: function (xhr, status, error) {
                console.log("Error uploading file: " + error);
                // Handle error response
            }
        });

    }

}

function ExecuteExcelFile() {
    var branchId = $("#BranchIdOption").val();
    console.log(branchId);

    $.ajax({
        url: '/AccountingConfiguration/UploadAccounts/' + branchId,
        type: 'Post',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {

        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function RegisterEntryRule() {



    var message = "WARNING!!!\n";
    message += "Are you sure you want to add this multiple accounting event rule?\n";
    SavingAccountingRuleToDB('Confirm accounting event code ', message, '/AccountingConfiguration/AddAccountingRole', null);
}




function AddEntryRule() {

    var entry = EntryRule();
    if (entry.AccountingRule.RuleName === null || entry.AccountingRule.RuleName === "") {
        appalert("Please kindly enter the Accounting RuleName", 2, 1);
        return;
    }
    if (entry.AccountingRule.Description === null || entry.AccountingRule.Description === "") {
        appalert("Please kindly enter a description for this rule", 2, 1);
        return;
    }
    if (entry.AccountingRule.MFI_ChartOfAccountId === null || entry.AccountingRule.MFI_ChartOfAccountId === "") {
        appalert("Please kindly  Select the Accounting Chart", 2, 1);
        return;
    }
    var message = "WARNING!!!\n";
    message += "Are you sure you want to add accounting event code with " + entry.AccountingRule.RuleName + " into " + entry.AccountingRule.BookingDirection + "?\n";
    SavingAccountingRule('Confirm accounting event code ', message, '/AccountingConfiguration/AddOrUpdateRole', entry);
}




function LoadAccountingRuleData(controller, action, divLoader, tableID, serviceoption, KEY, partialView, path, order) {
    KEY = $('#EntryTempData_Reference').val();
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + serviceoption + '&KEY=' + KEY + '&partialView=' + partialView + '&path=' + path,
        success: function (dataResponse) {
            console.log(dataResponse);
            $('#' + divLoader).html(dataResponse);
            LoadDT(tableID, order);

        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}

function DeleteAccountingRole(key, controller, action) {

    var entry = EntryRule();
    if (entry.AccountingRule.RuleName === null || entry.AccountingRule.RuleName === "") {
        appalert("Please kindly enter the Accounting RuleName", 2, 1);
        return;
    }
    var message = "WARNING!!!\n";
    message += "Are you sure you want to add accounting event code with " + entry.AccountingRule.RuleName + " into " + entry.AccountingRule.BookingDirection + "?\n";
    DeleteAccountingRoleFromDB('Confirm accounting event code ', message, '/' + controller + '/' + action, key);
}

function DeleteAccountingRoleFromDB(title, message, ajaxUrl, key) {
    $.ajax({
        type: "GET",
        url: ajaxUrl + '?Id=' + key,
        success: function (dataResponse) {
            console.log(dataResponse);
            var table;
            initializeDataTableAccountingRuleData(dataResponse)
        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });

}
function ReadEntryRule(dataResponse) {
    $("#ServiceOptionn").val(dataResponse.ServiceOption);

    $("#AccountingRule_RuleName").val(dataResponse.RuleName);
    $("#AccountingRule_AccountingEntryRuleId").val(dataResponse.AccountingEntryRuleId);
    $("#AccountingRule_BookingDirection").val(dataResponse.BookingDirection);

}

function SavingAccountingRuleToDB(title, message, ajaxUrl, data) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 3, 1);
                    }
                    else {
                        appalert(response.message, 1, 3);

                    }
                },
                error: function () {
                    appalert(error, 0, 3);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}
function SavingAccountingRule(title, message, ajaxUrl, data) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 3, 1);
                        initializeDataTableAccountingRuleData(data);
                    }
                    else {
                        appalert(response.message, 1, 3);

                    }
                },
                error: function () {
                    appalert(error, 0, 3);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}
function EntryRule() {
    const formData = {
        ServiceOption: $("#ServiceOptionn").val(),
        AccountingRule: {
            RuleName: $("#AccountingRule_RuleName").val(),
            AccountingEntryRuleId: $("#AccountingRule_AccountingEntryRuleId").val(),
            BookingDirection: $("#AccountingRule_BookingDirection").val(),
        },
        Action: $("#Action").val()

    };
    return formData;
}
//function initializeDataTableForAccountUpload(data) {
//    if ($.fn.DataTable.isDataTable('#myDataTable_AccountUploadData')) {
//        // If the DataTable instance already exists, destroy it
//        table.destroy();
//    }
//    console.log(data);
//    if (data && data.length > 0) {
//        // Create a new DataTable instance with the provided data
//        table = $('#myDataTable_AccountUploadData').DataTable({
//            data: data,
//            columns: [
//                { data: 'AccountNumber' },
//                { data: 'AccountName' },
//                { data: 'ChartofAccount' },
//                { data: 'CreatedDate' },
//                { data: 'CurrentBalance' },
//                { data: 'BeginningBalance' },
//                { data: 'BranchCode' },

//            ]
//        });
//    } else {
//        // Create an empty DataTable instance
//        table = $('#myDataTable_AccountUploadData').DataTable();
//        table.clear().draw();
//    }
//}

function initializeDataTableAccountingRuleData(data) {
    // Get the table element
    var tableElement = $('#myDataTable_AccountingRuleData');

    // Check if a DataTable instance already exists
    if ($.fn.DataTable.isDataTable(tableElement)) {
        // If it exists, destroy it
        tableElement.DataTable().destroy();
    }

    console.log(data);

    if (data && data.length > 0) {
        // Initialize a new DataTable instance with data
        table = tableElement.DataTable({
            data: data,
            columns: [
                { data: 'MFI_ChartOfAccountId' },
                { data: 'BookingDirection' },
                { data: 'Description' },
                {
                    "data": "Id",
                    "orderable": "false",
                    "render": function (data) {
                        return "<a href='/AccountingConfiguration/DeleteAccountingRole?Id=" + data + " class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Delete</a>";
                    }
                }
            ]
        });
    } else {
        // Initialize an empty DataTable instance
        table = tableElement.DataTable();
        table.clear().draw();
    }
}

function padNumberDigits(number, completingNumber) {
    // Convert the number to a string
    let numString = number.toString();

    // Check if the number is already 12 or more digits
    if (numString.length >= completingNumber) {
        return numString;
    }

    // Calculate how many zeros we need to add
    let zerosToAdd = completingNumber - numString.length;

    // Add the zeros to the right of the number
    return numString + '0'.repeat(zerosToAdd);
}
function loadAccountCategoryByChartNumber(number) {
    console.log(number);

    $.ajax({
        url: '/AccountingConfiguration/GetAccountForChartOfAccountManagementPositionId',
        type: 'GET',
        dataType: 'json',
        data: { Id: number },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#Account_AccountCategoryId').empty();

            $('#AccountNumber').val(data.AccountNumber);
            $('#AccountName').val(data.description);
            var accNumber = padNumberDigits(data.AccountNumber, 6) + "[BCD]" + "000"
            $('#AccountNumberCU').val(accNumber);
            // Add new options based on the fetched data
            $.each(data.accountCategoryList, function (index, item) {
                $('#Account_AccountCategoryId').append($('<option>').text(item.Text).attr('value', item.Value));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function extractNumericCode(selectedValue) {

    console.log('extractNumericCode' + selectedValue);
    // Check if the selectedValue is not null or undefined
    if (selectedValue) {
        // Split the string by the hyphen and trim any whitespace
        const parts = selectedValue.split('-').map(part => part.trim());

        // Return the first part (which should be the numeric code)
        return parts[0];
    }

    // Return an empty string or null if there's no valid input
    return '';
}
function loadOperationEventAttributeIds(operationEventId) {
    console.log(operationEventId);

    $.ajax({
        url: '/AccountingConfiguration/GetOperationEventAttribute',
        type: 'GET',
        dataType: 'json',
        data: { operationEventId: operationEventId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#OperationEventAttributeId').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#OperationEventAttributeId').append($('<option>').text(item.Name).attr('value', item.Id));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
//     <div id="jstree-checkbox"></div>jstree-checkbox    url: "/AccountingConfiguration/InitializeData?KEY=null&partialView=_ChartOfAccountData&path=list&serviceOption=account",
function loadChartAccount() {
    $.ajax({
        url: "/AccountingChart/InitializeData?KEY=null&partialView=null&path=list",
        success: function (data) {

            $('#jstree-checkbox').jstree({
                core: {
                    data: data
                }
            });

        }



    });

}

function loadAccounts() {
    $.ajax({
        url: "/AccountingChart/InitializeData?KEY=null&partialView=null&path=list",
        success: function (data) {

            $('#jstree-context-menu').jstree({
                core: {
                    data: data
                }
            });

        }



    });

}
function AjaxPostAndUpdateChartOfAccount(form) {

    /* $.validator.unobtrusive.parse(form);*/
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {

                    LoadChartOfAccounts();
                }
                else {
                    alert("Error has occured!");
                    appalert(response.message, 1, 4);
                }

            }
            , error: function (err) {
                appalert(err.statusText, 0, 1);
            }
        };

        if ($(form).attr('enctype') === "multipart/form-data") {
            ajaxConfig["contentType"] = false;
            ajaxConfig["processData"] = false;
        }
        $.ajax(ajaxConfig);

    }
    return false;

}

$('#jstree-context-menu').on('click', '.parent', function (e, data) {

    // Get information about the clicked element or perform specific actions
    var clickedElement = $(this);
    var nodeId = clickedElement.closest('li').attr('id');
    var nodeName = clickedElement.text();

    // Example: Display information about the clicked node
    alert('Clicked Node ID: ' + nodeId + '\nClicked Node Name: ' + nodeName);
    $("#selectedID").val(nodeId);
    $(".RootId").val(nodeId);

});
$('#jstree-checkbox').on('click', '.parent', function (e, data) {

    // Get information about the clicked element or perform specific actions
    var clickedElement = $(this);
    var nodeId = clickedElement.closest('li').attr('id');
    var nodeName = clickedElement.text();

    // Example: Display information about the clicked node
    alert('Clicked Node ID: ' + nodeId + '\nClicked Node Name: ' + nodeName);
    $("#selectedIDchart").val(nodeId);
    $(".RootId").val(nodeId);

});
function GetObjectView(partialview, path, serviceOption, divToLoadContent) {
    var idChart = $("#selectedID").val();
    var idAcc = $("#selectedIDchart").val();
    alert("id from Chart:" + idChart);
    alert("id from Account:" + idAcc);
    loadPartialView2(id, partialview, path, serviceOption, divToLoadContent);
    //loadPartialView(nodeId, "_Operation", "Transit");
}

function GetObjectViewForChart(partialview, path, serviceOption, divToLoadContent) {
    var id = $("#selectedIDchart").val();
    alert("id:" + id);
    loadPartialView2(id, partialview, path, serviceOption, divToLoadContent);
    //loadPartialView(nodeId, "_Operation", "Transit");
}
function loadPartialView2(nodeId, view, path, serviceOption, divToLoadContent) {
    // Use AJAX to load the partial view based on the nodeId
    $.ajax({
        url: '/AccountingConfiguration/InitializeData?KEY=' + nodeId + '&partialView=' + view + '&path=' + path + '&serviceOption=' + serviceOption,
        type: 'GET',
        data: { nodeId: nodeId },
        success: function (result) {
            // Assuming you have a container where you want to display the partial view
            $('.' + divToLoadContent).html(result);
        },
        error: function (error) {
            /*console.error('Error loading partial view:', error);*/
            alert('Error loading partial view:', error);
        }
    });

}

function TrialBalanceReferenceDataConfiguration(controller, KEY, tableID, partialView, order, divToLoadTheData, serviceOption) {
    LoadDataTableNew(controller, tableID, "InitializeData", KEY, partialView, order, "details", divToLoadTheData, serviceOption);
}

function DownloadMFIChartOfAccount() {
    // (string KEY = null, string partialView = null, string path = null, string serviceOption = null)

    window.open('/Reports/DownloadExcelFilelist', '_blank');


}
