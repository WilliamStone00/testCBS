// Load Members into DataTable
function loadMemberData() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        searching: false,
        order: [[0, 'desc']],
        ajax: {
            url: '/Individual/LoadMembersData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectMemberExportParams();

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
            }
        },
        columns: [
            {
                data: 'CreateDate',
                name: 'CreateDate',
                render: function (data) {
                    return moment(data).format('DD/MM/YYYY HH:mm:ss');
                }
            },
            { data: 'FullName', name: 'FullName' },
            { data: 'CustomerId', name: 'CustomerId' },
            { data: 'Phone', name: 'Phone' },
            {
                data: 'CustomerType',
                name: 'CustomerType',
                render: function (data, type, row) {
                    const label = data || '';
                    const isApproved = row.MembershipApprovalStatus === 'Approved';
                    const badgeClass = isApproved ? 'bg-primary' : 'bg-warning text-dark';
                    return `<span class="badge ${badgeClass}">${label}</span>`;
                }
            },
            {
                data: null,
                orderable: false,
                render: function (_, __, row) {
                    return `
        <div class="text-center dropdown">
            <button class="btn btn-sm btn-info dropdown-toggle" type="button" data-bs-toggle="dropdown">
                <i class="mdi mdi-eye"></i> Action
            </button>
            <ul class="dropdown-menu">
                <li>
                    <a class="dropdown-item" href='/Individual/CustomerProfile?KEY=${row.CustomerId}' target="_blank">
                       <i class="mdi mdi-account-circle"></i> Profile
                    </a>
                </li>
                <li>
                   <a class="dropdown-item text-warning" href="#"
                      onclick="openMemberNameAdjustmentModal('${row.CustomerId}')">
                       <i class="mdi mdi-pencil-outline me-1"></i> Initiate Member Name Adjustment
                    </a>
                </li>
                <li>
                   <a class="dropdown-item text-warning" href="#"
                      onclick="openMemberStatusAdjustmentModal('${row.CustomerId}')">
                       <i class="mdi mdi-pencil-outline me-1"></i> Initiate Member Status Adjustment
                    </a>
                </li>
                <li>
                   <a class="dropdown-item text-warning" href="#"
                      onclick="openMemberMemberAccountAdjustmentModal('${row.CustomerId}')">
                       <i class="mdi mdi-pencil-outline me-1"></i> Initiate Member Account Balance Adjustment
                    </a>
                </li>
            </ul>
        </div>`;
                }
            }
        ]
    });
}

function collectMemberExportParams() {
    return {
        CustomerId: $('#customerId').val(),
        FirstName: $('#firstName').val(),
        LastName: $('#lastName').val(),
        BranchId: $('#branchId').val(),
        Gender: $('#gender').val(),
        MaritalStatus: $('#maritalStatus').val(),
        WorkingStatus: $('#workingStatus').val(),
        MembershipApprovalStatus: $('#membershipApprovalStatus').val(),
        LegalForm: $('#legalForm').val(),
        CustomerType: $('#customerType').val(),
        AgeCategoryStatus: $('#ageCategoryStatus').val(),
        DateOfBirthFrom: $('#dobFrom').val(),
        DateOfBirthTo: $('#dobTo').val(),
        CreatedFrom: $('#createdFrom').val(),
        CreatedTo: $('#createdTo').val(),
        ShowAll: false,
        options: {}
    };
}

function openMemberNameAdjustmentModal(key) {
    console.log('Opening modal for member:', key);

    // Show loader and clear previous content
    $('#memberAdjustmentLoader').removeClass('d-none');
    console.log(modal);

    $.get(`/MemberAdjustmentConsole/GetMemberNamesDetailPartial?id=${key}`)
        .done(function (html) {
            console.log('Received HTML:', html);
            $('#memberNameAdjustmentsContainer').html(html);

            // Show the modal
            $('#memberNameAdjustmentModal').modal('show');
        })
        .fail(function (xhr, status, error) {
            console.error('Error loading partial:', error);
            appAlertError("Failed to load member details. Please try again.");
        })
        .always(function () {
            $('#memberAdjustmentLoader').addClass('d-none');
        });
}


