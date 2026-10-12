class ZxAudioTestProcessor extends AudioWorkletProcessor {
    constructor() {
        super();
        this.buffers = new Array(8).fill(null);
        this.lengths = new Int32Array(8);
        this.head = 0;
        this.count = 0;
        this.offset = 0;
        this.running = false;
        this.primed = false;
        this.last = 0;
        this.consumed = 0;
        this.underruns = 0;
        this.port.onmessage = e => {
            this.audioPort = e.data.port;
            this.audioPort.onmessage = event => this.receive(event.data);
        };
    }

    receive(message) {
        if (message.type === 'start') {
            this.target = message.target;
            this.running = true;
            this.primed = false;
            this.consumed = 0;
            this.underruns = 0;
        } else if (message.type === 'stop') {
            this.running = false;
            if (!this.count) this.stopped();
        } else if (message.type === 'samples') {
            if (this.count === this.buffers.length) throw new Error('Audio FIFO overflow.');
            const tail = (this.head + this.count) % this.buffers.length;
            this.buffers[tail] = message.buffer;
            this.lengths[tail] = message.length;
            this.count++;
            if (this.count >= this.target) this.primed = true;
        } else if (message.type === 'stats') {
            this.audioPort.postMessage({ type: 'stats', id: message.id, ...this.stats() });
        }
    }

    stats() {
        return { consumed: this.consumed, underruns: this.underruns, queued: this.count };
    }

    stopped() {
        this.primed = false;
        this.audioPort.postMessage({ type: 'stopped', ...this.stats() });
    }

    process(_, outputs) {
        const output = outputs[0][0];
        let missed = false;
        for (let i = 0; i < output.length; i++) {
            if (this.primed && this.count) {
                const buffer = this.buffers[this.head];
                output[i] = this.last = buffer[this.offset++];
                this.consumed++;
                if (this.offset === this.lengths[this.head]) {
                    this.buffers[this.head] = null;
                    this.head = (this.head + 1) % this.buffers.length;
                    this.count--;
                    this.offset = 0;
                    this.audioPort.postMessage({ type: 'returned', buffer, ...this.stats() }, [buffer.buffer]);
                    if (!this.running && !this.count) this.stopped();
                }
            } else {
                output[i] = this.last *= 0.98;
                if (this.running && this.primed && !missed) {
                    this.underruns++;
                    missed = true;
                }
            }
        }
        return true;
    }
}

registerProcessor('zx-audio-test', ZxAudioTestProcessor);
