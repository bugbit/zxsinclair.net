var instance;
var ctx;
var imageData;
var buf8;
var data;

function gameLoop(timeStamp) {
    window.requestAnimationFrame(gameLoop);
    instance.invokeMethod('GameLoop', timeStamp);
}

function initcanvas(_instance) {
    if (_instance)
        instance = _instance;

    var canvas = document.getElementById('canvas');
    var canvasWidth = canvas.width;
    var canvasHeight = canvas.height;
    ctx = canvas.getContext('2d');
    imageData = ctx.getImageData(0, 0, canvasWidth, canvasHeight);
    var buf = new ArrayBuffer(imageData.data.length);
    buf8 = new Uint8ClampedArray(buf);
    data = new Uint32Array(buf);

    window.requestAnimationFrame(gameLoop);
}

function setpixel(pos, rgba) {
    data[pos] = rgba;

    return true;
}

function putimage() {
    imageData.data.set(buf8);
    ctx.putImageData(imageData, 0, 0);
}

function putimage2(dataPtr) {
    var buf = Uint8ClampedArray.from(Module.HEAPU8.subarray(dataPtr, dataPtr + imageData.data.length));

    imageData.data.set(buf);
    ctx.putImageData(imageData, 0, 0);
}