using Common;
using LogParserConcept.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LogParserConcept
{
    public static class Methods
    {
        public static async Task ParseAsync(SessionLogInfo logInfo, string logFile)
        {
            Debug.WriteLine($"Beginning to parse {logFile}");
            await using var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
            using var sr = new StreamReader(fs);
            // TODO: Stop being lazy while testing and check for the actual timezone.
            // TODO: Stop being lazy while testing and check for the log type instead of assuming that we're logging with Standard.
            var logType = LogType.Standard;


        }
    }
}
