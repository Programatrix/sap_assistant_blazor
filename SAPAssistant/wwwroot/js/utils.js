window.downloadFile = (fileName, base64Content) => {
    const link = document.createElement("a");
    link.download = fileName;
    link.href = "data:text/csv;base64," + base64Content;
    link.click();
};
