/* ========================= TOASTS ========================= */

document.addEventListener("DOMContentLoaded", function () {
    const toastElements = document.querySelectorAll('.toast');
    toastElements.forEach(toastEl => {
        const t = new bootstrap.Toast(toastEl);
        t.show();
    });
});


/* ========================= FLOATING BUTTONS ========================= */

document.addEventListener("mousemove", function (e) {
    const container = document.querySelector(".floating-buttons");
    if (!container) return;

    const nearRight = e.clientX > window.innerWidth - 150;
    const nearBottom = e.clientY > window.innerHeight - 150;

    if (nearRight && nearBottom) {
        container.classList.add("show-buttons");
    } else {
        container.classList.remove("show-buttons");
    }
});


/* ========================= DROPDOWN SUBMENU ========================= */

document.addEventListener("DOMContentLoaded", function () {
    const submenuLinks = document.querySelectorAll(".dropdown-submenu > a");

    submenuLinks.forEach(link => {
        link.addEventListener("mouseenter", function () {
            const submenu = this.nextElementSibling;
            if (submenu) submenu.classList.add("show");
        });

        link.addEventListener("mouseleave", function () {
            const submenu = this.nextElementSibling;
            if (submenu) submenu.classList.remove("show");
        });
    });
});
