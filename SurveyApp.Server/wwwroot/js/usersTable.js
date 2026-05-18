window.surveyUsersTable = (() => {
    const initialized = new WeakSet();

    function initResizable(tableId) {
        const table = document.getElementById(tableId);
        if (!table || initialized.has(table)) {
            return;
        }

        initialized.add(table);

        table.querySelectorAll("thead th").forEach((th) => {
            const handle = th.querySelector(".users-column-resizer");
            if (!handle) {
                return;
            }

            handle.addEventListener("mousedown", (event) => {
                event.preventDefault();
                event.stopPropagation();

                const startX = event.pageX;
                const startWidth = th.offsetWidth;
                const columnIndex = Array.from(th.parentElement.children).indexOf(th) + 1;
                const minWidth = 80;

                document.body.style.cursor = "col-resize";
                document.body.style.userSelect = "none";

                const onMouseMove = (moveEvent) => {
                    const nextWidth = Math.max(minWidth, startWidth + moveEvent.pageX - startX);

                    table.querySelectorAll(`tr > *:nth-child(${columnIndex})`).forEach((cell) => {
                        cell.style.width = `${nextWidth}px`;
                        cell.style.minWidth = `${nextWidth}px`;
                    });
                };

                const onMouseUp = () => {
                    document.body.style.cursor = "";
                    document.body.style.userSelect = "";
                    document.removeEventListener("mousemove", onMouseMove);
                    document.removeEventListener("mouseup", onMouseUp);
                };

                document.addEventListener("mousemove", onMouseMove);
                document.addEventListener("mouseup", onMouseUp);
            });
        });
    }

    return { initResizable };
})();
