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

public unsafe partial class Z80Cpu : Cpu<byte, Z80Pins, Z80Regs>
{
    public Z80Cpu(IMemoryBuffer<byte> buffer, IMemory<byte>? memory = null) : base(buffer ?? new MemoryBuffer8Bit(), memory) { }

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

    public byte Read_M_BC_M() => ReadMemory(Regs.BC);
    public byte Read_M_DE_M() => ReadMemory(Regs.DE);
    public byte Read_M_HL_M() => ReadMemory(Regs.HL);

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

    public void Nop() { }

    // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    ~Z80Cpu()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: false);
    }
}
