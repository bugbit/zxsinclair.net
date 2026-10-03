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

using ZXSinclair.Net.Core.Abstractions;

namespace ZXSinclair.Net.Core.Z80;

/// <summary>
/// Bus seen by a Z80 CPU (Specs/spec-buses-memoria.md, section 9.2). Every operation applies the
/// machine's contention at the T-state where its machine cycle starts and then adds its own cost,
/// so the CPU only emits cycles and knows nothing about the ULA.
/// Use it only as a generic constraint: <c>Z80Cpu&lt;TBus&gt; where TBus : struct, IZ80Bus</c>.
/// </summary>
public interface IZ80Bus : IBus, IBusData<ushort, byte>, IBusIo<ushort, byte>
{
    /// <summary>M1 opcode fetch: contention at T1, then 4 T-states (including refresh).</summary>
    byte FetchOpcode(ushort address);

    /// <summary>
    /// Memory read of an operand that a conditional instruction does not use to jump (JR/JP/CALL/DJNZ
    /// not taken): same cycle, contention and 3 T-state cost as <c>Read</c>, and it returns the byte.
    /// It exists so recording buses can follow FUSE, which logs this cycle as contention only (MC without MR).
    /// </summary>
    byte ReadDiscarded(ushort address);

    /// <summary>Internal CPU cycles with <paramref name="address"/> on the bus: one contended T-state each.</summary>
    void Internal(ushort address, int tstates);

    /// <summary>True while the INT line is active.</summary>
    bool IntActive { get; }

    /// <summary>Interrupt acknowledge cycle: 6 T-states (M1 with IORQ and two wait states). Returns the byte on the data bus.</summary>
    byte AcknowledgeInterrupt();
}
