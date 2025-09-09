// Location: ~/Scripts/Js/Crud/ChequeManagement/CategoryConfig/FeeConfig.js
$(document).ready(function () {
    // This function runs once the page DOM is ready.
    // It sets up all the initial event listeners on the static parts of the page.

    // 1. Set up the toggle for the "Is Centralized" checkbox.
    setupFeeConfigToggles();

    // 2. Set up the event handler for the "Load" button.
    $('#loadConfigBtn').on('click', loadConfiguration);

    // 3. Set up event handlers for the dynamic form that will be loaded later.
    // We use event delegation on a static parent container ('#config-form-container').
    setupDynamicFormHandlers();
});


/**
 * Sets up the interactive toggles for the main selection area.
 */
function setupFeeConfigToggles() {
    $('#isCentralized').on('change', function () {
        if (this.checked) {
            // Centralized is ON: Hide the branches and clear its value.
            $('#branch-selection-container').slideUp();
            $('#branchId').val('').trigger('change'); // Clear selection for select2
        } else {
            // Centralized is OFF: Show the branches.
            $('#branch-selection-container').slideDown();
        }
    });
}


/**
 * Main function to trigger the loading of the configuration form partial view.
 */
function loadConfiguration() {
    var isCentralized = $('#isCentralized').is(':checked');
    var branchId = isCentralized ? null : $('#branchId').val();
    var feeType = $('#feeType').val();

    // Validation
    if (!isCentralized && !branchId) {
        appalert('Please select a branch for non-centralized configuration.', 2, 1);
        return;
    }
    if (!feeType) {
        appalert('Please select a fee type.', 2, 1);
        return;
    }

    // Use jQuery's .load() function to call the InitializeData action and inject
    // the returned HTML directly into our container div. This is the core of your pattern.
    $('#config-form-container').load(
        '/FeeConfig/InitializeData',
        {
            partialView: '_Create',
            isCentralized: isCentralized,
            branchId: branchId,
            feeType: feeType
        }
    );
}


/**
 * Sets up event handlers for elements that will exist inside the dynamically loaded form.
 * Using event delegation is essential here.
 */
function setupDynamicFormHandlers() {
    var container = $('#config-form-container');

    // Add a new row to the range table.
    container.on('click', '#addRangeRowBtn', function () {
        // We get the new row HTML from a client-side template function.
        var newRowHtml = getRangeRowTemplate();
        $('#range-table-body').append(newRowHtml);
        // After adding, re-index all rows to ensure names are correct for submission.
        reindexRangeRows();
    });

    // Remove a row from the range table.
    container.on('click', '.remove-range-row', function () {
        $(this).closest('tr').remove();
        // After removing, re-index is critical.
        reindexRangeRows();
    });

    // Handle the form submission when the Save button is clicked.
    // Note: We listen for the 'submit' event on the form itself.
    container.on('submit', '#feeConfigForm', function (e) {
        e.preventDefault(); // Prevent the default browser form post.
        saveConfiguration(this); // Pass the form DOM element to the save function.
    });

    // Handle the delete button click.
    container.on('click', '#deleteConfigBtn', function () {
        deleteConfiguration();
    });
}


/**
 * Returns an HTML string for a new, empty range table row.
 * This is a client-side template.
 */
function getRangeRowTemplate() {
    // The '{INDEX}' placeholder will be replaced by the reindex function.
    return `
        <tr>
            <td>
                <input type="hidden" name="FeeTypeRanges.Index" value="{INDEX}" />
                <input type="hidden" name="FeeTypeRanges[{INDEX}].Id" value="" />
                <input type="number" step="0.01" name="FeeTypeRanges[{INDEX}].FromAmount" value="" class="form-control" />
            </td>
            <td>
                <input type="number" step="0.01" name="FeeTypeRanges[{INDEX}].ToAmount" value="" class="form-control" />
            </td>
            <td>
                <input type="number" step="0.01" name="FeeTypeRanges[{INDEX}].Fee" value="" class="form-control" />
            </td>
            <td class="text-center">
                <button type="button" class="btn btn-sm btn-outline-danger remove-range-row"><i class="mdi mdi-delete"></i></button>
            </td>
        </tr>
    `;
}


/**
 * Re-indexes the name attributes of the range table rows to ensure they form a continuous
 * sequence (0, 1, 2, ...). This is essential for MVC model binding to lists.
 */
function reindexRangeRows() {
    $('#range-table-body tr').each(function (newIndex) {
        // Find all input elements within this row.
        $(this).find('input').each(function () {
            var oldName = $(this).attr('name');
            if (oldName) {
                // Use a regular expression to replace the number inside the brackets.
                var newName = oldName.replace(/\[\d*\]|{INDEX}/, '[' + newIndex + ']');
                $(this).attr('name', newName);

                // Also update the hidden .Index input value.
                if (oldName.includes('.Index')) {
                    $(this).val(newIndex);
                }
            }
        });
    });
}

//function saveConfiguration(form) {
//    // We use jQuery's serialize() which correctly reads all named inputs.
//    var formData = $(form).serialize();

//    // The anti-forgery token must also be included.
//    var token = $('input[name="__RequestVerificationToken"]').val();

//    $.ajax({
//        url: '/FeeConfig/CreateOrUpdate',
//        type: 'POST',
//        data: formData + "&__RequestVerificationToken=" + token,
//        success: function (response) {
//            if (response.success) {
//                appalert(response.message, 1, 1);
//                // On success, we reload the configuration to get the updated state, including the new ID.
//                loadConfiguration();
//            } else {
//                appalert(response.message, 2, 1);
//            }
//        },
//        error: function (err) {
//            appalert('An error occurred while saving: ' + err.statusText, 0, 1);
//        }
//    });
//}


///**
// * Submits a request to delete the current configuration.
// */
//function deleteConfiguration() {
//    var configId = $('#Id').val(); // The ID is in a hidden field in the form.
//    if (!configId) {
//        appalert('Cannot delete a configuration that has not been saved yet.', 3, 1);
//        return;
//    }

//    if (confirm('Are you sure you want to delete this fee configuration?')) {
//        var token = $('input[name="__RequestVerificationToken"]').val();
//        $.ajax({
//            url: '/FeeConfig/Delete',
//            type: 'POST',
//            data: {
//                KEY: configId,
//                __RequestVerificationToken: token
//            },
//            success: function (response) {
//                if (response.success) {
//                    appalert(response.message, 1, 1);
//                    // On successful deletion, clear the form from the page.
//                    $('#config-form-container').empty();
//                } else {
//                    appalert(response.message, 2, 1);
//                }
//            },
//            error: function (err) {
//                appalert('An error occurred during deletion: ' + err.statusText, 0, 1);
//            }
//        });
//    }
}