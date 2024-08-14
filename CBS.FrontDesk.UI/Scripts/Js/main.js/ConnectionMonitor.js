$(function () {
    var connection = $.hubConnection();
    var hubProxy = connection.createHubProxy('connectionHub');

    // Subscribe to 'ReceiveConnectionStatus' event
    hubProxy.on('ReceiveConnectionStatus', function (isConnected) {
        if (!isConnected) {
            // Show disconnected message
            showModal('Terminal is disconnected', 'red');
        } else {
            // Show reconnected message
            showModal('Connection has been restored', 'green');

            // Hide connection modal after 5 seconds
            setTimeout(function () {
                $('#connectionModal').hide();
                $('#modalMessage').text('Terminal is disconnected').css('color', 'red');
            }, 5000);
        }
    });

    // Start the SignalR connection
    connection.start().done(function () {
        console.log('SignalR Connected');
        // Call server method to check connection status
        hubProxy.invoke('ReceiveConnectionStatus').done(function (isConnected) {
            // Handle the initial connection status response
            if (!isConnected) {
                showModal('Terminal is disconnected', 'red');
            } else {
                showModal('Terminal is connected', 'green');
                setTimeout(function () {
                    $('#connectionModal').hide();
                }, 5000);
            }
        }).fail(function (error) {
            console.error('Error invoking CheckConnectionStatus: ' + error);
        });
    }).fail(function (error) {
        console.error('SignalR Connection Error: ' + error);
    });
    var serializedHtml = "<div id=\"connectionModal\" style=\"display: none; position: fixed; z-index: 9999; left: 0; top: 0; width: 100%; height: 100%; overflow: auto; background-color: rgba(0,0,0,0.0001);\">" +
        "<div style=\"background-color: #fefefe; margin: 15% auto; padding: 20px; border: 1px solid #888; width: 80%; text-align: center;\">" +
        "<h1 id=\"modalMessage\" style=\"font-size: 48px;\"></h1>" +
        "</div>" +
        "</div>";
    function showModal(message, color) {
        $('body').append(
            serializedHtml
        );
        $('#modalMessage').text(message).css('color', color);
        $('#connectionModal').show();
    }
});
//$(function () {
//    $.connection.hub.logging = true;
//    var connection = $.hubConnection();
//    var hubProxy = connection.createHubProxy('connectionHub');

//    hubProxy.on('ReceiveConnectionStatus', function (isConnected) {
//        if (!isConnected)
//        {
//            // Show disconnected message
//            showModal('Terminal is disconnected', 'red');
//        } else {
//            // Show reconnected message
//            showModal('Connection has been restored', 'green');
 
        
//        }
//    });

//    connection.start().done(function () {
//        console.log('SignalR Connected');
   
//    }).fail(function (error) {
//        console.error('SignalR Connection Error: ' + error);
//    });
//    var serializedHtml = "<div id=\"connectionModal\" style=\"display: none; position: fixed; z-index: 9999; left: 0; top: 0; width: 100%; height: 100%; overflow: auto; background-color: rgba(0,0,0,0.4);\">" +
//        "<div style=\"background-color: #fefefe; margin: 15% auto; padding: 20px; border: 1px solid #888; width: 80%; text-align: center;\">" +
//        "<h1 id=\"modalMessage\" style=\"font-size: 48px;\"></h1>" +
//        "</div>" +
//        "</div>";
//    function showModal(message, color) {
//        // Implement your modal display logic here
//        $('body').append(
//            serializedHtml
//        );
//        // Example: display a Bootstrap modal
//         $('#modalMessage').text(message).css('color', color);
//         $('#connectionModal').modal('show');
//    }
//});