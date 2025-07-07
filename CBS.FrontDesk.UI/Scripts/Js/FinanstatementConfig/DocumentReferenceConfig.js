$(document).ready(function () {
    showInsertMode();
    $('#updateReference').hide();
    $(document).on('change', '#FinancialDocumentReference_DocumentId', function () {
        var selectedValue = $(this).val();
        var IdModel = "";
        console.log(selectedValue);
        $.ajax({
            url: '/AccountingConfiguration/GetDocumentType',
            type: 'GET',
            dataType: 'json',
            data: { DocumentId: selectedValue },
            success: function (data) {
                // Clear existing options in the OperationEventAttributeId combo
                $('#FinancialDocumentReference_DocumentTypeId').empty();
                // Add new options based on the fetched data
                $.each(data, function (index, item) {
                    $('#FinancialDocumentReference_DocumentTypeId').append($('<option>').text(item.Value).attr('value', item.Text));
                });

            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        });
    });
    /*    LoadDataForEventRule();*/
});

/**
 * TAB NAVIGATION
 * Switches between "Add New Document" and "View Documents" tabs
 * @param {string} tabName - Name of tab to show ('add' or 'view')
 * @param {Event} event - Click event from the tab button
 */
function showTab(tabName, event) {
    // Hide all tab content
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));

    // Remove active state from all tab buttons
    document.querySelectorAll('.nav-tab').forEach(tab => tab.classList.remove('active'));

    // Show the selected tab content
    document.getElementById(tabName + '-tab').classList.add('active');

    // Mark the clicked button as active 
    event.target.classList.add('active');
    if (tabName == "view") {
        loadDocuments();
    } else {
      
        resetForm();
    }
}
/**
* GLOBAL DATA STORAGE
* Array to hold all financial documents in memory
*/
let documents = [];

