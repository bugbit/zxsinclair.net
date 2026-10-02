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

using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Fuse;

public sealed class FuseTestBusState
{
    public byte[] Memory { get; } = new byte[0x10000];
    public int Cycles;
    public List<Z80BusEvent>? Events;
}

public struct FuseTestBus : IZ80Bus
{
    private readonly FuseTestBusState state;

    public FuseTestBus(FuseTestBusState state) => this.state = state;
    public int Cycles => state.Cycles;
    public bool IntActive => false;
    public void Reset() => state.Cycles = 0;

    private void Record(Z80BusEventType type, ushort address, byte? data = null) =>
        state.Events?.Add(new(state.Cycles, type, address, data));

    private byte MemoryRead(ushort address, int cost)
    {
        Record(Z80BusEventType.MC, address);
        state.Cycles += cost;
        var value = state.Memory[address];
        Record(Z80BusEventType.MR, address, value);
        return value;
    }

    public byte FetchOpcode(ushort address) => MemoryRead(address, 4);
    public byte Read(ushort address) => MemoryRead(address, 3);

    // FUSE's contend_read: the cycle is contended and timed, but no MR event is logged.
    public byte ReadDiscarded(ushort address)
    {
        Record(Z80BusEventType.MC, address);
        state.Cycles += 3;
        return state.Memory[address];
    }

    public void Write(ushort address, byte data)
    {
        Record(Z80BusEventType.MC, address);
        state.Cycles += 3;
        state.Memory[address] = data;
        Record(Z80BusEventType.MW, address, data);
    }

    public void Internal(ushort address, int tstates)
    {
        for (var i = 0; i < tstates; i++)
        {
            Record(Z80BusEventType.MC, address);
            state.Cycles++;
        }
    }

    public byte AcknowledgeInterrupt()
    {
        state.Cycles += 7;
        return 0xFF;
    }

    private void IoEarly(ushort port)
    {
        if ((port & 0xC000) == 0x4000)
            Record(Z80BusEventType.PC, port);
        state.Cycles++;
    }

    private void IoLate(ushort port)
    {
        if ((port & 1) == 0)
        {
            Record(Z80BusEventType.PC, port);
            state.Cycles += 3;
        }
        else if ((port & 0xC000) == 0x4000)
        {
            for (var i = 0; i < 3; i++)
            {
                Record(Z80BusEventType.PC, port);
                state.Cycles++;
            }
        }
        else
            state.Cycles += 3;
    }

    public byte In(ushort port)
    {
        IoEarly(port);
        // FUSE's test machine returns the high byte of the port.
        var value = (byte)(port >> 8);
        Record(Z80BusEventType.PR, port, value);
        IoLate(port);
        return value;
    }

    public void Out(ushort port, byte data)
    {
        IoEarly(port);
        Record(Z80BusEventType.PW, port, data);
        IoLate(port);
    }
}
