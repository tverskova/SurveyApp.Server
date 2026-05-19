window.surveyUi = (() => {
    const eyeOpenIcon = `
        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
            <path d="M2.25 12s3.5-6.25 9.75-6.25S21.75 12 21.75 12 18.25 18.25 12 18.25 2.25 12 2.25 12Z"></path>
            <circle cx="12" cy="12" r="2.75"></circle>
        </svg>`;

    const eyeClosedIcon = `
        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
            <path d="M3 3l18 18"></path>
            <path d="M10.7 5.9A9.1 9.1 0 0 1 12 5.75C18.25 5.75 21.75 12 21.75 12a17 17 0 0 1-3.1 3.75"></path>
            <path d="M6.55 6.9A17 17 0 0 0 2.25 12S5.75 18.25 12 18.25a9.3 9.3 0 0 0 4.1-.95"></path>
            <path d="M9.9 9.9a2.75 2.75 0 0 0 3.9 3.9"></path>
        </svg>`;

    function updatePasswordButton(button, input) {
        const hasValue = input.value.length > 0;
        const isVisible = input.type === "text";

        button.hidden = !hasValue;
        button.innerHTML = isVisible ? eyeOpenIcon : eyeClosedIcon;
        button.setAttribute("aria-label", isVisible ? "Скрыть пароль" : "Показать пароль");
        button.setAttribute("title", isVisible ? "Скрыть пароль" : "Показать пароль");
    }

    function initPasswordToggles() {
        document.querySelectorAll("[data-password-toggle]").forEach((button) => {
            if (button.dataset.passwordToggleReady === "true") {
                return;
            }

            const targetId = button.getAttribute("data-password-toggle");
            const input = document.getElementById(targetId);
            if (!input) {
                return;
            }

            button.dataset.passwordToggleReady = "true";
            updatePasswordButton(button, input);

            input.addEventListener("input", () => updatePasswordButton(button, input));
            button.addEventListener("click", () => {
                input.type = input.type === "password" ? "text" : "password";
                updatePasswordButton(button, input);
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
