
$(document).ready(function () {
    // Initialize DataTable for #myDataTable
    $('#myDataTable').DataTable({
        paging: true, // Enable pagination
        searching: true, // Enable search bar
        ordering: true, // Enable column ordering
        pageLength: 10, // Default rows per page
        lengthMenu: [5, 10, 25, 50, 100], // Options for rows per page
        columnDefs: [
            { orderable: false, targets: -1 } // Disable ordering for the last column (Action)
        ],
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "No entries to show",
            infoFiltered: "(filtered from _MAX_ total entries)",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        }
    });

});