/**
 * FORM SUBMISSION HANDLER
 * Processes form submission via AJAX
 * @param {HTMLElement} form - The form element being submitted
*/
function AjaxPostAndUpdateForFS(form) {
    console.log("Form Action:", form.action);
    console.log("Form Method:", form.method);

    var formData = new FormData(form);
    for (var pair of formData.entries()) {
        console.log(pair[0] + ', ' + pair[1]);
    }
    var payLoad = collecteFormData(form);
    $.validator.unobtrusive.parse(form);
    console.log(payLoad);
    if ($(form).valid()) {
        alertify.confirm("Confirmation", "Are you sure you want to perform this action? ",
            function () {
                var ajaxConfig = {
                    type: 'POST',
                    url: form.action,
                    data: formData,
                    success: function (response) {
                        console.log("Response:", response);
                        if (response.success) {
                            if (response.status === "Exist") {
                                appalert(response.message, 3, 1);
                            }
                            else if (response.status === "Failed") {
                                appalert(response.message, 2, 1);
                            }
                            else {
                                appalert(response.message, 1, 1);
                            }

                            if (response.option === 'Update' && response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                            else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                                EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                            }
                            else if (response.reloadDataView === "Yes") {
                                LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                            }
                        } else {
                            if (response.Status === "Exist") {
                                appalert(response.message, 3, 1);
                            } else {
                                appalert(response.message, 2, 1);
                            }
                        }
                    },
                    error: function (err) {
                        console.log("Error:", err);

                        if (err.status === 401) { // Unauthorized
                            // Session has expired, redirect to the login page
                            window.location.href = '/Authentication/Login'; // Adjust the URL as needed
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
    }
    return false;
}
function collecteFormData(form) {

    // Get form data
    const formData = new FormData(form);
    const action = formData.get('Action');
    const serviceOption = formData.get('ServiceOption');
    const isEdit = action === 'update';

    const newDocument = {
        Id: isEdit ? formData.get('FinancialDocumentReference.Id') : generateId(),
        Reference: formData.get('FinancialDocumentReference.Reference'),
        DescriptionEn: formData.get('FinancialDocumentReference.DescriptionEn'),
        DescriptionFr: formData.get('FinancialDocumentReference.DescriptionFr') || null,
        DescriptionSp: formData.get('FinancialDocumentReference.DescriptionSp') || null,
        DocumentId: formData.get('FinancialDocumentReference.DocumentId'),
        DocumentTypeId: formData.get('FinancialDocumentReference.DocumentTypeId'),
        CorrespondingAccount: {
            ContainsCondition: formData.get('FinancialDocumentReference.CorrespondingAccount.ContainsCondition') === 'true',
            ContainsException: formData.get('FinancialDocumentReference.CorrespondingAccount.ContainsException') === 'true',
            GrossAccounts: collectAccounts('grossAccounts'),
            ProvisionAccounts: collectAccounts('provisionAccounts'),
            ContainsConditionAccounts: collectAccounts('ConditionAccounts'),
            GrossExceptionAccounts: collectAccounts('grossExceptionAccounts'),
            ProvisionExceptionAccounts: collectAccounts('provisionExceptionAccounts')

        }

    };
    const Datas = {

        "FinancialDocumentReference": newDocument,

        "ServiceOption": serviceOption,
        "Action": action
    }
    console.log(Datas);

    return Datas;


}


/**
 * MESSAGE DISPLAY SYSTEM
 * Shows success or error messages to the user with auto-removal
 * @param {string} message - Text to display to user
* @param {string} type - 'success' or 'error' for styling
*/
function showMessage(message, type) {
    // Remove any existing messages first
    document.querySelectorAll('.alert').forEach(msg => msg.remove());

    // Create new message element
    const messageDiv = document.createElement('div');
    messageDiv.className = `alert alert-${type === 'success' ? 'success' : 'danger'}`;
    messageDiv.textContent = message;

    // Insert message above the form
    const cardBody = document.querySelector('.card-body');
    cardBody.insertBefore(messageDiv, cardBody.firstChild);

    // Auto-remove message after 5 seconds
    setTimeout(() => messageDiv.remove(), 5000);
}

/**
 * UNIQUE ID GENERATOR
 * Creates unique document IDs using timestamp and random string  FinancialDocumentReference_DocumentId
 * @returns {string} Unique document ID
*/
function generateId() {
    return 'DOC' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}






/**
 * CONDITIONAL SECTION VISIBILITY
 * Shows/hides account sections based on checkbox states
 * @param {string} sectionType - 'condition' or 'exception'
*/
 

function toggleAccountSection(type) {
    console.log("🔀 toggleAccountSection called with type: "+type);

    if (type === 'condition') {
        const conditionCheckbox = $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]');

        if (conditionCheckbox.length) {
            const isChecked = conditionCheckbox.is(':checked');
            console.log(`📌 'ContainsCondition' checkbox is ${isChecked ? 'checked' : 'unchecked'}`);
            $('#conditionSection').toggle(isChecked);
            console.log(`📂 '#conditionSection' is now ${isChecked ? 'visible' : 'hidden'}`);
        } else {
            console.warn("⚠️ 'ContainsCondition' checkbox not found in the DOM.");
        }
    }

    if (type === 'exception') {
        const exceptionCheckbox = $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]');

        if (exceptionCheckbox.length) {
            const isChecked = exceptionCheckbox.is(':checked');
            console.log(`📌 'ContainsException' checkbox is ${isChecked ? 'checked' : 'unchecked'}`);
            $('#exceptionSection').toggle(isChecked);
            console.log(`📂 '#exceptionSection' is now ${isChecked ? 'visible' : 'hidden'}`);
        } else {
            console.warn("⚠️ 'ContainsException' checkbox not found in the DOM.");
        }
    }

    if (type !== 'condition' && type !== 'exception') {
        console.warn(`⚠️ Unknown toggle type '${type}' passed to toggleAccountSection.`);
    }
}


/**
 * ACCOUNT COLLECTION UTILITY
 * Extracts account data from a specific container
 * @param {string} containerId - ID of the container holding account inputs
* @returns {Array} Array of account objects
*/
function collectAccounts(containerId) {
    const container = document.getElementById(containerId);
    const accounts = [];

    if (!container) return accounts;

    container.querySelectorAll('.account-item').forEach(item => {
        const accountNumber = item.querySelector('input[name$="AccountNumber"]')?.value;
        const documentBooking = item.querySelector('select[name$="DocumentBooking"]')?.value;

        if (accountNumber && documentBooking) {
            accounts.push({
                AccountNumber: accountNumber,
                DocumentBooking: documentBooking
            });
        }
    });
    console.log("Logging Collection:" + containerId + " Values" + accounts);
    return accounts;
}

/**
 * DYNAMIC ACCOUNT INPUT CREATION containsconditionAccounts
 * Adds new account input fields to specified account type container
 * @param {string} type - Account type ('gross', 'provision', 'condition', etc.)
*/
function addAccount(type) {
    const containerMap = {
        gross: 'grossAccounts',
        provision: 'provisionAccounts',
        condition: 'containsconditionAccounts',
        grossException: 'grossExceptionAccounts',
        provisionException: 'provisionExceptionAccounts'
    };

    const containerId = containerMap[type];
    const container = document.getElementById(containerId);
    const index = container.querySelectorAll('.account-item').length;

    const accountItem = document.createElement('div');
  

    // Create the HTML for the new account item
    accountItem.innerHTML = `
    <div class="account-item gy-1">
        <!-- Account Number Field - 40% width -->
       <div class="col-md-5 col-12 ">
            <div class="form-floating form-floating-outline">
                <input type="text" 
                    name="FinancialDocumentReference.CorrespondingAccount.${type.charAt(0).toUpperCase() + type.slice(1)}Accounts[${index}].AccountNumber"
                    class="form-control"  
                    placeholder="Account Number" 
                    required />
                <label>Account Number</label>
            </div>
        </div>
        
        <!-- Document Booking Field - 40% width -->
        <div class="col-md-5 col-12">
            <div class="form-floating form-floating-outline">
                <select name="FinancialDocumentReference.CorrespondingAccount.${type.charAt(0).toUpperCase() + type.slice(1)}Accounts[${index}].DocumentBooking"
                    class="form-control select2" search=true
                    required>
                    <option value="">--- Select Booking Direction ---</option>
                    <option value="DEBIT">DEBIT</option>
                    <option value="CREDIT">CREDIT</option>
                    <option value="NEGATE">NEGATE</option>
                    <option value="NONE">NONE</option>
                </select>
                <label>Document Booking</label>
            </div>
        </div>
        
        <!-- Remove Button - 20% width -->
       <div class="col-md-1 col-12 d-flex align-items-end">
            <button type="button" 
                class="btn btn-danger btn-sm w-20" 
                onclick="removeAccount(this)"
                title="Remove Account">
                <i class="fas fa-times"></i>
            </button>
        </div>
    </div>
`;

    container.appendChild(accountItem);
}



/**
 * ACCOUNT REMOVAL
 * Removes an account item from the form
 * @param {HTMLElement} button - The remove button that was clicked
*/
function removeAccount(button) {
    button.closest('.account-item').remove();
    // Reindex remaining accounts if needed
}

/**
 * FORM RESET UTILITY
 * Clears all form inputs and resets to default state
 */
function resetForm() {
    $('select[name="FinancialDocumentReference.DocumentId"]').val('').trigger('change');
    $('select[name="FinancialDocumentReference.DocumentTypeId"]').val('').trigger('change').scrollTop('');

    // Reset text input fields
    $('input[name="FinancialDocumentReference.Reference"]').val('');
    $('input[name="FinancialDocumentReference.DescriptionEn"]').val('');
    $('input[name="FinancialDocumentReference.DescriptionFr"]').val('');
    $('input[name="FinancialDocumentReference.DescriptionSp"]').val('');
    document.getElementById('conditionSection').style.display = 'none';
    document.getElementById('exceptionSection').style.display = 'none';
    showInsertMode();
    // Clear all account containers
    ['grossAccounts', 'provisionAccounts', 'conditionAccounts',
        'grossExceptionAccounts', 'provisionExceptionAccounts'].forEach(id => {
            const container = document.getElementById(id);
            if (container) container.innerHTML = '';
        });
}
function showInsertMode() {
    $('#addReference').show();
    $('#updateReference').hide();
    console.log("🟢 Insert mode activated.");
}

function showUpdateMode() {
    $('#addReference').hide();
    $('#updateReference').show();
    console.log("🟡 Update mode activated.");
}









/**
 * DELETE DOCUMENT
 * Removes a document from the collection
 * @param {string} id - The ID of the document to delete
*/
function deleteDocument(id) {
    if (!confirm('Are you sure you want to delete this document?')) return;

    documents = documents.filter(doc => doc.Id !== id);
    showMessage('Document deleted successfully', 'success');

}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Set up event listeners for checkboxes
    const conditionCheckbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]');
    const exceptionCheckbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]');

    if (conditionCheckbox) {
        conditionCheckbox.addEventListener('change', () => toggleAccountSection('condition'));
    }

    if (exceptionCheckbox) {
        exceptionCheckbox.addEventListener('change', () => toggleAccountSection('exception'));
    }


});

