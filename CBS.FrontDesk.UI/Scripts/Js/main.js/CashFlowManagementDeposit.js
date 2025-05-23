$(document).ready(function () {

    $('#hideBranchID').hide();
    $('#hideAccountId').hide();
 
    LoadDepositRequestForHO("HeadOfficeDataTable")
    LoadDepositRequestForBO("BranchOfficeDataTable")
    console.log("Is loaded");

    $(document).on('change', '#DepositNotificationDto_ApprovedBy', function () {

        // Get the selected value 
        var selectedValue = $(this).val();
        console.log(selectedValue);
        if (selectedValue === 'RedirectToBranchBTB' || selectedValue.includes('Approved')) {
            // Show the element 
            console.log(selectedValue);
            $('#hideBranchID').show();
            $('#hideAccountId').hide();
            const parts = selectedValue.split('@');

            var branchId = parts[1];
            if (parts[0].includes('Approved')) {
                $('#hideBranchID').hide();
                $('#hideAccountId').show();
               
                GetBranchBankAccount(branchId);
            } else {
                loadBranch();
                console.log("has taged :"+selectedValue+"for options");
            }
        } else {
            // Hide the element
            $('#hideBranchID').hide();
            $('#hideAccountId').hide();
        }
    });
    $(document).on('change', '#QueryModel_BranchId', function () {
        // Get the selected value (AccountId) from the dropdown
        var selectedValue = $(this).val();
        // Call the `loadAccountById` function with the selected AccountId
        loadIssuingBranchUsers(selectedValue);
    });


    $('#ValidateApproverUnionWide').change(function () {
        // Check if the checkbox is checked
        if ($(this).is(':checked')) {
            // ✅ Checkbox is checked – perform your data loading logic here
            loadApproverBranchUsers("XXXXX");
        } else {
            // Optionally handle when it's unchecked
            console.log("Checkbox unchecked");
        }
    });
    $(document).on('change', '#AccountId', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        loadAccountBalance(selectedValue);
    });

});
function Search(controller, tableDiv, partialView, datalistViewDIV, filterOption) {
    // if (e) e.preventDefault(); // Prevent default if the event is passed

    const jsonData = {
        ServiceOption: $('#ServiceOption').val(),
        Action: $('#Action').val(),
        BranchId: $('select[name="QueryModel.BranchId"]').val(),
        IssuedBy: $('#QueryModel_IssuedBy').val(),
        ValidateApproverUnionWide: $('#ValidateApproverUnionWide').is(':checked'),
        ApprovedBy: $('#QueryModel_ApprovedBy').val(),
        Status: $('#QueryModel_Status').val(),
        FromDate: $('#fromDate').val(),
        ToDate: $('#toDate').val(),
        SelectedBranchID: $('#selectedBranchID').val()
    };
    var jsonDataj = JSON.stringify(jsonData);
    console.log("Collected Form Data:", jsonData);

    LoadSearchData(
        controller,  //'ManuallyJournalEntry',
        tableDiv, //  'myDataTable',
        partialView,// '_PendingEntries',
        datalistViewDIV, //'datalistingview_pendingEntries',
        jsonDataj,
        filterOption,// 'FilteringOption',
        $('select[name="QueryModel.FilteringOption"]').val()
    );

    return false;
}
function LoadSearchData(controller, tableID, partialView, datalistingview, KEY, serviceOption, path = 'list') {
    path = serviceOption;
    // Call LoadDataTableNewVersion with predefined action "InitializeData" and other parameters
    LoadDataTableNewVersion(
        controller,
        tableID,
        "InitializeData",
        KEY,
        partialView,
        path,
        datalistingview,
        serviceOption
    );
}
/**
 * Loads data into a table via AJAX and initializes it as a DataTable.
 * 
 * @param {string} controller - The controller name for the AJAX request.
 * @param {string} tableID - The ID of the table to be initialized as a DataTable.
 * @param {string} action - The action name for the AJAX request.
 * @param {string} KEY - A key parameter for the request.
 * @param {string} partialView - The name of the partial view to be loaded.
 * @param {string} path - A path parameter for the request.
 * @param {string} diveToloadtheData - The ID of the div where the loaded data will be inserted.
 * @param {string} serviceOption - An option parameter for the service.
 */
