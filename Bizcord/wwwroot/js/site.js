// Feedback ved afsendelse af formularer:
// Når en formular sendes, deaktiveres dens submit-knap og viser en spinner, så brugeren kan se,
// at der sker noget, og ikke kommer til at sende to gange (f.eks. oprette den samme chat to gange).
// Teksten mens der arbejdes sættes med data-loading-text="..." på knappen.
document.addEventListener('submit', (event) => {
    const form = event.target;
    const button = form.querySelector('button[type="submit"]');
    if (!button || event.defaultPrevented) {
        return;
    }

    // Allerede i gang: stop dobbelt-afsendelse (f.eks. Enter i et felt to gange)
    if (form.dataset.submitting === 'true') {
        event.preventDefault();
        return;
    }
    form.dataset.submitting = 'true';

    const loadingText = button.dataset.loadingText || 'Arbejder …';
    button.dataset.originalHtml = button.innerHTML;
    button.disabled = true;
    button.setAttribute('aria-busy', 'true');
    button.innerHTML =
        '<span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>' +
        '<span role="status">' + loadingText + '</span>';
});

// Hvis siden vises igen fra browserens cache (tilbage-knappen), skal knapperne virke igen.
window.addEventListener('pageshow', (event) => {
    if (!event.persisted) {
        return;
    }
    document.querySelectorAll('form[data-submitting="true"]').forEach((form) => {
        delete form.dataset.submitting;
        const button = form.querySelector('button[type="submit"]');
        if (button && button.dataset.originalHtml) {
            button.innerHTML = button.dataset.originalHtml;
            button.disabled = false;
            button.removeAttribute('aria-busy');
        }
    });
});
