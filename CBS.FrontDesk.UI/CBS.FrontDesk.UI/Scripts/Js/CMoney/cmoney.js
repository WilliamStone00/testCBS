$(document).ready(function () {
    // Event delegation for handling button clicks
    $(document).on("click", ".btn", function () {
        // Remove underline and blue color from all buttons
        $(".btn").removeClass("clicked");
        // Add underline and blue color to the clicked button
        $(this).addClass("clicked");
    });

    // Handle checkbox state changes
    // Format date mask



});
function AjaxPostAndUpdateActivateCMoneyMembersOTP(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {

        var name = $("#memberName").val();
        var phonNumberID = $("#otpphonNumber").val();

        alertify.confirm("C-MONEY Services Activation", "Are you sure you want to send OTP code to " + name + " [" + phonNumberID +"] ? ",
            function () {


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

                                $("#phoneNumberId").val(response.phonenumber);

                                //GetMemberData(response.activationid, '_ActivationDesk', 'datalistingview', 'reload');
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
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig["contentType"] = false;
                    ajaxConfig["processData"] = false;
                }
                console.log(ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );
    }
    return false;


}


function AjaxPostAndUpdateActivateCMoneyMembers(form) {


    $.validator.unobtrusive.parse(form);
    if ($(form).valid()) {

        var name = $("#memberName").val();

        alertify.confirm("C-MONEY Services Activation", "Are you sure you want to activate C-MONEY Mobile Services for " + name + " ? ",
            function () {


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

                                GetActivation(response.activationid);

                                //GetMemberData(response.activationid, '_ActivationDesk', 'datalistingview', 'reload');
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
                        console.log(err.statusText);
                        appalert(err.statusText, 0, 1);
                    }
                };

                if ($(form).attr('enctype') === "multipart/form-data") {
                    ajaxConfig["contentType"] = false;
                    ajaxConfig["processData"] = false;
                }
                console.log(ajaxConfig);
                $.ajax(ajaxConfig);
            },
            function () {
                appalert('Transaction cancelled', 3, 1);

            }

        );
    }
    return false;


}

function GetActivation(KEY) {
    $.ajax({
        type: "GET",
        url: '/CMoneyMembership/GetActivatatedMember?Key=' + KEY,
        success: function (data) {
            // Update input fields
            $("#phoneNumberId").val(data.PhoneNumber);
            $("#loginId").val(data.LoginId);
            $("#otpid").val('0000');

            // Update activation status with icon
            const statusIndicator = $(".status-indicator");
            if (data.IsActive) {
                statusIndicator.html('<span class="badge bg-success py-2 px-4"><i class="mdi mdi-check-circle-outline me-2"></i> Member is <strong>Activated</strong></span>');
            } else {
                statusIndicator.html('<span class="badge bg-danger py-2 px-4"><i class="mdi mdi-alert-circle-outline me-2"></i> Member is <strong>Not Activated</strong></span>');
            }
        },
        error: function (err) {
            appalert(err.statusText, 1, 3);
        }
    });
}


function GetMember() {
    var operation = $("#currentselectedOperation").val();
    var memberId = $('#manualSearchInput').val().trim();

    if (!memberId) {

        alert("Please enter a valid Member Reference Number to search.");
        return;
    }

    GetMemberData(memberId, '_ActivationDesk', 'datalistingview', operation);
}


function GetMemberData(Key, partialView, divToloadPV, path) {
    $("#currentselectedOperation").val(path);
    var spanElement = document.getElementById('cashDeskOperations');

    AddORUpdateGen(Key, divToloadPV, partialView, path, "CMoneyMembership");
}





function ShowImagePreview(input, imgElement) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            imgElement.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Function to show image preview
function ShowImagePreview(input, imgElement) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            imgElement.src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Function to open camera interface
function OpenCamera() {
    const modalHtml = `
            <div class="modal fade" id="cameraModal" tabindex="-1" role="dialog">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Capture Photo</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body text-center">
                            <video id="videoElement" autoplay style="width: 100%; height: auto;"></video>
                            <canvas id="canvasElement" style="display:none;"></canvas>
                        </div>
                        <div class="modal-footer">
                            <button id="captureButton" class="btn btn-primary">Capture</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    const cameraModal = new bootstrap.Modal(document.getElementById('cameraModal'), {});
    cameraModal.show();

    const video = document.getElementById('videoElement');
    const canvas = document.getElementById('canvasElement');
    const capture = document.getElementById('captureButton');

    navigator.mediaDevices.getUserMedia({ video: true })
        .then(stream => {
            video.srcObject = stream;
            capture.onclick = () => {
                const context = canvas.getContext('2d');
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                context.drawImage(video, 0, 0, canvas.width, canvas.height);
                const dataURL = canvas.toDataURL('image/png');
                document.getElementById('previewimage').src = dataURL;
                video.srcObject.getTracks().forEach(track => track.stop());
                cameraModal.hide();
                document.getElementById('cameraModal').remove(); // Clean up modal after use
            };
        })
        .catch(err => {
            alert('Unable to access the camera. Please check your device permissions.');
            cameraModal.hide();
            document.getElementById('cameraModal').remove(); // Clean up modal if camera access fails
        });
}

// Bind function to the global scope
window.OpenCamera = OpenCamera;