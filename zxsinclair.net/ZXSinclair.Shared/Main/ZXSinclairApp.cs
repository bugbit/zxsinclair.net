using System;
using System.Collections.Generic;
using System.Text;
using ZXSinclair.Native;

namespace ZXSinclair.Main
{
    public class ZXSinclairApp : IDisposable
    {
        private bool disposedValue;
        private bool mIsInit;

        public void Start()
        {
            var pRet = SDL2.SDL_Init((int)SDL2.InitFlags.Video);

            if (pRet != 0)
                throw new ApplicationException(SDL2.GetError());

            mIsInit = true;
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
                if (mIsInit)
                    SDL2.Quit();

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
