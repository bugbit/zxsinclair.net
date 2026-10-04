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

namespace ZXSinclair.Net.Core.Z80;

public sealed partial class Z80Cpu<TBus>
{
    private void ExitHalt()
    {
        if (Registers.Halted)
        {
            Registers.Halted = false;
            Registers.PC++;
        }
    }

    private void AcceptNmi()
    {
        Registers.Q = 0;
        Registers.SpecialLoadPending = false;
        ExitHalt();
        nmiPending = false;
        Registers.EiPending = false;
        Registers.IFF1 = false;
        bus.FetchOpcode(Registers.PC);
        Registers.IncrementR();
        bus.Internal(Registers.IR, 1);
        Push(Registers.PC);
        Registers.PC = Registers.WZ = 0x0066;
    }

    private void AcceptInterrupt()
    {
        Registers.Q = 0;
        if (Registers.SpecialLoadPending)
            Registers.F &= unchecked((byte)~Z80Flags.PV);
        Registers.SpecialLoadPending = false;
        ExitHalt();
        Registers.IFF1 = Registers.IFF2 = false;
        Registers.IncrementR();
        var data = bus.AcknowledgeInterrupt();
        switch (Registers.IM)
        {
            case 0:
                if (IsIm0SingleByte(data))
                    ExecuteMain(data);
                else
                    Unimplemented();
                return;
            case 1:
                bus.Internal(Registers.IR, 1);
                Push(Registers.PC);
                Registers.PC = 0x0038;
                break;
            case 2:
                bus.Internal(Registers.IR, 1);
                Push(Registers.PC);
                var vector = (ushort)((Registers.I << 8) | data);
                var low = bus.Read(vector);
                Registers.PC = (ushort)(low | bus.Read(unchecked((ushort)(vector + 1))) << 8);
                break;
            default:
                Unimplemented();
                return;
        }
        Registers.WZ = Registers.PC;
    }
}
