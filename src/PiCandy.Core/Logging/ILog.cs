using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Logging
{
    public interface ILog
    {
        bool IsErrorEnabled { get; }
        void Error(object message, Exception exception);

        bool IsWarnEnabled { get; }
        void Warn(object message);
        void WarnFormat(string message, params object[] args);

        bool IsInfoEnabled { get; }
        void Info(object message);
        void InfoFormat(string message, params object[] args);

        bool IsDebugEnabled { get; }
        void Debug(object message);
        void DebugFormat(string message, params object[] args);

        bool IsVerboseEnabled { get; }
        void Verbose(object message);
        void VerboseFormat(string message, params object[] args);
    }
}
