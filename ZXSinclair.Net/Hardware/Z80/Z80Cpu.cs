#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZXSincalir.Net.
    ZXSincalir.Net is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

namespace ZXSinclair.Net.Hardware.Z80;

// http://www.zilog.com/docs/z80/um0080.pdf
// http://www.myquest.nl/z80undocumented/z80-documented-v0.91.pdf
// http://www.z80.info/zip/z80-documented.pdf

public unsafe partial class Z80Cpu : Cpu<ushort, byte, Z80Pins, Z80Regs>
{
    protected static readonly byte[] mTablePV;
    protected static readonly byte[] mTableZS53;

    static Z80Cpu()
    {
        mTablePV = CreateTablePV();
        mTableZS53 = CreateTableZS53();
    }

    private static byte[] CreateTablePV()
    {
        var pTable = new byte[0x100];

        for (var pByte = 0x00; pByte <= 0xFF; pByte++)
            pTable[(byte)pByte] = Z80Flags.GetParity((byte)pByte);

        return pTable;
    }

    private static byte[] CreateTableZS53()
    {
        var pTable = new byte[0x100];

        for (var pByte = 0x00; pByte <= 0xFF; pByte++)
            pTable[(byte)pByte] = (byte)(pByte & (uint)(Z80Flags.S | Z80Flags.F3 | Z80Flags.F5));
        pTable[0] |= Z80Flags.Z;

        return pTable;
    }

    public Z80Cpu(IMemoryBuffer<byte> buffer, IMemory<ushort, byte>? memory = null) : base(buffer ?? new MemoryBuffer8Bit(), memory) { }

#if Z80_OPCODES_TEST
    public bool instrNotImp { get; protected set; } = false;
#endif

    public override void Reset()
    {
#if Z80_OPCODES_TEST
        instrNotImp = false;
#endif
        base.Reset();
    }

    public override byte ReadOpCode(ushort address)
    {
        var opcode = base.ReadOpCode(address);

        Ticks.AddCycles(4);
        Regs.RefreshR();

        return opcode;
    }

    public override byte ReadMemory(ushort address)
    {
        var data = base.ReadMemory(address);

        Ticks.AddCycles(3);

        return data;
    }

    public ushort ReadWordMemoryPCAndINC()
    {
        var r = Regs;

        var r2 = new Z80Reg16Bits();

        r2.L = ReadMemory(r.GetPCAndInc());
        r2.H = ReadMemory(r.GetPCAndInc());

        return r2.W;
    }

    public override void WriteMemory(ushort address, byte data)
    {
        base.WriteMemory(address, data);

        Ticks.AddCycles(3);
    }

    public override void Instrfetch()
    {
        var opcode = ReadOpCode(Regs.GetPCAndInc());

        ExecOpCode(opcode);
    }

    public void InstrfetchDD()
    {
        var opcode = ReadOpCode(Regs.GetPCAndInc());

        ExecOpCodeDD(opcode);
    }

    public void InstrfetchFD()
    {
        var opcode = ReadOpCode(Regs.GetPCAndInc());

        ExecOpCodeFD(opcode);
    }

    public void InstrfetchED()
    {
        var opcode = ReadOpCode(Regs.GetPCAndInc());

        ExecOpCodeED(opcode);
    }

    public byte Read_M_BC_M() => ReadMemory(Regs.BC);
    public byte Read_M_DE_M() => ReadMemory(Regs.DE);
    public byte Read_M_HL_M() => ReadMemory(Regs.HL);

    public byte Read_M_nnn_M()
    {
        var nnn = ReadWordMemoryPCAndINC();
        var data = ReadMemory(nnn);

        return data;
    }

    public byte Read_M_IX_PLUS_D_M()
    {
        var d = (sbyte)ReadMemory(Regs.GetPCAndInc());

        Ticks.AddCycles(5);

        var n = ReadMemory(Regs.GetIX_d(d));

        return n;
    }

    public byte Read_M_IY_PLUS_D_M()
    {
        var d = (sbyte)ReadMemory(Regs.GetPCAndInc());

        Ticks.AddCycles(5);

        var n = ReadMemory(Regs.GetIY_d(d));

        return n;
    }

    // public void Write_M_HL_M(byte data) => WriteMemory(Regs.HL, data);
    // public void LD_M_HL_M_A()
    // {
    //     var r = Regs;

    //     WriteMemory(r.HL, r.A);
    // }

    public void Nop() { }

    protected void LD_A_I()
    {
        var rr = Regs;
        var n = rr.I;

        rr.SetA_n(n);

        /*
         S is set if the I Register is negative; otherwise, it is reset.
Z is set if the I Register is 0; otherwise, it is reset.
H is reset.
P/V contains contents of IFF2.
N is reset.
C is not affected.
If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
         */

        var f2 = rr.F;
        var f = (byte)((uint)(f2 & Z80Flags.C) | mTableZS53[n]);

        if (Pins.HasFlag(Z80Pins.IFF2))
            f |= Z80Flags.PV;

        rr.SetF(f);

        Ticks.AddCycles(1);
    }

    protected void LD_A_R()
    {
        var rr = Regs;
        var n = rr.R;

        rr.SetA_n(n);

        /*
         S is set if the I Register is negative; otherwise, it is reset.
Z is set if the I Register is 0; otherwise, it is reset.
H is reset.
P/V contains contents of IFF2.
N is reset.
C is not affected.
If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
         */

        var f2 = rr.F;
        var f = (byte)((uint)(f2 & Z80Flags.C) | mTableZS53[n]);

        if (Pins.HasFlag(Z80Pins.IFF2))
            f |= Z80Flags.PV;

        rr.SetF(f);

        Ticks.AddCycles(1);
    }

    // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    ~Z80Cpu()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: false);
    }
}
