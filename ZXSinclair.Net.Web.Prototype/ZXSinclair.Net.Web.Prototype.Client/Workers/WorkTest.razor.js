import { dotnet } from '../_framework/dotnet.js';

const runtimeReady = dotnet.create().then(async runtime => {
    runtime.setModuleImports('emulator', { createResult });
    return runtime.getAssemblyExports('ZXSinclair.Net.Web.Prototype.Workers.dll');
});

// Requests report startup failures through the response channel.
runtimeReady.catch(() => { });

self.addEventListener('message', async e => {
    try {
        const assemblyExports = await runtimeReady;

        let result;
        switch (e.data.command) {
            case 'Test':
                result = assemblyExports.ZXSinclair.Net.Web.Prototype.Workers.WorkTest.Test();
                break;
            case 'TestPutImage':
                result = assemblyExports.ZXSinclair.Net.Web.Prototype.Workers.WorkTest.TestPutImage();
                break;
            default:
                throw new Error(`Unknown command: ${e.data.command}`);
        }

        self.postMessage({
            command: 'response',
            requestId: e.data.requestId, result
        });
    } catch (err) {
        self.postMessage({
            command: 'response',
            requestId: e.data.requestId, error: err.message
        });
    }
});

export function createResult(value, data) {
    return { value, data };
}