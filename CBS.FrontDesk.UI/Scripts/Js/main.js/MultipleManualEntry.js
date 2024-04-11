$(document).ready(function () {
    $(document).on('change', '#EntryTempData_AccountId', function () {
        var EventId = $(this).val();

        loadAccountBalance(EventId)
    });
    $(document).on('change', '#EntryTempData_BookingDirection', function () {
        var EventId = $(this).val();

        loadDescriptionByOperationDirection(EventId)
    });
});

function loadAccountBalance(accountId) {

    $.ajax({
        url: '/ManuallyJournalEntry/GetAccountBalance',
        type: 'GET',
        dataType: 'json',
        data: { Id: accountId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo 
            $('.account_balance').empty();
            $('.account_name').empty();
            $('.account_number').empty();
            $(".account_balance").val(data.Account.CurrentBalance);
            $(".account_name").val(data.Account.AccountHolder);
            $(".account_number").val(data.Account.AccountNumber);
            // Add new options based on the fetched data
            console.log(data.Account.CurrentBalance);
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}


function loadDescriptionByOperationDirection(operation) {
    $('.booking_direction').empty();
    if (operation === "DEBIT")
    {
       
        var account_name = $(".account_name").val();
        var account_amount = $(".account_amount").val();
        var account_number = $(".account_number").val();
        var message = account_name + "-" + account_number +" will be debited by {0}.0 XFA "
        $(".booking_direction").val(message);
      
    } else {
        var account_name = $(".account_name").val();
        var account_amount = $(".account_amount").val();
        var account_number = $(".account_number").val();
        var message = account_name + "-" + account_number + " will be credited by {0}.0 XFA "
        $(".booking_direction").val(message);
    }

  
}

function LoadJournalEntryData(controller, action, divLoader, tableID, serviceoption, KEY, partialView, path, order) {
    KEY = $('#EntryTempData_Reference').val();
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + serviceoption + '&KEY=' + KEY + '&partialView=' + partialView + '&path=' + path ,
        success: function (dataResponse) {
            console.log(dataResponse);
            $('#' + divLoader).html(dataResponse);
            LoadDT(tableID, order);

        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}
function DeleteDataJournal(controller, KEY, tableID, partialView, order, divToLoadTheData, serviceOption) {
    var Id=$('#EntryTempData_Reference').val();
    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY + "&serviceOption=" + serviceOption;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                     
                        LoadJournalEntryData(controller, "InitializeData", divToLoadTheData, tableID, serviceOption, KEY, partialView, "list", order);
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }

                }, error: function (err) {

                    appalert(err.statusText, 3, 1);
                }
            });
        },
        function () {
            appalert('Transaction cancelled', 3, 1);

        });


}
function AjaxPostAndUpdateJournalEntry(form) {  
     
    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success)
                {
                    if (response.status === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else if (response.status === "Failed") {
                        appalert(response.message, 2, 1);
                    }
                    else {
                        appalert(response.message, 1, 1);

                    } 
                    LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable","EntryTempData", "", "_JournalEntries", "list","desc");
                     
                }
                else {
                    
                        appalert(response.message, 3, 1);
                    LoadJournalEntryData("ManuallyJournalEntry", "InitializeData", "datalistingview_JournalEntries", "JournalEntryDataTable", "EntryTempData", "", "_JournalEntries", "list", "desc");


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
function loadPartialView2(nodeId, view, path, serviceOption, divToLoadContent) {
    // Use AJAX to load the partial view based on the nodeId
    $.ajax({
        url: '/AccountingConfiguration/InitializeData?KEY=' + nodeId + '&partialView=' + view + '&path=' + path + '&serviceOption=' + serviceOption,
        type: 'GET',
        data: { nodeId: nodeId },
        success: function (result) {
            // Assuming you have a container where you want to display the partial view
            $('.' + divToLoadContent).html(result);
        },
        error: function (error) {
            /*console.error('Error loading partial view:', error);*/
            alert('Error loading partial view:', error);
        }
    });
}

