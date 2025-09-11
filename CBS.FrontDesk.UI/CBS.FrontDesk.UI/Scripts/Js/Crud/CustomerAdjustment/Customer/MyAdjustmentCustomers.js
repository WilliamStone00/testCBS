// Load Members into DataTable
function loadMemberData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/MemberAdjustmentConsole/LoadMembersData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectMemberExportParams();

                filters.options = {
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    skip: d.start,
                    pageSize: d.length,
                    searchValue: '',
                    sortColumnName: d.columns[d.order[0].column].data,
                    sortColumnDirection: d.order[0].dir
                };

                return JSON.stringify(filters);
            }
        },
        columns: [
            {
                data: 'CreateDate',
                name: 'CreateDate',
                render: function (data) {
                    return moment(data).format('DD/MM/YYYY HH:mm:ss');
                }
            },
            { data: 'FullName', name: 'FullName' },
            { data: 'CustomerId', name: 'CustomerId' },
            { data: 'Phone', name: 'Phone' },
            {
                data: 'CustomerType',
                name: 'CustomerType',
                render: function (data, type, row) {
                    const label = data || '';
                    const isApproved = row.MembershipApprovalStatus === 'Approved';
                    const badgeClass = isApproved ? 'bg-primary' : 'bg-warning text-dark';
                    return `<span class="badge ${badgeClass}">${label}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (_, __, row) {
                    return `
                                    <div class="text-center dropdown">
                                        <button class="btn btn-sm btn-info dropdown-toggle" type="button" data-bs-toggle="dropdown">
                                            <i class="mdi mdi-eye"></i> Action
                                        </button>
                                        <ul class="dropdown-menu">

                                            <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="window.open('/Individual/CustomerProfile?KEY=${row.CustomerId}', '_blank')">
                                                    <i class="mdi mdi-account-circle"></i> Profile
                                                </button>
                                            </li>

                                            <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberNamesDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Initiate Member Name Adjustment
                                                </button>
                                            </li>

                                            <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberStatusDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i>Activate Or Deactivate Member Adjustment
                                                </button>
                                            </li>

                                              <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberActiveStatusDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Initiate Member Active Status Adjustment
                                                </button>
                                            </li>

                                            <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberMembershipStatusDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Initiate Membership Status Adjustment
                                                </button>
                                            </li>

                                             <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberReferenceDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Member Reference Adjustment
                                                </button>
                                            </li>

                                             <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberCategoryDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Member Category Adjustment
                                                </button>
                                            </li>

                                            <li>
                                                <button class="dropdown-item text-dark"
                                                        onclick="openMemberAdjustmentModal('/MemberAdjustmentConsole/GetMemberAccountBalanceDetailPartial?id=${row.CustomerId}','N/A','${row.CustomerId}','${row.FullName}','${row.BranchName}')">
                                                    <i class="mdi mdi-pencil-outline me-1"></i> Member Account Balance Adjustment
                                                </button>
                                            </li>

                                        </ul>
                                    </div>`;
                }

            }
        ]
    });
}

function collectMemberExportParams() {
    return {
        CustomerId: $('#customerId').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        BranchId: $('#branchId').val(),
        Gender: $('#gender').val(),
        MaritalStatus: $('#maritalStatus').val(),
        WorkingStatus: $('#workingStatus').val(),
        MembershipApprovalStatus: $('#membershipApprovalStatus').val(),
        LegalForm: $('#legalForm').val(),
        CustomerType: $('#customerType').val(),
        AgeCategoryStatus: $('#ageCategoryStatus').val(),
        DateOfBirthFrom: $('#dobFrom').val(),
        DateOfBirthTo: $('#dobTo').val(),
        CreatedFrom: $('#createdFrom').val(),
        CreatedTo: $('#createdTo').val(),
        ShowAll: false,
        options: {}
    };
}


// ==============================
// Generic Modal Loader
// ==============================
function openMemberAdjustmentModal(url, memberAdjustmentReference, memberRef, memberName, branchName) {
    $('#memberAdjustmentLoader').removeClass('d-none');
    $.get(url)
        .done(function (html) {
            var modalEl = document.getElementById('memberAdjustmentModal');
            var modal = new bootstrap.Modal(modalEl);
            var contentDiv = document.getElementById('memberNameAdjustmentsContainer');
            //document.getElementById("memberAdjustmentReference").textContent = memberAdjustmentReference || "N/A";
            document.getElementById("memberReference").textContent = memberRef || "N/A";
            document.getElementById("memberName").textContent = memberName || "N/A";
            document.getElementById("branchName").textContent = branchName || "N/A";

            // Show loading indicator
            showLoading(modal, contentDiv, html, `Loading Member Adjustment...`);
        })
        .fail(function (_, __, error) {
            console.error('Error loading partial:', error);
            appAlertError("Failed to load member details. Please try again.");
        })
        .always(function () {
            $('#memberAdjustmentLoader').addClass('d-none');
        });
}


function showLoading(modal, contentDiv, html, message) {
    // Show loading state
    contentDiv.innerHTML = html;
    modal.show();
}
// ==============================
// Form Handling inside Modal
// ==============================
function bindModalFormHandler() {
    const $form = $('#memberAdjustmentModal form');

    if ($form.length === 0) return;

    // prevent duplicate binding
    $form.off('submit').on('submit', function (e) {
        e.preventDefault();

        // simple validation example
        let isValid = true;
        $form.find('input[required], select[required]').each(function () {
            if (!$(this).val()) {
                $(this).addClass('is-invalid');
                isValid = false;
            } else {
                $(this).removeClass('is-invalid');
            }
        });

        if (!isValid) {
            appAlertError("Please fill in all required fields.");
            return;
        }

        // disable submit to avoid double clicks
        const $btn = $form.find('button[type="submit"]');
        $btn.prop('disabled', true).text('Processing...');

        // send AJAX
        $.post($form.attr('action'), $form.serialize())
            .done(function (response) {
                if (response.success) {
                    appAlertSuccess(response.message || "Adjustment saved successfully.");
                    $('#memberAdjustmentModal').modal('hide');
                    $('#myDataTable').DataTable().ajax.reload();
                } else {
                    appAlertError(response.message || "Something went wrong.");
                }
            })
            .fail(function () {
                appAlertError("Server error. Please try again.");
            })
            .always(function () {
                $btn.prop('disabled', false).text('Submit');
            });
    });
}

// ==============================
// Export
// ==============================
function exportMemberData() {
    const filters = collectMemberExportParams();
    const params = new URLSearchParams();

    for (const key in filters) {
        if (typeof filters[key] === 'object') {
            for (const subKey in filters[key]) {
                params.append(`${key}.${subKey}`, filters[key][subKey]);
            }
        } else {
            params.append(key, filters[key] ?? '');
        }
    }

    window.location.href = `/Individual/DownloadMembers?${params.toString()}`;
}

// ==============================
// Reset
//==============================
function resetMemberFilters() {
    $('#memberFilterForm').trigger('reset');
    $('.select2').val('').trigger('change');
    $('#demographicFilters, #approvalFilters, #dateFilters').addClass('d-none');
    loadMemberData();
}

// ==============================
// Init
// ==============================
$(document).ready(function () {
    // loadMemberData(); // enable if you want auto-load

    $('#applyFilterBtn').on('click', function (e) {
        e.preventDefault();
        loadMemberData();
    });

    $('#resetFilterBtn').on('click', function (e) {
        e.preventDefault();
        resetMemberFilters();
    });

    $('#exportBtn').on('click', function (e) {
        e.preventDefault();
        exportMemberData();
    });

});


function postMemberAdjustmentRequest(form) {
    $.validator.unobtrusive.parse(form);
    if (!$(form).valid()) {
        return false;
    }

    console.log("Closing Modal....")
    // ✅ Close the modal first
/*    var modalEl = document.getElementById('memberAdjustmentModal');
    var modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) {
        modal.hide();
    }*/

    // ✅ Confirmation message
    var confirmMessage = `
            <div class="text-start">
                <p>📝 This adjustment request will be <strong>logged</strong> and submitted for approval.</p>
                <p><strong>Do you want to proceed?</strong></p>
            </div>
        `;

    alertify.confirm("Confirm Adjustment Request", confirmMessage,
        function () {
            // ✅ Proceed with AJAX POST
            $.ajax({
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        alertify.success(response.message || 'Operation completed successfully');
                        appalert(response.message || 'Operation completed successfully', 1, 1);
                        // Redirect after success
                        window.location.href = '/MemberAdjustmentConsole';
                    } else {
                        appalert(response.message || 'Error during operation', 2, 1);
                    }
                },
                error: function (err) {
                    console.error("Error:", err);
                    if (err.status === 401) {
                        window.location.href = '/Authentication/Login';
                    } else {
                        appalert(err.responseJSON?.message || err.statusText || 'An error occurred', 3, 1);
                    }
                }
            });
        },
        function () {
            // ✅ Cancelled
            appalert('Operation cancelled', 3, 1);
        }
    ).set('labels', { ok: 'Yes, Proceed', cancel: 'No, Cancel' }); // Better buttons

    return false;
}