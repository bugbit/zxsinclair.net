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
/// Precomputed floating bus (spec, section 6): for each T-state of the frame, the offset inside the
/// visible screen (bitmap 0x0000-0x17FF, attributes 0x1800-0x1AFF) of the byte the ULA has on the
/// data bus, or <see cref="Idle"/> when the bus is idle and reads 0xFF.
/// </summary>
public static class FloatingBusTable
{
    public const short Idle = -1;

    public const int AttributesOffset = 0x1800;

    public static short[] Build(MachineTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);

        var table = new short[timing.TStatesPerFrame + ContentionTable.Margin];
        var first = timing.FloatingBusFirstTState + timing.LateTimingOffset;

        Array.Fill(table, Idle);
        for (var line = 0; line < MachineTiming.ScreenLines; line++)
        {
            var start = first + line * timing.TStatesPerLine;

            for (var pos = 0; pos < MachineTiming.ScreenTStatesPerLine; pos++)
            {
                var column = (pos >> 3) * 2;

                table[start + pos] = (pos & 7) switch
                {
                    0 => BitmapOffset(line, column),
                    1 => AttributeOffset(line, column),
                    2 => BitmapOffset(line, column + 1),
                    3 => AttributeOffset(line, column + 1),
                    _ => Idle,
                };
            }
        }

        return table;
    }

    /// <summary>Offset of the bitmap byte of screen line <paramref name="y"/> (0-191), column <paramref name="x"/> (0-31).</summary>
    public static short BitmapOffset(int y, int x) =>
        (short)(((y & 0xC0) << 5) | ((y & 0x07) << 8) | ((y & 0x38) << 2) | x);

    /// <summary>Offset of the attribute byte for screen line <paramref name="y"/>, column <paramref name="x"/>.</summary>
    public static short AttributeOffset(int y, int x) =>
        (short)(AttributesOffset + ((y >> 3) << 5) + x);
}
