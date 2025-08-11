
function buildTransferConfirmationDialog(data) {
    return `
                        <div class="text-start">
                            <p class="mb-2">
                                <i class="mdi mdi-transfer-check me-1 text-tsc"></i>
                                You are about to <strong>${data.Status.toUpperCase()}</strong> the selected transfer request.
                            </p>

                            <p class="mb-2">
                                <strong>Transfer ID:</strong> <code>${data.TransferId}</code><br/>
                                <strong>Status:</strong> ${data.Status}
                            </p>

                            <div class="border rounded bg-light mb-3 p-0">
                                <div class="bg-secondary bg-opacity-10 px-3 py-2 border-bottom fw-bold text-dark">
                                    <i class="mdi mdi-cash-multiple me-1"></i> Transfer Summary
                                </div>
                                <table class="table table-sm table-bordered m-0">
                                    <thead class="table-light">
                                        <tr>
                                            <th>Amount</th>
                                            <th>Fee</th>
                                            <th class="text-primary">Total Amount</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td>${data.Amount.toLocaleString('en-US')} FCFA</td>
                                            <td>${data.Fee.toLocaleString('en-US')} FCFA</td>
                                            <td class="fw-bold text-dark">${data.Total.toLocaleString('en-US')} FCFA</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>

                            <p class="mb-2">
                                <strong>Your Note + System Log:</strong><br/>
                                <span class="text-dark border rounded bg-white d-block p-2 white-space-pre-line">${data.DisplayNote}</span>
                            </p>

                            <hr/>
                            <p class="text-danger mb-0">
                                <i class="mdi mdi-alert-circle-outline me-1"></i>
                                This action cannot be reversed once submitted. Please confirm to continue.
                            </p>
                        </div>
                    `;
}

function buildTransferApprovalPayload(transferId, status, userNote) {
    const approverName = $("#accountantName").val() || "System User";
    const approvalTime = new Date().toLocaleString("en-GB", {
        weekday: 'short', year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });

    const amount = parseFloat($("#summaryTransferAmount")?.text()?.replace(/[^\d.]/g, '')) || 0;
    const fee = parseFloat($("#summaryTransferFee")?.text()?.replace(/[^\d.]/g, '')) || 0;
    const total = amount + fee;

    const autoNote = `[${status.toUpperCase()} by: ${approverName} on ${approvalTime}]`;
    const fullNote = `${userNote}\n\n${autoNote}`;

    return {
        TransferId: transferId,
        Status: status,
        Note: fullNote,
        DisplayNote: fullNote, // for UI rendering
        Amount: amount,
        Fee: fee,
        Total: total
    };
}

function submitTransferApproval(data) {
    $.ajax({
        url: "/AccountToAccountTransfer/ApproveTransfer",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            TransferId: data.TransferId,
            Status: data.Status,
            Note: data.Note
        }),
        success: function (response) {
            if (response.success) {
                appalert(`✅ ${response.message}`, 1, 1);
                setTimeout(() => location.reload(), 1200);
                Reprint(response.redirectUrl);
            } else {
                appalert("❌ " + (response.message || `Failed to ${data.Status.toLowerCase()} transfer.`), 2, 1);
            }
        },
        error: function () {
            appalert(`❌ Network error while trying to ${data.Status.toLowerCase()} the transfer.`, 3, 1);
        }
    });
}

function Reprint(redirectUrl) {
    window.open(redirectUrl, '_blank');
    ReportView("AccountToAccountTransfer", null, "GetReport", null, null, "receipts", "ReportParameterLessWithSubReports");
}

