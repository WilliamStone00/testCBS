$(document).ready(function () {
    // Initialize form
    updateCommissionTotal();

    // Set initial commission type visibility
    const isProportional = $('input[name="CommissionSetting.IsProportional"]:checked');
    if (isProportional.length) {
        toggleCommissionType(isProportional.val() === 'True');
    }
});
// Event handler for branch selection change
$(document).on('change', '#CommissionSetting_BranchId', function () {
    var branchId = $(this).val();
    loadAgentsByBranch(branchId);
});

// Initialize on page load if branch is already selected
$(document).ready(function () {
    var initialBranchId = $('#CommissionSetting_BranchId').val();
    if (initialBranchId) {
        loadAgentsByBranch(initialBranchId);
    }
});
function loadAgentsByBranch(branchId) {
    console.log('Loading agents for branch:', branchId);

    if (!branchId) {
        // Clear agents dropdown if no branch selected
        $('#CommissionSetting_AgentId').empty().append(
            $('<option>').text('---Select Agent---').val('')
        );
        return;
    }

    $.ajax({
        url: '/DailyCollectionSetting/GetAgentsByBranchId', // Update with your actual endpoint
        type: 'GET',
        dataType: 'json',
        data: { branchId: branchId },
        success: function (data) {
            // Get reference to the agent dropdown
            var $agentDropdown = $('#CommissionSetting_AgentId');

            // Clear existing options and add default
            $agentDropdown.empty().append(
                $('<option>').text('---Select Agent---').val('')
            );

            // Add new options from the response
            $.each(data, function (index, item) {
                $agentDropdown.append(
                    $('<option>').text(item.Text).val(item.Value)
                );
            });

            // Reinitialize Select2 if you're using it
            if ($agentDropdown.hasClass('select2')) {
                $agentDropdown.trigger('change.select2');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading agents:', error);
            // Optionally show user feedback
            toastr.error('Failed to load agents for selected branch');
        }
    });
}


function selectCommissionType(isProportional) {
    document.getElementById('proportionalTrue').checked = isProportional;
    document.getElementById('proportionalFalse').checked = !isProportional;
    toggleCommissionType(isProportional);
    updateCommissionTypeCards();
}

function toggleCommissionType(isProportional) {
    const proportionalSection = document.getElementById('proportionalSection');
    const fixedAmountSection = document.getElementById('fixedAmountSection');

    if (isProportional) {
        proportionalSection.classList.remove('hidden-section');
        fixedAmountSection.classList.add('hidden-section');
    } else {
        proportionalSection.classList.add('hidden-section');
        fixedAmountSection.classList.remove('hidden-section');
    }
    updateCommissionTypeCards();
}

function updateCommissionTypeCards() {
    const cards = document.querySelectorAll('.commission-type-card');
    const isProportional = document.getElementById('proportionalTrue').checked;

    cards.forEach((card, index) => {
        if ((index === 0 && isProportional) || (index === 1 && !isProportional)) {
            card.classList.add('active');
        } else {
            card.classList.remove('active');
        }
    });
}

function selectScope(scopeType) {
    // Uncheck all checkboxes
    document.getElementById('isGlobalChk').checked = false;
    document.getElementById('isBranchChk').checked = false;
    document.getElementById('isAgentChk').checked = false;

    // Hide all scope-specific sections by default
    document.getElementById('branchSelection').classList.add('hidden-section');
    document.getElementById('agentSelection').classList.add('hidden-section');

    // Apply logic based on selected scope
    switch (scopeType) {
        case 'global':
            document.getElementById('isGlobalChk').checked = true;
            break;

        case 'branch':
            document.getElementById('isBranchChk').checked = true;
            document.getElementById('branchSelection').classList.remove('hidden-section');
            break;

        case 'agent':
            document.getElementById('isAgentChk').checked = true;
            document.getElementById('isBranchChk').checked = true; // Agent implies branch-level scope
            document.getElementById('branchSelection').classList.remove('hidden-section');
            document.getElementById('agentSelection').classList.remove('hidden-section');
            break;
    }

    updateScopeCards();
}

function updateScopeCards() {
    const isGlobal = document.getElementById('isGlobalChk').checked;
    const isBranch = document.getElementById('isBranchChk').checked;
    const isAgent = document.getElementById('isAgentChk').checked;

    document.querySelectorAll('.scope-selector').forEach((card, index) => {
        card.classList.remove('active');

        if ((index === 0 && isGlobal) ||
            (index === 1 && isBranch && !isAgent) || // Only branch selected
            (index === 2 && isAgent)) {             // Agent implies both agent & branch
            card.classList.add('active');
        }
    });
}


function updateCommissionTotal() {
    const provider = parseFloat(document.querySelector('input[name="CommissionSetting.CommissionProviderShare"]').value) || 0;
    const agent = parseFloat(document.querySelector('input[name="CommissionSetting.AgentShare"]').value) || 0;
    const total = provider + agent;

    document.getElementById('providerDisplay').textContent = provider + '%';
    document.getElementById('agentDisplay').textContent = agent + '%';
    document.getElementById('totalDisplay').textContent = total + '%';

    const warning = document.getElementById('totalWarning');
    const warningText = document.getElementById('warningText');

    if (total !== 100 && total > 0) {
        warning.style.display = 'block';
        if (total > 100) {
            warningText.textContent = 'Total exceeds 100%';
            warning.className = 'validation-warning text-danger';
        } else {
            warningText.textContent = 'Total should equal 100%';
            warning.className = 'validation-warning text-warning';
        }
    } else {
        warning.style.display = 'none';
    }
}

function updateOperationDisplay() {
    const operationType = document.querySelector('select[name="CommissionSetting.OperationType"]').value;
    // You can add operation-specific logic here if needed
}

function resetForm() {
    if (confirm('Are you sure you want to reset the form? All unsaved changes will be lost.')) {
        document.getElementById('commissionForm').reset();
        document.querySelectorAll('.scope-selector').forEach(card => card.classList.remove('active'));
        document.querySelectorAll('.commission-type-card').forEach(card => card.classList.remove('active'));
        document.querySelectorAll('.hidden-section').forEach(section => section.classList.add('hidden-section'));
        updateCommissionTotal();
    }
}

// Show the beautiful reset confirmation modal
function showResetConfirmation() {
    var modal = new bootstrap.Modal(document.getElementById('resetConfirmationModal'), {
        keyboard: false
    });
    modal.show();
}


function AjaxPostAndUpdate(form) {
    // Show loading indicator
    const submitButton = form.querySelector('button[type="submit"]');
    const originalButtonText = submitButton.innerHTML;
    submitButton.innerHTML = '<i class="mdi mdi-loading mdi-spin me-2"></i> Processing...';
    submitButton.disabled = true;

    // Validate required fields first
    const operationType = form.querySelector('select[name="CommissionSetting.OperationType"]').value;
    const providerType = form.querySelector('select[name="CommissionSetting.ProviderType"]').value;

    if (!operationType) {
        appalert('Please select an Operation Type', 2, 1);
        resetSubmitButton();
        highlightInvalidField(form.querySelector('select[name="CommissionSetting.OperationType"]'));
        return false;
    }

    if (!providerType) {
        appalert('Please select a Provider Type', 2, 1);
        resetSubmitButton();
        highlightInvalidField(form.querySelector('select[name="CommissionSetting.ProviderType"]'));
        return false;
    }

    // Validate scope selection
    const isGlobal = document.getElementById('isGlobalChk').checked;
    const isBranch = document.getElementById('isBranchChk').checked;
    const isAgent = document.getElementById('isAgentChk').checked;

    if (!isGlobal && !isBranch && !isAgent) {
        appalert('Please select a configuration scope (Global, Branch, or Agent)', 2, 1);
        resetSubmitButton();
        highlightInvalidField(document.querySelector('.scope-selector'));
        return false;
    }

    // Validate branch/agent selection if scope is selected
    if (isBranch) {
        const branchSelect = form.querySelector('select[name="CommissionSetting.BranchId"]');
        if (!branchSelect.value) {
            appalert('Please select a branch for branch-level configuration', 2, 1);
            resetSubmitButton();
            highlightInvalidField(branchSelect);
            return false;
        }
    }

    if (isAgent) {
        const agentSelect = form.querySelector('select[name="CommissionSetting.AgentId"]');
        if (!agentSelect.value) {
            appalert('Please select an agent for agent-level configuration', 2, 1);
            resetSubmitButton();
            highlightInvalidField(agentSelect);
            return false;
        }
    }

    // Validate commission percentages for proportional type
    if (document.getElementById('proportionalTrue').checked) {
        const providerInput = form.querySelector('input[name="CommissionSetting.CommissionProviderShare"]');
        const agentInput = form.querySelector('input[name="CommissionSetting.AgentShare"]');
        const provider = parseFloat(providerInput.value) || 0;
        const agent = parseFloat(agentInput.value) || 0;
        const total = provider + agent;

        if (total !== 100) {
            Swal.fire({
                title: 'Commission Percentage Warning',
                html: `<p>Commission total is <strong>${total}%</strong> (recommended total is 100%).</p>
                      <p>Are you sure you want to continue?</p>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, Continue',
                cancelButtonText: 'No, Go Back',
                customClass: {
                    confirmButton: 'btn btn-warning',
                    cancelButton: 'btn btn-secondary'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    showPreviewBeforeSubmit();
                } else {
                    resetSubmitButton();
                    highlightInvalidField(providerInput);
                    highlightInvalidField(agentInput);
                }
            });

            return false;
        }
    }

    // If all validations pass, show preview
    showPreviewBeforeSubmit();
    return false;

    function showPreviewBeforeSubmit() {
        // Store the form submission function
        window.pendingFormSubmission = proceedWithSubmission;

        // Show the preview modal
        previewConfiguration();

        // Reset the button state since we're showing preview first
        resetSubmitButton();
    }

    function proceedWithSubmission() {
        // Re-enable loading state
        submitButton.innerHTML = '<i class="mdi mdi-loading mdi-spin me-2"></i> Processing...';
        submitButton.disabled = true;

        // Prepare form data
        const formData = new FormData(form);

        // AJAX submission
        $.ajax({
            url: form.action,
            type: form.method,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message, 'Success', {
                        timeOut: 3000,
                        progressBar: true,
                        showMethod: 'slideDown',
                        hideMethod: 'fadeOut'
                    });

                    if ($('#Action').val() === 'insert') {
                        setTimeout(() => {
                            resetForm();
                        }, 1500);
                    }
                } else {
                    toastr.error(response.message || 'An error occurred', 'Error', {
                        timeOut: 5000
                    });
                }
            },
            error: function (xhr, status, error) {
                let errorMessage = 'An error occurred while processing your request.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                toastr.error(errorMessage, 'Error', {
                    timeOut: 5000
                });
                console.error('AJAX Error:', status, error);
            },
            complete: function () {
                resetSubmitButton();
            }
        });
    }

    function resetSubmitButton() {
        submitButton.innerHTML = originalButtonText;
        submitButton.disabled = false;
    }

    function highlightInvalidField(field) {
        if (field) {
            field.classList.add('is-invalid');
            field.scrollIntoView({ behavior: 'smooth', block: 'center' });
            setTimeout(() => {
                field.classList.remove('is-invalid');
            }, 3000);
        }
    }
}


// Form validation before submission
 
 
function buildPayloadFromForm(formData) {
    const payload = {
        ServiceOption: $('#ServiceOption').val(),
        Action: $('#Action').val(),
        KEY: "KEY", // Static value

        CommissionSetting: {
            Id: $('#CommissionSetting_Id').val() || null,
            OperationType: $('#CommissionSetting_OperationType').val(),
            ProviderType: $('#CommissionSetting_ProviderType').val(),
            IsProportional: $('#proportionalTrue').is(':checked'),
            IsInterbranch: $('#interbranchChk').is(':checked'),
            CommissionProviderShare: parseFloat($('#CommissionSetting_CommissionProviderShare').val() || 0),
            AgentShare: parseFloat($('#CommissionSetting_AgentShare').val() || 0),
            AmountPerTransaction: parseFloat($('#CommissionSetting_AmountPerTransaction').val() || 0),
            IsGlobal: $('#isGlobalChk').is(':checked'),
            IsBranch: $('#isBranchChk').is(':checked'),
            IsAgent: $('#isAgentChk').is(':checked'),
            BranchId: $('#CommissionSetting_BranchId').val() || null,
            AgentId: $('#CommissionSetting_AgentId').val() || null
        },

        Zones: [], // Populate dynamically if needed
        CommissionSettings: [],

        AgentDailyCashLimit: {
            CashIn: $('#AgentDailyCashLimit_CashIn').val() || "0",
            CashOut: $('#AgentDailyCashLimit_CashOut').val() || "0",
            AgentId: $('#AgentDailyCashLimit_AgentId').val() || null,
            BranchId: $('#AgentDailyCashLimit_BranchId').val() || null
        },

        AgentDailyCashLimits: [] // Optional bulk input
    };

    console.log(payload);
    return payload;
}

function previewConfiguration() {
    const form = document.getElementById('commissionForm');
    const formData = new FormData(form);
    const model = buildPayloadFromForm(formData);
    const payload = {
        OperationType: formData.get('CommissionSetting.OperationType'),
        ProviderType: formData.get('CommissionSetting.ProviderType'),
        CommissionType: document.getElementById('proportionalTrue').checked ? 'Proportional' : 'Fixed',
        InterBranch: document.getElementById('interbranchChk').checked,
        Scope: '',
        BranchName: '',
        AgentName: '',
        CommissionProviderShare: 0,
        AgentShare: 0,
        AmountPerTransaction: 0
    };

    // Scope logic
    if (document.getElementById('isGlobalChk').checked) {
        payload.Scope = 'Global';
    } else if (document.getElementById('isBranchChk').checked) {
        payload.Scope = 'Branch';
        const branchSelect = document.querySelector('select[name="CommissionSetting.BranchId"]');
        payload.BranchName = branchSelect?.options[branchSelect.selectedIndex]?.text || '';
    } else if (document.getElementById('isAgentChk').checked) {
        payload.Scope = 'Agent';
        const agentSelect = document.querySelector('select[name="CommissionSetting.AgentId"]');
        payload.AgentName = agentSelect?.options[agentSelect.selectedIndex]?.text || '';
    }

    // Commission type details
    if (document.getElementById('proportionalTrue').checked) {
        payload.CommissionProviderShare = parseFloat(formData.get('CommissionSetting.CommissionProviderShare') || 0);
        payload.AgentShare = parseFloat(formData.get('CommissionSetting.AgentShare') || 0);
    } else {
        payload.AmountPerTransaction = parseFloat(formData.get('CommissionSetting.AmountPerTransaction') || 0);
    }

    // Create preview UI
    let previewHTML = `
        <div class="preview-container compact-view">
            <h5 class="text-primary"><i class="mdi mdi-file-document-outline"></i> Commission Configuration Preview</h5>
            <table class="table table-sm table-borderless mt-2">
                <tbody>
                    <tr><td class="text-muted">Operation:</td><td><strong>${payload.OperationType || '—'}</strong></td>
                        <td class="text-muted">Provider:</td><td><strong>${payload.ProviderType || '—'}</strong></td></tr>
                    <tr><td class="text-muted">Type:</td><td><strong>${payload.CommissionType}</strong></td>
                        <td class="text-muted">Inter-branch:</td><td><strong>${payload.InterBranch ? 'Yes' : 'No'}</strong></td></tr>
    `;

    if (payload.CommissionType === 'Proportional') {
        previewHTML += `
            <tr><td class="text-muted">Provider %:</td><td><strong>${payload.CommissionProviderShare}%</strong></td>
                <td class="text-muted">Agent %:</td><td><strong>${payload.AgentShare}%</strong></td></tr>
        `;
    } else {
        previewHTML += `
            <tr><td class="text-muted">Amount:</td><td colspan="3"><strong>${payload.AmountPerTransaction}</strong></td></tr>
        `;
    }

    previewHTML += `
        <tr><td class="text-muted">Scope:</td><td colspan="3"><strong>${payload.Scope === 'Branch' ? payload.BranchName :
            payload.Scope === 'Agent' ? payload.AgentName : 'Global'}</strong></td></tr>
        </tbody>
    </table>

    <div class="d-flex justify-content-end mt-2 pt-2 border-top">
        <button type="button" class="btn btn-outline-secondary btn-sm me-2" data-bs-dismiss="modal">
            <i class="mdi mdi-close me-1"></i> Cancel
        </button>
        <button type="button" class="btn btn-primary btn-sm" id="acceptAndSubmit">
            <i class="mdi mdi-check-circle-outline me-1"></i> Confirm & Submit
        </button>
    </div>
    </div>`;

    document.getElementById('previewContent').innerHTML = previewHTML;

    // Show modal
    const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));
    previewModal.show();

    // Submit logic
    document.getElementById('acceptAndSubmit').addEventListener('click', function () {
        const btn = this;
        btn.innerHTML = '<i class="mdi mdi-loading mdi-spin me-1"></i> Submitting...';
        btn.disabled = true;

        fetch('/DailyCollectionSetting/AddOrUpdate', { // 🔁 Replace with your actual endpoint
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(model)
        })
            .then(response => {
                if (!response.ok) throw new Error('Submission failed');
                return response.json();
            })
            .then(data => {
                previewModal.hide();
 
                if (data.success === true) {
                    appalert(data.message, 1, 1);
                } else {
                    appalert(data.message || 'Submission failed.', 2, 1);
                }
            })
            .catch(error => {
                console.error(error);
                appalert('Something went wrong. Try again.', 2, 1);
            })
            .finally(() => {
                btn.innerHTML = '<i class="mdi mdi-check-circle-outline me-1"></i> Confirm & Submit';
                btn.disabled = false;
            });
    });
}