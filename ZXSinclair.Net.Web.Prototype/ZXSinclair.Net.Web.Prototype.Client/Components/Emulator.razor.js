import * as WorkTest from '../Clients/ClientWorkTest.razor.js';
let canvas = null;
let c2d = null;
let webgl = null;
let imageData = null;
let pixels = null;
let presentElement = null;
let presentReady = null;

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
    presentElement = null;
    presentReady = null;
    canvas = null;
    c2d = null;
    webgl = null;
    imageData = null;
    pixels = null;
}

export function reportTiming(name, ms, extra = {}) {
    (globalThis.__zxTimings ??= []).push({ name, ms, ...extra, backend: name === "TestPresent" ? presentElement?.dataset.backend : undefined });
}

export async function TestPresent(canvasElement) {
    if (presentElement !== canvasElement) {
        presentElement = canvasElement;
        // Cache the promise before transferring so concurrent clicks cannot transfer twice.
        presentReady = Promise.resolve().then(async () => {
            const backend = new URLSearchParams(location.search).get('present') ?? 'webgl2';
            if (backend !== 'webgl2' && backend !== '2d') throw new Error(`Unknown presentation backend: ${backend}`);
            const offscreen = canvasElement.transferControlToOffscreen();
            const info = await WorkTest.InitPresent(offscreen, canvasElement.width, canvasElement.height, backend);
            canvasElement.dataset.backend = info.backend;
            canvasElement.dataset.renderer = info.renderer;
        });
    }
    // A rejected initialization stays rejected: the transferred canvas cannot be retried.
    await presentReady;
    return WorkTest.TestPresent();
}

export function readPresentPixel() {
    return WorkTest.ReadPresentPixel();
}