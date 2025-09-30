/* Numconfog.js
   Combined generic initializer + Numconfog configuration
   Requires: jQuery, DataTables. Optional: moment, select2, bootstrap tooltip.
*/

(function () {
    'use strict';

    // -------------------- Generic initializer --------------------
    function initGenericCrud(config) {
        const cfg = Object.assign({
            tableSelector: '#myDataTable',
            formSelector: '#filterForm',
            modalSelector: '#genericModal', // fallback to document.body if not found
            loadUrl: '/LoadNumConfigData',
            deleteUrl: '/Delete',
            exportUrl: null,
            columns: [],
            collectFilters: function () { return { options: {} }; },
            dragConfig: null,
            onRowRendered: null
        }, config);

        // DataTable loader
        function loadData() {
            if ($.fn.dataTable.isDataTable(cfg.tableSelector)) {
                $(cfg.tableSelector).DataTable().destroy();
            }

            $(cfg.tableSelector).DataTable({
                serverSide: true,
                processing: false,
                searching: false,
                responsive: true,
                autoWidth: false,
                order: [[0, 'desc']],
                ajax: {
                    url: cfg.loadUrl,
                    type: 'POST',
                    contentType: 'application/json',
                    data: function (d) {
                        const filters = cfg.collectFilters();
                        filters.options = {
                            draw: d.draw,
                            start: d.start,
                            length: d.length,
                            skip: d.start,
                            pageSize: d.length,
                            searchValue: '',
                            sortColumnName: (d.columns && d.order && d.order.length) ? d.columns[d.order[0].column].data : '',
                            sortColumnDirection: (d.order && d.order.length) ? d.order[0].dir : ''
                        };
                        return JSON.stringify(filters);
                    }
                },
                columns: cfg.columns,
                drawCallback: function () {
                    if (typeof ($.fn.tooltip) !== 'undefined') {
                        $('[data-toggle="tooltip"]').tooltip({ container: 'body' });
                    }
                    if (typeof cfg.onRowRendered === 'function') cfg.onRowRendered();
                }
            });
        }

        // Reset filters
        function resetFilters() {
            $(cfg.formSelector).trigger('reset');
            $('.select2').val('').trigger('change');
            // hide any toggled filter groups if present
            $('#branchFilter, #dateFilters, #flagFilters').addClass('d-none');
            loadData();
        }

        // Export
        function doExport() {
            if (!cfg.exportUrl) return;
            const filters = cfg.collectFilters();
            const params = new URLSearchParams();
            Object.keys(filters).forEach(k => {
                if (filters[k] !== null && filters[k] !== undefined && k !== 'options') {
                    params.append(k, filters[k]);
                }
            });
            window.location.href = `${cfg.exportUrl}?${params.toString()}`;
        }

        // Delete fallback AJAX
        function fallbackDelete(id) {
            if (!confirm('Are you sure you want to delete this record?')) return;
            $.ajax({
                url: `${cfg.deleteUrl}?id=${encodeURIComponent(id)}`,
                type: 'POST',
                success: function (res) {
                    if ($.fn.dataTable.isDataTable(cfg.tableSelector)) $(cfg.tableSelector).DataTable().ajax.reload(null, false);
                },
                error: function () {
                    alert('Delete failed (server error)');
                }
            });
        }

        // Expose fallback delete for inline calls that expect DeleteRecordDataTable
        window.DeleteRecordDataTable = window.DeleteRecordDataTable || function (entity, id, dt, partial, index, container, extra, mode) {
            // if a global DeleteRecordDataTable exists originally, this will not override it
            fallbackDelete(id);
        };

        // Wire filter buttons
        $(document).on('click', `${cfg.formSelector} #applyFilterBtn`, function (e) { e.preventDefault(); loadData(); });
        $(document).on('click', `${cfg.formSelector} #resetFilterBtn`, function (e) { e.preventDefault(); resetFilters(); });
        if (cfg.exportUrl) $(document).on('click', `${cfg.formSelector} #exportBtn`, function (e) { e.preventDefault(); doExport(); });

        // -------------------- Drag & Drop modal handler (optional) --------------------
        if (cfg.dragConfig) {
            const modalRoot = document.querySelector(cfg.modalSelector) || document.body;

            // when create/edit partial is inserted, populate drag zone from inputs and attach behaviors
            const observer = new MutationObserver(muts => {
                muts.forEach(m => {
                    m.addedNodes.forEach(node => {
                        if (!(node instanceof HTMLElement)) return;
                        const zone = node.querySelector(cfg.dragConfig.zoneSelector) || (node.id === cfg.dragConfig.zoneSelector.replace('#', '') ? node : null);
                        if (zone) {
                            // small delay to allow inputs to be present
                            setTimeout(() => {
                                populateDragFromInputs(zone);
                                initDrag(zone);
                                // intercept form submit to sync positions before AjaxPostAndUpdate
                                const form = node.querySelector('form');
                                if (form) {
                                    const prev = form.onsubmit;
                                    form.onsubmit = function (ev) {
                                        syncDragToInputs(zone);
                                        if (typeof window.NumconfogSyncBeforeSubmit === 'function') {
                                            return window.NumconfogSyncBeforeSubmit(form);
                                        }
                                        if (typeof prev === 'function') return prev(ev);
                                        return true;
                                    };
                                }
                            }, 50);
                        }
                    });
                });
            });

            observer.observe(modalRoot, { childList: true, subtree: true });

            function populateDragFromInputs(zone) {
                const mapping = cfg.dragConfig.inputs || {};
                const items = [...zone.querySelectorAll('.draggable')];
                const nodesByKey = {};
                items.forEach(n => nodesByKey[n.getAttribute('data-variable')] = n);

                const entries = Object.keys(mapping).map(k => ({
                    key: k,
                    pos: parseInt($(mapping[k]).val() || 9999),
                    node: nodesByKey[k] || null
                }));

                // serial checkbox handling
                if (cfg.dragConfig.serialCheckbox) {
                    const accept = $(cfg.dragConfig.serialCheckbox).is(':checked');
                    if (!accept && nodesByKey['SerialNumber']) nodesByKey['SerialNumber'].style.display = 'none';
                    else if (nodesByKey['SerialNumber']) nodesByKey['SerialNumber'].style.display = 'inline-block';
                }

                entries.sort((a, b) => (a.pos || 9999) - (b.pos || 9999));
                entries.forEach(e => { if (e.node) zone.appendChild(e.node); });
            }

            function syncDragToInputs(zone) {
                const nodes = [...zone.querySelectorAll('.draggable')].filter(n => n.style.display !== 'none');
                let pos = 1;
                nodes.forEach(n => {
                    const key = n.getAttribute('data-variable');
                    const inputSelector = cfg.dragConfig.inputs && cfg.dragConfig.inputs[key];
                    if (inputSelector) $(inputSelector).val(pos++);
                });
            }

            function initDrag(zone) {
                let dragged = null;

                function onDragStart(e) {
                    dragged = this;
                    try { e.dataTransfer.setData('text/plain', this.getAttribute('data-variable')); } catch (err) { }
                    setTimeout(() => { this.style.opacity = '0.4'; }, 0);
                }

                function onDragEnd() {
                    if (dragged) dragged.style.opacity = '1';
                    dragged = null;
                }

                function getDragAfterElement(container, x) {
                    const draggableElements = [...container.querySelectorAll('.draggable')].filter(el => el.style.display !== 'none' && el !== dragged);
                    let closest = null;
                    let closestOffset = Number.NEGATIVE_INFINITY;
                    draggableElements.forEach(child => {
                        const box = child.getBoundingClientRect();
                        const offset = x - box.left - box.width / 2;
                        if (offset < 0 && offset > closestOffset) {
                            closestOffset = offset;
                            closest = child;
                        }
                    });
                    return closest;
                }

                zone.querySelectorAll('.draggable').forEach(item => {
                    item.setAttribute('draggable', 'true');
                    item.removeEventListener('dragstart', onDragStart);
                    item.addEventListener('dragstart', onDragStart);
                    item.removeEventListener('dragend', onDragEnd);
                    item.addEventListener('dragend', onDragEnd);
                });

                zone.removeEventListener('dragover', zone._onDragOver);
                zone._onDragOver = function (e) {
                    e.preventDefault();
                    const afterElement = getDragAfterElement(zone, e.clientX);
                    if (afterElement == null) zone.appendChild(dragged);
                    else zone.insertBefore(dragged, afterElement);
                };
                zone.addEventListener('dragover', zone._onDragOver);
            }

            // expose sync function (so other code can call before Ajax submit)
            window.NumconfogSyncBeforeSubmit = window.NumconfogSyncBeforeSubmit || function (form) {
                const zone = form.querySelector(cfg.dragConfig.zoneSelector);
                if (zone) syncDragToInputs(zone);
                return true;
            };
        }

        if (cfg.dragConfig && cfg.dragConfig.numericToggleSelector) {
            const checkbox = document.querySelector(cfg.dragConfig.numericToggleSelector);
            const numericContainer = document.querySelector(cfg.dragConfig.numericContainerSelector);
            if (checkbox && numericContainer) {
                checkbox.addEventListener('change', function () {
                    numericContainer.style.display = this.checked ? 'flex' : 'none';
                });
            }
        }


        // initial load
        loadData();

        // expose for manual control if needed
        return {
            reload: loadData,
            reset: resetFilters,
            export: doExport
        };
    }

    // -------------------- Numconfog specific instantiation --------------------
    // configure columns and collectFilters to match your server response / view fields
    const numconfogCrud = initGenericCrud({
        tableSelector: '#myDataTable',
        formSelector: '#numconfogFilterForm',
        modalSelector: '#genericModal', // change if your modal has a specific id
        loadUrl: '/NumConfig/LoadNumConfigData',    // server endpoint to return DataTables JSON
        deleteUrl: '/NumConfig/Delete',    // delete endpoint (fallback)
        exportUrl: '/NumConfig/Download',  // optional export endpoint
        columns: [
            { data: 'Name', name: 'Name' },
            { data: 'BranchName', name: 'BranchName' },
            { data: 'BankCodePosition', name: 'BankCodePosition' },
            { data: 'BranchCodePosition', name: 'BranchCodePosition' },
            { data: 'YearPosition', name: 'YearPosition' },
            { data: 'SeriaNumberPosition', name: 'SeriaNumberPosition' },
            {
                data: null,
                name: 'Components',
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    const comps = [
                        { label: '🏦 Bank Code', pos: Number(row.BankCodePosition) || 0, key: 'BankCode' },
                        { label: '🏢 Branch Code', pos: Number(row.BranchCodePosition) || 0, key: 'BranchCode' },
                        { label: '📅 Year', pos: Number(row.YearPosition) || 0, key: 'Year' }
                    ];
                    if (row.AcceptSerialNumber === true || String(row.AcceptSerialNumber).toLowerCase() === 'true') {
                        comps.push({ label: '#️⃣ Serial Number', pos: Number(row.SeriaNumberPosition) || 0, key: 'SerialNumber' });
                    }
                    comps.sort((a, b) => (a.pos || 9999) - (b.pos || 9999));
                    return comps.map(c => `<span class="badge bg-light text-dark me-1 border">${c.label}</span>`).join('');
                }
            },
            {
                data: 'AcceptSerialNumber',
                name: 'AcceptSerialNumber',
                render: function (data) {
                    return data ? '<span class="badge bg-success">Yes</span>' : '<span class="badge bg-secondary">No</span>';
                }
            },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data, type, row) {
                    const id = encodeURIComponent(row.Id || row.id || '');
                    const nameEsc = (row.Name || '').replace(/['"]/g, '');
                    return `
                        <a href="#"
                           onclick="AddORUpdateGen('${id}', 'datalistingview', '_Create', 'get', 'NumConfig')"
                           class="btn btn-sm btn-outline-primary me-1"
                           data-toggle="tooltip" title="Edit ${nameEsc}">
                           <i class="mdi mdi-pencil"></i> Edit
                        </a>
                        <a href="#"
                           onclick="DeleteRecordDataTable('NumConfig', '${id}', 'myDataTable', '_DataTable', 0, 'datalistingview', null, 'list')"
                           class="btn btn-sm btn-outline-danger"
                           data-toggle="tooltip" title="Delete ${nameEsc}">
                           <i class="mdi mdi-trash-can"></i> Delete
                        </a>
                    `;
                }
            }
        ],
        collectFilters: function () {
            function emptyToNull(v) { return (v === undefined || v === null || v === '') ? null : v; }
            return {
                Name: emptyToNull($('#name').val()),
                BranchId: emptyToNull($('#branchId').val()),
                BankCodePosition: parseInt($('#bankCodePosition').val() || 0),
                BranchCodePosition: parseInt($('#branchCodePosition').val() || 0),
                YearPosition: parseInt($('#yearPosition').val() || 0),
                AcceptSerialNumber: $('#acceptSerialNumber').is(':checked') ? true : null,
                SeriaNumberPosition: parseInt($('#seriaNumberPosition').val() || 0),
                StartDate: emptyToNull($('#startDate').val()),
                EndDate: emptyToNull($('#endDate').val()),
                options: {}
            };
        },
        dragConfig: {
            zoneSelector: '#dragZone',
            inputs: {
                BankCode: '#bankCodePosition',
                BranchCode: '#branchCodePosition',
                Year: '#yearPosition',
                SerialNumber: '#seriaNumberPosition'
            },
            serialCheckbox: '#acceptSerialNumber',
            numericToggleSelector: '#toggleNumericPositions',       // NEW
            numericContainerSelector: '#numericPositions'           // NEW
        }

    });

    // optional: expose reload function for console / other scripts
    window.reloadNumconfogTable = function () {
        if (numconfogCrud && typeof numconfogCrud.reload === 'function') numconfogCrud.reload();
    };

    // Wire apply/reset buttons in case they exist outside the form element or need extra behavior
    $(document).ready(function () {
        // ensure select2 init if used
        if ($.fn.select2) $('.select2').select2({ width: '100%' });

        // If you need to trigger modal population manually after AddORUpdateGen loads content into datalistingview,
        // call window.NumconfogSyncBeforeSubmit(form) before submission (the script sets it up automatically).
    });

})();
