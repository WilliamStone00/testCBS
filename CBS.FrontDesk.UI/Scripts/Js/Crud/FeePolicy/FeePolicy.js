
$(document).ready(function () {
    // Function to toggle MFI and Event sections and update the label

    $('#eventDiv').hide();  // Show EventDiv if needed

    // Initial load: hide/show based on checkbox state
    toggleSections();

    // Attach event listener to checkbox
    $('#isCentralisedCheck').change(function () {
        toggleSections();
    });
});
function toggleSections() {
    if ($('#isCentralisedCheck').is(':checked')) {
        $('#mfi').show();  // Show MFI if IsCentralised is checked
        $('#isCentralisedLabel').text("Uncheck to centralise this policy");  // Update label text
    } else {
        $('#mfi').hide();  // Hide MFI if IsCentralised is not checked
        $('#isCentralisedLabel').text("Check to de-centralise this policy");  // Revert label text
    }

}
function AddORUpdateGenCustom(KEY, divToLoadData, partialView, path, controller, serviceOption, feeid) {
    // Initialize data and reset the main form
    EditResetMainCustom(KEY, partialView, divToLoadData, controller, "InitializeData", null, path, null, serviceOption);

    // Initialize select2 elements
    $('.select2').select2();


}
function GetBranchesByBank(KEY, affectedID) {
    path = "target";
    var url = "/FeePolicy/Ajaxloader?Key=" + KEY + "&path=" + path;
    FillDropDownAjaxCallParam(url, affectedID, "---Select Option---");
}
function EditResetMainCustom(KEY, partialView, divID, controller, action, div1, path, div2, serviceOption) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?KEY=' + KEY + '&partialView=' + partialView + '&path=' + path + '&serviceOption=' + serviceOption,
        success: function (data) {
            $('#' + divID).html(data);
            $('#' + div2).hide();
            $('#' + div1).show();
            $('.select2').select2();

        }, error: function (err) {

            appalert(err.statusText, 3, 0);
        }
    });
}

function GetFee(KEY) {
    // Make an AJAX request
    $.ajax({
        type: "GET", // Using HTTP GET method
        url: '/FeePolicy/GetFee?KEY=' + KEY, // URL to fetch fee data based on the provided key
        success: function (data) { // Callback function executed if the request is successful
            console.log(data.FeeType); // Output the FeeType received from the server to the console

            // Show or hide the eventDiv based on the OperationFeeType
            if (data.OperationFeeType === "MemberShip" || data.OperationFeeType === "DailySaverRegistrationFee") {
                $('#eventDiv').show(); // Show the eventDiv if OperationFeeType is "MemberShip"
            } else {
                $('#eventDiv').hide(); // Hide the eventDiv if OperationFeeType is not "MemberShip"
            }

            // Check the FeeType received from the server
            if (data.FeeType === "Rate") {
                // If FeeType is Rate, show input for entering rate (%) and hide input for entering range
                $('#lblevalue').html("Enter value as rate (%) E.G 2.5");
                $('#percentage').show(); // Show input for rate
                $('#range').hide(); // Hide input for range

                // Reset values and hide elements within #range
                $('#range input[type="text"]').val(''); // Reset input values
                $('#range').hide(); // Hide the entire #range section
            }
            else if (data.OperationFeeType === "MemberShip" || data.OperationFeeType === "DailySaverRegistrationFee")
            {
                $('#memberShip').show(); // Show input for range
                $('#range').show(); // Show input for range
                $('#amountfrom').hide(); // Show input for range
                $('#amountto').hide(); // Show input for range
                $('#percentage').hide(); // Show input for rate
            }
            else {
                // If FeeType is neither Rate nor Flat, assume it's Range and show input for entering range
                $('#percentage').hide(); // Hide input for rate

                // Reset input values and show #range
                $('#range input[type="text"]').val(''); // Reset input values
                $('#range').show(); // Show input for range
                $('#amountfrom').show(); // Show input for range
                $('#amountto').show(); // Show input for range
                $('#memberShip').show(); // Show input for range

            }

            // Output the selected FeeType to the UI
            $('#select_base').html("Configuration option: " + data.FeeType + "[" + data.OperationFeeType + "]");
        },
        error: function (err) { // Callback function executed if there's an error in the AJAX request
            appalert(err.statusText, 1, 3); // Show an alert with the error message
        }
    });
}


function EditReset(KEY, partialView) {
    EditResetMain(KEY, partialView, "mainview", "Individual", "InitializeData");
}


function AjaxPostLoanScedule(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    LoadLocalSchedule();
                }
                else {
                    appalert(response.message, 2, 1);

                }

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



function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}

function LoadLocalSchedule() {
    LoadDataGen('MemberOperation', null, '_LoanSimulationScheduleData', 0, 'amortization_schedule_data_div', "KEY", 'applications', 'loan_schedule')
}



