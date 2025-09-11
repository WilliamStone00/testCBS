
$(document).ready(function () {
    LoadUsers("All");
    /*LoadDataGen('Individual', 'myDataTable', '_IndividualData', 0, 'datalistingview', null, 'all')*/
});


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}



function manualSearch() {
    LoadUsers($('#manualSearchInput').val())
}



function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Transactions", "InitializeData");
}



function LoadUsers(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/Group/LoadData",
            "type": "GET",
            "data": {
                "searchCriteria": search
            },
            "dataSrc": function (json) {
                if (json.error) {
                    console.error(json.error);
                    return [];
                }
                return json.data;
            }
        },
        "columns": [
            { "data": "GroupName", "name": "GroupName", "autoWidth": true },
            { "data": "RegistrationNumber", "name": "RegistrationNumber", "autoWidth": true },
            { "data": "TaxPayerNumber", "name": "TaxPayerNumber", "autoWidth": true },
            {
                "data": "GroupId", "orderable": false, "render": function (data)
                {
                    return `<a href='/Group/Details?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View ${data} detail'>Details</a>`;
                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "70%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "10%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "10%" },
        ],
        "order": [[0, "asc"]],
        "orderFixed": [[0, "asc"]],
    });
}





function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}

function AjaxPostAndUpdateGroupRegistration(form) {
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

    // --- Validate Profile Section ---
    function validateProfileSection(formElement) {
        console.log("🔍 Validating Profile Section...");

        const requiredFields = [
            "GroupTypeId", "GroupName", "CustomerType",
            "RegistrationNumber", "DateOfEstablishment",
            "Phone", "IDNumber", "IDNumberIssueDate", "IDNumberIssueAt"
        ];

        requiredFields.forEach(function (fieldName) {
            const field = $(formElement).find(`[name="${fieldName}"]`);
            if (field.length && ($.trim(field.val()) === "" || field.val() == null)) {
                markInvalid(field);
            }
        });

        const establishmentDate = new Date($(formElement).find('[name="DateOfEstablishment"]').val());
        if (establishmentDate && establishmentDate > todayOnly) {
            markInvalid($(formElement).find('[name="DateOfEstablishment"]'), "❌ Date of Establishment cannot be in the future.");
        }

        const idIssueDate = new Date($(formElement).find('[name="IDNumberIssueDate"]').val());
        if (idIssueDate && idIssueDate > todayOnly) {
            markInvalid($(formElement).find('[name="IDNumberIssueDate"]'), "❌ ID Issue Date cannot be in the future.");
        }
    }

    // --- Validate Address Section ---
    function validateAddressSection(formElement) {
        console.log("🔍 Validating Address Section...");

        const requiredDropdowns = ["CountryId", "RegionId", "DivisionId", "SubDivisionId", "TownId"];
        requiredDropdowns.forEach(function (fieldName) {
            const field = $(formElement).find(`[name="${fieldName}"]`);
            if (field.length && (!field.val() || field.val() === "")) {
                markInvalid(field);
            }
        });

        const emailField = $(formElement).find('[name="Email"]');
        const faxField = $(formElement).find('[name="Fax"]');
        const addressField = $(formElement).find('[name="Address"]');
        const poBoxField = $(formElement).find('[name="POBox"]');

        const email = $.trim(emailField.val());
        const fax = $.trim(faxField.val());
        const address = $.trim(addressField.val());
        const poBox = $.trim(poBoxField.val());

        // ✅ Email is REQUIRED and Valid
        if (email === "") {
            markInvalid(emailField, "❌ Email address is required.");
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
            markInvalid(emailField, "❌ Please enter a valid email address.");
        }

        if (fax === "") {
            markInvalid(faxField, "❌ Fax Number is required.");
        }
        if (address === "") {
            markInvalid(addressField, "❌ Address is required.");
        }
        if (poBox === "") {
            markInvalid(poBoxField, "❌ Post Box is required.");
        }
    }

    // --- Validate Activities Section ---
    function validateActivitiesSection(formElement) {
        console.log("🔍 Validating Activities Section...");

        const requiredFields = [
            "EconomicActivitiesId", "Occupation", "CustomerCategoryId", "FormalOrInformalSector"
        ];

        requiredFields.forEach(function (fieldName) {
            const field = $(formElement).find(`[name="${fieldName}"]`);
            if (field.length && ($.trim(field.val()) === "" || field.val() == null)) {
                markInvalid(field);
            }
        });

        const incomeField = $(formElement).find('[name="Income"]');
        const income = parseFloat($.trim(incomeField.val())) || 0;
        if (isNaN(income) || income <= 0) {
            markInvalid(incomeField, "❌ Capital Amount must be greater than 0.");
        }
    }

    // --- Clear all previous validations first
    clearValidation(form);

    // --- Validate All Sections
    validateProfileSection(form);
    validateAddressSection(form);
    validateActivitiesSection(form);

    if (!isValid) {
        if (firstInvalid) firstInvalid.focus();
        if (!$(".appalert:visible").length) {
            appalert("❌ Please correct the highlighted fields before submitting.", 2, 1);
        }
        return false;
    }

    // --- Standard jQuery Unobtrusive
    $.validator.unobtrusive.parse(form);
    if (!$(form).valid()) {
        appalert("❌ Please correct validation errors before submitting.", 2, 1);
        return false;
    }

    // --- Confirmation and Submit
    alertify.confirm("Confirmation", "Are you sure you want to perform this action?",
        function () {
            const ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {
                    console.log("Response:", response);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        setTimeout(() => location.reload(), 1500);
                    } else {
                        appalert(response.message || "❌ Operation failed.", 2, 1);
                    }
                },
                error: function (err) {
                    console.log("Error:", err);
                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.statusText, 0, 1);
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
            appalert('Transaction cancelled.', 3, 1);
        }
    );

    return false;
}


