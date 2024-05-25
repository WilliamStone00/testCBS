$(document).ready(function () {
    
    LoadBranchAndAccountDataSetDT("BranchDataTable");
    LoadAccountDataSetDT("AccountDataTable");
    $('#AccountToHide').hide();
    $("#btnData").click(function () {
        LoadData();
    });
    $(document).on('change', '#SystemQuery_BranchId', function () {
        // Get the selected value AccountNumber
        var selectedValue = $(this).val();

        var selectedReportType = $("#SystemQuery_ReportType").val();

        if (selectedReportType == "LL") {
            // Load another dropdown based on the selected value
            loadBranchLiasonAccount(selectedValue);
        }

    });

    $(document).on('change', '#SystemQuery_ReportType', function () {
        var selectedValue = $(this).val();

        // Check if the selected value matches the specific value
        if (selectedValue === 'GL') {
            // Show the element
            $('#AccountToHide').show();
        } else {
            // Hide the element
            $('#AccountToHide').hide();
        }

    });


});

function LoadBranchAndAccountDataSetDT(tableID) {


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
            { "targets": 0, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "50%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "25%" }
           

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
function AjaxPostSearch(form) {
    var fileType = $("#SystemQuery_FileType").val();
    var reportType = $("#SystemQuery_ReportType").val();
    alert(fileType + reportType);
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                appalert(response.message, 2, 1);
                if (fileType === "EXCEL") {
                    if (reportType === "TB4") {
                        window.open("/Reports/PrintTrialBalance4Column", "_blank");
                    } else if (reportType === "TB6") {

                        window.open("/Reports/PrintTrialBalance6Column", "_blank");
                    }
                } else {
                    window.open("/Reports/DownloadExcelFile", "_blank");
                }

                // Updated URL



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
            appalert("GeneralLedger for " + branchName +" was created successfully" , 2, 1);
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

function DownLoadJE(fileType) {
    var BranchId = $('#selectedBranchId').val();
    var dateFrom = $('#selectedDateFromForJE').val();
    var dateTo = $('#selectedDateToForJE').val();
        $.ajax({
        url: '/AccountingStatements/GenerateJEByBranchId',
        type: 'Get',
        dataType: 'json',
        data: { branchId: BranchId, fileType: fileType, FromDate: dateFrom, ToDate:dateTo },
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
            /** Populate the table with the fetched data
            //var tableBody = $('#BranchAccountDataTable tbody');
            //tableBody.empty(); // Clear existing rows
            //if (data && data.length) {
            //    $.each(data, function (index, item) {
            //        var row = $('<tr>');

            //        row.append($('<td>').text(item.AccountNumber));
            //        row.append($('<td>').text(item.AccountName));
            //        row.append($('<td>').text(item.CurrentBalance));

            //        // Escape single quotes in the AccountName
            //        var escapedAccountName = item.AccountName.replace(/'/g, "\\'");
            //        // Add the new cell with the anchor tag
            //        var anchorTag = '<td style="width:10%"><a href="#" class="btn btn-outline-primary" onclick="loadAccountingEntriesByAccountId(\'' + item.Id + '\')" class="mr-2 btn btn-default" data-toggle="tooltip" data-placement="top" data-bs-toggle="modal" data-bs-target="#largeModal" title="Select ' + escapedAccountName + ' to journal entry">Download Journal Entry</a></td>';
            //        row.append(anchorTag);

            //        tableBody.append(row);
            //    });

            //} else {

            //// Handle the case when data is null or empty
            //var row = $('<tr>');
            //    row.append($('<td colspan="4" style="text-align: center;">No data available</td>'));
            //tableBody.append(row);

            }**/


            // Append text to the modal title
            initializeDataTableForGL(data);
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
    console.log(branchId);
    const $fromDate = $('#FromDate');
    const $toDate = $('#ToDate');
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

function initializeDataTableForJE(data) {
    console.log(data);
    if ($.fn.DataTable.isDataTable('#JournalEntriesTable')) {
        // If the DataTable instance already exists, destroy it
        table.destroy();
    }

    if (data && data.length > 0)
    {
       
        // Create a new DataTable instance with the provided data
        table = $('#JournalEntriesTable').DataTable({
            data: data,
            columns: [
                { data: 'EntryDate' },
                { data: 'TransactionReference' },
                { data: 'AccountNumber'},
                { data: 'Description' },
                { data: 'DebitAmount' },
                { data: 'CreditAmount' },
             
            ]
        });
    } else {
        // Create an empty DataTable instance
        table = $('#JournalEntriesDataTable').DataTable();
        table.clear().draw();
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