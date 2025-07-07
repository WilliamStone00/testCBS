document.addEventListener("DOMContentLoaded", function () {
    initializeTransferUI();
    $('#receiverSearchModal').on('hidden.bs.modal', function () {
        // Clear input and feedback
        $('#receiverSearchInput').val('');
        $('#receiverSearchFeedback').text('');

        // Hide and reset receiver info
        $('#receiverInfoSection').hide();
        $('#receiverAccountBody').empty();
        $('#receiverFullName').text('');
        $('#receiverBranchName').text('');
        $('#receiverBranchCode').text('');

        // Reset button/spinner states
        $('#receiverSearchBtnText').removeClass('d-none');
        $('#receiverSearchSpinner').addClass('d-none');
    });
});

function changeReceiver() {
    // Reset summary and selection
    $('#receiverSummarySection').addClass('d-none');
    $('#summaryReceiverName').text('');
    $('#summaryReceiverBranch').text('');
    $('#summaryReceiverAccount').text('');
    $('#ReceiverAccountNumber').val('');

    // Toggle buttons
    $('#confirmTransferBtn').addClass('d-none');
    $('#searchReceiverBtn').removeClass('d-none');

    // Reopen modal
    $('#receiverSearchModal').modal('show');
}


function initializeTransferUI() {
    const rows = document.querySelectorAll("#senderAccountsTable tr");

    rows.forEach(row => {
        row.classList.add("grayed-out");
        const amount = row.querySelector(".amount-input");
        const fee = row.querySelector(".fee-input");

        if (amount) {
            amount.value = 0;
            amount.setAttribute("readonly", true);
        }
        if (fee) {
            fee.value = 0;
            fee.setAttribute("readonly", true);
        }
    });

    updateTotalTransfer();
}

function enforceSingleTransferSelection() {
    const rows = document.querySelectorAll("#senderAccountsTable tr");
    const selected = document.querySelector('input[name="senderAccountRadio"]:checked');

    rows.forEach(row => {
        const amount = row.querySelector(".amount-input");
        const fee = row.querySelector(".fee-input");
        const isThisRow = row.contains(selected);

        if (amount && fee) {
            if (isThisRow) {
                amount.removeAttribute("readonly");
                fee.removeAttribute("readonly");
                row.classList.remove("grayed-out");
            } else {
                amount.value = 0;
                fee.value = 0;
                amount.setAttribute("readonly", true);
                fee.setAttribute("readonly", true);
                row.classList.add("grayed-out");
            }

            amount.classList.remove("is-valid", "is-invalid");
            const icon = row.querySelector(".status-indicator");
            if (icon) icon.innerHTML = "";
        }
    });

    updateTotalTransfer();
}

function updateTotalTransfer() {
    let total = 0;
    let fees = 0;

    document.querySelectorAll(".amount-input").forEach(input => {
        total += parseFloat(input.value || "0");
    });

    document.querySelectorAll(".fee-input").forEach(input => {
        fees += parseFloat(input.value || "0");
    });

    const grand = total + fees;
    document.getElementById("totalTransferAmount").innerText = total.toLocaleString(undefined, { minimumFractionDigits: 2 });
    document.getElementById("transferFeeAmount").innerText = fees.toLocaleString(undefined, { minimumFractionDigits: 2 });
    document.getElementById("grandTotalAmount").innerText = grand.toLocaleString(undefined, { minimumFractionDigits: 2 });
}
function SearchReceiver() {
    const ref = document.getElementById('receiverSearchInput').value.trim();

    if (ref.length !== 10) {
        showReceiverFeedback("❌ Enter a valid 10-digit reference number.");
        return;
    }

    // Reset UI
    $("#receiverSearchFeedback").text("");
    $("#receiverInfoSection").hide();
    $("#receiverAccountBody").empty();
    $("#receiverSearchBtnText").addClass("d-none");
    $("#receiverSearchSpinner").removeClass("d-none");

    $.ajax({
        url: '/AccountToAccountTransfer/GetReceiverDetails',
        method: 'GET',
        data: { receiverRef: ref },
        success: function (res) {
            $("#receiverSearchBtnText").removeClass("d-none");
            $("#receiverSearchSpinner").addClass("d-none");

            if (!res.success || !res.data) {
                showReceiverFeedback(res.message || "❌ Receiver not found or has no accounts.");
                return;
            }

            const { name, branch, branchCode, accounts } = res.data;

            $("#receiverFullName").text(name);
            $("#receiverBranchName").text(branch);
            $("#receiverBranchCode").text(branchCode);

            if (!Array.isArray(accounts) || accounts.length === 0) {
                showReceiverFeedback("⚠️ No eligible receiver accounts found.");
                return;
            }

            const accountRows = accounts.map(a => `
            <tr>
                <td>${a.accountType}</td>
                <td class="text-end">${parseFloat(a.balance).toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                <td class="text-center">
                    <input type="radio" name="receiverAccountRadio" value="${a.accountNumber}"/>
                </td>
            </tr>
        `).join("");


            $("#receiverAccountBody").html(accountRows);
            $("#receiverInfoSection").fadeIn(150);
        },
        error: function () {
            $("#receiverSearchBtnText").removeClass("d-none");
            $("#receiverSearchSpinner").addClass("d-none");
            showReceiverFeedback("❌ Network or server error occurred.");
        }
    });
}
$('.amount-input').on('input', function () {
    handleTransferButtonsVisibility();
});

