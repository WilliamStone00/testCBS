'use strict';

(function () {
    const phoneMaskList = document.querySelectorAll('.phone-mask'),
        creditCardMask = document.querySelector('.credit-card-mask'),
        expiryDateMask = document.querySelector('.expiry-date-mask'),
        cvvMask = document.querySelector('.cvv-code-mask'),
        datepickerList = document.querySelectorAll('.date-picker'),
        formCheckInputPayment = document.querySelectorAll('.form-check-input-payment');

    // Phone Number
    if (phoneMaskList) {
        phoneMaskList.forEach(function (phoneMask) {
            new Cleave(phoneMask, {
                phone: true,
                phoneRegionCode: 'US'
            });
        });
    }

    // Credit Card
    if (creditCardMask) {
        new Cleave(creditCardMask, {
            creditCard: true,
            onCreditCardTypeChanged: function (type) {
                if (type != '' && type != 'unknown') {
                    document.querySelector('.card-type').innerHTML =
                        '<img src="' + assetsPath + 'img/icons/payments/' + type + '-cc.png" height="28"/>';
                } else {
                    document.querySelector('.card-type').innerHTML = '';
                }
            }
        });
    }

    // Expiry Date Mask
    if (expiryDateMask) {
        new Cleave(expiryDateMask, {
            date: true,
            delimiter: '/',
            datePattern: ['m', 'y']
        });
    }

    // CVV
    if (cvvMask) {
        new Cleave(cvvMask, {
            numeral: true,
            numeralPositiveOnly: true
        });
    }

    // Flat Picker Birth Date
    if (typeof datepickerList != undefined) {
        datepickerList.flatpickr({
            mode: 'range'
        });
    }
    //if (datepickerList) {
    //    datepickerList.forEach(function (datepicker) {
    //        datepicker.flatpickr({
    //            monthSelectorType: 'static'
    //        });
    //    });
    //}

    // Toggle CC Payment Method based on selected option
    if (formCheckInputPayment) {
        formCheckInputPayment.forEach(function (paymentInput) {
            paymentInput.addEventListener('change', function (e) {
                const paymentInputValue = e.target.value;
                if (paymentInputValue === 'credit-card') {
                    document.querySelector('#form-credit-card').classList.remove('d-none');
                } else {
                    document.querySelector('#form-credit-card').classList.add('d-none');
                }
            });
        });
    }
})();
// Call extendSessionTimeout every 5 minutes (300,000 milliseconds)
//setInterval(extendSessionTimeout, 300000);

// Function to extend session timeout
//$(document).ready(function () {
//    // Attach click event to document or specific elements
//    $(document).on('click', function () {
//        extendSessionTimeout();
//    });
//});

//function extendSessionTimeout() {
//    $.ajax({
//        url: '/Session/ExtendSessionTimeout', // Controller action to extend session
//        type: 'GET',
//        success: function (response) {
//            // Success handling
//        },
//        error: function (xhr, status, error) {
//            // Error handling
//        }
//    });
//}
//var warningTimer;

//function startSessionWarning() {
//    // Display a warning message 25 minutes after login (5 minutes before session timeout)
//    warningTimer = setTimeout(function () {
//        // Show a modal, alert, or any notification to warn the user about the session timeout
//        alert("Your session will expire in 5 minutes. Click OK to extend your session.");

//        // Optionally, call the function to extend the session if the user interacts with the notification
//        // extendSessionTimeout();
//    }, 25 * 60 * 1000); // 25 minutes in milliseconds
//}
//var loader = $('#loading');
//// Reset the warning timer on user activity (e.g., mouse click)
//$(document).ready(function () {
//    $(document).on('click', function () {
//        clearTimeout(warningTimer); // Reset the warning timer on user activity
//        startSessionWarning();
//        //loader.hide();// Restart the warning timer after user activity
//    });
//});

 // Assuming 'loader' is the ID of the loader element





// Start the warning timer after successful login


