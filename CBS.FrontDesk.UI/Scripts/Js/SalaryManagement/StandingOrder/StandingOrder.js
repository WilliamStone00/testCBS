$(document).ready(function () {
    // Event delegation for handling button clicks
    $(document).on("click", ".btn", function () {
        // Remove underline and blue color from all buttons
        $(".btn").removeClass("clicked");
        // Add underline and blue color to the clicked button
        $(this).addClass("clicked");
    });

    // Handle checkbox state changes
    // Format date mask



});


function toggleExternalAccountSection() {
    const isExternalAccount = document.getElementById("externalAccountCheckbox").checked;
    const externalAccountSection = document.getElementById("externalAccountSection");
    const destinationAccountSection = document.getElementById("destinationAccountSection");
    const destinationAccountNone = document.querySelector("input[name='AddOrUpdateStandingOrderCommand.DestinationAccountType'][value='None']");

    if (isExternalAccount) {
        // Show the external account section and hide the destination account section
        externalAccountSection.style.display = "flex";
        destinationAccountSection.style.display = "none";

        // Set the destination account type to "None"
        if (destinationAccountNone) {
            destinationAccountNone.checked = true;
        }
    } else {
        // Show the destination account section and hide the external account section
        externalAccountSection.style.display = "none";
        destinationAccountSection.style.display = "flex";
    }
}



function showStandingOrderDetails(orderId) {
    // Clear the modal content
    $('#standingOrderDetailsContent').html('<p>Loading...</p>');

    // Load the partial view into the modal
    $.ajax({
        url: '/StandingOrder/GetStandingOrderPartialView', // Endpoint to fetch the partial view
        type: 'GET',
        data: { Key: orderId }, // Pass the standing order ID as a parameter
        success: function (html) {
            // Load the partial view HTML into the modal content
            $('#standingOrderDetailsContent').html(html);

            // Show the modal
            $('#standingOrderDetailsModal').modal('show');
        },
        error: function () {
            $('#standingOrderDetailsContent').html('<p>Error loading standing order details.</p>');
            $('#standingOrderDetailsModal').modal('show');
        }
    });
}

function GetMember() {
    var memberId = $('#manualSearchInput').val().trim();

    if (!memberId) {

        alert("Please enter a valid Member Reference Number to search.");
        return;
    }

    GetMemberData(memberId, '_StandingOrderDesk', 'datalistingview', 'search');
}


function GetMemberData(Key, partialView, divToloadPV, path) {
    AddORUpdateGen(Key, divToloadPV, partialView, path, "StandingOrder");
}



