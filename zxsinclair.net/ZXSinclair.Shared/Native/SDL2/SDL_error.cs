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

using static ZXSinclair.Native.SDL2.SDL;

namespace ZXSinclair.Native.SDL2
{
    public static class SDL_error
    {
        public enum SDL_errorcode
        {

            SDL_ENOMEM,
            SDL_EFREAD,
            SDL_EFWRITE,
            SDL_EFSEEK,
            SDL_UNSUPPORTED,
            SDL_LASTERROR

        }

        private delegate IntPtr SDL_GetError__t();

        private static SDL_GetError__t s_SDL_GetError__t = __LoadFunction<SDL_GetError__t>("SDL_GetError");

        static IntPtr _SDL_GetError() => s_SDL_GetError__t();
        public static string SDL_GetError() => Marshal.PtrToStringAnsi(_SDL_GetError());

        private delegate void SDL_ClearError__t();

        private static SDL_ClearError__t s_SDL_ClearError__t = __LoadFunction<SDL_ClearError__t>("SDL_ClearError");

        public static void SDL_ClearError() => s_SDL_ClearError__t();

        private delegate int SDL_Error_SDL_errorcode_t(SDL_errorcode code);

        private static SDL_Error_SDL_errorcode_t s_SDL_Error_SDL_errorcode_t = __LoadFunction<SDL_Error_SDL_errorcode_t>("SDL_Error");

        public static int SDL_Error(SDL_errorcode code) => s_SDL_Error_SDL_errorcode_t(code);
    }
}
