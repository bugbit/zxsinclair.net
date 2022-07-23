using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZXSinclair.Net.Test
{
    public class clsTestLine1
    {
        public ushort af, bc, de, hl, af_, bc_, de_, hl_, ix, iy, sp, pc;

        public void read(string line)
        {
            //&af, &bc, &de, &hl, &af_, &bc_, &de_, &hl_, &ix, &iy, &sp, &pc

            var reghs = line.Split(' ').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            Debug.Assert(reghs.Length == 12);

            int i = 0;

            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.af));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.bc));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.de));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.hl));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.af_));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.bc_));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.de_));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.hl_));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.ix));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.iy));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.sp));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.pc));
        }
    }
    public class clsTestLine2
    {
        public ushort i, r, iff1, iff2, im;
        public int halted;
        public ushort endtstates;

        public void read(string line)
        {
            // &i, &r, &iff1, &iff2, &im, &halted, &end_tstates2
            var reghs = line.Split(' ').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            Debug.Assert(reghs.Length == 7);

            int i = 0;

            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out this.i));
            Debug.Assert(HelperNumber.TryUShortHex(reghs[i++], out r));

            Debug.Assert(ushort.TryParse(reghs[i++], out this.iff1));
            Debug.Assert(ushort.TryParse(reghs[i++], out this.iff2));
            Debug.Assert(ushort.TryParse(reghs[i++], out this.im));
            Debug.Assert(int.TryParse(reghs[i++], out this.halted));
            Debug.Assert(ushort.TryParse(reghs[i++], out this.endtstates));
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
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                    break;

                i++;

                var reghs = line.Split(' ');
                var j = 0;
                var addressstr = reghs[j++];

                if (string.Equals(addressstr, "-1"))
                    break;

                Debug.Assert(HelperNumber.TryUShortHex(addressstr, out ushort address));

                var len = (string.Equals(reghs.LastOrDefault(), "-1")) ? reghs.Length - 2 : reghs.Length - 1;
                var data = reghs.Skip(j).Take(len).Select(h => HelperNumber.HexToShort(h)).ToArray();

                Debug.Assert(data.All(h => h.HasValue));

                memories.Add(new clsTestMemory { Address = address, Data = data.Select(h => (byte)h.Value).ToArray() });
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
        public string type;
        public ushort address;
        public byte? data;

        public static clsTestEvent? Read(string line)
        {
            /*
        <time> <type> <address> <data>

<time> is simply the time at which the event occurs.
<type> is one of MR (memory read), MW (memory write), MC (memory
       contend), PR (port read), PW (port write) or PC (port contend).
<address> is the address (or IO port) affected.
<data> is the byte written or read. Missing for contentions.
        */
            var reghs = line.Split(' ').Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            byte? data;

            if (reghs.Length != 4 && reghs.Length != 3)
                return null;

            if (!ushort.TryParse(reghs[0], out ushort time))
                return null;

            if (!HelperNumber.TryUShortHex(reghs[2], out ushort address))
                return null;

            if (reghs.Length == 3)
                data = null;
            else
            {
                if (!HelperNumber.TryUShortHex(reghs[3], out ushort data2))
                    return null;

                data = (byte)data2;
            }

            return new clsTestEvent
            {
                time = time,
                type = reghs[1],
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
}