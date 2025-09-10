// ===================================================================
//
//  Manual Daily Collection - Custom Script
//  This script provides the special handling for the file upload form.
//
// ===================================================================

// Use a self-executing anonymous function to create a private scope.
(function ($) {

    /**
     * This is the custom function that replaces the generic AjaxPostAndUpdate.
     * It will be called by the onsubmit attribute of the upload form.
     * @param {HTMLFormElement} form - The form element being submitted.
     */
    window.handleManualCollectionUpload = function (form) {
        // Prevent the browser's default submission which causes a full page reload.
        event.preventDefault();

        const $form = $(form);
        const actionUrl = $form.attr('action');
        const formData = new FormData(form);
        const $submitButton = $form.find('button[type="submit"]');
        const $resultContainer = $('#uploadResultContainer'); // The div to show the preview

        $.ajax({
            url: actionUrl,
            type: 'POST',
            data: formData,
            contentType: false, // Mandatory for sending files
            processData: false, // Mandatory for sending files
            beforeSend: function () {
                // Give the user visual feedback
                $submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> Uploading...');
                $resultContainer.slideUp().html(''); // Hide old results
            },
            success: function (response) {
                if (response.success) {
                    // --- SUCCESS PATH ---
                    alert(response.message || "File validated successfully!");

                    // Get the unique ID from the JSON response
                    const fileId = response.data.FileUploadId;

                    // Now, make the second AJAX call to get the PREVIEW HTML
                    const detailsUrl = `/ManualDailyCollection/InitializeData?KEY=${fileId}&partialView=_FileDetails&path=details`;

                    // Use jQuery's .load() to fetch the HTML and inject it
                    $resultContainer.load(detailsUrl, function () {
                        // After the HTML is loaded, show it
                        $resultContainer.slideDown();
                    });

                    form.reset(); // Clear the form for the next upload

                } else {
                    // --- FAILURE PATH ---
                    alert("Upload Failed: " + response.message);
                }
            },
            error: function () {
                // --- ERROR PATH ---
                alert("An unexpected server error occurred. Please try again.");
            },
            complete: function () {
                // Re-enable the button, whether the upload succeeded or failed
                $submitButton.prop('disabled', false).html('<i class="mdi mdi-cloud-upload-outline me-1"></i> Upload File');
            }
        });

        // Return false to be absolutely sure the default form submission is stopped.
        return false;
    };

}(jQuery));