
    var numConfigTable = null;

    function emptyToNull(v) {
            return (v === undefined || v === null || v === '') ? null : v;
        }

    function collectNumConfigFilters() {
            return {
        Name: emptyToNull($('#name').val()),
    BranchId: emptyToNull($('#branchId').val()),
    BankCodePosition: parseInt($('#bankCodePosition').val() || 0),
    BranchCodePosition: parseInt($('#branchCodePosition').val() || 0),
    YearPosition: parseInt($('#yearPosition').val() || 0),
    AcceptSerialNumber: $('#acceptSerialNumber').is(':checked') ? true : null,
    SerialNumberPosition: parseInt($('#serialNumberPosition').val() || 0),
    StartDate: emptyToNull($('#startDate').val()),
    EndDate: emptyToNull($('#endDate').val()),
    options: { }
            };
        }

    function loadNumConfigData() {
        numConfigTable = $('#myDataTable').DataTable({
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

                    return JSON.stringify(filters);
                },
                error: function () {
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
                                       data-toggle="tooltip" title="Edit">
                                       <i class="mdi mdi-pencil"></i> Edit
                                    </a>
                                    <a href="#"
                                       class="btn btn-sm btn-outline-danger"
                                       data-toggle="tooltip" title="Delete">
                                       <i class="mdi mdi-trash-can"></i> Delete
                                    </a>
                                `;
                    }
                }
            ],
            language: {
                emptyTable: 'No number configurations found'
            },
            drawCallback: function () {
                if (typeof ($.fn.tooltip) !== 'undefined') {
                    $('[data-toggle="tooltip"]').tooltip({ container: 'body' });
                }
            }
        });
        }

        // onclick="AddORUpdateGen('${id}', 'datalistingview', '_Create', 'get', 'NumConfig')"

        //  onclick="DeleteRecordDataTable('NumConfig', '${id}', 'myDataTable', '_DataTable', 0, 'datalistingview', null, 'list')"

    function exportNumConfigData() {
            const filters = collectNumConfigFilters();
    const params = new URLSearchParams(filters).toString();
    window.location.href = `/NumConfig/Download?${params}`;
        }

    function resetNumConfigFilters() {
        $('#numconfigFilterForm').trigger('reset');

    if ($.fn.select2 && $('#branchId').hasClass('select2-hidden-accessible')) {
        $('#branchId').val(null).trigger('change');
            }

    $('#branchFilter, #dateFilters').addClass('d-none');
    $('#acceptSerialNumber').prop('checked', false);

            //loadNumConfigData();
        }

    $(document).ready(function () {
        $('#applyFilterBtn').on('click', function (e) {
            e.preventDefault();
            loadNumConfigData();
        });

    $('#resetFilterBtn').on('click', function (e) {
        e.preventDefault();
    resetNumConfigFilters();
            });

    $('#exportBtn').on('click', function (e) {
        e.preventDefault();
    exportNumConfigData();
            });

    if ($.fn.select2) {
                try {
        $('#branchId').select2({
            width: '100%',
            placeholder: 'Select Branch'
        });
                } catch (err) {
        console.warn('select2 init failed', err);
                }
            }

    //loadNumConfigData();

    $('#numconfigFilterForm input').on('keypress', function (e) {
                if (e.which === 13) {
        e.preventDefault();
    loadNumConfigData();
                }
            });
        });
