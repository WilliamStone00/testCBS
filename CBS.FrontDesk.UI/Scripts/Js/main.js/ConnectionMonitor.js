// ==========================
// Session & Connectivity Script (AWS-style post-expiry popup)
// ==========================

// ---------- Config ----------
let sessionTimeoutMinutes = 10;  // fetched from server
let sessionWarningMinutes = 2;   // fetched from server
let maxSessionMinutes = 240;

// ---------- State ----------
let warningTimer, logoutTimer, modalCountdownTimer, overallSessionTimer;
let sessionRemainingSeconds;
let overallSessionSeconds = maxSessionMinutes * 60;
let lastStorageSync = 0;

const STORAGE_KEY = "sessionExpireAt";
const LAST_INTERACTION_KEY = "lastInteraction";

// Expired popup state
let expiredHooksAttached = false;

// ==========================
// Utilities
// ==========================
function nowMs() { return Date.now(); }
function msFromMinutes(mins) { return mins * 60 * 1000; }

// ==========================
// Section A: Idle Time Management (Shared Across All Tabs)
// ==========================

// Cross-tab sync
window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
        if (!Number.isNaN(expireAt) && nowMs() >= expireAt) {
            enterExpiredState();     // mirror expired popup, no auto-logout
        } else {
            resetSessionTimers();    // resume normal session
        }
    }
});

/** Fetch idle timeout config from server */
function fetchIdleTimeoutConfig() {
    fetch('/Session/GetIdleTimeout', { cache: 'no-store' })
        .then(r => r.json())
        .then(cfg => {
            sessionTimeoutMinutes = cfg.Timeout || 10;
            sessionWarningMinutes = cfg.Warning || 2;
            setSessionExpireAt();
            startSessionTimers();
        })
        .catch(() => {
            setSessionExpireAt();
            startSessionTimers();
        });
}

/** Set expiration in localStorage */
function setSessionExpireAt() {
    const expireAt = nowMs() + msFromMinutes(sessionTimeoutMinutes);
    localStorage.setItem(STORAGE_KEY, String(expireAt));
    localStorage.setItem(LAST_INTERACTION_KEY, String(nowMs()));
    lastStorageSync = nowMs();
}

/** Start timers */
function startSessionTimers() {
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    let expireAtStr = localStorage.getItem(STORAGE_KEY);
    if (!expireAtStr || isNaN(expireAtStr)) {
        setSessionExpireAt();
        expireAtStr = localStorage.getItem(STORAGE_KEY);
    }
    const expireAt = parseInt(expireAtStr, 10);
    const now = nowMs();

    if (now >= expireAt) {
        // Already expired: show AWS-style popup and wait for click
        enterExpiredState();
        return;
    }

    // Pre-expiry
    sessionRemainingSeconds = Math.floor((expireAt - now) / 1000);
    const warningTimeMs = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
    const expiryTimeMs = sessionRemainingSeconds * 1000;

    if (warningTimeMs > 0) {
        warningTimer = setTimeout(showWarningModal, warningTimeMs);
    }

    // When raw expiry time is hit, DO NOT auto-logout; show expired popup instead
    logoutTimer = setTimeout(() => {
        enterExpiredState();
    }, expiryTimeMs);

    attachPreExpiryInteractionHandlers();
}

/** Reset timers (only allowed pre-expiry) */
function resetSessionTimers() {
    if (isExpired()) return; // don’t revive after expiry

    hideWarningModal();
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    setSessionExpireAt();
    startSessionTimers();
}

/** Pre-expiry activity: light debounce to reset */
let preExpiryHandlerAttached = false;
function onPreExpiryInteraction() {
    if (isExpired()) return;
    const t = nowMs();
    if (t - lastStorageSync > 400) {
        resetSessionTimers();
    }
}
function attachPreExpiryInteractionHandlers() {
    if (preExpiryHandlerAttached) return;
    ['click', 'keypress', 'scroll', 'touchstart', 'mousemove'].forEach(evt => {
        document.addEventListener(evt, onPreExpiryInteraction, { passive: true });
    });
    preExpiryHandlerAttached = true;
}

function isExpired() {
    const exp = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
    return !Number.isNaN(exp) && nowMs() >= exp;
}

