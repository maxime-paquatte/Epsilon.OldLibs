using System;
using System.Collections.Generic;
using System.Text;

namespace Epsilon.BackgroundTask
{
    public interface IConfig
    {
        string ConnectionString { get; }
    }
    public interface IExceptionLoggerService
    {
        long Log(Exception ex);
    }
}
