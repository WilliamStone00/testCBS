document.addEventListener("DOMContentLoaded", function () {
    HandleTransferSourceChange(); // Run on page load to set initial state
});



function HandleTransferSourceChange() {
    // Get Transfer Source values
    var isLocalRemittance = document.getElementById("transferSourceLocal").checked;
    var isInternationalRemittance = document.getElementById("transferSourceInternational").checked;

    // Get Transfer Type section and elements
    var transferTypeSection = document.getElementById("transferTypeSection");
    var localTypes = document.querySelectorAll(".transfer-type.local");
    var internationalTypes = document.querySelectorAll(".transfer-type.international");

    // Get External Reference and International Date elements
    var externalReferenceSection = document.getElementById("externalReferenceSection");
    var externalReferenceInput = document.getElementById("externalReference");
    var internationalDateInput = document.getElementById("internationalTransfterDate");
    // Hide all transfer types initially
    document.querySelectorAll(".transfer-type").forEach(el => el.style.display = "none");

    // Reset Transfer Type radio buttons
    document.querySelectorAll('input[name="TransferType"]').forEach(input => input.checked = false);

    if (isLocalRemittance) {
        // Show only Local Transfer types
        localTypes.forEach(el => el.style.display = "block");
        transferTypeSection.style.display = "block";

        // Default Local Transfer checked
        localTypes[0].querySelector("input").checked = true;

        // Hide and reset External Reference and Date fields
        externalReferenceSection.style.display = "none";
        externalReferenceInput.value = "";
        internationalDateInput.value = "";
    } else if (isInternationalRemittance) {
        // Show only International Transfer types
        internationalTypes.forEach(el => el.style.display = "block");
        transferTypeSection.style.display = "block";

        // Show External Reference and Date fields
        externalReferenceSection.style.display = "block";
    } else {
        // Hide Transfer Type and External Reference sections if none selected
        transferTypeSection.style.display = "none";
        externalReferenceSection.style.display = "none";

        // Reset fields
        externalReferenceInput.value = "";
        internationalDateInput.value = "";
    }
    $("#AccountNumberId").val("")
    // Call existing function
    LoadRemittanceAccountBaseOnTransferSource();
}
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

    // Add event listener to ChargeType radio buttons
    $("input[name='ChargeType']").on("change", function () {
        GetRemittanceCharge(); // Call the function when a radio button is checked
    });
    //// Add event listener to ChargeType radio buttons
    //$("input[name='TransferType']").on("change", function () {
    //    HandleTransferTypeChange(); // Call the function when a radio button is checked
    //});
});

function LoadRemittanceAccountBaseOnTransferSource() {
    var transferTypeValue = $("input[name='TransferSource']:checked").val(); // Get the selected radio button value
    var branchId = $("#BranchId").val(); // Get the selected branch ID

    if (!branchId) {
        appalert("Please select a branch", 3, 1);

        return;
    }

    var url = "/Remittance/Ajaxloader?Key=" + branchId + "&transfterType=" + transferTypeValue;
    FillDropDownAjaxCallParam(url, "RemittanceTypeId", "---Select Option---");
}

function GetBranchRemittanceAccounts(branchid) {
    var transferTypeValue = $("input[name='TransferSource']:checked").val();
    var url = "/Remittance/Ajaxloader?Key=" + branchid + "&transfterType=" + transferTypeValue;
    FillDropDownAjaxCallParam(url, "RemittanceTypeId", "---Select Option---");
}


function GetAccountNumber(accountType) {
    var branchid = $("#BranchId").val();
    $.ajax({
        type: "GET",
        url: '/Remittance/GetRemittanceAccount?branchid=' + branchid + '&accountType=' + accountType,
        success: function (data) {
            // Assuming 'data' is an object containing the loan details
            $('#AccountNumberId').val(data.accountNumber);
            $('#AccountName').html(data.accountName);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}

function GetRemittanceCharge() {
    var accountType = $("#RemittanceTypeId").val(); // Get remittance type
    var amount = $("#initialAmount").val(); // Get initial amount
    var accountNumber = $("#AccountNumberId").val(); // Get account number
    var chargeType = $("input[name='ChargeType']:checked").val(); // Get ChargeType
    var transferType = $("input[name='TransferType']:checked").val(); // Get TransferType

    $.ajax({
        type: "GET",
        url: '/Remittance/GetRemittanceCharge',
        data: {
            accountNumber: accountNumber,
            accountType: accountType,
            amount: amount,
            transferType: transferType,
            chargeType: chargeType
        },
        success: function (data) {
            if (data.success) {
                const formatter = new Intl.NumberFormat('fr-FR', {
                    style: 'currency',
                    currency: 'XAF',
                    minimumFractionDigits: 1,
                    maximumFractionDigits: 1
                });

                // Update form fields
                $('#AmountId').val(data.amount);
                $('#FeeId').val(data.charge);

                // Update the summary table
                $("#feeName").text(data.feeName);
                $("#charge").text(formatter.format(data.charge));
                $("#initalAmountvalue").text(formatter.format(data.initailAmount));
                $("#amountValue").text(formatter.format(data.amount));
                $("#percentageValue").text(data.percentageValue + "%");
                $("#remittanceType").text(data.remittanceType);
                $("#feeType").text(data.feeType);
                $("#globalStatus").text(data.globalStatus);
                $("#serviceCharge").text(formatter.format(data.serviceCharge));
                $("#totalCharges").text(formatter.format(data.totalCharges));

                // Generate transaction summary
                var statement = `
                <div class="alert alert-primary mt-4 p-4">
                    <div class="text-center mb-3">
                        <h5 class="mb-0"><strong>TRANSACTION SUMMARY</strong></h5>
                        <hr class="mt-2" style="border-top: 2px solid #ffffff;">
                    </div>
                    <div>
                        <p class="mb-2"><strong>1.</strong> Total amount to be paid by the sender: <strong>${formatter.format(data.totalCharges)}</strong>.</p>
                        <p class="mb-2"><strong>2.</strong> Total charge: <strong>${formatter.format(data.charge)}</strong>.</p>
                        <p class="mb-2"><strong>3.</strong> Amount to be received by the receiver: <strong>${formatter.format(data.amount)}</strong>.</p>
                    </div>
                    <hr class="mt-4" style="border-top: 1px dashed #ffffff;">
                    <div class="text-end mt-3">
                        <p class="mb-0"><em>Transaction Manager:</em></p>
                        <p class="mb-0"><strong>${data.globalStatus}</strong></p>
                        <p class="mb-0">Trust Soft Credit</p>
                    </div>
                </div>
                `;
                $("#transactionSummary").html(statement);

                $("#chargesdiv").show(); // Show the table
            } else {
                resetFormData(); // Reset data on failure
                appalert(data.message || "Failed to fetch remittance charges. Please try again.", 2, 1);
            }
        },
        error: function (err) {
            resetFormData(); // Reset data on error
            appalert("Error: " + err.statusText, 1, 3);
        }
    });

    function resetFormData() {
        // Reset form fields
        $('#AmountId, #FeeId').val(0);

        // Clear summary fields
        $("#feeName, #charge, #initalAmountvalue, #amountValue, #percentageValue, #remittanceType, #feeType, #globalStatus, #serviceCharge, #totalCharges").text('');

        // Clear transaction summary
        $("#transactionSummary").html('');

        // Hide table
        $("#chargesdiv").hide();
    }
}