//function AjaxPostAndUpdateGroupRegistration(form) {
//    console.log("📄 Submitting Group/Moral Person form:", form.action);

//    var formData = new FormData(form);
//    for (var pair of formData.entries()) {
//        console.log(pair[0] + ": " + pair[1]);
//    }

//    // Custom Validation Function
//    function validateGroupForm(formElement) {
//        let isValid = true;
//        let firstInvalid = null;

//        const today = new Date();
//        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

//        $(formElement).find('.is-invalid').removeClass('is-invalid');

//        $(formElement).find('input, select, textarea').each(function () {
//            const field = $(this);
//            const name = field.attr("name");
//            const val = $.trim(field.val());
//            const isRequired = field.prop('required') || field.hasClass('required');

//            // General required field check
//            if (isRequired && (val === "" || val === null)) {
//                field.addClass('is-invalid');
//                if (!firstInvalid) firstInvalid = field;
//                isValid = false;
//                return;
//            }

//            // Email validation
//            if (name === "Email" && val && !emailPattern.test(val)) {
//                field.addClass('is-invalid');
//                if (!firstInvalid) firstInvalid = field;
//                isValid = false;
//                appalert("❌ Please enter a valid email address.", 2, 1);
//                return;
//            }

//            // Income (Capital) validation
//            if (name === "Income" && val) {
//                const income = parseFloat(val);
//                if (isNaN(income) || income <= 0) {
//                    field.addClass('is-invalid');
//                    if (!firstInvalid) firstInvalid = field;
//                    isValid = false;
//                    appalert("❌ Capital/Income must be a number greater than 0.", 2, 1);
//                    return;
//                }
//            }

//            // Date of Establishment must not be in the future
//            if (name === "DateOfEstablishment" && val) {
//                const estDate = new Date(val);
//                if (estDate > today) {
//                    field.addClass('is-invalid');
//                    if (!firstInvalid) firstInvalid = field;
//                    isValid = false;
//                    appalert("❌ Date of Establishment cannot be in the future.", 2, 1);
//                    return;
//                }
//            }

//            // ID Issue Date must not be in future
//            if (name === "IDNumberIssueDate" && val) {
//                const issueDate = new Date(val);
//                if (issueDate > today) {
//                    field.addClass('is-invalid');
//                    if (!firstInvalid) firstInvalid = field;
//                    isValid = false;
//                    appalert("❌ ID Card Issue Date cannot be in the future.", 2, 1);
//                    return;
//                }
//            }
//        });

//        if (!isValid && firstInvalid) {
//            firstInvalid.focus();
//            if (!$(".appalert:visible").length) {
//                appalert("❌ Please correct the highlighted fields before submitting.", 2, 1);
//            }
//        }

//        return isValid;
//    }

//    // 🔵 Run Validation
//    if (!validateGroupForm(form)) {
//        return false;
//    }

//    // 🔵 Also Run jQuery Validation
//    $.validator.unobtrusive.parse(form);
//    if (!$(form).valid()) {
//        appalert("❌ Please fix all validation errors before submitting.", 2, 1);
//        return false;
//    }

//    // 🟢 Confirmation and Ajax Submit
//    alertify.confirm("Confirmation", "Are you sure you want to perform this action?",
//        function () {
//            const ajaxConfig = {
//                type: 'POST',
//                url: form.action,
//                data: new FormData(form),
//                success: function (response) {
//                    console.log("✅ Server Response:", response);
//                    if (response.success) {
//                        appalert(response.message || "✔️ Operation successful.", 1, 1);
//                        setTimeout(() => location.reload(), 1500);
//                    } else {
//                        appalert(response.message || "❌ Operation failed.", 2, 1);
//                    }
//                },
//                error: function (err) {
//                    console.error("❌ Error:", err);
//                    if (err.status === 401) {
//                        window.location.href = '/Authentication/Login';
//                    } else {
//                        appalert(err.statusText, 0, 1);
//                    }
//                }
//            };

//            if ($(form).attr('enctype') === "multipart/form-data") {
//                ajaxConfig.contentType = false;
//                ajaxConfig.processData = false;
//            }

//            $.ajax(ajaxConfig);
//        },
//        function () {
//            appalert('⛔ Transaction cancelled.', 3, 1);
//        }
//    );

//    return false;
//}
