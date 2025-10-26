document.addEventListener('htmx:configRequest', function (evt) {
    // Prefer the hidden input from @Html.AntiForgeryToken()
    const inputToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    // Fallback to a meta tag if you use that pattern elsewhere
    const metaToken = document.querySelector('meta[name="csrf-token"]')?.content;
    const token = inputToken || metaToken;
    if (token) {
        // Default header ASP.NET Core anti-forgery looks for
        evt.detail.headers['RequestVerificationToken'] = token;
    }
});

//enable tooltips
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))


// Confirmation dialog for nav link tiles
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.nav-link-tile[data-confirm-message]').forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            const message = link.getAttribute('data-confirm-message');
            if (confirm('Are you sure you want to continue?')) {
                window.location.href = link.getAttribute('href');
            }
        });
    });
});