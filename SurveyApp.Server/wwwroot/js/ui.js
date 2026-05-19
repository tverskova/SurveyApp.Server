window.surveyUi = (() => {
    function initPasswordToggles() {
        document.querySelectorAll("[data-password-toggle]").forEach((button) => {
            if (button.dataset.passwordToggleReady === "true") {
                return;
            }

            button.dataset.passwordToggleReady = "true";
            button.addEventListener("click", () => {
                const targetId = button.getAttribute("data-password-toggle");
                const input = document.getElementById(targetId);
                if (!input) {
                    return;
                }

                const isPassword = input.type === "password";
                input.type = isPassword ? "text" : "password";
                button.textContent = isPassword ? "Скрыть" : "Показать";
                button.setAttribute("aria-label", isPassword ? "Скрыть пароль" : "Показать пароль");
            });
        });
    }

    function scrollToElement(elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            return;
        }

        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }

    document.addEventListener("DOMContentLoaded", initPasswordToggles);

    return {
        initPasswordToggles,
        scrollToElement
    };
})();
