// ==========================
// Session & Connectivity Script (Revised)
// Requirement: When countdown hits 0, DO NOT auto-logout.
//              Wait for the next user interaction anywhere, then logout.
// ==========================

// ==========================
// Section A: Idle Time Management (Shared Across All Tabs)
// ==========================
let sessionTimeoutMinutes = 10;
let sessionWarningMinutes = 2;
let maxSessionMinutes = 240;

let warningTimer, logoutTimer, modalCountdownTimer, overallSessionTimer;
let sessionRemainingSeconds;
let overallSessionSeconds = maxSessionMinutes * 60;
let lastStorageSync = 0;

const STORAGE_KEY = "sessionExpireAt";
const LAST_INTERACTION_KEY = "lastInteraction";

// ---- Grace-state management (post-expiry wait-for-click) ----
let inGraceState = false;
let graceListenersAttached = false;
const graceEvents = ['click', 'keydown', 'touchstart', 'pointerdown', 'mousedown', 'scroll', 'wheel'];
const graceTargets = [window, document];

function onGraceInteractionOnce() {
    // Any user interaction after expiry triggers logout
    detachGraceInteractionHooks();
    forceLogout();
}

function attachGraceInteractionHooks() {
    if (graceListenersAttached) return;
    // Capture-phase to run before anything can stop propagation (e.g., modal backdrops)
    graceTargets.forEach(t => {
        graceEvents.forEach(evt => t.addEventListener(evt, onGraceInteractionOnce, { capture: true, passive: true }));
    });
    // body may not exist yet in some edge cases; guard it
    if (document.body) {
        graceEvents.forEach(evt => document.body.addEventListener(evt, onGraceInteractionOnce, { capture: true, passive: true }));
        graceTargets.push(document.body);
    }
    graceListenersAttached = true;
}

function detachGraceInteractionHooks() {
    if (!graceListenersAttached) return;
    graceTargets.forEach(t => {
        graceEvents.forEach(evt => t.removeEventListener(evt, onGraceInteractionOnce, { capture: true }));
    });
    graceListenersAttached = false;
}

function enterGraceState() {
    if (inGraceState) return;
    inGraceState = true;

    // Stop active timers
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    // Show modal and freeze countdown at 0 (no auto-logout)
    const modal = $('#sessionTimeoutModal');
    if (modal.length && !modal.hasClass('show')) modal.modal('show');
    updateCountdownDisplay(0);

    // Attach high-priority "any interaction" hooks
    attachGraceInteractionHooks();
}

function exitGraceState() {
    if (!inGraceState) return;
    inGraceState = false;
    detachGraceInteractionHooks();
}

// ------------------------------------------------------------

// Synchronize timers across tabs
window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
        if (!isNaN(expireAt) && Date.now() >= expireAt) {
            // Another tab expired: mirror grace state (no auto-logout)
            enterGraceState();
        } else {
            // Not expired: resume normal session timing
            exitGraceState();
            resetSessionTimers();
        }
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
    localStorage.setItem(STORAGE_KEY, String(expireAt));
    localStorage.setItem(LAST_INTERACTION_KEY, String(Date.now()));
    lastStorageSync = Date.now();
}

/**
 * Start session timers based on the expiration timestamp in local storage.
 * Initializes the warning and fallback timers. (Fallback enters grace state; no auto-logout.)
 */
function startSessionTimers() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    let expireAt = localStorage.getItem(STORAGE_KEY);
    if (!expireAt || isNaN(expireAt)) {
        setSessionExpireAt();
        expireAt = localStorage.getItem(STORAGE_KEY);
    }

    const now = Date.now();
    const expireAtNum = parseInt(expireAt, 10);

    if (now >= expireAtNum) {
        // Already expired: enter grace state (no auto-logout)
        enterGraceState();
        return;
    }

    exitGraceState(); // ensure we're not in grace state

    sessionRemainingSeconds = Math.floor((expireAtNum - now) / 1000);
    const warningTime = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
    const logoutTime = sessionRemainingSeconds * 1000;

    if (warningTime > 0) {
        warningTimer = setTimeout(showWarningModal, warningTime);
    }

    // Fallback: when the raw expiry timestamp is reached, enter grace state (no auto-logout)
    logoutTimer = setTimeout(() => {
        enterGraceState();
    }, logoutTime);

    // Pre-expiry interaction: reset timers (light debounce)
    attachPreExpiryInteractionHandlers();
}

