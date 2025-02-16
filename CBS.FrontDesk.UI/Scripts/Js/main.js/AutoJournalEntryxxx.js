 
    $(document).ready(function () {
        function updateTotals() {
            let totalDebit = 0;
            let totalCredit = 0;

            $(".debitInput").each(function () {
                let value = parseFloat($(this).val()) || 0;
                totalDebit += value;
            });

            $(".creditInput").each(function () {
                let value = parseFloat($(this).val()) || 0;
                totalCredit += value;
            });

            $("#debitTotal").text(totalDebit.toLocaleString());
            $("#creditTotal").text(totalCredit.toLocaleString());

            let balanceMessage = $("#balanceMessage");
            let submitButton = $("#submitButton");
            let descriptionField = $("#descriptionField");

            if (totalDebit === totalCredit) {
                balanceMessage.text("✅ Balanced. Now enter a description.");
                balanceMessage.removeClass("text-danger").addClass("text-primary");

                if (descriptionField.val().trim().length > 0) {
                    balanceMessage.text("✅ Entry is balanced & description is filled. Ready to submit.");
                    balanceMessage.removeClass("text-primary").addClass("text-success");
                    submitButton.prop("disabled", false);
                } else {
                    balanceMessage.text("⚠️ Entry is balanced. Please enter a description.");
                    submitButton.prop("disabled", true);
                }
            } else {
                let difference = Math.abs(totalDebit - totalCredit);
                if (totalDebit > totalCredit) {
                    balanceMessage.text(`⚠️ Unbalanced: Increase Credit by ${difference.toLocaleString()} XFA.`);
                } else {
                    balanceMessage.text(`⚠️ Unbalanced: Increase Debit by ${difference.toLocaleString()} XFA.`);
                }
                balanceMessage.removeClass("text-success text-primary").addClass("text-danger");
                submitButton.prop("disabled", true);
            }
        }

        $(".debitInput, .creditInput").on("input", updateTotals);
    $("#descriptionField").on("input", function () {
        let balanceMessage = $("#balanceMessage");
    let submitButton = $("#submitButton");

    if ($("#debitTotal").text() === $("#creditTotal").text()) {
                if ($(this).val().trim().length > 0) {
        balanceMessage.text("✅ Entry is balanced & description is filled. Ready to submit.");
    balanceMessage.removeClass("text-primary").addClass("text-success");
    submitButton.prop("disabled", false);
                } else {
        balanceMessage.text("⚠️ Entry is balanced. Please enter a description.");
    submitButton.prop("disabled", true);
                }
            }
        });

    // Submit function using AJAX
    $("#manualEntryForm").submit(function (e) {
        e.preventDefault();

    let formData = {
        accountingRules: [],
    description: $("#descriptionField").val().trim()
            };

    $(".debitInput, .creditInput").each(function () {
        let row = $(this).closest("tr");
    let accountDesc = row.find(".accountDesc").text();
    let debit = parseFloat(row.find(".debitInput").val()) || 0;
    let credit = parseFloat(row.find(".creditInput").val()) || 0;

    formData.accountingRules.push({
        accountDescription: accountDesc,
    debit: debit,
    credit: credit
                });
            });

    $.ajax({
        url: "/ManuallyJournalEntry/SubmitManualEntry",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(formData),
    beforeSend: function () {
        $("#submitButton").prop("disabled", true).text("Submitting...");
                },
    success: function (response) {
        alert("✅ Entry successfully submitted!");
    $("#manualEntryForm")[0].reset();
    $("#submitButton").prop("disabled", true).text("Submit Entry");
    updateTotals();
                },
    error: function (xhr) {
        alert("❌ Error submitting entry. Please try again.");
    $("#submitButton").prop("disabled", false).text("Submit Entry");
                }
            });
        });

    updateTotals();
    });
 
