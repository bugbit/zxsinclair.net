using System;
using System.Collections.Generic;
using System.Text;

namespace ZXSinclair.Resources
{
    public class ResManager
    {
        private static readonly Lazy<ResManager> mInstance = new Lazy<ResManager>(() => new ResManager());


        private ResManager()
        {
            
        }
        public static ResManager Instance => mInstance.Value;
    }
}
