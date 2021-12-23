export function getWindowSize() {
    var win = window,
        doc = document,
        docElem = doc.documentElement,
        body = doc.getElementsByTagName('body')[0],
        x = win.innerWidth || docElem.clientWidth || body.clientWidth,
        y = win.innerHeight || docElem.clientHeight || body.clientHeight;

    return { width: x, height: y }
}

export function addEventResize(dotNetHelper, method) {
    window.addEventListener("resize", function () {
        dotNetHelper.invokeMethodAsync(method, getWindowSize())
    });
}