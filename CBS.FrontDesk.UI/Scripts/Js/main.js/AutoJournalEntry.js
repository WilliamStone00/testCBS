let basket = [];
let tableJE;

$(document).ready(function () {
    LoadAccountingRuleDataSetDT("AccountingEventxxDataTable")

    tableJE = $('#AccountingRulebasketTable').DataTable({
        columns: [
            { data: 'ruleName' },
            { data: 'MFI_ChartOfAccountId' },
            { data: 'bookingDirection' },
            { data: 'description' },
            {
                data: null,
                defaultContent: '<button class="btn btn-danger removeBtn">Remove</button>',
                orderable: false
            }
        ],
        columnDefs: [
            { width: '15%', targets: 0 },  // ruleName
            { width: '30%', targets: 1 },  // chartOfAccount
            { width: '5%', targets: 2 },   // bookingDirection
            { width: '40%', targets: 3 },  // description
            { width: '5%', targets: 4 }    // remove button
        ],
        autoWidth: false  // This is important to enforce our custom widths
    });

    $('#AccountingRulebasketTable tbody').on('click', 'button.removeBtn', function () {
        const data = tableJE.row($(this).parents('tr')).data();
        removeItem(data);
    });
});

let manuallyJournalEntryDataSet = createManuallyJournalEntryDataSet();
function addToBasket() {
    const item = {
        ruleName: $('#AccountingRule_RuleName').val(),
        MFI_ChartOfAccountId: $('#AccountingRule_MFI_ChartOfAccountId option:selected').text(),
        bookingDirection: $('#AccountingRule_BookingDirection option:selected').text(),
        description: $('#AccountingRule_Description').val()
    };
    console.log(item);
    if (item.bookingDirection === "---Select Direction---") {
        appalert('No booking direction has been set for the entry rule has been set,Please kindly contact administrators for help', 2, 1);
        return;
    } else if (item.MFI_ChartOfAccountId === "---Select ChartOfAccount---") {
        appalert('No ChartOfAcccount number has been selected for the entry rule has been set,Please kindly contact administrators for help', 2, 1);
        return;
    }
    else if (item.ruleName === "") {
        appalert('No Entry RuleName  has been set for the entry rule has been set,Please kindly contact administrators for help', 2, 1);
        return;
    } else if (item.description === "") {
        appalert('A description must be set for the user is mandatetory,Please kindly contact administrators for help', 2, 1);
        return;
    }
    basket.push(item);
    updateBasketDisplay();
    if (basket.length>0) {
        $('#AccountingRule_RuleName').prop('disabled', true);
    } else {
        $('#AccountingRule_RuleName').prop('disabled', false);
    }
    // Clear the form fields after adding to basket
    $('#basket_Label').text('Event Entry Rule for: ' + item.ruleName);
    $('#AccountingRule_MFI_ChartOfAccountId').val('').change();
    $('#AccountingRule_BookingDirection').val('').change();
    $('#AccountingRule_Description').val('');
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
        i.ruleName === item.ruleName &&
        i.chartOfAccount === item.chartOfAccount &&
        i.bookingDirection === item.bookingDirection &&
        i.description === item.description
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
            let totalCredit = 0;
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
            $('#totalBalance').text("Total:"+(totalCredit - totalDebit).toFixed(2));
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
    $('#totalBalance').text("Total:"+(totalCredit - totalDebit).toFixed(2));
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

 
function LoanAccountingEventEntrySystemId(system_Id) {
    console.log(system_Id);
    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountingEntryEventID/',
        type: 'Get',
        dataType: 'json',
        data: { system_Id: system_Id },
        success: function (data) {
            /* $('#largeModal').hide();*/
            $('#exampleModalLabel3').empty();
            //var description = $("#" + branchId + "-Name").text();
            var names = "Accounting Event Entry for " + data[0].RuleName + ".";
            console.log(data);
            $("#exampleModalLabel3").text(names);
            //// Append text to the modal title
            ////$('#LiaisonLedgerHeading').append(names);
            var table;
            InitiliseDataTable(data);


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


// Attach the function to a button click event
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
       

        const entry = {
            AccountNumber: rowData.AccountNumber,
            Amount: parseFloat(rowData.BookingDirection.toUpperCase() === "DEBIT" ? debitInput : creditInput),
            BookingDirection: rowData.BookingDirection,
            Description: rowData.Description,
            MFI_ChartOfAccountId: rowData.MFI_ChartOfAccountId
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

                            
                            appalert(response.MessageString,  1, 1);
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

function LoadAccountingRuleDataSetDT(tableID) {


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
