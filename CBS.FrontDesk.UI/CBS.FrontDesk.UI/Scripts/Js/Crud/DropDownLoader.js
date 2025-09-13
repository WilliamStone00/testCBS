


function LoadOperationEventAttributes(id, OperationEventAttributeId) {
    console.log(id);

    console.log(OperationEventAttributeId);
    var url = "/TransactionConfiguration/Ajaxloader?Key=" + id;
    FillDropDownAjaxCall(url, OperationEventAttributeId, "---Select operation event attribute---")
}

function LoadProductConfig(key, partialView, serviceOption, path) {
    const baseUrl = '/TransactionConfiguration/InitializeData';
    const queryParams = $.param({
        KEY: key,
        partialView: partialView,
        serviceOption: serviceOption,
        path: path
    });

    const apiUrl = `${baseUrl}?${queryParams}`; // Construct the full URL

    $.getJSON(apiUrl, function (response) {
        console.log(response);
        renderTables(response);
    });
}
function renderTables(data) {
    const container = $('#productContainer');
  
    data.forEach(product => {
        // Create a section for each product
        const tableId = `table-${product.productId}`;
        const section = `
            <h3>${product.productName} (${product.productId})</h3>
            <table id="${tableId}" class="display" style="width:100%">
                <thead>
                    <tr>
                        <th>Operation Event</th>
                        <th>Account Number</th>
                        <th>Description</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody></tbody>
            </table>
        `;
        container.append(section);

        // Populate the DataTable for each product
        $(`#${tableId}`).DataTable({
            data: product.accountingChart,
            columns: [
                { data: 'operationEvent', title: 'Operation Event' },
                { data: 'accountNumber', title: 'Account Number' },
                { data: 'description', title: 'Description' },
                { data: 'status', title: 'Status' }
            ],
            pageLength: 5
        });
    });
}