function LoadDataTableNewVersion(controller, tableID, action, KEY, partialView, path, diveToloadtheData, serviceOption) {
    // Construct the URL for the AJAX request
    var encodedURL = '/' + controller + '/' + action +
        '?KEY=' + encodeURIComponent(KEY) +
        '&partialView=' + encodeURIComponent(partialView) +
        '&serviceOption=' + encodeURIComponent(serviceOption) +
        '&path=' + encodeURIComponent(path);

    // Log the constructed URL and other details
    console.log(encodedURL + " tableId= " + tableID + " divloader:" + diveToloadtheData);

    // Perform the AJAX request
    $.ajax({
        type: "GET",
        url: encodedURL,
        success: function (data) {
            // Insert the received data into the specified div
            $('#' + diveToloadtheData).html(data);

            // Log the presence of the table element
            console.log($('#' + tableID).length);

            // Initialize the DataTable
            LoadDataInfo(tableID);
        },
        error: function (err) {
            // Display an error alert if the request fails
            appalert(err.statusText, 1, 3);
        }
    });
}
function LoadDataInfo(tableID) {
    var tableSelector = '#' + tableID;
    console.log("Initializing DataTable for:", tableSelector);

    try {
        // Check if the table exists
        if ($(tableSelector).length === 0) {
            throw new Error("Table not found: " + tableSelector);
        }

        // Get the number of columns in the table
        var columnCount = $(tableSelector + ' thead th').length;
        console.log("Number of columns detected:", columnCount);

        // Prepare column definitions based on the actual number of columns
        var columnDefs = [];
        for (var i = 0; i < columnCount; i++) {
            columnDefs.push({
                targets: i,
                searchable: true,
                orderable: true
            });
        }

        var dataThumbView = $(tableSelector).DataTable({
            responsive: false,
            columns: Array(columnCount).fill(null),  // Create empty column definitions
            columnDefs: columnDefs,
            language: {
                lengthMenu: "_MENU_",
                search: ""
            },
            lengthMenu: [[10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],
            order: [[0, "asc"]],
            info: true,
            pageLength: 10
        });

        console.log("DataTable initialized successfully");
        return dataThumbView;
    } catch (error) {
        console.error("Error initializing DataTable:", error);
        console.log("Table HTML:", $(tableSelector).prop('outerHTML'));
    }
}
function loadIssuingBranchUsers(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/CashFlowManagement/GetBranchUsersByBranchId',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#QueryModel_IssuedBy').empty();
            $('#QueryModel_ApprovedBy').empty();
            // Add new options based on the fetched data
            if (branchId === "XXXXX") {

                $.each(data, function (index, item) {
                    $('#QueryModel_ApprovedBy').append($('<option>').text(item.Text).attr('value', item.Value));
                });
            } else {
                $.each(data, function (index, item) {
                    $('#QueryModel_IssuedBy').append($('<option>').text(item.Text).attr('value', item.Value));
                });
                $.each(data, function (index, item) {
                    $('#QueryModel_ApprovedBy').append($('<option>').text(item.Text).attr('value', item.Value));
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function loadApproverBranchUsers(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/CashFlowManagement/GetBranchUsersByBranchId',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo

            $('#QueryModel_ApprovedBy').empty();
            // Add new options based on the fetched data
            /*   if (branchId === "XXXXX") {*/

            $.each(data, function (index, item) {
                $('#QueryModel_ApprovedBy').append($('<option>').text(item.Text).attr('value', item.Value));
            });
            //} else {
            //    $.each(data, function (index, item) {
            //        $('#QueryModel_IssuedBy').append($('<option>').text(item.Value).attr('value', item.Text));
            //    });
            //    $.each(data, function (index, item) {
            //        $('#QueryModel_ApprovedBy').append($('<option>').text(item.Value).attr('value', item.Text));
            //    });
            //}
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function loadBankAccountForBranch(BranchId, Option)
{
 
    $.ajax({
        url: '/CashFlowManagement/GetAllBranchAccountUsedToCreditCashFlow',
        type: 'GET',
        dataType: 'json',
        data: { branchId: BranchId ,optionQuery:Option },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo  DepositNotificationDto_Temp3
            $('#DepositNotificationDto_Temp3').empty();
            $.each(data, function (index, item) {
                $('#DepositNotificationDto_Temp3').append($('<option>').text(item.Value).attr('value', item.Text));
            });

            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function loadBranch() {

    $.ajax({
        url: '/CashFlowManagement/GetAllBranch',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo  DepositNotificationDto_Temp3
            $('#CorrespondingBranchID').empty();
            $.each(data, function (index, item) {
                $('#CorrespondingBranchID').append($('<option>').text(item.Value).attr('value', item.Text));
            });

            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function GetBranchBankAccount(BranchId) {

    $.ajax({
        url: '/CashFlowManagement/GetBranchBankAccount',
        type: 'GET',
        dataType: 'json',
        data: { branchId: BranchId},
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo  DepositNotificationDto_Temp3
            $('#DepositNotificationDto_Temp3').empty();
            $.each(data, function (index, item) {
                $('#DepositNotificationDto_Temp3').append($('<option>').text(item.Value).attr('value', item.Text));
            });

            // Add new options based on the fetched data
            console.log(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function loadAccountBalance(accountId) {
    console.log(accountId);
    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountBalance',
        type: 'GET',
        dataType: 'json',
        data: { Id: accountId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo 
            $('#BankCashOut_Balance').empty();
            $("#BankCashOut_Balance").val(data.Account.CurrentBalance);
 
            // Add new options based on the fetched data
            console.log(data.Account.CurrentBalance);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function refreshPage() {
    // Refresh the current page
    window.location.reload();
    console.log("Page Refreshed");
    // Redirect to the home page
    window.location.href = "/CashFlowManagement/GetAllCashReplenimentRequestData";
}
function refreshPageHome() {
    // Refresh the current page
    window.location.reload();
    console.log("Page Refreshed");
    // Redirect to the home page
    window.location.href = "/home";
}


function loadBranchCreditingAccount(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/CashFlowManagement/GetAllBranchAccountUsedToCreditCashFlow',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#CashReplenimentRequestdto_AccountId').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#CashReplenimentRequestdto_AccountId').append($('<option>').text(item.Value).attr('value', item.Text));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function LoadDepositRequestForHO(tableID) {


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
            { "targets": 0, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 6, "searchable": true, "orderable": true, "width": "10%" },
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
//

function LoadDepositRequestForBO(tableID) {


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
            { "targets": 1, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "20%" },
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
function LoadInfomation(referenceId) {
    console.log(referenceId);

    $.ajax({
        url: '/CashFlowManagement/GetBankTransactionByDepositId',
        type: 'GET',
        dataType: 'json',
        data: { referenceId: referenceId },
        success: function (data) {

            console.log(data.BankTransaction);

            $('#exampleModalLabel3').empty();

            // Append text to the modal title
            $('#exampleModalLabel3').append('Bank Transaction Information:' + data.BankTransaction.Id);
 
            $('#BranchName').text(data.BankTransaction.TransactionType);
            $('#Balance').text(data.BankTransaction.Balance.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));
            $('#BankTransactionDate').text(data.BankTransaction.ValueDate);
            $('#Amount').text( data.BankTransaction.Amount.toLocaleString('en-US', { style: 'currency', currency: 'XAF' }));
            $('#TransactionId').text(data.BankTransaction.BankTransactionReference);
       
            // Update image source
            $('#previewimage').attr('src', data.BankTransaction.FileUpload);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function convertMicrosoftDate(microsoftDate) {
    // Extract the number from the string
    const matches = microsoftDate.match(/\d+/);
    if (!matches) {
        return "Invalid date format";
    }

    // Convert to number and create a Date object
    const timestamp = parseInt(matches[0], 10);
    const date = new Date(timestamp);

    // Format the date as desired
    return date.toLocaleString(); // Or use any other date formatting method
}

function AjaxPostBankTransaction(form) {
    formData = new FormData(form);

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {


        alertify.confirm("Confirmation", "Are you sure you want to perform this action! ",
            function () {


                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: formData,
                    success: function (response) {

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
                        }
                        else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else {
                                appalert(response.message, 2, 1);
                            }

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
function LoadCashReplenishmentDataDT(tableID) {


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
            { "targets": 1, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "20%" },

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

//InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
function LoadDataTable(controller, tableID, KEY, partialView, order, path, diveToLoadTheData, serviceOption) {
    var action = 'InitializeData';
    var encodedURL = '/' + controller + '/' + action + '?KEY=' + encodeURIComponent(KEY) + '&partialView=' + encodeURIComponent(partialView) + '&path=' + encodeURIComponent(path) + '&serviceOption=' + encodeURIComponent(serviceOption);
    $.ajax({
        type: "GET",
        url: encodedURL,
        success: function (data) {
            $('#' + diveToLoadTheData).html(data);
            LoadDT(tableID, order);
        },
        error: function (xhr, status, error) {
            appalert(error, 1, 3);
        }
    });
}


function DeleteCashRequestData(controller, KEY, serviceOption, status) {

    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY + "&serviceOption=" + serviceOption;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    console.log(response.success);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        window.location.reload();
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

function calculateCashBalance() {
    // Get the total amount of currency notes and coins
    var note10000 = parseInt(document.getElementById('Notes_note10000').value) || 0;
    var note5000 = parseInt(document.getElementById('Notes_note5000').value) || 0;
    var note2000 = parseInt(document.getElementById('Notes_note2000').value) || 0;
    var note1000 = parseInt(document.getElementById('Notes_note1000').value) || 0;
    var note500 = parseInt(document.getElementById('Notes_note500').value) || 0;
    var coin500 = parseInt(document.getElementById('Notes_coin500').value) || 0;
    var coin100 = parseInt(document.getElementById('Notes_coin100').value) || 0;
    var coin50 = parseInt(document.getElementById('Notes_coin50').value) || 0;
    var coin25 = parseInt(document.getElementById('Notes_coin25').value) || 0;
    var coin10 = parseInt(document.getElementById('Notes_coin10').value) || 0;
    var coin5 = parseInt(document.getElementById('Notes_coin5').value) || 0;
    var coin1 = parseInt(document.getElementById('Notes_coin1').value) || 0;

    // Calculate total amount
    var totalAmount = (note10000 * 10000) + (note5000 * 5000) + (note2000 * 2000) + (note1000 * 1000) +
        (note500 * 500) + (coin500 * 500) + (coin100 * 100) + (coin50 * 50) + (coin25 * 25) + (coin10 * 10) + (coin5 * 5) + coin1;
    console.log(totalAmount);
    // Format total amount as currency
    var formattedTotalAmount = totalAmount.toLocaleString('en-US', { style: 'currency', currency: 'XAF' });
    $("#totalNoteAmount").val(totalAmount);
    console.log(formattedTotalAmount);
    
    $("#lblBalanceDeposit").val(totalAmount);
    // Update the lblDepositRequest_amount span with the formatted total amount
    var element = document.getElementById("lblDepositRequest_amount");
    if (element) { element.textContent = "Total Amount: " + formattedTotalAmount; } 
  


    ////Primary teller

    //// Get total note amount
    //var totalNoteAmount = parseFloat(document.getElementById("totalNoteAmount").value);
    //var totalProvision = parseFloat(document.getElementById("totalProvision").value);
    //// Calculate balance
    //var balance = totalNoteAmount - totalProvision ;
    //console.log(totalNoteAmount);

    // Format balance with commas and one decimal place
    var formattedTotalAmount = formattedTotalAmount.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
    console.log(totalAmount);
    // Display balance
   
    document.getElementById("lblBalanceDeposit").innerText = "Balance: " + formattedTotalAmount;
    // Check if balance is 0 and enable/disable the save button accordingly
    var btnSave = document.getElementById("btnSave");
    if (totalAmount === 0) {
        btnSave.disabled = true; // Enable save button
    } else {
        btnSave.disabled = false; // Disable save button
    }

    
}



