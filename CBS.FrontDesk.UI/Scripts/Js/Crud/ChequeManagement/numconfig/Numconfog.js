var numConfigTable = null;

function emptyToNull(v) {
    return (v === undefined || v === null || v === '') ? null : v;
}

function collectNumConfigFilters() {
    console.log('Collecting filters...');
    try {
        const filters = {
            Name: emptyToNull($('#name').val()),
            BranchId: emptyToNull($('#branchId').val()),
            BankCodePosition: $('#bankCodePosition').val() ? parseInt($('#bankCodePosition').val()) : null,
            BranchCodePosition: $('#branchCodePosition').val() ? parseInt($('#branchCodePosition').val()) : null,
            YearPosition: $('#yearPosition').val() ? parseInt($('#yearPosition').val()) : null,
            AcceptSerialNumber: $('#acceptSerialNumber').is(':checked') ? true : null,
            SerialNumberPosition: $('#serialNumberPosition').val() ? parseInt($('#serialNumberPosition').val()) : null,
            StartDate: emptyToNull($('#startDate').val()),
            EndDate: emptyToNull($('#endDate').val()),
            options: {}
        };
        console.log('Filters collected:', filters);
        return filters;
    } catch (error) {
        console.error('Error collecting filters:', error);
        return {};
    }
}

function loadNumConfigData() {
    console.log('Loading NumConfig data...');
    try {
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#myDataTable')) {
            numConfigTable.destroy();
            $('#myDataTable').empty();
        }

        numConfigTable = $('#myDataTable').DataTable({
            serverSide: true,
            destroy: true,
            searching: false,
            responsive: true,
            processing: true,
            order: [[0, 'desc']],
            ajax: {
                url: '/NumConfig/LoadNumConfigData',
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    const filters = collectNumConfigFilters();

                    filters.options = {
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        skip: d.start,
                        pageSize: d.length,
                        searchValue: '',
                        sortColumnName: d.columns[d.order[0].column].data,
                        sortColumnDirection: d.order[0].dir
                    };

                    console.log('Sending AJAX data:', filters);
                    return JSON.stringify(filters);
                },
                error: function (xhr, error, thrown) {
                    console.error('AJAX error:', error, thrown);
                    alert('Failed to load data. Please try again.');
                }
            },
            columns: [
                {
                    data: 'CreatedDate',
                    name: 'Created Date',
                    render: function (data) {
                        return data && window.moment
                            ? moment(data).format('DD/MM/YYYY HH:mm:ss')
                            : (data || '');
                    }
                },
                { data: 'Name', name: 'Name' },
                { data: 'BranchName', name: 'Branch' },
                { data: 'BankCodePosition', name: 'Bank Code Pos.' },
                { data: 'BranchCodePosition', name: 'Branch Code Pos.' },
                { data: 'YearPosition', name: 'Year Pos.' },
                { data: 'SerialNumberPosition', name: 'Serial No. Pos.' },
                {
                    data: 'AcceptSerialNumber',
                    name: 'Accept Serial',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success">Yes</span>'
                            : '<span class="badge bg-secondary">No</span>';
                    }
                },
                {
                    data: 'Id',
                    orderable: false,
                    searchable: false,
                    render: function (id) {
                        return `
                            <a href="#"
                               class="btn btn-sm btn-outline-primary me-1"
                               data-toggle="tooltip" title="Edit"
                               onclick="AddORUpdateGen('${id}', 'datalistingview', '_Create', 'get', 'NumConfig')">
                               <i class="mdi mdi-pencil"></i> Edit
                            </a>
                            <a href="#"
                               class="btn btn-sm btn-outline-danger"
                               data-toggle="tooltip" title="Delete"
                               onclick="DeleteRecordDataTable('NumConfig', '${id}', 'myDataTable', '_DataTable', 0, 'datalistingview', null, 'list')">
                               <i class="mdi mdi-trash-can"></i> Delete
                            </a>
                        `;
                    }
                }
            ],
            language: {
                emptyTable: 'No number configurations found',
                processing: '<i class="mdi mdi-loading mdi-spin"></i> Loading...',
                info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                infoEmpty: 'Showing 0 to 0 of 0 entries',
                infoFiltered: '(filtered from _MAX_ total entries)',
                lengthMenu: 'Show _MENU_ entries',
                loadingRecords: 'Loading...',
                zeroRecords: 'No matching records found',
                paginate: {
                    first: 'First',
                    last: 'Last',
                    next: 'Next',
                    previous: 'Previous'
                }
            },
            drawCallback: function () {
                console.log('DataTable draw callback');
                if (typeof ($.fn.tooltip) !== 'undefined') {
                    $('[data-toggle="tooltip"]').tooltip({ container: 'body' });
                }
            },
            initComplete: function () {
                console.log('DataTable initialization complete');
            }
        });

        console.log('DataTable initialized successfully');
    } catch (error) {
        console.error('Error initializing DataTable:', error);
        alert('Error initializing table. Please check console for details.');
    }
}

