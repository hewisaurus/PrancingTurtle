using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Database;
using Database.Repositories;
using Database.Repositories.Interfaces;
using PrancingTurtle.Helpers.Charts;
using StructureMap;

namespace PrancingTurtle.DependencyResolution.Registries
{
    public class RepositoryRegistry : Registry
    {
        public RepositoryRegistry()
        {
            string ptGaleraConnectionString = ConfigurationManager.ConnectionStrings["PTGalera"].ConnectionString;

            For<IConnectionFactory>()
                .Use<MySqlConnectionFactory>()
                .Ctor<string>("connectionString")
                .Is(ptGaleraConnectionString);
            
            For<IAuthUserCharacterRepository>().Use<AuthUserCharacterRepository>();
            For<IShardRepository>().Use<ShardRepository>();
            For<IGuildRepository>().Use<GuildRepository>();
            For<IGuildRankRepository>().Use<GuildRankRepository>();
            For<IAuthUserCharacterGuildApplicationRepository>().Use<AuthUserCharacterGuildApplicationRepository>();
            For<ISessionRepository>().Use<SessionRepository>();
            For<IEncounterRepository>().Use<EncounterRepository>();
            For<INavigationRepository>().Use<NavigationRepository>();
            For<IPlayerRepository>().Use<PlayerRepository>();
            For<IUpdateRepository>().Use<UpdateRepository>();
            For<INpcRepository>().Use<NpcRepository>();
            For<IStatRepository>().Use<StatRepository>();
            For<IEncounterOverviewRepository>().Use<EncounterOverviewRepository>();
            For<ISessionEncounterRepository>().Use<SessionEncounterRepository>();
            For<ISessionLogRepository>().Use<SessionLogRepository>();
            For<IAbilityRepository>().Use<AbilityRepository>();
            For<IGuildStatusRepository>().Use<GuildStatusRepository>();
            For<ISearchRepository>().Use<SearchRepository>();
            For<IScheduledTaskRepository>().Use<ScheduledTaskRepository>();
            For<IBossFightRepository>().Use<BossFightRepository>();
            For<IInstanceRepository>().Use<InstanceRepository>();
            For<INewsRecentChangesRepository>().Use<NewsRecentChangesRepository>();
            For<IBossFightDifficultyRepository>().Use<BossFightDifficultyRepository>();
            For<ISiteNotificationRepository>().Use<SiteNotificationRepository>();
            For<IEncounterDifficultyRepository>().Use<EncounterDifficultyRepository>();
            For<IBossFightSingleTargetDetail>().Use<BossFightSingleTargetDetail>();
            For<IEncounterPlayerRoleRepository>().Use<EncounterPlayerRoleRepository>();
            For<IEncounterPlayerStatisticsRepository>().Use<EncounterPlayerStatisticsRepository>();
            For<IRecordsRepository>().Use<RecordsRepository>();
            For<IApiRepository>().Use<ApiRepository>();
            For<IAbilityRoleRepository>().Use<AbilityRoleRepository>();
            For<IRoleIconRepository>().Use<RoleIconRepository>();
            For<IPlayerClassRepository>().Use<PlayerClassRepository>();

            For<IRecordCharts>().Use<RecordCharts>();
            For<IEncounterCharts>().Use<EncounterCharts>();
            For<ILeaderboardRepository>().Use<LeaderboardRepository>();
            For<IRecurringTaskRepo>().Use<RecurringTaskRepo>();
        }
    }
}