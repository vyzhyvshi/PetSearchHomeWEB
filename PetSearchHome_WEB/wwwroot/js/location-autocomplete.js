(() => {
    function initAutocomplete(container) {
        const input = container.querySelector("[data-location-input]");
        const suggestions = container.querySelector("[data-location-suggestions]");
        const endpoint = container.dataset.autocompleteUrl;

        if (!input || !suggestions || !endpoint) {
            return;
        }

        let controller = null;
        let activeRequestId = 0;

        function hideSuggestions() {
            suggestions.innerHTML = "";
            suggestions.classList.remove("is-visible");
        }

        function selectSuggestion(value) {
            input.value = value;
            hideSuggestions();
            input.focus();
        }

        async function fetchSuggestions() {
            const query = input.value.trim();
            if (query.length < 2) {
                hideSuggestions();
                return;
            }

            activeRequestId += 1;
            const requestId = activeRequestId;

            if (controller) {
                controller.abort();
            }

            controller = new AbortController();

            try {
                const response = await fetch(`${endpoint}${encodeURIComponent(query)}`, {
                    signal: controller.signal
                });

                if (!response.ok) {
                    throw new Error("Network error");
                }

                const items = await response.json();
                if (requestId !== activeRequestId || !Array.isArray(items) || items.length === 0) {
                    hideSuggestions();
                    return;
                }

                suggestions.innerHTML = "";

                for (const item of items) {
                    const option = document.createElement("button");
                    option.type = "button";
                    option.className = "list-group-item list-group-item-action";
                    option.textContent = item;
                    option.addEventListener("click", () => selectSuggestion(item));
                    suggestions.appendChild(option);
                }

                suggestions.classList.add("is-visible");
            } catch (error) {
                if (error.name !== "AbortError") {
                    hideSuggestions();
                }
            }
        }

        input.addEventListener("input", fetchSuggestions);
        input.addEventListener("blur", () => {
            window.setTimeout(hideSuggestions, 150);
        });

        document.addEventListener("click", (event) => {
            if (!container.contains(event.target)) {
                hideSuggestions();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        document.querySelectorAll("[data-location-autocomplete]").forEach(initAutocomplete);
    }, { once: true });
})();
