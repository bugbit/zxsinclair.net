using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZXSinclair.Net.Test
{
    public static class HelperNumber
    {
        public static ushort? HexToShort(string hex) => (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ushort num)) ? num : null;

        public static bool TryUShortHex(string hex, out ushort num)
        {
            var numh = HexToShort(hex);

            if (numh.HasValue)
            {
                num = numh.Value;

                return true;
            }

            num = default(ushort);

            return false;
        }
    }
}