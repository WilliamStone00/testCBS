// ==========================
// Section A: Idle Time Management (Shared Across All Tabs)
// ==========================
let sessionTimeoutMinutes = 10;
let sessionWarningMinutes = 2;
let maxSessionMinutes = 240;

let warningTimer, logoutTimer, countdownTimer, modalCountdownTimer, overallSessionTimer;
let sessionRemainingSeconds;
let overallSessionSeconds = maxSessionMinutes * 60;
let lastStorageSync = 0;

const STORAGE_KEY = "sessionExpireAt";
const LAST_INTERACTION_KEY = "lastInteraction";

// Synchronize timers across tabs
window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
        resetSessionTimers();
    }
});

/**
 * Fetch idle timeout configuration from the server.
 * Updates the session and warning timeout values and initializes the session timers.
 */
function fetchIdleTimeoutConfig() {
    fetch('/Session/GetIdleTimeout', { cache: 'no-store' })
        .then(response => response.json())
        .then(config => {
            sessionTimeoutMinutes = config.Timeout || 10;
            sessionWarningMinutes = config.Warning || 2;
            setSessionExpireAt();
            startSessionTimers();
        })
        .catch(() => {
            setSessionExpireAt();
            startSessionTimers();
        });
}

/**
 * Set the session expiration time and update it in local storage.
 */
function setSessionExpireAt() {
    const expireAt = Date.now() + (sessionTimeoutMinutes * 60 * 1000);
    localStorage.setItem(STORAGE_KEY, expireAt);
    localStorage.setItem(LAST_INTERACTION_KEY, Date.now());
    lastStorageSync = Date.now();
}

/**
 * Start session timers based on the expiration timestamp in local storage.
 * Initializes the warning and logout timers.
 */
function startSessionTimers() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    let expireAt = localStorage.getItem(STORAGE_KEY);
    if (!expireAt || isNaN(expireAt) || Date.now() >= parseInt(expireAt)) {
        setSessionExpireAt();
        expireAt = localStorage.getItem(STORAGE_KEY);
    }

    sessionRemainingSeconds = Math.floor((parseInt(expireAt) - Date.now()) / 1000);
    const warningTime = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
    const logoutTime = sessionRemainingSeconds * 1000;

    if (warningTime > 0) {
        warningTimer = setTimeout(showWarningModal, warningTime);
    }

    logoutTimer = setTimeout(forceLogout, logoutTime);

    // Reset timers on interaction
    ['click', 'keypress', 'scroll', 'touchstart'].forEach(event => {
        document.addEventListener(event, resetSessionTimers);
    });
}

/**
 * Reset session timers and update expiration timestamp.
 */
function resetSessionTimers() {
    $('#sessionTimeoutModal').modal('hide');
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    setSessionExpireAt();
    startSessionTimers();
}

/**
 * Display the warning modal with a countdown timer.
 */
function showWarningModal() {
    $('#sessionTimeoutModal').modal('show');
    startModalCountdown();
}

/**
 * Start the countdown timer in the warning modal.
 */
function startModalCountdown() {
    let expireAt = localStorage.getItem(STORAGE_KEY);
    let remainingSeconds = Math.floor((parseInt(expireAt) - Date.now()) / 1000);

    if (remainingSeconds > sessionWarningMinutes * 60) {
        $('#sessionTimeoutModal').modal('hide');
        return;
    }

    clearInterval(modalCountdownTimer);

    modalCountdownTimer = setInterval(() => {
        remainingSeconds--;
        updateCountdownDisplay(remainingSeconds);

        if (remainingSeconds <= 0) {
            clearInterval(modalCountdownTimer);
            forceLogout();
        }
    }, 1000);
}

/**
 * Update the countdown display in the warning modal.
 * @param {number} seconds - Remaining seconds for the session expiration.
 */
function updateCountdownDisplay(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    document.getElementById('countdown').innerText = `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
}

// ==========================
// Section B: Get Accounting Date
// ==========================

/**
 * Fetch and display the current accounting date.
 */
function fetchAccountingDate() {
    $.ajax({
        url: '/AccountingDay/GetCurrentAccountingDay',
        type: 'GET',
        global: false,
        success: function (res) {
            const label = document.getElementById("accountingDayLabel");
            const icon = document.getElementById("accountingDayStatus");
            const error = document.getElementById("accountingDayError");

            if (res.success) {
                label.textContent = 'Accounting Day: ' + res.data;
                label.style.color = 'gray';
                icon.style.display = 'inline';
                error.textContent = '';
            } else {
                label.textContent = 'Accounting Day: N/A';
                label.style.color = 'red';
                icon.style.display = 'none';
                error.textContent = 'Error: Could not load accounting day.';
            }
        },
        error: function () {
            const label = document.getElementById("accountingDayLabel");
            label.textContent = 'Accounting Day: Error';
            label.style.color = 'red';
        }
    });
}

// ==========================
// Section C: Internet Connection Check
// ==========================

/**
 * Check internet connection and display a warning banner if offline.
 */
function checkInternetConnection() {
    const banner = document.getElementById('internetStatusBanner');
    if (navigator.onLine) {
        banner.style.display = 'none';
    } else {
        banner.style.display = 'block';
        banner.innerHTML = '🚫 No Internet Connection';
        banner.style.backgroundColor = '#dc3545';
    }
}

window.addEventListener('online', checkInternetConnection);
window.addEventListener('offline', checkInternetConnection);
checkInternetConnection();

// ==========================
// Section D: Overall Session Management
// ==========================

/**
 * Start the overall session timer that logs out the user after the maximum session time.
 */
function startOverallSessionTimer() {
    clearInterval(overallSessionTimer);
    overallSessionSeconds = maxSessionMinutes * 60;

    overallSessionTimer = setInterval(() => {
        overallSessionSeconds--;
        if (overallSessionSeconds <= 0) {
            clearInterval(overallSessionTimer);
            forceLogout();
        }
    }, 1000);
}

// ==========================
// Logout Handling
// ==========================

/**
 * Immediately logs out the user and clears all timers.
 */
function forceLogout() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);
    clearInterval(overallSessionTimer);

    window.location.href = '/Authentication/Logout';
}

/**
 * Handles the session renewal action when the user clicks "Stay Signed In".
 */
function handleRenewSession() {
    resetSessionTimers();
}

// ==========================
// Event Listeners
// ==========================
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById('logoutSessionBtn').addEventListener('click', forceLogout);
    document.getElementById('renewSessionBtn').addEventListener('click', handleRenewSession);

    fetchIdleTimeoutConfig();
    fetchAccountingDate();
    startOverallSessionTimer();

    setInterval(fetchAccountingDate, 5 * 60 * 1000); // Refresh accounting date every 5 mins
});
