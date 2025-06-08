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
                    if (response.state === "Exist") {
                        appalert(response.message, 3, 1);
                    }
                    else if (response.state === "Expired") {
                        appalert(response.message, 2, 1);
                        window.location.href = response.urldirect;
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
