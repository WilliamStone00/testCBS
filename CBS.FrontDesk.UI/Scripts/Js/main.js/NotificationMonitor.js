$(document).ready(function () {
    // Create a connection to your SignalR hub
    var connection = $.hubConnection();
    var hubProxy = connection.createHubProxy('notificationHub');

    // Event handler for receiving notifications
    hubProxy.on('ReceiveNotifications', function (notifications) {
        if (notifications.length > 0) {
            $('.dropdown-notifications-list .list-group').empty();
            $('#simulateNotificationButton').empty();
            notifications.forEach(function (notification) {
                addNotification(notification, notifications.length);
            });

            $('#simulateNotificationButton').text("Pending Request(" + notifications.length + ")");
        } else {
            $('.dropdown-notifications-list .list-group').empty();
            $('#simulateNotificationButton').empty();
            $('#simulateNotificationButton').text("No Pending Request");
            $('#totalNotification').text("0");
            console.warn('No new notifications or unexpected data format:', notifications);
        }
    });

    // Start the SignalR connection
    connection.start().done(function () {
        console.log('SignalR connection established.');
        checkForNewEvents(); // Start checking for events after connection is established
    });

    // Function to check for new events
    function checkForNewEvents() {
        // Ensure the loader is hidden specifically for this function
        $('#loading').hide();

        // Call the SignalR hub method to fetch notifications
        hubProxy.invoke('GetUserNotificationRequest')
            .fail(function (error) {
                console.error('Error fetching notifications:', error);
            })
            .always(function () {
                // Ensure the loader remains hidden after the request
                $('#loading').hide();
            });
    }

    // Function to add a new notification
    function addNotification(notification, count) {
        // Update the badge with the total number of notifications
        let totalNotificationsElement = $('#totalNotification');
        totalNotificationsElement.text(count + ' New');
        $('.badge-dot').addClass('active');

        // Generate initials from the user's name
        let initials = getInitials(notification.UserName);

        // Construct the HTML for the new notification item
        let notificationItem = `
            <li class="list-group-item list-group-item-action dropdown-notifications-item">
                <!-- Your notification item HTML -->
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
            $(this).text(timeAgo);
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