function openTransferModal(action, id, title, path, partialView) {
    // Set modal title
    $("#transferModalTitle").text(title);
    $("#transferModalDescription").text("Loading transfer details...");

    // Show loader
    $("#transferLoader").removeClass("d-none");

    // Clear previous body
    $("#transferModalBody").html("");
    $("#transferModalFooter").addClass("d-none").html("");

    // Show modal
    $("#transferModal").modal("show");

    // Load partial view from server
    $.get(`/AccountToAccountTransfer/${action}`, {
        KEY: id,
        path: path,
        partialView: partialView
    }).done(function (html) {
        $("#transferModalBody").html(html);
        $("#transferModalDescription").text("Review the request and take action.");
    }).fail(function () {
        $("#transferModalBody").html(`
                                    <div class="alert alert-danger">
                                        <i class="mdi mdi-alert-circle-outline me-1"></i>
                                        Failed to load content. Please try again later.
                                    </div>
                                `);
        $("#transferModalDescription").text("An error occurred while loading.");
    }).always(function () {
        $("#transferLoader").addClass("d-none");
    });
}
function approveOrRejectTransfer(status) {
    const transferId = $("#approveTransferId").val();
    const userNote = $("#ValidatorComment").val()?.trim();

    if (!transferId) return appalert("❌ Transfer ID is missing.", 2, 1);
    if (!userNote || userNote.length < 5)
        return appalert(`❌ Please enter a valid ${status.toLowerCase()} note (min 5 characters).`, 2, 1);

    const payload = buildTransferApprovalPayload(transferId, status, userNote);
    const confirmationHtml = buildTransferConfirmationDialog(payload);

    alertify.confirm(
        `🚦 Confirm Transfer ${status}`,
        confirmationHtml,
        () => submitTransferApproval(payload),
        () => appalert("🚫 Operation cancelled by user.", 2, 1)
    ).set('labels', { ok: 'Yes, Confirm', cancel: 'Cancel' });
}
function printTransferSummary() {
    const printContents = document.getElementById("printableTransferSummary").innerHTML;

    const serviceTitle = sessionStorage.getItem("serviceTitle") || "Account to Account Transfer Summary";
    const printedBy = document.getElementById("accountantName")?.value || "System User";
    const branch = document.getElementById("branchName")?.value || "Main Branch";
    const printDate = new Date().toLocaleString("en-GB", {
        weekday: 'short', year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });
    const currentYear = new Date().getFullYear();

    const html = `
        <html>
        <head>
            <title>${serviceTitle}</title>
            <link href="https://cdn.materialdesignicons.com/5.4.55/css/materialdesignicons.min.css" rel="stylesheet">
            <style>
                @@media print {
                    @@page {
                        size: A4 portrait;
                        margin: 10mm 12mm;
                    }
                }

                body {
                    font-family: 'Segoe UI', Tahoma, sans-serif;
                    font-size: 13px;
                    color: #000;
                    margin: 0;
                    padding: 0;
                    line-height: 1.4;
                }

                .container {
                    max-width: 700px;
                    margin: auto;
                }

                .header {
                    text-align: center;
                    padding: 10px 0 5px;
                    border-bottom: 1px solid #026937;
                }

                .header h2 {
                    margin: 0;
                    font-size: 18px;
                    color: #026937;
                    text-transform: uppercase;
                }

                .header .meta {
                    font-size: 11px;
                    color: #333;
                    margin-top: 2px;
                }

                .report-title {
                    margin: 15px 0;
                    font-size: 14px;
                    font-weight: bold;
                    color: #026937;
                    text-align: center;
                    text-decoration: underline;
                }

                .content {
                    padding: 0 5px;
                }

                table {
                    border-collapse: collapse;
                    width: 100%;
                    margin-top: 10px;
                    font-size: 12px;
                }

                th, td {
                    border: 1px solid #ccc;
                    padding: 6px 8px;
                    vertical-align: top;
                }

                th {
                    background-color: #f5f5f5;
                    text-align: left;
                }

                .section-header {
                    background-color: #e0f2f1;
                    color: #014F2A;
                    font-weight: bold;
                }

                .footer {
                    font-size: 10.5px;
                    color: #555;
                    text-align: center;
                    border-top: 1px solid #ccc;
                    margin-top: 25px;
                    padding-top: 5px;
                }
            </style>
        </head>
        <body onload="window.print(); setTimeout(() => window.close(), 500);">

            <div class="container">
                <div class="header">
                    <h2>Trust Soft Credit</h2>
                    <div class="meta">
                        Printed by: <strong>${printedBy}</strong> |
                        Branch: <strong>${branch}</strong> |
                        Date: <strong>${printDate}</strong>
                    </div>
                </div>

                <div class="report-title">${serviceTitle}</div>

                <div class="content">
                    ${printContents}
                </div>

                <div class="footer">
                    Powered by <strong>Flux SARL</strong> © 2023 - ${currentYear}. All rights reserved.<br/>
                    Generated by <strong>Trust Soft Credit</strong>
                </div>
            </div>
        </body>
        </html>
    `;

    const win = window.open('', '_blank', 'width=900,height=1100');
    win.document.write(html);
    win.document.close();
}



// =====================
// Transfers DataTable
// =====================
var transfersDt;

// -------- Filters --------
function collectTransferFilters() {
    return {
        DataTableOptions: {
            draw: "1",
            start: 0,
            length: 10,
            searchValue: "",
            sortColumnName: "DateOfInitiation",
            sortColumnDirection: "desc",
            skip: 0
        },
        BranchId: $('#branchInput').val() || null,
        SourceAccountNumber: $('#sourceAccountNumber').val() || null,
        DestinationAccountNumber: $('#destinationAccountNumber').val() || null,
        TransactionRef: $('#transactionRef').val() || null,
        TransactionType: $('#transactionType').val() || null,
        Status: $('#status').val() || null,
        StartDate: $('#startDate').val() ? new Date($('#startDate').val()).toISOString() : null,
        EndDate: $('#endDate').val() ? new Date($('#endDate').val()).toISOString() : null,
        IsInterBranchOperation: $('#isInterBranch').is(':checked') ? true : null,
        SourceCustomerId: $('#sourceCustomerId').val() || null,
        DestinationCustomerId: $('#destinationCustomerId').val() || null,
        SendingMemberReference: $('#sendingMemberReference').val() || null,
        ReceivingCustomerReference: $('#receivingCustomerReference').val() || null,
        Initiator: $('#initiator').val() || null
    };
}


