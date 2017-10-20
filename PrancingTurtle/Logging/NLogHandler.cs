using System;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;

namespace Logging
{
    public class NLogHandler : ILogger
    {
        private static Logger GetInnerLogger(string sourceFilePath)
        {
            var logger = sourceFilePath == null ? LogManager.GetCurrentClassLogger() : LogManager.GetLogger(Path.GetFileName(sourceFilePath));
            return logger;
        }

        public void Info(string message, [CallerFilePath] string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Info(message);
        }

        public void Info(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Info(message, exc);
        }

        public void Debug(string message, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Debug(message);
        }

        public void Debug(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Debug(message, exc);
        }

        public void Warn(string message, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Warn(message);
        }

        public void Warn(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Warn(message, exc);
        }

        public void Error(string message, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Error(message);
        }

        public void Error(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Error(message, exc);
        }

        public void Fatal(string message, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Fatal(message);
        }

        public void Fatal(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Fatal(message, exc);
        }

        public void Trace(string message, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Trace(message);
        }

        public void Trace(string message, Exception exc, [CallerFilePath]string sourceFilePath = null)
        {
            GetInnerLogger(sourceFilePath).Trace(message, exc);
        }
    }
}
