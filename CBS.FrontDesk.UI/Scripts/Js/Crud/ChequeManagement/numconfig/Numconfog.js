// numconfig-shared.js - Shared functionality for NumConfig
window.NumConfig = window.NumConfig || {};

// --- ENHANCED DRAG & DROP SYSTEM ---
window.initializeDragAndDrop = function (container) {
    console.log('Initializing drag and drop...');

    // Use specific container or search for dragZone
    const dragZone = container ? container.find('#dragZone').get(0) : document.getElementById('dragZone');
    if (!dragZone) {
        console.log('Drag zone not found');
        return;
    }

    let draggedItem = null;

    function updateNumericPositionsFromDrag() {
        const items = [...dragZone.querySelectorAll('.draggable')];
        console.log('Updating positions from drag, items:', items.length);

        items.forEach((item, index) => {
            const varName = item.dataset.variable;
            const position = index + 1;

            console.log(`Setting ${varName} to position ${position}`);

            const positionInput = document.getElementById(varName.toLowerCase() + 'Position');
            if (positionInput) {
                positionInput.value = position;
            }
        });
    }

    // Clear existing event listeners
    dragZone.querySelectorAll('.draggable').forEach(item => {
        item.replaceWith(item.cloneNode(true));
    });

    // Make draggable and add events
    dragZone.querySelectorAll('.draggable').forEach(item => {
        item.setAttribute('draggable', true);

        item.addEventListener('dragstart', (e) => {
            draggedItem = item;
            setTimeout(() => item.style.opacity = '0.4', 0);
            e.dataTransfer.effectAllowed = 'move';
        });

        item.addEventListener('dragend', () => {
            if (draggedItem) {
                draggedItem.style.opacity = '1';
                draggedItem = null;
            }
            updateNumericPositionsFromDrag();
        });
    });

    // Improved drag over handler
    dragZone.addEventListener('dragover', e => {
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';

        const afterElement = getDragAfterElement(dragZone, e.clientX);
        if (!afterElement) {
            dragZone.appendChild(draggedItem);
        } else {
            dragZone.insertBefore(draggedItem, afterElement);
        }
    });

    function getDragAfterElement(container, x) {
        const draggableElements = [...container.querySelectorAll('.draggable:not(.dragging)')];
        let closest = null;
        let closestOffset = Number.NEGATIVE_INFINITY;

        draggableElements.forEach(element => {
            const box = element.getBoundingClientRect();
            const offset = x - box.left - box.width / 2;

            if (offset < 0 && offset > closestOffset) {
                closestOffset = offset;
                closest = element;
            }
        });

        return closest;
    }

    // Initialize numeric positions from current drag zone state
    updateNumericPositionsFromDrag();

    // Set up numeric position toggle
    const toggleCheckbox = document.getElementById('toggleNumericPositions');
    const numericContainer = document.getElementById('numericPositions');

    if (toggleCheckbox && numericContainer) {
        // Remove existing event listeners
        toggleCheckbox.replaceWith(toggleCheckbox.cloneNode(true));
        const newToggle = document.getElementById('toggleNumericPositions');

        newToggle.addEventListener('change', function () {
            numericContainer.style.display = this.checked ? 'flex' : 'none';
        });

        // Initialize state
        numericContainer.style.display = newToggle.checked ? 'flex' : 'none';
    }

    console.log('Drag and drop initialized successfully');
};

// --- UNIFIED FORM INITIALIZATION ---
window.initializeNumConfigForm = function (container) {
    console.log('Initializing NumConfig form...');

    const $container = container ? $(container) : $(document);

    // Disable template customizer for AJAX-loaded content to prevent errors
    if (window.TemplateCustomizer) {
        console.log('TemplateCustomizer detected, disabling for AJAX content');
        // Prevent template customizer from running on AJAX content
        $container.find('.template-customizer').remove();
    }

    // Initialize drag and drop
    if (typeof initializeDragAndDrop === 'function') {
        initializeDragAndDrop($container);
    }

    // Initialize Select2
    if ($.fn.select2) {
        $container.find('#branchId').each(function () {
            if (!$(this).hasClass('select2-hidden-accessible')) {
                $(this).select2({
                    width: '100%',
                    placeholder: 'Select Branch',
                    dropdownParent: $container.closest('.modal-body') || $('body')
                });
            }
        });
    }

    // Initialize tooltips
    if ($.fn.tooltip) {
        $container.find('[data-bs-toggle="tooltip"]').tooltip({ container: 'body' });
    }

    // Form submission handler
    $container.find('#manualForm').off('submit').on('submit', function (e) {
        console.log('Form submitted, updating positions...');
        // Ensure positions are updated before submit
        if (typeof initializeDragAndDrop === 'function') {
            const $formContainer = $(this).closest('#dragZone').length ? $(this).closest('#dragZone').parent() : $(this);
            initializeDragAndDrop($formContainer);
        }
        return true;
    });

    console.log('NumConfig form initialization complete');
};

