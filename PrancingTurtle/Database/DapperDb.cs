using Dapper;
using Database.Models;
using AuthUser = Database.Models.AuthUser;
using AuthUserCharacter = Database.Models.AuthUserCharacter;
using AuthUserCharacterGuildApplication = Database.Models.AuthUserCharacterGuildApplication;
using BossFight = Database.Models.BossFight;
using Encounter = Database.Models.Encounter;
using EncounterNpc = Database.Models.EncounterNpc;
using Guild = Database.Models.Guild;
using Instance = Database.Models.Instance;
using Player = Database.Models.Player;
using Session = Database.Models.Session;
using SessionEncounter = Database.Models.SessionEncounter;
using SessionLog = Database.Models.SessionLog;
using EncounterPlayerStatistics = Database.Models.EncounterPlayerStatistics;
using EncounterPlayerRole = Database.Models.EncounterPlayerRole;
using EncounterOverview = Database.Models.EncounterOverview;
using NpcDeath = Database.Models.NpcDeath;

namespace Database
{
    public class DapperDb : Database<DapperDb>
    {
        public Table<PlayerClass> PlayerClassTable { get; set; }
        public Table<Soul> SoulTable { get; set; }
        public Table<Ability> AbilityTable { get; set; }
        public Table<Instance> InstanceTable { get; set; }
        public Table<BossFight> BossFightTable { get; set; }
        public Table<Buff> BuffTable { get; set; }
        public Table<AuthUser> AuthUserTable { get; set; }
        public Table<AuthUserCharacter> AuthUserCharacterTable { get; set; }
        public Table<Guild> GuildTable { get; set; }
        public Table<AuthUserCharacterGuildApplication> AuthUserCharacterGuildApplicationTable { get; set; }
        public Table<Session> SessionTable { get; set; }
        public Table<SessionLog> SessionLogTable { get; set; }
        public Table<SessionEncounter> SessionEncounterTable { get; set; } 
        public Table<EncounterOverview> EncounterOverviewTable { get; set; }
        public Table<Player> PlayerTable { get; set; }
        public Table<Encounter> EncounterTable { get; set; }
        public Table<EncounterPlayerRole> EncounterPlayerRoleTable { get; set; }
        public Table<EncounterNpc> EncounterNpcTable { get; set; }
        public Table<EncounterPlayerStatistics> EncounterPlayerStatisticsTable { get; set; }
        public Table<NpcDeath> NpcDeathTable { get; set; } 
    }
}
