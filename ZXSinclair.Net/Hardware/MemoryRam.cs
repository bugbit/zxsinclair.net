namespace ZXSinclair.Net.Hardware;

public class MemoryRam16Bits<D> : CpuMemory<ushort, D>
{
    public MemoryRam16Bits(IMemoryBuffer<D> buffer) : base(buffer)
    {
    }

    public override D Read(ushort address) => Buffer.Read(address);

    public override D ReadOpCode(ushort address) => Buffer.Read(address);

    public override void Write(ushort address, D data) => Buffer.Write(address, data);
}
