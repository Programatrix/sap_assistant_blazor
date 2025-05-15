window.autoResize = (selector) => {
    const textarea = document.querySelector(selector);
    if (textarea)
    {
        textarea.style.height = 'auto';
        textarea.style.height = `${ textarea.scrollHeight}
        px`;
    }
};

window.preventDefaultEnter = () => {
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
        }
    }, { once: true });
};
