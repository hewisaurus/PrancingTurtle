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
            //var logPath = @"F:\PrancingTurtle\InputLogs\3crg4s0lva5-CombatLog.txt";
            //3crg4s0lva5-CombatLog

            //TODO: Stop being lazy and hardcoding while testing

            var info = new SessionLogInfo
            {
                SessionDate = new DateTime(2019, 12, 23, 7, 11, 00),
                SessionId = 18243,
                OwnerGuild = "Ducktales",
                OwnerName = "Toar",
                OwnerShard = "Laethys",
                PublicSession = false,
                SessionName = "Some session name",
                //UploaderTimezone = "UTC"
                UploaderTimezone = "AUS Eastern Standard Time"
            };

            //var logPath = @"F:\PrancingTurtle\InputLogs\lxsnngvaax3\CombatLog.txt";

            ////TODO: Stop being lazy and hardcoding while testing

            //var info = new SessionLogInfo
            //{
            //    SessionDate = new DateTime(2017, 11, 30),
            //    SessionId = 16926,
            //    OwnerGuild = "UnknownGuild",
            //    OwnerName = "Someone",
            //    OwnerShard = "SomeShard",
            //    PublicSession = true,
            //    SessionName = "Some session name",
            //    UploaderTimezone = "UTC"
            //};

            await Methods.ParseAsync(info, logPath);

            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();
        }
    }
}
