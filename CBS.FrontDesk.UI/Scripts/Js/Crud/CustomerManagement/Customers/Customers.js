


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Frontend", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Frontend", "InitializeData");
}


function AjaxPostAndUpdateMemberRegistration(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);

    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }

    // --- Custom Client-Side Validation ---
    function validateMemberForm(formElement) {
        let isValid = true;
        let firstInvalid = null;

        const today = new Date();
        const minDOB = new Date(today.getFullYear() - 5, today.getMonth(), today.getDate());
        const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        const maritalStatus = $(formElement).find('[name="MaritalStatus"]').val();
        const workingStatus = $(formElement).find('[name="WorkingStatus"]').val();

        $(formElement).find('.is-invalid').removeClass('is-invalid');

        const employerFields = ["EmployerName", "EmployerTelephone", "EmployerAddress", "Income"]; // ⬅️ Moved out for clarity

        $(formElement).find('input, select, textarea').each(function () {
            const field = $(this);
            const name = field.attr("name");
            const val = $.trim(field.val());
            const isRequired = field.prop('required') || field.hasClass('required');
            const isEmployerField = employerFields.includes(name);

            const income = parseFloat($(formElement).find('[name="Income"]').val()) || 0;

            // ✅ Updated Working Status Dependent Validation
            if (workingStatus && isEmployerField) {
                if (workingStatus === "Unemployed") {
                    // If Unemployed, skip employer fields validation entirely
                    return;
                }

                if (workingStatus === "Self_Employed" || workingStatus === "Other_Sources_Of_Income") {
                    if (name === "Income" && (isNaN(income) || income <= 0)) {
                        field.addClass('is-invalid');
                        if (!firstInvalid) firstInvalid = field;
                        isValid = false;
                        appalert("❌ Income must be greater than 0 for Self-Employed or Other Sources of Income.", 2, 1);
                        return;
                    }
                } else {
                    // Normal Employed
                    if (!val && name !== "Income") {
                        field.addClass('is-invalid');
                        if (!firstInvalid) firstInvalid = field;
                        isValid = false;
                        appalert("❌ Employer information is required for employed members.", 2, 1);
                        return;
                    }
                    if (name === "Income" && (isNaN(income) || income <= 0)) {
                        field.addClass('is-invalid');
                        if (!firstInvalid) firstInvalid = field;
                        isValid = false;
                        appalert("❌ Income must be greater than 0 for employed members.", 2, 1);
                        return;
                    }
                }
            }

            // ✅ General required check (smart)
            if (isRequired && (val === "" || val === null)) {
                if (workingStatus === "Unemployed" && isEmployerField) {
                    return; // ✅ Skip if unemployed and employer field
                }

                field.addClass('is-invalid');
                if (!firstInvalid) firstInvalid = field;
                isValid = false;
                return;
            }

            // ✅ Marital status dependent fields
            const maritalRequiredFields = [
                "SpouseName",
                "SpouseAddress",
                "SpouseContactNumber",
                "NumberOfKids",
                "SpouseOccupation"
            ];

            if (maritalStatus === "Married" && maritalRequiredFields.includes(name)) {
                if (!val) {
                    field.addClass('is-invalid');
                    if (!firstInvalid) firstInvalid = field;
                    isValid = false;
                    appalert("❌ Please complete all spouse-related fields for Married status.", 2, 1);
                    return;
                }
            }

            // ✅ Date of Birth: Minimum 5 years
            if (name === "DateOfBirth" && val) {
                const dob = new Date(val);
                if (dob > minDOB) {
                    field.addClass('is-invalid');
                    if (!firstInvalid) firstInvalid = field;
                    isValid = false;
                    appalert("❌ Member must be at least 5 years old.", 2, 1);
                    return;
                }
            }

            // ✅ ID Card Issue Date: Cannot be in the future
            if (name === "IDNumberIssueDate" && val) {
                const issueDate = new Date(val);
                if (issueDate > today) {
                    field.addClass('is-invalid');
                    if (!firstInvalid) firstInvalid = field;
                    isValid = false;
                    appalert("❌ ID Card Issue Date cannot be in the future.", 2, 1);
                    return;
                }
            }

            // ✅ ID Card Expiry Date: Must be strictly after today
            if (name === "IDNumberExpiryDate" && val) {
                const expiryDate = new Date(val);
                const todayOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
                if (expiryDate <= todayOnly) {
                    field.addClass('is-invalid');
                    if (!firstInvalid) firstInvalid = field;
                    isValid = false;
                    appalert("❌ ID Card Expiry Date must be greater than today.", 2, 1);
                    return;
                }
            }

            // ✅ Email validation
            if (name === "Email" && val && !emailPattern.test(val)) {
                field.addClass('is-invalid');
                if (!firstInvalid) firstInvalid = field;
                isValid = false;
                appalert("❌ Please enter a valid email address.", 2, 1);
                return;
            }
        });

        if (!isValid && firstInvalid) {
            firstInvalid.focus();
            if (!$(".appalert:visible").length) {
                appalert("❌ Please correct the highlighted fields before submitting.", 2, 1);
            }
        }

        // ✅ Validate Mother's Information: All required
        $(formElement).find('input[name="MName"], input[name="MPhone"], input[name="MOccupation"], input[name="MAddress"]').each(function () {
            const field = $(this);
            if (!$.trim(field.val())) {
                field.addClass('is-invalid');
                if (!firstInvalid) firstInvalid = field;
                isValid = false;
                appalert("❌ Please complete all required Mother's information fields.", 2, 1);
                return false; // break the each
            }
        });

        return isValid;
    }



    // Validate form
    if (!validateMemberForm(form)) {
        return false;
    }

    // Proceed with jQuery Unobtrusive Validation
    $.validator.unobtrusive.parse(form);
    if (!$(form).valid()) {
        appalert("❌ Please correct validation errors before submitting.", 2, 1);
        return false;
    }

    // Confirmation
    alertify.confirm("Confirmation", "Are you sure you want to perform this action? ",
        function () {
            const ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {
                    console.log("Response:", response);
                    if (response.success) {
                        if (response.status === "Exist") {
                            appalert(response.message, 3, 1);
                        } else if (response.status === "Failed") {
                            appalert(response.message, 2, 1);
                        } else {
                            appalert(response.message, 1, 1);
                            setTimeout(() => location.reload(), 1500);
                        }
                    } else {
                        const msg = response.message || "❌ Operation failed.";
                        appalert(msg, response.status === "Exist" ? 3 : 2, 1);
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

            console.log("AJAX Config:", ajaxConfig);
            $.ajax(ajaxConfig);
        },
        function () {
            appalert('Transaction cancelled', 3, 1);
        }
    );

    return false;
}


function manualSearch() {
    LoadUsers($('#manualSearchInput').val())
}

function AjaxPostAndUpdateMembers(form) {
    // Parse form for client-side validation
    $.validator.unobtrusive.parse(form);

    // Run custom field-level validation
    if (!validateMemberForm(form)) {
        return false;
    }

    if ($(form).valid()) {
        alertify.confirm("Confirmation", "Are you sure you want to update the member profile?",
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    success: function (response) {
                        if (response.success) {
                            appalert(response.message, 1, 1);
                            setTimeout(() => location.reload(), 1500);
                        } else {
                            appalert(response.message || "Update failed.", 2, 1);
                        }
                    },
                    error: function (err) {
                        appalert(err.statusText || "Server error.", 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig.contentType = false;
                    ajaxConfig.processData = false;
                }

                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Update cancelled', 3, 1);
            }
        );
    }

    return false;
}


function showConfirmMessage(KEY, ServiceOption, tableID) {
    DeleteData("Transactions", KEY, ServiceOption, "datalistingview", tableID, "InitializeData");

}
function EditReset(KEY, ServiceOption) {
    EditResetMain(KEY, ServiceOption, "mainview", "Transactions", "InitializeData");
}

//function LoadUsers(search) {
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "processing": false, // Display processing indicator during AJAX request
//        "paging": true, // Enable pagination
//        "info": true, // Enable table information display
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
//        "searching": false, // Enable search bar
//        "ajax": {
//            "url": "/Individual/LoadData",
//            "type": "GET",
//            "dataType": "json",
//            "data": function (d) {
//                // Pass additional parameters to the server
//                d.searchCriteria = search;
//            },
//            "dataSrc": function (json) {
//                // Set total records count from the server response
//                return json.data;
//            }
//        },
//        "columns": [
//            { "data": "name", "name": "userName", "autoWidth": true },
//            { "data": "CustomerId", "name": "email", "autoWidth": true },
//            { "data": "Phone", "name": "name", "autoWidth": true },
//            { "data": "branch", "name": "phoneNumber", "autoWidth": true },
//            { "data": "MembershipApprovalStatus", "name": "MembershipApprovalStatus", "autoWidth": true },
//            {
//                "data": "CustomerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Individual/CustomerProfile?KEY=" + data + "' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'>Profile</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
//            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
//            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },

//        ],
//        "order": [[0, "asc"]],
//        "orderFixed": [[0, "asc"]],
//        "createdRow": function (row, data, dataIndex) {
//            // Add a badge based on the membership approval status
//            var approvalStatus = data.MembershipApprovalStatus;
//            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
//            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
//            $('td:eq(5)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
//        }
//    });
//}

function LoadUsers(search) {
    $("#myDataTable").DataTable({
        "destroy": true,
        "serverSide": true,
        "info": true,
        "stateSave": true,
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
        "searching": false,
        "ajax": {
            "url": "/Individual/LoadData",
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
            { "data": "name", "name": "firstName", "autoWidth": true },
            { "data": "CustomerId", "name": "customerId", "autoWidth": true },
            { "data": "Phone", "name": "phone", "autoWidth": true },
            { "data": "branch", "name": "branch", "autoWidth": true },
            { "data": "MembershipApprovalStatus", "name": "membershipApprovalStatus", "autoWidth": true },
            {
                "data": "CustomerId", "orderable": false, "render": function (data)
                {
                    return `<a href='/Individual/CustomerProfile?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View ${data} detail'>Profile</a>`;
                }
            }
        ],
        "columnDefs": [
            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },
        ],
        "order": [[0, "asc"]],
        "orderFixed": [[0, "asc"]],
        "createdRow": function (row, data, dataIndex) {
            var approvalStatus = data.MembershipApprovalStatus;
            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
            $('td:eq(4)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
        }
    });
}


//function LoadUsers(searchObj) {
//    $("#myDataTable").DataTable({
//        "destroy": true,
//        "serverSide": true,
//        "info": true,
//        "stateSave": true,
//        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]],
//        "searching": false, // Hide the search bar
//        "ajax": {
//            "url": "/Individual/LoadData?searchCriteria=" + searchObj,
//            "type": "GET",
//            "datatype": "json"
//        },
//        "columns": [
//            { "data": "name", "name": "FirstName", "autoWidth": true },
//            { "data": "CustomerId", "name": "CustomerId", "autoWidth": true },
//            { "data": "Phone", "name": "Phone", "autoWidth": true },
//            { "data": "branch", "name": "Branch", "autoWidth": true },
//            { "data": "MembershipApprovalStatus", "name": "MembershipApprovalStatus", "autoWidth": true },
//            {
//                "data": "CustomerId", "orderable": false, "render": function (data) {
//                    return "<a href='/Individual/CustomerProfile?KEY=" + data + "' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'>Profile</a>";
//                }
//            }
//        ],
//        "columnDefs": [
//            { "targets": 0, "searchable": true, "orderable": true, "width": "40%" },
//            { "targets": 1, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 2, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 3, "searchable": true, "orderable": true, "width": "29%" },
//            { "targets": 4, "searchable": true, "orderable": true, "width": "8%" },
//            { "targets": 5, "searchable": true, "orderable": true, "width": "7%" },
//        ],
//        "order": [[0, "asc"]],
//        "orderFixed": [[0, "asc"]],
//        "createdRow": function (row, data, dataIndex) {
//            // Add a badge based on the membership approval status
//            var approvalStatus = data.MembershipApprovalStatus;
//            var badgeClass = approvalStatus === "Approved" ? "badge-success" : "badge-secondary";
//            var badgeText = approvalStatus === "Approved" ? "Approved" : "Awaits Validation";
//            $('td:eq(4)', row).html('<label class="ql-color-purple"><span class="badge rounded-pill rounded-2 badge ' + badgeClass + ' bg-label-primary fs-tiny py-1">' + badgeText + '</span></label>');
//        }
//    });
//}



function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}



