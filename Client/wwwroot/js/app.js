window.downloadFile = function (path) {
    var link = document.createElement("a");
    link.href = "api/" + path;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}