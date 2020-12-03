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
using System.Text;
using ZXSinclair.Main;
using ZXSinclair.Native;

using static ZXSinclair.Native.SDL2.SDL;
using static ZXSinclair.Native.SDL2.SDL_messagebox;
using static ZXSinclair.Native.SDL2.SDL_error;

namespace ZXSinclair.SDL.Main
{
    /*
        ZXSinclair.Net.prueba.resources
        ZXSinclair.NetCore.prueba.resources
     */
    public class ZXSinclairApp : IDisposable
    {
        private bool disposedValue;
        private string[] mArgs;
        private bool mIsSDLInit;

        public ZXCmdLine CmdLine { get; } = new ZXCmdLine();

        public ZXSinclairApp(string[] argArgs)
        {
            mArgs = argArgs;
        }

        public void Run()
        {
            if (Init())
            {

            }
            SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION, "ZXSinclair", "Hello", IntPtr.Zero);
        }

        private bool Init()
        {
            var pRet = SDL_Init(SDL_INIT_VIDEO);

            if (pRet != 0)
                throw new ApplicationException(SDL_GetError());

            mIsSDLInit = true;

            try
            {
                InitCmdLine();
            }
            catch (Exception ex)
            {
                SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "ZXSinclair", ex.Message, IntPtr.Zero);

                return false;
            }

            return true;
        }

        private void InitCmdLine()
        {
            var i = 0;

            while (i < mArgs.Length)
            {
                var pArg = mArgs[i++];

                if (pArg.StartsWith("-"))
                {
                    var pOpt = pArg.Substring(1);

                    if (pOpt.Equals("xxx", StringComparison.InvariantCultureIgnoreCase))
                    {
                    }
                    else
                        throw new ZXCmdLineException(Resources.ResManager.Instance.Strings["ParamCmdLineInvalidError", pArg]);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminar el estado administrado (objetos administrados)
                }

                // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
                if (mIsSDLInit)
                    SDL_Quit();

                // TODO: establecer los campos grandes como NULL
                disposedValue = true;
            }
        }

        // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
        ~ZXSinclairApp()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
