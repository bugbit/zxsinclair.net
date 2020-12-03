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

using static ZXSinclair.Native.MarshalHelpers;
using static ZXSinclair.Native.SDL2.SDL;

namespace ZXSinclair.Native.SDL2
{
    public static class SDL_messagebox
    {
        public enum SDL_MessageBoxFlags
        {

            SDL_MESSAGEBOX_ERROR = 0x00000010,   /**< error dialog */
            SDL_MESSAGEBOX_WARNING = 0x00000020,   /**< warning dialog */
            SDL_MESSAGEBOX_INFORMATION = 0x00000040    /**< informational dialog */

        }
        public enum SDL_MessageBoxButtonFlags
        {

            SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT = 0x00000001,  /**< Marks the default button when return is hit */
            SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT = 0x00000002   /**< Marks the default button when escape is hit */

        }
        public enum SDL_MessageBoxColorType
        {

            SDL_MESSAGEBOX_COLOR_BACKGROUND,
            SDL_MESSAGEBOX_COLOR_TEXT,
            SDL_MESSAGEBOX_COLOR_BUTTON_BORDER,
            SDL_MESSAGEBOX_COLOR_BUTTON_BACKGROUND,
            SDL_MESSAGEBOX_COLOR_BUTTON_SELECTED,
            SDL_MESSAGEBOX_COLOR_MAX

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_MessageBoxButtonData
        {

            public UInt32 flags;       /**< ::SDL_MessageBoxButtonFlags */
            public int buttonid;       /**< User defined button id (value returned via SDL_ShowMessageBox) */
            public IntPtr text;  /**< The UTF-8 button text */

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_MessageBoxColor
        {

            public byte r, g, b;

        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_MessageBoxColorScheme
        {
            public unsafe fixed byte colors[15];
            // SDL_MessageBoxColor colors[SDL_MESSAGEBOX_COLOR_MAX];
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_MessageBoxData
        {

            public UInt32 flags;                       /**< ::SDL_MessageBoxFlags */
            public IntPtr window;                 /**< Parent window, can be NULL */
            public IntPtr title;                  /**< UTF-8 title */
            public IntPtr message;                /**< UTF-8 message text */

            public int numbuttons;
            public IntPtr buttons;

            public IntPtr colorScheme;   /**< ::SDL_MessageBoxColorScheme, can be NULL to use system settings */

        }

        private delegate int SDL_ShowMessageBox_SDL_MessageBoxData_int_t(ref SDL_MessageBoxData messageboxdata, ref int buttonid);

        private static SDL_ShowMessageBox_SDL_MessageBoxData_int_t s_SDL_ShowMessageBox_SDL_MessageBoxData_int_t = __LoadFunction<SDL_ShowMessageBox_SDL_MessageBoxData_int_t>("SDL_ShowMessageBox");

        public static int SDL_ShowMessageBox(ref SDL_MessageBoxData messageboxdata, ref int buttonid) => s_SDL_ShowMessageBox_SDL_MessageBoxData_int_t(ref messageboxdata, ref buttonid);

        private delegate int SDL_ShowSimpleMessageBox_UInt32_IntPtr_IntPtr_IntPtr_t(UInt32 flags, IntPtr title, IntPtr message, IntPtr window);

        private static SDL_ShowSimpleMessageBox_UInt32_IntPtr_IntPtr_IntPtr_t s_SDL_ShowSimpleMessageBox_UInt32_IntPtr_IntPtr_IntPtr_t = __LoadFunction<SDL_ShowSimpleMessageBox_UInt32_IntPtr_IntPtr_IntPtr_t>("SDL_ShowSimpleMessageBox");

        public static int SDL_ShowSimpleMessageBox(UInt32 flags, IntPtr title, IntPtr message, IntPtr window) => s_SDL_ShowSimpleMessageBox_UInt32_IntPtr_IntPtr_IntPtr_t(flags, title, message, window);

        public static int SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags flags, string title, string message, IntPtr window)
        {
            var titlePtr = StringToHGlobalUTF8(title);
            var messagePtr = StringToHGlobalUTF8(message);

            return SDL_ShowSimpleMessageBox((uint)flags, titlePtr, messagePtr, window);
        }
    }
}
