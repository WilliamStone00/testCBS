$(document).ready(function () {
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
//function toggleAccountSection(sectionType) {
//    if (sectionType === 'condition') {
//        const checkbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]');
//const section = document.getElementById('conditionSection');
//section.style.display = checkbox.checked ? 'block' : 'none';
//    } else if (sectionType === 'exception') {
//        const checkbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]');
//const section = document.getElementById('exceptionSection');
//section.style.display = checkbox.checked ? 'block' : 'none';
//    }
//}

function toggleAccountSection(type) {
    if (type === 'condition') {
        const isChecked = $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]').is(':checked');
        $('#conditionSection').toggle(isChecked);
    }
    if (type === 'exception') {
        const isChecked = $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]').is(':checked');
        $('#exceptionSection').toggle(isChecked);
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
    accountItem.className = 'account-item mb-3 p-3 border rounded';

    // Create the HTML for the new account item
    accountItem.innerHTML = `
      
    <div class="row gy-3 account-item>
        <div class="col-6">
            <div class="form-floating form-floating-outline">
                <input type="text" name="FinancialDocumentReference.CorrespondingAccount.${type.charAt(0).toUpperCase() + type.slice(1)}Accounts[${index}].AccountNumber"
                    class="form-control"  placeholder="Account Number" required />
                <label>Account Number</label>
            </div>
        </div>
        <div class="col-6">
            <div class="form-floating form-floating-outline">
                <select name="FinancialDocumentReference.CorrespondingAccount.${type.charAt(0).toUpperCase() + type.slice(1)}Accounts[${index}].DocumentBooking"
                    class="form-control select2" type="search" required>
                    <option value="">--- Select Booking Direction ---</option>
                    <option value="DEBIT">DEBIT</option>
                    <option value="CREDIT">CREDIT</option>
                    <option value="NEGATE">NEGATE</option>
                      <option value="NONE">NONE</option>
                </select>
                <label>Document Booking</label>
            </div>
        </div>
         <button type="button" class="btn btn-danger btn-sm float-end" onclick="removeAccount(this)">×</button>
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
    document.querySelector('form').reset();
    document.getElementById('conditionSection').style.display = 'none';
    document.getElementById('exceptionSection').style.display = 'none';

    // Clear all account containers
    ['grossAccounts', 'provisionAccounts', 'conditionAccounts',
        'grossExceptionAccounts', 'provisionExceptionAccounts'].forEach(id => {
            const container = document.getElementById(id);
            if (container) container.innerHTML = '';
        });
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
function editDocument(id) {
    if (!id) {
        console.error('Document ID is undefined or null.');
        return;
    }

    showLoadingState();

    const url = "/AccountingConfiguration/GetAccountDetailsByReference/" + id;
    console.log("Calling URL:", url);
    console.log("Document ID:", id);

    $.ajax({
        url: url,
        method: 'GET',
        dataType: 'json',
        contentType: 'application/json',
        success: function (data) {
            console.log('Raw response:', data);

            // ✅ Handle response array
            const doc = Array.isArray(data) && data.length > 0 ? data[0] : null;

            if (!doc) {
                alert('Document not found or response is invalid.');
                return;
            }

            // Set basic form fields
            $('input[name="FinancialDocumentReference.Id"]').val(doc.Id);
            $('input[name="FinancialDocumentReference.Reference"]').val(doc.Reference);
            $('input[name="FinancialDocumentReference.DescriptionEn"]').val(doc.DescriptionEn);
            $('input[name="FinancialDocumentReference.DescriptionFr"]').val(doc.DescriptionFr || '');
            $('input[name="FinancialDocumentReference.DescriptionSp"]').val(doc.DescriptionSp || '');
            $('select[name="FinancialDocumentReference.DocumentId"]').val(doc.DocumentId);
            $('select[name="FinancialDocumentReference.DocumentTypeId"]').val(doc.DocumentTypeId);

            setTimeout(() => {
                $('select[name="FinancialDocumentReference.DocumentId"]').val(doc.DocumentId);
                $('select[name="FinancialDocumentReference.DocumentTypeId"]').val(doc.DocumentTypeId);
            }, 1000);

            // ✅ Safely handle CorrespondingAccount
            if (doc.CorrespondingAccount && typeof doc.CorrespondingAccount === 'object') {
                const ca = doc.CorrespondingAccount;

                const containsCondition = !!ca.ContainsCondition;
                const containsException = !!ca.ContainsException;

                $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]').prop('checked', containsCondition);
                $('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]').prop('checked', containsException);

                if (containsCondition) toggleAccountSection('condition');
                if (containsException) toggleAccountSection('exception');

                if (Array.isArray(ca.GrossAccounts)) {
                    loadAccounts('grossAccounts', ca.GrossAccounts, 'GrossAccounts');
                }

                if (Array.isArray(ca.ProvisionAccounts)) {
                    loadAccounts('provisionAccounts', ca.ProvisionAccounts, 'ProvisionAccounts');
                }

                if (Array.isArray(ca.ContainsConditionAccounts)) {
                    loadAccounts('containsconditionAccounts', ca.ContainsConditionAccounts, 'containsconditionAccounts');
                }

                if (Array.isArray(ca.GrossExceptionAccounts)) {
                    loadAccounts('grossExceptionAccounts', ca.GrossExceptionAccounts, 'GrossExceptionAccounts');
                }

                if (Array.isArray(ca.ProvisionExceptionAccounts)) {
                    loadAccounts('provisionExceptionAccounts', ca.ProvisionExceptionAccounts, 'ProvisionExceptionAccounts');
                }
            } else {
                console.log("⚠️ 'CorrespondingAccount' is missing or invalid in the document.");
            }

            // Set action to update
            $('input[name="Action"]').val('update');

            // Scroll to form
            $('.card')[0].scrollIntoView();
        },
        error: function (xhr) {
            console.error('Error response:', xhr.responseText);
            alert(`HTTP error! status: ${xhr.status}, message: ${xhr.responseText}`);
        }
    });
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
    const $container = $('#' + containerId);
    $container.empty(); // Clear previous rows

    if (!Array.isArray(accounts)) return;

    accounts.forEach((account, index) => {
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
                                  <option value="NONE" ${account.DocumentBooking === 'NEGATE' ? 'selected' : ''}>NEGATE</option>
                            </select>
                            <label>Document Booking</label>
                        </div>
                    </div>
                </div>
            </div>`;
        $container.append(html);
    });
}

