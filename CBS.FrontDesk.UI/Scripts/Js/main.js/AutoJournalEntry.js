let basket = [];
let tableJE;

$(document).ready(function () {
    LoadAccountingRuleDataSetDT("AccountingEventxxDataTable")

    tableJE = $('#AccountingRulebasketTable').DataTable({
        columns: [
          /*  { data: 'ruleName' },*/
            { data: 'MFI_ChartOfAccountId' },
            { data: 'bookingDirection' },
      /*      { data: 'description' },*/
            {
                data: null,
                defaultContent: '<button class="btn btn-danger removeBtn">Remove</button>',
                orderable: false
            }
        ],
        columnDefs: [
         /*   { width: '15%', targets: 0 },  // ruleName*/
            { width: '70%', targets: 0 },  // chartOfAccount
            { width: '15%', targets: 1 },   // bookingDirection
          /*  { width: '40%', targets: 3 },  // description*/
            { width: '15%', targets: 2 }    // remove button
        ],
        autoWidth: false  // This is important to enforce our custom widths
    });

    $('#AccountingRulebasketTable tbody').on('click', 'button.removeBtn', function () {
        const data = tableJE.row($(this).parents('tr')).data();
        removeItem(data);
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
            $('#ReferenceId').val(data);
            $('.Reference').text("Reference:" + data)
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
let manuallyJournalEntryDataSet = createManuallyJournalEntryDataSet();
function addToBasket() {
    const item = {
        ruleName: $('#AccountingRule_RuleName').val(),
        MFI_ChartOfAccountId: $('#AccountingRule_MFI_ChartOfAccountId option:selected').text(),
        bookingDirection: $('#AccountingRule_BookingDirection option:selected').text(),
        IsValidationNeed: $('#AccountingRule_IsValidationNeed').val(),
        ListOfEligibleBranchId: $('#AccountingRule_ListOfEligibleBranchId').val(),
        EntryType: $('#AccountingRule_EntryType').val(),
        LevelOfExecution: $('#AccountingRule_LevelOfExecution').val()
    };

    console.log(item);

    // Validation checks
    if (item.bookingDirection === "---Select Direction---") {
        appalert('No booking direction has been set for the entry rule. Please kindly contact administrators for help', 2, 1);
        return;
    } else if (item.MFI_ChartOfAccountId === "---Select ChartOfAccount---") {
        appalert('No ChartOfAcccount number has been selected for the entry rule. Please kindly contact administrators for help', 2, 1);
        return;
    } else if (item.ruleName === "") {
        appalert('No Entry RuleName has been set for the entry rule. Please kindly contact administrators for help', 2, 1);
        return;
    }

    // Add item to basket and update display
    basket.push(item);
    updateBasketDisplay();

    // Update form state after adding to basket
    $('#AccountingRule_RuleName').prop('disabled', true);
    $('#AccountingRule_IsValidationNeed').prop('disabled', true);
    $('#ListOfEligibleBranchId').prop('disabled', true);
    $('#EntryType').prop('disabled', true);
    $('#LevelOfExecution').prop('disabled', true);

    // Update basket label and clear relevant form fields
    $('#basket_Label').text('Event: ' + item.ruleName);
    $('#AccountingRule_MFI_ChartOfAccountId').val('').change();
    $('#AccountingRule_BookingDirection').val('').change();
 
}
function createManuallyJournalEntryDataSet(basket) {
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
        accountingRules: basket,
        accountingRule: {
            // Add properties as needed
        },
        serviceOption: "",
        action: "",
        key: "",
        hasApproved: false
    };
}
function validateBasketDirections(basket) {
    let hasCredit = false;
    let hasDebit = false;

    basket.forEach(item => {
        if (item.bookingDirection === 'CREDIT') {
            hasCredit = true;
        } else if (item.bookingDirection === 'DEBIT') {
            hasDebit = true;
        }
    });

    return hasCredit && hasDebit;
}
function updateBasketDisplay() {
    tableJE.clear();
    tableJE.rows.add(basket).draw();
}

function removeItem(item) {
    const index = basket.findIndex(i =>     
        i.chartOfAccount === item.chartOfAccount &&
        i.bookingDirection === item.bookingDirection 
     /*   &&   i.ruleName === item.ruleName &&  i.description === item.description*/
    );
    if (index > -1) {
        basket.splice(index, 1);
        updateBasketDisplay();
    }
}
function submitBasket() {
    if (basket.length === 0) {
        appalert('No Accounting entry rule has been set. Please contact administrators.', 2, 1);
        return;
    }
    if (validateBasketDirections(basket)) {
        alertify.confirm("T R U S T S O F T C R E D I T", "Are you sure you want to submit the element in accounting event entry rule is correct?",
            function () {
                // If the user confirms, proceed with the submission
                $.ajax({
                    type: 'POST',
                    url: '/ManuallyJournalEntry/AddAccountingEntryRule',
                    contentType: 'application/json',
                    data: JSON.stringify(createManuallyJournalEntryDataSet(basket)),

                    success: function (response) {
                        if (response.success) {

                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            } else {
                                $('#AccountingRulebasketTable').DataTable().clear().draw();
                                appalert(response.message, 1, 1);
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
        appalert('No Accounting entry rule must contain atleast 1 DEBIT AND 1 CREDIT booking direction. Please contact administrators for assitances.', 1, 2);
        return;
    }

    // Confirmation dialog using alertify

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
function loadPostedEntriesByReference(reference) {
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

            const cellsparts = createdByCell.split('*');
            createdByCell = cellsparts[0];
      
            branchCodeCell = cellsparts[2];

            // Append text to the modal title
            $('#exampleModalLabel3').append('Reference :' + referenceCell);
            $("#issuer").text(createdByCell);
            $("#branchCode").text(branchCodeCell);
            $("#dateIssued").text(createdDateCell);
         
            let td = $('#status');
            let cleanStatus = statusCell.trim().toUpperCase();
            if (cleanStatus.toUpperCase() === "PENDING") {
                td.html(`<span class="badge rounded-pill bg-dark fs-6">
       <i class="fas fa-check-circle"></i> ${cleanStatus.toUpperCase()}</span>`) ;
            } else if (cleanStatus.toUpperCase() === "REJECTED") {
                td.html(`<span class="badge rounded-pill bg-danger fs-6">
       <i class="fas fa-times-circle"></i> ${cleanStatus.toUpperCase()}</span>`);
            } else if (cleanStatus.toUpperCase() === "APPROVED") {
                td.html(`<span class="badge rounded-pill bg-success fs-6">
       <i class="fas fa-check-circle"></i> ${cleanStatus.toUpperCase()}</span>`);
            }
            console.log("Status:", cleanStatus);
            $("#status").text(cleanStatus.toUpperCase());
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
                //row.append($('<td>').text(item.AccountNumber));

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
            console.log(cleanStatus);
            updatePageActionButton(cleanStatus);

        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}

function updatePageActionButton(status) {
    if (status.toUpperCase() === "PENDING") {
        $("#under_review").show();
        $("#rejected").hide();
        $("#approved").hide();
    } else if (status.toUpperCase() === "REJECTED") {
        $("#under_review").hide();
        $("#rejected").show();
        $("#approved").hide();
    } else if (status.toUpperCase() === "APPROVED") {
        $("#under_review").hide();
        $("#rejected").hide();
        $("#approved").show();
    }
}
function InitiliseDataTable(serverResponse) {
    // Destroy the existing table instance
    if ($.fn.DataTable.isDataTable('#AccountingEventEntriesDataTable')) {
        $('#AccountingEventEntriesDataTable').DataTable().destroy();
    }

    // Initialize the DataTable
    const table = $('#AccountingEventEntriesDataTable').DataTable({
        data: serverResponse,
        destroy: true,
        columns: [
            { data: 'AccountNumber' },
            {
                data: 'Amount',
                render: function (data, type, row) {
                    if (row.BookingDirection.toUpperCase() === "DEBIT") {
                        return `<input type="text" class="debit" value="${data.toFixed(2)}">`;
                    }
                    return `<input type="text" class="debit" value="0.00" readonly>`;
                }
            },
            {
                data: 'Amount',
                render: function (data, type, row) {
                    if (row.BookingDirection.toUpperCase() === "CREDIT") {
                        return `<input type="text" class="credit" value="${data.toFixed(2)}">`;
                    }
                    return `<input type="text" class="credit" value="0.00" readonly>`;
                }
            },
            {
                data: 'MFI_ChartOfAccountId',
                render: function (data, type, row) {

                    return `<input type="hidden" class="MFI_ChartOfAccountId" value="${data}" readonly>`;
                }
            },
            {
                data: 'System_Id',
                render: function (data, type, row) {

                    return `<input type="hidden" class="System_Id" value="${data}" readonly>`;
                }
            }
        ],
        columnDefs: [
            { width: '70%', targets: 0 },  // AccountNumber
            { width: '15%', targets: 1 },  // Debit
            { width: '15%', targets: 2 },  // Credit

        ],
        autoWidth: false,
        footerCallback: function (row, data, start, end, display) {
            let totalDebit = 0;
            let totalCredit = 1;


            data.forEach(row => {
                if (row.BookingDirection.toUpperCase() === "DEBIT") {
                    totalDebit += parseFloat(row.Amount);

                }
                if (row.BookingDirection.toUpperCase() === "CREDIT") {
                    totalCredit += parseFloat(row.Amount);

                }
            });
            $('#totalDebit').text(totalDebit.toFixed(2));
            $('#totalCredit').text(totalCredit.toFixed(2));
            $('#totalBalance').text("Total:" + (totalCredit - totalDebit).toFixed(2));
            // Show or hide the description row based on the totals

            if (totalDebit === totalCredit) {
                if ($('#descriptionRow').length === 0) {
                    $('#AccountingEventEntriesDataTable').append(
                        `<tr id="descriptionRow">
                            <td colspan="3">
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
    });

    // Add a row for the totals in the footer
    $('#AccountingEventEntriesDataTable tfoot').remove();
    $('#AccountingEventEntriesDataTable').append(
        `<tfoot>
            <tr>
                <th id="totalBalance">Total:</th>
                <th id="totalDebit">0.00</th>
                <th id="totalCredit">0.00</th>
                <th></th>
            </tr>
        </tfoot>`
    );

    // Update totals initially
    updateTotals();

    // Attach event listeners to input fields
    $('#AccountingEventEntriesDataTable').on('input', '.credit', function () {
        updateTotals();
    });
    $('#AccountingEventEntriesDataTable').on('input', '.debit', function () {
        updateTotals();
    });
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


function LoadAccountingEventEntrySystemId(system_Id, yourModalId) {
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


function LoanAccountingEventEntrySystemId(system_Id) {
    GetSequenceReference();
    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountingEntryEventID/',
        type: 'Get',
        dataType: 'json',
        data: { system_Id: system_Id },
        success: function (data) {
            console.log($("#" + system_Id + "-RuleName").text());
            $('#exampleModalLabel3').empty();
            //var description = $("#" + branchId + "-RuleName").text();   
            if (data.HasError == true) {
                var names = "Your branch is missing some accounts needed to record " + $("#" + system_Id + "-RuleName").text() + " transactions.";
                $("#exampleModalLabel3").text(names);
                document.querySelector('#exampleModalLabel3').classList.add('text-danger');
                //$('#elementId').append('&nbsp;');
            } else {
                var names = "\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0\u00A0" + $("#" + system_Id + "-RuleName").text() + "accounting entries.";
                console.log(names);
                $("#exampleModalLabel3").text('                                                         ' + names);
                var table;
                InitiliseDataTable(data.AccountingRule);
            }






        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
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
        const System_Id = this.data().System_Id;
        console.log(MFI_ChartOfAccountId);
        console.log(this.data().System_Id);

        const entry = {

            Amount: parseFloat(rowData.BookingDirection.toUpperCase() === "DEBIT" ? debitInput : creditInput),
            BookingDirection: rowData.BookingDirection,

            MFI_ChartOfAccountId: rowData.MFI_ChartOfAccountId,
            System_Id: rowData.System_Id
        };

        dataToSend.push(entry);
    });

    // Collect the operation description if it exists
    const operationDescription = $('#operationDescription').val() || '';
    const System = $('#System_Id').val() || '';
    // Prepare the final data object
    const finalData = {
        Entries: dataToSend,
        Description: operationDescription,
        ReferenceId: $('#ReferenceId').val(),
        System_Id: System
    };
    //const finalData = createManuallyJournalEntry();
    //finalData.entryTempDatas = dataToSend;
    //finalData.Description = operationDescription;
    //finalData.ReferenceId = $('#ReferenceId').val();
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
function submitAccountingEntries() {
    const table = $('#AccountingEventEntriesDataTable').DataTable();
    const dataToSend = [];

    // Collect data from each row
    table.rows().every(function (rowIdx, tableLoop, rowLoop) {
        const rowData = this.data();
        const debitInput = this.nodes().to$().find('input.debit').val();
        const creditInput = this.nodes().to$().find('input.credit').val();
        const MFI_ChartOfAccountId = this.data().MFI_ChartOfAccountId;
        const System_Id = this.data().System_Id;
        const entry = {
            AccountNumber: rowData.AccountNumber,
            Amount: parseFloat(rowData.BookingDirection.toUpperCase() === "DEBIT" ? debitInput : creditInput),
            BookingDirection: rowData.BookingDirection,
            Description: rowData.Description,
            MFI_ChartOfAccountId: rowData.MFI_ChartOfAccountId,
            System_Id: rowData.System_Id
        };

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

function createManuallyJournalEntry() {
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
        EntryTempDatas: [],
        EntryTempData: {
            // Add properties as needed
        },
        serviceOption: "CreateAccountingEntries",
        action: "insert",
        key: "",
        hasApproved: false
    };
}
function LoadAccountingRuleDataSetDT(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [   ],
        "columnDefs": [
 
            { "targets": 0, "searchable": true, "orderable": true, "width": "75%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "15%" }

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
