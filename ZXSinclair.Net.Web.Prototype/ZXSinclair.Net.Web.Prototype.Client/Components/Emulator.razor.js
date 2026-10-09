const pendingRequests = {};
let pendingRequestId = 0;
let dotnetWorker = null;
let workerError = null;

function failRequests(error) {
    workerError = error;
    dotnetWorker?.terminate();
    dotnetWorker = null;
    for (const requestId of Object.keys(pendingRequests)) {
        pendingRequests[requestId].reject(error);
        delete pendingRequests[requestId];
    }
}

function getWorker() {
    if (workerError) {
        throw workerError;
    }
    if (!dotnetWorker) {
        dotnetWorker = new Worker(
            new URL('Workers/DotNetWorkTest.js', document.baseURI),
            { type: 'module' });
        dotnetWorker.addEventListener('message', e => {
            if (e.data.command !== 'response') {
                return;
            }
            const request = pendingRequests[e.data.requestId];
            if (!request) {
                return;
            }
            delete pendingRequests[e.data.requestId];
            if (e.data.error) {
                request.reject(new Error(e.data.error));
            } else {
                request.resolve(e.data.result);
            }
        });
        dotnetWorker.addEventListener('error', e => {
            failRequests(new Error(e.message || 'Worker failed to load.'));
        });
        dotnetWorker.addEventListener('messageerror', () => {
            failRequests(new Error('Worker response could not be decoded.'));
        });
    }
    return dotnetWorker;
}


let canvas = null;
let c2d = null;
let webgl = null;
let imageData = null;
let pixels = null;

export function hello() {
    alert("Hello, World!");
}

export function initCanvas(canvasElement, useWebGL) {
    canvas = canvasElement;
    if (useWebGL) {
        webgl = canvas.getContext("webgl");
        if (webgl)
            console.log("using webgl rendering");
    }
    if (!webgl) {
        c2d = canvas.getContext("2d");
        if (c2d)
            console.log("using 2d rendering");
    }
}

export async function Run() {
    return await Test();
}

export async function Test() {
    const worker = getWorker();
    const requestId = ++pendingRequestId;
    const result = await new Promise((resolve, reject) => {
        pendingRequests[requestId] = { resolve, reject };
        try {
            worker.postMessage({ command: 'Test', requestId });
        } catch (error) {
            delete pendingRequests[requestId];
            reject(error);
        }
    });
    console.log('Worker response:', JSON.stringify({
        value: result.value,
        data: Array.from(result.data)
    }));
    return result;
}

export function dispose() {
    failRequests(new Error('Emulator disposed.'));
    canvas = null;
    c2d = null;
    webgl = null;
    workerError = null;
}
