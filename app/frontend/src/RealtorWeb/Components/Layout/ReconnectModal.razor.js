const modal = document.getElementById("components-reconnect-modal");

if (modal) {
    modal.addEventListener("components-reconnect-state-changed", (event) => {
        const state = event.detail?.state;
        const visibleStates = ["show", "retrying", "failed", "rejected"];

        if (visibleStates.includes(state) && !modal.open) {
            modal.showModal();
        }

        if (state === "hide" && modal.open) {
            modal.close();
        }
    });

    modal.querySelectorAll("[data-reconnect-action='retry']").forEach((button) => {
        button.addEventListener("click", () => {
            globalThis.Blazor?.reconnect?.();
        });
    });

    modal.querySelectorAll("[data-reconnect-action='reload']").forEach((button) => {
        button.addEventListener("click", () => {
            globalThis.location.reload();
        });
    });
}
