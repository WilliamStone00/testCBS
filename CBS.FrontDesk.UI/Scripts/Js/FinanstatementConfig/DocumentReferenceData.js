 
    let documents = [];

    // Load all documents into memory (Read)
    function loadFinancialDocuments() {
        $.ajax({
            url: '/AccountingConfiguration/GetAllDocumentReference',
            method: 'GET',
            success: function (data) {
                documents = data;
                populateTable(data);
            },
            error: function (xhr) {
                console.error('Failed to load financial documents:', xhr);
                showNotification("Could not load financial documents.", "error");
            }
        });
    }

    // Populate into table (optional, if a table view exists)
    function populateTable(data) {
        const $tableBody = $('#financialDocsTable tbody');
    $tableBody.empty();
        data.forEach(doc => {
            const row = `<tr>
        <td>${doc.Reference}</td>
        <td>${doc.DescriptionEn}</td>
        <td>${doc.DescriptionFr}</td>
        <td>${doc.DescriptionSp}</td>
        <td>
            <button class="btn btn-sm btn-primary" onclick="editDocument('${doc.Id}')">Edit</button>
            <button class="btn btn-sm btn-danger" onclick="deleteDocument('${doc.Id}')">Delete</button>
        </td>
    </tr>`;
    $tableBody.append(row);
        });
    }

    // Edit (load into form)
    function editDocument(id) {
        const doc = documents.find(d => d.Id === id);
    if (!doc) return;

    // Set main form fields
    $('input[name="FinancialDocumentReference.Id"]').val(doc.Id);
    $('input[name="FinancialDocumentReference.Reference"]').val(doc.Reference);
    $('input[name="FinancialDocumentReference.DescriptionEn"]').val(doc.DescriptionEn);
    $('input[name="FinancialDocumentReference.DescriptionFr"]').val(doc.DescriptionFr);
    $('input[name="FinancialDocumentReference.DescriptionSp"]').val(doc.DescriptionSp);
    $('select[name="FinancialDocumentReference.DocumentId"]').val(doc.DocumentId).trigger('change');
    $('select[name="FinancialDocumentReference.DocumentTypeId"]').val(doc.DocumentTypeId).trigger('change');

    // Clear and populate account sections dynamically
    clearAccounts();
    if (doc.CorrespondingAccount) {
        renderAccounts(doc.CorrespondingAccount);
        }

    // Change mode to Update
    $('#Action').val('update');
    }

    // Delete
    function deleteDocument(id) {
        if (!confirm('Are you sure you want to delete this financial document?')) return;
    $.ajax({
        url: '/AccountingConfiguration/DeleteDocumentReference',
    method: 'POST',
    data: {id},
    success: function () {
        showNotification("Document deleted successfully.", "success");
    loadFinancialDocuments();
            },
    error: function (xhr) {
        console.error("Delete failed:", xhr);
    showNotification("Error deleting document.", "error");
            }
        });
    }

    // Add new account row dynamically
 
    // Remove account row
    function removeAccount(button) {
        $(button).closest('.account-item').remove();
    }

    // Reset form
    function resetForm() {
        $('#Action').val('insert');
    $('form')[0].reset();
    $('.select2').val(null).trigger('change');
    clearAccounts();
    }

    // Clear dynamic account sections
    function clearAccounts() {
        $('#grossAccounts, #provisionAccounts, #containsconditionAccounts, #grossExceptionAccounts, #provisionExceptionAccounts').empty();
    $('#conditionSection, #exceptionSection').hide();
    }

    // Toggle conditional account sections
    function toggleAccountSection(type) {
        if (type === 'condition') {
        $('#conditionSection').toggle($('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsCondition"]').is(':checked'));
        } else if (type === 'exception') {
        $('#exceptionSection').toggle($('input[name="FinancialDocumentReference.CorrespondingAccount.ContainsException"]').is(':checked'));
        }
    }

    // Notification helper (optional)
    function showNotification(message, type) {
        alert(`${type.toUpperCase()}: ${message}`);
    }

    // On DOM ready
    $(function () {
        loadFinancialDocuments(); // Initial load
    });
 