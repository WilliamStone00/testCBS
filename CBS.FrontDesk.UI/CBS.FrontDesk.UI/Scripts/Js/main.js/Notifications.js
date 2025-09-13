$(document).ready(function () {
    //$('#loading').hide();
    //checkForNewEvents();
    LoadCashNotificationDataDT("ListOfPendingNotification")
    let notifications = [];
    $('.dropdown-notifications-list .list-group').empty();
    $('#simulateNotificationButton').empty();
    $('#totalNotification').text("0");
    $('#simulateNotificationButton').text("No Pending Request");
    // Function to periodically check for new events

    $(document).on('change', '#CorrespondingBranchID', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        console.log(selectedValue);
        if (selectedValue === 'RedirectToBranch') {
            // Show the element
            $('#hideBranchID').show();
        } else {
            // Hide the element
            $('#hideBranchID').hide();
        }
    });
    $(document).on('change', '#BranchID', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        console.log(selectedValue);
        loadBankAccountForBranch(selectedValue)
    });

    $(document).on('change', '#AccountId', function () {

        // Get the selected value
        var selectedValue = $(this).val();
        loadAccountBalance(selectedValue);
    });
    function checkForNewEvents() {
         //Ensure the loader is hidden specifically for this function
        //$('#loading').hide();

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
                //$('#loading').hide();
            }
        });
    }

    // Start checking for new events every 5 seconds
    //setInterval(checkForNewEvents, 5000);


    function loadBankAccountForBranch(BranchId) {

        $.ajax({
            url: '/CashFlowManagement/GetAllBranchAccountUsedToCreditCashFlow',
            type: 'GET',
            dataType: 'json',
            data: { branchId: BranchId },
            success: function (data) {
                // Clear existing options in the OperationEventAttributeId combo 
                $('#LoadBankAccountID').empty();
                $.each(data, function (index, item) {
                    $('#LoadBankAccountID').append($('<option>').text(item.Value).attr('value', item.Text));
                });

                // Add new options based on the fetched data
                console.log(data);
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        });
    }
    function loadAccountBalance(accountId) {
        console.log(accountId);
        $.ajax({
            url: '/ManuallyJournalEntry/GetAccountBalance',
            type: 'GET',
            dataType: 'json',
            data: { Id: accountId },
            success: function (data) {
                // Clear existing options in the OperationEventAttributeId combo 
                $('#BankCashOut_Balance').empty();
                $("#BankCashOut_Balance").val(data.Account.CurrentBalance);

                // Add new options based on the fetched data
                console.log(data.Account.CurrentBalance);
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        });
    }


    function AjaxPostAndUpdateValidationDecision(form) {

        $.validator.unobtrusive.parse(form);
        if ($(form).valid()) {
            var ajaxConfig = {
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                success: function (response) {

                    if (response.success) {
                        appalert(response.message, 1, 1);
                        PageReload();
                    }
                    else {
                        appalert(response.message, 2, 1);

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


    function addNotification(notification,count) {
        // Update the badge with the total number of notifications
        let totalNotificationsElement = $('#totalNotification');
        //let currentCount = parseInt(totalNotificationsElement.text()) || 0;
        //totalNotificationsElement.text((currentCount + 1) + ' New');
        let currentCount = parseInt(totalNotificationsElement.text()) || 0;
        totalNotificationsElement.text((count ) + ' New');
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

    // Update the timestamps every minute
    setInterval(updateTimestamps,1000*60);
});

function LoadCashNotificationDataDT(tableID) {


    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        "columns": [
            //{ "data": "ReferenceId", "name": "ReferenceId", "autoWidth": true },
            //{ "data": "Amount", "name": "Amount", "autoWidth": true },
            //{ "data": "RequestMessage", "name": "RequestMessage", "autoWidth": true },
            //{ "data": "IssuedBy", "name": "IssuedBy", "autoWidth": true },
            //{ "data": "IssuedDate", "name": "IssuedDate", "autoWidth": true },
            //{
            //    "data": "Id", "orderable": "false", "render": function (data) {
            //        return "<a href='/CashFlowManagement/GetCashReplenimentRequest?KEY=" + data + " class='mr-2' data-toggle='tooltip' data-placement='top' title='View " + data + " detail'> Click to Approve</a>";
            //    }
            //}
        ],
        "columnDefs": [
            /*//{ "targets": 0, "searchable": true, "orderable": true, "width": "10%" },*/
            { "targets": 0, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 1, "searchable": true, "orderable": true, "width": "20%" },
            { "targets": 2, "searchable": true, "orderable": true, "width": "30%" },
            { "targets": 3, "searchable": true, "orderable": true, "width": "15%" },
            { "targets": 4, "searchable": true, "orderable": true, "width": "15%" },

        ],

        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


        order: [[0, "asc"]],
        bInfo: true,
        pageLength: 10

    });
}
