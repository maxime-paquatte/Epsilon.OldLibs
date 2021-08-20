using System;
using System.Collections.Generic;
using System.Text;

namespace Epsilon.Messaging.Host.Tests.Impl
{
    class BusLogger : IBusLogger
    {
        public void Log(string cmdId, string message)
        {
            Console.WriteLine(cmdId + ":\t" + message);
        }
    }
}
