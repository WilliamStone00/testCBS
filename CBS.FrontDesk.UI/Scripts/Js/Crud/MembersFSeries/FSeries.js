
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
function loadLoans(memberId) {
    var filter = $('#loanFilterDropdown').val();

    console.log('Filter:', filter, 'Member ID:', memberId);

    $.ajax({
        url: '/MembersFSeries/GetFilteredLoans',
        type: 'GET',
        data: { filter: filter, memberId: memberId },
        success: function (response) {
            console.log('Response:', response);

            $('#myDataTable tbody').empty();  // Clear the table rows

            // Check if response has loans
            if (Array.isArray(response) && response.length > 0) {
                // Show the table and hide the "no loans" message
                $('#loansTableContainer').show();
                $('#noLoansMessage').hide();

                var totalPaid = 0;
                var totalBalance = 0;

                // Loop through the loans and append to the table
                response.forEach(function (loan) {
                    var loanDate = loan.LoanDate || 'N/A';
                    var principal = typeof loan.Principal === 'number' ? loan.Principal.toLocaleString() : '0';
                    var interestRate = loan.InterestRate || '0';
                    var accrualInterest = typeof loan.AccrualInterest === 'number' ? loan.AccrualInterest.toLocaleString() : '0';
                    var penalty = typeof loan.Penalty === 'number' ? loan.Penalty.toLocaleString() : '0';
                    var tax = typeof loan.Tax === 'number' ? loan.Tax.toLocaleString() : '0';
                    var paid = typeof loan.Paid === 'number' ? loan.Paid.toLocaleString() : '0';
                    var balance = typeof loan.Balance === 'number' ? loan.Balance.toLocaleString() : '0';
                    var loanAmount = typeof loan.LoanAmount === 'number' ? loan.LoanAmount.toLocaleString() : '0';
                    var dueAmount = typeof loan.DueAmount === 'number' ? loan.DueAmount.toLocaleString() : '0';

                    var row = `
                        <tr>
                            <td>${loanDate}</td>
                            <td>${loanAmount}</td>
                            <td>${principal}</td>
                            <td>${interestRate}</td>
                            <td>${accrualInterest}</td>
                            <td>${penalty}</td>
                            <td>${tax}</td>
                            <td>${paid}</td>
                            <td>${balance}</td>
                            <td>${dueAmount}</td>
                            <td><a href="/Loan/Details?KEY=${loan.Id}" target="_blank">Details</a></td>
                        </tr>`;

                    $('#myDataTable tbody').append(row);

                    totalPaid += loan.Paid || 0;
                    totalBalance += loan.Balance || 0;
                });

                // Update totals
                $('#totalPaid').text(totalPaid.toLocaleString());
                $('#totalBalance').text(totalBalance.toLocaleString());
            } else {
                // If no loans, hide the table and show the message
                $('#loansTableContainer').hide();
                $('#noLoansMessage').show();
            }

            //// Reinitialize the DataTable only if there is data
            //if ($.fn.DataTable.isDataTable('#myDataTable')) {
            //    $('#myDataTable').DataTable().clear().destroy();
            //}
            //// Initialize DataTable
            //if (response.length > 0) {
            //    $('#myDataTable').DataTable({
            //        "paging": true,
            //        "ordering": true,
            //        "info": true,
            //        "searching": true,
            //        "autoWidth": false
            //    });
            //}
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data:', xhr.responseText, 'Status:', status, 'Error:', error);
            $('#loansTableContainer').hide();
            $('#noLoansMessage').text('Error fetching data').show();  // Show error message
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




