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

namespace ZXSinclair.Net.Core.Timing;

/// <summary>
/// Frame timing of a ULA-based Spectrum (Specs/spec-buses-memoria.md, sections 3, 5 and 6).
/// T-state values are measured from the start of the INT signal.
/// </summary>
public sealed record MachineTiming
{
    public const int ScreenLines = 192;
    public const int ScreenTStatesPerLine = 128;

    public required int TStatesPerLine { get; init; }
    public required int LinesPerFrame { get; init; }
    public required int FirstContendedTState { get; init; }
    public required int FloatingBusFirstTState { get; init; }
    public required int InterruptLength { get; init; }

    /// <summary>"Late timing" ULAs do everything one T-state later.</summary>
    public bool LateTiming { get; init; }

    public int TStatesPerFrame => TStatesPerLine * LinesPerFrame;

    /// <summary>Offset applied to contention and floating bus by late timing.</summary>
    public int LateTimingOffset => LateTiming ? 1 : 0;

    /// <summary>16K and 48K: 224 T/line, 312 lines, 69 888 T/frame.</summary>
    public static MachineTiming Spectrum48 { get; } = new()
    {
        TStatesPerLine = 224,
        LinesPerFrame = 312,
        FirstContendedTState = 14335,
        FloatingBusFirstTState = 14338,
        InterruptLength = 32,
    };

    /// <summary>
    /// 128K and grey +2: 228 T/line, 311 lines, 70 908 T/frame.
    /// The INT length of 36 T-states is FUSE's value and still has to be verified (spec, section 10).
    /// </summary>
    public static MachineTiming Spectrum128 { get; } = new()
    {
        TStatesPerLine = 228,
        LinesPerFrame = 311,
        FirstContendedTState = 14361,
        FloatingBusFirstTState = 14364,
        InterruptLength = 36,
    };

    public MachineTiming WithLateTiming() => this with { LateTiming = true };
}