// --- LIST VIEW MANAGEMENT ---
window.NumConfigListView = {
    table: null,
    isInitialized: false,

    emptyToNull: function (v) {
        return (v === undefined || v === null || v === '') ? null : v;
    },

    collectFilters: function () {
        console.log('Collecting NumConfig filters...');
        try {
            const filters = {
                Name: this.emptyToNull($('#name').val()),
                BranchId: this.emptyToNull($('#branchId').val()),
                BankCodePosition: $('#bankCodePosition').val() ? parseInt($('#bankCodePosition').val()) : null,
                BranchCodePosition: $('#branchCodePosition').val() ? parseInt($('#branchCodePosition').val()) : null,
                YearPosition: $('#yearPosition').val() ? parseInt($('#yearPosition').val()) : null,
                AcceptSerialNumber: $('#acceptSerialNumber').is(':checked') ? true : null,
                SerialNumberPosition: $('#serialNumberPosition').val() ? parseInt($('#serialNumberPosition').val()) : null,
                StartDate: this.emptyToNull($('#startDate').val()),
                EndDate: this.emptyToNull($('#endDate').val()),
                options: {}
            };
            return filters;
        } catch (error) {
            console.error('Error collecting filters:', error);
            return {};
        }
    },

    initializeTable: function () {
        console.log('Initializing NumConfig DataTable...');

        if (!$('#myDataTable').length) {
            console.error('Table element #myDataTable not found');
            return false;
        }

        try {
            // Destroy existing table if it exists
            if ($.fn.DataTable.isDataTable('#myDataTable')) {
                this.table.destroy();
                $('#myDataTable').closest('.dataTables_wrapper').remove();
            }

            this.table = $('#myDataTable').DataTable({
                serverSide: true,
                destroy: true,
                searching: false,
                responsive: true,
                order: [[0, 'desc']],
                ajax: {
                    url: '/NumConfig/LoadNumConfigData',
                    type: 'POST',
                    contentType: 'application/json',
                    data: function (d) {
                        const filters = NumConfigListView.collectFilters();
                        filters.options = {
                            draw: d.draw,
                            start: d.start,
                            length: d.length,
                            skip: d.start,
                            pageSize: d.length,
                            searchValue: d.search?.value || '',
                            sortColumnName: d.columns[d.order[0].column].data,
                            sortColumnDirection: d.order[0].dir
                        };
                        console.log('Sending filters to server:', filters);
                        return JSON.stringify(filters);
                    },
                    error: function (xhr, error, thrown) {
                        console.error('AJAX error:', error, thrown);
                        if (typeof appalert === 'function') {
                            appalert('Failed to load data. Please try again.', 0, 1);
                        }
                    }
                },
                columns: [
                    {
                        data: 'CreatedDate',
                        render: function (data, type, row) {
                            const v = row.CreatedDate || row.CreatedOn;
                            return v ? (window.moment ? moment(v).format('DD/MM/YYYY HH:mm:ss') : v) : '-';
                        }
                    },
                    { data: 'Name' },
                    { data: 'BranchName' },
                    { data: 'BankCodePosition' },
                    { data: 'BranchCodePosition' },
                    { data: 'YearPosition' },
                    {
                        data: 'AcceptSerialNumber',
                        render: function (data) {
                            return data ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>';
                        }
                    },
                    {
                        data: 'Id',
                        orderable: false,
                        render: function (data, type, row) {
                            var recordId = encodeURIComponent(data || '');
                            var name = (row.name || row.Name || '').replace(/"/g, '&quot;');

                            return `
                                    <div class="dropdown">
                                        <button class="btn btn-sm btn-info dropdown-toggle text-white" type="button" data-bs-toggle="dropdown">
                                            <i class="mdi mdi-cog-outline"></i> Actions
                                        </button>
                                        <ul class="dropdown-menu">
                                            <li>
                                                <button type="button" class="dropdown-item btn-edit" data-id="${recordId}" data-name="${name}">
                                                    <i class="mdi mdi-pencil-outline me-1 text-success"></i> Edit
                                                </button>
                                            </li>
                                            <li>
                                                <button type="button" class="dropdown-item btn-delete text-danger" data-id="${recordId}" data-name="${name}">
                                                    <i class="mdi mdi-delete-outline me-1 text-danger"></i> Delete
                                                </button>
                                            </li>
                                        </ul>
                                    </div>
                                `;
                        }
                    }
                ],
                language: {
                    emptyTable: 'No number configurations found. Click "Apply Filter" to load data.',
                    processing: '<i class="mdi mdi-loading mdi-spin"></i> Loading data...',
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
                initComplete: function () {
                    console.log('NumConfig DataTable initialized');
                    NumConfigListView.isInitialized = true;
                    NumConfigListView.attachEventHandlers();
                }
            });

            return true;
        } catch (error) {
            console.error('Error initializing DataTable:', error);
            if (typeof appalert === 'function') {
                appalert('Error initializing table. Please check console for details.', 0, 1);
            }
            return false;
        }
    },

    loadData: function () {
        console.log('Loading NumConfig data...');
        if (this.table && this.isInitialized) {
            console.log('Table exists, reloading data...');
            this.table.ajax.reload(null, false);
        } else {
            console.log('Table not initialized, initializing now...');
            this.initializeTable();
        }
    },

    applyFilters: function () {
        console.log('Applying NumConfig filters...');
        this.loadData();
    },

    resetFilters: function () {
        console.log('Resetting NumConfig filters...');
        $('#numconfigFilterForm')[0].reset();
        if ($.fn.select2 && $('#branchId').length) {
            $('#branchId').val('').trigger('change');
        }
        $('#branchFilter, #dateFilters').addClass('d-none');
        $('#toggleBranch, #toggleDates').prop('checked', false);

        if (this.isInitialized && this.table) {
            this.table.ajax.reload(null, false);
        }

        console.log('Filters reset successfully');
        if (typeof appalert === 'function') {
            appalert('Filters reset', 3, 1);
        }
    },

    loadEditForm: function (id) {
        console.log('Loading edit form for ID:', id);

        var url = '/NumConfig/InitializeData' +
            '?KEY=' + encodeURIComponent(id) +
            '&partialView=' + encodeURIComponent('Index') +
            '&path=details';

        var $target = $('#checknumdrop');
        var previousHtml = $target.html();

        // Show loading
        $target.html('<div class="text-center p-4"><i class="mdi mdi-loading mdi-spin mdi-36px"></i><div class="mt-2">Loading form...</div></div>');

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'html',
            cache: false
        }).done(function (html) {
            $target.html(html);
            console.log('Edit form loaded successfully');

            // Initialize the form with all components
            if (typeof initializeNumConfigForm === 'function') {
                initializeNumConfigForm($target);
            } else {
                console.error('initializeNumConfigForm function not found - make sure numconfig-shared.js is loaded');
            }

        }).fail(function (xhr, status, error) {
            console.error('Failed to load edit form:', status, error);
            $target.html(previousHtml);
            if (typeof appalert === 'function') {
                appalert('Failed to load edit form. Please try again.', 0, 1);
            }
        });
    },

    attachEventHandlers: function () {
        console.log('Attaching NumConfig event handlers...');

        // Remove existing handlers to prevent duplicates
        $('#applyFilterBtn').off('click');
        $('#resetFilterBtn').off('click');
        $('#toggleBranch').off('change');
        $('#toggleDates').off('change');
        $(document).off('click', '.btn-edit');
        $(document).off('click', '.btn-delete');

        // Filter buttons
        $('#applyFilterBtn').on('click', function (e) {
            e.preventDefault();
            NumConfigListView.applyFilters();
        });

        $('#resetFilterBtn').on('click', function (e) {
            e.preventDefault();
            NumConfigListView.resetFilters();
        });

        // Toggle filters
        $('#toggleBranch').on('change', function () {
            $('#branchFilter').toggleClass('d-none', !this.checked);
        });

        $('#toggleDates').on('change', function () {
            $('#dateFilters').toggleClass('d-none', !this.checked);
        });

        // Edit button
        $(document).on('click', '.btn-edit', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            if (id) {
                NumConfigListView.loadEditForm(id);
            }
        });

        $(document).on('click', '.btn-delete', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            if (id) {
                DeleteRecordDataTable('NumConfig', id, 'myDataTable', '_DataTable', 0, 'datalistingview', null, 'list');
            }
        });

        // Form enter key prevention
        $('#numconfigFilterForm input').off('keypress').on('keypress', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                NumConfigListView.applyFilters();
            }
        });
    },

    initializeListView: function () {
        console.log('Initializing NumConfig list view...');

        // Attach event handlers first
        this.attachEventHandlers();

        // Initialize Select2
        if ($.fn.select2 && $('#branchId').length && !$('#branchId').hasClass('select2-hidden-accessible')) {
            $('#branchId').select2({
                width: '100%',
                placeholder: 'Select Branch'
            });
        }

        // Initialize tooltips
        if ($.fn.tooltip) {
            $('[data-bs-toggle="tooltip"]').tooltip({ container: 'body' });
        }

        console.log('NumConfig list view initialized');
    }
};

// --- GLOBAL INITIALIZATION ---
$(document).ready(function () {
    console.log('NumConfig shared scripts loaded');

    // Initialize based on current page content
    if ($('#dragZone').length) {
        console.log('Form view detected - initializing form');
        if (typeof initializeNumConfigForm === 'function') {
            initializeNumConfigForm();
        }
    }

    if ($('#numconfigFilterForm').length) {
        console.log('List view detected - initializing list view');
        NumConfigListView.initializeListView();
    }
});