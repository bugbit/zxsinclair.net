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
    internal static class OS
    {
        [DllImport("libc")]
        static extern int uname(IntPtr buf);

        public static readonly bool IsWindows;
        public static readonly bool IsMacOSX;
        public static readonly bool IsLinux;
        public static readonly string Rid;

        static OS()
        {
#if DOTNET
            Rid = "";
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                    IsWindows = true;
                    break;
                case PlatformID.MacOSX:
                    IsMacOSX = true;
                    break;
                case PlatformID.Unix:
                    IsMacOSX = true;
                    var buf = IntPtr.Zero;

                    try
                    {
                        buf = Marshal.AllocHGlobal(8192);

                        if (uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Linux")
                        {
                            IsMacOSX = false;
                            IsLinux = true;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        if (buf != IntPtr.Zero)
                            Marshal.FreeHGlobal(buf);
                    }

                    break;
            }
#elif DOTNETCORE
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        IsWindows = true;
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        IsMacOSX = true;
    else
        IsLinux = true;
#endif
            if (OS.IsWindows && Environment.Is64BitProcess)
                Rid = "win-x64";
            else if (OS.IsWindows && !Environment.Is64BitProcess)
                Rid = "win-x86";
            else if (OS.IsLinux)
                Rid = "linux-x64";
            else if (OS.IsMacOSX)
                Rid = "osx";
            else
                Rid = "unknown";
        }
    }
}