let deleteParams = {};

function DeleteDataConfiguration(module, id, tableId, reloadPartial, typeId, viewId, domId) {
    deleteParams = { module, id, tableId, reloadPartial, typeId, viewId, domId };

    console.log(deleteParams);

    $('#deleteModalBody').text(`Are you sure you want to delete the document with ID: ${id}?`);
    $('#deleteConfirmModal').modal('show');
}

$('#confirmDeleteBtn').click(function () {
    const { module, id, tableId } = deleteParams;

    $.ajax({
        url: `/AccountingConfiguration/DeleteDocumentReference/${id}`,
        type: 'POST',
        success: function () {
            $('#deleteConfirmModal').modal('hide');
            $(`#${tableId}`).DataTable().row($(`#${tableId} tbody tr:has(td:contains('${id}'))`)).remove().draw();
        },
        error: function () {
            console.error('Failed to delete document reference.');
        }
    });
});


function loadDocuments() {
    const $table = $('#myDataTable');
    const $tableBody = $table.find('tbody');
    $tableBody.empty(); // Clear previous rows

    $.ajax({
        url: '/AccountingConfiguration/GetAllDocumentReference', // ✅ Use your actual endpoint
        type: 'GET',
        dataType: 'json',
        success: function (documents) {
            $.each(documents, function (i, doc) {
                const category = doc.CorrespondingAccount?.Category || '';
                const badgeClass = getCategoryBadgeClass(category);
                const grossCount = doc.CorrespondingAccount?.GrossAccounts?.length || 0;
                const provisionCount = doc.CorrespondingAccount?.ProvisionAccounts?.length || 0;
                const grossExceptionCount = doc.CorrespondingAccount?.GrossExceptionAccounts?.length || 0;
                const provisionExceptionCount = doc.CorrespondingAccount?.ProvisionExceptionAccounts?.length || 0;

                const exceptionBadge = doc.CorrespondingAccount?.ContainsException
                    ? '<span class="badge badge-warning">Has Exceptions</span>'
                    : '';
                const conditionBadge = doc.CorrespondingAccount?.ContainsCondition
                    ? '<span class="badge badge-info">Has Conditions</span>'
                    : '';

                const row = `
                    <tr>
                        <td style="width:10%">
                            <strong>${doc.Reference}</strong><br>
                        </td>
                        <td style="width:30%">
                            <strong>EN:</strong> ${doc.DescriptionEn || ''}<br>
                            <strong>FR:</strong> ${doc.DescriptionFr || ''}<br>
                            <strong>SP:</strong> ${doc.DescriptionSp || ''}
                        </td>
                        <td style="width:15%">
                            <strong>${doc.DocumentName}</strong><br>
                            <small class="text-muted">Section: ${doc.DocumentSectionName}</small>
                        </td>
                        <td style="width:30%">
                            <div class="small">
                                <div><strong>Gross:</strong> ${grossCount} accounts</div>
                                <div><strong>Provision:</strong> ${provisionCount} accounts</div>
                                <div><strong>Gross Exception:</strong> ${grossExceptionCount} accounts</div>
                                <div><strong>Provision Exception:</strong> ${provisionExceptionCount} accounts</div>
                                  <div>${exceptionBadge} ${conditionBadge}</div>
                            </div>
                        </td>
                        <td style="width:10%">
                            <div class="btn-group" role="group">
                       
                                <button type="button" class="btn btn-sm btn-outline-success"
                                        onclick="editDocument('${doc.Id}')"
                                        title="Edit ${doc.Reference}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-outline-danger"
                                        onclick="deleteDocument('${doc.Id}')"
                                        title="Delete ${doc.Reference}">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `;

                $tableBody.append(row);
            });
            //type = "button"
            //class="btn btn-primary"
            //data - bs - toggle="modal"
            //data - bs - target="#largeModal"
            // Refresh or initialize DataTable showTab('add', event)
            if ($.fn.DataTable.isDataTable('#myDataTable')) {
                $table.DataTable().destroy();
            }

            $table.DataTable({
                pageLength: 10,
                responsive: true,
                ordering: true,
                searching: true,
                lengthChange: true,
                autoWidth: false
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading document references:', error);

        }
    });
}

function getCategoryBadgeClass(category) {
    if (!category) return 'secondary';
    switch (category.toLowerCase()) {
        case 'income': return 'success';
        case 'assets': return 'primary';
        case 'liability': return 'warning';
        case 'expense': return 'danger';
        default: return 'secondary';
    }
}
 
 
function setDropdownValuesAfterLoad(doc) {
 
    $('select[name="FinancialDocumentReference.DocumentId"]').val(doc.DocumentId);
    // Set DocumentTypeId without triggering change
    $('select[name="FinancialDocumentReference.DocumentTypeId"]').val(doc.DocumentTypeId);
}
 
// Only trigger cascade if not in update mode
function handleDocumentChange(isUpdateMode,selectedValue) {
    if (!isUpdateMode) {
        // Only load B options when inserting new data
        loadDropdownDocumentTypeOptions(selectedValue);
    }
    // Always update the form value
    setDropdownDocumentValue(selectedValue);
}
// Set dropdown A value without triggering change events
function setDropdownDocumentValue(value, silent = false) {
    const dropdownA = document.getElementById('FinancialDocumentReference_DocumentId');
    dropdownA.value = value;

    // If you need to trigger change events for other listeners
    if (!silent) {
        dropdownA.dispatchEvent(new Event('change'));
    }
}


function loadDropdownDocumentTypeOptions(selectedAValue) {
    const dropdownB = document.getElementById('FinancialDocumentReference_DocumentTypeId');

    // Clear existing options
    dropdownB.innerHTML = '<option value="">Select...</option>';

    // Show loading state
    dropdownB.disabled = true;

    // Fetch data (replace with your actual data source)
    fetch('/AccountingConfiguration/GetAccountDetailsByReference?id='+selectedAValue)
        .then(response => response.json())
        .then(data => {
            // Populate dropdown B with new options
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item.id;
                option.textContent = item.name;
                dropdownB.appendChild(option);
            });

            dropdownB.disabled = false;
        })
        .catch(error => {
            console.error('Error loading dropdown B options:', error);
            dropdownB.disabled = false;
        });
}

