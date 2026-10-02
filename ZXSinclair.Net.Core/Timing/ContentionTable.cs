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
/// Precomputed ULA contention delays, one byte per T-state of the frame (spec, sections 5.2 and 9.3),
/// so the bus pays a single table read per contended access.
/// </summary>
public static class ContentionTable
{
    /// <summary>
    /// Extra entries after the end of the frame, for instructions that start before the frame ends and
    /// finish after it. The machine must call EndFrame before T-states run past this margin.
    /// </summary>
    public const int Margin = 256;

    private static readonly byte[] Pattern = { 6, 5, 4, 3, 2, 1, 0, 0 };

    public static byte[] Build(MachineTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);

        var table = new byte[timing.TStatesPerFrame + Margin];
        var first = timing.FirstContendedTState + timing.LateTimingOffset;

        for (var line = 0; line < MachineTiming.ScreenLines; line++)
        {
            var start = first + line * timing.TStatesPerLine;

            for (var pos = 0; pos < MachineTiming.ScreenTStatesPerLine; pos++)
                table[start + pos] = Pattern[pos & 7];
        }

        return table;
    }
}
