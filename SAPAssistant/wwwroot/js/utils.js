window.downloadFile = (fileName, base64Content) => {
    const link = document.createElement("a");
    link.download = fileName;
    link.href = "data:text/csv;base64," + base64Content;
    link.click();
};

window.ClickOutsideHelper = {
    register: function (elementId, dotNetHelper, methodName = "CloseMenu") {
        const handler = (event) => {
            const el = document.getElementById(elementId);
            if (el && !el.contains(event.target)) {
                dotNetHelper.invokeMethodAsync(methodName);
                document.removeEventListener("click", handler);
            }
        };
        setTimeout(() => document.addEventListener("click", handler), 50);
    }
};
