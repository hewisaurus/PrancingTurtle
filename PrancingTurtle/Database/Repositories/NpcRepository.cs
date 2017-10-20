using System;
using System.Linq;
using Dapper;
using Database.Repositories.Interfaces;
using Logging;

namespace Database.Repositories
{
    public class NpcRepository : DapperRepositoryBase, INpcRepository
    {
        private readonly ILogger _logger;

        public NpcRepository(IConnectionFactory connectionFactory, ILogger logger)
            : base(connectionFactory)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the name of an NPC from the given ID
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="encounterId"></param>
        /// <returns></returns>
        public string GetName(string npcId, int encounterId)
        {
            try
            {
                string timeElapsed;
                
                var result =
                    Query(
                        q =>
                            q.Query<string>(MySQL.Encounter.Character.Npc.GetNameFromIdDamageDone,
                                new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

                if (!string.IsNullOrEmpty(result)) return result;

                result =
                Query(
                    q =>
                        q.Query<string>(
                            MySQL.Encounter.Character.Npc.GetNameFromIdDamageTaken,
                            new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

                if (!string.IsNullOrEmpty(result)) return result;

                result =
                    Query(
                        q =>
                            q.Query<string>(
                                MySQL.Encounter.Character.Npc.GetNameFromIdHealingDone,
                                new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

                if (!string.IsNullOrEmpty(result)) return result;

                result =
                    Query(
                        q =>
                            q.Query<string>(
                                MySQL.Encounter.Character.Npc.GetNameFromIdHealingTaken,
                                new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

                if (!string.IsNullOrEmpty(result)) return result;

                result =
                    Query(
                        q =>
                            q.Query<string>(
                                MySQL.Encounter.Character.Npc.GetNameFromIdShieldingDone,
                                new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

                if (!string.IsNullOrEmpty(result)) return result;

                result =
                    Query(
                        q =>
                            q.Query<string>(
                                MySQL.Encounter.Character.Npc.GetNameFromIdShieldingTaken,
                                new { npcId, encounterId }), out timeElapsed).SingleOrDefault();

               return result;
            }
            catch (Exception ex)
            {
                _logger.Debug(string.Format("Error while trying to get NPC Name from Id: {0}", ex.Message));
                return null;
            }
        }
    }
}