// Set dropdown B value (usually called after options are loaded)
function setDropdownDocumentTypeValue(value) {
    const dropdownB = document.getElementById('FinancialDocumentReference_DocumentTypeId');
    console.log("FinancialDocumentReference_DocumentTypeId=" + value);
    dropdownB.value = value;
}
function editDocument(id) {
    console.log("▶️ editDocument called with ID:", id);

    // 1. Validate the input ID
    if (!id) {
        console.error('❌ Document ID is undefined or null.');
        return;
    }

    // 2. Indicate loading state
    console.log("⏳ Showing loading state...");
    showLoadingState();
    showTab('add', { target: $('.nav-tab').first()[0] });

    // 3. Construct API URL
    const url = `/AccountingConfiguration/GetAccountDetailsByReference/${id}`;
    console.log("🌐 API URL:", url);

    // 4. Make AJAX GET request
    $.ajax({
        url: url,
        method: 'GET',
        dataType: 'json',
        success: handleDocumentLoadSuccess,
        error: handleDocumentLoadError
    });

    function handleDocumentLoadSuccess(data) {
        console.log('✅ AJAX call successful. Raw response:', data);

        // 5. Extract document from response
        const doc = data?.FinancialDocumentReference || (Array.isArray(data) ? data[0] : data);

        if (!doc) {
            console.warn('⚠️ Document not found or response is invalid.');
            alert('Document not found or response is invalid.');
            hideLoadingState();
            return;
        }

        // 6. Reset form before populating new data
        resetForm();

        // 7. Populate basic form fields
        console.log("📄 Setting basic form fields...");
        $('#FinancialDocumentReference_Id').val(doc.Id);
        $('#FinancialDocumentReference_Reference').val(doc.Reference);
        $('#FinancialDocumentReference_DescriptionEn').val(doc.DescriptionEn);
        $('#FinancialDocumentReference_DescriptionFr').val(doc.DescriptionFr || '');
        $('#FinancialDocumentReference_DescriptionSp').val(doc.DescriptionSp || '');

        // 8. Handle dropdowns with proper initialization
        //$('#FinancialDocumentReference_DocumentId').val(doc.DocumentId).trigger('change');
        //$('select[name="FinancialDocumentReference.DocumentTypeId"]').val(doc.DocumentTypeId);
        handleDocumentChange(true, doc.DocumentId);
        setDropdownDocumentTypeValue(doc.DocumentTypeId);
        // Set DocumentTypeId without triggering change
        showUpdateMode();
        // 9. Handle CorrespondingAccount section
        if (doc.CorrespondingAccount && typeof doc.CorrespondingAccount === 'object') {
            const ca = doc.CorrespondingAccount;
            console.log("📦 Processing CorrespondingAccount...", ca);

            // Set checkboxes and toggle sections
            const hasCondition = !!ca.ContainsCondition;
            const hasException = !!ca.ContainsException;

            $('#FinancialDocumentReference_CorrespondingAccount_ContainsCondition')
                .prop('checked', hasCondition)
                .trigger('change');

            $('#FinancialDocumentReference_CorrespondingAccount_ContainsException')
                .prop('checked', hasException)
                .trigger('change');

            // Load account sections
            loadAccountSection('grossAccounts', ca.GrossAccounts, 'GrossAccounts');
            loadAccountSection('provisionAccounts', ca.ProvisionAccounts, 'ProvisionAccounts');

            if (hasCondition) {
                loadAccountSection('containsconditionAccounts', ca.ContainsConditionAccounts, 'ContainsConditionAccounts');
            }

            if (hasException) {
                loadAccountSection('grossExceptionAccounts', ca.GrossExceptionAccounts, 'GrossExceptionAccounts');
                loadAccountSection('provisionExceptionAccounts', ca.ProvisionExceptionAccounts, 'ProvisionExceptionAccounts');
            }
        }

        // 10. Set form action 
        console.log("🔄 Setting form action to 'update'...");

        // 11. Initialize plugins and finalize
        $('.select2').select2();
        hideLoadingState();

        // 12. Scroll to form
        console.log("📜 Scrolling to form...");
        $('html, body').animate({
            scrollTop: $('.card').offset().top - 20
        }, 500);
    }

    function handleDocumentLoadError(xhr) {
        console.error('❌ AJAX error occurred:', xhr.responseText);
        hideLoadingState();
        alert(`Error loading document: ${xhr.statusText}`);
    }

    function loadAccountSection(containerId, accounts, accountType) {
        if (!Array.isArray(accounts)) {
            console.log(`ℹ️ No accounts to load for '${containerId}'.`);
            return;
        }

        const $container = $(`#${containerId}`).empty();
        console.log(`📥 Loading ${accounts.length} account(s) into '${containerId}'...`);

        accounts.forEach((account, index) => {
            $container.className = 'account-item  border rounded';
            $container.append(`
   <div class="account-item gy-1">
        <!-- Account Number Field - 40% width -->
       <div class="col-md-5 col-12 ">
            <div class="form-floating form-floating-outline">
                <input type="text"
                  name="FinancialDocumentReference.CorrespondingAccount.${accountType}[${index}].AccountNumber"
                    class="form-control"  
                   value="${account.AccountNumber}" placeholder="Account Number" required />

                <label>Account Number</label>

            </div>
        </div>
        <!-- Document Booking Field - 40% width -->
        <div class="col-md-5 col-12">
            <div class="form-floating form-floating-outline">
                <select class="form-control select2 col-md-5 col-12"
                   name="FinancialDocumentReference.CorrespondingAccount.${accountType}[${index}].DocumentBooking"
                   search=true  required>
                    <option value="">--- Select Document Booking ---</option>
                    <option value="DEBIT" ${account.DocumentBooking === 'DEBIT' ? 'selected' : ''}>DEBIT</option>
                    <option value="CREDIT" ${account.DocumentBooking === 'CREDIT' ? 'selected' : ''}>CREDIT</option>
                    <option value="NEGATE" ${account.DocumentBooking === 'NEGATE' ? 'selected' : ''}>NEGATE</option>
                    <option value="NONE" ${account.DocumentBooking === 'NONE' ? 'selected' : ''}>NONE</option>
                </select>
                <label>Document Booking</label>
            </div>
        </div>
        
    <div class="col-md-1 col-12 d-flex align-items-end">
            <button type="button" 
                class="btn btn-danger remove-account w-20"
                title="Remove Account">
               <i class="fas fa-times"></i>
            </button>
        </div>
    </div>
`);
        });

        // Initialize select2 and remove handlers for new elements
        $container.find('.select2').select2();
        $container.find('.remove-account').click(function () {
            $(this).closest('.account-item').remove();
        });
    }
}