// Apply Filter button handler
function applyNumConfigFilters() {
    console.log('applyNumConfigFilters function called');
    try {
        if (numConfigTable) {
            console.log('Reloading DataTable with filters...');
            numConfigTable.ajax.reload();
        } else {
            console.log('DataTable not initialized, loading data...');
            loadNumConfigData();
        }
    } catch (error) {
        console.error('Error applying filters:', error);
        alert('Error applying filters. Please check console for details.');
    }
}

// Reset Filter button handler
function resetNumConfigFilters() {
    console.log('resetNumConfigFilters function called');
    try {
        // Reset form fields
        $('#numconfigFilterForm')[0].reset();

        // Reset select2 if used
        if ($.fn.select2 && $('#branchId').length) {
            $('#branchId').val('').trigger('change');
        }

        // Hide the togglable sections
        $('#branchFilter').addClass('d-none');
        $('#dateFilters').addClass('d-none');
        $('#toggleBranch').prop('checked', false);
        $('#toggleDates').prop('checked', false);

        // Reload DataTable with reset filters
        if (numConfigTable) {
            console.log('Reloading DataTable after reset...');
            numConfigTable.ajax.reload();
        } else {
            console.log('DataTable not initialized, loading data...');
            loadNumConfigData();
        }

        console.log('Filters reset successfully');
    } catch (error) {
        console.error('Error resetting filters:', error);
        alert('Error resetting filters. Please check console for details.');
    }
}

// Enhanced document ready with better event handling
$(document).ready(function () {
    console.log('Document ready - initializing NumConfig functionality');

    try {
        // Initialize DataTable first
        loadNumConfigData();

        // Use event delegation for buttons to handle dynamic content
        $(document).on('click', '#applyFilterBtn', function (e) {
            console.log('Apply Filter button clicked');
            e.preventDefault();
            e.stopPropagation();
            applyNumConfigFilters();
        });

        $(document).on('click', '#resetFilterBtn', function (e) {
            console.log('Reset Filter button clicked');
            e.preventDefault();
            e.stopPropagation();
            resetNumConfigFilters();
        });

        // Form enter key prevention
        $(document).on('keypress', '#numconfigFilterForm input', function (e) {
            if (e.which === 13) {
                console.log('Enter key pressed in filter form');
                e.preventDefault();
                applyNumConfigFilters();
            }
        });

        // Toggle functionality for branch and date filters
        $(document).on('change', '#toggleBranch', function () {
            $('#branchFilter').toggleClass('d-none', !this.checked);
        });

        $(document).on('change', '#toggleDates', function () {
            $('#dateFilters').toggleClass('d-none', !this.checked);
        });

        console.log('NumConfig event handlers attached successfully');

    } catch (error) {
        console.error('Error during NumConfig initialization:', error);
    }
});

// Global function to handle page reloads or external calls
function refreshNumConfigTable() {
    console.log('Refreshing NumConfig table');
    if (numConfigTable) {
        numConfigTable.ajax.reload();
    } else {
        loadNumConfigData();
    }
}

// Handle browser back/forward buttons
$(window).on('popstate', function () {
    console.log('Page state changed, refreshing table');
    refreshNumConfigTable();
});