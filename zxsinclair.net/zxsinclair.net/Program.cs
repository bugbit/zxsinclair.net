﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXSinclair.Main;

namespace ZXSinclair.Net
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
