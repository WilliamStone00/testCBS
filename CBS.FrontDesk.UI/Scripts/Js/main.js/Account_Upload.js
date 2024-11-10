$(document).ready(function () {
 
  
    // Trigger the processing simulation when needed
    $('#ReadUploadedFile').click(function () {
        simulateProcessing();
    });
 



});
function updateProgressBar(progress) {
    var progressBar = $('.progress-bar');
    progressBar.css('width', progress + '%');
    progressBar.attr('aria-valuenow', progress);
    progressBar.text(progress + '%');
}

function simulateProcessing() {
    var progress = 0;
    var interval = setInterval(function () {
        progress += 10;
        updateProgressBar(progress);

        if (progress >= 100) {
            clearInterval(interval);
            console.log('Processing complete!');
        }
    }, 1000); // Update every 1 second
}

function DownloadFile(path) {
 window.open(path, "_blank");
}

function ReadExcelFile() {
    var formData = new FormData();
    var file = $("#uploadedFile")[0].files[0];
    var branchCode = $("#branchCodeDropdown").val();

    if (branchCode === "") {
        appalert("PLEASE KINDLY SELECT YOUR BRANCH CODE", 2, 1);
        return;
    }

    formData.append("ExcelFile", file);
    formData.append("BranchId", branchCode);

    event.preventDefault();

    // Initialize progress bar
    updateProgressBar(0);
    $('#progressBarContainer').show();

    $.ajax({
        url: "/AccountingConfiguration/UploadAccountModel/",
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    var progress = Math.round(percentComplete * 100);
                    updateProgressBar(progress);
                }
            }, false);
            return xhr;
        },
        success: function (response) {
            updateProgressBar(100);
            appalert("File uploaded successfully", 1, 1);
            displayResults(response);
            console.log(response);
        },
        error: function (xhr, status, error) {
            updateProgressBar(0);
            appalert("Error uploading file: " + error, 3, 1);
            console.log("Error uploading file: " + error);
        },
        complete: function () {
            setTimeout(function () {
                $('#progressBarContainer').hide();
            }, 2000);
        }
    });
}

function updateProgressBar(progress) {
    var progressBar = $('.progress-bar');
    progressBar.css('width', progress + '%');
    progressBar.attr('aria-valuenow', progress);
    progressBar.text(progress + '%');
}


function LoadFileHistory(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "30%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "25%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "20%" },
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

function displayResults(response) {
    if (response.success && response.Data && response.Data.apiResponseData) {
        var data = response.Data.apiResponseData;
        var accountsPresent = data.Account_Present;
        var totalAccounts = data.Total_Account;
        var accountsNotMatching = totalAccounts - accountsPresent;
        var filePath = data.file_path;

        $('#accountsPresent').text(accountsPresent);
        $('#totalAccounts').text(totalAccounts);
        $('#accountsNotMatching').text(accountsNotMatching);
        // Update download linkDownload non-matching accounts:
        var message = "";
        if (accountsPresent === 0) {
            message = "Download chart of account";
        } else {
            message = "Download non-matching accounts";
        }
      
        var downloadUrl = filePath;
        //  window.open("", "_blank");
        $('#downloadLink').attr('href', downloadUrl);
        $('#downloadLink').text(message);
        $('#downloadLink').attr('target', '_blank'); // This opens the link in a new tab/window
        //      window.open("/Reports/PrintAccountLedgerDtoInExcel", "_blank");
        // Add onclick event to trigger download in a new window
        $('#downloadLink').off('click').on('click', function (e) {
            e.preventDefault(); // Prevent default link behavior
            window.open(downloadUrl, '_blank'); // Open in new window
        });

        // Show the result container
        $('#resultContainer').show();

        // Display success message
        appalert(response.message, 1, 1);
    } else {
        console.log(response);
      
        appalert(response.message, 3, 1);
    }
}