function showReceiverFeedback(msg) {
    $("#receiverSearchFeedback").text(msg);
}
function handleTransferButtonsVisibility() {
    const hasAmount = [...document.querySelectorAll('.amount-input')]
        .some(input => parseFloat(input.value || 0) > 0);

    const hasReceiverSelected = $('#ReceiverAccountNumber').val()?.trim().length > 0;

    if (hasAmount) {
        if (hasReceiverSelected) {
            $('#searchReceiverBtn').addClass('d-none');
            $('#confirmTransferBtn').removeClass('d-none');
        } else {
            $('#searchReceiverBtn').removeClass('d-none');
            $('#confirmTransferBtn').addClass('d-none');
        }
    } else {
        $('#searchReceiverBtn').addClass('d-none');
        $('#confirmTransferBtn').addClass('d-none');
    }
}


// Bind to amount input change
$('.amount-input').on('input', handleTransferButtonsVisibility);

function ApplyReceiverSelection() {
    const selected = $('input[name="receiverAccountRadio"]:checked').val();
    if (!selected) {
        showReceiverFeedback("Please select an account.");
        return;
    }

    const selectedRow = $('input[name="receiverAccountRadio"]:checked').closest("tr");
    const accountType = selectedRow.find("td:eq(0)").text().trim();
    const balance = selectedRow.find("td:eq(1)").text().trim();

    // Extract other info from modal
    const fullName = $('#receiverSearchModal #receiverFullName').text().trim();
    const branch = $('#receiverSearchModal #receiverBranchName').text().trim();
    const branchCode = $('#receiverSearchModal #receiverBranchCode').text().trim(); // ✅ NEW
    const phone = $('#receiverSearchModal #receiverPhone').text()?.trim() || "N/A";
    const cni = $('#receiverSearchModal #receiverCNI').text()?.trim() || "N/A";

    // ✅ Populate visible summary
    $('#summaryReceiverName').text(fullName);
    $('#summaryReceiverBranch').text(branch);
    $('#summaryReceiverAccount').text(`${accountType} - ${selected}`);

    // ✅ Populate hidden fields
    $('#ReceiverAccountNumber').val(selected);
    $('#ReceiverAccountType').val(accountType);
    $('#ReceiverAccountBalance').val(balance.replace(/[^0-9.]/g, ''));
    $('#ReceiverPhone').val(phone);
    $('#ReceiverCNI').val(cni);
    $('#summaryReceiverBranchCode').val(branchCode); // ✅ Set branch code

    // Show summary & close modal
    $('#receiverSummarySection').css('display', 'block');
    $('#receiverSearchModal').modal('hide');

    handleTransferButtonsVisibility();
    showReceiverToast("✅ Receiver selected: " + selected);
}







function showReceiverToast(message) {
    appalert("✅ " + message, 1, 1);

}



