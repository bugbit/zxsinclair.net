import { dotnet } from '../_framework/dotnet.js';

// Spectrum: bit 0 blue, bit 1 red, bit 2 green, bit 3 bright; both blacks are black.
const palette = new Uint8Array(64);
const palette32 = new Uint32Array(16);
for (let index = 0; index < 16; index++) {
    const level = index & 8 ? 255 : 215;
    const red = index & 2 ? level : 0;
    const green = index & 4 ? level : 0;
    const blue = index & 1 ? level : 0;
    palette.set([red, green, blue, 255], index * 4);
    palette32[index] = (255 << 24) | (blue << 16) | (green << 8) | red;
}
// Uint32Array writes use native byte order. Match ImageData RGBA even on a big-endian host.
if (new Uint8Array(new Uint32Array([1]).buffer)[0] !== 1) {
    for (let index = 0; index < 16; index++) {
        const offset = index * 4;
        palette32[index] = (palette[offset] << 24) | (palette[offset + 1] << 16) | (palette[offset + 2] << 8) | 255;
    }
}

let gl = null;
let c2d = null;
let image = null;
let out = null;
let frameView = null;
let indexTexture = null;
let backend = null;
let frameWidth = 0;
let frameHeight = 0;
let initialized = false;
const pixel = new Uint8Array(4);

const runtimeReady = dotnet.create().then(async runtime => {
    runtime.setModuleImports('emulator', { createResult, present });
    return runtime.getAssemblyExports('ZXSinclair.Net.Web.Prototype.Workers.dll');
});
runtimeReady.catch(() => { });

function compileShader(type, source) {
    const shader = gl.createShader(type);
    if (!shader) throw new Error('WebGL2 shader allocation failed.');
    gl.shaderSource(shader, source);
    gl.compileShader(shader);
    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
        const log = gl.getShaderInfoLog(shader);
        gl.deleteShader(shader);
        throw new Error(`WebGL2 shader compilation failed: ${log}`);
    }
    return shader;
}

function initPresent(canvas, width, height, requested) {
    if (initialized) throw new Error('Presentation canvas already initialized.');
    // Initialization, including a failure after context creation, cannot be retried.
    initialized = true;
    if (requested !== 'webgl2' && requested !== '2d') throw new Error(`Unknown presentation backend: ${requested}`);
    frameWidth = width;
    frameHeight = height;
    frameView = new Uint8Array(width * height);
    if (requested === 'webgl2') gl = canvas.getContext('webgl2', { alpha: false, antialias: false });
    if (gl) {
        // Errors after acquiring WebGL2 propagate; this canvas cannot acquire a 2D context.
        gl.pixelStorei(gl.UNPACK_ALIGNMENT, 1);
        const vertex = compileShader(gl.VERTEX_SHADER, `#version 300 es
        void main() {
            vec2 p = vec2(float((gl_VertexID << 1) & 2), float(gl_VertexID & 2));
            gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
        }`);
        const fragment = compileShader(gl.FRAGMENT_SHADER, `#version 300 es
        precision highp float;
        precision highp usampler2D;
        uniform usampler2D indices;
        uniform sampler2D colors;
        out vec4 color;
        void main() {
            uint index = texelFetch(indices, ivec2(gl_FragCoord.xy), 0).r;
            color = texelFetch(colors, ivec2(int(index), 0), 0);
        }`);
        const program = gl.createProgram();
        if (!program) throw new Error('WebGL2 program allocation failed.');
        gl.attachShader(program, vertex);
        gl.attachShader(program, fragment);
        gl.linkProgram(program);
        if (!gl.getProgramParameter(program, gl.LINK_STATUS)) throw new Error(`WebGL2 link failed: ${gl.getProgramInfoLog(program)}`);
        gl.deleteShader(vertex);
        gl.deleteShader(fragment);
        gl.useProgram(program);
        const vao = gl.createVertexArray();
        if (!vao) throw new Error('WebGL2 VAO allocation failed.');
        gl.bindVertexArray(vao);
        indexTexture = gl.createTexture();
        if (!indexTexture) throw new Error('WebGL2 index texture allocation failed.');
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, indexTexture);
        gl.texStorage2D(gl.TEXTURE_2D, 1, gl.R8UI, width, height);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.uniform1i(gl.getUniformLocation(program, 'indices'), 0);
        const colors = gl.createTexture();
        if (!colors) throw new Error('WebGL2 palette texture allocation failed.');
        gl.activeTexture(gl.TEXTURE1);
        gl.bindTexture(gl.TEXTURE_2D, colors);
        gl.texStorage2D(gl.TEXTURE_2D, 1, gl.RGBA8, 16, 1);
        gl.texSubImage2D(gl.TEXTURE_2D, 0, 0, 0, 16, 1, gl.RGBA, gl.UNSIGNED_BYTE, palette);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.uniform1i(gl.getUniformLocation(program, 'colors'), 1);
        gl.activeTexture(gl.TEXTURE0);
        gl.viewport(0, 0, width, height);
        const error = gl.getError();
        if (error !== gl.NO_ERROR) throw new Error(`WebGL2 initialization error: 0x${error.toString(16)}`);
        backend = 'webgl2';
        const debug = gl.getExtension('WEBGL_debug_renderer_info');
        return { backend, renderer: String(gl.getParameter(debug ? debug.UNMASKED_RENDERER_WEBGL : gl.RENDERER)) };
    }
    c2d = canvas.getContext('2d');
    if (!c2d) throw new Error('OffscreenCanvas 2D context unavailable.');
    image = c2d.createImageData(width, height);
    out = new Uint32Array(image.data.buffer);
    backend = '2d';
    return { backend, renderer: 'Canvas 2D (acceleration not queried)' };
}

