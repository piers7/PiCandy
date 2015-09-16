using OpenPixels.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RpiWs2812OpcServer
{
    public class ConsoleLogger : ILog
    {
        TextWriter _out;

        public ConsoleLogger()
        {
            _out = Console.Out;
            IsInfoEnabled = IsWarnEnabled = IsErrorEnabled = IsDebugEnabled = true;
        }

        public bool IsErrorEnabled { get;set; }

        public void Error(object message, Exception exception)
        {
            if (IsErrorEnabled)
            {
                WriteLine(MethodBase.GetCurrentMethod(), message);
                if (exception != null)
                    WriteLine(MethodBase.GetCurrentMethod(), exception);
            }
        }

        public bool IsWarnEnabled { get; set; }

        public void Warn(object message)
        {
            if (IsWarnEnabled) WriteLine(MethodBase.GetCurrentMethod(), message);
        }

        public void WarnFormat(string message, params object[] args)
        {
            if (IsWarnEnabled) WriteLine(MethodBase.GetCurrentMethod(), message, args);
        }

        public bool IsInfoEnabled {get;set;}

        public void Info(object message)
        {
            if (IsInfoEnabled) WriteLine(MethodBase.GetCurrentMethod(), message);
        }

        public void InfoFormat(string message, params object[] args)
        {
            if (IsInfoEnabled) WriteLine(MethodBase.GetCurrentMethod(), message, args);
        }

        public bool IsDebugEnabled {get;set;}

        public void Debug(object message)
        {
            if (IsDebugEnabled) WriteLine(MethodBase.GetCurrentMethod(), message);
        }

        public void DebugFormat(string message, params object[] args)
        {
            if (IsDebugEnabled) WriteLine(MethodBase.GetCurrentMethod(), message, args);
        }

        public bool IsVerboseEnabled {get;set;}

        public void Verbose(object message)
        {
            if (IsVerboseEnabled) WriteLine(MethodBase.GetCurrentMethod(), message);
        }

        public void VerboseFormat(string message, params object[] args)
        {
            if (IsVerboseEnabled) WriteLine(MethodBase.GetCurrentMethod(), message, args);
        }

        private void WriteLine(MethodBase methodBase, object message)
        {
            _out.WriteLine("[" + methodBase.Name + "] " + message);
        }

        private void WriteLine(MethodBase methodBase, string message, object[] args)
        {
            _out.WriteLine("[" + methodBase.Name + "] " + message, args);
        }
    }
}
