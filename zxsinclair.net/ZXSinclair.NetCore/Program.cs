using System;
using ZXSinclair.Main;

namespace ZXSinclair.NetCore
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var pApp = new ZXSinclairApp())
            {
                pApp.Start();
            }
        }
    }
}
