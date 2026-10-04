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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZXSinclair.Net.Core.Z80;

namespace ZXSinclair.Net.Core.Machines.Spectrum;

/// <summary>
/// Z80 bus of a ULA-based Spectrum (spec, sections 5-8). A struct holding a single reference so that
/// <c>Z80Cpu&lt;SpectrumBus&gt;</c> is specialised by the JIT and every call is inlined.
/// </summary>
public readonly struct SpectrumBus : IZ80Bus
{
    private const ushort PagingPortMask = 0x8002;

    private readonly SpectrumMachine machine;

    public SpectrumBus(SpectrumMachine machine) => this.machine = machine;

    public int Cycles => machine.tstates;

    public bool IntActive => machine.tstates < machine.interruptLength;

    public void Reset() => machine.Reset();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte FetchOpcode(ushort address)
    {
        Cycle(address, 4);

        return machine.memory.Read(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read(ushort address)
    {
        Cycle(address, 3);

        return machine.memory.Read(address);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadDiscarded(ushort address) => Read(address);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort address, byte data)
    {
        Cycle(address, 3);
        machine.memory.Write(address, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Internal(ushort address, int tstates)
    {
        var m = machine;
        var t = m.tstates;

        if (IsContended(address))
        {
            for (var i = 0; i < tstates; i++)
                t += Delay(t) + 1;
        }
        else
            t += tstates;
        m.tstates = t;
    }

    /// <summary>
    /// IN: I/O contention sequence of section 5.4. The device is read after the first phase
    /// (N:1 or C:1), as FUSE does.
    /// </summary>
    public byte In(ushort port)
    {
        var highContended = IsContended(port);
        var ula = (port & 1) == 0;

        IoEarly(highContended);

        byte value;

        if (ula)
            value = machine.ReadUlaPort(port);
        else
            value = machine.FloatingBusValue(); // includes reads of 0x7FFD (spec 4.3, to verify)

        IoLate(highContended, ula);

        return value;
    }

    /// <summary>OUT: same contention sequence as <see cref="In"/>; the device is written after the first phase.</summary>
    public void Out(ushort port, byte data)
    {
        var highContended = IsContended(port);
        var ula = (port & 1) == 0;

        IoEarly(highContended);
        if (ula)
            machine.WriteUlaPort(data);
        if ((port & PagingPortMask) == 0)
            machine.Paging?.Write(data);
        IoLate(highContended, ula);
    }

    /// <summary>Interrupt acknowledge: the ULA does not drive the bus, so the vector is 0xFF (to verify).</summary>
    public byte AcknowledgeInterrupt()
    {
        machine.tstates += SpectrumMachine.InterruptAcknowledgeTStates;

        return 0xFF;
    }

    /// <summary>0xFF if the page of <paramref name="address"/> is contended, 0 otherwise.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ContentionMask(ushort address) =>
        Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(machine.contendedPages), address >> machine.pageShift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsContended(ushort address) => ContentionMask(address) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Delay(int tstate)
    {
        Debug.Assert((uint)tstate < (uint)machine.contention.Length, "EndFrame was not called in time.");

        return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(machine.contention), tstate);
    }

    /// <summary>
    /// One machine cycle: contention at its first T-state, then its cost. Branch-free (the delay is
    /// masked by the page's contention flag) and the T-state counter is read and written once.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Cycle(ushort address, int cost)
    {
        var m = machine;
        var t = m.tstates;

        m.tstates = t + (Delay(t) & ContentionMask(address)) + cost;
    }

    private void IoEarly(bool highContended)
    {
        var t = machine.tstates;

        if (highContended)
            t += Delay(t);
        machine.tstates = t + 1;
    }

    private void IoLate(bool highContended, bool ula)
    {
        var t = machine.tstates;

        if (ula)
            t += Delay(t) + 3;
        else if (highContended)
        {
            for (var i = 0; i < 3; i++)
                t += Delay(t) + 1;
        }
        else
            t += 3;
        machine.tstates = t;
    }
}
