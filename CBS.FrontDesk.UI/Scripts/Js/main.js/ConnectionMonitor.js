$(document).ready(function () {
    // Initialize the SignalR connection to the hub
    var connection = $.hubConnection();
    var hub = connection.createHubProxy('connectionHub');

    // Variable to track the current connection status
    var currentConnectionStatus = true; // Assume connection is OK initially

    // Start the connection
    connection.start().done(function () {
        // Set an interval to check connection status every 10 seconds
        setInterval(function () {
            // Call the CheckConnection method on the server
            hub.invoke('CheckConnection').done(function (response) {
                // Check if the connection status has changed
                if (response !== currentConnectionStatus) {
                    currentConnectionStatus = response; // Update the current status

                    if (response === true) {
                        // Connection has been restored
                        $('#connectionModal').css({ "color": "green", "background-color": "white" });
                        $('#connectionMessage').text("Connection has been restored");
                        $('#connectionModal').fadeOut(2000);
                     
                    } else {
                        $('#connectionModal').fadeIn();
                        // Connection has been lost
                        $('#connectionModal').css({ "color": "red", "background-color": "white" });
                        $('#connectionMessage').text("Terminal is disconnect");
                    }

                    // Show the modal
               
                    // Hide the modal after 5 seconds
                 
                }
            }).fail(function () {
                // Handle any errors if the server couldn't be reached
                if (currentConnectionStatus === true) { // Only display if connection was previously OK
                    currentConnectionStatus = false;
                    $('#connectionModal').css({ "color": "red", "background-color": "white" });
                    $('#connectionMessage').text('Failed to check connection status');
                    $('#connectionModal').fadeIn();
                    setTimeout(function () {
                        $('#connectionModal').fadeOut();
                    }, 2000);
                }
            });
        }, 10000);
    });

    // Handle disconnect event
    connection.disconnected(function () {
        if (currentConnectionStatus === true) { // Only show if it was previously connected
            currentConnectionStatus = false;
            console.log("Is disconnected");
            $('#connectionModal').css({ "color": "red", "background-color": "white" });
            $('#connectionMessage').text('Terminal is disconnect');
            $('#connectionModal').fadeIn();
            //setTimeout(function () {
            //    $('#connectionModal').fadeOut();
            //}, 1000);
        }
    });

    // Handle connection restored event
    connection.reconnecting(function () {
        if (currentConnectionStatus === false) { // Only show if it was previously disconnected
            currentConnectionStatus = true;
            console.log("Is Connected");
            $('#connectionModal').css({ "color": "green", "background-color": "white" });
            $('#connectionMessage').text('Connection is restored');
            $('#connectionModal').fadeIn(2000);
   
    }
    });
});
