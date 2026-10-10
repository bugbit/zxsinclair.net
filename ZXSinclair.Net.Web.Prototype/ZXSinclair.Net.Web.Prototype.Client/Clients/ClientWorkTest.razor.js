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
            new URL('../Workers/WorkTest.razor.js', document.baseURI),
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

function request(command, payload = {}, transfer = []) {
    const worker = getWorker();
    const requestId = ++pendingRequestId;
    return new Promise((resolve, reject) => {
        pendingRequests[requestId] = { resolve, reject };
        try {
            worker.postMessage({ ...payload, command, requestId }, transfer);
        } catch (error) {
            delete pendingRequests[requestId];
            reject(error);
        }
    });
}

export function Test() { return request('Test'); }
export function TestPutImage() { return request('TestPutImage'); }
export function InitPresent(canvas, width, height, backend) {
    return request('InitPresent', { canvas, width, height, backend }, [canvas]);
}
export function TestPresent() { return request('TestPresent'); }
export function ReadPresentPixel() { return request('ReadPresentPixel'); }

export function dispose() {
    failRequests(new Error('Emulator disposed.'));
    workerError = null;
}