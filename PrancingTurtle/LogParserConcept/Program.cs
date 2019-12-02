using System;
using System.Threading.Tasks;
using LogParserConcept.Models;

namespace LogParserConcept
{
    /// <summary>
    /// .NET Core 3.0 log parser PoC with the aim to move to a serverless application.
    /// Some paths will be hardcoded here for the sake of testing.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            var logPath = @"F:\PrancingTurtle\InputLogs\CombatLog.txt";

            //TODO: Stop being lazy and hardcoding while testing

            var info = new SessionLogInfo
            {
                SessionDate = new DateTime(2019, 11, 18, 2, 32, 00),
                SessionId = 18200,
                OwnerGuild = "Casually Elite",
                OwnerName = "Kyela",
                OwnerShard = "Faeblight",
                PublicSession = true,
                SessionName = "Some session name",
                UploaderTimezone = "UTC"
            };

            await Methods.ParseAsync(info, logPath);

            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();
        }
    }
}
