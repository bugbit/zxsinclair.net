using System;
using SDL2;

namespace TestSDL
{
    class MainClass
    {
        public static void Main(string[] args)
        {           
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.WriteLine($"SDL_Init Error: {SDL.SDL_GetError()}");
                return;
            }

            IntPtr win = SDL.SDL_CreateWindow("Hello World!", 100, 100, 640, 480, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (win == IntPtr.Zero)
            {
                Console.WriteLine($"SDL_CreateWindow Error: {SDL.SDL_GetError()}");
                SDL.SDL_Quit();
                return;
            }

            IntPtr ren = SDL.SDL_CreateRenderer(win, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (ren == IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(win);
                Console.WriteLine($"SDL_CreateRenderer Error: {SDL.SDL_GetError()}");
                SDL.SDL_Quit();
                return;
            }


            //IntPtr namep = SDL2.LPUtf8StrMarshaler.GetInstance().MarshalManagedToNative ("hello.bmp");
            IntPtr bmp = SDL.SDL_LoadBMP("hello.bmp");

            //SDL2.LPUtf8StrMarshaler.GetInstance ().CleanUpNativeData (namep);
            if (bmp == IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(ren);
                SDL.SDL_DestroyWindow(win);
                Console.WriteLine($"SDL_LoadBMP Error: {SDL.SDL_GetError()}");
                SDL.SDL_Quit();
                return;
            }

            /*
			IntPtr tex = SDL.SDL_CreateTextureFromSurface(ren, bmp);
			SDL.SDL_FreeSurface(bmp);
			if (tex == IntPtr.Zero){
				SDL.SDL_DestroyRenderer(ren);
				SDL.SDL_DestroyWindow(win);
				Console.WriteLine($"SDL_CreateTextureFromSurface Error: {SDL.SDL_GetError()}");
				SDL.SDL_Quit();
				return;
			}
            */

            IntPtr tex = SDL.SDL_CreateTexture(ren, SDL.SDL_PIXELFORMAT_ARGB8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, 320, 320);

            if (tex == IntPtr.Zero)
            {
                SDL.SDL_DestroyRenderer(ren);
                SDL.SDL_DestroyWindow(win);
                Console.WriteLine($"SDL_CreateTextureFromSurface Error: {SDL.SDL_GetError()}");
                SDL.SDL_Quit();
                return;
            }

            UInt32[] pixels = new UInt32[320 * 320];

            unsafe
            {
                fixed (UInt32* ptr = &pixels[0])
                {
                    UInt32* ptr2 = ptr;
                    for (var i = 320 * 320; i-- > 0;)
                        *ptr2++ = 0xFF00FF00; // rgra 0xAARRGGRR

                    SDL.SDL_UpdateTexture(tex, IntPtr.Zero, (IntPtr)ptr, 320 * 4);
                }
            }

            

            

            //A sleepy rendering loop, wait for 3 seconds and render and present the screen each time
            for (int i = 0; i < 3; ++i)
            {
                //First clear the renderer
                SDL.SDL_RenderClear(ren);
                //Draw the texture
                SDL.SDL_RenderCopy(ren, tex, IntPtr.Zero, IntPtr.Zero);
                //Update the screen
                SDL.SDL_RenderPresent(ren);
                //Take a quick break after all that hard work
                SDL.SDL_Delay(1000);
            }

            SDL.SDL_DestroyTexture(tex);
            SDL.SDL_DestroyRenderer(ren);
            SDL.SDL_DestroyWindow(win);
            SDL.SDL_Quit();
        }
    }
}
