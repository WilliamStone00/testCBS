$(document).ready(function () {
    // Initialize the SignalR connection to the hub
    var hub = $.connection.notificationHub;
    if ($.connection && $.connection.notificationHub) {
        var hub = $.connection.notificationHub;

        // Define the client-side method that the server can call
        hub.client.ReceiveNotification = function () {
            // Call your existing function to check for new events
            checkForNewEvents();
        };

        // Start the SignalR connection
        $.connection.hub.start().done(function () {
            console.log("Connected to SignalR Hub");
            checkForNewEvents();
        }).fail(function (error) {
            console.log("Could not connect to SignalR Hub: " + error);
        });
    } else {
        console.error("SignalR hub is not initialized correctly. Make sure the hub script is loaded and the hub name matches.");
    }
    // Define what happens when the server sends a notification


    // Client-side function that is called when the server invokes it


    // Start the SignalR connection



    function showNotification(message) {
        // Implement your notification display logic here
        alert(message);
    }
    $.connection.hub.start().done(function () {
        console.log("Connected to SignalR Hub");
        checkForNewEvents();
    });
    function checkForNewEvents() {
        // Ensure the loader is hidden specifically for this function
        // $('#loading').hide();

        $.ajax({
            url: '/Notification/GetUserNotificationRequest', // Replace with your API endpoint
            method: 'GET',
            beforeSend: function () {
                // Do nothing regarding the loader here, ensuring it's not shown
            },
            success: function (data) {
                console.log(data);

                if (data.length > 0) {
                    $('.dropdown-notifications-list .list-group').empty();
                    $('#simulateNotificationButton').empty();
                    data.forEach(notification => {
                        addNotification(notification, data.length);
                    });

                    $('#simulateNotificationButton').text("Pending Request(" + data.length + ")");

                    // Notify clients of the new event
                    notifyClientsOfNewEvent();
                } else {
                    $('.dropdown-notifications-list .list-group').empty();
                    $('#simulateNotificationButton').empty();
                    $('#simulateNotificationButton').text("No Pending Request");
                    $('#totalNotification').text("0");
                    console.warn('No new notifications or unexpected data format:', data);
                }
            },
            error: function (error) {
                console.error('Error fetching notifications:', error);
            },
            complete: function () {
                // Ensure the loader remains hidden after the request
                $('#loading').hide();
            }
        });
    }

    function notifyClientsOfNewEvent() {
        //var hub = $.connection.notificationHub; // Initialize the SignalR hub
        $.connection.notificationHub.server.notifyClients(); // Call the NotifyClients method on the server
    }

    function addNotification(notification, count) {
        // Update the badge with the total number of notifications
        let totalNotificationsElement = $('#totalNotification');
        let currentCount = parseInt(totalNotificationsElement.text()) || 0;
        totalNotificationsElement.text((count) + ' New');

        // Change the notification dot color to red and scale it up
        $('.badge-dot').addClass('active');

        // Generate initials from the user's name
        let initials = getInitials(notification.UserName);

        // Construct the HTML for the new notification item
        let notificationItem = `
        <li class="list-group-item list-group-item-action dropdown-notifications-item">
            <div class="d-flex gap-2">
                <div class="flex-shrink-0">
                    <div class="avatar me-1">
                        <span class="avatar-initial rounded-circle bg-label-danger">${initials}</span>
                    </div>
                </div>
                <div class="d-flex flex-column flex-grow-1 overflow-hidden w-px-200">
                    <h6 class="mb-1 text-truncate">${notification.UserName}</h6>
                    <div class="mb-1 text-truncate">${notification.BranchName}</div>
                    <a href="${notification.ActionUrl}" class="text-decoration-none">
                        <small class="text-truncate text-body" id="action_indented">${notification.Action}</small>
                    </a>
                </div>
                <div class="flex-shrink-0 dropdown-notifications-actions">
                    <small class="text-muted timestamp" data-timestamp="${notification.Timestamp}"></small>
                </div>
            </div>
        </li>
    `;

        // Add the new notification item to the list
        $('.dropdown-notifications-list .list-group').append(notificationItem);
        updateTimestamps();
    }

    function getInitials(name) {
        let initials = name.split(' ').map(word => word.charAt(0)).join('');
        return initials.toUpperCase();
    }


    function updateTimestamps() {
        $('.timestamp').each(function () {
            let timestamp = $(this).data('timestamp');

            let timeAgo = timeSince(new Date(timestamp));
  
            $(this).text(timestamp);
        });
    }

    function timeSince(date) {
        let seconds = Math.floor((new Date() - date) / 1000);
        let interval = Math.floor(seconds / 31536000);

        if (interval > 1) {
            return interval + " years ago";
        }
        interval = Math.floor(seconds / 2592000);
        if (interval > 1) {
            return interval + " months ago";
        }
        interval = Math.floor(seconds / 86400);
        if (interval > 1) {
            return interval + " days ago";
        }
        interval = Math.floor(seconds / 3600);
        if (interval > 1) {
            return interval + " hrs ago";
        }
        interval = Math.floor(seconds / 60);
        if (interval > 1) {
            return interval + " min ago";
        }
        return Math.floor(seconds) + " seconds ago";
    }
});



