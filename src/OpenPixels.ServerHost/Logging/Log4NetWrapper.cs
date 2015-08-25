using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.Server.Logging
{
    class Log4NetWrapper : ILog
    {
        private readonly log4net.ILog _logger;

        public Log4NetWrapper(log4net.ILog logger)
        {
            _logger = logger;
        }

        public bool IsErrorEnabled
        {
            get { return _logger.IsErrorEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _logger.IsWarnEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _logger.IsInfoEnabled; }
        }

        public bool IsDebugEnabled
        {
            get { return _logger.IsDebugEnabled; }
        }

        public bool IsVerboseEnabled
        {
            get { return _logger.Logger.IsEnabledFor(log4net.Core.Level.Verbose); }
        }


        public void Error(object message)
        {
            _logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            _logger.Error(message, exception);
        }


        public void Warn(object message)
        {
            _logger.Warn(message);
        }

        public void WarnFormat(string message, params object[] args)
        {
            _logger.WarnFormat(message, args);
        }


        public void Info(object message)
        {
            _logger.Info(message);
        }

        public void InfoFormat(string message, params object[] args)
        {
            _logger.InfoFormat(message, args);
        }


        public void Debug(object message)
        {
            _logger.Debug(message);
        }

        public void DebugFormat(string message, params object[] args)
        {
            _logger.DebugFormat(message, args);
        }


        public void Verbose(object message)
        {
            if (IsVerboseEnabled)
            {
                _logger.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                    log4net.Core.Level.Verbose,
                    message,
                    null);
            }
        }

        public void VerboseFormat(string message, params object[] args)
        {
            if (IsVerboseEnabled)
            {
                _logger.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                    log4net.Core.Level.Verbose,
                    new log4net.Util.SystemStringFormat(CultureInfo.InvariantCulture, message, args),
                    null);
            }
        }
    }
}
