$(document).ready(function () {

    
    LoadCashRequestDataBranch("CashRequestDataTable")
    //LoadCashRequestDataHo("myRequestDataTable")
    LoadCashReplenishmentDataDT("GetAllCashRequestDataTable")
    
    $(document).on('change', '#CorrespondingBranchID', function () {
 
        // Get the selected value
        var selectedValue = $(this).val();
        if (selectedValue === 'RedirectToBranch') {
            // Show the element
            $('#hideBranchID').show();
        } else {
            // Hide the element
            $('#hideBranchID').hide();
        }
    });

    $(document).on('change', '#AccountId', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        loadAccountBalance(selectedValue);
    });

});

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

function LoadInfomation(referenceId) {
    console.log(referenceId);

    $.ajax({
        url: '/CashFlowManagement/GetBankTransactionByReferenceId',
        type: 'GET',
        dataType: 'json',
        data: { referenceId: referenceId },
        success: function (data) {

            console.log(data);

            $('#exampleModalLabel3').empty();

            // Append text to the modal title
            $('#exampleModalLabel3').append('Bank Transaction Information:' + data.BankTransaction.Id);
            $('#CreatedBy').text(data.BankTransaction.CreatedBy);
            $('#CreatedDate').text(data.BankTransaction.CreatedDate);
            var AccountReferenceId = data.Account.AccountNumberCU + "-" + data.Account.AccountName;
            $('#AccountReferenceId').text(AccountReferenceId);
            $('#Balance').text("XAF" +data.BankTransaction.Balance);
            $('#BankTransactionDate').text(data.BankTransaction.ValueDate);
            $('#Amount').text("XAF"+data.BankTransaction.Amount);
            $('#CashRequestedBy').text(data.Cashreplenishment.IssuedBy);
            $('#CashRequestedAmount').text("XAF" +data.Cashreplenishment.AmountRequested);
            $('#CashRequestedDate').text(data.Cashreplenishment.IssuedDate);
            $('#CashApprovedBy').text(data.Cashreplenishment.ApprovedBy);
            $('#CashApprovedAmount').text("XAF" +data.Cashreplenishment.AmountApproved);
            $('#CashApprovedDate').text(convertMicrosoftDate(data.Cashreplenishment.ApprovedDate));
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
            { "targets": 0, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "10%" },
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

function LoadCashRequestDataBranch(tableID) {


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
            { "targets": 2, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "25%" },
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

function GetCashReplenimentRequest(Id) {
    $('#exampleModalLabel_CashRequestApproval').empty();

    $.ajax({
        url: '/TellerCashDemand/GetCashReplenimentRequest',
        type: 'GET',
        dataType: 'json',
        data: { KEY: Id },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            // Update the Reference ID
            console.log(data);
            var newReferenceId = data.id == null ? "Not Defined" : data.id;
            var newRequestedBy = data.requesterUserId == null ? "Not Defined" : data.requesterUserId;
            var newApprovedMessage = data.approvedComment === null ? "Not Defined" : data.approvedComment;
            var newApprovedBy = data.approvedByUserId === null ? "Not Defined" : data.approvedByUserId;
            var newApprovedAmount = data.confirmedAmount === null ? 0.0 : data.confirmedAmount;
            var newApprovedDate = data.approvedDate === null ? "Not Defined" : data.approvedDate;
            var newStatus = data.approvedStatus === null ? "Not Defined" : data.approvedStatus;

            var newRequestedAmount = data.approvedStatus === null ? "Not Defined" : data.approvedStatus;
            $('#exampleModalLabel_CashRequestApproval').text('Voucher ReferenceId: ' + data.id);
            $('#requestedBy').text(data.requesterUserId);
            // Update the table data
            $('#invoiceNumber').text(data.id);
            $('#newStatus').text(data.approvedStatus);
            $('#amountRequested').text('XFA ' + data.requestedAmount + '.0');
            $('#requestMessage').text(data.requetcomment);
            $('#requestedBy').text(data.requesterUserId);
            $('#approvedBy').text(newApprovedBy);
            $('#amountApproved').text('XFA ' + newApprovedAmount + '.0');
            $('#approvedDate').text(formatDate(newApprovedDate));
            $('#approvedMessage').text(newApprovedMessage);
            $('#requestedBy').text(data.requesterUserId);
            // Update the status badge
            var statusBadge = $('#statusBadge');
            if (newStatus.toUpperCase() === 'PENDING') {
                statusBadge.text('PENDING').removeClass('bg-label-success bg-label-danger').addClass('bg-label-primary');
            } else if (newStatus.toUpperCase() === 'APPROVE') {
                statusBadge.text('APPROVE').removeClass('bg-label-primary bg-label-danger').addClass('bg-label-success');
            } else {
                statusBadge.text('REJECTED').removeClass('bg-label-primary bg-label-success').addClass('bg-label-danger');
            }

            // Update the alert message
            var alertMessage = $('.alert-heading');
            if (newStatus.toUpperCase() === 'APPROVE') {
                alertMessage.text('The Cash replenishment request issued by ' + newRequestedBy + ' with reference: ' + newReferenceId + ' with Amount ' + newApprovedAmount + ' has already been approved.');
            } else if (newStatus.toUpperCase() === 'REJECTED') {
                alertMessage.text('The Cash replenishment request issued by ' + newRequestedBy + ' with reference: ' + newReferenceId + ' with Amount ' + newRequestedAmount + ' has already been rejected.');
            } else {
                alertMessage.text('ReferenceId: ' + newReferenceId + ' with Amount ' + newRequestedAmount + ' has not yet been approved');
            }

        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });

    // Helper function to format the date
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

            return `${day}-${month}-${year} ${hours}:${minutes}:${seconds}`;
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

            return `${day}-${month}-${year} ${hours}:${minutes}:${seconds}`;
        }
    }


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
    // Update the lblDepositRequest_amount span with the formatted total amount
    document.getElementById("lblDepositRequest_amount").textContent = "Total Amount: " + formattedTotalAmount;



    //Primary teller

    // Get total note amount
    var totalNoteAmount = parseFloat(document.getElementById("totalNoteAmount").value);
    var totalProvision = parseFloat(document.getElementById("totalProvision").value);
    // Calculate balance
    var balance = totalNoteAmount - totalProvision ;
    console.log(totalNoteAmount);

    // Format balance with commas and one decimal place
    var formattedBalance = balance.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
    console.log(formattedBalance);
    // Display balance
    document.getElementById("lblBalance").innerText = "Balance: " + formattedBalance;
    document.getElementById("lblBalanceDeposit").innerText = "Balance: " + formattedBalance;
    // Check if balance is 0 and enable/disable the save button accordingly
    var btnSave = document.getElementById("btnSave");
    if (balance === 0) {
        btnSave.disabled = false; // Enable save button
    } else {
        btnSave.disabled = true; // Disable save button
    }

    // Change balance color based on condition
    if (totalNoteAmount !==totalProvision) {
        document.getElementById("lblBalance").style.color = "red"; // Set red color for balance
        document.getElementById("lblBalanceDeposit").style.color = "black";
    } else {
        document.getElementById("lblBalance").style.color = "black"; // Set default color for balance
        document.getElementById("lblBalanceDeposit").style.color = "black";
    }
    // Change balance color only if balance is not zero and differs from provision amount
    if (balance !== totalProvision) {
        document.getElementById("lblBalance").style.color = "red"; // Set red color for balance
        document.getElementById("lblBalanceDeposit").style.color = "black";
    } else {
        document.getElementById("lblBalance").style.color = "black"; // Set black color for balance
        document.getElementById("lblBalanceDeposit").style.color = "black";
    }

    if (totalNoteAmount == 0 || totalNoteAmount == totalProvision) {
        document.getElementById("lblBalance").style.color = "black"; // Set black color for balance 
        document.getElementById("lblBalanceDeposit").style.color = "black";
    }
}



