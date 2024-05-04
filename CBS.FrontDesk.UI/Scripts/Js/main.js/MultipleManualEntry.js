$(document).ready(function () {
    LoadDataEntryDT('PostedEntriesDataTable');
    //
    $(document).on('change', '#EntryTempData_AccountId', function () {
        var EventId = $(this).val();

        loadAccountBalance(EventId);


    });
    $(document).on('change', '#EntryTempData_BookingDirection', function () {
        var EventId = $(this).val();

        loadDescriptionByOperationDirection(EventId);
    });
});

function loadAccountBalance(accountId) {

    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountBalance',
        type: 'GET',
        dataType: 'json',
        data: { Id: accountId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo 
            $('.account_balance').empty();
            $('.account_name').empty();
            $('.account_number').empty();
            $(".account_balance").val(data.Account.CurrentBalance);
            $(".account_name").val(data.Account.AccountName);
            $(".account_number").val(data.Account.AccountNumber);
            // Add new options based on the fetched data
            console.log(data.Account.CurrentBalance);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}


function loadDescriptionByOperationDirection(operation) {
    $('.booking_direction').empty();
    if (operation === "DEBIT") {

        var account_name = $(".account_name").val();
        var account_amount = $(".account_amount").val();
        var account_number = $(".account_number").val();
        var message = account_name + "-" + account_number + " will be debited by {0}.0 XFA "
        $(".booking_direction").val(message);

    } else {
        var account_name = $(".account_name").val();
        var account_amount = $(".account_amount").val();
        var account_number = $(".account_number").val();
        var message = account_name + "-" + account_number + " will be credited by {0}.0 XFA "
        $(".booking_direction").val(message);
    }


}
function loadAccountingEntries(reference)
{
    alert(reference);
    $('#exampleModalLabel3').val("Loading ***");

    $.ajax({
        url: '/ManuallyJournalEntry/GetAllEntriesForJournalEntryReference',
        type: 'GET',
        dataType: 'json',
        data: { Id: reference },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo

            $('#exampleModalLabel3').empty();
            var description = $('.' + reference + '-Description').val();
            // Append text to the modal title
            $('#exampleModalLabel3').append('Description :' + description);

            // Populate the table with the fetched data
            var tableBody = $('#ReferenceEntriesDataTable tbody');
            tableBody.empty(); // Clear existing rows

            $.each(data, function (index, item) {
                var row = $('<tr>');
                row.append($('<td>').text(item.AccountName));
                row.append($('<td>').text(item.AccountNumber));

                if (item.bookingDirection.toLowerCase() === 'debit') {
                    row.append($('<td>').text(item.Amount.toFixed(2)));
                    row.append($('<td>').text('0.00'));
                } else {
                    row.append($('<td>').text('0.00'));
                    row.append($('<td>').text(item.Amount.toFixed(2)));
                }

                tableBody.append(row);
            });

            // Append text to the modal title
            $('#selectedId').val(id);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function LoadJournalEntryData(controller, action, divLoader, tableID, serviceoption, KEY, partialView, path, order) {
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
function DeleteDataJournal(controller, KEY, tableID, partialView, order, divToLoadTheData, serviceOption) {
    var Id = $('#EntryTempData_Reference').val();
    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY + "&serviceOption=" + serviceOption;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);

                        LoadJournalEntryData(controller, "InitializeData", divToLoadTheData, tableID, serviceOption, KEY, partialView, "list", order);
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
function AjaxPostAndUpdateJournalEntry(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
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
                    LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable", "EntryTempData", "", "_JournalEntries", "list", "desc");

                }
                else {

                    appalert(response.message, 3, 1);
                    LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable", "EntryTempData", "", "_JournalEntries", "list", "desc");


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

function postingconfirmTransactions(title, message, ajaxUrl, data) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable", "EntryTempData", "", "_JournalEntries", "list", "desc");

                    }
                    else {
                        appalert(response.message, 3, 1);

                        LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable", "EntryTempData", "", "_JournalEntries", "list", "desc");

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

function collectpostEntries() {
    const formData = {
        ServiceOption: $("#ServiceOptionEntryDescription").val(),
        EntryDescription: {
            Description: $("#EntryDescription_Description").val()
        },
        EntryTempDataResult: []
    };

    if ($('#EntryTempDataResult_0__Reference').length > 0) {
        formData.EntryTempDataResult.push({
            Reference: $('#EntryTempDataResult_0__Reference').val()
        });
    }

    formData.Action = $("#Action").val();
    return formData;
}
function collectEntries() {
    const formData = {
        ServiceOption: "EntryTempData",
        EntryTempData: {
            Reference: $("#EntryTempData_Reference").val(),
            AccountId: $("#EntryTempData_AccountId").val(),
            AccountBalance: $("#EntryTempData_AccountBalance").val(),
            AccountName: $(".account_name").val(),
            AccountNumber: $("#EntryTempData_AccountNumber").val(),
            BookingDirection: $("#EntryTempData_BookingDirection").val(),
            Description: $("#EntryTempData_Description").val(),
            Amount: $("#EntryTempData_Amount").val()
        },
        Action: $("#Action").val()

    };
    return formData;
}
function DoPosting() {
    var entry = collectpostEntries();
    if (entry.EntryTempDataResult[0].Reference === null || entry.EntryTempDataResult[0].Reference === "") {
        appalert("Please kindly enter the reference number for the entry", 3, 1);
        return;
    }
    if (entry.EntryDescription.Description === '') {
        appalert("Please kindly enter the purpose of the entries", 3, 1);
        return;
    }
    var message = "WARNING!!!\n";
    message += "Are you sure you want to confirm this various account adjustment?\n";
    postingconfirmTransactions('Confirm Manual Entry Operation', message, '/ManuallyJournalEntry/AddOrUpdate', entry);
}
function PostEntries() {


    var entry = collectEntries();
    if (entry.EntryTempData.Reference === null || entry.EntryTempData.Reference === "") {
        appalert("Please kindly enter the reference number", 3, 1);
        return;
    }
    var message = "WARNING!!!\n";
    message += "Are you sure you want to add a " + entry.EntryTempData.BookingDirection + " manual entry of XAF" + entry.EntryTempData.Amount + " into " + entry.EntryTempData.AccountNumber + "-" + entry.EntryTempData.AccountName + "?\n";
    postingconfirmTransactions('Confirm Manual Entry Operation', message, '/ManuallyJournalEntry/AddOrUpdate', entry);
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
function updateStatus(status) {
    if (status === "Under Review") {
        $("#under_review").show();
        $("#rejected").hide();
        $("#approved").hide();
    } else if (status === "Rejected") {
        $("#under_review").hide();
        $("#rejected").show();
        $("#approved").hide();
    } else if (status === "Posted") {
        $("#under_review").hide();
        $("#rejected").hide();
        $("#approved").show();
    }
}
function ApproveJournalEntryTransactions(title, message, ajaxUrl, serviceoption, Response, Id,comment) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'GET',
                contentType: 'application/json',
                data: { Id: Id, HasApproved: Response, Comment: comment },
                success: function (response) {

                    appalert(response.message, 3, 1);

                },
                error: function () {

                    appalert(response.message, 0, 3);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

function loadAccountingEntries(reference) {
    $("#under_review").hide();
    $("#rejected").hide();
    $("#approved").hide();
    $('#exampleModalLabel3').val("Loading ***");

    $.ajax({
        url: '/ManuallyJournalEntry/GetAllEntriesForJournalEntryReference',
        type: 'GET',
        dataType: 'json',
        data: { Id: reference },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo


            $('#exampleModalLabel3').empty();
            var descriptionCell = $("#" + reference + "-Description").text();
            var branchCodeCell = $("#" + reference + "-BranchCode").text();
            var createdDateCell = $("#" + reference + "-CreatedDate").text();
            var createdByCell = $("#" + reference + "-CreatedBy").text();
            var descriptionCell = $("#" + reference + "-Description").text();
            var statusCell = $("#" + reference + "-Status").text();
            console.log(descriptionCell);
            // Append text to the modal title
            $('#exampleModalLabel3').append('Description :' + descriptionCell);
            $("#issuer").text(createdByCell);
            $("#branchCode").text(branchCodeCell);
            $("#dateIssued").text(createdDateCell);
            $("#status").text(statusCell);
            $("#referenceID").text("Journal Entries Reference:" + reference);
            // Populate the table with the fetched data
            var tableBody = $('#ReferenceEntriesDataTable tbody');
            tableBody.empty(); // Clear existing rows

            $.each(data, function (index, item) {
                var row = $('<tr>');
                row.append($('<td>').text(item.AccountName));
                row.append($('<td>').text(item.AccountNumber));

                if (item.BookingDirection.toLowerCase() === 'debit') {
                    row.append($('<td>').text(item.Amount.toFixed(2)));
                    row.append($('<td>').text('0.00'));
                } else {
                    row.append($('<td>').text('0.00'));
                    row.append($('<td>').text(item.Amount.toFixed(2)));
                }

                tableBody.append(row);
            });

            // Append text to the modal title
            $('#selectedId').val(reference);
            console.log(statusCell);
            updateStatus(statusCell);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function ApproveJournalEntry(response) {
    //comment_description
    var storedId = $("#selectedId").val();
    var comment = $("#comment_description").val();

    if (comment === "")
    {
  
        appalert("Please kindly enter your comment for this entry with referenceId:" + storedId+" before you continue", 3, 1);
        return;
    } else {
        var ServiceOption = "EntryTempData";
        var message = "WARNING!!!\n";
        message += "Are you sure you want to confirm this various account adjustment?\n";
        ApproveJournalEntryTransactions('Confirm Manual Entry Operation', message, '/ManuallyJournalEntry/ApproveEntries', ServiceOption, response, storedId,comment);

    }
    }
function postentries()
{
    // Get the form element
    const form = document.getElementById('myForm');

    // Add event listener for form submission
    form.addEventListener('submit', function (event) {
        // Prevent default form submission
        event.preventDefault();

        // Get form field values
        const name = document.getElementById('name').value;
        const email = document.getElementById('email').value;

        // Validate form fields
        if (name.trim() === '' || email.trim() === '') {
            // Display validation error
            alert('Please fill in all required fields.');
        } else {
            // Validation passed, submit the form
            form.submit();
        }
    });
}



function LoadDataEntryDT(tableID) {


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
            { "targets": 0, "searchable": true, "orderable": true, "width": "5%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 6, "searchable": true, "orderable": true, "width": "10%" },


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
