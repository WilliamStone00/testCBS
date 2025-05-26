$(document).ready(function () {

    // Listen for changes on the radio buttons
    $('input[name="operationType"]').change(function () {
        if ($(this).val() === "unique") {
            $("#isContribution").val("true"); // Set to true if "Contribution" is selected
            $("#mainview1").hide(); // Hide Top Up view
            $("#mainview2").show(); // Show Contribution view
        } else {
            $("#isContribution").val("false"); // Set to false if "Top Up" is selected
            $("#mainview1").show(); // Show Top Up view
            $("#mainview2").hide(); // Hide Contribution view
        }
    });

    // Initialize on page load
    if ($('input[name="operationType"]:checked').val() === "unique") {
        $("#isContribution").val("true");
        $("#mainview1").hide();
        $("#mainview2").show();
    } else {
        $("#isContribution").val("false");
        $("#mainview1").show();
        $("#mainview2").hide();
    }
 
});


function AjaxPostAndUpdateBulkOperation(form) {
    console.log("Form Action:", form.action);

    const today = new Date();
    const todayOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());

    let isValid = true;
    let firstInvalid = null;

    function markInvalid(field, message) {
        field.addClass('is-invalid');
        if (!firstInvalid) firstInvalid = field;
        isValid = false;
        if (message) appalert(message, 2, 1);
    }

    function clearValidation(formElement) {
        $(formElement).find('.is-invalid').removeClass('is-invalid');
    }

    // --- Validate Simulation Details Section ---
    function validateSimulationDetails(formElement) {
        console.log("🔍 Validating Simulation Details...");

        const simulationType = $(formElement).find('[name="SimulationType"]');
        const description = $(formElement).find('[name="Description"]');

        if (!simulationType.val()) {
            markInvalid(simulationType, "❌ Please select a simulation type.");
        }

        if ($.trim(description.val()) === "") {
            markInvalid(description, "❌ Description is required.");
        }
    }

    // --- Validate Source Accounts Section ---
    function validateSourceAccounts(formElement) {
        console.log("🔍 Validating Source Accounts...");

        const requiredFields = [
            "SourceAccountType", "Branch"
        ];

        requiredFields.forEach(function (fieldName) {
            const field = $(formElement).find(`[name="${fieldName}"]`);
            if (field.length && ($.trim(field.val()) === "" || field.val() == null)) {
                markInvalid(field);
            }
        });

        const minAmount = parseFloat($(formElement).find('[name="SourceAccountMinAmount"]').val()) || 0;
        const maxAmount = parseFloat($(formElement).find('[name="SourceAccountMaxAmount"]').val()) || 0;

        if (isNaN(minAmount) || minAmount < 0) {
            markInvalid($(formElement).find('[name="SourceAccountMinAmount"]'),
                "❌ Minimum amount must be 0 or greater.");
        }

        if (isNaN(maxAmount) || maxAmount < 0) {
            markInvalid($(formElement).find('[name="SourceAccountMaxAmount"]'),
                "❌ Maximum amount must be 0 or greater.");
        }

        if (maxAmount > 0 && minAmount > maxAmount) {
            markInvalid($(formElement).find('[name="SourceAccountMinAmount"]'),
                "❌ Minimum amount cannot be greater than maximum amount.");
        }
    }

    // --- Validate Destination Configuration ---
    function validateDestination(formElement) {
        console.log("🔍 Validating Destination Configuration...");

        const isUniqueAccount = $(formElement).find('input[name = "operationType"]:checked').val() === 'unique';

        console.log("Verify Contribution : ", isUniqueAccount);
        const targetAmount = parseFloat($(formElement).find('[name="TargetAmount"]').val()) || 0;

        if (isNaN(targetAmount) || targetAmount <= 0) {
            markInvalid($(formElement).find('[name="TargetAmount"]'),
                "❌ Target amount must be greater than 0.");
        }

        if (isUniqueAccount) {
            $(formElement).find($('#isContribution').val($(this).val() === 'unique'));
            const destAccountId = $(formElement).find('[name="DestinationAccountId"]');
            if ($.trim(destAccountId.val()) === "") {
                markInvalid(destAccountId, "❌ Destination account ID is required.");
            }
        } else {
            const destAccountType = $(formElement).find('[name="DestinationAccountType"]');
            if (!destAccountType.val()) {
                markInvalid(destAccountType, "❌ Destination account type is required.");
            }
        }
    }

    // --- Clear all previous validations first
    clearValidation(form);

    // --- Validate All Sections
    validateSimulationDetails(form);
    validateSourceAccounts(form);
    validateDestination(form);

    if (!isValid) {
        if (firstInvalid) firstInvalid.focus();
        if (!$(".appalert:visible").length) {
            appalert("❌ Please correct the highlighted fields before submitting.", 2, 1);
        }
        return false;
    }

    // --- Standard jQuery Unobtrusive
   /* $.validator.unobtrusive.parse(form);
    if (!$(form).valid()) {
        appalert("❌ Please correct validation errors before submitting.", 2, 1);
        return false;
    }*/

    console.log("Form Action ", form.action);

    // --- Confirmation and Submit
    alertify.confirm("Confirmation", "Are you sure you want to simulate this bulk operation?",
        function () {
            const ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {
                    console.log("Response:", response);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        setTimeout(() => {
                            if (response.redirectUrl) {
                                window.location.href = response.redirectUrl;
                            } else {
                                location.reload();
                            }
                        }, 1500);
                    } else {
                        appalert(response.message || "❌ Simulation failed.", 2, 1);
                    }
                },
                error: function (err) {
                    console.log("Error:", err);
                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.statusText || "❌ An error occurred during simulation.", 0, 1);
                    }
                }
            };

            if ($(form).attr('enctype') === "multipart/form-data") {
                ajaxConfig.contentType = false;
                ajaxConfig.processData = false;
            }

            $.ajax(ajaxConfig);
        },
        function () {
            appalert('Simulation cancelled.', 3, 1);
        }
    );

    return false;
}