using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SDL2;

namespace ZXSinclairNet
{
    public sealed class ZXSinclair : IDisposable
    {
        public static void Main(string[] args)
        {
            var pApp = new ZXSinclair();
            var pRet = SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            if (pRet == 0)
            {
                SDL.SDL_Quit();
            }
            if (pRet != 0)
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "ZXSinclair", SDL.SDL_GetError(), IntPtr.Zero);

            pApp.Dispose();
        }
        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: elimine el estado administrado (objetos administrados).
                }

                // TODO: libere los recursos no administrados (objetos no administrados) y reemplace el siguiente finalizador.
                // TODO: configure los campos grandes en nulos.

                disposedValue = true;
            }
        }

        // TODO: reemplace un finalizador solo si el anterior Dispose(bool disposing) tiene código para liberar los recursos no administrados.
        // ~ZXSinclair() {
        //   // No cambie este código. Coloque el código de limpieza en el anterior Dispose(colocación de bool).
        //   Dispose(false);
        // }

        // Este código se agrega para implementar correctamente el patrón descartable.
        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el anterior Dispose(colocación de bool).
            Dispose(true);
            // TODO: quite la marca de comentario de la siguiente línea si el finalizador se ha reemplazado antes.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
