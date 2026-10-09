const dotnetWorker =
    new Worker('./WorkTest.razor.js', { type: 'module' });

dotnetWorker.addEventListener('message', e => {
    switch (e.data.command) {
        case 'response':
            const request = pendingRequests[e.data.requestId];
            delete pendingRequests[e.data.requestId];
            if (e.data.error) {
                request.reject(new Error(e.data.error));
            }
            request.resolve(e.data.result);
            break;
        default:
            console.log('Worker said:', e.data);
    }
});

function sendRequestToWorker(request) {
    pendingRequestId++;
    const promise = new Promise((resolve, reject) => {
        pendingRequests[pendingRequestId] = { resolve, reject };
    });

    dotnetWorker.postMessage({ ...request, requestId: pendingRequestId });
    return promise;
}


async function Test(text, size) {
    const response = await sendRequestToWorker({ command: 'Test', text, size });
    console.log('Worker response:', response);
    return response;
}