function PostAccountTransfer() {
    const senderValidation = validateSenderAccount();
    if (!senderValidation.isValid) {
        appalert(senderValidation.message || "❌ Please correct sender account info.", 2, 1);
        return;
    }

    const receiverValidation = validateReceiverAccount();
    if (!receiverValidation.isValid) {
        appalert(receiverValidation.message, 2, 1);
        return;
    }

    const rawNote = $("#Transfer_Note").val()?.trim() || "No additional note.";
    const accountant = $("#accountantName").val() || "System";
    const timestamp = new Date().toLocaleString("en-GB", {
        weekday: 'short', year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });

    const transferData = {
        senderAccountNumber: senderValidation.senderAccount,
        amount: senderValidation.amount,
        fee: senderValidation.fee,
        note: `${rawNote} [Initiated by: ${accountant} on ${timestamp}]`,
        receiverAccountNumber: receiverValidation.accountNumber,
        receiverName: receiverValidation.name,
        receiverBranch: receiverValidation.branch
    };

    // ✅ Now safe to compare
    if (transferData.senderAccountNumber === transferData.receiverAccountNumber) {
        appalert("❌ Sender and Receiver accounts cannot be the same.", 2, 1);
        return;
    }

    showTransferConfirmation(transferData);
}


function validateSenderAccount() {
    const amountInputs = Array.from(document.querySelectorAll('.amount-input'));
    const filledSenderInputs = amountInputs.filter(input => parseFloat(input.value) > 0);

    // 1️⃣ If no or multiple sender inputs filled
    if (filledSenderInputs.length !== 1) {
        amountInputs.forEach(input => {
            const maxBalance = parseFloat(input.getAttribute("max") || "0");
            validateTransferAmount(input, maxBalance);
        });

        return {
            isValid: false,
            message: "⚠️ Please enter amount in exactly one sender account."
        };
    }

    // 2️⃣ Validate the amount
    const input = filledSenderInputs[0];
    const amount = parseFloat(input.value);
    const max = parseFloat(input.getAttribute("max"));
    const accountNumber = input.getAttribute("data-account");

    if (!validateTransferAmount(input, max)) {
        return {
            isValid: false,
            message: "❌ Amount is invalid or exceeds balance for the selected account."
        };
    }


    // 3️⃣ Fetch Fee
    const row = $(input).closest("tr");
    const fee = parseFloat(row.find(".fee-input").val()) || 0;
    const accountType = row.find("td:eq(2)").text().trim();

    // 4️⃣ Sender Metadata
    const customerId = $("#customerId").val();
    const memberName = $("#memberName").val();
    const branchName = $("#branchName").val();
    const branchCode = $("#branchCode").val();
    const accountant = $("#accountantName").val();

    return {
        isValid: true,
        senderAccount: accountNumber,
        accountType: accountType,
        amount: amount,
        fee: fee,
        customerId: customerId,
        memberName: memberName,
        branchName: branchName,
        branchCode: branchCode,
        accountant: accountant
    };
}

function validateReceiverAccount() {
    const account = $("#ReceiverAccountNumber").val();
    const name = $("#summaryReceiverName").text().trim();
    const branch = $("#summaryReceiverBranch").text().trim();
    const branchCode = $("#summaryReceiverBranchCode").val() || "N/A"; // hidden input

    if (!account || account === "") {
        return {
            isValid: false,
            message: "⚠️ Please select a receiver account before confirming."
        };
    }

    if (!name || !branch) {
        return {
            isValid: false,
            message: "⚠️ Receiver information is incomplete. Please re-select a receiver."
        };
    }

    return {
        isValid: true,
        accountNumber: account,
        name: name,
        branch: branch,
        branchCode: branchCode
    };
}
function getAccountTypeName(accountNumber) {
    const row = $(`#myDataTableT tbody tr`).filter(function () {
        return $(this).find("input[type=radio]").val() === accountNumber;
    });

    return row.length ? row.find("td:eq(2)").text().trim() : "Unknown";
}