function drawFrame() {
    if (backend === 'webgl2') {
        gl.texSubImage2D(gl.TEXTURE_2D, 0, 0, 0, frameWidth, frameHeight, gl.RED_INTEGER, gl.UNSIGNED_BYTE, frameView);
        gl.drawArrays(gl.TRIANGLES, 0, 3);
    } else if (backend === '2d') {
        for (let i = 0; i < frameView.length; i++) out[i] = palette32[frameView[i]];
        c2d.putImageData(image, 0, 0);
    } else {
        throw new Error('Presentation is not initialized.');
    }
}

export function present(view) {
    // The Span view is valid only during this synchronous import. Copy into the reusable buffer.
    view.copyTo(frameView);
    drawFrame();
}

function readPresentPixel() {
    // Redraw and read in the same task because WebGL may discard its drawing buffer after presentation.
    drawFrame();
    if (backend === 'webgl2') {
        gl.readPixels(0, 0, 1, 1, gl.RGBA, gl.UNSIGNED_BYTE, pixel);
        const error = gl.getError();
        if (error !== gl.NO_ERROR) throw new Error(`WebGL2 readback error: 0x${error.toString(16)}`);
        return Array.from(pixel);
    }
    return Array.from(c2d.getImageData(0, 0, 1, 1).data);
}

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
            case 'InitPresent':
                result = initPresent(e.data.canvas, e.data.width, e.data.height, e.data.backend);
                break;
            case 'TestPresent': {
                const start = performance.now();
                const index = assemblyExports.ZXSinclair.Net.Web.Prototype.Workers.WorkTest.TestPresent();
                result = { index, workerMs: performance.now() - start };
                break;
            }
            case 'ReadPresentPixel':
                result = readPresentPixel();
                break;
            default:
                throw new Error(`Unknown command: ${e.data.command}`);
        }
        self.postMessage({ command: 'response', requestId: e.data.requestId, result });
    } catch (err) {
        self.postMessage({ command: 'response', requestId: e.data.requestId, error: err.message });
    }
});

export function createResult(value, data) {
    return { value, data };
}
