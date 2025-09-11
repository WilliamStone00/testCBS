$(document)
    .ajaxStart(function () {
        $("#loading").fadeIn();
    })
    .ajaxStop(function () {
        $("#loading").fadeOut();
    });


function setLanguage(lang, flagPath) {
    const flagElement = document.getElementById('current-lang-flag');

    // 🌐 Determine message in target language
    let loadingMessage = "Changing language...";
    if (lang === "fr") loadingMessage = "Changement de langue...";
    else if (lang === "en") loadingMessage = "Changing language...";
    else loadingMessage = "Loading...";

    // 🔵 Create soft loading overlay
    const loaderOverlay = document.createElement('div');
    loaderOverlay.id = 'language-loader-overlay';
    loaderOverlay.innerHTML = `
    <div class="loader-content">
        <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-3 text-primary fw-semibold">${loadingMessage}</p>
    </div>
    `;
    document.body.appendChild(loaderOverlay);

    // 🌀 Blur page and block scrolling
    document.body.style.overflow = 'hidden';

    fetch(`/Localization/SetLanguage?lang=${lang}`)
        .then(() => {
            if (flagElement) {
                flagElement.src = flagPath.replace('~', '');
            }

            setTimeout(() => {
                loaderOverlay.remove();
                document.body.style.overflow = 'auto';
                location.reload();
            }, 1000);
        })
        .catch((error) => {
            console.error('Language switch failed:', error);
            alert('Failed to change language. Please try again.');
            loaderOverlay.remove();
            document.body.style.overflow = 'auto';
        });
}


