

//$(document).ready(function () {

//    LoadCategories();
//});


function LoadCategories() {
    LoadDataGen('Categoryconfic', 'myDataTable', '_CategoryDataTable', 0, 'datalistingview', null, 'list')

}






//$(document).ready(function () {
//    // This is the only document ready block you need.
//    // It is important to structure it this way to avoid multiple bindings.

//    // 1. Initialize the DataTable
//    var dataTable = loadBulkOperationDataTable();

//    // 2. Initialize filter toggles
//    initFilterToggles();

//    // 3. Bind filter buttons to the specific DataTable instance
//    bindFilterActions(dataTable);

//    // 4. Set up the modern event handlers for Edit and Delete
//    // This should be called only ONCE.
//    setupActionHandlers();
//});


//function loadBulkOperationDataTable() {
//    if ($.fn.DataTable.isDataTable('#myDataTable')) {
//        $('#myDataTable').DataTable().destroy();
//    }

//    // Return the table instance so other functions can use it
//    return $('#myDataTable').DataTable({
//        processing: false,
//        serverSide: true,
//        responsive: true,
//        searching: false,
//        order: [[0, 'asc']],
//        ajax: {
//            url: '/Categoryconfic/LoadCategories',
//            type: 'POST',
//            contentType: 'application/json',
//            data: function (d) {
//                var payload = {
//                    Name: $('#userName').val() || '',
//                    BranchId: $('#branchInput').val() || '',
//                    DataTableOptions: {
//                        draw: d.draw,
//                        start: d.start,
//                        length: d.length,
//                        sortColumnName: d.columns[d.order[0].column].data,
//                        sortColumnDirection: d.order[0].dir
//                    }
//                };
//                return JSON.stringify(payload);
//            }
//        },
//        columns: [
//            { data: 'name' },
//            { data: 'basePrice' },
//            { data: 'numberOfPages' },
//            {
//                data: 'isActive',
//                render: function (data) {
//                    return data ? '<span class="badge bg-success">Active</span>' : '<span class="badge bg-danger">Inactive</span>';
//                }
//            },
//            { data: 'validityPeriodInMonths' },
//            { data: 'issuanceLimitPerCustomerType' },
//            {
//                data: 'id',
//                orderable: false,
//                render: function (data, type, row) {
//                    // This render function is correct. It adds the hooks for our event handlers.
//                    return `
//                        <a href="#" class="mr-2 js-edit" data-id="${data}" title="Edit ${row.name}">Edit</a>
//                        <a href="#" class="mr-2 text-danger js-delete" data-id="${data}" title="Delete ${row.name}">Delete</a>
//                    `;
//                }
//            }
//        ]
//    });
//}


///**
// * Sets up the click handlers for the Edit and Delete buttons.
// * This function should only be called once.
// */
//function setupActionHandlers() {
//    // Use event delegation on the table body. This is more efficient.
//    $('#myDataTable tbody').off('click').on('click', '.js-edit', function (e) {
//        e.preventDefault();
//        var categoryId = $(this).data('id');

//        // --- RESTORED: Calling your custom helper function for editing ---
//        AddORUpdateGen(categoryId, 'datalistingview', '_Categories', 'get', 'Categoryconfic', '');
//    });

//    $('#myDataTable tbody').on('click', '.js-delete', function (e) {
//        e.preventDefault();
//        var categoryId = $(this).data('id');

//        // --- RESTORED: Calling your custom helper function for deleting ---
//        // This function contains YOUR specific confirmation popup logic.
//        DeleteRecordDataTable('Categoryconfic', categoryId, 'myDataTable', '_CategoryDataTable', 0, 'datalistingview');
//    });
//}


//// --- Filter and Reset Functions (No changes needed here) ---

//function initFilterToggles() {
//    $('#byBranch').change(function () {
//        $('#branchFilterSection').slideToggle(this.checked);
//        if (!this.checked) $('#branchInput').val('').trigger('change');
//    });

//    $('#byUser').change(function () {
//        $('#userFilterSection').slideToggle(this.checked);
//        if (!this.checked) $('#userName').val('');
//    });
//}

//function bindFilterActions(table) {
//    $('#applyFilterBtn').off('click').on('click', function () {
//        table.ajax.reload();
//    });

//    $('#resetFilterBtn').off('click').on('click', function () {
//        resetFilterForm();
//        table.ajax.reload();
//    });
//}

//function resetFilterForm() {
//    $('#userName').val('');
//    $('#branchInput').val('').trigger('change');
//    $('#byBranch, #byUser').prop('checked', false);
//    $('#branchFilterSection, #userFilterSection').hide();
//}