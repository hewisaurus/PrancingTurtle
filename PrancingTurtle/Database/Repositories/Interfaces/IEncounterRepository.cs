using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Database.Models;
using Database.Models.Misc;
using Database.QueryModels;
using Database.QueryModels.Misc;
using Encounter = Database.Models.Encounter;
using EncounterPlayerStatistics = Database.Models.EncounterPlayerStatistics;
using EncounterPlayerRole = Database.Models.EncounterPlayerRole;
using EncounterNpc = Database.Models.EncounterNpc;

namespace Database.Repositories.Interfaces
{
    public interface IEncounterRepository
    {
        #region QUERY METHODS

        Encounter Get(int id);

        /// <summary>
        /// Get a list of all the encounter IDs that have no duration (00:00:00)
        /// </summary>
        /// <returns></returns>
        List<int> GetEncounterIdsWithNoDuration();

        /// <summary>
        /// Gets the duration (in seconds) from the DamageDone table
        /// Useful for where encounters have no duration for whatever reason
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetTotalSecondsFromDamageDone(int id);

        /// <summary>
        /// Gets a list of all of the successful encounters for a given BossFightId
        /// </summary>
        /// <param name="bossFightId"></param>
        /// <returns></returns>
        List<Encounter> GetSuccessfulEncounters(int bossFightId);
        List<Encounter> GetSuccessfulEncounters(int bossFightId, bool onlyNotValidForRankings);

        List<Encounter> GetSuccessfulEncountersSince(DateTime date);
        List<Encounter> GetUnsuccessfulEncountersBefore(DateTime date);
        List<Encounter> GetUnsuccessfulEncountersBefore(DateTime date, int bossFightId);

        /// <summary>
        /// Gets the single top total damage taken for the named NPC
        /// If there are multiple records for the same NPC name, then the one with the most damage taken will be returned.
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="npcName"></param>
        /// <returns></returns>
        long GetTopDamageTakenForNpc(int encounterId, string npcName);

        /// <summary>
        /// Gets the list of fastest kills for a particular encounter, not limited in number of results
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<Encounter> GetFastestKills(int id, int d = -1);
        List<EncounterPlayerStatistics> GetTopDamageHits(int id);
        List<EncounterPlayerStatistics> GetTopHealingHits(int id);
        List<EncounterPlayerStatistics> GetTopShieldHits(int id);

        // Get date sorted kills for record charts
        List<Encounter> GetDateSortedKills(int id, int d = -1);

        /// <summary>
        /// Returns the DPS for an encounter, if the duration is known
        /// </summary>
        /// <param name="id">The Encounter ID</param>
        /// <param name="totalSeconds">The length (in seconds) of the encounter</param>
        /// <returns>The average DPS for the given encounter</returns>
        long GetEncounterDps(int id, int totalSeconds);

        /// <summary>
        /// Returns the HPS for an encounter, if the total duration is known
        /// </summary>
        /// <param name="id">The Encounter ID</param>
        /// <param name="totalSeconds">The length (in seconds) of the encounter</param>
        /// <returns>The average HPS for the given encounter</returns>
        long GetEncounterHps(int id, int totalSeconds);

        /// <summary>
        /// Returns the APS for an encounter, if the total duration is known
        /// </summary>
        /// <param name="id">The Encounter ID</param>
        /// <param name="totalSeconds">The length (in seconds) of the encounter</param>
        /// <returns>The average APS for the given encounter</returns>
        long GetEncounterAps(int id, int totalSeconds);
        int GetTotalPlayerDeaths(int id);
        List<int> GetAllPlayerDeathTimers(int id);
        List<int> GetAllNpcDeathTimers(int id);
        List<EncounterDeath> GetDeaths(int id);
        List<PlayerIdDeathCount> CountDeathsPerPlayer(int id);

        #region Comparison Methods

        List<DamageDone> GetDamageForEncounter(int id, List<int> playerIds);
        List<HealingDone> GetHealingForEncounter(int id, List<int> playerIds);
        List<ShieldingDone> GetShieldingForEncounter(int id, List<int> playerIds);
        #endregion

        #region Character-based (Player or NPC) Encounter info

        List<CharacterBuffAction> GetMainRaidBuffs(int id);
        List<CharacterBuffAction> GetCharacterBuffs(int id, string target);
        List<CharacterDebuffAction> GetCharacterDebuffs(int id, string target);

        #endregion
        #region Overview
        // X Done
        List<OverviewPlayerSomethingDone> GetOverviewPlayerDamageDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerDamageDoneGraph(int id);
        List<OverviewNpcSomethingDone> GetOverviewNpcDamageDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcDamageDoneGraph(int id);

        List<OverviewPlayerSomethingDone> GetOverviewPlayerHealingDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerHealingDoneGraph(int id);
        List<OverviewNpcSomethingDone> GetOverviewNpcHealingDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcHealingDoneGraph(int id);

