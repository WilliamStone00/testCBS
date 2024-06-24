
$(document).ready(function () {

    LoadLoans("All");
});

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}


function manualSearch() {
    LoadLoans($('#manualSearchInput').val())
}
function LoadLoans(search) {
    $("#myDataTable").DataTable({
        "destroy": true, // Reinitialize DataTable on each load
        "serverSide": true, // Enable server-side processing
        "info": true, // Show table information (pagination details)
        "stateSave": true, // Maintain table state (pagination, search, etc.) using localStorage
        "lengthMenu": [[10, 20, 100, 500], [10, 20, 100, 500]], // Define page length menu options
        "searching": false, // Disable search input
        "ajax": {
            "url": "/Loan/LoadData", // URL to fetch data from server
            "type": "POST", // HTTP method
            "data": {
                "searchCriteria": search // Additional data to send to server (optional)
            },
            "dataSrc": function (json) { // Function to manipulate returned JSON data
                if (json.error) {
                    console.error(json.error); // Log any errors returned from the server
                    return [];
                }
                return json.data; // Return the data array to DataTable for rendering
            }
        },
        "columns": [ // Define columns and their properties
            { "data": "DisbursementDate", "name": "DisbursementDate", "autoWidth": true },
            { "data": "CustomerId", "name": "CustomerId", "autoWidth": true },
            { "data": "Principal", "name": "Principal", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) }, // Format numbers with thousand separators
            { "data": "InterestRate", "name": "InterestRate", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "AccrualInterest", "name": "AccrualInterest", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Tax", "name": "Tax", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Fines", "name": "Fines", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Paid", "name": "Paid", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            { "data": "Balance", "name": "Balance", "autoWidth": true, "render": $.fn.dataTable.render.number(',', '.', 1) },
            {
                "data": "Id", "orderable": false, // Disable ordering on this column
                "render": function (data) {
                    return `<a href='/Loan/Details?KEY=${data}' target='_blank' class='mr-2' data-toggle='tooltip' data-placement='top' title='View loan details of ${data}.'>Details</a>`;
                }
            }
        ],

        //"order": [[0, "desc"]], // Uncomment to set initial sorting by DisbursementDate in descending order
        //"orderFixed": [[0, "desc"]] // Uncomment to fix sorting by DisbursementDate in descending order
    });
}
