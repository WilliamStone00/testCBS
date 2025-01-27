let basket = [];
let tableJE;

$(document).ready(function ()
{
    GetSequenceReference();
    LoadPendingPostedEntries("myDataTable")
    $("#datalistingview_AddEntryPurpose").hide();
    tableJE = $('#DataEntrybasketTable').DataTable({

        columns: [{
            data: 'accountName'
        }, {
            data: 'debit',
            render: function (data, type, row) {
                // Check if the booking direction is 'debit', if true, show '0' in debit column
                return (row.bookingDirection.toLowerCase() === 'debit') ? 0 : data;
            }
        }, {
            data: 'credit',
            render: function (data, type, row) {
                // Check if the booking direction is 'credit', if true, show '0' in credit column
                return (row.bookingDirection.toLowerCase() === 'credit') ? 0 : data;
            }
        }, {
            data: null,
            defaultContent: '<button class="btn btn-danger removeBtn">Remove</button>',
            orderable: false
        }
        ],
        columnDefs: [{
            width: '50%',
            targets: 0
        }, // ruleName
        {
            width: '20%',
            targets: 1
        }, // chartOfAccount
        {
            width: '20%',
            targets: 2
        }, // bookingDirection
        {
            width: '10%',
            targets: 3
        }
        ],
        autoWidth: false // This is important to enforce our custom widths
    });

    $(document).on('change', '#EntryTempData_AccountId', function () {
        // Get the selected value (AccountId) from the dropdown
        var selectedValue = $(this).val();
        // Call the `loadAccountById` function with the selected AccountId
        loadAccountById(selectedValue);
    });
    $('#DataEntrybasketTable tbody').on('click', 'button.removeBtn', function () {
        const data = tableJE.row($(this).parents('tr')).data();
        removeItem(data);
    });
    // Posted entry request approval 
  

    $(document).on('change', '#EntryBookingDirection', function () {
        var EventId = $(this).val();

        loadDescriptionByOperationDirection(EventId);
    });
});