function loadAccounts000(containerId, accounts, accountType) {
    const container = document.getElementById(containerId);
    if (!container || !accounts) return;

    container.innerHTML = '';

    accounts.forEach((account, index) => {
        const accountItem = document.createElement('div');
        accountItem.className = 'account-item mb-3 p-3 border rounded';

        accountItem.innerHTML = `
            <button type="button" class="btn btn-danger btn-sm float-end" onclick="removeAccount(this)">×</button>
            <div class="row gy-3">
                <div class="col-md-6">
                    <div class="form-floating form-floating-outline">
                        <input type="text" name="FinancialDocumentReference.CorrespondingAccount.${accountType}[${index}].AccountNumber"
                            class="form-control" value="${account.AccountNumber}" placeholder="Account Number" required />
                        <label>Account Number</label>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-floating form-floating-outline">
                        <select name="FinancialDocumentReference.CorrespondingAccount.${accountType}[${index}].DocumentBooking"
                            class="form-control" required>
                            <option value="">--- Select Booking Direction ---</option>
                            <option value="Debit" ${account.DocumentBooking === 'Debit' ? 'selected' : ''}>Debit</option>
                            <option value="Credit" ${account.DocumentBooking === 'Credit' ? 'selected' : ''}>Credit</option>
                        </select>
                        <label>Document Booking</label>
                    </div>
                </div>
            </div>
        `;

        container.appendChild(accountItem);
    });
}