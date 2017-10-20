using System;
using System.Runtime.CompilerServices;

namespace Logging
{
    public interface ILogger
    {
        void Info(string message, [CallerFilePath] string sourceFilePath = null);
        void Info(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);

        void Debug(string message, [CallerFilePath] string sourceFilePath = null);
        void Debug(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);

        void Warn(string message, [CallerFilePath] string sourceFilePath = null);
        void Warn(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);

        void Error(string message, [CallerFilePath] string sourceFilePath = null);
        void Error(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);

        void Fatal(string message, [CallerFilePath] string sourceFilePath = null);
        void Fatal(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);

        void Trace(string message, [CallerFilePath] string sourceFilePath = null);
        void Trace(string message, Exception exc, [CallerFilePath] string sourceFilePath = null);
    }
}
