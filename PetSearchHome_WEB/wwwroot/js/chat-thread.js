(() => {
    function toggleEdit(thread, messageId) {
        const viewMode = thread.querySelector(`[data-message-view="${messageId}"]`);
        const editForm = thread.querySelector(`[data-message-edit="${messageId}"]`);

        if (!viewMode || !editForm) {
            return;
        }

        const willShowEditor = editForm.hidden;
        viewMode.hidden = willShowEditor;
        editForm.hidden = !willShowEditor;

        if (willShowEditor) {
            const textarea = editForm.querySelector("textarea");
            if (textarea) {
                textarea.focus();
                textarea.setSelectionRange(textarea.value.length, textarea.value.length);
            }
        }
    }

    function initThread(thread) {
        const scrollContainer = thread.querySelector("[data-chat-messages]");
        if (scrollContainer) {
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        }

        thread.addEventListener("click", (event) => {
            const toggle = event.target.closest("[data-chat-edit-toggle]");
            if (!toggle) {
                return;
            }

            event.preventDefault();
            toggleEdit(thread, toggle.dataset.chatEditToggle);
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-chat-thread]").forEach(initThread);
    }, { once: true });
})();
