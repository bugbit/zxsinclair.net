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

/// <summary>CPU-independent memory and ordered bus-event comparisons.</summary>
public static class FuseComparison
{
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

    /// <summary>Checks every byte, retaining at most maxRanges contiguous ranges and an omitted-range count.</summary>
    public static IReadOnlyList<FuseMismatch> DiffMemory(byte[] expectedMemory, Func<ushort, byte> read, int maxRanges = 16)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRanges, 1);
        if (expectedMemory.Length > 65536) throw new ArgumentException("Memory exceeds the Z80 address space.", nameof(expectedMemory));
        var result = new List<FuseMismatch>();
        var wanted = new List<byte>();
        var found = new List<byte>();
        var start = -1;
        var ranges = 0;
        void Finish(int end)
        {
            if (start < 0) return;
            ranges++;
            if (result.Count < maxRanges)
            {
                var address = start == end ? $"{start:x4}" : $"{start:x4}-{end:x4}";
                result.Add(new(FuseMismatchKind.Memory, $"memory {address}",
                    string.Join(" ", wanted.Select(b => b.ToString("x2"))),
                    string.Join(" ", found.Select(b => b.ToString("x2")))));
            }
            wanted.Clear();
            found.Clear();
            start = -1;
        }
        for (var address = 0; address < expectedMemory.Length; address++)
        {
            var value = read((ushort)address);
            if (value == expectedMemory[address])
            {
                Finish(address - 1);
                continue;
            }
            if (start < 0) start = address;
            if (result.Count < maxRanges)
            {
                wanted.Add(expectedMemory[address]);
                found.Add(value);
            }
        }
        Finish(expectedMemory.Length - 1);
        if (ranges > maxRanges)
            result.Add(new(FuseMismatchKind.Memory, "memory remaining ranges", (ranges - maxRanges).ToString(),
                "omitidos", $"y {ranges - maxRanges} rangos más"));
        return result;
    }

    public static string? CompareMemory(byte[] expectedMemory, Func<ushort, byte> read) =>
        DiffMemory(expectedMemory, read, 1).FirstOrDefault()?.ToString();

    /// <summary>The first divergence with an index-aligned window and a diagnostic hint.</summary>
    public static IReadOnlyList<FuseMismatch> DiffEvents(clsTestExpected expected,
        IReadOnlyList<Z80BusEvent> actualEvents, int context = 3)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(context);
        var count = Math.Max(expected.Events.Length, actualEvents.Count);
        for (var i = 0; i < count; i++)
        {
            Z80BusEvent? wanted = i < expected.Events.Length ? expected.Events[i].ToBusEvent() : null;
            Z80BusEvent? found = i < actualEvents.Count ? actualEvents[i] : null;
            if (wanted == found) continue;
            var detail = new System.Text.StringBuilder();
            detail.AppendLine($"Total eventos: expected {expected.Events.Length}, actual {actualEvents.Count}.");
            detail.AppendLine(EventHint(wanted, found, i, actualEvents));
            detail.AppendLine($"     # | {"Esperado",-24} | Real");
            for (var row = Math.Max(0, i - context); row < Math.Min(count, (long)i + context + 1); row++)
            {
                var left = row < expected.Events.Length ? expected.Events[row].ToBusEvent().ToString() : "<missing>";
                var right = row < actualEvents.Count ? actualEvents[row].ToString() : "<missing>";
                detail.AppendLine($"{(row == i ? ">" : " ")} {row,4} | {left,-24} | {right}");
            }
            return [new(FuseMismatchKind.Event, $"bus event {i}", wanted?.ToString() ?? "<missing>",
                found?.ToString() ?? "<missing>", detail.ToString().TrimEnd())];
        }
        return [];
    }

    private static string EventHint(Z80BusEvent? wanted, Z80BusEvent? found, int index,
        IReadOnlyList<Z80BusEvent> actual)
    {
        if (found is { Type: Z80BusEventType.MR } read && wanted?.Type != Z80BusEventType.MR
            && index > 0 && actual[index - 1] is { Type: Z80BusEventType.MC } contend
            && read.Address == contend.Address)
            return "MR de más tras MC: posible lectura que FUSE modela como contención; revisar ReadDiscarded.";
        if (wanted is null) return "Evento de más al final de la secuencia real.";
        if (found is null) return "Falta un evento al final de la secuencia real.";
        if (wanted.Value.Type == found.Value.Type)
        {
            if (wanted.Value.Address != found.Value.Address) return "Mismo tipo de evento con distinta dirección.";
            if (wanted.Value.Data != found.Value.Data) return "Mismo tipo de evento con distinto dato.";
            return "Mismo tipo, dirección y dato con distinto tiempo: revisar contención o ciclos internos.";
        }
        return "Tipos de evento distintos: revisar la secuencia de ciclos de bus.";
    }

    public static string? CompareEvents(clsTestExpected expected, IReadOnlyList<Z80BusEvent> actualEvents) =>
        DiffEvents(expected, actualEvents).FirstOrDefault()?.ToString();
}