// -------- Actions (btn-group exactly like your Razor) --------
function renderTransferActions(row) {
    const id = row.Id;
    const s = (row.Status || '').toLowerCase();

    // Details always
    let html = `
    <div class="btn-group" role="group">
      <button class="btn btn-sm btn-outline-info" title="Details"
        onclick="openTransferModal('InitializeData','${id}','Details','details','_TransferDetails')">
        <i class="mdi mdi-eye-outline"></i>
      </button>
  `;

    if (s === 'pending') {
        // Approve + Reject (open same approve partial; modal buttons will call approveOrRejectTransfer)
        html += `
      <button class="btn btn-sm btn-outline-success" title="Approve"
        onclick="openTransferModal('InitializeData','${id}','Approve or Reject Request','details','_TransferApprove')">
        <i class="mdi mdi-check-bold"></i>
      </button>
    `;
    }
    else if (s === 'approved' || s === 'completed') {
        // Reverse
        html += `
      <button class="btn btn-sm btn-outline-dark" title="Reverse"
        onclick="openTransferModal('InitializeData','${id}','Reverse Transfer','details','_TransferReverse')">
        <i class="mdi mdi-undo"></i>
      </button>
    `;
    }

    html += `</div>`;
    return html;
}

// -------- Table --------
function loadTransfersTable(reset) {
    if (transfersDt && reset) {
        transfersDt.destroy();
        $('#myDataTable tbody').empty(); // <- make sure your table has id="myDataTable"
    }

    transfersDt = $('#myDataTable').DataTable({
        serverSide: true,
        processing: false,           // per your request
        destroy: true,
        searching: false,
        order: [[0, 'desc']],        // Date
        pageLength: 10,
        ajax: {
            url: '/AccountToAccountTransfer/LoadTransferData',
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: function (d) {
                const filters = collectTransferFilters();

                filters.DataTableOptions.draw = d.draw?.toString() || '1';
                filters.DataTableOptions.start = d.start || 0;
                filters.DataTableOptions.length = d.length || 10;
                filters.DataTableOptions.skip = d.start || 0;

                const colIndex = d.order?.[0]?.column ?? 0;
                const dir = d.order?.[0]?.dir ?? 'desc';
                const colName = d.columns?.[colIndex]?.data || 'DateOfInitiation';
                filters.DataTableOptions.sortColumnName = colName;
                filters.DataTableOptions.sortColumnDirection = dir;

                filters.DataTableOptions.searchValue = d.search?.value || '';
                return JSON.stringify(filters);
            },
            dataSrc: function (json) {
                $('#transferDataCard').show(); // show results card after first response
                return json?.data || [];
            },
            error: function (xhr) {
                console.error('LoadTransferData failed:', xhr.status, xhr.responseText);
            }
        },
        columns: [
            { data: 'DateOfInitiation', render: v => (v ? moment(v).format('DD/MM/YYYY HH:mm') : '') },
            { data: 'SenderName' },
            { data: 'RecieverName' },  // keep your DTO spelling
            { data: 'Amount', render: $.fn.dataTable.render.number(',', '.', 0) },
            {
                data: 'Status',
                render: function (v) {
                    const s = (v || '').toLowerCase();
                    let cls = 'bg-secondary';

                    if (s === 'approved' || s === 'completed') {
                        cls = 'bg-success';
                    } else if (s === 'pending') {
                        cls = 'bg-warning text-dark'; // exactly like your Razor: "bg-warning text-dark"
                    } else if (s === 'rejected' || s === 'failed') {
                        cls = 'bg-danger';
                    }

                    return `<span class="badge ${cls}">${v || ''}</span>`;
                }
            },
            { data: null, orderable: false, render: (_d, _t, row) => renderTransferActions(row) }
        ]
    });
}

// -------- Wire up basic UI --------
$(document).ready(function () {
    if ($.fn.select2) { $('#branchInput').select2({ width: '100%' }); }

    // toggles for extra filters (optional)
    $('#byBranch').on('change', function () { $('#branchFilterSection').toggle(this.checked); });
    $('#byDate').on('change', function () { $('#dateRangeSection').toggle(this.checked); });
    $('#moreOptions').on('change', function () { $('#extraFiltersSection').toggle(this.checked); });

    // actions
    $('#applyFilterBtn').on('click', function (e) { e.preventDefault(); loadTransfersTable(true); });
    $('#resetFilterBtn').on('click', function (e) {
        e.preventDefault();
        $('#branchInput, #startDate, #endDate, #sourceAccountNumber, #destinationAccountNumber, #transactionRef, #transactionType, #status, #sourceCustomerId, #destinationCustomerId, #sendingMemberReference, #receivingCustomerReference, #initiator').val('');
        $('#isInterBranch, #byBranch, #byDate, #moreOptions').prop('checked', false).trigger('change');
        loadTransfersTable(true);
    });
    $('#exportBtn').on('click', function (e) { e.preventDefault(); /* hook custom export if needed */ });

});


