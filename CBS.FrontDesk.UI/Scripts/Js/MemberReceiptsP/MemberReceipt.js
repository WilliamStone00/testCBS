$(document).ready(function () {
    //loadMemberReceipts();

    $('#applyFilterBtn').click(function () {
        loadMemberReceipts();
    });

    $('#resetFilterBtn').click(function () {
        resetFilters();
    });

    $('#exportBtn').click(function () {
        exportMemberReceipts();
    });
});
$(function () {
    $('#showTransactionFilter').on('change', function () {
        $('#transactionFilterSection').toggle(this.checked);
    });

    $('#showDateFilter').on('change', function () {
        $('#dateFilterSection').toggle(this.checked);
    });

    // Initialize based on checkbox state
    $('#showTransactionFilter').trigger('change');
    $('#showDateFilter').trigger('change');
});
function loadMemberReceipts() {
    $('#myDataTable').DataTable({
        serverSide: true,
        destroy: true,
        processing: false,
        searching: false,
        order: [[4, 'desc']],
        ajax: {
            url: '/MemberReceipt/LoadDatatable',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const filters = collectReceiptFilterParams();
                filters.options.draw = d.draw;
                filters.options.start = d.start;
                filters.options.length = d.length;
                filters.options.skip = d.start;
                filters.options.pageSize = d.length;
                filters.options.sortColumnName = d.columns[d.order[0].column].data;
                filters.options.sortColumnDirection = d.order[0].dir;
                return JSON.stringify(filters);
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'MemberName' },
            { data: 'MemberReference' },
            {
                data: 'TotalAmount',
                render: function (data) {
                    return `${parseFloat(data).toLocaleString()}`;
                }
            },
            {
                data: 'Date',
                render: function (data) {
                    return moment(data).format('DD/MM/YYYY HH:mm');
                }
            },
            {
                data: 'Id',
                orderable: false,
                render: function (id) {
                    return `<button onclick="viewReceipt('${id}')" class='btn btn-sm btn-outline-info'>
            <i class='mdi mdi-receipt-outline'></i> View
        </button>`;
                }
            }
        ]
    });
}
function viewReceipt(receiptId) {
    $.ajax({
        url: `/MemberReceipt/ViewReceipt/${receiptId}`,
        type: 'GET',
        success: function (html) {
            $('#receiptModalContainer').html(html);
            $('#viewReceiptModal').modal('show');
        },
        error: function () {
            alert("❌ Failed to load the receipt.");
        }
    });
}


function collectReceiptFilterParams() {
    return {
        options: { searchValue: '' },
        branchId: $('#branchInput').val(),
        memberReference: $('#memberReference').val(),
        transactionReference: $('#transactionReference').val(),
        memberName: $('#memberName').val(),
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val()
    };
}

function resetFilters() {
    $('#branchInput').val('').trigger('change');
    $('#memberReference').val('');
    $('#transactionReference').val('');
    $('#memberName').val('');
    $('#startDate').val('');
    $('#endDate').val('');
    loadMemberReceipts();
}

function exportMemberReceipts() {
    const url = `/MemberReceipt/Download?branchId=${$('#branchInput').val()}&memberReference=${$('#memberReference').val()}&transactionReference=${$('#transactionReference').val()}&memberName=${$('#memberName').val()}&startDate=${$('#startDate').val()}&endDate=${$('#endDate').val()}`;
    window.location.href = url;
}
