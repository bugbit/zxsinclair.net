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

namespace ZXSinclair.Net.Fuse;

/// <summary>
/// CPU-independent checks of a FUSE test result. They will be used again when the Z80 of
/// ZXSinclair.Net.Core runs the tests through a recording test bus (Specs/spec-cpu-z80.md, section 8.1).
/// </summary>
public static class FuseComparison
{
    /// <summary>Memory expected after the test: the initial memory with the expected blocks applied.</summary>
    public static byte[] ExpectedMemory(byte[] initialMemory, clsTestExpected expected)
    {
        var memory = (byte[])initialMemory.Clone();

        foreach (var block in expected.Base.Memories)
        {
            var address = block.Address;

            foreach (var data in block.Data)
                memory[address++] = data;
        }

        return memory;
    }

    /// <summary>Returns a description of the first differing byte, or null if both memories match.</summary>
    public static string? CompareMemory(byte[] expectedMemory, Func<ushort, byte> read)
    {
        for (var address = 0; address < expectedMemory.Length; address++)
        {
            var actual = read((ushort)address);

            if (actual != expectedMemory[address])
                return $"memory {address:x4}: expected {expectedMemory[address]:x2}, actual {actual:x2}";
        }

        return null;
    }

    /// <summary>Returns a description of the first differing bus event, or null if both sequences match.</summary>
    public static string? CompareEvents(clsTestExpected expected, IReadOnlyList<Z80BusEvent> actualEvents)
    {
        var expectedEvents = expected.Events;

        for (var i = 0; i < Math.Max(expectedEvents.Length, actualEvents.Count); i++)
        {
            Z80BusEvent? expectedEvent = i < expectedEvents.Length ? expectedEvents[i].ToBusEvent() : null;
            Z80BusEvent? actualEvent = i < actualEvents.Count ? actualEvents[i] : null;

            if (expectedEvent != actualEvent)
                return $"bus event {i}: expected [{expectedEvent?.ToString() ?? "<missing>"}], actual [{actualEvent?.ToString() ?? "<missing>"}] (counts {expectedEvents.Length}/{actualEvents.Count})";
        }

        return null;
    }
}