        List<OverviewPlayerSomethingDone> GetOverviewPlayerShieldingDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewPlayerShieldingDoneGraph(int id);
        List<OverviewNpcSomethingDone> GetOverviewNpcShieldingDone(int id);
        List<OverviewCharacterSomethingDoneGraph> GetOverviewNpcShieldingDoneGraph(int id);
        // X Taken
        List<OverviewPlayerSomethingTaken> GetOverviewPlayerDamageTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerDamageTakenGraph(int id);
        List<OverviewNpcSomethingTaken> GetOverviewNpcDamageTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcDamageTakenGraph(int id);
        List<string> GetOverviewNpcDamageTakenTop25Abilities(int id, string npcId, string filter);
        List<OverviewPlayerSomethingTaken> GetOverviewPlayerHealingTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerHealingTakenGraph(int id);
        List<OverviewNpcSomethingTaken> GetOverviewNpcHealingTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcHealingTakenGraph(int id);

        List<OverviewPlayerSomethingTaken> GetOverviewPlayerShieldingTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewPlayerShieldingTakenGraph(int id);
        List<OverviewNpcSomethingTaken> GetOverviewNpcShieldingTaken(int id);
        List<OverviewCharacterSomethingTakenGraph> GetOverviewNpcShieldingTakenGraph(int id);
        #endregion
        #region Detail
        List<DetailDamageByPlane> GetDetailDamageToNpcsByPlane(int encounterId);
        List<DetailDamageByPlane> GetDetailDamageToPlayersByPlane(int encounterId);
        List<DetailDamageByClass> GetDetailDamageToNpcsByClass(int encounterId);
        #endregion

        // Player Roles
        List<EncounterPlayerRole> GetEncounterRoleRecords(int id);
        List<PlayerRole> GetPlayerRoles(int id);

        // NPCs per encounter
        bool EncounterNpcRecordsExist(int id);

        /// <summary>
        /// Gets the list of NPCs seen in the given encounter from the damage, healing and shielding tables.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<EncounterNpc> GetEncounterNpcsFromEncounterInfo(int id);

        /// <summary>
        /// Gets the name of an NPC from the given encounter ID and NPC ID (from the combatlog)
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="npcId"></param>
        /// <returns></returns>
        string GetNpcName(int encounterId, string npcId);

        /// <summary>
        /// Gets a list of encounters that are missing EncounterNpc records, optionally limited by EncounterLimit
        /// </summary>
        /// <param name="encounterLimit"></param>
        /// <returns></returns>
        List<Encounter> GetEncountersMissingNpcRecords(int encounterLimit = 20);

        List<int> GetEncountersMissingPlayerRecords(int encounterLimit = 20);

        int EncountersMissingPlayerStatistics();

        int EncountersMissingBiggestHitStatistics();

        RecordCounts CountBasicRecordsForEncounter(int encounterId);

        List<int> GetEncountersIdsMissingPlayerStatistics(int encounterLimit = 20);
        List<Encounter> GetEncountersMissingPlayerStatistics(int encounterLimit = 20);
        List<Encounter> GetEncountersMissingBiggestHitStatistics(int encounterLimit = 20);
        Task<List<Encounter>> GetEncountersMissingBurstStatistics(int encounterLimit = 20);
        List<Encounter> GetEncountersMissingSingleTargetDpsStatistics(int encounterLimit = 100);
        List<PlayerIdSingleTargetDamage> GetPlayerSingleTargetDamageDone(int id, string targetName);

        List<EncounterDeathEvent> GetEventsBeforeDeath(int encounterId, int targetPlayerId, int minSeconds, int maxSeconds);

        // Individual encounter
        Task<Encounter> GetAsync(int id);

        // NPCs per encounter
        Task<bool> EncounterNpcRecordsExistAsync(int id);
        Task<List<EncounterNpc>> GetEncounterNpcsFromEncounterInfoAsync(int id);

        // Player Roles
        Task<List<EncounterPlayerRole>> GetEncounterRoleRecordsAsync(int id);
        Task<List<PlayerRole>> GetPlayerRolesAsync(int id);

        // Encounter statistics
        Task<long> GetEncounterDpsAsync(int id, int totalSeconds);
        Task<long> GetEncounterHpsAsync(int id, int totalSeconds);
        Task<long> GetEncounterApsAsync(int id, int totalSeconds);
        Task<List<EncounterDebuffAction>> GetDebuffActionsAsync(int id);
        Task<List<EncounterBuffAction>> GetBuffActionsAsync(int id);
        Task<List<EncounterNpcCast>> GetNpcCastsAsync(int id);
        Task<int> GetTotalPlayerDeathsAsync(int id);

