
const sessionTimeoutMinutes = 5;  // Total allowed idle time
const sessionWarningMinutes = 2;  // Warning before timeout
let warningTimer;
let logoutTimer;
let countdownTimer;
let countdownSeconds = 30;
let sessionRemainingSeconds = sessionTimeoutMinutes * 60;
let floatingCountdownTimer;

function startSessionTimers() {
    clearInterval(floatingCountdownTimer);
    sessionRemainingSeconds = sessionTimeoutMinutes * 60;

    const warningTime = (sessionTimeoutMinutes - sessionWarningMinutes) * 60 * 1000;
    const logoutTime = sessionTimeoutMinutes * 60 * 1000;

    warningTimer = setTimeout(showWarningModal, warningTime);
    logoutTimer = setTimeout(forceLogout, logoutTime);

    startFloatingCountdown();
}
// 🌟 Background ping every 2 minutes to refresh session automatically
setInterval(function () {
    $.ajax({
        url: '/Session/KeepAlive',
        method: 'GET',
        cache: false
    });
}, 2 * 60 * 1000); // every 2 minutes

function resetSessionTimers() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(countdownTimer);
    clearInterval(floatingCountdownTimer);
    removeBlurAndOverlay();
    startSessionTimers();
}

function showWarningModal() {
    $('#sessionTimeoutModal').modal('show');
    addBlurAndOverlay();
    startCountdown();
}

function startCountdown() {
    // ⏳ Instead of forcing 30s, calculate based on true remaining session time
    countdownSeconds = sessionRemainingSeconds; // True remaining time when popup shows
    updateCountdownDisplay();

    countdownTimer = setInterval(function () {
        countdownSeconds--;

        if (countdownSeconds <= 0) {
            clearInterval(countdownTimer);
            forceLogout();
        } else {
            updateCountdownDisplay();
        }
    }, 1000);
}

function checkInternetConnection() {
    const banner = document.getElementById('internetStatusBanner');

    if (navigator.onLine) {
        // ✅ Online - hide internet banner
        if (banner) {
            banner.style.display = 'none';
        }
    } else {
        // 🚫 Offline - show internet banner
        if (banner) {
            banner.style.display = 'flex';
            banner.innerHTML = '🚫 No Internet Connection';
            banner.style.backgroundColor = '#dc3545'; // Red
        }
    }
}




function startFloatingCountdown() {
    const banner = document.getElementById('idleCountdownBanner');
    if (banner) {
        banner.style.display = 'block';
    }

    floatingCountdownTimer = setInterval(function () {
        sessionRemainingSeconds--;

        if (sessionRemainingSeconds <= 0) {
            clearInterval(floatingCountdownTimer);
            if (banner) banner.style.display = 'none';
            forceLogout();
        } else {
            updateFloatingCountdown();
        }
    }, 1000);

    updateFloatingCountdown();
}

function updateFloatingCountdown() {
    const textElement = document.getElementById('floatingCountdownText');

    if (textElement) {
        const minutes = Math.floor(sessionRemainingSeconds / 60);
        const seconds = sessionRemainingSeconds % 60;
        textElement.innerText = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;

        // 🎨 Color Transition based on time
        if (sessionRemainingSeconds > 180) { // > 3 min
            textElement.style.color = '#28a745'; // Green
        } else if (sessionRemainingSeconds > 60) { // > 1 min
            textElement.style.color = '#fd7e14'; // Orange
        } else {
            textElement.style.color = '#dc3545'; // Red
        }
    }
}


function forceLogout() {
    removeBlurAndOverlay();
    const banner = document.getElementById('idleCountdownBanner');
    if (banner) {
        banner.style.display = 'none';
    }
    window.location.href = '/Authentication/Logout?reason=sessiontimeout';
}

function renewSession() {
    $.ajax({
        url: '/Session/KeepAlive',
        method: 'GET',
        success: function () {
            $('#sessionTimeoutModal').modal('hide');
            clearInterval(countdownTimer);
            clearInterval(floatingCountdownTimer);
            removeBlurAndOverlay();
            resetSessionTimers();
        },
        error: function () {
            forceLogout();
        }
    });
}
function updateCountdownDisplay() {
    const countdownElement = document.getElementById('countdown');

    if (countdownElement) {
        countdownElement.innerText = countdownSeconds;

        // ✨ Big Bold Countdown for clarity
        countdownElement.style.fontSize = '2.5rem';
        countdownElement.style.fontWeight = 'bold';

        // 🎨 Strong Color Transition
        if (countdownSeconds > 180) { // > 3 min
            countdownElement.style.color = '#003366'; // Navy Blue (Deep Strong Blue)
        } else if (countdownSeconds > 60) { // > 1 min
            countdownElement.style.color = '#ff9900'; // Strong Deep Orange
        } else {
            countdownElement.style.color = '#ff0000'; // Bright Red
        }
    }
}



function addBlurAndOverlay() {
    const content = document.getElementById('pageContent');
    const overlay = document.getElementById('darkOverlay');
    if (content) {
        content.classList.add('blur-background');
    }
    if (overlay) {
        overlay.style.display = 'block';
    }
}

function removeBlurAndOverlay() {
    const content = document.getElementById('pageContent');
    const overlay = document.getElementById('darkOverlay');
    if (content) {
        content.classList.remove('blur-background');
    }
    if (overlay) {
        overlay.style.display = 'none';
    }
}

$(document).ready(function () {
    // 🌐 Check Internet on load
    checkInternetConnection();

    // 🌐 Listen to online/offline events
    window.addEventListener('online', checkInternetConnection);
    window.addEventListener('offline', checkInternetConnection);
    startSessionTimers();

    $('#renewSessionBtn').click(function () {
        renewSession();
    });

    $('#logoutSessionBtn').click(function () {
        forceLogout();
    });

    // 🛡️ Reset session timers on any user activity
    const events = ['click', 'mousemove', 'keypress', 'scroll', 'touchstart'];

    events.forEach(event => {
        document.addEventListener(event, function () {
            resetSessionTimers();
        });
    });

    // Clean up if modal is manually closed
    $('#sessionTimeoutModal').on('hidden.bs.modal', function () {
        removeBlurAndOverlay();
    });
});


function fetchAccountingDate() {
    $.ajax({
        url: '/AccountingDay/GetCurrentAccountingDay',
        type: 'GET',
        global: false, // 👈 disables triggering #loading or global spinner
        success: function (res) {
            const label = document.getElementById("accountingDayLabel");
            const icon = document.getElementById("accountingDayStatus");
            const error = document.getElementById("accountingDayError");

            if (res.success) {
                label.textContent = 'Accounting Day: ' + res.data;
                label.style.color = 'gray';
                label.style.fontWeight = 'normal';

                icon.style.display = 'inline';
                error.textContent = '';
            } else {
                label.textContent = 'Accounting Day: N/A';
                label.style.color = 'red';
                label.style.fontWeight = 'bold';

                icon.style.display = 'none';
                error.textContent = 'Error: Could not load accounting day.';
            }
        },
        error: function (xhr, status, errorThrown) {
            const label = document.getElementById("accountingDayLabel");
            const icon = document.getElementById("accountingDayStatus");
            const error = document.getElementById("accountingDayError");

            label.textContent = 'Accounting Day: Error';
            label.style.color = 'red';
            label.style.fontWeight = 'bold';

            icon.style.display = 'none';
            error.textContent = 'Error: ' + errorThrown;
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    if (document.getElementById("accountingDayLabel")) {
        fetchAccountingDate();
        setInterval(fetchAccountingDate, 60000);
    }
});