// Debounced pre-expiry reset
let preExpiryHandlerAttached = false;
function onPreExpiryInteraction() {
    if (inGraceState) return; // do nothing during grace; clicks handled by grace hooks
    const now = Date.now();
    if (now - lastStorageSync > 400) {
        resetSessionTimers();
    }
}
function attachPreExpiryInteractionHandlers() {
    if (preExpiryHandlerAttached) return;
    ['click', 'keypress', 'scroll', 'touchstart', 'mousemove'].forEach(event => {
        document.addEventListener(event, onPreExpiryInteraction, { passive: true });
    });
    preExpiryHandlerAttached = true;
}

/**
 * Reset session timers and update expiration timestamp.
 */
function resetSessionTimers() {
    if (inGraceState) {
        // Do not revive a session during grace; wait for user interaction to logout.
        return;
    }

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
 * At 0, enter grace state and wait for next user interaction to logout.
 */
function startModalCountdown() {
    clearInterval(modalCountdownTimer);

    modalCountdownTimer = setInterval(() => {
        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
        const now = Date.now();
        let remainingSeconds = Math.floor((expireAt - now) / 1000);

        if (remainingSeconds <= 0) {
            updateCountdownDisplay(0);
            clearInterval(modalCountdownTimer);
            enterGraceState(); // <-- key change: NO auto-logout
        } else {
            updateCountdownDisplay(remainingSeconds);
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
    const el = document.getElementById('countdown');
    if (el) el.innerText = `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
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

            if (res && res.success) {
                label.textContent = 'AD: ' + res.data;
                label.style.color = 'gray';
                if (icon) icon.style.display = 'inline';
                if (error) error.textContent = '';
            } else {
                label.textContent = 'AD: N/A';
                label.style.color = 'red';
                if (icon) icon.style.display = 'none';
                if (error) error.textContent = 'Error: Could not load accounting day.';
            }
        },
        error: function () {
            const label = document.getElementById("accountingDayLabel");
            label.textContent = 'AD: Error';
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
    if (!banner) return;

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
 * Keep as hard cap. If you want grace here too, replace forceLogout() with enterGraceState().
 */
function startOverallSessionTimer() {
    clearInterval(overallSessionTimer);
    overallSessionSeconds = maxSessionMinutes * 60;

    overallSessionTimer = setInterval(() => {
        overallSessionSeconds--;
        if (overallSessionSeconds <= 0) {
            clearInterval(overallSessionTimer);
            forceLogout(); // or: enterGraceState();
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
    detachGraceInteractionHooks();

    window.location.href = '/Authentication/Logout';
}

/**
 * Handles the session renewal action when the user clicks "Stay Signed In".
 * If already expired, per requirement, next interaction should logout — so we do nothing here.
 */
function handleRenewSession() {
    if (inGraceState) {
        // Do not revive session after expiry; await any interaction to logout
        return;
    }
    resetSessionTimers();
}

// ==========================
// Event Listeners
// ==========================
document.addEventListener("DOMContentLoaded", function () {
    const logoutBtn = document.getElementById('logoutSessionBtn');
    const renewBtn = document.getElementById('renewSessionBtn');

    if (logoutBtn) logoutBtn.addEventListener('click', forceLogout);
    if (renewBtn) renewBtn.addEventListener('click', handleRenewSession);

    fetchIdleTimeoutConfig();
    fetchAccountingDate();
    startOverallSessionTimer();

    // Refresh accounting date every 5 mins
    setInterval(fetchAccountingDate, 5 * 60 * 1000);
});

// Do NOT auto-logout when tab becomes visible after expiry. Enter grace state instead.
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible') {
        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
        if (Date.now() >= expireAt) {
            enterGraceState();
        }
    }
});
