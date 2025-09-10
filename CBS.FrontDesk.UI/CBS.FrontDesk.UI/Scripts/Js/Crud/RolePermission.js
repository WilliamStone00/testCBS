$(document).ready(function () {
    LoadDTSelect("myDataTable", 0);

    // Handle Select All
    $('#selectAll').change(function () {
        $('#myDataTable input[name="selectedMenus"]').prop('checked', this.checked);
    });

    // Handle Submit Click
    $('#submitBtn').click(function (e) {
        e.preventDefault();

        // Collect selected menu IDs
        let selectedMenuIds = [];
        $('#myDataTable input[name="selectedMenus"]:checked').each(function () {
            selectedMenuIds.push(parseInt($(this).val()));
        });

        const selectedRoles = $('#roleIds').val(); // this returns a list of strings
        const selectedRoleNames = $('#roleIds option:selected').map(function () { return $(this).text(); }).get();

        if (!selectedRoles || selectedRoles.length === 0) {
            appalert("Validation", "Please select at least one role before saving.", 2, 1);
            return;
        }

        if (selectedMenuIds.length === 0) {
            appalert("Validation", "You must select at least one menu to assign.", 2, 1);
            return;
        }

        alertify.confirm(
            "Confirm Permission Assignment",
            `You are about to assign <strong>${selectedMenuIds.length}</strong> menu item(s) to the following role(s):<br><strong>${selectedRoleNames.join(", ")}</strong>.<br><br>Do you want to continue?`,
            function () {
                const permissionDto = {
                    Action: "insert",
                    roleIDs: selectedRoles, // keep as string[]
                    ServiceOption: "role_permission",
                    MenuMasterId: selectedMenuIds
                };

                $.ajax({
                    type: 'POST',
                    url: '/RolePermission/AddOrUpdate',
                    contentType: 'application/json',
                    data: JSON.stringify(permissionDto),
                    success: function (response) {
                        if (response.success) {
                            appalert(response.message, 1, 1);
                            setTimeout(() => window.location.reload(), 1000);
                        } else {
                            alertify.error(response.message);
                        }
                    },
                    error: function (error) {
                        console.error(error);
                        appalert("An unexpected error occurred.", 3, 1);
                    }
                });
            },
            function () {
                appalert("Operation canceled by user.", 2, 1);
            }
        );

    });
});
