// Requiere bootstrap.bundle.min.js cargado (la parte de Collapse)
window.faqInit = (accordionId) => {
    // Nada especial por ahora; se deja por si luego ampliamos
};

window.faqToggle = (accordionId, targetId) => {
    const acc = document.getElementById(accordionId);
    const target = document.getElementById(targetId);
    if (!acc || !target) return;

    const instance = bootstrap.Collapse.getOrCreateInstance(target, { toggle: false });
    const isOpen = target.classList.contains('show');

    if (isOpen) {
        // Si está abierto → cerrar
        instance.hide();
    } else {
        // Cerrar otros abiertos
        acc.querySelectorAll('.accordion-collapse.show').forEach(el => {
            if (el !== target) bootstrap.Collapse.getOrCreateInstance(el, { toggle: false }).hide();
        });
        // Abrir el solicitado
        instance.show();
    }
};
