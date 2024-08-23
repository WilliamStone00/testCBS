$(function () {
    var notificationHub = $.connection.notificationHub;

    notificationHub.client.ReceiveNotification = function (message)
    {
        // Handle the received notification
        showNotification(message);
    };

    $.connection.hub.start().done(function () {
        console.log("SignalR connection established");
    });
});

function showNotification(message)
{
    // Implement your notification display logic here
    alert(message);
}

 