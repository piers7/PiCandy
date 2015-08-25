using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server
{
    public sealed class NullLogger : ILog
    {
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger()
        {}

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsVerboseEnabled
        {
            get { return false; }
        }


        public void Error(object message, Exception exception)
        {
        }


        public void Warn(object message)
        {
        }

        public void WarnFormat(string message, params object[] args)
        {
        }


        public void Info(object message)
        {
        }

        public void InfoFormat(string message, params object[] args)
        {
        }


        public void Debug(object message)
        {
        }

        public void DebugFormat(string message, params object[] args)
        {
        }


        public void Verbose(object message)
        {
        }

        public void VerboseFormat(string message, params object[] args)
        {
        }
    }
}