function openMemberStatusAdjustmentModal(key) {
    console.log('Opening modal for member:', key);

    // Show loader and clear previous content
    $('#memberAdjustmentLoader').removeClass('d-none');
  
    $.get(`/MemberAdjustmentConsole/GetMemberNamesDetailPartial?id=${key}`)
        .done(function (html) {
            console.log('Received HTML:', html);
            $('#memberNameAdjustmentsContainer').html(html);

            // Show the modal
            $('#memberNameAdjustmentModal').modal('show');
        })
        .fail(function (xhr, status, error) {
            console.error('Error loading partial:', error);
            appAlertError("Failed to load member details. Please try again.");
        })
        .always(function () {
            $('#memberAdjustmentLoader').addClass('d-none');
        });
}

function openMemberMemberAccountAdjustmentModal(key) {
    console.log('Opening modal for member:', key);

    // Show loader and clear previous content
    $('#memberAdjustmentLoader').removeClass('d-none');
    // Ensure modal is properly initialized
    console.log(modal);

    $.get(`/MemberAdjustmentConsole/GetMemberNamesDetailPartial?id=${key}`)
        .done(function (html) {
            console.log('Received HTML:', html);
            $('#memberNameAdjustmentsContainer').html(html);

            // Show the modal
            $('#memberNameAdjustmentModal').modal('show');
        })
        .fail(function (xhr, status, error) {
            console.error('Error loading partial:', error);
            appAlertError("Failed to load member details. Please try again.");
        })
        .always(function () {
            $('#memberAdjustmentLoader').addClass('d-none');
        });
}
/*
function openMemberNameAdjustmentModal(mode, model) {
    // Reset form & errors
    $('#memberNameAdjustmentModal form')[0].reset();
    $('.text-danger').text('');

    // 🧾 Hidden POST fields
    $('#LoanId').val(model.LoanId);
    $('#CustomerId').val(model.CustomerId);
    $('#BranchId').val(model.BranchId);

    // 👤 Footer summary
    $('#footerCustomerName').text(model.CustomerName || 'N/A');
    $('#footerCustomerId').text(model.CustomerId || 'N/A');
    $('#footerLoanId').text(model.LoanId || 'N/A');

    // 💼 Old values – Display
    $('#oldLoanAmountLabel').val(formatCurrency(model.LoanAmount ?? 0));
    $('#oldBalanceLabel').val(formatCurrency(model.Balance ?? 0));
    $('#oldInterestLabel').val(formatCurrency(model.AccrualInterest ?? 0));
    $('#oldVatLabel').val(formatCurrency(model.Tax ?? 0));
    $('#oldPenaltyLabel').val(formatCurrency(model.Penalty ?? 0));
    $('#oldDueAmountLabel').val(formatCurrency(model.DueAmount ?? 0));
    $('#oldVatRateLabel').val(model.VatRate ?? 0);
    $('#oldIntRateLabel').val(model.InterestRate ?? 0); // ✅ Fix ID name


    // 🆕 Pre-fill new fields
    $('#NewLoanAmount').val(model.LoanAmount ?? 0);
    $('#NewBalance').val(model.Balance ?? 0);
    $('#NewInterest').val(model.AccrualInterest ?? 0);
    $('#NewVat').val(model.Tax ?? 0);
    $('#NewPenalty').val(model.Penalty ?? 0);
    $('#newDueAmount').val(model.DueAmount ?? 0);
    $('#NewVatRate').val(model.VatRate ?? 0);
    $('#NewIntRate').val(model.InterestRate ?? 0);

    // 🎯 Control buttons
    $('#submitRequestBtn').toggleClass('d-none', mode !== 'request');
    $('#approveRequestBtn, #rejectRequestBtn').toggleClass('d-none', mode !== 'validation');

    // 🪟 Show modal
    $('#memberNameAdjustmentModal').modal('show');
}
*/
function exportMemberData() {
    const filters = collectMemberExportParams();
    const params = new URLSearchParams(filters).toString();
    window.location.href = `/Individual/DownloadMembers?${params}`;
}


function resetMemberFilters() {
    $('#memberFilterForm').trigger('reset');
    $('.select2').val('').trigger('change');
    $('#demographicFilters, #approvalFilters, #dateFilters').addClass('d-none');
    loadMemberData();
}

$(document).ready(function () {
    //loadMemberData();

    $('#applyFilterBtn').on('click', function (e) {
        e.preventDefault();
        loadMemberData();
    });

    $('#resetFilterBtn').on('click', function (e) {
        e.preventDefault();
        resetMemberFilters();
    });

    $('#exportBtn').on('click', function (e) {
        e.preventDefault();
        exportMemberData();
    });
});





