$(document).ready(function () {
  
    $("#loadChartOfAccount").click(function () {
        LoadChartOfAccounts();
    });
});


function LoadChartOfAccounts() {
    console.log("LoadChartOfAccounts");
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
    // Example: Load a partial view based on the clicked node
    //loadPartialView3(nodeId,"_Operation","Transit");
  //  loadPartialView2(id, partialview);
});

function GetObject(partialview, path) {
    var id = $("#selectedID").val();

    loadPartialView3(id, partialview, path);  
    loadCartegoryId(id);
}
function loadPartialView3(nodeId,view,path) {
    // Use AJAX to load the partial view based on the nodeId
    $.ajax({
        url: '/AccountingChart/InitializeData?KEY=' + nodeId + '&partialView=' + view + '&path=' + path ,
        type: 'GET',
        data: { nodeId: nodeId },
        success: function (result) {
            // Assuming you have a container where you want to display the partial view
            $('.viewSelector').html(result);
        
        },
        error: function (error) {
            /*console.error('Error loading partial view:', error);*/
            alert('Error loading partial view:', error);
        }
    });
}

function loadPartialView2(nodeId, view) {
    // Use AJAX to load the partial view based on the nodeId
    $.ajax({
        url: '/AccountingChart/InitializeData?KEY=' + nodeId + '&partialView=' + view + '&path=null',
        type: 'GET',
        data: { nodeId: nodeId },
        success: function (result) {
            // Assuming you have a container where you want to display the partial view
            $('.viewSelector').html(result);
        },
        error: function (error) {
            /*console.error('Error loading partial view:', error);*/
            alert('Error loading partial view:', error);
        }
    });
}

function loadCartegoryId(accountNumber) {

    // Make an AJAX request to fetch the OperationEventAttributeIds based on the selected OperationEventId
    $.ajax({
        url: '/AccountingChart/InitializeData?KEY=' + accountNumber + '&partialView=null&path=Cartegory',
        type: 'GET',
        dataType: 'json',
        data: { accountNumber: accountNumber },
        success: function (data) {
            // Clear existing options in the OperationEventAttributeId combo
            $('#AccountCartegoryId').empty();

            // Add new options based on the fetched data
            $.each(data, function (index, item) {
                $('#AccountCartegoryId').append($('<option>').text(item.Name).attr('value', item.Id));
            });
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
        }
    });
}