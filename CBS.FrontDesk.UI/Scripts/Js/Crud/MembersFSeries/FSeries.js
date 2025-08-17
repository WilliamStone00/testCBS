
$(document).ready(function () {
    // Event listener for dropdown change


    // Call this on page load if you want to load the default loans
    //$(document).ready(function () {
    //    applyFilter(); // Load default on page load
    //});

    $('#openModalButton').click(function () {
        $('#searchModal').modal('show'); // Show the modal
    });
    $('#myDataTableT tbody tr').each(function () {
        let balanceInput = $(this).find('.amount-input');
        let blockedInput = $(this).find('.blocked-input');

        let balance = parseFloat(balanceInput.val());
        let blocked = parseFloat(blockedInput.val());

        // Update the input fields with formatted currency
        if (!isNaN(balance)) {
            balanceInput.val(new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'XAF' }).format(balance));
        }
        if (!isNaN(blocked)) {
            blockedInput.val(new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'XAF' }).format(blocked));
        }
    });

});
function parseDateOrNull(v) {
    if (!v) return null;
    // if you use a date-picker plugin, get its formatted value; here we trust yyyy-mm-dd or dd/mm/yyyy
    const parts = v.includes('/') ? v.split('/') : v.split('-');
    // try DD/MM/YYYY first
    let d;
    if (parts.length === 3 && v.includes('/')) {
        d = new Date(parts[2], parts[1] - 1, parts[0]);
    } else {
        d = new Date(v);
    }
    return isNaN(d) ? null : d.toISOString();
}



function clearRefundFilters() {
    $('#refundDateFrom').val('');
    $('#refundDateTo').val('');
}
function loadRefundDetails(refundId) {
    const modalEl = document.getElementById("refundDetailsModal");
    const $body = $("#refundDetailsModalBody");

    const showModal = () => {
        if (window.bootstrap?.Modal) bootstrap.Modal.getOrCreateInstance(modalEl).show();
        else if ($.fn.modal) $("#refundDetailsModal").modal("show");
    };

    const renderSpinner = () => {
        $body.html(`
            <div class="text-center py-4">
                <div class="spinner-border" role="status"></div>
                <div class="small mt-2 text-muted">Loading refund details…</div>
            </div>
        `);
    };

    const renderError = (heading, detail) => {
        $body.html(`
            <div class="alert alert-danger">
                <div class="fw-bold mb-1">${heading}</div>
                <div class="small">${detail || "An unexpected error occurred."}</div>
            </div>
        `);
    };

    // Always open the modal and show spinner immediately
    showModal();
    renderSpinner();

    // Guard: missing id
    if (!refundId) {
        renderError("Missing refund id.", "No refund identifier was provided.");
        return;
    }

    $.ajax({
        url: "/MembersFSeries/RefundDetailsPartial",
        type: "GET",
        cache: false,
        timeout: 15000, // 15s
        data: { id: refundId },
        success: function (html, _status, xhr) {
            // If server redirected to login or returned a full page, show a friendly message
            const text = (html || "").toString();
            const looksLikeFullPage = /<\s*html[\s>]/i.test(text);
            const looksLikeLogin = /login|sign\s*in|account/i.test(text) && looksLikeFullPage;

            if (!text.trim()) {
                renderError("Empty response.", "The server returned no content.");
                return;
            }
            if (looksLikeLogin || xhr.responseURL?.toLowerCase().includes("login")) {
                renderError("Session expired.", "Please sign in again and retry.");
                return;
            }

            // Render the partial as-is
            $body.html(text);
        },
        error: function (xhr, _status, err) {
            if (xhr.status === 0) {
                renderError("Network error.", "Check your internet connection.");
                return;
            }

            // Specific friendly messages
            const map = {
                401: "Unauthorized. Please sign in again.",
                403: "Forbidden. You don’t have access to this refund.",
                404: "Refund not found.",
                408: "Request timed out.",
                409: "Conflict while loading refund.",
                500: "Server error while loading refund."
            };
            const heading = map[xhr.status] || `Error ${xhr.status || ""}`.trim();

            // Include server payload (stack/HTML) collapsed
            const payload = xhr.responseText ? `
                <details class="mt-2"><summary>Details</summary>
                    <pre class="mt-2" style="white-space:pre-wrap;">${xhr.responseText}</pre>
                </details>` : "";

            $body.html(`
                <div class="alert alert-danger">
                    <div class="fw-bold mb-1">${heading}</div>
                    <div class="small">${err || xhr.statusText || "Request failed."}</div>
                    ${payload}
                </div>
            `);
        }
    });
}

