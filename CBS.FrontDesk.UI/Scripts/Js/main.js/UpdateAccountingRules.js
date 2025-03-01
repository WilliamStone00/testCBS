 
$(document).ready(function () {
 
    function loadAccountingRules() {
        $.ajax({
            url: '/ManuallyJournalEntry/GetAccountingRules',
            type: 'GET',
            dataType: 'json', // Ensure JSON response
            success: function (response) {
                if (!response || !Array.isArray(response)) {
                    console.log("Invalid data format received:", response);
                    return;
                }
                let tableBody = $('#AccountingRulebasketTable tbody');
                tableBody.empty();

                $.each(response, function (index, item) {
                    addRow(item.Id, item.MFI_ChartOfAccountId, item.BookingDirection);
                });
            },
            error: function (xhr, status, error) {
                console.error("Error loading data: ", error);
            }
        });
    }

    function addRow(id = null, chartOfAccountId = '', bookingDirection = '') {
        $.ajax({
            url: '/ManuallyJournalEntry/GetAccountMFIChartOfAccount',
            type: 'GET',
            success: function (accounts) {
                let options = '';

                // Check if accounts is an array and has map function
                if (Array.isArray(accounts) && typeof accounts.map === 'function') {
                    options = accounts.map(coa =>
                        `<option value="${coa.Value}" ${coa.Value == chartOfAccountId ? 'selected' : ''}>${coa.Text}</option>`
                    ).join('');
                } else {
                    // Handle case where accounts is not an array or doesn't have map
                    // Convert to array if possible or process differently
                    if (typeof accounts === 'object') {
                        // If accounts is an object with data property that's an array
                        if (Array.isArray(accounts.data)) {
                            options = accounts.data.map(coa =>
                                `<option value="${coa.Value}" ${coa.Value == chartOfAccountId ? 'selected' : ''}>${coa.Text}</option>`
                            ).join('');
                        } else {
                            // If it's just an object, convert to array if possible
                            const accountArray = Object.values(accounts);
                            if (accountArray.length > 0) {
                                options = accountArray.map(coa =>
                                    `<option value="${coa.Value}" ${coa.Value == chartOfAccountId ? 'selected' : ''}>${coa.Text}</option>`
                                ).join('');
                            }
                        }
                    }
                }

                let row = `
                <tr data-id="${id}">
                    <td>
                        <select class="form-control">${options}</select>
                    </td>
                    <td>
                        <select class="form-control">
                            <option value="Debit" ${bookingDirection == 'Debit' ? 'selected' : ''}>Debit</option>
                            <option value="Credit" ${bookingDirection == 'Credit' ? 'selected' : ''}>Credit</option>
                        </select>
                    </td>
                    <td>
                        <button type="button" class="btn btn-danger btn-sm delete-entry">
                            <i class="fas fa-minus-circle"></i> Remove
                        </button>
                    </td>
                </tr>`;
                $('#AccountingRulebasketTable tbody').append(row);
            },
            error: function (xhr, status, error) {
                console.error('Error fetching chart of accounts:', error);
            }
        });
    }
    }

    $('#addEntry').click(function () {
        addRow();
    });

    $(document).on('click', '.delete-entry', function () {
        let row = $(this).closest('tr');
        let rowId = row.data('id');
        if (confirm("Are you sure you want to delete this entry? This action cannot be undone.")) {
            if (rowId) {
                $.ajax({
                    url: `/api/deleteAccountingRule/${rowId}`,
                    type: 'DELETE',
                    success: function () {
                        row.remove();
                    },
                    error: function (error) {
                        console.log("Error deleting data: ", error);
                    }
                });
            } else {
                row.remove();
            }
        }
    });

    function saveForm() {
        let entries = [];
        $('#AccountingRulebasketTable tbody tr').each(function () {
            let rowId = $(this).data('id');
            let chartOfAccountId = $(this).find('td:eq(0) select').val();
            let bookingDirection = $(this).find('td:eq(1) select').val();
            entries.push({ id: rowId, chartOfAccountId, bookingDirection });
        });

        $.ajax({
            url: '/api/saveAccountingRules',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(entries),
            success: function (response) {
                alert("Data saved successfully!");
                loadAccountingRules();
            },
            error: function (error) {
                console.log("Error saving data: ", error);
            }
        });
    }

    loadAccountingRules();
});