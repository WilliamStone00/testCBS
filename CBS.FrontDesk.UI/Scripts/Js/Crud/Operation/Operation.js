$(document).ready(function () {
    $(document).ready(function () {
        $('#myDataTableT').DataTable();
    });
    $("#btnData").click(function () {
        LoadData();
    });
    var depositerDiv = document.getElementById("depositer");
    depositerDiv.style.display = "none";
});
function toggleDepositer(isChecked) {
    var depositerDiv = document.getElementById("depositer");
    if (isChecked) {
        // Checkbox is checked, hide the depositer div
        depositerDiv.style.display = "none";
    } else {
        // Checkbox is unchecked, show the depositer div
        depositerDiv.style.display = "block";
    }
}
function GetTransactionHistory(KEY, divToLoadData, partialView, path, myDataTable, order) {
    LoadDataTableNew("Operation", myDataTable, "InitializeData", KEY, partialView, order, path, divToLoadData);

}

function GetObject(KEY, divToLoadData, partialView, path) {
    EditResetMain(KEY, partialView, divToLoadData, "Operation", "InitializeData", null, null, path);
    calculateBalance();
}
function PostData(form) {
    var partialView = $("#partialView").val();
    var accountNumber = $("#accountNumber").val();
    var divID = $("#divID").val();
    var controller = $("#controller").val();
    var actionMethod = $("#actionMethod").val();
    var path = $("#path").val();
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    location.reload();
                    /*GetObject(accountNumber, partialView, divID, path)*/
                }
                else
                    if (response.message === undefined) {
                        alert("Your session is expired.")
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }


            }
            , error: function (err) {
                appalert(err.statusText, 0, 1);
            }
        };

        if ($(form).attr('enctype') === "multipart/form-data") {
            ajaxConfig["contentType"] = false;
            ajaxConfig["processData"] = false;
        }
        $.ajax(ajaxConfig);

    }
    return false;

}


function GetLoan(KEY) {
    $.ajax({
        type: "GET",
        url: '/Operation/GetLoan?KEY=' + KEY,
        success: function (data) {
            var balance = parseFloat(data.Balance).toFixed(1); // Format Balance with 1 decimal place
            var paid = parseFloat(data.Paid).toFixed(1); // Format Paid with 1 decimal place

            // Format numbers with commas as thousands separators
            balance = parseFloat(balance).toLocaleString('en-US');
            paid = parseFloat(paid).toLocaleString('en-US');

            // Assuming #balance and #paid are HTML input elements
            $('#balance').val(balance);
            $('#paid').val(paid);
            $('#loanid').val(data.Id);
            //$('#balance').val(data.Balance);
            //$('#paid').val(data.Paid);
        }, error: function (err) {
            appalert(err.statusText, 3, 0);
        }
    });
}




function calculateBalance() {
    // Get the amount entered by the user
    var amount = parseFloat(document.getElementById("DepositRequest_amount").value) || 0;

    // Get the total amount of currency notes and coins
    var note10000 = parseInt(document.getElementById('DepositRequest_currencyNotes_note10000').value) || 0;
    var note5000 = parseInt(document.getElementById('DepositRequest_currencyNotes_note5000').value) || 0;
    var note2000 = parseInt(document.getElementById('DepositRequest_currencyNotes_note2000').value) || 0;
    var note1000 = parseInt(document.getElementById('DepositRequest_currencyNotes_note1000').value) || 0;
    var note500 = parseInt(document.getElementById('DepositRequest_currencyNotes_note500').value) || 0;
    var coin500 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin500').value) || 0;
    var coin100 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin100').value) || 0;
    var coin50 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin50').value) || 0;
    var coin25 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin25').value) || 0;
    var coin10 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin10').value) || 0;
    var coin5 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin5').value) || 0;
    var coin1 = parseInt(document.getElementById('DepositRequest_currencyNotes_coin1').value) || 0;

    // Calculate total amount
    var totalAmount = (note10000 * 10000) + (note5000 * 5000) + (note2000 * 2000) + (note1000 * 1000) +
        (note500 * 500) + (coin500 * 500) + (coin100 * 100) + (coin50 * 50) + (coin25 * 25) + (coin10 * 10) + (coin5 * 5) + coin1;

    // Calculate the balance
    var balance = amount - totalAmount;

    // Calculate number of notes and coins required
    var notes10000Required = Math.floor(balance / 10000);
    var notes5000Required = Math.floor((balance % 10000) / 5000);
    var notes2000Required = Math.floor(((balance % 10000) % 5000) / 2000);
    var notes1000Required = Math.floor((((balance % 10000) % 5000) % 2000) / 1000);
    var notes500Required = Math.floor(((((balance % 10000) % 5000) % 2000) % 1000) / 500);
    var coin500Required = Math.floor((((((balance % 10000) % 5000) % 2000) % 1000) % 500) / 500);
    var coin100Required = Math.floor(((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) / 100);
    var coin50Required = Math.floor((((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) % 100) / 50);
    var coin25Required = Math.floor(((((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) % 100) % 50) / 25);
    var coin10Required = Math.floor((((((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) % 100) % 50) % 25) / 10);
    var coin5Required = Math.floor(((((((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) % 100) % 50) % 25) % 10) / 5);
    var coin1Required = Math.floor((((((((((((balance % 10000) % 5000) % 2000) % 1000) % 500) % 500) % 100) % 50) % 25) % 10) % 5));

    // Format the balance and required notes and coins as financial values with commas as thousand separators and two decimal places
    var formattedBalance = balance.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formattedNotes10000Required = notes10000Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedNotes5000Required = notes5000Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedNotes2000Required = notes2000Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedNotes1000Required = notes1000Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedNotes500Required = notes500Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin500Required = coin500Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin100Required = coin100Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin50Required = coin50Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin25Required = coin25Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin10Required = coin10Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin5Required = coin5Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    var formattedCoin1Required = coin1Required.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 });

    // Update the calculatedBalance and required notes and coins elements
    document.getElementById("calculatedBalance").textContent = formattedBalance;
    document.getElementById("notes10000Required").textContent = formattedNotes10000Required;
    document.getElementById("notes5000Required").textContent = formattedNotes5000Required;
    document.getElementById("notes2000Required").textContent = formattedNotes2000Required;
    document.getElementById("notes1000Required").textContent = formattedNotes1000Required;
    document.getElementById("notes500Required").textContent = formattedNotes500Required;
    document.getElementById("coins500Required").textContent = formattedCoin500Required;
    document.getElementById("coins100Required").textContent = formattedCoin100Required;
    document.getElementById("coins50Required").textContent = formattedCoin50Required;
    document.getElementById("coins25Required").textContent = formattedCoin25Required;
    document.getElementById("coins10Required").textContent = formattedCoin10Required;
    document.getElementById("coins5Required").textContent = formattedCoin5Required;
    document.getElementById("coins1Required").textContent = formattedCoin1Required;

}

// Call the calculateBalance function whenever input fields change
document.querySelectorAll("#accordionPopoutIcon-3 input[type='text']").forEach(input => {
    input.addEventListener("input", calculateBalance);
});