function showTransferConfirmation(transferData) {
    const amountFormatted = transferData.amount.toLocaleString('en-US') + " FCFA";
    const feeFormatted = transferData.fee.toLocaleString('en-US') + " FCFA";
    const totalTransfer = (transferData.amount + transferData.fee).toLocaleString('en-US') + " FCFA";

    // Sender details
    const senderName = $("#memberName").val() || "N/A";
    const senderBranch = $("#branchName").val() || "N/A";
    const senderBranchCode = $("#branchCode").val() || "N/A";
    const senderAccountNumber = transferData.senderAccountNumber;
    const senderAccountType = getAccountTypeName(senderAccountNumber) || "N/A";

    // Receiver details
    const receiverName = transferData.receiverName || "N/A";
    const receiverBranch = transferData.receiverBranch || "N/A";
    const receiverBranchCode = transferData.receiverBranchCode || "N/A"; // ensure you pass this value in transferData
    const receiverAccountNumber = transferData.receiverAccountNumber;
    const receiverAccountType = getAccountTypeName(receiverAccountNumber) || "N/A";

    // Inter-branch check
    const isInterBranch = senderBranchCode !== receiverBranchCode;

    // Accountant details
    const accountant = $("#accountantName").val() || "System";
    const timestamp = new Date().toLocaleString("en-GB", {
        weekday: 'short', year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
    });

    // 💬 Account Table Summary
    const accountSummary = `
        <table class="table table-sm table-bordered w-100 mt-3">
            <thead class="table-light">
                <tr>
                    <th colspan="3" class="text-start text-dark fw-bold bg-light">
                        <i class="mdi mdi-transfer me-1"></i> Transfer Request Summary
                    </th>
                </tr>
                <tr>
                    <th>Account Type</th>
                    <th>Amount</th>
                    <th>Fee</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>${senderAccountType}</td>
                    <td>${amountFormatted}</td>
                    <td>${feeFormatted}</td>
                </tr>
            </tbody>
            <tfoot>
                <tr class="fw-bold text-dark">
                    <td class="text-end">Total</td>
                    <td>${amountFormatted}</td>
                    <td>${feeFormatted}</td>
                </tr>
            </tfoot>
        </table>
        <p class="text-success mt-3 fw-bold">Total Amount to be Transferred: ${totalTransfer}</p>
    `;

    // ✅ Final content
    const confirmationHtml = `
        <div class="text-start">
            <p class="mb-1 fw-bold text-success">Sender</p>
            <p>
                <strong>Name:</strong> ${senderName}<br/>
                <strong>Branch:</strong> ${senderBranch} [${senderBranchCode}]<br/>
                <strong>Account:</strong> ${senderAccountType} - ${senderAccountNumber}
            </p>

            <p class="mb-1 fw-bold text-success">Receiver</p>
            <p>
                <strong>Name:</strong> ${receiverName}<br/>
                <strong>Branch:</strong> ${receiverBranch} [${receiverBranchCode}]<br/>
                <strong>Account:</strong> ${receiverAccountType} - ${receiverAccountNumber}
            </p>

            <p class="mb-2 fw-bold ${isInterBranch ? 'text-warning' : 'text-info'}">
                <i class="mdi mdi-swap-horizontal"></i>
                ${isInterBranch ? 'Inter-Branch Transfer' : 'Same Branch Transfer'}
            </p>

            ${accountSummary}

            <p class="mt-3 small text-muted">
                <strong>Initiated By:</strong> ${accountant}<br/>
                <strong>Date & Time:</strong> ${timestamp}
            </p>
        </div>
    `;

    alertify.confirm(
        "💳 Transfer Request Confirmation",
        confirmationHtml,
        function () {
            postTransferToServer(transferData); // ✅ Post to server
        },
        function () {
            appalert("❌ Transfer cancelled by user.", 2, 1);
        }
    ).set('labels', { ok: 'Yes, Confirm', cancel: 'Cancel' });
}

