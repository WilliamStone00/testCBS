let sessionTimeoutMinutes = 15; // Total allowed idle time
let sessionWarningMinutes = 2; // Warning before timeout
let warningTimer;
let logoutTimer;
let countdownTimer;
let countdownSeconds = 30;
let sessionRemainingSeconds = sessionTimeoutMinutes * 60;
let floatingCountdownTimer;



async function fetchSessionConfigAndStart() {
    try {
        console.log("Fetching session config...");
        const response = await fetch('/Session/GetIdleTimeout', { cache: 'no-store' });

        if (!response.ok) throw new Error('Failed to fetch session timeout config.');

        const config = await response.json(); // ✅ await this too
        console.log("Session config loaded:", config);

        sessionTimeoutMinutes = config.Timeout || 5;
        sessionWarningMinutes = config.Warning || 2;

        setSessionExpireAt();
        startSessionTimers();
    } catch (err) {
        console.error('⚠️ Could not load session config:', err);
        setSessionExpireAt(); // ✅ don't skip these
        startSessionTimers();
    }
}



function setSessionExpireAt() {
    const expireAt = Date.now() + (sessionTimeoutMinutes * 60 * 1000);
    localStorage.setItem('sessionExpireAt', expireAt);
}
window.addEventListener('storage', function (e) {
    if (e.key === 'sessionExpireAt') {
        clearTimeout(warningTimer);
        clearTimeout(logoutTimer);
        clearInterval(countdownTimer);
        clearInterval(floatingCountdownTimer);
        startSessionTimers();
    }
});
function startSessionTimers() {
    clearInterval(floatingCountdownTimer);

    let expireAt = localStorage.getItem('sessionExpireAt');

    // ✅ Validate and reset if missing or expired
    if (!expireAt || isNaN(expireAt) || Date.now() >= parseInt(expireAt)) {
        console.warn("⚠️ Invalid or expired sessionExpireAt found, resetting...");
        setSessionExpireAt();
        expireAt = localStorage.getItem('sessionExpireAt');
    }

    sessionRemainingSeconds = Math.max(0, Math.floor((parseInt(expireAt) - Date.now()) / 1000));

    // ⛔ If still expired or bad timing, force logout
    if (sessionRemainingSeconds <= 0) {
        console.warn("⛔ Session already expired or invalid.");
        forceLogout();
        return;
    }

    const warningTime = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
    const logoutTime = sessionRemainingSeconds * 1000;

    // ⏳ Schedule modal only if enough time remains
    if (warningTime > 0) {
        warningTimer = setTimeout(showWarningModal, warningTime);
    }

    logoutTimer = setTimeout(forceLogout, logoutTime);

    startFloatingCountdown();
}


//function startSessionTimers() {
//    if (sessionRemainingSeconds <= 0) {
//        console.warn("⛔ Session already expired before timers started.");
//        forceLogout();
//        return;
//    }

//    clearInterval(floatingCountdownTimer);

//    let expireAt = localStorage.getItem('sessionExpireAt');

//    if (!expireAt) {
//        setSessionExpireAt();
//        expireAt = localStorage.getItem('sessionExpireAt');
//    }

//    sessionRemainingSeconds = Math.max(0, Math.floor((new Date(expireAt) - Date.now()) / 1000));
//    // 🕐 Only warn if remaining time > warning time
//    if (sessionRemainingSeconds > sessionWarningMinutes * 60) {
//        const warningTime = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
//        warningTimer = setTimeout(showWarningModal, warningTime);
//    }
//    logoutTimer = setTimeout(forceLogout, logoutTime);

//    startFloatingCountdown();
//}
// 🌟 Background ping every 2 minutes to refresh session automatically
setInterval(function () {
    fetch('/Session/KeepAlive', {
        method: 'GET',
        cache: 'no-store'
    }).catch(() => {
        // Optionally handle error silently
        console.warn('Background session ping failed');
    });
}, 2 * 60 * 1000); // every 2 minutes


function resetSessionTimers() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(countdownTimer);
    clearInterval(floatingCountdownTimer);

    removeBlurAndOverlay();

    setSessionExpireAt(); // updates localStorage and resets expiration
    startSessionTimers(); // restarts countdown and modal trigger logic
}


function showWarningModal() {
    $('#sessionTimeoutModal').modal('show');
    addBlurAndOverlay();
    startCountdown();
}

function startCountdown() {
    updateCountdownDisplay();

    countdownTimer = setInterval(function () {
        sessionRemainingSeconds--;

        if (sessionRemainingSeconds <= 0) {
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
        const expireAt = localStorage.getItem('sessionExpireAt');
        sessionRemainingSeconds = Math.floor((expireAt - Date.now()) / 1000);

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
            startSessionTimers(); // ⬅ Load timeout from DB first
        },
        error: function () {
            forceLogout();
        }
    });
}
$('#sessionTimeoutModal').on('hidden.bs.modal', function () {
    removeBlurAndOverlay();
    clearInterval(countdownTimer);
    clearInterval(floatingCountdownTimer);
});

function updateCountdownDisplay() {
    const countdownElement = document.getElementById('countdown');

    if (countdownElement) {
        const minutes = Math.floor(sessionRemainingSeconds / 60);
        const seconds = sessionRemainingSeconds % 60;

        countdownElement.innerText = `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;

        // Style & color transitions
        countdownElement.style.fontSize = '2.5rem';
        countdownElement.style.fontWeight = 'bold';

        if (sessionRemainingSeconds > 180) {
            countdownElement.style.color = '#003366'; // Blue
        } else if (sessionRemainingSeconds > 60) {
            countdownElement.style.color = '#ff9900'; // Orange
        } else {
            countdownElement.style.color = '#ff0000'; // Red
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
    fetchSessionConfigAndStart(); // ⬅ Load timeout from DB first
    // 🌐 Listen to online/offline events
    window.addEventListener('online', checkInternetConnection);
    window.addEventListener('offline', checkInternetConnection);

    $('#renewSessionBtn').click(function () {
        renewSession();
    });

    $('#logoutSessionBtn').click(function () {
        forceLogout();
    });

    // 🛡️ Reset session timers on any user activity
    //const events = ['click', 'mousemove', 'keypress', 'scroll', 'touchstart'];
    const events = ['click', 'keypress', 'scroll', 'touchstart'];

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
    //if ($('#sessionTimeoutModal').is(':visible')) {
    //    $('#sessionTimeoutModal').modal('hide');
    //    resetSessionTimers(); // renew session
    //}
    if (document.getElementById("accountingDayLabel")) {
        fetchAccountingDate();
        setInterval(fetchAccountingDate, 5 * 60 * 1000); // every 5 minutes
    }
});

