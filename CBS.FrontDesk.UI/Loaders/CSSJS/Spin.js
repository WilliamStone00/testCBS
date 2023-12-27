$(document)
    .ajaxStart(function () {
        $("#loading").fadeIn();
    })
    .ajaxStop(function () {
        $("#loading").fadeOut();
    });


