$(document).ready(function () {
    let isOffline = false;

    function checkInternetConnection() {
        console.log("Terminal is checking for internet connection");
        if (!navigator.onLine && !isOffline) {
            if ($('#connectionModal').length === 0) {
                $('body').append(`
                <div id="connectionModal" style="
                    display: none;
                    position: fixed;
                    z-index: 9999;
                    left: 0;
                    top: 0;
                    width: 100%;
                    height: 100%;
                    overflow: auto;
                    background-color: rgba(0,0,0,0.4);
                ">
                    <div style="
                        background-color: #fefefe;
                        margin: 15% auto;
                        padding: 20px;
                        border: 1px solid #888;
                        width: 80%;
                        text-align: center;
                    ">
                        <h1 id="modalMessage" style="font-size: 48px;"></h1>
                        <!--<button id="closeModal" style="font-size:1.5em; padding:10px 20px; margin-top:20px;">Close</button> -->
                    </div>
                </div>
            `);

                // Add click event to close button
                $('#closeModal').on('click', function () {
                    $('#connectionModal').hide();
                });
            }

            $('#modalMessage').text('Terminal is disconnected').css('color', 'red');
            $('#connectionModal').show();
            isOffline = true;
            console.log("Terminal is disconnected from the internet");
        } else if (navigator.onLine && isOffline) {
            $('#modalMessage').text('Connection has been restored').css('color', 'green');
            $('#connectionModal').show();
            setTimeout(function () {
                $('#connectionModal').hide();
                $('#modalMessage').text('Terminal is disconnected').css('color', 'red');
            }, 5000);
            isOffline = false;
        }
    }

    // Check connection status every 5 seconds
    setInterval(checkInternetConnection, 5000);

    // Also check when online/offline events are triggered
    $(window).on('online offline', checkInternetConnection);
});
