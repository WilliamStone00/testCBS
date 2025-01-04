$(document).ready(function () {
    
    LoadDownloadedReportByUser("myDataTable");
    LoadBranchAndAccountDataSetDT("BranchDataTable");
    LoadAccountDataSetDT("AccountDataTable");
    $('#AccountToHide').hide();
    $("#btnData").click(function () {
        LoadData();
    });
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

        // Check if the selected value matches the specific value
        if (selectedValue === 'GL') {
            // Show the element
            $('#AccountToHide').show();
            var selectedId = $("#selectedBranchID").val();
            loadBranchAccounts(selectedId);
        } else {
            // Hide the element
            $('#AccountToHide').hide();
        }

    });
    $(document).on('click', '#print_btn', function () {
       
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
      

        alertify.confirm("TRUST SOFT CREDIT", "Are you sure you want to  generate " + getReportTitle(model.ReportType) + " for the period of " + $("input[name='SystemQuery.FromDate']").val() + " to " + $("input[name='SystemQuery.ToDate']").val() + " !!! ",
            function () {


                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        //  openReportWindow(model.FileType, model.ReportType);
                        console.log(response);
                        window.location.reload();
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
    } else {
        if (reportType === "BS") {
            url = "/Reports/DownloadBSFile";
        } else {
            url = "/Reports/DownloadExcelFile";
        }
        
    }
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