function postTransferToServer(data) {
    const accountant = $("#accountantName").val() || "System";
    const timestamp = new Date().toLocaleString("en-GB", {
        weekday: 'short',
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });

    const sender = data.senderAccountNumber || "[MISSING SENDER]";
    const receiver = data.receiverAccountNumber || "[MISSING RECEIVER]";
    const amount = data.amount;
    const fee = data.fee || 0;
    const total = amount + fee;

    const senderBranchCode = $("#branchCode").val() || "N/A";
    const receiverBranchCode = data.receiverBranchCode || "N/A";

    const isInterBranch = senderBranchCode !== receiverBranchCode;
    const interBranchLabel = isInterBranch
        ? "🔁 Inter-Branch Transfer"
        : "🏦 Local Branch Transfer";

    // ✏️ Handle note content
    let baseNote = (data.note || "").trim();
    let placeholderWarnings = [];

    // ✅ Replace placeholders
    if (baseNote.includes("[Sender's Account]")) {
        if (sender !== "[MISSING SENDER]") {
            baseNote = baseNote.replace("[Sender's Account]", sender);
        } else {
            placeholderWarnings.push("⚠️ Sender account could not be resolved.");
        }
    }

    if (baseNote.includes("[Receiver's Account]")) {
        if (receiver !== "[MISSING RECEIVER]") {
            baseNote = baseNote.replace("[Receiver's Account]", receiver);
        } else {
            placeholderWarnings.push("⚠️ Receiver account could not be resolved.");
        }
    }

    // ✅ Prevent duplicated '[Initiated by ...]' lines
    const initiatedLine = `[Initiated by: ${accountant} on ${timestamp}]`;
    if (!baseNote.includes(initiatedLine)) {
        baseNote += `\n\n${initiatedLine}`;
    }

    // ✅ Append inter-branch info
    baseNote += `\n${interBranchLabel}`;

    // ✅ Append placeholder warnings if any
    if (placeholderWarnings.length > 0) {
        baseNote += `\n\n⚠️ Note Warnings:\n- ${placeholderWarnings.join("\n- ")}`;
    }

    const payload = {
        SenderAccountNumber: sender,
        ReceiverAccountNumber: receiver,
        Amount: amount,
        Fee: fee,
        Total: total,
        Note: baseNote
    };

    $.ajax({
        url: "/AccountToAccountTransfer/SubmitTransfer",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            if (response.success) {
                appalert("✅ " + response.message, 1, 1);
                setTimeout(() => location.reload(), 1200);
               
            } else {
                appalert(response.message || "❌ Transfer failed.", 2, 1);
            }
        },
        error: function () {
            appalert("❌ Network error occurred while submitting the transfer.", 3, 1);
        }
    });
}


function validateTransferAmount(input, maxBalance) {
    const row = input.closest("tr");
    const iconSpan = row.querySelector(".status-indicator");
    const value = parseFloat((input.value || "0").replace(/,/g, ""));
    const balance = parseFloat((maxBalance || "0").toString().replace(/,/g, ""));

    iconSpan.innerHTML = "";
    input.classList.remove("is-invalid", "is-valid");

    // Handle empty/zero without marking it invalid, but return false
    if (value === 0) {
        updateTotalTransfer();
        handleTransferButtonsVisibility();
        return false; // ❌ This is critical!
    }

    if (isNaN(value) || value < 0) {
        input.classList.add("is-invalid");
        iconSpan.innerHTML = `<i class="mdi mdi-alert-circle text-danger" title="Invalid amount"></i>`;
        updateTotalTransfer();
        handleTransferButtonsVisibility();
        return false;
    }

    if (value > balance) {
        input.classList.add("is-invalid");
        iconSpan.innerHTML = `<i class="mdi mdi-close-circle text-danger" title="Exceeds balance"></i>`;
        updateTotalTransfer();
        handleTransferButtonsVisibility();
        return false;
    }

    input.classList.add("is-valid");
    iconSpan.innerHTML = `<i class="mdi mdi-check-circle text-success" title="Valid amount"></i>`;
    updateTotalTransfer();
    handleTransferButtonsVisibility();
    return true; // ✅ Valid
}

function SearchSender() {
    const referenceNumber = document.getElementById('senderSearchInput').value.trim();

    if (referenceNumber.length !== 10) {
        alert("Please enter a valid 10-digit sender reference number.");
        return;
    }

    $.ajax({
        url: "/AccountToAccountTransfer/GetSenderAccountsPartial",
        method: "GET",
        data: { senderRef: referenceNumber },
        success: function (html) {
            $("#senderAccountsTableContainer").html(html);
            updateTotalTransfer(); // Reset total
        },
        error: function () {
            alert("An error occurred while loading sender accounts.");
        }
    });
}
