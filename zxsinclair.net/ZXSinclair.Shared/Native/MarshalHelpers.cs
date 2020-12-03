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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ZXSinclair.Native
{
    internal static class MarshalHelpers
    {
        /// <summary>
        /// Convert a pointer to a Utf8 null-terminated string to a .NET System.String
        /// </summary>
        public static unsafe string Utf8ToString(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return string.Empty;

            var ptr = (byte*)handle;
            while (*ptr != 0)
                ptr++;

            var len = ptr - (byte*)handle;
            if (len == 0)
                return string.Empty;

            var bytes = new byte[len];
            Marshal.Copy(handle, bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }
        public unsafe static int strlen(byte* str)
        {
            int result = 0;
            while (*str != 0)
            {
                result++;
                str++;
            }
            return result;
        }

        public unsafe static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return null;

            var p = (byte*)ptr;
            var length = strlen(p);

            if (length == 0)
                return string.Empty;

#if DOTNETCORE
            return Encoding.UTF8.GetString(p, length);
#else
            p[length] = 0;

            var bytes = new byte[length];

            Marshal.Copy(ptr, bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
#endif
        }

        public unsafe static IntPtr StringToHGlobalUTF8(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);

            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            *((byte*)ptr + bytes.Length) = 0;
            
            return ptr;
        }
    }
}