// Helper functions (implement as needed)
function showLoadingState() {
    // Show loading spinner or disable form
    const form = document.querySelector('.card');
    if (form) {
        form.style.opacity = '0.5';
        form.style.pointerEvents = 'none';
    }
    showTab('add', event)
}

function hideLoadingState() {
    // Hide loading spinner or re-enable form
    const form = document.querySelector('.card');
    if (form) {
        form.style.opacity = '1';
        form.style.pointerEvents = 'auto';
    }
}

function showErrorMessage(message) {
    // Display error message to user
    alert(message); // Replace with a better UI notification
}
function loadAccounts(containerId, accounts, namePrefix) {
    console.log(`📥 loadAccounts called for container: '${containerId}', namePrefix: '${namePrefix}'`);

    const $container = $('#' + containerId);
    if (!$container.length) {
        console.log(`⚠️ Container with ID '${containerId}' not found in the DOM.`);
        return;
    }

    // Clear existing content
    $container.empty();
    console.log(`🧹 Cleared container '${containerId}'`);

    // Validate account array
    if (!Array.isArray(accounts)) {
        console.log(`⚠️ Provided accounts for '${namePrefix}' is not a valid array.`);
        return;
    }

    console.log(`🔄 Rendering ${accounts.length} account(s) into '${containerId}'`);

    accounts.forEach((account, index) => {
        console.log(`🔧 Rendering account #${index + 1}:`, account);

        const html = `
            <div class="account-item">
                <button type="button" class="remove-account" onclick="removeAccount(this)" title="Remove Account">×</button>
                <div class="row gy-3">
                    <div class="col-md-6">
                        <div class="form-floating form-floating-outline">
                            <input type="text" class="form-control" 
                                name="FinancialDocumentReference.CorrespondingAccount.${namePrefix}[${index}].AccountNumber" 
                                value="${account.AccountNumber || ''}" />
                            <label>Account Number</label>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="form-floating form-floating-outline">
                            <select name="FinancialDocumentReference.CorrespondingAccount.${namePrefix}[${index}].DocumentBooking"
                                    class="form-control">
                                <option value="">--- Select Booking Direction ---</option>
                                <option value="DEBIT" ${account.DocumentBooking === 'DEBIT' ? 'selected' : ''}>DEBIT</option>
                                <option value="CREDIT" ${account.DocumentBooking === 'CREDIT' ? 'selected' : ''}>CREDIT</option>
                                <option value="NONE" ${account.DocumentBooking === 'NONE' ? 'selected' : ''}>NONE</option>
                                <option value="NEGATE" ${account.DocumentBooking === 'NEGATE' ? 'selected' : ''}>NEGATE</option>
                            </select>
                            <label>Document Booking</label>
                        </div>
                    </div>
                </div>
            </div>`;

        $container.append(html);
    });

    console.log(`✅ Successfully rendered ${accounts.length} account(s) in '${containerId}'`);
}

