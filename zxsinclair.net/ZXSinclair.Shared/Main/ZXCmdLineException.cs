using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ZXSinclair.Main
{
    class ZXCmdLineException : ApplicationException
    {
        public ZXCmdLineException()
        {
        }

        public ZXCmdLineException(string message) : base(message)
        {
        }

        public ZXCmdLineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ZXCmdLineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
