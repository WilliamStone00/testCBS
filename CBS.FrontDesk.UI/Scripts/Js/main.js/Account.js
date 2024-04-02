$(document).ready(function () {
     
    $("#loadChartAccount").click(function () {
        console.log("loadChartAccount");
        loadChartAccount();
    });

    $("#loadAccounts").click(function () {
        console.log("loadAccounts");
        loadAccounts();
    });
    $(document).on('change', '#OperationEventId', function () {
        var EventId = $(this).val();
        console.log("Operation Event Id selected: " + EventId);
        loadOperationEventAttributeIds(EventId);
    });
});
 

function loadOperationEventAttributeIds(operationEventId) {
    console.log(operationEventId);
    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingConfiguration/GetOperationEventAttribute',
        type: 'GET',
        dataType: 'json',
        data: { operationEventId: operationEventId },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#OperationEventAttributeId').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#OperationEventAttributeId').append($('<option>').text(item.Name).attr('value', item.Id));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}
//     <div id="jstree-checkbox"></div>jstree-checkbox    url: "/AccountingConfiguration/InitializeData?KEY=null&partialView=_ChartOfAccountData&path=list&serviceOption=account",
function loadChartAccount() {
    $.ajax({
        url: "/AccountingChart/InitializeData?KEY=null&partialView=null&path=list",
        success: function (data) {

            $('#jstree-checkbox').jstree({
                core: {
                    data: data
                }
            });

        }



    });

}

function loadAccounts() {
    $.ajax({
        url: "/AccountingChart/InitializeData?KEY=null&partialView=null&path=list",
        success: function (data) {

            $('#jstree-context-menu').jstree({
                core: {
                    data: data
                }
            });

        }



    });

}
function AjaxPostAndUpdateChartOfAccount(form) {

    /* $.validator.unobtrusive.parse(form);*/
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {

                    LoadChartOfAccounts();
                }
                else {
                    alert("Error has occured!");
                    appalert(response.message, 1, 4);
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

$('#jstree-context-menu').on('click', '.parent', function (e, data) {

    // Get information about the clicked element or perform specific actions
    var clickedElement = $(this);
    var nodeId = clickedElement.closest('li').attr('id');
    var nodeName = clickedElement.text();

    // Example: Display information about the clicked node
  alert('Clicked Node ID: ' + nodeId + '\nClicked Node Name: ' + nodeName);
    $("#selectedID").val(nodeId);
    $(".RootId").val(nodeId);

});
$('#jstree-checkbox').on('click', '.parent', function (e, data) {

    // Get information about the clicked element or perform specific actions
    var clickedElement = $(this);
    var nodeId = clickedElement.closest('li').attr('id');
    var nodeName = clickedElement.text();

    // Example: Display information about the clicked node
      alert('Clicked Node ID: ' + nodeId + '\nClicked Node Name: ' + nodeName);
    $("#selectedIDchart").val(nodeId);
    $(".RootId").val(nodeId);

});
function GetObjectView(partialview, path, serviceOption, divToLoadContent) {
    var idChart = $("#selectedID").val();
    var idAcc = $("#selectedIDchart").val();
    alert("id from Chart:" + idChart);
    alert("id from Account:" + idAcc);
    loadPartialView2(id, partialview, path, serviceOption, divToLoadContent);
    //loadPartialView(nodeId, "_Operation", "Transit");
}

function GetObjectViewForChart(partialview, path, serviceOption, divToLoadContent) {
    var id = $("#selectedIDchart").val();
    alert("id:" + id);
    loadPartialView2(id, partialview, path, serviceOption, divToLoadContent);
    //loadPartialView(nodeId, "_Operation", "Transit");
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



