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

using ZXSinclair.Net.Core.Memory;
using ZXSinclair.Net.Core.Timing;

namespace ZXSinclair.Net.Core.Machines.Spectrum;

public enum SpectrumModel
{
    Spectrum16K,
    Spectrum48K,
    Spectrum128K,

    /// <summary>Grey +2: same memory, paging and timing as the 128K.</summary>
    SpectrumPlus2,
}

/// <summary>
/// State of a ULA-based Spectrum (16K, 48K, 128K, grey +2) as seen by the bus: T-state counter,
/// memory, contention and floating bus tables, paging and the ULA port 0xFE.
/// The CPU reaches it through <see cref="SpectrumBus"/>, a struct wrapper used as a generic argument.
/// </summary>
public sealed class SpectrumMachine
{
    public const int ScreenPage = 1;

    /// <summary>Contention flags per page are masks, so the bus can apply them without branching.</summary>
    public const byte Contended = 0xFF;
    public const byte Uncontended = 0x00;
    public const int KeyboardHalfRows = 8;
    public const byte NoKeysPressed = 0x1F;

    /// <summary>T-states of the interrupt acknowledge cycle before the CPU starts pushing PC (FUSE value, to verify).</summary>
    public const int InterruptAcknowledgeTStates = 6;

    // Fields read by SpectrumBus in the hot path.
    internal int tstates;
    internal readonly PagedMemory memory;
    internal readonly byte[] contention;
    internal readonly byte[] contendedPages;
    internal readonly short[] floatingBus;
    internal readonly int pageShift;
    internal readonly int interruptLength;

    private readonly byte[] keyboard = new byte[KeyboardHalfRows];
    private readonly int fixedScreenBase;

    public SpectrumMachine(SpectrumModel model, bool lateTiming = false)
    {
        Model = model;

        var is128 = model is SpectrumModel.Spectrum128K or SpectrumModel.SpectrumPlus2;
        var timing = is128 ? MachineTiming.Spectrum128 : MachineTiming.Spectrum48;

        Timing = lateTiming ? timing.WithLateTiming() : timing;
        memory = new PagedMemory(model switch
        {
            SpectrumModel.Spectrum16K => Layouts.Spectrum16K(),
            SpectrumModel.Spectrum48K => Layouts.Spectrum48K(),
            _ => Layouts.Spectrum128K(),
        });
        pageShift = memory.PageShift;
        interruptLength = Timing.InterruptLength;
        contention = ContentionTable.Build(Timing);
        floatingBus = FloatingBusTable.Build(Timing);
        contendedPages = new byte[memory.PageCount];
        contendedPages[ScreenPage] = Contended;
        if (is128)
            Paging = new Spectrum128Paging(memory, contendedPages);
        else
            fixedScreenBase = memory.GetRegionBase(Layouts.SpectrumRam);
        Reset();
    }

    public SpectrumModel Model { get; }
    public MachineTiming Timing { get; }
    public PagedMemory Memory => memory;

    /// <summary>Port 0x7FFD, only on the 128K and +2.</summary>
    public Spectrum128Paging? Paging { get; }

    /// <summary>The bus to give the CPU. Pass it as a generic argument, never as <c>IZ80Bus</c>.</summary>
    public SpectrumBus Bus => new(this);

    /// <summary>T-states since the start of the current frame.</summary>
    public int TStates
    {
        get => tstates;
        set => tstates = value;
    }

    /// <summary>Border colour (0-7) from the last OUT to the ULA.</summary>
    public int Border { get; private set; }
    public bool Mic { get; private set; }
    public bool Speaker { get; private set; }

    /// <summary>EAR input bit (tape signal) returned in bit 6 of the ULA port.</summary>
    public bool EarInput { get; set; }

    /// <summary>Physical offset of the screen the ULA displays (bank 5 or 7 on the 128K).</summary>
    public int ScreenBase => Paging?.ScreenBase ?? fixedScreenBase;

    /// <summary>Loads ROM <paramref name="index"/> (0 on 16K/48K; 0 or 1 on the 128K/+2).</summary>
    public void LoadRom(int index, ReadOnlySpan<byte> rom)
    {
        var romCount = Paging is null ? 1 : 2;

        if ((uint)index >= (uint)romCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        memory.LoadRegion(index, rom);
    }

    /// <summary>Presses or releases a key: half row 0-7 (selected by A8-A15), bit 0-4 inside it.</summary>
    public void SetKey(int halfRow, int bit, bool pressed)
    {
        if (pressed)
            keyboard[halfRow] &= (byte)~(1 << bit);
        else
            keyboard[halfRow] |= (byte)(1 << bit);
    }

    /// <summary>
    /// Starts the next frame. Must be called once T-states reach <see cref="MachineTiming.TStatesPerFrame"/>
    /// and before they exceed it by <see cref="ContentionTable.Margin"/>.
    /// </summary>
    public void EndFrame() => tstates -= Timing.TStatesPerFrame;

    public void Reset()
    {
        tstates = 0;
        Border = 0;
        Mic = false;
        Speaker = false;
        EarInput = false;
        Array.Fill(keyboard, NoKeysPressed);
        Paging?.Reset();
    }

    /// <summary>Byte the ULA has on the data bus at the current T-state.</summary>
    internal byte FloatingBusValue()
    {
        var offset = floatingBus[tstates];

        return offset == FloatingBusTable.Idle ? (byte)0xFF : memory.ReadPhysical(ScreenBase + offset);
    }

    internal byte ReadUlaPort(ushort port)
    {
        var value = NoKeysPressed;
        var rows = port >> 8;

        for (var row = 0; row < KeyboardHalfRows; row++)
        {
            if ((rows & (1 << row)) == 0)
                value &= keyboard[row];
        }

        return (byte)(value | 0xA0 | (EarInput ? 0x40 : 0));
    }

    internal void WriteUlaPort(byte value)
    {
        Border = value & 0x07;
        Mic = (value & 0x08) != 0;
        Speaker = (value & 0x10) != 0;
    }
}