function ShowImagePreview(imageUploader, previewImage) {

    if (imageUploader.files && imageUploader.files[0]) {

        var reader = new FileReader();
        reader.onload = function (e) {

            $(previewImage).attr('src', e.target.result);

        };
        reader.readAsDataURL(imageUploader.files[0]);

    }

}
function ReportView(controller, serviceOption, action, KEY, ReadOptions, path, rptType, ReportName, reportpath, fileTitle, datefrom, dateto, bankid, clientid) {
    $.ajax({
        type: "POST",
        url: '/' + controller + '/' + action + '?serviceoption=' + serviceOption + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&ReportName=' + ReportName + '&rptType=' + rptType + '&bankid=' + bankid + '&clientid=' + clientid + '&reportpath=' + reportpath + '&fileTitle=' + fileTitle + '&dateto=' + dateto + '&datefrom=' + datefrom,
        success: function () {
            appalert("Plaese wait, downloading file", 1);
            window.open("/Reports/" + rptType, "_blank");
        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}
function LoadDataMainExcess(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto, startpath, actionType) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group + '&actionType=' + actionType + '&datefrom=' + datefrom + '&dateto=' + dateto,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDT(tableID);
            LoadCustomerStatistics('Litigation', 'ExcessRefund', null, "List", null, null, "statistics");
        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}

function DownloadFile(url) {
    window.open(url);
}
//$(document).ready(function () {
//    $('.mdatefrom').bootstrapMaterialDatePicker({
//        weekStart: 0, time: false
//    });
//    $('#date').bootstrapMaterialDatePicker({
//        weekStart: 0, time: false
//    });
//    $('.mdateto').bootstrapMaterialDatePicker({
//        weekStart: 0, time: false
//    });
//    $("#btnreload").click(function () {
//        LoadCollectPhaseTwo();
//    });
//    try {
//        $("input[type='text']").each(function () {
//            $(this).attr("autocomplete", "off");
//        });
//    }
//    catch (e) { }
//});


function PrintSingleObject(objectID, option, reportStructureType) {
    var url = "/Reporting/Report?objectID=" + objectID + "&reportStructureType=" + reportStructureType + "&option=" + option;
    $.ajax({
        type: "Get",
        url: "/Reporting/AjaxCaller",
        cache: false,
        success: function (response) {

            if (response.success) {
                appalert("Plaese wait, downloading file", 1);
                window.open(url, '_blank');

            }
            else {


                appalert(response.message, 0);
            }


        }, error: function (err) {

            appalert(err.statusText);
        }
    });


}


function LoadDT(tableID, order) {

    if (order === "desc") {
        var T = '#' + tableID;
        var dataThumbView = $(T).DataTable({
            responsive: false,
            columnDefs: [
                {
                    orderable: true,
                    targets: 0

                }
            ],
            oLanguage: {
                sLengthMenu: "_MENU_",
                sSearch: ""
            },
            aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


            //order: [[0, "desc"]],
            bInfo: true,
            pageLength: 10

        });
    }

    else {
        var T = '#' + tableID;
        var dataThumbView = $(T).DataTable({
            responsive: false,
            columnDefs: [
                {
                    orderable: true,
                    targets: 0

                }
            ],
            oLanguage: {
                sLengthMenu: "_MENU_",
                sSearch: ""
            },
            aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],


            order: [[0, "asc"]],
            bInfo: true,
            pageLength: 10

        });
    }





}

function InfinitiySroll(iTable, iAction, iParams) {
    this.table = iTable;        // Reference to the table where data should be added
    this.action = iAction;      // Name of the conrtoller action
    this.params = iParams;      // Additional parameters to pass to the controller
    this.loading = false;       // true if asynchronous loading is in process
    this.AddTableLines = function (firstItem) {
        this.loading = true;
        this.params.firstItem = firstItem;
        $.ajax({
            type: 'POST',
            url: self.action,
            data: self.params,
            dataType: "html"
        })
            .done(function (result) {
                if (result) {
                    $("#" + self.table).append(result);
                    LoadDTSelect(iTable);
                    self.loading = false;
                }
            })
            .fail(function (xhr, ajaxOptions, thrownError) {
                console.log("Error in AddTableLines:", thrownError);
                appalert(err.statusText + thrownError, 3, 1);
            })
            .always(function () {
                // $("#footer").css("display", "none"); // hide loading info
            });
    }

    var self = this;
    window.onscroll = function (ev) {
        if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight) {
            //User is currently at the bottom of the page
            if (!self.loading) {
                var itemCount = $('#' + self.table + ' tr').length - 1;
                self.AddTableLines(itemCount);
            }
        }
    };
    this.AddTableLines(0);
}


function LoadDTSelect(tableID, order) {
    var T = '#' + tableID;

    var table = $(T).DataTable();

    $('#' + tableID + ' tbody').on('click', 'tr', function () {
        if ($(this).hasClass('selected')) {
            $(this).removeClass('selected');
        }
        else {
            table.$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }
    });
    if (order !== null) {

        order: [[order, "asc"]]

    }
    else {
        order: [[0, "asc"]]
    }
}
function MultiSelectDT(tableID) {
    var T = '#' + tableID;
    $(T).DataTable({
        select: {
            style: 'multi'
        }
    });

}

