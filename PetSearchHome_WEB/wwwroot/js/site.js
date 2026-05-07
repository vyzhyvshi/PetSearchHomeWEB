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

    function showNotificationToast(notification) {
        let container = document.querySelector("[data-notification-toasts]");
        if (!container) {
            container = document.createElement("div");
            container.dataset.notificationToasts = "";
            container.className = "toast-container position-fixed top-0 end-0 p-3";
            container.style.zIndex = "1080";
            document.body.appendChild(container);
        }

        const toast = document.createElement("div");
        toast.className = "toast align-items-center text-bg-primary border-0";
        toast.role = "status";
        toast.ariaLive = "polite";
        toast.ariaAtomic = "true";
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${escapeHtml(notification.message ?? "Нове сповіщення")}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Закрити"></button>
            </div>`;

        container.appendChild(toast);

        const bootstrapToast = new bootstrap.Toast(toast, { delay: 6000 });
        toast.addEventListener("hidden.bs.toast", () => toast.remove(), { once: true });
        bootstrapToast.show();
    }

    function escapeHtml(value) {
        const element = document.createElement("div");
        element.textContent = value;
        return element.innerHTML;
    }

    function wireRealtimeNotifications() {
        if (!window.signalR) {
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationsHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", showNotificationToast);
        connection.start().catch((error) => console.error("SignalR notification connection failed.", error));
    }

    document.addEventListener("DOMContentLoaded", () => {
        wireConfirmations();
        wireRealtimeNotifications();
    }, { once: true });
})();