// ==========================
// Section A1: Warning Modal (countdown)
// ==========================
function showWarningModal() {
    // Show your existing Bootstrap modal with id="sessionTimeoutModal"
    const $m = $('#sessionTimeoutModal');
    if ($m.length) {
        $m.modal('show');
        startModalCountdown();
    }
}

function hideWarningModal() {
    const $m = $('#sessionTimeoutModal');
    if ($m.length) $m.modal('hide');
}

function startModalCountdown() {
    clearInterval(modalCountdownTimer);

    modalCountdownTimer = setInterval(() => {
        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
        const remain = Math.floor((expireAt - nowMs()) / 1000);

        if (remain <= 0) {
            clearInterval(modalCountdownTimer);
            // Countdown finished. Hide the warning modal and show the AWS-style popup.
            hideWarningModal();
            enterExpiredState();
        } else {
            updateCountdownDisplay(remain);
        }
    }, 1000);
}

function updateCountdownDisplay(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    const el = document.getElementById('countdown');
    if (el) el.innerText = `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
}

// ==========================
// Section A2: Expired AWS-style Popup
// ==========================

/** Build the AWS-style popup if missing */
// Centered, branded TSC popup.
function ensureExpiredPopup() {
    if (document.getElementById('sessionExpiredPopup')) return;

    const div = document.createElement('div');
    div.id = 'sessionExpiredPopup';
    div.innerHTML = `
    <div class="tsc-expired-backdrop"></div>
    <div class="tsc-expired-card" role="dialog" aria-live="assertive" aria-modal="true" aria-label="Session expired">
        <div class="tsc-expired-header">
            <span class="tsc-expired-title">You have been signed out</span>
            <button type="button" class="tsc-expired-close" aria-label="Close">&times;</button>
        </div>
        <div class="tsc-expired-body">
            You've been signed out of your session. Sessions in all tabs have been signed out.
        </div>
        <div class="tsc-expired-footer">
            <button id="signInAgainBtn" class="tsc-expired-action">Sign in again</button>
        </div>
    </div>
    `;
    document.body.appendChild(div);

    // ===== Centered TSC brand styles =====
    const css = document.createElement('style');
    css.textContent = `
    #sessionExpiredPopup{ position:fixed; inset:0; z-index:1050; display:none; }
    .tsc-expired-backdrop{
        position:absolute; inset:0; background:rgba(0,0,0,.55); backdrop-filter: blur(2px);
    }
    .tsc-expired-card{
        position:absolute; top:50%; left:50%; transform:translate(-50%, -50%);
        width:min(560px, calc(100% - 48px));
        background: linear-gradient(180deg, #0e1b2e 0%, #0b1220 100%);
        color:#e2e8f0;
        border:1px solid rgba(22,163,74,.35);
        border-radius:16px;
        box-shadow:0 18px 48px rgba(0,0,0,.45);
        padding:18px 18px 16px 18px;
        animation: tscPop .18s ease-out;
    }
    .tsc-expired-card::before{
        content:""; position:absolute; left:0; right:0; top:0; height:3px;
        background: linear-gradient(90deg, #16a34a 0%, #14b8a6 50%, #16a34a 100%);
        opacity:.8; border-top-left-radius:16px; border-top-right-radius:16px;
    }
    @keyframes tscPop{ from{ transform:translate(-50%,-50%) scale(.98); opacity:.8 } to{ transform:translate(-50%,-50%) scale(1); opacity:1 } }
    .tsc-expired-header{ display:flex; align-items:center; justify-content:space-between; margin-bottom:8px; }
    .tsc-expired-title{ font-weight:800; font-size:18px; color:#e8fff2; text-shadow:0 0 6px rgba(20,184,166,.25); }
    .tsc-expired-close{
        background:transparent; border:0; color:#c7f9cc; font-size:22px; line-height:1;
        cursor:pointer; padding:2px 6px; border-radius:8px;
    }
    .tsc-expired-close:hover{ color:#ffffff; background:rgba(22,163,74,.1); }
    .tsc-expired-body{ color:#a3b2c7; margin-bottom:14px; font-size:14px; }
    .tsc-expired-footer{ display:flex; justify-content:flex-end; gap:8px; }
    .tsc-expired-action{
        border:0; border-radius:999px; padding:10px 18px; cursor:pointer;
        background:#16a34a; color:#0b1220; font-weight:800; font-size:14px;
        box-shadow:0 8px 22px rgba(22,163,74,.35);
        transition: transform .06s ease, box-shadow .15s ease, background .15s ease;
    }
    .tsc-expired-action:hover{ background:#15803d; box-shadow:0 10px 26px rgba(22,163,74,.45); }
    .tsc-expired-action:active{ transform:translateY(1px); }
    .tsc-expired-action:focus{ outline:2px solid #22c55e; outline-offset:2px; }
    @media (max-width:520px){
        .tsc-expired-card{ width:calc(100% - 32px); padding:16px; }
        .tsc-expired-title{ font-size:16px; }
    }
    `;
    document.head.appendChild(css);

    // Buttons -> logout on click (per your rule: any click after expiry logs out)
    div.querySelector('.tsc-expired-close').addEventListener('click', triggerLogoutOnce);
    div.querySelector('#signInAgainBtn').addEventListener('click', triggerLogoutOnce);
}

/** Show the popup */
function showExpiredPopup() {
    ensureExpiredPopup();
    document.getElementById('sessionExpiredPopup').style.display = 'block';
}

/** Hide the popup */
function hideExpiredPopup() {
    const el = document.getElementById('sessionExpiredPopup');
    if (el) el.style.display = 'none';
}

/** Attach global "any interaction" to logout (after expiry) */
function attachExpiredHooks() {
    if (expiredHooksAttached) return;
    const events = ['click', 'keydown', 'touchstart', 'pointerdown', 'mousedown', 'wheel', 'scroll'];
    events.forEach(evt => window.addEventListener(evt, triggerLogoutOnce, { capture: true, passive: true }));
    expiredHooksAttached = true;
}

/** Detach global hooks */
function detachExpiredHooks() {
    if (!expiredHooksAttached) return;
    const events = ['click', 'keydown', 'touchstart', 'pointerdown', 'mousedown', 'wheel', 'scroll'];
    events.forEach(evt => window.removeEventListener(evt, triggerLogoutOnce, { capture: true }));
    expiredHooksAttached = false;
}

/** Single-shot logout after any interaction */
let logoutFired = false;
function triggerLogoutOnce() {
    if (logoutFired) return;
    logoutFired = true;
    detachExpiredHooks();
    forceLogout();
}

/** Enter expired state: stop timers, hide countdown modal, show AWS-style popup, wait for click */
function enterExpiredState() {
    hideWarningModal();
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);

    showExpiredPopup();

    // Important: attach AFTER showing popup to avoid capturing the same event that opened it
    setTimeout(attachExpiredHooks, 0);
}

// ==========================
// Section B: Accounting Date
// ==========================
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
function startOverallSessionTimer() {
    clearInterval(overallSessionTimer);
    overallSessionSeconds = maxSessionMinutes * 60;

    overallSessionTimer = setInterval(() => {
        overallSessionSeconds--;
        if (overallSessionSeconds <= 0) {
            clearInterval(overallSessionTimer);
            // Hard cap: also show expired popup (no auto-logout)
            enterExpiredState();
        }
    }, 1000);
}

// ==========================
// Logout Handling
// ==========================
function forceLogout() {
    hideExpiredPopup(); // optional
    clearTimeout(warningTimer);
    clearTimeout(logoutTimer);
    clearInterval(modalCountdownTimer);
    clearInterval(overallSessionTimer);
    detachExpiredHooks();
    window.location.href = '/Authentication/Logout';
}

// ==========================
// Boot
// ==========================
document.addEventListener("DOMContentLoaded", function () {
    // Optional: if you keep “Stay Signed In” or “Logout” buttons in your warning modal
    const logoutBtn = document.getElementById('logoutSessionBtn');
    const renewBtn = document.getElementById('renewSessionBtn');
    if (logoutBtn) logoutBtn.addEventListener('click', forceLogout);
    if (renewBtn) renewBtn.addEventListener('click', resetSessionTimers); // pre-expiry only

    fetchIdleTimeoutConfig();
    fetchAccountingDate();
    startOverallSessionTimer();

    setInterval(fetchAccountingDate, 5 * 60 * 1000);
});

// On tab return: if expired, show AWS-style popup; do NOT auto-logout
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible' && isExpired()) {
        enterExpiredState();
    }
});





//// ==========================
//// Session & Connectivity Script (Revised)
//// Requirement: When countdown hits 0, DO NOT auto-logout.
////              Wait for the next user interaction anywhere, then logout.
//// ==========================

//// ==========================
//// Section A: Idle Time Management (Shared Across All Tabs)
//// ==========================
//let sessionTimeoutMinutes = 10;
//let sessionWarningMinutes = 2;
//let maxSessionMinutes = 240;

//let warningTimer, logoutTimer, modalCountdownTimer, overallSessionTimer;
//let sessionRemainingSeconds;
//let overallSessionSeconds = maxSessionMinutes * 60;
//let lastStorageSync = 0;

//const STORAGE_KEY = "sessionExpireAt";
//const LAST_INTERACTION_KEY = "lastInteraction";

//// ---- Grace-state management (post-expiry wait-for-click) ----
//let inGraceState = false;
//let graceListenersAttached = false;
//const graceEvents = ['click', 'keydown', 'touchstart', 'pointerdown', 'mousedown', 'scroll', 'wheel'];
//const graceTargets = [window, document];

//function onGraceInteractionOnce() {
//    // Any user interaction after expiry triggers logout
//    detachGraceInteractionHooks();
//    forceLogout();
//}

//function attachGraceInteractionHooks() {
//    if (graceListenersAttached) return;
//    // Capture-phase to run before anything can stop propagation (e.g., modal backdrops)
//    graceTargets.forEach(t => {
//        graceEvents.forEach(evt => t.addEventListener(evt, onGraceInteractionOnce, { capture: true, passive: true }));
//    });
//    // body may not exist yet in some edge cases; guard it
//    if (document.body) {
//        graceEvents.forEach(evt => document.body.addEventListener(evt, onGraceInteractionOnce, { capture: true, passive: true }));
//        graceTargets.push(document.body);
//    }
//    graceListenersAttached = true;
//}

//function detachGraceInteractionHooks() {
//    if (!graceListenersAttached) return;
//    graceTargets.forEach(t => {
//        graceEvents.forEach(evt => t.removeEventListener(evt, onGraceInteractionOnce, { capture: true }));
//    });
//    graceListenersAttached = false;
//}

//function enterGraceState() {
//    if (inGraceState) return;
//    inGraceState = true;

//    // Stop active timers
//    clearTimeout(warningTimer);
//    clearTimeout(logoutTimer);
//    clearInterval(modalCountdownTimer);

//    // Show modal and freeze countdown at 0 (no auto-logout)
//    const modal = $('#sessionTimeoutModal');
//    if (modal.length && !modal.hasClass('show')) modal.modal('show');
//    updateCountdownDisplay(0);

//    // Attach high-priority "any interaction" hooks
//    attachGraceInteractionHooks();
//}

//function exitGraceState() {
//    if (!inGraceState) return;
//    inGraceState = false;
//    detachGraceInteractionHooks();
//}

//// ------------------------------------------------------------

//// Synchronize timers across tabs
//window.addEventListener("storage", (e) => {
//    if (e.key === STORAGE_KEY) {
//        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
//        if (!isNaN(expireAt) && Date.now() >= expireAt) {
//            // Another tab expired: mirror grace state (no auto-logout)
//            enterGraceState();
//        } else {
//            // Not expired: resume normal session timing
//            exitGraceState();
//            resetSessionTimers();
//        }
//    }
//});

///**
// * Fetch idle timeout configuration from the server.
// * Updates the session and warning timeout values and initializes the session timers.
// */
//function fetchIdleTimeoutConfig() {
//    fetch('/Session/GetIdleTimeout', { cache: 'no-store' })
//        .then(response => response.json())
//        .then(config => {
//            sessionTimeoutMinutes = config.Timeout || 10;
//            sessionWarningMinutes = config.Warning || 2;
//            setSessionExpireAt();
//            startSessionTimers();
//        })
//        .catch(() => {
//            setSessionExpireAt();
//            startSessionTimers();
//        });
//}

///**
// * Set the session expiration time and update it in local storage.
// */
//function setSessionExpireAt() {
//    const expireAt = Date.now() + (sessionTimeoutMinutes * 60 * 1000);
//    localStorage.setItem(STORAGE_KEY, String(expireAt));
//    localStorage.setItem(LAST_INTERACTION_KEY, String(Date.now()));
//    lastStorageSync = Date.now();
//}

///**
// * Start session timers based on the expiration timestamp in local storage.
// * Initializes the warning and fallback timers. (Fallback enters grace state; no auto-logout.)
// */
//function startSessionTimers() {
//    clearTimeout(warningTimer);
//    clearTimeout(logoutTimer);
//    clearInterval(modalCountdownTimer);

//    let expireAt = localStorage.getItem(STORAGE_KEY);
//    if (!expireAt || isNaN(expireAt)) {
//        setSessionExpireAt();
//        expireAt = localStorage.getItem(STORAGE_KEY);
//    }

//    const now = Date.now();
//    const expireAtNum = parseInt(expireAt, 10);

//    if (now >= expireAtNum) {
//        // Already expired: enter grace state (no auto-logout)
//        enterGraceState();
//        return;
//    }

//    exitGraceState(); // ensure we're not in grace state

//    sessionRemainingSeconds = Math.floor((expireAtNum - now) / 1000);
//    const warningTime = (sessionRemainingSeconds - sessionWarningMinutes * 60) * 1000;
//    const logoutTime = sessionRemainingSeconds * 1000;

//    if (warningTime > 0) {
//        warningTimer = setTimeout(showWarningModal, warningTime);
//    }

//    // Fallback: when the raw expiry timestamp is reached, enter grace state (no auto-logout)
//    logoutTimer = setTimeout(() => {
//        enterGraceState();
//    }, logoutTime);

//    // Pre-expiry interaction: reset timers (light debounce)
//    attachPreExpiryInteractionHandlers();
//}

//// Debounced pre-expiry reset
//let preExpiryHandlerAttached = false;
//function onPreExpiryInteraction() {
//    if (inGraceState) return; // do nothing during grace; clicks handled by grace hooks
//    const now = Date.now();
//    if (now - lastStorageSync > 400) {
//        resetSessionTimers();
//    }
//}
//function attachPreExpiryInteractionHandlers() {
//    if (preExpiryHandlerAttached) return;
//    ['click', 'keypress', 'scroll', 'touchstart', 'mousemove'].forEach(event => {
//        document.addEventListener(event, onPreExpiryInteraction, { passive: true });
//    });
//    preExpiryHandlerAttached = true;
//}

///**
// * Reset session timers and update expiration timestamp.
// */
//function resetSessionTimers() {
//    if (inGraceState) {
//        // Do not revive a session during grace; wait for user interaction to logout.
//        return;
//    }

//    $('#sessionTimeoutModal').modal('hide');
//    clearTimeout(warningTimer);
//    clearTimeout(logoutTimer);
//    clearInterval(modalCountdownTimer);

//    setSessionExpireAt();
//    startSessionTimers();
//}

///**
// * Display the warning modal with a countdown timer.
// */
//function showWarningModal() {
//    $('#sessionTimeoutModal').modal('show');
//    startModalCountdown();
//}

///**
// * Start the countdown timer in the warning modal.
// * At 0, enter grace state and wait for next user interaction to logout.
// */
//function startModalCountdown() {
//    clearInterval(modalCountdownTimer);

//    modalCountdownTimer = setInterval(() => {
//        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || "0", 10);
//        const now = Date.now();
//        let remainingSeconds = Math.floor((expireAt - now) / 1000);

//        if (remainingSeconds <= 0) {
//            updateCountdownDisplay(0);
//            clearInterval(modalCountdownTimer);
//            enterGraceState(); // <-- key change: NO auto-logout
//        } else {
//            updateCountdownDisplay(remainingSeconds);
//        }
//    }, 1000);
//}

///**
// * Update the countdown display in the warning modal.
// * @param {number} seconds - Remaining seconds for the session expiration.
// */
//function updateCountdownDisplay(seconds) {
//    const minutes = Math.floor(seconds / 60);
//    const secs = seconds % 60;
//    const el = document.getElementById('countdown');
//    if (el) el.innerText = `${minutes}:${secs < 10 ? '0' : ''}${secs}`;
//}

//// ==========================
//// Section B: Get Accounting Date
//// ==========================

///**
// * Fetch and display the current accounting date.
// */
//function fetchAccountingDate() {
//    $.ajax({
//        url: '/AccountingDay/GetCurrentAccountingDay',
//        type: 'GET',
//        global: false,
//        success: function (res) {
//            const label = document.getElementById("accountingDayLabel");
//            const icon = document.getElementById("accountingDayStatus");
//            const error = document.getElementById("accountingDayError");

//            if (res && res.success) {
//                label.textContent = 'AD: ' + res.data;
//                label.style.color = 'gray';
//                if (icon) icon.style.display = 'inline';
//                if (error) error.textContent = '';
//            } else {
//                label.textContent = 'AD: N/A';
//                label.style.color = 'red';
//                if (icon) icon.style.display = 'none';
//                if (error) error.textContent = 'Error: Could not load accounting day.';
//            }
//        },
//        error: function () {
//            const label = document.getElementById("accountingDayLabel");
//            label.textContent = 'AD: Error';
//            label.style.color = 'red';
//        }
//    });
//}

//// ==========================
//// Section C: Internet Connection Check
//// ==========================

///**
// * Check internet connection and display a warning banner if offline.
// */
//function checkInternetConnection() {
//    const banner = document.getElementById('internetStatusBanner');
//    if (!banner) return;

//    if (navigator.onLine) {
//        banner.style.display = 'none';
//    } else {
//        banner.style.display = 'block';
//        banner.innerHTML = '🚫 No Internet Connection';
//        banner.style.backgroundColor = '#dc3545';
//    }
//}

//window.addEventListener('online', checkInternetConnection);
//window.addEventListener('offline', checkInternetConnection);
//checkInternetConnection();

//// ==========================
//// Section D: Overall Session Management
//// ==========================

///**
// * Start the overall session timer that logs out the user after the maximum session time.
// * Keep as hard cap. If you want grace here too, replace forceLogout() with enterGraceState().
// */
//function startOverallSessionTimer() {
//    clearInterval(overallSessionTimer);
//    overallSessionSeconds = maxSessionMinutes * 60;

//    overallSessionTimer = setInterval(() => {
//        overallSessionSeconds--;
//        if (overallSessionSeconds <= 0) {
//            clearInterval(overallSessionTimer);
//            forceLogout(); // or: enterGraceState();
//        }
//    }, 1000);
//}

//// ==========================
//// Logout Handling
//// ==========================

///**
// * Immediately logs out the user and clears all timers.
// */
//function forceLogout() {
//    clearTimeout(warningTimer);
//    clearTimeout(logoutTimer);
//    clearInterval(modalCountdownTimer);
//    clearInterval(overallSessionTimer);
//    detachGraceInteractionHooks();

//    window.location.href = '/Authentication/Logout';
//}

///**
// * Handles the session renewal action when the user clicks "Stay Signed In".
// * If already expired, per requirement, next interaction should logout — so we do nothing here.
// */
//function handleRenewSession() {
//    if (inGraceState) {
//        // Do not revive session after expiry; await any interaction to logout
//        return;
//    }
//    resetSessionTimers();
//}

//// ==========================
//// Event Listeners
//// ==========================
//document.addEventListener("DOMContentLoaded", function () {
//    const logoutBtn = document.getElementById('logoutSessionBtn');
//    const renewBtn = document.getElementById('renewSessionBtn');

//    if (logoutBtn) logoutBtn.addEventListener('click', forceLogout);
//    if (renewBtn) renewBtn.addEventListener('click', handleRenewSession);

//    fetchIdleTimeoutConfig();
//    fetchAccountingDate();
//    startOverallSessionTimer();

//    // Refresh accounting date every 5 mins
//    setInterval(fetchAccountingDate, 5 * 60 * 1000);
//});

//// Do NOT auto-logout when tab becomes visible after expiry. Enter grace state instead.
//document.addEventListener('visibilitychange', () => {
//    if (document.visibilityState === 'visible') {
//        const expireAt = parseInt(localStorage.getItem(STORAGE_KEY) || '0', 10);
//        if (Date.now() >= expireAt) {
//            enterGraceState();
//        }
//    }
//});