function LoadDTWithTheckBox(tableID) {

    var T = '#' + tableID;
    var dataThumbView = $(T).DataTable({
        responsive: false,
        columnDefs: [
            {
                orderable: true,
                targets: 0,
                checkboxes: { selectRow: true }
            }
        ],
        dom:
            '<"top"<"actions action-btns"B><"action-filters"lf>><"clear">rt<"bottom"<"actions">p>',
        oLanguage: {
            sLengthMenu: "_MENU_",
            sSearch: ""
        },
        aLengthMenu: [[4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000], [4, 10, 15, 20, 100, 500, 1000, 2000, 5000, 10000]],
        select: {
            style: "multi"
        },
        order: [[1, "asc"]],
        bInfo: false,
        pageLength: 5,
        buttons: [
            {
                text: "<i class='feather icon-search'></i> Search",
                action: function () {
                    $(this).removeClass("btn-secondary");
                    $(".add-new-data").addClass("show");
                    $(".overlay-bg").addClass("show");
                },
                className: "btn-outline-primary"
            }
        ],
        initComplete: function (settings, json) {
            $(".dt-buttons .btn").removeClass("btn-secondary");
        }
    });

    dataThumbView.on('draw.dt', function () {
        setTimeout(function () {
            if (navigator.userAgent.indexOf("Mac OS X") !== -1) {
                $(".dt-checkboxes-cell input, .dt-checkboxes").addClass("mac-checkbox");
            }
        }, 50);
    });

    // To append actions dropdown before add new button
    var actionDropdown = $(".actions-dropodown");
    actionDropdown.insertBefore($(".top .actions .dt-buttons"));


    // Scrollbar
    if ($(".data-items").length > 0) {
        new PerfectScrollbar(".data-items", { wheelPropagation: false });
    }

    // Close sidebar
    $(".hide-data-sidebar, .cancel-data-btn, .overlay-bg").on("click", function () {
        $(".add-new-data").removeClass("show");
        $(".overlay-bg").removeClass("show");
        $("#data-name, #data-price").val("");
        $("#data-category, #data-status").prop("selectedIndex", 0);
    });

    // dropzone init

    // mac chrome checkbox fix
    if (navigator.userAgent.indexOf("Mac OS X") !== -1) {
        $(".dt-checkboxes-cell input, .dt-checkboxes").addClass("mac-checkbox");
    }
}

