using System;
using System.Collections.Generic;
using Common;
using Database.Models;
using Database.QueryModels.Misc;
using Database.QueryModels.Parser;

namespace Database.Repositories.Interfaces
{
    public interface IAutoParserRepository
    {
        // SessionLog
        SessionLogInfo GetInfoByToken(string token);
        // Player
        IEnumerable<string> GetPlayerIdsNotInDb(IEnumerable<string> playerIds);
        IEnumerable<Player> GetPlayersById(IEnumerable<string> playerIds);
        ReturnValue AddPlayers(List<Player> players);
        ReturnValue UpdatePlayers(List<Player> players);
        // Ability
        IEnumerable<ShortAbility> GetShortAbilities(IEnumerable<long> abilityIds);
        IEnumerable<long> GetAbilityIdsNotInDb(IEnumerable<long> abilityIds);
        IEnumerable<long> GetAbilityIdsWithoutDamageTypes(List<long> abilityIds);
        ReturnValue AddAbilities(List<Ability> abilities);
        ReturnValue UpdateAbilityDamageTypes(Dictionary<long, string> abilities);
        // BossFight
        IEnumerable<BossFight> GetBossFights(string fightName);
        //Added 20160125
        Dictionary<int, string> GetBossFights();  
        // Encounter
        int GetEncounterId(int uploaderId, DateTime encounterDate, int bossFightId, TimeSpan duration);

        ReturnValue AddEncounter(Encounter encounter);
        EncounterInformation GetInfo(int encounterId);
        List<int> GetEncounterIdsForSession(int sessionId);
        // Ignored encounter names
        List<string> GetIgnoredEncounterNames();
        // Session
        DateTime GetFirstEncounterDateForSession(int sessionId);
        // BossFight Difficulty detection
        bool DifficultyRecordsExist(int bossFightId);
        List<BossFightDifficulty> GetDifficultySettings(int bossFightId);
        int GetDefaultDifficultyId();
        // NPC Death
        List<NpcDeath> GetExistingNpcDeaths(List<string> npcNames);
        // Record inserting
        ReturnValue AddDamageDone(List<DamageDone> damageDone);
        ReturnValue AddHealingDone(List<HealingDone> healingDone);
        ReturnValue AddShieldingDone(List<ShieldingDone> shieldingDone);
        ReturnValue AddEncounterDeaths(List<EncounterDeath> encounterDeaths);
        ReturnValue AddDebuffAction(List<EncounterDebuffAction> debuffActions);
        ReturnValue AddBuffAction(List<EncounterBuffAction> buffActions);
        ReturnValue AddNpcCasts(List<EncounterNpcCast> npcCasts);
        ReturnValue AddSessionEncounters(List<SessionEncounter> sessionEncounters);
        ReturnValue AddNpcDeath(NpcDeath npcDeath);

        ReturnValue UpdateSessionLogPostParse(int sessionLogId, long totalPlayedTime, long logSize, long totalLines);
        ReturnValue UpdateSessionPostParse(int sessionId);
        ReturnValue UpdateNpcDeath(NpcDeath npcDeath);

    }
}