function loadMemberRefunds(customerId) {
    const df = $('#refundDateFrom').val() || '';
    const dt = $('#refundDateTo').val() || '';

    $.ajax({
        url: '/MembersFSeries/MemberRefundsPartial',
        type: 'GET',
        data: { customerId: customerId, dateFrom: df, dateTo: dt },
        //beforeSend: function () {
        //    $("#refundsTableContainer").html(`<div class="text-center py-3">
        //        <div class="spinner-border" role="status"></div>
        //        <div class="small mt-2 text-muted">Loading refunds…</div>
        //    </div>`);
        //},
        success: function (html) {
            $("#refundsTableContainer").html(html);
        },
        error: function (xhr) {
            $("#refundsTableContainer").html(`<div class="alert alert-danger">
                Error: ${xhr.status} ${xhr.statusText}
            </div>`);
        }
    });
}

function loadLoanDetails(loanId) {
    const modalEl = document.getElementById('loanDetailsModal');
    const modalBody = modalEl.querySelector('.modal-body');
    const customerNameLabel = document.getElementById('loanCustomerName');
    const loanIdLabel = document.getElementById('loanIdDisplay');

    // Reset modal content and labels
    modalBody.querySelectorAll('.loan-details-container').forEach(el => el.remove());
    customerNameLabel.textContent = '...';
    loanIdLabel.textContent = loanId || '...';

    $.get('/MembersFSeries/LoanDetailsPartial', { loanId: loanId })
        .done(function (html) {
            // Append the loaded HTML
            const container = document.createElement('div');
            container.classList.add('loan-details-container');
            container.innerHTML = html;
            modalBody.appendChild(container);

            // Set customer name if provided in data attribute
            const nameSpan = container.querySelector('[data-loan-customer-name]');
            if (nameSpan) {
                customerNameLabel.textContent = nameSpan.getAttribute('data-loan-customer-name');
            }

            loanIdLabel.textContent = loanId;

            // Show modal
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        })
        .fail(function () {
            modalBody.innerHTML += `
                <div class="alert alert-danger text-center loan-details-container">
                    ❌ Failed to load loan details.
                </div>
            `;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        });
}


function loadLoansPartial(memberId, filter) {
    //$('#loansTableContainer').html('<div class="text-center p-3"><i class="spinner-border text-primary"></i> Loading...</div>');

    $.get('/MembersFSeries/GetFilteredLoansPartial', { memberId: memberId, filter: filter })
        .done(function (html) {
            $('#loansTableContainer').html(html);
        })
        .fail(function () {
            $('#loansTableContainer').html('<div class="alert alert-danger text-center">❌ Failed to load loan data.</div>');
        });
}

function ModalShowUp() {
    $('#searchModal').modal('show')
}
function GetPrintForm(customerId) {
    EditResetModal(customerId, 'modal', 'modalContent', 'MembersFSeries', 'InitializeData', '_PrintDateForm', 'print_by_date', 'Print parameters', 'modalLabel')
}
function GetTransactionMembersTransactions(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("MembersFSeries", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);
}
function ExportIndividaulReport(customerId, path) {
    var dateFrom = $('#DateFrom').val();
    var dateTo = $('#DateTo').val();
    $.ajax({
        url: '/MembersFSeries/GetReport',
        type: 'POST',
        data: {
            datefrom: dateFrom,
            dateto: dateTo,
            KEY: customerId,
            path: path
        },
        success: function (response) {
            if (response.success) {
                appalert("Plaese wait, downloading file", 1);
                window.open("/Reports/ReportWithParameter", "_blank")
            } else {
                if (response.message === undefined) {
                    alert("Your session is expired.");
                } else {
                    appalert(response.message, 3, 1);
                }
            }
        },
        error: function (xhr, status, error) {
            // Handle error
            console.error(xhr.responseText);
        }
    });
}
// Function to fetch and load loans based on the selected filter


let loanTable;

