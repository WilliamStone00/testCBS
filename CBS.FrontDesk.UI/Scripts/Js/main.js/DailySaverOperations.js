let isHarmonizationActivated = false;
$(document).ready(function () {
    $("#HideDataResult").hide();
    $(document).on('change', '#AgentBranchId', function () {
        var selectedValue = $(this).val();
        loadBranchAccounts(selectedValue);
       

    });
    $('#downloadExcelOPButton').click(function () {
        // Define the file name and path relative to your domain
        const fileName = 'CorrectedCollectorEntryFile.xlsx';
        const filePath = `/AppFiles/SampleDocument/${fileName}`;

        // Create a temporary anchor element
        const $link = $('<a>')
            .attr('href', filePath)  // Set the file URL
            .attr('download', fileName); // Set the download attribute

        // Append the link to the body, trigger the download, and then remove it
        $('body').append($link);
        $link[0].click();
        $link.remove();
    });

    $('#AgentBranchId').select2({
        placeholder: "Select branches",
        allowClear: true,
        width: '100%'
    });
});

function DownloadFile(path) {
    window.open(path, "_blank");
}
function loadBranchAccounts(branchId) {
    console.log(branchId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/DailyAgentManagement/GetAgentAllAgentByBranch',
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {

            console.log(data);
    
            // Clear existing options
            $('#CollectorxCollectorId').empty();

            // Optional: Add a default "select" option
            $('#CollectorxCollectorId').append($('<option>').text('-- Choose Collector --').attr('value', ''));


            // Append new options
            $.each(data.data, function (index, item) {
                console.log("Value", item.Value);
                console.log("Text", item.Text);
                const option = $('<option>')
                    .val(item.Value)
                    .text(item.Text);

                if (item.Selected) {
                    option.prop('selected', true);
                }

                $('#CollectorxCollectorId').append(option);
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}



function ReadExcelFile() {
    try {
        // Prevent form submission if event exists
        if (typeof event !== 'undefined') {
            event.preventDefault();
        }

        // Get form elements - be very specific about file input
        var fileInput = document.getElementById("ExcelFile");
        var file = null;

        // Ensure we have a valid file input and file selected
        if (fileInput && fileInput.files && fileInput.files.length > 0) {
            file = fileInput.files[0];
        }

        var branchId = $("#AgentBranchId").val();
        var collectorId = $("#CollectorxCollectorId").val();

        // Debug logging
        console.log("File input element:", fileInput);
        console.log("File object:", file);
        console.log("Files array:", fileInput ? fileInput.files : "No file input");
        console.log("BranchId:", branchId);
        console.log("CollectorId:", collectorId);

        // Enhanced validation checks
        if (!branchId) {
            appalert("PLEASE KINDLY SELECT YOUR BRANCH", 2, 1);
            return false;
        }
        if (!collectorId) {
            appalert("PLEASE KINDLY SELECT A DAILY COLLECTOR", 2, 1);
            return false;
        }
        if (!fileInput) {
            appalert("FILE INPUT ELEMENT NOT FOUND", 2, 1);
            return false;
        }
        if (!file) {
            appalert("PLEASE SELECT AN EXCEL FILE TO UPLOAD", 2, 1);
            console.log("No file selected. File input files:", fileInput.files);
            return false;
        }

        // Validate file extension
        var allowedExtensions = ['.xlsx', '.xls'];
        var fileName = file.name.toLowerCase();
        var fileExtension = fileName.substring(fileName.lastIndexOf('.'));
        if (!allowedExtensions.includes(fileExtension)) {
            appalert("PLEASE SELECT A VALID EXCEL FILE (.xlsx or .xls)", 2, 1);
            return false;
        }

        // Check file size (limit to 10MB)
        var maxSize = 10 * 1024 * 1024; // 10MB
        if (file.size > maxSize) {
            appalert("FILE SIZE MUST BE LESS THAN 10MB", 2, 1);
            return false;
        }

        // Additional file validation
        if (file.size === 0) {
            appalert("SELECTED FILE IS EMPTY", 2, 1);
            return false;
        }

        // Create FormData - THIS IS CRITICAL FOR ASP.NET CORE
        var formData = new FormData();

        // IMPORTANT: The parameter name MUST exactly match the property name in your C# model
        // Your model has: public IFormFile ExcelFile { get; set; }
        // So the FormData key must be exactly "ExcelFile" (case-sensitive)
        formData.append("ExcelFile", file, file.name);
/*        formData.append("BranchId", branchId);*/
        formData.append("CollectorId", collectorId);

        // CRITICAL FOR ASP.NET CORE: Add anti-forgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append("__RequestVerificationToken", token);
            console.log("Anti-forgery token added:", token);
        } else {
            console.warn("No anti-forgery token found - this might cause the request to fail");
        }

        // Debug: Check if file is properly appended
        console.log("FormData contents:");
        for (var pair of formData.entries()) {
            if (pair[1] instanceof File) {
                console.log(pair[0] + ': FILE - Name:', pair[1].name, 'Size:', pair[1].size, 'Type:', pair[1].type);
            } else {
                console.log(pair[0] + ':', pair[1]);
            }
        }

        // Verify FormData has the file
        var hasFile = formData.has("ExcelFile");
        var fileFromFormData = formData.get("ExcelFile");
        console.log("FormData has ExcelFile:", hasFile);
        console.log("File from FormData:", fileFromFormData);

        if (!hasFile || !fileFromFormData || fileFromFormData.size === 0) {
            appalert("ERROR: FILE NOT PROPERLY ATTACHED TO FORM DATA", 2, 1);
            return false;
        }

        // Initialize progress bar
        updateProgressBar(0);
        $('#progressBarContainer').show();

        var messageStatus = "Are you sure you want to upload and process the Collectors Member Excel File?";
        alertify.confirm("COLLECTORS MEMBER UPLOAD", messageStatus,
            function () {
                // Double-check FormData before sending
                console.log("About to send FormData with entries:");
                for (var pair of formData.entries()) {
                    console.log(pair[0], pair[1]);
                }

                $.ajax({
                    url: "/DailyAgentManagement/UploadDailyCollectorEndOfdayOperation/",
                    type: "POST",
                    data: formData,
                    contentType: false,  // CRITICAL: Let browser set the boundary
                    processData: false,  // CRITICAL: Don't process the FormData
                    cache: false,
                    // Add headers that help with ASP.NET Core model binding
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    xhr: function () {
                        var xhr = new window.XMLHttpRequest();
                        xhr.upload.addEventListener("progress", function (evt) {
                            if (evt.lengthComputable) {
                                var percentComplete = evt.loaded / evt.total;
                                var progress = Math.round(percentComplete * 100);
                                updateProgressBar(progress);
                                console.log("Upload progress:", progress + "%");
                            }
                        }, false);
                        return xhr;
                    },
                    beforeSend: function (xhr, settings) {
                        console.log("Sending request with data:", settings.data);
                        console.log("Request headers will be set automatically for multipart/form-data");
                    },
                    success: function (response) {
                        console.log("Upload response:", response);
                        updateProgressBar(100);
                        if (response.success) {
                           
                            $("#HideDataResult").show();
                            var uploadSummary = {
                                TotalAmount: response.data.TotalAmount,
                                TotalMembers: response.data.TotalMembers,
                                BranchName: response.data.BranchName,
                                UploadedBy: response.data.UploadedBy,
                                manualEntryDailyCollectorUploadListDtos: response.data.manualEntryDailyCollectorUploadListDtos
                            };
                          
                            // Display results if needed
                            if (typeof SummaryPresentation === 'function') {
                                appalert("COLLECTORS MEMBER FILE UPLOADED SUCCESSFULLY", 1, 1);
                                SummaryPresentation(uploadSummary)
                            }
                            resetForm();
                        } else {
                            appalert(response.message || "UPLOAD FAILED", 1, 2);
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("Upload error details:", {
                            status: status,
                            error: error,
                            responseText: xhr.responseText,
                            statusCode: xhr.status,
                            readyState: xhr.readyState
                        });

                        updateProgressBar(0);
                        var errorMessage = "ERROR UPLOADING FILE: " + error;

                        if (xhr.responseJSON && xhr.responseJSON.message) {
                            errorMessage = xhr.responseJSON.message;
                        } else if (xhr.responseText) {
                            try {
                                var errorResponse = JSON.parse(xhr.responseText);
                                errorMessage = errorResponse.message || errorMessage;
                            } catch (e) {
                                errorMessage = "Server error: " + xhr.status + " - " + xhr.statusText;
                            }
                        }

                        appalert(errorMessage, 3, 1);
                    },
                    complete: function () {
                        setTimeout(function () {
                            $('#progressBarContainer').hide();
                        }, 2000);
                    }
                });
            },
            function () {
                appalert('UPLOAD CANCELLED', 3, 1);
                $('#progressBarContainer').hide();
            }
        );

    } catch (e) {
        console.error("Function error:", e);
        appalert("AN ERROR OCCURRED: " + e.message, 3, 1);
        $('#progressBarContainer').hide();
    }

    return false;
}


function SummaryPresentation(summary) {
    console.log(summary);
    // Populate summary values
    $('#totalAmount').text(`XAF ${summary.TotalAmount}`);
    $('#totalMembers').text(summary.TotalMembers);
    $('#branchName').text(summary.BranchName);
    $('#uploadedBy').text(summary.UploadedBy);

    // Set current date
    const currentDate = new Date().toLocaleString();
    $('#currentDate').text(currentDate);

    // Populate table with member details
    const tableBody = $('#myDataTable tbody');
    tableBody.empty(); // Clear existing rows

    if (Array.isArray(summary.manualEntryDailyCollectorUploadListDtos)) {
        summary.manualEntryDailyCollectorUploadListDtos.forEach(item => {
            const row = `
                    <tr>
                        <td>${item.MemberReference}</td>
                        <td>${item.MemberName}</td>
                        <td>XAF ${item.Amount}</td>
                        <td>${item.DailyCollectorName}</td>
                        <td>${item.MemberBranchCode}</td>
                        <td>${item.MemberBranchName}</td>
                    </tr>
                `;
            tableBody.append(row);
        });
    }

    // Optional: initialize or refresh DataTable
    if ($.fn.DataTable && $.fn.DataTable.isDataTable('#myDataTable')) {
        $('#myDataTable').DataTable().destroy();
    }
    $('#myDataTable').DataTable();
}



function updateProgressBar(progress) {
    if ($('#progressBar').length) {
        $('#progressBar').css('width', progress + '%').attr('aria-valuenow', progress);
        $('#progressBar').text(progress + '%');
    }
}

// Helper function to reset form after successful upload
function resetForm() {
    $("#AgentBranchId").val('').trigger('change'); // Reset select2 dropdown
    $("#DailyCollectorxCollectorId").val('').trigger('change'); // Reset select2 dropdown
    $("#ExcelFile").val(''); // Clear file input
}

// Optional: Add progress bar HTML if not present in your view
// Add this to your Razor view if you don't have a progress bar:
/*
<div id="progressBarContainer" style="display: none;" class="mt-3">
    <div class="progress">
        <div id="progressBar" class="progress-bar progress-bar-striped progress-bar-animated" 
             role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%">
            0%
        </div>
    </div>
</div>
*/

 
 



