$(document).ready(function () {

    $(document).on('change', '#CorrespondingBankBranch_RegionId', function () {
        var selectedValue = $(this).val();
        console.log(selectedValue);
        loadValues("DIVISION", selectedValue, "CorrespondingBankBranch_DivisionId");

    });
    $(document).on('change', '#CorrespondingBankBranch_DivisionId', function () {
        var selectedValue = $(this).val();
        console.log(selectedValue);
        loadValues("SUBDIVISION", selectedValue, "CorrespondingBankBranch_SubdivisionId");
      
    });
    $(document).on('change', '#CorrespondingBankBranch_SubdivisionId', function () {
        var selectedValue = $(this).val(); 
        console.log(selectedValue);
        loadValues("TOWN", selectedValue, "CorrespondingBankBranch_TownId");

    });
  
});
 
function loadValues(zone,Key,loadKey) {
    console.log(zone + " " + Key);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/CorrespondingBankManagement/GetValueOption',
        type: 'GET',
        dataType: 'json',
        data: { switch_on: zone, Id:Key },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#' + loadKey).empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#' + loadKey).append($('<option>').text(item.Value).attr('value', item.Text));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
/**
 * Wrapper function to load data and initialize a DataTable.
 * 
 * @param {string} controller - The controller name for the AJAX request.
 * @param {string} tableID - The ID of the table to be initialized as a DataTable.
 * @param {string} partialView - The name of the partial view to be loaded.
 * @param {string} datalistingview - The ID of the div where the loaded data will be inserted.
 * @param {string} KEY - A key parameter for the request.
 * @param {string} serviceOption - An option parameter for the service.
 * @param {string} [path='list'] - A path parameter for the request, defaults to 'list'.
 */
function LoadDataView(controller, tableID, partialView, datalistingview, KEY, serviceOption, path = 'list') {
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
            LoadData(tableID);
        },
        error: function (err) {
            // Display an error alert if the request fails
            appalert(err.statusText, 1, 3);
        }
    });
}

/**
 * Initializes a DataTable with specific configuration.
 * @param {string} tableID - The ID of the table element to be transformed into a DataTable.
 */
function LoadData(tableID) {
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