$(document).ready(function () {
    loanTable = $('#myDataTableT2').DataTable({
        paging: false,
        searching: false,
        info: false,
        columns: [
            { data: 'LoanDate' }, // Already formatted from server
            {
                data: 'LoanAmount',
                render: function (data) {
                    return formatXaf(data);
                }
            },
            {
                data: 'InterestRate',
                render: function (data) {
                    return data ? `${data}%` : '0%';
                }
            },
            {
                data: 'Paid',
                render: function (data) {
                    return formatXaf(data);
                }
            },
            {
                data: 'Balance',
                render: function (data) {
                    return `<span class="text-danger">${formatXaf(data)}</span>`;
                }
            },
            { data: 'LoanStatus' }
        ]
    });
});

function loadLoans(memberId) {
    const filter = $('#loanFilterDropdown').val();

    $.ajax({
        url: '/MembersFSeries/GetFilteredLoans',
        type: 'GET',
        data: { filter: filter, memberId: memberId },
        success: function (response) {
            if (Array.isArray(response) && response.length > 0) {
                $('#loansTableContainer').show();
                $('#noLoansMessage').hide();

                let totalPaid = 0;
                let totalBalance = 0;

                response.forEach(loan => {
                    totalPaid += loan.Paid || 0;
                    totalBalance += loan.Balance || 0;
                });

                loanTable.clear().rows.add(response).draw();

                $('#totalPaid').text(formatXaf(totalPaid));
                $('#totalBalance').text(formatXaf(totalBalance));
            } else {
                loanTable.clear().draw();
                $('#loansTableContainer').hide();
                $('#noLoansMessage').text('No loans found.').show();
            }
        },
        error: function () {
            $('#loansTableContainer').hide();
            $('#noLoansMessage').text('Error loading loan data.').show();
        }
    });
}

function formatXaf(value) {
    return typeof value === 'number'
        ? value.toLocaleString('en-CM', { style: 'currency', currency: 'XAF' })
        : '0';
}


function showLoanDetails(loanId) {
    // Fetch loan details using AJAX
    $.ajax({
        type: "GET",
        url: `/MemberOperation/GetLoan?Key=${loanId}`,
        success: function (data) {
            if (data) {
                // Build the loan detail view dynamically
                let loanDetailsHtml = `
                    <div class="row">
                        <div class="col-md-6">
                            <h6>Loan ID:</h6> <p>${data.Id}</p>
                            <h6>Customer Name:</h6> <p>${data.CustomerName}</p>
                            <h6>Loan Amount:</h6> <p>${data.LoanAmount.toLocaleString()}</p>
                            <h6>Principal:</h6> <p>${data.Principal.toLocaleString()}</p>
                            <h6>Interest Rate:</h6> <p>${data.InterestRate}%</p>
                            <h6>Disbursement Date:</h6> <p>${new Date(data.DisbursementDate).toLocaleDateString()}</p>
                        </div>
                        <div class="col-md-6">
                            <h6>Balance:</h6> <p>${data.Balance.toLocaleString()}</p>
                            <h6>Due Amount:</h6> <p>${data.DueAmount.toLocaleString()}</p>
                            <h6>Penalty:</h6> <p>${data.Penalty.toLocaleString()}</p>
                            <h6>Tax:</h6> <p>${data.Tax.toLocaleString()}</p>
                            <h6>Loan Status:</h6> <p>${data.LoanStatus}</p>
                            <h6>Maturity Date:</h6> <p>${new Date(data.MaturityDate).toLocaleDateString()}</p>
                        </div>
                    </div>
                    <hr>
                    <h5>Additional Information</h5>
                    <div class="row">
                        <div class="col-md-12">
                            <p><strong>Loan Type:</strong> ${data.LoanType}</p>
                            <p><strong>Loan Category:</strong> ${data.LoanCategory}</p>
                            <p><strong>Delinquent Status:</strong> ${data.DeliquentStatus || "N/A"}</p>
                            <p><strong>Last Payment:</strong> ${data.LastPayment.toLocaleString()}</p>
                            <p><strong>Advanced Payment Amount:</strong> ${data.AdvancedPaymentAmount.toLocaleString()}</p>
                        </div>
                    </div>
                `;

                // Inject the details into the modal
                $("#loanDetailsContent").html(loanDetailsHtml);

                // Show the modal
                $("#loanDetailModal").modal("show");
            } else {
                alert("No loan details found for the specified loan ID.");
            }
        },
        error: function (err) {
            alert(`Error fetching loan details: ${err.statusText}`);
        }
    });
}

