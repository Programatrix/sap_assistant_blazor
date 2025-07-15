// theme.js (¡sin <script> tags!)
window.setTheme = function (theme) {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("theme", theme);
};

window.loadTheme = function () {
    const saved = localStorage.getItem("theme") || "dark";
    document.documentElement.setAttribute("data-theme", saved);
};

window.onload = () => {
    window.loadTheme();
};

