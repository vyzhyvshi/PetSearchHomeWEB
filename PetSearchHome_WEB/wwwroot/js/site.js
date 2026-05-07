(() => {
    function wireConfirmations() {
        document.addEventListener("submit", (event) => {
            const form = event.target;
            if (!(form instanceof HTMLFormElement)) {
                return;
            }

            const message = form.dataset.confirm;
            if (message && !window.confirm(message)) {
                event.preventDefault();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", wireConfirmations, { once: true });
})();