function printAccountsSection() {
    // Get the HTML content of the desired section
    var accountsSection = document.getElementById('accountsSection').innerHTML;

    // Open a new window
    var printWindow = window.open('', '_blank');

    // Write the HTML content to the new window
    printWindow.document.write('<html><head><title>Print</title>');
    printWindow.document.write('<link rel="stylesheet" type="text/css" href="~/Loaders/CSSJS/Spin.css">'); // Link print styles
    printWindow.document.write('</head><body>');
    printWindow.document.write(accountsSection);
    printWindow.document.write('</body></html>');

    // Close the document
    printWindow.document.close();

    // Trigger the print dialog for the new window
    printWindow.print();
}
function GetMembersData(Key, partialView, divToloadPV, path) {


    AddORUpdateGen(Key, divToloadPV, partialView, path, "MembersFSeries");
}

function GetMembersAccounts(Key, partialView, divToloadPV, path) {

    var url = '/MembersFSeries/InitializeData?KEY=' + Key + '&partialView=' + partialView + '&path=' + path;
    $.get(url, function (data) {
        $('#datalistingview').html(data);
        // Call calculateTotalAmounts after the partial view is loaded
        calculateTotalAmounts();
        calculateLoanTotals();
    });

}

function DownloadLoans(path) {
    var datefrom = $("#mdatefromexport").val();
    var dateto = $("#mdatetoexport").val()
    var url = "/Transactions/Download?serviceOption=Loan&dateFrom=" + datefrom + "&dateTo=" + dateto + "&path=" + path + "&readOption=Download";
    DownloadFile(url);

}
function calculateTotalAmounts() {
    // Initialize variables to store total sums
    let totalAccountBalance = 0;
    let totalTableAccountBalance = 0;
    let totalBlockedAmount = 0;

    // Loop through each row in the table
    $('#myDataTableT tbody tr').each(function () {
        // Get account balance and blocked amount from input fields
        let balance = parseFloat($(this).find('.amount-input').val());
        let blocked = parseFloat($(this).find('.blocked-input').val());

        // Update totalAccountBalance by deducting blocked amount
        totalAccountBalance += balance - blocked;

        // Update totalTableAccountBalance
        totalTableAccountBalance += balance;

        // Update totalBlockedAmount
        totalBlockedAmount += blocked;
    });

    // Format amounts in currency of FCFA
    let formattedTotalAccountBalance = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'XAF' }).format(totalAccountBalance);
    let formattedTotalTableAccountBalance = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'XAF' }).format(totalTableAccountBalance);
    let formattedTotalBlockedAmount = new Intl.NumberFormat('fr-FR', { style: 'currency', currency: 'XAF' }).format(totalBlockedAmount);

    // Update the corresponding elements with formatted amounts
    $('#totalAccountBalance').text('Actual account balance: ' + formattedTotalAccountBalance);
    $('#actualAccountBalance').text(formattedTotalAccountBalance);
    $('#tableTotalAccountBalance').text(formattedTotalTableAccountBalance);
    $('#totalBlockedAmount').text(formattedTotalBlockedAmount);

    // Format input fields with currency


}
function calculateLoanTotals() {
    let totalPaid = 0;
    let totalBalance = 0;

    $('#myDataTable tbody tr').each(function () {
        // Parse values from each row
        let paid = parseFloat($(this).find('td:nth-child(7)').text().replace(/,/g, ''));
        let balance = parseFloat($(this).find('td:nth-child(8)').text().replace(/,/g, ''));

        // Update totals
        totalPaid += paid;
        totalBalance += balance;
    });
    // Format totals
    let formattedTotalPaid = totalPaid.toLocaleString('en-US', { maximumFractionDigits: 2 });
    let formattedTotalBalance = totalBalance.toLocaleString('en-US', { maximumFractionDigits: 2 });
    // Update elements with totals
    $('#totalPaid').text(formattedTotalPaid);
    $('#totalBalance').text(formattedTotalBalance);
}




