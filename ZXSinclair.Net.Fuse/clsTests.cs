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

namespace ZXSinclair.Net.Fuse
{
    public class clsTestLine1
    {
        public ushort af, bc, de, hl, af_, bc_, de_, hl_, ix, iy, sp, pc;

        public void read(string line)
        {
            // af bc de hl af' bc' de' hl' ix iy sp pc
            var fields = FuseFormat.Fields(line, 12);
            var i = 0;

            af = FuseFormat.Hex(fields[i++]);
            bc = FuseFormat.Hex(fields[i++]);
            de = FuseFormat.Hex(fields[i++]);
            hl = FuseFormat.Hex(fields[i++]);
            af_ = FuseFormat.Hex(fields[i++]);
            bc_ = FuseFormat.Hex(fields[i++]);
            de_ = FuseFormat.Hex(fields[i++]);
            hl_ = FuseFormat.Hex(fields[i++]);
            ix = FuseFormat.Hex(fields[i++]);
            iy = FuseFormat.Hex(fields[i++]);
            sp = FuseFormat.Hex(fields[i++]);
            pc = FuseFormat.Hex(fields[i++]);
        }
    }
    public class clsTestLine2
    {
        public ushort i, r, iff1, iff2, im;
        public int halted;
        public ushort endtstates;

        public void read(string line)
        {
            // i r iff1 iff2 im halted end_tstates
            var fields = FuseFormat.Fields(line, 7);
            var j = 0;

            i = FuseFormat.Hex(fields[j++]);
            r = FuseFormat.Hex(fields[j++]);
            iff1 = FuseFormat.Decimal(fields[j++]);
            iff2 = FuseFormat.Decimal(fields[j++]);
            im = FuseFormat.Decimal(fields[j++]);
            halted = FuseFormat.Decimal(fields[j++]);
            endtstates = FuseFormat.Decimal(fields[j++]);
        }
    }
    public class clsTestMemory
    {
        public ushort Address { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        public static clsTestMemory[] Read(string[] lines, ref int i)
        {
            var memories = new List<clsTestMemory>();

            while (i < lines.Length)
            {
                var line = lines[i++];

                if (string.IsNullOrWhiteSpace(line))
                    break;

                var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (fields[0] == "-1")
                    break;

                var address = FuseFormat.Hex(fields[0]);
                var length = fields[^1] == "-1" ? fields.Length - 2 : fields.Length - 1;
                var data = new byte[length];

                for (var j = 0; j < length; j++)
                {
                    var value = FuseFormat.Hex(fields[j + 1]);

                    if (value > 0xFF)
                        throw new FormatException($"Memory byte out of range: '{fields[j + 1]}'.");
                    data[j] = (byte)value;
                }

                memories.Add(new clsTestMemory { Address = address, Data = data });
            }

            return memories.ToArray();
        }
    }
    public class clsTestBase
    {
        public string Name { get; set; } = "";
        public clsTestLine1 Line1 { get; } = new();
        public clsTestLine2 Line2 { get; } = new();
        public clsTestMemory[] Memories { get; set; } = new clsTestMemory[0];
    }
    public class clsTestEvent
    {
        /*
        <time> <type> <address> <data>

<time> is simply the time at which the event occurs.
<type> is one of MR (memory read), MW (memory write), MC (memory
       contend), PR (port read), PW (port write) or PC (port contend).
<address> is the address (or IO port) affected.
<data> is the byte written or read. Missing for contentions.
        */
        public ushort time;
        public string type = "";
        public ushort address;
        public byte? data;

        public Z80BusEvent ToBusEvent() => new(time, Enum.Parse<Z80BusEventType>(type), address, data);

        /// <summary>Parses an event line, or returns null if the line is not an event (the register line follows).</summary>
        public static clsTestEvent? Read(string line)
        {
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            byte? data;

            if (fields.Length != 4 && fields.Length != 3)
                return null;

            if (!ushort.TryParse(fields[0], out ushort time))
                return null;

            if (!HelperNumber.TryUShortHex(fields[2], out ushort address))
                return null;

            if (fields.Length == 3)
                data = null;
            else
            {
                if (!HelperNumber.TryUShortHex(fields[3], out ushort data2))
                    return null;

                data = (byte)data2;
            }

            return new clsTestEvent
            {
                time = time,
                type = fields[1],
                address = address,
                data = data
            };
        }

        public static clsTestEvent[] ReadEvents(string[] lines, ref int i)
        {
            var events = new List<clsTestEvent>();
            clsTestEvent? _event;

            while (i < lines.Length && (_event = Read(lines[i])) != null)
            {
                events.Add(_event);
                i++;
            }

            return events.ToArray();
        }
    }
    public class clsTestIn
    {
        public clsTestBase Base { get; } = new();
    }

    public class clsTestExpected
    {
        public clsTestBase Base { get; } = new();
        public clsTestEvent[] Events { get; set; } = new clsTestEvent[0];
    }

    /// <summary>Field parsing for the FUSE files. Errors throw <see cref="FormatException"/> in Debug and Release.</summary>
    internal static class FuseFormat
    {
        public static string[] Fields(string line, int count)
        {
            var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length != count)
                throw new FormatException($"Expected {count} fields, found {fields.Length}: '{line}'.");

            return fields;
        }

        public static ushort Hex(string field) =>
            HelperNumber.TryUShortHex(field, out var value) ? value : throw new FormatException($"Invalid hexadecimal value '{field}'.");

        public static ushort Decimal(string field) =>
            ushort.TryParse(field, out var value) ? value : throw new FormatException($"Invalid decimal value '{field}'.");
    }
}
