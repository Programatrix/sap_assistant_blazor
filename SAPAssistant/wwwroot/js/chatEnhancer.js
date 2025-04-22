window.chatEnhancer = {
    autoResize: (textarea) => {
        if (textarea)
        {
            textarea.style.height = "auto";
            textarea.style.height = textarea.scrollHeight + "px";
        }
    },
    isScrolledToBottom: (element) => {
        if (!element) return true;
        return element.scrollHeight - element.scrollTop - element.clientHeight < 50;
    },
    scrollToBottom: (element) => {
        if (element)
        {
            element.scrollTop = element.scrollHeight;
        }
    }
};
