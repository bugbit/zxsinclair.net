import * as WorkTest from '../Clients/ClientWorkTest.razor.js';
let canvas = null;
let c2d = null;
let webgl = null;
let imageData = null;
let pixels = null;
let presentElement = null;
let presentReady = null;
let audioReady = null;
let audioContext = null;
let audioNode = null;
let audioRunning = false;
let audioBusy = false;
let audioInfo = null;
let audioProcessorError = null;

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

export async function dispose() {
    // Await initialization before closing; it may still be loading the worker runtime.
    try { await audioReady; } catch { }
    audioNode?.disconnect();
    audioNode?.port.close();
    if (audioContext) await audioContext.close();
    audioContext = audioNode = audioReady = audioInfo = null;
    audioRunning = audioBusy = false;
    audioProcessorError = null;
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

export async function TestAudio() {
    if (audioBusy) throw new Error('Audio toggle is already in progress.');
    audioBusy = true;
    try {
        if (!audioReady) {
            const value = new URLSearchParams(location.search).get('audioFrames');
            const target = value === null ? 3 : Number(value);
            if (!Number.isInteger(target) || target < 2 || target > 6) throw new Error('audioFrames must be 2–6.');
            audioContext = new AudioContext({ latencyHint: 'interactive' });
            const resumed = audioContext.resume();
            audioReady = (async () => {
                await resumed;
                if (audioContext.state !== 'running') throw new Error('AudioContext is not running.');
                await audioContext.audioWorklet.addModule(new URL('../Worklets/AudioTestWorklet.razor.js', document.baseURI));
                audioNode = new AudioWorkletNode(audioContext, 'zx-audio-test', { numberOfInputs: 0, outputChannelCount: [1] });
                audioNode.onprocessorerror = () => { audioProcessorError = 'AudioWorklet processor failed.'; };
                audioNode.connect(audioContext.destination);
                const channel = new MessageChannel();
                audioNode.port.postMessage({ port: channel.port1 }, [channel.port1]);
                audioInfo = await WorkTest.InitAudio(channel.port2, audioContext.sampleRate, target);
            })();
        }
        await audioReady;
        if (audioProcessorError) throw new Error(audioProcessorError);
        if (audioRunning) await WorkTest.StopAudio();
        else {
            await audioContext.resume();
            await WorkTest.StartAudio();
        }
        audioRunning = !audioRunning;
        const button = document.querySelector('[data-testid="test-audio"]');
        button.dataset.audioState = audioRunning ? 'running' : 'stopped';
        button.dataset.sampleRate = String(audioContext.sampleRate);
        return { running: audioRunning, sampleRate: audioContext.sampleRate, ...audioInfo };
    } finally {
        audioBusy = false;
    }
}

export async function audioStats() {
    await audioReady;
    if (audioProcessorError) throw new Error(audioProcessorError);
    return { ...await WorkTest.AudioStats(), sampleRate: audioContext.sampleRate,
        baseLatency: audioContext.baseLatency, outputLatency: audioContext.outputLatency,
        contextState: audioContext.state };
}
