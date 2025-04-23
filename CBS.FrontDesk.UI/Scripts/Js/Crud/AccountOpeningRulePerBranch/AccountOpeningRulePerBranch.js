// 📁 File: /Scripts/Js/Crud/AccountOpening/AccountOpeningRulePerBranch.js

$(document).ready(function () {
    $('.select2').select2({ width: '100%' });

    $('#filterRulesBtn').on('click', function () {
        const branchId = $('#branchIdFilter').val();
        const productId = $('#savingProductIdFilter').val();
        loadAccountOpeningRules(branchId, productId);
    });

    $('#saveRuleBtn').on('click', function () {
        saveAccountOpeningRule();
    });

    loadAccountOpeningRules($('#branchIdFilter').val(), $('#savingProductIdFilter').val());
});

function loadAccountOpeningRules(branchId = '', savingProductId = '') {
    $('#myDataTable').DataTable({
        destroy: true,
        serverSide: false,
        ajax: {
            url: '/AccountOpeningRulePerBranch/GetAllAccountOpeningRules',
            type: 'POST',
            data: {
                BranchId: branchId,
                SavingProductId: savingProductId
            },
            dataSrc: function (json) {
                if (!json.success && json.status === "ValidationError") {
                    appalert(json.message, 3, 1);
                    return [];
                }
                return json.data;
            }
        },
        columns: [
            { data: 'BranchName', title: 'BranchName' },
            { data: 'ProductName', title: 'Product' },
            { data: 'LegalForm', title: 'Legal Form' },
            {
                data: 'MinimumOpeningBalance',
                title: 'Min Balance',
                render: $.fn.dataTable.render.number(',', '.', 2, 'XAF ')
            },
            {
                data: 'Id',
                title: 'Actions',
                orderable: false,
                render: function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <button class="btn btn-outline-primary btn-sm me-1" onclick="editRule('${data}')">
                                <i class="mdi mdi-pencil-outline me-1"></i>Edit
                            </button>
                            <button class="btn btn-outline-danger btn-sm" onclick="deleteRule('${data}')">
                                <i class="mdi mdi-delete-outline me-1"></i>Delete
                            </button>
                        </div>
                    `;
                }
            }
        ]
        //columnDefs: [
        //    { targets: 0, width: "50%" },
        //    { targets: 1, width: "10%" },
        //    { targets: 2, width: "15%" },
        //    { targets: 3, width: "10%" },
        //    { targets: 4, width: "15%", className: "text-center" }
        //]
    });
}

function openCreateRuleModal() {
    clearForm();

    const selectedBranchId = $('#branchIdFilter').val();
    const selectedProductId = $('#savingProductIdFilter').val();

    if (!selectedBranchId || !selectedProductId) {
        appalert("Please select both a branch and a saving product before adding a rule.", 3, 1);
        return;
    }

    $('#branchId').val(selectedBranchId);
    $('#savingProductId').val(selectedProductId);

    $('#accountRuleModalTitle').text("Create Account Opening Rule");
    $('#accountRuleModal').modal('show');
}

function editRule(id) {
    $.get('/AccountOpeningRulePerBranch/GetById', { id: id }, function (response) {
        if (response.success) {
            const rule = response.data;
            $('#ruleId').val(rule.Id);
            $('#branchId').val(rule.BranchId);
            $('#legalForm').val(rule.LegalForm);
            $('#savingProductId').val(rule.SavingProductId);
            $('#minBalance').val(rule.MinimumOpeningBalance);
            $('#accountRuleModalTitle').text("Edit Account Opening Rule");
            $('#accountRuleModal').modal('show');
        } else {
            appalert(response.message, 3, 1);
        }
    });
}

function saveAccountOpeningRule() {
    let isValid = true;

    $('#accountRuleForm .form-control, .form-select').removeClass('is-invalid');
    $('#accountRuleForm .invalid-feedback').hide();

    if (!$('#branchId').val()) {
        $('#branchId').addClass('is-invalid');
        $('#branchId-error').show();
        isValid = false;
    }
    if (!$('#legalForm').val()) {
        $('#legalForm').addClass('is-invalid');
        $('#legalForm-error').show();
        isValid = false;
    }
    if (!$('#savingProductId').val()) {
        $('#savingProductId').addClass('is-invalid');
        $('#savingProductId-error').show();
        isValid = false;
    }
    if (!$('#minBalance').val()) {
        $('#minBalance').addClass('is-invalid');
        $('#minBalance-error').show();
        isValid = false;
    }

    if (!isValid) {
        appalert("Please correct the highlighted fields before saving.", 3, 1);
        return;
    }

    alertify.confirm(
        'Confirm Save',
        'Do you want to save this account opening rule?',
        function () {
            $('#accountRuleLoader').removeClass('d-none');

            const model = {
                Id: $('#ruleId').val(),
                BranchId: $('#branchId').val(),
                LegalForm: $('#legalForm').val(),
                SavingProductId: $('#savingProductId').val(),
                MinimumOpeningBalance: parseFloat($('#minBalance').val())
            };

            $.post('/AccountOpeningRulePerBranch/CreateOrUpdate', model, function (response) {
                $('#accountRuleLoader').addClass('d-none');

                if (response.success) {
                    $('#accountRuleModal').modal('hide');
                    $('#myDataTable').DataTable().ajax.reload();
                    appalert(response.message, 1, 1); // ✅ success
                } else {
                    appalert(response.message, 3, 1); // ❌ server/validation failure
                }
            }).fail(function () {
                $('#accountRuleLoader').addClass('d-none');
                appalert("Something went wrong while saving. Please try again.", 3, 1); // ❌ AJAX fail
            });
        },
        function () {
            appalert("Save cancelled by user.", 2, 1); // ⚠️ user cancelled
        }
    ).set('labels', { ok: 'Yes, Save', cancel: 'Cancel' })
        .set('closable', false);
}

function deleteRule(id) {
    alertify.confirm(
        'Confirm Deletion',
        'Are you sure you want to delete this rule?',
        function () {
            $.post('/AccountOpeningRulePerBranch/Delete', { id: id }, function (response) {
                if (response.success) {
                    $('#myDataTable').DataTable().ajax.reload();
                    appalert(response.message, 1, 1); // ✅ success
                } else {
                    appalert(response.message, 3, 1); // ❌ failure
                }
            }).fail(function () {
                appalert("An error occurred while processing the request.", 3, 1);
            });
        },
        function () {
            appalert("Deletion cancelled by user.", 2, 1); // ⚠️ cancel
        }
    ).set('labels', { ok: 'Yes, Delete', cancel: 'Cancel' })
        .set('closable', false);
}


function clearForm() {
    $('#ruleId').val('');
    $('#branchId').val('');
    $('#legalForm').val('');
    $('#savingProductId').val('');
    $('#minBalance').val('');
    $('#accountRuleForm .form-control, .form-select').removeClass('is-invalid');
    $('#accountRuleForm .invalid-feedback').hide();
}