function DeleteWithRedirect(controller, KEY, option, url_redirect) {

    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY + "&serviceoption=" + option + '&actionType=Delete';
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    console.log(response.success);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        Details(url_redirect);
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
//LoadDataTableNew("Country", "myDataTable", "InitializeData", null, "_Data", 1);
function DeleteRecordDataTable(controller, KEY, tableID, partialView, order,divToLoadTheData) {

    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        LoadDataTableNew(controller, tableID, "InitializeData", KEY, partialView, order, "list", divToLoadTheData)
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

function DeleteData(controller, KEY) {

    alertify.confirm("DELETE WARNING!!!", "Are you sure, you want to delete this file?\nYou won't be able to revert this! ",
        function () {
            var url = "/" + controller + "/Delete?KEY=" + KEY;
            $.ajax({
                type: "Get",
                url: url,
                success: function (response) {
                    console.log(response.success);
                    if (response.success) {
                        appalert(response.message, 1, 1);
                        window.location.reload();
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
function ChangePasswordAjax(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    appalert(response.message, 1, 1);
                    window.location.href = response.urldirect;
                }
                else {
                    appalert(response.message, 2, 1);
                    if (response.state === "Expired") {
                        window.location.href = response.url;
                    }

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
//
function AjaxPostAndUpdateEnableMFA(form) {

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

function AjaxPostAndUpdateX(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    if (response.status === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else if (response.status === "Failed") {
                        appalert(response.message, 2, 1);
                    }
                    else {
                        appalert(response.message, 1, 1);

                    }
                    if (response.option === 'Update' && response.reloadDataView === "Yes") {
                        LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                    }
                    else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                        EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                    }
                    else if (response.reloadDataView === "Yes") {
                        LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                    }
                }
                else {
                    if (response.Status === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else {
                        appalert(response.message, 2, 1);
                    }

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

function PageReload() {
    location.reload();
}

function AjaxPostAndUpdate(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {

                if (response.success) {
                    if (response.status === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else if (response.status === "Failed") {
                        appalert(response.message, 2, 1);
                    }
                    else {
                        appalert(response.message, 1, 1);

                    }
                    if (response.option === 'Update' && response.reloadDataView === "Yes") {
                        LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                    }
                    else if (response.optype === 'Insert' && response.reloadDataView === "Yes") {
                        EditResetMain("KEY", response.option, response.divLoaderCreator, response.controllerName, response.reinitializedActionName, response.groupID);
                    }
                    else if (response.reloadDataView === "Yes") {
                        LoadDataMain(response.controllerName, response.option, response.divLoaderList, response.tableName, response.dataLoaderActionName, "KEY", "List");
                    }
                }
                else {
                    if (response.Status === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else {
                        appalert(response.message, 2, 1);
                    }

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


function EditResetModal(KEY, modalBodyID, ModalcontentID, controller, actionMethod, partialView, modallabelName, labelID) {
    $('#' + labelID).html(modallabelName);
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + actionMethod + '?KEY=' + KEY + '&partialView=' + partialView,
        success: function (data) {
            $('#' + ModalcontentID).html(data);
            $('#' + modalBodyID).modal('show');
        }, error: function (err) {
            appalert(err.statusText, 3, 0);
        }
    });
}

function LoadDataGen(controller, tableID, partialView, order, datalistingview,KEY,serviceOption) {
    LoadDataTableNew(controller, tableID, "InitializeData", KEY, partialView, order, "list", datalistingview, serviceOption);

}
function AddORUpdateGen(KEY, divToLoadData, partialView, path, controller,serviceOption) {
    EditResetMain(KEY, partialView, divToLoadData, controller, "InitializeData", null, path, null, serviceOption);
    $('.select2').select2();
}

function navigateToDetails(url) {
    window.location.href = url;
}
function EditResetMain(KEY, partialView, divID, controller, action, div1, path, div2, serviceOption) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?KEY=' + KEY + '&partialView=' + partialView + '&path=' + path + '&serviceOption=' + serviceOption,
        success: function (data) {
            $('#' + divID).html(data);
            $('#' + div2).hide();
            $('#' + div1).show();
            $('.select2').select2();

        }, error: function (err) {

            appalert(err.statusText, 3, 0);
        }
    });
}
function DetailPage(url) {
    window.open(url);
}

//function appalert(message, state, alertType) {

//    if (alertType === 1) {
//        if (state === 1) {
//            $.notify(message, "success");
//        }
//        else if (state === 2) {
//            $.notify(message, "warn");
//        }
//        else if (state === 3) {
//            $.notify(message, "info");
//        }
//        else {
//            $.notify(message, "error");
//        }
//    }
//    else if (alertType === 2) {
//        if (state === 1) {
//            alertify.alert(message, function () { alertify.message('OK'); });
//        }
//        else if (state === 2) {
//            alertify.confirm("Delete", message, function () { alertify.success('Ok'); },
//                function () {
//                    alertify.error('Transaction cancelled');
//                });
//        }

//    }

//}
function appalert(message, state, alertType) {

    if (alertType === 1) {
        if (state === 1) {
            $("#message_div_success").show();
            $("#message_div_danger").hide();
            $("#message_div_warning").hide();
            $("#message_div_info").hide();
            $("#messagesuccess").html(message);
            toastr.success(message)
            // $.alert(message, "success");
        }
        else if (state === 2) {
            $("#message_div_success").hide();
            $("#message_div_danger").hide();
            $("#message_div_warning").show();
            $("#message_div_info").hide();
            $("#messagewarning").html(message);
            toastr.warning(message)
        }
        else if (state === 3) {
            $("#message_div_success").hide();
            $("#message_div_danger").hide();
            $("#message_div_warning").hide();
            $("#message_div_info").show();
            $("#messageinfo").html(message);
            toastr.info(message)
            //toastr.success('This is a success notification from toastr.')  
            //$.alert(message, "info");
        }
        else {
            $("#message_div_success").hide();
            $("#message_div_danger").show();
            $("#message_div_warning").hide();
            $("#message_div_info").hide();
            $("#messagedanger").html(message);
            toastr.error(message)

            //$.alert(message, "error");
        }
    }
    else if (alertType === 2) {
        if (state === 1) {
            alertify.alert(message, function () { alertify.message('OK'); });
        }
        else if (state === 2) {
            alertify.confirm("Delete", message, function () { alertify.success('Ok'); },
                function () {
                    alertify.error('Transaction cancelled');
                });
        }

    }

}

function LoadDataNoSelect(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto) {
    console.log(dateto);
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group + '&datefrom=' + datefrom + '&dateto=' + dateto,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDT(tableID);
        }, error: function (err) {

            appalert(err.statusText, 3);
        }
    });


}
function LoadDataNoSelectAutodebit(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto, statpath) {
    console.log(dateto);
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group + '&datefrom=' + datefrom + '&dateto=' + dateto,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDT(tableID);
            LoadCustomerStatistics(controller, option, KEY, ReadOptions, datefrom, dateto, statpath);
        }, error: function (err) {

            appalert(err.statusText, 3);
        }
    });



}
function LoadDataTableNew(controller, tableID, action, KEY, partialView, order, path, diveToloadtheData,serviceOption) {
    var encodedURL = '/' + controller + '/' + action +
        '?KEY=' + encodeURIComponent(KEY) +
        '&partialView=' + encodeURIComponent(partialView) +
        '&serviceOption=' + encodeURIComponent(serviceOption) +
        '&path=' + encodeURIComponent(path);

    $.ajax({
        type: "GET",
        url: encodedURL,
        success: function (data) {
            $('#' + diveToloadtheData).html(data);
            LoadDT(tableID, order);
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}


function LoadDataMain(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto, startpath, actionType, order) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group + '&actionType=' + actionType + '&datefrom=' + datefrom + '&dateto=' + dateto + '&startpath=' + startpath,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDT(tableID, order);
            //LoadCustomerStatistics(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto, startpath);
        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}


function LoadCustomerTransaction(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDT(tableID);
            LoadCustomerStatistics(controller, option, KEY, ReadOptions, null, null, null, path);
        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}

function LoadCustomerStatistics(controller, option, KEY, ReadOptions, datefrom, dateto, startpath, service_action) {
    var action = "AjaxCaller";
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + startpath + '&datefrom=' + datefrom + '&dateto=' + dateto + "&",
        success: function (data) {
            $('#loanvolume').html(data.volume_1);
            $('#numberofloans').html(data.counts_1);
            $('#volumeofrefund').html(data.volume_2);
            $('#numberofrefund').html(data.counts_2);
            $('#counts_3').html(data.counts_3);
            $('#counts_4').html(data.counts_4);

            $('#volume_3').html(data.volume_3);
            $('#volume_4').html(data.volume_4);


            $('#counts_5').html(data.counts_5);
            $('#counts_6').html(data.counts_6);
            $('#counts_7').html(data.counts_7);

            $('#volume_5').html(data.volume_5);
            $('#volume_6').html(data.volume_6);
            $('#volume_7').html(data.volume_7);

        }, error: function (err) {

            appalert(err.statusText, 1, 3);
        }
    });


}

function AddorEdit(url, myData) {
    $.ajax({
        type: "POST",
        url: url,
        data: { 'model': myData },
        success: function (response) {
            if (response.success) {
                if (response.status === "Exist") {
                    appalert(response.message, 3, 1);
                }
                else if (response.status === "Failed") {
                    appalert(response.message, 2, 1);
                }
                else {
                    appalert(response.message, 1, 1);

                }
            }
            else {
                if (response.Status === "Exist") {
                    appalert(response.message, 3, 1);
                }
                else {
                    appalert(response.message, 2, 1);
                }

            }

        }
    });
}
function AjaxCall(url, reloadurl) {
    $.ajax({
        type: "POST",
        url: url,
        success: function (response) {
            if (response.success) {
                appalert(response.message, 1, 1);
                if (reloadurl !== null || reloadurl !== "") {
                    window.location.href = reloadurl;
                }

            }
            else {
                appalert(response.message, 2, 1);
            }
        }
    });
}

function AjaxPostObject(url, model) {
    $.ajax({
        type: "POST",
        url: url,
        data: JSON.stringify(model),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                appalert(response.message, 1, 1);
            }
            else {
                appalert(response.message, 2, 1);
            }
        }
    });




}

function AdvancedSearchData(advancedsearch, controller, divLoader, tableID, action, DataTableType) {
    console.log(advancedsearch);
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action,
        data: JSON.stringify({ 'advancedsearch': advancedsearch }),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);
            $('#' + divLoader).html(data);
            if (DataTableType === 'checkbox') {
                LoadDTWithTheckBox(tableID);
            }
            else {
                LoadDT(tableID);
            }

        }, error: function (err) {

            appalert(err.statusText, 3);
        }
    });
}
function LoadDataMainWithSelect(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group, datefrom, dateto, order) {
    console.log(dateto);
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group + '&datefrom=' + datefrom + '&dateto=' + dateto,
        success: function (data) {
            $('#' + divLoader).html(data);
            LoadDTSelect(tableID);
        }, error: function (err) {

            appalert(err.statusText, 3);
        }
    });


}
function LoadDataWithMultipleSelect(controller, option, divLoader, tableID, action, KEY, ReadOptions, path, group) {
    $.ajax({
        type: "GET",
        url: '/' + controller + '/' + action + '?serviceoption=' + option + '&KEY=' + KEY + '&ReadOptions=' + ReadOptions + '&path=' + path + '&group=' + group,
        success: function (data) {
            $('#' + divLoader).html(data);
            MultiSelectDT(tableID);
        }, error: function (err) {

            appalert(err.statusText, 3);
        }
    });


}
var table = null;
$(document).ready(function () {
    //$('#myDataTable tbody').on('click', 'tr', function () {
    //    $(this).toggleClass('selected');
    //});



    //$("#btnsubMitCustomers").on("click", function () {
    //    var Recovery = new Array();
    //    $('#myDataTable tbody tr.selected').each(function () {
    //        var row = $(this);
    //        var DATA = {};
    //        DATA.LoanID = row.find("TD").eq(7).html();
    //        DATA.UserName = row.find("TD").eq(1).html();
    //        DATA.Telephone = row.find("TD").eq(2).html();
    //        DATA.LoanAmount = row.find("TD").eq(3).html();
    //        DATA.Payable = row.find("TD").eq(4).html();
    //        DATA.SMSContentConfigurationID = $("#SMSContentConfigurationID").val();
    //        DATA.SMSTypeID = $("#SMSTypeID").val();
    //        DATA.option = 94;
    //        Recovery.push(DATA);
    //    });

    //    var obj = {
    //        LoanSMSRecoveryTemplates: Recovery,
    //    };
    //    console.log(obj);
    //    $.ajax({
    //        type: "POST",
    //        url: "/LoanRecovery/SENDLoanRecoverySMS",
    //        data: JSON.stringify({ 'model': obj }),
    //        contentType: "application/json;charset=utf-8",
    //        dataType: "json",
    //        success: function (response) {
    //            if (response.success) {
    //                $.notify(response.message, "success");
    //            }
    //            else {
    //                $.notify(response.message, "error");
    //            }

    //        }
    //    });
    //});

    //$("#btngotosendsms").click(function () {
    //    window.location.href = "/LoanRecovery/SendSMS";
    //});


});
var myFunc = function (event) {
    event.stopPropagation();

};
function disableButton(btn, status) {
    document.getElementById(btn).disabled = status;
    alert("Button has been");
}
function Details(url) {
    window.location.href = url;
}

