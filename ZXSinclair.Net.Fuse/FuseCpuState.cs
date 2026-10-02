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

public static class FuseCpuState
{
    public static Z80Registers Load(clsTestBase test)
    {
        var a = test.Line1;
        var b = test.Line2;
        return new Z80Registers
        {
            AF = a.af, BC = a.bc, DE = a.de, HL = a.hl,
            AF_ = a.af_, BC_ = a.bc_, DE_ = a.de_, HL_ = a.hl_,
            IX = a.ix, IY = a.iy, SP = a.sp, PC = a.pc,
            I = (byte)b.i, R = (byte)b.r, IFF1 = b.iff1 != 0,
            IFF2 = b.iff2 != 0, IM = (byte)b.im, Halted = b.halted != 0,
            // FUSE SCF/CCF assume a preceding flag writer (spec-instr-control.md 5.1).
            Q = (byte)a.af,
        };
    }

    /// <summary>All fields represented by FUSE; WZ, Q and EI delay are absent from its format.</summary>
    public static IReadOnlyList<FuseMismatch> Diff(clsTestBase expected, in Z80Registers actual, int cycles)
    {
        var e = Load(expected);
        var result = new List<FuseMismatch>();
        void Register(string name, ushort wanted, ushort found, int width = 4, string? detail = null)
        {
            if (wanted != found)
                result.Add(new(FuseMismatchKind.Register, name, wanted.ToString($"x{width}"), found.ToString($"x{width}"), detail));
        }
        void State(string name, string wanted, string found)
        {
            if (wanted != found) result.Add(new(FuseMismatchKind.State, name, wanted, found));
        }
        void Flags(string name, byte wanted, byte found)
        {
            if (wanted != found)
                result.Add(new(FuseMismatchKind.Flags, name,
                    $"{wanted:x2} ({DecodeFlags(wanted)})", $"{found:x2} ({DecodeFlags(found)})",
                    $"Difieren: {DecodeFlags((byte)(wanted ^ found))}"));
        }
        Register("A", e.A, actual.A, 2);
        Flags("F", e.F, actual.F);
        Register("BC", e.BC, actual.BC);
        Register("DE", e.DE, actual.DE);
        Register("HL", e.HL, actual.HL);
        Register("A'", (byte)(e.AF_ >> 8), (byte)(actual.AF_ >> 8), 2);
        Flags("F'", (byte)e.AF_, (byte)actual.AF_);
        Register("BC'", e.BC_, actual.BC_);
        Register("DE'", e.DE_, actual.DE_);
        Register("HL'", e.HL_, actual.HL_);
        Register("IX", e.IX, actual.IX);
        Register("IY", e.IY, actual.IY);
        Register("SP", e.SP, actual.SP);
        Register("PC", e.PC, actual.PC);
        Register("I", e.I, actual.I, 2);
        var refreshDifference = e.R ^ actual.R;
        Register("R", e.R, actual.R, 2, (refreshDifference & 0x80) == 0
            ? "Difieren solo los 7 bits bajos de R."
            : (refreshDifference & 0x7F) == 0 ? "Difiere solo el bit 7 de R." : "Difieren el bit 7 y los bits bajos de R.");
        State("IFF1", e.IFF1.ToString(), actual.IFF1.ToString());
        State("IFF2", e.IFF2.ToString(), actual.IFF2.ToString());
        State("IM", e.IM.ToString(), actual.IM.ToString());
        State("halted", e.Halted.ToString(), actual.Halted.ToString());
        if (expected.Line2.endtstates != cycles)
            result.Add(new(FuseMismatchKind.TStates, "T-states", expected.Line2.endtstates.ToString(), cycles.ToString()));
        return result;
    }

    public static string DecodeFlags(byte flags)
    {
        string[] names = ["S", "Z", "5", "H", "3", "P/V", "N", "C"];
        var set = new List<string>();
        for (var bit = 7; bit >= 0; bit--)
            if ((flags & (1 << bit)) != 0) set.Add(names[7 - bit]);
        return set.Count == 0 ? "ninguno" : string.Join(" ", set);
    }

    public static string? Compare(clsTestBase expected, in Z80Registers actual, int cycles) =>
        Diff(expected, in actual, cycles).FirstOrDefault()?.ToString();
}
