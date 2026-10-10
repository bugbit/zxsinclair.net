import * as WorkTest from '../Clients/ClientWorkTest.razor.js';
let canvas = null;
let c2d = null;
let webgl = null;
let imageData = null;
let pixels = null;

export function hello() {
    alert("Hello, World!");
}

export function initCanvas(canvasElement, width, height, useWebGL) {
    canvas = canvasElement;
    c2d = null;
    webgl = null;
    imageData = null;
    pixels = null;
    if (useWebGL) {
        webgl = canvas.getContext("webgl");
        if (webgl)
            console.log("using webgl rendering");
    }
    if (!webgl) {
        c2d = canvas.getContext("2d");
        if (c2d) {
            imageData = c2d.createImageData(width, height);
            pixels = imageData.data;
            canvas.dataset.ready = "1";
            console.log("using 2d rendering");
        }
    }
}

export function putImagen2D(data) {
    if (!c2d || !pixels || !imageData) return;
    pixels.set(data);
    c2d.putImageData(imageData, 0, 0);
}

export async function Test1() {
    const result = await WorkTest.Test();
    console.log('Worker response:', JSON.stringify({
        value: result.value,
        data: Array.from(result.data)
    }));
    return result;
}

export async function TestPutImage() {
    const result = await WorkTest.TestPutImage();

    putImagen2D(result.data);
}

export function dispose() {
    WorkTest.dispose();
    canvas = null;
    c2d = null;
    webgl = null;
    imageData = null;
    pixels = null;
}

export function reportTiming(name, ms) {
    (globalThis.__zxTimings ??= []).push({ name, ms });
}