function UploadFile(inputField, url) {

    var T = '#' + inputField;
    var fileUpload = $(T).get(0);
    var files = fileUpload.files;
    var data = new FormData();
    for (var i = 0; i < files.length; i++) {
        data.append(files[i].name, files[i]);
    }
    $.ajax({
        type: "POST",
        url: url,
        contentType: false,
        processData: false,
        data: data,
        success: function (response) {
            if (response.success) {
                appalert(response.message, 1, 1);
            }
            else {
                appalert(response.message, 2, 1);
            }

        }, error: function (err) {

            appalert(err.statusText, 3);
        }
        //success: function (message) {
        //    alert(message);
        //},
        //error: function () {
        //    alert("There was error uploading files!");
        //}
    });
}
function AjaxPostAndUpdateMFA(form) {

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
                else
                    if (response.message === undefined) {
                        alert("Your session is expired.")
                    }
                    else {
                        appalert(response.message, 3, 1);
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

function AjaxPostAndUpdateChangePassword(form) {

    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {
        var ajaxConfig = {
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            success: function (response) {
                if (response.success) {
                    appalert(response.message, 1, 1);
                    window.location.href = "/";
                }
                else {
                    if (response.message === undefined) {
                        alert("Your change password session is expired.")
                    }
                    else {
                        appalert(response.message, 3, 1);
                    }
                }

            }
            , error: function (err) {
                alert("Your session is expired." + err.statusText);
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


