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
    if (tabName=="view") {
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
            ContainsConditionAccounts: collectAccounts('ContainsConditionAccounts'),
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
    function toggleAccountSection(sectionType) {
        if (sectionType === 'condition') {
            const checkbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]');
    const section = document.getElementById('conditionSection');
    section.style.display = checkbox.checked ? 'block' : 'none';
        } else if (sectionType === 'exception') {
            const checkbox = document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]');
    const section = document.getElementById('exceptionSection');
    section.style.display = checkbox.checked ? 'block' : 'none';
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
    document.addEventListener('DOMContentLoaded', function() {
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
/*    try {*/
        // Show loading state (optional)
        showLoadingState();

        // Log the URL being called
        const url = `/AccountingConfiguration/GetAccountDetailsByReference/${id}`;
        console.log('Calling URL:', url);
        console.log('Document ID:', id);

        const response =  fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                // Add CSRF token if needed (for ASP.NET)
               // 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value,
                // Add other headers if needed
             //   'X-Requested-With': 'XMLHttpRequest'
            }
        });
        console.log('Response status:', response.status);
        console.log('Response headers:', response.headers);

        if (!response.ok) {
            const errorText =  response.text();
            console.error('Error response:', errorText);
            throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
        }

        const doc =  response.json();
        console.log('Received document:', doc);

 
        if (!doc) {
            throw new Error('Document not found');
        }

        // Set form values
        document.querySelector('input[name="FinancialDocumentReference.Id"]').value = doc.Id;
        document.querySelector('input[name="FinancialDocumentReference.Reference"]').value = doc.Reference;
        document.querySelector('input[name="FinancialDocumentReference.DescriptionEn"]').value = doc.DescriptionEn;
        document.querySelector('input[name="FinancialDocumentReference.DescriptionFr"]').value = doc.DescriptionFr || '';
        document.querySelector('input[name="FinancialDocumentReference.DescriptionSp"]').value = doc.DescriptionSp || '';
        document.querySelector('select[name="FinancialDocumentReference.DocumentId"]').value = doc.DocumentId;
        document.querySelector('select[name="FinancialDocumentReference.DocumentTypeId"]').value = doc.DocumentTypeId;

        // Set checkboxes
        document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]').checked = doc.CorrespondingAccount.ContainsCondition;
        document.querySelector('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]').checked = doc.CorrespondingAccount.ContainsException;

        // Toggle sections
        toggleAccountSection('condition');
        toggleAccountSection('exception');

        // Load accounts
        loadAccounts('grossAccounts', doc.CorrespondingAccount.GrossAccounts, 'GrossAccounts');
        loadAccounts('provisionAccounts', doc.CorrespondingAccount.ProvisionAccounts, 'ProvisionAccounts');
        loadAccounts('containsconditionAccounts', doc.CorrespondingAccount.ContainsConditionAccounts, 'containsconditionAccounts');
        loadAccounts('grossExceptionAccounts', doc.CorrespondingAccount.GrossExceptionAccounts, 'GrossExceptionAccounts');
        loadAccounts('provisionExceptionAccounts', doc.CorrespondingAccount.ProvisionExceptionAccounts, 'ProvisionExceptionAccounts');

        // Set action to update
        document.querySelector('input[name="Action"]').value = 'update';

        // Scroll to form
        document.querySelector('.card').scrollIntoView();

    //} catch (error) {
    //    console.error('Error fetching document:', error);

    //    // Handle different types of errors
    //    if (error.message.includes('404')) {
    //        showErrorMessage('Document not found');
    //    } else if (error.message.includes('403')) {
    //        showErrorMessage('Access denied');
    //    } else if (error.message.includes('500')) {
    //        showErrorMessage('Server error. Please try again later.');
    //    } else {
    //        showErrorMessage('Failed to load document. Please try again.');
    //    }
    //} finally {
    //    // Hide loading state (optional)
    //    hideLoadingState();
    //}
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

function loadAccounts(containerId, accounts, accountType) {
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