using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class NullLogger: ILogger
    {
        public void Info(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Info(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }

        public void Debug(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Debug(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }

        public void Warn(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Warn(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }

        public void Error(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Error(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }

        public void Fatal(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Fatal(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }

        public void Trace(string message, string sourceFilePath = null)
        {
            return;
        }

        public void Trace(string message, Exception exc, string sourceFilePath = null)
        {
            return;
        }
    }
}
