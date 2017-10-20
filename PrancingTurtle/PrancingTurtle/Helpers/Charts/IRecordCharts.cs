using System;
using DotNet.Highcharts;

namespace PrancingTurtle.Helpers.Charts
{
    public interface IRecordCharts
    {
        Highcharts GuildHybridXpsOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName);
        Highcharts GuildPlayerXpsOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName, string xpsType, int topX);
        Highcharts GetEncounterDurationOverTime(int bossFightId, int difficultyId, int guildId, string guildName, string bossFightName);

        [Obsolete("This method is obsolete", true)]
        Highcharts GuildKillTimers(int bossFightId, int difficultyId);
    }
}