function GetSequenceReference() {
 
    $.ajax({
        url: '/ManuallyJournalEntry/GetSequenceReference',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            console.log(data);
            $('#EntryTempData_Reference').val(data);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function loadPostedEntryByReference(reference)
{
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

            console.log(data);
            
            $('#exampleModalLabel3').empty();
            var referenceCell = $("#" + reference + "-Reference").text();
            var branchCodeCell = $("#" + reference + "-BranchCode").text();
            var createdDateCell = $("#" + reference + "-CreatedDate").text();
            var createdByCell = $("#" + reference + "-CreatedBy").text();
            var description = $("#" + reference + "-Description").text();
            var statusCell = $("#" + reference + "-Status").text();
          
            const parts = description.split('*');
            description = parts[0];
            var ApprovedDateCell = parts[1];
            var ApprovedByCell = parts[2];
            // Append text to the modal title
            $('#exampleModalLabel3').append('Reference :' + referenceCell);
            $("#issuer").text(createdByCell);
            $("#branchCode").text(branchCodeCell);
            $("#dateIssued").text(createdDateCell);
            $("#status").text(statusCell);
            $("#approvedDate").text(ApprovedDateCell);
            $("#approvedBy").text(ApprovedByCell);
            $("#description").text(description);
            $("#referenceID").text("Journal Entries Reference:" + reference);
            // Populate the table with the fetched data
            var tableBody = $('#ReferenceEntriesDataTable tbody');
            tableBody.empty(); // Clear existing rows

            $.each(data.EntryDetail, function (index, item) {
                let amount = parseFloat(item.Amount);
                var row = $('<tr>');
                row.append($('<td>').text(item.AccountName));
                row.append($('<td>').text(item.AccountNumber));

                if (item.BookingDirection.toLowerCase() === 'debit') {
                    row.append($('<td>').text(amount.toFixed(2)));
                    row.append($('<td>').text('0.00'));
                } else {
                    row.append($('<td>').text('0.00'));
                    row.append($('<td>').text(amount.toFixed(2)));
                }

                tableBody.append(row);

            });

            // Append text to the modal title
            $('#selectedId').val(reference);
            console.log(statusCell);
            updatePageStatus(statusCell);

        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
function ApprovePostedEntries(response) {
    //comment_description
    var storedId = $("#selectedId").val();
    var comment = $("#comment_description").val();

    if (comment === "") {

        appalert("Please kindly enter your decision for this entry with referenceId:" + storedId + " before you continue", 2, 1);
        return;
    } else {
        var ServiceOption = "EntryTempData";
        var message = "WARNING!!!\n";
        message += "Are you sure you want to confirm this various account adjustment?\n";
        ApprovePostedEntriesTransactions('Confirm Manual Entry Operation', message, '/ManuallyJournalEntry/ApproveEntries', ServiceOption, response, storedId, comment);

    }
}
function ApprovePostedEntriesTransactions(title, message, ajaxUrl, serviceoption, Response, Id, comment) {
    alertify.confirm(title, message,
        function () {
            $.ajax({
                url: ajaxUrl,
                type: 'GET',
                contentType: 'application/json',
                data: { Id: Id, HasApproved: Response, Comment: comment },
                success: function (response) {

                    appalert(response.message, 3, 1);
                    setTimeout(function () {
                        window.location.reload();
                    }, 20000);
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

function updatePageStatus(status) {
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
function extractAccountDetails(fullObject) {
    // Check if Account object exists
    if (!fullObject.Account) {
        console.error('No Account object found');
        return null;
    }

    // Create a new object with specified properties
    const accountDetails = {
        AccountNumberCU: fullObject.Account.AccountNumberCU || null,
        Id: fullObject.Account.Id || null,
        CurrentBalance: fullObject.Account.CurrentBalance || null,
        AccountName: fullObject.Account.AccountName || null
    };
    console.log(accountDetails);
    return accountDetails;
}
function loadAccountById(AccountId) {
    console.log(AccountId);
    // Make an AJAX GET request to the server
    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountBalance', // API endpoint
        type: 'GET', // HTTP method
        dataType: 'json', // Expected response format
        data: { Id: AccountId }, // Send the selected AccountId as a parameter
        success: function (data) {
            // Process the response data
            var model = extractAccountDetails(data);
            // Update input fields with returned account details
            $('#EntryTempData_AccountBalance').val(model.CurrentBalance);
            $('#EntryTempData_AccountName').val(model.AccountName);
            $('#EntryTempData_AccountNumber').val(model.AccountNumberCU);
            $('#EntryTempData_AccountId').val(model.Id);
        },
        error: function (xhr, status, error) {
            // Log any errors from the AJAX request
            console.error(xhr.responseText);
        }
    });
}
 
function calculateDebitAndCreditTotals(entries) {
    // Initialize totals
    let totalDebit = 0;
    let totalCredit = 0;

    // Iterate through each entry in the collection
    entries.forEach(entry => {
        const amount = parseFloat(entry.amount || 0); // Ensure the amount is a valid number
        const bookingDirection = entry.bookingDirection?.toLowerCase();

        if (bookingDirection === "debit") {
            totalDebit += amount; // Add to debit total
        } else if (bookingDirection === "credit") {
            totalCredit += amount; // Add to credit total
        }
    });
    let balance = totalCredit - totalDebit;
    let balancePrefix = balance >= 0 ? "DR"  : "CR";
    $('#totalDebit').text(totalDebit);
    $('#totalCredit').text(totalCredit);
    let response = balance === 0 ? "" : balancePrefix;
    $('#balance').text((response +balance));
}


let manuallyJournalEntryDataSet = createManuallyJournalEntryDataSet();
/**
 * Adds an item to the shopping basket with validation
 * @returns {void}
 */
function addToBasket()
{
    // Get form values
    const item = {
        reference: $('#EntryTempData_Reference').val().trim(),
        accountBalance: $('#EntryTempData_AccountBalance').val(),
        accountId: $('#EntryTempData_AccountId').val(),
        accountName: $('#EntryTempData_AccountName').val(),
        accountNumber: $('#EntryTempData_AccountNumber').val(),
        bookingDirection: $('#EntryTempData_BookingDirection option:selected').val(),
        amount: $('#EntryTempData_Amount').val(),
        credit: $('#EntryTempData_Amount').val(),
        debit: $('#EntryTempData_Amount').val(),
        description: $('#EntryTempData_Description').val().trim()
    };

    // Format account name and reference
    item.accountName = `${item.accountNumber}-${item.accountName}`;
    item.reference = `${item.reference}`;

    console.log('Processing item:', item);

    // Validation checks
    const validationErrors = {
        'bookingDirection': {
            condition: item.bookingDirection === "---Select Direction---" || item.bookingDirection === "",
            message: 'No booking direction has been set. Please contact administrators for help.'
        },
        'accountId': {
            condition: item.accountId === "---Select Account---" || item.accountId === "", 
            message: 'No account number has been selected. Please contact administrators for help.'
        },
        'reference': {
            condition: !item.reference ,
            message: 'No entry reference has been set. Please contact administrators for help.'
        },
        'description': {
            condition: !item.description,
            message: 'A description is mandatory. Please contact administrators for help.'
        }
    };

    // Check for validation errors
    for (const [field, check] of Object.entries(validationErrors)) {
        if (check.condition) {
            appalert(check.message, 2, 1);
            return;
        }
    }

    // Validate debit transaction
    if (!validateDebitTransaction(item)) {
        appalert('The account balance is insufficient for this entry', 2, 1);
        return;
    }

    // Check for duplicate account
    const existingItem = basket.find(x => x.accountId === item.accountId);
    if (existingItem) {  // Changed from checking if null to checking if exists
        appalert(`The entry list already contains a value for account ${item.accountName}`, 2, 1);
        return;
    }

    // Add item to basket and update UI
    basket.push(item);
    updateBasketDisplay();
    calculateDebitAndCreditTotals(basket);

    // Update form state
    const isBasketEmpty = basket.length === 0;
    $('#EntryTempData_Reference, #EntryTempData_Description').prop('disabled', !isBasketEmpty);
    $('#basket_Label').text(`Reference: ${item.reference}`);

    // Clear form fields
    $('#EntryTempData_BookingDirection').val('').change();
  
}

function validateDebitTransaction(item) {
    const accountBalance = parseFloat(item.accountBalance || 0);
    const amount = parseFloat(item.amount || 0);
    const bookingDirection = item.bookingDirection; // Debit or Credit
    console.log(amount + ' current balance =' + accountBalance);
    // Check if booking direction is Debit
    if (bookingDirection.toLowerCase() === 'debit') {

        if (accountBalance - amount > 0) {

            return true; // Invalid transaction
        } else {
            console.log('Insufficient account balance for the debit transaction.');
            return false;
        }
    }

    // If all checks pass
    return true;
}
function createManuallyJournalEntryDataSet(basket)
{
    return {
        entryTempData: {
            // Add properties as needed
        },
        account: {
            // Add properties as needed
        },
        entryDescription: {
            // Add properties as needed
        },
        entryTempDatas: [],
        postedEntries: [],
        entryTempDataResult: [],
        accounts: [],
        EntryTempDatas: basket,
        EntryTempData: {
            // Add properties as needed
        },
        serviceOption: "CreateAccountingEntries",
        action: "insert",
        key: "",
        hasApproved: false
    };
}
function validateBasketDirections(basket) {
    console.log(basket);
    let hasCredit = false;
    let hasDebit = false;
    let resultA = false;
    basket.forEach(item => {
        if (item.bookingDirection === 'CREDIT') {
            hasCredit = true;
        } else if (item.bookingDirection === 'DEBIT') {
            hasDebit = true;
        }
    });
    resultA = hasCredit && hasDebit;
    resultB = checkDoubleEntryPrinciple(basket);
    return resultA && resultB;
}
function updateBasketDisplay() {
    tableJE.clear();
    tableJE.rows.add(basket).draw();
}

function removeItem(item) {
    const index = basket.findIndex(i =>
        i.accountName === item.accountName &&
        i.bookingDirection === item.bookingDirection &&
        i.description === item.description
    );
    if (index > -1) {
        basket.splice(index, 1);
        updateBasketDisplay();
        calculateDebitAndCreditTotals(basket);
    }
}
function submitBasket() {
    if (basket.length === 0) {
        appalert('No Accounting entry rule has been set. Please contact administrators.', 2, 1);
        return;
    }

    if (validateBasketDirections(basket))
    {

        alertify.confirm("T R U S T S O F T C R E D I T", "The system is about to submit your accounting entries with referenceId : " + basket[0].reference + ".\nAre sure you want to persit this operation?",
            function () {
                console.log(createManuallyJournalEntryDataSet(basket));
                // If the user confirms, proceed with the submission
                $.ajax({
                    type: 'POST',
                    url: '/ManuallyJournalEntry/AddOrUpdate',
                    contentType: 'application/json',
                    data: JSON.stringify(createManuallyJournalEntryDataSet(basket)),

                    success: function (response) {
                        if (response.success) {

                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            } else {
                                $('#DataEntrybasketTable').DataTable().clear().draw();
                       
                                location.reload();
                               /* appalert(response.message, 1, 1);*/
                            }

                            if (response.reloadDataView === "Yes") {
                                if (response.option === 'Update') {
                                    LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                                } else if (response.optype === 'Insert') {
                                    EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                                } else {
                                    LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                                }
                            }
                        } else {
                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else {
                                appalert(response.message, 2, 1);
                            }
                        }
                    },
                    error: function (err) {
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                });
            },
            function () {
                appalert('Transaction cancelled', 3, 1);
            }
        );
    } else {
        alertify.confirm("T R U S T S O F T C R E D I T","Your accounting entries are not balance. Please contact administrators for assitances.", 1, 2);
        return;
    }

    // Confirmation dialog using alertify

}


function updateTotals() {
    const totalDebit = $('#AccountingEventEntriesDataTable .debit').toArray().reduce((sum, input) => {
        return sum + parseFloat($(input).val());
    }, 0);
    const totalCredit = $('#AccountingEventEntriesDataTable .credit').toArray().reduce((sum, input) => {
        return sum + parseFloat($(input).val());
    }, 0);

    $('#totalDebit').text(totalDebit.toFixed(2));
    $('#totalCredit').text(totalCredit.toFixed(2));
    $('#totalBalance').text("Total:" + (totalCredit - totalDebit).toFixed(2));
    // Show or hide the description row based on the totals
    if (totalDebit === totalCredit) {
        if ($('#descriptionRow').length === 0) {
            $('#AccountingEventEntriesDataTable').append(
                `<tr id="descriptionRow">
                    <td colspan="4">
                        <textarea id="operationDescription" placeholder="Enter description of the operation" style="width: 100%;"></textarea>
                    </td>
                </tr>`
            );
        }
        $('#descriptionRow').show();
    } else {
        $('#descriptionRow').hide();
    }
}


function LoanAccountingEventEntrySystemId(system_Id, yourModalId) {
    if (system_Id === 0 || system_Id === null || system_Id === undefined) {
        appalert('There is no system id present on this record', 1, 2);
        return;
    }

    fetch('/ManuallyJournalEntry/GetAccountingEntryEventID/?system_Id=' + system_Id, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response => response.json())
        .then(data => {
            console.log('Success:', data);
            InitiliseDataTable(data);
            openModalWithData(data, yourModalId);
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while retrieving data.');
        });
}

function openModalWithData(data, yourModalId) {
    const modalElement = document.getElementById(yourModalId);
    modalElement.querySelector('.modal-body').innerHTML = JSON.stringify(data, null, 2);
    $('#' + yourModalId).modal('show');
}


//function LoanAccountingEventEntrySystemId(system_Id) {

//    $.ajax({
//        url: '/ManuallyJournalEntry/GetAccountingEntryEventID/',
//        type: 'Get',
//        dataType: 'json',
//        data: { system_Id: system_Id },
//        success: function (data) {
//            console.log($("#" + system_Id + "-RuleName").text());
//            $('#exampleModalLabel3').empty();
//            //var description = $("#" + branchId + "-RuleName").text();   
//            if (data.HasError == true) {
//                var names = "Your branch is missing some accounts needed to record " + $("#" + system_Id + "-RuleName").text() + " transactions.";
//                $("#exampleModalLabel3").text(names);
//                document.querySelector('#exampleModalLabel3').classList.add('text-danger');

//            } else {
//                var names = "Accounting Event Entry for " + $("#" + system_Id + "-RuleName").text() + ".";

//                $("#exampleModalLabel3").text(names);
//                var table;
//                InitiliseDataTable(data);
//            }






//        },
//        error: function (xhr, status, error) {
//            console.error(xhr.responseText);
//        }
//    });
//}

function LoadPendingPostedEntries(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [
        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "5%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "19%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "1%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 6, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 7, "searchable": true, "orderable": true, "width": "10%" },
     
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

function collectAndPostData() {
    const table = $('#AccountingEventEntriesDataTable').DataTable();
    const dataToSend = [];

    // Collect data from each row
    table.rows().every(function (rowIdx, tableLoop, rowLoop) {
        const rowData = this.data();
        const debitInput = this.nodes().to$().find('input.debit').val();
        const creditInput = this.nodes().to$().find('input.credit').val();
        const MFI_ChartOfAccountId = this.data().MFI_ChartOfAccountId;
        console.log(MFI_ChartOfAccountId);
        console.log(rowData);

        const entry = {

            Amount: parseFloat(rowData.BookingDirection.toUpperCase() === "DEBIT" ? debitInput : creditInput),
            BookingDirection: rowData.BookingDirection,

            MFI_ChartOfAccountId: rowData.MFI_ChartOfAccountId
        };

        dataToSend.push(entry);
    });

    // Collect the operation description if it exists
    const operationDescription = $('#operationDescription').val() || '';

    // Prepare the final data object
    const finalData = {
        Entries: dataToSend,
        Description: operationDescription,
        ReferenceId: $('#ReferenceId').val()
    };

    // Post the data to the server
    $.ajax({
        type: 'POST',
        url: '/ManuallyJournalEntry/PostAutoJournalEntries', // Replace with your server endpoint
        data: JSON.stringify(finalData),
        contentType: 'application/json',
        success: function (response) {
            console.log('Success:', response);
            alert('Data submitted successfully!');
        },
        error: function (error) {
            console.error('Error:', error);
            alert('An error occurred while submitting the data.');
        }
    });
}
$('#submitButton').on('click', function () {
    submitAccountingEntries();
});

function checkDoubleEntryPrinciple(entries) {
    console.log(entries);
    const debitTotal = entries
        .filter(entry => entry.bookingDirection.toUpperCase() === 'DEBIT')
        .reduce((sum, entry) => sum + parseFloat(entry.amount), 0);
    const creditTotal = entries
        .filter(entry => entry.bookingDirection.toUpperCase() === 'CREDIT')
        .reduce((sum, entry) => sum + parseFloat(entry.amount), 0);
    console.log(debitTotal + ' ' + creditTotal);
    return debitTotal === creditTotal;
}

function submitAccountingEntries() {
    const table = $('#AccountingEventEntriesDataTable').DataTable();
    const dataToSend = [];

    // Collect data from each row
    table.rows().every(function (rowIdx, tableLoop, rowLoop) {
        const rowData = this.data();
        const debitInput = this.nodes().to$().find('input.debit').val();
        const creditInput = this.nodes().to$().find('input.credit').val();
        const MFI_ChartOfAccountId = this.data().MFI_ChartOfAccountId;
        const entry = {
            AccountNumber: rowData.AccountNumber,
            Amount: parseFloat(rowData.BookingDirection.toUpperCase() === "DEBIT" ? debitInput : creditInput),
            BookingDirection: rowData.BookingDirection,
            Description: rowData.Description,
            MFI_ChartOfAccountId: rowData.MFI_ChartOfAccountId
        };
  //      const entry = createManuallyJournalEntryDataSet();
        dataToSend.push(entry);
    });

    // Collect the operation description if it exists
    const operationDescription = $('#operationDescription').val() || '';
 
    if (operationDescription === "") {
        appalert('The journal entry cannot be void of description. Please contact administrators.', 2, 1);
        return;
    }
    if (dataToSend.length === 0) {
        appalert('An empty journal cannot be posted. Please contact administrators.', 2, 1);
        return;
    }
    if (!checkDoubleEntryPrinciple(dataToSend)) {
        appalert('Your journal entries do not balance. Please contact administrator for support', 2, 1);
        return;
    }
    // Prepare the final data object
    const finalData = {
        Entries: dataToSend,
        Description: operationDescription,
        ReferenceId: $('#ReferenceId').val()
    };

    console.log(finalData);


    // Confirmation dialog using alertify
    alertify.confirm("T R U S T S O F T C R E D I T", "Are you sure you want to this accounting entries?",
        function () {
            // If the user confirms, proceed with the submission
            $.ajax({
                type: 'POST',
                url: '/ManuallyJournalEntry/PostAutoJournalEntries', // Replace with your server endpoint
                contentType: 'application/json',
                data: JSON.stringify(finalData),
                success: function (response) {
                    console.log(response);
                    console.log(response.MessageString);

                    if (response) {
                        if (response.MessageStatus === "Exist") {
                            appalert(response.MessageString, 3, 1);
                        } else if (response.MessageStatus === "Failed") {
                            appalert(response.MessageString, 2, 1);
                        } else {
                            appalert(response.MessageString, 1, 1);
                        }
                    } else {
                        if (response.Result.MessageStatus === "Exist") {
                            appalert(response.Result.MessageString, 3, 1);
                        } else {
                            appalert(response.Result.MessageString, 2, 1);
                        }
                    }
                },
                error: function (err) {
                    console.log(err);
                    appalert(err.statusText, 0, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );
}

function LoadEntryTempDataDataSetDT(tableID) {


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
            { "targets": 0, "searchable": true, "orderable": true, "width": "80%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" }


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
