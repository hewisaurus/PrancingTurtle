using System.Collections.Generic;
using Database.Models;
using Database.QueryModels.Misc;

namespace Database.Repositories.Interfaces
{
    public interface IRecordsRepository
    {
        List<EncounterPlayerStatistics> GetTopDpsOverTime(int bossFightId, int difficultyId, string playerClass = "NA");
        List<EncounterPlayerStatistics> GetTopDpsOverTime(int bossFightId, int difficultyId, int guildId);

        List<EncounterPlayerStatistics> GetTopXpsOverTime(int bossFightId, int difficultyId, string playerClass = "NA");
        List<EncounterPlayerStatistics> GetTopXpsOverTime(int bossFightId, int difficultyId, int guildId, string xpsType);

        List<Encounter> GetGuildStatsOverTimeHybrid(int bossFightId, int difficultyId, int guildId);
        List<Encounter> GetEncounterDurationOverTime(int bossFightId, int difficultyId, int guildId);

        // TOP Player-based
        RankPlayerGuild GetSingleTopDps(int bossFightId, int d = -1);
        RankPlayerGuild GetSingleTopHps(int bossFightId, int d = -1);
        RankPlayerGuild GetSingleTopAps(int bossFightId, int d = -1);
        //TOP Guild-based
        RankPlayerGuild GetSingleTopDpsGuild(int bossFightId, int d = -1);
        RankPlayerGuild GetSingleTopHpsGuild(int bossFightId, int d = -1);
        RankPlayerGuild GetSingleTopApsGuild(int bossFightId, int d = -1);
        // Specific guild-based
        List<RankPlayerGuild> GetTopGuildDps(int bossFightId, int guildId, int d = -1);
        List<RankPlayerGuild> GetTopGuildHps(int bossFightId, int guildId, int d = -1);
        List<RankPlayerGuild> GetTopGuildAps(int bossFightId, int guildId, int d = -1);
    }
}
