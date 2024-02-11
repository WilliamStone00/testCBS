
    //$(document).ready(function () {
    //    // Function to handle "Select All" checkbox
    //    $('#selectAll').change(function () {
    //        // Check or uncheck all checkboxes based on the state of "Select All" checkbox
    //        $('#myDataTable input[name="selectedMenus"]').prop('checked', this.checked);
    //    });

    //    // Function to handle form submission
    //    $('#submitBtn').click(function () {
    //        // Array to store selected MenuMasterId values
    //        var selectedMenuIds = [];

    //        // Loop through selected checkboxes and push MenuMasterId to the array
    //        $('#myDataTable input[name="selectedMenus"]:checked').each(function () {
    //            selectedMenuIds.push($(this).val());
    //        });

    //        // Check if at least one checkbox is selected
    //        if (selectedMenuIds.length > 0) {
    //            // Your AJAX code to submit the data to the controller
    //            var roleID = $('#roleID').val();
    //            $.ajax({
    //                type: 'POST',
    //                url: '/RolePermission/SubmitSelectedMenus',
    //                data: { menuIds: selectedMenuIds, roleID: roleID },
    //                success: function (response) {
    //                    // Handle success response
    //                    console.log(response);
    //                    if (response.success) {
    //                        appalert(response.message, 1, 1);
    //                        window.location.reload();
    //                    }
    //                    else {
    //                        appalert(response.message, 3, 1);
    //                    }
    //                },
    //                error: function (error) {
    //                    // Handle error response
    //                    console.error(error);
    //                }
    //            });
    //        } else {
    //            // Handle the case when no checkboxes are selected
    //            alert('Please select at least one menu.');
    //        }
    //    });
    //});



                        
$(document).ready(function () {
   /* $("#myDataTable").DataTable();*/
    LoadDTSelect("myDataTable", 0)
    // Function to handle "Select All" checkbox
    $('#selectAll').change(function () {
        // Check or uncheck all checkboxes based on the state of "Select All" checkbox
        $('#myDataTable input[name="selectedMenus"]').prop('checked', this.checked);
    });

    // Function to handle form submission
    $('#submitBtn').click(function () {
        // Array to store selected MenuMasterId values
        var selectedMenuIds = [];

        // Loop through selected checkboxes and push MenuMasterId to the array
        $('#myDataTable input[name="selectedMenus"]:checked').each(function () {
            selectedMenuIds.push(parseInt($(this).val())); // Convert to integer
        });

        // Check if at least one checkbox is selected
        if (selectedMenuIds.length > 0) {
            // Get the roleID value
            var roleID = $("#roleID").val(); // Provide the actual role ID here

            // Check if roleID is not null
            if (roleID !== null) {
                // Create the PermissionMenuLoaderDto object
                var permissionDto = {
                    Action: "insert",
                    roleID: roleID,
                    ServiceOption: "role_permission",
                    MenuMasterId: selectedMenuIds
                };

                // Your AJAX code to submit the data to the controller
                $.ajax({
                    type: 'POST',
                    url: '/RolePermission/AddOrUpdate',
                    contentType: 'application/json', // Set content type to JSON
                    data: JSON.stringify(permissionDto), // Convert object to JSON string
                    success: function (response) {
                        // Handle success response
                        console.log(response);
                        if (response.success) {
                            appalert(response.message, 1, 1);
                            window.location.reload();
                        }
                        else {
                            appalert(response.message, 3, 1);
                        }
                    },
                    error: function (error) {
                        // Handle error response
                        console.error(error);
                    }
                });
            } else {
                // Handle the case when roleID is null
                alert('Role ID is null. Please provide a valid role ID.');
            }
        } else {
            // Handle the case when no checkboxes are selected
            alert('Please select at least one menu.');
        }
    });
});
