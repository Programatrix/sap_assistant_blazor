// landing-deps.js
(function () {
    // --- CSS ---
    const aosCss = document.createElement("link");
    aosCss.rel = "stylesheet";
    aosCss.href = "https://cdnjs.cloudflare.com/ajax/libs/aos/2.3.4/aos.css";
    document.head.appendChild(aosCss);

    // --- JS ---
    const aosJs = document.createElement("script");
    aosJs.src = "https://cdnjs.cloudflare.com/ajax/libs/aos/2.3.4/aos.js";
    aosJs.defer = true;
    aosJs.onload = () => {
        // Inicializar landing cuando AOS esté cargado
        if (window.landingInit) {
            window.landingInit();
        }
    };
    document.body.appendChild(aosJs);
})();