        // Big resultsets
        Task<List<DamageDone>> GetAllDamageDoneForEncounterAsync(int id);
        Task<List<HealingDone>> GetAllHealingDoneForEncounterAsync(int id);
        Task<List<ShieldingDone>> GetAllShieldingDoneForEncounterAsync(int id);

        // Interaction methods
        Task<List<CharacterInteractionPerSecond>> CharacterInteractionPerSecondAsync(
        int id, int playerId = -1, string npcId = "", bool outgoing = true, CharacterType characterType = CharacterType.Player,
        InteractionType interactionType = InteractionType.Damage, InteractionFilter filter = InteractionFilter.All, InteractionMode mode = InteractionMode.Ability);

        Task<List<EncounterCharacterAbilityBreakdownDetail>> CharacterInteractionTotalsAsync(
            int id, int playerId = -1, string npcId = "", bool outgoing = true, CharacterType characterType = CharacterType.Player,
            InteractionType interactionType = InteractionType.Damage, InteractionFilter filter = InteractionFilter.All,
            InteractionMode mode = InteractionMode.Ability, int totalSeconds = 0);

        /// <summary>
        /// Count the number of players seen in an encounter, and the number of player roles in the database
        /// (for comparison purposes to see if we need to update the roles table)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<EncounterPlayersAndRoles> CountEncounterPlayersAndRoles(int id);

        Task<List<int>> GetAllEncounterIdsDescending();
        #endregion

        #region COMMAND METHODS

        Task<bool> RemoveRoleRecordsForEncounter(int id);

        /// <summary>
        /// Attempts to modify encounter privacy. Fails if the user does not have the correct access
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="setToPublic"></param>
        /// <returns></returns>
        ReturnValue ChangePrivacy(int id, int userId, bool setToPublic = false);

        /// <summary>
        /// Sets an encounter to be able to be included in rankings
        /// </summary>
        /// <param name="id"></param>
        void MakeValidForRankings(int id);

        void MakeValidForRankings(int id, int difficultyId);

        /// <summary>
        /// Prevents an encounter from being included in rankings
        /// </summary>
        /// <param name="id"></param>
        void MakeInvalidForRankings(int id);

        ReturnValue AddPlayerEncounterRoles(List<EncounterPlayerRole> playerRoles);

        ReturnValue AddEncounterNpcs(List<EncounterNpc> encounterNpcs);

        /// <summary>
        /// This is the new stats save method, rather than splitting them up into two methods
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        ReturnValue SaveEncounterPlayerStatistics(List<EncounterPlayerStatistics> list);

        // This method will eventually be obsolete once we incorporate it into the normal SaveEncounterPlayerStatistics method
        ReturnValue UpdateEncounterSingleTargetDpsStatistics(List<EncounterPlayerStatistics> list);

        /// <summary>
        /// Removes the entire encounter from the database.
        /// This method is only used by the scheduled task that removes zero duration encounters
        /// </summary>
        /// <param name="email">The email address of the user performing the delete / remove</param>
        /// <param name="encounterId">The ID of the encounter to remove</param>
        void RemoveEncounter(string email, int encounterId);

        /// <summary>
        /// Removes encounters that have been marked for deletion - standard method
        /// </summary>
        /// <param name="email"></param>
        /// <param name="console"></param>
        /// <param name="overrideConnectionString"></param>
        /// <returns></returns>
        ReturnValue RemoveEncountersMarkedForDeletion(string email, bool console = false, string overrideConnectionString = null);

        /// <summary>
        /// This is the new method to remove records from encounters marked for deletion.
        /// </summary>
        /// <remarks>Date implemented: 20191126</remarks>
        /// <example>DoSomething();</example>
        /// <param name="email">The email address of the user performing the cleanup</param>
        /// <returns></returns>
        Task<ReturnValue> RemoveEncountersMarkedForDeletionAsync(string email);

        /// <summary>
        /// This method checks whether there are encounters without records, and records with encounters (in tables such as HealingDone and DamageDone)
        /// </summary>
        /// <returns></returns>
        Task<ReturnValue> CheckForOrphanedEncountersAsync();

        /// <summary>
        /// Marks multiple encounters for deletion
        /// </summary>
        /// <param name="encounterIds"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        ReturnValue MarkEncountersForDeletion(List<int> encounterIds, string email);

        void UpdateDurationForEncounter(int encounterId, TimeSpan duration);

        Task<ReturnValue> AddEncounterNpcsAsync(List<EncounterNpc> encounterNpcs);
        Task<ReturnValue> AddPlayerEncounterRolesAsync(List<EncounterPlayerRole> playerRoles);
        // This method will eventually be obsolete after it's integrated with the SaveEncounterPlayerStatistics method
        Task<ReturnValue> UpdateEncounterBurstStatistics(List<EncounterPlayerStatistics> list);
        #endregion
    }
}